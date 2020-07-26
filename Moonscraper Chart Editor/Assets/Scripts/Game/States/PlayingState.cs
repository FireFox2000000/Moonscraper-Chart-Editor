// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class PlayingState : SystemManagerState
{
    float? resetBackToTimeOnStop = null;
    float playFromTime = 0;

    bool audioStarted = false;
    List<SongObject> selectedBeforePlay = new List<SongObject>();

    public PlayingState(bool enableBot, float playFromTime, float? resetBackToTimeOnStop = null)
    {
        Debug.Log("Playing State Enter. Bot enabled = " + enableBot);

        ChartEditor editor = ChartEditor.Instance;

        this.playFromTime = playFromTime;
        this.resetBackToTimeOnStop = resetBackToTimeOnStop;

        AddSystem(new GameplayStateSystem(playFromTime, enableBot));
        AddSystem(new MetronomePlaybackSystem());

        if (enableBot)
        {
            AddSystem(new ClapPlaybackSystem(playFromTime));
        }
        else
        {
            AddSystem(new IndicatorPressedInputSystem(editor.indicators.animations, editor.currentGameMode));
        }
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

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepIncrease))
            Globals.gameSettings.snappingStep.Increment();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepDecrease))
            Globals.gameSettings.snappingStep.Decrement();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepIncreaseBy1))
            Globals.gameSettings.snappingStep.AdjustBy(1);

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepDecreaseBy1))
            Globals.gameSettings.snappingStep.AdjustBy(-1);

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.PlayPause) || MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StartGameplay))
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
