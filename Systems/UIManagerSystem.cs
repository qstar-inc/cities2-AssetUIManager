using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Prefabs;
using Game.SceneFlow;
using Game;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Colossal.IO.AssetDatabase;

namespace AssetUIManager.Systems
{
    public partial class UIManagerSystem : GameSystemBase
    {
        public class AssetMenuData
        {
            public Dictionary<string, Entity> Menu { get; set; } = new Dictionary<string, Entity>();
            public Dictionary<string, int> Priority { get; set; } = new Dictionary<string, int>();
        }
        private EntityQuery CreateQuery(ComponentType[] all, ComponentType[] none = null)
        {
            if (none != null)
            {
                return GetEntityQuery(new EntityQueryDesc() { All = all, None = none });
            } else
            {
                return GetEntityQuery(new EntityQueryDesc() { All = all});
            }
        }

        private PrefabSystem prefabSystem;
        private EntityQuery uiAssetMenuDataQuery;
        private EntityQuery uiAssetCategoryDataQuery;
        private EntityQuery roadQuery;
        private EntityQuery bridgeQuery;
        private EntityQuery educationQuery;
        private EntityQuery parkQuery;
        //private EntityQuery zoneQuery;
        private static bool log;
        public static Dictionary<string, Entity> assetMenuDataDict = new();
        public static Dictionary<string, Entity> assetCatDataDict = new();
        public static Dictionary<string, int> roadMenuPriority = new();
        public static AssetMenuData pedStreetAssetMenuData = new();
        public static AssetMenuData bridgesAssetMenuData = new();
        public static AssetMenuData parkingRoadAssetMenuData = new();
        public static AssetMenuData schoolsAssetMenuData = new();
        public static AssetMenuData parksAssetMenuData = new();

        internal List<KeyValuePair<string, int>> GetRoadMenuPriority()
        {
            var sortedList = roadMenuPriority.OrderBy(pair => pair.Value).ToList();
            return sortedList;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            uiAssetMenuDataQuery = CreateQuery(new[] { ComponentType.ReadWrite<UIAssetMenuData>() });
            uiAssetCategoryDataQuery = CreateQuery(new[] { ComponentType.ReadWrite<UIAssetCategoryData>() });
            roadQuery = CreateQuery(new[] { ComponentType.ReadWrite<RoadData>() }, new[] { ComponentType.ReadWrite<BridgeData>() });
            bridgeQuery = CreateQuery(new[] { ComponentType.ReadWrite<BridgeData>(), ComponentType.ReadWrite<RoadData>() }, new[] { ComponentType.ReadWrite<TrackData>() });
            educationQuery = CreateQuery(new[] { ComponentType.ReadWrite<SchoolData>() });
            parkQuery = CreateQuery(new[] { ComponentType.ReadWrite<ParkData>() }, new[] { ComponentType.ReadWrite<ServiceUpgradeData>() });
            //zoneQuery = CreateQuery(new[] { ComponentType.ReadWrite<ZoneData>() });
            GameManager.instance.RegisterUpdater(() => CollectData());
        }

        public void CollectData()
        {
            try
            {
                var entities = uiAssetMenuDataQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    string prefabName = prefabSystem.GetPrefabName(entity);
                    if (!assetMenuDataDict.ContainsKey(prefabName))
                    {
                        assetMenuDataDict.Add(prefabName, entity);
                    }
                    if (prefabName == "Roads")
                    {
                        DynamicBuffer<UIGroupElement> assetMenuBuffer = EntityManager.GetBuffer<UIGroupElement>(entity);

                        for (int i = 0; i < assetMenuBuffer.Length; i++)
                        {
                            Entity uge = assetMenuBuffer[i].m_Prefab;
                            var name = prefabSystem.GetPrefabName(uge);
                            prefabSystem.TryGetPrefab(uge, out PrefabBase prefabBase);
                            prefabBase.TryGet(out UIObject ubj);
                            if (!roadMenuPriority.ContainsKey(name))
                            {
                                roadMenuPriority.Add(name, ubj.m_Priority);
                            }
                            if (log) Mod.log.Info($"Adding {name} to roadMenuPriority as {ubj.m_Priority}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Mod.log.Error(e);
            }
            
            try
            {
                var entities = uiAssetCategoryDataQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    string prefabName = prefabSystem.GetPrefabName(entity);
                    if (!assetCatDataDict.ContainsKey(prefabName))
                    {
                        assetCatDataDict.Add(prefabName, entity);
                    }
                    if (log) Mod.log.Info($"Adding {prefabName} to assetCatDataDict");
                }
            }
            catch (Exception e)
            {
                Mod.log.Error(e);
            }
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            Mod.m_Setting.PathwayPriorityDropdownVersion++;
            RefreshUI();
            //No();
        }

        public void RefreshUI()
        {
            CollectData();
            log = Mod.m_Setting.VerboseLogging;
            try {
                TogglePathway(Mod.m_Setting.PathwayInRoads, Mod.m_Setting.PathwayPriorityDropdown);
                ToggleSchool(Mod.m_Setting.SeparatedSchools);
                ProcessMovingAssets(Mod.m_Setting.BridgesInRoads, "StarQ_RoadsBridges", "Roads", "Media/Game/Icons/CableStayed.svg", 65, "", bridgeQuery, bridgesAssetMenuData, "component", new[] { "Hydroelectric_Power_Plant_01 Dam" });
                ProcessMovingAssets(Mod.m_Setting.ParkingRoadsInRoads, "StarQ_RoadsParkingRoads", "Roads", "Media/Game/Icons/TwolanePerpendicularparkingRoad.svg", 74, "Parking Lane", roadQuery, parksAssetMenuData, "lane", new[] { "Alley Oneway" });
                ProcessMovingAssets(Mod.m_Setting.SeparatedPocketParks, "StarQ_PocketParks", "Parks & Recreation", "coui://starq-asset-ui-manager/PocketParks.svg", 5, "", parkQuery, parksAssetMenuData, "component", Array.Empty<string>(), "PocketPark", "startsWith");
                ProcessMovingAssets(Mod.m_Setting.SeparatedCityParks, "StarQ_CityParks", "Parks & Recreation", "Media/Game/Icons/PropsPark.svg", 6, "", parkQuery, parksAssetMenuData, "component", Array.Empty<string>(), "CityPark", "startsWith");
            } catch (Exception ex) { Mod.log.Error(ex); }
            if (log) Mod.log.Info("Refresh Complete!");
        }

        public void TogglePathway(bool yes, int priority)
        {
            try
            {
                string Neighbor;
                if (yes)
                {
                    Neighbor = "RoadsSmallRoads";
                }
                else
                {
                    Neighbor = "Terraforming";
                    priority = 30;
                }

                string itemName = "Pathways";
                Entity itemValue = assetCatDataDict[itemName];
                if (EntityManager.TryGetComponent(itemValue, out PrefabData prefabData) && prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase) && prefabSystem.TryGetComponentData(prefabBase, out UIAssetCategoryData oldCat) && prefabSystem.TryGetComponentData(prefabBase, out UIObjectData uiObj))
                {
                    EntityManager.TryGetComponent(assetCatDataDict[Neighbor], out UIAssetCategoryData newCat);

                    RefreshBuffer(oldCat.m_Menu, newCat.m_Menu, "Pathways", itemValue);

                    uiObj.m_Priority = priority;
                    if (log) Mod.log.Info($"Moving Pathways to {Neighbor} at {priority}");

                    oldCat.m_Menu = newCat.m_Menu;
                    EntityManager.SetComponentData(itemValue, newCat);
                    EntityManager.SetComponentData(itemValue, uiObj);

                    bool pedInPath = Mod.m_Setting.PedestrianInPathway;
                    if (!yes)
                    {
                        pedInPath = false;
                    }
                    ProcessMovingAssets(pedInPath, "Pathways", "", "", 0, "Pedestrian Section", roadQuery, pedStreetAssetMenuData, "lane", Array.Empty<string>());
                }
            }
            catch (Exception e)
            {
                Mod.log.Error(e);
            }
        }

        public void ToggleSchool(bool yes)
        {

            if (yes)
            {
                Entity edu1Tab = CreateUIAssetCategoryPrefab("StarQ_Schools", "Education & Research", "coui://starq-asset-ui-manager/Edu1.svg", 1);
                Entity edu2Tab = CreateUIAssetCategoryPrefab("StarQ_HighSchools", "Education & Research", "coui://starq-asset-ui-manager/Edu2.svg", 2);
                Entity edu3Tab = CreateUIAssetCategoryPrefab("StarQ_Colleges", "Education & Research", "coui://starq-asset-ui-manager/Edu3.svg", 3);
                Entity edu4Tab = CreateUIAssetCategoryPrefab("StarQ_Universities", "Education & Research", "coui://starq-asset-ui-manager/Edu4.svg", 4);

                try
                {
                    Entity educationCat = assetCatDataDict["Education"];
                    var entities = educationQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);

                        if (!EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData) || !prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase) || !prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData uiObj) || uiObj.m_Group != educationCat || !prefabSystem.TryGetComponentData(assetPrefabBase, out SchoolData schoolData))
                            continue;

                        if (!schoolsAssetMenuData.Menu.ContainsKey(name))
                        {
                            schoolsAssetMenuData.Menu.Add(name, uiObj.m_Group);
                            schoolsAssetMenuData.Priority.Add(name, uiObj.m_Priority);
                        }

                        Entity eduTab = uiObj.m_Group;
                        //Entity eduAp = Entity.Null;

                        if (schoolData.m_EducationLevel == 1)
                        {
                            eduTab = edu1Tab;
                        }
                        else if (schoolData.m_EducationLevel == 2)
                        {
                            eduTab = edu2Tab;
                        }
                        else if (schoolData.m_EducationLevel == 3)
                        {
                            eduTab = edu3Tab;
                        }
                        else if (schoolData.m_EducationLevel == 4)
                        {
                            eduTab = edu4Tab;
                        }

                        RefreshBuffer(uiObj.m_Group, eduTab, name, entity);

                        uiObj.m_Group = eduTab;
                        EntityManager.SetComponentData(entity, uiObj);
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }

            }
            else
            {
                prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", "Schools"), out PrefabBase schoolTab);
                if (schoolTab != null)
                {
                    try
                    {
                        var entities = educationQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);
                            prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData uiObj);

                            if (!schoolsAssetMenuData.Menu.ContainsKey(name))
                            {
                                continue;
                            }

                            RefreshBuffer(uiObj.m_Group, schoolsAssetMenuData.Menu[name], name, entity);

                            uiObj.m_Group = schoolsAssetMenuData.Menu[name];
                            uiObj.m_Priority = schoolsAssetMenuData.Priority[name];
                            EntityManager.SetComponentData(entity, uiObj);

                        }
                    }
                    catch (Exception e)
                    {
                        Mod.log.Error(e);
                    }
                }
            }
        }

        public void ProcessMovingAssets(bool yes, string UIAssetCategoryName, string UIAssetMenuName, string UIAssetCategoryIcon, int UIAssetCategoryPriority, string sectionName, EntityQuery entityQuery, AssetMenuData assetMenuData, string processType, string[] excludeList, string includePattern = null, string includeType = null)
        {
            if (yes)
            {
                Entity tab = CreateUIAssetCategoryPrefab(UIAssetCategoryName, UIAssetMenuName, UIAssetCategoryIcon, UIAssetCategoryPriority);

                try
                {
                    var entities = entityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);
                        if (excludeList.Contains(name))
                            continue;
                        if (!EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData) || !prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase))
                            continue;
                        if (!prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject))
                            continue;

                        bool isValid = false;

                        if (processType == "lane")
                        {
                            DynamicBuffer<NetGeometrySection> x = EntityManager.GetBuffer<NetGeometrySection>(entity);
                            foreach (NetGeometrySection item in x)
                            {
                                try
                                {
                                    string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                    if (laneName.Contains(sectionName))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Mod.log.Error(e);
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
                                    if (includeType == "startsWith" && prefabName.StartsWith(includePattern))
                                    {
                                        isValid = true;
                                    }
                                    else if (includeType == "endsWith" && prefabName.EndsWith(includePattern))
                                    {
                                        isValid = true;
                                    }
                                    else if (includeType == "contains" && prefabName.Contains(includePattern))
                                    {
                                        isValid = true;
                                    }
                                    if (includePattern == "PocketPark" && prefabSystem.TryGetComponentData(assetPrefabBase, out BuildingData buildingData) && (buildingData.m_LotSize.x == 1 || buildingData.m_LotSize.y == 1))
                                    {    
                                        isValid = true;      
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Mod.log.Error(e);
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

                                if (EntityManager.TryGetComponent(currentTab, out PrefabData currentTabPrefabData) && prefabSystem.TryGetPrefab(currentTabPrefabData, out PrefabBase currentTabPrefabBase) && prefabSystem.TryGetComponentData(currentTabPrefabBase, out UIObjectData currentTabUIObject))
                                {
                                    RefreshBuffer(currentTab, tab, name, entity);
                                    int newPriority = (currentTabUIObject.m_Priority * 1000) + currentPriority;
                                    assetUIObject.m_Priority = newPriority;
                                    assetUIObject.m_Group = tab;
                                    EntityManager.SetComponentData(entity, assetUIObject);
                                }
                            }
                            catch (Exception e)
                            {
                                Mod.log.Error(e);
                            }
                        }
                        
                    }
                }
                catch (Exception e)
                {
                    Mod.log.Error(e);
                }

            }
            else
            {
                if (prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", UIAssetCategoryName), out PrefabBase tab))
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
                            if (!EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData) || prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase))
                                continue;
                            if (!prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject))
                                continue;
                            if (!assetMenuData.Menu.ContainsKey(name))
                                continue;

                            bool isValid = false;

                            if (processType == "lane")
                            {
                                DynamicBuffer<NetGeometrySection> x = EntityManager.GetBuffer<NetGeometrySection>(entity);

                                foreach (NetGeometrySection item in x)
                                {
                                    string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                    if (laneName.Contains(sectionName))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                            }
                            else if (processType == "component")
                            {
                                isValid = true;
                            }

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
                        Mod.log.Error(e);
                    }
                }
            }
        }

        public Entity CreateUIAssetCategoryPrefab(string name, string group, string icon, int priority)
        {

            if (!prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", name), out PrefabBase tab))
            {

                UIAssetCategoryPrefab menuPrefab = ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
                menuPrefab.name = name;
                EntityManager.TryGetComponent(assetMenuDataDict[group], out PrefabData prefabData);
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

        public void RefreshBuffer(Entity oldCat, Entity newCat, string moverName, Entity moverEntity)
        {
            DynamicBuffer<UIGroupElement> uiGroupElementbuffer = EntityManager.GetBuffer<UIGroupElement>(oldCat);

            var itemName = prefabSystem.GetPrefabName(moverEntity);
            for (int i = 0; i < uiGroupElementbuffer.Length; i++)
            {
                var uge = uiGroupElementbuffer[i].m_Prefab;
                var tabName = prefabSystem.GetPrefabName(uge);
                if (tabName == moverName)
                {
                    uiGroupElementbuffer.RemoveAt(i);
                    if (log) Mod.log.Info($"Removing {itemName} from {tabName}");
                    break;
                }
            }

            var tabNameNew = prefabSystem.GetPrefabName(newCat);
            EntityManager.GetBuffer<UIGroupElement>(newCat).Add(new UIGroupElement(moverEntity));
            EntityManager.GetBuffer<UnlockRequirement>(newCat).Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
            if (log) Mod.log.Info($"Adding {itemName} to {tabNameNew}");
        }

        protected override void OnUpdate() {}

        //public void No()
        //{
        //    var entities = zoneQuery.ToEntityArray(Allocator.Temp);

        //    Mod.log.Info($"---------------");
        //    foreach (Entity entity in entities)
        //    {
        //        string prefabName = prefabSystem.GetPrefabName(entity);
        //        //Mod.log.Info($"{prefabName}");
        //        if (EntityManager.TryGetComponent(entity, out PrefabData prefabData))
        //        {
        //            if (prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase))
        //            {
        //                List<ComponentBase> comps = prefabBase.components;
        //                var ZoneProperties = (Game.Prefabs.ZoneProperties)(comps.FirstOrDefault(c => c.name == "ZoneProperties"));
        //                if (ZoneProperties != null)
        //                {
        //                    //Mod.log.Info(ZoneProperties.m_ResidentialProperties);
        //                    //Mod.log.Info(ZoneProperties.m_SpaceMultiplier);
        //                    //Mod.log.Info(ZoneProperties.m_ResidentialProperties / ZoneProperties.m_SpaceMultiplier);
        //                    if (ZoneProperties.m_ResidentialProperties > 0)
        //                    {
        //                        if (!ZoneProperties.m_ScaleResidentials)
        //                        {
        //                            Mod.log.Info($"{prefabName} is Low Density Residential");
        //                            continue;
        //                        }
        //                        if (ZoneProperties.m_ResidentialProperties / ZoneProperties.m_SpaceMultiplier < 1f)
        //                        {

        //                            Mod.log.Info($"{prefabName} is Medium Density Residential");
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            Mod.log.Info($"{prefabName} is High Density Residential");
        //                            continue;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    Mod.log.Info($"---------------");
        //}
    }
}
