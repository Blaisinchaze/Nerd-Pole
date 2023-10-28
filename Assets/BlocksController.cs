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
    public GameObject blockBreakPrefab;
    Dictionary<Vector2Int, BlockInformation> placedBlockLocations = new Dictionary<Vector2Int, BlockInformation>();

    [SerializeField]
    Vector2Int startingFloorBlock;
    [SerializeField]
    int floorLength;

    float blockDestructionTimerCurrent;
    float blockDestructionTimerMax = 0.3f;

    private List<Vector2Int> nextBlocksToBreak = new List<Vector2Int>();
    private int idCount;
    private GameplayState currentState;

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

        }
    }

    [System.Serializable]
    public class BlockInformation
    {
        public int id;
        public Vector2Int location;
        public bool floor;
        public bool nextToBreak;
        public bool markedForDeath;
        private GameObject blockBody;
        public BlockInformation(int _id,Vector2Int _location, bool _floor,bool _nextToBreak, bool _markedForDeath)
        {
            id = _id;
            location = _location;
            floor = _floor;
            nextToBreak = _nextToBreak;
            markedForDeath = _markedForDeath;
        }
        public void SetBlockBody(GameObject _body)
        {
            blockBody = _body;
        }


        public void DestroyBlock()
        {
            Debug.Log("Destroy Block " + location);

            Destroy(blockBody);
        }

    }

    void Start()
    {

        for (int i = -(floorLength/2); i < floorLength/2; i++)
        {
            placedBlockLocations.Add(new Vector2Int(i,1), new BlockInformation(idCount++,new Vector2Int(i, 1), true, false, false));
            Instantiate(floorPrefab, new Vector3(i, 1, 0), Quaternion.identity, this.transform);
        }

        placedBlockLocations.Add(new Vector2Int(0, 6), new BlockInformation(idCount++, new Vector2Int(0, 6), true, false, false));
        Instantiate(floorPrefab, new Vector3(0, 6, 0), Quaternion.identity, this.transform);
        Debug.Log("Floor Placed");
    }

    void Update()
    {
        if (currentState != GameplayState.GAME) return;
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


        placedBlockLocations.Add(_location, new BlockInformation(idCount++,_location, false, false,true));

        bool noneForDeath = true;
        foreach (var item in ReturnNeighbourBlocks(_location))
        {
            if (placedBlockLocations[item].markedForDeath)
            {
                noneForDeath = false;
            }
            else
            {
                placedBlockLocations[_location].markedForDeath = false;
            }
        }
        if(noneForDeath == false && placedBlockLocations[_location].markedForDeath == false) { FixBlocks(_location); }
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

        List<Vector2Int> _startingBlocks = ReturnNeighbourBlocks(_location);
        foreach (var directionToCheck in _startingBlocks)
        {
            bool _destroyBlocks = true;
            List<Vector2Int> _checkedBlocks = new List<Vector2Int>();
            _checkedBlocks.Add(_location);
            List<Vector2Int> _blocksToCheck = new List<Vector2Int>(); 
            _blocksToCheck.Add(directionToCheck);
            while (_blocksToCheck.Count > 0)
            {
                if (placedBlockLocations[_blocksToCheck[_blocksToCheck.Count - 1]].floor == true)
                {
                    _destroyBlocks = false;
                    break;
                }

                List<Vector2Int> _tempBlocks = new List<Vector2Int>();
                foreach (var item in ReturnNeighbourBlocks(placedBlockLocations[_blocksToCheck[_blocksToCheck.Count - 1]].location))
                {
                    if (_checkedBlocks.Contains(item))
                    {
                        continue;
                    }
                    else
                    {
                        Vector2Int location = item;
                        _tempBlocks.Add(item);
                    }
                }

                _checkedBlocks.Add(_blocksToCheck[_blocksToCheck.Count - 1]);
                _blocksToCheck.RemoveAt(_blocksToCheck.Count - 1);

                foreach (var item in _tempBlocks)
                {
                    _blocksToCheck.Add(item);
                }
            }

            if (_destroyBlocks == true)
            {
                foreach (var item in _checkedBlocks)
                {
                    placedBlockLocations[item].markedForDeath = true;
                }
                placedBlockLocations[directionToCheck].nextToBreak = true;
                nextBlocksToBreak.Add(directionToCheck);
            }
        }
        Instantiate(blockBreakPrefab, new Vector3(_location.x, _location.y), Quaternion.identity);
        placedBlockLocations[_location].DestroyBlock();
        placedBlockLocations.Remove(_location);
        return true;

    }

    public void FixBlocks(Vector2Int _location)
    {
        List<Vector2Int> _startingBlocks = ReturnNeighbourBlocks(_location);
        foreach (var directionToCheck in _startingBlocks)
        {
            if(placedBlockLocations[directionToCheck].markedForDeath == false)
            {
                continue;
            }

            List<Vector2Int> _checkedBlocks = new List<Vector2Int>();
            _checkedBlocks.Add(_location);
            List<Vector2Int> _blocksToCheck = new List<Vector2Int>();
            _blocksToCheck.Add(directionToCheck);
            while (_blocksToCheck.Count > 0)
            {
                List<Vector2Int> _tempBlocks = new List<Vector2Int>();
                foreach (var item in ReturnNeighbourBlocks(placedBlockLocations[_blocksToCheck[_blocksToCheck.Count - 1]].location))
                {
                    if (_checkedBlocks.Contains(item))
                    {
                        continue;
                    }
                    else
                    {
                        Vector2Int location = item;
                        _tempBlocks.Add(item);
                    }
                }

                _checkedBlocks.Add(_blocksToCheck[_blocksToCheck.Count - 1]);
                _blocksToCheck.RemoveAt(_blocksToCheck.Count - 1);

                foreach (var item in _tempBlocks)
                {
                    _blocksToCheck.Add(item);
                }
            }
            foreach (var item in _checkedBlocks)
            {
                placedBlockLocations[item].markedForDeath = false;
                placedBlockLocations[item].nextToBreak = false;
                if (nextBlocksToBreak.Contains(item))
                {
                    nextBlocksToBreak.Remove(item);
                }
            }
        }
    }

    private void BreakBlocks()
    {
        List<Vector2Int> _tempList = new List<Vector2Int>();
        for (int i = nextBlocksToBreak.Count - 1; i >= 0 ; i--)
        {
            _tempList.AddRange(ReturnNeighbourBlocks(nextBlocksToBreak[i]));
            if (placedBlockLocations.ContainsKey(nextBlocksToBreak[i]))
            {
                Instantiate(blockBreakPrefab, new Vector3(nextBlocksToBreak[i].x, nextBlocksToBreak[i].y), Quaternion.identity);
                placedBlockLocations[nextBlocksToBreak[i]].DestroyBlock();
                placedBlockLocations.Remove(nextBlocksToBreak[i]);
            }
            nextBlocksToBreak.RemoveAt(i);

        }

        foreach (var item in _tempList)
        {
            if (placedBlockLocations.ContainsKey(item))
            {
                placedBlockLocations[item].nextToBreak = true;
                nextBlocksToBreak.Add(item);
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

    public List<Vector2Int> ReturnNeighbourBlocks(Vector2Int _blockLocation)
    {
        List<Vector2Int> _tempList = new List<Vector2Int>();
        if(CheckBlockInDirection(_blockLocation, Vector2Int.up)) { _tempList.Add(placedBlockLocations[_blockLocation + Vector2Int.up].location); }
        if(CheckBlockInDirection(_blockLocation, Vector2Int.down)) {  _tempList.Add(placedBlockLocations[_blockLocation + Vector2Int.down].location);}
        if(CheckBlockInDirection(_blockLocation, Vector2Int.left)) {  _tempList.Add(placedBlockLocations[_blockLocation + Vector2Int.left].location);}
        if(CheckBlockInDirection(_blockLocation, Vector2Int.right)) { _tempList.Add(placedBlockLocations[_blockLocation + Vector2Int.right].location);}
        return _tempList;
    }
}
