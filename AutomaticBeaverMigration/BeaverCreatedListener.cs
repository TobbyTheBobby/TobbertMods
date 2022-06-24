using System.Reflection;
using HarmonyLib;
using Timberborn.Reproduction;

namespace AutomaticBeaverMigration;

[HarmonyPatch]
public static class ReplaceBeaverCreatedPatch
{
    private static MethodInfo TargetMethod()
    {
        return typeof(ChildSpawner).GetMethod("SpawnChild", BindingFlags.Public | BindingFlags.Instance);
    }
    
    private static void Postfix()
    {
        AutomaticBeaverMigrationPlugin.Log.LogFatal("ReplaceCharacterCreatedPatch");
        // TimberAPI.DependencyContainer.GetInstance<BeaverMigrationController>().MigrateExcessBeavers();
    }
}