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

        AddSystem(new MetronomePlaybackSystem());
        AddSystem(new ClapPlaybackSystem(playFromTime));
    }

    public override void Enter()
    {
        ChartEditor editor = ChartEditor.Instance;

        selectedBeforePlay.Clear();
        selectedBeforePlay.AddRange(editor.selectedObjectsManager.currentSelectedObjects);
        editor.selectedObjectsManager.currentSelectedObject = null;

        float playPoint = playFromTime;
        float audioPlayPoint = playPoint + editor.services.totalSongAudioOffset;

        editor.movement.SetTime(playPoint);

        if (audioPlayPoint >= 0)
        {
            editor.PlayAudio(audioPlayPoint);
            audioStarted = true;
        }

        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        ChartEditor editor = ChartEditor.Instance;

        if (!audioStarted)
        {
            float audioPlayPoint = playFromTime + editor.services.totalSongAudioOffset;
            float currentTime = editor.services.currentAudioTime;
            if (currentTime >= audioPlayPoint && currentTime > 0)
            {
                editor.PlayAudio(currentTime);
                audioStarted = true;
            }
        }

        if (ShortcutInput.GetInputDown(Shortcut.StepIncrease))
            GameSettings.snappingStep.Increment();

        else if (ShortcutInput.GetInputDown(Shortcut.StepDecrease))
            GameSettings.snappingStep.Decrement();

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
