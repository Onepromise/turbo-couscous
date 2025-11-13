using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Board board;
    public MinionData warriorData;

    [Header("Deck Configuration")]
    public List<MinionData> starterDeck = new List<MinionData>(); // Assign cards in Inspector

    [Header("Game State")]
    public int currentPlayerTurn = 0;
    public int turnNumber = 0;

    [Header("Mana System")]
    public int[] currentMana = new int[2];
    public int[] maxMana = new int[2];
    private const int MAX_MANA_CAP = 6;

    // NEW - Deck & Hand System
    [Header("Card System")]
    private List<MinionData>[] playerDecks = new List<MinionData>[2];
    private List<MinionData>[] playerHands = new List<MinionData>[2];
    private const int MAX_HAND_SIZE = 10;
    private const int STARTING_HAND_SIZE = 3;

    private List<Minion> allMinions = new List<Minion>();
    private Minion selectedMinion;
    private MinionData selectedCard; // NEW - Track selected card for spawning
    private bool gameStarted = false;
    private ManaUI manaUI;
    private HandUI handUI;

    private HashSet<Minion> minionsActedThisTurn = new HashSet<Minion>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate GameManager detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (gameStarted) return;
        gameStarted = true;

        board = FindFirstObjectByType<Board>();

        if (board == null)
        {
            Debug.LogError("Board not found!");
            return;
        }

        if (warriorData == null)
        {
            Debug.LogError("Warrior Data not assigned!");
            return;
        }

        // Create UIs
        GameObject uiObj = new GameObject("ManaUI");
        manaUI = uiObj.AddComponent<ManaUI>();

        GameObject handObj = new GameObject("HandUI");
        handUI = handObj.AddComponent<HandUI>();
        handUI.OnCardClicked = OnCardSelected;

        // Initialize systems
        InitializeMana();
        InitializeDecks();

        // Place Villagers (free placement at start)
        PlaceVillager(0, new Vector2Int(4, 0));  // Player 1
        PlaceVillager(1, new Vector2Int(5, 7));  // Player 2 (AI)

        // Draw starting hands
        DrawCards(0, STARTING_HAND_SIZE);
        DrawCards(1, STARTING_HAND_SIZE);
        UpdateHandUI();

        Debug.Log($"Game started. Turn: {turnNumber}");
    }

    void InitializeMana()
    {
        currentMana[0] = 1;
        currentMana[1] = 1;
        maxMana[0] = 1;
        maxMana[1] = 1;

        UpdateManaUI();
    }

    [Header("Deck Configuration")]
    public MinionData villagerData;  // Keep this for Villager placement
    public MinionData zombieData;
    public MinionData skeletonData;
    public MinionData creeperData;
    public MinionData ironGolemData;

    void InitializeDecks()
    {
        // Create decks for both players
        for (int i = 0; i < 2; i++)
        {
            playerDecks[i] = new List<MinionData>();
            playerHands[i] = new List<MinionData>();

            // Build a starter deck with variety (15 cards total)
            // 5 Zombies (cheap fodder)
            for (int j = 0; j < 5; j++)
            {
                if (zombieData != null) playerDecks[i].Add(zombieData);
            }

            // 4 Iron Golems (mid-range)
            for (int j = 0; j < 4; j++)
            {
                if (ironGolemData != null) playerDecks[i].Add(ironGolemData);
            }

            // 3 Skeletons (ranged)
            for (int j = 0; j < 3; j++)
            {
                if (skeletonData != null) playerDecks[i].Add(skeletonData);
            }

            // 3 Creepers (explosive)
            for (int j = 0; j < 3; j++)
            {
                if (creeperData != null) playerDecks[i].Add(creeperData);
            }

            // Shuffle deck
            ShuffleDeck(i);
        }

        Debug.Log($"Decks initialized: {playerDecks[0].Count} cards per player");
    }

    void PlaceVillager(int playerID, Vector2Int position)
    {
        if (villagerData == null)
        {
            Debug.LogError("Villager data not assigned!");
            return;
        }

        // Villagers are placed for free at game start
        GameObject villagerObj = new GameObject($"Villager_P{playerID}");
        Minion villager = villagerObj.AddComponent<Minion>();
        villager.Initialize(villagerData, position, playerID);
        allMinions.Add(villager);

        Debug.Log($"👑 Villager placed for Player {playerID + 1} at {position}");
    }

    void ShuffleDeck(int playerID)
    {
        List<MinionData> deck = playerDecks[playerID];

        // Fisher-Yates shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            MinionData temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }

        Debug.Log($"Player {playerID + 1} deck shuffled");
    }

    void DrawCards(int playerID, int count)
    {
        for (int i = 0; i < count; i++)
        {
            DrawCard(playerID);
        }
    }

    void DrawCard(int playerID)
    {
        if (playerHands[playerID].Count >= MAX_HAND_SIZE)
        {
            Debug.Log($"Player {playerID + 1} hand is full! Cannot draw.");
            return;
        }

        if (playerDecks[playerID].Count == 0)
        {
            Debug.Log($"Player {playerID + 1} deck is empty!");
            return;
        }

        MinionData drawnCard = playerDecks[playerID][0];
        playerDecks[playerID].RemoveAt(0);
        playerHands[playerID].Add(drawnCard);

        Debug.Log($"Player {playerID + 1} drew: {drawnCard.minionName}. Hand: {playerHands[playerID].Count}, Deck: {playerDecks[playerID].Count}");
    }


    void OnCardSelected(MinionData card)
    {
        if (currentPlayerTurn != 0)
        {
            Debug.Log("Not your turn!");
            return;
        }

        selectedCard = card;
        selectedMinion = null; // Deselect any minion

        Debug.Log($"Selected card: {card.minionName} (Cost: {card.manaCost})");
        Debug.Log("Click on your spawn zone to place this minion");
    }

    void Update()
    {
        int beforeCount = allMinions.Count;
        allMinions.RemoveAll(m => m == null);
        if (allMinions.Count != beforeCount)
        {
            Debug.Log($"Minion died. Remaining: {allMinions.Count}");
            CheckWinCondition();
        }

        HandleInput();
    }

    void HandleInput()
    {
        if (currentPlayerTurn != 0) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // LEFT CLICK
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                Vector2Int gridPos = board.WorldToGrid(hit.point);
                Minion clickedMinion = hit.collider.GetComponent<Minion>();

                // If we have a card selected, try to spawn it
                if (selectedCard != null)
                {
                    if (board.IsInSpawnZone(gridPos, currentPlayerTurn) &&
                        board.IsValidPosition(gridPos) &&
                        GetMinionAt(gridPos) == null)
                    {
                        // Check mana cost
                        if (SpendMana(currentPlayerTurn, selectedCard.manaCost))
                        {
                            SpawnMinionFromCard(selectedCard, gridPos, currentPlayerTurn);
                            playerHands[currentPlayerTurn].Remove(selectedCard);
                            selectedCard = null;
                            UpdateHandUI();
                            UpdateManaUI();
                        }
                        else
                        {
                            Debug.Log($"Not enough mana! Need {selectedCard.manaCost}, have {currentMana[currentPlayerTurn]}");
                        }
                    }
                    else
                    {
                        Debug.Log("Invalid spawn location! Must be in your spawn zone.");
                    }
                }
                // Otherwise, select minion or move
                else if (clickedMinion != null && clickedMinion.ownerID == 0)
                {
                    if (selectedMinion != null)
                    {
                        selectedMinion.SetHighlight(false);
                    }

                    selectedMinion = clickedMinion;
                    selectedMinion.SetHighlight(true);
                    Debug.Log($"✓ Selected: {selectedMinion.data.minionName}");
                }
                // Movement logic
                else if (selectedMinion != null)
                {
                    bool hasFreeMove = !minionsActedThisTurn.Contains(selectedMinion);

                    // Check if this minion's movement costs mana (Villager)
                    bool movementCostsMana = selectedMinion.data.movementCostsMana;

                    if (movementCostsMana)
                    {
                        // Villager: Always costs mana to move (1 for move, 2 for dash)
                        int moveCost = hasFreeMove ? 1 : 2; // Dash costs 2 total for Villager

                        List<Vector2Int> validMoves = selectedMinion.GetValidMoves();
                        if (validMoves.Contains(gridPos) && SpendMana(currentPlayerTurn, moveCost))
                        {
                            Debug.Log($"Villager move to {gridPos} (cost {moveCost} mana)");
                            selectedMinion.MoveTo(gridPos);
                            minionsActedThisTurn.Add(selectedMinion);
                            selectedMinion.SetHighlight(false);
                            selectedMinion = null;
                            board.ClearHighlight();
                            UpdateManaUI();
                        }
                        else if (!validMoves.Contains(gridPos))
                        {
                            Debug.Log("Cannot move there");
                        }
                        else
                        {
                            Debug.Log($"Not enough mana! Need {moveCost}");
                        }
                    }
                    else
                    {
                        // Normal minion movement
                        List<Vector2Int> validMoves = selectedMinion.GetValidMoves();

                        if (hasFreeMove)
                        {
                            if (validMoves.Contains(gridPos))
                            {
                                Debug.Log($"Free move to {gridPos}");
                                selectedMinion.MoveTo(gridPos);
                                minionsActedThisTurn.Add(selectedMinion);
                                selectedMinion.SetHighlight(false);
                                selectedMinion = null;
                                board.ClearHighlight();
                            }
                        }
                        else
                        {
                            if (validMoves.Contains(gridPos) && SpendMana(currentPlayerTurn, 1))
                            {
                                Debug.Log($"Dash to {gridPos} (cost 1 mana)");
                                selectedMinion.MoveTo(gridPos);
                                selectedMinion.SetHighlight(false);
                                selectedMinion = null;
                                board.ClearHighlight();
                                UpdateManaUI();
                            }
                            else if (!validMoves.Contains(gridPos))
                            {
                                Debug.Log("Cannot move there");
                            }
                            else
                            {
                                Debug.Log("Not enough mana to dash!");
                            }
                        }
                    }
                }
            }
        }

        // RIGHT CLICK - Attack
        if (mouse.rightButton.wasPressedThisFrame && selectedMinion != null)
        {
            selectedCard = null;

            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                Vector2Int targetPos = board.WorldToGrid(hit.point);
                List<Vector2Int> validTargets = selectedMinion.GetValidAttackTargets();

                if (validTargets.Contains(targetPos))
                {
                    if (SpendMana(currentPlayerTurn, 1))
                    {
                        Minion targetMinion = GetMinionAt(targetPos);

                        Debug.Log($"⚔️ {selectedMinion.data.minionName} attacks! (cost 1 mana)");

                        // Handle special attacks
                        if (selectedMinion.data.attackPattern == CHEGG.AttackPattern.Explosion)
                        {
                            // Creeper explosion - hits all surrounding
                            foreach (Vector2Int explosionPos in validTargets)
                            {
                                Minion victim = GetMinionAt(explosionPos);
                                if (victim != null)
                                {
                                    victim.TakeDamage(selectedMinion.data.attack);
                                }
                            }
                            // Creeper dies after exploding
                            selectedMinion.TakeDamage(999);
                        }
                        else
                        {
                            // Normal attack
                            Attack(selectedMinion, targetMinion);
                        }

                        minionsActedThisTurn.Add(selectedMinion);
                        selectedMinion.SetHighlight(false);
                        selectedMinion = null;
                        UpdateManaUI();
                    }
                    else
                    {
                        Debug.Log("Not enough mana to attack!");
                    }
                }
                else
                {
                    Debug.Log("Invalid target!");
                }
            }
        }

        // SPACE - End turn
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            selectedCard = null;
            EndTurn();
        }

        // ESC - Deselect
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (selectedMinion != null)
            {
                selectedMinion.SetHighlight(false);
                selectedMinion = null;
            }
            selectedCard = null;
            Debug.Log("Deselected");
        }
    }

    void SpawnMinionFromCard(MinionData data, Vector2Int position, int ownerID)
    {
        GameObject minionObj = new GameObject($"{data.minionName}_{ownerID}_{position}");
        Minion minion = minionObj.AddComponent<Minion>();
        minion.Initialize(data, position, ownerID);
        allMinions.Add(minion);

        // Minions cannot act on spawn turn
        minionsActedThisTurn.Add(minion);

        Debug.Log($"Spawned {data.minionName} at {position} for {data.manaCost} mana");
    }

    void UpdateHandUI()
    {
        if (handUI != null && currentPlayerTurn == 0)
        {
            handUI.UpdateHand(playerHands[0]);
        }
    }

    bool SpendMana(int playerID, int amount)
    {
        if (currentMana[playerID] >= amount)
        {
            currentMana[playerID] -= amount;
            return true;
        }
        return false;
    }

    void RefreshMana(int playerID)
    {
        currentMana[playerID] = maxMana[playerID];
    }

    void IncreaseManaPool(int playerID)
    {
        if (maxMana[playerID] < MAX_MANA_CAP)
        {
            maxMana[playerID]++;
        }
        RefreshMana(playerID);
    }

    void UpdateManaUI()
    {
        if (manaUI != null)
        {
            manaUI.UpdateManaDisplay(0, currentMana[0], maxMana[0]);
            manaUI.UpdateManaDisplay(1, currentMana[1], maxMana[1]);
        }
    }

    void Attack(Minion attacker, Minion target)
    {
        target.TakeDamage(attacker.data.attack);

        if (target != null && target.currentHealth > 0)
        {
            attacker.TakeDamage(target.data.attack);
        }
    }

    bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        int distance = Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
        return distance == 1;
    }

    bool CanMoveTo(Minion minion, Vector2Int targetPos)
    {
        if (targetPos == minion.gridPosition) return false;

        int distance = Mathf.Abs(targetPos.x - minion.gridPosition.x) +
                       Mathf.Abs(targetPos.y - minion.gridPosition.y);

        if (distance > minion.data.moveRange) return false;

        Minion occupant = GetMinionAt(targetPos);
        if (occupant != null && occupant != minion) return false;

        return true;
    }

    public Minion GetMinionAt(Vector2Int position)
    {
        foreach (Minion minion in allMinions)
        {
            if (minion != null && minion.gridPosition == position)
            {
                return minion;
            }
        }
        return null;
    }

    void CheckWinCondition()
    {
        bool playerHasMinions = allMinions.Exists(m => m != null && m.ownerID == 0);
        bool aiHasMinions = allMinions.Exists(m => m != null && m.ownerID == 1);

        if (!playerHasMinions)
        {
            Debug.Log("💀 GAME OVER - PLAYER 2 WINS!");
        }
        else if (!aiHasMinions)
        {
            Debug.Log("🎉 PLAYER 1 WINS!");
        }
    }

    // Add this method after CheckWinCondition()
    public void OnVillagerDeath(int deadPlayerID)
    {
        int winnerID = deadPlayerID == 0 ? 1 : 0;
        Debug.Log($"💀👑 VILLAGER DIED! Player {deadPlayerID + 1} loses!");
        Debug.Log($"🎉 PLAYER {winnerID + 1} WINS!");

        // You can add UI popup here later
        Time.timeScale = 0; // Pause game
    }

    void EndTurn()
    {
        Debug.Log($"=== END TURN {turnNumber} ===");

        currentPlayerTurn = (currentPlayerTurn + 1) % 2;
        turnNumber++;

        minionsActedThisTurn.Clear();

        IncreaseManaPool(currentPlayerTurn);
        DrawCard(currentPlayerTurn);
        UpdateManaUI();
        UpdateHandUI();

        Debug.Log($"=== TURN {turnNumber} - PLAYER {currentPlayerTurn + 1} ===");

        if (currentPlayerTurn == 1)
        {
            Invoke("AITurn", 1f);
        }
    }

    void AITurn()
    {
        Debug.Log($"AI Turn Start - Mana: {currentMana[1]}/{maxMana[1]}, Hand: {playerHands[1].Count}");

        // Try to spawn ONE minion if possible
        bool spawnedThisTurn = false;

        if (playerHands[1].Count > 0 && currentMana[1] >= 1 && !spawnedThisTurn)
        {
            // Find cheapest affordable card
            MinionData cheapestCard = null;
            foreach (MinionData card in playerHands[1])
            {
                if (card.manaCost <= currentMana[1])
                {
                    if (cheapestCard == null || card.manaCost <= cheapestCard.manaCost)
                    {
                        cheapestCard = card;
                    }
                }
            }

            // Try to spawn the cheapest card
            if (cheapestCard != null)
            {
                // Find valid spawn position
                for (int x = 0; x < board.width && !spawnedThisTurn; x++)
                {
                    for (int z = 6; z <= 7 && !spawnedThisTurn; z++)
                    {
                        Vector2Int pos = new Vector2Int(x, z);
                        if (GetMinionAt(pos) == null)
                        {
                            SpendMana(1, cheapestCard.manaCost);
                            SpawnMinionFromCard(cheapestCard, pos, 1);
                            playerHands[1].Remove(cheapestCard);
                            UpdateManaUI();
                            spawnedThisTurn = true; // Stop spawning
                            Debug.Log($"AI spawned {cheapestCard.minionName} at {pos} for {cheapestCard.manaCost} mana");
                            break; // Break inner loop
                        }
                    }
                }
            }
            else
            {
                Debug.Log("AI has no affordable cards to spawn");
            }
        }

        // Try to attack or move with ONE minion
        List<Minion> aiMinions = allMinions.FindAll(m => m != null && m.ownerID == 1 && !minionsActedThisTurn.Contains(m));

        if (aiMinions.Count > 0)
        {
            // Pick first available minion
            Minion aiMinion = aiMinions[0];
            Minion playerMinion = allMinions.Find(m => m != null && m.ownerID == 0);

            if (playerMinion != null)
            {
                // Try to attack if adjacent and have mana
                if (IsAdjacent(aiMinion.gridPosition, playerMinion.gridPosition) && currentMana[1] >= 1)
                {
                    if (SpendMana(1, 1))
                    {
                        Debug.Log($"AI {aiMinion.gameObject.name} attacks {playerMinion.gameObject.name}");
                        Attack(aiMinion, playerMinion);
                        minionsActedThisTurn.Add(aiMinion);
                        UpdateManaUI();
                    }
                }
                else
                {
                    // Move toward player (free movement)
                    Vector2Int currentPos = aiMinion.gridPosition;
                    Vector2Int targetDirection = Vector2Int.zero;

                    if (playerMinion.gridPosition.x > aiMinion.gridPosition.x)
                        targetDirection = new Vector2Int(1, 0);
                    else if (playerMinion.gridPosition.x < aiMinion.gridPosition.x)
                        targetDirection = new Vector2Int(-1, 0);
                    else if (playerMinion.gridPosition.y > aiMinion.gridPosition.y)
                        targetDirection = new Vector2Int(0, 1);
                    else if (playerMinion.gridPosition.y < aiMinion.gridPosition.y)
                        targetDirection = new Vector2Int(0, -1);

                    Vector2Int targetPos = currentPos + targetDirection;

                    if (board.IsValidPosition(targetPos) && GetMinionAt(targetPos) == null)
                    {
                        Debug.Log($"AI {aiMinion.gameObject.name} moves from {currentPos} to {targetPos}");
                        aiMinion.MoveTo(targetPos);
                        minionsActedThisTurn.Add(aiMinion);
                    }
                }
            }
        }

        Debug.Log($"AI Turn End - Mana: {currentMana[1]}/{maxMana[1]}");
        EndTurn();
    }

}