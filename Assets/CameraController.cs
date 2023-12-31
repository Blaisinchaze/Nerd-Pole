using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //List<GameObject> players = new List<GameObject>();
    Camera cam;
    Transform targetPlayer;
    public float speed = 5f;
    public float maxPlayerY=0, maxPlayerX = 0, minCameraScale, maxCameraScale;
    private GameplayState currentState;

    public float shake, shakeAmount = 0.0001f, decreaseFactor = 1f;
    private List<GameObject> playersToDie = new List<GameObject>();

    private List<GameObject> trackedPlayers = new List<GameObject>();
    private void OnEnable()
    {
        GameplayController.OnGameplayStateChange += GameplayStateChange;
        PlayerController.OnPlayerStateChange += PlayerStateChange;
    }
    private void OnDisable()
    {
        GameplayController.OnGameplayStateChange -= GameplayStateChange;
        PlayerController.OnPlayerStateChange -= PlayerStateChange;
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
            float _targetOrthographicSize = Mathf.Clamp(Mathf.Max(maxPlayerX, maxPlayerY), minCameraScale, maxCameraScale);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, _targetOrthographicSize, Time.deltaTime * speed);

            float _minHeight = cam.orthographicSize +0.5f;
            transform.position = new Vector3(0, Mathf.Clamp(Mathf.Lerp(transform.position.y, targetPlayer.position.y - 1f, Time.deltaTime * speed), _minHeight, 500f), -10);

            if(shake > 0)
            {
                transform.localPosition = Random.insideUnitSphere * shakeAmount;
                shake -= Time.deltaTime;

            }
            else
            {
                shake = 0;
            }
        }
    }

    public void CalculateCameraWidth()
    {
        playersToDie.Clear();
        maxPlayerX = 0;
        maxPlayerY = 0;
        foreach (var item in trackedPlayers)
        {
            maxPlayerX = Mathf.Abs(item.transform.position.x) > maxPlayerX ? Mathf.Abs(item.transform.position.x) : maxPlayerX;
            maxPlayerY = cam.transform.position.y + 1f - item.transform.position.y > maxPlayerY ? cam.transform.position.y + 1f - item.transform.position.y : maxPlayerY;


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

    //public void PlayerJoin(GameObject player)
    //{
    //    players.Add(player);
    //    maxCameraScale+= 1.5f;
    //    if(targetPlayer == null) { targetPlayer = player.transform; }
    //}

    public void PlayerLeave(GameObject player)
    {
        Debug.Log("Player Left");
        if(currentState == GameplayState.LOBBY)
        {
            maxCameraScale -= 1.5f;
        }

        trackedPlayers.Remove(player);
        if(targetPlayer == player)
        {
            targetPlayer = trackedPlayers[0].transform;
        }
    }

    private void PlayerStateChange(PlayerController _pc, PlayerState _ps)
    {
        switch (_ps)
        {
            case PlayerState.SPAWN:
                if (trackedPlayers.Contains(_pc.gameObject)) return;
                trackedPlayers.Add(_pc.gameObject);
                maxCameraScale += 1.5f;
                if (targetPlayer == null) { targetPlayer = _pc.gameObject.transform; }
                break;
            case PlayerState.DEAD:
                if (!trackedPlayers.Contains(_pc.gameObject)) return;
                shake = 1f;
                trackedPlayers.Remove(_pc.gameObject);
                break;
            case PlayerState.DESPAWN:
                PlayerLeave(_pc.gameObject);
                break;
            case PlayerState.NULL:
                break;
            default:
                break;
        }
    }
}
