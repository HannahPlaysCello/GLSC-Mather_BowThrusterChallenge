using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BowThrust_MonoGame
{
    public class Ship
    {

        private Texture2D _boatTexture;
        private Vector2 _position;
        private Rectangle _sourceRectangle; // Defines the current frame of the sprite sheet
        private int _frameWidth = 1024; // Width of each frame in the sprite sheet
        private int _frameHeight = 1024; // Height of each frame in the sprite sheet
        private int _currentFrame = 0; // Index of the current frame
        private double _frameTime = 0.2f; // Time between each frame change (in seconds)
        private double _elapsedTime = 0; // Time accumulator to track when to change frame
        private float _speed = 50f; // Speed at which the ship moves

        

        private int _numFramesPerRow = 2;
        private int _numRows = 2; //sprite sheet is 2x2

        public Vector2 Position { get => _position; set => _position = value; }
        
        public Ship(Vector2 initialPosition)
        {
            _position = initialPosition;
        }

        public void LoadContent(Texture2D boatTexture)
        {
            _boatTexture = boatTexture;
            _sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight); // Set to the first frame
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // Handle ship movement
            if (keyboardState.IsKeyDown(Keys.W)) // Move up
                _position.Y -= _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.S)) // Move down
                _position.Y += _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.A)) // Move left
                _position.X -= _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.D)) // Move right
                _position.X += _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle animation frame update
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame++;

                if (_currentFrame >= _numFramesPerRow * _numRows) //this is 4
                    _currentFrame = 0;
                
                // Update the source rectangle for the current frame
                int row = _currentFrame / _numFramesPerRow; // Find which row (0 or 1)
                int col = _currentFrame % _numFramesPerRow; // Find which column (0 or 1)

                _sourceRectangle.X = col * _frameWidth; // Update X based on column
                _sourceRectangle.Y = row * _frameHeight; // Update Y based on row
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //make the sprite the size i want
            float scale = 0.1f;

            //sraw the damn boat
            spriteBatch.Draw(_boatTexture, _position, _sourceRectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
