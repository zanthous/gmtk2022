using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDie : Tile, IMoveable, IHasDie
{
    struct LevelDieState
    {
        public (int, int) pos;
        public int[] dieFaces;
    }

    public Die die;

    public Vector3 TickPosition { get; set; }
    public Quaternion TickRotation { get; set; }
    public Vector3 LerpTargetPosition { get; set; }
    public Quaternion LerpTargetRotation { get; set; }
    public bool ShouldMove { get; set; }
    public int Face
    {
        get { return die.Face; }
    }

    private Stack<LevelDieState> levelDieStates = new Stack<LevelDieState>();

    private void Start()
    {
        die = new Die();
        AddCurrentState();
    }

    public override void Tick(int dieFace, Direction dir)
    {
        ShouldMove = false;
        TickPosition = transform.position;
        TickRotation = transform.rotation;
        if(game.transactions.ContainsKey(Identifier))
        {
            var tx = game.transactions[Identifier];
            (int, int) newPos = tx.newPos;
            CalculateTarget(newPos, dir);
            die.RollDie(dir);
            Pos = newPos;
            ShouldMove = true;
        }
    }

    public override void LateTick(int dieFace)
    {
        AddCurrentState();
    }

    public override void Undo()
    {
        ShouldMove = false;
        TickPosition = transform.position;
        TickRotation = transform.rotation;
        var previousPos = this.Pos;
        levelDieStates.Pop();
        Pos = levelDieStates.Peek().pos;
        die.SetDie(levelDieStates.Peek().dieFaces);
        if(previousPos != this.Pos)
        {
            ShouldMove = true;
        }
        //recalculate lerp target
        CalculateTarget(Pos, Dir.dirTuple[(Pos.Item1 - previousPos.Item1,
            Pos.Item2 - previousPos.Item2)]);
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

    public void LerpMove()
    {
        if(ShouldMove)
        { 
            transform.position = Vector3.Lerp(TickPosition, LerpTargetPosition, Game.tickTimer / Game.transitionDuration);
            transform.rotation = Quaternion.Lerp(TickRotation, LerpTargetRotation, Game.tickTimer / Game.transitionDuration);
        }
    }

    public override void AddCurrentState()
    {
        LevelDieState levelDieState = new LevelDieState();
        levelDieState.pos = this.Pos;
        levelDieState.dieFaces = die.GetDie();
        levelDieStates.Push(levelDieState);
    }
}
