using System.Reflection;
using HarmonyLib;
using Timberborn.Golems;
using Timberborn.Reproduction;

namespace AutomaticBeaverMigration;

[HarmonyPatch]
public static class ReplaceGolemCreatedPatch
{
    private static MethodInfo TargetMethod()
    {
        return typeof(GolemFactory).GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
    }
    
    private static void Postfix()
    {
        AutomaticBeaverMigrationPlugin.Log.LogFatal("ReplaceCharacterCreatedPatch");
        // TimberAPI.DependencyContainer.GetInstance<BeaverMigrationController>().MigrateExcessBeavers();
    }
}