using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skeleton.cs - Ranged diagonal attacker
/// 
/// WHY: This shows a more complex attack pattern than basic minions
/// 
/// The Skeleton demonstrates:
/// - Different movement vs attack patterns
/// - Ranged attacks (up to 3 squares)
/// - Diagonal-only attacks
/// - Lateral-only movement
/// 
/// This creates interesting tactical choices:
/// - Good for long-range pressure
/// - But limited lateral movement makes it vulnerable
/// - Can't attack what it's standing next to
/// </summary>

namespace Chegg
{
    public class Skeleton : Minion
    {
        public Skeleton()
        {
            Type = MinionType.Skeleton;
            ManaCost = 3;
        }
        
        /// <summary>
        /// Movement: Lateral only (4 directions)
        /// WHY: This balances its powerful ranged attack
        /// Can only move forward/back/left/right
        /// </summary>
        public override List<Vector2Int> GetMovePattern()
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
        /// Attack: Diagonal only, up to 3 squares away
        /// WHY: Creates interesting positioning puzzles
        /// - Long range is powerful
        /// - But diagonal-only is a significant limitation
        /// - Can't hit adjacent enemies!
        /// </summary>
        public override List<Vector2Int> GetAttackPattern()
        {
            List<Vector2Int> attacks = new List<Vector2Int>();
            
            // Add all diagonal positions from 1-3 squares away
            for (int distance = 1; distance <= 3; distance++)
            {
                attacks.Add(new Vector2Int(distance, distance));    // Up-Right
                attacks.Add(new Vector2Int(distance, -distance));   // Down-Right
                attacks.Add(new Vector2Int(-distance, distance));   // Up-Left
                attacks.Add(new Vector2Int(-distance, -distance));  // Down-Left
            }
            
            return attacks;
        }
        
        public override string GetDescription()
        {
            return "Skeleton (Archer)\n" +
                   "Cost: 3 mana\n" +
                   "Move: Lateral only (4 directions)\n" +
                   "Attack: Diagonal, range 1-3\n" +
                   "Strategy: Keep distance, use diagonals";
        }
    }
}