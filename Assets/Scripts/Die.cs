using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die 
{
    public int[] vertical;
    public int[] horizontal;
    public int back;

    public int Face
    {
        get { return this.horizontal[1]; }
    }


    public Die()
    {
        vertical = new int[3] { 2, 1, 5 }; //front/face/behind
        horizontal = new int[3] { 4, 1, 3 }; //left/face/right
        back = 6;
    }

    public void RollDie(Direction dir)
    {
        int temp;
        switch(dir)
        {
            case Direction.up:
                temp = vertical[0];
                vertical[0] = vertical[1];
                vertical[1] = vertical[2];
                vertical[2] = back;
                back = temp;
                horizontal[1] = vertical[1];
                break;
            case Direction.right:
                temp = horizontal[2];
                horizontal[2] = horizontal[1];
                horizontal[1] = horizontal[0];
                horizontal[0] = back;
                back = temp;
                vertical[1] = horizontal[1];
                break;
            case Direction.down:
                temp = vertical[2];
                vertical[2] = vertical[1];
                vertical[1] = vertical[0];
                vertical[0] = back;
                back = temp;
                horizontal[1] = vertical[1];
                break;
            case Direction.left:
                temp = horizontal[0];
                horizontal[0] = horizontal[1];
                horizontal[1] = horizontal[2];
                horizontal[2] = back;
                back = temp;
                vertical[1] = horizontal[1];
                break;
        }
    }

    public void SetDie(int[] dieFaces)
    {
        vertical[0] = dieFaces[0];
        vertical[1] = dieFaces[1];
        vertical[2] = dieFaces[2];
        horizontal[0] = dieFaces[3];
        horizontal[1] = dieFaces[4];
        horizontal[2] = dieFaces[5];
        back = dieFaces[6];
    }


    public int[] GetDie()
    {
        int[] dieFaces = new int[] {
            vertical[0], vertical[1], vertical[2],
            horizontal[0], horizontal[1], horizontal[2],
            back
        };
        return dieFaces;
    }

    public static int GetNewFace(Direction dir, ref Die die)
    {
        switch(dir)
        {
            case Direction.none:
                return die.horizontal[1]; //or 4
            case Direction.up:
                return die.vertical[2];
            case Direction.right:
                return die.horizontal[0];
            case Direction.down:
                return die.vertical[0];
            case Direction.left:
                return die.horizontal[2];
            default:
                break;
        }
        return -1;
    }

}
