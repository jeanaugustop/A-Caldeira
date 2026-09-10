using UnityEngine;

namespace ACaldeira.UI
{
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        private Rect last;
        private int width, height;
        private void Update()
        {
            Rect area = Screen.safeArea;
            if (area == last && width == Screen.width && height == Screen.height) return;
            last = area; width = Screen.width; height = Screen.height;
            if (width <= 0 || height <= 0) return;
            content.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            content.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
            content.offsetMin = content.offsetMax = Vector2.zero;
        }
    }
}
