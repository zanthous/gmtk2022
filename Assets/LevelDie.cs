using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDie : Tile
{

    private int[] dieVertical;
    private int[] dieHorizontal;
    private int backNumber;

    public int Face
    {
        get { return this.dieHorizontal[1]; }
    }

    private void Start()
    {
        dieVertical = new int[3] { 2, 1, 5 };
        dieHorizontal = new int[3] { 4, 1, 3 };
        backNumber = 6;
    }

    public override void Tick(int dieFace)
    {
    }

    public int[] GetDie()
    {
        int[] dieFaces = new int[] {
            dieVertical[0], dieVertical[1], dieVertical[2],
            dieHorizontal[0], dieHorizontal[1], dieHorizontal[2],
            backNumber
        };
        return dieFaces;
    }

    public void SetDie(int[] dieFaces)
    {
        if(dieFaces.Length != 7)
        {
            Debug.LogError("Player.SetDie wrong length");
            return;
        }

        dieVertical[0] = dieFaces[0];
        dieVertical[1] = dieFaces[1];
        dieVertical[2] = dieFaces[2];
        dieHorizontal[0] = dieFaces[4];
        dieHorizontal[1] = dieFaces[5];
        dieHorizontal[2] = dieFaces[6];
        backNumber = dieFaces[7];
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

}
