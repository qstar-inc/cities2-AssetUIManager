using AssetUIManager.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI;
using Game.Modding;
using Game.SceneFlow;
using Game;
using System.IO;
using Unity.Entities;

namespace AssetUIManager
{
    public class Mod : IMod
    {
        public static string Name = "Asset UI Manager";
        public static string Version = "1.1.0";
        public static string Author = "StarQ";
        public static string uiHostName = "starq-asset-ui-manager";

        public static ILog log = LogManager.GetLogger($"{nameof(AssetUIManager)}").SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            //log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                //log.Info($"Current mod asset at {asset.path}");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            UIManager.defaultUISystem.AddHostLocation(uiHostName, Path.Combine(Path.GetDirectoryName(asset.path), ".Thumbs"), false);
            AssetDatabase.global.LoadSettings(nameof(AssetUIManager), m_Setting, new Setting(this));
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UIManagerSystem>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AssetPackSystem>();
        }

        public void OnDispose()
        {
            //log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.PathwayPriorityDropdownVersion = 0;
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
            UIManager.defaultUISystem.RemoveHostLocation(uiHostName);
        }
    }
}
