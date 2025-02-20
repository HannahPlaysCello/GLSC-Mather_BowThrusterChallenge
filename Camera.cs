using Microsoft.Xna.Framework;

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
}