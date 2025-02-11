using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ScoreManager
{
    public int Score { get; private set; } = 0;

    private SpriteFont font;

    public ScoreManager(SpriteFont font)
    {
        this.font = font;
    }

    public void AddCollisionPoints(int points = -1)
    {
        Score += points;
    }

    public void ResetScore()
    {
        Score = 0;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        spriteBatch.DrawString(font, "Score: " + Score, position, Color.White);
    }
}
