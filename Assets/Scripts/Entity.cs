using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//todo think of a name
public abstract class Entity : MonoBehaviour
{
    public EntityID entityID;

    protected Game game;

    public bool moves = false;
    public bool collision = true;
    public int Identifier { get;set; }


    public (int,int) Pos
    {
        get;set;
    }

    public virtual void Awake()
    {
        Game.TickEvent += Tick;
        Game.LateTickEvent += LateTick;
        Game.UndoEvent += Undo;

        game = FindObjectOfType<Game>();
    }

    public virtual void OnDestroy()
    {
        Game.TickEvent -= Tick;
        Game.LateTickEvent -= LateTick;
        Game.UndoEvent -= Undo;
    }

    //normal logic happens here
    public abstract void Tick(int dieFace, Direction direction);
    //post processing happens here for effects or additional logic for when landing on something (pressure plate, hitting a lazer)
    public abstract void LateTick(int dieFace);
    public abstract void Undo();

    public abstract void AddCurrentState(); //to be used in start, as well as end of latetick
}
