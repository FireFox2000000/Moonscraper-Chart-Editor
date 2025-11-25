using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class LyricEditor2EndSpacer : MonoBehaviour
{
    LayoutElement layout;
    [SerializeField]
    RectTransform viewport = null;

    public void Resize() {
        Rect viewportSize = viewport.rect;
        layout.preferredHeight = viewportSize.height;
    }

    void Start() {
        layout = GetComponent<LayoutElement>();
        Resize();
    }
}
