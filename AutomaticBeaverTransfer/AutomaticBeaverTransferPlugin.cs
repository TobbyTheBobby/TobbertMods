﻿using BepInEx;
using BepInEx.Logging;
using TimberbornAPI;

namespace AutomaticBeaverTransfer
{
    [BepInPlugin("name.plugin.automaticbeavertransfer", "AutomaticBeaverTransfer", "1.0.0")]
    [BepInDependency("com.timberapi.timberapi")]
    
    public class AutomaticBeaverTransferPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        
        private void Awake()
        {
            // Add static logger for internal mod use
            Log = Logger;

            // Plugin startup logic
            // Logger.LogInfo($"Plugin AutomaticBeaverTransfer is loaded!!");

            // Autogenerated first Configurator
            TimberAPI.DependencyRegistry.AddConfigurator(new AutomaticBeaverTransferConfigurator());
        }
    }
    
}