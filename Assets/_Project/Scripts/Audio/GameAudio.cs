using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.Simulation;
using ACaldeira.UI;
using UnityEngine;

namespace ACaldeira.Audio
{
    public sealed class GameAudio : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SettingsManager settings;
        [SerializeField] private GameplaySimulation simulation;
        [SerializeField] private GameStateEventChannelSO stateChanged;
        [SerializeField] private AudioSource music, sfx;
        private float lastSound;
        private void OnEnable() { stateChanged.Raised += State; simulation.EnemyKilled += Kill; settings.Changed += Apply; }
        private void Start() { Apply(settings.Current); }
        private void OnDisable() { stateChanged.Raised -= State; simulation.EnemyKilled -= Kill; settings.Changed -= Apply; }
        private void Apply(SettingsData data)
        {
            // Sources already route through the mixer. No per-frame settings work.
            music.mute = data.MasterVolume <= 0 || data.MusicVolume <= 0;
            sfx.mute = data.MasterVolume <= 0 || data.SfxVolume <= 0;
        }
        private void State(GameState state)
        {
            if (state == GameState.Playing)
            {
                AudioClip clip = gameManager.ActiveStage.Music;
                if (music.clip != clip) { music.clip = clip; music.Play(); }
                else if (!music.isPlaying) music.UnPause();
            }
            else if (state == GameState.Paused || state == GameState.LevelUp) music.Pause();
            else music.Stop();
        }
        private void Kill()
        {
            if (Time.unscaledTime - lastSound < 0.07f) return;
            lastSound = Time.unscaledTime; sfx.Play();
        }
    }
}
