using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Colossal.Entities;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using StarQ.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace AssetUIManager.Systems
{
    public partial class AssetPackSystem : GameSystemBase
    {
        public bool NeedUpdate = false;
#nullable disable
        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;

#nullable enable
        private static int priority = 1750;
        public static Dictionary<string, Entity> assetPacks = new();
        public static GameMode gameMode;
        public static bool isSet = false;
        public static Dictionary<PrefabBase, PrefabPackChanges> _changes = new();

        public class PrefabPackChanges
        {
            public Entity PrefabBaseEntity = Entity.Null;
            public List<string> AddedPackPrefabs = new();
            public bool ContentPrerequisiteAdded = false;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            prefabSystem = WorldHelper.PrefabSystem;
            allAssets = SystemAPI
                .QueryBuilder()
                .WithAll<PrefabData>()
                .WithAny<ZoneData, ObjectData, NetData, AreaData, RouteData, NetLaneData>()
                .Build();
            Mod.m_Setting.onSettingsApplied += OnSettingsChanged;
            CreateAssetPacksInBulk();
            NeedUpdate = true;
        }

        protected override void OnUpdate() { }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            gameMode = mode;
            NeedUpdate = true;
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            RefreshOrDisable();
        }

        private void OnSettingsChanged(Game.Settings.Setting setting)
        {
            NeedUpdate = true;
            RefreshOrDisable();
        }

        private void RefreshOrDisable()
        {
            if (gameMode.IsGame())
                RefreshUI();
            //else
            //DisableUI();
        }

        public static class PacksToAdd
        {
            public const string BaseGame = "BaseGame";
            public const string TransportDepot = "TransportDepot";
            public const string PublicTransport = "PublicTransport";
            public const string CargoTransport = "CargoTransport";
            public const string TransportLane = "TransportLane";
            public const string BicycleStop = "BicycleStop";
        }

        public void CreateAssetPacksInBulk()
        {
            CreateContentPrefab();
            CreateAssetPacks(PacksToAdd.BaseGame, UIHostHelper.Icon("CS2"), -20);
            CreateAssetPacks(PacksToAdd.TransportDepot, UIHostHelper.Icon("Depots"), 7700001);
            CreateAssetPacks(
                PacksToAdd.PublicTransport,
                UIHostHelper.MGI("Transportation"),
                7700002
            );
            CreateAssetPacks(PacksToAdd.CargoTransport, UIHostHelper.MGI("DeliveryVan"), 7700003);
            CreateAssetPacks(
                PacksToAdd.TransportLane,
                UIHostHelper.MGI("DoublePublicTransportLane"),
                7700004
            );
            CreateAssetPacks(PacksToAdd.BicycleStop, UIHostHelper.MGI("Bicycle"), 7700000);
        }

        public void RefreshUI()
        {
            if (Mod.m_Setting == null || !NeedUpdate)
                return;
            var stopWatch = Stopwatch.StartNew();

            Dictionary<Entity, bool> sectionIsPublicTransport = new();

            if (Mod.m_Setting.EnableAssetPacks)
            {
                EntityQuery netGeo = SystemAPI.QueryBuilder().WithAll<NetGeometryData>().Build();
                var netGeoEntities = netGeo.ToEntityArray(Allocator.Temp);
                foreach (var section in netGeoEntities)
                {
                    string name = prefabSystem.GetPrefabName(section);

                    bool isPT = name.StartsWith("Public Transport") || name.Contains("Track");

                    sectionIsPublicTransport.Add(section, isPT);
                }
            }

            NativeParallelHashMap<Entity, byte> publicTransportSections = new(
                sectionIsPublicTransport.Count,
                Allocator.TempJob
            );

            foreach (var kv in sectionIsPublicTransport)
                publicTransportSections.Add(kv.Key, kv.Value ? (byte)1 : (byte)0);

            var entities = allAssets.ToEntityArray(Allocator.Temp);
            NativeArray<byte> transportDepot = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> publicTransport = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> cargoTransport = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> transportLane = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> bicycleStop = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> baseGamePack = new(entities.Length, Allocator.TempJob);
            NativeArray<byte> baseGameContent = new(entities.Length, Allocator.TempJob);

            try
            {
                var job = new FilterJob
                {
                    entities = entities,

                    uiDataLookup = SystemAPI.GetComponentLookup<UIObjectData>(true),
                    prefabDataLookup = SystemAPI.GetComponentLookup<PrefabData>(true),
                    zoneLookup = SystemAPI.GetComponentLookup<ZoneData>(true),
                    objectLookup = SystemAPI.GetComponentLookup<ObjectData>(true),
                    netLookup = SystemAPI.GetComponentLookup<NetData>(true),
                    areaLookup = SystemAPI.GetComponentLookup<AreaData>(true),
                    routeLookup = SystemAPI.GetComponentLookup<RouteData>(true),
                    netLaneLookup = SystemAPI.GetComponentLookup<NetLaneData>(true),
                    contentPrerequisiteLookup =
                        SystemAPI.GetComponentLookup<ContentPrerequisiteData>(true),
                    buildingLookup = SystemAPI.GetComponentLookup<BuildingData>(true),
                    transportDepotLookup = SystemAPI.GetComponentLookup<TransportDepotData>(true),
                    serviceUpgradeLookup = SystemAPI.GetComponentLookup<ServiceUpgradeData>(true),
                    publicTransportStationLookup =
                        SystemAPI.GetComponentLookup<PublicTransportStationData>(true),
                    transportStopLookup = SystemAPI.GetComponentLookup<TransportStopData>(true),
                    roadLookup = SystemAPI.GetComponentLookup<RoadData>(true),
                    trackLookup = SystemAPI.GetComponentLookup<TrackData>(true),
                    transportLineLookup = SystemAPI.GetComponentLookup<TransportLineData>(true),
                    cargoTransportStationLookup =
                        SystemAPI.GetComponentLookup<CargoTransportStationData>(true),
                    parkingSpaceLookup = SystemAPI.GetComponentLookup<ParkingSpaceData>(true),
                    parkingFacilityLookup = SystemAPI.GetComponentLookup<ParkingFacilityData>(true),
                    netGeometrySectionLookup = SystemAPI.GetBufferLookup<NetGeometrySection>(true),
                    sectionMap = publicTransportSections,

                    transportDepot = transportDepot,
                    publicTransport = publicTransport,
                    cargoTransport = cargoTransport,
                    transportLane = transportLane,
                    bicycleStop = bicycleStop,
                    baseGamePack = baseGamePack,
                    baseGameContent = baseGameContent,
                };

                job.Schedule(entities.Length, 32).Complete();

                prefabSystem.TryGetPrefab(
                    new PrefabID("ContentPrefab", Mod.Name),
                    out PrefabBase? contentPrefab
                );

                LogHelper.SendLog($"Filtered {entities.Length} items", LogLevel.DEV);

                for (int i = 0; i < entities.Length; i++)
                {
                    bool isBaseGameContent =
                        Mod.m_Setting.BaseGameAssetPacks && baseGameContent[i] == 1;
                    bool isBaseGamePack = Mod.m_Setting.BaseGameAssetPacks && baseGamePack[i] == 1;
                    bool isTransportDepot =
                        Mod.m_Setting.EnableAssetPacks && transportDepot[i] == 1;
                    bool isPublicTransport =
                        Mod.m_Setting.EnableAssetPacks && publicTransport[i] == 1;
                    bool isCargoTransport =
                        Mod.m_Setting.EnableAssetPacks && cargoTransport[i] == 1;
                    bool isTransportLane = Mod.m_Setting.EnableAssetPacks && transportLane[i] == 1;
                    bool isBicycleStop = Mod.m_Setting.EnableAssetPacks && bicycleStop[i] == 1;

                    var entity = entities[i];
                    if (!prefabSystem.TryGetPrefab(entity, out PrefabBase prefabBase))
                        continue;

                    if (isBaseGameContent && prefabBase.isBuiltin)
                    {
                        if (contentPrefab != null)
                        {
                            ContentPrerequisite cpd =
                                ScriptableObject.CreateInstance<ContentPrerequisite>();
                            cpd.m_ContentPrerequisite = contentPrefab as ContentPrefab;
                            prefabBase.AddComponentFrom(cpd);
                        }
                    }

                    if (
                        !isBaseGamePack
                        && !isTransportDepot
                        && !isPublicTransport
                        && !isCargoTransport
                        && !isTransportLane
                        && !isBicycleStop
                    )
                        continue;

                    if (!_changes.TryGetValue(prefabBase, out var record))
                    {
                        record = new PrefabPackChanges();
                        _changes[prefabBase] = record;
                        record.PrefabBaseEntity = entity;
                    }

                    if (isBaseGameContent && prefabBase.isBuiltin)
                        record.ContentPrerequisiteAdded = true;

                    List<string> packsToAdd = new();

                    if (isBaseGamePack && prefabBase.isBuiltin)
                        packsToAdd.Add(PacksToAdd.BaseGame);

                    if (isTransportDepot)
                        packsToAdd.Add(PacksToAdd.TransportDepot);

                    if (isPublicTransport)
                    {
                        if (
                            !EntityManager.TryGetBuffer(
                                entity,
                                false,
                                out DynamicBuffer<NetGeometrySection> ngsBuffer
                            )
                        )
                        {
                            packsToAdd.Add(PacksToAdd.PublicTransport);
                        }
                        else
                        {
                            bool isValid = false;

                            foreach (NetGeometrySection ngs in ngsBuffer)
                            {
                                string ngsName = prefabSystem.GetPrefabName(ngs.m_Section);
                                if (
                                    !ngsName.StartsWith("Public Transport")
                                    && !ngsName.Contains("Track")
                                )
                                    continue;

                                isValid = true;
                                break;
                            }

                            if (isValid)
                                packsToAdd.Add(PacksToAdd.PublicTransport);
                        }
                    }

                    if (isCargoTransport)
                        packsToAdd.Add(PacksToAdd.CargoTransport);

                    if (isTransportLane)
                        packsToAdd.Add(PacksToAdd.TransportLane);

                    if (isBicycleStop)
                    {
                        if (
                            !prefabBase.Has<ParkingFacility>()
                            || (
                                prefabBase.TryGet<ParkingFacility>(out var parkingFacility)
                                && parkingFacility.m_RoadTypes.HasFlag(Game.Net.RoadTypes.Bicycle)
                            )
                        )
                            packsToAdd.Add(PacksToAdd.BicycleStop);
                    }

                    if (
                        !EntityManager.TryGetBuffer(
                            entity,
                            false,
                            out DynamicBuffer<AssetPackElement> apEBuffer
                        )
                    )
                        apEBuffer = EntityManager.AddBuffer<AssetPackElement>(entity);

                    var existing = new NativeArray<Entity>(apEBuffer.Length, Allocator.Temp);
                    for (int j = 0; j < apEBuffer.Length; j++)
                        existing[j] = apEBuffer[j].m_Pack;

                    //AssetPackItem api = prefabBase.AddOrGetComponent<AssetPackItem>();
                    //List<AssetPackPrefab>? packList = null;

                    //if (api.m_Packs != null)
                    //    packList = new List<AssetPackPrefab>(api.m_Packs.Length + packsToAdd.Count);
                    //else
                    //    packList = new List<AssetPackPrefab>(packsToAdd.Count);

                    //if (api.m_Packs != null)
                    //{
                    //    for (int j = 0; j < api.m_Packs.Length; j++)
                    //        packList.Add(api.m_Packs[j]);
                    //}

                    foreach (var packName in packsToAdd)
                    {
                        Entity apEntity = assetPacks[packName];
                        if (apEntity == Entity.Null)
                        {
                            LogHelper.SendLog($"AssetPack {packName} not found");
                            continue;
                        }

                        bool exists = false;
                        for (int j = 0; j < existing.Length; j++)
                        {
                            if (existing[j] == apEntity)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                            apEBuffer.Add(new() { m_Pack = apEntity });

                        //prefabSystem.TryGetPrefab(apEntity, out AssetPackPrefab apPrefab);

                        //if (!packList.Contains(apPrefab))
                        //    packList.Add(apPrefab);
                        //api.m_Packs = packList.ToArray();
                    }
                    record.AddedPackPrefabs = packsToAdd;
                }
            }
            catch (Exception ex)
            {
                LogHelper.SendLog(ex, LogLevel.Error);
            }
            finally
            {
                baseGameContent.Dispose();
                baseGamePack.Dispose();
                transportDepot.Dispose();
                publicTransport.Dispose();
                cargoTransport.Dispose();
                transportLane.Dispose();
                bicycleStop.Dispose();
                publicTransportSections.Dispose();
            }
            stopWatch.Stop();
            LogHelper.SendLog(
                $"Refreshed AssetPacks in {stopWatch.Elapsed.TotalSeconds:0.000}s!",
                LogLevel.DEVD
            );
            //findItSystem.RefreshFindIt();
            NeedUpdate = false;
            isSet = true;
        }

        public void DisableUI()
        {
            if (!isSet)
                return;

            var stopWatch = new Stopwatch();
            try
            {
                foreach (var kvp in _changes)
                {
                    PrefabBase prefab = kvp.Key;
                    PrefabPackChanges record = kvp.Value;
                    Entity entity = record.PrefabBaseEntity;

                    if (record.AddedPackPrefabs.Count > 0)
                    {
                        foreach (var packName in record.AddedPackPrefabs)
                        {
                            var apEntity = assetPacks[packName];
                            if (
                                entity != Entity.Null
                                && EntityManager.TryGetBuffer(
                                    entity,
                                    false,
                                    out DynamicBuffer<AssetPackElement> buffer
                                )
                            )
                            {
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    if (buffer[i].m_Pack == apEntity)
                                    {
                                        buffer.RemoveAt(i);
                                        break;
                                    }
                                }

                                if (buffer.Length == 0)
                                    EntityManager.RemoveComponent<AssetPackElement>(entity);
                            }

                            //var api = prefab.GetComponent<AssetPackItem>();
                            //if (api == null || api.m_Packs == null)
                            //    break;

                            //api.m_Packs = api
                            //    .m_Packs.Where(p => p.name != $"StarQ_AP {packName}")
                            //    .ToArray();
                        }
                    }

                    if (record.ContentPrerequisiteAdded)
                        prefab.Remove<ContentPrerequisite>();
                }

                _changes.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.SendLog(ex, LogLevel.Error);
            }
            stopWatch.Stop();
            LogHelper.SendLog(
                $"Disabled AssetPacks in {stopWatch.Elapsed.TotalSeconds:0.000}s!",
                LogLevel.DEVD
            );
            NeedUpdate = false;
            isSet = false;
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

                var EACO = assetPackPrefab.AddComponent<EditorAssetCategoryOverride>();
                List<string> eaco_inc = new() { $"StarQ/_Utils/{Mod.Name}" };
                EACO.m_IncludeCategories = eaco_inc.ToArray();

                prefabSystem.AddPrefab(assetPackPrefab);
                assetPack = assetPackPrefab;
            }
            prefabSystem.TryGetEntity(assetPack, out Entity assetPackEntity);
            assetPacks.TryAdd(name, assetPackEntity);
        }

        public void CreateContentPrefab()
        {
            if (
                !prefabSystem.TryGetPrefab(
                    new PrefabID("ContentPrefab", $"{Mod.Name}"),
                    out PrefabBase _
                )
            )
            {
                ContentPrefab contentPrefabNew = ScriptableObject.CreateInstance<ContentPrefab>();
                contentPrefabNew.name = Mod.Name;
                var dlc = contentPrefabNew.AddComponent<DlcRequirement>();

                dlc.m_Notes = "Base Game content";
                dlc.m_BaseGameRequiresDatabase = false;
                dlc.m_Dlc = new DlcId(-2009);

                PrefabIdentifierInfo pii = new()
                {
                    m_Name = "StarQ_CP BaseGame",
                    m_Type = "ContentPrefab",
                };
                List<PrefabIdentifierInfo> list = new() { pii };

                var obsolete = contentPrefabNew.AddComponent<ObsoleteIdentifiers>();
                obsolete.m_PrefabIdentifiers = list.ToArray();

                var EACO = contentPrefabNew.AddComponent<EditorAssetCategoryOverride>();
                List<string> eaco_inc = new() { $"StarQ/_Utils/{Mod.Name}" };
                EACO.m_IncludeCategories = eaco_inc.ToArray();

                prefabSystem.AddPrefab(contentPrefabNew);
            }
        }

        [BurstCompile]
        public struct FilterJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Entity> entities;

            [ReadOnly]
            public ComponentLookup<UIObjectData> uiDataLookup;

            [ReadOnly]
            public ComponentLookup<PrefabData> prefabDataLookup;

            [ReadOnly]
            public ComponentLookup<ZoneData> zoneLookup;

            [ReadOnly]
            public ComponentLookup<ObjectData> objectLookup;

            [ReadOnly]
            public ComponentLookup<NetData> netLookup;

            [ReadOnly]
            public ComponentLookup<AreaData> areaLookup;

            [ReadOnly]
            public ComponentLookup<RouteData> routeLookup;

            [ReadOnly]
            public ComponentLookup<NetLaneData> netLaneLookup;

            [ReadOnly]
            public ComponentLookup<ContentPrerequisiteData> contentPrerequisiteLookup;

            [ReadOnly]
            public ComponentLookup<BuildingData> buildingLookup;

            [ReadOnly]
            public ComponentLookup<TransportDepotData> transportDepotLookup;

            [ReadOnly]
            public ComponentLookup<ServiceUpgradeData> serviceUpgradeLookup;

            [ReadOnly]
            public ComponentLookup<PublicTransportStationData> publicTransportStationLookup;

            [ReadOnly]
            public ComponentLookup<TransportStopData> transportStopLookup;

            [ReadOnly]
            public ComponentLookup<RoadData> roadLookup;

            [ReadOnly]
            public ComponentLookup<TrackData> trackLookup;

            [ReadOnly]
            public ComponentLookup<TransportLineData> transportLineLookup;

            [ReadOnly]
            public ComponentLookup<CargoTransportStationData> cargoTransportStationLookup;

            [ReadOnly]
            public ComponentLookup<ParkingSpaceData> parkingSpaceLookup;

            [ReadOnly]
            public ComponentLookup<ParkingFacilityData> parkingFacilityLookup;

            [ReadOnly]
            public BufferLookup<NetGeometrySection> netGeometrySectionLookup;

            [ReadOnly]
            public NativeParallelHashMap<Entity, byte> sectionMap;

            public NativeArray<byte> transportDepot;
            public NativeArray<byte> publicTransport;
            public NativeArray<byte> cargoTransport;
            public NativeArray<byte> transportLane;
            public NativeArray<byte> bicycleStop;
            public NativeArray<byte> baseGamePack;
            public NativeArray<byte> baseGameContent;

            public void Execute(int index)
            {
                transportDepot[index] = 0;
                publicTransport[index] = 0;
                cargoTransport[index] = 0;
                transportLane[index] = 0;
                bicycleStop[index] = 0;
                baseGamePack[index] = 0;
                baseGameContent[index] = 0;
                Entity e = entities[index];

                if (prefabDataLookup.HasComponent(e) && !contentPrerequisiteLookup.HasComponent(e))
                    baseGameContent[index] = 1;

                if (
                    !(
                        uiDataLookup.HasComponent(e)
                        || zoneLookup.HasComponent(e)
                        || objectLookup.HasComponent(e)
                        || netLookup.HasComponent(e)
                        || areaLookup.HasComponent(e)
                        || routeLookup.HasComponent(e)
                        || netLaneLookup.HasComponent(e)
                    )
                )
                    return;

                if (prefabDataLookup.HasComponent(e) && !contentPrerequisiteLookup.HasComponent(e))
                    baseGamePack[index] = 1;

                if (!serviceUpgradeLookup.HasComponent(e))
                {
                    if (transportDepotLookup.HasComponent(e) && buildingLookup.HasComponent(e))
                        transportDepot[index] = 1;

                    if (
                        (
                            publicTransportStationLookup.HasComponent(e)
                            && buildingLookup.HasComponent(e)
                        ) || transportStopLookup.HasComponent(e)
                    )
                        publicTransport[index] = 1;

                    if (
                        (
                            cargoTransportStationLookup.HasComponent(e)
                            && buildingLookup.HasComponent(e)
                        )
                    )
                        cargoTransport[index] = 1;

                    if (
                        transportLineLookup.HasComponent(e)
                        || roadLookup.HasComponent(e)
                        || trackLookup.HasComponent(e)
                    )
                    {
                        if (netGeometrySectionLookup.HasBuffer(e))
                        {
                            bool isValid = false;

                            var buffer = netGeometrySectionLookup[e];

                            for (int i = 0; i < buffer.Length; i++)
                            {
                                var ngs = buffer[i];
                                if (
                                    !sectionMap.TryGetValue(ngs.m_Section, out byte isPT)
                                    && isPT == 1
                                )
                                    continue;

                                isValid = true;
                                break;
                            }

                            if (isValid)
                                transportLane[index] = 1;
                        }
                    }

                    if (
                        parkingSpaceLookup.HasComponent(e)
                        || (
                            parkingFacilityLookup.HasComponent(e)
                            && parkingFacilityLookup[e].m_RoadTypes == Game.Net.RoadTypes.Bicycle
                        )
                    )
                    {
                        bicycleStop[index] = 1;
                        publicTransport[index] = 0;
                    }
                }
            }
        }
    }
}
