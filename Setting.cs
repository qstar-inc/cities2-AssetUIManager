using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Unity.Entities;
using UnityEngine.Device;

namespace AssetUIShuffler
{
    [FileLocation("ModsSettings\\StarQ\\"+nameof(AssetUIShuffler))]
    [SettingsUITabOrder(OptionsTab, AboutTab)]
    [SettingsUIGroupOrder(ButtonGroup, OptionsGroup, InfoGroup)]
    //[SettingsUIShowGroupName(OptionsGroup)]
    public class Setting(IMod mod) : ModSetting(mod)
    {
        private static readonly UIShufflerSystem uiO = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UIShufflerSystem>();

        public const string OptionsTab = "Options";
        public const string ButtonGroup = "Save Changes";
        public const string OptionsGroup = "Options";

        public const string AboutTab = "About";
        public const string InfoGroup = "Info";

        [SettingsUISection(OptionsTab, ButtonGroup)]
        public bool Button { set { uiO.RefreshUI(); } }

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool PathwayInRoads { get; set; }

        [SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool PedestrianInPathway { get; set; }

        [SettingsUIHidden]
        public int PathwayPriorityDropdownVersion { get; set; }

        [SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        [SettingsUIDropdown(typeof(Setting), nameof(GetPathwayPriorityDropdownItems))]
        [SettingsUIValueVersion(typeof(Setting), nameof(PathwayPriorityDropdownVersion))]
        [SettingsUISection(OptionsTab, OptionsGroup)]
        public int PathwayPriorityDropdown { get; set; }

        public DropdownItem<int>[] GetPathwayPriorityDropdownItems()
        {
            var items = new List<DropdownItem<int>>();

            foreach (var item in uiO.GetRoadMenuPriority())
            {
                string input = item.Key;
                string withoutPrefix = Regex.Replace(input, @"^(Roads)+", "");
                string result = Regex.Replace(withoutPrefix, @"(?<!^)([A-Z])", " $1");
                items.Add(new DropdownItem<int>()
                {
                    value = item.Value+1,

                    displayName = $"After {result}",
                });
            }
            return [.. items];
        }

        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool BridgesInRoads { get; set; }


        [SettingsUISection(OptionsTab, OptionsGroup)]
        public bool SeparatedSchools { get; set; }

        public override void SetDefaults()
        {
            PathwayInRoads = true;
            PathwayPriorityDropdown = 74;
            PathwayPriorityDropdownVersion = 0;
            BridgesInRoads = true;
            SeparatedSchools = true;
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
