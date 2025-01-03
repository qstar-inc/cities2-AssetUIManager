using AssetUIManager.Systems;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Unity.Entities;
using UnityEngine.Device;

namespace AssetUIManager
{
    [FileLocation("ModsSettings\\StarQ\\"+nameof(AssetUIManager))]
    [SettingsUITabOrder(OptionsTab, AboutTab)]
    [SettingsUIGroupOrder(ButtonGroup, OptionsGroup, InfoGroup)]
    //[SettingsUIShowGroupName(OptionsGroup)]
    public class Setting : ModSetting
    {
        private static readonly UIManagerSystem uiO = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UIManagerSystem>();
        private static readonly AssetPackSystem apS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AssetPackSystem>();

        public const string OptionsTab = "Options";
        public const string ButtonGroup = "Save Changes";
        public const string OptionsGroup = "Options";

        public const string AboutTab = "About";
        public const string InfoGroup = "Info";

        public Setting(IMod mod) : base(mod)
        {
        }

        [SettingsUISection(OptionsTab, ButtonGroup)]
        public bool Button
        {
            set
            {
                if (!PathwayInRoads) PedestrianInPathway = false;
                uiO.RefreshUI();
                apS.RefreshUI();
            }
        }


        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool PathwayInRoads { get; set; } = true;

        [SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool PedestrianInPathway { get; set; } = true;

        [SettingsUIHidden]
        public int PathwayPriorityDropdownVersion { get; set; } = 0;

        //[SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        [SettingsUIDropdown(typeof(Setting), nameof(GetPathwayPriorityDropdownItems))]
        [SettingsUIValueVersion(typeof(Setting), nameof(PathwayPriorityDropdownVersion))]
        [SettingsUISection(OptionsTab, OptionsGroup)]
        public int PathwayPriorityDropdown { get; set; } = 74;

        public DropdownItem<int>[] GetPathwayPriorityDropdownItems()
        {
            var items = new List<DropdownItem<int>>();
            foreach (var item in uiO.GetRoadMenuPriority())
            {
                
                string input = item.Key; Mod.log.Info(input);
                string withoutPrefix = Regex.Replace(input, @"^(Roads)+", "");
                string result = Regex.Replace(withoutPrefix, @"(?<!^)([A-Z])", " $1");
                items.Add(new DropdownItem<int>()
                {
                    value = item.Value+1,
                    displayName = $"After {result}",
                });
            }
            return items.ToArray();
        }

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool BridgesInRoads { get; set; } = true;

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool ParkingRoadsInRoads { get; set; } = true;

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool SeparatedSchools { get; set; } = true;

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool SeparatedPocketParks { get; set; } = true;

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool SeparatedCityParks { get; set; } = true;

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool EnableAssetPacks { get; set; } = true;

        [SettingsUISection(OptionsTab, InfoGroup)]
        public bool VerboseLogging { get; set; } = false;

        public override void SetDefaults()
        {
            PathwayInRoads = true;
            PathwayPriorityDropdown = 74;
            PathwayPriorityDropdownVersion = 0;
            BridgesInRoads = true;
            ParkingRoadsInRoads = true;
            SeparatedSchools = true;
            SeparatedPocketParks = true;
            SeparatedCityParks = true;
            EnableAssetPacks = true;
            VerboseLogging = false;
        }

        [SettingsUISection(AboutTab, InfoGroup)]
        public string NameText => Mod.Name;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string VersionText => Mod.Version;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string AuthorText => "StarQ";

        [SettingsUIButtonGroup("Social")]
        [SettingsUIButton]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool BMaCLink
        {
            set
            {
                try
                {
                    Application.OpenURL($"https://buymeacoffee.com/starq");
                }
                catch (Exception e)
                {
                    Mod.log.Info(e);
                }
            }
        }
    }
}
