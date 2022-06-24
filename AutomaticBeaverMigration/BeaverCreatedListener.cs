using System.Reflection;
using HarmonyLib;
using Timberborn.Reproduction;
using TimberbornAPI;

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
        TimberAPI.DependencyContainer.GetInstance<BeaverMigrationController>().MigrateExcessBeavers();
    }
}