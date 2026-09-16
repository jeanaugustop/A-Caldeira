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
        private float ringTimer, panicTimer, coilTimer, sirenTimer, cableTimer;
        private float sirenSecondPulseTimer;
        private float fuseEmergencyTimer, fuseEscapeTimer;
        private int activatedFuseLevel;
        private int ringCharges;
        private bool fuseUsed, fuseShieldReady;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private int debugAccessoryIndex = (int)AccessoryId.Siren;
#endif

        public event Action ChoicesChanged;
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int Collected { get; private set; }
        public int Rerolls { get; private set; }
        public float CadenceMultiplier => 1f + cadence;
        public float Evasion => evasion;
        public int RingCharges => ringCharges;
        public float RingBlockInvulnerability => accessoryLevels[(int)AccessoryId.Ring] >= 3 ? 1.25f : 0.25f;
        public bool RingPushes => accessoryLevels[(int)AccessoryId.Ring] >= 4;
        public int AccessoryCount { get { int n = 0; for (int i = 0; i < accessoryLevels.Length; i++) if (accessoryLevels[i] > 0) n++; return n; } }
        public int EquipmentCount => weapons.EquippedCount + AccessoryCount;
        public bool HasEquipmentSpace => EquipmentCount < maxEquipmentSlots;
        public float MaxHealthBonus => Mathf.Floor(Level / 10f) * 20f;
        public float MovementMultiplier
        {
            get
            {
                float multiplier = panicTimer > 0f ? 1.7f + 0.08f * Mathf.Max(0, accessoryLevels[(int)AccessoryId.PanicValve] - 1) : 1f;
                if (fuseEmergencyTimer > 0f && activatedFuseLevel >= 2) multiplier = Mathf.Max(multiplier, 1.6f);
                if (fuseEscapeTimer > 0f) multiplier = Mathf.Max(multiplier, 1.35f);
                return multiplier;
            }
        }
        public bool FuseShieldReady => fuseShieldReady;
        public int Required => Mathf.CeilToInt(18f + Level * 8f + Level * Level * 1.5f);

        public void Begin()
        {
            Array.Clear(attributeStacks, 0, attributeStacks.Length); Array.Clear(accessoryLevels, 0, accessoryLevels.Length);
            Array.Clear(flat, 0, flat.Length); Array.Clear(additive, 0, additive.Length); Array.Clear(offers, 0, offers.Length);
            cadence = evasion = luck = 0f; Level = 1; Experience = 0; Collected = 0; Rerolls = 3;
            ringTimer = 40f; panicTimer = 0f; coilTimer = 40f; sirenTimer = 14f; sirenSecondPulseTimer = -1f; cableTimer = 0f;
            fuseEmergencyTimer = fuseEscapeTimer = 0f; activatedFuseLevel = 0; ringCharges = 0; fuseUsed = false; fuseShieldReady = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugAccessoryIndex = (int)AccessoryId.Siren;
#endif
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
                if (i == (int)AccessoryId.Fuse && fuseUsed) continue;
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
            cableTimer = Mathf.Max(0f, cableTimer - dt);
            if (fuseEmergencyTimer > 0f)
            {
                fuseEmergencyTimer -= dt;
                if (fuseEmergencyTimer <= 0f)
                {
                    if (activatedFuseLevel >= 5) fuseShieldReady = true;
                    if (activatedFuseLevel >= 6) fuseEscapeTimer = 4f;
                }
            }
            fuseEscapeTimer = Mathf.Max(0f, fuseEscapeTimer - dt);
            int ring = accessoryLevels[(int)AccessoryId.Ring];
            if (ring > 0)
            {
                float recharge = ring >= 2 ? 32f : 40f;
                int capacity = ring >= 6 ? 2 : 1;
                if (ringCharges < capacity)
                {
                    ringTimer -= dt;
                    if (ringTimer <= 0f) { ringCharges++; ringTimer = recharge; }
                }
                else ringTimer = recharge;
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
                if (sirenSecondPulseTimer >= 0f)
                {
                    sirenSecondPulseTimer -= dt;
                    if (sirenSecondPulseTimer <= 0f)
                    {
                        EmitSirenPulse(simulation, siren);
                        sirenSecondPulseTimer = -1f;
                    }
                }
                sirenTimer -= dt;
                if (sirenTimer <= 0f)
                {
                    EmitSirenPulse(simulation, siren);
                    float interval = siren >= 2 ? 11f : 14f;
                    if (siren >= 6) sirenSecondPulseTimer = interval * 0.5f;
                    sirenTimer += interval;
                }
            }
        }
        private static void EmitSirenPulse(GameplaySimulation simulation, int level)
        {
            float radius = level >= 3 ? 6f : 4f;
            float duration = level >= 4 ? 3f : 1.5f;
            float damageReduction = level >= 5 ? 0.2f : 0f;
            simulation.PulseSiren(radius, 3f, 0.2f, duration, damageReduction);
        }
        public bool TryBlockDamage(out bool triggerCable)
        {
            triggerCable = false;
            if (ringCharges <= 0) return false;
            ringCharges--; triggerCable = TryTriggerCable(4); return true;
        }
        public float DamageMultiplier(float health, float maximum)
        {
            int plate = accessoryLevels[(int)AccessoryId.Plate];
            float reduction = plate * 0.04f;
            if (plate >= 5 && health <= maximum * 0.35f) reduction += 0.12f;
            if (accessoryLevels[(int)AccessoryId.Ring] >= 5 && ringCharges <= 0) reduction += 0.10f;
            return Mathf.Clamp(1f - reduction, 0.2f, 1f);
        }
        public bool OnDamaged()
        {
            if (accessoryLevels[(int)AccessoryId.PanicValve] > 0) panicTimer = 3f + 0.5f * (accessoryLevels[(int)AccessoryId.PanicValve] - 1);
            return TryTriggerCable(1);
        }
        public bool TryConsumeFuse(out float healthFraction, out int fuseLevel)
        {
            healthFraction = 0.01f;
            fuseLevel = accessoryLevels[(int)AccessoryId.Fuse];
            if (fuseLevel <= 0 || fuseUsed) return false;
            fuseUsed = true;
            activatedFuseLevel = fuseLevel;
            fuseEmergencyTimer = 5f;
            fuseEscapeTimer = 0f;
            fuseShieldReady = false;
            accessoryLevels[(int)AccessoryId.Fuse] = 0;
            healthFraction = fuseLevel >= 4 ? 0.25f : 0.01f;
            return true;
        }
        public bool TryConsumeFuseShield()
        {
            if (!fuseShieldReady) return false;
            fuseShieldReady = false;
            return true;
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
        private void ApplyAccessory(AccessoryId id, bool isNew)
        {
            accessoryLevels[(int)id] = isNew ? 1 : Mathf.Min(6, accessoryLevels[(int)id] + 1);
            if (id == AccessoryId.Ring && accessoryLevels[(int)id] >= 2) ringTimer = Mathf.Min(ringTimer, 32f);
        }
        public bool TryDodge(out bool triggerCable)
        {
            bool dodged = UnityEngine.Random.value < evasion;
            triggerCable = dodged && TryTriggerCable(4);
            return dodged;
        }
        private bool TryTriggerCable(int requiredLevel)
        {
            if (accessoryLevels[(int)AccessoryId.Cable] < requiredLevel || cableTimer > 0f) return false;
            cableTimer = 8f;
            return true;
        }
        public int AccessoryLevel(int id) => id < 0 || id >= accessoryLevels.Length ? 0 : accessoryLevels[id];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public string DebugSelectNextAccessory()
        {
            debugAccessoryIndex = (debugAccessoryIndex + 1) % AccessoryNames.Length;
            return AccessoryNames[debugAccessoryIndex];
        }
        public int DebugAdvanceSelectedAccessory(out string accessoryName)
        {
            accessoryName = AccessoryNames[debugAccessoryIndex];
            if (accessoryLevels[debugAccessoryIndex] <= 0 && !HasEquipmentSpace) return -1;
            accessoryLevels[debugAccessoryIndex] = Mathf.Min(6, accessoryLevels[debugAccessoryIndex] + 1);
            if (debugAccessoryIndex == (int)AccessoryId.Fuse) fuseUsed = false;
            if (debugAccessoryIndex == (int)AccessoryId.Ring)
            {
                ringCharges = accessoryLevels[debugAccessoryIndex] >= 6 ? 2 : 1;
                ringTimer = accessoryLevels[debugAccessoryIndex] >= 2 ? 32f : 40f;
            }
            if (debugAccessoryIndex == (int)AccessoryId.Siren) sirenTimer = Mathf.Min(sirenTimer, 0.25f);
            if (debugAccessoryIndex == (int)AccessoryId.Cable) cableTimer = 0f;
            return accessoryLevels[debugAccessoryIndex];
        }
#endif
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
            if (id == "Rebites de Pressao") return new[] { "Disparo automático básico", "+1 rebite alinhado por ataque", "Rebites atravessam mais inimigos", "Cadência própria +25%", "Rebites maiores com impacto em área", "A cada 5 disparos, lança um Pistão de Impacto" }[level - 1];
            if (id == "Oleo Cru") return new[] { "Cospe uma bolota a cada 3 s; cai a 5 m e deixa uma poça", "Lança duas bolotas por ativação", "Poças desaceleram os inimigos em 18%", "Bolotas caem a 6,5 m", "Poças maiores e com duração de 4,25 s", "A cada quatro ativações, cria poças reforçadas" }[level - 1];
            if (id == "Serras Orbitais") return new[] { "Uma serra orbita o exotraje", "Adiciona a segunda serra", "Adiciona a terceira serra", "Órbita maior", "Rotação mais rápida", "Adiciona a quarta serra; todas ficam maiores e mais fortes" }[level - 1];
            return new[] { "Estaca pesada de longo alcance", "Adiciona uma estaca paralela", "Atravessa mais inimigos", "Velocidade e alcance +30%", "Estacas maiores, mais fortes e com empurrão", "Deixa um trilho de pressão durante o percurso" }[level - 1];
        }
        private static string AccessoryLevelDescription(AccessoryId id, int level)
        {
            string[][] d = {
                new[] { "Bloqueia um dano a cada 40 s", "Recarga reduzida para 32 s", "1,25 s de invulnerabilidade após bloquear", "Bloqueio empurra inimigos em 4 m", "10% menos dano enquanto recarrega", "Guarda duas cargas" },
                new[] { "Salva da morte uma vez; 5 s invulnerável", "+60% de movimento durante a invulnerabilidade", "Onda de empurrão em 6 m", "Retorna com 25% da vida máxima", "Bloqueia o primeiro dano após a invulnerabilidade", "+35% de movimento por mais 4 s" },
                new[] { "Velocidade após receber dano", "Impulso dura mais", "Recarga menor", "Empurra inimigos ao ativar", "Atravessar desacelera inimigos", "Acelera enquanto ativo" },
                new[] { "Redução fixa de dano", "Mais redução", "Resistência temporária após dano", "Empurra quem acerta", "Mais resistência com pouca vida", "Blindagem modular" },
                new[] { "Puxa XP a cada 40 s", "Pulsa mais rápido", "Maior alcance", "Coleta aumenta o ímã", "Pulso duplo", "Impulso após coletar" },
                new[] { "Empurra e desacelera inimigos", "Pulsa mais rápido", "Raio maior", "Desaceleração dura mais", "Inimigos causam menos dano", "Pulso adicional na metade do intervalo" },
                new[] { "Dano recebido gera pulso; recarga de 8 s", "Pulso maior", "Pulso causa dano", "Bloqueios e esquivas também ativam", "Descarga salta para até 3 inimigos", "Campo desacelerador por 3 s" } };
            return d[(int)id][level - 1];
        }
    }
}
