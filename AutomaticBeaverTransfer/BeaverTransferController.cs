using System.Collections.Generic;
using Bindito.Core;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.GameDistrictsUI;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace AutomaticBeaverTransfer;

public class BeaverTransferController : MonoBehaviour, ISaveableSingleton, ILoadableSingleton
{
    private static readonly SingletonKey BeaverTransferControllerKey = new SingletonKey(nameof(BeaverTransferController));
    private static readonly PropertyKey<bool> ControllerEnabledKey = new PropertyKey<bool>(nameof(ControllerEnabled));
    
    private MigrationService _migrationService;
    private EntityComponentRegistry _entityComponentRegistry;
    private ISingletonLoader _singletonLoader;
    
    private readonly List<DistrictCenter> _migratableDistrictList = new List<DistrictCenter>();
    private readonly List<DesiredBeavers> _desiredBeaversList = new List<DesiredBeavers>();
    private DistrictCenter _sourceDistrict;
    private DistrictCenter _targetDistrict;
    private DistrictPopulation _districtPopulation;
    private DesiredBeavers _desiredBeavers;

    private int _excessAdults;
    private int _excessChildren;
    private int _excessGolems;
    
    private int _neededAdults;
    private int _neededChildren;
    private int _neededGolems;
    
    private int _migratingAdults;
    private int _migratingChildren;
    private int _migratingsGolems;

    private bool ControllerEnabled { get; set; }
    public bool ControllerToggled => ControllerEnabled;
    

    [Inject]
    public void InjectDependencies(MigrationService migrationService, EntityComponentRegistry entityComponentRegistry, ISingletonLoader singletonLoader)
    {
        _migrationService = migrationService;
        _entityComponentRegistry = entityComponentRegistry;
        _singletonLoader = singletonLoader;
    }

    public void TransferExcessBeavers()
    {
        if (ControllerEnabled)
        {
            _desiredBeaversList.AddRange(_entityComponentRegistry.GetEnabled<DesiredBeavers>());

            foreach (DesiredBeavers desiredBeavers in _desiredBeaversList)
            {
                _districtPopulation = desiredBeavers.gameObject.GetComponent<DistrictPopulation>();

                _excessAdults = _districtPopulation.NumberOfAdults - desiredBeavers.DesiredAmountOfAdults;
                _excessChildren = _districtPopulation.NumberOfChildren - desiredBeavers.DesiredAmountOfChildren;
                _excessGolems = _districtPopulation.NumberOfGolems - desiredBeavers.DesiredAmountOfGolems;


                if (_excessAdults < 0)
                {
                    _excessAdults = 0;
                }

                if (_excessChildren < 0)
                {
                    _excessChildren = 0;
                }

                if (_excessGolems < 0)
                {
                    _excessGolems = 0;
                }
                
                _sourceDistrict = desiredBeavers.gameObject.GetComponent<DistrictCenter>();
                _migratableDistrictList.AddRange(_migrationService.ValidMigrationTargets(_sourceDistrict));

                foreach (DistrictCenter migratableDistrict in _migratableDistrictList)
                {
                    if (_excessAdults > 0 || _excessChildren > 0 || _excessGolems > 0)
                    {
                        _districtPopulation = migratableDistrict.gameObject.GetComponent<DistrictPopulation>();
                        _desiredBeavers = migratableDistrict.gameObject.GetComponent<DesiredBeavers>();
                        _targetDistrict = migratableDistrict.gameObject.GetComponent<DistrictCenter>();
                        
                        AutomaticBeaverTransferPlugin.Log.LogInfo(_neededAdults + " = " + _desiredBeavers.DesiredAmountOfAdults + " - " + _districtPopulation.NumberOfAdults);
                        
                        AutomaticBeaverTransferPlugin.Log.LogInfo(_neededAdults + " = " + _desiredBeavers.DesiredAmountOfAdults + " - " + _districtPopulation.NumberOfAdults);
                        AutomaticBeaverTransferPlugin.Log.LogInfo(_neededChildren + " = " + _desiredBeavers.DesiredAmountOfChildren + " - " + _districtPopulation.NumberOfChildren);
                        AutomaticBeaverTransferPlugin.Log.LogInfo(_neededGolems + " = " + _desiredBeavers.DesiredAmountOfGolems + " - " + _districtPopulation.NumberOfGolems);

                        _neededAdults = _desiredBeavers.DesiredAmountOfAdults - _districtPopulation.NumberOfAdults;
                        _neededChildren = _desiredBeavers.DesiredAmountOfChildren - _districtPopulation.NumberOfChildren;
                        _neededGolems = _desiredBeavers.DesiredAmountOfGolems - _districtPopulation.NumberOfGolems;
                        
                        if (_neededAdults < 0)
                        {
                            _neededAdults = 0;
                        }

                        if (_neededChildren < 0)
                        {
                            _neededChildren = 0;
                        }

                        if (_neededGolems < 0)
                        {
                            _neededGolems = 0;
                        }

                        _migratingAdults = _neededAdults > _excessAdults ? _excessAdults : _neededAdults;
                        _migratingChildren = _neededChildren > _excessChildren ? _excessChildren : _neededChildren;
                        _migratingsGolems = _neededGolems > _excessGolems ? _excessGolems : _neededGolems;
                        
                        AutomaticBeaverTransferPlugin.Log.LogInfo(_sourceDistrict.DistrictName + " => " + _targetDistrict.DistrictName);
                        AutomaticBeaverTransferPlugin.Log.LogInfo("Moving: " + _migratingAdults + " excess: " + _excessAdults + " needed: " + _neededAdults);
                        AutomaticBeaverTransferPlugin.Log.LogInfo("Moving: " + _migratingChildren + " excess: " + _excessChildren + " needed: " + _neededAdults);
                        AutomaticBeaverTransferPlugin.Log.LogInfo("Moving: " + _migratingsGolems + " excess: " + _excessGolems + " needed: " + _neededAdults);

                        _migrationService.Migrate(_sourceDistrict, _targetDistrict, _migratingAdults,
                            _migratingChildren, _migratingsGolems);
                        
                        // Timberborn.CoreUI.TextFields
                        
                        _excessAdults -= _migratingAdults;
                        _excessChildren -= _migratingChildren;
                        _excessGolems -= _migratingsGolems;
                    }
                }

                _targetDistrict = null;
                _migratableDistrictList.Clear();
            }
            _sourceDistrict = null;
            _desiredBeaversList.Clear();
        }
    }
    
    [OnEvent]
    public void OnCharacterKilled(CharacterKilledEvent characterKilledEvent)
    {
        if (ControllerEnabled)
        {
            TransferExcessBeavers();
        }
    }
    
    [OnEvent]
    public void OnCharacterCreated(CharacterCreatedEvent characterCreated)
    {
        if (ControllerEnabled)
        {
            TransferExcessBeavers();
        }
    }

    public void ToggleController(bool value)
    {
        ControllerEnabled = value;
    }
    
    public void Save(ISingletonSaver singletonSaver)
    {
        IObjectSaver singleton = singletonSaver.GetSingleton(BeaverTransferControllerKey);
        singleton.Set(ControllerEnabledKey, ControllerEnabled);
    }
    
    public void Load()
    {
        ISingletonLoader singletonLoader = _singletonLoader;
        if (!singletonLoader.HasSingleton(BeaverTransferControllerKey))
        {
            return;
        }
        IObjectLoader singleton = singletonLoader.GetSingleton(BeaverTransferControllerKey);
        if (singleton.Has(ControllerEnabledKey))
        {
            ControllerEnabled = singleton.Get(ControllerEnabledKey);
        }
        AutomaticBeaverTransferPlugin.Log.LogInfo(ControllerEnabled);
    }
}