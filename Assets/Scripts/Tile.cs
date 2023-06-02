using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//parent class for all tiles
public abstract class Tile : Entity
{

    //Either one of these
    protected bool even = false;
    protected bool odd = false;

    //or this
    protected int activateNumber = -1;

    //with one of these modifiers
    protected bool less = false;
    protected bool greater = false;

    protected bool not = false;

    protected Direction pointingDirection = Direction.none;
    protected float verticalOffset = 0.0f;

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

    public Direction Direction
    {
        get { return pointingDirection; }
        set { pointingDirection = value; }
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
    
    public bool DetermineFaceMatch(int dieFace)
    {
        if(even && dieFace % 2 == 0) return true;
        if(odd && dieFace % 2 == 1) return true;
        if(greater && dieFace > activateNumber) return true;
        if(less && dieFace < activateNumber) return true;
        if(not && dieFace != activateNumber) return true;
        if(!even && !odd && !greater && !less && !not && activateNumber == dieFace) return true;

        return false;
    }
}
