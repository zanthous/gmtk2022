using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levels 
{

    private Leveldata[] levels;

    private Dictionary<int, int> indexToLevel = new Dictionary<int, int>()
    {
        {0, 9},
        //{0, 2},
        {1, 3},
        {2, 0},
        {3, 1},
        {4, 4},
        {5, 5},
        {6, 6},
        {7,7 },
        {8,8}
    };

    public Leveldata GetLevel(int index)
    {
        return levels[indexToLevel[index]];
    }

    public void Init()
    {
        levels = new Leveldata[10];


        //level definitions
        levels[0].name = "Lazer";
        levels[0].width = 5;
        levels[0].height = 7;

        levels[1].name = "Narrow escape";
        levels[1].width = 7;
        levels[1].height = 9;

        //test level
        levels[2].name = "Intro";
        levels[2].width = 5;
        levels[2].height = 5;

        levels[3].name = "blocktest";
        levels[3].width = 7;
        levels[3].height = 7;

        levels[4].name = "tricky";
        levels[4].width = 7;
        levels[4].height = 7;

        levels[5].name = "whatever";
        levels[5].width = 9;
        levels[5].height = 9;

        levels[6].name = "diediedie";
        levels[6].width = 7;
        levels[6].height = 7;

        levels[7].name = "surrounded"; //terrible
        levels[7].width = 7;
        levels[7].height = 8;

        levels[8].name = "surrounded2";
        levels[8].width = 5;
        levels[8].height = 6;


        //fill blank tiles
        for(int i = 0; i < levels.Length; i++)
        {
            levels[i].data = new Tiledata[levels[i].width,levels[i].height];
            for(int x = 0; x < levels[i].width; x++)
            {
                for(int y = 0; y < levels[i].height; y++)
                {
                    levels[i].data[x, y] = new Tiledata((int) tileID.empty, Direction.none, -1);
                }
            }
        }

        //level obstacles
        //level 0
        levels[0].data[2, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[0].data[2, levels[0].height - 1] = new Tiledata(tileID.goal, Direction.none, 2);
        levels[0].data[4, 3] = new Tiledata(tileID.lazer, Direction.left, 2);

        //level 1
        levels[1].data[3, 0] =                      new Tiledata(tileID.spawn,  Direction.none,     -1);
        levels[1].data[3, levels[1].height - 1] =   new Tiledata(tileID.goal,   Direction.none,     -1);
        levels[1].data[0, 4] =                      new Tiledata(tileID.lazer,  Direction.right,    4,  not: true);
        levels[1].data[0, 5] =                      new Tiledata(tileID.lazer,  Direction.right,    1,  not: true);
        levels[1].data[0, 6] =                      new Tiledata(tileID.lazer,  Direction.right,    3,  not: true);
        levels[1].data[0, 7] =                      new Tiledata(tileID.lazer,  Direction.right,    6,  not: true);

        //level 2 test level
        levels[2].data[0, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[2].data[levels[2].width - 1, levels[2].height - 1] = new Tiledata(tileID.goal, Direction.none, 6);

        //level 3
        levels[3].data[0, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[3].data[levels[3].width - 1, levels[3].height - 1] = new Tiledata(tileID.goal, Direction.none, 2);

        levels[3].data[2, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[3, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[4, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[2, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[3, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[4, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[2, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[3, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[3].data[4, 4] = new Tiledata(tileID.block, Direction.none, -1);

        //level 4

        levels[4].data[0, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[4].data[3, levels[4].height - 1] = new Tiledata(tileID.goal, Direction.none, -1);

        levels[4].data[0, 1] = new Tiledata(tileID.lazer, Direction.right, 5, not: true);
        levels[4].data[0, 2] = new Tiledata(tileID.lazer, Direction.right, 1, not: true);
        levels[4].data[3, 2] = new Tiledata(tileID.lazer, Direction.right, 3);
        levels[4].data[levels[4].width - 1, 3] = new Tiledata(tileID.lazer, Direction.left, 4);
        levels[4].data[levels[4].width - 1, 4] = new Tiledata(tileID.lazer, Direction.left, 2, not: true);

        levels[4].data[0,4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[4].data[1, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[4].data[2, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[4].data[3, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[4].data[4, 4] = new Tiledata(tileID.block, Direction.none, -1);

        //level 5
        levels[5].data[0, 0] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[5].data[levels[5].width - 1, levels[5].height - 1] = new Tiledata(tileID.goal, Direction.none, 3);

        levels[5].data[2, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[3, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[5, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 1] = new Tiledata(tileID.block, Direction.none, -1);

        levels[5].data[1, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[2, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[7, 2] = new Tiledata(tileID.block, Direction.none, -1);

        levels[5].data[1, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[2, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 3] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[7, 3] = new Tiledata(tileID.block, Direction.none, -1);

        levels[5].data[1, 5] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[2, 5] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 5] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[7, 5] = new Tiledata(tileID.block, Direction.none, -1);

        levels[5].data[1, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[2, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[7, 6] = new Tiledata(tileID.block, Direction.none, -1);

        levels[5].data[2, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[3, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[5, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[6, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[7, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[5].data[8, 7] = new Tiledata(tileID.block, Direction.none, -1);


        levels[5].data[8, 4] = new Tiledata(tileID.lazer, Direction.left, 5, not:true);
        levels[5].data[4, 0] = new Tiledata(tileID.lazer, Direction.up, 2);

        //level 6
        levels[6].data[3, 1] = new Tiledata(tileID.spawn, Direction.none, -1);
        levels[6].data[3, 5] = new Tiledata(tileID.goal, Direction.none, 5);

        levels[6].data[0, 1] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[6].data[6, 1] = new Tiledata(tileID.levelDie, Direction.none, -1);

        levels[6].data[1, 3] = new Tiledata(tileID.lazer, Direction.right, -1, not: true);

        levels[6].data[1, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[5, 1] = new Tiledata(tileID.block, Direction.none, -1);

        levels[6].data[1, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[5, 2] = new Tiledata(tileID.block, Direction.none, -1);

        levels[6].data[5, 3] = new Tiledata(tileID.block, Direction.none, -1);

        levels[6].data[1, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[5, 4] = new Tiledata(tileID.block, Direction.none, -1);

        levels[6].data[1, 5] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[5, 5] = new Tiledata(tileID.block, Direction.none, -1);

        levels[6].data[1, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[2, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[3, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[4, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[6].data[5, 6] = new Tiledata(tileID.block, Direction.none, -1);

        //level 7
        levels[7].data[3, 2] = new Tiledata(tileID.spawn, Direction.none, 5);
        levels[7].data[3, 6] = new Tiledata(tileID.goal, Direction.none, 3);

        levels[7].data[0, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[2, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[4, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[6, 0] = new Tiledata(tileID.block, Direction.none, -1);

        levels[7].data[1, 1] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[7].data[3, 1] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[7].data[5, 1] = new Tiledata(tileID.levelDie, Direction.none, -1);

        levels[7].data[0, 2] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[6, 2] = new Tiledata(tileID.block, Direction.none, -1);

        levels[7].data[1, 3] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[7].data[3, 3] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[7].data[5, 3] = new Tiledata(tileID.levelDie, Direction.none, -1);

        levels[7].data[0, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[2, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[4, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[6, 4] = new Tiledata(tileID.block, Direction.none, -1);

        levels[7].data[3, 5] = new Tiledata(tileID.levelDie, Direction.none, -1);

        levels[7].data[0, 6] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[6, 6] = new Tiledata(tileID.block, Direction.none, -1);

        levels[7].data[0, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[6, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[2, 7] = new Tiledata(tileID.block, Direction.none, -1);
        levels[7].data[4, 7] = new Tiledata(tileID.block, Direction.none, -1);

        //level 8
        levels[8].data[2, 1] = new Tiledata(tileID.spawn, Direction.none, 5);
        levels[8].data[2, 5] = new Tiledata(tileID.goal, Direction.none, 3);

        levels[8].data[1, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[0, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[4, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[3, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[2, 0] = new Tiledata(tileID.levelDie, Direction.none, -1);

        levels[8].data[1, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[4, 1] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[1, 0] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[3, 1] = new Tiledata(tileID.block, Direction.none, -1);

        levels[8].data[4, 2] = new Tiledata(tileID.block, Direction.none, -1);

        levels[8].data[2, 3] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[8].data[1, 3] = new Tiledata(tileID.block, Direction.none, -1);

        levels[8].data[1, 4] = new Tiledata(tileID.block, Direction.none, -1);
        levels[8].data[2, 4] = new Tiledata(tileID.levelDie, Direction.none, -1);
        levels[8].data[3, 4] = new Tiledata(tileID.block, Direction.none, -1);

        
        

    }
}
