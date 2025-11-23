using System.Collections.Generic;
using System.Text.RegularExpressions;
using AssetUIManager.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using StarQ.Shared.Extensions;
using Unity.Entities;

namespace AssetUIManager
{
    [FileLocation("ModsSettings\\StarQ\\" + nameof(AssetUIManager))]
    [SettingsUITabOrder(GeneralTab, AboutTab, LogTab)]
    [SettingsUIGroupOrder(RoadsGroup, ServiceGroup, InfoGroup)]
    public class Setting : ModSetting
    {
        public Setting(IMod mod)
            : base(mod) => SetDefaults();

        public const string GeneralTab = "GeneralTab";
        public const string RoadsGroup = "RoadsGroup";
        public const string ServiceGroup = "ServiceGroup";
        public const string AssetPackGroup = "AssetPackGroup";

        public const string AboutTab = "AboutTab";
        public const string InfoGroup = "InfoGroup";

        public const string LogTab = "LogTab";

        [Exclude]
        [SettingsUIHidden]
        public bool IsGame { get; set; } = false;

        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool BridgesInRoads { get; set; } = true;

        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool PathwayInRoads { get; set; } = true;

        [SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool PedestrianInPathway { get; set; } = true;

        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool BikewayInRoads { get; set; } = true;

        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool QuaysInRoads { get; set; } = true;

        [SettingsUISection(GeneralTab, RoadsGroup)]
        public bool ParkingRoadsInRoads { get; set; } = true;

        //[Exclude]
        //[SettingsUIHidden]
        //public int PathwayPriorityDropdownVersion { get; set; } = 0;

        ////[SettingsUIDisableByCondition(typeof(Setting), nameof(PathwayInRoads), true)]
        //[SettingsUIDropdown(typeof(Setting), nameof(GetPathwayPriorityDropdownItems))]
        //[SettingsUIValueVersion(typeof(Setting), nameof(PathwayPriorityDropdownVersion))]
        //[SettingsUISection(GeneralTab, GeneralGroup)]
        //public int PathwayPriorityDropdown { get; set; } = 74;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparatedHospitals { get; set; } = false;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparateControlAndResearch { get; set; } = false;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparatedSchools { get; set; } = true;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparatedPolice { get; set; } = false;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparatedPocketParks { get; set; } = true;

        [SettingsUISection(GeneralTab, ServiceGroup)]
        public bool SeparatedCityParks { get; set; } = true;

        [SettingsUISection(GeneralTab, AssetPackGroup)]
        public bool BaseGameAssetPacks { get; set; } = true;

        [SettingsUISection(GeneralTab, AssetPackGroup)]
        public bool EnableAssetPacks { get; set; } = true;

        //[SettingsUISection(GeneralTab, InfoGroup)]
        //public bool VerboseLogging { get; set; } = false;

        public override void SetDefaults()
        {
            //VerboseLogging = false;
            //PathwayPriorityDropdown = 74;
            //PathwayPriorityDropdownVersion = 0;
            BridgesInRoads = true;
            PathwayInRoads = true;
            QuaysInRoads = true;
            BikewayInRoads = true;
            ParkingRoadsInRoads = true;
            SeparatedHospitals = true;
            SeparateControlAndResearch = true;
            SeparatedSchools = true;
            SeparatedPolice = true;
            SeparatedPocketParks = true;
            SeparatedCityParks = true;
            BaseGameAssetPacks = true;
            EnableAssetPacks = true;
        }

        //public DropdownItem<int>[] GetPathwayPriorityDropdownItems()
        //{
        //    var items = new List<DropdownItem<int>>();
        //    bool firstDone = false;
        //    foreach (var item in UIManagerSystem.GetRoadMenuPriority())
        //    {
        //        string input = item.Key;
        //        string withoutPrefix = Regex.Replace(
        //            input.Replace("StarQ_UIC", ""),
        //            @"^(Roads)+",
        //            ""
        //        );
        //        string result = Regex.Replace(withoutPrefix, @"(?<!^)([A-Z])", " $1");
        //        if (!firstDone)
        //        {
        //            items.Add(
        //                new DropdownItem<int>()
        //                {
        //                    value = item.Value - 1,
        //                    displayName = $"Before {result}",
        //                }
        //            );
        //            firstDone = true;
        //        }
        //        items.Add(
        //            new DropdownItem<int>()
        //            {
        //                value = item.Value + 1,
        //                displayName = $"After {result}",
        //            }
        //        );
        //    }
        //    return items.ToArray();
        //}

        [SettingsUISection(AboutTab, InfoGroup)]
        public string NameText => Mod.Name;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string VersionText => VariableHelper.AddDevSuffix(Mod.Version);

        [SettingsUISection(AboutTab, InfoGroup)]
        public string AuthorText => VariableHelper.StarQ;

        [SettingsUIButton]
        [SettingsUIButtonGroup("Social")]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool BMaCLink
        {
            set => VariableHelper.OpenBMAC();
        }

        [SettingsUIButton]
        [SettingsUIButtonGroup("Social")]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool Discord
        {
            set => VariableHelper.OpenDiscord("1324716071311114351");
        }

        [SettingsUIMultilineText]
        [SettingsUIDisplayName(typeof(LogHelper), nameof(LogHelper.LogText))]
        [SettingsUISection(LogTab, "")]
        public string LogText => string.Empty;

        [Exclude]
        [SettingsUIHidden]
        public bool IsLogMissing
        {
            get => VariableHelper.CheckLog(Mod.Id);
        }

        [SettingsUIButton]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(IsLogMissing))]
        [SettingsUISection(LogTab, "")]
        public bool OpenLog
        {
            set => VariableHelper.OpenLog(Mod.Id);
        }
    }
}
