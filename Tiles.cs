//types of tiles for tile maps

using Microsoft.Xna.Framework.Graphics;

namespace BowThrust_MonoGame
{
    public class Tiles
    {
        public int TileID { get; set; }
        public bool IsPassable { get; set; }
        public bool IsEndTile { get; set; } //for new end screen trigger tile
        public Texture2D TileTexture { get; set; }
        public string Description { get; set; }

        public Tiles(int tileID, bool isPassable, bool isEndTile, Texture2D tileTexture, string description)
        {
            TileID = tileID;
            IsPassable = isPassable;
            IsEndTile = isEndTile;
            TileTexture = tileTexture;
            Description = description;
        }
    }
}
