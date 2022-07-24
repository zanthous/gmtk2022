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

    public static Action DieEvent;

    private new Renderer renderer;
    private Stack<PlayerState> playerStates = new Stack<PlayerState>();

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

    public override void Awake()
    {
        base.Awake();
        DieEvent += Die_;
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
        //        if(game.transactions.ContainsKey(Identifier))

        var tx = game.transactions[Identifier];
        (int, int) newPos = tx.newPos;
        CalculateTarget(newPos, dir);
        die.RollDie(dir);
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
        CalculateTarget(Pos, Dir.dirTuple[(Pos.Item1 - previousPos.Item1, Pos.Item2 - previousPos.Item2)]);
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
        transform.position = Vector3.Lerp(TickPosition, LerpTargetPosition, Game.tickTimer / Game.transitionDuration);
        transform.rotation = Quaternion.Lerp(TickRotation, LerpTargetRotation, Game.tickTimer / Game.transitionDuration);
    }
}
