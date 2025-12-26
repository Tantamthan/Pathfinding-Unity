using UnityEngine;
using System.Collections.Generic;

public class CaveGenerator
{
    private Grid<PathNode> grid;
    private int width;
    private int height;

    [Range(0, 100)]
    private int randomFillPercent = 47; 

    public CaveGenerator(Pathfinding pathfinding)
    {
        this.grid = pathfinding.GetGrid();
        this.width = grid.GetWidth();
        this.height = grid.GetHeight();
    }

    public void GenerateMap()
    {
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }
        
        UpdateGridVisuals();
        Debug.Log("Đã tạo xong bản đồ (Đảo ngược: Tường là đường đi)!");
    }

    // --- SỬA ĐỔI 1: Logic tạo ngẫu nhiên ---
    private void RandomFillMap()
    {
        string seed = Time.time.ToString();
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Viền bản đồ là TƯỜNG -> Bây giờ TƯỜNG LÀ ĐI ĐƯỢC (True)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    grid.GetGridObject(x, y).isWalkable = true; 
                }
                else
                {
                    // Random ra Tường
                    bool isWall = (pseudoRandom.Next(0, 100) < randomFillPercent);
                    
                    // Logic cũ: isWalkable = !isWall (Tường thì không đi được)
                    // Logic MỚI: isWalkable = isWall (Tường LÀ ĐI ĐƯỢC)
                    grid.GetGridObject(x, y).isWalkable = isWall; 
                }
            }
        }
    }

    // --- SỬA ĐỔI 2: Logic làm mịn ---
    private void SmoothMap()
    {
        bool[,] newStates = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int wallCount = GetSurroundingWallCount(x, y);

            
                
                if (wallCount > 4)
                    newStates[x, y] = true;  
                else if (wallCount < 4)
                    newStates[x, y] = false; // Cave = Blocked
                else
                    newStates[x, y] = grid.GetGridObject(x, y).isWalkable;
                
                // Giữ viền là tường (Đi được)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                     newStates[x, y] = true;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid.GetGridObject(x, y).isWalkable = newStates[x, y];
            }
        }
    }

    // --- SỬA ĐỔI 3: Cách đếm tường ---
    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                      
                        if (grid.GetGridObject(neighbourX, neighbourY).isWalkable)
                        {
                            wallCount++;
                        }
                    }
                }
                else
                {
                   
                    wallCount++;
                }
            }
        }
        return wallCount;
    }
    
    private void UpdateGridVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid.TriggerGridObjectChanged(x, y);
            }
        }
    }
}