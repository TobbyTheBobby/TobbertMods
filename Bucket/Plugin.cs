using BepInEx;
using BepInEx.Logging;
using TimberbornAPI;
using TimberbornAPI.Common;

namespace Bucket
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.bucket";
        public const string PluginName = "Bucket";
        public const string PluginVersion = "1.0.0";
        
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.Global);
            
            TimberAPI.DependencyRegistry.AddConfiguratorBeforeLoad(new Bucket.BucketConfigurator(), SceneEntryPoint.MainMenu);
        }
    }
}