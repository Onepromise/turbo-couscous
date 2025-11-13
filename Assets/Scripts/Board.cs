using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;
    
    [Header("Visual")]
    public Color lightColor = Color.white;
    public Color darkColor = Color.gray;
    
    private GameObject[,] cells;
    
    void Start()
    {
        CreateBoard();
    }
    
    void CreateBoard()
    {
        cells = new GameObject[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create a cell (quad)
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cell.name = $"Cell_{x}_{y}";
                cell.transform.parent = transform;
                cell.transform.position = new Vector3(x * cellSize, y * cellSize, 0);
                
                // Checkered pattern
                bool isLight = (x + y) % 2 == 0;
                cell.GetComponent<Renderer>().material.color = isLight ? lightColor : darkColor;
                
                cells[x, y] = cell;
            }
        }
        
        // Center camera
        Camera.main.transform.position = new Vector3(
            (width * cellSize) / 2f - cellSize / 2f,
            (height * cellSize) / 2f - cellSize / 2f,
            -10
        );
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }
    
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }
    
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}