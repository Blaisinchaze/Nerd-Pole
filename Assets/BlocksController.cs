using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public enum Directions { UP, DOWN, LEFT, RIGHT }
public class BlocksController : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject floorPrefab;
    Dictionary<Vector2Int, BlockInformation> placedBlockLocations = new Dictionary<Vector2Int, BlockInformation>();

    [SerializeField]
    Vector2Int startingFloorBlock;
    [SerializeField]
    int floorLength;

    public class BlockInformation
    {
        public Vector2Int location;
        public bool floor;
        public bool nextToBreak;
        public bool markedForDeath;
        private BlockInformation[] connectedBlocks = new BlockInformation[4];

        public BlockInformation(Vector2Int _location, bool _floor,bool _nextToBreak, bool _markedForDeath)
        {
            location = _location;
            floor = _floor;
            nextToBreak = _nextToBreak;
            markedForDeath = _markedForDeath;
        }

        public void AddNeighbour(int _index, BlockInformation _value)
        {
            if (connectedBlocks[_index] == _value) return;
            connectedBlocks[_index] = _value;
            _value.AddNeighbour(ReverseIndex(_index), this);
        }
        private int ReverseIndex(int _index)
        {
            if(_index == 0) return 1;
            if(_index == 1) return 0;
            if(_index == 2) return 3;
            if(_index == 3) return 2;

            //crash.jpeg
            return 10;
        }
    }

    void Start()
    {

        for (int i = -(floorLength/2); i < floorLength/2; i++)
        {
            placedBlockLocations.Add(new Vector2Int(i,1), new BlockInformation(new Vector2Int(i, 1), true, false, false));
            Instantiate(floorPrefab, new Vector3(i, 1, 0), Quaternion.identity, this.transform);
        }
        Debug.Log("Floor Placed");
    }

    void Update()
    {
        
    }

    public bool CanPlaceBlock(Vector2Int _location, float _delay = 0)
    {
        if(placedBlockLocations.ContainsKey(_location) || (!placedBlockLocations.ContainsKey(_location + Vector2Int.up) &&
            !placedBlockLocations.ContainsKey(_location + Vector2Int.down) && !placedBlockLocations.ContainsKey(_location + Vector2Int.right) &&
            !placedBlockLocations.ContainsKey(_location + Vector2Int.left)))
        {
            return false;
        }


        placedBlockLocations.Add(_location, new BlockInformation(_location, false, false,false));

        if(CheckBlockInDirection(_location, Vector2Int.up)) { placedBlockLocations[_location].AddNeighbour(0, placedBlockLocations[_location + Vector2Int.up]); }
        if(CheckBlockInDirection(_location, Vector2Int.down)) { placedBlockLocations[_location].AddNeighbour(0, placedBlockLocations[_location + Vector2Int.down]); }
        if(CheckBlockInDirection(_location, Vector2Int.left)) { placedBlockLocations[_location].AddNeighbour(0, placedBlockLocations[_location + Vector2Int.left]); }
        if(CheckBlockInDirection(_location, Vector2Int.right)) { placedBlockLocations[_location].AddNeighbour(0, placedBlockLocations[_location + Vector2Int.right]); }


        Vector3 _placeLocation = new Vector3(_location.x, _location.y, 0);
        if (_delay > 0)
        {
            StartCoroutine(DelayPlace(_delay, _placeLocation));
        }
        else
        {
            Instantiate(blockPrefab,_placeLocation,Quaternion.identity,this.transform);
        }

        return true;
    }

    public void BreakBlock()
    {

    }

    public bool CheckBlockInDirection(Vector2Int _location, Vector2Int directionCheck)
    {
        return (placedBlockLocations.ContainsKey(_location + directionCheck));
    }

    IEnumerator DelayPlace(float _delay, Vector3 _location)
    {
        yield return new WaitForSeconds(_delay);
        Instantiate(blockPrefab, _location, Quaternion.identity, this.transform);
    }
}
