using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Tile
{

    private bool disableGoal = true;
    private bool active = false;

    private bool on = false;

    public bool On
    {
        get { return on; }
    }

    public override void Tick(int dieFace)
    {
        //need access to moveable
        var moveables = game.moveables;

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
        //active = DetermineActive(dieFace);
    }

    //private void LateTick(int dieFace)
    //{

    //}

    // Start is called before the first frame update
    void Start()
    {
        //Game.LateTickEvent += LateTick;
    }


    private void OnDestroy()
    {
        //Game.LateTickEvent -= LateTick;
        Game.Tick -= Tick;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
