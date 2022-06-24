using System.Reflection;
using HarmonyLib;
using Timberborn.Beavers;
using Timberborn.Characters;
using TimberbornAPI;

namespace AutomaticBeaverMigration;

[HarmonyPatch]
public static class ReplaceCharacterKilledPatch
{
    private static MethodInfo TargetMethod()
    {
        return typeof(Character).GetMethod("DestroyCharacter", BindingFlags.Public | BindingFlags.Instance);
    }
    
    private static void Postfix()
    {
        AutomaticBeaverMigrationPlugin.Log.LogFatal("ReplaceCharacterKilledPatch");
        // TimberAPI.DependencyContainer.GetInstance<BeaverMigrationController>().MigrateExcessBeavers();
    }
}