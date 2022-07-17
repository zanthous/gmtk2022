using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Tile
{

    private bool disableGoal = true;
    private bool active = false;

    private bool on = false;

    public bool levelDieOnly = true;

    private bool previousValue = false;

    private bool shouldPlay = false;
    public bool On
    {
        get { return on; }
    }

    public override void Tick(int dieFace)
    {
        //need access to moveable
        var moveables = game.moveables;
        if(levelDieOnly)
        {
            on = false;
            foreach(var m in moveables)
            {
                if(m.Item2 is LevelDie && m.Item2.Position == this.position)
                {
                    if(DetermineActive((m.Item2 as LevelDie).Face))
                    {
                        on = true;
                    }
                }
            }
        }
        else
        {
            on = false;
            foreach(var m in moveables)
            {
                if(m.Item2 is LevelDie && m.Item2.Position == this.position)
                {
                    if(DetermineActive((m.Item2 as LevelDie).Face))
                    {
                        on = true;
                    }
                }
            }
            if(game.ActivePlayer.Pos == this.position && DetermineActive(game.ActivePlayer.Face))
            {
                on = true;
            }
        }
        if(previousValue == false && on)
        {
            shouldPlay = true;
            
        }
        previousValue = on;
    }


    private void Start()
    {
        Game.LateTickEvent += LateTick;
    }

    private void LateTick(int dieFace)
    {
        if(shouldPlay)
        {
            if(!game.PressurePlateSound.isPlaying)
            {
                game.PressurePlateSound.Play();
            }
            shouldPlay = false;
        }
    }

    private void OnDestroy()
    {
        Game.LateTickEvent -= LateTick;
        Game.Tick -= Tick;
    }
}
