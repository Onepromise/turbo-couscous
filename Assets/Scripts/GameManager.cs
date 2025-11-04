using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameManager.cs - The "brain" of the game
/// 
/// WHY: This is the central controller that:
/// - Initializes the game
/// - Manages turns
/// - Executes player actions (spawn, move, attack)
/// - Checks win conditions
/// - Coordinates between all systems
/// 
/// Think of it as the "referee" - it enforces rules and manages game flow
/// 
/// PATTERN: This uses the Singleton pattern so any script can access it
/// via GameManager.Instance
/// </summary>

namespace Chegg
{
    public class GameManager : MonoBehaviour
    {
        // ==================== SINGLETON ====================
        
        public static GameManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        // ==================== REFERENCES ====================
        
        /// <summary>
        /// The game board
        /// </summary>
        public Board Board { get; private set; }
        
        /// <summary>
        /// Red player (goes first)
        /// </summary>
        public Player RedPlayer { get; private set; }
        
        /// <summary>
        /// Blue player (goes second)
        /// </summary>
        public Player BluePlayer { get; private set; }
        
        /// <summary>
        /// Which player's turn is it?
        /// </summary>
        public Player CurrentPlayer { get; private set; }
        
        /// <summary>
        /// Is the game over?
        /// </summary>
        public bool GameOver { get; private set; }
        
        /// <summary>
        /// Who won? (null if game not over)
        /// </summary>
        public Player Winner { get; private set; }
        
        // ==================== EVENTS ====================
        // Other scripts can subscribe to these to react to game events
        
        public UnityEvent<Player> OnTurnStart = new UnityEvent<Player>();
        public UnityEvent<Player> OnTurnEnd = new UnityEvent<Player>();
        public UnityEvent<Minion, Vector2Int> OnMinionSpawned = new UnityEvent<Minion, Vector2Int>();
        public UnityEvent<Minion, Vector2Int, Vector2Int> OnMinionMoved = new UnityEvent<Minion, Vector2Int, Vector2Int>();
        public UnityEvent<Minion, Minion> OnMinionAttacked = new UnityEvent<Minion, Minion>();
        public UnityEvent<Minion> OnMinionDied = new UnityEvent<Minion>();
        public UnityEvent<Player> OnGameOver = new UnityEvent<Player>();
        
        // ==================== INITIALIZATION ====================
        
        void Start()
        {
            InitializeGame();
        }
        
        /// <summary>
        /// Set up the entire game
        /// </summary>
        public void InitializeGame()
        {
            Debug.Log("=== INITIALIZING CHEGG GAME ===");
            
            // Get or create the Board
            Board = GetComponent<Board>();
            if (Board == null)
            {
                Board = gameObject.AddComponent<Board>();
            }
            Board.Initialize();
            
            // Create both players
            RedPlayer = new Player(PlayerTeam.Red, "Red Player");
            BluePlayer = new Player(PlayerTeam.Blue, "Blue Player");
            
            // Set up decks
            SetupDecks();
            
            // Draw starting hands (3 cards each)
            RedPlayer.Hand.AddRange(RedPlayer.Deck.DrawMultiple(3));
            BluePlayer.Hand.AddRange(BluePlayer.Deck.DrawMultiple(3));
            
            // Red player goes first
            CurrentPlayer = RedPlayer;
            GameOver = false;
            Winner = null;
            
            Debug.Log("Game initialized! Red player's turn");
            
            // Start the first turn
            CurrentPlayer.StartTurn();
            OnTurnStart?.Invoke(CurrentPlayer);
        }
        
        /// <summary>
        /// Create example decks for both players
        /// In a real game, players would build custom decks
        /// </summary>
        private void SetupDecks()
        {
            // Create a balanced 15-card deck
            List<MinionType> redDeck = new List<MinionType>
            {
                // Low cost (1 mana) - 4 cards
                MinionType.Zombie, MinionType.Zombie, MinionType.Zombie,
                MinionType.Pig,
                
                // Medium cost (2 mana) - 4 cards
                MinionType.IronGolem, MinionType.IronGolem,
                MinionType.Rabbit, MinionType.PufferFish,
                
                // Mid cost (3 mana) - 3 cards
                MinionType.Skeleton, MinionType.Skeleton,
                MinionType.Blaze,
                
                // High cost (4 mana) - 2 cards
                MinionType.Slime, MinionType.Enderman,
                
                // Very high cost (5-6 mana) - 2 cards
                MinionType.Cat, MinionType.Wither
            };
            
            // Blue gets the same deck (could be different)
            List<MinionType> blueDeck = new List<MinionType>(redDeck);
            
            // Initialize decks
            RedPlayer.Deck.Initialize(redDeck);
            BluePlayer.Deck.Initialize(blueDeck);
            
            Debug.Log("Decks created and shuffled");
        }
        
        // ==================== TURN MANAGEMENT ====================
        
        /// <summary>
        /// End the current player's turn and switch to opponent
        /// </summary>
        public void EndTurn()
        {
            if (GameOver) return;
            
            Debug.Log($"{CurrentPlayer.PlayerName} ends their turn");
            
            OnTurnEnd?.Invoke(CurrentPlayer);
            
            // Switch players
            CurrentPlayer = (CurrentPlayer == RedPlayer) ? BluePlayer : RedPlayer;
            
            // Start new turn
            CurrentPlayer.StartTurn();
            OnTurnStart?.Invoke(CurrentPlayer);
        }
        
        // ==================== MINION ACTIONS ====================
        
        /// <summary>
        /// Spawn a minion from the player's hand onto the board
        /// </summary>
        public bool SpawnMinion(MinionType minionType, Vector2Int position)
        {
            if (GameOver) return false;
            
            // Create the minion
            Minion minion = MinionFactory.CreateMinion(minionType);
            if (minion == null)
            {
                Debug.LogError($"Failed to create minion of type {minionType}");
                return false;
            }
            
            // Check if player has this card
            if (!CurrentPlayer.Hand.Contains(minionType))
            {
                Debug.LogWarning($"{CurrentPlayer.PlayerName} doesn't have {minionType} in hand!");
                return false;
            }
            
            // Check if player can afford it
            if (!CurrentPlayer.CanAfford(minion.ManaCost))
            {
                Debug.LogWarning($"{CurrentPlayer.PlayerName} can't afford {minionType} (costs {minion.ManaCost})");
                return false;
            }
            
            // Check if position is valid spawn zone
            Tile tile = Board.GetTile(position);
            if (tile == null || !tile.IsSpawnZone || tile.SpawnZoneTeam != CurrentPlayer.Team)
            {
                Debug.LogWarning("Invalid spawn location!");
                return false;
            }
            
            // Check if tile is occupied
            if (tile.IsOccupied)
            {
                Debug.LogWarning("Spawn location is occupied!");
                return false;
            }
            
            // Everything checks out - spawn the minion!
            minion.Owner = CurrentPlayer.Team;
            minion.Position = position;
            minion.JustSpawned = true;
            
            // Place on board
            Board.PlaceMinion(minion, position);
            
            // Update player state
            CurrentPlayer.RemoveCardFromHand(minionType);
            CurrentPlayer.SpendMana(minion.ManaCost);
            CurrentPlayer.AddMinion(minion);
            
            // Trigger spawn effects
            minion.OnSpawn(this);
            
            Debug.Log($"{CurrentPlayer.PlayerName} spawned {minionType} at {position}");
            OnMinionSpawned?.Invoke(minion, position);
            
            return true;
        }
        
        /// <summary>
        /// Move a minion to a new position
        /// </summary>
        public bool MoveMinion(Minion minion, Vector2Int targetPosition)
        {
            if (GameOver) return false;
            
            // Check if it's the minion owner's turn
            if (minion.Owner != CurrentPlayer.Team)
            {
                Debug.LogWarning("Can't move opponent's minion!");
                return false;
            }
            
            // Check if minion can move
            if (!minion.CanMove())
            {
                Debug.LogWarning($"{minion.Type} cannot move (summoning sickness or already acted)");
                return false;
            }
            
            // Check if move is valid
            List<Vector2Int> validMoves = Board.GetValidMoves(minion);
            if (!validMoves.Contains(targetPosition))
            {
                Debug.LogWarning("Invalid move position!");
                return false;
            }
            
            // Execute the move
            Vector2Int oldPosition = minion.Position;
            Board.MoveMinion(minion, targetPosition);
            minion.HasMovedThisTurn = true;
            
            Debug.Log($"{minion.Type} moved from {oldPosition} to {targetPosition}");
            OnMinionMoved?.Invoke(minion, oldPosition, targetPosition);
            
            return true;
        }
        
        /// <summary>
        /// Attack with a minion
        /// </summary>
        public bool AttackWithMinion(Minion attacker, Vector2Int targetPosition)
        {
            if (GameOver) return false;
            
            // Check if it's the attacker owner's turn
            if (attacker.Owner != CurrentPlayer.Team)
            {
                Debug.LogWarning("Can't attack with opponent's minion!");
                return false;
            }
            
            // Check if minion can attack
            if (!attacker.CanAttack())
            {
                Debug.LogWarning($"{attacker.Type} cannot attack");
                return false;
            }
            
            // Check if player can afford attack (costs 1 mana)
            if (!CurrentPlayer.CanAfford(1))
            {
                Debug.LogWarning("Not enough mana to attack!");
                return false;
            }
            
            // Check if target position is valid
            List<Vector2Int> validAttacks = Board.GetValidAttacks(attacker);
            if (!validAttacks.Contains(targetPosition))
            {
                Debug.LogWarning("Invalid attack target!");
                return false;
            }
            
            // Get the target minion
            Tile targetTile = Board.GetTile(targetPosition);
            Minion target = targetTile.OccupyingMinion;
            
            if (target == null)
            {
                Debug.LogWarning("No target at attack position!");
                return false;
            }
            
            // Execute the attack
            CurrentPlayer.SpendMana(1);
            attacker.HasAttackedThisTurn = true;
            
            Debug.Log($"{attacker.Type} attacks {target.Type}!");
            OnMinionAttacked?.Invoke(attacker, target);
            
            // Destroy the target
            DestroyMinion(target);
            
            // Check win condition
            if (!target.Owner.Equals(CurrentPlayer.Team))
            {
                Player opponent = (CurrentPlayer == RedPlayer) ? BluePlayer : RedPlayer;
                if (!opponent.IsAlive())
                {
                    EndGame(CurrentPlayer);
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Destroy a minion (remove from board)
        /// </summary>
        public void DestroyMinion(Minion minion)
        {
            Debug.Log($"{minion.Type} was destroyed!");
            
            // Remove from board
            Board.RemoveMinion(minion);
            
            // Remove from player's minion list
            Player owner = (minion.Owner == PlayerTeam.Red) ? RedPlayer : BluePlayer;
            owner.RemoveMinion(minion);
            
            // Trigger death effects
            minion.OnDeath(this);
            
            OnMinionDied?.Invoke(minion);
        }
        
        // ==================== WIN CONDITION ====================
        
        /// <summary>
        /// End the game with a winner
        /// </summary>
        private void EndGame(Player winner)
        {
            GameOver = true;
            Winner = winner;
            
            Debug.Log($"=== GAME OVER - {winner.PlayerName} WINS! ===");
            OnGameOver?.Invoke(winner);
        }
        
        // ==================== UTILITY ====================
        
        /// <summary>
        /// Get the opponent of the current player
        /// </summary>
        public Player GetOpponent()
        {
            return (CurrentPlayer == RedPlayer) ? BluePlayer : RedPlayer;
        }
        
        /// <summary>
        /// Get player by team
        /// </summary>
        public Player GetPlayer(PlayerTeam team)
        {
            return (team == PlayerTeam.Red) ? RedPlayer : BluePlayer;
        }
    }
}