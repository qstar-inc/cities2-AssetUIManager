using System;
using System.Collections.Generic;
using System.Linq;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Buildings;
using Game.Prefabs;
using StarQ.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AssetUIManager.Systems
{
    public partial class AssetPackSystem : GameSystemBase
    {
        public bool NeedUpdate = false;
#nullable disable
        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;
        private EntityQuery transportDepot;
        private EntityQuery publicTransportStation;
        private EntityQuery publicTransportStop;
        private EntityQuery publicTransportNetwork;
        private EntityQuery cargoTransportStation;
        private EntityQuery lineTool;
#nullable enable
        private static int priority = 1750;
        public static Dictionary<string, Entity> assetPacks = new();

        protected override void OnCreate()
        {
            base.OnCreate();
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            allAssets = SystemAPI
                .QueryBuilder()
                .WithAll<PrefabData>()
                .WithNone<ContentPrerequisiteData>()
                .Build();
            transportDepot = SystemAPI
                .QueryBuilder()
                .WithAll<TransportDepotData, BuildingData, UIObjectData>()
                .WithNone<ServiceUpgradeData>()
                .Build();
            publicTransportStation = SystemAPI
                .QueryBuilder()
                .WithAll<PublicTransportStationData, BuildingData, UIObjectData>()
                .WithNone<ServiceUpgradeData>()
                .Build();
            publicTransportStop = SystemAPI
                .QueryBuilder()
                .WithAll<TransportStopData, UIObjectData>()
                .WithNone<ServiceUpgradeData>()
                .Build();
            publicTransportNetwork = SystemAPI
                .QueryBuilder()
                .WithAny<RoadData, TrackData, UIObjectData>()
                .Build();
            lineTool = SystemAPI.QueryBuilder().WithAll<TransportLineData>().Build();
            cargoTransportStation = SystemAPI
                .QueryBuilder()
                .WithAll<CargoTransportStationData, BuildingData, UIObjectData>()
                .WithNone<ServiceUpgradeData>()
                .Build();
            Mod.m_Setting.onSettingsApplied += OnSettingsChanged;
            CreateAssetPacksInBulk();
            NeedUpdate = true;
        }

        protected override void OnUpdate() { }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            RefreshOrDisable();
        }

        //protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        //{
        //    base.OnGameLoadingComplete(purpose, mode);
        //    //Mod.m_Setting.IsGame = mode.IsGame();
        //    //RefreshOrDisable();
        //    //RefreshUI();
        //}

        private void OnSettingsChanged(Game.Settings.Setting setting)
        {
            NeedUpdate = true;
            RefreshOrDisable();
        }

        public void RefreshOrDisable()
        {
            //if (Mod.m_Setting.IsGame)
            //{
            //Mod.m_Setting.PathwayPriorityDropdownVersion++;
            CreateAssetPacksInBulk();
            RefreshUI();
            //    return;
            //}

            //DisableUI();
        }

        public void CreateAssetPacksInBulk()
        {
            CreateContentPrefab("BaseGame");
            CreateAssetPacks("BaseGame", UIHostHelper.DLC("Game"), -20);
            CreateAssetPacks("TransportDepot", UIHostHelper.Icon("Depots"), 7700001);
            CreateAssetPacks("PublicTransport", UIHostHelper.MGI("Transportation"), 7700002);
            CreateAssetPacks("CargoTransport", UIHostHelper.MGI("DeliveryVan"), 7700003);
            CreateAssetPacks(
                "TransportLane",
                UIHostHelper.MGI("DoublePublicTransportLane"),
                7700004
            );
        }

        public void RefreshUI()
        {
            if (Mod.m_Setting == null || !NeedUpdate)
                return;

            try
            {
                AddAssetPacks(Mod.m_Setting.BaseGameAssetPacks, "BaseGame", allAssets);
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "TransportDepot", transportDepot);
                AddAssetPacks(
                    Mod.m_Setting.EnableAssetPacks,
                    "PublicTransport",
                    publicTransportStation
                );
                AddAssetPacks(
                    Mod.m_Setting.EnableAssetPacks,
                    "PublicTransport",
                    publicTransportStop
                );
                AddAssetPacks(
                    Mod.m_Setting.EnableAssetPacks,
                    "CargoTransport",
                    cargoTransportStation
                );
                AddAssetPacks(Mod.m_Setting.EnableAssetPacks, "TransportLane", lineTool);
                AddAssetPacksToNetwork(
                    Mod.m_Setting.EnableAssetPacks,
                    "TransportLane",
                    publicTransportNetwork
                );
            }
            catch (Exception ex)
            {
                LogHelper.SendLog(ex, LogLevel.Error);
            }
            LogHelper.SendLog("Refresh Complete!");
            NeedUpdate = false;
        }

        //public void DisableUI()
        //{
        //    try
        //    {
        //        AddAssetPacks(false, "BaseGame", allAssets);
        //        AddAssetPacks(false, "TransportDepot", transportDepot);
        //        AddAssetPacks(false, "PublicTransport", publicTransportStation);
        //        AddAssetPacks(false, "PublicTransport", publicTransportStop);
        //        AddAssetPacks(false, "CargoTransport", cargoTransportStation);
        //        AddAssetPacks(false, "TransportLane", lineTool);
        //        AddAssetPacksToNetwork(false, "TransportLane", publicTransportNetwork);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.SendLog(ex, LogLevel.Error);
        //    }
        //    if (log)
        //        LogHelper.SendLog("Disabling Complete!");
        //    NeedUpdate = true;
        //}

        public void AddAssetPacks(bool enabled, string packName, EntityQuery entityQuery)
        {
            if (enabled)
            {
                try
                {
                    PrefabBase? contentPrefab = null;
                    if (packName == "BaseGame")
                        prefabSystem.TryGetPrefab(
                            new PrefabID("ContentPrefab", "StarQ_CP BaseGame"),
                            out contentPrefab
                        );

                    Entity apEntity = assetPacks[packName];
                    if (apEntity == null)
                    {
                        LogHelper.SendLog($"AssetPack {packName} not found");
                        return;
                    }
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (
                            packName != "BaseGame"
                            && EntityManager.TryGetComponent(entity, out UIObjectData uiObj)
                            && uiObj.m_Group == null
                        )
                            continue;

                        if (packName == "BaseGame")
                        {
                            if (prefabSystem.TryGetPrefab(entity, out PrefabBase pb))
                            {
                                if (
                                    !(
                                        pb is ZonePrefab
                                        || pb is ObjectPrefab
                                        || pb is NetPrefab
                                        || pb is AreaPrefab
                                        || pb is RoutePrefab
                                        || pb is NetLanePrefab
                                    )
                                )
                                    continue;

                                if (
                                    !(
                                        pb.asset == null
                                        || (
                                            pb.asset != null
                                            && pb.asset.path.Contains("/Cities2_Data/Content/")
                                        )
                                    )
                                )
                                    continue;
                            }
                        }

                        AssetPackElement app = new() { m_Pack = apEntity };

                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<AssetPackElement> apEBuffer
                            )
                        )
                            apEBuffer = EntityManager.AddBuffer<AssetPackElement>(entity);

                        bool contains = false;
                        foreach (var item in apEBuffer)
                        {
                            if (item.m_Pack == apEntity)
                            {
                                contains = true;
                                break;
                            }
                        }
                        if (!contains)
                            apEBuffer.Add(app);

                        string entityName = prefabSystem.GetPrefabName(entity);
                        //LogHelper.SendLog($"Adding {entityName} to {packName}");

                        //if (EntityManager.TryGetComponent(entity, out AssetPackData apd))
                        //{
                        //    try
                        //    {
                        //        LogHelper.SendLog($"Has {apd}");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        LogHelper.SendLog(entity.GetType().Name);
                        //        LogHelper.SendLog(ex);
                        //    }
                        //}

                        EntityManager.TryGetComponent(apEntity, out PrefabData apData);
                        prefabSystem.TryGetPrefab(apData, out AssetPackPrefab apPrefab);

                        EntityManager.TryGetComponent(entity, out PrefabData prefabData);
                        prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase);
                        AssetPackItem api = prefabBase.GetComponent<AssetPackItem>();

                        if (apPrefab != null)
                        {
                            if (api != null && !api.m_Packs.Contains(apPrefab))
                            {
                                Array.Resize(ref api.m_Packs, api.m_Packs.Length + 1);
                                api.m_Packs[^1] = apPrefab;
                            }
                            else
                            {
                                AssetPackItem ap = ScriptableObject.CreateInstance<AssetPackItem>();
                                ap.m_Packs = new AssetPackPrefab[] { apPrefab };
                                prefabBase.AddComponentFrom(ap);
                            }
                        }

                        if (contentPrefab != null)
                        {
                            ContentPrerequisite cpd =
                                ScriptableObject.CreateInstance<ContentPrerequisite>();
                            cpd.m_ContentPrerequisite = contentPrefab as ContentPrefab;
                            prefabBase.AddComponentFrom(cpd);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
            else
            {
                PrefabBase? contentPrefab = null;
                if (packName == "BaseGame")
                    prefabSystem.TryGetPrefab(
                        new PrefabID("ContentPrefab", "StarQ_CP BaseGame"),
                        out contentPrefab
                    );
                try
                {
                    Entity apEntity = assetPacks[packName];
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (
                            !prefabSystem.TryGetPrefab(
                                new PrefabID("AssetPackPrefab", packName),
                                out PrefabBase packPrefab
                            )
                        )
                            continue;

                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<AssetPackElement> apBuffer
                            )
                        )
                            continue;

                        string entityName = prefabSystem.GetPrefabName(entity);
                        for (int i = 0; i < apBuffer.Length; i++)
                        {
                            var packTemp = apBuffer[i].m_Pack;
                            var packInAsset = prefabSystem.GetPrefabName(packTemp);
                            if (packInAsset == packName)
                            {
                                apBuffer.RemoveAt(i);
                                //LogHelper.SendLog($"Removing {entityName} from {packName}");
                                break;
                            }
                        }

                        EntityManager.TryGetComponent(apEntity, out PrefabData apData);
                        prefabSystem.TryGetPrefab(apData, out AssetPackPrefab apPrefab);

                        EntityManager.TryGetComponent(entity, out PrefabData prefabData);
                        if (prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase))
                        {
                            AssetPackItem api = prefabBase.GetComponent<AssetPackItem>();
                            if (api != null && api.m_Packs != null)
                            {
                                List<AssetPackPrefab> updatedPacks = api
                                    .m_Packs.Where(p => p != apPrefab)
                                    .ToList();

                                if (updatedPacks.Count > 0)
                                    api.m_Packs = updatedPacks.ToArray();
                                else
                                    prefabBase.Remove<AssetPackItem>();
                            }
                        }

                        if (
                            contentPrefab != null
                            && prefabBase.TryGet(out ContentPrerequisite cPrereq)
                            && cPrereq.m_ContentPrerequisite == contentPrefab
                        )
                            prefabBase.Remove<ContentPrerequisite>();
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
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
                        LogHelper.SendLog($"AssetPack {packName} not found");
                        return;
                    }
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (
                            EntityManager.TryGetComponent(entity, out UIObjectData uiObj)
                            && uiObj.m_Group == null
                        )
                            continue;

                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<NetGeometrySection> ngsBuffer
                            )
                        )
                            continue;

                        bool isValid = false;

                        foreach (NetGeometrySection ngs in ngsBuffer)
                        {
                            string ngsName = prefabSystem.GetPrefabName(ngs.m_Section);
                            if (
                                !ngsName.StartsWith("Public Transport")
                                && !ngsName.Contains("Track")
                            )
                            {
                                continue;
                            }
                            isValid = true;
                            break;
                        }

                        if (!isValid)
                            continue;

                        AssetPackElement app = new() { m_Pack = apEntity };

                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<AssetPackElement> apBuffer
                            )
                        )
                            apBuffer = EntityManager.AddBuffer<AssetPackElement>(entity);

                        string entityName = prefabSystem.GetPrefabName(entity);
                        apBuffer.Add(app);

                        //LogHelper.SendLog($"Adding {entityName} to {packName}");

                        if (EntityManager.TryGetComponent(entity, out AssetPackData apd))
                        {
                            try
                            {
                                LogHelper.SendLog($"Has {apd}");
                            }
                            catch (Exception ex)
                            {
                                LogHelper.SendLog(entity.GetType().Name);
                                LogHelper.SendLog(ex);
                            }
                        }

                        EntityManager.TryGetComponent(apEntity, out PrefabData apData);
                        prefabSystem.TryGetPrefab(apData, out AssetPackPrefab apPrefab);

                        EntityManager.TryGetComponent(entity, out PrefabData prefabData);
                        prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase);

                        if (apPrefab != null)
                        {
                            AssetPackItem ap = ScriptableObject.CreateInstance<AssetPackItem>();
                            ap.m_Packs = new AssetPackPrefab[] { apPrefab };
                            prefabBase.AddComponentFrom(ap);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
            else
            {
                try
                {
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        if (
                            !prefabSystem.TryGetPrefab(
                                new PrefabID("AssetPackPrefab", packName),
                                out PrefabBase packPrefab
                            )
                        )
                            continue;

                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<AssetPackElement> apBuffer
                            )
                        )
                            continue;

                        string entityName = prefabSystem.GetPrefabName(entity);
                        for (int i = 0; i < apBuffer.Length; i++)
                        {
                            var packTemp = apBuffer[i].m_Pack;
                            var packInAsset = prefabSystem.GetPrefabName(packTemp);
                            if (packInAsset == packName)
                            {
                                apBuffer.RemoveAt(i);
                                //LogHelper.SendLog($"Removing {entityName} from {packName}");
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
        }

        public void CreateAssetPacks(string name, string icon, int priorityFixed = -9999999)
        {
            if (
                !prefabSystem.TryGetPrefab(
                    new PrefabID("AssetPackPrefab", $"StarQ_AP {name}"),
                    out PrefabBase assetPack
                )
            )
            {
                AssetPackPrefab assetPackPrefab =
                    ScriptableObject.CreateInstance<AssetPackPrefab>();
                assetPackPrefab.name = $"StarQ_AP {name}";
                var MenuUI = assetPackPrefab.AddComponent<UIObject>();
                MenuUI.m_Icon = icon;
                if (priorityFixed == 9999999)
                    MenuUI.m_Priority = priority++;
                else
                    MenuUI.m_Priority = priorityFixed;
                MenuUI.active = true;
                MenuUI.m_IsDebugObject = false;
                MenuUI.m_Group = null;
                prefabSystem.AddPrefab(assetPackPrefab);
                assetPack = assetPackPrefab;
            }
            prefabSystem.TryGetEntity(assetPack, out Entity assetPackEntity);
            if (!assetPacks.ContainsKey(name))
                assetPacks.Add(name, assetPackEntity);
        }

        public void CreateContentPrefab(string name)
        {
            if (
                !prefabSystem.TryGetPrefab(
                    new PrefabID("ContentPrefab", $"StarQ_CP {name}"),
                    out PrefabBase contentPrefab
                )
            )
            {
                ContentPrefab contentPrefabNew = ScriptableObject.CreateInstance<ContentPrefab>();
                contentPrefabNew.name = $"StarQ_CP {name}";
                var dlc = contentPrefabNew.AddComponent<DlcRequirement>();

                dlc.m_Notes = "Base Game content";
                dlc.m_BaseGameRequiresDatabase = false;
                dlc.m_Dlc = new Colossal.PSI.Common.DlcId(-2009);
                prefabSystem.AddPrefab(contentPrefabNew);
                contentPrefab = contentPrefabNew;
            }
            prefabSystem.TryGetEntity(contentPrefab, out Entity contentPrefabEntity);
            if (!assetPacks.ContainsKey(name))
                assetPacks.Add(name, contentPrefabEntity);
        }
    }
}
