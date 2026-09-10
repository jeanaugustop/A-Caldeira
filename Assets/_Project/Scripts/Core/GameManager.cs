using System.Collections;
using ACaldeira.Combat;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.UI;
using ACaldeira.Simulation;
using ACaldeira.Progression;
using ACaldeira.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ACaldeira.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private RuntimeServicesSO runtimeServices;
        [SerializeField] private GameplaySimulation simulation;
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private GameObject worldRoot;
        [SerializeField] private PerformanceProbe performance;
        [SerializeField] private GameStateEventChannelSO gameStateChanged;
        [SerializeField] private VoidEventChannelSO runStarted;
        [SerializeField] private VoidEventChannelSO runEnded;
        [SerializeField] private string mainMenuScene = "01_MainMenu";
        [SerializeField] private bool loadMainMenuOnStart = true;

        private StageSO _activeStage;
        private bool rewarded;
        private bool ownsServices;

        public GameState State { get; private set; } = GameState.MainMenu;
        public StageSO ActiveStage => _activeStage;

        private void Awake()
        {
            if (!runtimeServices.TryBind(this, settingsManager))
            { transform.root.gameObject.SetActive(false); return; }
            ownsServices = true;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(transform.root.gameObject);
            poolManager.Initialize();
            worldRoot.SetActive(false);
            SetState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (!ownsServices) return;
            if (runtimeServices != null) runtimeServices.Clear(this);
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (loadMainMenuOnStart && SceneManager.GetActiveScene().name != mainMenuScene)
            {
                StartCoroutine(LoadMainMenuRoutine());
            }
        }

        public void StartRun(StageSO stage)
        {
            if (stage == null || (State != GameState.MainMenu && State != GameState.GameOver))
            {
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(stage.SceneName))
            { Debug.LogError("Stage scene is missing from Build Settings."); return; }

            _activeStage = stage;
            StartCoroutine(LoadStageRoutine(stage));
        }

        public void Pause()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused && State != GameState.LevelUp)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void EnterLevelUp()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.LevelUp);
            }
        }

        public void EndRun()
        {
            if (State != GameState.Playing && State != GameState.Paused && State != GameState.LevelUp)
            {
                return;
            }

            Time.timeScale = 0f;
            waveSpawner.Stop();
            performance.Finish();
            SetState(GameState.GameOver);
            if (!rewarded && !simulation.StressMode)
            { rewarded = true; permanent.Award(simulation.Kills / 3 + (simulation.Won ? 50 : 5)); }
            runEnded?.Raise();
        }

        public void ReturnToMainMenu()
        {
            if (State == GameState.Loading || State == GameState.MainMenu) return;
            StartCoroutine(LoadMainMenuRoutine());
        }

        public void Retry() { if (State == GameState.GameOver) StartRun(_activeStage); }

        private IEnumerator LoadStageRoutine(StageSO stage)
        {
            Time.timeScale = 1f;
            SetState(GameState.Loading);
            waveSpawner.Stop(); poolManager.ReturnAll(); worldRoot.SetActive(false);
            AsyncOperation operation = SceneManager.LoadSceneAsync(stage.SceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            worldRoot.SetActive(true);
            rewarded = false;
            simulation.Begin(stage);
            SetState(GameState.Playing);
            if (simulation.StressMode) performance.Begin();
            runStarted?.Raise();
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            Time.timeScale = 1f;
            SetState(GameState.Loading);
            performance.Finish();
            waveSpawner.Stop(); poolManager.ReturnAll(); worldRoot.SetActive(false);
            AsyncOperation operation = SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            _activeStage = null;
            SetState(GameState.MainMenu);
        }

        private void SetState(GameState state)
        {
            State = state;
            gameStateChanged?.Raise(state);
        }

        private void OnApplicationPause(bool paused) { if (paused) Pause(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
    }
}
