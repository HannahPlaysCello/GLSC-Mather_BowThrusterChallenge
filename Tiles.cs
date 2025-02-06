using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace BowThrust_MonoGame
{
    public class TileMap
    {
        private Dictionary<int, Tile> _tileDefinitions = new Dictionary<int, Tile>();

        public TileMap(ContentManager content)
        {
            // Load the JSON file containing tile definitions
            string json = File.ReadAllText("Content/Tiles.json");
            List<TileData> tileDataList = JsonConvert.DeserializeObject<List<TileData>>(json);

            // Load each texture and create tile objects
            foreach (var tileData in tileDataList)
            {
                Texture2D texture = content.Load<Texture2D>(tileData.Texture);
                Tile tile = new Tile(tileData.TileID, tileData.IsPassable, texture, tileData.Description);
                _tileDefinitions[tileData.TileID] = tile;
            }
        }

        public Tile GetTile(int tileID)
        {
            return _tileDefinitions[tileID];
        }
    }

    public class Tile
    {
        public int TileID { get; set; }
        public bool IsPassable { get; set; }
        public Texture2D TileTexture { get; set; }
        public string Description { get; set; }

        public Tile(int tileID, bool isPassable, Texture2D tileTexture, string description)
        {
            TileID = tileID;
            IsPassable = isPassable;
            TileTexture = tileTexture;
            Description = description;
        }
    }

    public class TileData
    {
        public int TileID { get; set; }
        public bool IsPassable { get; set; }
        public string Texture { get; set; }
        public string Description { get; set; }
    }
}
