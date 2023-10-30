using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState { SPAWN, DEAD,DESPAWN, NULL}
public enum PlayerAimDirection { NONE,LEFT,DOWN,RIGHT,NULL }

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Vector2Int currentLocation;
    PlayerAimDirection currentPlacingDirection;

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
    private GameplayState currentGameState;

    public GameObject body;

    private PlayerState currentPlayerState;
    public delegate void PlayerStateChange(PlayerController playerController, PlayerState playerState);
    public static event PlayerStateChange OnPlayerStateChange;


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
        currentGameState = _gameplayState;
        if (currentGameState == GameplayState.LOBBY)
        {
            InitialisePlayer();
        }
    }

    void InitialisePlayer()
    {
        DontDestroyOnLoad(this);
        rb = GetComponent<Rigidbody2D>();
        blocksController = Object.FindObjectOfType<BlocksController>();
        currentLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        groundedDistance = (GetComponent<CapsuleCollider2D>().size.y / 2) + 0.05f;
        placeCooldownCurrent = placeCooldownMax;
        currentPlayerState = PlayerState.SPAWN;
        PlayerStateHasChange();
    }

    // Start is called before the first frame update
    void Awake()
    {
        InitialisePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        //Count Down Cooldown On Placing Objects
        BlockPlaceCheck();
        ShieldCheck();

    }

    private void FixedUpdate()
    {
        if (currentGameState != GameplayState.LOBBY && currentGameState != GameplayState.GAME)
        {
            return;
        }
        Collider2D hit = Physics2D.OverlapBox(transform.position + boxCastLocation, boxCastSize, 0f, layerMask);
        grounded = (hit != null);
        if (grounded && placingBlock && placeCooldownCurrent <= 0) { placingBlock = false; }
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
        rb.velocity = tempVelocity;



    }
    void ShieldCheck()
    {

    }

    public void BlockPlaceCheck()
    {
        if(placeCooldownCurrent >= 0)
        {
            placeCooldownCurrent -= Time.deltaTime;
            return;
        }
        if (currentPlacingDirection == PlayerAimDirection.NONE || shield || !grounded || currentGameState != GameplayState.GAME) return;
        currentLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        switch (currentPlacingDirection)
        {
            case PlayerAimDirection.LEFT:
                blocksController.CanPlaceBlock(currentLocation + new Vector2Int(-1, -1));
                break;
            case PlayerAimDirection.DOWN:
                if (blocksController.CheckBlockInDirection(currentLocation, Vector2Int.up)) { return; }
                if (blocksController.CanPlaceBlock(currentLocation, 0.35f))
                {
                    placingBlock = true;
                    rb.velocity += new Vector2(0, -rb.velocity.y);
                }
                break;
            case PlayerAimDirection.RIGHT:
                blocksController.CanPlaceBlock(currentLocation + new Vector2Int(1, -1));
                break;
            default:
                break;
        }
        placeCooldownCurrent = placeCooldownMax;
    }
    public void Respawn()
    {
        transform.position = new Vector3(0, 5, 0);
        body.SetActive(true);
        currentPlayerState = PlayerState.SPAWN;
        PlayerStateHasChange();
    }
    public void Die()
    {
        Instantiate(deathPS, transform.position, Quaternion.identity);
        body.SetActive(false);
        currentPlayerState = PlayerState.DEAD;
        PlayerStateHasChange();

    }
    public void Despawn()
    {
        currentPlayerState = PlayerState.DESPAWN;
        PlayerStateHasChange();
    }

    private void PlayerStateHasChange()
    {
        OnPlayerStateChange.Invoke(this, currentPlayerState);
    }
    public void OnMove(InputValue _movementValue)
    {
        Vector2 movementVector = _movementValue.Get<Vector2>();
        if (Mathf.Abs(movementVector.x) < 0.2f) { movementVector.x = 0f; }
        if (Mathf.Abs(movementVector.y) < 0.2f) { movementVector.y = 0f; }

        currentLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (shield)
        {
            blocksController.BreakBlockCheck(currentLocation + Vector2Int.RoundToInt(movementVector));
        }
        else
        {
            velocity = movementVector.x * speed;
        }


    }
    public void OnPlaceLeft(InputValue _pressedValue)
    {
        OnChangeTargetPlaceCheck(PlayerAimDirection.LEFT, _pressedValue.Get<float>() > 0.5);
    }
    public void OnPlaceDown(InputValue _pressedValue)
    {
        OnChangeTargetPlaceCheck(PlayerAimDirection.DOWN, _pressedValue.Get<float>() > 0.5);
    }
    public void OnPlaceRight(InputValue _pressedValue)
    {
        OnChangeTargetPlaceCheck(PlayerAimDirection.RIGHT, _pressedValue.Get<float>() > 0.5);
    }

    public void OnStartGame()
    {
        if (currentGameState != GameplayState.LOBBY && currentGameState != GameplayState.RESULTS) return;
        GameplayController.Instance.IncrementState(1);
    }

    public void OnShield(InputValue _pressedValue)
    {
        float value = _pressedValue.Get<float>();
        shield = value > 0;
        rb.velocity = Vector2.zero;
        velocity = 0;
    }

    public void OnChangeTargetPlaceCheck(PlayerAimDirection _targetDirection, bool _pressed)
    {
        if (!_pressed /*&& currentPlacingDirection == _targetDirection*/)
        {
            currentPlacingDirection = PlayerAimDirection.NONE;
        }
        else
        {
            currentPlacingDirection = _targetDirection;
        }
    }

    public PlayerState ReturnPlayerState()
    {
        return currentPlayerState;
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
        Gizmos.color = playerColour;
        switch (currentPlacingDirection)
        {
            case PlayerAimDirection.LEFT:
                Gizmos.DrawWireCube(currentLocation + new Vector2(-1,-1), Vector2.one);
                break;
            case PlayerAimDirection.DOWN:
                Gizmos.DrawWireCube(currentLocation + new Vector2(0, 0), Vector2.one);
                break;
            case PlayerAimDirection.RIGHT:
                Gizmos.DrawWireCube(currentLocation + new Vector2(1, -1), Vector2.one);
                break;
            default:
                break;
        }
    }

}
