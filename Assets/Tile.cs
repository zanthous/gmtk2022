using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//parent class for all tiles
public abstract class Tile : MonoBehaviour
{
    public bool collision = true;
    public tileID tileID;

    //this
    protected bool even = false;
    protected bool odd = false;

    //or this
    protected int activateNumber = -1;

    //with modifiers
    protected bool less = false;
    protected bool greater = false;

    protected bool not = false;

    protected Direction direction = Direction.none;
    protected float verticalOffset = 0.0f;

    protected (int, int) position;

    public bool Even
    {
        get { return even; }
        set { even = value; }
    }

    public bool Odd
    {
        get { return odd; } 
        set { odd = value; }
    }

    public bool Less
    {
        get { return less; }
        set { less = value; }
    }
    public bool Greater
    {
        get { return greater; }
        set
        {
            greater = value;
        }   
    }

    public bool Not
    {
        get { return not; }
        set
        {
            not = value;
        }
    }

    public (int, int) Position
    {
        get { return position; }
        set { position = value; }
    }
    public Direction Direction
    {
        get { return direction; }
        set { direction = value; }
    }
    public int ActivateNumber
    {
        get { return activateNumber; }
        set { activateNumber = value; }
    }

    public float VerticalOffset
    {
        get { return verticalOffset; }
    }

    private void Awake()
    {
        Game.Tick += Tick;
    }

    private void OnDestroy()
    {
        Game.Tick -= Tick;
    }

    public bool DetermineActive(int dieFace)
    {
        if(even && dieFace % 2 == 0) return true;
        if(odd && dieFace % 2 == 1) return true;
        if(greater && dieFace > activateNumber) return true;
        if(less && dieFace < activateNumber) return true;
        if(not && dieFace != activateNumber) return true;
        if(!even && !odd && !greater && !less && !not && activateNumber == dieFace) return true;

        return false;
    }

    public abstract void Tick(int dieFace);
}
