using UnityEngine;
using System.Collections;

public class DisplayMenu : MonoBehaviour {
    protected ChartEditor editor;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected virtual void OnEnable()
    {
        editor.Stop();
        Globals.applicationMode = Globals.ApplicationMode.Menu;
    }

    protected virtual void OnDisable()
    {
        Globals.applicationMode = Globals.ApplicationMode.Editor;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

}
