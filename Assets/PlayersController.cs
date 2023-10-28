using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersController : MonoBehaviour
{
    [SerializeField]
    GameObject playerPrefab;
    CameraController camController;
    public List<Color> availableColors = new List<Color>();
    private GameplayState currentState;
    private PlayerInputManager inputManager;
    private List<PlayerController> connectedPlayers = new List<PlayerController>();
    public static PlayersController Instance;

    private void OnEnable()
    {
        GameplayController.OnGameplayStateChange += GameplayStateChange;
    }
    private void OnDisable()
    {
        GameplayController.OnGameplayStateChange -= GameplayStateChange;
    }

    void GameplayStateChange(GameplayState _gameplayState)
    {
        currentState = _gameplayState;
        if (currentState == GameplayState.LOBBY) { inputManager.EnableJoining(); RespawnPlayers(); } else { inputManager.DisableJoining(); }
    }
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        inputManager = GetComponent<PlayerInputManager>();
        camController = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RespawnPlayers()
    {
        Debug.Log("Respawning " + connectedPlayers.Count + " players");
        foreach (var item in connectedPlayers)
        {
            item.Live();
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {

        Debug.Log("Player " + playerInput.name + " joined");
        camController.PlayerJoin(playerInput.gameObject);
        playerInput.GetComponent<PlayerController>().UpdateColour(availableColors[0]);
        //GameplayController.Instance.connectedPlayers.Add(playerInput, availableColors[0]);
        connectedPlayers.Add(playerInput.GetComponent<PlayerController>());
        availableColors.RemoveAt(0);

    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        availableColors.Add(playerInput.GetComponent<PlayerController>().playerColour);
        Debug.Log("Player " + playerInput.name + " left");
        playerInput.GetComponent<PlayerController>().Die();
        camController.PlayerLeave(playerInput.gameObject);
        //GameplayController.Instance.connectedPlayers.Remove(playerInput);
        connectedPlayers.Remove(playerInput.GetComponent<PlayerController>());
    }


}
