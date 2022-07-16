using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum tileID
{
    empty,
    goal,
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

        leveldata.data[0, leveldata.width - 1] = new Tiledata(tileID.lazer, Direction.right, 2);
        leveldata.data[leveldata.height - 1, leveldata.width - 1] = new Tiledata(tileID.goal, Direction.none, -1);

        //Build level tiles
        for(int x = 0; x < leveldata.width; x++)
        {
            for(int y = 0; y < leveldata.height; y++)
            {
                var obj = Instantiate(baseTile);
                obj.transform.position = new Vector3(x, 0, y);
                obj.transform.parent = parent.transform;
                var thisTileData = leveldata.data[x, y];
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
                    var tileScript = tile.GetComponent<Tile>();
                    tileScript.Direction = thisTileData.direction;
                    tileScript.ActivateNumber = thisTileData.activateNumber;
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
        var newPos = Dir.Add(activePlayer.Pos, dir);
        float y = tiles[newPos.Item1, newPos.Item2].GetComponent<Tile>().VerticalOffset;
        Vector3 lerpTarget = new Vector3(newPos.Item1, y, newPos.Item2);
        Quaternion startRotation = activePlayer.transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(Dir.Rotation90(dir)); //this might be wrong
        float timer = 0.0f;
        while(timer < transitionDuration)
        {


            yield return null;
        }
        
    }

    //returns true if collision or OOB, else false
    private bool CheckCollision((int, int) pos)
    {
        if(pos.Item1 < 0 || pos.Item2 < 0 ||
            pos.Item1 > leveldata.width - 1 || pos.Item2 > leveldata.height - 1)
        {
            return true;
        }
        var curTile = tiles[pos.Item1, pos.Item2].GetComponent<Tile>();

        if(curTile == null)
        {
            return false;
        }
        else
        {
            return curTile.collision;
        }
    }
}