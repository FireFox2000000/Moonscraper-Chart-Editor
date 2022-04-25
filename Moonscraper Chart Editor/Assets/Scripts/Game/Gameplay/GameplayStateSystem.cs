using UnityEngine;
using TimingConfig;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class GameplayStateSystem : SystemManagerState.System
{
    // Configurable properties
    bool botEnabled = true;

    OneShotSampleStream missSoundSample;
    HitWindowFeeder hitWindowFeeder = new HitWindowFeeder();
    float playFromTime;

    delegate void GameplayUpdateFn(float time);
    GameplayUpdateFn gameplayUpdateFn = null;
    BaseGameplayRulestate currentRulestate;

    const int HIT_WINDOW_DELAY_TOTAL_FRAMES = 2;
    int hitWindowFrameDelayCount = HIT_WINDOW_DELAY_TOTAL_FRAMES;

    delegate void UpdateFn();
    UpdateFn currentUpdate = null;

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

    public GameplayStateSystem(float playFromTime, bool botEnabled)
    {
        this.botEnabled = botEnabled;
        this.playFromTime = playFromTime;

        currentUpdate = UpdateWaitingForNotesSettled;
    }

    public override void SystemEnter()
    {
        ChartEditor editor = ChartEditor.Instance;

        GameplayType gameplayType = DetermineGameplayType(botEnabled, editor.currentGameMode);
        LoadSoundClip();

        DetermineUpdateRulestate(gameplayType, out gameplayUpdateFn, out currentRulestate);

        hitWindowFeeder.hitWindow = CreateHitWindow(gameplayType);

        ChartEditor.Instance.uiServices.SetGameplayUIActive(!botEnabled);
    }

    public override void SystemUpdate()
    {
        currentUpdate();
    }

    void UpdateWaitingForNotesSettled()
    {
        // We need to wait a couple of frames for the physics system to settle down, otherwise notes can be sprawled all over the place if we're being spammy about playing
        --hitWindowFrameDelayCount;

        if (hitWindowFrameDelayCount <= 0)
        {
            currentUpdate = UpdateGameplay;
        }
    }

    void UpdateGameplay()
    {
        hitWindowFeeder.Update();

        float currentTime = ChartEditor.Instance.currentVisibleTime;
        gameplayUpdateFn?.Invoke(currentTime);

        GameState gamestate = new GameState();
        gamestate.stats = currentRulestate.stats;

        ChartEditor.Instance.gameplayEvents.gameplayUpdateEvent.Fire(gamestate);
    }

    public override void SystemExit()
    {
        missSoundSample = null;
        ChartEditor.Instance.uiServices.SetGameplayUIActive(false);
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
                    return new HitWindow<DrumsNoteHitKnowledge>(DrumsTiming.frontendHitWindowTime, DrumsTiming.backendHitWindowTime);
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
