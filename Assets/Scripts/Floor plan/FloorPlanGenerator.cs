using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class FloorPlanGenerator : MonoBehaviour
{
    public Texture2D testWalls;
    public Texture2D transparent;
    public Renderer scanRenderer;
    public bool growCorners;
    public bool growCenters;
    public bool paused;

    private Texture2D texture;
    private Texture2D scanTexture;
    private Transform tr;
    private Camera cam;
    private RaycastHit selected;
    private System.Random random;
    private List<Room> rooms;
    private int growableRooms;
    private int growableCorners;
    private int bigCorner;
    private int bigCenter;
    private Room bigCornerRoom;
    private Room bigCenterRoom;
    private List<RoomWall> growableWalls;
    private List<RoomWall> cornerSegments;
    private List<RoomWall> segments;
    private List<RoomWall> centerSegments;

    void Start()
    {
        tr = transform;
        cam = Camera.main;
        random = new System.Random(DateTime.Now.Millisecond);
        rooms = new List<Room>();
        growableWalls = new List<RoomWall>();
        cornerSegments = new List<RoomWall>();
        segments = new List<RoomWall>();
        centerSegments = new List<RoomWall>();
        texture = new Texture2D(testWalls.width, testWalls.height) { filterMode = FilterMode.Point };
        scanTexture = new Texture2D(transparent.width, transparent.height) { filterMode = FilterMode.Point };
        ResetTexture();
        ResetDebugTexture();
        RandomRooms();
        //InvokeRepeating("TakeScreenshot", 0, 0.1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            scanRenderer.enabled = !scanRenderer.enabled;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ResetTexture();
            ResetDebugTexture();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            RandomWalls();
            RandomRooms();
            ResetDebugTexture();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            ResetTexture();
            RandomRooms();
            ResetDebugTexture();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out selected) && selected.transform == tr)
            {
                var x = (int)(selected.textureCoord.x * texture.width);
                var y = (int)(selected.textureCoord.y * texture.height);

                if (!CheckRect(x - 1, y - 1, x + 1, y + 1, Color.white, Color.white)) return;

                var color = new Color(Random.value * 0.6f + 0.1f, Random.value * 0.6f + 0.1f,
                                      Random.value * 0.6f + 0.1f);
                rooms.Add(new Room(color,
                                   new List<GridVector>
                               {
                                   new GridVector(x - 1, y - 1),
                                   new GridVector(x - 1, y + 1),
                                   new GridVector(x + 1, y + 1),
                                   new GridVector(x + 1, y - 1)
                               }));
                DrawRoom(rooms[rooms.Count - 1]);
            }
        }

        ResetDebugTexture();
        foreach (var room in rooms)
        {
            foreach (var wall in room.walls)
            {
                BresenhamLine(wall, room.color);
            }
        }
        texture.Apply();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
        if (!paused)
        {
            growableRooms = 0;
            growableCorners = 0;
            bigCorner = 0;
            bigCenter = 0;
            foreach (var room in rooms)
            {
                segments.Clear();
                growableWalls.Clear();
                cornerSegments.Clear();
                centerSegments.Clear();
                
                FindCandidates(room);

                if (cornerSegments.Count > 0)
                {
                    var l = LongWall(cornerSegments).length;
                    if (bigCorner < l && !room.canSpawn)
                    {
                        bigCornerRoom = room;
                        bigCorner = l;
                    }
                }
                if (centerSegments.Count > 0)
                {
                    var l = LongWall(centerSegments).length;
                    if (bigCenter < l)
                    {
                        bigCenterRoom = room;
                        bigCenter = l;
                    }
                }

                GrowRoom(room);
                foreach (var wall in room.walls)
                {
                    BresenhamLine(wall, room.color);
                }
                texture.Apply();
                scanTexture.Apply();
            }
            if (growableRooms == 0)
            {
                if (growableCorners > 0)
                {
                    growCorners = true;
                    bigCornerRoom.canSpawn = true;
                }
                else
                {
                    growCorners = false;
                    growCenters = true;
                    bigCenterRoom.canSpawn = true;
                }
            }
        }
        
        GetComponent<Renderer>().material.mainTexture = texture;
        scanRenderer.material.mainTexture = scanTexture;
    }

    void FindCandidates(Room room)
    {
        foreach (var wall in room.walls)
        {
            segments.AddRange(FindSegments(wall, Color.white, room.color));
            foreach (var segment in segments)
            {
                if ((segment.start == wall.start && segment.end == wall.end) ||
                    (segment.start == wall.end && segment.end == wall.start))
                {
                    growableWalls.Add(segment);
                }
                else if (segment.start == wall.start || segment.end == wall.end ||
                            segment.start == wall.end || segment.end == wall.start)
                {
                    cornerSegments.Add(segment);
                    growableCorners++;
                }
                else
                {
                    centerSegments.Add(segment);
                }
            }
        }
    }

    void GrowRoom(Room room)
    {
        if (growableWalls.Count > 0)
        {
            room.GrowWall(LongWall(growableWalls));
            growableRooms++;
        }
        else if (growCorners)
        {
            if (cornerSegments.Count > 0 && room.canSpawn)
            {
                var wall = LongWall(cornerSegments);
                room.AddWall(wall);
                room.GrowWall(wall);
                room.canSpawn = false;
            }
        }
        else if (growCenters)
        {
            if (cornerSegments.Count > 0)
            {
                var wall = LongWall(cornerSegments);
                room.AddWall(wall);
                room.GrowWall(wall);
            }
            else if (centerSegments.Count > 0)
            {
                var wall = LongWall(centerSegments);
                room.AddWall(wall);
                room.GrowWall(wall);
            }
        }
    }

    List<RoomWall> FindSegments(RoomWall wall, Color freeColor, Color roomColor)
    {
        var moved = wall + wall.outwards.minimized;
        var x0 = moved.start.x;
        var y0 = moved.start.y;
        var x1 = moved.end.x;
        var y1 = moved.end.y;
        var segments = new List<RoomWall>();
        GridVector start = null;
        GridVector end = null;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                int coordX = steep ? y : x;
                int coordY = steep ? x : y;
                Color color = texture.GetPixel(coordX, coordY);
                if (color != freeColor && color != roomColor)
                {
                    if (end != null && start != null)
                    {
                        var segment = new RoomWall(start, end);
                        segment -= wall.outwards.minimized;
                        segments.Add(segment);
                        start = null;
                        end = null;
                    }
                    scanTexture.SetPixel(coordX, coordY, Color.red);
                }
                else
                {
                    if (start == null)
                    {
                        start = new GridVector(coordX, coordY);
                    }
                    end = new GridVector(coordX, coordY);
                    scanTexture.SetPixel(coordX, coordY, Color.green);
                }
            }
        }
        if (end != null && start != null)
        {
            var segment = new RoomWall(start, end);
            segment -= wall.outwards.minimized;
            segments.Add(segment);
        }
        return segments;
    }

    bool CheckRect(int x0, int y0, int x1, int y1, Color freeColor, Color roomColor)
    {
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                Color color = texture.GetPixel(steep ? y : x, steep ? x : y);
                if (color != freeColor && color != roomColor) return false;
            }
        }
        return true;
    }

    bool CheckRect(GridVector start, GridVector end, Color freeColor, Color roomColor)
    {
        return CheckRect(start.x, start.y, end.x, end.y, freeColor, roomColor);
    }

    bool CheckRect(RoomWall wall, Color freeColor, Color roomColor)
    {
        return CheckRect(wall.start, wall.end, freeColor, roomColor);
    }

    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    void BresenhamLine(int x0, int y0, int x1, int y1, Color color, Texture2D tex = null)
    {
        if (tex == null) tex = texture;
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);
        int error = dx/2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            tex.SetPixel(steep ? y : x, steep ? x : y, color);
            error -= dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
    }

    void BresenhamLine(GridVector start, GridVector end, Color color, Texture2D tex = null)
    {
        BresenhamLine(start.x, start.y, end.x, end.y, color, tex);
    }

    void BresenhamLine(RoomWall wall, Color color, Texture2D tex = null)
    {
        BresenhamLine(wall.start, wall.end, color, tex);
    }


    RoomWall LongWall(List<RoomWall> walls)
    {
        walls.Sort(RoomWall.CompareByLength);
        var longWalls = new List<RoomWall>();
        foreach (var line in walls)
        {
            if (line.length == walls[walls.Count - 1].length)
            {
                longWalls.Add(line);
            }
        }
        random = new System.Random(DateTime.Now.Millisecond);
        return longWalls[random.Next(longWalls.Count - 1)];
    }

    void ResetTexture()
    {
        rooms.Clear();
        growCorners = false;
        growCenters = false;
        texture.SetPixels(testWalls.GetPixels(0, 0, testWalls.width, testWalls.height));
        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }
    void ResetDebugTexture()
    {
        scanTexture.SetPixels(transparent.GetPixels(0, 0, transparent.width, transparent.height));
        scanTexture.Apply();
        scanRenderer.material.mainTexture = scanTexture;
    }

    void DrawRoom(Room room)
    {
        foreach (var wall in room.walls)
        {
            BresenhamLine(wall, room.color);
        }
        texture.Apply();
    }

    void RandomWalls()
    {
        rooms.Clear();
        growCorners = false;
        growCenters = false;
        texture.SetPixels(transparent.GetPixels(0, 0, transparent.width, transparent.height));
        var r = new List<GridVector>();
        for (var i = 0; i < 3; i++)
        {
            r.Add(new GridVector(random.Next(16, texture.width / 3), random.Next(16, texture.height / 3)));
            r.Add(new GridVector(random.Next(texture.width / 3 * 2, texture.width - 16), random.Next(texture.height / 3 * 2, texture.height - 16)));
        }
        for (var i = 0; i < r.Count; i += 2)
        {
            for (var x = r[i].x; x < r[i+1].x; x++)
            {
                for (var y = r[i].y; y < r[i+1].y; y++)
                {
                    texture.SetPixel(x, y, Color.black);
                }
            }
        }
        for (var i = 0; i < r.Count; i += 2)
        {
            for (int x = r[i].x + 1; x < r[i + 1].x - 1; x++)
            {
                for (int y = r[i].y + 1; y < r[i + 1].y - 1; y++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    void RandomRooms()
    {
        for (int i = 0; i < 10; i++)
        {
            int x = random.Next(0, texture.width);
            int y = random.Next(0, texture.height);
            if (CheckRect(x - 1, y - 1, x + 1, y + 1, Color.white, Color.white))
            {
                var color = new Color(Random.value*0.7f + 0.1f, Random.value*0.7f + 0.1f,
                                      Random.value*0.7f + 0.1f);
                rooms.Add(new Room(color,
                                   new List<GridVector>
                                       {
                                           new GridVector(x - 1, y - 1),
                                           new GridVector(x - 1, y + 1),
                                           new GridVector(x + 1, y + 1),
                                           new GridVector(x + 1, y - 1)
                                       }));
                DrawRoom(rooms[rooms.Count - 1]);
            }
        }
    }
}