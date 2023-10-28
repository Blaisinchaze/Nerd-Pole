using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameplayState { LOBBY, GAME, PAUSE, WIN, RESULTS, RESET, NULL }
public class GameplayController : MonoBehaviour
{

    public GameplayState gameState = GameplayState.LOBBY;

    public delegate void GameplayStateChanged(GameplayState gameplayState);
    public static event GameplayStateChanged OnGameplayStateChange;

    public delegate void ResetGame();
    public static event ResetGame OnResetGame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeState(GameplayState _nextState)
    {
        gameState = _nextState;
        OnGameplayStateChange.Invoke(gameState);
    }
    public void IncrementState(int _amountToIncrement)
    {
        int nextStateNumber = ((int)gameState + _amountToIncrement) % (Enum.GetNames(typeof(GameplayState)).Length - 1);
        ChangeState((GameplayState)nextStateNumber);
    }
}
