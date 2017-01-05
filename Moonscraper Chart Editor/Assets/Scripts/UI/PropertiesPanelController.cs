using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PropertiesPanelController : MonoBehaviour {
    public Text positionText;

    protected ChartEditor editor;

    void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }
}
