using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameplayController.Instance.gameState != GameplayState.GAME) return;
        Debug.Log(other.gameObject.name);
        if (other.transform.tag == "Player")
        {
            Debug.Log("VictoryFound");
            GameplayController.Instance.ChangeState(GameplayState.WIN);
        }
    }

}
