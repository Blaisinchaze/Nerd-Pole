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

    float blockDestructionTimerCurrent;
    float blockDestructionTimerMax = 1f;

    //private List<BlockInformation> nextBlocksToBreak = new List<BlockInformation>();

    public class BlockInformation
    {
        public Vector2Int location;
        public bool floor;
        public bool nextToBreak;
        public bool markedForDeath;
        private List<BlockInformation> connectedBlocks = new List<BlockInformation>();
        private GameObject blockBody;
        public BlockInformation(Vector2Int _location, bool _floor,bool _nextToBreak, bool _markedForDeath)
        {
            location = _location;
            floor = _floor;
            nextToBreak = _nextToBreak;
            markedForDeath = _markedForDeath;
        }
        public void SetBlockBody(GameObject _body)
        {
            blockBody = _body;
        }

        public void AddNeighbour(BlockInformation _value)
        {
            if (_value.location == location || connectedBlocks.Contains(_value)) return;

            

            connectedBlocks.Add(_value);
            _value.AddNeighbour(this);
            Debug.Log("Block at " + location + " is connected to " + connectedBlocks.Count);

        }

        public void UpdateNeighbour(BlockInformation _value)
        {
            if (_value.location == location || connectedBlocks.Contains(_value)) return;

            connectedBlocks.Add(_value);
        }
        public List<BlockInformation> ReturnNeighbours()
        {
            return connectedBlocks;
        }
        public void DestroyBlock()
        {
            Debug.Log("Destroy Block "+ location);
            foreach (var item in connectedBlocks)
            {
                print("destroy next the block at " + item.location);
                if (item.markedForDeath) { nextToBreak = true; }
                RemoveNeighbour(this);
            }
            Destroy(blockBody);
        }
        public void RemoveNeighbour(BlockInformation _value)
        {
            if (!connectedBlocks.Contains(_value))
            {
                return;
            }
            connectedBlocks.Remove(_value);
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
        blockDestructionTimerCurrent -= Time.deltaTime;
        if(blockDestructionTimerCurrent < 0)
        {
            BreakBlocks();
            blockDestructionTimerCurrent = blockDestructionTimerMax;
        }
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

        if(CheckBlockInDirection(_location, Vector2Int.up)) { placedBlockLocations[_location].AddNeighbour(placedBlockLocations[_location + Vector2Int.up]); }
        if(CheckBlockInDirection(_location, Vector2Int.down)) {  placedBlockLocations[_location].AddNeighbour(placedBlockLocations[_location + Vector2Int.down]); }
        if(CheckBlockInDirection(_location, Vector2Int.left)) {  placedBlockLocations[_location].AddNeighbour(placedBlockLocations[_location + Vector2Int.left]); }
        if(CheckBlockInDirection(_location, Vector2Int.right)) {  placedBlockLocations[_location].AddNeighbour(placedBlockLocations[_location + Vector2Int.right]); }


        Vector3Int _placeLocation = new Vector3Int(_location.x, _location.y, 0);
        if (_delay > 0)
        {
            StartCoroutine(DelayPlace(_delay, _placeLocation));
        }
        else
        {
            GameObject _newBlock = Instantiate(blockPrefab,_placeLocation,Quaternion.identity,this.transform);
            placedBlockLocations[_location].SetBlockBody(_newBlock);
        }

        return true;
    }

    public bool BreakBlockCheck(Vector2Int _location)
    {

        if (!placedBlockLocations.ContainsKey(_location) || placedBlockLocations[_location].floor) { return false; }
        Debug.Log("Breaking Block at " + placedBlockLocations[_location].location);
        List<BlockInformation> _checkedBlocks = new List<BlockInformation>();
        List<BlockInformation> _blocksToCheck = placedBlockLocations[_location].ReturnNeighbours();
        int cycle = 0;
        bool _destroyBlocks = true;
        while (_blocksToCheck.Count > 0)
        {
            if (_blocksToCheck[_blocksToCheck.Count - 1].floor == true)
            {
                _destroyBlocks = false;
                break;
            }

            List<BlockInformation> _tempBlocks = new List<BlockInformation>();
            foreach (var item in _blocksToCheck[_blocksToCheck.Count - 1].ReturnNeighbours())
            {
                if (_checkedBlocks.Contains(item))
                {
                    continue;
                }
                else
                {
                    _tempBlocks.Add(item);
                }
            }

            _checkedBlocks.Add(_blocksToCheck[_blocksToCheck.Count - 1]);
            _blocksToCheck.RemoveAt(_blocksToCheck.Count - 1);
            foreach (var item in _tempBlocks)
            {
                _blocksToCheck.Add(item);
            }
            cycle++;
            Debug.Log("Cycle - " + cycle);
        }

        if (_destroyBlocks == true)
        {
            foreach (var item in _checkedBlocks)
            {
                item.markedForDeath = true;
            }
        }


        placedBlockLocations[_location].DestroyBlock();
        placedBlockLocations.Remove(_location);
        return true;

    }

    private void BreakBlocks()
    {
        foreach (var item in placedBlockLocations)
        {
            if(item.Value.nextToBreak)
            {
                item.Value.DestroyBlock() ;
                placedBlockLocations.Remove(item.Key);
            }
        }
    }

    public bool CheckBlockInDirection(Vector2Int _location, Vector2Int directionCheck)
    {
        return (placedBlockLocations.ContainsKey(_location + directionCheck));
    }

    IEnumerator DelayPlace(float _delay, Vector3Int _location)
    {
        yield return new WaitForSeconds(_delay);
        Vector2Int _placeLocation = new Vector2Int(_location.x, _location.y);
        GameObject _newBlock = Instantiate(blockPrefab, _location, Quaternion.identity, this.transform);
        placedBlockLocations[_placeLocation].SetBlockBody(_newBlock);
    }
}
