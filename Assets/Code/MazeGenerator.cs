using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    private Pathfinding pathfinding;
    private Grid<PathNode> grid;
    private int width;
    private int height;

    public MazeGenerator(Pathfinding pathfinding)
    {
        this.pathfinding = pathfinding;
        this.grid = pathfinding.GetGrid();
        this.width = grid.GetWidth();
        this.height = grid.GetHeight();
    }

    public void GenerateMaze()
    {
        // 1. Reset: Biến TẤT CẢ thành TƯỜNG (isWalkable = false)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var node = grid.GetGridObject(x, y);
                node.isWalkable = false; 
                grid.TriggerGridObjectChanged(x, y); // Cập nhật màu đỏ
            }
        }

        // 2. Bắt đầu đào hầm từ ô (0,0)
        CarvePassage(0, 0);
    }

    // Hàm đệ quy để đào đường
    private void CarvePassage(int x, int y)
    {
        // Đánh dấu ô hiện tại là ĐƯỜNG ĐI (isWalkable = true)
        var currentNode = grid.GetGridObject(x, y);
        currentNode.isWalkable = true;
        grid.TriggerGridObjectChanged(x, y);

        // Tạo danh sách 4 hướng (Lên, Xuống, Trái, Phải)
        // Số 2 nghĩa là nhảy cóc 2 ô (để chừa 1 ô làm tường ở giữa)
        int[][] directions = new int[][]
        {
            new int[] { 0, 2 },  // Lên
            new int[] { 0, -2 }, // Xuống
            new int[] { -2, 0 }, // Trái
            new int[] { 2, 0 }   // Phải
        };

        // Trộn ngẫu nhiên các hướng (Shuffle) để mê cung không bị lặp lại
        Shuffle(directions);

        // Duyệt qua từng hướng
        foreach (var dir in directions)
        {
            int nextX = x + dir[0];
            int nextY = y + dir[1];

            // Kiểm tra nếu ô đích nằm trong bản đồ và ĐANG LÀ TƯỜNG (chưa đào tới)
            if (IsInside(nextX, nextY) && !grid.GetGridObject(nextX, nextY).isWalkable)
            {
                // Đập bỏ bức tường NẰM GIỮA ô hiện tại và ô đích
                int wallX = x + (dir[0] / 2);
                int wallY = y + (dir[1] / 2);
                
                var wallNode = grid.GetGridObject(wallX, wallY);
                wallNode.isWalkable = true;
                grid.TriggerGridObjectChanged(wallX, wallY);

                // Đệ quy: Tiếp tục đào từ ô đích
                CarvePassage(nextX, nextY);
            }
        }
    }

    // Hàm tiện ích kiểm tra tọa độ có nằm trong Grid không
    private bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    // Hàm xáo trộn mảng (Fisher-Yates shuffle)
    private void Shuffle(int[][] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(0, array.Length);
            var temp = array[i];
            array[i] = array[rnd];
            array[rnd] = temp;
        }
    }
}