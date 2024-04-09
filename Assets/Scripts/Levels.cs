using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Didn't use a level editor for the gamejam, just wrote the levels out for speed
public class Levels 
{
    private Leveldata[] levels;

    public int LevelCount { get { return levels.Length; } }

    private Dictionary<int, int> indexToLevel = new Dictionary<int, int>()
    {
        //{0, 11},
        {0, 2},
        {1, 3},
        {2, 0},
        {3, 1},
        {4, 4},
        {5, 5},
        {6, 6},
        {7, 7},
        {8, 8},
        {9, 9},
        {10,10},
        {11,11}
    };

    public Leveldata GetLevel(int index)
    {
        return levels[indexToLevel[index]];
    }

    public void Init()
    {
        levels = new Leveldata[12];


        //level definitions
        levels[0].name = "Lazer";
        levels[0].width = 5;
        levels[0].height = 7;

        levels[1].name = "Particular";
        levels[1].width = 7;
        levels[1].height = 9;

        //test level
        levels[2].name = "Welcome";
        levels[2].width = 5;
        levels[2].height = 5;

        levels[3].name = "Border";
        levels[3].width = 7;
        levels[3].height = 7;

        levels[4].name = "Matrix";
        levels[4].width = 7;
        levels[4].height = 7;

        levels[5].name = "Corners";
        levels[5].width = 9;
        levels[5].height = 9;

        levels[6].name = "Friends";
        levels[6].width = 7;
        levels[6].height = 7;

        levels[7].name = "Traffic"; //terrible
        levels[7].width = 7;
        levels[7].height = 8;

        levels[8].name = "Junction";
        levels[8].width = 5;
        levels[8].height = 6;

        levels[9].name = "Anniversary";
        levels[9].width = 7;
        levels[9].height = 7;

        levels[10].name = "Separate";
        levels[10].width = 8;
        levels[10].height = 8;

        levels[11].name = "Farewell"; 
        levels[11].width = 9;
        levels[11].height = 9;
        levels[11].cameraSize = 6.0f;

        //fill blank tiles
        for(int i = 0; i < levels.Length; i++)
        {
            levels[i].data = new Tiledata[levels[i].width,levels[i].height];
            for(int x = 0; x < levels[i].width; x++)
            {
                for(int y = 0; y < levels[i].height; y++)
                {
                    levels[i].data[x, y] = new Tiledata(EntityID.empty);
                }
            }
        }

        //level obstacles, sometimes the combination not:true and -1 are used to mean any number is valid
        //level 0
        levels[0].data[2, 0] = new Tiledata(EntityID.spawn);
        levels[0].data[2, levels[0].height - 1] = new Tiledata(EntityID.goal, 2);
        levels[0].data[4, 3] = new Tiledata(EntityID.lazer, 2, Direction.left);

        //level 1
        levels[1].data[3, 0] =                      new Tiledata(EntityID.spawn);
        levels[1].data[3, levels[1].height - 1] =   new Tiledata(EntityID.goal);
        levels[1].data[0, 4] =                      new Tiledata(EntityID.lazer,  4,  Direction.right,    not: true);
        levels[1].data[0, 5] =                      new Tiledata(EntityID.lazer,  1,  Direction.right,    not: true);
        levels[1].data[0, 6] =                      new Tiledata(EntityID.lazer,  3,  Direction.right,    not: true);
        levels[1].data[0, 7] =                      new Tiledata(EntityID.lazer,  6,  Direction.right,    not: true);

        //level 2 test level
        levels[2].data[0, 0] = new Tiledata(EntityID.spawn );
        levels[2].data[levels[2].width - 1, levels[2].height - 1] = new Tiledata(EntityID.goal, 6, Direction.none);

        levels[2].data[4, 0] = new Tiledata(EntityID.hole);
        levels[2].data[0, 4] = new Tiledata(EntityID.hole);
        levels[2].data[3, 0] = new Tiledata(EntityID.hole);
        levels[2].data[0, 3] = new Tiledata(EntityID.hole);
        levels[2].data[4, 1] = new Tiledata(EntityID.hole);
        levels[2].data[1, 4] = new Tiledata(EntityID.hole);

        //level 3
        levels[3].data[0, 0] = new Tiledata(EntityID.spawn);
        levels[3].data[levels[3].width - 1, levels[3].height - 1] = new Tiledata(EntityID.goal, 2, Direction.none);

        levels[3].data[2, 2] = new Tiledata(EntityID.block);
        levels[3].data[3, 2] = new Tiledata(EntityID.block);
        levels[3].data[4, 2] = new Tiledata(EntityID.block);
        levels[3].data[2, 3] = new Tiledata(EntityID.block);
        levels[3].data[3, 3] = new Tiledata(EntityID.block);
        levels[3].data[4, 3] = new Tiledata(EntityID.block);
        levels[3].data[2, 4] = new Tiledata(EntityID.block);
        levels[3].data[3, 4] = new Tiledata(EntityID.block);
        levels[3].data[4, 4] = new Tiledata(EntityID.block);

        //level 4

        levels[4].data[0, 0] = new Tiledata(EntityID.spawn);
        levels[4].data[3, levels[4].height - 1] = new Tiledata(EntityID.goal);

        levels[4].data[0, 1] = new Tiledata(EntityID.lazer, 5, Direction.right, not: true);
        levels[4].data[0, 2] = new Tiledata(EntityID.lazer, 1, Direction.right, not: true);
        levels[4].data[3, 2] = new Tiledata(EntityID.lazer, 3, Direction.right);
        levels[4].data[levels[4].width - 1, 3] = new Tiledata(EntityID.lazer, 4, Direction.left);
        levels[4].data[levels[4].width - 1, 4] = new Tiledata(EntityID.lazer, 2, Direction.left, not: true);

        levels[4].data[0,4] = new Tiledata(EntityID.block);
        levels[4].data[1, 4] = new Tiledata(EntityID.block);
        levels[4].data[2, 4] = new Tiledata(EntityID.block);
        levels[4].data[3, 4] = new Tiledata(EntityID.block);
        levels[4].data[4, 4] = new Tiledata(EntityID.block);

        levels[4].data[3, 0] = new Tiledata(EntityID.block);
        levels[4].data[3, 1] = new Tiledata(EntityID.block);

        //level 5
        levels[5].data[0, 0] = new Tiledata(EntityID.spawn);
        levels[5].data[levels[5].width - 1, levels[5].height - 1] = new Tiledata(EntityID.goal, 3, Direction.none);

        levels[5].data[2, 1] = new Tiledata(EntityID.block);
        levels[5].data[3, 1] = new Tiledata(EntityID.block);
        levels[5].data[5, 1] = new Tiledata(EntityID.block);
        levels[5].data[6, 1] = new Tiledata(EntityID.block);

        levels[5].data[1, 2] = new Tiledata(EntityID.block);
        levels[5].data[2, 2] = new Tiledata(EntityID.block);
        levels[5].data[6, 2] = new Tiledata(EntityID.block);
        levels[5].data[7, 2] = new Tiledata(EntityID.block);

        levels[5].data[1, 3] = new Tiledata(EntityID.block);
        levels[5].data[2, 3] = new Tiledata(EntityID.block);
        levels[5].data[6, 3] = new Tiledata(EntityID.block);
        levels[5].data[7, 3] = new Tiledata(EntityID.block);

        levels[5].data[1, 5] = new Tiledata(EntityID.block);
        levels[5].data[2, 5] = new Tiledata(EntityID.block);
        levels[5].data[6, 5] = new Tiledata(EntityID.block);
        levels[5].data[7, 5] = new Tiledata(EntityID.block);

        levels[5].data[1, 6] = new Tiledata(EntityID.block);
        levels[5].data[2, 6] = new Tiledata(EntityID.block);
        levels[5].data[6, 6] = new Tiledata(EntityID.block);
        levels[5].data[7, 6] = new Tiledata(EntityID.block);

        levels[5].data[2, 7] = new Tiledata(EntityID.block);
        levels[5].data[3, 7] = new Tiledata(EntityID.block);
        levels[5].data[5, 7] = new Tiledata(EntityID.block);
        levels[5].data[6, 7] = new Tiledata(EntityID.block);
        levels[5].data[7, 7] = new Tiledata(EntityID.block);
        levels[5].data[8, 7] = new Tiledata(EntityID.block);


        levels[5].data[8, 4] = new Tiledata(EntityID.lazer, 5, Direction.left, not:true);
        levels[5].data[4, 0] = new Tiledata(EntityID.lazer, 2, Direction.up);

        //level 6
        levels[6].data[3, 1] = new Tiledata(EntityID.spawn);
        levels[6].data[3, 5] = new Tiledata(EntityID.goal, 5, Direction.none);

        levels[6].data[0, 1] = new Tiledata(EntityID.levelDie);
        levels[6].data[6, 1] = new Tiledata(EntityID.levelDie);

        levels[6].data[1, 3] = new Tiledata(EntityID.lazer, -1, Direction.right, not: true);
        levels[6].data[5, 3] = new Tiledata(EntityID.lazer, -1, Direction.left, not: true);

        levels[6].data[1, 1] = new Tiledata(EntityID.block);
        levels[6].data[5, 1] = new Tiledata(EntityID.block);

        levels[6].data[1, 2] = new Tiledata(EntityID.block);
        levels[6].data[5, 2] = new Tiledata(EntityID.block);

        levels[6].data[1, 4] = new Tiledata(EntityID.block);
        levels[6].data[5, 4] = new Tiledata(EntityID.block);

        levels[6].data[1, 5] = new Tiledata(EntityID.block);
        levels[6].data[5, 5] = new Tiledata(EntityID.block);

        levels[6].data[1, 6] = new Tiledata(EntityID.block);
        levels[6].data[2, 6] = new Tiledata(EntityID.block);
        levels[6].data[3, 6] = new Tiledata(EntityID.block);
        levels[6].data[4, 6] = new Tiledata(EntityID.block);
        levels[6].data[5, 6] = new Tiledata(EntityID.block);

        //level 7
        levels[7].data[3, 2] = new Tiledata(EntityID.spawn, 5, Direction.none);
        levels[7].data[3, 6] = new Tiledata(EntityID.goal, 3, Direction.none);

        levels[7].data[0, 0] = new Tiledata(EntityID.block);
        levels[7].data[2, 0] = new Tiledata(EntityID.block);
        levels[7].data[4, 0] = new Tiledata(EntityID.block);
        levels[7].data[6, 0] = new Tiledata(EntityID.block);

        levels[7].data[1, 1] = new Tiledata(EntityID.levelDie);
        levels[7].data[3, 1] = new Tiledata(EntityID.levelDie);
        levels[7].data[5, 1] = new Tiledata(EntityID.levelDie);

        levels[7].data[0, 2] = new Tiledata(EntityID.block);
        levels[7].data[6, 2] = new Tiledata(EntityID.block);

        levels[7].data[1, 3] = new Tiledata(EntityID.levelDie);
        levels[7].data[3, 3] = new Tiledata(EntityID.levelDie);
        levels[7].data[5, 3] = new Tiledata(EntityID.levelDie);

        levels[7].data[0, 4] = new Tiledata(EntityID.block);
        levels[7].data[2, 4] = new Tiledata(EntityID.block);
        levels[7].data[4, 4] = new Tiledata(EntityID.block);
        levels[7].data[6, 4] = new Tiledata(EntityID.block);

        levels[7].data[3, 5] = new Tiledata(EntityID.levelDie);

        levels[7].data[0, 6] = new Tiledata(EntityID.block);
        levels[7].data[6, 6] = new Tiledata(EntityID.block);

        levels[7].data[0, 7] = new Tiledata(EntityID.block);
        levels[7].data[6, 7] = new Tiledata(EntityID.block);
        levels[7].data[2, 7] = new Tiledata(EntityID.block);
        levels[7].data[4, 7] = new Tiledata(EntityID.block);

        //level 8
        levels[8].data[2, 1] = new Tiledata(EntityID.spawn, 5, Direction.none);
        levels[8].data[2, 5] = new Tiledata(EntityID.goal,  3, Direction.none);

        levels[8].data[1, 0] = new Tiledata(EntityID.block);
        levels[8].data[0, 0] = new Tiledata(EntityID.block);
        levels[8].data[4, 0] = new Tiledata(EntityID.block);
        levels[8].data[3, 0] = new Tiledata(EntityID.block);
        levels[8].data[2, 0] = new Tiledata(EntityID.levelDie);

        levels[8].data[1, 1] = new Tiledata(EntityID.block);
        levels[8].data[4, 1] = new Tiledata(EntityID.block);
        levels[8].data[1, 0] = new Tiledata(EntityID.block);
        levels[8].data[3, 1] = new Tiledata(EntityID.block);

        levels[8].data[4, 2] = new Tiledata(EntityID.block);

        levels[8].data[2, 3] = new Tiledata(EntityID.levelDie);
        levels[8].data[1, 3] = new Tiledata(EntityID.block);

        levels[8].data[1, 4] = new Tiledata(EntityID.block);
        levels[8].data[2, 4] = new Tiledata(EntityID.levelDie);
        levels[8].data[3, 4] = new Tiledata(EntityID.block);

        //level 9
        levels[9].data[0, 3] = new Tiledata(EntityID.goal, 3, Direction.none);
        levels[9].data[6, 3] = new Tiledata(EntityID.spawn);

        //levels[9].data[0, 0] = new Tiledata(tileID.block);

        levels[9].data[1, 1] = new Tiledata(EntityID.block);
        levels[9].data[2, 1] = new Tiledata(EntityID.block);
        levels[9].data[3, 1] = new Tiledata(EntityID.block);
        levels[9].data[6, 1] = new Tiledata(EntityID.block);

        levels[9].data[3, 2] = new Tiledata(EntityID.block);

        levels[9].data[1, 3] = new Tiledata(EntityID.block);
        levels[9].data[2, 3] = new Tiledata(EntityID.block);
        levels[9].data[3, 3] = new Tiledata(EntityID.block);

        levels[9].data[3, 4] = new Tiledata(EntityID.block);

        levels[9].data[1, 5] = new Tiledata(EntityID.block);
        levels[9].data[2, 5] = new Tiledata(EntityID.block);
        levels[9].data[3, 5] = new Tiledata(EntityID.block);
        levels[9].data[6, 5] = new Tiledata(EntityID.block);

        levels[9].data[2, 2] = new Tiledata(EntityID.levelDie);
        levels[9].data[2, 4] = new Tiledata(EntityID.levelDie);

        levels[9].data[6, 2] = new Tiledata(EntityID.pressurePlate, not: true);
        levels[9].data[6, 4] = new Tiledata(EntityID.pressurePlate, not: true);

        //level 10
        levels[10].data[7, 0] = new Tiledata(EntityID.spawn);
        levels[10].data[3, 3] = new Tiledata(EntityID.goal, 6, Direction.none);

        levels[10].data[0, 7] = new Tiledata(EntityID.levelDie);

        levels[10].data[7, 2] = new Tiledata(EntityID.pressurePlatePlayer, 1, Direction.none);
        levels[10].data[7, 7] = new Tiledata(EntityID.pressurePlatePlayer, 1, Direction.none);

        levels[10].data[1, 1] = new Tiledata(EntityID.block);
        levels[10].data[6, 6] = new Tiledata(EntityID.block);

        levels[10].data[0, 3] = new Tiledata(EntityID.block);
        levels[10].data[1, 3] = new Tiledata(EntityID.block);
        levels[10].data[2, 3] = new Tiledata(EntityID.block);
        levels[10].data[4, 3] = new Tiledata(EntityID.block);
        levels[10].data[5, 3] = new Tiledata(EntityID.block);
        levels[10].data[6, 3] = new Tiledata(EntityID.block);
        levels[10].data[7, 3] = new Tiledata(EntityID.block);

        levels[10].data[0, 4] = new Tiledata(EntityID.block);
        levels[10].data[1, 4] = new Tiledata(EntityID.block);
        levels[10].data[2, 4] = new Tiledata(EntityID.block);
        levels[10].data[3, 4] = new Tiledata(EntityID.block);
        levels[10].data[4, 4] = new Tiledata(EntityID.block);
        levels[10].data[5, 4] = new Tiledata(EntityID.block);
        levels[10].data[6, 4] = new Tiledata(EntityID.block);
        levels[10].data[7, 4] = new Tiledata(EntityID.block);

        levels[10].data[0, 5] = new Tiledata(EntityID.block);
        levels[10].data[1, 5] = new Tiledata(EntityID.block);
        levels[10].data[2, 5] = new Tiledata(EntityID.block);
        levels[10].data[3, 5] = new Tiledata(EntityID.block);
        levels[10].data[4, 5] = new Tiledata(EntityID.block);
        levels[10].data[5, 5] = new Tiledata(EntityID.block);
        levels[10].data[6, 5] = new Tiledata(EntityID.block);
        levels[10].data[7, 5] = new Tiledata(EntityID.block);

        levels[10].data[0, 6] = new Tiledata(EntityID.block);
        levels[10].data[1, 6] = new Tiledata(EntityID.block);
        levels[10].data[2, 6] = new Tiledata(EntityID.block);
        levels[10].data[5, 6] = new Tiledata(EntityID.block);
        levels[10].data[6, 6] = new Tiledata(EntityID.block);
        levels[10].data[7, 6] = new Tiledata(EntityID.block);

        //level 11
        levels[11].data[1, 7] = new Tiledata(EntityID.spawn);
        levels[11].data[5, 8] = new Tiledata(EntityID.goal, 6, Direction.none); 

        levels[11].data[0, 7] = new Tiledata(EntityID.levelDie);
        levels[11].data[7, 0] = new Tiledata(EntityID.levelDie);
        levels[11].data[8, 0] = new Tiledata(EntityID.levelDie);

        levels[11].data[0, 8] = new Tiledata(EntityID.pressurePlatePlayer, not:true);
        levels[11].data[3, 8] = new Tiledata(EntityID.pressurePlatePlayer, not: true);
        levels[11].data[8, 8] = new Tiledata(EntityID.pressurePlatePlayer, not: true);

        levels[11].data[7, 6] = new Tiledata(EntityID.lazer, -1, Direction.left, not: true);

        levels[11].data[1, 0] = new Tiledata(EntityID.block);

        levels[11].data[6, 1] = new Tiledata(EntityID.block);
        levels[11].data[7, 1] = new Tiledata(EntityID.block);
        levels[11].data[8, 1] = new Tiledata(EntityID.block);

        levels[11].data[2, 2] = new Tiledata(EntityID.block);

        levels[11].data[7, 3] = new Tiledata(EntityID.block);

        levels[11].data[2, 4] = new Tiledata(EntityID.block);
        levels[11].data[6, 4] = new Tiledata(EntityID.block);
        levels[11].data[7, 4] = new Tiledata(EntityID.block);

        levels[11].data[2, 5] = new Tiledata(EntityID.block);
        levels[11].data[7, 5] = new Tiledata(EntityID.block);

        levels[11].data[2, 6] = new Tiledata(EntityID.block);

        levels[11].data[2, 7] = new Tiledata(EntityID.block);
        levels[11].data[7, 7] = new Tiledata(EntityID.block);

        levels[11].data[2, 8] = new Tiledata(EntityID.block);
        levels[11].data[7, 8] = new Tiledata(EntityID.block);
    }
}
