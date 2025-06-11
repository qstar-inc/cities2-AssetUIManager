//using System;
//using AssetUIManager;
//using Colossal.Entities;
//using Colossal.UI.Binding;
//using Game.Common;
//using Game.Prefabs;
//using Game.UI.InGame;
//using HarmonyLib;
//using Unity.Collections;
//using Unity.Entities;

//namespace AssetUIManager.Patches
//{
//    [HarmonyPatch(typeof(UpgradeMenuUISystem), "BindUpgrades")]
//    public static class BindUpgrades_Override
//    {
//        public struct PriorityEntry : System.IComparable<PriorityEntry>
//        {
//            public int Priority;
//            public Entity Entity;

//            public int CompareTo(PriorityEntry other)
//            {
//                return Priority.CompareTo(other.Priority);
//            }
//        }

//        private static readonly AccessTools.FieldRef<
//            UpgradeMenuUISystem,
//            NativeList<Entity>
//        > _mUpgrades = AccessTools.FieldRefAccess<UpgradeMenuUISystem, NativeList<Entity>>(
//            "m_Upgrades"
//        );

//        private static readonly AccessTools.FieldRef<
//            UpgradeMenuUISystem,
//            ToolbarUISystem
//        > _mToolbarUISystem = AccessTools.FieldRefAccess<UpgradeMenuUISystem, ToolbarUISystem>(
//            "m_ToolbarUISystem"
//        );

//        private delegate ValueTuple<bool, bool> CheckExtDelegate(
//            UpgradeMenuUISystem self,
//            Entity upgradableEntity,
//            Entity upgradeEntity
//        );

//        private static readonly CheckExtDelegate _checkExtensionBuiltStatus =
//            AccessTools.MethodDelegate<CheckExtDelegate>(
//                AccessTools.Method(typeof(UpgradeMenuUISystem), "CheckExtensionBuiltStatus")
//            );

//        static bool Prefix(UpgradeMenuUISystem __instance, IJsonWriter writer, Entity upgradable)
//        {
//            var entityManager = __instance.EntityManager;

//            if (
//                !entityManager.Exists(upgradable)
//                || !entityManager.TryGetComponent(upgradable, out PrefabRef prefabRef)
//            )
//            {
//                writer.WriteEmptyArray();
//                return false;
//            }

//            var upgrades = _mUpgrades(__instance);
//            upgrades.Clear();

//            if (
//                entityManager.TryGetBuffer<BuildingUpgradeElement>(
//                    prefabRef.m_Prefab,
//                    true,
//                    out var dynamicBuffer
//                ) && !entityManager.HasComponent<Destroyed>(upgradable)
//            )
//            {
//                NativeList<PriorityEntry> priorities = new(Allocator.TempJob);
//                for (int i = 0; i < dynamicBuffer.Length; i++)
//                {
//                    Entity upgrade = dynamicBuffer[i].m_Upgrade;
//                    if (entityManager.TryGetComponent<UIObjectData>(upgrade, out var comp))
//                    {
//                        priorities.Add(
//                            new PriorityEntry { Priority = comp.m_Priority, Entity = upgrade }
//                        );
//                    }
//                }
//                priorities.Sort();

//                for (int i = 0; i < priorities.Length; i++)
//                {
//                    var item = priorities[i];
//                    upgrades.Add(in item.Entity);
//                }

//                priorities.Dispose();
//            }

//            writer.ArrayBegin(upgrades.Length);
//            var toolbarUISystem = _mToolbarUISystem(__instance);
//            for (int j = 0; j < upgrades.Length; j++)
//            {
//                Entity entity = upgrades[j];
//                var (built, isUpgradable) = _checkExtensionBuiltStatus(
//                    __instance,
//                    upgradable,
//                    entity
//                );
//                toolbarUISystem.BindAsset(writer, entity, built, isUpgradable);
//            }
//            writer.ArrayEnd();

//            return false;
//        }
//    }
//}
