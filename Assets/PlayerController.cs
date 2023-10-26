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


    public Color playerColour;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        blocksController = Object.FindObjectOfType<BlocksController>();
    }

    // Update is called once per frame
    void Update()
    {
        placingLocation = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt( transform.position.y) );
    }

    private void FixedUpdate()
    {

        rb.velocity = new Vector2(rb.velocity.x + velocity, rb.velocity.y);
    }

    public void OnMove(InputValue _movementValue)
    {
        Vector2 movementVector = _movementValue.Get<Vector2>();
        movementVector.y = 0;
        velocity = movementVector.x * speed;
    }
    public void OnPlaceLeft()
    {
        blocksController.CanPlaceBlock(placingLocation + new Vector2Int(-1,-1));
    }
    public void OnPlaceDown()
    {
        if (blocksController.CanPlaceBlock(placingLocation))
        {
            rb.transform.position += new Vector3(0, 1, 0);
        }
    }
    public void OnPlaceRight()
    {
        blocksController.CanPlaceBlock(placingLocation + new Vector2Int(1, -1));
    }

    public void UpdateColour(Color _color)
    {
        playerColour = _color;
        GetComponentInChildren<SpriteRenderer>().color = playerColour;
    }

}
