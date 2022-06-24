using System.Collections.Generic;
using Bindito.Core;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.GameDistrictsUI;
using Timberborn.Persistence;
using Timberborn.PrioritySystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace AutomaticBeaverMigration;

public class BeaverMigrationController : MonoBehaviour, ISaveableSingleton, ILoadableSingleton
{
    private static readonly SingletonKey BeaverMigrationControllerKey = new (nameof(BeaverMigrationController));
    private static readonly PropertyKey<bool> ControllerEnabledKey = new (nameof(ControllerEnabled));
    
    private MigrationService _migrationService;
    private EntityComponentRegistry _entityComponentRegistry;
    private ISingletonLoader _singletonLoader;
    
    private readonly List<DistrictCenter> _migratableDistrictList = new ();
    private readonly List<DistrictCenter> _orderedMigratableDistrictList = new ();
    private readonly List<DesiredBeavers> _desiredBeaversList = new ();
    private readonly List<DesiredBeavers> _orderedDesiredBeaversList = new ();
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
    
    private EventBus _eventBus;
    public BeaverMigrationController(EventBus eventBus) => _eventBus = eventBus;


    [Inject]
    public void InjectDependencies(MigrationService migrationService, EntityComponentRegistry entityComponentRegistry, ISingletonLoader singletonLoader, EventBus eventBus)
    {
        _migrationService = migrationService;
        _entityComponentRegistry = entityComponentRegistry;
        _singletonLoader = singletonLoader;
        _eventBus = eventBus;
    }

    public void MigrateExcessBeavers()
    {
        if (!ControllerEnabled) return;
        _desiredBeaversList.AddRange(_entityComponentRegistry.GetEnabled<DesiredBeavers>());

        foreach (var priority in Priorities.Ascending)
        {
            foreach (DesiredBeavers desiredBeavers in _desiredBeaversList)
            {
                if (desiredBeavers.Priority == priority)
                {
                    _orderedDesiredBeaversList.Add(desiredBeavers);
                }
            }
        }

        foreach (var desiredBeavers in _orderedDesiredBeaversList)
        {
            _districtPopulation = desiredBeavers.gameObject.GetComponent<DistrictPopulation>();

            _excessAdults = _districtPopulation.NumberOfAdults - desiredBeavers.DesiredAmountOfAdults;
            _excessChildren = _districtPopulation.NumberOfChildren - desiredBeavers.DesiredAmountOfChildren;
            _excessGolems = _districtPopulation.NumberOfGolems - desiredBeavers.DesiredAmountOfGolems;


            _excessAdults = _excessAdults < 0 ? 0 : _excessAdults;
            _excessChildren = _excessChildren < 0 ? 0 : _excessChildren;
            _excessGolems = _excessGolems < 0 ? 0 : _excessGolems;
            
            _sourceDistrict = desiredBeavers.gameObject.GetComponent<DistrictCenter>();
            _migratableDistrictList.AddRange(_migrationService.ValidMigrationTargets(_sourceDistrict));

            foreach (var priority in Priorities.Descending)
            {
                foreach (var districtCenter in _migratableDistrictList)
                {
                    _desiredBeavers = districtCenter.gameObject.GetComponent<DesiredBeavers>();
                    if (_desiredBeavers.Priority == priority)
                    {
                        _orderedMigratableDistrictList.Add(districtCenter);
                    }
                }
            }

            foreach (var migratableDistrict in _orderedMigratableDistrictList)
            {
                if (_excessAdults <= 0 && _excessChildren <= 0 && _excessGolems <= 0) continue;
                _districtPopulation = migratableDistrict.gameObject.GetComponent<DistrictPopulation>();
                _desiredBeavers = migratableDistrict.gameObject.GetComponent<DesiredBeavers>();
                _targetDistrict = migratableDistrict.gameObject.GetComponent<DistrictCenter>();

                _neededAdults = _desiredBeavers.DesiredAmountOfAdults - _districtPopulation.NumberOfAdults;
                _neededChildren = _desiredBeavers.DesiredAmountOfChildren - _districtPopulation.NumberOfChildren;
                _neededGolems = _desiredBeavers.DesiredAmountOfGolems - _districtPopulation.NumberOfGolems;
                
                _neededAdults = _neededAdults < 0 ? 0 : _neededAdults;
                _neededChildren = _neededChildren < 0 ? 0 : _neededChildren;
                _neededGolems = _neededAdults < 0 ? 0 : _neededGolems;

                _migratingAdults = _neededAdults > _excessAdults ? _excessAdults : _neededAdults;
                _migratingChildren = _neededChildren > _excessChildren ? _excessChildren : _neededChildren;
                _migratingsGolems = _neededGolems > _excessGolems ? _excessGolems : _neededGolems;

                _migrationService.Migrate(_sourceDistrict, _targetDistrict, _migratingAdults,
                    _migratingChildren, _migratingsGolems);

                _excessAdults -= _migratingAdults;
                _excessChildren -= _migratingChildren;
                _excessGolems -= _migratingsGolems;
            }
            _orderedMigratableDistrictList.Clear();
            _targetDistrict = null;
            _migratableDistrictList.Clear();
        }
        _orderedDesiredBeaversList.Clear();
        _sourceDistrict = null;
        _desiredBeaversList.Clear();
    }
    
    // [OnEvent]
    // public void OnCharacterKilled(CharacterKilledEvent characterKilledEvent)
    // {
    //     if (!ControllerEnabled) return;
    //     MigrateExcessBeavers();
    //     AutomaticBeaverMigrationPlugin.Log.LogFatal("a baby");
    // }
    //
    // [OnEvent]
    // public void OnCharacterCreated(CharacterCreatedEvent characterCreated)
    // {
    //     if (!ControllerEnabled) return;
    //     MigrateExcessBeavers();
    //     AutomaticBeaverMigrationPlugin.Log.LogFatal("ohno, i died");
    // }
    
    public void ToggleController(bool value)
    {
        ControllerEnabled = value;
    }
    
    public void Save(ISingletonSaver singletonSaver)
    {
        IObjectSaver singleton = singletonSaver.GetSingleton(BeaverMigrationControllerKey);
        singleton.Set(ControllerEnabledKey, ControllerEnabled);
    }
    
    public void Load()
    {
        ISingletonLoader singletonLoader = _singletonLoader;
        if (!singletonLoader.HasSingleton(BeaverMigrationControllerKey))
        {
            return;
        }
        IObjectLoader singleton = singletonLoader.GetSingleton(BeaverMigrationControllerKey);
        if (singleton.Has(ControllerEnabledKey))
        {
            ControllerEnabled = singleton.Get(ControllerEnabledKey);
        }

        _eventBus.Register(this);
    }
}