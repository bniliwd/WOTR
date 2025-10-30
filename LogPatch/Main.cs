using HarmonyLib;
using Kingmaker;
using Kingmaker.Items;
using Kingmaker.Logging.Configuration.Platforms;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Owlcat.Runtime.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Controllers;
using UnityModManagerNet;

namespace LogPatch
{
    public class Main
    {
        public static bool Enabled;

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool toggleOn)
        {
            Harmony harmonyInstance = new(modEntry.Info.Id);
            if (toggleOn)
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmonyInstance.UnpatchAll(modEntry.Info.Id);
            }
            Enabled = toggleOn;
            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            new Harmony(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        [HarmonyPatch]
        static class Logger_Patch
        {
            [HarmonyPatch(typeof(Logger), nameof(Logger.Log))]
            [HarmonyPrefix]
            public static bool LogPrefix()
            {
                return false;
            }

            [HarmonyPatch(typeof(UnityEngine.Logger), nameof(UnityEngine.Logger.IsLogTypeAllowed))]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return [new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ret)];
            }
        }

        //Remove unnecessary code in SetMagusFeatureActive.OnTurnOff
        [HarmonyPatch(typeof(SetMagusFeatureActive), nameof(SetMagusFeatureActive.OnTurnOff))]
        internal static class SetMagusFeatureActiveFix
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var HandsEquipmentSet_get_GripType = AccessTools.PropertyGetter(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.GripType));

                var toMatch = new Func<CodeInstruction, bool>[]
                {
                    ci => ci.opcode == OpCodes.Ldarg_0,
                    ci => ci.opcode == OpCodes.Call,
                    ci => ci.opcode == OpCodes.Callvirt,
                    ci => ci.opcode == OpCodes.Callvirt,
                    ci => ci.Calls(HandsEquipmentSet_get_GripType),
                    ci => ci.opcode == OpCodes.Pop
                };

                var match = instructions.FindInstructionsIndexed(toMatch).ToArray();

                if (match.All(x => x.instruction.opcode == OpCodes.Nop))
                {
                    return instructions;
                }

                if (match.Length != toMatch.Length)
                    throw new Exception("Could not find target instructions");

                var iList = instructions.ToList();

                foreach (var (index, _) in match)
                {
                    iList[index].opcode = OpCodes.Nop;
                    iList[index].operand = null;
                }

                return iList;
            }

        }

        //GameStatistic.Tick null Player fix
        [HarmonyPatch(typeof(GameStatistic), nameof(GameStatistic.Tick))]
        internal static class GameStatistic_Tick_PlayerNullFix
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var insertAfter = instructions.Indexed().FirstOrDefault(i => i.item.opcode == OpCodes.Nop);

                if (insertAfter == default)
                {
                    return instructions;
                }

                var insertAtIndex = insertAfter.index + 1;

                var iList = instructions.ToList();

                var targetNop = new CodeInstruction(OpCodes.Nop);
                var jumpLabel = generator.DefineLabel();
                targetNop.labels.Add(jumpLabel);

                iList.InsertRange(insertAtIndex,
                    [
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Game), nameof(Game.Player))),
                        new CodeInstruction(OpCodes.Brtrue_S, jumpLabel),
                        new CodeInstruction(OpCodes.Ret),
                        targetNop
                ]);

                return iList;
            }
        }

        //据说是事件订阅导致的内存泄露
        [HarmonyPatch(typeof(ActionBarVM), MethodType.Constructor)]
        partial class EventSubscriptionLeakFixes
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> DoubleSubscribe_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var toMatch = new Func<CodeInstruction, bool>[]
                {
                    ci => ci.opcode == OpCodes.Ldarg_0,
                    ci => ci.Calls(AccessTools.Method(typeof(EventBus), nameof(EventBus.Subscribe), [typeof(object)])),
                    ci => ci.opcode == OpCodes.Pop
                };

                var match = instructions.FindInstructionsIndexed(toMatch).ToArray();

                if (match.All(x => x.instruction.opcode == OpCodes.Nop))
                {
                    return instructions;
                }

                if (match.Length != toMatch.Length) throw new Exception("Could not find target instructions");

                var iList = instructions.ToList();

                foreach (var (index, _) in match)
                {
                    iList[index].opcode = OpCodes.Nop;
                    iList[index].operand = null;
                }

                return iList;
            }
        }

        //No BloodyFaceController for non-UnitEntityView
        [HarmonyPatch(typeof(StandardMaterialController), nameof(StandardMaterialController.Awake))]
        internal class BloodyFaceControllerUnitEntityView
        {
            static bool HasUnitEntityView(StandardMaterialController smc) => smc.gameObject.GetComponent<UnitEntityView>() is not null;

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var Application_isPlaying =
                    AccessTools.PropertyGetter(typeof(UnityEngine.Application), nameof(UnityEngine.Application.isPlaying));

                var index = instructions.FindIndex(ci => ci.Calls(Application_isPlaying));

                if (index < 0) return instructions;

                var iList = instructions.ToList();

                var ifFalse = iList[index + 1];

                iList.InsertRange(index - 2,
                [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call((StandardMaterialController x) => HasUnitEntityView(x)),
                    ifFalse
                ]);

                return iList;
            }
        }

        //Less verbose short log exceptions
        [HarmonyPatch]
        internal static class Log_Exceptions_Patch
        {
            class ShortLogWithoutCallstacks(string filename, string path = null!, bool includeCallStacks = true, bool extendedLog = false) : UberLoggerFile(filename, path, includeCallStacks, extendedLog), IDisposableLogSink, ILogSink
            {
                void ILogSink.Log(LogInfo logInfo)
                {
                    if (!logInfo.IsException)
                    {
                        logInfo = new LogInfo(logInfo.Source, logInfo.Channel, logInfo.TimeStamp, logInfo.Severity,
                            null, logInfo.IsException, logInfo.Message);
                    }

                    base.Log(logInfo);
                }
            }

            [HarmonyPatch(typeof(LogSinkFactory), nameof(LogSinkFactory.CreateShort))]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> LogSinkFactory_CreateShort_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var i in instructions)
                {
                    if (i.opcode == OpCodes.Newobj && typeof(UberLoggerFile).GetConstructors(AccessTools.all).Any(i.OperandIs))
                        i.operand = typeof(ShortLogWithoutCallstacks).GetConstructors().First();

                    yield return i;
                }
            }

            [HarmonyPatch(typeof(Logger), nameof(Logger.Log))]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Logger_Log_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
            {
                var exParamIndex = 4;

                var lb = ilGen.DeclareLocal(typeof(Exception));
                lb.SetLocalSymInfo("ex2");
                var localIndex = lb.LocalIndex;

                var iList = instructions.ToList();

                iList.InsertRange(0,
                [
                    new CodeInstruction(OpCodes.Ldarg_S, exParamIndex), // Exception ex
                    new CodeInstruction(OpCodes.Stloc_S, localIndex),
                ]);

                var logInfoCtorIndex = iList
                    .Indexed()
                    .Where(i => i.item.opcode == OpCodes.Newobj &&
                        typeof(LogInfo).GetConstructors()
                            .Any(i.item.OperandIs))
                    .Select(i => i.index)
                    .First();

                var getEx = iList[logInfoCtorIndex - 5];

                if (!getEx.IsLdarg(exParamIndex)) 
                {
                    return iList;
                }

                getEx.opcode = OpCodes.Ldloc_S;
                getEx.operand = localIndex;

                return iList;
            }

        }

        [HarmonyPatch]
        internal static class SilenceNoBindingLog
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> Methods() =>
            [
                AccessTools.Method(typeof(KeyboardAccess), nameof(KeyboardAccess.Bind)),
                AccessTools.Method(typeof(KeyboardAccess), nameof(KeyboardAccess.DoUnbind))
            ];

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Patch_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var PFLog_get_Default = AccessTools.PropertyGetter(typeof(PFLog), nameof(PFLog.Default));

                var match = instructions.FindInstructionsIndexed(
                [
                    ci => ci.opcode == OpCodes.Brfalse_S,
                    ci => ci.Calls(PFLog_get_Default),
                    ci => ci.opcode == OpCodes.Ldstr && ci.operand as string == "Bind: no binding named {0}"
                ]);

                if (match.Count() != 3) return instructions;

                var (index, i) = match.First();

                var iList = instructions.ToList();

                iList[index] = new CodeInstruction(OpCodes.Br, i.operand);
                iList.Insert(index, new CodeInstruction(OpCodes.Pop));

                return iList;
            }
        }

        //Fix surprise round turns
        [HarmonyPatch]
        internal class ActingInSurpriseRoundFix
        {
            static IEnumerable<MethodInfo> TargetMethods()
            {
                yield return AccessTools.Method(typeof(CombatController), nameof(CombatController.HandleCombatStart));
                foreach (var m in typeof(CombatController).GetNestedTypes(AccessTools.all)
                    .SelectMany(AccessTools.GetDeclaredMethods)
                    .Where(m => m.Name.Contains(nameof(CombatController.HandleCombatStart))))
                    yield return m;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
            {
                var Timespan_get_Seconds = AccessTools.PropertyGetter(typeof(TimeSpan), nameof(TimeSpan.Seconds));

                foreach (var i in instructions)
                {
                    if (i.opcode == OpCodes.Call && (MethodInfo)i.operand == Timespan_get_Seconds)
                    {
                        i.operand = AccessTools.PropertyGetter(typeof(TimeSpan), nameof(TimeSpan.TotalSeconds));
                    }

                    yield return i;
                }
            }
        }

        //Only load .dll files from owlmod Assemblies directories
        [HarmonyPatch(typeof(OwlcatModification), nameof(OwlcatModification.LoadAssemblies))]
        internal class LoadAssembliesFix
        {
            static IEnumerable<string> GetAssemblies(IEnumerable<string> files)
            {
                foreach (var f in files)
                {
                    if (Path.GetExtension(f) == ".dll")
                    {
                        yield return f;
                    }
                }
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var i in instructions)
                {
                    yield return i;
                    if (i.Calls(AccessTools.Method(typeof(OwlcatModification), nameof(OwlcatModification.GetFilesFromDirectory))))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadAssembliesFix), nameof(GetAssemblies)));
                    }
                }
            }
        }

    }
}