using UnityEngine;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(DrumRollController))]
public class PlaceDrumRoll : PlaceSongObject
{
    public DrumRoll drumRoll { get { return (DrumRoll)songObject; } set { songObject = value; } }
    new public DrumRollController controller { get { return (DrumRollController)base.controller; } set { base.controller = value; } }
    DrumRoll m_lastPlaceDrumRoll = null;

    protected override void Update()
    {
        base.Update();
        if ((Input.GetMouseButtonUp(0) && !Globals.gameSettings.keysModeEnabled) || (Globals.gameSettings.keysModeEnabled && Input.GetButtonUp("Add Object")))
        {
            // Reset
            m_lastPlaceDrumRoll = null;
        }
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new DrumRoll(drumRoll)));

        int insertionIndex = SongObjectHelper.FindObjectPosition(drumRoll, editor.currentChart.drumRoll);
        Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
        editor.selectedObjectsManager.SelectSongObject(drumRoll, editor.currentChart.drumRoll);

        m_lastPlaceDrumRoll = editor.currentChart.drumRoll[insertionIndex];
    }

    protected override void SetSongObjectAndController()
    {
        drumRoll = new DrumRoll(0, 256);

        controller = GetComponent<DrumRollController>();
        controller.drumRoll = drumRoll;
    }

    protected override void Controls()
    {
        if (!Globals.gameSettings.keysModeEnabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (m_lastPlaceDrumRoll == null)
                {
                    AddObject();
                }
                else
                {
                    //UpdateLastPlacedSp();
                }
            }
        }
        else if (MSChartEditorInput.GetInput(MSChartEditorInputActions.AddSongObject))
        {
            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.AddSongObject))
            {
                var searchArray = editor.currentChart.drumRoll;
                int pos = SongObjectHelper.FindObjectPosition(drumRoll, searchArray);
                if (pos == SongObjectHelper.NOTFOUND)
                {
                    AddObject();
                }
                else
                {
                    editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
                }
            }
            else if (m_lastPlaceDrumRoll != null)
            {
                //UpdateLastPlacedSp();
            }
        }
    }
}
