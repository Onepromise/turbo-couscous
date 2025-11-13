using UnityEngine;
using TMPro;
using CHEGG;
using System.Collections.Generic;

public class Minion : MonoBehaviour
{
    public MinionData data;
    public Vector2Int gridPosition;
    public int currentHealth;
    public int ownerID;
    
    private Board board;
    private Renderer minionRenderer;
    private GameObject visualObject;
    private TextMeshPro healthText;
    
    private static int nextID = 0;
    public int minionID;
    
    public bool IsVillager => data.minionType == MinionType.Villager;
    
    public void Initialize(MinionData minionData, Vector2Int startPos, int owner)
    {
        minionID = nextID++;
        
        data = minionData;
        gridPosition = startPos;
        currentHealth = data.health;
        ownerID = owner;
        
        gameObject.name = $"{minionData.minionName}_P{owner}_ID{minionID}";
        
        board = FindFirstObjectByType<Board>();
        
        // Create visual
        visualObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualObject.name = "Visual";
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        
        // Scale based on type
        if (IsVillager)
        {
            visualObject.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f); // Bigger for Villager
        }
        else
        {
            visualObject.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        }
        
        // Material
        minionRenderer = visualObject.GetComponent<Renderer>();
        Material minionMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        if (minionMaterial.shader == null || minionMaterial.shader.name == "Hidden/InternalErrorShader")
            minionMaterial = new Material(Shader.Find("Standard"));
        
        if (minionMaterial.shader == null || minionMaterial.shader.name == "Hidden/InternalErrorShader")
            minionMaterial = new Material(Shader.Find("Unlit/Color"));
        
        // Color based on owner + special color for Villager
        if (IsVillager)
        {
            minionMaterial.color = ownerID == 0 ? new Color(0.5f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f); // Lighter shade
        }
        else
        {
            minionMaterial.color = ownerID == 0 ? Color.blue : Color.red;
        }
        
        minionRenderer.material = minionMaterial;
        
        // Collider
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = 0.5f;
        col.center = new Vector3(0, 0.5f, 0);
        
        // Position
        transform.position = board.GridToWorld(gridPosition);
        
        // Health display
        CreateHealthDisplay();
        
        Debug.Log($"[ID:{minionID}] {data.minionType} created at {transform.position}");
    }
    
    void CreateHealthDisplay()
    {
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        healthText = textObj.AddComponent<TextMeshPro>();
        healthText.text = IsVillager ? $"👑 {currentHealth}/{data.health}" : $"{currentHealth}/{data.health}";
        healthText.fontSize = 3;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;
        
        textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - Camera.main.transform.position);
    }
    
    /// <summary>
    /// Get valid movement positions based on this minion's movement pattern
    /// </summary>
    public List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        switch (data.movementPattern)
        {
            case MovementPattern.None:
                return moves; // Cannot move
                
            case MovementPattern.Standard8:
                AddStandard8Moves(moves);
                break;
                
            case MovementPattern.ForwardOnly:
                AddForwardMoves(moves);
                break;
                
            case MovementPattern.LateralOnly:
                AddLateralMoves(moves);
                break;
                
            case MovementPattern.DiagonalOnly:
                AddDiagonalMoves(moves);
                break;
        }
        
        return moves;
    }
    
    void AddStandard8Moves(List<Vector2Int> moves)
    {
        // All 8 surrounding squares
        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // Forward
            new Vector2Int(0, -1),  // Back
            new Vector2Int(1, 0),   // Right
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 1),   // Forward-Right
            new Vector2Int(1, -1),  // Back-Right
            new Vector2Int(-1, 1),  // Forward-Left
            new Vector2Int(-1, -1)  // Back-Left
        };
        
        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i <= data.moveRange; i++)
            {
                Vector2Int checkPos = gridPosition + (dir * i);
                if (!board.IsValidPosition(checkPos)) break;
                if (GameManager.Instance.GetMinionAt(checkPos) != null) break;
                
                moves.Add(checkPos);
            }
        }
    }
    
    void AddForwardMoves(List<Vector2Int> moves)
    {
        // Zombie: Can only move forward (3 squares above current position)
        int forwardDir = ownerID == 0 ? 1 : -1; // Player 0 moves up, Player 1 moves down
        
        Vector2Int[] forwardPositions = {
            new Vector2Int(gridPosition.x - 1, gridPosition.y + forwardDir),  // Forward-left
            new Vector2Int(gridPosition.x, gridPosition.y + forwardDir),      // Forward
            new Vector2Int(gridPosition.x + 1, gridPosition.y + forwardDir)   // Forward-right
        };
        
        foreach (Vector2Int pos in forwardPositions)
        {
            if (board.IsValidPosition(pos) && GameManager.Instance.GetMinionAt(pos) == null)
            {
                moves.Add(pos);
            }
        }
    }
    
    void AddLateralMoves(List<Vector2Int> moves)
    {
        // Skeleton: Only lateral (up, down, left, right)
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i <= data.moveRange; i++)
            {
                Vector2Int checkPos = gridPosition + (dir * i);
                if (!board.IsValidPosition(checkPos)) break;
                if (GameManager.Instance.GetMinionAt(checkPos) != null) break;
                
                moves.Add(checkPos);
            }
        }
    }
    
    void AddDiagonalMoves(List<Vector2Int> moves)
    {
        // Diagonal only
        Vector2Int[] directions = {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };
        
        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i <= data.moveRange; i++)
            {
                Vector2Int checkPos = gridPosition + (dir * i);
                if (!board.IsValidPosition(checkPos)) break;
                if (GameManager.Instance.GetMinionAt(checkPos) != null) break;
                
                moves.Add(checkPos);
            }
        }
    }
    
    /// <summary>
    /// Get valid attack targets based on attack pattern
    /// </summary>
    public List<Vector2Int> GetValidAttackTargets()
    {
        List<Vector2Int> targets = new List<Vector2Int>();
        
        switch (data.attackPattern)
        {
            case AttackPattern.None:
                return targets;
                
            case AttackPattern.Melee8:
                AddMelee8Targets(targets);
                break;
                
            case AttackPattern.MeleeLateral:
                AddMeleeLateralTargets(targets);
                break;
                
            case AttackPattern.RangedDiagonal:
                AddRangedDiagonalTargets(targets);
                break;
                
            case AttackPattern.Explosion:
                AddExplosionTargets(targets);
                break;
                
            case AttackPattern.Sweep:
                // Sweep is directional, handled differently
                break;
        }
        
        return targets;
    }
    
    void AddMelee8Targets(List<Vector2Int> targets)
    {
        // All 8 surrounding squares
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = gridPosition + dir;
            if (board.IsValidPosition(checkPos))
            {
                Minion targetMinion = GameManager.Instance.GetMinionAt(checkPos);
                if (targetMinion != null && targetMinion.ownerID != ownerID)
                {
                    targets.Add(checkPos);
                }
            }
        }
    }
    
    void AddMeleeLateralTargets(List<Vector2Int> targets)
    {
        // 4 lateral adjacent squares only
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = gridPosition + dir;
            if (board.IsValidPosition(checkPos))
            {
                Minion targetMinion = GameManager.Instance.GetMinionAt(checkPos);
                if (targetMinion != null && targetMinion.ownerID != ownerID)
                {
                    targets.Add(checkPos);
                }
            }
        }
    }
    
    void AddRangedDiagonalTargets(List<Vector2Int> targets)
    {
        // Diagonal ranged attack (Skeleton)
        Vector2Int[] directions = {
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i <= data.attackRange; i++)
            {
                Vector2Int checkPos = gridPosition + (dir * i);
                if (!board.IsValidPosition(checkPos)) break;
                
                Minion targetMinion = GameManager.Instance.GetMinionAt(checkPos);
                if (targetMinion != null)
                {
                    if (targetMinion.ownerID != ownerID)
                    {
                        targets.Add(checkPos);
                    }
                    break; // Can't shoot through minions
                }
            }
        }
    }
    
    void AddExplosionTargets(List<Vector2Int> targets)
    {
        // Creeper: Hits ALL surrounding squares (enemies AND friendlies!)
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = gridPosition + dir;
            if (board.IsValidPosition(checkPos))
            {
                Minion targetMinion = GameManager.Instance.GetMinionAt(checkPos);
                if (targetMinion != null) // Hits EVERYONE (friendly fire!)
                {
                    targets.Add(checkPos);
                }
            }
        }
    }
    
    public void MoveTo(Vector2Int newPosition)
    {
        Vector3 oldPos = transform.position;
        gridPosition = newPosition;
        transform.position = board.GridToWorld(gridPosition);
        
        gameObject.name = $"{data.minionName}_P{ownerID}_ID{minionID}_{gridPosition}";
        
        Debug.Log($"[ID:{minionID}] Moved from {oldPos} to {transform.position}");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthDisplay();
        
        Debug.Log($"[ID:{minionID}] {gameObject.name} took {damage} damage. Health: {currentHealth}/{data.health}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, data.health);
        UpdateHealthDisplay();
    }
    
    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = IsVillager ? $"👑 {currentHealth}/{data.health}" : $"{currentHealth}/{data.health}";
            
            float healthPercent = (float)currentHealth / data.health;
            if (healthPercent > 0.6f)
                healthText.color = Color.green;
            else if (healthPercent > 0.3f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }
    
    void Die()
    {
        Debug.Log($"[ID:{minionID}] {gameObject.name} DIED! IsVillager: {IsVillager}");
        
        // Check for Villager death (win condition)
        if (IsVillager)
        {
            GameManager.Instance.OnVillagerDeath(ownerID);
        }
        
        Destroy(gameObject);
    }
    
    public void SetHighlight(bool highlighted)
    {
        if (minionRenderer != null && minionRenderer.material != null)
        {
            if (highlighted)
            {
                minionRenderer.material.color = Color.yellow;
            }
            else
            {
                if (IsVillager)
                {
                    minionRenderer.material.color = ownerID == 0 ? new Color(0.5f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f);
                }
                else
                {
                    minionRenderer.material.color = ownerID == 0 ? Color.blue : Color.red;
                }
            }
        }
    }
    
    void OnDestroy()
    {
        Debug.Log($"[ID:{minionID}] Minion {gameObject.name} destroyed!");
    }
}