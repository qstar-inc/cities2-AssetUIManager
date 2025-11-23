using System;
using System.Collections.Generic;
using System.Linq;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using StarQ.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AssetUIManager.Systems
{
    public partial class UIManagerSystem : GameSystemBase
    {
        public bool NeedUpdate = false;

        public class AssetMenuData
        {
            public Dictionary<string, Entity> Menu { get; set; } = new Dictionary<string, Entity>();
            public Dictionary<string, int> Priority { get; set; } = new Dictionary<string, int>();
        }

#nullable disable
        private PrefabSystem prefabSystem;
        private EntityQuery roadQuery;
        private EntityQuery bridgeQuery;
        private EntityQuery hospitalQuery;
        private EntityQuery educationQuery;
        private EntityQuery policeQuery;
        private EntityQuery parkQuery;

#nullable enable
        //public static Dictionary<string, Entity> assetMenuDataDict = new();
        //public static Dictionary<string, Entity> assetCatDataDict = new();
        //public static Dictionary<string, int> roadMenuPriority = new();
        public static AssetMenuData pedStreetAssetMenuData = new();
        public static AssetMenuData bridgesAssetMenuData = new();
        public static AssetMenuData parkingRoadAssetMenuData = new();
        public static AssetMenuData hospitalsAssetMenuData = new();
        public static AssetMenuData schoolsAssetMenuData = new();
        public static AssetMenuData policeAssetMenuData = new();
        public static AssetMenuData parksAssetMenuData = new();

        //public static List<KeyValuePair<string, int>> GetRoadMenuPriority()
        //{
        //    var sortedList = roadMenuPriority.OrderBy(pair => pair.Value).ToList();
        //    return sortedList;
        //}

        protected override void OnCreate()
        {
            base.OnCreate();
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            //uiAssetMenuDataQuery = SystemAPI.QueryBuilder().WithAll<UIAssetMenuData>().Build();
            //uiAssetCategoryDataQuery = SystemAPI
            //    .QueryBuilder()
            //    .WithAll<UIAssetCategoryData>()
            //    .Build();
            roadQuery = SystemAPI.QueryBuilder().WithAll<RoadData>().WithNone<BridgeData>().Build();
            bridgeQuery = SystemAPI
                .QueryBuilder()
                .WithAll<BridgeData, RoadData>()
                .WithNone<TrackData>()
                .Build();
            hospitalQuery = SystemAPI.QueryBuilder().WithAll<HospitalData>().Build();
            educationQuery = SystemAPI.QueryBuilder().WithAll<SchoolData>().Build();
            policeQuery = SystemAPI.QueryBuilder().WithAny<PoliceStationData, PrisonData>().Build();
            parkQuery = SystemAPI
                .QueryBuilder()
                .WithAll<ParkData>()
                .WithNone<ServiceUpgradeData>()
                .Build();

            Mod.m_Setting.onSettingsApplied += OnSettingsChanged;
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
        //    RefreshUI();
        //}

        private void OnSettingsChanged(Game.Settings.Setting setting)
        {
            NeedUpdate = true;
            RefreshOrDisable();
        }

        public void RefreshOrDisable()
        {
            DataCollectionSystem.CollectData();
            RefreshUI();
            //    return;
            //}

            //DisableUI();
        }

        public void RefreshUI()
        {
            if (Mod.m_Setting == null || !NeedUpdate)
                return;

            //log = Mod.m_Setting.VerboseLogging;

            LogHelper.SendLog("Refreshing UI elements", LogLevel.DEVD);
            try
            {
                TogglePathway(Mod.m_Setting.PathwayInRoads, 66);
                TogglePathway(Mod.m_Setting.QuaysInRoads, 67, PathTypes.PiersAndQuays);
                TogglePathway(Mod.m_Setting.BikewayInRoads, 68, PathTypes.BikePaths);
                ToggleHospital(Mod.m_Setting.SeparatedHospitals);
                ToggleSchool(Mod.m_Setting.SeparatedSchools);
                TogglePolice(Mod.m_Setting.SeparatedPolice);
                ProcessMovingAssets(
                    Mod.m_Setting.BridgesInRoads,
                    "StarQ_UIC RoadsBridges",
                    "Roads",
                    UIHostHelper.MGI("CableStayed"),
                    65,
                    "",
                    bridgeQuery,
                    bridgesAssetMenuData,
                    "component",
                    new[] { "Hydroelectric_Power_Plant_01 Dam" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.QuaysInRoads,
                    "PiersAndQuays",
                    "Roads",
                    UIHostHelper.MGI("QuaySmall03"),
                    67,
                    "",
                    bridgeQuery,
                    bridgesAssetMenuData,
                    "component",
                    new[] { "Hydroelectric_Power_Plant_01 Dam" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.ParkingRoadsInRoads,
                    "StarQ_UIC RoadsParkingRoads",
                    "Roads",
                    UIHostHelper.MGI("TwolanePerpendicularparkingRoad"),
                    74,
                    "Parking Lane",
                    roadQuery,
                    parksAssetMenuData,
                    "lane",
                    new[] { "Alley Oneway" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.SeparatedPocketParks,
                    "StarQ_UIC PocketParks",
                    "Parks & Recreation",
                    UIHostHelper.Icon("PocketParks"),
                    5,
                    "",
                    parkQuery,
                    parksAssetMenuData,
                    "component",
                    Array.Empty<string>(),
                    "PocketPark",
                    "startsWith"
                );
                ProcessMovingAssets(
                    Mod.m_Setting.SeparatedCityParks,
                    "StarQ_UIC CityParks",
                    "Parks & Recreation",
                    UIHostHelper.MGI("PropsPark"),
                    6,
                    "",
                    parkQuery,
                    parksAssetMenuData,
                    "component",
                    Array.Empty<string>(),
                    "CityPark",
                    "startsWith"
                );
            }
            catch (Exception ex)
            {
                LogHelper.SendLog(ex, LogLevel.Error);
            }
            //if (log)
            LogHelper.SendLog("UI Elements Refresh Completed!", LogLevel.DEVD);
            NeedUpdate = false;
        }

        //public void DisableUI()
        //{
        //    try
        //    {
        //        TogglePathway(false, 66);
        //        TogglePathway(false, 67, 2);
        //        ToggleHospital(false);
        //        ToggleSchool(false);
        //        TogglePolice(false);
        //        ProcessMovingAssets(
        //            false,
        //            "StarQ_UIC RoadsBridges",
        //            "Roads",
        //            "Media/Game/Icons/CableStayed.svg",
        //            65,
        //            "",
        //            bridgeQuery,
        //            bridgesAssetMenuData,
        //            "component",
        //            new[] { "Hydroelectric_Power_Plant_01 Dam" }
        //        );
        //        ProcessMovingAssets(
        //            false,
        //            "StarQ_UIC RoadsParkingRoads",
        //            "Roads",
        //            "Media/Game/Icons/TwolanePerpendicularparkingRoad.svg",
        //            74,
        //            "Parking Lane",
        //            roadQuery,
        //            parksAssetMenuData,
        //            "lane",
        //            new[] { "Alley Oneway" }
        //        );
        //        ProcessMovingAssets(
        //            false,
        //            "StarQ_UIC PocketParks",
        //            "Parks & Recreation",
        //            UIHostHelper.Icon("PocketParks.svg"),
        //            5,
        //            "",
        //            parkQuery,
        //            parksAssetMenuData,
        //            "component",
        //            Array.Empty<string>(),
        //            "PocketPark",
        //            "startsWith"
        //        );
        //        ProcessMovingAssets(
        //            false,
        //            "StarQ_UIC CityParks",
        //            "Parks & Recreation",
        //            "Media/Game/Icons/PropsPark.svg",
        //            6,
        //            "",
        //            parkQuery,
        //            parksAssetMenuData,
        //            "component",
        //            Array.Empty<string>(),
        //            "CityPark",
        //            "startsWith"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.SendLog(ex, LogLevel.Error);
        //    }
        //    if (log)
        //        LogHelper.SendLog("Disabling Complete!");
        //    NeedUpdate = true;
        //}

        public enum PathTypes
        {
            Pathways = 1,
            PiersAndQuays = 2,
            BikePaths = 3,
        }

        public void TogglePathway(bool yes, int priority, PathTypes type = PathTypes.Pathways)
        {
            try
            {
                FixedString64Bytes Neighbor;
                if (yes)
                {
                    Neighbor = "RoadsSmallRoads";
                }
                else
                {
                    Neighbor = "Terraforming";
                    priority = type == PathTypes.Pathways ? 30 : 31;
                }

                FixedString64Bytes itemName =
                    type == PathTypes.PiersAndQuays ? "PiersAndQuays"
                    : type == PathTypes.BikePaths ? "BikePaths"
                    : "Pathways";
                DataCollectionSystem.assetCatDataDict.TryGetValue(itemName, out Entity itemValue);
                if (
                    EntityManager.TryGetComponent(itemValue, out PrefabData prefabData)
                    && prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase)
                    && prefabSystem.TryGetComponentData(prefabBase, out UIAssetCategoryData oldCat)
                    && prefabSystem.TryGetComponentData(prefabBase, out UIObjectData uiObj)
                )
                {
                    EntityManager.TryGetComponent(
                        DataCollectionSystem.assetCatDataDict[Neighbor],
                        out UIAssetCategoryData newCat
                    );

                    RefreshBuffer(oldCat.m_Menu, newCat.m_Menu, itemName, itemValue);

                    uiObj.m_Priority = priority;
                    //if (log)
                    //LogHelper.SendLog($"Moving {itemName} to {Neighbor} at {priority}");

                    oldCat.m_Menu = newCat.m_Menu;
                    EntityManager.SetComponentData(itemValue, newCat);
                    EntityManager.SetComponentData(itemValue, uiObj);

                    if (type != PathTypes.BikePaths)
                    {
                        bool pedInPath = Mod.m_Setting.PedestrianInPathway;
                        if (!yes)
                            pedInPath = false;

                        if (type != PathTypes.PiersAndQuays)
                            ProcessMovingAssets(
                                pedInPath,
                                itemName,
                                "",
                                "",
                                0,
                                "Pedestrian Section",
                                roadQuery,
                                pedStreetAssetMenuData,
                                "lane",
                                Array.Empty<string>()
                            );
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.SendLog(e, LogLevel.Error);
            }
        }

        public void ToggleHospital(bool yes)
        {
            if (yes)
            {
                Entity clinicTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Clinics",
                    "Health & Deathcare",
                    UIHostHelper.MGI("Healthcare"),
                    1
                );
                Entity hospitalTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Hospitals",
                    "Health & Deathcare",
                    UIHostHelper.Icon("Hospital"),
                    2
                );
                Entity diseaseTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC DiseaseControl",
                    "Health & Deathcare",
                    UIHostHelper.Icon("DiseaseControl"),
                    3
                );
                Entity researchTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC HealthResearch",
                    "Health & Deathcare",
                    UIHostHelper.Icon("HealthResearch"),
                    4
                );
                Entity mergedControlAndResearchTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC HealthResearch",
                    "Health & Deathcare",
                    UIHostHelper.Icon("HealthResearch"),
                    3
                );

                try
                {
                    FixedString64Bytes key = "Healthcare";
                    DataCollectionSystem.assetCatDataDict.TryGetValue(key, out Entity hospitalCat);
                    var entities = hospitalQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);

                        if (
                            !EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData)
                            || !prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            )
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            )
                            || !(
                                uiObj.m_Group == hospitalCat
                                || uiObj.m_Group == clinicTab
                                || uiObj.m_Group == hospitalTab
                                || uiObj.m_Group == diseaseTab
                                || uiObj.m_Group == researchTab
                            )
                        )
                            continue;

                        if (!hospitalsAssetMenuData.Menu.ContainsKey(name))
                        {
                            hospitalsAssetMenuData.Menu.Add(name, uiObj.m_Group);
                            hospitalsAssetMenuData.Priority.Add(name, uiObj.m_Priority);
                        }

                        Entity selectedTab = uiObj.m_Group;

                        if (
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out HospitalData hospitalData
                            )
                        )
                        {
                            if (!hospitalData.m_TreatDiseases && !hospitalData.m_TreatInjuries)
                            {
                                if (
                                    hospitalData.m_HealthRange.x == 0
                                    && hospitalData.m_HealthRange.y == 0
                                )
                                    selectedTab = clinicTab;
                                else if (!Mod.m_Setting.SeparateControlAndResearch)
                                    selectedTab = mergedControlAndResearchTab;
                                else
                                    selectedTab = researchTab;
                            }
                            else if (hospitalData.m_TreatDiseases && !hospitalData.m_TreatInjuries)
                            {
                                if (!Mod.m_Setting.SeparateControlAndResearch)
                                    selectedTab = mergedControlAndResearchTab;
                                else
                                    selectedTab = diseaseTab;
                            }
                            else if (
                                hospitalData.m_TreatmentBonus >= 30
                                && hospitalData.m_HealthRange.x == 0
                                && hospitalData.m_HealthRange.y >= 100
                            )
                                selectedTab = hospitalTab;
                            else
                                selectedTab = clinicTab;
                        }

                        RefreshBuffer(uiObj.m_Group, selectedTab, name, entity);

                        uiObj.m_Group = selectedTab;
                        EntityManager.SetComponentData(entity, uiObj);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
            else
            {
                prefabSystem.TryGetPrefab(
                    new PrefabID("UIAssetCategoryPrefab", "Healthcare"),
                    out PrefabBase healthcare
                );
                if (healthcare != null)
                {
                    try
                    {
                        var entities = hospitalQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            );
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            );

                            if (!hospitalsAssetMenuData.Menu.ContainsKey(name))
                                continue;

                            RefreshBuffer(
                                uiObj.m_Group,
                                hospitalsAssetMenuData.Menu[name],
                                name,
                                entity
                            );

                            uiObj.m_Group = hospitalsAssetMenuData.Menu[name];
                            uiObj.m_Priority = hospitalsAssetMenuData.Priority[name];
                            EntityManager.SetComponentData(entity, uiObj);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.SendLog(e, LogLevel.Error);
                    }
                }
            }
        }

        public void ToggleSchool(bool yes)
        {
            if (yes)
            {
                Entity edu1Tab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Schools",
                    "Education & Research",
                    UIHostHelper.Icon("Edu1"),
                    1
                );
                Entity edu2Tab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC HighSchools",
                    "Education & Research",
                    UIHostHelper.Icon("Edu2"),
                    2
                );
                Entity edu3Tab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Colleges",
                    "Education & Research",
                    UIHostHelper.Icon("Edu3"),
                    3
                );
                Entity edu4Tab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Universities",
                    "Education & Research",
                    UIHostHelper.Icon("Edu4"),
                    4
                );

                try
                {
                    FixedString64Bytes key = "Education";
                    DataCollectionSystem.assetCatDataDict.TryGetValue(key, out Entity educationCat);
                    var entities = educationQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);

                        if (
                            !EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData)
                            || !prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            )
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            )
                            || !(
                                uiObj.m_Group == educationCat
                                || uiObj.m_Group == edu1Tab
                                || uiObj.m_Group == edu2Tab
                                || uiObj.m_Group == edu3Tab
                                || uiObj.m_Group == edu4Tab
                            )
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out SchoolData schoolData
                            )
                        )
                            continue;

                        if (!schoolsAssetMenuData.Menu.ContainsKey(name))
                        {
                            schoolsAssetMenuData.Menu.Add(name, uiObj.m_Group);
                            schoolsAssetMenuData.Priority.Add(name, uiObj.m_Priority);
                        }

                        Entity selectedTab = uiObj.m_Group;

                        if (schoolData.m_EducationLevel == 1)
                            selectedTab = edu1Tab;
                        else if (schoolData.m_EducationLevel == 2)
                            selectedTab = edu2Tab;
                        else if (schoolData.m_EducationLevel == 3)
                            selectedTab = edu3Tab;
                        else if (schoolData.m_EducationLevel == 4)
                            selectedTab = edu4Tab;

                        RefreshBuffer(uiObj.m_Group, selectedTab, name, entity);

                        uiObj.m_Group = selectedTab;
                        EntityManager.SetComponentData(entity, uiObj);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
            else
            {
                prefabSystem.TryGetPrefab(
                    new PrefabID("UIAssetCategoryPrefab", "Schools"),
                    out PrefabBase schoolTab
                );
                if (schoolTab != null)
                {
                    try
                    {
                        var entities = educationQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            );
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            );

                            if (!schoolsAssetMenuData.Menu.ContainsKey(name))
                                continue;

                            RefreshBuffer(
                                uiObj.m_Group,
                                schoolsAssetMenuData.Menu[name],
                                name,
                                entity
                            );

                            uiObj.m_Group = schoolsAssetMenuData.Menu[name];
                            uiObj.m_Priority = schoolsAssetMenuData.Priority[name];
                            EntityManager.SetComponentData(entity, uiObj);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.SendLog(e, LogLevel.Error);
                    }
                }
            }
        }

        public void TogglePolice(bool yes)
        {
            if (yes)
            {
                Entity localPD = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC LocalPolice",
                    "Police & Administration",
                    UIHostHelper.MGI("Police"),
                    1
                );
                Entity hqTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC PoliceHQ",
                    "Police & Administration",
                    UIHostHelper.Icon("PoliceHQ"),
                    2
                );
                Entity intelTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Intelligence",
                    "Police & Administration",
                    UIHostHelper.Icon("Intelligence"),
                    3
                );
                Entity prisonTab = CreateUIAssetCategoryPrefab(
                    "StarQ_UIC Prison",
                    "Police & Administration",
                    UIHostHelper.Icon("Prison"),
                    4
                );

                try
                {
                    FixedString64Bytes key = "Police";
                    DataCollectionSystem.assetCatDataDict.TryGetValue(key, out Entity policeCat);
                    var entities = policeQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);

                        if (
                            !EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData)
                            || !prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            )
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            )
                            || !(
                                uiObj.m_Group == policeCat
                                || uiObj.m_Group == localPD
                                || uiObj.m_Group == hqTab
                                || uiObj.m_Group == intelTab
                                || uiObj.m_Group == prisonTab
                            )
                        )
                            continue;

                        if (!policeAssetMenuData.Menu.ContainsKey(name))
                        {
                            policeAssetMenuData.Menu.Add(name, uiObj.m_Group);
                            policeAssetMenuData.Priority.Add(name, uiObj.m_Priority);
                        }

                        Entity selectedTab = uiObj.m_Group;
                        if (
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out PrisonData prisonData
                            )
                        )
                            selectedTab = prisonTab;
                        else if (
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out PoliceStationData policeStationData
                            )
                        )
                        {
                            if (
                                policeStationData.m_PurposeMask.HasFlag(PolicePurpose.Patrol)
                                && policeStationData.m_PurposeMask.HasFlag(PolicePurpose.Emergency)
                                && policeStationData.m_JailCapacity >= 100
                            )
                                selectedTab = hqTab;
                            else if (
                                policeStationData.m_PurposeMask.HasFlag(PolicePurpose.Intelligence)
                            )
                                selectedTab = intelTab;
                            else
                                selectedTab = localPD;
                        }
                        ;

                        RefreshBuffer(uiObj.m_Group, selectedTab, name, entity);

                        uiObj.m_Group = selectedTab;
                        EntityManager.SetComponentData(entity, uiObj);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.SendLog(e, LogLevel.Error);
                }
            }
            else
            {
                prefabSystem.TryGetPrefab(
                    new PrefabID("UIAssetCategoryPrefab", "Police"),
                    out PrefabBase policeTab
                );
                if (policeTab != null)
                {
                    try
                    {
                        var entities = policeQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            );
                            prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData uiObj
                            );

                            if (!policeAssetMenuData.Menu.ContainsKey(name))
                                continue;

                            RefreshBuffer(
                                uiObj.m_Group,
                                policeAssetMenuData.Menu[name],
                                name,
                                entity
                            );

                            uiObj.m_Group = policeAssetMenuData.Menu[name];
                            uiObj.m_Priority = policeAssetMenuData.Priority[name];
                            EntityManager.SetComponentData(entity, uiObj);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.SendLog(e, LogLevel.Error);
                    }
                }
            }
        }

        public void ProcessMovingAssets(
            bool enabled,
            FixedString64Bytes UIAssetCategoryName,
            string UIAssetMenuName,
            string UIAssetCategoryIcon,
            int UIAssetCategoryPriority,
            FixedString64Bytes sectionName,
            EntityQuery entityQuery,
            AssetMenuData assetMenuData,
            string processType,
            string[] excludeList,
            string? includePattern = null,
            string? includeType = null
        )
        {
            if (enabled)
            {
                Entity tab = CreateUIAssetCategoryPrefab(
                    UIAssetCategoryName,
                    UIAssetMenuName,
                    UIAssetCategoryIcon,
                    UIAssetCategoryPriority
                );

                try
                {
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);
                        if (excludeList.Contains(name))
                            continue;
                        if (
                            !EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData)
                            || !prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            )
                        )
                            continue;
                        if (
                            !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData assetUIObject
                            )
                        )
                            continue;

                        bool isValid = false;

                        if (
                            UIAssetCategoryName == "PiersAndQuays"
                            || UIAssetCategoryName == "StarQ_UIC RoadsBridges"
                        )
                        {
                            assetPrefabBase.TryGet(out Bridge bridgeData);
                            if (
                                bridgeData.m_BuildStyle == BridgeBuildStyle.Quay
                                && UIAssetCategoryName == "PiersAndQuays"
                            )
                                isValid = true;
                            else if (
                                bridgeData.m_BuildStyle != BridgeBuildStyle.Quay
                                && UIAssetCategoryName == "StarQ_UIC RoadsBridges"
                            )
                                isValid = true;
                            else
                                continue;
                        }

                        if (processType == "lane")
                        {
                            DynamicBuffer<NetGeometrySection> x =
                                EntityManager.GetBuffer<NetGeometrySection>(entity);
                            foreach (NetGeometrySection item in x)
                            {
                                try
                                {
                                    string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                    if (laneName.Contains(sectionName.ToString()))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.SendLog(e, LogLevel.Error);
                                }
                            }
                        }
                        else if (processType == "component")
                        {
                            try
                            {
                                if (includePattern == null || includeType == null)
                                {
                                    isValid = true;
                                }
                                else if (includePattern != null && includeType != null)
                                {
                                    string prefabName = prefabSystem.GetPrefabName(entity);
                                    if (
                                        includeType == "startsWith"
                                        && prefabName.StartsWith(includePattern)
                                    )
                                        isValid = true;
                                    else if (
                                        includeType == "endsWith"
                                        && prefabName.EndsWith(includePattern)
                                    )
                                        isValid = true;
                                    else if (
                                        includeType == "contains"
                                        && prefabName.Contains(includePattern)
                                    )
                                        isValid = true;

                                    if (
                                        includePattern == "PocketPark"
                                        && prefabSystem.TryGetComponentData(
                                            assetPrefabBase,
                                            out BuildingData buildingData
                                        )
                                        && (
                                            buildingData.m_LotSize.x == 1
                                            || buildingData.m_LotSize.y == 1
                                        )
                                    )
                                        isValid = true;
                                }
                            }
                            catch (Exception e)
                            {
                                LogHelper.SendLog(e, LogLevel.Error);
                            }
                        }

                        if (isValid)
                        {
                            try
                            {
                                Entity currentTab = assetUIObject.m_Group;
                                int currentPriority = assetUIObject.m_Priority;
                                if (!assetMenuData.Menu.ContainsKey(name))
                                {
                                    assetMenuData.Menu.Add(name, currentTab);
                                    assetMenuData.Priority.Add(name, currentPriority);
                                }

                                if (
                                    EntityManager.TryGetComponent(
                                        currentTab,
                                        out PrefabData currentTabPrefabData
                                    )
                                    && prefabSystem.TryGetPrefab(
                                        currentTabPrefabData,
                                        out PrefabBase currentTabPrefabBase
                                    )
                                    && prefabSystem.TryGetComponentData(
                                        currentTabPrefabBase,
                                        out UIObjectData currentTabUIObject
                                    )
                                )
                                {
                                    RefreshBuffer(currentTab, tab, name, entity);
                                    int newPriority =
                                        (currentTabUIObject.m_Priority * 1000) + currentPriority;
                                    assetUIObject.m_Priority = newPriority;
                                    assetUIObject.m_Group = tab;
                                    EntityManager.SetComponentData(entity, assetUIObject);
                                }
                            }
                            catch (Exception e)
                            {
                                LogHelper.SendLog(e, LogLevel.Error);
                            }
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
                if (
                    prefabSystem.TryGetPrefab(
                        new PrefabID("UIAssetCategoryPrefab", UIAssetCategoryName.ToString()),
                        out PrefabBase tab
                    )
                )
                {
                    try
                    {
                        var entities = entityQuery.ToEntityArray(Allocator.Temp);
                        prefabSystem.TryGetEntity(tab, out Entity tabEntity);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            if (excludeList.Contains(name))
                                continue;
                            if (
                                !EntityManager.TryGetComponent(
                                    entity,
                                    out PrefabData assetPrefabData
                                )
                                || prefabSystem.TryGetPrefab(
                                    assetPrefabData,
                                    out PrefabBase assetPrefabBase
                                )
                            )
                                continue;
                            if (
                                !prefabSystem.TryGetComponentData(
                                    assetPrefabBase,
                                    out UIObjectData assetUIObject
                                )
                            )
                                continue;
                            if (!assetMenuData.Menu.ContainsKey(name))
                                continue;

                            bool isValid = false;

                            if (processType == "lane")
                            {
                                DynamicBuffer<NetGeometrySection> x =
                                    EntityManager.GetBuffer<NetGeometrySection>(entity);

                                foreach (NetGeometrySection item in x)
                                {
                                    string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                    if (laneName.Contains(sectionName.ToString()))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                            }
                            else if (processType == "component")
                                isValid = true;

                            if (isValid)
                            {
                                RefreshBuffer(tabEntity, assetMenuData.Menu[name], name, entity);

                                assetUIObject.m_Group = assetMenuData.Menu[name];
                                assetUIObject.m_Priority = assetMenuData.Priority[name];
                                EntityManager.SetComponentData(entity, assetUIObject);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.SendLog(e, LogLevel.Error);
                    }
                }
            }
        }

        public Entity CreateUIAssetCategoryPrefab(
            FixedString64Bytes name,
            FixedString64Bytes group,
            string icon,
            int priority
        )
        {
            if (
                !prefabSystem.TryGetPrefab(
                    new PrefabID("UIAssetCategoryPrefab", name.ToString()),
                    out PrefabBase tab
                )
            )
            {
                UIAssetCategoryPrefab menuPrefab =
                    ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
                menuPrefab.name = name.ToString();

                DataCollectionSystem.assetMenuDataDict.TryGetValue(group, out Entity groupEntity);
                EntityManager.TryGetComponent(groupEntity, out PrefabData prefabData);
                prefabSystem.TryGetPrefab(prefabData, out PrefabBase roadMenu);

                menuPrefab.m_Menu = roadMenu.GetComponent<UIAssetMenuPrefab>();
                var MenuUI = menuPrefab.AddComponent<UIObject>();
                MenuUI.m_Icon = icon;
                MenuUI.m_Priority = priority;
                MenuUI.active = true;
                MenuUI.m_IsDebugObject = false;
                MenuUI.m_Group = null;
                prefabSystem.AddPrefab(menuPrefab);
                tab = menuPrefab;
            }
            prefabSystem.TryGetEntity(tab, out Entity tabEntity);

            return tabEntity;
        }

        public void RefreshBuffer(
            Entity oldCat,
            Entity newCat,
            FixedString64Bytes moverName,
            Entity moverEntity
        )
        {
            DynamicBuffer<UIGroupElement> uiGroupElementbuffer =
                EntityManager.GetBuffer<UIGroupElement>(oldCat);

            //var itemName = prefabSystem.GetPrefabName(moverEntity);
            //var tabNameOld = prefabSystem.GetPrefabName(oldCat);
            for (int i = 0; i < uiGroupElementbuffer.Length; i++)
            {
                var uge = uiGroupElementbuffer[i].m_Prefab;
                var tabName = prefabSystem.GetPrefabName(uge);
                if (tabName == moverName)
                {
                    uiGroupElementbuffer.RemoveAt(i);
                    //LogHelper.SendLog($"Removing {itemName} from {tabNameOld}");
                    break;
                }
            }

            //var tabNameNew = prefabSystem.GetPrefabName(newCat);
            EntityManager.GetBuffer<UIGroupElement>(newCat).Add(new UIGroupElement(moverEntity));
            EntityManager
                .GetBuffer<UnlockRequirement>(newCat)
                .Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
            //if (log)
            //LogHelper.SendLog($"Adding {itemName} to {tabNameNew}");
        }
    }
}
