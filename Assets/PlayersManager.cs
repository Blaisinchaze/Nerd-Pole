using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersManager : MonoBehaviour
{
    public List<Color> availableColors = new List<Color>();
    private PlayerInputManager inputManager;
    //private List<PlayerController> connectedPlayers = new List<PlayerController>();
    public List<PlayerController> connectedPlayers = new List<PlayerController>();
    public static PlayersManager Instance;
    GameplayState currentState;

    private void OnEnable()
    {
        GameplayController.OnGameplayStateChange += GameplayStateChange;
        PlayerController.OnPlayerStateChange += PlayerStateHasChanged;
    }
    private void OnDisable()
    {
        GameplayController.OnGameplayStateChange -= GameplayStateChange;
        PlayerController.OnPlayerStateChange -= PlayerStateHasChanged;
    }

    void GameplayStateChange(GameplayState _gameplayState)
    {
        currentState = _gameplayState;
        if (_gameplayState == GameplayState.LOBBY) { GetComponent<PlayerInputManager>().EnableJoining(); RespawnPlayers(); } else { GetComponent<PlayerInputManager>().DisableJoining(); }
    }

    void PlayerStateHasChanged(PlayerController _pc, PlayerState _ps)
    {

        if(currentState != GameplayState.GAME)
        {
            return;
        }

        PlayerController _potentialWinner = null;

        foreach (var item in connectedPlayers)
        {
            if (item.ReturnPlayerState() == PlayerState.SPAWN)
            {
                if(_potentialWinner != null)
                {
                    return;
                }
                _potentialWinner = item;
            }
        }

        GameplayController.Instance.WinnerFound(_potentialWinner);
    }

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);        
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }

    }

    public void RespawnPlayers()
    {
        foreach (var item in connectedPlayers)
        {
            item.Respawn();
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerController _pc = playerInput.GetComponent<PlayerController>();
        connectedPlayers.Add(_pc);
        _pc.UpdateColour(availableColors[0]);
        availableColors.RemoveAt(0);


    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        PlayerController _pc = playerInput.GetComponent<PlayerController>();
        availableColors.Add(_pc.playerColour);
        _pc.Despawn();
        connectedPlayers.Remove(_pc);
    }


}
