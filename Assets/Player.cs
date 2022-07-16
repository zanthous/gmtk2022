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

    private void Awake()
    {
        pos = (0, 0);
        dieVertical = new int[3] { 2, 1, 5 }; //front/face/behind
        dieHorizontal = new int[3] { 4, 1, 3 }; //left/face/right
        backNumber = 6;
    }

    public int Face
    {
        get { return this.dieHorizontal[1]; }
    }

    public ValueTuple<int,int> Pos
    {
        get { return pos; }
        set { pos = value; }
    }
    public void RollDie(Direction dir)
    {
        if(dieVertical.Length != 3 || dieHorizontal.Length != 3)
        {
            Debug.LogError("RollDie input arrays are not the right size");
            return;
        }
        int temp;
        switch(dir)
        {
            case Direction.up:
                temp = dieVertical[0];
                dieVertical[0] = dieVertical[1];
                dieVertical[1] = dieVertical[2];
                dieVertical[2] = backNumber;
                backNumber = temp;
                dieHorizontal[1] = dieVertical[1];
                break;
            case Direction.right:
                temp = dieHorizontal[2];
                dieHorizontal[2] = dieHorizontal[1];
                dieHorizontal[1] = dieHorizontal[0];
                dieHorizontal[0] = backNumber;
                backNumber = temp;
                dieVertical[1] = dieHorizontal[1];
                break;
            case Direction.down:
                temp = dieVertical[2];
                dieVertical[2] = dieVertical[1];
                dieVertical[1] = dieVertical[0];
                dieVertical[0] = backNumber;
                backNumber = temp;
                dieHorizontal[1] = dieVertical[1];
                break;
            case Direction.left:
                temp = dieHorizontal[0];
                dieHorizontal[0] = dieHorizontal[1];
                dieHorizontal[1] = dieHorizontal[2];
                dieHorizontal[2] = backNumber;
                backNumber = temp;
                dieVertical[1] = dieHorizontal[1];
                break;
        }
    }

    //public void RollDie(ref int[] vert, ref int[] hor, ref int back, Direction dir)
    //{
    //    if(vert.Length != 3 || hor.Length != 3)
    //    {
    //        Debug.LogError("RollDie input arrays are not the right size");
    //        return;
    //    }
    //    int temp;
    //    switch(dir)
    //    {
    //        case Direction.up:
    //            temp = vert[0];
    //            vert[0] = vert[1];
    //            vert[1] = vert[2];
    //            vert[2] = back;
    //            back = temp;
    //            hor[1] = vert[1];
    //            break;
    //        case Direction.right:
    //            temp = hor[2];
    //            hor[2] = hor[1];
    //            hor[1] = hor[0];
    //            hor[0] = back;
    //            back = temp;
    //            vert[1] = hor[1];
    //            break;
    //        case Direction.down:
    //            temp = vert[2];
    //            vert[2] = vert[1];
    //            vert[1] = vert[0];
    //            vert[0] = temp;
    //            hor[1] = vert[1];
    //            break;
    //        case Direction.left:
    //            temp = hor[0];
    //            hor[0] = hor[1];
    //            hor[1] = hor[2];
    //            hor[2] = back;
    //            back = temp;
    //            vert[1] = hor[1];
    //            break;
    //    }
    //}
}
