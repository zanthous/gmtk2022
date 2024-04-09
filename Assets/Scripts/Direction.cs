using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    none = -1,
    up = 0,
    right = 1,
    down = 2,
    left = 3
}

/// <summary>
/// Direction helper class, easily add Vector2 with a direction
/// </summary>
public static class Dir
{
    public static Dictionary<Direction, (int, int)> dir =
        new Dictionary<Direction, (int, int)>(new DirectionComparer())
    {                                     
            { Direction.none,  (0,0) },
            { Direction.up,    (0,1) },
            { Direction.right, (1,0) },
            { Direction.down,  (0,-1) },
            { Direction.left,  (-1,0) }
    };

    public static Dictionary<(int, int), Direction> dirTuple =
        new Dictionary<(int, int), Direction>()
    {
            {(0,0) ,  Direction.none},
            {(0,1) ,  Direction.up },
            {(1,0) ,  Direction.right},
            {(0,-1),  Direction.down },
            {(-1,0),  Direction.left }
    };

    public static int Y(Direction d)
    {
        return dir[d].Item2;
    }
    public static int X(Direction d)
    {
        return dir[d].Item1;
    }

    public static void Add(ref Vector2 pos, Direction direction)
    {
        pos.x += dir[direction].Item1;
        pos.y += dir[direction].Item2;
    }
    public static void Add(ref(int, int)pos, Direction direction)
    {
        pos.Item1 += dir[direction].Item1;
        pos.Item2 += dir[direction].Item2;
    }

    public static ValueTuple<int,int> Add((int, int) pos, Direction direction)
    {
       (int, int)result;
        result.Item1 = pos.Item1 + dir[direction].Item1;
        result.Item2 = pos.Item2 + dir[direction].Item2;
        return result;
    }

    public static Vector3 Rotation90(Direction dir)
    {
        switch(dir)
        {
            case Direction.none:
                return new Vector3();
            case Direction.up:
                return new Vector3(90, 0.0f, 0.0f);
            case Direction.right:
                return new Vector3(0.0f, 0.0f, -90.0f);
            case Direction.down:
                return new Vector3(-90, 0.0f, 0.0f);
            case Direction.left:
                return new Vector3(90, 0.0f, 0.0f);
        }
        return new Vector3();
    }

    /// <summary>
    /// https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html
    /// Avoid boxing with custom comparer for enums
    /// </summary>
    public class DirectionComparer : IEqualityComparer<Direction>
    {
        public bool Equals(Direction x, Direction y)
        {
            return x == y;
        }
        public int GetHashCode(Direction x)
        {
            return (int) x;
        }
    }
}