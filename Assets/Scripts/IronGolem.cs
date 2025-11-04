using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IronGolem.cs - Area-of-effect attacker
/// 
/// WHY: This demonstrates multi-target attacks
/// 
/// The Iron Golem shows:
/// - Sweeping attacks (hits 3 tiles at once)
/// - Normal movement
/// - Medium cost with good board control
/// 
/// IMPLEMENTATION NOTE:
/// For now, we return all possible attack positions.
/// In a more advanced version, you'd let the player CHOOSE
/// which direction to sweep (North, South, East, or West)
/// </summary>

namespace Chegg
{
    public class IronGolem : Minion
    {
        public IronGolem()
        {
            Type = MinionType.IronGolem;
            ManaCost = 2;
        }
        
        /// <summary>
        /// Movement: Standard 8 surrounding squares
        /// </summary>
        public override List<Vector2Int> GetMovePattern()
        {
            return new List<Vector2Int>
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
                new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
            };
        }
        
        /// <summary>
        /// Attack: Sweeping attack in 3 adjacent tiles
        /// 
        /// Pattern explanation:
        /// If attacking NORTH, hits these 3 tiles:
        ///   [X][X][X]
        ///      [G]
        /// 
        /// Can sweep in any lateral direction (N, S, E, W)
        /// </summary>
        public override List<Vector2Int> GetAttackPattern()
        {
            List<Vector2Int> attacks = new List<Vector2Int>();
            
            // North sweep (top 3 tiles)
            attacks.Add(new Vector2Int(-1, 1));
            attacks.Add(new Vector2Int(0, 1));
            attacks.Add(new Vector2Int(1, 1));
            
            // South sweep (bottom 3 tiles)
            attacks.Add(new Vector2Int(-1, -1));
            attacks.Add(new Vector2Int(0, -1));
            attacks.Add(new Vector2Int(1, -1));
            
            // West sweep (left 3 tiles)
            attacks.Add(new Vector2Int(-1, -1));
            attacks.Add(new Vector2Int(-1, 0));
            attacks.Add(new Vector2Int(-1, 1));
            
            // East sweep (right 3 tiles)
            attacks.Add(new Vector2Int(1, -1));
            attacks.Add(new Vector2Int(1, 0));
            attacks.Add(new Vector2Int(1, 1));
            
            return attacks;
        }
        
        public override string GetDescription()
        {
            return "Iron Golem (Tank)\n" +
                   "Cost: 2 mana\n" +
                   "Move: 8 surrounding squares\n" +
                   "Attack: Sweeping (3 adjacent tiles)\n" +
                   "Good for: Controlling space, clearing groups";
        }
    }
}