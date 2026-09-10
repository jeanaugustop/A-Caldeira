using ACaldeira.UI;
using UnityEngine;

namespace ACaldeira.Core
{
    [CreateAssetMenu(menuName = "A Caldeira/Core/Runtime Services", fileName = "RuntimeServices")]
    public sealed class RuntimeServicesSO : ScriptableObject
    {
        public GameManager GameManager { get; private set; }
        public SettingsManager SettingsManager { get; private set; }

        public bool TryBind(GameManager gameManager, SettingsManager settingsManager)
        {
            if (GameManager != null && GameManager != gameManager) return false;
            GameManager = gameManager;
            SettingsManager = settingsManager;
            return true;
        }

        public void Clear(GameManager owner)
        {
            if (GameManager != owner)
            {
                return;
            }

            GameManager = null;
            SettingsManager = null;
        }
    }
}
