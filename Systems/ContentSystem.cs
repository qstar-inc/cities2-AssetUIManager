using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using StarQ.Shared.Extensions;
using Unity.Entities;

namespace AssetUIManager.Systems
{
    public enum CT
    {
        //AP,
        C,
        UIC,
    }

    public static class CTExtension
    {
        public static string GetName(this CT type)
        {
            return type switch
            {
                //CT.AP => nameof(AssetPackPrefab),
                CT.C => nameof(ContentPrefab),
                CT.UIC => nameof(UIAssetCategoryPrefab),
                _ => type.ToString(),
            };
        }
    }

    public sealed class AUM_Content
    {
        public string Name { get; }
        public string GUID { get; } = string.Empty;
        public bool Vanilla { get; } = false;
        public CT ContentType { get; }

        public Entity Entity { get; set; } = Entity.Null;
        public PrefabBase PrefabBase { get; set; } = null;

        public AUM_Content(string n, string g, CT c)
        {
            Name = c switch
            {
                //CT.AP => $"StarQ AUM AP {n}",
                CT.UIC => $"StarQ AUM UIC {n}",
                _ => n,
            };
            GUID = g;
            Vanilla = false;
            ContentType = c;
        }

        public AUM_Content(string n, bool v, CT c)
        {
            Name = n;
            GUID = "";
            Vanilla = true;
            ContentType = c;
        }
    }

    public static class AUM_Contents
    {
        public static readonly AUM_Content RoadsSmallRoads = new("RoadsSmallRoads", true, CT.UIC);
        public static readonly AUM_Content Healthcare = new("Healthcare", true, CT.UIC);
        public static readonly AUM_Content Police = new("Police", true, CT.UIC);
        public static readonly AUM_Content Education = new("Education", true, CT.UIC);
        public static readonly AUM_Content Pathways = new("Pathways", true, CT.UIC);
        public static readonly AUM_Content BikePaths = new("BikePaths", true, CT.UIC);
        public static readonly AUM_Content PiersAndQuays = new("PiersAndQuays", true, CT.UIC);
        public static readonly AUM_Content Terraforming = new("Terraforming", true, CT.UIC);

        //public static readonly AUM_Content AUM_DLC = new(
        //    "Asset UI Manager",
        //    "9aad98259293712854cfe52ff4323a50",
        //    CT.C
        //);

        //public static readonly AUM_Content BaseGame = new(
        //    "BaseGame",
        //    "076d97edadc7339827d21bf873053572",
        //    CT.AP
        //);
        //public static readonly AUM_Content PDXMods = new(
        //    "PDXMods",
        //    "842207f603b96ddcab6222a113e3cefc",
        //    CT.AP
        //);
        //public static readonly AUM_Content TransportDepot = new(
        //    "TransportDepot",
        //    "55c4e85e081bf4e5f25f6dd7f5412493",
        //    CT.AP
        //);
        //public static readonly AUM_Content PublicTransport = new(
        //    "PublicTransport",
        //    "7261508c476e8e7ac8ad4f4855cd027a",
        //    CT.AP
        //);
        //public static readonly AUM_Content CargoTransport = new(
        //    "CargoTransport",
        //    "2f6a22554a53366d32f125ac59e3f798",
        //    CT.AP
        //);
        //public static readonly AUM_Content TransportLane = new(
        //    "TransportLane",
        //    "236bce3b391d0bee8f63ba1a2f5b970b",
        //    CT.AP
        //);
        //public static readonly AUM_Content BicycleStop = new(
        //    "BicycleStop",
        //    "ca75418ef1c3c8e4ce09e6dbcf8f247f",
        //    CT.AP
        //);
        public static readonly AUM_Content RoadsBridges = new(
            "RoadsBridges",
            "a54f3af0878619130e29f0574d166dde",
            CT.UIC
        );
        public static readonly AUM_Content RoadsParkingRoads = new(
            "RoadsParkingRoads",
            "8438524b0f8558b3174d154ae0fcd210",
            CT.UIC
        );
        public static readonly AUM_Content Clinics = new(
            "Clinics",
            "0d12bc8f29ec1ca3133572c0c3c98c57",
            CT.UIC
        );
        public static readonly AUM_Content CityParks = new(
            "CityParks",
            "6d201ab3db663bdc170618849d8f8351",
            CT.UIC
        );
        public static readonly AUM_Content Colleges = new(
            "Colleges",
            "8cfdd31bd1dae5e58a193b7bbdce2beb",
            CT.UIC
        );
        public static readonly AUM_Content DiseaseControls = new(
            "DiseaseControls",
            "389bddb24a2866162323c05b7f031aab",
            CT.UIC
        );
        public static readonly AUM_Content HealthResearchCenters = new(
            "HealthResearchCenters",
            "764be8a169614e40ca09679fb3e8064f",
            CT.UIC
        );
        public static readonly AUM_Content Highschools = new(
            "Highschools",
            "2dc35f193fd893580520d99138928458",
            CT.UIC
        );
        public static readonly AUM_Content Hospitals = new(
            "Hospitals",
            "f0b193166263f8d661059525a1d8bf68",
            CT.UIC
        );
        public static readonly AUM_Content Intelligences = new(
            "Intelligences",
            "e0591be8d947b01b9f8ef979c5bd1e6e",
            CT.UIC
        );
        public static readonly AUM_Content LocalPolices = new(
            "LocalPolices",
            "edb83d2a8d061ff53c62411a076a5b5f",
            CT.UIC
        );
        public static readonly AUM_Content PocketParks = new(
            "PocketParks",
            "783f0608d6a199a25ecc098d950b3fe7",
            CT.UIC
        );
        public static readonly AUM_Content PoliceHQs = new(
            "PoliceHQs",
            "f9cde9fb8fbe0e96346f83a456098651",
            CT.UIC
        );
        public static readonly AUM_Content Prisons = new(
            "Prisons",
            "74ad3d4186fb424560fa751645f9b0df",
            CT.UIC
        );
        public static readonly AUM_Content Schools = new(
            "Schools",
            "0280b43f48bc6c429d7f5c1b1ae08b42",
            CT.UIC
        );
        public static readonly AUM_Content Universities = new(
            "Universities",
            "56a7aa504a443a37a70ed4370506476b",
            CT.UIC
        );

        public static readonly AUM_Content[] All =
        {
            RoadsSmallRoads,
            Healthcare,
            Police,
            Education,
            Pathways,
            BikePaths,
            PiersAndQuays,
            Terraforming,
            //AUM_DLC,
            //BaseGame,
            //PDXMods,
            //TransportDepot,
            //PublicTransport,
            //CargoTransport,
            //TransportLane,
            //BicycleStop,
            CityParks,
            Clinics,
            Colleges,
            DiseaseControls,
            HealthResearchCenters,
            Highschools,
            Hospitals,
            Intelligences,
            LocalPolices,
            PocketParks,
            PoliceHQs,
            Prisons,
            RoadsBridges,
            RoadsParkingRoads,
            Schools,
            Universities,
        };
    }

    public partial class ContentSystem : GameSystemBase
    {
        private static bool EntitiesAssigned = false;

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            if (!EntitiesAssigned)
                AssignEntities();
        }

        protected override void OnUpdate()
        {
            Enabled = false;
            AssignEntities();
        }

        public void AssignEntities()
        {
            foreach (AUM_Content item in AUM_Contents.All)
            {
                if (item.ContentType == CT.C)
                {
                    PrefabHelper.TryGetPrefab(
                        item.ContentType.GetName(),
                        item.Name,
                        Colossal.Hash128.Parse(item.GUID),
                        out PrefabBase prefabBase
                    );
                    item.PrefabBase = prefabBase;
                    LogHelper.SendLog(
                        $"Found prefabBase for {item.ContentType.GetName()}:{item.Name} ({item.GUID})",
                        LogLevel.DEVD
                    );
                    continue;
                }

                if (
                    PrefabHelper.TryGetEntity(
                        item.ContentType.GetName(),
                        item.Name,
                        Colossal.Hash128.Parse(item.GUID),
                        out Entity entity
                    )
                )
                {
                    item.Entity = entity;
                    LogHelper.SendLog(
                        $"Found entity for {item.ContentType.GetName()}:{item.Name} ({item.GUID})",
                        LogLevel.DEVD
                    );
                    continue;
                }

                LogHelper.SendLog(
                    $"{item.ContentType.GetName()}:{item.Name} ({item.GUID}) not found",
                    LogLevel.DEVD
                );
            }
            LogHelper.SendLog($"AssigningEntities done", LogLevel.DEVD);
            EntitiesAssigned = true;
        }
    }
}
