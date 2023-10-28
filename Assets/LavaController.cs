using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaController : MonoBehaviour
{
    private GameplayState currentState;
    private bool moving;
    public float speed;
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
        moving = currentState == GameplayState.GAME;
    }

    private void Update()
    {
        if (moving)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().Die();
        }
    }

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.transform.CompareTag("Player"))
    //    {
    //        collision.gameObject.GetComponent<PlayerController>().Die();
    //    }
    //}
}
