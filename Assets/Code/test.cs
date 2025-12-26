using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class test : MonoBehaviour
{
    private Pathfinding pathfinding;
    private CaveGenerator caveGenerator;
    private GameObject[,] visualArray;
    private float cellSize = 1f;

    // Biến kiểm soát việc chạy tìm đường
    private bool isSearching = false; 

    private void Start()
    {
        pathfinding = new Pathfinding(60, 40, cellSize);
        caveGenerator = new CaveGenerator(pathfinding);
        
        InitializeVisuals();
        caveGenerator.GenerateMap();
        
        // Cập nhật màu lần đầu
        ResetVisuals();
    }

    private void InitializeVisuals()
    {
        int width = pathfinding.GetGrid().GetWidth();
        int height = pathfinding.GetGrid().GetHeight();
        visualArray = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Renderer renderer = quad.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Unlit/Color"));

                Vector3 gridPos = pathfinding.GetGrid().GetWorldPosition(x, y);
                quad.transform.position = gridPos + new Vector3(cellSize, cellSize) * 0.5f;
                quad.transform.localScale = Vector3.one * (cellSize * 0.9f);
                
                Destroy(quad.GetComponent<Collider>());
                visualArray[x, y] = quad;
            }
        }
    }

    private void ResetVisuals()
    {
        int width = pathfinding.GetGrid().GetWidth();
        int height = pathfinding.GetGrid().GetHeight();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PathNode node = pathfinding.GetGrid().GetGridObject(x, y);
                if (node.isWalkable) 
                    visualArray[x, y].GetComponent<Renderer>().material.color = Color.white;
                else 
                    visualArray[x, y].GetComponent<Renderer>().material.color = Color.black;
            }
        }
    }

    private void Update()
    {
        // 1. Phím M: Tạo map mới
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            StopAllCoroutines(); 
            isSearching = false;
            caveGenerator.GenerateMap();
            ResetVisuals();
            
            // Cập nhật HUD nếu có
            if (GameHUD.Instance != null) GameHUD.Instance.SetStatus("Đã tạo map mới");
        }

        // 2. Phím SPACE: Bắt đầu tìm đường SLOW MOTION
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isSearching)
        {
            StartCoroutine(FindPathSlowly(0, 0, 59, 39));
        }

        // 3. Chuột giữa: Tìm đường đến vị trí chuột
        if (Mouse.current.middleButton.wasPressedThisFrame && !isSearching)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            int endX = Mathf.FloorToInt(mousePos.x / cellSize);
            int endY = Mathf.FloorToInt(mousePos.y / cellSize);

            if (endX >= 0 && endX < 60 && endY >= 0 && endY < 40)
            {
                StartCoroutine(FindPathSlowly(0, 0, endX, endY));
            }
        }
    }

    // --- TRÁI TIM CỦA THUẬT TOÁN (Phiên bản chạy chậm & Có HUD) ---
    private IEnumerator FindPathSlowly(int startX, int startY, int endX, int endY)
    {
        isSearching = true;
        ResetVisuals(); 

        // --- HUD AN TOÀN: Kiểm tra xem GameHUD có tồn tại không trước khi gọi ---
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.SetStatus("Đang tìm đường...");
            GameHUD.Instance.SetInfo(0, 0);
        }
        // ----------------------------------------------------------------------

        Grid<PathNode> grid = pathfinding.GetGrid();
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (!startNode.isWalkable || !endNode.isWalkable)
        {
            Debug.Log("Điểm đầu hoặc điểm cuối bị chặn!");
            if (GameHUD.Instance != null) GameHUD.Instance.SetStatus("Bị chặn ngay từ đầu!");
            isSearching = false;
            yield break;
        }

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();
        
        int nodesCheckedCount = 0; // Biến đếm số ô duyệt

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                ShowFinalPath(endNode);
                isSearching = false;
                yield break;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            
            // --- CẬP NHẬT HUD ---
            nodesCheckedCount++;
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.SetInfo(0, nodesCheckedCount);
            }
            // --------------------

            if (currentNode != startNode && currentNode != endNode)
            {
                visualArray[currentNode.x, currentNode.y].GetComponent<Renderer>().material.color = Color.red;
            }

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                        if (neighbourNode != endNode)
                            visualArray[neighbourNode.x, neighbourNode.y].GetComponent<Renderer>().material.color = Color.green;
                    }
                }
            }

            // --- QUAN TRỌNG: Dòng này giúp Unity KHÔNG BỊ TREO ---
            yield return new WaitForSeconds(0.01f); 
        }

        if (GameHUD.Instance != null) GameHUD.Instance.SetStatus("Không tìm thấy đường!");
        Debug.Log("Không tìm thấy đường!");
        isSearching = false;
    }

    private void ShowFinalPath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while (currentNode != null)
        {
            visualArray[currentNode.x, currentNode.y].GetComponent<Renderer>().material.color = Color.blue;
            path.Add(currentNode);
            currentNode = currentNode.cameFromNode;
        }
        
        // --- HUD: Báo cáo kết quả ---
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.SetStatus("Đã tìm thấy đích!");
            GameHUD.Instance.SetInfo(path.Count, 0);
        }
        // ---------------------------
        
        Debug.Log("Đã tìm thấy đường đi!");
    }

    // --- Các hàm phụ trợ ---
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        Grid<PathNode> grid = pathfinding.GetGrid();
        List<PathNode> neighbourList = new List<PathNode>();
        if (currentNode.x - 1 >= 0) {
            neighbourList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y));
            if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y - 1));
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            neighbourList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y));
            if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y - 1));
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y + 1));
        }
        if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentNode.x, currentNode.y - 1));
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetGridObject(currentNode.x, currentNode.y + 1));
        return neighbourList;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = pathNodeList[i];
        return lowestFCostNode;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return 14 * Mathf.Min(xDistance, yDistance) + 10 * remaining;
    }
    
    public static Vector3 GetMouseWorldPosition() { 
        Vector3 vec = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        vec.z = 0f; return vec;
    }
}