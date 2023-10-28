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
        if (currentState == GameplayState.LOBBY) { inputManager.EnableJoining(); } else { inputManager.DisableJoining(); }
    }
    // Start is called before the first frame update
    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        camController = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {

        Debug.Log("Player " + playerInput.name + " joined");
        camController.PlayerJoin(playerInput.gameObject);
        playerInput.GetComponent<PlayerController>().UpdateColour(availableColors[0]);
        availableColors.RemoveAt(0);

    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        availableColors.Add(playerInput.GetComponent<PlayerController>().playerColour);
        Debug.Log("Player " + playerInput.name + " left");
        playerInput.GetComponent<PlayerController>().Die();
        camController.PlayerLeave(playerInput.gameObject);
    }
}
