using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 10;  // Changed from 8 to 10
    public int height = 8;
    public float cellSize = 1f;
    
    [Header("Materials")]
    public Material lightMaterial;
    public Material darkMaterial;
    public Material highlightMaterial;
    public Material player1SpawnMaterial;  // NEW - Blue spawn zone
    public Material player2SpawnMaterial;  // NEW - Red spawn zone
    
    private GameObject[,] cells;
    private GameObject highlightedCell;
    
    void Start()
    {
        CreateMaterials();
        CreateBoard();
    }
    
    void CreateMaterials()
    {
        // Standard checkerboard materials
        if (lightMaterial == null)
        {
            lightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (lightMaterial.shader == null || lightMaterial.shader.name == "Hidden/InternalErrorShader")
                lightMaterial = new Material(Shader.Find("Standard"));
            lightMaterial.color = new Color(0.9f, 0.9f, 0.9f);
        }
        
        if (darkMaterial == null)
        {
            darkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (darkMaterial.shader == null || darkMaterial.shader.name == "Hidden/InternalErrorShader")
                darkMaterial = new Material(Shader.Find("Standard"));
            darkMaterial.color = new Color(0.5f, 0.5f, 0.5f);
        }
        
        // NEW - Player 1 spawn zone (Blue) - rows 0-1
        if (player1SpawnMaterial == null)
        {
            player1SpawnMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (player1SpawnMaterial.shader == null || player1SpawnMaterial.shader.name == "Hidden/InternalErrorShader")
                player1SpawnMaterial = new Material(Shader.Find("Standard"));
            player1SpawnMaterial.color = new Color(0.3f, 0.3f, 0.8f); // Blue
        }
        
        // NEW - Player 2 spawn zone (Red) - rows 6-7
        if (player2SpawnMaterial == null)
        {
            player2SpawnMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (player2SpawnMaterial.shader == null || player2SpawnMaterial.shader.name == "Hidden/InternalErrorShader")
                player2SpawnMaterial = new Material(Shader.Find("Standard"));
            player2SpawnMaterial.color = new Color(0.8f, 0.3f, 0.3f); // Red
        }
        
        // Highlight material
        if (highlightMaterial == null)
        {
            highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (highlightMaterial.shader == null || highlightMaterial.shader.name == "Hidden/InternalErrorShader")
                highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = new Color(1f, 1f, 0.5f);
        }
    }
    
    void CreateBoard()
    {
        cells = new GameObject[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Plane);
                cell.name = $"Cell_{x}_{z}";
                cell.transform.parent = transform;
                cell.transform.localScale = new Vector3(cellSize * 0.1f, 1, cellSize * 0.1f);
                cell.transform.position = new Vector3(x * cellSize, 0, z * cellSize);
                
                // Determine material based on position
                Material cellMaterial;
                
                // Player 1 spawn zone (rows 0-1) - BLUE
                if (z >= 0 && z <= 1)
                {
                    cellMaterial = player1SpawnMaterial;
                }
                // Player 2 spawn zone (rows 6-7) - RED
                else if (z >= 6 && z <= 7)
                {
                    cellMaterial = player2SpawnMaterial;
                }
                // Middle area (rows 2-5) - checkered
                else
                {
                    bool isLight = (x + z) % 2 == 0;
                    cellMaterial = isLight ? lightMaterial : darkMaterial;
                }
                
                cell.GetComponent<Renderer>().material = cellMaterial;
                cells[x, z] = cell;
            }
        }
        
        // Position camera to see the 10x8 board
        Camera.main.transform.position = new Vector3(
            (width * cellSize) / 2f - cellSize / 2f,
            12f,  // Higher for larger board
            (height * cellSize) / 2f - cellSize / 2f - 6f
        );
        Camera.main.transform.rotation = Quaternion.Euler(60, 0, 0);
        
        Debug.Log($"Board created: {width}x{height}");
    }
    
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0.5f, gridPos.y * cellSize);
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int z = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector2Int(x, z);
    }
    
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
    
    // NEW - Check if position is in player's spawn zone
    public bool IsInSpawnZone(Vector2Int pos, int playerID)
    {
        if (playerID == 0) // Player 1: rows 0-1
        {
            return pos.y >= 0 && pos.y <= 1;
        }
        else // Player 2: rows 6-7
        {
            return pos.y >= 6 && pos.y <= 7;
        }
    }
    
    public void HighlightCell(Vector2Int pos)
    {
        ClearHighlight();
        
        if (!IsValidPosition(pos)) return;
        
        highlightedCell = GameObject.CreatePrimitive(PrimitiveType.Plane);
        highlightedCell.name = "Highlight";
        highlightedCell.transform.localScale = new Vector3(cellSize * 0.1f, 1, cellSize * 0.1f);
        highlightedCell.transform.position = new Vector3(pos.x * cellSize, 0.01f, pos.y * cellSize);
        highlightedCell.GetComponent<Renderer>().material = highlightMaterial;
        
        Destroy(highlightedCell.GetComponent<Collider>());
    }
    
    public void ClearHighlight()
    {
        if (highlightedCell != null)
        {
            Destroy(highlightedCell);
        }
    }
}