using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BowThrust_MonoGame
{
    public class ScoreManager
    {
        public int Score { get; private set; } = 0;
        public int Collisions { get; private set; } = 0;  // Track collisions

        private SpriteFont font;

        public ScoreManager(SpriteFont font)
        {
            this.font = font;
            //Score = 1; //temporary hack because it starts on teh boundary lol
            //Collisions = -1;
        }

        public void AddCollisionPoints(int points = -1)
        {
            Score += points;
            Collisions++;  // Increment collision counter
        }

        public void ResetScore()
        {
            Score = 0;
            Collisions = 0;  // Reset collision count
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 scorePosition, Vector2 collisionPosition)
        {
            //spriteBatch.DrawString(font, "Score: " + Score, scorePosition, Color.White); //
            spriteBatch.DrawString(font, "Collisions: " + Collisions, collisionPosition, Color.Red);
        }
    }
}


