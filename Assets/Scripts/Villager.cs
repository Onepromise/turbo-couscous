using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Villager.cs - The "King" piece
/// 
/// WHY: This is an example of a concrete minion implementation.
/// Each minion type inherits from Minion and defines its unique:
/// - Movement pattern
/// - Attack pattern
/// - Special abilities
/// - Mana cost
/// 
/// The Villager is special because:
/// - It's placed for free at game start
/// - If it dies, you lose the game
/// - It moves to the position it attacks (like a King in chess)
/// - Movement ALWAYS costs mana (even the free move)
/// </summary>

namespace Chegg
{
    public class Villager : Minion
    {
        /// <summary>
        /// Constructor - sets up the villager's properties
        /// </summary>
        public Villager()
        {
            Type = MinionType.Villager;
            ManaCost = 0; // Free to place at game start
        }
        
        /// <summary>
        /// Movement pattern: Can move to any of the 8 surrounding squares
        /// Like a King in chess
        /// </summary>
        public override List<Vector2Int> GetMovePattern()
        {
            return new List<Vector2Int>
            {
                // Top row
                new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1),
                // Middle row (left and right)
                new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
                // Bottom row
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
            };
        }
        
        /// <summary>
        /// Attack pattern: Same as movement - can attack any surrounding square
        /// When attacking, the Villager moves to that square (like capturing in chess)
        /// </summary>
        public override List<Vector2Int> GetAttackPattern()
        {
            // Attack pattern is the same as movement pattern
            return GetMovePattern();
        }
        
        /// <summary>
        /// Special rule: Villager ALWAYS costs mana to move
        /// Override CanMove to reflect this unique behavior
        /// </summary>
        public override bool CanMove()
        {
            // Unlike other minions, Villager doesn't get a free move
            // Moving the Villager will cost 1 mana
            // Dashing (moving twice) costs 2 mana
            return !JustSpawned && !HasAttackedThisTurn;
        }
        
        /// <summary>
        /// Get description for UI tooltips
        /// </summary>
        public override string GetDescription()
        {
            return "Villager (King)\n" +
                   "Cost: 0 (placed at start)\n" +
                   "Move: 8 surrounding squares\n" +
                   "Attack: Moves to attacked square\n" +
                   "Special: Always costs 1 mana to move\n" +
                   "Warning: If destroyed, you lose!";
        }
    }
}