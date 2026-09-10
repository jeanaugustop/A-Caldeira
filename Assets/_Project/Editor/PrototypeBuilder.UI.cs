using System;
using System.IO;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using ACaldeira.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ACaldeira.Editor
{
    public static partial class PrototypeBuilder
    {
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(0.055f, 0.075f, 0.085f, 0.98f);
        private static readonly Color Amber = new Color(0.95f, 0.58f, 0.19f);
        private static void CreateUI(Transform parent, GameManager game, RuntimeServicesSO services, GameplaySimulation simulation,
            RunProgression progression, PermanentProgression permanent, SettingsManager settings, GameStateEventChannelSO state,
            ExperienceEventChannelSO xp, StageSO yard, StageSO assembly, StageSO stress)
        {
            const string essentials = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
            const string defaultFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            if (!Directory.Exists("Assets/TextMesh Pro/Resources") && File.Exists(essentials))
            {
                AssetDatabase.ImportPackage(essentials, false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Art/InterfaceFont.asset");
            if (font == null)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(defaultFontPath);
            }
            if (font == null)
            {
                throw new InvalidOperationException("TextMesh Pro Essential Resources could not be loaded. Import TMP Essential Resources, then run Generate Prototype again.");
            }
            var canvasGO = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(parent, false); canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            var eventSystem = Child("EventSystem", parent); eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            var ui = canvasGO.AddComponent<GameplayUI>();
            var safeContent = Panel("SafeArea", canvasGO.transform, false);
            var safeArea = canvasGO.AddComponent<SafeAreaPanel>(); Set(safeArea, "content", safeContent.transform);
            var menu = Panel("Menu", safeContent.transform, true);
            var main = Panel("Principal", menu.transform, true); var upgrades = Panel("Permanentes", menu.transform, true); var options = Panel("Configuracoes", menu.transform, true);
            var controller = menu.AddComponent<MainMenuManager>();
            Wire(controller, "runtimeServices", services, "defaultStage", yard, "assemblyStage", assembly, "stressStage", stress,
                "mainPanel", main, "permanentUpgradesPanel", upgrades, "settingsPanel", options);
            Label(main.transform, "INDUSTRIA / SETOR 07", 0, 275, 22, Amber);
            Label(main.transform, "A CALDEIRA", 0, 210, 62, Color.white);
            Label(main.transform, "SOBREVIVENCIA DE FERRO", 0, 155, 22, Amber);
            Button(main.transform, "INICIAR / PATIO DE TRIAGEM", 0, 70, controller.StartGame);
            Button(main.transform, "LINHAS DE MONTAGEM", 0, 8, controller.StartAssembly);
            Button(main.transform, "UPGRADES PERMANENTES", 0, -54, controller.ShowPermanentUpgrades);
            Button(main.transform, "CONFIGURACOES", 0, -116, controller.ShowSettings);
            Button(main.transform, "TESTE DE CARGA / 1200", 0, -178, controller.StartStress);
            Button(main.transform, "SAIR", 0, -240, controller.Quit);
            Label(main.transform, "WASD / SETAS / CONTROLE   |   TOQUE: ARRASTE A METADE ESQUERDA", 0, -315, 16, new Color(0.55f,0.65f,0.65f));
            Label(upgrades.transform, "OFICINA PERMANENTE", 0, 235, 38, Amber);
            var credits = Label(upgrades.transform, "CREDITOS 0000000\nBlindagem Nv.00   Potencia Nv.00", 0, 150, 24, Color.white);
            Button(upgrades.transform, "BLINDAGEM +10 HP", 0, 25, permanent.BuyArmor);
            Button(upgrades.transform, "POTENCIA +5% DANO", 0, -40, permanent.BuyDamage);
            var costs = Label(upgrades.transform, "", 0, -110, 20, Amber);
            var workshop = upgrades.AddComponent<WorkshopPanel>(); Wire(workshop, "permanent", permanent, "costs", costs);
            Button(upgrades.transform, "VOLTAR", 0, -220, controller.ShowMain);
            var settingsPanel = options.AddComponent<SettingsPanel>();
            Label(options.transform, "CONFIGURACOES", 0, 275, 38, Amber);
            Label(options.transform, "MASTER", -270, 190, 20, Color.white, 180);
            Label(options.transform, "EFEITOS", -270, 135, 20, Color.white, 180);
            Label(options.transform, "MUSICA", -270, 80, 20, Color.white, 180);
            var master = Slider(options.transform, 60, 190, true, Amber);
            var sfx = Slider(options.transform, 60, 135, true, Amber);
            var music = Slider(options.transform, 60, 80, true, Amber);
            UnityEventTools.AddPersistentListener(master.onValueChanged, settingsPanel.PreviewAudio);
            UnityEventTools.AddPersistentListener(sfx.onValueChanged, settingsPanel.PreviewAudio);
            UnityEventTools.AddPersistentListener(music.onValueChanged, settingsPanel.PreviewAudio);
            var resolution = Button(options.transform, "Resolucao", 0, 0, settingsPanel.NextResolution);
            var mode = Button(options.transform, "Modo de janela", 0, -62, settingsPanel.NextMode);
            Button(options.transform, "APLICAR VIDEO", 0, -135, settingsPanel.ApplyVideo);
            Button(options.transform, "SALVAR E VOLTAR", 0, -220, controller.ApplySettingsAndReturn);
            var confirmation = Panel("Confirmar video", options.transform, true);
            var countdown = Label(confirmation.transform, "Manter alteracoes?", 0, 90, 32, Amber);
            Button(confirmation.transform, "MANTER", 0, 0, settingsPanel.ConfirmVideo);
            Button(confirmation.transform, "REVERTER", 0, -65, settingsPanel.RevertVideo);
            Wire(settingsPanel, "settings", settings, "master", master, "sfx", sfx, "music", music,
                "resolution", resolution.GetComponentInChildren<TMP_Text>(), "mode", mode.GetComponentInChildren<TMP_Text>(),
                "resolutionButton", resolution, "modeButton", mode, "confirmation", confirmation, "countdown", countdown);
            confirmation.SetActive(false);
            var hud = Panel("HUD", safeContent.transform, false);
            var status = Label(hud.transform, "CALDEIRA / 000000s    MAQUINAS 000000", 0, 315, 22, Color.white);
            var xpText = Label(hud.transform, "NIVEL 000   SUCATA 000000/000000", 0, -270, 20, Amber);
            var health = Slider(hud.transform, -385, 275, false, new Color(0.35f,0.8f,0.75f));
            var experience = Slider(hud.transform, 0, -310, false, Amber);
            Button(hud.transform, "PAUSA", 430, 275, game.Pause, 170);
            var pause = Panel("Pausa", safeContent.transform, true);
            Label(pause.transform, "SISTEMAS SUSPENSOS", 0, 160, 42, Amber);
            Button(pause.transform, "CONTINUAR", 0, 40, game.Resume);
            Button(pause.transform, "ABANDONAR / MENU", 0, -40, game.ReturnToMainMenu);
            var choices = Panel("Fundicao", safeContent.transform, true);
            Label(choices.transform, "FUNDIR NOVO EQUIPAMENTO", 0, 200, 38, Amber);
            var choiceButtons = new Button[3]; var choiceLabels = new TMP_Text[3];
            UnityAction[] choose = { ui.ChooseFirst, ui.ChooseSecond, ui.ChooseThird };
            for (int i = 0; i < 3; i++)
            {
                choiceButtons[i] = Button(choices.transform, "Upgrade", 0, 80-i*110, choose[i], 650, 90);
                choiceLabels[i] = choiceButtons[i].GetComponentInChildren<TMP_Text>(); choiceLabels[i].fontSize = 23;
            }
            var gameOver = Panel("Resultado", safeContent.transform, true);
            var result = Label(gameOver.transform, "EXOTRAJE DESTRUIDO", 0, 170, 38, Amber);
            Button(gameOver.transform, "NOVA TENTATIVA", 0, 10, game.Retry);
            Button(gameOver.transform, "MENU", 0, -65, game.ReturnToMainMenu);
            var loading = Panel("Carregamento", safeContent.transform, true);
            Label(loading.transform, "PRESSURIZANDO SISTEMAS...", 0, 0, 34, Amber);
            Wire(ui, "gameManager", game, "simulation", simulation, "progression", progression, "permanent", permanent,
                "stateChanged", state, "experienceChanged", xp, "menu", menu, "hud", hud, "pause", pause, "choices", choices,
                "gameOver", gameOver, "loading", loading, "status", status, "xpText", xpText, "result", result,
                "credits", credits, "healthBar", health, "xpBar", experience);
            Array(ui, "choiceButtons", choiceButtons); Array(ui, "choiceLabels", choiceLabels);
            upgrades.SetActive(false); options.SetActive(false); hud.SetActive(false); pause.SetActive(false);
            choices.SetActive(false); gameOver.SetActive(false); loading.SetActive(false);
        }
        private static GameObject Panel(string name, Transform parent, bool background)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            if (background) { var image = go.AddComponent<Image>(); image.color = Ink; }
            return go;
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform)); var r = (RectTransform)go.transform;
            r.SetParent(parent, false); r.anchorMin = r.anchorMax = new Vector2(0.5f,0.5f); r.anchoredPosition = new Vector2(x,y); r.sizeDelta = new Vector2(width,height); return r;
        }
        private static TMP_Text Label(Transform parent, string text, float x, float y, float size, Color color, float width = 1100)
        {
            var r = Rect("Text", parent, x,y,width,100); var label = r.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = size; label.color = color; label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false; label.text = text; label.enableWordWrapping = true; return label;
        }
        private static Button Button(Transform parent, string text, float x, float y, UnityAction action, float width = 440, float height = 50)
        {
            var r = Rect(text, parent, x,y,width,height); var image = r.gameObject.AddComponent<Image>(); image.color = new Color(0.17f,0.21f,0.22f);
            var button = r.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = Amber; colors.selectedColor = Amber; button.colors = colors;
            var label = Label(r, text,0,0,22,Color.white,width-20); label.rectTransform.sizeDelta = new Vector2(width-20,height);
            UnityEventTools.AddPersistentListener(button.onClick, action);
            if (parent.GetComponent<MenuFocus>() == null)
            { var focus = parent.gameObject.AddComponent<MenuFocus>(); Set(focus, "first", button); }
            return button;
        }
        private static Slider Slider(Transform parent, float x, float y, bool interactive, Color color)
        {
            var r = Rect("Bar", parent,x,y,350,22); var background = r.gameObject.AddComponent<Image>(); background.color = new Color(0.16f,0.2f,0.2f);
            var fill = Rect("Fill",r,0,0,350,22);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
            var image = fill.gameObject.AddComponent<Image>(); image.color = color;
            var slider = r.gameObject.AddComponent<Slider>(); slider.fillRect = fill; slider.targetGraphic = image;
            slider.minValue = 0; slider.maxValue = 1; slider.value = 1; slider.interactable = interactive;
            var handle = Rect("Handle", r, 0,0,18,30); var handleImage = handle.gameObject.AddComponent<Image>(); handleImage.color = Color.white;
            slider.handleRect = handle; handle.gameObject.SetActive(interactive); return slider;
        }
    }
}
