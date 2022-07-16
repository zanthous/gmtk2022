using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum tileID
{
    empty,
    goal,
    spawn,
    lazer
}

public struct GameState
{
    System.Tuple<int, int> position;
    int[] dieLayout; 
}

public class Game : MonoBehaviour
{
    private const float Y = 0.53f;
    Leveldata leveldata;

    private bool transition = false;

    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject camera;
    //[SerializeField] private GameObject player;

    [SerializeField] private GameObject number1;
    [SerializeField] private GameObject number2;
    [SerializeField] private GameObject number3;
    [SerializeField] private GameObject number4;
    [SerializeField] private GameObject number5;
    [SerializeField] private GameObject number6;

    private float transitionDuration = 0.25f;

    public static System.Action<int> Tick;
    public static System.Action<int> LateTick;

    private Stack<GameState> moves;
    private GameObject[,] tiles;
    private GameObject[,] tileNumbers;
    private GameObject[] players;
    private Dictionary<tileID, GameObject> tileObjects = new Dictionary<tileID, GameObject>();
    private Dictionary<int, GameObject> numberObjects = new Dictionary<int, GameObject>();

    private Player activePlayer;
    private GameObject lockingObject = null;


    //the model I have is clockwise probably https://en.wikipedia.org/wiki/Dice#Construction

    // Start is called before the first frame update
    void Start()
    {
        tileObjects[tileID.empty] = null;
        tileObjects[tileID.lazer] = Resources.Load("Prefabs/lazer") as GameObject;
        tileObjects[tileID.goal] = Resources.Load("Prefabs/goal") as GameObject;

        numberObjects[1] = Resources.Load("Prefabs/number1") as GameObject;
        numberObjects[2] = Resources.Load("Prefabs/number2") as GameObject;
        numberObjects[3] = Resources.Load("Prefabs/number3") as GameObject;
        numberObjects[4] = Resources.Load("Prefabs/number4") as GameObject;
        numberObjects[5] = Resources.Load("Prefabs/number5") as GameObject;
        numberObjects[6] = Resources.Load("Prefabs/number6") as GameObject;


        players = GameObject.FindGameObjectsWithTag("Player");
        activePlayer = players[0].GetComponent<Player>();

        //debug leveldata / in the future load
        leveldata.width = 10;
        leveldata.height = 10;
        leveldata.name = "cool level";
        leveldata.data = new Tiledata[leveldata.width, leveldata.height];

        //init arrays 
        tiles = new GameObject[leveldata.width, leveldata.height];
        tileNumbers = new GameObject[leveldata.width, leveldata.height];

        GameObject parent = new GameObject("Level");
        parent.transform.position = new Vector3();

        //fill test data
        for(int x = 0; x < leveldata.width; x++)
        {
            for(int y = 0; y < leveldata.height; y++)
            {
                leveldata.data[x, y] = new Tiledata((int) tileID.empty, Direction.none, -1);
            }
        }
        leveldata.data[0, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        activePlayer.Pos = (0, 0);

        leveldata.data[0, leveldata.height - 1] = new Tiledata(tileID.lazer, Direction.right, 2);
        leveldata.data[leveldata.width - 1, leveldata.height - 2] = new Tiledata(tileID.lazer, Direction.left, 2);
        leveldata.data[leveldata.width - 1, 0] = new Tiledata(tileID.lazer, Direction.up, 2);
        leveldata.data[4, leveldata.height - 1] = new Tiledata(tileID.lazer, Direction.down, 2);

        leveldata.data[leveldata.width - 1, leveldata.height - 1] = new Tiledata(tileID.goal, Direction.none, -1);

        //Build level tiles
        for(int x = 0; x < leveldata.width; x++)
        {
            for(int y = 0; y < leveldata.height; y++)
            {
                var obj = Instantiate(baseTile);
                obj.transform.position = new Vector3(x, 0, y);
                obj.transform.parent = parent.transform;
                var thisTileData = leveldata.data[x, y];
                if(tileObjects.ContainsKey(thisTileData.id))
                { 
                    var tileObject = tileObjects[thisTileData.id];
                    if(tileObject != null)
                    {
                        var tile = Instantiate(tileObject);
                        tile.transform.position = new Vector3(x, 1, y);
                        float rotation = 0.0f;
                        switch(thisTileData.direction)
                        {
                            case Direction.up:
                                rotation = 0.0f; 
                                break;
                            case Direction.right:
                                rotation = 90.0f;
                                break;
                            case Direction.down:
                                rotation = 180.0f;
                                break;
                            case Direction.left:
                                rotation = 270.0f;
                                break;
                            default:
                                break;
                        }
                        tile.transform.rotation = Quaternion.Euler(0, rotation, 0);

                        this.tiles[x, y] = tile;
                        //fill script tiledata from leveldata
                        var tileScript = tile.GetComponent<Tile>();
                        tileScript.Direction = thisTileData.direction;
                        tileScript.ActivateNumber = thisTileData.activateNumber;
                        tileScript.Position = (x, y);
                        //add number too
                        if(thisTileData.activateNumber > 0 && thisTileData.activateNumber < 7)
                        { 
                            var number = Instantiate(numberObjects[thisTileData.activateNumber]);
                            number.transform.position = new Vector3(x, Y, y);
                            number.transform.rotation = Quaternion.Euler(90, 0, 0);
                            tileNumbers[x, y] = number;
                        }
                    }
                }
            }
        }
        //build walls
        for(int x = -1; x <= leveldata.width; x++)
        {
            var obj = Instantiate(wall);
            obj.transform.position = new Vector3(x, 0.5f, leveldata.height);
            obj.transform.parent = parent.transform;    
        }
        for(int y = 0; y < leveldata.width; y++)
        {
            var obj1 = Instantiate(wall);
            var obj2 = Instantiate(wall);

            obj1.transform.position = new Vector3(-1, 0.5f, y);
            obj1.transform.parent = parent.transform;

            obj2.transform.position = new Vector3(leveldata.width, 0.5f, y);
            obj2.transform.parent = parent.transform;
        }

        //camera.transform.position = new Vector3((leveldata.width / 2.0f) + .5f, camera.transform.position.y, (leveldata.height / 2.0f) + .5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(transition || lockingObject != null)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            var newPos = Dir.Add(activePlayer.Pos, Direction.up);
            if(!CheckCollision(newPos))
            {
                StartCoroutine(Transition(Direction.up));
            }
            return;
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            var newPos = Dir.Add(activePlayer.Pos, Direction.left);
            if(!CheckCollision(newPos))
            {
                StartCoroutine(Transition(Direction.left));
            }
            return;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            var newPos = Dir.Add(activePlayer.Pos, Direction.down);
            if(!CheckCollision(newPos))
            {
                StartCoroutine(Transition(Direction.down));
            }
            return;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            var newPos = Dir.Add(activePlayer.Pos, Direction.right);
            if(!CheckCollision(newPos))
            {
                StartCoroutine(Transition(Direction.right));
            }
            return;
        }
    }

    private IEnumerator Transition(Direction dir)
    {
        transition = true;
        var newPos = Dir.Add(activePlayer.Pos, dir);
        var curTile = tiles[newPos.Item1, newPos.Item2];
        float y = curTile == null ? 1 : 1 + curTile.GetComponent<Tile>().VerticalOffset;
        Vector3 lerpStart = activePlayer.transform.position;
        Vector3 lerpTarget = new Vector3(newPos.Item1, y, newPos.Item2);
        Quaternion startRotation = activePlayer.transform.rotation;

        Quaternion targetRotation = startRotation;

        switch(dir)
        {
            case Direction.none:
                break;
            case Direction.up:
                targetRotation = Quaternion.Euler(Vector3.right * 90) * targetRotation;
                break;
            case Direction.right:
                targetRotation = Quaternion.Euler(Vector3.forward * -90) * targetRotation;
                break;
            case Direction.down:
                targetRotation = Quaternion.Euler(Vector3.right * -90)  * targetRotation;
                break;
            case Direction.left:
                targetRotation = Quaternion.Euler(Vector3.forward * 90) * targetRotation;
                break;
            default:
                break;
        }
        float timer = 0.0f;

        activePlayer.RollDie(dir);
        Tick.Invoke(activePlayer.Face);

        while(timer < transitionDuration)
        {
            timer += Time.deltaTime;
            activePlayer.transform.position = Vector3.Lerp(lerpStart, lerpTarget, timer/transitionDuration);
            activePlayer.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timer/transitionDuration);
            yield return null;
        }

        activePlayer.Pos = newPos;
        //todo fix
        transition = false;
        //latetick.invoke()

    }

    //returns true if collision or OOB, else false
    private bool CheckCollision((int, int) pos)
    {
        if(pos.Item1 < 0 || pos.Item2 < 0 ||
            pos.Item1 > leveldata.width - 1 || pos.Item2 > leveldata.height - 1)
        {
            return true;
        }
        var curTile = tiles[pos.Item1, pos.Item2]?.GetComponent<Tile>();

        if(curTile == null)
        {
            return false;
        }
        else
        {
            return curTile.collision;
        }
    }

    public (int,int) GetCollision((int,int) position, Direction direction)
    {
        switch(direction)
        {
            case Direction.none:
                Debug.LogError("GetCollision was passed an invalid direction");
                return (-1, -1);
            case Direction.up:
                for(int y = position.Item2+1; y < leveldata.height; y++)
                {
                    var curTile = tiles[position.Item1, y];
                    if(curTile == null) continue;
                    if(curTile.GetComponent<Tile>().collision)
                    {
                        return (position.Item1, y);
                    }
                }
                return (position.Item1, leveldata.height);
            case Direction.right:
                for(int x = position.Item1+1; x < leveldata.width; x++)
                {
                    var curTile = tiles[x, position.Item2];
                    if(curTile == null) continue;
                    if(curTile.GetComponent<Tile>().collision)
                    {
                        return (x, position.Item2);
                    }
                }
                return (leveldata.width, position.Item2);
            case Direction.down:
                for(int y = position.Item2-1; y > -1; y--)
                {
                    var curTile = tiles[position.Item1, y];
                    if(curTile == null) continue;
                    if(curTile.GetComponent<Tile>().collision)
                    {
                        return (position.Item1, y);
                    }
                }
                return (position.Item1, 0);
            case Direction.left:
                for(int x = position.Item1-1; x > -1; x--) 
                {
                    var curTile = tiles[x, position.Item2];
                    if(curTile == null) continue;
                    if(curTile.GetComponent<Tile>().collision)
                    {
                        return (x, position.Item2);
                    }
                }
                return (0, position.Item2);
        }

        return (-1, -1);
    }
}