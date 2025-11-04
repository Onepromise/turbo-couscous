using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zombie.cs - Basic infantry unit
/// 
/// WHY: This shows a different movement/attack pattern than Villager
/// 
/// The Zombie is a simple, cheap unit that:
/// - Costs only 1 mana
/// - Moves forward (direction based on team)
/// - Attacks in 4 lateral directions
/// - Good for early game pressure
/// </summary>

namespace Chegg
{
    public class Zombie : Minion
    {
        /// <summary>
        /// Constructor - set up zombie properties
        /// </summary>
        public Zombie()
        {
            Type = MinionType.Zombie;
            ManaCost = 1; // Cheap unit
        }
        
        /// <summary>
        /// Movement: Can only move forward (3 squares ahead)
        /// Direction depends on which team owns this zombie
        /// Red team moves UP (positive Y), Blue team moves DOWN (negative Y)
        /// </summary>
        public override List<Vector2Int> GetMovePattern()
        {
            // Determine forward direction based on owner
            int forwardDirection = (Owner == PlayerTeam.Red) ? 1 : -1;
            
            return new List<Vector2Int>
            {
                new Vector2Int(-1, forwardDirection),  // Forward-left
                new Vector2Int(0, forwardDirection),   // Forward
                new Vector2Int(1, forwardDirection)    // Forward-right
            };
        }
        
        /// <summary>
        /// Attack: Can attack in 4 lateral (orthogonal) directions
        /// This is DIFFERENT from movement - zombie attacks sideways but moves forward
        /// </summary>
        public override List<Vector2Int> GetAttackPattern()
        {
            return new List<Vector2Int>
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };
        }
        
        /// <summary>
        /// Get description for UI
        /// </summary>
        public override string GetDescription()
        {
            return "Zombie\n" +
                   "Cost: 1 mana\n" +
                   "Move: Forward only (3 squares)\n" +
                   "Attack: 4 lateral directions\n" +
                   "Good starter unit for early pressure";
        }
    }
}