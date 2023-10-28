using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Vector2Int placingLocation;
    Rigidbody2D rb;
    float velocity;
    BlocksController blocksController;
    public float speed;
    public float jumpForce;

    public Color playerColour;

    public bool grounded;
    public bool placingBlock;
    float groundedDistance;
    public LayerMask layerMask;

    [SerializeField]
    Vector3 boxCastLocation;
    [SerializeField]
    Vector2 boxCastSize;

    public float maxVelocityX, maxPlacingVelocityX;

    public float placeCooldownMax, placeCooldownCurrent;

    public PhysicsMaterial2D inAirMaterial, onGroundMaterial;

    public bool shield;

    public GameObject deathPS;
    private GameplayState currentState;

    public GameObject body;
    bool alive = true;
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
        if(currentState == GameplayState.LOBBY)
        {
            blocksController = Object.FindObjectOfType<BlocksController>();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);
        rb = GetComponent<Rigidbody2D>();
        blocksController = Object.FindObjectOfType<BlocksController>();
        placingLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        groundedDistance = (GetComponent<CapsuleCollider2D>().size.y/2) + 0.05f;
        placeCooldownCurrent = placeCooldownMax;
    }

    // Update is called once per frame
    void Update()
    {
        placeCooldownCurrent -= Time.deltaTime;
        placingLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt( transform.position.y) );
    }

    private void FixedUpdate()
    {
        if(currentState != GameplayState.LOBBY && currentState != GameplayState.GAME)
        {
            return;
        }
        Collider2D hit = Physics2D.OverlapBox(transform.position + boxCastLocation, boxCastSize,0f, layerMask);
        grounded = (hit != null);
        if(grounded && placingBlock && placeCooldownCurrent <= 0) { placingBlock = false; }
        rb.sharedMaterial = grounded ? onGroundMaterial : inAirMaterial;

        Vector2 tempVelocity = Vector2.zero;
        tempVelocity.x = placingBlock ? Mathf.Clamp(rb.velocity.x + velocity, -maxPlacingVelocityX, maxPlacingVelocityX) : Mathf.Clamp(rb.velocity.x + velocity, -maxVelocityX, maxVelocityX);
        if (placingBlock && placeCooldownCurrent >= 0)
        {
            tempVelocity.y = 5;
        }
        else 
        {
            if (grounded)
            {
                tempVelocity.y = 0;
            }
            else
            {
                tempVelocity.y = Mathf.Clamp(rb.velocity.y - 1, -10, 10);
            }
        }
        //tempVelocity.y = placingBlock && placeCooldownCurrent >= 0 ? 3 : rb.velocity.y;
        rb.velocity = tempVelocity;


        
    }
    public void Live()
    {
        alive = true;
        transform.position = new Vector3(0, 5, 0);
        body.SetActive(true);
    }
    public void Die()
    {
        if (alive)
        {
            Instantiate(deathPS, transform.position, Quaternion.identity);
            //gameObject.SetActive(false);
            body.SetActive(false);
            //Destroy(this.gameObject);
            alive = false;
        }

    }
    public void OnMove(InputValue _movementValue)
    {        
        Vector2 movementVector = _movementValue.Get<Vector2>();
        if(Mathf.Abs(movementVector.x) < 0.2f) { movementVector.x = 0f; }
        if(Mathf.Abs(movementVector.y) < 0.2f) { movementVector.y = 0f; }
        if (shield)
        {
            blocksController.BreakBlockCheck(placingLocation + Vector2Int.RoundToInt(movementVector));
        }
        else
        {
            movementVector.y = 0;
            velocity = movementVector.x * speed;
        }


    }
    public void OnPlaceLeft()
    {
        if (shield || currentState != GameplayState.GAME) return;
        blocksController.CanPlaceBlock(placingLocation + new Vector2Int(-1,-1));
    }
    public void OnPlaceDown()
    {

        if (!grounded || shield || currentState != GameplayState.GAME) {return; }
        if (blocksController.CheckBlockInDirection(placingLocation, Vector2Int.up)) { return; }
        if (blocksController.CanPlaceBlock(placingLocation, 0.35f))
        {
            placingBlock = true;
            placeCooldownCurrent = placeCooldownMax;
            rb.velocity += new Vector2(0, -rb.velocity.y);
            //rb.AddForce(Vector2.up * jumpForce);
        }
    }
    public void OnPlaceRight()
    {
        if (shield || currentState != GameplayState.GAME) return;
        blocksController.CanPlaceBlock(placingLocation + new Vector2Int(1, -1));
    }

    public void OnShield(InputValue _pressedValue)
    {
        float value = _pressedValue.Get<float>();
        shield = value > 0;
        rb.velocity = Vector2.zero;
        velocity = 0;
    }

    public void UpdateColour(Color _color)
    {
        playerColour = _color;
        GetComponentInChildren<SpriteRenderer>().color = playerColour;
    }


    private void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + boxCastLocation, boxCastSize);
    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Debug.Log(collision.gameObject.name);
    //    if (collision.transform.tag == "Goal")
    //    {
    //        Debug.Log("VictoryFound");
    //        GameplayController.Instance.ChangeState(GameplayState.WIN);
    //    }
        
    //}
}
