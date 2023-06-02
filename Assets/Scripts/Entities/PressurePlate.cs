using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Tile
{
    private struct PressurePlateState
    {
        public bool on;
        public bool previousValue;
    }

    public bool levelDieOnly = true;

    private bool previousValue = false;

    private bool shouldPlay = false;
    public bool On { get; set; }

    private Stack<PressurePlateState> pressurePlateStates = new Stack<PressurePlateState>();

    private void Start()
    {
        AddCurrentState();
    }

    public override void Tick(int dieFace, Direction direction)
    {
        var moveables = game.moveablesNew;
        if(levelDieOnly)
        {
            On = false;
            foreach(var m in moveables)
            {
                var die = m as IHasDie;
                var levelDie = m as LevelDie;
                var entity = m as Entity;
                if(die != null && levelDie != null && entity.Pos == this.Pos)
                {
                    if(DetermineFaceMatch(die.Face))
                    {
                        On = true;
                    }
                }
            }
        }
        else
        {
            On = false;
            foreach(var m in moveables)
            {
                var die = m as IHasDie;
                var entity = m as Entity;
                if(die != null && entity.Pos == this.Pos)
                {
                    if(DetermineFaceMatch(die.Face))
                    {
                        On = true;
                    }
                }
            }
        }

        if(previousValue == false && On)
        {
            shouldPlay = true;
        }
        previousValue = On;
    }

    public override void LateTick(int dieFace)
    {
        if(shouldPlay)
        {
            if(!game.PressurePlateSound.isPlaying)
            {
                Game.PlaySound(game.PressurePlateSound);
                game.PressurePlateSound.Play();
            }
            shouldPlay = false;
        }
        AddCurrentState();
    }

    public override void Undo()
    {
        pressurePlateStates.Pop();
        this.On = pressurePlateStates.Peek().on;
        this.previousValue = pressurePlateStates.Peek().on;
    }

    public override void AddCurrentState()
    {
        PressurePlateState state = new PressurePlateState();
        state.previousValue = this.previousValue;
        state.on = this.On;
        pressurePlateStates.Push(state);
    }
}
