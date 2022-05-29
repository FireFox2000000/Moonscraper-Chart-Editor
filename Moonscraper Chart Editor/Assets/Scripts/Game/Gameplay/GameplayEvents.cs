using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayEvents
{
    public MoonscraperEngine.Event explicitMissEvent { get; private set; }
    public MoonscraperEngine.Event<GameplayStateSystem.GameState> gameplayUpdateEvent { get; private set; }

    public GameplayEvents()
    {
        explicitMissEvent = new MoonscraperEngine.Event();
        gameplayUpdateEvent = new MoonscraperEngine.Event<GameplayStateSystem.GameState>();
    }
}
