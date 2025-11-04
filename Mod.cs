using System;
using System.Collections.Generic;
using System.Reflection;
using AssetUIManager.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI;
using Game;
using Game.Modding;
using Game.SceneFlow;
using StarQ.Shared.Extensions;
using Unity.Entities;

namespace AssetUIManager
{
    public class Mod : IMod
    {
        public static string Id = nameof(AssetUIManager);
        public static string Name = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            .Title;
        public static string Version = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString(3);

        public static ILog log = LogManager.GetLogger($"{Id}").SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        public static string uiHostName = "starq-asset-ui-manager";

        public void OnLoad(UpdateSystem updateSystem)
        {
            LogHelper.Init(Id, log);
            LocaleHelper.Init(Id, Name, GetReplacements, AddLocale);
            UIHostHelper.Init(Id, uiHostName);

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                UIHostHelper.LoadUIHost(asset);

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings(nameof(AssetUIManager), m_Setting, new Setting(this));

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<DataCollectionSystem>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UIManagerSystem>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AssetPackSystem>();
        }

        public void OnDispose()
        {
            //    if (DataCollectionSystem.assetMenuDataDict.IsCreated)
            //        DataCollectionSystem.assetMenuDataDict.Dispose();
            //    if (DataCollectionSystem.assetCatDataDict.IsCreated)
            //        DataCollectionSystem.assetCatDataDict.Dispose();
            //log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
            UIManager.defaultUISystem.RemoveHostLocation(uiHostName);
        }

        public static Dictionary<string, string> GetReplacements()
        {
            return new Dictionary<string, string>()
            {
                { "Pathway", LocaleHelper.GetSubserviceName("Pathways") },
            };
        }

        private static void AddLocale()
        {
            Dictionary<string, List<string>> XinXMenu = new()
            {
                {
                    "PathwayInRoads",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("Pathways"),
                        LocaleHelper.GetServiceName("Roads"),
                        LocaleHelper.GetServiceName("Landscaping"),
                    }
                },
                {
                    "QuaysInRoads",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("PiersAndQuays"),
                        LocaleHelper.GetServiceName("Roads"),
                        LocaleHelper.GetServiceName("Landscaping"),
                    }
                },
            };

            var XinXMenu_Label = LocaleHelper.Translate($"{Id}.Mod.XinXMenu_Label");
            var XinXMenu_Desc = LocaleHelper.Translate($"{Id}.Mod.XinXMenu_Desc");

            Dictionary<string, List<string>> SeparatedTabsInMenu = new()
            {
                {
                    "BridgesInRoads",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("StarQ_UIC RoadsBridges"),
                        LocaleHelper.GetServiceName("Roads"),
                    }
                },
                {
                    "ParkingRoadsInRoads",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("StarQ_UIC RoadsParkingRoads"),
                        LocaleHelper.GetServiceName("Roads"),
                    }
                },
                {
                    "SeparatedPocketParks",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("StarQ_UIC PocketParks"),
                        LocaleHelper.GetServiceName("Parks & Recreation"),
                    }
                },
                {
                    "SeparatedCityParks",
                    new List<string>()
                    {
                        LocaleHelper.GetSubserviceName("StarQ_UIC CityParks"),
                        LocaleHelper.GetServiceName("Parks & Recreation"),
                    }
                },
            };

            var SeparatedTabsInMenu_Label = LocaleHelper.Translate(
                $"{Id}.Mod.SeparatedTabsInMenu_Label"
            );
            var SeparatedTabsInMenu_Desc = LocaleHelper.Translate(
                $"{Id}.Mod.SeparatedTabsInMenu_Desc"
            );

            Dictionary<string, List<string>> SeparatedTabs = new()
            {
                {
                    "SeparatedHospitals",
                    new List<string>() { LocaleHelper.GetSubserviceName("Healthcare") }
                },
                {
                    "SeparatedSchools",
                    new List<string>() { LocaleHelper.GetSubserviceName("Education") }
                },
                {
                    "SeparatedPolice",
                    new List<string>() { LocaleHelper.GetSubserviceName("Police") }
                },
            };

            var SeparatedTabs_Label = LocaleHelper.Translate($"{Id}.Mod.SeparatedTabs_Label");
            var SeparatedTabs_Desc = LocaleHelper.Translate($"{Id}.Mod.SeparatedTabs_Desc");

            LoopDict(XinXMenu, XinXMenu_Label, XinXMenu_Desc);
            LoopDict(SeparatedTabsInMenu, SeparatedTabsInMenu_Label, SeparatedTabsInMenu_Desc);
            LoopDict(SeparatedTabs, SeparatedTabs_Label, SeparatedTabs_Desc);
        }

        private static void LoopDict(
            Dictionary<string, List<string>> dict,
            string label,
            string desc
        )
        {
            foreach (var item in dict)
            {
                var itemValue = item.Value;
                try
                {
                    string v1 = itemValue.Count > 0 ? itemValue[0] ?? "{1}" : "{1}";
                    string v2 = itemValue.Count > 1 ? itemValue[1] ?? "{2}" : "{2}";
                    string v3 = itemValue.Count > 2 ? itemValue[2] ?? "{3}" : "{3}";

                    LocaleHelper.AddLocalization(
                        LocaleHelper.GetOptionsLabelLocaleId(item.Key),
                        label.Replace("{1}", v1).Replace("{2}", v2).Replace("{3}", v3)
                    );
                    LocaleHelper.AddLocalization(
                        LocaleHelper.GetOptionsDescLocaleId(item.Key),
                        desc.Replace("{1}", v1).Replace("{2}", v2).Replace("{3}", v3)
                    );
                }
                catch (Exception ex)
                {
                    LogHelper.SendLog(ex, LogLevel.Error);
                }
            }
        }
    }
}
