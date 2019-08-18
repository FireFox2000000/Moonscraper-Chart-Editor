using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingState : SystemManagerState
{
    float? resetBackToTimeOnStop = null;
    float playFromTime = 0;

    bool audioStarted = false;
    List<SongObject> selectedBeforePlay = new List<SongObject>();

    public PlayingState(float playFromTime, float? resetBackToTimeOnStop = null)
    {
        this.playFromTime = playFromTime;
        this.resetBackToTimeOnStop = resetBackToTimeOnStop;
    }

    public override void Enter()
    {
        base.Enter();

        ChartEditor editor = ChartEditor.Instance;

        selectedBeforePlay.Clear();
        selectedBeforePlay.AddRange(editor.selectedObjectsManager.currentSelectedObjects);
        editor.selectedObjectsManager.currentSelectedObject = null;

        float playPoint = playFromTime;
        float audioPlayPoint = playPoint + editor.currentAudioOffset;

        editor.movement.SetTime(playPoint);

        if (audioPlayPoint >= 0)
        {
            editor.PlayAudio(audioPlayPoint);
            audioStarted = true;
        }
    }

    public override void Update()
    {
        base.Update();

        ChartEditor editor = ChartEditor.Instance;

        if (!audioStarted)
        {
            float audioPlayPoint = playFromTime + editor.currentAudioOffset;
            float currentTime = editor.currentAudioTime;
            if (currentTime >= audioPlayPoint && currentTime > 0)
            {
                editor.PlayAudio(currentTime);
                audioStarted = true;
            }
        }

        if (ShortcutInput.GetInputDown(Shortcut.PlayPause) || editor.inputManager.mainGamepad.GetButtonPressed(GamepadInput.Button.Start))
        {
            editor.Stop();
        }
    }

    public override void Exit()
    {
        base.Exit();

        ChartEditor editor = ChartEditor.Instance;

        editor.StopAudio();

        if (selectedBeforePlay.Count > 0)
        {
            // Check if the user switched view modes while playing
            if (Globals.viewMode == Globals.ViewMode.Chart)
            {
                if (selectedBeforePlay[0].GetType().IsSubclassOf(typeof(ChartObject)))
                    editor.selectedObjectsManager.currentSelectedObjects = selectedBeforePlay;
            }
            else
            {
                if (!selectedBeforePlay[0].GetType().IsSubclassOf(typeof(ChartObject)))
                    editor.selectedObjectsManager.currentSelectedObjects = selectedBeforePlay;
            }
        }

        selectedBeforePlay.Clear();

        if (resetBackToTimeOnStop.HasValue)
            editor.movement.SetTime(resetBackToTimeOnStop.Value);
    }
}
