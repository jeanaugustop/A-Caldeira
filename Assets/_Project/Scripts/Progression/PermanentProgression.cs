using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ACaldeira.Progression
{
    public sealed class PermanentProgression : MonoBehaviour
    {
        [Serializable] private sealed class SaveData
        {
            public int Version = 1;
            public int Credits;
            public int ArmorLevel;
            public int DamageLevel;
        }
        private SaveData data = new SaveData();
        private string path;
        public event Action Changed;
        public int Credits => data.Credits;
        public int ArmorLevel => data.ArmorLevel;
        public int DamageLevel => data.DamageLevel;
        public int ArmorCost => 20 * (1 + data.ArmorLevel);
        public int DamageCost => 20 * (1 + data.DamageLevel);
        private void Awake()
        {
            path = Path.Combine(Application.persistentDataPath, "progression-v1.json");
            try
            {
                if (File.Exists(path)) data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path)) ?? new SaveData();
                data.Credits = Mathf.Clamp(data.Credits, 0, 1000000);
                data.ArmorLevel = Mathf.Clamp(data.ArmorLevel, 0, 20);
                data.DamageLevel = Mathf.Clamp(data.DamageLevel, 0, 20);
            }
            catch (Exception e) when (e is IOException || e is JsonException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Progression save unavailable; defaults loaded."); data = new SaveData(); }
        }
        public void Award(int credits) { data.Credits = Mathf.Min(1000000, data.Credits + Mathf.Max(0, credits)); Save(); }
        public void BuyArmor()
        {
            if (data.ArmorLevel >= 20 || data.Credits < ArmorCost) return;
            data.Credits -= ArmorCost; data.ArmorLevel++; Save();
        }
        public void BuyDamage()
        {
            if (data.DamageLevel >= 20 || data.Credits < DamageCost) return;
            data.Credits -= DamageCost; data.DamageLevel++; Save();
        }
        private void Save()
        {
            try
            {
                string temp = path + ".tmp";
                File.WriteAllText(temp, JsonConvert.SerializeObject(data));
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Progression could not be saved."); }
            Changed?.Invoke();
        }
    }
}
