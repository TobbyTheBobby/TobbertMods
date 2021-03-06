using System;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using Timberborn.PrioritySystem;
using UnityEngine;

namespace AutomaticBeaverMigration;

public class DesiredBeavers : MonoBehaviour, IPersistentEntity, IRegisteredComponent
{
    private static readonly ComponentKey DesiredBeaversKey = new (nameof(DesiredBeavers));
    private static readonly PropertyKey<int> DesiredAmountOfAdultsKey = new (nameof(_desiredAmountOfAdults));
    private static readonly PropertyKey<int> DesiredAmountOfChildrenKey = new (nameof(_desiredAmountOfChildren));
    private static readonly PropertyKey<int> DesiredAmountOfGolemsKey = new (nameof(_desiredAmountOfGolems));
    private static readonly PropertyKey<Priority> PriorityKey = new (nameof(Priority));

    private int _desiredAmountOfAdults { get; set; }
    private int _desiredAmountOfChildren { get; set; }
    private int _desiredAmountOfGolems { get; set; }

    public int DesiredAmountOfAdults => _desiredAmountOfAdults;
    public int DesiredAmountOfChildren => _desiredAmountOfChildren;
    public int DesiredAmountOfGolems => _desiredAmountOfGolems;
    
    public event EventHandler<PriorityChangedEventArgs> PriorityChanged; 
    public Priority Priority { get; set; } = Priority.Normal;

    private readonly EnumObjectSerializer<Priority> _prioritySerializer = new ();

    public void ChangeDesiredAmountOfAdults(int newValue)
    {
         _desiredAmountOfAdults = newValue;
    }
    
    public void ChangeDesiredAmountOfChildren(int newValue)
    {
        _desiredAmountOfChildren = newValue;
    }
    
    public void ChangeDesiredAmountOfGolems(int newValue)
    {
        _desiredAmountOfGolems = newValue;
    }
    
    public void Save(IEntitySaver entitySaver)
    {
        var component = entitySaver.GetComponent(DesiredBeaversKey);
        component.Set(DesiredAmountOfAdultsKey, _desiredAmountOfAdults);
        component.Set(DesiredAmountOfChildrenKey, _desiredAmountOfChildren);
        component.Set(DesiredAmountOfGolemsKey, _desiredAmountOfGolems);
        component.Set(PriorityKey, Priority, _prioritySerializer);
    }
    
    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(DesiredBeaversKey))
        {
            return;
        }
        var component = entityLoader.GetComponent(DesiredBeaversKey);
        if (component.Has(DesiredAmountOfAdultsKey))
        {
            _desiredAmountOfAdults = component.Get(DesiredAmountOfAdultsKey);
        }
        if (component.Has(DesiredAmountOfChildrenKey))
        {
            _desiredAmountOfChildren = component.Get(DesiredAmountOfChildrenKey);
        }
        if (component.Has(DesiredAmountOfGolemsKey))
        {
            _desiredAmountOfGolems = component.Get(DesiredAmountOfGolemsKey);
        }
        if (component.Has(PriorityKey))
        {
            Priority = component.Get(PriorityKey, _prioritySerializer);
        }
    }
    
    public void SetPriority(Priority priority)
    {
        if (priority == Priority)
            return;
        var priority1 = Priority;
        Priority = priority;
        var priorityChanged = PriorityChanged;
        priorityChanged?.Invoke( this, new PriorityChangedEventArgs(priority1));
    }
}