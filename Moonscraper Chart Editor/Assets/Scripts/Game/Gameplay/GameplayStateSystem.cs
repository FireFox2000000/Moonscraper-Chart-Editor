using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimingConfig;

public class GameplayStateSystem : SystemManagerState.System
{
    // Configurable properties
    bool botEnabled = true;

    OneShotSampleStream missSoundSample;
    HitWindowFeeder hitWindowFeeder; 

    delegate void GameplayUpdateFn(float time);
    GameplayUpdateFn gameplayUpdateFn = null;
    BaseGameplayRulestate currentRulestate;

    public struct GameState
    {
        public BaseGameplayRulestate.NoteStats stats;
    }

    enum GameplayType
    {
        Bot,
        Guitar,
        Drums,

        None,
    }

    public GameplayStateSystem(HitWindowFeeder hitWindowFeeder, bool botEnabled)
    {
        this.hitWindowFeeder = hitWindowFeeder;
        this.botEnabled = botEnabled;
    }

    public override void SystemEnter()
    {
        ChartEditor editor = ChartEditor.Instance;

        GameplayType gameplayType = DetermineGameplayType(botEnabled, editor.currentGameMode);
        LoadSoundClip();

        DetermineUpdateRulestate(gameplayType, out gameplayUpdateFn, out currentRulestate);

        hitWindowFeeder.hitWindow = CreateHitWindow(gameplayType);
        hitWindowFeeder.enabled = true;

        if (botEnabled)
        {
            // We want the bot to automatically hit any sustains that are currently active in the view, but for which the notes are already past the strikeline
            Song song = editor.currentSong;
            float currentTime = editor.currentVisibleTime;
            uint currentTick = song.TimeToTick(currentTime, song.resolution);
            int index = SongObjectHelper.FindClosestPositionRoundedDown(currentTick, editor.currentChart.notes);
            if (index != SongObjectHelper.NOTFOUND)
            {
                Note note = editor.currentChart.notes[index];
                List<Note> sustainNotes = new List<Note>();
                NoteFunctions.GetPreviousOfSustains(sustainNotes, note, GameSettings.extendedSustainsEnabled);

                foreach (Note chordNote in note.chord)
                {
                    sustainNotes.Add(chordNote);
                }
                foreach (Note sustainNote in sustainNotes)
                {
                    if (sustainNote.controller != null)
                    {
                        hitWindowFeeder.TryAddNote(sustainNote.controller);
                    }
                }
            }
        }
    }

    public override void SystemUpdate()
    {
        float currentTime = ChartEditor.Instance.currentVisibleTime;
        gameplayUpdateFn?.Invoke(currentTime);

        GameState gamestate = new GameState();
        gamestate.stats = currentRulestate.stats;

        ChartEditor.Instance.gameplayEvents.gameplayUpdateEvent.Fire(gamestate);
    }

    public override void SystemExit()
    {
        missSoundSample = null;
        hitWindowFeeder.enabled = false;
    }

    void LoadSoundClip()
    {
        missSoundSample = ChartEditor.Instance.sfxAudioStreams.GetSample(SkinKeys.break0);
        Debug.Assert(missSoundSample != null);
        missSoundSample.onlyPlayIfStopped = true;
    }

    void KickMissFeedback()
    {
        if (missSoundSample.Play())      // If we try to play this again before the sample has ended we'll get rejected. Should also reject the whole event.
        {
            ChartEditor.Instance.gameplayEvents.explicitMissEvent.Fire();
        }
    }

    void DetermineUpdateRulestate(GameplayType gameplayType, out GameplayUpdateFn gameplayUpdateFn, out BaseGameplayRulestate currentRulestate)
    {
        gameplayUpdateFn = null;
        currentRulestate = null;

        switch (gameplayType)
        {
            case GameplayType.Bot:
                {
                    gameplayUpdateFn = UpdateBotGameplay;
                    currentRulestate = new BotGameplayRulestate(KickMissFeedback); 
                }
                break;
            case GameplayType.Guitar:
                {
                    gameplayUpdateFn = UpdateGuitarGameplay;
                    currentRulestate = new GuitarGameplayRulestate(KickMissFeedback);
                }
                break;
            case GameplayType.Drums:
                {
                    gameplayUpdateFn = UpdateDrumsGameplay;
                    currentRulestate = new DrumsGameplayRulestate(KickMissFeedback);
                }
                break;
            default:
                {
                }
                break;
        }
    }

    void UpdateBotGameplay(float time)
    {
        ((BotGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<NoteHitKnowledge>);
    }

    void UpdateGuitarGameplay(float time)
    {
        ((GuitarGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<GuitarNoteHitKnowledge>);
    }

    void UpdateDrumsGameplay(float time)
    {
        ((DrumsGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<DrumsNoteHitKnowledge>);
    }

    void UpdateUIStats(BaseGameplayRulestate currentRulestate)
    {
        BaseGameplayRulestate.NoteStats stats = currentRulestate.stats;
    }

    IHitWindow CreateHitWindow(GameplayType gameplayType)
    {
        switch (gameplayType)
        {
            case GameplayType.Bot:
                {
                    return new HitWindow<NoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
                }

            case GameplayType.Guitar:
                {
                    return new HitWindow<GuitarNoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
                }

            case GameplayType.Drums:
                {
                    return new HitWindow<GuitarNoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
                }
        }

        return null;
    }

    static GameplayType DetermineGameplayType(bool botEnabled, Chart.GameMode gameMode)
    {
        if (botEnabled)
        {
            return GameplayType.Bot;
        }
        else if (gameMode == Chart.GameMode.Guitar)
        {
            return GameplayType.Guitar;
        }
        else if (gameMode == Chart.GameMode.Drums)
        {
            return GameplayType.Drums;
        }

        return GameplayType.None;
    }
}
