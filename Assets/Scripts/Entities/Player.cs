using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity, IMoveable, IHasDie
{
    struct PlayerState
    {
        public bool dead;
        public (int, int) pos;
        public int[] dieFaces;
    }

    public Die die;
    public bool active = false;
    public bool indicatorsActive = false;

    public static Action DieEvent;

    private new Renderer renderer;
    private Stack<PlayerState> playerStates = new Stack<PlayerState>();

    [SerializeField] GameObject leftIndicator;
    [SerializeField] GameObject rightIndicator;
    [SerializeField] GameObject backIndicator;

    private Dictionary<int, Sprite> indicatorSprites = new Dictionary<int, Sprite>();
    private new GameObject camera;

    private Vector3 leftIndicatorStartPos;
    private Vector3 rightIndicatorStartPos;
    private Vector3 backIndicatorStartPos;

    private SpriteRenderer leftRenderer;
    private SpriteRenderer rightRenderer;
    private SpriteRenderer backRenderer;

    public int Face
    {
        get { return die.Face; }
    }
    
    private bool dead = false;
    public bool Dead { 
        get { return dead; }
        set
        {
            dead = value;
            renderer.gameObject.SetActive(!dead);
        }
    }

    public Vector3 TickPosition { get; set; }
    public Quaternion TickRotation { get; set; }
    public Vector3 LerpTargetPosition { get; set; }
    public Quaternion LerpTargetRotation { get; set; }
    public bool ShouldMove { get; set; }

    Vector3 cameraLookAt;

    public override void Awake()
    {
        base.Awake();
        DieEvent += Die_;
        indicatorSprites.Add(1,Resources.Load<Sprite>("1"));
        indicatorSprites.Add(2,Resources.Load<Sprite>("2"));
        indicatorSprites.Add(3,Resources.Load<Sprite>("3"));
        indicatorSprites.Add(4,Resources.Load<Sprite>("4"));
        indicatorSprites.Add(5,Resources.Load<Sprite>("5"));
        indicatorSprites.Add(6,Resources.Load<Sprite>("6"));
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        DieEvent -= Die_;
    }

    private void Start()
    {
        renderer = GetComponentInChildren<Renderer>();
        TickPosition = transform.position;
        TickRotation = transform.rotation;
        die = new Die();
        AddCurrentState();
        camera = FindObjectOfType<Camera>().gameObject;

        cameraLookAt = camera.transform.position;
        cameraLookAt += camera.transform.forward * 10.0f;

        leftIndicatorStartPos = leftIndicator.transform.position;
        rightIndicatorStartPos = rightIndicator.transform.position;
        backIndicatorStartPos = backIndicator.transform.position;

        leftRenderer = leftIndicator.GetComponent<SpriteRenderer>();
        rightRenderer = rightIndicator.GetComponent<SpriteRenderer>();
        backRenderer = backIndicator.GetComponent<SpriteRenderer>();
    }


    private void Die_()
    {
        Dead = true;
        //play die animation / sfx
        Debug.Log("dead");
    }

    public void CalculateTarget((int, int) newPos, Direction dir)
    {
        //1 is just the base vertical offset for obstacles, this may change
        LerpTargetPosition = new Vector3(newPos.Item1, 1, newPos.Item2);
        LerpTargetRotation = TickRotation;
        switch(dir)
        {
            case Direction.none:
                break;
            case Direction.up:
                LerpTargetRotation = Quaternion.Euler(Vector3.right * 90) * LerpTargetRotation;
                break;
            case Direction.right:
                LerpTargetRotation = Quaternion.Euler(Vector3.forward * -90) * LerpTargetRotation;
                break;
            case Direction.down:
                LerpTargetRotation = Quaternion.Euler(Vector3.right * -90) * LerpTargetRotation;
                break;
            case Direction.left:
                LerpTargetRotation = Quaternion.Euler(Vector3.forward * 90) * LerpTargetRotation;
                break;
            default:
                break;
        }
    }

    public override void Tick(int dieFace, Direction dir)
    {
        TickPosition = transform.position;
        TickRotation = transform.rotation;

        var tx = game.transactions[Identifier];
        (int, int) newPos = tx.newPos;
        CalculateTarget(newPos, dir);
        die.RollDie(dir);
        leftRenderer.sprite = indicatorSprites[die.horizontal[0]];
        rightRenderer.sprite = indicatorSprites[die.vertical[0]];
        backRenderer.sprite = indicatorSprites[die.back];

        Pos = newPos;
    }

    public override void LateTick(int dieFace)
    {
        //add gamestate stuff
        AddCurrentState();
    }

    public override void Undo()
    {
        TickPosition = transform.position;
        TickRotation = transform.rotation;
        var previousPos = Pos;
        var previousDead = Dead;
        playerStates.Pop();
        Pos = playerStates.Peek().pos;
        Dead = playerStates.Peek().dead;

        if(!Dead && previousDead)
        {
            game.diedHintText.gameObject.SetActive(false);
        }
        die.SetDie(playerStates.Peek().dieFaces);
        leftIndicator.GetComponent<SpriteRenderer>().sprite = indicatorSprites[die.horizontal[0]];
        rightIndicator.GetComponent<SpriteRenderer>().sprite = indicatorSprites[die.vertical[0]];
        backIndicator.GetComponent<SpriteRenderer>().sprite = indicatorSprites[die.back];
        CalculateTarget(Pos, Dir.dirTuple[(Pos.Item1 - previousPos.Item1, Pos.Item2 - previousPos.Item2)]);
    }

    private void Update()
    {
        leftIndicator.SetActive(indicatorsActive);
        backIndicator.SetActive(indicatorsActive);
        rightIndicator.SetActive(indicatorsActive);


        cameraLookAt = camera.transform.position;
        cameraLookAt += camera.transform.forward * 10.0f;

        leftIndicator.transform.position =  cameraLookAt + leftIndicatorStartPos;
        rightIndicator.transform.position = cameraLookAt + rightIndicatorStartPos;
        backIndicator.transform.position =  cameraLookAt + backIndicatorStartPos;

        if(indicatorsActive)
        {
            leftIndicator.transform.LookAt(camera.transform.position);
            rightIndicator.transform.LookAt(camera.transform.position);
            backIndicator.transform.LookAt(camera.transform.position);
        }
    }

    public override void AddCurrentState()
    {
        PlayerState state = new PlayerState();
        state.pos = this.Pos;
        state.dead = this.Dead;
        state.dieFaces = die.GetDie();
        playerStates.Push(state);
    }

    public void LerpMove()
    {
        transform.position = Vector3.Lerp(TickPosition, LerpTargetPosition, Mathf.Clamp(Game.tickTimer / Game.transitionDuration,0,1.0f));
        transform.rotation = Quaternion.Lerp(TickRotation, LerpTargetRotation, Mathf.Clamp(Game.tickTimer / Game.transitionDuration, 0, 1.0f));
    }
}
