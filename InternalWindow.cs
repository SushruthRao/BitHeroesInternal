using com.ultrabit.bitheroes.core;
using com.ultrabit.bitheroes.model.application;
using com.ultrabit.bitheroes.ui;
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
                var wm = windowInstance.GetComponent<WindowsMain>();
                if (wm != null)
                    wm.OnClose();
                else
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

            // Grab the checkBoxPrefab from the general panel before we destroy it
            Transform checkBoxPrefab = settingsWindow.gameSettingsGeneralPanel.checkBoxPrefab;
            float listWidth = settingsWindow.gameSettingsGeneralPanel.GetComponent<RectTransform>().sizeDelta.x;
            // Grab the content container reference for sizing
            GameObject panelContent = settingsWindow.gameSettingsGeneralPanel.gameSettingsPanelContent;

            // Destroy the GameSettingsWindow component so it doesn't run its own Start()
            Destroy(settingsWindow);

            // Set the title text
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

            // Find and clear/hide the game-specific UI elements
            // Hide all tab buttons (general, language, support, news, ignores, forums, logout, admin, test, dates, google)
            var buttons = windowInstance.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string btnName = btn.gameObject.name.ToLower();
                // Keep the close button, hide everything else that's a tab/action button
                if (btnName.Contains("close"))
                    continue;
                if (btnName.Contains("general") || btnName.Contains("language") || btnName.Contains("support") ||
                    btnName.Contains("news") || btnName.Contains("ignore") || btnName.Contains("forum") ||
                    btnName.Contains("logout") || btnName.Contains("admin") || btnName.Contains("test") ||
                    btnName.Contains("dates") || btnName.Contains("google"))
                {
                    btn.gameObject.SetActive(false);
                }
            }

            // Hide the footer text (terms, privacy, account request)
            foreach (var txt in topperTexts)
            {
                string txtName = txt.gameObject.name.ToLower();
                if (txtName.Contains("term") || txtName.Contains("privacy") || txtName.Contains("account"))
                {
                    txt.gameObject.SetActive(false);
                }
            }

            // Hide the language and support panels
            var languagePanel = windowInstance.GetComponentInChildren<com.ultrabit.bitheroes.ui.game.GameSettingsLanguagePanel>(true);
            if (languagePanel != null)
                languagePanel.gameObject.SetActive(false);

            var supportPanel = windowInstance.GetComponentInChildren<com.ultrabit.bitheroes.ui.game.GameSettingsSupportPanel>(true);
            if (supportPanel != null)
                supportPanel.gameObject.SetActive(false);

            // Clear the general panel's existing content (all the game's checkboxes/sliders)
            var generalPanel = windowInstance.GetComponentInChildren<com.ultrabit.bitheroes.ui.game.GameSettingsGeneralPanel>(true);
            if (generalPanel != null)
            {
                // Clear the content container children
                if (panelContent != null)
                {
                    for (int i = panelContent.transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(panelContent.transform.GetChild(i).gameObject);
                    }
                }

                // Destroy the panel's script so it doesn't init its own checkboxes
                Destroy(generalPanel);
            }

            // Now add our own CheckBoxTile using the game's prefab
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

                // Wire up the toggle change directly
                testingCheckbox.toggle.onValueChanged.AddListener(OnTestingToggleChanged);
            }

            // Trigger the animator to scroll the window in (same as the game does)
            Animator animator = windowInstance.GetComponent<Animator>();
            if (animator != null)
                animator.SetBool("onDown", true);

            // Set up the WindowsMain component for proper window behavior
            var windowsMain = windowInstance.GetComponent<WindowsMain>();
            if (windowsMain != null)
            {
                windowsMain.DESTROYED.AddListener(OnWindowDestroyed);
                // Let WindowsMain.Start() run naturally - it calls CreateWindow()
            }
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
            // Sync toggle if TESTING changed externally (e.g. F7 key)
            if (testingCheckbox != null && testingCheckbox.isChecked != AppInfo.TESTING)
            {
                testingCheckbox.isChecked = AppInfo.TESTING;
            }
        }
    }
}
