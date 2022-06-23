using System;
using Bindito.Core;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using Timberborn.TemplateSystem;

namespace AutomaticBeaverTransfer
{
    internal class AutomaticBeaverTransferConfigurator : IConfigurator
    {

        public void Configure(IContainerDefinition containerDefinition)
        {
            // containerDefinition.Bind<StockpileInventoryFragment>().AsSingleton();
            containerDefinition.Bind<AutomaticBeaverTransferUI>().AsSingleton();
            containerDefinition.Bind<BeaverTransferController>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(new Func<TemplateModule>(AutomaticBeaverTransferConfigurator.ProvideTemplateModule)).AsSingleton();
        }

        private static TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<DistrictCenter, DesiredBeavers>();
            return builder.Build();
        }
        
        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly AutomaticBeaverTransferUI _fragment;
            // private readonly StockpileInventoryFragment _stockpileInventoryFragment;

            public EntityPanelModuleProvider(
                AutomaticBeaverTransferUI fragmentExample 
                // StockpileInventoryFragment stockpileInventoryFragment
                )
            {
                _fragment = fragmentExample;
                // _stockpileInventoryFragment = stockpileInventoryFragment;
            }
        
            public EntityPanelModule Get()
            {
                EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
                builder.AddMiddleFragment(_fragment);
                // builder.AddBottomFragment(_stockpileInventoryFragment);
                return builder.Build();
            }
        }
    }
}