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

        //for boundary manager
        private int _screenWidth;
        private int _screenHeight;

        //sprite sheet
        private Rectangle _sourceRectangle; // current frame of the sprite sheet
        private int _frameWidth = 160; // Width of  frame
        private int _frameHeight = 160; // Height for frame
        private int _currentFrame = 0; // index for sprite sheet frame
        private double _frameTime = 0.2f; // time between frames (seconds) <- CHANGE THIS VALUE TO CHANGE ANIMATION RATE
        private double _elapsedTime = 0;
        private int _numFramesPerRow = 2; //sprite sheet is 2x2
        private int _numRows = 2; 

        
        // motion var for boat
        private float _rotation; //in radians
        private float _speed = 100f; //for forward motion
        private float _turnSpeed = 0.8f; //rotational speed

        public Vector2 Position { get => _position; set => _position = value; }
        
        //boat graphic
        public Ship(Vector2 initialPosition, int screenWidth, int screenHeight)
        {
            _position = initialPosition;
            _rotation = 0f;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void LoadContent(Texture2D boatTexture)
        {
            _boatTexture = boatTexture;
            _sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight); 
            _origin = new Vector2(_boatTexture.Width / 100, _boatTexture.Height / 4); //figure out the numbers . 4 seems to be half the height, but seem to need a very large number to get it to spin around it's back end on width
        }

        //boat movement
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // rudder
            if (keyboardState.IsKeyDown(Keys.A)) //turn left
                _rotation += _turnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //last term just for debug
            if (keyboardState.IsKeyDown(Keys.D)) // Move right
                _rotation -= _turnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //last term just for debug

            
            // forward movement
            if (keyboardState.IsKeyDown(Keys.W)) // forward
            {
                float deltaX = (float)Math.Cos(_rotation) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float deltaY = (float)Math.Sin(_rotation) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
                _position.X += deltaX;
                _position.Y += deltaY;
            }
            
            //boundary conditions
            float spriteHalfWidth = _frameWidth * 0.5f;
            float spriteHalfHeight = _frameHeight * 0.5f;
            _position = BoundaryManager.ClampToBounds(_position, _screenWidth, _screenHeight, spriteHalfWidth, spriteHalfHeight);


            //animation
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame++;

                if (_currentFrame >= _numFramesPerRow * _numRows) //this is 4
                    _currentFrame = 0;
                
                int row = _currentFrame / _numFramesPerRow; // find row
                int col = _currentFrame % _numFramesPerRow; // find column 

                _sourceRectangle.X = col * _frameWidth; // Update X based on column!
                _sourceRectangle.Y = row * _frameHeight; // Update Y based on row!
            }
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            //make the sprite the size i want
            float scale = 1f;

            //draw the damn boat please
            spriteBatch.Draw(_boatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
        
        
        }
    }
}
