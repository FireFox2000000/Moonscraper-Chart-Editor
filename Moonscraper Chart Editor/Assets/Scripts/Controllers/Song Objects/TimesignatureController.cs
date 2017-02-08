using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignatureController : SongObjectController {

    public TimeSignature ts { get { return (TimeSignature)songObject; } set { Init(value, this); } }
    public Text tsText;
    public const float position = 1.5f;

    public override void UpdateSongObject()
    {
        if (ts.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, ts.worldYPosition, 0);

            tsText.text = ts.value.ToString() + "/4";
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (ts.position != 0 && Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            /*
            // Pass note data to a ghost note
            GameObject moveTS = Instantiate(editor.ghostTimeSignature);
            moveTS.SetActive(true);

            moveTS.name = "Moving Timesignature";
            Destroy(moveTS.GetComponent<PlaceTimesignature>());
            MoveTimeSignature movement = moveTS.AddComponent<MoveTimeSignature>();
            movement.Init(ts);*/
            editor.groupMove.SetSongObjects(ts);
            ts.Delete();
        }
    }
}
