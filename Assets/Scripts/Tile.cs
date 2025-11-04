using UnityEngine;
namespace Chegg
{
    public class Tile
    {
        public Vector2Int Position { get; set; }
        public TileType Type { get; set; }
        public Minion OccupyingMinion { get; set; }
        public bool IsSpawnZone { get; set; }
        public PlayerTeam SpawnZoneTeam { get; set; }
        public bool IsOccupied => OccupyingMinion != null;
        public Tile(Vector2Int position)
        {
            Position = position;
            Type = TileType.Light;
            OccupyingMinion = null;
            IsSpawnZone = false;
        }
        public bool IsWalkable()
        {
            return !IsOccupied && Type != TileType.Wall;
        }
    }
}