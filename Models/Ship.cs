using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BowThrust_MonoGame
{
    public class Ship
    {

        private Texture2D _boatTexture;
        private Vector2 _position;
        private Vector2 _origin;

        //sprite sheet
        private Rectangle _sourceRectangle; // Defines the current frame of the sprite sheet
        private int _frameWidth = 160; // Width of each frame in the sprite sheet
        private int _frameHeight = 160; // Height of each frame in the sprite sheet
        private int _currentFrame = 0; // Index of the current frame
        private double _frameTime = 0.2f; // Time between each frame change (in seconds)
        private double _elapsedTime = 0; // Time accumulator to track when to change frame
                
        private int _numFramesPerRow = 2;
        private int _numRows = 2; //sprite sheet is 2x2

        
        // motion var for boat
        private float _rotation; //radians
        private float _speed = 50f;
        private float _turnSpeed = 2f; //rotational speed

        
        public Vector2 Position { get => _position; set => _position = value; }
        
        public Ship(Vector2 initialPosition)
        {
            _position = initialPosition;
            _rotation = 0f; //initial angle
        }


        public void LoadContent(Texture2D boatTexture)
        {
            _boatTexture = boatTexture;
            _sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight); // Set to the first frame
            _origin = new Vector2(_boatTexture.Width /2, _boatTexture.Height / 2); 
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // Handle ship movement
            if (keyboardState.IsKeyDown(Keys.A)) //turn left
                _rotation -= _turnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.D)) // Move right
                _rotation += _turnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            
            // forward movement
            if (keyboardState.IsKeyDown(Keys.W)) // forward
            {
                float deltaX = (float)Math.Cos(_rotation) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float deltaY = (float)Math.Sin(_rotation) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
                _position.X += deltaX;
                _position.Y -= deltaY;
            }
            
               
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
            float scale = 1f;

            //sraw the damn boat
            spriteBatch.Draw(_boatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
        
        
            // Debug: Draw a line indicating the forward direction
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Red });

            Vector2 lineEnd = _position + new Vector2((float)Math.Cos(_rotation), -(float)Math.Sin(_rotation)) * 50f; // Extend line forward
            spriteBatch.Draw(pixel, new Rectangle((int)_position.X, (int)_position.Y, (int)(lineEnd.X - _position.X), 2), null, Color.Red, _rotation, Vector2.Zero, SpriteEffects.None, 0);
        
        
        }
    }
}
