using System.Collections.Generic;
using UnityEngine;

public class Room
{
    // Примыкающие стены и углы считаются по часовой стрелке, если смотреть из центра комнаты
    public List<GridVector> corners;
    public Color color;
    public bool canSpawn = false;
    public List<RoomWall> walls
    {
        get
        {
            var roomWalls = new List<RoomWall>();
            for (var i = 0; i < corners.Count; i++)
            {
                roomWalls.Add(new RoomWall(corners[i], i == corners.Count - 1 ? corners[0] : corners[i + 1]));
            }
            return roomWalls;
        }
    }
    public int perimeter
    {
        get
        {
            var p = 0;
            foreach (var wall in walls)
            {
                p += wall.length;
            }
            return p;
        }
    }

    public Room(Color color, List<GridVector> corners)
    {
        this.color = color;
        this.corners = corners;
        SortCorners();
    }

    public GridVector SortCorners()
    {
        // Ищем границы комнаты
        var minX = corners[0].x;
        var maxX = corners[0].x;
        var minY = corners[0].y;
        var maxY = corners[0].y;
        foreach (var corner in corners)
        {
            if (corner.x < minX) minX = corner.x;
            if (corner.x > maxX) maxX = corner.x;
            if (corner.y < minY) minY = corner.y;
            if (corner.y > maxY) maxY = corner.y;
        }

        // Сортируем углы комнаты
        var oldC = new List<GridVector>(corners);
        var newC = new List<GridVector>();
        bool parallelX = false;
        while (oldC.Count > 1)
        {
            // Ищем первый угол
            if (newC.Count == 0)
            {
                if (ScanUp(ref oldC, ref newC, minX, minY, maxY)) continue;
                if (ScanRight(ref oldC, ref newC, minX, minY, maxX)) continue;
                if (ScanDown(ref oldC, ref newC, minX, minY, minY)) continue;
                if (!ScanLeft(ref oldC, ref newC, minX, minY, minX))
                {
                    Debug.Log("Error on start");
                    return null;
                }
            }
                // Ищем остальные углы
            else
            {
                var last = newC[newC.Count - 1];
                if (parallelX)
                {
                    if (ScanRight(ref oldC, ref newC, last.x, last.y, maxX))
                    {
                        parallelX = false;
                        continue;
                    }
                    if (ScanLeft(ref oldC, ref newC, last.x, last.y, minX))
                    {
                        parallelX = false;
                        continue;
                    }
                }
                else
                {
                    if (ScanUp(ref oldC, ref newC, last.x, last.y, maxY))
                    {
                        parallelX = true;
                        continue;
                    }
                    if (ScanDown(ref oldC, ref newC, last.x, last.y, minY))
                    {
                        parallelX = true;
                        continue;
                    }
                }
                Debug.Log("Error -------------------------------------------------");
                Debug.Log("Corners: " + corners.Count);
                Debug.Log("OldC: " + oldC.Count);
                Debug.Log("NewC: " + newC.Count);
                Debug.Log(last);
                color = Color.red;
                return last;
            }
        }
        // Добавляем последний оставшийся угол
        newC.Add(oldC[0]);
        corners = newC;
        return null;
    }

    bool ScanLeft(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int minX)
    {
        for (var x = startX; x >= minX; x--)
        {
            var index = oldC.FindIndex(gv => gv.x == x && gv.y == startY);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanUp(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int maxY)
    {
        for (var y = startY; y <= maxY; y++)
        {
            var index = oldC.FindIndex(gv => gv.x == startX && gv.y == y);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanRight(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int maxX)
    {
        for (var x = startX; x <= maxX; x++)
        {
            var index = oldC.FindIndex(gv => gv.x == x && gv.y == startY);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanDown(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int minY)
    {
        for (var y = startY; y >= minY; y--)
        {
            var index = oldC.FindIndex(gv => gv.x == startX && gv.y == y);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    public void GrowWall(RoomWall wall)
    {
        for (var i = 0; i < corners.Count; i++)
        {
            if (i < corners.Count - 1)
            {
                if (corners[i] == wall.start && corners[i + 1] == wall.end)
                {
                    corners[i] += wall.outwards.minimized;
                    corners[i + 1] += wall.outwards.minimized;
                    return;
                }
                if (corners[i] == wall.end && corners[i + 1] == wall.start)
                {
                    corners[i] -= wall.outwards.minimized;
                    corners[i + 1] -= wall.outwards.minimized;
                    return;
                }
            }
            else
            {
                if (corners[i] == wall.start && corners[0] == wall.end)
                {
                    corners[i] += wall.outwards.minimized;
                    corners[0] += wall.outwards.minimized;
                    return;
                }
                if (corners[i] == wall.end && corners[0] == wall.start)
                {
                    corners[i] -= wall.outwards.minimized;
                    corners[0] -= wall.outwards.minimized;
                    return;
                }
            }
        }
    }

    public void AddWall(RoomWall wall)
    {
        for (var i = 0; i < corners.Count; i++)
        {
            if (corners[i] == wall.start)
            {
                corners.Add(wall.end);
                corners.Add(wall.end);
                SortCorners();
                return;
            }
            if (corners[i] == wall.end)
            {
                corners.Add(wall.start);
                corners.Add(wall.start);
                SortCorners();
                return;
            }
        }
        corners.Add(wall.start);
        corners.Add(wall.start);
        corners.Add(wall.end);
        corners.Add(wall.end);
        SortCorners();
    }

    public static int CompareByPerimeter(Room a, Room b)
    {
        if (a.perimeter > b.perimeter) return 1;
        if (a.perimeter == b.perimeter) return 0;
        return -1;
    }
}