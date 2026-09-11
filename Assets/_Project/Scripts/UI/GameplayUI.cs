using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        private static readonly string[] DeathLines =
        {
            "O exotraje cedeu. A Caldeira não para.",
            "As engrenagens não param por um operador.",
            "O ferro venceu esta rodada.",
            "A fumaça engoliu mais uma tentativa.",
            "A linha de produção seguiu sem você.",
            "A pressão foi maior desta vez.",
            "Seu visor apagou, mas a fábrica continua desperta.",
            "A sucata volta para a esteira.",
            "O pátio cobrou seu preço.",
            "As máquinas retomaram o turno.",
            "A Caldeira guardou seu nome em ferro.",
            "O aço não perdoa hesitação.",
            "A próxima tentativa começa nas cinzas desta.",
            "A sirene tocou tarde demais.",
            "A linha fechou sobre o exotraje.",
            "O turno termina onde a máquina decide.",
            "A produção não admite falhas.",
            "O calor levou a melhor.",
            "Mais uma peça caiu no pátio.",
            "A forja ainda espera seu retorno."
        };
        private static readonly string[] SurvivalLines =
        {
            "Você sobreviveu ao Pátio de Triagem. A próxima linha já está em movimento.",
            "Você saiu da linha. A fumaça ficou.",
            "A sirene silenciou por enquanto.",
            "O pátio respira; a fábrica, não.",
            "Seu exotraje aguentou mais um turno.",
            "O ferro foi domado por alguns minutos.",
            "Restos de metal contam a história.",
            "A linha recuou. Não por muito tempo.",
            "A fumaça abre caminho para o próximo setor.",
            "O visor continua aceso.",
            "A Caldeira registra mais um sobrevivente.",
            "Um turno vencido é uma dívida para o próximo.",
            "Você atravessou o ruído e saiu inteiro.",
            "As máquinas caíram; as esteiras continuam.",
            "O silêncio industrial dura pouco.",
            "O próximo setor já chama seu nome.",
            "A pressão baixou. Não confie nela.",
            "O pátio ficou para trás.",
            "A fábrica perdeu terreno.",
            "Ainda há fogo no exotraje."
        };
        private readonly int[] deathOrder = new int[DeathLines.Length];
        private readonly int[] survivalOrder = new int[SurvivalLines.Length];
        private int deathIndex;
        private int survivalIndex;
        private void Awake()
        {
            ResetOrder(deathOrder);
            ResetOrder(survivalOrder);
        }
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
            {
                string title = simulation.Won ? "TURNO CONCLUÍDO" : "VOCÊ MORREU";
                result.text = title + "\n" + NextResultLine(simulation.Won) + "\n\nMáquinas neutralizadas: " + simulation.Kills;
            }
        }
        private void Hud(float health, float maximum, float elapsed, int alive)
        {
            healthBar.SetValueWithoutNotify(health / maximum);
            status.SetText("CALDEIRA / {0:0}s     MAQUINAS {1:0}", elapsed, alive);
        }
        private void Experience(int current, int required)
        {
            xpBar.SetValueWithoutNotify(Mathf.Clamp01((float)current / required));
            xpText.SetText("NIVEL {0:0}   SUCATA {1:0}/{2:0}   RERROLLS {3}", progression.Level, current, required, progression.Rerolls);
        }
        private void ShowChoices()
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var u = progression.Offer(i); choiceButtons[i].gameObject.SetActive(u != null);
                if (u != null) choiceLabels[i].text = u.Title + "\n" + u.Description;
            }
        }
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (gameManager.State != GameState.LevelUp || keyboard == null) return;
            if (keyboard.rKey.wasPressedThisFrame) { progression.TryReroll(); return; }
            if (!keyboard.spaceKey.wasPressedThisFrame && !keyboard.enterKey.wasPressedThisFrame && !keyboard.numpadEnterKey.wasPressedThisFrame) return;

            Button selected = null;
            var selectedObject = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selectedObject != null && selectedObject.transform.IsChildOf(choices.transform)) selected = selectedObject.GetComponent<Button>();
            if (selected == null || !selected.isActiveAndEnabled || !selected.interactable)
                for (int i = 0; i < choiceButtons.Length; i++)
                    if (choiceButtons[i].gameObject.activeInHierarchy && choiceButtons[i].interactable) { selected = choiceButtons[i]; break; }
            if (selected != null) selected.onClick.Invoke();
        }
        private string NextResultLine(bool survived)
        {
            if (survived)
            {
                if (survivalIndex >= survivalOrder.Length) { ResetOrder(survivalOrder); survivalIndex = 0; }
                return SurvivalLines[survivalOrder[survivalIndex++]];
            }
            if (deathIndex >= deathOrder.Length) { ResetOrder(deathOrder); deathIndex = 0; }
            return DeathLines[deathOrder[deathIndex++]];
        }
        private static void ResetOrder(int[] order)
        {
            for (int i = 0; i < order.Length; i++) order[i] = i;
            for (int i = order.Length - 1; i > 0; i--)
            {
                int other = Random.Range(0, i + 1);
                int value = order[i]; order[i] = order[other]; order[other] = value;
            }
        }
        private void Permanent()
        {
            credits.SetText("CREDITOS {0}\nBlindagem Nv.{1}   Potencia Nv.{2}", permanent.Credits, permanent.ArmorLevel, permanent.DamageLevel);
        }
        public void ChooseFirst() => progression.Choose(0);
        public void ChooseSecond() => progression.Choose(1);
        public void ChooseThird() => progression.Choose(2);
        public void RerollChoices() => progression.TryReroll();
    }
}
