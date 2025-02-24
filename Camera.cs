using Microsoft.Xna.Framework;
using System;

public class Camera
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }

    public Camera(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
    }

    public void Follow(Vector2 targetPosition, TileMap tileMap)
    {
        float maxX = Math.Max(0, tileMap.WorldWidth - ScreenWidth);
        float maxY = Math.Max(0, tileMap.WorldHeight - ScreenHeight);

        Position = new Vector2(
            MathHelper.Clamp(targetPosition.X - ScreenWidth / 2, 0, maxX),
            MathHelper.Clamp(targetPosition.Y - ScreenHeight / 2, 0, maxY)
        );
    }
}