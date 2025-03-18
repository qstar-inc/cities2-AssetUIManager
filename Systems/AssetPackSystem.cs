using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Prefabs;
using Game;
using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AssetUIManager.Systems
{
    public partial class AssetPackSystem : GameSystemBase
    {
        private EntityQuery CreateQuery(ComponentType[] all, ComponentType[]? none = null)
        {
            if (none != null)
            {
                return GetEntityQuery(new EntityQueryDesc() { All = all, None = none });
            }
            else
            {
                return GetEntityQuery(new EntityQueryDesc() { All = all });
            }
        }

        private PrefabSystem? prefabSystem;
        private EntityQuery transportDepot;
        private EntityQuery publicTransportStation;
        private EntityQuery publicTransportStop;
        private EntityQuery publicTransportNetwork;
        private EntityQuery cargoTransportStation;
        private EntityQuery lineTool;
        private static bool log;
        private static int priority = 1750;
        public static Dictionary<string, Entity> assetPacks = new();

        protected override void OnCreate()
        {
            base.OnCreate();
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            transportDepot = CreateQuery(new[] { ComponentType.ReadWrite<TransportDepotData>(), ComponentType.ReadWrite<BuildingData>(), ComponentType.ReadWrite<UIObjectData>() }, new[] { ComponentType.ReadOnly<ServiceUpgradeData>() });
            publicTransportStation = CreateQuery(new[] { ComponentType.ReadWrite<PublicTransportStationData>(), ComponentType.ReadWrite<BuildingData>(), ComponentType.ReadWrite<UIObjectData>() }, new[] { ComponentType.ReadOnly<ServiceUpgradeData>() });
            publicTransportStop = CreateQuery(new[] { ComponentType.ReadWrite<TransportStopData>(), ComponentType.ReadWrite<UIObjectData>() }, new[] { ComponentType.ReadOnly<ServiceUpgradeData>() });
            publicTransportNetwork = GetEntityQuery(new EntityQueryDesc() { Any = new[] { ComponentType.ReadWrite<RoadData>(), ComponentType.ReadWrite<TrackData>(), ComponentType.ReadWrite<UIObjectData>() } });
            lineTool = CreateQuery(new[] { ComponentType.ReadWrite<TransportLineData>() });
            cargoTransportStation = CreateQuery(new[] { ComponentType.ReadWrite<CargoTransportStationData>(), ComponentType.ReadWrite<BuildingData>(), ComponentType.ReadWrite<UIObjectData>() }, new[] { ComponentType.ReadOnly<ServiceUpgradeData>() });
            CreateAssetPacksInBulk();
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            RefreshUI();
        }

        public void CreateAssetPacksInBulk()
        {
            CreateAssetPacks("TransportDepot", "coui://starq-asset-ui-manager/Depots.svg");
            CreateAssetPacks("PublicTransport", "Media/Game/Icons/Transportation.svg");
            CreateAssetPacks("CargoTransport", "Media/Game/Icons/DeliveryVan.svg");
            CreateAssetPacks("TransportLane", "Media/Game/Icons/DoublePublicTransportLane.svg");
        }

        public void RefreshUI()
        {
            log = Mod.m_Setting.VerboseLogging;
            try
            {
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "TransportDepot", transportDepot);
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "PublicTransport", publicTransportStation);
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "PublicTransport", publicTransportStop);
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "CargoTransport", cargoTransportStation);
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "TransportLane", lineTool);
                AddAssetPacksToNetwork(Mod.m_Setting.EnableAssetPacks, "TransportLane", publicTransportNetwork);
            }
            catch (Exception ex) { Mod.log.Error(ex); }
            if (log) Mod.log.Info("Refresh Complete!");
        }

        public void AddAssetPacks(bool yes, string packName, EntityQuery entityQuery)
        {
            if (yes)
            {
                try
                {
                    Entity apEntity = assetPacks[packName];
                    if (apEntity == null)
                    {
                        Mod.log.Info($"AssetPack {packName} not found");
                        return;
                    }
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (EntityManager.TryGetComponent(entity, out UIObjectData uiObj) && uiObj.m_Group == null)
                        {
                            continue;
                        }

                        AssetPackElement app = new()
                        {
                            m_Pack = apEntity
                        };

                        if (!EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<AssetPackElement> apEBuffer))
                        {
                            apEBuffer = EntityManager.AddBuffer<AssetPackElement>(entity);
                        }
                        string entityName = prefabSystem.GetPrefabName(entity);
                        apEBuffer.Add(app);
                        if (log) Mod.log.Info($"Adding {entityName} to {packName}");

                        if (EntityManager.TryGetComponent(entity, out AssetPackData apd))
                        {
                            try { Mod.log.Info($"Has {apd}"); } catch (Exception ex) { Mod.log.Info(entity.GetType().Name); Mod.log.Info(ex); }
                        }

                        EntityManager.TryGetComponent(apEntity, out PrefabData apData);
                        prefabSystem.TryGetPrefab(apData, out AssetPackPrefab apPrefab);

                        EntityManager.TryGetComponent(entity, out PrefabData prefabData);
                        prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase);

                        AssetPackItem ap = ScriptableObject.CreateInstance<AssetPackItem>();
                        ap.m_Packs = new AssetPackPrefab[] { apPrefab };
                        prefabBase.AddComponentFrom(ap);
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }
            }
            else
            {
                try
                {
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (!prefabSystem.TryGetPrefab(new PrefabID("AssetPackPrefab", packName), out PrefabBase packPrefab))
                        {
                            continue;
                        }

                        if (!EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<AssetPackElement> apBuffer))
                        {
                            continue;
                        }

                        string entityName = prefabSystem.GetPrefabName(entity);
                        for (int i = 0; i < apBuffer.Length; i++)
                        {
                            var packTemp = apBuffer[i].m_Pack;
                            var packInAsset = prefabSystem.GetPrefabName(packTemp);
                            if (packInAsset == packName)
                            {
                                apBuffer.RemoveAt(i);
                                if (log) Mod.log.Info($"Removing {entityName} from {packName}");
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }
            }
        }

        public void AddAssetPacksToNetwork(bool yes, string packName, EntityQuery entityQuery)
        {
            if (yes)
            {
                try
                {
                    Entity apEntity = assetPacks[packName];
                    if (apEntity == null)
                    {
                        Mod.log.Info($"AssetPack {packName} not found");
                        return;
                    }
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (EntityManager.TryGetComponent(entity, out UIObjectData uiObj) && uiObj.m_Group == null)
                        {
                            continue;
                        }

                        if (!EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<NetGeometrySection> ngsBuffer))
                        {
                            continue;
                        }

                        bool isValid = false;

                        foreach (NetGeometrySection ngs in ngsBuffer)
                        {
                            string ngsName = prefabSystem.GetPrefabName(ngs.m_Section);
                            if (!ngsName.StartsWith("Public Transport") && !ngsName.Contains("Track"))
                            {
                                continue;
                            }
                            isValid = true;
                            break;
                        }

                        if (!isValid)
                        {
                            continue;
                        }
                        AssetPackElement app = new()
                        {
                            m_Pack = apEntity
                        };

                        if (!EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<AssetPackElement> apBuffer))
                        {
                            apBuffer = EntityManager.AddBuffer<AssetPackElement>(entity);
                        }
                        string entityName = prefabSystem.GetPrefabName(entity);
                        apBuffer.Add(app);

                        if (log) Mod.log.Info($"Adding {entityName} to {packName}");

                        if (EntityManager.TryGetComponent(entity, out AssetPackData apd))
                        {
                            try { Mod.log.Info($"Has {apd}"); } catch (Exception ex) { Mod.log.Info(entity.GetType().Name); Mod.log.Info(ex); }
                        }

                        EntityManager.TryGetComponent(apEntity, out PrefabData apData);
                        prefabSystem.TryGetPrefab(apData, out AssetPackPrefab apPrefab);

                        EntityManager.TryGetComponent(entity, out PrefabData prefabData);
                        prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase);

                        AssetPackItem ap = ScriptableObject.CreateInstance<AssetPackItem>();
                        ap.m_Packs = new AssetPackPrefab[] { apPrefab };
                        prefabBase.AddComponentFrom(ap);
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }
            }
            else
            {
                try
                {
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (!prefabSystem.TryGetPrefab(new PrefabID("AssetPackPrefab", packName), out PrefabBase packPrefab))
                        {
                            continue;
                        }

                        if (!EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<AssetPackElement> apBuffer))
                        {
                            continue;
                        }

                        string entityName = prefabSystem.GetPrefabName(entity);
                        for (int i = 0; i < apBuffer.Length; i++)
                        {
                            var packTemp = apBuffer[i].m_Pack;
                            var packInAsset = prefabSystem.GetPrefabName(packTemp);
                            if (packInAsset == packName)
                            {
                                apBuffer.RemoveAt(i);
                                if (log) Mod.log.Info($"Removing {entityName} from {packName}");
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }
            }
        }

        public void CreateAssetPacks(string name, string icon)
        {
            if (!prefabSystem.TryGetPrefab(new PrefabID("AssetPackPrefab", name), out PrefabBase assetPack))
            {
                AssetPackPrefab assetPackPrefab = ScriptableObject.CreateInstance<AssetPackPrefab>();
                assetPackPrefab.name = $"StarQ_AP {name}";
                var MenuUI = assetPackPrefab.AddComponent<UIObject>();
                MenuUI.m_Icon = icon;
                MenuUI.m_Priority = priority++;
                MenuUI.active = true;
                MenuUI.m_IsDebugObject = false;
                MenuUI.m_Group = null;
                prefabSystem.AddPrefab(assetPackPrefab);
                assetPack = assetPackPrefab;
            }
            prefabSystem.TryGetEntity(assetPack, out Entity assetPackEntity);
            if (!assetPacks.ContainsKey(name))
            {
                assetPacks.Add(name, assetPackEntity);
            }
        }

        protected override void OnUpdate()
        {

        }
    }
}
