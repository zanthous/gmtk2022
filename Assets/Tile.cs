using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//parent class for all tiles
public abstract class Tile : MonoBehaviour
{
    public bool collision = true;
    public tileID tileID;

    protected int activateNumber = -1;
    protected Direction direction = Direction.none;
    protected float verticalOffset = 0.0f;

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

    public abstract void Tick(int dieFace);
}
