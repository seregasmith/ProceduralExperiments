public class GridVector {
    public static GridVector plusX = new GridVector(1, 0);
    public static GridVector minusX = new GridVector(-1, 0);
    public static GridVector plusY = new GridVector(0, 1);
    public static GridVector minusY = new GridVector(0, -1);
    public static GridVector zero = new GridVector(0, 0);

    public int x;
    public int y;

    public GridVector minimized
    {
        get
        {
            var m = new GridVector(this);
            if (m.x > 0) m.x = 1;
            if (m.x < 0) m.x = -1;
            if (m.y > 0) m.y = 1;
            if (m.y < 0) m.y = -1;
            return m;
        }
    }

    public GridVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public GridVector(GridVector point)
    {
        x = point.x;
        y = point.y;
    }

    public static GridVector operator +(GridVector a, GridVector b)
    {
        return new GridVector(a.x + b.x, a.y + b.y);
    }

    public static GridVector operator -(GridVector a, GridVector b)
    {
        return new GridVector(a.x - b.x, a.y - b.y);
    }

    public static GridVector operator /(GridVector a, int b)
    {
        return new GridVector(a.x / b, a.y / b);
    }

    public static GridVector operator *(GridVector a, int b)
    {
        return new GridVector(a.x * b, a.y * b);
    }

    public static bool operator ==(GridVector a, GridVector b)
    {
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        if ((object)a == null || (object)b == null)
        {
            return false;
        }
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(GridVector a, GridVector b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}