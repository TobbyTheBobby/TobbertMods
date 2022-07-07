using System;
using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using Timberborn.InputSystem;
using Timberborn.PrioritySystem;
using Timberborn.PrioritySystemUI;
using Timberborn.WorkSystem;
using Timberborn.WorkSystemUI;
using TimberbornAPI.UIBuilderSystem;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.Length.Unit;
using Object = UnityEngine.Object;

namespace AutomaticBeaverMigration
{
    public class AutomaticBeaverMigrationUI : IEntityPanelFragment, IInputProcessor
    {
        private readonly UIBuilder _builder;
        private readonly BeaverMigrationController _beaverMigrationController;
        private readonly InputService _inputService;
        private readonly WorkplacePriorityToggleFactory _workplacePriorityToggleFactory;
        private DistrictCenter _sourceDistrict;
        private DesiredBeavers _desiredBeavers;
        
        private Toggle _controllerToggle;
        
        private TextField _desiredNumberOfAdultsField;
        private TextField _desiredNumberOfChildrenField;
        private TextField _desiredNumberOfGolemsField;
        
        private Label _currentNumberOfAdults; 
        private Label _currentNumberOfChildren; 
        private Label _currentNumberOfGolems;
        
        private readonly List<PriorityToggle> _toggles = new ();
        private readonly TooltipRegistrar _tooltipRegistrar;
        
        private VisualElement _root;

        public AutomaticBeaverMigrationUI(
            UIBuilder builder, 
            BeaverMigrationController beaverMigrationController, 
            InputService inputService, 
            WorkplacePriorityToggleFactory workplacePriorityToggleFactory, 
            TooltipRegistrar tooltipRegistrar)
        {
            _builder = builder;
            _beaverMigrationController = beaverMigrationController;
            _inputService = inputService;
            _workplacePriorityToggleFactory = workplacePriorityToggleFactory;
            _tooltipRegistrar = tooltipRegistrar;
        }
        
        public VisualElement InitializeFragment()
        {
            _root = _builder.CreateFragmentBuilder().SetBackground(TimberApiStyle.Backgrounds.Bg3)
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetJustifyContent(Justify.Center).SetHeight(30).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Toggles().CheckmarkInverted( "ControllerToggle", locKey: "ControllerToggle")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetJustifyContent(Justify.Center).SetHeight(30).SetAlignItems(Align.Center)
                    .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetName("Priorities")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Type", text: "", builder: builder => builder.SetWidth(170)))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Desired", text: "Desired"))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Separator", text: " / ", builder: builder => builder.SetColor(new Color(0.74f, 0.64f, 0.42f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Population", text: "Population")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Adults", text: "Adults", builder: builder => builder.SetWidth(200)))
                    .AddPreset(factory => factory.TextFields().InGameTextField(25, name:"DesiredNumberOfAdults", builder: builder => builder.SetBackgroundColor(new Color(0.08f, 0.15f, 0.14f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Separator", text: "/", builder: builder => builder.SetColor(new Color(0.74f, 0.64f, 0.42f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"MaxNumberOfAdults", text: " 0")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Children", text: "Children", builder: builder => builder.SetWidth(200)))
                    .AddPreset(factory => factory.TextFields().InGameTextField(25, name:"DesiredNumberOfChildren", builder: builder => builder.SetBackgroundColor(new Color(0.08f, 0.15f, 0.14f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Separator", text: "/", builder: builder => builder.SetColor(new Color(0.74f, 0.64f, 0.42f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"MaxNumberOfChildren", text: " 0")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Golems", text: "Golems", builder: builder => builder.SetWidth(200)))
                    .AddPreset(factory => factory.TextFields().InGameTextField(25, name:"DesiredNumberOfGolemsField", builder: builder => builder.SetBackgroundColor(new Color(0.08f, 0.15f, 0.14f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"Separator", text: "/", builder: builder => builder.SetColor(new Color(0.74f, 0.64f, 0.42f))))
                    .AddPreset(factory => factory.Labels().GameTextBig(name:"MaxNumberOfGolems", text: " 0")))
                
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetJustifyContent(Justify.Center).SetHeight(60).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Buttons().Button("Force migration", new Length(200, Pixel), name: "ForceMigration")))

                .BuildAndInitialize();
            
            _controllerToggle = _root.Q<Toggle>("ControllerToggle");
            _root.Q<Toggle>("ControllerToggle").RegisterValueChangedCallback(value => _beaverMigrationController.ToggleController(value.newValue));
            _root.Q<Button>("ForceMigration").clicked += _beaverMigrationController.MigrateExcessBeavers;
            
            _desiredNumberOfAdultsField = _root.Q<TextField>("DesiredNumberOfAdults");
            TextFields.InitializeIntTextField(_desiredNumberOfAdultsField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfAdults(value)));
            _currentNumberOfAdults = _root.Q<Label>("MaxNumberOfAdults");

            _desiredNumberOfChildrenField = _root.Q<TextField>("DesiredNumberOfChildren");
            TextFields.InitializeIntTextField(_desiredNumberOfChildrenField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfChildren(value)));
            _currentNumberOfChildren = _root.Q<Label>("MaxNumberOfChildren");

            _desiredNumberOfGolemsField = _root.Q<TextField>("DesiredNumberOfGolemsField");
            TextFields.InitializeIntTextField(_desiredNumberOfGolemsField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfGolems(value)));
            _currentNumberOfGolems = _root.Q<Label>("MaxNumberOfGolems");

            var visualElement = _root.Q<VisualElement>("Priorities");
            _tooltipRegistrar.RegisterLocalizable(visualElement, "Work.PriorityTitle");
            foreach (var priority1 in Priorities.Ascending)
            {
                var priority = priority1;
                _toggles.Add(_workplacePriorityToggleFactory.Create(priority, () => _desiredBeavers.Priority == priority, (value => SetPriority(priority, value)), visualElement));
            }
            
            _root.ToggleDisplayStyle(false);
            return _root;
        }

        public void ShowFragment(GameObject entity)
        {
            _sourceDistrict = entity.GetComponent<DistrictCenter>();
            _desiredBeavers = entity.GetComponent<DesiredBeavers>();
            
            if (!(bool) (Object)  _sourceDistrict)
                return;
            _root.ToggleDisplayStyle(true);
            
            _controllerToggle.value = _beaverMigrationController.ControllerToggled;
            
            _desiredNumberOfAdultsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfAdults.ToString());
            _desiredNumberOfChildrenField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfChildren.ToString());
            _desiredNumberOfGolemsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfGolems.ToString());
            
            _inputService.AddInputProcessor(this);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(false);
            _sourceDistrict = null;
            _desiredBeavers = null;
            _inputService.RemoveInputProcessor(this);
        }

        public void UpdateFragment()
        {
            if (!(bool) (Object)  _sourceDistrict)
                return;
            UpdateToggles();
            _root.ToggleDisplayStyle(true);
            
            var districtPopulation = _sourceDistrict.DistrictPopulation;

            _currentNumberOfAdults.text = " " + districtPopulation.NumberOfAdults;
            _currentNumberOfChildren.text = " " + districtPopulation.NumberOfChildren;
            _currentNumberOfGolems.text = " " + districtPopulation.NumberOfGolems;
            
            _desiredNumberOfAdultsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfAdults.ToString());
            _desiredNumberOfChildrenField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfChildren.ToString());
            _desiredNumberOfGolemsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfGolems.ToString());
        }

        private bool _desiredAdultsWasFocusedLastFrame;
        private bool _desiredChildrenWasFocusedLastFrame;
        private bool _desiredGolemsWasFocusedLastFrame;
        
        private bool DesiredAdultsIsFocused => _desiredNumberOfAdultsField.focusController?.focusedElement == _desiredNumberOfAdultsField;
        private bool DesiredChildrenIsFocused => _desiredNumberOfChildrenField.focusController?.focusedElement == _desiredNumberOfChildrenField;
        private bool DesiredGolemsIsFocused => _desiredNumberOfGolemsField.focusController?.focusedElement == _desiredNumberOfGolemsField;
        
        public bool ProcessInput()
        {
            var num1 = _desiredAdultsWasFocusedLastFrame ? 1 : 0;
            _desiredAdultsWasFocusedLastFrame = DesiredAdultsIsFocused;
            if (_desiredAdultsWasFocusedLastFrame)
            {
                return num1 != 0;
            }
            
            var num2 = _desiredChildrenWasFocusedLastFrame ? 1 : 0;
            _desiredChildrenWasFocusedLastFrame = DesiredChildrenIsFocused;
            if (_desiredChildrenWasFocusedLastFrame)
            {
                return num2 != 0;
            }
            
            var num3 = _desiredGolemsWasFocusedLastFrame ? 1 : 0;
            _desiredGolemsWasFocusedLastFrame = DesiredGolemsIsFocused;
            if (_desiredGolemsWasFocusedLastFrame)
            {
                return num3 != 0;
            }

            return false;
        }
        
        private void SetPriority(Priority priority, bool value)
        {
            if (!value)
                return;
            _desiredBeavers.SetPriority(priority);
        }
        
        private void UpdateToggles()
        {
            foreach (var toggle in _toggles)
                toggle.UpdateState();
        }
    }
}