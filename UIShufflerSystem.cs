using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Prefabs;
using Game;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AssetUIShuffler
{
    public partial class UIShufflerSystem : GameSystemBase
    {

        private PrefabSystem prefabSystem;
        private EntityQuery uiAssetMenuDataQuery;
        private EntityQuery uiAssetCategoryDataQuery;
        private EntityQuery pedStreetQuery;
        private EntityQuery bridgeQuery;
        private EntityQuery educationQuery;
        public static Dictionary<string, Entity> assetMenuDataDict = [];
        public static Dictionary<string, Entity> assetCatDataDict = [];
        public static Dictionary<string, int> roadMenuPriority = [];
        public static Dictionary<string, Entity> pedStreetOGmenu = [];
        public static Dictionary<string, int> pedStreetOGpriority = [];
        public static Dictionary<string, Entity> bridgesOGmenu = [];
        public static Dictionary<string, int> bridgesOGpriority = [];
        public static Dictionary<string, Entity> schoolsOGmenu = [];
        public static Dictionary<string, int> schoolsOGpriority = [];

        //private static WaterSystem waterSystem;

        internal List<KeyValuePair<string, int>> GetRoadMenuPriority()
        {
            var sortedList = roadMenuPriority.OrderBy(pair => pair.Value).ToList();
            return sortedList;
        }

        protected override void OnCreate()
        {
            
            base.OnCreate();

            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            uiAssetMenuDataQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadWrite<UIAssetMenuData>()
                    ],
            });
            uiAssetCategoryDataQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadWrite<UIAssetCategoryData>()
                    ],
            });
            pedStreetQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadWrite<RoadData>()
                    ],
                None = [
                    ComponentType.ReadWrite<BridgeData>()
                    ]
            });
            bridgeQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadWrite<BridgeData>()
                    ],
            });
            educationQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadWrite<SchoolData>()
                    ],
            });

            try
            {
                var entities = uiAssetMenuDataQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    string prefabName = prefabSystem.GetPrefabName(entity);
                    assetMenuDataDict.Add(prefabName, entity);

                    if (prefabName == "Roads")
                    {
                        DynamicBuffer<UIGroupElement> assetMenuBuffer = EntityManager.GetBuffer<UIGroupElement>(entity);

                        for (int i = 0; i < assetMenuBuffer.Length; i++)
                        {
                            Entity uge = assetMenuBuffer[i].m_Prefab;
                            var name = prefabSystem.GetPrefabName(uge);
                            prefabSystem.TryGetPrefab(uge, out PrefabBase prefabBase);
                            prefabBase.TryGet(out UIObject ubj);
                            roadMenuPriority.Add(name, ubj.m_Priority);
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
                    assetCatDataDict.Add(prefabName, entity);
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
            RefreshUI();
        }

        public void TogglePathway(bool yes, int priority)
        {
            try
            {
                string itemName = "Pathways";
                Entity itemValue = assetCatDataDict[itemName];
                EntityManager.TryGetComponent(itemValue, out PrefabData prefabData);
                prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase);
                prefabSystem.TryGetComponentData(prefabBase, out UIAssetCategoryData oldCat);
                prefabSystem.TryGetComponentData(prefabBase, out UIObjectData uiObj);

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
                EntityManager.TryGetComponent(assetCatDataDict[Neighbor], out UIAssetCategoryData newCat);

                RefreshBuffer(oldCat.m_Menu, newCat.m_Menu, "Pathways", itemValue);

                uiObj.m_Priority = priority;

                oldCat.m_Menu = newCat.m_Menu;
                EntityManager.SetComponentData(itemValue, oldCat);
                EntityManager.SetComponentData(itemValue, uiObj);

                if (Mod.m_Setting.PedestrianInPathway && yes)
                {
                    try
                    {
                        var entities = pedStreetQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);

                            DynamicBuffer<NetGeometrySection> x = EntityManager.GetBuffer<NetGeometrySection>(entity);

                            bool isValidPedStreet = false; 
                            foreach (NetGeometrySection item in x)
                            {
                                string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                if (laneName.Contains("Pedestrian Section"))
                                {
                                    isValidPedStreet = true;
                                    break;
                                }
                            }

                            if (isValidPedStreet)
                            {
                                prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject);

                                if (!pedStreetOGmenu.ContainsKey(name))
                                {
                                    pedStreetOGmenu.Add(name, assetUIObject.m_Group);
                                    pedStreetOGpriority.Add(name, assetUIObject.m_Priority);
                                }

                                RefreshBuffer(assetUIObject.m_Group, itemValue, name, entity);

                                EntityManager.TryGetComponent(entity, out PrefabData assetMenuPrefabData);
                                prefabSystem.TryGetPrefab(assetMenuPrefabData, out PrefabBase assetMenuPrefabBase);
                                prefabSystem.TryGetComponentData(assetMenuPrefabBase, out UIObjectData assetMenuUIObj);

                                int newPriority = (assetMenuUIObj.m_Priority * 1000) + assetUIObject.m_Priority;
                                Mod.log.Info($"{assetMenuUIObj.m_Priority} * 1000 + {assetUIObject.m_Priority} = {newPriority}");
                                assetUIObject.m_Priority = newPriority;
                                assetUIObject.m_Group = itemValue;
                                EntityManager.SetComponentData(entity, assetUIObject);
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
                    try
                    {
                        var entities = pedStreetQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);

                            DynamicBuffer<NetGeometrySection> x = EntityManager.GetBuffer<NetGeometrySection>(entity);

                            bool isValidPedStreet = false;
                            foreach (NetGeometrySection item in x)
                            {
                                string laneName = prefabSystem.GetPrefabName(item.m_Section);
                                if (laneName.Contains("Pedestrian Section"))
                                {
                                    isValidPedStreet = true;
                                    break;
                                }
                            }

                            if (isValidPedStreet)
                            {
                                prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject);

                                if (!pedStreetOGmenu.ContainsKey(name))
                                {
                                    continue;
                                }

                                RefreshBuffer(itemValue, pedStreetOGmenu[name], name, entity);

                                assetUIObject.m_Priority = pedStreetOGpriority[name];
                                assetUIObject.m_Group = pedStreetOGmenu[name];
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
            catch (Exception e)
            {
                Mod.log.Error(e);
            }
        }

        public void RefreshUI()
        {
            try { TogglePathway(Mod.m_Setting.PathwayInRoads, Mod.m_Setting.PathwayPriorityDropdown); } catch (Exception ex) { Mod.log.Error(ex); }
            try { ToggleBridge(Mod.m_Setting.BridgesInRoads); } catch (Exception ex) { Mod.log.Error(ex); }
            try { ToggleSchool(Mod.m_Setting.SeparatedSchools); } catch (Exception ex) { Mod.log.Error(ex); }
            Mod.log.Info("Refresh Complete!");
        }

        public void ToggleBridge(bool yes)
        {

            if (yes)
            {
                Entity bridgeTab = CreateUIAssetCategoryPrefab("Bridges", "Roads", "Media/Game/Icons/CableStayed.svg", 65);

                try
                {
                    var entities = bridgeQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);
                        if (name != "Hydroelectric_Power_Plant_01 Dam")
                        {
                            EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                            prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);
                            prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject);

                            if (!bridgesOGmenu.ContainsKey(name))
                            {
                                bridgesOGmenu.Add(name, assetUIObject.m_Group);
                                bridgesOGpriority.Add(name, assetUIObject.m_Priority);
                            }

                            RefreshBuffer(assetUIObject.m_Group, bridgeTab, name, entity);

                            EntityManager.TryGetComponent(assetUIObject.m_Group, out PrefabData assetMenuPrefabData);
                            prefabSystem.TryGetPrefab(assetMenuPrefabData, out PrefabBase assetMenuPrefabBase);
                            prefabSystem.TryGetComponentData(assetMenuPrefabBase, out UIObjectData assetMenuUIObj);

                            int newPriority = (assetMenuUIObj.m_Priority * 100) + assetUIObject.m_Priority;
                            assetUIObject.m_Priority = newPriority;
                            assetUIObject.m_Group = bridgeTab;
                            EntityManager.SetComponentData(entity, assetUIObject);
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
                prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", "Bridges"), out PrefabBase bridgesTab);
                if (bridgesTab != null)
                {
                    try
                    {
                        var entities = bridgeQuery.ToEntityArray(Allocator.Temp);
                        foreach (Entity entity in entities)
                        {
                            var name = prefabSystem.GetPrefabName(entity);
                            if (name != "Hydroelectric_Power_Plant_01 Dam")
                            {
                                EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                                prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);
                                prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData uiObj);

                                if (!bridgesOGmenu.ContainsKey(name))
                                {
                                    continue;
                                }

                                RefreshBuffer(uiObj.m_Group, bridgesOGmenu[name], name, entity);

                                uiObj.m_Group = bridgesOGmenu[name];
                                uiObj.m_Priority = bridgesOGpriority[name];
                                EntityManager.SetComponentData(entity, uiObj);
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

        public void ToggleSchool(bool yes)
        {

            if (yes)
            {
                Entity edu1Tab = CreateUIAssetCategoryPrefab("Schools", "Education & Research", "Media/Game/Icons/Education.svg", 1);
                Entity edu2Tab = CreateUIAssetCategoryPrefab("High Schools", "Education & Research", "Media/Game/Icons/Education.svg", 2);
                Entity edu3Tab = CreateUIAssetCategoryPrefab("Colleges", "Education & Research", "Media/Game/Icons/Education.svg", 3);
                Entity edu4Tab = CreateUIAssetCategoryPrefab("Universities", "Education & Research", "Media/Game/Icons/Education.svg", 4);

                try
                {
                    Entity educationCat = assetCatDataDict["Education"];
                    var entities = educationQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity entity in entities)
                    {
                        var name = prefabSystem.GetPrefabName(entity);
                        
                        EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData);
                        prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase);
                        prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData uiObj);
                        prefabSystem.TryGetComponentData(assetPrefabBase, out SchoolData schoolData);

                        if (uiObj.m_Group != educationCat)
                        {
                            continue;
                        }

                        if (!schoolsOGmenu.ContainsKey(name))
                        {
                            schoolsOGmenu.Add(name, uiObj.m_Group);
                            schoolsOGpriority.Add(name, uiObj.m_Priority);
                        }

                        Entity eduTab = uiObj.m_Group;

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
                        } else if (schoolData.m_EducationLevel == 4)
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

                            if (!schoolsOGmenu.ContainsKey(name))
                            {
                                continue;
                            }

                            RefreshBuffer(uiObj.m_Group, schoolsOGmenu[name], name, entity);

                            uiObj.m_Group = schoolsOGmenu[name];
                            uiObj.m_Priority = schoolsOGpriority[name];
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

        public Entity CreateUIAssetCategoryPrefab(string name, string group, string icon, int priority)
        {

            prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", name), out PrefabBase edu4);
            if (edu4 == null)
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
                edu4 = menuPrefab;
            }
            prefabSystem.TryGetEntity(edu4, out Entity edu4Tab);

            return edu4Tab;
        }

        public void RefreshBuffer(Entity oldCat, Entity newCat, string moverName, Entity moverEntity)
        {
            DynamicBuffer<UIGroupElement> uiGroupElementbuffer = EntityManager.GetBuffer<UIGroupElement>(oldCat);

            for (int i = 0; i < uiGroupElementbuffer.Length; i++)
            {
                var uge = uiGroupElementbuffer[i].m_Prefab;
                var name = prefabSystem.GetPrefabName(uge);
                Mod.log.Info(name);
                if (name == moverName)
                {
                    uiGroupElementbuffer.RemoveAt(i);
                    break;
                }
            }

            EntityManager.GetBuffer<UIGroupElement>(newCat).Add(new UIGroupElement(moverEntity));
            EntityManager.GetBuffer<UnlockRequirement>(newCat).Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
