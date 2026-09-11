using System;
using ACaldeira.Combat;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Events;
using ACaldeira.Simulation;
using UnityEngine;

namespace ACaldeira.Progression
{
    public sealed class RunProgression : MonoBehaviour
    {
        private enum AttributeId { Damage, MoveSpeed, Pickup, Cadence, Evasion, Luck }
        private enum AccessoryId { Ring, Fuse, PanicValve, Plate, Coil, Siren, Cable }
        private static readonly string[] AttributeNames = { "Pressão Extra", "Servo Rápido", "Eletroímã", "Cadência de Disparo", "Esquiva", "Sorte" };
        private static readonly string[] AccessoryNames = { "Anel de Contingência", "Fusível Sacrificial", "Válvula de Pânico", "Placa de Amortecimento", "Bobina de Recolhimento", "Sirene de Contenção", "Cabo de Aterramento" };
        private static readonly float[] RarityValues = { 0.04f, 0.07f, 0.10f, 0.13f, 0.16f };
        private static readonly string[] RarityNames = { "Comum", "Incomum", "Rara", "Épica", "Lendária" };

        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponManager weapons;
        [SerializeField] private ExperienceEventChannelSO experienceChanged;
        [SerializeField, Range(1, 12)] private int maxEquipmentSlots = 8;
        private readonly RunOffer[] offers = new RunOffer[3];
        private readonly int[] attributeStacks = new int[6];
        private readonly int[] accessoryLevels = new int[7];
        private readonly float[] flat = new float[9];
        private readonly float[] additive = new float[9];
        private float cadence, evasion, luck;
        private float ringTimer, panicTimer, coilTimer, sirenTimer;
        private int ringCharges;
        private bool fuseUsed;

        public event Action ChoicesChanged;
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int Collected { get; private set; }
        public int Rerolls { get; private set; }
        public float CadenceMultiplier => 1f + cadence;
        public float Evasion => evasion;
        public int AccessoryCount { get { int n = 0; for (int i = 0; i < accessoryLevels.Length; i++) if (accessoryLevels[i] > 0) n++; return n; } }
        public int EquipmentCount => weapons.EquippedCount + AccessoryCount;
        public bool HasEquipmentSpace => EquipmentCount < maxEquipmentSlots;
        public float MaxHealthBonus => Mathf.Floor(Level / 10f) * 20f;
        public float MovementMultiplier => panicTimer > 0f ? 1.7f + 0.08f * Mathf.Max(0, accessoryLevels[(int)AccessoryId.PanicValve] - 1) : 1f;
        public int Required => Mathf.CeilToInt(18f + Level * 8f + Level * Level * 1.5f);

        public void Begin()
        {
            Array.Clear(attributeStacks, 0, attributeStacks.Length); Array.Clear(accessoryLevels, 0, accessoryLevels.Length);
            Array.Clear(flat, 0, flat.Length); Array.Clear(additive, 0, additive.Length); Array.Clear(offers, 0, offers.Length);
            cadence = evasion = luck = 0f; Level = 1; Experience = 0; Collected = 0; Rerolls = 3;
            ringTimer = 40f; panicTimer = 0f; coilTimer = 40f; sirenTimer = 12f; ringCharges = 0; fuseUsed = false;
            experienceChanged.Raise(0, Required);
        }
        public float Stat(StatId stat, float baseline)
        {
            int i = (int)stat;
            return Mathf.Max(0.01f, (baseline + flat[i]) * (1f + additive[i]));
        }
        public void Gain(int xp)
        {
            Experience += xp; Collected += xp; experienceChanged.Raise(Experience, Required); TryLevelUp();
        }
        private void TryLevelUp()
        {
            if (Experience < Required) return;
            Experience -= Required; Level++; RollOffers(); experienceChanged.Raise(Experience, Required);
            gameManager.EnterLevelUp(); ChoicesChanged?.Invoke();
        }
        private void RollOffers()
        {
            offers[0] = RollWeaponOffer(); offers[1] = RollAttributeOffer(-1); offers[2] = RollAccessoryOffer();
            if (offers[0] == null) offers[0] = RollAttributeOffer(offers[1].Id);
            if (offers[2] == null) offers[2] = RollAttributeOffer(offers[1].Id);
        }
        private RunOffer RollWeaponOffer()
        {
            if (!weapons.TryGetCandidate(HasEquipmentSpace, out WeaponSO weapon, out bool isNew)) return null;
            int next = isNew ? 1 : weapons.LevelOf(weapon) + 1;
            return new RunOffer(RunOfferKind.Weapon, weapon.DisplayName + "  Nv." + next,
                (isNew ? "Adicionar arma" : "Aprimorar arma") + " — " + WeaponLevelDescription(weapon, next), weapon.GetInstanceID(), isNew);
        }
        private RunOffer RollAccessoryOffer()
        {
            int selected = -1, candidates = 0;
            for (int i = 0; i < accessoryLevels.Length; i++)
            {
                bool eligible = accessoryLevels[i] > 0 ? accessoryLevels[i] < 6 : HasEquipmentSpace;
                if (eligible && UnityEngine.Random.Range(0, ++candidates) == 0) selected = i;
            }
            if (selected < 0) return null;
            bool isNew = accessoryLevels[selected] == 0; int next = isNew ? 1 : accessoryLevels[selected] + 1;
            return new RunOffer(RunOfferKind.Accessory, AccessoryNames[selected] + "  Nv." + next,
                (isNew ? "Adicionar acessório" : "Aprimorar acessório") + " — " + AccessoryLevelDescription((AccessoryId)selected, next), selected, isNew);
        }
        private RunOffer RollAttributeOffer(int excluded)
        {
            int selected = -1; float total = 0f;
            for (int i = 0; i < attributeStacks.Length; i++)
            {
                if (i == excluded) continue;
                float weight = 100f / (1f + 0.5f * attributeStacks[i]); total += weight;
                if (UnityEngine.Random.value * total < weight) selected = i;
            }
            if (selected < 0) selected = 0;
            int rarity = RollRarity(selected == (int)AttributeId.Luck); float value = RarityValues[rarity];
            return new RunOffer(RunOfferKind.Attribute, AttributeNames[selected] + " — " + RarityNames[rarity], AttributeDescription((AttributeId)selected, value), selected, false, value, rarity);
        }
        private int RollRarity(bool luckCard)
        {
            float[] weights = { Mathf.Max(0.001f, 0.60f - luck), 0.25f + luck * 0.8f / 1.5f, 0.10f + luck * 0.4f / 1.5f, 0.04f + luck * 0.2f / 1.5f, 0.01f + luck * 0.1f / 1.5f };
            int minimum = luckCard ? 1 : 0; float total = 0f;
            for (int i = minimum; i < weights.Length; i++) total += weights[i];
            float roll = UnityEngine.Random.value * total;
            for (int i = minimum; i < weights.Length; i++) { roll -= weights[i]; if (roll <= 0f) return i; }
            return 4;
        }
        public RunOffer Offer(int slot) => slot < 0 || slot >= offers.Length ? null : offers[slot];
        public void Choose(int slot)
        {
            if (gameManager.State != GameState.LevelUp || slot < 0 || slot >= offers.Length || offers[slot] == null) return;
            RunOffer offer = offers[slot];
            if (offer.Kind == RunOfferKind.Attribute) ApplyAttribute((AttributeId)offer.Id, offer.Value);
            else if (offer.Kind == RunOfferKind.Weapon) weapons.ApplyCandidate(offer.Id, offer.IsNewEquipment);
            else ApplyAccessory((AccessoryId)offer.Id, offer.IsNewEquipment);
            gameManager.Resume(); TryLevelUp();
        }
        public bool TryReroll()
        {
            if (gameManager.State != GameState.LevelUp || Rerolls <= 0) return false;
            Rerolls--; RollOffers(); ChoicesChanged?.Invoke(); return true;
        }
        public void TickEquipment(float dt, GameplaySimulation simulation)
        {
            panicTimer = Mathf.Max(0f, panicTimer - dt);
            int ring = accessoryLevels[(int)AccessoryId.Ring];
            if (ring > 0)
            {
                float recharge = Mathf.Max(12f, 40f - 5f * (ring - 1));
                ringTimer -= dt;
                int capacity = ring >= 6 ? 2 : 1;
                while (ringTimer <= 0f && ringCharges < capacity) { ringCharges++; ringTimer += recharge; }
            }
            int coil = accessoryLevels[(int)AccessoryId.Coil];
            if (coil > 0)
            {
                coilTimer -= dt;
                if (coilTimer <= 0f) { simulation.AttractExperience(12f + coil * 3f); coilTimer += Mathf.Max(18f, 40f - coil * 4f); }
            }
            int siren = accessoryLevels[(int)AccessoryId.Siren];
            if (siren > 0)
            {
                sirenTimer -= dt;
                if (sirenTimer <= 0f) { simulation.PushEnemies(3f + siren, 2f + siren); sirenTimer += Mathf.Max(5f, 14f - siren); }
            }
        }
        public bool TryBlockDamage(out bool triggerCable)
        {
            triggerCable = false;
            if (ringCharges <= 0) return false;
            ringCharges--; triggerCable = accessoryLevels[(int)AccessoryId.Cable] > 0; return true;
        }
        public float DamageMultiplier(float health, float maximum)
        {
            int plate = accessoryLevels[(int)AccessoryId.Plate];
            float reduction = plate * 0.04f;
            if (plate >= 5 && health <= maximum * 0.35f) reduction += 0.12f;
            if (accessoryLevels[(int)AccessoryId.Ring] >= 5 && ringCharges > 0) reduction += 0.05f;
            return Mathf.Clamp(1f - reduction, 0.2f, 1f);
        }
        public void OnDamaged() { if (accessoryLevels[(int)AccessoryId.PanicValve] > 0) panicTimer = 3f + 0.5f * (accessoryLevels[(int)AccessoryId.PanicValve] - 1); }
        public bool TryConsumeFuse(out float healthFraction)
        {
            healthFraction = 0.01f;
            int fuse = accessoryLevels[(int)AccessoryId.Fuse];
            if (fuse <= 0 || fuseUsed) return false;
            fuseUsed = true; healthFraction = fuse >= 4 ? 0.25f : 0.01f; return true;
        }
        private void ApplyAttribute(AttributeId id, float value)
        {
            attributeStacks[(int)id]++;
            if (id == AttributeId.Damage) additive[(int)StatId.Damage] += value;
            else if (id == AttributeId.MoveSpeed) additive[(int)StatId.MoveSpeed] += value;
            else if (id == AttributeId.Pickup) additive[(int)StatId.PickupRadius] += value;
            else if (id == AttributeId.Cadence) cadence += value;
            else if (id == AttributeId.Evasion) evasion = Mathf.Min(0.65f, evasion + value);
            else luck += value <= 0.07f ? 0.015f : value <= 0.10f ? 0.03f : value <= 0.13f ? 0.045f : 0.06f;
        }
        private void ApplyAccessory(AccessoryId id, bool isNew) => accessoryLevels[(int)id] = isNew ? 1 : Mathf.Min(6, accessoryLevels[(int)id] + 1);
        public bool TryDodge() => UnityEngine.Random.value < evasion;
        public int AccessoryLevel(int id) => id < 0 || id >= accessoryLevels.Length ? 0 : accessoryLevels[id];
        private static string AttributeDescription(AttributeId id, float value)
        {
            string p = (value * 100f).ToString("0") + "%";
            if (id == AttributeId.Damage) return "Dano global +" + p;
            if (id == AttributeId.MoveSpeed) return "Velocidade de movimento +" + p;
            if (id == AttributeId.Pickup) return "Raio de coleta +" + p;
            if (id == AttributeId.Cadence) return "Cadência de disparo +" + p;
            if (id == AttributeId.Evasion) return "Chance de Esquiva +" + p;
            return "Melhora a chance de raridades altas";
        }
        private static string WeaponLevelDescription(WeaponSO weapon, int level)
        {
            string id = weapon.Id;
            if (id == "Rebites de Pressao") return new[] { "Disparo automático básico", "+1 rebite por ataque", "Rebites atravessam mais inimigos", "Cadência própria maior", "Rebites maiores e mais fortes", "Rebite pesado de impacto" }[level - 1];
            if (id == "Oleo Cru") return new[] { "Leque de óleo curto", "Mais gotículas", "Óleo desacelera", "Jato mais longo", "Maior duração e perfuração", "Leque reforçado periódico" }[level - 1];
            if (id == "Serras Orbitais") return new[] { "Serras orbitam o exotraje", "+1 serra", "Órbita maior", "Rotação mais rápida", "Serras maiores e mais fortes", "Órbita oscilante" }[level - 1];
            return new[] { "Estaca pesada de longo alcance", "+1 estaca", "Mais perfuração", "Mais velocidade e alcance", "Mais dano e empurrão", "Linha de dano mais longa" }[level - 1];
        }
        private static string AccessoryLevelDescription(AccessoryId id, int level)
        {
            string[][] d = {
                new[] { "Bloqueia um dano a cada 40 s", "Recarga mais rápida", "Breve invulnerabilidade após bloquear", "Bloqueio empurra inimigos próximos", "Redução leve enquanto carregado", "Guarda duas cargas" },
                new[] { "Salva da morte uma vez", "Velocidade durante a invulnerabilidade", "Onda de empurrão na ativação", "Retorna com mais vida", "Primeiro dano após o retorno é bloqueado", "Impulso de fuga após retornar" },
                new[] { "Velocidade após receber dano", "Impulso dura mais", "Recarga menor", "Empurra inimigos ao ativar", "Atravessar desacelera inimigos", "Acelera enquanto ativo" },
                new[] { "Redução fixa de dano", "Mais redução", "Resistência temporária após dano", "Empurra quem acerta", "Mais resistência com pouca vida", "Blindagem modular" },
                new[] { "Puxa XP a cada 40 s", "Pulsa mais rápido", "Maior alcance", "Coleta aumenta o ímã", "Pulso duplo", "Impulso após coletar" },
                new[] { "Empurra e desacelera inimigos", "Pulsa mais rápido", "Raio maior", "Desaceleração dura mais", "Inimigos causam menos dano", "Pulso duplo" },
                new[] { "Bloqueio gera pulso", "Pulso maior", "Pulso causa dano", "Esquiva também ativa", "Descarga salta", "Campo desacelerador" } };
            return d[(int)id][level - 1];
        }
    }
}
