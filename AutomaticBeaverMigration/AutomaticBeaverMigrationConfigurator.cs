using System;
using Bindito.Core;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using Timberborn.TemplateSystem;

namespace AutomaticBeaverMigration
{
    internal class AutomaticBeaverMigrationConfigurator : IConfigurator
    {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<AutomaticBeaverMigrationUI>().AsSingleton();
            containerDefinition.Bind<BeaverMigrationController>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private static TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<DistrictCenter, DesiredBeavers>();
            return builder.Build();
        }
        
        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly AutomaticBeaverMigrationUI _fragment;

            public EntityPanelModuleProvider(
                AutomaticBeaverMigrationUI fragmentExample 
                )
            {
                _fragment = fragmentExample;
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