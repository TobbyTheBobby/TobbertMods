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
            private readonly AutomaticBeaverMigrationUI _automaticBeaverMigrationUI;

            public EntityPanelModuleProvider(
                AutomaticBeaverMigrationUI automaticBeaverMigrationUI 
                )
            {
                _automaticBeaverMigrationUI = automaticBeaverMigrationUI;
            }
        
            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddMiddleFragment(_automaticBeaverMigrationUI);
                return builder.Build();
            }
        }
    }
}