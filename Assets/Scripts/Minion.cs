using System.Collections.Generic;
using UnityEngine;


namespace Chegg
{
    public abstract class Minion
    {
        // ==================== PROPERTIES ====================
        // What type of minion this is (Zombie, Skeleton, etc.)

        public MinionType Type { get; protected set; }

        // Which player owns this minion
        public PlayerTeam Owner { get; set; }

        // Current position on the board
        public Vector2Int Position { get; set; }

        // How much mana it costs to spawn this minion
        public int ManaCost { get; protected set; }

        // ==================== TURN STATE ====================


        /// Has this minion moved this turn?
        /// Used to prevent moving twice
        public bool HasMovedThisTurn { get; set; }


        // Has this minion attacked this turn?
        // Used to prevent attacking twice
        public bool HasAttackedThisTurn { get; set; }


        // Was this minion just spawned?
        // Minions can't act on the turn they're spawned (summoning sickness)
        public bool JustSpawned { get; set; }

        // ==================== ABSTRACT METHODS ====================
        // These MUST be implemented by each minion type

        
        // Returns list of positions this minion can move to (relative to current position)
        // Example: Zombie returns {(-1,1), (0,1), (1,1)} for forward movement
        public abstract List<Vector2Int> GetMovePattern();

        // Returns list of positions this minion can attack (relative to current position)
        // Example: Skeleton returns diagonal positions up to 3 squares away
        public abstract List<Vector2Int> GetAttackPattern();

        // ==================== VIRTUAL METHODS ====================
        // These CAN be overridden by specific minions, but have default behavior

        // Can this minion move to a specific tile?
        // Override for special movement rules (e.g., Phantom only on dark tiles)
        public virtual bool CanMoveToTile(Tile tile)
        {
            return true; // By default, minions can move anywhere
        }

        // Called when this minion is spawned
        // Override for spawn effects (e.g., Pig draws a card, Wither explodes)
        public virtual void OnSpawn(GameManager gameManager)
        {
            // Default: do nothing
        }
 
        // Called when this minion dies
        // Override for death effects (e.g., Pig draws a card)
        public virtual void OnDeath(GameManager gameManager)
        {
            // Default: do nothing
        }

        
        // Can this minion attack right now?
        // Checks summoning sickness and turn state
        
        public virtual bool CanAttack()
        {
            return !JustSpawned && !HasAttackedThisTurn;
        }

        // Can this minion move right now?
        // Checks summoning sickness and turn state
        public virtual bool CanMove()
        {
            return !JustSpawned && !HasMovedThisTurn && !HasAttackedThisTurn;
        }

        // Reset this minion's state at the start of a new turn
        public void ResetTurnState()
        {
            HasMovedThisTurn = false;
            HasAttackedThisTurn = false;
            JustSpawned = false;
        }

        // Get a description of this minion (for tooltips, etc.)
        public virtual string GetDescription()
        {
            return $"{Type} - Cost: {ManaCost}";
        }
    }
}