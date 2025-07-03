using System.Collections.Generic;
using Colossal;
using static Game.Prefabs.CharacterGroup;

namespace AssetUIManager
{
    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts
        )
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
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.Button)),
                    $"Save all changes set below."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.PathwayInRoads)),
                    "Pathway Tab In Roads Menu"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.PathwayInRoads)),
                    $"Move the Pathway tabs to the Roads menu instead of Terraforming."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.PathwayPriorityDropdown)),
                    "Pathway Tab Position"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.PathwayPriorityDropdown)),
                    $"Select the position of the Pathway tab."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.PedestrianInPathway)),
                    "Pedestrian Streets In Pathway tab"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.PedestrianInPathway)),
                    $"Move all Pedestrian Streets to the Pathway tab."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.BridgesInRoads)),
                    "Separate Bridges Tab In Roads Menu"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.BridgesInRoads)),
                    $"Move all Bridges to a new tab in the Roads Menu."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.QuaysInRoads)),
                    "Separate Quays Tab In Roads Menu"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.QuaysInRoads)),
                    $"Move all Quays to a new tab in the Roads Menu."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.ParkingRoadsInRoads)),
                    "Separate Parking Roads Tab In Roads Menu"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ParkingRoadsInRoads)),
                    $"Move all Parking Roads to a new tab in the Roads Menu."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedHospitals)),
                    "Separated Hospital Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedHospitals)),
                    $"Move all Hospitals to new tabs according to its function.\r\n- [BETA] Icons to be updated later."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparateControlAndResearch)),
                    "Separated Disease Control and Health Research Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparateControlAndResearch)),
                    $"Move Disease Control Centers and Health Research Institutes to own new tabs."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedSchools)),
                    "Separated Education Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedSchools)),
                    $"Move all Schools to new tabs according to the Education Level."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedPolice)),
                    "Separated Police/Prison Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedPolice)),
                    $"Move all Police buildings to new tabs according to its function.\r\n- [BETA] Icons to be updated later."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedPocketParks)),
                    "Separated Pocket Park Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedPocketParks)),
                    $"Move all \"Pocket Parks\" to new tabs in the Parks & Recreation menu."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparatedCityParks)),
                    "Separated City Park Tabs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparatedCityParks)),
                    $"Move all \"City Parks\" to new tabs in the Parks & Recreation menu."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableAssetPacks)),
                    "Asset Packs"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableAssetPacks)),
                    $"Add asset packs for the following types of assets:\r\n- Transport Depot\r\n- Public Transport\r\n- Cargo Transport\r\n- Transport Lane\r\nRequires reloading into the save for disabling."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.VerboseLogging)),
                    "Verbose Logging"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.VerboseLogging)),
                    $"Enable detailed logging for troubleshooting."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Mod Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NameText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Mod Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuthorText)), "Author" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AuthorText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BMaCLink)), "Buy Me a Coffee" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.BMaCLink)),
                    "Support the author."
                },
                { "SubServices.NAME[StarQ_UIC RoadsBridges]", "Bridges" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC RoadsBridges]",
                    "Roads that span across gaps, crossing waterways or valleys seamlessly."
                },
                { "SubServices.NAME[StarQ_UIC RoadsQuays]", "Quays" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC RoadsQuays]",
                    "Roads placed along water edges or shorelines, creating elevated borders that allow for waterfront development and vehicle access."
                },
                { "SubServices.NAME[StarQ_UIC RoadsParkingRoads]", "Parking Roads" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC RoadsParkingRoads]",
                    "Specialized roads with integrated parking spaces."
                },
                //StarQ_UIC Clinics", "Health & Deathcare", "Media/Game/Icons/Healthcare.svg", 1);
                //                Entity hospitalTab = CreateUIAssetCategoryPrefab("StarQ_UIC Hospital", "Health & Deathcare", "coui://starq-asset-ui-manager/Hospital.svg", 2);
                //                Entity diseaseTab = CreateUIAssetCategoryPrefab("StarQ_UIC DiseaseControl", "Health & Deathcare", "coui://starq-asset-ui-manager/DiseaseControl.svg", 3);
                //                Entity researchTab = CreateUIAssetCategoryPrefab("StarQ_UIC HealthResearch


                { "SubServices.NAME[StarQ_UIC Clinics]", "Medical Clinics" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Clinics]",
                    "Small healthcare facilities that provide basic medical services, treating minor illnesses and injuries to keep citizens healthy."
                },
                { "SubServices.NAME[StarQ_UIC Hospitals]", "Hospitals" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Hospitals]",
                    "Large medical centers equipped to handle emergencies, surgeries, and specialized treatments, ensuring comprehensive healthcare for the city."
                },
                { "SubServices.NAME[StarQ_UIC DiseaseControl]", "Disease Control Centers" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC DiseaseControl]",
                    "Specialized facilities focused on monitoring, preventing, and responding to outbreaks, keeping the population safe from widespread diseases."
                },
                { "SubServices.NAME[StarQ_UIC HealthResearch]", "Health Research Centers" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC HealthResearch]",
                    "Advanced institutions dedicated to medical research and innovation, developing new treatments and improving overall healthcare in the city."
                },
                { "SubServices.NAME[StarQ_UIC Schools]", "Elementary Schools" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Schools]",
                    "The first step in a child's education, elementary schools provide basic learning and help build the foundation for future studies."
                },
                { "SubServices.NAME[StarQ_UIC HighSchools]", "High Schools" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC HighSchools]",
                    "High schools offer a more advanced curriculum, preparing students for higher education or direct entry into the workforce."
                },
                { "SubServices.NAME[StarQ_UIC Colleges]", "Colleges" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Colleges]",
                    "Colleges provide specialized education and training, equipping students with the skills needed for various careers and industries."
                },
                { "SubServices.NAME[StarQ_UIC Universities]", "Universities" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Universities]",
                    "Universities are the pinnacle of education, offering a wide range of advanced degrees and research opportunities to shape the city's future professionals."
                },
                { "SubServices.NAME[StarQ_UIC LocalPolice]", "Local Police Departments" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC LocalPolice]",
                    "Local police stations serve as the backbone of law enforcement, responding to crimes, patrolling neighborhoods, and ensuring public safety in their districts."
                },
                { "SubServices.NAME[StarQ_UIC PoliceHQ]", "Police Headquarters" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC PoliceHQ]",
                    "The central hubs for law enforcement operations, coordinating citywide crime prevention."
                },
                { "SubServices.NAME[StarQ_UIC Intelligence]", "Intelligence Services" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Intelligence]",
                    "The covert agencies focused on gathering intelligence and preventing crime before they escalate."
                },
                { "SubServices.NAME[StarQ_UIC Prison]", "Prisons" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC Prison]",
                    "Secure facilities where convicted criminals serve their sentences, ensuring public safety by keeping dangerous offenders off the streets."
                },
                { "SubServices.NAME[StarQ_UIC PocketParks]", "Pocket Parks" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC PocketParks]",
                    "Small, compact spaces designed to enhance local neighborhood aesthetics and minimum recreation."
                },
                { "SubServices.NAME[StarQ_UIC CityParks]", "City Parks" },
                {
                    "Assets.SUB_SERVICE_DESCRIPTION[StarQ_UIC CityParks]",
                    "Recreational areas promoting relaxation, leisure activities, and community events."
                },
                { "Assets.NAME[StarQ_AP TransportDepot]", "Transport Depot" },
                { "Assets.DESCRIPTION[StarQ_AP TransportDepot]", "Transport Depot" },
                { "Assets.NAME[StarQ_AP PublicTransport]", "Transport Stops" },
                { "Assets.DESCRIPTION[StarQ_AP PublicTransport]", "Transport Stops" },
                { "Assets.NAME[StarQ_AP CargoTransport]", "Cargo Transport" },
                { "Assets.DESCRIPTION[StarQ_AP CargoTransport]", "Cargo Transport" },
                { "Assets.NAME[StarQ_AP TransportLane]", "Transport Lane" },
                { "Assets.DESCRIPTION[StarQ_AP TransportLane]", "Transport Lane" },
            };
        }

        public void Unload() { }
    }
}
