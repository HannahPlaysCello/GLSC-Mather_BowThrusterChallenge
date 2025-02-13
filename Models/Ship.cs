using Microsoft.Xna.Framework;

namespace BowThrust_MonoGame
{
    //standard ship with no thrusters
    public class Ship : ShipBase
    {
        public Ship(Vector2 initialPosition, int screenWidth, int screenHeight, ScoreManager scoreManager)
            : base(initialPosition, screenWidth, screenHeight, scoreManager) { }
    }
}