using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberbornAPI;

namespace AutomaticBeaverMigration
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    
    public class AutomaticBeaverMigrationPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "name.plugin.automaticbeavermigration";
        public const string PluginName = "Automatic Beaver Migration";
        public const string PluginVersion = "1.0.1";
        
        internal static ManualLogSource Log;
        
        private void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");

            TimberAPI.DependencyRegistry.AddConfigurator(new AutomaticBeaverMigrationConfigurator());
            
            new Harmony("name.plugin.automaticbeavermigration").PatchAll();
        }
    }
    
}
