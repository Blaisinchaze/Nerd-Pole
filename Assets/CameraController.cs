using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    List<GameObject> players = new List<GameObject>();
    Camera cam;
    Transform targetPlayer;
    public float speed = 5f;
    public float minPlayerX, maxPlayerX, minCameraScale, maxCameraScale;
    private GameplayState currentState;
    private List<GameObject> playersToDie = new List<GameObject>();
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
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == GameplayState.GAME)
        {
            CalculateCameraWidth();
        }            

    }

    private void FixedUpdate()
    {
        if(targetPlayer != null)
        {
            float _minHeight = (cam.orthographicSize / cam.aspect) * 2;
            transform.position = new Vector3(0, Mathf.Clamp(Mathf.Lerp(transform.position.y, targetPlayer.position.y, Time.deltaTime * speed), _minHeight, 500f), -10);
            cam.orthographicSize = Mathf.Clamp(Mathf.Lerp(cam.orthographicSize, maxPlayerX, Time.deltaTime * speed), minCameraScale, maxCameraScale);
        }
    }

    public void CalculateCameraWidth()
    {
        maxPlayerX = 0;
        playersToDie.Clear();
        foreach (var item in players)
        {
            maxPlayerX = Mathf.Abs(item.transform.position.x) > maxPlayerX ? Mathf.Abs(item.transform.position.x) : maxPlayerX;
            Vector2 screenPosition = cam.WorldToScreenPoint(item.transform.position);
            {
                if (screenPosition.y < -20) 
                {
                    playersToDie.Add(item);
                }
            }
            if (targetPlayer == null)
            {
                targetPlayer = item.transform;
                continue;
            }
            if (item.transform.position.y > targetPlayer.position.y)
            {
                targetPlayer = item.transform;
            }
        }
        for (int i = playersToDie.Count - 1; i >= 0; i--)
        {
            playersToDie[i].GetComponent<PlayerController>().Die();
            Debug.Log(playersToDie[i].gameObject.name + " has died");
        }
    }

    public void PlayerJoin(GameObject player)
    {
        players.Add(player);
        maxCameraScale++;
        if(targetPlayer == null) { targetPlayer = player.transform; }
    }

    public void PlayerLeave(GameObject player)
    {
        maxCameraScale--;
        players.Remove(player);
        if(targetPlayer == player)
        {
            targetPlayer = players[0].transform;
        }
    }
}
