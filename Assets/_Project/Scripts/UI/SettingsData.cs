using System;
using ACaldeira.Data;
using UnityEngine;

namespace ACaldeira.UI
{
    [Serializable]
    public sealed class SettingsData
    {
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField] private int resolutionWidth = 1920;
        [SerializeField] private int resolutionHeight = 1080;
        [SerializeField] private int refreshRate = 60;
        [SerializeField] private WindowMode windowMode = WindowMode.Borderless;

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;
        public int ResolutionWidth => resolutionWidth;
        public int ResolutionHeight => resolutionHeight;
        public int RefreshRate => refreshRate;
        public WindowMode WindowMode => windowMode;

        public void SetAudio(float master, float sfx, float music)
        {
            masterVolume = Mathf.Clamp01(master);
            sfxVolume = Mathf.Clamp01(sfx);
            musicVolume = Mathf.Clamp01(music);
        }

        public void SetVideo(int width, int height, int rate, WindowMode mode)
        {
            resolutionWidth = Mathf.Max(640, width);
            resolutionHeight = Mathf.Max(360, height);
            refreshRate = Mathf.Max(0, rate);
            windowMode = mode;
        }
    }
}
