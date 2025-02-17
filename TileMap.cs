using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

using BowThrust_MonoGame;

public class TileMap
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int[,] Map { get; private set; }
    public int TileSize { get; private set; }

    // Stores tile definitions loaded from JSON
    private Dictionary<int, Tiles> _tileDefinitions;

    public void LoadFromJson(ContentManager content, string mapFilePath, string tileDefinitionsFile, int tileSize)
    {
        TileSize = tileSize;

        // Load map
        string mapJson = File.ReadAllText(mapFilePath);
        var mapData = JsonConvert.DeserializeObject<TileMapData>(mapJson);
        Map = ConvertTo2DArray(mapData.map);
        Width = Map.GetLength(1);
        Height = Map.GetLength(0);

        // Load tile metadata from JSON
        string tileDefinitionsJson = File.ReadAllText(tileDefinitionsFile);
        List<TileData> tileDataList = JsonConvert.DeserializeObject<List<TileData>>(tileDefinitionsJson);

        _tileDefinitions = new Dictionary<int, Tiles>();
        foreach (var tileData in tileDataList)
        {
            Texture2D texture = content.Load<Texture2D>(tileData.Texture);
            bool isEndTile = tileData.TileID == 2; //check if end tile
            _tileDefinitions[tileData.TileID] = new Tiles(tileData.TileID, tileData.IsPassable, isEndTile, texture, tileData.Description);
        }
    }

    public Tiles GetTile(int tileID)
    {
        return _tileDefinitions[tileID];
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int tileID = Map[y, x];
                Tiles tile = GetTile(tileID);
                Vector2 position = new Vector2(x * TileSize, y * TileSize);
                float scale = (float)TileSize / 64f;  //Assuming base tile size is 64 

                spriteBatch.Draw(tile.TileTexture, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }

    public bool IsCollisionTile(Vector2 position)
    {
        int tileX = (int)(position.X / TileSize);
        int tileY = (int)(position.Y / TileSize);

        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            return true;  // Treat out-of-bounds as a collision

        int tileID = Map[tileY, tileX];
        return !_tileDefinitions[tileID].IsPassable;
    }

    private int[,] ConvertTo2DArray(int[][] jaggedArray)
    {
        int rows = jaggedArray.Length;
        int cols = jaggedArray[0].Length;
        int[,] array2D = new int[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                array2D[i, j] = jaggedArray[i][j];

        return array2D;
    }

    private class TileMapData
    {
        public int[][] map { get; set; }
    }

    private class TileData
    {
        public int TileID { get; set; }
        public bool IsPassable { get; set; }
        public string Texture { get; set; }
        public string Description { get; set; }
    }
}
