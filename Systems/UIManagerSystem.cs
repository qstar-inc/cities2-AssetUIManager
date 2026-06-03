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
using static Colossal.AssetPipeline.Diagnostic.Report;

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
            //DataCollectionSystem.CollectData();
            RefreshUI();
            //    return;
            //}

            //DisableUI();
        }

        public void RefreshUI()
        {
            if (!WorldHelper.IsGame)
            {
                LogHelper.SendLog("Not in game, skipping UI refresh");
                return;
            }

            if (!ContentSystem.EntitiesAssigned)
            {
                LogHelper.SendLog("Entities not assigned yet, skipping UI refresh");
                return;
            }

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
                    AUM_Contents.RoadsBridges,
                    "",
                    bridgeQuery,
                    bridgesAssetMenuData,
                    "component",
                    new[] { "Hydroelectric_Power_Plant_01 Dam" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.QuaysInRoads,
                    AUM_Contents.PiersAndQuays,
                    "",
                    bridgeQuery,
                    bridgesAssetMenuData,
                    "component",
                    new[] { "Hydroelectric_Power_Plant_01 Dam" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.ParkingRoadsInRoads,
                    AUM_Contents.RoadsParkingRoads,
                    "Parking Lane",
                    roadQuery,
                    parksAssetMenuData,
                    "lane",
                    new[] { "Alley Oneway" }
                );
                ProcessMovingAssets(
                    Mod.m_Setting.SeparatedPocketParks,
                    AUM_Contents.PocketParks,
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
                    AUM_Contents.CityParks,
                    "",
                    parkQuery,
                    parksAssetMenuData,
                    "component",
                    Array.Empty<string>(),
                    "CityPark",
                    "startsWith"
                );
                CleanUpServiceUpgrade();
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
        //            "StarQ AUM UIC RoadsBridges",
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
        //            "StarQ AUM UIC RoadsParkingRoads",
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
        //            "StarQ AUM UIC PocketParks",
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
        //            "StarQ AUM UIC CityParks",
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
                AUM_Content Neighbor;
                if (yes)
                {
                    Neighbor = AUM_Contents.RoadsSmallRoads;
                }
                else
                {
                    Neighbor = AUM_Contents.Terraforming;
                    priority = type == PathTypes.Pathways ? 30 : 31;
                }

                AUM_Content content =
                    type == PathTypes.PiersAndQuays ? AUM_Contents.PiersAndQuays
                    : type == PathTypes.BikePaths ? AUM_Contents.BikePaths
                    : AUM_Contents.Pathways;
                if (
                    !EntityManager.TryGetComponent(content.Entity, out PrefabData prefabData)
                    || !prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase)
                    || !prefabSystem.TryGetComponentData(prefabBase, out UIAssetCategoryData oldCat)
                    || !prefabSystem.TryGetComponentData(prefabBase, out UIObjectData uiObj)
                    || !EntityManager.TryGetComponent(
                        Neighbor.Entity,
                        out UIAssetCategoryData newCat
                    )
                )
                    return;

                RefreshBuffer(oldCat.m_Menu, newCat.m_Menu, content.Name, content.Entity);

                uiObj.m_Priority = priority;
                //if (log)
                //LogHelper.SendLog($"Moving {itemName} to {Neighbor} at {priority}");

                oldCat.m_Menu = newCat.m_Menu;
                EntityManager.SetComponentData(content.Entity, newCat);
                EntityManager.SetComponentData(content.Entity, uiObj);

                if (type != PathTypes.BikePaths)
                {
                    bool pedInPath = Mod.m_Setting.PedestrianInPathway;
                    if (!yes)
                        pedInPath = false;

                    if (type != PathTypes.PiersAndQuays)
                        ProcessMovingAssets(
                            pedInPath,
                            content,
                            "Pedestrian Section",
                            roadQuery,
                            pedStreetAssetMenuData,
                            "lane",
                            Array.Empty<string>()
                        );
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
                Entity clinicTab = AUM_Contents.Clinics.Entity;
                Entity hospitalTab = AUM_Contents.Hospitals.Entity;
                Entity diseaseTab = AUM_Contents.DiseaseControls.Entity;
                Entity researchTab = AUM_Contents.HealthResearchCenters.Entity;
                Entity mergedControlAndResearchTab = AUM_Contents.HealthResearchCenters.Entity;

                if (
                    clinicTab == Entity.Null
                    || hospitalTab == Entity.Null
                    || diseaseTab == Entity.Null
                    || researchTab == Entity.Null
                    || mergedControlAndResearchTab == Entity.Null
                )
                {
                    LogHelper.CheckNull(clinicTab, "Clinic Tab", "Not found");
                    LogHelper.CheckNull(hospitalTab, "Hospital Tab", "Not found");
                    LogHelper.CheckNull(diseaseTab, "Disease Control Tab", "Not found");
                    LogHelper.CheckNull(researchTab, "Health Research Tab", "Not found");
                    LogHelper.CheckNull(
                        mergedControlAndResearchTab,
                        "Merged Control and Research Tab",
                        "Not found"
                    );
                    return;
                }

                try
                {
                    Entity hospitalCat = AUM_Contents.Healthcare.Entity;
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
                try
                {
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
                            || !hospitalsAssetMenuData.Menu.ContainsKey(name)
                        )
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

        public void ToggleSchool(bool yes)
        {
            if (yes)
            {
                Entity edu1Tab = AUM_Contents.Schools.Entity;
                Entity edu2Tab = AUM_Contents.Highschools.Entity;
                Entity edu3Tab = AUM_Contents.Colleges.Entity;
                Entity edu4Tab = AUM_Contents.Universities.Entity;

                if (
                    edu1Tab == Entity.Null
                    || edu2Tab == Entity.Null
                    || edu3Tab == Entity.Null
                    || edu4Tab == Entity.Null
                )
                {
                    LogHelper.CheckNull(edu1Tab, "Education Tab 1", "Not found");
                    LogHelper.CheckNull(edu2Tab, "Education Tab 2", "Not found");
                    LogHelper.CheckNull(edu3Tab, "Education Tab 3", "Not found");
                    LogHelper.CheckNull(edu4Tab, "Education Tab 4", "Not found");
                    return;
                }

                try
                {
                    Entity educationCat = AUM_Contents.Education.Entity;
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
                try
                {
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
                            || !schoolsAssetMenuData.Menu.ContainsKey(name)
                        )
                            continue;

                        RefreshBuffer(uiObj.m_Group, schoolsAssetMenuData.Menu[name], name, entity);

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

        public void TogglePolice(bool yes)
        {
            if (yes)
            {
                Entity localPD = AUM_Contents.LocalPolices.Entity;
                Entity hqTab = AUM_Contents.PoliceHQs.Entity;
                Entity intelTab = AUM_Contents.Intelligences.Entity;
                Entity prisonTab = AUM_Contents.Prisons.Entity;

                if (
                    localPD == Entity.Null
                    || hqTab == Entity.Null
                    || intelTab == Entity.Null
                    || prisonTab == Entity.Null
                )
                {
                    LogHelper.CheckNull(localPD, "Local PD Tab", "Not found");
                    LogHelper.CheckNull(hqTab, "Police HQ Tab", "Not found");
                    LogHelper.CheckNull(intelTab, "Intelligence Tab", "Not found");
                    LogHelper.CheckNull(prisonTab, "Prison Tab", "Not found");
                    return;
                }
                try
                {
                    Entity policeCat = AUM_Contents.Police.Entity;
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
                try
                {
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
                            || !policeAssetMenuData.Menu.ContainsKey(name)
                        )
                            continue;

                        RefreshBuffer(uiObj.m_Group, policeAssetMenuData.Menu[name], name, entity);

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

        public void ProcessMovingAssets(
            bool enabled,
            AUM_Content content,
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
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData assetUIObject
                            )
                        )
                            continue;

                        bool isValid = false;

                        if (
                            content.Name == AUM_Contents.PiersAndQuays.Name
                            || content.Name == AUM_Contents.RoadsBridges.Name
                        )
                        {
                            assetPrefabBase.TryGet(out Bridge bridgeData);
                            if (
                                bridgeData.m_BuildStyle == BridgeBuildStyle.Quay
                                && content.Name == AUM_Contents.PiersAndQuays.Name
                            )
                                isValid = true;
                            else if (
                                bridgeData.m_BuildStyle != BridgeBuildStyle.Quay
                                && content.Name == AUM_Contents.RoadsBridges.Name
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
                                    !EntityManager.TryGetComponent(
                                        currentTab,
                                        out PrefabData currentTabPrefabData
                                    )
                                    || !prefabSystem.TryGetPrefab(
                                        currentTabPrefabData,
                                        out PrefabBase currentTabPrefabBase
                                    )
                                    || !prefabSystem.TryGetComponentData(
                                        currentTabPrefabBase,
                                        out UIObjectData currentTabUIObject
                                    )
                                )
                                    continue;

                                RefreshBuffer(currentTab, content.Entity, name, entity);
                                int newPriority =
                                    (currentTabUIObject.m_Priority * 1000) + currentPriority;
                                assetUIObject.m_Priority = newPriority;
                                assetUIObject.m_Group = content.Entity;
                                EntityManager.SetComponentData(entity, assetUIObject);
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
                            || prefabSystem.TryGetPrefab(
                                assetPrefabData,
                                out PrefabBase assetPrefabBase
                            )
                            || !prefabSystem.TryGetComponentData(
                                assetPrefabBase,
                                out UIObjectData assetUIObject
                            )
                            || !assetMenuData.Menu.ContainsKey(name)
                        )
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
                            RefreshBuffer(content.Entity, assetMenuData.Menu[name], name, entity);

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

        //public Entity CreateUIAssetCategoryPrefab(
        //    FixedString64Bytes name,
        //    FixedString64Bytes group,
        //    string icon,
        //    int priority
        //)
        //{
        //    if (
        //        !prefabSystem.TryGetPrefab(
        //            new PrefabID(nameof(UIAssetCategoryPrefab), name.ToString()),
        //            out PrefabBase tab
        //        )
        //    )
        //    {
        //        UIAssetCategoryPrefab menuPrefab =
        //            ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
        //        menuPrefab.name = name.ToString();

        //        DataCollectionSystem.assetMenuDataDict.TryGetValue(group, out Entity groupEntity);
        //        EntityManager.TryGetComponent(groupEntity, out PrefabData prefabData);
        //        prefabSystem.TryGetPrefab(prefabData, out PrefabBase roadMenu);

        //        menuPrefab.m_Menu = roadMenu.GetComponent<UIAssetMenuPrefab>();
        //        UIObject MenuUI = menuPrefab.AddOrGetComponent<UIObject>();
        //        MenuUI.m_Icon = icon;
        //        MenuUI.m_Priority = priority;
        //        MenuUI.active = true;
        //        MenuUI.m_IsDebugObject = false;
        //        MenuUI.m_Group = null;

        //        EditorAssetCategoryOverride eaco =
        //            menuPrefab.AddOrGetComponent<EditorAssetCategoryOverride>();
        //        eaco.m_IncludeCategories = new List<string>()
        //        {
        //            "StarQ/_Utils/Asset UI Manager",
        //        }.ToArray();

        //        tab = menuPrefab;
        //        prefabSystem.AddOrUpdatePrefab(menuPrefab);
        //    }
        //    prefabSystem.TryGetEntity(tab, out Entity tabEntity);

        //    return tabEntity;
        //}

        public void RefreshBuffer(
            Entity oldCat,
            Entity newCat,
            FixedString64Bytes moverName,
            Entity moverEntity
        )
        {
            if (!EntityManager.Exists(oldCat) || !EntityManager.Exists(newCat))
                return;

            if (!EntityManager.HasBuffer<UIGroupElement>(oldCat))
                return;

            DynamicBuffer<UIGroupElement> oldBuffer = EntityManager.GetBuffer<UIGroupElement>(
                oldCat
            );

            for (int i = oldBuffer.Length - 1; i >= 0; i--)
            {
                Entity prefab = oldBuffer[i].m_Prefab;
                if (!EntityManager.Exists(prefab))
                    continue;
                if (prefabSystem.GetPrefabName(prefab) == moverName)
                {
                    oldBuffer.RemoveAt(i);
                    break;
                }
            }

            DynamicBuffer<UIGroupElement> newBuffer = EntityManager.GetBuffer<UIGroupElement>(
                newCat
            );

            bool alreadyExists = false;
            for (int i = 0; i < newBuffer.Length; i++)
            {
                if (newBuffer[i].m_Prefab == moverEntity)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                newBuffer.Add(new UIGroupElement(moverEntity));

                if (EntityManager.HasBuffer<UnlockRequirement>(newCat))
                    EntityManager
                        .GetBuffer<UnlockRequirement>(newCat)
                        .Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
            }
        }

        private void RefreshBuffer(Entity oldCat, string moverName)
        {
            if (!EntityManager.Exists(oldCat))
                return;

            if (!EntityManager.HasBuffer<UIGroupElement>(oldCat))
                return;

            DynamicBuffer<UIGroupElement> buffer = EntityManager.GetBuffer<UIGroupElement>(oldCat);

            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                Entity prefab = buffer[i].m_Prefab;
                if (!EntityManager.Exists(prefab))
                    continue;
                if (prefabSystem.GetPrefabName(prefab) == moverName)
                {
                    buffer.RemoveAt(i);
                    break;
                }
            }
        }

        private void CleanUpServiceUpgrade()
        {
            EntityQuery serviceUpgQuery = SystemAPI
                .QueryBuilder()
                .WithAll<ServiceUpgradeData, UIObjectData>()
                .Build();
            using var upgEntities = serviceUpgQuery.ToEntityArray(Allocator.Temp);

            Dictionary<Entity, List<string>> groupsToClean = new();
            foreach (Entity entity in upgEntities)
            {
                if (
                    EntityManager.TryGetComponent(entity, out UIObjectData uio)
                    && uio.m_Group != Entity.Null
                )
                {
                    try
                    {
                        string moverName = prefabSystem.GetPrefabName(entity);
                        if (!groupsToClean.ContainsKey(uio.m_Group))
                            groupsToClean[uio.m_Group] = new List<string>();
                        groupsToClean[uio.m_Group].Add(moverName);
                        uio.m_Group = Entity.Null;
                        EntityManager.SetComponentData(entity, uio);
                    }
                    catch (Exception e)
                    {
                        LogHelper.SendLog(e, LogLevel.Error);
                    }
                }
            }

            foreach (var entityGroup in groupsToClean)
            {
                foreach (var item in entityGroup.Value)
                    RefreshBuffer(entityGroup.Key, item);
            }
        }
    }
}
