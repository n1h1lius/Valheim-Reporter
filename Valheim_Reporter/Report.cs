using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using HarmonyLib;

namespace ValheimReporter
{
    public class Report
    {
        // --- Properties ---
        public string Date { get; set; }
        public string Time { get; set; }
        public Texture2D Image1 { get; set; }
        public Texture2D Image2 { get; set; }
        public string PlayerName { get; set; }
        public string SteamID { get; set; }
        public string DiscordID { get; set; }
        public string Coordinates { get; set; }
        public List<string> Webhook { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string ReportID { get; private set; }

        // Constructor
        public Report()
        {
            // Initialize with current data to ensure ReportID generation is possible
            DateTime now = DateTime.Now;
            Date = now.ToString("yyyy-MM-dd");
            Time = now.ToString("HH-mm-ss");
        }

        /// <summary>
        /// Generates the unique ReportID based on internal data.
        /// </summary>
        public void GenerateReportID()
        {
            ReportID = $"[{PlayerName}] - {Date}_{Time}";
        }


        /// <summary>
        /// Saves the Report data to a local file in the ValheimReporter Logs directory.
        /// </summary>
        /// <remarks>
        /// The file will be saved in the format {Date}_{Time}_log.json.
        /// </remarks>
        private void SaveToLocalLogs()
        {
            try
            {
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string folderName = $"{Date}_{Time}";
                string directoryPath = Path.Combine(dllPath, "ValheimReporter", "Logs", folderName);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, $"{Date}_log.json");

                var logData = new
                {
                    ReportID,
                    Date,
                    Time,
                    PlayerName,
                    SteamID,
                    DiscordID,
                    Coordinates,
                    Webhook,
                    Category,
                    SubCategory,
                    Subject,
                    Description
                };

                string json = JsonConvert.SerializeObject(logData, Formatting.Indented);
                File.WriteAllText(filePath, json);

                if (Image1 != null){ Utils.Screen.SaveTextureToFile(Image1, Path.Combine(directoryPath, "world_view")); }
                if (Image2 != null){ Utils.Screen.SaveTextureToFile(Image2, Path.Combine(directoryPath, "map_view")); }
                
                Utils.LogSystem("Report", "SaveLocal", $"Log saved to: {filePath}", "success", 3);
            }
            catch (Exception ex)
            {
                Utils.LogSystem("Report", "SaveLocal", $"Error saving log: {ex.Message}", "error", 1);
            }
        }


        /// <summary>
        /// Sends a report to the Discord Webhook specified in the Webhook property.
        /// </summary>
        /// <remarks>
        /// This function will prepare the report data as a JSON object and send it to the Discord Webhook.
        /// If the Webhook property is empty, this function will log an error and stop execution.
        /// If any of the images are not null, they will be added as attachments to the request.
        /// </remarks>
        public IEnumerator SendReportCoroutine()
        {
            GenerateReportID();
            SaveToLocalLogs();

            foreach(string webhook in Webhook)
            {
                if (string.IsNullOrEmpty(webhook))
                {
                    Utils.LogSystem("Report", "Send", "Webhook URL is empty!", "error", 1);
                    continue;
                }

                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

                // 1. Prepare the Embed JSON

                string safeCategory = Category ?? "Not Specified";
                string safeSubCategory = SubCategory ?? "Not Specified";
                string safeSubject = string.IsNullOrEmpty(Subject) ? "Not Specified" : Subject;
                string safeDescription = string.IsNullOrEmpty(Description) ? "Not Specified" : Description;

                string embedJson = JsonConvert.SerializeObject(new
                {
                    username = "Valheim Reporter",
                    avatar_url = "https://i.ibb.co/KpNZ88cv/Gemini-Generated-Image-wk1a8fwk1a8fwk1a.png",
                    webhook = webhook.StartsWith(Utils.ENCRIPTION_HEADER) ? webhook : null,
                    embeds = new[]
                    {
                        new
                        {
                            title = $"New Report: {ReportID}",
                            color = 15105570, // Orange
                            fields = new[]
                            {
                                new { name = "Player", value = PlayerName, inline = true },
                                new { name = "Steam ID", value = SteamID, inline = true },
                                new { name = "Discord ID", value = "<@" + DiscordID + ">", inline = true },
                                new { name = "Coordinates", value = Coordinates, inline = false },
                                new { name = "Timestamp", value = $"{Date} | {Time}", inline = false },
                                new { name = "Category", value = safeCategory, inline = true },
                                new { name = "Subcategory", value = safeSubCategory, inline = true },
                                new { name = "Subject", value = safeSubject, inline = false },
                                new { name = "Description", value = safeDescription, inline = false }
                            },
                            footer = new { text = "Valheim Reporter System" }
                        }
                    }
                });


                formData.Add(new MultipartFormDataSection("payload_json", embedJson));

                // 2. Add Images as attachments
                if (Image1 != null)
                    formData.Add(new MultipartFormFileSection("file", Image1.EncodeToJPG(75), "screenshot1.jpg", "image/jpeg"));
                
                if (Image2 != null)
                    formData.Add(new MultipartFormFileSection("file2", Image2.EncodeToJPG(75), "screenshot2.jpg", "image/jpeg"));

                // 3. Send Request

                using (UnityWebRequest www = UnityWebRequest.Post(webhook.StartsWith(Utils.ENCRIPTION_HEADER) ? ValheimReporter.ConfigAPIKey.Value : webhook, formData))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        string errorResponse = www.downloadHandler != null ? www.downloadHandler.text : "No response body";
                        Utils.LogSystem("Report", "Send", $"Discord Webhook Error: {www.error} | Details: {errorResponse}", "error", 1);
                    }
                    else
                    {
                        ValheimReporter.playSoundFlag = true;
                        Utils.LogSystem("Report", "Send", "Report successfully sent to Discord.", "success", 3);
                    }
                }  
               
            }

            
        }
    }
}
