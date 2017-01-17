using UnityEngine;
using System.Collections;

public class StarpowerTailController : SelectableClick {
    public StarpowerController spCon;
    public ChartEditor editor;

    void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    public override void OnSelectableMouseDown()
    {
        if (Input.GetMouseButton(1))
            OnSelectableMouseDrag();
    }

    public override void OnSelectableMouseDrag()
    {
        // Update sustain
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            if (spCon.unmodifiedSP == null)
                spCon.unmodifiedSP = (StarPower)spCon.starpower.Clone();

            spCon.TailDrag();
        }
    }

    public override void OnSelectableMouseUp()
    {
        if (spCon.unmodifiedSP != null)
            editor.actionHistory.Insert(new ActionHistory.Modify(spCon.unmodifiedSP, spCon.starpower));

        spCon.unmodifiedSP = null;
    }
}
