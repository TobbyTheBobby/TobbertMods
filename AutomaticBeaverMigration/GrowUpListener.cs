using System.Reflection;
using HarmonyLib;
using Timberborn.Beavers;
using TimberbornAPI;

namespace AutomaticBeaverMigration;

[HarmonyPatch]
public static class ReplaceGrowUpPatch
{
    private static MethodInfo TargetMethod()
    {
        return typeof(Child).GetMethod("GrowUp", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    private static void Postfix()
    {
        AutomaticBeaverMigrationPlugin.Log.LogFatal("Postfix");
        TimberAPI.DependencyContainer.GetInstance<BeaverMigrationController>().MigrateExcessBeavers();
    }
}
