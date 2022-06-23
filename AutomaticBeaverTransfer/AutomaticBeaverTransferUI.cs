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

namespace AutomaticBeaverTransfer
{
    public class AutomaticBeaverTransferUI : IEntityPanelFragment, IInputProcessor
    {
        private readonly UIBuilder _builder;
        private readonly BeaverTransferController _beaverTransferController;
        private readonly InputService _inputService;
        private readonly WorkplacePriorityToggleFactory _workplacePriorityToggleFactory;
        private DistrictCenter _sourceDistrict;
        private DesiredBeavers _desiredBeavers;
        
        private Toggle _controlerToggle;
        
        private TextField _DesiredNumberOfAdultsField;
        private TextField _DesiredNumberOfChildrenField;
        private TextField _DesiredNumberOfGolemsField;
        
        private Label _CurrentNumberOfAdults; 
        private Label _CurrentNumberOfChildren; 
        private Label _CurrentNumberOfGolems;
        
        private readonly List<PriorityToggle> _toggles = new List<PriorityToggle>();
        private TooltipRegistrar _tooltipRegistrar;
        private Label _text;
        private WorkplaceDescriber _workplaceDescriber;
        
        private VisualElement _root;

        public AutomaticBeaverTransferUI(
            UIBuilder builder, 
            BeaverTransferController beaverTransferController, 
            InputService inputService, 
            WorkplacePriorityToggleFactory workplacePriorityToggleFactory, 
            TooltipRegistrar tooltipRegistrar,
            VisualElementLoader visualElementLoader)
        {
            _builder = builder;
            _beaverTransferController = beaverTransferController;
            _inputService = inputService;
            _workplacePriorityToggleFactory = workplacePriorityToggleFactory;
            _tooltipRegistrar = tooltipRegistrar;
        }
        
        public VisualElement InitializeFragment()
        {
            _root = _builder.CreateFragmentBuilder().SetBackground(TimberApiStyle.Backgrounds.Bg3)
                .AddComponent(builder => builder.SetFlexDirection(FlexDirection.Row).SetJustifyContent(Justify.Center).SetHeight(30).SetAlignItems(Align.Center)
                    .AddPreset(factory => factory.Toggles().CheckmarkInverted( "ControllerToggle", locKey: "ControllerToggle")))
                
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
                    .AddPreset(factory => factory.Buttons().Button("Force migration", new Length(200, Pixel), name: "ForceTransfer")))
                
                .AddPreset(factory => factory.Labels().GameTextBig(name:"Text", text: "Text", builder: builder => builder.SetWidth(200)))
                .AddPreset(factory => factory.Labels().GameTextBig(name:"Priorities", builder: builder => builder.SetWidth(200)))
                
                .BuildAndInitialize();
            
            _controlerToggle = _root.Q<Toggle>("ControllerToggle");
            _root.Q<Toggle>("ControllerToggle").RegisterValueChangedCallback(value => _beaverTransferController.ToggleController(value.newValue));
            _root.Q<Button>("ForceTransfer").clicked += _beaverTransferController.TransferExcessBeavers;
            
            _DesiredNumberOfAdultsField = _root.Q<TextField>("DesiredNumberOfAdults");
            TextFields.InitializeIntTextField(_DesiredNumberOfAdultsField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfAdults(value)));
            _CurrentNumberOfAdults = _root.Q<Label>("MaxNumberOfAdults");

            _DesiredNumberOfChildrenField = _root.Q<TextField>("DesiredNumberOfChildren");
            TextFields.InitializeIntTextField(_DesiredNumberOfChildrenField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfChildren(value)));
            _CurrentNumberOfChildren = _root.Q<Label>("MaxNumberOfChildren");

            _DesiredNumberOfGolemsField = _root.Q<TextField>("DesiredNumberOfGolemsField");
            TextFields.InitializeIntTextField(_DesiredNumberOfGolemsField, 0, midEditingCallback: (value => _desiredBeavers.ChangeDesiredAmountOfGolems(value)));
            _CurrentNumberOfGolems = _root.Q<Label>("MaxNumberOfGolems");
            
            _text = _root.Q<Label>("Text");
            _tooltipRegistrar.Register(_text,  () => _workplaceDescriber.GetWorkersTooltip());
            
           
            VisualElement visualElement = _root.Q<Label>("Priorities");
            _tooltipRegistrar.RegisterLocalizable(visualElement, "Work.PriorityTitle");
            foreach (Priority priority1 in Priorities.Ascending)
            {
                Priority priority = priority1;
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
            
            _controlerToggle.value = _beaverTransferController.ControllerToggled;
            
            _DesiredNumberOfAdultsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfAdults.ToString());
            _DesiredNumberOfChildrenField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfChildren.ToString());
            _DesiredNumberOfGolemsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfGolems.ToString());

            _inputService.AddInputProcessor((IInputProcessor) this);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(false);
            _sourceDistrict = (DistrictCenter) null;
            this._inputService.RemoveInputProcessor((IInputProcessor) this);
        }

        public void UpdateFragment()
        {
            if (!(bool) (Object)  _sourceDistrict)
                return;
            _root.ToggleDisplayStyle(true);
            
            DistrictPopulation districtPopulation = _sourceDistrict.DistrictPopulation;

            _CurrentNumberOfAdults.text = " " + districtPopulation.NumberOfAdults;
            _CurrentNumberOfChildren.text = " " + districtPopulation.NumberOfChildren;
            _CurrentNumberOfGolems.text = " " + districtPopulation.NumberOfGolems;
            
            _DesiredNumberOfAdultsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfAdults.ToString());
            _DesiredNumberOfChildrenField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfChildren.ToString());
            _DesiredNumberOfGolemsField.SetValueWithoutNotify(_desiredBeavers.DesiredAmountOfGolems.ToString());
        }

        private bool _desiredAdultsWasFocusedLastFrame;
        private bool _desiredChildrenWasFocusedLastFrame;
        private bool _desiredGolemsWasFocusedLastFrame;
        
        private bool DesiredAdultsIsFocused => _DesiredNumberOfAdultsField.focusController?.focusedElement == _DesiredNumberOfAdultsField;
        private bool DesiredChildrenIsFocused => _DesiredNumberOfChildrenField.focusController?.focusedElement == _DesiredNumberOfChildrenField;
        private bool DesiredGolemsIsFocused => _DesiredNumberOfGolemsField.focusController?.focusedElement == _DesiredNumberOfGolemsField;
        
        public bool ProcessInput()
        {
            int num1 = _desiredAdultsWasFocusedLastFrame ? 1 : 0;
            _desiredAdultsWasFocusedLastFrame = DesiredAdultsIsFocused;
            if (_desiredAdultsWasFocusedLastFrame)
            {
                return num1 != 0;
            }
            
            int num2 = _desiredChildrenWasFocusedLastFrame ? 1 : 0;
            _desiredChildrenWasFocusedLastFrame = DesiredChildrenIsFocused;
            if (_desiredChildrenWasFocusedLastFrame)
            {
                return num2 != 0;
            }
            
            int num3 = _desiredGolemsWasFocusedLastFrame ? 1 : 0;
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
    }
}