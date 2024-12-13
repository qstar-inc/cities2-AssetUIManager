using System.Collections.Generic;
using Colossal;
using Game.Settings;
using Game.UI.Widgets;

namespace AssetUIShuffler
{
    public class LocaleEN(Setting setting) : IDictionarySource
    {
        private readonly Setting m_Setting = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), Mod.Name },
                { m_Setting.GetOptionTabLocaleID(Setting.OptionsTab), Setting.OptionsTab },
                { m_Setting.GetOptionGroupLocaleID(Setting.ButtonGroup), Setting.ButtonGroup },
                { m_Setting.GetOptionGroupLocaleID(Setting.OptionsGroup), Setting.OptionsGroup },

                { m_Setting.GetOptionTabLocaleID(Setting.AboutTab), Setting.AboutTab },
                { m_Setting.GetOptionGroupLocaleID(Setting.InfoGroup), Setting.InfoGroup },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Button)), "Save Changes" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Button)), $"Save all changes set below." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.PathwayInRoads)), "Pathway Tab In Roads Menu" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.PathwayInRoads)), $"Move the Pathway tabs to the Roads menu instead of Terraforming." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.PathwayPriorityDropdown)), "Pathway Tab Position" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.PathwayPriorityDropdown)), $"Select the position of the Pathway tab." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.PedestrianInPathway)), "Pedestrian Streets In Pathway tab" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.PedestrianInPathway)), $"Move all Pedestrian Streets to the Pathway tab." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BridgesInRoads)), "Separate Bridges Tab In Roads Menu" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BridgesInRoads)), $"Move all Bridges to a new tab in the Roads Menu." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ParkingRoadsInRoads)), "Separate Parking Roads Tab In Roads Menu" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ParkingRoadsInRoads)), $"Move all Parking Roads to a new tab in the Roads Menu." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedSchools)), "Separated Education Tabs" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedSchools)), $"Move all Schools to new tabs according to the Education Level." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedPocketParks)), "Separated Pocket Park Tabs" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedPocketParks)), $"Move all \"Pocket Parks\" to new tabs in the Parks & Recreatoion menu." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedCityParks)), "Separated City Park Tabs" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedCityParks)), $"Move all \"City Parks\" to new tabs in the Parks & Recreatoion menu." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Mod Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NameText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Mod Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuthorText)), "Author" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AuthorText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BMaCLink)), "Buy Me a Coffee" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BMaCLink)), "Support the author." },

                { "SubServices.NAME[StarQ_RoadsBridges]", "Bridges" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_RoadsBridges]", "Build bridges, whichever you like. //Subject to change" },
                { "SubServices.NAME[StarQ_RoadsParkingRoads]", "Parking Roads" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_RoadsParkingRoads]", "Build parking roads, whichever you like. //Subject to change" },

                { "SubServices.NAME[StarQ_Schools]", "Schools" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_Schools]", "Build primary/elementary schools, whichever you like. //Subject to change" },
                { "SubServices.NAME[StarQ_HighSchools]", "High Schools" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_HighSchools]", "Build high schools, whichever you like. //Subject to change" },
                { "SubServices.NAME[StarQ_Colleges]", "Colleges" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_Colleges]", "Build colleges, whichever you like. //Subject to change" },
                { "SubServices.NAME[StarQ_Universities]", "Universities" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_Universities]", "Build universities, whichever you like. //Subject to change" },

                { "SubServices.NAME[StarQ_PocketParks]", "Pocket Parks" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_PocketParks]", "Build pocket parks, whichever you like. //Subject to change" },
                { "SubServices.NAME[StarQ_CityParks]", "City Parks" },
                { "Assets.SUB_SERVICE_DESCRIPTION[StarQ_CityParks]", "Build city parks, whichever you like. //Subject to change" },

            };
        }

        public void Unload()
        {

        }
    }
}
