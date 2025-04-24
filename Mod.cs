using System.IO;
using System.Reflection;
using AssetUIManager.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using Unity.Entities;

namespace AssetUIManager
{
    public class Mod : IMod
    {
        public static string Name = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            .Title;
        public static string Version = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString(3);
        public static string Author = "StarQ";

        public static string uiHostName = "starq-asset-ui-manager";

        public static ILog log = LogManager
            .GetLogger($"{nameof(AssetUIManager)}")
            .SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            //log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset)) { } //log.Info($"Current mod asset at {asset.path}");

            var harmony = new Harmony("StarQ.AssetUIManager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            UIManager.defaultUISystem.AddHostLocation(
                uiHostName,
                Path.Combine(Path.GetDirectoryName(asset.path), "Resources"),
                false
            );
            AssetDatabase.global.LoadSettings(nameof(AssetUIManager), m_Setting, new Setting(this));
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UIManagerSystem>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AssetPackSystem>();
#if DEBUG
            m_Setting.VerboseLogging = true;
#endif
        }

        public void OnDispose()
        {
            //log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
            UIManager.defaultUISystem.RemoveHostLocation(uiHostName);
        }
    }
}
