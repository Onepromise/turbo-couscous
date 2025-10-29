using UnityEngine;

namespace Chegg
{
    public enum TileType 
    { 
        Light,      // Light colored checkered squares
        Dark,       // Dark colored checkered squares
        Water,      // Water tiles (for custom boards)
        Wall        // Wall/obstacle tiles (for custom boards)
    }

    public enum MinionType 
    { 
        // Cost 0
        Villager,
        
        // Cost 1
        Zombie,
        Creeper,
        Pig,
        
        // Cost 2
        Rabbit,
        PufferFish,
        IronGolem,
        Frog,
        
        // Cost 3
        Skeleton,
        Blaze,
        Phantom,
        
        // Cost 4
        Enderman,
        Slime,
        ShulkerBox,
        
        // Cost 5
        Parrot,
        Cat,
        Sniffer,
        
        // Cost 6
        Wither 
    }

    public enum PlayerTeam 
    { 
        Red,        // Goes first, spawns on bottom (rows 0-2)
        Blue        // Goes second, spawns on top (rows 13-15)
    }
}