using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải có để dùng UI

public class GameHUD : MonoBehaviour
{
    // Tạo Singleton để gọi được từ file test.cs dễ dàng
    public static GameHUD Instance { get; private set; }

    [Header("Kéo các UI Text vào đây")]
    public Text statusText; // Hiển thị: Đang tạo map, Đang tìm đường...
    public Text infoText;   // Hiển thị: Độ dài đường đi, số ô đã duyệt...

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm cập nhật trạng thái
    public void SetStatus(string message)
    {
        if (statusText != null) statusText.text = "Trạng thái: " + message;
    }

    // Hàm cập nhật thông số chi tiết
    public void SetInfo(int pathLength, int nodesChecked)
    {
        if (infoText != null)
        {
            infoText.text = $"Độ dài đường đi: {pathLength}\nSố ô đã duyệt: {nodesChecked}";
        }
    }
}