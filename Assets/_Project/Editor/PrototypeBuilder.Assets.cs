using System.IO;
using System.Text;
using ACaldeira.Data;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ACaldeira.Editor
{
    public static partial class PrototypeBuilder
    {
        private static void ConfigureRendering()
        {
            var renderer = Asset<Renderer2DData>("Renderer2D");
            ResourceReloader.ReloadAllNullIn(renderer, "Packages/com.unity.render-pipelines.universal");
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root + "/Data/URP.asset");
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, Root + "/Data/URP.asset");
            }
            pipeline.supportsHDR = false; pipeline.msaaSampleCount = 1;
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
        }
        private static void CreateArt()
        {
            string path = Root + "/Art/Steel.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    float shade = x < 2 || y < 2 || x > 29 || y > 29 ? 0.45f : ((x + y) % 7 == 0 ? 0.85f : 1f);
                    texture.SetPixel(x, y, new Color(shade, shade, shade));
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            square = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Steel.mat");
            if (spriteMaterial == null)
            {
                spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(spriteMaterial, Root + "/Art/Steel.mat");
            }
        }
        private static EnemySO Enemy(string name, PoolKeySO key, PoolKeySO drop, float health, float speed, float damage, int xp, CollectibleKind kind)
        {
            var e = Asset<EnemySO>(name);
            Wire(e, "id", key.Id, "displayName", name, "sprite", square, "actorPool", key, "collectiblePool", drop,
                "maxHealth", health, "moveSpeed", speed, "contactDamage", damage, "experienceValue", xp, "dropKind", kind);
            return e;
        }
        private static WeaponSO Weapon(string name, PoolKeySO key, WeaponDeliveryMode mode, float damage, float cooldown, float speed, float duration, int count, int pierce)
        {
            var w = Asset<WeaponSO>(name);
            Wire(w, "id", name, "displayName", name, "icon", square, "projectilePool", key, "deliveryMode", mode,
                "damage", damage, "cooldown", cooldown, "projectileSpeed", speed, "duration", duration, "projectilesPerShot", count, "pierce", pierce,
                "damageType", mode == WeaponDeliveryMode.Beam ? DamageType.Heat : mode == WeaponDeliveryMode.Orbital ? DamageType.Cutting : DamageType.Impact);
            return w;
        }
        private static UpgradeSO Upgrade(string name, string description, UpgradeCategory category, WeaponSO granted = null, WeaponSO evolved = null,
            StatId stat = StatId.Damage, float value = 0, int stacks = 1)
        {
            var u = Asset<UpgradeSO>(name + "Upgrade");
            Wire(u, "id", name, "displayName", name, "description", description, "category", category, "grantedWeapon", granted,
                "evolvedWeapon", evolved, "maxStacks", stacks, "icon", square);
            var so = new SerializedObject(u); var modifiers = so.FindProperty("modifiers"); modifiers.arraySize = category == UpgradeCategory.StatBoost ? 1 : 0;
            if (modifiers.arraySize > 0)
            {
                var m = modifiers.GetArrayElementAtIndex(0); m.FindPropertyRelative("stat").intValue = (int)stat;
                m.FindPropertyRelative("operation").intValue = (int)ModifierOperation.AdditivePercent; m.FindPropertyRelative("value").floatValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo(); return u;
        }
        private static StageSO Stage(string name, string scene, string id, EnemySO[] enemies, PoolKeySO hazard, AudioClip music, bool stress)
        {
            var s = Asset<StageSO>(name);
            Wire(s, "id", id, "displayName", name, "sceneName", scene, "durationSeconds", stress ? 65f : 300f, "music", music);
            var so = new SerializedObject(s); var waves = so.FindProperty("waves"); waves.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                var w = waves.GetArrayElementAtIndex(i); w.FindPropertyRelative("startTime").floatValue = stress ? 0 : i * 45f;
                w.FindPropertyRelative("endTime").floatValue = stress ? 65 : 300;
                w.FindPropertyRelative("spawnInterval").floatValue = stress ? 10000 : (i == 0 ? 0.12f : 0.6f + i * 0.4f);
                w.FindPropertyRelative("maxAlive").intValue = i == 0 ? 1200 : i == 1 ? 150 : 60;
                var choices = w.FindPropertyRelative("enemies"); choices.arraySize = 1;
                choices.GetArrayElementAtIndex(0).FindPropertyRelative("enemy").objectReferenceValue = enemies[i];
                choices.GetArrayElementAtIndex(0).FindPropertyRelative("weight").intValue = 1;
            }
            var hazards = so.FindProperty("hazards"); hazards.arraySize = stress ? 0 : 1;
            if (!stress)
            {
                var h = hazards.GetArrayElementAtIndex(0); h.FindPropertyRelative("hazardPool").objectReferenceValue = hazard;
                h.FindPropertyRelative("startTime").floatValue = 10; h.FindPropertyRelative("interval").floatValue = id == "assembly" ? 3 : 6;
                h.FindPropertyRelative("telegraphDuration").floatValue = 1.5f; h.FindPropertyRelative("activeDuration").floatValue = id == "assembly" ? 2 : 0.6f;
                h.FindPropertyRelative("damage").floatValue = 25;
            }
            so.ApplyModifiedPropertiesWithoutUndo(); return s;
        }
        private static void CreateEnvironment(bool assembly)
        {
            var floor = new GameObject(assembly ? "Linhas de Montagem" : "Patio de Triagem");
            Visual("Chao de aco", floor.transform, new Color(0.09f,0.12f,0.13f), new Vector2(64,44), -10);
            for (int x = -30; x <= 30; x += 4)
            {
                var line = Visual("Junta", floor.transform, new Color(0.13f,0.18f,0.18f), new Vector2(0.035f,40), -9);
                line.transform.position = new Vector3(x,0,0);
            }
            for (int y = -20; y <= 20; y += 4)
            {
                var line = Visual("Travessa", floor.transform, new Color(0.13f,0.18f,0.18f), new Vector2(60,0.035f), -9);
                line.transform.position = new Vector3(0,y,0);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int y = -20; y <= 20; y += 2)
                {
                    var stripe = Visual("Faixa de seguranca", floor.transform, new Color(0.55f,0.4f,0.13f), new Vector2(1,0.6f), -8);
                    stripe.transform.position = new Vector3(side*30,y,0); stripe.transform.rotation = Quaternion.Euler(0,0,35);
                }
            if (assembly)
                for (int x = -16; x <= 16; x += 16)
                {
                    var belt = Visual("Esteira", floor.transform, new Color(0.14f,0.15f,0.13f), new Vector2(3,40), -8);
                    belt.transform.position = new Vector3(x,0,0);
                }
        }
        private static AudioClip CreateTone(string name, float seconds, bool music)
        {
            string path = Root + "/Audio/" + name + ".wav";
            const int rate = 22050; int count = (int)(rate * seconds);
            using (var stream = File.Create(path)) using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2); writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate*2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(count*2);
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i/rate; float envelope = music ? Mathf.Exp(-(t % 0.5f)*12) : Mathf.Exp(-t*32);
                    float value = (Mathf.Sin(t*2*Mathf.PI*55) + 0.3f*Mathf.Sin(t*2*Mathf.PI*173)) * envelope * 0.18f;
                    writer.Write((short)(value * short.MaxValue));
                }
            }
            AssetDatabase.ImportAsset(path); return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        private static AudioMixer CreateMixer()
        {
            // Native Unity serialized asset. Parameter GUIDs are stable; mixer is local to Generated.
            string path = Root + "/Audio/Caldeira.mixer";
            var b = new StringBuilder("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            b.Append("--- !u!241 &24100000\nAudioMixerController:\n  m_ObjectHideFlags: 0\n  m_Name: Caldeira\n  m_OutputGroup: {fileID: 0}\n  m_MasterGroup: {fileID: 24300000}\n  m_Snapshots:\n  - {fileID: 24500000}\n  m_StartSnapshot: {fileID: 24500000}\n  m_SuspendThreshold: -80\n  m_EnableSuspend: 1\n  m_UpdateMode: 0\n  m_ExposedParameters:\n");
            string[] names = { "Master", "SFX", "BGM" }; string[] parameters = { "MasterVolume", "SfxVolume", "MusicVolume" };
            for (int i = 0; i < 3; i++) b.Append("  - guid: ").Append((i+1).ToString("x32")).Append("\n    name: ").Append(parameters[i]).Append('\n');
            b.Append("  m_AudioMixerGroupViews:\n  - guids:\n    - 0000000000000000000000000000000a\n    - 0000000000000000000000000000000b\n    - 0000000000000000000000000000000c\n    name: View\n  m_CurrentViewIndex: 0\n  m_TargetSnapshot: {fileID: 24500000}\n");
            for (int i = 0; i < 3; i++)
            {
                b.Append("--- !u!243 &").Append(24300000+i).Append("\nAudioMixerGroupController:\n  m_ObjectHideFlags: 0\n  m_Name: ").Append(names[i]);
                b.Append("\n  m_AudioMixer: {fileID: 24100000}\n  m_GroupID: ").Append((i+10).ToString("x32"));
                b.Append(i == 0 ? "\n  m_Children:\n  - {fileID: 24300001}\n  - {fileID: 24300002}\n" : "\n  m_Children: []\n");
                b.Append("  m_Volume: ").Append((i+1).ToString("x32")).Append("\n  m_Pitch: ").Append((i+20).ToString("x32"));
                b.Append("\n  m_Send: 00000000000000000000000000000000\n  m_Effects:\n  - {fileID: ").Append(24400000+i);
                b.Append("}\n  m_UserColorIndex: 0\n  m_Mute: 0\n  m_Solo: 0\n  m_BypassEffects: 0\n");
                b.Append("--- !u!244 &").Append(24400000+i).Append("\nAudioMixerEffectController:\n  m_ObjectHideFlags: 0\n  m_Name: Attenuation\n  m_EffectID: ").Append((i+30).ToString("x32"));
                b.Append("\n  m_EffectName: Attenuation\n  m_MixLevel: ").Append((i+40).ToString("x32")).Append("\n  m_Parameters: []\n  m_SendTarget: {fileID: 0}\n  m_EnableWetMix: 0\n  m_Bypass: 0\n");
            }
            b.Append("--- !u!245 &24500000\nAudioMixerSnapshotController:\n  m_ObjectHideFlags: 0\n  m_Name: Default\n  m_AudioMixer: {fileID: 24100000}\n  m_SnapshotID: 00000000000000000000000000000032\n  m_FloatValues:\n");
            for (int i = 0; i < 3; i++) b.Append("    ").Append((i+1).ToString("x32")).Append(": 0\n    ").Append((i+20).ToString("x32")).Append(": 1\n");
            b.Append("  m_TransitionOverrides: {}\n"); File.WriteAllText(path, b.ToString()); AssetDatabase.ImportAsset(path);
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer == null || mixer.FindMatchingGroups("SFX").Length != 1 || mixer.FindMatchingGroups("BGM").Length != 1)
                throw new System.InvalidOperationException("AudioMixer generation failed validation.");
            return mixer;
        }
    }
}
