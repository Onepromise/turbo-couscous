using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Board board;
    public MinionData warriorData;

    [Header("Settings")]
    public int currentPlayerTurn = 0;

    private List<Minion> allMinions = new List<Minion>();
    private Minion selectedMinion;
    private bool isAttackMode = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        board = FindFirstObjectByType<Board>();
        SpawnStartingMinions();
    }

    void SpawnStartingMinions()
    {
        SpawnMinion(warriorData, new Vector2Int(3, 1), 0);
        SpawnMinion(warriorData, new Vector2Int(4, 6), 1);
    }

    public void SpawnMinion(MinionData data, Vector2Int position, int ownerID)
    {
        GameObject minionObj = new GameObject($"{data.minionName}_{ownerID}");
        Minion minion = minionObj.AddComponent<Minion>();
        minion.Initialize(data, position, ownerID);
        allMinions.Add(minion);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (currentPlayerTurn != 0) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Left-click
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Get mouse position in world space
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());

            // Check if we clicked on a minion using physics
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                // Check if we hit a minion
                Minion clickedMinion = hit.collider.GetComponent<Minion>();

                if (clickedMinion != null && clickedMinion.ownerID == 0)
                {
                    selectedMinion = clickedMinion;
                    Debug.Log($"Selected: {selectedMinion.data.minionName} at {selectedMinion.gridPosition}");
                    return;
                }
            }

            // If we have a selected minion and didn't click another minion, try to move
            if (selectedMinion != null)
            {
                Vector2Int gridPos = board.WorldToGrid(mousePos);

                if (board.IsValidPosition(gridPos))
                {
                    if (CanMoveTo(selectedMinion, gridPos))
                    {
                        selectedMinion.MoveTo(gridPos);
                        selectedMinion = null;
                        EndTurn();
                    }
                }
            }
        }
    }

    bool CanMoveTo(Minion minion, Vector2Int targetPos)
    {
        int distance = Mathf.Abs(targetPos.x - minion.gridPosition.x) +
                       Mathf.Abs(targetPos.y - minion.gridPosition.y);

        if (distance > minion.data.moveRange) return false;
        if (GetMinionAt(targetPos) != null) return false;

        return true;
    }

    Minion GetMinionAt(Vector2Int position)
    {
        foreach (Minion minion in allMinions)
        {
            if (minion.gridPosition == position) return minion;
        }
        return null;
    }

    void EndTurn()
    {
        currentPlayerTurn = (currentPlayerTurn + 1) % 2;
        Debug.Log($"Turn: {(currentPlayerTurn == 0 ? "Player" : "AI")}");

        if (currentPlayerTurn == 1)
        {
            Invoke("AITurn", 1f);
        }
    }

    void AITurn()
    {
        Minion aiMinion = allMinions.Find(m => m.ownerID == 1);
        if (aiMinion == null)
        {
            EndTurn();
            return;
        }

        Minion playerMinion = allMinions.Find(m => m.ownerID == 0);
        if (playerMinion == null)
        {
            EndTurn();
            return;
        }

        Vector2Int direction = new Vector2Int(
            playerMinion.gridPosition.x > aiMinion.gridPosition.x ? 1 : -1,
            playerMinion.gridPosition.y > aiMinion.gridPosition.y ? 1 : -1
        );

        Vector2Int targetPos = aiMinion.gridPosition + new Vector2Int(direction.x, 0);

        if (board.IsValidPosition(targetPos) && GetMinionAt(targetPos) == null)
        {
            aiMinion.MoveTo(targetPos);
        }

        EndTurn();
    }
}