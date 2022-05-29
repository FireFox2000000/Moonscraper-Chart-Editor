using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayStatsDisplay : MonoBehaviour
{
    public Text noteStreakText;
    public Text percentHitText;
    public Text totalHitText;

    void Start()
    {
        ChartEditor.Instance.gameplayEvents.gameplayUpdateEvent.Register(GameplayUpdate);
    }

    void GameplayUpdate(in GameplayStateSystem.GameState gamestate)
    {
        if (enabled)
        {
            UpdateUIStats(gamestate.stats);
        }
    }

    void UpdateUIStats(in BaseGameplayRulestate.NoteStats stats)
    {
        uint noteStreak = stats.noteStreak;
        uint totalNotes = stats.totalNotes;
        uint notesHit = stats.notesHit;

        noteStreakText.text = noteStreak.ToString();
        if (totalNotes > 0)
            percentHitText.text = ((float)notesHit / (float)totalNotes * 100).Round(2).ToString() + "%";
        else
            percentHitText.text = "0.00%";

        totalHitText.text = notesHit.ToString() + " / " + totalNotes.ToString();
    }

    private void OnEnable()
    {
        Reset();
    }

    void Reset()
    {
        noteStreakText.text = "0";
        percentHitText.text = "0%";
        totalHitText.text = "0/0";
    }
}
