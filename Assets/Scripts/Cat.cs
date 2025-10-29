using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cat.cs - Mana generation support unit
/// 
/// WHY: This demonstrates passive abilities and game-changing effects
/// 
/// The Cat shows:
/// - Non-combat units (can't move or attack)
/// - Permanent buffs (increases mana pool)
/// - Risk/reward (expensive but powerful if protected)
/// - OnSpawn and OnDeath effects
/// 
/// STRATEGY:
/// - High cost (5 mana) makes it a late-game play
/// - Each Cat adds +1 max mana per turn (stacks!)
/// - Must be protected or you lose the mana boost
/// - Can't defend itself, needs other minions
/// </summary>

namespace Chegg
{
    public class Cat : Minion
    {
        public Cat()
        {
            Type = MinionType.Cat;
            ManaCost = 5;
        }
        
        /// <summary>
        /// Movement: CANNOT MOVE
        /// Cats are stationary support units
        /// </summary>
        public override List<Vector2Int> GetMovePattern()
        {
            return new List<Vector2Int>(); // Empty list = can't move
        }
        
        /// <summary>
        /// Attack: CANNOT ATTACK
        /// Cats are purely defensive/support
        /// </summary>
        public override List<Vector2Int> GetAttackPattern()
        {
            return new List<Vector2Int>(); // Empty list = can't attack
        }
        
        /// <summary>
        /// Override movement check - cats can NEVER move
        /// Even with special abilities
        /// </summary>
        public override bool CanMove()
        {
            return false; // Always returns false
        }
        
        /// <summary>
        /// Override attack check - cats can NEVER attack
        /// </summary>
        public override bool CanAttack()
        {
            return false; // Always returns false
        }
        
        /// <summary>
        /// OnSpawn: Grant the owner +1 max mana per turn
        /// WHY: This creates long-term value
        /// - Costs 5 mana now
        /// - But generates +1 mana every turn after
        /// - Pays for itself after 5 turns
        /// - Each additional turn is pure profit
        /// </summary>
        public override void OnSpawn(GameManager gameManager)
        {
            Player owner = gameManager.GetPlayer(Owner);
            owner.BonusMana++;
            
            Debug.Log($"{owner.PlayerName} gained +1 bonus mana from Cat! " +
                     $"(Total bonus: {owner.BonusMana})");
        }
        
        /// <summary>
        /// OnDeath: Remove the mana bonus
        /// WHY: Losing the Cat hurts your economy
        /// - Protecting Cats becomes crucial
        /// - Opponent wants to kill them ASAP
        /// - Creates interesting targeting priorities
        /// </summary>
        public override void OnDeath(GameManager gameManager)
        {
            Player owner = gameManager.GetPlayer(Owner);
            owner.BonusMana = Mathf.Max(0, owner.BonusMana - 1);
            
            Debug.Log($"{owner.PlayerName} lost bonus mana from Cat death! " +
                     $"(Remaining bonus: {owner.BonusMana})");
        }
        
        public override string GetDescription()
        {
            return "Cat (Economy)\n" +
                   "Cost: 5 mana\n" +
                   "Move: Cannot move\n" +
                   "Attack: Cannot attack\n" +
                   "Effect: +1 max mana per turn\n" +
                   "Strategy: Place safely, protect at all costs!\n" +
                   "STACKS with multiple Cats!";
        }
    }
}