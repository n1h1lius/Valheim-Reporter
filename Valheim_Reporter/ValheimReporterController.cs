using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

namespace ValheimReporter
{
    

    public class ValheimReporterController : MonoBehaviour
    {
        [Header("Status")]
        private bool isReporterOpen = false;
        private bool isMainReporterOpen = false;

        [Header("Main Settings")]
        private float fadeDuration = ValheimReporter.ConfigFadeDuration.Value;
        private Canvas mainCanvas;
        private CanvasGroup mainCanvasGroup;

        [Header("Navigation Buttons")]
        private Button openReportsModalButton;
        private Button helpButton;
        private Button aboutButton;

        [Header("Reports Modal")]
        private Canvas reportsModalCanvas;
        private CanvasGroup reportsModalCanvasGroup;
        private TextMeshProUGUI modalSectionTitle;
        private TextMeshProUGUI modalSectionType;
        private TextMeshProUGUI modalSectionSubType;

        [Header("Reports Body Content")]
        private Canvas reportsModalBodyCanvas;
        private Button generateScreenshotsButton;
        private Button submitReportButton;
        private RawImage screenshotImage1;
        private RawImage screenshotImage2;
        private TMP_Dropdown reportTypeDropdown;
        private TMP_Dropdown reportSubTypeDropdown;
        private TMP_InputField reportAboutInput;
        private TMP_InputField reportContentInput;

        [Header("Help Modal")]
        private Canvas helpModalCanvas;
        private CanvasGroup helpModalCanvasGroup;
        private TextMeshProUGUI helpStanzaText;
        private TextMeshProUGUI helpStanzaReference;

        [Header("About Modal")]
        private Canvas aboutModalCanvas;
        private CanvasGroup aboutModalCanvasGroup;
        private TextMeshProUGUI aboutStanzaText;
        private TextMeshProUGUI aboutStanzaReference;
        private Button checkMyGithubButton;
        private Button checkModGithubButton;

        [Header("Footer Info")]
        private TextMeshProUGUI footerDateLabel;
        private TextMeshProUGUI footerTimeLabel;
        private TextMeshProUGUI footerCoorLabel;

        [Header("Transition Tracking")]
        private Canvas currentActiveSubModal = null;
        private CanvasGroup currentActiveSubModalGroup = null;

        [Header("Warning Modal")]
        private Canvas warningModalCanvas;
        private CanvasGroup warningModalCanvasGroup;
        private TextMeshProUGUI warningTextReason;
        private Button warningAcceptButton;

        // PRIVATE VARS
        private TextMeshProUGUI stanzaText;
        private TextMeshProUGUI referenceText;

        private Texture2D defaultTexture;
        private Category currentCategory;
        private Subcategory currentSubcategory;
        private Boolean aboutIsRequired = false;
        private Boolean aboutIsRequiredAnimationFlag = false;
        private Boolean contentIsRequiredAnimationFlag = false;
        private Boolean pulsatingIsRequired = false;
        private int stoppedPulsating = 0;
        private Boolean contentIsRequired = false;

        public bool finishedReport = false; // Set to true after submitting report for forms reset
        public bool screenshotTaken = false; // Set to true after taking screenshot

        // PRIVATE VARS

        private string apiUrl = "https://odin-api.orlog.workers.dev/havamal/english/random";

        void Awake()
        {
            mainCanvasGroup = GetComponent<CanvasGroup>();
        }

        void Update()
        {
            if (ValheimReporter.ConfigRequirementsRemark.Value)
            {
                if (aboutIsRequired){ RequiredAnimation(reportAboutInput, ref aboutIsRequiredAnimationFlag); }
                if (contentIsRequired){ RequiredAnimation(reportContentInput, ref contentIsRequiredAnimationFlag); }
                if (stoppedPulsating < 2) 
                { 
                    RequiredImageAnimation(screenshotImage1, pulsatingIsRequired);
                    RequiredImageAnimation(screenshotImage2, pulsatingIsRequired); 
                }   
            }

            if (ValheimReporter.ConfigConfirmationSound.Value && ValheimReporter.playSoundFlag)
            {
                PlayConfirmSound();
                ValheimReporter.playSoundFlag = false;
                if (ValheimReporter.ConfigCloseOnSend.Value){ToggleMainManager();}
            }
        }
        public void StartReporter()
        {
            Utils.LogSystem("ValheimReporterController", "StartReporter", "Initializing Valheim Reporter...", "log", 4);
            InitializeReferences();
            BindUIEvents();
            SetupInitialState();

            mainCanvas.gameObject.SetActive(false);
            Utils.LogSystem("ValheimReporterController", "StartReporter", "Valheim Reporter initialized.", "success", 3);
        }

        private void InitializeReferences()
        {
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Valheim Reporter references...", "log", 4);
            // Main Container
            mainCanvas = GetComponent<Canvas>();

            // Navigation
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Navigation references...", "log", 4);
            openReportsModalButton = GameObject.Find("ReporterFrameOpenReportsModalButton").GetComponent<Button>();
            helpButton = GameObject.Find("ReporterFrameHelpButton").GetComponent<Button>();
            aboutButton = GameObject.Find("ReporterFrameAboutButton").GetComponent<Button>();

            // Reports Modal & Header
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Reports Modal & Header references...", "log", 4);
            reportsModalCanvas = GameObject.Find("ReporterModalCanvas").GetComponent<Canvas>();
            reportsModalCanvasGroup = reportsModalCanvas.GetComponent<CanvasGroup>();
            modalSectionTitle = GameObject.Find("ReporterModalSectionTitle").GetComponent<TextMeshProUGUI>();
            modalSectionType = GameObject.Find("ReporterModalSectionType").GetComponent<TextMeshProUGUI>();
            modalSectionSubType = GameObject.Find("ReporterModalSectionSubType").GetComponent<TextMeshProUGUI>();

            // Reports Body
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Reports Body references...", "log", 4);

            Texture2D tex = LoadTextureFromFile(Path.Combine(ValheimReporter.dllPath, "ValheimReporter", "Data", "Images", "NoPicture.png"));
            defaultTexture = tex;

            reportsModalBodyCanvas = GameObject.Find("ReporterModalBody").GetComponent<Canvas>();
            generateScreenshotsButton = GameObject.Find("GenerateScreenshotsButton").GetComponent<Button>();
            submitReportButton = GameObject.Find("SubmitReportButton").GetComponent<Button>();
            screenshotImage1 = GameObject.Find("ScreenshotImage1").GetComponent<RawImage>();
            screenshotImage2 = GameObject.Find("ScreenshotImage2").GetComponent<RawImage>();
            reportTypeDropdown = GameObject.Find("ReportInfoTypeDropdown").GetComponent<TMP_Dropdown>();
            reportSubTypeDropdown = GameObject.Find("ReportInfoSubTypeDropdown").GetComponent<TMP_Dropdown>();
            reportAboutInput = GameObject.Find("ReportInfoAboutInput").GetComponent<TMP_InputField>();
            reportContentInput = GameObject.Find("ReportInfoContentInput").GetComponent<TMP_InputField>();

            ResetReporterModal();
            
            // Help Modal
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Help Modal references...", "log", 4);
            helpModalCanvas = GameObject.Find("HelpModalCanvas").GetComponent<Canvas>();
            helpModalCanvasGroup = helpModalCanvas.GetComponent<CanvasGroup>();
            helpStanzaText = GameObject.Find("HelpStanzaText").GetComponent<TextMeshProUGUI>();
            helpStanzaReference = GameObject.Find("HelpStanzaReference").GetComponent<TextMeshProUGUI>();

            // About Modal
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing About Modal references...", "log", 4);
            aboutModalCanvas = GameObject.Find("AboutModalCanvas").GetComponent<Canvas>();
            aboutModalCanvasGroup = aboutModalCanvas.GetComponent<CanvasGroup>();
            aboutStanzaText = GameObject.Find("AboutStanzaText").GetComponent<TextMeshProUGUI>();
            aboutStanzaReference = GameObject.Find("AboutStanzaReference").GetComponent<TextMeshProUGUI>();
            checkMyGithubButton = GameObject.Find("CheckMyGithubButton").GetComponent<Button>();
            checkModGithubButton = GameObject.Find("CheckModGithubButton").GetComponent<Button>();

            // Footer
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Footer references...", "log", 4);
            footerDateLabel = GameObject.Find("ReporterModalFooterDateLabel").GetComponent<TextMeshProUGUI>();
            footerTimeLabel = GameObject.Find("ReporterModalFooterTimeLabel").GetComponent<TextMeshProUGUI>();
            footerCoorLabel = GameObject.Find("ReporterModalFooterCoorLabel").GetComponent<TextMeshProUGUI>();

            // Warning Modal
            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Initializing Warning Modal references...", "log", 4);
            warningModalCanvas = GameObject.Find("WarningModalCanvas").GetComponent<Canvas>();
            warningModalCanvasGroup = warningModalCanvas.GetComponent<CanvasGroup>();
            warningTextReason = GameObject.Find("WarningTextReason").GetComponent<TextMeshProUGUI>();
            warningAcceptButton = GameObject.Find("WarningAcceptButton").GetComponent<Button>();

            Utils.LogSystem("ValheimReporterController", "InitializeReferences", "Valheim Reporter references initialized.", "success", 4);
        }

        private void BindUIEvents()
        {
            Utils.LogSystem("ValheimReporterController", "BindUIEvents", "Binding Valheim Reporter UI events...", "log", 4);

            openReportsModalButton.onClick.AddListener(() => SwitchSubModal(reportsModalCanvas, reportsModalCanvasGroup, "reporter"));
            helpButton.onClick.AddListener(() => SwitchSubModal(helpModalCanvas, helpModalCanvasGroup, "help"));
            aboutButton.onClick.AddListener(() => SwitchSubModal(aboutModalCanvas, aboutModalCanvasGroup, "about"));

            reportAboutInput.onValueChanged.AddListener(delegate { SetRequiredFlags(); });
            reportContentInput.onValueChanged.AddListener(delegate { SetRequiredFlags(); });

            generateScreenshotsButton.onClick.AddListener(() => TakeScreenshot());
            submitReportButton.onClick.AddListener(() => SubmitReport());
            warningAcceptButton.onClick.AddListener(() => StartCoroutine(Fade(warningModalCanvasGroup, 1, 0, () => warningModalCanvas.gameObject.SetActive(false))));

            reportTypeDropdown.onValueChanged.AddListener(delegate { HandleDropdown(reportTypeDropdown, "type"); });
            reportSubTypeDropdown.onValueChanged.AddListener(delegate { HandleDropdown(reportSubTypeDropdown, "subtype"); });

            checkMyGithubButton.onClick.AddListener(() => Application.OpenURL("https://github.com/n1h1lius"));
            checkModGithubButton.onClick.AddListener(() => Application.OpenURL("https://github.com/n1h1lius/Valheim-Reporter"));

            Utils.LogSystem("ValheimReporterController", "BindUIEvents", "Valheim Reporter UI events bound.", "log", 4);
        }

        private void SetupInitialState()
        {
            Utils.LogSystem("ValheimReporterController", "SetupInitialState", "Setting Valheim Reporter initial state...", "log", 4);

            PopulateCategories(true);

            mainCanvasGroup.alpha = 0;
            reportsModalCanvasGroup.alpha = 0;
            helpModalCanvasGroup.alpha = 0;
            aboutModalCanvasGroup.alpha = 0;
            
            reportsModalCanvas.gameObject.SetActive(false);
            helpModalCanvas.gameObject.SetActive(false);
            aboutModalCanvas.gameObject.SetActive(false);
            warningModalCanvas.gameObject.SetActive(false);

            Utils.LogSystem("ValheimReporterController", "SetupInitialState", "Valheim Reporter initial state set.", "log", 4);

        }

        public void UpdateFooterInfo()
        {
            footerDateLabel.text = DateTime.Now.ToString("dd/MM/yyyy");
            footerTimeLabel.text = DateTime.Now.ToString("HH:mm:ss");

            if(Player.m_localPlayer != null)
            {
                Vector3 position = Utils.PlayerUtils.GetPlayerPosition();
                footerCoorLabel.text = $"({position.x:F0}, {position.y:F0}, {position.z:F0})";
            }
            
        }

        public void ToggleMainManager()
        {
            if (mainCanvas.gameObject.activeSelf && mainCanvasGroup.alpha >= 0.9f)
            {
                Utils.LogSystem("ValheimReporterController", "ToggleMainManager", "Closing Valheim Reporter...", "log", 4);
                isReporterOpen = false;
                isMainReporterOpen = false;
                reportTypeDropdown.value = 0;
                reportSubTypeDropdown.value = 0;
                HandleDropdown(reportTypeDropdown, "type");
                StartCoroutine(Fade(mainCanvasGroup, 1, 0, () => mainCanvas.gameObject.SetActive(false)));
                Utils.Screen.ToggleCursorMode(true);
            }
            else if (!mainCanvas.gameObject.activeSelf)
            {
                Utils.LogSystem("ValheimReporterController", "ToggleMainManager", "Opening Valheim Reporter...", "log", 4);
                CloseAllModals();
                isMainReporterOpen = true;
                mainCanvas.gameObject.SetActive(true);
                StartCoroutine(Fade(mainCanvasGroup, 0, 1));
                Utils.Screen.ToggleCursorMode(true);
            }
        }
        public void CloseAllModals()
        {
            if (reportsModalCanvas.gameObject.activeSelf && finishedReport){StartCoroutine(Fade(reportsModalCanvasGroup, 1, 0, () => reportsModalCanvas.gameObject.SetActive(false)));}
            if (helpModalCanvas.gameObject.activeSelf){StartCoroutine(Fade(helpModalCanvasGroup, 1, 0, () => helpModalCanvas.gameObject.SetActive(false)));}
            if (aboutModalCanvas.gameObject.activeSelf){StartCoroutine(Fade(aboutModalCanvasGroup, 1, 0, () => aboutModalCanvas.gameObject.SetActive(false)));}
            Utils.LogSystem("ValheimReporterController", "CloseAllModals", "All Valheim Reporter modals closed.", "log", 4);
        }

        private void ResetReporterModal()
        {
            screenshotImage1.texture = defaultTexture;
            screenshotImage2.texture = defaultTexture;
            reportAboutInput.text = "";
            reportContentInput.text = "";
            finishedReport = false;
            Utils.LogSystem("ValheimReporterController", "ResetReporterModal", "Valheim Reporter modal reset.", "log", 4);
        }

        private void SwitchSubModal(Canvas targetCanvas, CanvasGroup targetGroup, string mode)
        {
            StartCoroutine(TransitionSequence(targetCanvas, targetGroup, mode));
        }

        private IEnumerator TransitionSequence(Canvas nextCanvas, CanvasGroup nextGroup, string mode)
        {
            // 1. Close current sub-modal if exists
            if (currentActiveSubModal != null && currentActiveSubModal.gameObject.activeSelf)
            {
                yield return StartCoroutine(Fade(currentActiveSubModalGroup, 1, 0));
                currentActiveSubModal.gameObject.SetActive(false);
            }

            // 2. Prepare data for the next modal

            switch (mode)
            {
                case "reporter":
                    isReporterOpen = true;
                    HandleDropdown(reportTypeDropdown, "type");
                    HandleDropdown(reportSubTypeDropdown, "subtype");
                    break;
                case "help":
                    isReporterOpen = false;
                    stanzaText = helpStanzaText;
                    referenceText = helpStanzaReference;
                    break;
                case "about":
                    isReporterOpen = false;
                    stanzaText = aboutStanzaText;
                    referenceText = aboutStanzaReference;
                    break;
            }

            // 3. Open next sub-modal
            nextCanvas.gameObject.SetActive(true);

            if(finishedReport){ResetReporterModal();}
            if (mode != "reporter"){RequestNewStanza();}

            yield return StartCoroutine(Fade(nextGroup, 0, 1));

            // 4. Update tracking references
            currentActiveSubModal = nextCanvas;
            currentActiveSubModalGroup = nextGroup;
        }

        private IEnumerator Fade(CanvasGroup group, float startAlpha, float endAlpha, Action onComplete = null)
        {
            float elapsedTime = 0;
            group.alpha = startAlpha;

            // Interaction control
            if (endAlpha > 0)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
            }
            else
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                yield return null;
            }

            group.alpha = endAlpha;
            onComplete?.Invoke();
        }

        private void HandleDropdown(TMP_Dropdown dropdown, string mode)
        {
            if (dropdown.options.Count == 0) return;

            SetRequiredFlags(true);

            string selectedOption = dropdown.options[dropdown.value].text;

            switch (mode)
            {
                case "type":
                    modalSectionType.text = selectedOption;
                    currentCategory = ValheimReporter.ReportCategories.GetCategoryByName(selectedOption);
                    PopulateCategories();
                    break;
                case "subtype":
                    modalSectionSubType.text = selectedOption;
                    currentSubcategory = ValheimReporter.ReportCategories.GetSubcategoryByName(selectedOption);
                    break;
            }

            SetRequiredFlags();
            
        }

        private void SetRequiredFlags(bool reset=false)
        {
            if (ValheimReporter.ConfigRequirementsRemark.Value)
            {
                Outline aboutInputFieldOutline = reportAboutInput.GetComponent<Outline>();
                Outline contentInputFieldOutline = reportContentInput.GetComponent<Outline>();

                if (aboutInputFieldOutline == null) { aboutInputFieldOutline = reportAboutInput.gameObject.AddComponent<Outline>(); }
                if (contentInputFieldOutline == null) { contentInputFieldOutline = reportContentInput.gameObject.AddComponent<Outline>(); }

                if (reset)
                {
                    ApplyOutlineEffect(false, aboutInputFieldOutline);
                    ApplyOutlineEffect(false, contentInputFieldOutline);
                    RequiredImageAnimation(screenshotImage1, false);
                    RequiredImageAnimation(screenshotImage2, false);
                    return;
                }

                if (currentSubcategory != null)
                {
                    if (currentSubcategory.Required.Pictures && !screenshotTaken || currentSubcategory.Required.Pictures && currentCategory.Required.Pictures && !screenshotTaken)
                    {
                        pulsatingIsRequired = true;
                        stoppedPulsating = 0;
                    }
                    if (currentSubcategory.Required.Subject || currentSubcategory.Required.Subject && currentCategory.Required.Subject)
                    {
                        aboutIsRequired = ApplyOutlineEffect(reportAboutInput.text == "", aboutInputFieldOutline);
                    }

                    if (currentSubcategory.Required.Description || currentSubcategory.Required.Description && currentCategory.Required.Description)
                    {
                        contentIsRequired = ApplyOutlineEffect(reportContentInput.text == "", contentInputFieldOutline);
                    }
                }

                else
                {
                    if (currentCategory.Required.Pictures && !screenshotTaken)
                    {
                        pulsatingIsRequired = true;
                        stoppedPulsating = 0;
                    }
                
                    if (currentCategory.Required.Subject) { aboutIsRequired = ApplyOutlineEffect(reportAboutInput.text == "", aboutInputFieldOutline); }
                    if (currentCategory.Required.Description) { contentIsRequired = ApplyOutlineEffect(reportContentInput.text == "", contentInputFieldOutline); }  
                }
            }
            

        }

        private Boolean ApplyOutlineEffect(bool status, Outline outline)
        {
            if (ValheimReporter.ConfigRequirementsRemark.Value)
            {
                if (status) 
                { 
                    outline.effectColor = Color.red;
                    outline.effectDistance = new Vector2(5, -5); 
                    outline.enabled = true;
                }

                else { outline.enabled = false; }

                return status;
            }

            return false;

        }

        private void RequiredAnimation(TMP_InputField inputField, ref Boolean flag)
        {
            Outline outline = inputField.GetComponent<Outline>();

            if (outline.effectDistance.x < 10f && !flag) 
            { 
                outline.effectDistance += new Vector2(25f * Time.deltaTime, (25f * Time.deltaTime) * -1f); 
                
                if (outline.effectDistance.x >= 10f) {flag = true;}
            }
            else if (flag) 
            { 
                outline.effectDistance += new Vector2((25f * Time.deltaTime) * -1f, 25f * Time.deltaTime); 
                if (outline.effectDistance.x <= 0f) {flag = false;}  
            }
        }

        private void RequiredImageAnimation(RawImage image, bool flag)
        {
            // Original Color - (White) 1f, 1f, 1f
            // Required Color - (Red) 1f, 0.5f, 0.5f 
            
            if (flag)
            {
                float pulse = Mathf.PingPong(Time.time *2f, 0.5f) + 0.5f;

                image.color = new Color(1f, pulse, pulse, 1f);
            }

            else
            {
                image.color = Color.white;
                stoppedPulsating++;
            }
        }
        // STANZA FETCHER
        // --------------------------------------------------------------------------------------------------
        public void RequestNewStanza() { StartCoroutine(FetchStanza()); }

        private IEnumerator FetchStanza() {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("FETCHING API ERROR: " + webRequest.error);
                }
                else
                {
                    string jsonResult = webRequest.downloadHandler.text;
                    
                    string cleanJson = jsonResult.Trim('[', ']');
                    string[] parts = cleanJson.Split(new string[] { "\",\"" }, System.StringSplitOptions.None);

                    if (parts.Length >= 2)
                    {
                        stanzaText.text = parts[0].Replace("\"", "");
                        referenceText.text = "Hávamál - " + parts[1].Replace("\"", "");
                    }
                }
            }
        }


        // TAKE SCREENSHOT
        // --------------------------------------------------------------------------------------------------

        /// <summary>
        /// Takes a screenshot of the current game state, without any HUD elements.
        /// </summary>
        /// <remarks>
        /// This function will temporarily disable the HUD and main canvas, take a screenshot, and then re-enable them.
        /// </remarks>
        public void TakeScreenshot() { StartCoroutine(CaptureCleanScreenshot()); pulsatingIsRequired = false; }

        /// <summary>
        /// Takes a screenshot of the current game state, without any HUD elements.
        /// This function will temporarily disable the HUD and main canvas, take a screenshot, and then re-enable them.
        /// </summary>
        /// <remarks>
        /// This function will temporarily disable the HUD and main canvas, take a screenshot, and then re-enable them.
        /// This function will also take a second screenshot with the minimap open if the minimap is available.
        /// </remarks>

        private IEnumerator CaptureCleanScreenshot()
        {
            // --- STEP 1: CLEAN SCREENSHOT [NO HUD] ---
            bool hudWasActive = Hud.instance != null && Hud.instance.m_rootObject != null && Hud.instance.m_rootObject.activeSelf;
            
            if (Hud.instance != null && Hud.instance.m_rootObject != null)
                Hud.instance.m_rootObject.SetActive(false);

            if (mainCanvas != null) mainCanvas.enabled = false;

            yield return new WaitForEndOfFrame();
            screenshotImage1.texture = ScreenCapture.CaptureScreenshotAsTexture();

            // --- STEP 2: MINIMAP SCREENSHOT ---
            if (Minimap.instance != null)
            {
                Hud.instance.m_rootObject.SetActive(true);
                
                // We save if the map was already open
                bool mapWasOpen = Minimap.instance.m_mode == Minimap.MapMode.Large;

                // Open the map fullscreen
                Minimap.instance.SetMapMode(Minimap.MapMode.Large);
                

                // Zoom field Adjustment (m_zoom is a float, higher values usually mean further away)
                // 0.5f - 1.0f is a balanced zoom for seeing the area
                var zoomField = typeof(Minimap).GetField("m_largeZoom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (zoomField != null)
                {
                    // 0.5f is a good value. The smaller the number, the closer (0.1 very close, 1.0 very far)
                    zoomField.SetValue(Minimap.instance, ValheimReporter.ConfigMapZoom.Value); 
                }

                yield return new WaitForEndOfFrame();
                screenshotImage2.texture = ScreenCapture.CaptureScreenshotAsTexture();

                // Close the map in case it was already open
                if (!mapWasOpen) { Minimap.instance.SetMapMode(Minimap.MapMode.Small); }
            }

            // --- STEP 4: FINISHED SCREENSHOT - RESTORE PREVIOUS HUD STATE ---

            screenshotTaken = true;

            if (mainCanvas != null) mainCanvas.enabled = true;

            if (Hud.instance != null && Hud.instance.m_rootObject != null && hudWasActive)
            {
                Hud.instance.m_rootObject.SetActive(true);
            }
        }

        
        /// <summary>
        /// Submits a report to the Discord Webhook specified in the Category or Subcategory.
        /// </summary>
        /// <remarks>
        /// Checks if the Subcategory exists and if so, checks if the Subcategory requires fields that the Category does not.
        /// If the Subcategory does not exist, it checks if the Category requires fields.
        /// </remarks>
        public void SubmitReport()
        {
            // CHECK WORLD NAME
            if (ValheimReporter.ConfigWorldName.Value == "WORLD_NAME" || ValheimReporter.ConfigWorldName.Value != ZNet.instance.GetWorldName()) { ShowDefaultWarning(4); return; }
            
            Utils.LogSystem("ValheimReporterController", "SubmitReport", "Checking Category + Subcategory requirements...", "log", 4);
            Category category = ValheimReporter.ReportCategories.GetCategoryByName(reportTypeDropdown.options[reportTypeDropdown.value].text);
            Subcategory subcategory = ValheimReporter.ReportCategories.GetSubcategoryByName(reportSubTypeDropdown.options[reportSubTypeDropdown.value].text);
            
            // CHECK IF DISCORD ID IS CORRECT IN ADVANCED METHOD
            if (category.Webhook.StartsWith(Utils.ENCRIPTION_HEADER)) 
            { 
                if (ValheimReporter.ConfigDiscordID.Value == "Discord ID") { ShowDefaultWarning(5); return; }

                if (string.IsNullOrEmpty(ValheimReporter.ConfigDiscordID.Value) || !char.IsDigit(ValheimReporter.ConfigDiscordID.Value[0])) { ShowDefaultWarning(5); return; }
            
            }

            // CHECK CONDITIONS

            if (subcategory != null)
            {
                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Subcategory exists. Checking Subcategory requirements...", "log", 4);
                
                // First Case: Subcategory Requires Fields that Category Does Not
                if (!screenshotTaken && subcategory.Required.Pictures) { ShowDefaultWarning(1); return; }
                if (subcategory.Required.Subject && reportAboutInput.text == "") { ShowDefaultWarning(2); return; } 
                if (subcategory.Required.Description && reportContentInput.text == "") { ShowDefaultWarning(3); return; }

                // Second Case: Category Requires Fields that Subcategory Does Not, but Subcategory has priority, so no warning shown if category needs it but subcategory doesn't
                if (!screenshotTaken && category.Required.Pictures && subcategory.Required.Pictures) { ShowDefaultWarning(1); return; }
                if (category.Required.Subject && subcategory.Required.Subject && reportAboutInput.text == "") { ShowDefaultWarning(2); return; } 
                if (category.Required.Description && subcategory.Required.Description && reportContentInput.text == "") { ShowDefaultWarning(3); return; }
                
                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Subcategory requirements met.", "success", 4);
            }

            else
            {
                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Subcategory does not exist. Checking Category requirements...", "log", 4);

                // Third Case: Category Requires Fields and there is no subcategory
                if (!screenshotTaken && category.Required.Pictures) { ShowDefaultWarning(1); return; }
                if (category.Required.Subject && reportAboutInput.text == "") {ShowDefaultWarning(2); return; } 
                if (category.Required.Description && reportContentInput.text == "") { ShowDefaultWarning(3); return; }

                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Category requirements met.", "success", 4);

            }

            Utils.LogSystem("ValheimReporterController", "SubmitReport", "Generating Report...", "log", 4);

            Report report = new Report();

            report.PlayerName = Player.m_localPlayer.GetPlayerName();
            report.SteamID = Player.m_localPlayer.GetPlayerID().ToString();
            report.DiscordID = ValheimReporter.ConfigDiscordID.Value;
            report.Coordinates = footerCoorLabel.text;

            Utils.LogSystem("ValheimReporterController", "SubmitReport", $"Basic Data Fetched -> Name: {report.PlayerName}, SteamID: {report.SteamID}, DiscordID: {report.DiscordID}, Coordinates: {report.Coordinates}", "success", 4);

            Utils.LogSystem("ValheimReporterController", "SubmitReport", "Trying to Fetch Raw Images", "log", 4);

            if (screenshotTaken)
            {
                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Raw Image 1 -> Fetching", "success", 4);
                report.Image1 = screenshotImage1.texture as Texture2D;
                Utils.LogSystem("ValheimReporterController", "SubmitReport", "Raw Image 2 -> Fetching", "success", 4);
                report.Image2 = screenshotImage2.texture as Texture2D;
            }

            else
            {
                report.Image1 = null;
                report.Image2 = null;
            }

            report.Category = category.Name;

            if (subcategory != null) report.SubCategory = subcategory.Name;
            else report.SubCategory = "None";

            report.Subject = reportAboutInput.text;
            report.Description = reportContentInput.text;

            List<string> webhooks = new List<string>(){category.Webhook};

            if (subcategory != null)
            {
                if (subcategory.Webhook != "" && subcategory.Webhook != null && subcategory.Webhook != " " 
                && subcategory.Webhook != "None" && subcategory.Webhook != "none" && subcategory.Webhook != "NONE"
                && subcategory.Webhook != "Null" && subcategory.Webhook != "null" && subcategory.Webhook != "NULL"
                && !subcategory.Webhook.Equals(category.Webhook) || subcategory.Webhook.StartsWith("[NH]-3247"))
                {
                    webhooks.Add(subcategory.Webhook);
                }
            }

            report.Webhook = webhooks;

            StartCoroutine(report.SendReportCoroutine());

            finishedReport = true; 
            screenshotTaken = false;

            
            CloseAllModals();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Populate the dropdowns with the categories and subcategories if init is true.
        /// If init is false, it will clear the subtype dropdown and fill it with the subcategories of the selected category.
        /// </summary>
        /// <param name="init">True to populate both dropdowns, false to populate the subtype dropdown only</param>
        public void PopulateCategories(Boolean init=false)
        {

            // Method to populate the dropdowns categories

            if (init)
            {
                reportTypeDropdown.ClearOptions();
                reportSubTypeDropdown.ClearOptions();

                Boolean flag = false;
                foreach (Category category in ValheimReporter.ReportCategories.GetList())
                {
                    reportTypeDropdown.options.Add(new TMP_Dropdown.OptionData(category.Name));

                    if (!flag)
                    {
                        foreach (Subcategory subcategory in category.Subcategories.Values)
                        {
                            reportSubTypeDropdown.options.Add(new TMP_Dropdown.OptionData(subcategory.Name));
                        }
                        flag = true;
                    }
                }
            }
            else
            {
                Utils.LogSystem("ValheimReporterController", "PopulateCategories", "Clear Subtype Dropdown", "log", 4);
                reportSubTypeDropdown.ClearOptions();

                Utils.LogSystem("ValheimReporterController", "PopulateCategories", "Fetching Category", "log", 4);
                string selectedName = reportTypeDropdown.options[reportTypeDropdown.value].text;
                Category category = ValheimReporter.ReportCategories.GetCategoryByName(selectedName);

                if (category.HasSubcategories)
                {
                    Utils.LogSystem("ValheimReporterController", "PopulateCategories", "Filling Subtype Dropdown", "log", 4);
                    List<string> subOptions = new List<string>();
                    foreach (Subcategory subcategory in category.Subcategories.Values)
                    {
                        subOptions.Add(subcategory.Name);
                    }

                    reportSubTypeDropdown.AddOptions(subOptions);
                }

                else 
                {
                    reportSubTypeDropdown.AddOptions(new List<string>() { "------" });
                    Utils.LogSystem("ValheimReporterController", "PopulateCategories", "No Subcategories found in Category [" + selectedName + "]", "log", 4); 
                
                }

                HandleDropdown(reportSubTypeDropdown, "subtype");


            }




        }

        /// <summary>
        /// Shows a warning modal with the given reason for 1 second.
        /// </summary>
        /// <param name="reason">The reason for the warning.</param>
        public void ShowWarning(string reason)
        {
            warningModalCanvas.gameObject.SetActive(true);
            warningTextReason.text = reason;
            StartCoroutine(Fade(warningModalCanvasGroup, 0, 1));
        }

        /// <summary>
        /// Shows a warning modal with a default reason based on the given mode.
        /// </summary>
        /// <param name="mode">The mode to determine the default warning reason.</param>
        /// <remarks>
        /// Modes:
        /// 1: Screenshot Error
        /// 2: Subject Error
        /// 3: Description Error
        /// 4: World Name Error
        /// 5: Discord ID Invalid
        /// </remarks>
        public void ShowDefaultWarning(int mode)
        {
            // Screenshot Error
            if (mode == 1)
            {
                ShowWarning("You haven't taken a screenshot yet! Please take a screenshot and try again.");
            }
            // Subject Error
            else if (mode == 2)
            {
                ShowWarning("A subject is required for this category. Please enter a subject and try again.");
            }
            // Description Error
            else if (mode == 3)
            {
                ShowWarning("A description is required for this category. Please enter a description and try again.");
            }
            // World Name Error
            else if (mode == 4)
            {
                ShowWarning($"Your world name [ {ValheimReporter.ConfigWorldName.Value} ] does not match Server's World Name [ {ZNet.instance.GetWorldName()} ]. Make sure you have entered the correct world name / Make sure you are in the correct server and try again.");
            }
            // Discord ID Invalid
            else if (mode == 5)
            {
                ShowWarning("Your Discord ID is invalid. Please enter a valid Discord ID and try again.");
            }
        }

        /// <summary>
        /// Loads a texture from a file at the given path.
        /// If the file does not exist, or the file is not a valid image, it will return null.
        /// </summary>
        /// <param name="filePath">The path to the file to load.</param>
        /// <returns>A loaded Texture2D, or null if the file is not a valid image.</returns>
        public static Texture2D LoadTextureFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                
                // Create a temporary texture (size 2,2 will be auto-resized by LoadImage)
                Texture2D tex = new Texture2D(2, 2);
                
                // This method automatically detects PNG/JPG and resizes the texture
                if (tex.LoadImage(fileData)) 
                {
                    return tex;
                }
            }
            
            Utils.LogSystem("Utils", "LoadTexture", $"Failed to load texture at: {filePath}", "error", 1);
            return null;
        }

        
        // AUDIO 
        public void PlayConfirmSound()
        {
            string audioPath = Path.Combine(ValheimReporter.dllPath, "ValheimReporter", "Data", "Assets", "confirm.ogg");

            if (!File.Exists(audioPath))
            {
                Utils.LogSystem("Audio", "Load", $"Audio is not present in: {audioPath}", "error", 1);
                return;
            }

            StartCoroutine(LoadAndPlay(audioPath));
        }

        /// <summary>
        /// Loads an audio clip from a file at the given path, and plays it at the position of the local player.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        private IEnumerator LoadAndPlay(string path)
        {
            string url = "file://" + path;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    
                    if (Player.m_localPlayer != null)
                    {
                        AudioSource.PlayClipAtPoint(clip, Player.m_localPlayer.transform.position, 1.0f);
                    }
                }
                else
                {
                    Utils.LogSystem("Audio", "Error", $"Cannot load audio: {www.error}", "error", 1);
                }
            }
        }


        // GETTERS

        public TMP_InputField getReportAboutInput(){ return reportAboutInput; }
        public string getReportAboutInputText(){ return reportAboutInput.text; }
        public TMP_InputField getReportContentInput(){ return reportContentInput; }
        public string getReportContentInputText(){ return reportContentInput.text; }

        public string getDateText(){return footerDateLabel.text;}
        public string getTimeText(){return footerTimeLabel.text;}
        public string getCoorText(){return footerCoorLabel.text;}

        public TextMeshProUGUI getDateLabel(){return footerDateLabel;}
        public TextMeshProUGUI getTimeLabel(){return footerTimeLabel;}
        public TextMeshProUGUI getCoorLabel(){return footerCoorLabel;}

        public Canvas getMainCanvas(){return mainCanvas;}
        public CanvasGroup getMainCanvasGroup(){return mainCanvasGroup;}

        public bool getIsMainReporterOpen(){return isMainReporterOpen;}
        public bool getIsReporterOpen(){return isReporterOpen;}
        public float getFadeDuration(){return fadeDuration;}

        public RawImage getScreenshot1(){return screenshotImage1;}
        public RawImage getScreenshot2(){return screenshotImage2;}

        public Button getOpenReportsModalButton(){return openReportsModalButton;}
        public Button getHelpButton(){return helpButton;}
        public Button getAboutButton(){return aboutButton;}

        public Button getGenerateScreenshotsButton(){return generateScreenshotsButton;}
        public Button getSubmitReportButton(){return submitReportButton;}

        public TMP_Dropdown getReportTypeDropdown(){return reportTypeDropdown;}
        public TMP_Dropdown getReportSubTypeDropdown(){return reportSubTypeDropdown;}

        // SETTERS
        public void SetReportAboutInputText(string text){ reportAboutInput.text = text; }
        public void SetReportContentInputText(string text){ reportContentInput.text = text; }
        public void setDate(string date){footerDateLabel.text = date;}
        public void setTime(string time){footerTimeLabel.text = time;}
        public void setCoor(string coor){footerCoorLabel.text = coor;}

        public void setFadeDuration(float duration){fadeDuration = duration;}
        public void setScreenshot1(Texture image){screenshotImage1.texture = image;}
        public void setScreenshot2(Texture image){screenshotImage2.texture = image;}

    }
}
