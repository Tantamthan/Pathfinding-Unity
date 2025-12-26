using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode 
{
    private Grid<PathNode> grid; // Tham chiếu ngược lại Grid chứa nó
    public int x;
    public int y;

    public int gCost; // Chi phí từ điểm đầu đến đây
    public int hCost; // Khoảng cách ước lượng đến đích
    public int fCost; // Tổng (G + H)

    public PathNode cameFromNode; // Ô "cha" (để truy vết đường đi ngược lại)
    public bool isWalkable = true; // Có phải tường không? (true = đi được, false = tường)

    // Constructor
    public PathNode(Grid<PathNode> grid, int x, int y) 
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isWalkable = true;
    }

    public void CalculateFCost() 
    {
        fCost = gCost + hCost;
    }

    // Ghi đè phương thức ToString để debug cho dễ
    public override string ToString() 
    {
        return isWalkable ? x + "," + y : "X";
    }
}