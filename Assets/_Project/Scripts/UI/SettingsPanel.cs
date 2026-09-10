using ACaldeira.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ACaldeira.UI
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private SettingsManager settings;
        [SerializeField] private Slider master, sfx, music;
        [SerializeField] private TMP_Text resolution, mode;
        [SerializeField] private Button resolutionButton, modeButton;
        [SerializeField] private GameObject confirmation;
        [SerializeField] private TMP_Text countdown;
        private Resolution[] resolutions;
        private int oldWidth, oldHeight, oldRate;
        private WindowMode oldMode;
        private float remaining;
        private bool initialized;
        private int resolutionIndex, modeIndex;
        private void Start()
        {
            resolutions = settings.GetAvailableResolutions();
            resolutionButton.interactable = !Application.isMobilePlatform && resolutions.Length > 0;
            modeButton.interactable = !Application.isMobilePlatform;
            initialized = true; Refresh();
        }
        private void OnEnable() { if (initialized) Refresh(); }
        private void OnDisable() { if (remaining > 0) RevertVideo(); }
        private void Refresh()
        {
            var c = settings.Current;
            master.SetValueWithoutNotify(c.MasterVolume); sfx.SetValueWithoutNotify(c.SfxVolume); music.SetValueWithoutNotify(c.MusicVolume);
            modeIndex = (int)c.WindowMode;
            for (int i = 0; i < resolutions.Length; i++)
                if (resolutions[i].width == c.ResolutionWidth && resolutions[i].height == c.ResolutionHeight) resolutionIndex = i;
            Labels();
        }
        public void NextResolution() { if (resolutions.Length > 0) resolutionIndex = (resolutionIndex + 1) % resolutions.Length; Labels(); }
        public void NextMode() { modeIndex = (modeIndex + 1) % 3; Labels(); }
        private void Labels()
        {
            if (resolutions.Length > 0) { var r = resolutions[resolutionIndex]; resolution.SetText("Resolucao: {0} x {1} / {2} Hz", r.width, r.height, r.refreshRate); }
            else resolution.text = "Resolucao nativa";
            mode.text = modeIndex == 0 ? "Janela" : modeIndex == 1 ? "Janela sem bordas" : "Tela cheia";
        }
        public void PreviewAudio(float unused) { settings.SetAudio(master.value, sfx.value, music.value); }
        public void ApplyVideo()
        {
            if (Application.isMobilePlatform || resolutions.Length == 0) { settings.Save(); return; }
            if (remaining > 0) return;
            var c = settings.Current; oldWidth = c.ResolutionWidth; oldHeight = c.ResolutionHeight; oldRate = c.RefreshRate; oldMode = c.WindowMode;
            var r = resolutions[resolutionIndex]; settings.SetVideo(r.width, r.height, r.refreshRate, (WindowMode)modeIndex);
            remaining = 12f; confirmation.SetActive(true);
        }
        public void ConfirmVideo() { remaining = 0; confirmation.SetActive(false); settings.Save(); }
        public void RevertVideo()
        {
            settings.SetVideo(oldWidth, oldHeight, oldRate, oldMode); remaining = 0; confirmation.SetActive(false); Refresh();
        }
        private void Update()
        {
            if (remaining <= 0) return;
            remaining -= Time.unscaledDeltaTime; countdown.SetText("Manter alteracoes? {0:0}s", Mathf.Ceil(remaining));
            if (remaining <= 0) RevertVideo();
        }
    }
}
