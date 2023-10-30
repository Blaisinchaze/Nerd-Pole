using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameplayState { LOBBY, GAME, PAUSE, WIN, RESULTS, RESET, NULL }
public class GameplayController : MonoBehaviour
{
    public static GameplayController Instance;
    public GameplayState gameState = GameplayState.LOBBY;

    public delegate void GameplayStateChanged(GameplayState gameplayState);
    public static event GameplayStateChanged OnGameplayStateChange;

    public delegate void ResetGame();
    public static event ResetGame OnResetGame;

    public Dictionary<PlayerInput, Color> connectedPlayers = new Dictionary<PlayerInput, Color>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        ChangeState(gameState);
    }

    public void WinnerFound(PlayerController _winner)
    {
        Debug.Log("Winner is " + _winner);
        ChangeState(GameplayState.WIN);
    }

    public void ChangeState(GameplayState _nextState)
    {
        gameState = _nextState;
        OnGameplayStateChange.Invoke(gameState);
        //if(gameState == GameplayState.LOBBY)
        //{
        //    GameObject.FindObjectOfType<PlayersController>().RespawnPlayers(connectedPlayers);
        //}
        if(gameState == GameplayState.WIN) { StartCoroutine(WaitForStateChange(GameplayState.RESET)); }
        if (gameState == GameplayState.RESET) { StartCoroutine(ResetGameScene()); }
    }
    public void IncrementState(int _amountToIncrement)
    {
        //int nextStateNumber = (int)gameState + _amountToIncrement;
        int nextStateNumber = Modulas((int)gameState + _amountToIncrement, (Enum.GetNames(typeof(GameplayState)).Length - 1));

        ChangeState((GameplayState)nextStateNumber);
    }

    public static int Modulas(int input, int divisor)
    {
        return (input % divisor + divisor) % divisor;
    }

    IEnumerator WaitForStateChange(GameplayState _stateToChangeTo)
    {
        yield return new WaitForSeconds(2);
        ChangeState(_stateToChangeTo);
    }
    IEnumerator ResetGameScene()
    {
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return new WaitForSeconds(0.1f);
        ChangeState( GameplayState.LOBBY);
    }
}
