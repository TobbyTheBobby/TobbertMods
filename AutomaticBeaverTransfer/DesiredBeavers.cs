using System;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using Timberborn.PrioritySystem;
using UnityEngine;

namespace AutomaticBeaverTransfer;

public class DesiredBeavers : MonoBehaviour, IPersistentEntity, IRegisteredComponent
{
    private readonly EntityComponentRegistry _entityComponentRegistry;
    
    private static readonly ComponentKey DesiredBeaversKey = new ComponentKey(nameof(DesiredBeavers));
    private static readonly PropertyKey<int> DesiredAmountOfAdultsKey = new PropertyKey<int>(nameof(_desiredAmountOfAdults));
    private static readonly PropertyKey<int> DesiredAmountOfChildrenKey = new PropertyKey<int>(nameof(_desiredAmountOfChildren));
    private static readonly PropertyKey<int> DesiredAmountOfGolemsKey = new PropertyKey<int>(nameof(_desiredAmountOfGolems));

    private int _desiredAmountOfAdults { get; set; }
    private int _desiredAmountOfChildren { get; set; }
    private int _desiredAmountOfGolems { get; set; }

    public int DesiredAmountOfAdults => _desiredAmountOfAdults;
    public int DesiredAmountOfChildren => _desiredAmountOfChildren;
    public int DesiredAmountOfGolems => _desiredAmountOfGolems;
    
    public event EventHandler<PriorityChangedEventArgs> PriorityChanged;
    public Priority Priority { get; private set; } = Priority.Normal;
    
    public DesiredBeavers(EntityComponentRegistry entityComponentRegistry)
    {
        _entityComponentRegistry = entityComponentRegistry;
    }

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
        IObjectSaver component = entitySaver.GetComponent(DesiredBeaversKey);
        component.Set(DesiredAmountOfAdultsKey, _desiredAmountOfAdults);
        component.Set(DesiredAmountOfChildrenKey, _desiredAmountOfChildren);
        component.Set(DesiredAmountOfGolemsKey, _desiredAmountOfGolems);
    }
    
    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(DesiredBeaversKey))
        {
            return;
        }
        IObjectLoader component = entityLoader.GetComponent(DesiredBeaversKey);
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
    }
    
    public void SetPriority(Priority priority)
    {
        if (priority == this.Priority)
            return;
        Priority priority1 = this.Priority;
        this.Priority = priority;
        EventHandler<PriorityChangedEventArgs> priorityChanged = PriorityChanged;
        if (priorityChanged == null)
            return;
        priorityChanged((object) this, new PriorityChangedEventArgs(priority1));
    }
    
    // public void OnEnterFinishedState()
    // {
    //     base.enabled = true;
    //     _entityComponentRegistry.Register(this);
    // }
    //
    // public void OnExitFinishedState()
    // {
    //     base.enabled = false;
    //     _entityComponentRegistry.Unregister(this);
    // }
}