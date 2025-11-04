using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameUIController.cs - Manages all UI and player input
/// 
/// WHY: Separating UI from game logic:
/// - Easier to change UI without breaking game rules
/// - Can swap UI systems (e.g., Unity UI to custom)
/// - Easier to add different input methods (touch, controller)
/// - Game logic stays clean and testable
/// 
/// RESPONSIBILITIES:
/// - Display board visually
/// - Handle mouse/touch input
/// - Show player hand
/// - Display mana, turn info
/// - Highlight valid moves/attacks
/// - Create and update visual minion representations
/// </summary>

namespace Chegg
{
    public class GameUIController : MonoBehaviour
    {
        // ==================== REFERENCES ====================
        
        [Header("Game References")]
        [Tooltip("Reference to the game manager")]
        public GameManager gameManager;
        
        [Tooltip("Camera for raycasting clicks")]
        public Camera mainCamera;
        
        // ==================== PREFABS ====================
        
        [Header("Prefabs")]
        [Tooltip("Prefab for board tiles")]
        public GameObject tilePrefab;
        
        [Tooltip("Prefab for minion visual representation")]
        public GameObject minionPrefab;
        
        [Tooltip("Parent transform for board objects")]
        public Transform boardParent;
        
        [Tooltip("How big each tile is in world space")]
        public float tileSize = 1f;
        
        // ==================== UI ELEMENTS ====================
        
        [Header("UI Panels")]
        [Tooltip("Text showing current player")]
        public TextMeshProUGUI currentPlayerText;
        
        [Tooltip("Text showing mana amount")]
        public TextMeshProUGUI manaText;
        
        [Tooltip("Text showing turn number")]
        public TextMeshProUGUI turnNumberText;
        
        [Tooltip("Container for hand cards")]
        public Transform handContainer;
        
        [Tooltip("Prefab for card UI")]
        public GameObject cardPrefab;
        
        [Tooltip("Button to end turn")]
        public Button endTurnButton;
        
        [Tooltip("Panel shown when game ends")]
        public GameObject gameOverPanel;
        
        [Tooltip("Text showing winner")]
        public TextMeshProUGUI winnerText;
        
        // ==================== COLORS ====================
        
        [Header("Visual Settings")]
        [Tooltip("Color for light tiles")]
        public Color lightTileColor = Color.white;
        
        [Tooltip("Color for dark tiles")]
        public Color darkTileColor = new Color(0.7f, 0.7f, 0.7f);
        
        [Tooltip("Color for red spawn zone")]
        public Color redSpawnColor = new Color(1f, 0.6f, 0.6f);
        
        [Tooltip("Color for blue spawn zone")]
        public Color blueSpawnColor = new Color(0.6f, 0.6f, 1f);
        
        [Tooltip("Color for valid move highlights")]
        public Color moveHighlightColor = Color.yellow;
        
        [Tooltip("Color for valid attack highlights")]
        public Color attackHighlightColor = Color.red;
        
        // ==================== INTERNAL STATE ====================
        
        // Store visual representations
        private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Minion, GameObject> minionObjects = new Dictionary<Minion, GameObject>();
        
        // Current selection state
        private Minion selectedMinion = null;
        private MinionType? selectedCardToSpawn = null;
        private bool isSpawningMode = false;
        
        // ==================== INITIALIZATION ====================
        
        void Start()
        {
            // Find game manager if not assigned
            if (gameManager == null)
                gameManager = GameManager.Instance;
            
            // Subscribe to game events
            SubscribeToEvents();
            
            // Create the visual board
            CreateBoardVisualization();
            
            // Setup button
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
            
            // Hide game over panel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            // Initial UI update
            UpdateUI();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Subscribe to game manager events
        /// WHY: This keeps UI in sync with game state automatically
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager == null) return;
            
            gameManager.OnTurnStart.AddListener(OnTurnStarted);
            gameManager.OnMinionSpawned.AddListener(OnMinionSpawned);
            gameManager.OnMinionMoved.AddListener(OnMinionMoved);
            gameManager.OnMinionDied.AddListener(OnMinionDied);
            gameManager.OnGameOver.AddListener(OnGameOver);
        }
        
        private void UnsubscribeFromEvents()
        {
            if (gameManager == null) return;
            
            gameManager.OnTurnStart.RemoveListener(OnTurnStarted);
            gameManager.OnMinionSpawned.RemoveListener(OnMinionSpawned);
            gameManager.OnMinionMoved.RemoveListener(OnMinionMoved);
            gameManager.OnMinionDied.RemoveListener(OnMinionDied);
            gameManager.OnGameOver.RemoveListener(OnGameOver);
        }
        
        // ==================== BOARD CREATION ====================
        
        /// <summary>
        /// Create visual representation of the board
        /// WHY: Separates visual from logic - board could be represented
        /// many ways (3D, 2D sprites, ASCII, etc.) without changing logic
        /// </summary>
        void CreateBoardVisualization()
        {
            for (int x = 0; x < Board.WIDTH; x++)
            {
                for (int y = 0; y < Board.HEIGHT; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = gameManager.Board.GetTile(pos);
                    
                    // Create tile object
                    GameObject tileObj = Instantiate(tilePrefab, boardParent);
                    tileObj.transform.position = GridToWorldPosition(pos);
                    tileObj.name = $"Tile_{x}_{y}";
                    
                    // Color the tile based on type
                    Renderer renderer = tileObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = GetTileColor(tile);
                    }
                    
                    // Store reference
                    tileObjects[pos] = tileObj;
                }
            }
            
            Debug.Log($"Created {tileObjects.Count} tile visuals");
        }
        
        /// <summary>
        /// Get the appropriate color for a tile
        /// </summary>
        Color GetTileColor(Tile tile)
        {
            if (tile.IsSpawnZone)
            {
                return tile.SpawnZoneTeam == PlayerTeam.Red ? redSpawnColor : blueSpawnColor;
            }
            else
            {
                return tile.Type == TileType.Light ? lightTileColor : darkTileColor;
            }
        }
        
        // ==================== INPUT HANDLING ====================
        
        void Update()
        {
            HandleMouseInput();
            HandleKeyboardInput();
        }
        
        /// <summary>
        /// Handle mouse clicks on the board
        /// </summary>
        void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    // Convert hit position to grid coordinates
                    Vector2Int gridPos = WorldToGridPosition(hit.point);
                    OnTileClicked(gridPos);
                }
            }
        }
        
        /// <summary>
        /// Handle keyboard shortcuts
        /// </summary>
        void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnEndTurnClicked();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectAll();
            }
        }
        
        /// <summary>
        /// Handle clicking on a tile
        /// WHY: This is where player intent gets translated to game actions
        /// </summary>
        void OnTileClicked(Vector2Int position)
        {
            // MODE 1: Spawning a minion
            if (isSpawningMode && selectedCardToSpawn.HasValue)
            {
                if (gameManager.SpawnMinion(selectedCardToSpawn.Value, position))
                {
                    // Success! Exit spawning mode
                    DeselectAll();
                }
                return;
            }
            
            Tile tile = gameManager.Board.GetTile(position);
            if (tile == null) return;
            
            // MODE 2: Selecting own minion
            if (tile.IsOccupied && 
                tile.OccupyingMinion.Owner == gameManager.CurrentPlayer.Team)
            {
                SelectMinion(tile.OccupyingMinion);
                return;
            }
            
            // MODE 3: Moving selected minion
            if (selectedMinion != null)
            {
                List<Vector2Int> validMoves = gameManager.Board.GetValidMoves(selectedMinion);
                
                if (validMoves.Contains(position))
                {
                    if (gameManager.MoveMinion(selectedMinion, position))
                    {
                        DeselectAll();
                    }
                    return;
                }
                
                // MODE 4: Attacking with selected minion
                List<Vector2Int> validAttacks = gameManager.Board.GetValidAttacks(selectedMinion);
                
                if (validAttacks.Contains(position))
                {
                    if (gameManager.AttackWithMinion(selectedMinion, position))
                    {
                        DeselectAll();
                    }
                    return;
                }
            }
        }
        
        // ==================== SELECTION ====================
        
        /// <summary>
        /// Select a minion and show valid moves/attacks
        /// </summary>
        void SelectMinion(Minion minion)
        {
            DeselectAll();
            selectedMinion = minion;
            UpdateHighlights();
            
            Debug.Log($"Selected {minion.Type} at {minion.Position}");
        }
        
        /// <summary>
        /// Deselect everything and clear highlights
        /// </summary>
        void DeselectAll()
        {
            selectedMinion = null;
            selectedCardToSpawn = null;
            isSpawningMode = false;
            ClearHighlights();
        }
        
        /// <summary>
        /// Highlight valid moves and attacks for selected minion
        /// </summary>
        void UpdateHighlights()
        {
            ClearHighlights();
            
            if (selectedMinion != null)
            {
                // Highlight valid moves in yellow
                List<Vector2Int> validMoves = gameManager.Board.GetValidMoves(selectedMinion);
                foreach (Vector2Int pos in validMoves)
                {
                    HighlightTile(pos, moveHighlightColor);
                }
                
                // Highlight valid attacks in red
                List<Vector2Int> validAttacks = gameManager.Board.GetValidAttacks(selectedMinion);
                foreach (Vector2Int pos in validAttacks)
                {
                    HighlightTile(pos, attackHighlightColor);
                }
            }
        }
        
        /// <summary>
        /// Highlight a specific tile
        /// </summary>
        void HighlightTile(Vector2Int pos, Color color)
        {
            if (tileObjects.ContainsKey(pos))
            {
                Renderer renderer = tileObjects[pos].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }
        
        /// <summary>
        /// Clear all highlights and restore original colors
        /// </summary>
        void ClearHighlights()
        {
            foreach (var kvp in tileObjects)
            {
                Vector2Int pos = kvp.Key;
                GameObject tileObj = kvp.Value;
                Tile tile = gameManager.Board.GetTile(pos);
                
                Renderer renderer = tileObj.GetComponent<Renderer>();
                if (renderer != null && tile != null)
                {
                    renderer.material.color = GetTileColor(tile);
                }
            }
        }
        
        // ==================== UI UPDATES ====================
        
        /// <summary>
        /// Update all UI elements
        /// </summary>
        void UpdateUI()
        {
            if (gameManager == null || gameManager.CurrentPlayer == null) return;
            
            Player current = gameManager.CurrentPlayer;
            
            // Update text displays
            currentPlayerText.text = $"Current Player: {current.PlayerName} ({current.Team})";
            manaText.text = $"Mana: {current.CurrentMana}/{current.MaxMana}";
            turnNumberText.text = $"Turn: {current.TurnNumber}";
            
            // Update hand display
            UpdateHandDisplay();
        }
        
        /// <summary>
        /// Update the card hand display
        /// </summary>
        void UpdateHandDisplay()
        {
            // Clear existing cards
            foreach (Transform child in handContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create card for each card in hand
            foreach (MinionType cardType in gameManager.CurrentPlayer.Hand)
            {
                GameObject cardObj = Instantiate(cardPrefab, handContainer);
                
                // Setup card text
                TextMeshProUGUI cardText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (cardText != null)
                {
                    int cost = MinionFactory.GetManaCost(cardType);
                    cardText.text = $"{cardType}\n({cost} mana)";
                }
                
                // Setup click handler
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    MinionType type = cardType; // Capture for lambda
                    cardButton.onClick.AddListener(() => OnCardClicked(type));
                }
            }
        }
        
        /// <summary>
        /// Handle clicking a card in hand
        /// </summary>
        void OnCardClicked(MinionType cardType)
        {
            selectedCardToSpawn = cardType;
            isSpawningMode = true;
            selectedMinion = null;
            
            Debug.Log($"Ready to spawn {cardType}. Click on your spawn zone.");
        }
        
        /// <summary>
        /// Handle end turn button click
        /// </summary>
        void OnEndTurnClicked()
        {
            DeselectAll();
            gameManager.EndTurn();
        }
        
        // ==================== COORDINATE CONVERSION ====================
        
        /// <summary>
        /// Convert grid position to world position
        /// </summary>
        Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x * tileSize, 0, gridPos.y * tileSize);
        }
        
        /// <summary>
        /// Convert world position to grid position
        /// </summary>
        Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / tileSize);
            int z = Mathf.RoundToInt(worldPos.z / tileSize);
            return new Vector2Int(x, z);
        }
        
        // ==================== EVENT HANDLERS ====================
        
        void OnTurnStarted(Player player)
        {
            UpdateUI();
        }
        
        void OnMinionSpawned(Minion minion, Vector2Int position)
        {
            CreateMinionVisual(minion);
            UpdateUI();
        }
        
        void OnMinionMoved(Minion minion, Vector2Int oldPos, Vector2Int newPos)
        {
            UpdateMinionVisual(minion);
        }
        
        void OnMinionDied(Minion minion)
        {
            DestroyMinionVisual(minion);
        }
        
        void OnGameOver(Player winner)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                winnerText.text = $"{winner.PlayerName} ({winner.Team}) Wins!";
            }
        }
        
        // ==================== MINION VISUALS ====================
        
        void CreateMinionVisual(Minion minion)
        {
            GameObject minionObj = Instantiate(minionPrefab, boardParent);
            Vector3 worldPos = GridToWorldPosition(minion.Position);
            minionObj.transform.position = worldPos + Vector3.up * 0.5f;
            minionObj.name = $"{minion.Type}_{minion.Owner}";
            
            // Color based on team
            Renderer renderer = minionObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = minion.Owner == PlayerTeam.Red ? Color.red : Color.blue;
            }
            
            // Add label
            TextMeshPro label = minionObj.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.text = minion.Type.ToString().Substring(0, Mathf.Min(3, minion.Type.ToString().Length));
            }
            
            minionObjects[minion] = minionObj;
        }
        
        void UpdateMinionVisual(Minion minion)
        {
            if (minionObjects.ContainsKey(minion))
            {
                Vector3 worldPos = GridToWorldPosition(minion.Position);
                minionObjects[minion].transform.position = worldPos + Vector3.up * 0.5f;
            }
        }
        
        void DestroyMinionVisual(Minion minion)
        {
            if (minionObjects.ContainsKey(minion))
            {
                Destroy(minionObjects[minion]);
                minionObjects.Remove(minion);
            }
        }
    }
}