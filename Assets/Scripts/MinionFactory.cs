using UnityEngine;

/// <summary>
/// MinionFactory.cs - Creates minion instances
/// 
/// WHY: The Factory pattern centralizes object creation:
/// - One place to create all minion types
/// - Easy to add new minions (just add one case)
/// - Decouples creation from usage
/// - Makes testing easier
/// 
/// Instead of:
///   if (type == Zombie) minion = new Zombie();
///   else if (type == Skeleton) minion = new Skeleton();
///   (repeated in multiple places)
/// 
/// We have:
///   minion = MinionFactory.CreateMinion(type);
///   (used everywhere)
/// </summary>

namespace Chegg
{
    public static class MinionFactory
    {
        /// <summary>
        /// Create a new minion of the specified type
        /// Returns null if type is invalid
        /// </summary>
        public static Minion CreateMinion(MinionType type)
        {
            switch (type)
            {
                // Cost 0
                case MinionType.Villager:
                    return new Villager();

                // Cost 1
                case MinionType.Zombie:
                    return new Zombie();
                case MinionType.Creeper:
                    return new Creeper();
                case MinionType.Pig:
                    return new Pig();

                // Cost 2
                case MinionType.Rabbit:
                    return new Rabbit();
                case MinionType.PufferFish:
                    return new PufferFish();
                case MinionType.IronGolem:
                    return new IronGolem();
                case MinionType.Frog:
                    return new Frog();

                // Cost 3
                case MinionType.Skeleton:
                    return new Skeleton();
                case MinionType.Blaze:
                    return new Blaze();
                case MinionType.Phantom:
                    return new Phantom();

                // Cost 4
                case MinionType.Enderman:
                    return new Enderman();
                case MinionType.Slime:
                    return new Slime();
                case MinionType.ShulkerBox:
                    return new ShulkerBox();

                // Cost 5
                case MinionType.Parrot:
                    return new Parrot();
                case MinionType.Cat:
                    return new Cat();
                case MinionType.Sniffer:
                    return new Sniffer();

                // Cost 6
                case MinionType.Wither:
                    return new Wither();

                default:
                    Debug.LogError($"Unknown minion type: {type}");
                    return null;
            }
        }

        /// <summary>
        /// Get the mana cost of a minion type without creating it
        /// Useful for UI display
        /// </summary>
        public static int GetManaCost(MinionType type)
        {
            // Create a temporary instance to get cost
            // (Could also use a lookup table for better performance)
            Minion minion = CreateMinion(type);
            return minion?.ManaCost ?? 0;
        }

        /// <summary>
        /// Get a description of a minion type
        /// Useful for tooltips and card text
        /// </summary>
        public static string GetDescription(MinionType type)
        {
            Minion minion = CreateMinion(type);
            return minion?.GetDescription() ?? "Unknown minion";
        }
    }

    // ==================== PLACEHOLDER MINION CLASSES ====================
    // These are simple stubs - you'll need to implement the full versions

    public class Creeper : Minion
    {
        public Creeper()
        {
            Type = MinionType.Creeper;
            ManaCost = 1;
        }

        public override System.Collections.Generic.List<Vector2Int> GetMovePattern()
        {
            // 8 surrounding squares
            return new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
            };
        }

        public override System.Collections.Generic.List<Vector2Int> GetAttackPattern()
        {
            // Explodes in 8 surrounding squares (also destroys self)
            return GetMovePattern();
        }
    }

    public class Pig : Minion
    {
        public Pig()
        {
            Type = MinionType.Pig;
            ManaCost = 1;
        }

        public override System.Collections.Generic.List<Vector2Int> GetMovePattern()
        {
            return new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
            };
        }

        public override System.Collections.Generic.List<Vector2Int> GetAttackPattern()
        {
            // Cannot attack
            return new System.Collections.Generic.List<Vector2Int>();
        }

        public override bool CanAttack() { return false; }

        public override void OnSpawn(GameManager gameManager)
        {
            // Draw a card when spawned
            Player owner = gameManager.GetPlayer(Owner);
            MinionType? card = owner.Deck.Draw();
            if (card.HasValue)
            {
                owner.AddCardToHand(card.Value);
            }
        }

        public override void OnDeath(GameManager gameManager)
        {
            // Draw a card when dies
            Player owner = gameManager.GetPlayer(Owner);
            MinionType? card = owner.Deck.Draw();
            if (card.HasValue)
            {
                owner.AddCardToHand(card.Value);
            }
        }
    }

    // Stub classes for remaining minions
    // TODO: Implement full movement/attack patterns

    public class Rabbit : Minion
    {
        public Rabbit() { Type = MinionType.Rabbit; ManaCost = 2; }
        public override System.Collections.Generic.List<Vector2Int> GetMovePattern()
        {
            return new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(0, 2), new Vector2Int(0, -2),
                new Vector2Int(2, 0), new Vector2Int(-2, 0)
            };
        }
        public override System.Collections.Generic.List<Vector2Int> GetAttackPattern()
        {
            return new System.Collections.Generic.List<Vector2Int>();
        }
        public override bool CanAttack() { return false; }
    }

    public class PufferFish : Minion
    {
        public PufferFish() { Type = MinionType.PufferFish; ManaCost = 2; }
        public override System.Collections.Generic.List<Vector2Int> GetMovePattern()
        {
            return new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 0), new Vector2Int(-1, 0)
            };
        }
        public override System.Collections.Generic.List<Vector2Int> GetAttackPattern()
        {
            return new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(-1, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(1, 1)
            };
        }
    }
}
