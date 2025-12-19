using System;
using Game;
using Game.Prefabs;
using StarQ.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;

namespace AssetUIManager.Systems
{
    public partial class DataCollectionSystem : GameSystemBase
    {
        private static bool DataCollected = false;
        private static EntityQuery uiAssetMenuDataQuery;
        private static EntityQuery uiAssetCategoryDataQuery;

        public static NativeHashMap<FixedString64Bytes, Entity> assetMenuDataDict;
        public static NativeHashMap<FixedString64Bytes, Entity> assetCatDataDict;

        protected override void OnCreate()
        {
            base.OnCreate();
            assetMenuDataDict = HashMapHelper.CreateHashMapStringEntity();
            assetCatDataDict = HashMapHelper.CreateHashMapStringEntity();
            uiAssetMenuDataQuery = SystemAPI.QueryBuilder().WithAll<UIAssetMenuData>().Build();
            uiAssetCategoryDataQuery = SystemAPI
                .QueryBuilder()
                .WithAll<UIAssetCategoryData>()
                .Build();

            ModHelper.AddAfterActivePlaysetOrModStatusChanged(
                AddAfterActivePlaysetOrModStatusChanged
            );
            CollectData();
        }

        protected override void OnUpdate() { }

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();
        //    if (assetMenuDataDict.IsCreated)
        //        assetMenuDataDict.Dispose();
        //    if (assetCatDataDict.IsCreated)
        //        assetCatDataDict.Dispose();
        //}

        public static void AddAfterActivePlaysetOrModStatusChanged()
        {
            assetMenuDataDict.Clear();
            assetCatDataDict.Clear();
            DataCollected = false;
            CollectData();
        }

        public static void CollectData()
        {
            PrefabSystem prefabSystem = WorldHelper.PrefabSystem;
            if (DataCollected)
                return;

            LogHelper.SendLog("Collecting Data", LogLevel.DEVD);

            try
            {
                using var menuEntities = uiAssetMenuDataQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in menuEntities)
                {
                    FixedString64Bytes prefabName = prefabSystem.GetPrefabName(entity);
                    assetMenuDataDict.TryAdd(prefabName, entity);
                }

                using var categoryEntities = uiAssetCategoryDataQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in categoryEntities)
                {
                    FixedString64Bytes prefabName = prefabSystem.GetPrefabName(entity);
                    assetCatDataDict.TryAdd(prefabName, entity);
                }
            }
            catch (Exception e)
            {
                LogHelper.SendLog(e, LogLevel.Error);
            }
            DataCollected = true;
            LogHelper.SendLog("Data Collection completed", LogLevel.DEVD);
        }
    }
}
