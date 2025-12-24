using System;
using UnityEngine;
using Unity.Mathematics;

// <TGridObject> nghĩa là: Grid này có thể chứa bất cứ kiểu dữ liệu T nào
public class Grid<TGridObject>
{
    // Sự kiện để báo cho bên ngoài biết khi nào dữ liệu thay đổi (để vẽ lại màu)
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridArray; // Mảng chứa các object T
    private TextMesh[,] debugTextArray;

    // Constructor nhận thêm tham số: createGridObject (Hàm để tạo ra giá trị mặc định cho từng ô)
    public Grid(int width, int height, float cellSize, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new TGridObject[width, height];
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Tạo object mới cho ô (x, y) bằng hàm được truyền vào
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        // Vẽ Debug Text và Lưới
        bool showDebug = true;
        if (showDebug)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    debugTextArray[x, y] = CreateWorldText(gridArray[x, y]?.ToString(), null,
                        GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f,
                        1, Color.white, TextAnchor.MiddleCenter);

                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        }
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public float GetCellSize() { return cellSize; }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    // Set giá trị trực tiếp cho Object
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    // Báo hiệu thay đổi để cập nhật giao diện
    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        
        if (debugTextArray[x, y] != null)
        {
            debugTextArray[x, y].text = gridArray[x, y]?.ToString();
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    // Hàm tiện ích tạo TextMesh (Giữ nguyên)
    private TextMesh CreateWorldText(string text, Transform parent, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
    
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = text;
        textMesh.fontSize = 100; 
        textMesh.characterSize = 0.05f; 
        // --------------------
    
        textMesh.color = color;
        return textMesh;
    }
}