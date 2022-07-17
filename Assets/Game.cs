using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum tileID
{
    empty,
    goal,
    spawn,
    lazer,
    block,
    levelDie,
    pressurePlate,
    pressurePlatePlayer
}

public struct GameState
{
    public System.ValueTuple<int, int> position;
    public int[] dieLayout; 
    public bool forcedMove;
    public bool died;
    public bool goalActive;

    public GameState(System.ValueTuple<int, int> position, int[] dieLayout, bool forcedMove, bool died, bool goalActive)
    {
        this.position = position;
        this.dieLayout = dieLayout;
        this.forcedMove = forcedMove;
        this.died = died;
        this.goalActive = goalActive;
    }
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

    [SerializeField] private GameObject ConfettiEffect;
    [SerializeField] private AudioSource MoveSound;
    [SerializeField] private AudioSource LevelCompleteSound;
    [SerializeField] private AudioSource GoalEnabledSound;

    [SerializeField] public AudioSource PressurePlateSound;

    public static float transitionDuration = 0.25f;

    public static float normalDuration = 0.25f;
    public static float undoDuration = 0.1f;

    public static System.Action<int> Tick;
    public static System.Action<int> LateTickEvent;
    public static System.Action<bool,bool> LevelCompleteEvent;

    private Stack<GameState> moves = new Stack<GameState>();

    private GameObject[,] tiles;
    private GameObject[,] tilesSimulation;

    private GameObject[,] tileNumbers;
    private GameObject[] players;
    private Dictionary<tileID, GameObject> tileObjects = new Dictionary<tileID, GameObject>();
    private Dictionary<string, GameObject> numberObjects = new Dictionary<string, GameObject>();

    private Player activePlayer;
    public Player ActivePlayer { get { return activePlayer; } }

    public GameObject lockingObject = null;

    public static float tickTimer = 0.0f;
    private Levels levels;


    //50 degrees, 8y, -7.5x
    private float camYOffset = 9.5f;
    private float camZOffset = -6.0f;

    private float levelTransitionDuration = 2.0f;

    private Quaternion cameraStartQuaternion;
    private float cameraNudgeAngle = 1.0f;

    public List<(GameObject, Tile)> moveables = new List<(GameObject, Tile)>();

    private Dictionary<int, Stack<bool>> freezeMoveable = new Dictionary<int, Stack<bool>>();

    private int identifierIndex = 0;

    [SerializeField] private AnimationCurve cameraNudgeCurve;

    private List<int> movedIdentifiers = new List<int>();

    private List<PressurePlate> pressurePlates = new List<PressurePlate>();

    private GameObject goalObject;

    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI diedHintText;

    private bool transitionStarted = false;
    //the model I have is clockwise probably https://en.wikipedia.org/wiki/Dice#Construction

    // Start is called before the first frame update
    void Start()
    {
        LevelCompleteEvent += LevelComplete;
        Player.DieEvent += PlayerDied;
        LateTickEvent += LateTick;

        tileObjects[tileID.empty] = null;
        tileObjects[tileID.lazer] = Resources.Load("Prefabs/lazer") as GameObject;
        tileObjects[tileID.goal] = Resources.Load("Prefabs/goal") as GameObject;
        tileObjects[tileID.block] = Resources.Load("Prefabs/LevelCube") as GameObject;
        tileObjects[tileID.levelDie] = Resources.Load("Prefabs/LevelDie") as GameObject;
        tileObjects[tileID.pressurePlate] = Resources.Load("Prefabs/PressurePlate") as GameObject;
        tileObjects[tileID.pressurePlatePlayer] = Resources.Load("Prefabs/PressurePlatePlayer") as GameObject;

        numberObjects["1"] = Resources.Load("Prefabs/number1") as GameObject;
        numberObjects["2"] = Resources.Load("Prefabs/number2") as GameObject;
        numberObjects["3"] = Resources.Load("Prefabs/number3") as GameObject;
        numberObjects["4"] = Resources.Load("Prefabs/number4") as GameObject;
        numberObjects["5"] = Resources.Load("Prefabs/number5") as GameObject;
        numberObjects["6"] = Resources.Load("Prefabs/number6") as GameObject;
        numberObjects["not1"] = Resources.Load("Prefabs/not1") as GameObject;
        numberObjects["not2"] = Resources.Load("Prefabs/not2") as GameObject;
        numberObjects["not3"] = Resources.Load("Prefabs/not3") as GameObject;
        numberObjects["not4"] = Resources.Load("Prefabs/not4") as GameObject;
        numberObjects["not5"] = Resources.Load("Prefabs/not5") as GameObject;
        numberObjects["not6"] = Resources.Load("Prefabs/not6") as GameObject;

        players = GameObject.FindGameObjectsWithTag("Player");
        activePlayer = players[0].GetComponent<Player>();

        levels = new Levels();
        levels.Init();
        LoadLevel(Manager.currentLevel);

        //set camera position
        var levelCenter = (leveldata.width / 2.0f, leveldata.height / 2.0f);
        camera.transform.position = new Vector3(levelCenter.Item1 - .5f , camYOffset, levelCenter.Item2 + camZOffset);
        cameraStartQuaternion = camera.transform.rotation;
        ConfettiEffect.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + 5.0f);
        //add first gamestate
        GameState gs = new GameState(activePlayer.Pos, activePlayer.GetDie(), false, false,goalObject.activeSelf);
        moves.Push(gs);
        levelNameText.text = "- " + leveldata.name + " -";
    }

    private void OnDestroy()
    {
        LevelCompleteEvent -= LevelComplete;
        Player.DieEvent -= PlayerDied;
        LateTickEvent -= LateTick;
    }

    private void LoadLevel(int levelIndex)
    {
        GameObject parent = new GameObject("Level");
        parent.transform.position = new Vector3();
        leveldata = levels.GetLevel(levelIndex);
        //init arrays 
        tiles = new GameObject[leveldata.width, leveldata.height];
        tilesSimulation = new GameObject[leveldata.width, leveldata.height];
        tileNumbers = new GameObject[leveldata.width, leveldata.height];
        //Build level tiles
        BuildLevelTiles(parent);
        //build walls
        BuildWalls(parent);

        if(pressurePlates.Count > 0)
        {
            goalObject.SetActive(false);
        }
    }

    private void BuildWalls(GameObject parent)
    {
        for(int x = -1; x <= leveldata.width; x++)
        {
            var obj = Instantiate(wall);
            obj.transform.position = new Vector3(x, 0.5f, leveldata.height);
            obj.transform.parent = parent.transform;
        }
        for(int y = 0; y < leveldata.height; y++)
        {
            var obj1 = Instantiate(wall);
            var obj2 = Instantiate(wall);

            obj1.transform.position = new Vector3(-1, 0.5f, y);
            obj1.transform.parent = parent.transform;

            obj2.transform.position = new Vector3(leveldata.width, 0.5f, y);
            obj2.transform.parent = parent.transform;
        }
    }

    private void BuildLevelTiles(GameObject parent)
    {
        for(int x = 0; x < leveldata.width; x++)
        {
            for(int y = 0; y < leveldata.height; y++)
            {
                var obj = Instantiate(baseTile);
                obj.transform.position = new Vector3(x, 0, y);
                obj.transform.parent = parent.transform;
                var thisTileData = leveldata.data[x, y];
                if(thisTileData.id == tileID.spawn)
                {
                    activePlayer.Pos = (x, y);
                    activePlayer.transform.position = new Vector3(x, 1, y);
                }
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

                        if(tileScript is PressurePlate)
                        {
                            pressurePlates.Add(tileScript as PressurePlate);
                        }
                        else if(tileScript is Goal)
                        {
                            goalObject = tile;
                        }

                        tileScript.Direction = thisTileData.direction;
                        tileScript.ActivateNumber = thisTileData.activateNumber;
                        tileScript.Position = (x, y);
                        tileScript.Even = thisTileData.even;
                        tileScript.Odd = thisTileData.odd;
                        tileScript.Less = thisTileData.less;
                        tileScript.Greater = thisTileData.greater;
                        tileScript.Not = thisTileData.not;
                        tileScript.Identifier = identifierIndex;
                        identifierIndex++;
                        if(tileScript.moves) 
                        { 
                            moveables.Add((tile, tileScript));
                            freezeMoveable[tileScript.Identifier] = new Stack<bool>();
                        }

                        //add number too
                        if(thisTileData.activateNumber < 7)
                        {
                            StringBuilder sb = new StringBuilder();
                            if(thisTileData.even) sb.Append("even");
                            else if(thisTileData.odd) sb.Append("odd");
                            else if(thisTileData.less) sb.Append("less");
                            else if(thisTileData.greater) sb.Append("even");
                            else if(thisTileData.not) sb.Append("not");

                            if(thisTileData.activateNumber > -1) sb.Append(thisTileData.activateNumber.ToString());

                            if(numberObjects.ContainsKey(sb.ToString()))
                            {
                                var number = Instantiate(numberObjects[sb.ToString()]);
                                number.transform.position = new Vector3(x, Y, y);
                                number.transform.rotation = Quaternion.Euler(90, 0, 0);
                                tileNumbers[x, y] = number;
                            }
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(transition)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.R) && moves.Count > 1)
        {
            //undo
            var gs = moves.Pop();
            if(gs.died) 
            { 
                this.activePlayer.Dead = false;
                diedHintText.gameObject.SetActive(false);
            }
            goalObject.SetActive(moves.Peek().goalActive);
            //move
            lockingObject = this.gameObject;
            var offset = (moves.Peek().position.Item1 - gs.position.Item1,moves.Peek().position.Item2 - gs.position.Item2);
            var dir = Dir.dirTuple[offset];
            SimulateMovement(dir, true); //hopefully works right
            transitionDuration = undoDuration;
            StartCoroutine(Transition(dir, true));
        }

        if(lockingObject != null)
        {
            return;
        }

        //debug controls
        if(Input.GetKeyDown(KeyCode.Equals))
        {
            LevelCompleteEvent.Invoke(true,true);
        }
        else if(Input.GetKeyDown(KeyCode.Minus))
        {
            LevelCompleteEvent.Invoke(false,true);
        }
        else if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            RestartLevel();
        }

        if(!activePlayer.Dead)
        { 
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                SimulateMovement(Direction.up, false);
                var newPos = Dir.Add(activePlayer.Pos, Direction.up);
                if(!CheckCollision(newPos, ref tilesSimulation))
                {
                    StartCoroutine(Transition(Direction.up, false));
                }
                return;
            }
            else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SimulateMovement(Direction.left, false);
                var newPos = Dir.Add(activePlayer.Pos, Direction.left);
                if(!CheckCollision(newPos,ref tilesSimulation))
                {
                    StartCoroutine(Transition(Direction.left, false));
                }
                return;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                SimulateMovement(Direction.down, false);
                var newPos = Dir.Add(activePlayer.Pos, Direction.down);
                if(!CheckCollision(newPos, ref tilesSimulation))
                {
                    StartCoroutine(Transition(Direction.down, false));
                }
                return;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SimulateMovement(Direction.right, false);
                var newPos = Dir.Add(activePlayer.Pos, Direction.right);
                if(!CheckCollision(newPos, ref tilesSimulation))
                {
                    StartCoroutine(Transition(Direction.right, false));
                }
                return;
            }
        }
    }

    private void SimulateMovement(Direction dir, bool undo)
    {
        movedIdentifiers.Clear();

        int uBound0 = tilesSimulation.GetUpperBound(0);
        int uBound1 = tilesSimulation.GetUpperBound(1);

        for(int i = 0; i <= uBound0; i++)
        {
            for(int j = 0; j <= uBound1; j++)
            {
                tilesSimulation[i, j] = tiles[i, j];
            }
        }

        switch(dir)
        {
            case Direction.none:
                Debug.LogError("invalid direction @ Game.Simulatemovement");
                return;
            case Direction.up:
                //nothing at height-1 should move up
                for(int y = leveldata.height - 2; y >-1 ; y--)
                {
                    for(int x = 0; x < leveldata.width; x++)
                    {
                        if(tiles[x,y] != null)
                        {
                            var scriptData = tiles[x, y].GetComponent<Tile>();

                            if(scriptData.moves && undo && freezeMoveable[scriptData.Identifier].Peek()) continue;

                            if(scriptData.moves && !CheckCollision((x,y+1), ref tilesSimulation))
                            {
                                tilesSimulation[x, y + 1] = tiles[x, y];
                                tilesSimulation[x, y] = null;
                                movedIdentifiers.Add(scriptData.Identifier);
                            }
                        }
                    }
                }
                break;
            case Direction.right:
                for(int x = leveldata.width - 2; x > -1; x--)
                {
                    for(int y = 0; y < leveldata.height; y++)
                    {
                        if(tiles[x, y] != null)
                        {
                            var scriptData = tiles[x, y].GetComponent<Tile>();

                            if(scriptData.moves && undo && freezeMoveable[scriptData.Identifier].Peek()) continue;

                            if(scriptData.moves && !CheckCollision((x + 1, y), ref tilesSimulation))
                            {
                                tilesSimulation[x + 1, y] = tiles[x, y];
                                tilesSimulation[x, y] = null;
                                movedIdentifiers.Add(scriptData.Identifier);
                            }
                        }
                    }
                }
                break;
            case Direction.down:
                for(int y = 1; y < leveldata.height; y++)
                {
                    for(int x = 0; x < leveldata.width; x++)
                    {
                        if(tiles[x, y] != null)
                        {
                            var scriptData = tiles[x, y].GetComponent<Tile>();

                            if(scriptData.moves && undo && freezeMoveable[scriptData.Identifier].Peek()) continue;

                            if(scriptData.moves && !CheckCollision((x, y - 1), ref tilesSimulation))
                            {
                                tilesSimulation[x, y - 1] = tiles[x, y];
                                tilesSimulation[x, y] = null;
                                movedIdentifiers.Add(scriptData.Identifier);
                            }
                        }
                    }
                }
                break;
            case Direction.left:
                for(int x = 1; x < leveldata.width; x++)
                {
                    for(int y = 0; y < leveldata.height; y++)
                    {
                        if(tiles[x, y] != null)
                        {
                            var scriptData = tiles[x, y].GetComponent<Tile>();

                            if(scriptData.moves && undo && freezeMoveable[scriptData.Identifier].Peek()) continue;

                            if(scriptData.moves && !CheckCollision((x - 1, y), ref tilesSimulation))
                            {
                                tilesSimulation[x - 1, y] = tiles[x, y];
                                tilesSimulation[x, y] = null;
                                movedIdentifiers.Add(scriptData.Identifier);
                            }
                        }
                    }
                }
                break;
        }
    }

    private IEnumerator Transition(Direction dir, bool undo)
    {
        transition = true;
        float y = 1;
        //float y = curTile == null ? 1 : 1 + curTile.GetComponent<Tile>().VerticalOffset; //unused

        (int, int) newPos = Dir.Add(activePlayer.Pos, dir);
        GameObject curTile = tiles[newPos.Item1, newPos.Item2];

        Vector3 lerpStart = activePlayer.transform.position;
        Vector3 lerpTarget = new Vector3(newPos.Item1, y, newPos.Item2);
        Quaternion startRotation = activePlayer.transform.rotation;
        Quaternion targetRotation = startRotation; // placeholder value / default
        
        Quaternion cameraNudgedRotation = cameraStartQuaternion;

        List<(int, int)> moveableNewPositions = new List<(int, int)>();
        List<Vector3> moveableLerpStart = new List<Vector3>();
        List<Vector3> moveableLerpTarget = new List<Vector3>();
        List<Quaternion> moveableStartRotation = new List<Quaternion>();
        List<Quaternion> moveableTargetRotation = new List<Quaternion>();
        List<int> moveablesUsedIndices = new List<int>();

        List<GameObject> moved = new List<GameObject>();

        int index = 0;
        for(int j = 0; j < moveables.Count; j++)
        {
            if(!movedIdentifiers.Contains(moveables[j].Item2.Identifier))
            {
                moved.Add(null);
                if(!undo)
                {
                    freezeMoveable[moveables[j].Item2.Identifier].Push(true);
                }
            }
            else 
            {
                if(!undo)
                {
                    freezeMoveable[moveables[j].Item2.Identifier].Push(false);
                }
                moveableNewPositions.Add(Dir.Add(moveables[j].Item2.Position, dir));
                moveableLerpStart.Add(moveables[j].Item1.transform.position);
                moveableLerpTarget.Add(new Vector3(moveableNewPositions[index].Item1, y, moveableNewPositions[index].Item2));
                moveableStartRotation.Add(moveables[j].Item1.transform.rotation);
                moveableTargetRotation.Add(moveableStartRotation[index]);  // placeholder value / default
                moved.Add(moveables[j].Item1);
                (moveables[j].Item2 as LevelDie).RollDie(dir); //todo if more moveable types exist this has to change-W
                index++;
            }
        }

        switch(dir)
        {
            case Direction.none:
                break;
            case Direction.up:
                targetRotation = Quaternion.Euler(Vector3.right * 90) * targetRotation;
                for(int i = 0; i < moveableTargetRotation.Count; i++)
                {
                    moveableTargetRotation[i] = Quaternion.Euler(Vector3.right * 90) * moveableTargetRotation[i];
                }
                cameraNudgedRotation = Quaternion.Euler(Vector3.right * -cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.right:
                targetRotation = Quaternion.Euler(Vector3.forward * -90) * targetRotation;
                for(int i = 0; i < moveableTargetRotation.Count; i++)
                {
                    moveableTargetRotation[i] = Quaternion.Euler(Vector3.forward * -90) * moveableTargetRotation[i];
                }
                cameraNudgedRotation = Quaternion.Euler(Vector3.forward * cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.down:
                targetRotation = Quaternion.Euler(Vector3.right * -90) * targetRotation;
                for(int i = 0; i < moveableTargetRotation.Count; i++)
                {
                    moveableTargetRotation[i] = Quaternion.Euler(Vector3.right * -90) * moveableTargetRotation[i];
                }
                cameraNudgedRotation = Quaternion.Euler(Vector3.right * cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.left:
                targetRotation = Quaternion.Euler(Vector3.forward * 90) * targetRotation;
                for(int i = 0; i < moveableTargetRotation.Count; i++)
                {
                    moveableTargetRotation[i] = Quaternion.Euler(Vector3.forward * 90) * moveableTargetRotation[i];
                }
                cameraNudgedRotation = Quaternion.Euler(Vector3.forward * -cameraNudgeAngle) * cameraStartQuaternion;
                break;
            default:
                break;
        }

        tickTimer = 0.0f;

        activePlayer.RollDie(dir);

        activePlayer.Pos = newPos;
        //set moveables new positions
        int z = 0;
        for(int i = 0; i < moved.Count; i++)
        {
            if(moved[i] == null) continue;
            moved[i].GetComponent<Tile>().Position = moveableNewPositions[z];
            z++;
        }

        //copy simulated movements into tile to finalize
        int uBound0 = tiles.GetUpperBound(0);
        int uBound1 = tiles.GetUpperBound(1);
        for(int i = 0; i <= uBound0; i++)
        {
            for(int j = 0; j <= uBound1; j++)
            {
                tiles[i, j] = tilesSimulation[i, j];
            }
        }


        Tick.Invoke(activePlayer.Face);

        if(pressurePlates.Count > 0 && goalObject.activeSelf == false)
        {
            bool allOn = true;

            foreach(var p in pressurePlates)
            {
                if(!p.On)
                {
                    allOn = false;
                }
            }
            if(allOn)
            {
                GoalEnabledSound.Play();
                goalObject.SetActive(true);
            }
        }

        while(tickTimer < transitionDuration)
        {
            tickTimer += Time.deltaTime;
            activePlayer.transform.position = Vector3.Lerp(lerpStart, lerpTarget, tickTimer / transitionDuration);
            activePlayer.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, tickTimer / transitionDuration);

            int k = 0;
            for(int j = 0; j < moved.Count; j++)
            {
                if(moved[j] == null)
                {
                    continue;
                }

                moved[j].transform.position = Vector3.Lerp(moveableLerpStart[k], moveableLerpTarget[k], tickTimer / transitionDuration);
                moved[j].transform.rotation = Quaternion.Lerp(moveableStartRotation[k], moveableTargetRotation[k], tickTimer/ transitionDuration);
                k++;
            }

            camera.transform.rotation = Quaternion.Lerp(cameraNudgedRotation, cameraStartQuaternion, cameraNudgeCurve.Evaluate(tickTimer / transitionDuration));
            yield return null;
        }

        if(undo)
        {
            for(int i = 0; i < moveables.Count; i++)
            {
                freezeMoveable[moveables[i].Item2.Identifier].Pop();
            }
            transitionDuration = normalDuration;
        }

        //todo change if forced movement added
        if(!undo)
        { 
            GameState gs = new GameState(activePlayer.Pos, activePlayer.GetDie(), false, false,goalObject.activeSelf);
            moves.Push(gs);
        }
        else
        {
            lockingObject = null;
        }
        //todo fix?
        transition = false;
        LateTickEvent.Invoke(activePlayer.Face);
        camera.transform.rotation = cameraStartQuaternion;

    }

    private void LateTick(int dieFace)
    {
        float dampenValue = 0.6f;
        float startVolume = 0.5f;
        var stereoValue = ((activePlayer.Pos.Item1 / ((float)leveldata.width-1.0f)) - 0.5f) * dampenValue;
        var newVolume = startVolume - (0.3f * (activePlayer.Pos.Item2 / ((float) leveldata.height-1.0f)));
        MoveSound.panStereo = stereoValue;
        MoveSound.volume = newVolume;
        MoveSound.Play();
    }

    //returns true if collision or OOB, else false
    private bool CheckCollision((int, int) pos, ref GameObject[,] dataSource)
    {
        if(pos.Item1 < 0 || pos.Item2 < 0 ||
            pos.Item1 > leveldata.width - 1 || pos.Item2 > leveldata.height - 1)
        {
            return true;
        }

        var curTile = dataSource[pos.Item1, pos.Item2]?.GetComponent<Tile>();

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
                return (position.Item1, -1);
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
                return (-1, position.Item2);
        }

        return (-1, -1);
    }

    private void PlayerDied()
    {
        var gs = moves.Pop();
        gs.died = true;
        moves.Push(gs);

        diedHintText.gameObject.SetActive(true);
    }


    private void RestartLevel()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    private void LevelComplete(bool incrementLevel, bool fast = false)
    {
        if(transitionStarted) return;

        ConfettiEffect.SetActive(true);
        if(incrementLevel)
        {
            Manager.currentLevel++;
        }
        else
        {
            Manager.currentLevel--;
            if(Manager.currentLevel < 0) Manager.currentLevel = 0;
        }
        
        if(Manager.currentLevel >= levels.LevelCount)
        {
            StartCoroutine(LevelTransition(1, fast));
            LevelCompleteSound.Play();
            transitionStarted = true;
            return;
        }
        StartCoroutine(LevelTransition(0, fast));
        LevelCompleteSound.Play();
        transitionStarted = true;
    }
    private IEnumerator LevelTransition(int sceneIndex, bool fast = false)
    {
        float levelTransitionTimer = 0.0f;
        if(fast)
        {
            SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
        }
        else
        { 
            while(levelTransitionTimer < levelTransitionDuration)
            {
                levelTransitionTimer += Time.deltaTime;
                yield return null;
            }
            SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
        }
        
    }

}