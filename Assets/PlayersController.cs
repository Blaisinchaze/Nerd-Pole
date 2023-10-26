using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersController : MonoBehaviour
{
    CameraController camController;
    public List<Color> availableColors = new List<Color>();
    // Start is called before the first frame update
    void Start()
    {
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
        camController.PlayerLeave(playerInput.gameObject);
    }
}
