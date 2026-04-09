using com.ultrabit.bitheroes.core;
using com.ultrabit.bitheroes.model.application;
using com.ultrabit.bitheroes.ui.utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BitHeroesInternal
{
    internal class InternalWindow : MonoBehaviour
    {
        private GameObject windowInstance;
        private CheckBoxTile testingCheckbox;
        private bool isOpen;

        // Tab state
        private Button hacksBtn;
        private Button aboutBtn;
        private GameObject hacksPanel;
        private GameObject aboutPanel;
        private readonly Color activeColor = Color.white;
        private readonly Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);

        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        private void Open()
        {
            if (windowInstance != null)
                return;

            isOpen = true;
            BuildWindow();
        }

        public void Close()
        {
            if (windowInstance != null)
            {
                GameData.instance.audioManager.PlaySoundLink("buttonclick", 1f);
                Destroy(windowInstance);
            }
            windowInstance = null;
            isOpen = false;
        }

        private void BuildWindow()
        {
            var windowGenerator = GameData.instance.windowGenerator;

            // Load the actual GameSettingsWindow prefab from the game's resources
            Transform windowTransform = windowGenerator.GetFromResources("ui/game/GameSettingsWindow");
            windowInstance = windowTransform.gameObject;

            // Parent to the game's canvas, same as the game does in CenterAndAddCanvasParent
            windowTransform.SetParent(windowGenerator.canvas.transform);
            RectTransform windowRect = windowTransform.GetComponent<RectTransform>();
            windowRect.anchoredPosition = Vector2.zero;
            windowRect.sizeDelta = Vector2.one;
            windowRect.localScale = Vector3.one;

            // Get reference to the GameSettingsWindow component to access its fields
            var settingsWindow = windowInstance.GetComponent<com.ultrabit.bitheroes.ui.game.GameSettingsWindow>();

            // Grab prefabs from the general panel before we destroy it
            Transform checkBoxPrefab = settingsWindow.gameSettingsGeneralPanel.checkBoxPrefab;
            Transform textPrefab = settingsWindow.gameSettingsGeneralPanel.textPrefab;
            float listWidth = settingsWindow.gameSettingsGeneralPanel.GetComponent<RectTransform>().sizeDelta.x;
            GameObject panelContent = settingsWindow.gameSettingsGeneralPanel.gameSettingsPanelContent;

            // Save references to the tab buttons we want to repurpose
            hacksBtn = settingsWindow.generalBtn;
            aboutBtn = settingsWindow.languageBtn;

            // Save references to the panels and their content containers (which have VerticalLayoutGroup + ScrollRect)
            hacksPanel = settingsWindow.gameSettingsGeneralPanel.gameObject;
            aboutPanel = settingsWindow.gameSettingsLanguagePanel.gameObject;
            GameObject aboutPanelContent = settingsWindow.gameSettingsLanguagePanel.gameSettingsLanguageContent;

            // Grab the close button before destroying the component (GameSettingsWindow extends WindowsMain,
            // so destroying it removes the WindowsMain too — we need to wire up close ourselves)
            Button closeBtn = settingsWindow.closeBtn;

            // Destroy the GameSettingsWindow component so it doesn't run its own Start()
            Destroy(settingsWindow);

            // --- Set title text ---
            var topperTexts = windowInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in topperTexts)
            {
                if (txt.gameObject.name.ToLower().Contains("topper") ||
                    txt.gameObject.name.ToLower().Contains("title") ||
                    txt.gameObject.name.ToLower().Contains("header"))
                {
                    txt.text = "BH Internal";
                    break;
                }
            }

            // --- Configure tabs ---
            // Repurpose generalBtn as "Hacks" tab
            hacksBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Hacks";
            hacksBtn.gameObject.SetActive(true);
            hacksBtn.onClick.RemoveAllListeners();
            hacksBtn.onClick.AddListener(OnHacksTab);

            // Repurpose languageBtn as "About" tab
            aboutBtn.GetComponentInChildren<TextMeshProUGUI>().text = "About";
            aboutBtn.gameObject.SetActive(true);
            aboutBtn.onClick.RemoveAllListeners();
            aboutBtn.onClick.AddListener(OnAboutTab);

            // Hide all other tab buttons
            var allButtons = windowInstance.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn == hacksBtn || btn == aboutBtn)
                    continue;

                string btnName = btn.gameObject.name.ToLower();
                if (btnName.Contains("close"))
                    continue;

                if (btnName.Contains("support") || btnName.Contains("news") ||
                    btnName.Contains("ignore") || btnName.Contains("forum") ||
                    btnName.Contains("logout") || btnName.Contains("admin") ||
                    btnName.Contains("test") || btnName.Contains("dates") ||
                    btnName.Contains("google"))
                {
                    btn.gameObject.SetActive(false);
                }
            }

            // Hide footer text (terms, privacy, account request)
            foreach (var txt in topperTexts)
            {
                string txtName = txt.gameObject.name.ToLower();
                if (txtName.Contains("term") || txtName.Contains("privacy") || txtName.Contains("account"))
                {
                    txt.gameObject.SetActive(false);
                }
            }

            // Hide the support panel
            var supportPanel = windowInstance.GetComponentInChildren<com.ultrabit.bitheroes.ui.game.GameSettingsSupportPanel>(true);
            if (supportPanel != null)
                supportPanel.gameObject.SetActive(false);

            // --- Set up Hacks panel content ---
            // Destroy the GeneralPanel script so it doesn't init its own checkboxes
            var generalPanelScript = hacksPanel.GetComponent<com.ultrabit.bitheroes.ui.game.GameSettingsGeneralPanel>();
            if (generalPanelScript != null)
                Destroy(generalPanelScript);

            // Clear existing content
            if (panelContent != null)
            {
                for (int i = panelContent.transform.childCount - 1; i >= 0; i--)
                    Destroy(panelContent.transform.GetChild(i).gameObject);
            }

            // Add our SpeedHack checkbox
            if (checkBoxPrefab != null && panelContent != null)
            {
                Transform checkboxTransform = Instantiate(checkBoxPrefab);
                checkboxTransform.SetParent(panelContent.transform, false);

                testingCheckbox = checkboxTransform.GetComponent<CheckBoxTile>();
                testingCheckbox.Create(
                    new CheckBoxTile.CheckBoxObject("SpeedHack", null),
                    AppInfo.TESTING,
                    true,
                    listWidth,
                    "",
                    null
                );

                testingCheckbox.toggle.onValueChanged.AddListener(OnTestingToggleChanged);
            }

            // --- Set up About panel content ---
            // Destroy the LanguagePanel script so it doesn't init its own checkboxes
            var languagePanelScript = aboutPanel.GetComponent<com.ultrabit.bitheroes.ui.game.GameSettingsLanguagePanel>();
            if (languagePanelScript != null)
                Destroy(languagePanelScript);

            // Clear the content container children (this container has VerticalLayoutGroup for proper stacking/scrolling)
            if (aboutPanelContent != null)
            {
                for (int i = aboutPanelContent.transform.childCount - 1; i >= 0; i--)
                    Destroy(aboutPanelContent.transform.GetChild(i).gameObject);
            }

            // Use the game's textPrefab for about content, parented to the content container
            if (textPrefab != null && aboutPanelContent != null)
            {

                Transform urlLine = Instantiate(textPrefab);
                urlLine.SetParent(aboutPanelContent.transform, false);
                urlLine.GetComponent<TextMeshProUGUI>().text = "github.com/SushruthRao/BitHeroesInternal";
                urlLine.GetComponent<RectTransform>().sizeDelta = new Vector2(listWidth, urlLine.GetComponent<RectTransform>().sizeDelta.y);

                Transform titleLine = Instantiate(textPrefab);
                titleLine.SetParent(aboutPanelContent.transform, false);
                titleLine.GetComponent<TextMeshProUGUI>().text = "BH Internal";
                titleLine.GetComponent<RectTransform>().sizeDelta = new Vector2(listWidth, titleLine.GetComponent<RectTransform>().sizeDelta.y);

                Transform descLine = Instantiate(textPrefab);
                descLine.SetParent(aboutPanelContent.transform, false);
                descLine.GetComponent<TextMeshProUGUI>().text = "A Bit Heroes internal tool made by sr03";
                descLine.GetComponent<RectTransform>().sizeDelta = new Vector2(listWidth, descLine.GetComponent<RectTransform>().sizeDelta.y);

                Transform descLine2 = Instantiate(textPrefab);
                descLine2.SetParent(aboutPanelContent.transform, false);
                descLine2.GetComponent<TextMeshProUGUI>().text = "Discord : sr_003";
                descLine2.GetComponent<RectTransform>().sizeDelta = new Vector2(listWidth, descLine2.GetComponent<RectTransform>().sizeDelta.y);

                Transform hotkeyLine = Instantiate(textPrefab);
                hotkeyLine.SetParent(aboutPanelContent.transform, false);
                hotkeyLine.GetComponent<TextMeshProUGUI>().text = "Hotkey: F8 to toggle window";
                hotkeyLine.GetComponent<RectTransform>().sizeDelta = new Vector2(listWidth, hotkeyLine.GetComponent<RectTransform>().sizeDelta.y);
            }

            // --- Start on Hacks tab ---
            SetTab(0);

            // Wire up the close button manually since we destroyed the WindowsMain component
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(Close);
            }

            // Trigger the animator to scroll the window in
            Animator animator = windowInstance.GetComponent<Animator>();
            if (animator != null)
                animator.SetBool("onDown", true);
        }

        private void SetTab(int tab)
        {
            if (tab == 0)
            {
                // Hacks tab active
                hacksBtn.image.color = activeColor;
                hacksBtn.GetComponentInChildren<TextMeshProUGUI>().color = activeColor;
                hacksBtn.enabled = false;

                aboutBtn.image.color = inactiveColor;
                aboutBtn.GetComponentInChildren<TextMeshProUGUI>().color = inactiveColor;
                aboutBtn.enabled = true;

                hacksPanel.SetActive(true);
                aboutPanel.SetActive(false);
            }
            else
            {
                // About tab active
                aboutBtn.image.color = activeColor;
                aboutBtn.GetComponentInChildren<TextMeshProUGUI>().color = activeColor;
                aboutBtn.enabled = false;

                hacksBtn.image.color = inactiveColor;
                hacksBtn.GetComponentInChildren<TextMeshProUGUI>().color = inactiveColor;
                hacksBtn.enabled = true;

                aboutPanel.SetActive(true);
                hacksPanel.SetActive(false);
            }
        }

        private void OnHacksTab()
        {
            GameData.instance.audioManager.PlaySoundLink("buttonclick", 1f);
            SetTab(0);
        }

        private void OnAboutTab()
        {
            GameData.instance.audioManager.PlaySoundLink("buttonclick", 1f);
            SetTab(1);
        }

        private void OnTestingToggleChanged(bool value)
        {
            AppInfo.TESTING = value;
        }

        private void OnWindowDestroyed(object obj)
        {
            windowInstance = null;
            isOpen = false;
        }

        private void Update()
        {
            if (testingCheckbox != null && testingCheckbox.isChecked != AppInfo.TESTING)
            {
                testingCheckbox.isChecked = AppInfo.TESTING;
            }
        }
    }
}
