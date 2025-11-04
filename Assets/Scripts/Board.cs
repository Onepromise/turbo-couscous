using System.Collections.Generic;
using UnityEngine;

namespace Chegg
{
    public class Board : MonoBehaviour
    {
        // Board dimensions - 16x16 grid
        public const int WIDTH = 16;
        public const int HEIGHT = 16;
        
        // 2D array storing all tiles
        private Tile[,] tiles = new Tile[WIDTH, HEIGHT];
        
        // Initialize the board - called once at game start
        public void Initialize()
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    // Create new tile at this position
                    tiles[x, y] = new Tile(new Vector2Int(x, y));
                    
                    // Create checkerboard pattern
                    tiles[x, y].Type = (x + y) % 2 == 0 ? TileType.Light : TileType.Dark;
                    
                    // Set up spawn zones (3 rows each)
                    if (y <= 2)
                    {
                        // Bottom 3 rows = Red spawn zone
                        tiles[x, y].IsSpawnZone = true;
                        tiles[x, y].SpawnZoneTeam = PlayerTeam.Red;
                    }
                    else if (y >= HEIGHT - 3)
                    {
                        // Top 3 rows = Blue spawn zone
                        tiles[x, y].IsSpawnZone = true;
                        tiles[x, y].SpawnZoneTeam = PlayerTeam.Blue;
                    }
                }
            }
            
            Debug.Log($"Board initialized: {WIDTH}x{HEIGHT} with spawn zones");
        }
        
        
        // Get a tile at a specific position
        // Returns null if position is out of bounds (safe access) 
        public Tile GetTile(Vector2Int pos)
        {
            if (IsValidPosition(pos))
                return tiles[pos.x, pos.y];
            
            return null;
        }
        
        /// Check if a position is within board boundaries
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < WIDTH && pos.y >= 0 && pos.y < HEIGHT;
        }
        
        
        // Get all valid positions where a minion can move
        // Takes into account the minion's movement pattern and obstacles
        public List<Vector2Int> GetValidMoves(Minion minion)
        {
            List<Vector2Int> validMoves = new List<Vector2Int>();
            
            // Get the minion's potential move offsets
            List<Vector2Int> movePattern = minion.GetMovePattern();
            
            // Check each potential move
            foreach (Vector2Int offset in movePattern)
            {
                Vector2Int targetPos = minion.Position + offset;
                
                // Must be within bounds
                if (!IsValidPosition(targetPos)) continue;
                
                Tile tile = GetTile(targetPos);
                
                // Must be walkable and minion must be able to move there
                if (tile.IsWalkable() && minion.CanMoveToTile(tile))
                {
                    validMoves.Add(targetPos);
                }
            }
            
            return validMoves;
        }
        
       
        /// Get all valid positions where a minion can attack
        /// Checks for enemy minions within attack range
        public List<Vector2Int> GetValidAttacks(Minion minion)
        {
            List<Vector2Int> validAttacks = new List<Vector2Int>();
            
            // Get the minion's attack pattern
            List<Vector2Int> attackPattern = minion.GetAttackPattern();
            
            // Check each potential attack position
            foreach (Vector2Int offset in attackPattern)
            {
                Vector2Int targetPos = minion.Position + offset;
                
                // Must be within bounds
                if (!IsValidPosition(targetPos)) continue;
                
                Tile tile = GetTile(targetPos);
                
                // Must have an enemy minion
                if (tile.IsOccupied && tile.OccupyingMinion.Owner != minion.Owner)
                {
                    validAttacks.Add(targetPos);
                }
            }
            
            return validAttacks;
        }
        
      
        /// Place a minion on the board at a specific position
        public bool PlaceMinion(Minion minion, Vector2Int position)
        {
            Tile tile = GetTile(position);
            if (tile == null || tile.IsOccupied)
                return false;
            
            tile.OccupyingMinion = minion;
            minion.Position = position;
            return true;
        }
        
        
        /// Move a minion from one tile to another
        public bool MoveMinion(Minion minion, Vector2Int newPosition)
        {
            Tile oldTile = GetTile(minion.Position);
            Tile newTile = GetTile(newPosition);
            
            if (oldTile == null || newTile == null || newTile.IsOccupied)
                return false;
            
            // Remove from old tile
            oldTile.OccupyingMinion = null;
            
            // Place on new tile
            newTile.OccupyingMinion = minion;
            minion.Position = newPosition;
            
            return true;
        }
        
        /// Remove a minion from the board
        public void RemoveMinion(Minion minion)
        {
            Tile tile = GetTile(minion.Position);
            if (tile != null && tile.OccupyingMinion == minion)
            {
                tile.OccupyingMinion = null;
            }
        }
        
        
        // Customize board - add water, walls, etc.
        // Call this after Initialize() to create custom boards
        public void AddCustomTerrain(Vector2Int position, TileType type)
        {
            Tile tile = GetTile(position);
            if (tile != null)
            {
                tile.Type = type;
            }
        }
        
        // Example: Create a river down the middle of the board
        public void CreateRiverBoard()
        {
            // Add water tiles down the middle
            for (int y = 0; y < HEIGHT; y++)
            {
                AddCustomTerrain(new Vector2Int(7, y), TileType.Water);
                AddCustomTerrain(new Vector2Int(8, y), TileType.Water);
            }
            
            Debug.Log("River board created!");
        }
    }
}