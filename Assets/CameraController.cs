using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    List<GameObject> players = new List<GameObject>();
    Camera cam;
    Transform targetPlayer;
    public float speed = 5f;
    public float minPlayerX, maxPlayerX;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        maxPlayerX = 0;

        foreach (var item in players)
        {
            maxPlayerX = Mathf.Abs(item.transform.position.x) > maxPlayerX ? Mathf.Abs(item.transform.position.x) : maxPlayerX;
            Vector2 screenPosition = cam.WorldToScreenPoint(item.transform.position);
            {
                if(screenPosition.y < 0)
                {
                    Debug.Log(item.gameObject.name + " has died");
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


    }

    private void FixedUpdate()
    {
        if(targetPlayer != null)
        {
            transform.position = new Vector3(0, Mathf.Lerp(transform.position.y, targetPlayer.position.y, Time.deltaTime * speed), -10);
            cam.orthographicSize = Mathf.Clamp(Mathf.Lerp(cam.orthographicSize, maxPlayerX, Time.deltaTime * speed), 6f,20f);
        }
    }

    public void PlayerJoin(GameObject player)
    {
        players.Add(player);
        if(targetPlayer == null) { targetPlayer = player.transform; }
    }

    public void PlayerLeave(GameObject player)
    {
        players.Remove(player);
    }
}
