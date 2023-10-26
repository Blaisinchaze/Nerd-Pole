using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public enum BlockPlaceLocation { BELOW, LEFT, RIGHT }
public class BlocksController : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject floorPrefab;
    List<Vector2Int> placedBlockLocations = new List<Vector2Int>();

    [SerializeField]
    Vector2Int startingFloorBlock;
    [SerializeField]
    int floorLength;
    // Start is called before the first frame update
    void Start()
    {

        for (int i = -(floorLength/2); i < floorLength/2; i++)
        {
            placedBlockLocations.Add(new Vector2Int(i,1));
            Instantiate(floorPrefab, new Vector3(i, 1, 0), Quaternion.identity, this.transform);
        }
        Debug.Log("Floor Placed");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanPlaceBlock(Vector2Int location)
    {
        if(placedBlockLocations.Contains(location) || (!placedBlockLocations.Contains(location + Vector2Int.up) &&
            !placedBlockLocations.Contains(location + Vector2Int.down) && !placedBlockLocations.Contains(location + Vector2Int.right) &&
            !placedBlockLocations.Contains(location + Vector2Int.left)))
        {
            return false;
        }

        placedBlockLocations.Add(location);
        Instantiate(blockPrefab,new Vector3(location.x,location.y,0),Quaternion.identity,this.transform);
        Debug.Log("place block at " + location);
        return true;
    }
}
