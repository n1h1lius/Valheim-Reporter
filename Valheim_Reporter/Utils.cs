using System;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Reflection;
using System.Linq;

namespace ValheimReporter
{
    public static class Utils 
    {

        public static Boolean debugMode = false;
        public static Boolean outputMode = false;
        private static Boolean hasInitializedLogFile = false;
        public static Boolean isReporterOpen = false;
        public static readonly string ENCRIPTION_HEADER = "[NH]-3247";
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ValheimReporter");

        public static class Colors
        {
            public static class Text
            {
                public static readonly string RED = "\u001b[31m";
                public static readonly string RED_BOLD = "\u001b[31;1m";
                public static readonly string GREEN = "\u001b[32m";
                public static readonly string GREEN_BOLD = "\u001b[32;1m";
                public static readonly string YELLOW = "\u001b[33m";
                public static readonly string YELLOW_BOLD = "\u001b[33;1m";
                public static readonly string BLUE = "\u001b[34m";
                public static readonly string BLUE_BOLD = "\u001b[34;1m";
                public static readonly string MAGENTA = "\u001b[35m";
                public static readonly string MAGENTA_BOLD = "\u001b[35;1m";
                public static readonly string CYAN = "\u001b[36m";
                public static readonly string CYAN_BOLD = "\u001b[36;1m";
                public static readonly string WHITE = "\u001b[37m";
                public static readonly string WHITE_BOLD = "\u001b[37;1m";
                public static readonly string GRAY = "\u001b[90m";
                public static readonly string GRAY_BOLD = "\u001b[90;1m";
                public static readonly string BLACK = "\u001b[30m";
                public static readonly string BLACK_BOLD = "\u001b[30;1m";
            }
        
            public static class ForeGround
            {
             public static readonly string BLACK = "\u001b[40m";
             public static readonly string RED = "\u001b[41m";
             public static readonly string GREEN = "\u001b[42m";
             public static readonly string YELLOW = "\u001b[43m";
             public static readonly string BLUE = "\u001b[44m";
             public static readonly string MAGENTA = "\u001b[45m";
             public static readonly string CYAN = "\u001b[46m";
             public static readonly string WHITE = "\u001b[47m";
             
            }

            public static readonly string BOLD = "\u001b[1m";
            public static readonly string UNDERLINED = "\u001b[4m";
            public static readonly string RESET = "\u001b[0m";
        }

        public static class PlayerUtils
        {

            private static Vector3 playerLastPosition = Vector3.zero;
            public static Vector3 GetPlayerPosition() { playerLastPosition = Player.m_localPlayer.transform.position; return playerLastPosition;}
            public static Vector3 GetPlayerLastPosition() { return playerLastPosition; }
        }

        // PUBLIC METHODS
        public static string GetVersion() => typeof(ValheimReporter).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Logs a system message with color and formatting. It also handles an output file.
        /// </summary>
        /// <param name="sender">The object that sent the message.</param>
        /// <param name="function">The function that sent the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="type">The type of the message (log, error, debug, message, warning, fatal).</param>
        /// <param name="level">The level of verbosity (1-10).</param>
        /// <param name="forcedVerbose">Forces the message to be logged regardless of the verbosity level.</param>
        public static void LogSystem(string sender, string function, string message, string type="log", int level = 10, bool forcedVerbose=false)
        {
            
            if (debugMode || forcedVerbose || level < 4)
            {
                /*Types = log, error, debug, message, warning, fatal*/
                string foreGround = "";
                string separator = Colors.Text.WHITE_BOLD + " || ";
                string prompt = Colors.Text.WHITE_BOLD + " ->> ";

                string headerColor1 = Colors.Text.CYAN;
                string headerColor2 = Colors.Text.RED;
                string finalSenderColor = Colors.Text.MAGENTA_BOLD;
                string finalFunctionColor = Colors.Text.YELLOW;
                string finalTypeColor = Colors.Text.WHITE_BOLD;
                string finalMessageColor = Colors.Text.WHITE_BOLD;

                switch (type.ToLower())
                {
                    case "success":
                        foreGround = Colors.ForeGround.GREEN;
                        headerColor1 = Colors.Text.WHITE_BOLD;
                        headerColor2 = Colors.Text.WHITE_BOLD;
                        finalSenderColor = Colors.Text.WHITE_BOLD;
                        finalFunctionColor = Colors.Text.WHITE_BOLD;
                        finalTypeColor = Colors.Text.WHITE_BOLD;
                        finalMessageColor = Colors.Text.WHITE_BOLD;
                        break;
                    
                    case "error":
                        foreGround = Colors.ForeGround.RED;
                        headerColor1 = Colors.Text.WHITE_BOLD;
                        headerColor2 = Colors.Text.WHITE_BOLD;
                        finalSenderColor = Colors.Text.WHITE_BOLD;
                        finalFunctionColor = Colors.Text.WHITE_BOLD;
                        finalTypeColor = Colors.Text.WHITE_BOLD;
                        finalMessageColor = Colors.Text.WHITE_BOLD;
                        break;
                }

                string header = headerColor1 +"#### [NH]" + separator + headerColor2 + "[VALHEIM REPORTER]";
                string finalSender = finalSenderColor + "< " + sender + " >";
                string finalFunction = finalFunctionColor + "{ " + function + " }";
                string finalType = finalTypeColor + "[ " + type.ToUpper() + " ]";

                string finalMessage = finalMessageColor + message;

                string formattedMessage = foreGround + header + separator;

                if (debugMode || level == 1)
                {
                    formattedMessage +=  finalSender + separator + finalFunction + separator + finalType;
                }
                else if (level == 2)
                {
                    formattedMessage +=  finalSender + separator + finalType;
                }
                else if (level == 3)
                {
                    formattedMessage +=  finalType;
                }

                formattedMessage += prompt + finalMessage + Colors.RESET; 

                Logger.LogMessage(formattedMessage);

                if (outputMode)
                {
                    // --- LOG FILE LOGIC ---

                    separator = " || ";
                    prompt = " ->> ";
                    header = "#### [NH]" + separator + "[VALHEIM REPORTER]";
                    finalSender = "< " + sender + " >";
                    finalFunction = "{ " + function + " }";
                    finalType = "[ " + type.ToUpper() + " ]";

                    finalMessage = prompt + message;

                    formattedMessage = header + separator + finalSender + separator + finalFunction + separator + finalType + separator + finalMessage;

                    try 
                    {
                        string logFilePath = Path.Combine(ValheimReporter.RootConfigValheimPath, "NH-ValheimReporter.log");

                        if (!hasInitializedLogFile)
                        {
                            // Delete Previous Log File && Create New Log File
                            File.WriteAllText(logFilePath, formattedMessage + Environment.NewLine);
                            hasInitializedLogFile = true;
                        }
                        else
                        {
                            // Append Output to Log File
                            File.AppendAllText(logFilePath, formattedMessage + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback - Failed to write log
                        LogSystem("ValheimReporter", "LogSystem", $"Failed to write log file: {ex.Message}", "error");
                    }
                }
            }
            
        
        }

        /// <summary>
        /// Deletes old log folders based on the config value <see cref="ValheimReporter.ConfigMaxLogs"/>.
        /// </summary>
        /// <remarks>
        /// Folders are sorted by name and the oldest folders are deleted first.
        /// </remarks>
        public static void DeleteOldLogs()
        {
            try
            {

                if (!Directory.Exists(ValheimReporter.LogsPath)) return;

                DirectoryInfo dirInfo = new DirectoryInfo(ValheimReporter.LogsPath);
                DirectoryInfo[] folders = dirInfo.GetDirectories();

                int maxLogs = ValheimReporter.ConfigMaxLogs.Value;
                
                if (folders.Length > maxLogs)
                {
                    var foldersToDelete = folders
                        .OrderBy(f => f.Name) 
                        .Take(folders.Length - maxLogs);

                    foreach (var folder in foldersToDelete)
                    {
                        try 
                        {
                            folder.Delete(true);
                            Utils.LogSystem("LogManager", "DeleteOldLogs", $"Deleted old log folder: {folder.Name}", "log", 3);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogSystem("LogManager", "DeleteOldLogs", $"Failed to delete {folder.Name}: {ex.Message}", "error", 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogSystem("LogManager", "DeleteOldLogs", $"Critical error managing logs: {ex.Message}", "error", 1);
            }
        }
        
        public static class Screen
        {
            public static Boolean toggleCursorMode = false;
            public static Boolean preventPlayerMovement = false;

            /// <summary>
            /// Toggles the cursor mode for the game. If the cursor mode is toggled off, the cursor will be locked and hidden.
            /// If the cursor mode is toggled on, the cursor will be unlocked and visible.
            /// </summary>
            /// <param name="preventPlayerMovement">If true, the player's movement will be prevented while the cursor is active.</param>
            public static void ToggleCursorMode(Boolean preventPlayerMovement=false)
            {
                if (SceneManager.GetActiveScene().name != "start" && Player.m_localPlayer != null)
                {
                    if (toggleCursorMode)
                    {
                        // Deactivate Cursor
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        toggleCursorMode = false;

                        var mouseCapture = typeof(GameCamera).GetField("m_mouseCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (mouseCapture != null) { mouseCapture.SetValue(GameCamera.instance, true); }
                    }
                    else 
                    {
                        // Activate Cursor
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        toggleCursorMode = true;
                        Screen.preventPlayerMovement = preventPlayerMovement;
                    }

                }
            }
        

            /// <summary>
            /// Saves the given Texture2D to a file with the given file name.
            /// </summary>
            /// <param name="tex">The Texture2D to save.</param>
            /// <param name="fileName">The file name to save as. The file extension will be appended based on the image format set in the Config.</param>
            public static void SaveTextureToFile(Texture2D tex, string fileName)
            {
                if (tex == null) return;

                byte[] bytes = null;

                switch(ValheimReporter.ConfigImageFormat.Value)
                {
                    case ValheimReporter.ImageFormat.JPG:
                        fileName += ".jpg";
                        bytes = tex.EncodeToJPG(75);
                        break;
                    
                    case ValheimReporter.ImageFormat.PNG:
                        fileName += ".png";
                        bytes = tex.EncodeToPNG();
                        break;
                }
                
                File.WriteAllBytes(fileName, bytes);
            }
        
        }
    }

    // ==================================================================================
    //                                  HARMONY PATCHES
    // ==================================================================================

    // BLOCK CAMERA MOVEMENT WHILE MENU OPEN
    //---------------------------------------------------------------------------------------------------------
    [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
    public static class GameCamera_Patch
    {
        static bool Prefix()
        {

            if (Utils.Screen.toggleCursorMode)
            {

                var mouseCapture = typeof(GameCamera).GetField("m_mouseCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (mouseCapture != null)
                {
                    mouseCapture.SetValue(GameCamera.instance, false);
                }

                return false; 
            }
            
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "CanMove")]
    public static class Player_Movement
    {
        static bool Prefix()
        {
            if (Utils.Screen.toggleCursorMode && Utils.Screen.preventPlayerMovement)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Character), "Jump")]
    public static class Player_Jump
    {
        static bool Prefix(Character __instance)
        {
            if (__instance is Player player && player.IsPlayer() && Utils.Screen.toggleCursorMode && Utils.Screen.preventPlayerMovement)
                {
                    return false;
                }
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "TakeInput")]
    static class BlockInputPatch
    {
        /// <summary>
        /// If the UI is open, returns false and the game does NOT process the input
        /// </summary>
        static bool Prefix()
        {
            if (Utils.isReporterOpen) 
            {
                return false;
            }
            return true;
        }
    }



}
