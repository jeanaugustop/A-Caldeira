using System;
using ACaldeira.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace ACaldeira.UI
{
    public sealed class SettingsManager : MonoBehaviour
    {
        private const string PlayerPrefsKey = "acal.settings.v1";
        private const float MutedDecibels = -80f;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterParameter = "MasterVolume";
        [SerializeField] private string sfxParameter = "SfxVolume";
        [SerializeField] private string musicParameter = "MusicVolume";

        private SettingsData _current = new SettingsData();

        public event Action<SettingsData> Changed;
        public SettingsData Current => _current;

        private void Awake()
        {
            Load();
        }

        private void Start()
        {
            Apply();
        }

        public void SetAudio(float master, float sfx, float music)
        {
            _current.SetAudio(master, sfx, music);
            ApplyAudio();
            Changed?.Invoke(_current);
        }

        public void SetVideo(int width, int height, int refreshRate, WindowMode mode)
        {
            _current.SetVideo(width, height, refreshRate, mode);
            ApplyVideo();
            Changed?.Invoke(_current);
        }

        public void ApplyAndSave()
        {
            Apply();
            Save();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                Resolution current = Screen.currentResolution;
                _current.SetVideo(current.width, current.height, current.refreshRate, WindowMode.Borderless);
                return;
            }

            try
            {
                string json = PlayerPrefs.GetString(PlayerPrefsKey);
                SettingsData loaded = JsonUtility.FromJson<SettingsData>(json);
                if (loaded != null)
                {
                    _current = loaded;
                    _current.SetAudio(loaded.MasterVolume, loaded.SfxVolume, loaded.MusicVolume);
                    _current.SetVideo(loaded.ResolutionWidth, loaded.ResolutionHeight, loaded.RefreshRate,
                        Enum.IsDefined(typeof(WindowMode), loaded.WindowMode) ? loaded.WindowMode : WindowMode.Borderless);
                }
            }
            catch (ArgumentException)
            {
                PlayerPrefs.DeleteKey(PlayerPrefsKey);
            }
        }

        public Resolution[] GetAvailableResolutions()
        {
            // Menu-only API. Unity creates the returned array; never call from a gameplay loop.
            return Screen.resolutions;
        }

        public void Save()
        {
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(_current));
            PlayerPrefs.Save();
        }

        private void Apply()
        {
            ApplyAudio();
            ApplyVideo();
            Changed?.Invoke(_current);
        }

        private void ApplyAudio()
        {
            if (audioMixer == null)
            {
                return;
            }

            audioMixer.SetFloat(masterParameter, ToDecibels(_current.MasterVolume));
            audioMixer.SetFloat(sfxParameter, ToDecibels(_current.SfxVolume));
            audioMixer.SetFloat(musicParameter, ToDecibels(_current.MusicVolume));
        }

        private void ApplyVideo()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            FullScreenMode mode = ToFullScreenMode(_current.WindowMode);
            Screen.SetResolution(
                _current.ResolutionWidth,
                _current.ResolutionHeight,
                mode,
                _current.RefreshRate);
#else
            Screen.fullScreen = true;
#endif
        }

        private static float ToDecibels(float linearValue)
        {
            return linearValue <= 0.0001f ? MutedDecibels : Mathf.Log10(linearValue) * 20f;
        }

        private static FullScreenMode ToFullScreenMode(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Windowed:
                    return FullScreenMode.Windowed;
                case WindowMode.ExclusiveFullscreen:
                    return FullScreenMode.ExclusiveFullScreen;
                default:
                    return FullScreenMode.FullScreenWindow;
            }
        }
    }
}
