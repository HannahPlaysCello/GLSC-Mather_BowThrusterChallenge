using Microsoft.Xna.Framework.Graphics;

namespace BowThrust_MonoGame
{
    public class Tiles
    {
        public int TileID { get; set; }
        public bool IsPassable { get; set; }
        public Texture2D TileTexture { get; set; }
        public string Description { get; set; }

        public Tiles(int tileID, bool isPassable, Texture2D tileTexture, string description)
        {
            TileID = tileID;
            IsPassable = isPassable;
            TileTexture = tileTexture;
            Description = description;
        }
    }
}
