using ACaldeira.Progression;
using TMPro;
using UnityEngine;

namespace ACaldeira.UI
{
    public sealed class WorkshopPanel : MonoBehaviour
    {
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private TMP_Text costs;
        private void OnEnable() { permanent.Changed += Refresh; Refresh(); }
        private void OnDisable() { permanent.Changed -= Refresh; }
        private void Refresh() { costs.SetText("Custo blindagem: {0} | Custo potencia: {1}\nLimite: 20 niveis por melhoria", permanent.ArmorCost, permanent.DamageCost); }
    }
}
