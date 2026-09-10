using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ACaldeira.UI
{
    public sealed class MenuFocus : MonoBehaviour
    {
        [SerializeField] private Selectable first;
        private void OnEnable() { Focus(); }
        private void Start() { Focus(); }
        private void Focus()
        {
            if (first != null && first.IsActive() && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }
    }
}
