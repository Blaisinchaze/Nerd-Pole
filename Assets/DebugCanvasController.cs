using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugCanvasController : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI currentStateText;
    private GameplayState currentState;

    private void OnEnable()
    {
        GameplayController.OnGameplayStateChange += GameplayStateChange;
    }
    private void OnDisable()
    {
        GameplayController.OnGameplayStateChange -= GameplayStateChange;
    }

    private void Awake()
    {
        if(Object.FindObjectsOfType<DebugCanvasController>().Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
        }
    }
    private void Start()
    {
        
    }

    void GameplayStateChange(GameplayState _gameplayState)
    {
        currentState = _gameplayState;
        currentStateText.text = currentState.ToString();
    }

}
