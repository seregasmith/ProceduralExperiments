using System;

public class RoomWall
{
    public bool canGrow;
    public GridVector start;
    public GridVector end;

    public GridVector outwards
    {
        get { return new GridVector(-direction.y, direction.x); }
    }

    public GridVector direction
    {
        get { return end - start; }
    }

    public int length
    {
        get { return Math.Abs(end.x - start.x) + Math.Abs(end.y - start.y); }
    }

    public RoomWall()
    {
    }

    public RoomWall(GridVector start, GridVector end)
    {
        this.start = start;
        this.end = end;
    }

    public RoomWall(RoomWall wall)
    {
        start = wall.start;
        end = wall.end;
    }

    public static int CompareByLength(RoomWall a, RoomWall b)
    {
        if (a.length > b.length) return 1;
        if (a.length == b.length) return 0;
        return -1;
    }

    public static RoomWall operator +(RoomWall a, GridVector b)
    {
        var result = new RoomWall(a);
        result.start += b;
        result.end += b;
        return result;
    }

    public static RoomWall operator -(RoomWall a, GridVector b)
    {
        var result = new RoomWall(a);
        result.start -= b;
        result.end -= b;
        return result;
    }
}