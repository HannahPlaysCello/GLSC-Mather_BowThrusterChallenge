using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;

public class TileMap
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int[,] Map { get; private set; }
    public int TileSize { get; private set; }

    private Texture2D _waterTexture;
    private Texture2D _landTexture;

    public void LoadFromJson(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<TileMapData>(json);
        
        Map = ConvertTo2DArray(data.map);
        TileSize = data.tileSize;

        Width = Map.GetLength(1);
        Height = Map.GetLength(0); 
    }

    public void LoadContent(Texture2D waterTexture, Texture2D landTexture)
    {
        _waterTexture = waterTexture;
        _landTexture = landTexture;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                Texture2D texture = (Map[row, col] == 0) ? _waterTexture : _landTexture;
                Vector2 position = new Vector2(col * TileSize, row * TileSize);
                spriteBatch.Draw(texture, position, Color.White);
            }
        }
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
        public int tileSize { get; set; }
        public int[][] map { get; set; }
    }


    public bool IsCollisionTile(Vector2 position)
    {
        int tileX = (int)(position.X / TileSize);
        int tileY = (int)(position.Y / TileSize);

        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return true; // Treat out-of-bounds as collision
        }

        return Map[tileY, tileX] ==1; 
    }

}
