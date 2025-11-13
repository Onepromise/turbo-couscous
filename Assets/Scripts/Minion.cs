using UnityEngine;

public class Minion : MonoBehaviour
{
    public MinionData data;
    public Vector2Int gridPosition;
    public int currentHealth;
    public int ownerID; // 0 = player, 1 = AI

    private SpriteRenderer spriteRenderer;
    private Board board;

    public void Initialize(MinionData minionData, Vector2Int startPos, int owner)
    {
        data = minionData;
        gridPosition = startPos;
        currentHealth = data.health;
        ownerID = owner;

        board = FindFirstObjectByType<Board>();

        // Visual setup
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // ADD COLLIDER FOR CLICKING
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        // Create a simple circle sprite
        spriteRenderer.sprite = CreateCircleSprite();

        // Auto-assign color based on owner
        spriteRenderer.color = ownerID == 0 ? Color.blue : Color.red;

        // Position on board (CENTERED)
        Vector3 worldPos = board.GridToWorld(gridPosition);
        worldPos += new Vector3(board.cellSize / 2f, board.cellSize / 2f, -1);
        transform.position = worldPos;
    }

    public void MoveTo(Vector2Int newPosition)
    {
        gridPosition = newPosition;

        // Position on board (CENTERED)
        Vector3 worldPos = board.GridToWorld(gridPosition);
        worldPos += new Vector3(board.cellSize / 2f, board.cellSize / 2f, -1);
        transform.position = worldPos;
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance < radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}