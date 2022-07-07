using System.Collections.Generic;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.GameDistrictsUI;
using Timberborn.Persistence;
using Timberborn.PrioritySystem;
using Timberborn.SingletonSystem;

namespace AutomaticBeaverMigration;

public class BeaverMigrationController : ISaveableSingleton, ILoadableSingleton
{
    private static readonly SingletonKey BeaverMigrationControllerKey = new (nameof(BeaverMigrationController));
    private static readonly PropertyKey<bool> ControllerEnabledKey = new (nameof(ControllerEnabled));
    
    private readonly MigrationService _migrationService;
    private readonly EntityComponentRegistry _entityComponentRegistry;
    private readonly ISingletonLoader _singletonLoader;
    private readonly EventBus _eventBus;

    private DesiredBeavers _desiredBeavers;

    private bool ControllerEnabled { get; set; }
    public bool ControllerToggled => ControllerEnabled;
    
    public BeaverMigrationController(MigrationService migrationService, EntityComponentRegistry entityComponentRegistry, ISingletonLoader singletonLoader, EventBus eventBus)
    {
        _migrationService = migrationService;
        _entityComponentRegistry = entityComponentRegistry;
        _singletonLoader = singletonLoader;
        _eventBus = eventBus;
    }

    public void MigrateExcessBeavers()
    {
        List<DesiredBeavers> desiredBeaversList = new ();
        List<DesiredBeavers> orderedDesiredBeaversList = new ();

        if (!ControllerEnabled) return;
        desiredBeaversList.AddRange(_entityComponentRegistry.GetEnabled<DesiredBeavers>());

        foreach (var priority in Priorities.Ascending)
        {
            foreach (DesiredBeavers desiredBeavers in desiredBeaversList)
            {
                if (desiredBeavers.Priority == priority)
                {
                    orderedDesiredBeaversList.Add(desiredBeavers);
                }
            }
        }

        foreach (var desiredBeavers in orderedDesiredBeaversList)
        {
            var districtPopulation = desiredBeavers.gameObject.GetComponent<DistrictPopulation>();

            var excessAdults = districtPopulation.NumberOfAdults - desiredBeavers.DesiredAmountOfAdults;
            var excessChildren = districtPopulation.NumberOfChildren - desiredBeavers.DesiredAmountOfChildren;
            var excessGolems = districtPopulation.NumberOfGolems - desiredBeavers.DesiredAmountOfGolems;

            excessAdults = excessAdults < 0 ? 0 : excessAdults;
            excessChildren = excessChildren < 0 ? 0 : excessChildren;
            excessGolems = excessGolems < 0 ? 0 : excessGolems;
            
            var sourceDistrict = desiredBeavers.gameObject.GetComponent<DistrictCenter>();
            
            List<DistrictCenter> migratableDistrictList = new ();
            List<DistrictCenter> orderedMigratableDistrictList = new ();
            migratableDistrictList.AddRange(_migrationService.ValidMigrationTargets(sourceDistrict));

            foreach (var priority in Priorities.Descending)
            {
                foreach (var districtCenter in migratableDistrictList)
                {
                    _desiredBeavers = districtCenter.gameObject.GetComponent<DesiredBeavers>();
                    if (_desiredBeavers.Priority == priority)
                    {
                        orderedMigratableDistrictList.Add(districtCenter);
                    }
                }
            }

            foreach (var migratableDistrict in orderedMigratableDistrictList)
            {
                if (excessAdults <= 0 && excessChildren <= 0 && excessGolems <= 0) continue;
                districtPopulation = migratableDistrict.gameObject.GetComponent<DistrictPopulation>();
                _desiredBeavers = migratableDistrict.gameObject.GetComponent<DesiredBeavers>();
                var targetDistrict = migratableDistrict.gameObject.GetComponent<DistrictCenter>();

                var neededAdults = _desiredBeavers.DesiredAmountOfAdults - districtPopulation.NumberOfAdults;
                var neededChildren = _desiredBeavers.DesiredAmountOfChildren - districtPopulation.NumberOfChildren;
                var neededGolems = _desiredBeavers.DesiredAmountOfGolems - districtPopulation.NumberOfGolems;
                
                neededAdults = neededAdults < 0 ? 0 : neededAdults;
                neededChildren = neededChildren < 0 ? 0 : neededChildren;
                neededGolems = neededGolems < 0 ? 0 : neededGolems;

                var migratingAdults = neededAdults > excessAdults ? excessAdults : neededAdults;
                var migratingChildren = neededChildren > excessChildren ? excessChildren : neededChildren;
                var migratingGolems = neededGolems > excessGolems ? excessGolems : neededGolems;

                _migrationService.Migrate(sourceDistrict, targetDistrict, migratingAdults, migratingChildren, migratingGolems);

                excessAdults -= migratingAdults;
                excessChildren -= migratingChildren;
                excessGolems -= migratingGolems;
            }
        }
    }
    
    [OnEvent]
    public void OnCharacterKilled(CharacterKilledEvent characterKilledEvent)
    {
        if (!ControllerEnabled) return;
        MigrateExcessBeavers();
    }
    
    [OnEvent]
    public void OnCharacterCreated(CharacterCreatedEvent characterCreated)
    {
        if (!ControllerEnabled) return;
        MigrateExcessBeavers();
    }

    public void ToggleController(bool value)
    {
        ControllerEnabled = value;
    }
    
    public void Save(ISingletonSaver singletonSaver)
    {
        var singleton = singletonSaver.GetSingleton(BeaverMigrationControllerKey);
        singleton.Set(ControllerEnabledKey, ControllerEnabled);
    }
    
    public void Load()
    {
        _eventBus.Register(this);
        
        if (!_singletonLoader.HasSingleton(BeaverMigrationControllerKey))
        {
            return;
        }
        var singleton = _singletonLoader.GetSingleton(BeaverMigrationControllerKey);
        if (singleton.Has(ControllerEnabledKey))
        {
            ControllerEnabled = singleton.Get(ControllerEnabledKey);
        }
    }
}