using Microsoft.Xna.Framework;

namespace BowThrust_MonoGame
{
    public static class BoundaryManager
    {
        // Ensures an object stays within the screen bounds
        public static Vector2 ClampToBounds(Vector2 position, int screenWidth, int screenHeight, float spriteWidth, float spriteHeight)
        {
            float clampedX = MathHelper.Clamp(position.X, 0, screenWidth - spriteWidth);
            float clampedY = MathHelper.Clamp(position.Y, 0, screenHeight);
            return new Vector2(clampedX, clampedY);
        }
    }
}
