using System;
using ACaldeira.Core;
using ACaldeira.Combat;
using ACaldeira.Data;
using ACaldeira.Events;
using UnityEngine;

namespace ACaldeira.Progression
{
    public sealed class RunProgression : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponManager weapons;
        [SerializeField] private UpgradeSO[] catalog;
        [SerializeField] private ExperienceEventChannelSO experienceChanged;
        private int[] stacks;
        private readonly int[] offers = new int[3];
        private readonly float[] flat = new float[9];
        private readonly float[] additive = new float[9];
        private readonly float[] multiplicative = new float[9];
        public event Action ChoicesChanged;
        public int Level { get; private set; }
        public int Experience { get; private set; }
        // Keeps the first upgrades attainable while preventing late-run level-up spam.
        public int Required => Mathf.CeilToInt(18f + Level * 8f + Level * Level * 1.5f);
        public int Collected { get; private set; }
        private void Awake() { stacks = new int[catalog.Length]; }
        public void Begin()
        {
            Array.Clear(stacks, 0, stacks.Length);
            Array.Clear(flat, 0, flat.Length); Array.Clear(additive, 0, additive.Length);
            for (int i = 0; i < multiplicative.Length; i++) multiplicative[i] = 1f;
            Level = 1; Experience = 0; Collected = 0;
            experienceChanged.Raise(0, Required);
        }
        public float Stat(StatId stat, float baseline)
        {
            int i = (int)stat;
            return Mathf.Max(0.01f, (baseline + flat[i]) * (1f + additive[i]) * multiplicative[i]);
        }
        public void Gain(int xp)
        {
            Experience += xp; Collected += xp;
            experienceChanged.Raise(Experience, Required);
            TryLevelUp();
        }
        private void TryLevelUp()
        {
            if (Experience < Required) return;
            Experience -= Required; Level++;
            int count = 0;
            for (int slot = 0; slot < offers.Length; slot++)
            {
                offers[slot] = -1;
                int eligible = 0;
                for (int i = 0; i < catalog.Length; i++)
                {
                    bool duplicate = false;
                    for (int j = 0; j < slot; j++) if (offers[j] == i) duplicate = true;
                    if (duplicate || !Eligible(i)) continue;
                    if (UnityEngine.Random.Range(0, ++eligible) == 0) offers[slot] = i;
                }
                if (offers[slot] >= 0) count++;
            }
            experienceChanged.Raise(Experience, Required);
            if (count == 0) return;
            gameManager.EnterLevelUp(); ChoicesChanged?.Invoke();
        }
        private bool Eligible(int i)
        {
            var u = catalog[i];
            if (u == null || stacks[i] >= u.MaxStacks) return false;
            if (u.Category == UpgradeCategory.WeaponUnlock && (!weapons.HasSpace || weapons.Has(u.GrantedWeapon))) return false;
            if (u.Category == UpgradeCategory.WeaponEvolution && !weapons.Has(u.GrantedWeapon)) return false;
            var prerequisites = u.Prerequisites;
            if (prerequisites != null)
                for (int p = 0; p < prerequisites.Length; p++)
                {
                    bool found = false;
                    for (int j = 0; j < catalog.Length; j++) if (catalog[j] == prerequisites[p] && stacks[j] > 0) found = true;
                    if (!found) return false;
                }
            return true;
        }
        public UpgradeSO Offer(int slot) => offers[slot] < 0 ? null : catalog[offers[slot]];
        public void Choose(int slot)
        {
            if (gameManager.State != GameState.LevelUp || slot < 0 || slot >= offers.Length || offers[slot] < 0) return;
            int index = offers[slot]; var u = catalog[index]; stacks[index]++;
            if (u.Category == UpgradeCategory.WeaponUnlock) weapons.TryEquip(u.GrantedWeapon);
            if (u.Category == UpgradeCategory.WeaponEvolution) weapons.Evolve(u.GrantedWeapon, u.EvolvedWeapon);
            if (u.Modifiers != null)
                for (int i = 0; i < u.Modifiers.Length; i++)
                {
                    var m = u.Modifiers[i]; int s = (int)m.Stat;
                    if (m.Operation == ModifierOperation.Flat) flat[s] += m.Value;
                    else if (m.Operation == ModifierOperation.AdditivePercent) additive[s] += m.Value;
                    else multiplicative[s] *= 1f + m.Value;
                }
            gameManager.Resume(); TryLevelUp();
        }
    }
}
