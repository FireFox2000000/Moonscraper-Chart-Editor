using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayEvents
{
    public MSE.Event explicitMissEvent { get; private set; }
    public MSE.Event<GameplayStateSystem.GameState> gameplayUpdateEvent { get; private set; }

    public GameplayEvents()
    {
        explicitMissEvent = new MSE.Event();
        gameplayUpdateEvent = new MSE.Event<GameplayStateSystem.GameState>();
    }
}
