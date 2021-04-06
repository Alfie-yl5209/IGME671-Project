using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<T, U>
{
    public Pair()
    {
    }

    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }

    public T First { get; set; }
    public U Second { get; set; }
};


public class Coord<T, U>
{
    public Coord()
    {
    }

    public Coord(T x, U y)
    {
        this.x = x;
        this.y = y;
    }

    public T x { get; set; }
    public U y { get; set; }
};