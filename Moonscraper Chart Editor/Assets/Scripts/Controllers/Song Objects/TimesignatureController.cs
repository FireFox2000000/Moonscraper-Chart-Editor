using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignatureController : SongObjectController {

    public TimeSignature ts;
    public Text tsText;
    public float position = 0.0f;

    public void Init(TimeSignature _ts)
    {
        base.Init(_ts);
        ts = _ts;
        ts.controller = this;
    }

    public override void UpdateSongObject()
    {
        if (ts.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, ts.worldYPosition, 0);

            tsText.text = ts.value.ToString() + "/4";
        }
    }

    public override void Delete()
    {
        if (ts.position != 0)
        {
            ts.song.Remove(ts);

            Destroy(gameObject);
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (ts.position != 0 && Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Pass note data to a ghost note
            GameObject moveTS = Instantiate(editor.ghostTimeSignature);
            moveTS.SetActive(true);

            moveTS.name = "Moving Timesignature";
            Destroy(moveTS.GetComponent<PlaceTimesignature>());
            MoveTimeSignature movement = moveTS.AddComponent<MoveTimeSignature>();
            movement.Init(ts);

            Delete();
        }
    }
}
