using Microsoft.Xna.Framework;

namespace BowThrust_MonoGame
{
    public static class BoundaryManager
    {
        // Ensures an object stays within the screen bounds
        public static Vector2 ClampToBounds(Vector2 position, int screenWidth, int screenHeight, float spriteHalfWidth, float spriteHalfHeight)
        {
            float clampedX = MathHelper.Clamp(position.X, spriteHalfWidth, screenWidth - spriteHalfWidth);
            float clampedY = MathHelper.Clamp(position.Y, spriteHalfHeight, screenHeight - spriteHalfHeight);
            return new Vector2(clampedX, clampedY);
        }
    }
}
