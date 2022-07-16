using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int[] dieVertical;
    private int[] dieHorizontal;
    private int backNumber;

    private ValueTuple<int, int> pos;

    public bool active = false;

    public ValueTuple<int,int> Pos
    {
        get { return pos; }
        set { pos = value; }
    }


    private void RollDie(ref int[] vert, ref int[] hor, ref int back, Direction dir)
    {
        if(vert.Length != 3 || hor.Length != 3)
        {
            Debug.LogError("RollDie input arrays are not the right size");
            return;
        }
        int temp;
        switch(dir)
        {
            case Direction.up:
                temp = vert[0];
                vert[0] = vert[1];
                vert[1] = vert[2];
                vert[2] = back;
                back = temp;
                hor[1] = vert[1];
                break;
            case Direction.right:
                temp = hor[2];
                hor[2] = hor[1];
                hor[1] = hor[0];
                hor[0] = back;
                back = temp;
                vert[1] = hor[1];
                break;
            case Direction.down:
                temp = vert[2];
                vert[2] = vert[1];
                vert[1] = vert[0];
                vert[0] = temp;
                hor[1] = vert[1];
                break;
            case Direction.left:
                temp = hor[0];
                hor[0] = hor[1];
                hor[1] = hor[2];
                hor[2] = back;
                back = temp;
                vert[1] = hor[1];
                break;
        }
    }
}
