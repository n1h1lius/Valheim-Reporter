using BepInEx;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System;
using BepInEx.Configuration;

namespace ValheimReporter{
    [BepInPlugin("n1h1lius.valheimreporter", "Valheim Reporter", "1.0.0")]
    public class ValheimReporter : BaseUnityPlugin
    {
        public static AssetBundle MainAssetBundle;
        public static CategoriesRoot ReportCategories;
        public static string dllPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        
        private static GameObject _instanciaUI; 
        private static ValheimReporterController _controller;

        public static bool playSoundFlag = false;

        // CONFIG KEYS
        // -----------------------------------------------------------------------------------------------------------------------
        // GENERAL
        public static ConfigEntry<KeyCode> ConfigOpenKey;
        public static ConfigEntry<Boolean> ConfigCloseOnSend;
        public static ConfigEntry<float> ConfigFadeDuration;
        public static ConfigEntry<Boolean> ConfigRequirementsRemark;
        public static ConfigEntry<Boolean> ConfigConfirmationSound;

        // SERVER REQUIRED INFORMATION
        public static ConfigEntry<string> ConfigDiscordID;
        public static ConfigEntry<string> ConfigAPIKey;
        public static ConfigEntry<string> ConfigWorldName;

        // LOG SYSTEM
        public static ConfigEntry<Boolean> ConfigDeleteLogs;
        public static ConfigEntry<int> ConfigMaxLogs;

        // SCREENSHOTS
        public static ConfigEntry<float> ConfigMapZoom;
        public static ConfigEntry<ImageFormat> ConfigImageFormat;

        public enum ImageFormat
        {
            JPG,
            PNG
        }

        // DEBUG MODE
        public static ConfigEntry<Boolean> ConfigDebugMode;
        public static ConfigEntry<Boolean> ConfigOutputMode;
        

        private readonly Harmony harmony = new Harmony("com.n1h1lius.valheimreporter");

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Harmony is patched to all methods in the game.
        /// Then, the Init method is called to initialize the plugin.
        /// </summary>
        void Awake()
        {
            harmony.PatchAll();

            Init();
        }

        /// <summary>
        /// Called every frame. Handles the initialization of the Reporter UI and toggling of the Reporter modal.
        /// </summary>
        /// <remarks>
        /// If the Reporter UI is null, it will instantiate the ReporterCanvas prefab and initialize the ReporterController.
        /// If the ReporterController is not null, it will toggle the Reporter modal.
        /// If the ReporterController is null or the Reporter modal is not open, it will not do anything.
        /// </remarks>
        void Update()
        {
            if (HandleOpenManager())
            {
                
                if (_instanciaUI == null)
                {
                    Utils.LogSystem("ValheimReporter", "Update", "INIT UI...", "log", 3);
                    
                    GameObject prefab = MainAssetBundle.LoadAsset<GameObject>("ReporterCanvas");
                    _instanciaUI = Instantiate(prefab);
                    DontDestroyOnLoad(_instanciaUI);

                    _controller = _instanciaUI.AddComponent<ValheimReporterController>();
                    _controller.StartReporter();
                }

                if (_controller != null)
                {
                    Utils.LogSystem("ValheimReporter", "Update", "Toggle Main Manager...", "log", 4);
                    _controller.ToggleMainManager();
                }

            }

            if (_instanciaUI != null && _controller != null){if (_controller.getIsReporterOpen()){_controller.UpdateFooterInfo();}}

        }
    
        void OnDestroy() { harmony.UnpatchSelf(); }

        /// <summary>
        /// Initializes the plugin by loading the asset bundle and categories json.
        /// It also loads the config and sets up the log system.
        /// </summary>
        private void Init()
        {
            // LOAD ASSET BUNDLE
            // -----------------------------------------------------------------------------------------------------------------------
            string bundlePath = Path.Combine(dllPath, "ValheimReporter", "Data", "Assets", "valheim_reporter_bundle");
            
            if (File.Exists(bundlePath))
            {
                MainAssetBundle = AssetBundle.LoadFromFile(bundlePath);
                Utils.LogSystem("ValheimReporter", "Awake", "Bundle Loaded Successfully.", "success", 3);
            }
            else
            {
                Utils.LogSystem("ValheimReporter", "Awake", "Bundle Not Found.", "error", 1);
            }

            // LOAD CATEGORIES JSON
            // -----------------------------------------------------------------------------------------------------------------------

            ReportCategories = CategoryLoader.Load("categories.json");

            if (ReportCategories != null) { Utils.LogSystem("ValheimReporter", "Awake", "Categories Loaded Successfully.", "success", 3); }

            // LOAD CONFIG
            // -----------------------------------------------------------------------------------------------------------------------
            ConfigOpenKey = Config.Bind("General", "Open Key", KeyCode.F10, "The key to open the main manager. Default: F10");
            ConfigCloseOnSend = Config.Bind("General", "Close On Send", true, "Close the main manager when a report is sent. Default: true");
            ConfigFadeDuration = Config.Bind("General", "Fade Duration", 0.09f, "The duration of the fade animation of the main manager. Default: 0.09");
            ConfigRequirementsRemark = Config.Bind("General", "Requirements Remark", true, "Show the requirements remark in Report Screen. It's just a visual aid to notice when some parameters are missing. Default: true");
            ConfigConfirmationSound = Config.Bind("General", "Confirmation Sound", true, "Play a confirmation sound when a report is sent. Default: true");

            ConfigDiscordID = Config.Bind("ServerRequirements", "Discord ID", "Discord ID", "Your Discord ID. Default: Discord ID");
            ConfigAPIKey = Config.Bind("ServerRequirements", "API Key", "API_KEY", "Server's API Key. Default: API_KEY");
            ConfigWorldName = Config.Bind("ServerRequirements", "World Name", "WORLD_NAME", "Server's World Name. Default: WORLD_NAME");
           
            ConfigMapZoom = Config.Bind("Screenshots", "Map Zoom", 0.5f, "The zoom level of the map on the screenshots. 0.5f - 1.0f is a balanced zoom for seeing the area. // 0.5f is a good value. The smaller the number, the closer (0.1 very close, 1.0 very far). Default: 0.5");
            ConfigImageFormat = Config.Bind("Screenshots", "Image Format", ImageFormat.JPG, "The format of the screenshots ouput. This is just client side. Reports will always send JPG high compression images. Default: JPG");

            ConfigDeleteLogs = Config.Bind("LogSystem", "Delete Reports", false, "Delete the report files when the maximum number of reports is reached. Default: false");
            ConfigMaxLogs = Config.Bind("LogSystem", "Max Logs", 10, "The maximum number of report files to keep. Default: 10");
            ConfigOutputMode = Config.Bind("LogSystem", "Output Mode", false, "Enable output mode. It will output the logs a .log file. Default: false");
           
            ConfigDebugMode = Config.Bind("DebugMode", "Debug Mode", false, "Enable debug mode. For developers only. Default: false");

            Utils.debugMode = ConfigDebugMode.Value;
            Utils.outputMode = ConfigOutputMode.Value;

            // DELETE OLD LOG FILES
            // -----------------------------------------------------------------------------------------------------------------------

            if (ConfigDeleteLogs.Value && ConfigMaxLogs.Value > 0){ Utils.DeleteOldLogs(); }

            // LOG SYSTEM
            // -----------------------------------------------------------------------------------------------------------------------
            Utils.LogSystem("ValheimReporter", "Awake", "Plugin Loaded Successfully.", "success", 3);
            
        
        }

        /// <summary>
        /// Checks if the main manager can be opened. It can only be opened if no UI element has focus.
        /// </summary>
        /// <returns>
        /// True if the main manager can be opened, false otherwise.
        /// </returns>
        private Boolean HandleOpenManager()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return false;
            
            if (Chat.instance != null && Chat.instance.HasFocus()) return false;
            
            if (Console.IsVisible() || Menu.IsVisible() || TextInput.IsVisible()) return false;
     
            if (Game.IsPaused()) return false;

            if (Input.GetKeyDown(ConfigOpenKey.Value)) { return true; }

            return false;
        }

        public static GameObject GetInstance() { return _instanciaUI;}

    }


}
