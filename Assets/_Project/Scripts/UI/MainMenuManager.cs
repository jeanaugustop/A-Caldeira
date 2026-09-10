using ACaldeira.Core;
using ACaldeira.Data;
using UnityEngine;

namespace ACaldeira.UI
{
    public sealed class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private RuntimeServicesSO runtimeServices;
        [SerializeField] private StageSO defaultStage;
        [SerializeField] private StageSO assemblyStage;
        [SerializeField] private StageSO stressStage;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject permanentUpgradesPanel;
        [SerializeField] private GameObject settingsPanel;

        private void OnEnable()
        {
            ShowMain();
        }

        public void StartGame()
        {
            runtimeServices.GameManager.StartRun(defaultStage);
        }
        public void StartAssembly() => runtimeServices.GameManager.StartRun(assemblyStage);
        public void StartStress() => runtimeServices.GameManager.StartRun(stressStage);

        public void ShowMain()
        {
            SetPanels(true, false, false);
        }

        public void ShowPermanentUpgrades()
        {
            SetPanels(false, true, false);
        }

        public void ShowSettings()
        {
            SetPanels(false, false, true);
        }

        public void ApplySettingsAndReturn()
        {
            ShowMain();
            runtimeServices.SettingsManager.ApplyAndSave();
        }

        public void Quit()
        {
            runtimeServices.SettingsManager.Save();
            Application.Quit();
        }

        private void SetPanels(bool showMain, bool showUpgrades, bool showSettings)
        {
            if (mainPanel != null) mainPanel.SetActive(showMain);
            if (permanentUpgradesPanel != null) permanentUpgradesPanel.SetActive(showUpgrades);
            if (settingsPanel != null) settingsPanel.SetActive(showSettings);
        }
    }
}
