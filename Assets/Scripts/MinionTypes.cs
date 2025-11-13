using UnityEngine;

namespace CHEGG
{
    /// <summary>
    /// Defines how a minion can move on the board
    /// </summary>
    public enum MovementPattern
    {
        None,                    // Cannot move (Cat, Shulker-Box)
        Standard8,               // All 8 surrounding squares (Villager, Creeper, Iron Golem)
        ForwardOnly,             // Only forward 3 squares (Zombie)
        LateralOnly,             // Only 4 lateral directions (Skeleton)
        DiagonalOnly,            // Only 4 diagonal directions (Blaze)
        Knight,                  // L-shaped jumps (future)
        Custom                   // Custom pattern defined in data
    }
    
    /// <summary>
    /// Defines how a minion can attack
    /// </summary>
    public enum AttackPattern
    {
        None,                    // Cannot attack
        Melee8,                  // All 8 surrounding (Villager)
        MeleeLateral,            // 4 lateral adjacent (Zombie)
        MeleeDiagonal,           // 4 diagonal adjacent (Puffer-Fish)
        RangedDiagonal,          // Diagonal line, range X (Skeleton)
        RangedLateral,           // Lateral line, range X (Blaze)
        Explosion,               // Area explosion (Creeper)
        Sweep,                   // 3-tile sweep in chosen direction (Iron Golem)
        Custom                   // Custom pattern
    }
    
    /// <summary>
    /// Special minion type flags
    /// </summary>
    public enum MinionType
    {
        Normal,
        Villager,                // King piece - if dies, you lose!
        Zombie,
        Skeleton,
        Creeper,
        IronGolem,
        Cat,
        // Add more as needed
    }
}