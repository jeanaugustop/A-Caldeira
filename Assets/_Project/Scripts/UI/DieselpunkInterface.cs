using ACaldeira.Combat;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ACaldeira.UI
{
    public sealed class DieselpunkInterface : MonoBehaviour
    {
        public DieselpunkArt Art;
        public RunProgression Progression;
        public WeaponManager Weapons;
        public GameplaySimulation Simulation;
        public GameManager Game;
        public Image[] ChoiceIcons;
        public Image[] ChoiceFrames;
        public TMP_Text[] ChoiceKinds;
        public Image[] EquipmentIcons;
        public TMP_Text[] EquipmentLabels;
        public Button Reroll;
        public TMP_Text RerollLabel;
        public TMP_Text SectorLabel;
        public Transform Player;
        private static readonly Color[] Rarities = {
            new Color(.65f,.70f,.68f), new Color(.37f,.78f,.51f), new Color(.32f,.62f,.88f),
            new Color(.69f,.43f,.87f), new Color(1f,.69f,.23f)
        };

        private void Start()
        {
            Progression.ChoicesChanged += RefreshChoices;
            Simulation.HudChanged += RefreshHud;
            Reroll.onClick.AddListener(RerollOffers);
            RefreshChoices();
        }
        private void OnDestroy()
        {
            if (Progression != null) Progression.ChoicesChanged -= RefreshChoices;
            if (Simulation != null) Simulation.HudChanged -= RefreshHud;
        }
        private void RerollOffers() { Progression.TryReroll(); }
        private void RefreshChoices()
        {
            for (int i = 0; i < ChoiceIcons.Length; i++)
            {
                var offer = Progression.Offer(i);
                if (offer == null) continue;
                Color accent = new Color(.92f,.59f,.24f);
                Sprite icon;
                if (offer.Kind == RunOfferKind.Weapon)
                {
                    WeaponSO weapon = Weapons.KnownWeapon(offer.Id);
                    icon = weapon != null ? Art.Weapon(weapon.Id) : Art.Weapons[0];
                    ChoiceKinds[i].text = offer.IsNewEquipment ? "ARMA / NOVO EQUIPAMENTO" : "ARMA / MELHORIA";
                }
                else if (offer.Kind == RunOfferKind.Accessory)
                {
                    icon = Art.Accessories[Mathf.Clamp(offer.Id, 0, 6)];
                    accent = new Color(.34f,.76f,.71f);
                    ChoiceKinds[i].text = offer.IsNewEquipment ? "ACESSÓRIO / NOVO EQUIPAMENTO" : "ACESSÓRIO / MELHORIA";
                }
                else
                {
                    int[] icons = { 3, 2, 4, 5, 0, 1 };
                    icon = Art.Accessories[icons[Mathf.Clamp(offer.Id, 0, 5)]];
                    accent = Rarities[Mathf.Clamp(offer.Rarity, 0, 4)];
                    ChoiceKinds[i].text = "ATRIBUTO / BÔNUS PERMANENTE NA RUN";
                }
                ChoiceIcons[i].sprite = icon;
                ChoiceFrames[i].color = accent;
                ChoiceKinds[i].color = accent;
            }
            RerollLabel.text = "RERROLAR [R]  /  " + Progression.Rerolls;
            Reroll.interactable = Progression.Rerolls > 0;
        }
        private void RefreshHud(float health, float maximum, float elapsed, int alive)
        {
            int slot = 0;
            for (int i = 0; i < 8; i++)
            {
                var weapon = Weapons.EquippedAt(i);
                if (weapon == null) continue;
                ShowEquipment(slot++, Art.Weapon(weapon.Id), Weapons.LevelOf(weapon));
            }
            for (int i = 0; i < 7 && slot < 8; i++)
            {
                int level = Progression.AccessoryLevel(i);
                if (level > 0) ShowEquipment(slot++, Art.Accessories[i], level);
            }
            for (; slot < EquipmentIcons.Length; slot++)
            {
                EquipmentIcons[slot].enabled = false;
                EquipmentLabels[slot].text = "—";
            }
            Vector2 position = Player.position;
            string sector = Mathf.Abs(position.x) < 20 && Mathf.Abs(position.y) < 16 ? "PRAÇA DE TRIAGEM" :
                position.x < -25 ? "DEPÓSITO DE COMBUSTÍVEL" : position.x > 25 ? "CASA DAS MÁQUINAS" :
                position.y > 15 ? "PÁTIO DAS CALDEIRAS" : "DOCAS DE SUCATA";
            SectorLabel.text = "SETOR 07  /  " + sector;
        }
        private void ShowEquipment(int slot, Sprite icon, int level)
        {
            if (slot >= EquipmentIcons.Length) return;
            EquipmentIcons[slot].enabled = true;
            EquipmentIcons[slot].sprite = icon;
            EquipmentLabels[slot].text = "NV. " + level;
        }
    }
}
