using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ACaldeira.UI
{
    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameplaySimulation simulation;
        [SerializeField] private RunProgression progression;
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private GameStateEventChannelSO stateChanged;
        [SerializeField] private ExperienceEventChannelSO experienceChanged;
        [SerializeField] private GameObject menu, hud, pause, choices, gameOver, loading;
        [SerializeField] private TMP_Text status, xpText, result, credits;
        [SerializeField] private Slider healthBar, xpBar;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private TMP_Text[] choiceLabels;
        private void OnEnable()
        {
            stateChanged.Raised += ShowState; experienceChanged.Raised += Experience;
            simulation.HudChanged += Hud; progression.ChoicesChanged += ShowChoices; permanent.Changed += Permanent;
        }
        private void Start() { ShowState(gameManager.State); Permanent(); }
        private void OnDisable()
        {
            stateChanged.Raised -= ShowState; experienceChanged.Raised -= Experience;
            simulation.HudChanged -= Hud; progression.ChoicesChanged -= ShowChoices; permanent.Changed -= Permanent;
        }
        private void ShowState(GameState state)
        {
            menu.SetActive(state == GameState.MainMenu); hud.SetActive(state == GameState.Playing);
            pause.SetActive(state == GameState.Paused); choices.SetActive(state == GameState.LevelUp);
            gameOver.SetActive(state == GameState.GameOver); loading.SetActive(state == GameState.Loading);
            if (state == GameState.GameOver)
                result.SetText(simulation.Won ? "TURNO CONCLUIDO\nMaquinas neutralizadas: {0}" : "EXOTRAJE DESTRUIDO\nMaquinas neutralizadas: {0}", simulation.Kills);
        }
        private void Hud(float health, float maximum, float elapsed, int alive)
        {
            healthBar.SetValueWithoutNotify(health / maximum);
            status.SetText("CALDEIRA / {0:0}s     MAQUINAS {1:0}", elapsed, alive);
        }
        private void Experience(int current, int required)
        {
            xpBar.SetValueWithoutNotify(Mathf.Clamp01((float)current / required));
            xpText.SetText("NIVEL {0:0}   SUCATA {1:0}/{2:0}", progression.Level, current, required);
        }
        private void ShowChoices()
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var u = progression.Offer(i); choiceButtons[i].gameObject.SetActive(u != null);
                if (u != null) choiceLabels[i].text = u.DisplayName + "\n" + u.Description;
            }
        }
        private void Permanent()
        {
            credits.SetText("CREDITOS {0}\nBlindagem Nv.{1}   Potencia Nv.{2}", permanent.Credits, permanent.ArmorLevel, permanent.DamageLevel);
        }
        public void ChooseFirst() => progression.Choose(0);
        public void ChooseSecond() => progression.Choose(1);
        public void ChooseThird() => progression.Choose(2);
    }
}
