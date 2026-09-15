using System;
using System.Collections.Generic;
using System.IO;
using ACaldeira.Audio;
using ACaldeira.Core;
using ACaldeira.Combat;
using ACaldeira.Collectibles;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.Events;
using ACaldeira.Input;
using ACaldeira.Pooling;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using ACaldeira.UI;
using ACaldeira.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace ACaldeira.Editor
{
    // All authoring/creation is Editor-only. Runtime only rents serialized scene objects.
    [InitializeOnLoad]
    public static partial class PrototypeBuilder
    {
        private const string Root = "Assets/_Project/Generated";
        private static Sprite square;
        private static Material spriteMaterial;
        static PrototypeBuilder() { EditorApplication.delayCall += FirstImport; }
        private static void FirstImport()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!File.Exists(Root + "/Scenes/00_Bootstrap.unity") && !SessionState.GetBool("Caldeira.GenerationAttempted", false))
            {
                SessionState.SetBool("Caldeira.GenerationAttempted", true);
                if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) Generate();
                else Debug.Log("A Caldeira: save the current scene, then Tools > A Caldeira > Generate Prototype.");
            }
        }
        [MenuItem("Tools/A Caldeira/Generate Prototype")]
        public static void Generate()
        {
            if (File.Exists(Root + "/Scenes/00_Bootstrap.unity"))
            { Debug.Log("Prototype already exists. Open Generated/Scenes/00_Bootstrap; existing content was preserved."); return; }
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene before generation.");
            Directory.CreateDirectory(Root + "/Scenes"); Directory.CreateDirectory(Root + "/Data");
            Directory.CreateDirectory(Root + "/Art"); Directory.CreateDirectory(Root + "/Audio");
            Directory.CreateDirectory(Root + "/Prefabs"); AssetDatabase.Refresh();
            ConfigureRendering(); CreateArt();
            AudioMixer mixer = CreateMixer();
            AudioClip music = CreateTone("IndustrialPulse", 8f, true);
            AudioClip impact = CreateTone("MetalImpact", 0.15f, false);
            var keys = new PoolKeySO[9];
            string[] ids = { "Drone", "Tractor", "Heavy", "Rivet", "Flame", "Saw", "Stake", "Scrap", "Hazard" };
            for (int i = 0; i < ids.Length; i++) { keys[i] = Asset<PoolKeySO>(ids[i] + "Pool"); Set(keys[i], "id", ids[i]); }
            PoolKeySO oilKey = Asset<PoolKeySO>("OilPool"); Set(oilKey, "id", "Oil");
            EnemySO drone = Enemy("Drone de solda", keys[0], keys[7], 18, 1.6f, 6, 2, CollectibleKind.HotScrap);
            EnemySO tractor = Enemy("Trator sucatador", keys[1], oilKey, 65, 1.05f, 12, 5, CollectibleKind.Oil);
            EnemySO heavy = Enemy("Prensa autonoma", keys[2], oilKey, 180, 0.65f, 25, 12, CollectibleKind.Oil);
            WeaponSO rivet = Weapon("Rebites de Pressao", keys[3], WeaponDeliveryMode.Projectile, 14, 0.45f, 18, 1.1f, 1, 0);
            WeaponSO flame = Weapon("Oleo Cru", keys[4], WeaponDeliveryMode.Beam, 5, 0.18f, 8, 0.45f, 5, 3);
            WeaponSO saw = Weapon("Serras Orbitais", keys[5], WeaponDeliveryMode.Orbital, 24, 2f, 0, 2f, 3, 15);
            WeaponSO stake = Weapon("Estacas Hidraulicas", keys[6], WeaponDeliveryMode.Projectile, 70, 1.8f, 26, 0.8f, 1, 6);
            WeaponSO evolved = Weapon("Tempestade de Rebites", keys[3], WeaponDeliveryMode.Projectile, 26, 0.22f, 22, 1.2f, 3, 3);
            var upgrades = new List<UpgradeSO>();
            UpgradeSO pressure = Upgrade("Pressao extra", "Dano +20%", UpgradeCategory.StatBoost, null, null, StatId.Damage, 0.2f, 5);
            upgrades.Add(pressure);
            upgrades.Add(Upgrade("Servo rapido", "Velocidade +15%", UpgradeCategory.StatBoost, null, null, StatId.MoveSpeed, 0.15f, 4));
            upgrades.Add(Upgrade("Eletroima", "Coleta +30%", UpgradeCategory.StatBoost, null, null, StatId.PickupRadius, 0.3f, 4));
            upgrades.Add(Upgrade("Valvula rapida", "Intervalo de disparo -10%", UpgradeCategory.StatBoost, null, null, StatId.Cooldown, -0.1f, 5));
            upgrades.Add(Upgrade("Fundir lanca-chamas", "Abre um leque de oleo em combustao", UpgradeCategory.WeaponUnlock, flame));
            upgrades.Add(Upgrade("Fundir serras", "Tres serras giram ao redor do exotraje", UpgradeCategory.WeaponUnlock, saw));
            upgrades.Add(Upgrade("Fundir lanca-estacas", "Perfura ate sete maquinas", UpgradeCategory.WeaponUnlock, stake));
            var evolution = Upgrade("Tempestade de Rebites", "Evolui a pistola para disparos triplos", UpgradeCategory.WeaponEvolution, rivet, evolved);
            Array(evolution, "prerequisites", new Object[] { pressure }); upgrades.Add(evolution);
            StageSO yard = Stage("Patio de Triagem", "10_PatioTriagem", "yard", new[] { drone, tractor, heavy }, keys[8], music, false);
            StageSO assembly = Stage("Linhas de Montagem", "11_LinhasMontagem", "assembly", new[] { drone, tractor, heavy }, keys[8], music, false);
            StageSO stress = Stage("Teste 1200", "90_StressTest", "stress", new[] { drone }, keys[8], music, true);
            var state = Asset<GameStateEventChannelSO>("GameStateChanged"); var xp = Asset<ExperienceEventChannelSO>("ExperienceChanged");
            var started = Asset<VoidEventChannelSO>("RunStarted"); var ended = Asset<VoidEventChannelSO>("RunEnded");
            var services = Asset<RuntimeServicesSO>("RuntimeServices");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Caldeira.Persistent");
            var game = root.AddComponent<GameManager>(); var pools = root.AddComponent<PoolManager>();
            var waves = root.AddComponent<WaveSpawner>(); var weapons = root.AddComponent<WeaponManager>();
            var settings = root.AddComponent<SettingsManager>(); var permanent = root.AddComponent<PermanentProgression>();
            var progression = root.AddComponent<RunProgression>(); var input = root.AddComponent<PlayerInputReader>();
            var simulation = root.AddComponent<GameplaySimulation>(); var probe = root.AddComponent<PerformanceProbe>();
            var world = Child("World", root.transform);
            var player = Visual("Engenheiro", world.transform, new Color(0.3f, 0.72f, 0.76f), new Vector2(0.75f, 1f), 5).transform;
            var visor = Visual("Visor", player, new Color(1f, 0.75f, 0.2f), new Vector2(0.6f, 0.18f), 6);
            visor.transform.localPosition = new Vector3(0, 0.22f, 0);
            var camera = Child("Camera", root.transform).AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 10; camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.035f, 0.05f, 0.06f); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>(); camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var registrations = new List<PooledBehaviour[]>(); var registrationKeys = new List<PoolKeySO>();
            var enemyBank = new List<EnemyActor>(); var projectileBank = new List<ProjectileActor>();
            var collectibleBank = new List<CollectibleActor>();
            int[] capacities = { 1280, 160, 64, 768, 512, 64, 256, 1024, 32, 256 };
            Color[] colors = { new Color(0.85f,0.35f,0.15f), new Color(0.6f,0.55f,0.3f), Color.gray, Color.yellow, new Color(1,0.35f,0.05f), Color.white, Color.cyan, new Color(1,0.5f,0.05f), Color.red, new Color(0.3f,0.7f,0.9f) };
            HazardActor[] hazardBank = null;
            for (int i = 0; i < capacities.Length; i++)
            {
                PoolKeySO key = i == 9 ? oilKey : keys[i];
                var parent = Child(key.Id + "Bank", world.transform);
                PooledBehaviour[] bank = new PooledBehaviour[capacities[i]];
                for (int j = 0; j < bank.Length; j++)
                {
                    var size = i < 3 ? Vector2.one * (0.65f + i * 0.4f) : i == 8 ? Vector2.one * 3 : Vector2.one * (i >= 7 ? 0.22f : 0.3f);
                    var go = Visual(key.Id, parent.transform, colors[i], size, i < 3 ? 2 : 3);
                    if (i < 3) { var actor = go.AddComponent<EnemyActor>(); bank[j] = actor; enemyBank.Add(actor); }
                    else if (i < 7) { var actor = go.AddComponent<ProjectileActor>(); bank[j] = actor; projectileBank.Add(actor); }
                    else if (i == 8) { var actor = go.AddComponent<HazardActor>(); Set(actor, "visual", go.GetComponent<SpriteRenderer>()); bank[j] = actor; }
                    else { var actor = go.AddComponent<CollectibleActor>(); bank[j] = actor; collectibleBank.Add(actor); }
                    go.SetActive(false);
                    if (j == 0) PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/" + key.Id + ".prefab");
                }
                if (i == 8) { hazardBank = new HazardActor[bank.Length]; for (int j = 0; j < bank.Length; j++) hazardBank[j] = (HazardActor)bank[j]; }
                registrations.Add(bank); registrationKeys.Add(key);
            }
            var poolData = new SerializedObject(pools); var list = poolData.FindProperty("registrations"); list.arraySize = registrations.Count;
            for (int i = 0; i < registrations.Count; i++)
            {
                var entry = list.GetArrayElementAtIndex(i); entry.FindPropertyRelative("key").objectReferenceValue = registrationKeys[i];
                FillArray(entry.FindPropertyRelative("prewarmedInstances"), registrations[i]);
            }
            poolData.ApplyModifiedPropertiesWithoutUndo();
            Wire(game, "poolManager", pools, "waveSpawner", waves, "weaponManager", weapons, "settingsManager", settings, "runtimeServices", services,
                "simulation", simulation, "permanent", permanent, "worldRoot", world, "performance", probe, "gameStateChanged", state, "runStarted", started, "runEnded", ended);
            Wire(waves, "poolManager", pools, "player", player);
            Wire(weapons, "poolManager", pools, "gameManager", game, "progression", progression, "permanent", permanent, "muzzle", player, "maxWeaponSlots", 8);
            Array(weapons, "startingWeapons", new Object[] { rivet });
            Array(weapons, "stressWeapons", new Object[] { rivet, flame, saw, stake });
            Wire(progression, "gameManager", game, "weapons", weapons, "experienceChanged", xp, "maxEquipmentSlots", 8);
            Wire(input, "gameManager", game); Wire(probe, "gameManager", game, "simulation", simulation, "pools", pools);
            Wire(simulation, "gameManager", game, "pools", pools, "waves", waves, "weapons", weapons, "progression", progression,
                "permanent", permanent, "input", input, "player", player, "worldCamera", camera);
            Array(simulation, "enemies", enemyBank.ToArray()); Array(simulation, "projectiles", projectileBank.ToArray());
            Array(simulation, "collectibles", collectibleBank.ToArray()); Array(simulation, "hazards", hazardBank);
            Set(settings, "audioMixer", mixer);
            var audio = root.AddComponent<GameAudio>();
            var bgm = Child("BGM", root.transform).AddComponent<AudioSource>(); bgm.loop = true; bgm.playOnAwake = false;
            var sfx = Child("SFX", root.transform).AddComponent<AudioSource>(); sfx.clip = impact; sfx.playOnAwake = false;
            bgm.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0]; sfx.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            Wire(audio, "gameManager", game, "settings", settings, "simulation", simulation, "stateChanged", state, "music", bgm, "sfx", sfx);
            CreateUI(root.transform, game, services, simulation, progression, permanent, settings, state, xp, yard, assembly, stress);
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/00_Bootstrap.unity");
            string[] scenes = { "01_MainMenu", "10_PatioTriagem", "11_LinhasMontagem", "90_StressTest" };
            for (int s = 0; s < scenes.Length; s++)
            {
                var level = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (s > 0) CreateEnvironment(s == 2);
                EditorSceneManager.SaveScene(level, Root + "/Scenes/" + scenes[s] + ".unity");
            }
            var builds = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(Root + "/Scenes/00_Bootstrap.unity", true) };
            for (int i = 0; i < scenes.Length; i++) builds.Add(new EditorBuildSettingsScene(Root + "/Scenes/" + scenes[i] + ".unity", true));
            EditorBuildSettings.scenes = builds.ToArray();
            PlayerSettings.companyName = "Caldeira Studio"; PlayerSettings.productName = "A Caldeira";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var inputSetting = playerSettings.FindProperty("activeInputHandler");
            if (inputSetting != null) { inputSetting.intValue = 1; playerSettings.ApplyModifiedPropertiesWithoutUndo(); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root + "/Scenes/00_Bootstrap.unity");
            ValidatePrototype();
            Debug.Log("A Caldeira generated. Press Play. Stress test reports are saved in persistentDataPath.");
        }
        private static GameObject Child(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go; }
        private static GameObject Visual(string name, Transform parent, Color color, Vector2 size, int order)
        {
            var go = Child(name, parent); var r = go.AddComponent<SpriteRenderer>(); r.sprite = square; r.sharedMaterial = spriteMaterial; r.color = color; r.sortingOrder = order;
            go.transform.localScale = new Vector3(size.x, size.y, 1); return go;
        }
        private static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Root + "/Data/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path); if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        private static void Wire(Object target, params object[] pairs)
        { for (int i = 0; i < pairs.Length; i += 2) Set(target, (string)pairs[i], pairs[i + 1]); }
        private static void Set(Object target, string name, object value)
        {
            var so = new SerializedObject(target); var p = so.FindProperty(name);
            if (p == null) throw new InvalidOperationException(target.name + ": unknown serialized field " + name);
            if (value is Object o) p.objectReferenceValue = o;
            else if (value == null) p.objectReferenceValue = null;
            else if (value is string s) p.stringValue = s;
            else if (value is bool b) p.boolValue = b;
            else if (value is float f) p.floatValue = f;
            else if (value is int n) { if (p.propertyType == SerializedPropertyType.Float) p.floatValue = n; else p.intValue = n; }
            else if (value is Enum e) p.intValue = Convert.ToInt32(e);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Array(Object target, string name, Object[] values)
        { var so = new SerializedObject(target); FillArray(so.FindProperty(name), values); so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void FillArray(SerializedProperty p, Object[] values)
        { p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
    }
}
