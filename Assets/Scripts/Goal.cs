using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Tile
{
    private struct GoalState
    {
        public bool goalActive;
    }

    private Stack<GoalState> goalStates = new Stack<GoalState>();
    public GameObject goalNumber; //populated on loadlevel
    public bool goalActive = false;

    private PressurePlate[] pressurePlates;

    private void Start()
    {
        pressurePlates = FindObjectsOfType<PressurePlate>();
        if(pressurePlates.Length > 0)
        {
            gameObject.SetActive(false);
            if(goalNumber != null)
            { 
                goalNumber.SetActive(false);
            }
            goalActive = false;
        }
        else
        {
            goalActive = true;
        }
        AddCurrentState();
    }
    public override void Tick(int dieFace, Direction direction)
    {
    }

    public override void LateTick(int dieFace)
    {
        var match = DetermineFaceMatch(dieFace);
        if(game.ActivePlayer.Pos == Pos && (match || activateNumber == -1) && goalActive) 
        {
            game.lockingObject = gameObject;
            Game.LevelCompleteEvent.Invoke(true,false);
        }
        AddCurrentState();
    }

    public override void Undo()
    {
        goalStates.Pop();
        this.goalActive = goalStates.Peek().goalActive;
        gameObject.SetActive(this.goalActive);
        if(goalNumber != null)
        {
            goalNumber.SetActive(this.goalActive);
        }
    }

    public override void AddCurrentState()
    {
        GoalState state = new GoalState();
        state.goalActive = this.goalActive;
        goalStates.Push(state);
    }
}
