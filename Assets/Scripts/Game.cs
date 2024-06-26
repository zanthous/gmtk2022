using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum EntityID
{
    None                = 0,
    empty               = 1 << 0,
    goal                = 1 << 1,
    spawn               = 1 << 2,
    lazer               = 1 << 3,
    block               = 1 << 4,
    levelDie            = 1 << 5,
    pressurePlate       = 1 << 6,
    pressurePlatePlayer = 1 << 7,
    player              = 1 << 8,
    hole                = 1 << 9
}

public struct GameState
{
    public (int, int) position;
    public int[] dieLayout; 
    public bool forcedMove;
    public bool died;
    public bool goalActive;

    public GameState((int, int) position, int[] dieLayout, bool forcedMove, bool died, bool goalActive)
    {
        this.position = position;
        this.dieLayout = dieLayout;
        this.forcedMove = forcedMove;
        this.died = died;
        this.goalActive = goalActive;
    }
}

public struct Transaction
{
    public int entityID;
    public (int, int) move;
    public (int, int) newPos;
}

public class Game : MonoBehaviour
{
    public static System.Action<int, Direction> TickEvent;
    public static System.Action<int> LateTickEvent;
    public static System.Action UndoEvent;
    public static System.Action<bool, bool> LevelCompleteEvent;

    public const float normalDuration = 0.15f;
    public const float undoDuration = 0.1f;
    public static float transitionDuration = 0.15f;
    public static float tickTimer = 0.0f;
    
    public Dictionary<int, Transaction> transactions = new Dictionary<int, Transaction>();
    public List<IMoveable> moveablesNew = new List<IMoveable>();
    public Player ActivePlayer { get { return activePlayer; } }
    public GameObject lockingObject = null;

    [SerializeField] private GameObject postProcessVolume;
    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject wall;
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject ConfettiEffect;
    [SerializeField] private AnimationCurve cameraNudgeCurve;
    [SerializeField] private AudioSource MoveSound;
    [SerializeField] private AudioSource LevelCompleteSound;
    [SerializeField] private AudioSource GoalEnabledSound;
    [SerializeField] public AudioSource PressurePlateSound;
    [SerializeField] float camYOffset = 3.5f;
    [SerializeField] float camZOffset = 3.5f;
    [SerializeField] float camXOffset = 3.5f;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] public TextMeshProUGUI diedHintText;
    
    private const float numberYOffset = 0.53f;
    private const float cameraDefaultSize = 5.0f;

    private bool inTransition = false;
    private bool[,] meshVisited;
    private bool cameraNudgeEnabled = false;
    private bool transitionStarted = false; 
    private Dictionary<EntityID, GameObject> tileObjects = new Dictionary<EntityID, GameObject>();
    private Dictionary<string, GameObject> numberObjects = new Dictionary<string, GameObject>();
    private Dictionary<int, Entity> entitiesDict = new Dictionary<int, Entity>(); private Leveldata leveldata;
    private Entity[,] entities;
    private Entity[,] entitiesSimulation;
    private float levelBlockHeight = -24.5f;
    private float cameraLevelOffset = 0.0f;
    private float levelTransitionDuration = 2.0f;
    private float cameraNudgeAngle = 1.0f;
    private GameObject[,] tileNumbers;
    private GameObject[] players;
    private GameObject goalObject;
    private GameObject goalObjectNumber;
    private int tickNumber = 0;
    private int identifierIndex = 1;
    private Levels levels;
    private List<int> movedIdentifiers = new List<int>();
    private List<PressurePlate> pressurePlates = new List<PressurePlate>();
    private Player activePlayer;
    //each level can have a specific camera offset if wanted
    private Quaternion cameraStartQuaternion;
    
    //the model I have is clockwise https://en.wikipedia.org/wiki/Dice#Construction

    void Awake()
    {
        LevelCompleteEvent += LevelComplete;
        Player.DieEvent += PlayerDied;
        LateTickEvent += LateTick;

        tileObjects[EntityID.empty] = null;
        tileObjects[EntityID.lazer] = Resources.Load("Prefabs/lazer") as GameObject;
        tileObjects[EntityID.goal] = Resources.Load("Prefabs/goal") as GameObject;
        tileObjects[EntityID.block] = Resources.Load("Prefabs/LevelCube") as GameObject;
        tileObjects[EntityID.levelDie] = Resources.Load("Prefabs/LevelDie") as GameObject;
        tileObjects[EntityID.pressurePlate] = Resources.Load("Prefabs/PressurePlate") as GameObject;
        tileObjects[EntityID.pressurePlatePlayer] = Resources.Load("Prefabs/PressurePlatePlayer") as GameObject;
        tileObjects[EntityID.hole] = Resources.Load("Prefabs/hole") as GameObject;

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
        SetCameraPosition();

        cameraStartQuaternion = camera.transform.rotation;
        ConfettiEffect.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + 5.0f);
        levelNameText.text = "- " + leveldata.name + " -";
        entities[activePlayer.Pos.Item1, activePlayer.Pos.Item2] = activePlayer.gameObject.GetComponent<Entity>();
        entitiesDict.Add(activePlayer.Identifier, activePlayer.GetComponent<Entity>());
        moveablesNew.Add(activePlayer);
        activePlayer.Identifier = 0;
    }

    private void SetCameraPosition()
    {
        camera.transform.position = new Vector3(
            activePlayer.transform.position.x + camXOffset + leveldata.cameraVerticalOffset, 
            camYOffset, 
            activePlayer.transform.position.z + camZOffset - leveldata.cameraVerticalOffset);
    }

    private void OnDestroy()
    {
        LevelCompleteEvent -= LevelComplete;
        Player.DieEvent -= PlayerDied;
        LateTickEvent -= LateTick;
    }

    #region [===LoadLevel===]

    private void LoadLevel(int levelIndex)
    {
        GameObject parent = new GameObject("Level");
        parent.transform.position = new Vector3();

        leveldata = levels.GetLevel(levelIndex);
        //init arrays 
        entities = new Entity[leveldata.width, leveldata.height];
        entitiesSimulation = new Entity[leveldata.width, leveldata.height];
        tileNumbers = new GameObject[leveldata.width, leveldata.height];
        //Build level tiles
        BuildLevelTiles(parent);

        CombineMeshes();

        if(leveldata.cameraSize != 0)
        {
            camera.GetComponent<Camera>().orthographicSize = leveldata.cameraSize;
        }
        else
        {
            camera.GetComponent<Camera>().orthographicSize = cameraDefaultSize;
        }
    }

    private void BuildLevelTiles(GameObject parent)
    {
        for(int x = 0; x < leveldata.width; x++)
        {
            for(int y = 0; y < leveldata.height; y++)
            {
                var thisTileData = leveldata.data[x, y];

                if(thisTileData.entityID != EntityID.hole)
                {
                    var obj = Instantiate(baseTile);
                    obj.transform.position = new Vector3(x, levelBlockHeight, y);
                    obj.transform.parent = parent.transform;
                }

                if(thisTileData.entityID == EntityID.spawn)
                {
                    activePlayer.Pos = (x, y);
                    activePlayer.transform.position = new Vector3(x, 1, y);
                }
                if(tileObjects.ContainsKey(thisTileData.entityID))
                {
                    var tileObject = tileObjects[thisTileData.entityID];
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

                        //fill script tiledata from leveldata
                        var tileScript = tile.GetComponent<Tile>();

                        this.entities[x, y] = tileScript;

                        tileScript.Direction = thisTileData.direction;
                        tileScript.ActivateNumber = thisTileData.activateNumber;
                        tileScript.Pos = (x, y);
                        tileScript.Even = thisTileData.even;
                        tileScript.Odd = thisTileData.odd;
                        tileScript.Less = thisTileData.less;
                        tileScript.Greater = thisTileData.greater;
                        tileScript.Not = thisTileData.not;
                        tileScript.Identifier = identifierIndex;
                        identifierIndex++;

                        IMoveable moveable = tileScript as IMoveable;
                        if(moveable != null)
                        {
                            moveablesNew.Add(moveable);
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
                                number.transform.position = new Vector3(x, numberYOffset, y);
                                number.transform.rotation = Quaternion.Euler(90, 0, 0);
                                tileNumbers[x, y] = number;
                            }
                        }
                        PressurePlate pressurePlate = tileScript as PressurePlate;
                        Goal goal = tileScript as Goal;
                        if(pressurePlate != null)
                        {
                            pressurePlates.Add(pressurePlate);
                        }
                        else if(goal != null)
                        {
                            goalObject = tile;
                            goalObjectNumber = tileNumbers[x, y];
                            goal.goalNumber = goalObjectNumber;
                        }
                    }
                }
            }
        }

        for(int i = 0; i <= entities.GetUpperBound(0); i++)
        {
            for(int j = 0; j <= entities.GetUpperBound(1); j++)
            {
                if(entities[i, j] != null)
                {
                    entitiesDict.Add(entities[i, j].Identifier, entities[i, j]);
                }
            }
        }
    }


    #region [---MeshCombine---]
    void CombineMeshes()
    {
        List<List<GameObject>> groups = new List<List<GameObject>>();
        meshVisited = new bool[leveldata.width, leveldata.height];

        for(int y = 0; y < leveldata.height; y++)
        {
            for(int x = 0; x < leveldata.width; x++)
            {
                if(leveldata.data[x,y].entityID == EntityID.block && !meshVisited[x,y])
                {
                    groups.Add(MakeGroup(x, y));
                }
            }
        }

        List<List<MeshFilter>> meshfilters = new List<List<MeshFilter>>();
        for(int i = 0; i < groups.Count; i++)
        {
            List<MeshFilter> curGroup = new List<MeshFilter>();
            for(int j = 0; j < groups[i].Count; j++)
            {
                curGroup.Add(groups[i][j].GetComponent<MeshFilter>());
            }
            CombineInstance[] combine = new CombineInstance[curGroup.Count];
            int x = 0;
            while(x < curGroup.Count)
            {
                combine[x].mesh = curGroup[x].sharedMesh;
                combine[x].transform = curGroup[x].transform.localToWorldMatrix;
                curGroup[x].gameObject.SetActive(false);
                x++;
            }
            if(x > 0) 
            { 
                var newObj = new GameObject("Mesh " + i.ToString());
                var filter = newObj.AddComponent<MeshFilter>();
                var renderer = newObj.AddComponent<MeshRenderer>();
                filter.mesh = new Mesh();
                filter.mesh.CombineMeshes(combine, true);
                renderer.sharedMaterial = tileObjects[EntityID.block].GetComponent<MeshRenderer>().sharedMaterial;
            }
        }
    }

    List<GameObject> MakeGroup(int x, int y)
    {
        List<GameObject> group = new List<GameObject>();
        group.Add(entities[x, y].gameObject);
        meshVisited[x, y] = true;

        while(true)
        {

            if(x - 1 > -1)
            {
                GameObject left = meshVisited[x - 1, y] ? null : GetEntityObjectIfBlock(x - 1, y);
                if(left)
                {
                    meshVisited[x - 1, y] = true;
                    group.Add(left);
                    x--;
                    continue;
                }
            }
            if(y + 1 < leveldata.height)
            {
                GameObject up = meshVisited[x, y + 1] ? null : GetEntityObjectIfBlock(x, y + 1);
                if(up)
                {
                    meshVisited[x, y + 1] = true;
                    group.Add(up);
                    y++;
                    continue;
                }
            }
            if(x + 1 < leveldata.width)
            {
                GameObject right = meshVisited[x + 1, y] ? null : GetEntityObjectIfBlock(x + 1, y);
                if(right)
                {
                    meshVisited[x + 1, y] = true;
                    group.Add(right);
                    x++;
                    continue;
                }
            }
            if(y - 1 > -1)
            {
                GameObject down = meshVisited[x, y - 1] ? null : GetEntityObjectIfBlock(x, y - 1);
                if(down)
                {
                    meshVisited[x, y - 1] = true;
                    group.Add(down);
                    y--;
                    continue;
                }
            }
            break;
        }


        return group;
    }

    GameObject GetEntityObjectIfBlock(int x, int y)
    {
        if(x < 0) return null;
        if(x > leveldata.width - 1) return null;
        if(y < 0) return null;
        if(y > leveldata.height - 1) return null;

        var result = entities[x, y];
        if(result && result.entityID == EntityID.block) return result.gameObject;
        return null;

    }
    #endregion [---MeshCombine---]

    #endregion [===LoadLevel===]

    void Update()
    {
        activePlayer.indicatorsActive = Input.GetKey(KeyCode.Space);
        SetCameraPosition();

        if(inTransition)
        {
            return;
        }
        if(lockingObject != null)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.R) && tickNumber > 0)
        {
            StartCoroutine(Undo());
        }

        //debug controls
        if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            LevelCompleteEvent.Invoke(true, true);
        }
        else if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            LevelCompleteEvent.Invoke(false, true);
        }
        else if(Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Alpha6))
        {
            RestartLevel();
        }
        else if(Input.GetKeyDown(KeyCode.B)) 
        {
            postProcessVolume.SetActive(!postProcessVolume.activeSelf);
        }

        if(!activePlayer.Dead)
        {
            HandleMovement();
        }
    }

    private IEnumerator Undo()
    {
        lockingObject = this.gameObject;
        UndoEvent.Invoke();

        //undo "tick" to play animations 
        tickTimer = 0.0f;
        yield return StartCoroutine(UndoTick());

        tickNumber--;
        lockingObject = null;

        UpdateEntityArray();
    }

    private void UpdateEntityArray()
    {
        for(int i = 0; i <= entities.GetUpperBound(0); i++)
        {
            for(int j = 0; j <= entities.GetUpperBound(1); j++)
            {
                entities[i, j] = null;
            }
        }
        foreach(var entity in entitiesDict) 
        {
            entities[entity.Value.Pos.Item1, entity.Value.Pos.Item2] = entity.Value;
        }
    }

    private IEnumerator UndoTick()
    {
        transitionDuration = undoDuration;
        while(tickTimer < transitionDuration)
        {
            tickTimer += Time.deltaTime;

            foreach(IMoveable m in moveablesNew)
            {
                m.LerpMove();
            }
            yield return null;
        }
        tickTimer = 0.0f;
        transitionDuration = normalDuration;
    }

    private void HandleMovement()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SimulateMovement(Direction.up);
            if(transactions.ContainsKey(activePlayer.Identifier))
            {
                StartCoroutine(Transition(Direction.up));
            }
            return;
        }
        else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SimulateMovement(Direction.left);
            if(transactions.ContainsKey(activePlayer.Identifier))
            {
                StartCoroutine(Transition(Direction.left));
            }
            return;
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            SimulateMovement(Direction.down);
            if(transactions.ContainsKey(activePlayer.Identifier))
            {
                StartCoroutine(Transition(Direction.down));
            }
            return;
        }
        else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SimulateMovement(Direction.right);
            if(transactions.ContainsKey(activePlayer.Identifier))
            {
                StartCoroutine(Transition(Direction.right));
            }
            return;
        }

    }

    private void SimulateMovement(Direction dir)
    {
        movedIdentifiers.Clear();
        transactions.Clear();

        int uBound0 = entitiesSimulation.GetUpperBound(0);
        int uBound1 = entitiesSimulation.GetUpperBound(1);

        for(int i = 0; i <= uBound0; i++)
        {
            for(int j = 0; j <= uBound1; j++)
            {
                entitiesSimulation[i, j] = entities[i, j];
            }
        }

        //this logic only works for player and leveldie
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
                        if(entities[x,y] != null)
                        {
                            var scriptData = entities[x, y].GetComponent<IMoveable>();

                            if(scriptData != null && !CheckCollision((x, y + 1), ref entitiesSimulation))
                            {
                                entitiesSimulation[x, y + 1] = entities[x, y];
                                entitiesSimulation[x, y] = null;
                                var tx = new Transaction();
                                tx.entityID = entities[x, y].Identifier;
                                tx.newPos = (x, y + 1);
                                tx.move = (0, 1);
                                transactions.Add(tx.entityID, tx);

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
                        if(entities[x, y] != null)
                        {
                            var scriptData = entities[x, y].GetComponent<IMoveable>();

                            if(scriptData != null && !CheckCollision((x + 1, y), ref entitiesSimulation))
                            {
                                entitiesSimulation[x + 1, y] = entities[x, y];
                                entitiesSimulation[x, y] = null;
                                var tx = new Transaction();
                                tx.entityID = entities[x, y].Identifier;
                                tx.newPos = (x+1, y);
                                tx.move = (1, 0);
                                transactions.Add(tx.entityID, tx);
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
                        if(entities[x, y] != null)
                        {
                            var scriptData = entities[x, y].GetComponent<IMoveable>();

                            if(scriptData != null && !CheckCollision((x, y - 1), ref entitiesSimulation))
                            {
                                entitiesSimulation[x, y - 1] = entities[x, y];
                                entitiesSimulation[x, y] = null;
                                var tx = new Transaction();
                                tx.entityID = entities[x, y].Identifier;
                                tx.newPos = (x, y - 1);
                                tx.move = (0, -1);
                                transactions.Add(tx.entityID, tx);
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
                        if(entities[x, y] != null)
                        {
                            var scriptData = entities[x, y].GetComponent<IMoveable>();

                            if(scriptData != null && !CheckCollision((x - 1, y), ref entitiesSimulation))
                            {
                                entitiesSimulation[x - 1, y] = entities[x, y];
                                entitiesSimulation[x, y] = null;
                                var tx = new Transaction();
                                tx.entityID = entities[x, y].Identifier;
                                tx.newPos = (x-1, y);
                                tx.move = (-1, 0);
                                transactions.Add(tx.entityID, tx);
                            }
                        }
                    }
                }
                break;
        }
    }

    private IEnumerator Transition(Direction dir)
    {
        tickTimer = 0.0f;
        inTransition = true;

        Quaternion cameraNudgedRotation = cameraStartQuaternion;

        //copy simulated movements into tile to finalize
        int uBound0 = entities.GetUpperBound(0);
        int uBound1 = entities.GetUpperBound(1);
        for(int i = 0; i <= uBound0; i++)
        {
            for(int j = 0; j <= uBound1; j++)
            {
                entities[i, j] = entitiesSimulation[i, j];
            }
        }

        #region calculate lerp data

        switch(dir)
        {
            case Direction.none:
                break;
            case Direction.up:
                cameraNudgedRotation = Quaternion.Euler(Vector3.right * -cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.right:
                cameraNudgedRotation = Quaternion.Euler(Vector3.forward * cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.down:
                cameraNudgedRotation = Quaternion.Euler(Vector3.right * cameraNudgeAngle) * cameraStartQuaternion;
                break;
            case Direction.left:
                cameraNudgedRotation = Quaternion.Euler(Vector3.forward * -cameraNudgeAngle) * cameraStartQuaternion;
                break;
            default:
                break;
        }
        #endregion

        TickEvent.Invoke(Die.GetNewFace(dir, ref activePlayer.die), dir);


        CheckPressurePlatesAllOn();

        while(tickTimer < transitionDuration)
        {
            tickTimer += Time.deltaTime;

            foreach(IMoveable m in moveablesNew)
            {
                m.LerpMove();
            }
            if(cameraNudgeEnabled)
            {
                camera.transform.rotation = Quaternion.Lerp(cameraNudgedRotation, cameraStartQuaternion, cameraNudgeCurve.Evaluate(tickTimer / transitionDuration));
            }
            yield return null;
        }

        tickNumber++;
        inTransition = false;
        LateTickEvent.Invoke(activePlayer.Face);
    }

    //enable goal if all on and play sound
    private void CheckPressurePlatesAllOn()
    {
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
                PlaySound(GoalEnabledSound);
                goalObject.SetActive(true);
                goalObjectNumber.SetActive(true);
                goalObject.GetComponent<Goal>().goalActive = true;
            }
        }
    }

    //webgl doesnt play sounds that are too short or start at 0:00 and are short so sound files are .02s delayed
    public static void PlaySound(AudioSource audioSource)
    {
#if PLATFORM_WEBGL
            audioSource.Play();
#else
            audioSource.PlayDelayed(0.02f);
#endif
    }

    private void LateTick(int dieFace)
    {
        float dampenValue = 0.6f;
        float startVolume = 0.5f;
        var stereoValue = ((activePlayer.Pos.Item1 / ((float)leveldata.width-1.0f)) - 0.5f) * dampenValue;
        var newVolume = startVolume - (0.3f * (activePlayer.Pos.Item2 / ((float) leveldata.height-1.0f)));
        MoveSound.panStereo = stereoValue;
        MoveSound.volume = newVolume;
        PlaySound(MoveSound);
    }

    //returns true if collision or OOB, else false
    //Warning: does work with lazers (holes return collision as true, but a lazer should not hit it)
    //in reality collision / walkable should be two separate properties
    private bool CheckCollision((int, int) pos, ref Entity[,] dataSource)
    {
        if(pos.Item1 < 0 || pos.Item2 < 0 ||
            pos.Item1 > leveldata.width - 1 || pos.Item2 > leveldata.height - 1)
        {
            return true;
        }

        var curTile = dataSource[pos.Item1, pos.Item2];

        if(curTile == null)
        {
            return false;
        }
        else
        {
            if(curTile.entityID == EntityID.hole) return true;
            return curTile.collision;
        }
    }

    public static int GetTileMask(EntityID[] tiles)
    {
        int num = 0;
        foreach(EntityID t in tiles)
        {
            num |= 1 << (int)t;
        }
        return num;
    }

    bool IsBitSet(int flag, int? mask)
    {
        return (mask & flag) == mask;
    }

    //this gets the first collision in a specified direction from a position
    public (int,int) GetCollision((int,int) position, Direction direction, int? mask)
    {
        if(mask == null)
        { 
            switch(direction)
            {
                case Direction.none:
                    Debug.LogError("GetCollision was passed an invalid direction");
                    return (-1, -1);
                case Direction.up:
                    for(int y = position.Item2+1; y < leveldata.height; y++)
                    {   
                        var curTile = entities[position.Item1, y];
                        if(curTile == null) continue;
                        if(curTile.collision )
                        {
                            return (position.Item1, y);
                        }
                    }
                    return (position.Item1, leveldata.height);
                case Direction.right:
                    for(int x = position.Item1+1; x < leveldata.width; x++)
                    {
                        var curTile = entities[x, position.Item2];
                        if(curTile == null) continue;
                        if(curTile.collision)
                        {
                            return (x, position.Item2);
                        }
                    }
                    return (leveldata.width, position.Item2);
                case Direction.down:
                    for(int y = position.Item2-1; y > -1; y--)
                    {
                        var curTile = entities[position.Item1, y];
                        if(curTile == null) continue;
                        if(curTile.collision)
                        {
                            return (position.Item1, y);
                        }
                    }
                    return (position.Item1, -1);
                case Direction.left:
                    for(int x = position.Item1-1; x > -1; x--) 
                    {
                        var curTile = entities[x, position.Item2];
                        if(curTile == null) continue;
                        if(curTile.collision)
                        {
                            return (x, position.Item2);
                        }
                    }
                    return (-1, position.Item2);
            }
        }
        else
        {
            switch(direction)
            {
                case Direction.none:
                    Debug.LogError("GetCollision was passed an invalid direction");
                    return (-1, -1);
                case Direction.up:
                    for(int y = position.Item2 + 1; y < leveldata.height; y++)
                    {
                        var curTile = entities[position.Item1, y];
                        if(curTile == null) continue;
                        if(curTile.collision && !IsBitSet((int)curTile.entityID, mask))
                        {
                            return (position.Item1, y);
                        }
                    }
                    return (position.Item1, leveldata.height);
                case Direction.right:
                    for(int x = position.Item1 + 1; x < leveldata.width; x++)
                    {
                        var curTile = entities[x, position.Item2];
                        if(curTile == null) continue;
                        if(curTile.collision && !IsBitSet((int) curTile.entityID, mask))
                        {
                            return (x, position.Item2);
                        }
                    }
                    return (leveldata.width, position.Item2);
                case Direction.down:
                    for(int y = position.Item2 - 1; y > -1; y--)
                    {
                        var curTile = entities[position.Item1, y];
                        if(curTile == null) continue;
                        if(curTile.collision && !IsBitSet((int) curTile.entityID, mask))
                        {
                            return (position.Item1, y);
                        }
                    }
                    return (position.Item1, -1);
                case Direction.left:
                    for(int x = position.Item1 - 1; x > -1; x--)
                    {
                        var curTile = entities[x, position.Item2];
                        if(curTile == null) continue;
                        if(curTile.collision && !IsBitSet((int) curTile.entityID, mask))
                        {
                            return (x, position.Item2);
                        }
                    }
                    return (-1, position.Item2);
            }
        }

        return (-1, -1);
    }

    private void PlayerDied()
    {
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

            PlaySound(LevelCompleteSound);
            transitionStarted = true;
            return;
        }
        StartCoroutine(LevelTransition(0, fast));
        PlaySound(LevelCompleteSound);
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