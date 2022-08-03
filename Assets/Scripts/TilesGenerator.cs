using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TilesGenerator : MonoBehaviour
{
    #region Singleton
    public static TilesGenerator instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    public Vector3Int area;
    public static Vector3Int offset;

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap stairTilemap;
    public Tilemap doorTilemap;
    public TileBase floor;
    public TileBase wall;
    public TileBase stair;
    public TileBase door;
    public enum GenerationMethods { BSP, DrunkenWalk, CellularAutomata, PerlinNoise };
    public GenerationMethods generationMethods;
    TileBase[,] tiles;
    public static Vector3Int[,] coordinates;
    public Vector3Int spawnPoint;

    [Header("BSP")]
    public int BSPIterations;

    [Header("Drunken Walk")]
    public int walkCycles;
    public int walkIterations;
    public float turnChance;
    [Header("Cellular Automata")]
    public int noiseDensity;
    public int iterations;
    [Header("Perlin Noise")]
    public float tolerance;
    public float scale;
    public Vector2 perlinOffset;
    [Header("Door Generation")]
    public int doorSpawnRate;

    void Start()
    {
        offset = new Vector3Int(-area.x / 2, -area.y / 2, 0);
        coordinates = Generate_Coordinates();
        switch (generationMethods)
        {
            case GenerationMethods.CellularAutomata:
                Apply_Cellular_Automation(Generate_Noise_Map(noiseDensity), iterations);
                Draw(this.tiles);
                break;
            case GenerationMethods.PerlinNoise:
                Generate_Perlin_Noise();
                Draw(this.tiles);
                break;
            case GenerationMethods.DrunkenWalk:
                Drunken_Walk();
                Draw(this.tiles);
                break;
                //case GenerationMethods.BSP:
                //    TileBase[,] tiles = Generate_Walls();
                //    BSP(tiles, 1, area.x-1, 1, area.y-1);
                //    Draw(tiles);
                //    break;
        }
    }
    void Draw(TileBase[,] tiles)
    {
        List<Vector3Int> floorCoords = new();
        for (int x = 0; x < area.x; ++x)
        {
            for (int y = 0; y < area.y; ++y)
            {
                if(tiles[x, y] == floor)
                {
                    floorTilemap.SetTile(coordinates[x, y], tiles[x, y]);
                    floorCoords.Add(coordinates[x, y]);
                }
                else wallTilemap.SetTile(coordinates[x, y], tiles[x, y]);
            }
        }
        int rand = Random.Range(0, floorCoords.Count);
        stairTilemap.SetTile(floorCoords[rand], stair);
        rand = Random.Range(0, floorCoords.Count);
        spawnPoint = floorCoords[rand];
        Draw_Doors();
        GameManager.instance.SpawnPlayer(spawnPoint);
    }

    #region Drunken Walk
    void Drunken_Walk()
    {
        TileBase[,] tempGrid = Generate_Walls();
        Vector2Int up = new Vector2Int(0, 1);
        Vector2Int down = new Vector2Int(0, -1);
        Vector2Int left = new Vector2Int(-1, 0);
        Vector2Int right = new Vector2Int(1, 0);
        Vector2Int pos = new Vector2Int(area.x / 2, area.y / 2);
        Vector2Int currDir = new Vector2Int(0, 0);
        tempGrid[pos.x, pos.y] = floor;
        switch (Random.Range(1, 5))
        {
            case 1:
                currDir = up;
                break;
            case 2:
                currDir = down;
                break;
            case 3:
                currDir = right;
                break;
            case 4:
                currDir = right;
                break;
        }
        for (int j = 0; j < walkIterations; ++j)
        {
            for (int i = 0; i < walkCycles; ++i)
            {
                Vector2Int nextDir = pos + currDir;
                if (Is_Within_Bounds(nextDir.x, nextDir.y))
                {
                    tempGrid[nextDir.x, nextDir.y] = floor;
                    pos = nextDir;
                    if (Random.Range(0, 100) > 100 - turnChance)
                    {
                        Vector2Int newDir = new Vector2Int(0, 0);
                        do
                        {
                            switch (Random.Range(1, 5))
                            {
                                case 1:
                                    newDir = up;
                                    break;
                                case 2:
                                    newDir = down;
                                    break;
                                case 3:
                                    newDir = left;
                                    break;
                                case 4:
                                    newDir = right;
                                    break;
                            }
                        }
                        while (newDir == currDir);
                        currDir = newDir;
                    }
                }
                else
                {
                    pos = new Vector2Int(area.x / 2, area.y / 2);
                    switch (Random.Range(1, 5))
                    {
                        case 1:
                            currDir = up;
                            break;
                        case 2:
                            currDir = down;
                            break;
                        case 3:
                            currDir = left;
                            break;
                        case 4:
                            currDir = right;
                            break;
                    }
                }
            }
        }
        //A loop to block up the floors on the edge
        for (int y = 0; y < area.y; ++y)
        {
            for (int x = 0; x < area.x; ++x)
            {
                bool border = false;
                for (int j= y-1; j <= y+1; ++j)
                {
                    for (int k = x-1; k <= x+1; ++k)
                    {
                        if(!Is_Within_Bounds(k, j))
                        {
                            tempGrid[x, y] = wall;
                            border = true;
                            break;
                        }
                    }
                    if (border) break;
                }
            }
        }
        tiles = tempGrid;
    }
    #endregion

    #region Cellular Automata
    TileBase[,] Generate_Noise_Map(int density)
    {
        TileBase[,] noiseGrid = new TileBase[area.x, area.y];
        for (int y = 0; y < area.y; ++y)
        {
            for (int x = 0; x < area.x; ++x)
            {
                int random = Random.Range(1, 100);
                if (random > density)
                    noiseGrid[x, y] = floor;
                else noiseGrid[x, y] = wall;
            }
        }
        return noiseGrid;
    }

    void Apply_Cellular_Automation(TileBase[,] grid, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            TileBase[,] tempGrid = grid;

            for (int y = 0; y < area.y; ++y)
            {
                for (int x = 0; x < area.x; ++x)
                {
                    int wall_neighbor_count = 0;
                    bool border = false;
                    for (int j = y - 1; j <= y + 1; ++j)
                    {
                        for (int k = x - 1; k <= x + 1; ++k)
                        {
                            if (Is_Within_Bounds(k, j))
                            {
                                if (j != y || k != x)
                                {
                                    if (tempGrid[area.x - 1 - k, area.y - 1 - j] == wall)
                                    {
                                        wall_neighbor_count++;
                                    }
                                }
                            }
                            else border = true;
                        }
                    }
                    grid[x, y] = (wall_neighbor_count > 4 || border) ? wall : floor;
                }
            }
        }
        tiles = grid;
    }
    #endregion

    #region Binary Space Partition
    private int i = 0;
    private void BSP(TileBase[,] tiles, int minX, int maxX, int minY, int maxY, int axis = 1)
    {
        i++;
        // Return from recursion
        if (i >= BSPIterations)
        {
            return;
        }
        // 0 represents the x axis and 1 represents the y axis
        if (axis == -1)
        {
            int randX = Random.Range(minX, maxX);
            for (int y = minY; y < maxY; ++y)
            {
                tiles[randX, y] = floor;
            }
            BSP(tiles, minX, randX, minY, maxY, axis * -1);
        }
        else if (axis == 1)
        {
            int randY = Random.Range(minY, maxY);
            for (int x = minX; x < maxX; ++x)
            {
                tiles[x, randY] = floor;
            }
            BSP(tiles, minX, maxX, randY, maxY, axis * -1);
        }
    }
    #endregion

    #region Perlin Noise
    void Generate_Perlin_Noise()
    {
        TileBase[,] tempGrid = new TileBase[area.x, area.y];
        for (int y = 0; y < area.y; ++y)
        {
            for (int x = 0; x < area.x; ++x)
            {
                float floatX = (float)x / area.x * scale + perlinOffset.x;
                float floatY = (float)y / area.y * scale + perlinOffset.y;
                float noise = Mathf.PerlinNoise(floatX, floatY);
                if (noise > (1 - tolerance / 100))
                {
                    tempGrid[x, y] = wall;
                }
                else tempGrid[x, y] = floor;
            }
        }
        tiles = tempGrid;
    }
    #endregion

    #region Utilities
    void Destroy_Map()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        stairTilemap.ClearAllTiles();
    }

    void Draw_Doors()
    {
        for (int y = 0; y < area.y; ++y)
        {
            for (int x = 0; x < area.x; ++x)
            {
                if (tiles[x, y] == wall) continue;
                int neighbor_wall_count_hor = 0;
                int neighbor_wall_count_ver = 0;
                for (int k = x - 1; k <= x + 1; ++k)
                {
                    if (Is_Within_Bounds(k, y))
                        {
                            if (k != x)
                            {
                                if (tiles[k, y] == wall)
                                {
                                    neighbor_wall_count_hor++;
                                }
                            }
                        }
                }
                for (int j = y - 1; j <= y + 1; ++j)
                {
                    if (Is_Within_Bounds(x, j))
                    {
                        if (j != y)
                        {
                            if (tiles[x, j] == wall)
                            {
                                neighbor_wall_count_ver++;
                            }
                        }
                    }
                }
                if (neighbor_wall_count_hor > 1 || neighbor_wall_count_ver > 1)
                    {
                        int rand = Random.Range(0, 100);
                        if (rand > 100 - doorSpawnRate)
                            doorTilemap.SetTile(coordinates[x, y], door);
                    }
                
            }
        }
    }
    public void Destroy_Wall(Vector3Int pos)
    {
        wallTilemap.SetTile(pos, null);
        floorTilemap.SetTile(pos, floor);
    }
    bool Is_Within_Bounds(int x, int y)
    {
        return ((y >= 0 && y <= area.y - 1) && (x >= 0 && x <= area.x - 1));
    }
    TileBase[,] Generate_Walls()
    {
        TileBase[,] tempGrid = new TileBase[area.x, area.y];
        for (int x = 0; x < area.x; ++x)
        {
            for (int y = 0; y < area.y; ++y)
            {
                tempGrid[x, y] = wall;
            }
        }
        return tempGrid;
    }
    Vector3Int[,] Generate_Coordinates()
    {
        Vector3Int[,] tempGrid = new Vector3Int[area.x, area.y];
        for (int x = 0; x < area.x; ++x)
        {
            for (int y = 0; y < area.y; ++y)
            {
                tempGrid[x, y] = new Vector3Int(x, y, 0) + offset;
            }
        }
        return tempGrid;
    }
    #endregion
    // Update is called once per frame
}
