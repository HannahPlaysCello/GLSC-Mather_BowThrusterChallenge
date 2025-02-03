using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BowThrust_MonoGame
{
    public class ShipWThrusters
    {
        private Texture2D _boatTexture;
        private Vector2 _position;
        private Vector2 _origin;

        //for boundary manager
        private int _screenWidth;
        private int _screenHeight;

        //sprite sheet
        private Rectangle _sourceRectangle; 
        private int _frameWidth = 160; 
        private int _frameHeight = 160; 
        private int _currentFrame = 0; 
        private double _frameTime = 0.15f; 
        private double _elapsedTime = 0;
        private int _numFramesPerRow = 2; 
        private int _numRows = 2; 

        
        // motion vars for boat
        private float _rotation;
        private float _currentSpeed = 0f;
        private float _currentTurnSpeed = 0f;

        private const float _maxSpeed = 100f;
        private const float _accelerationRate = 100f;
        private const float _decelerationRate = 30f;

        private const float _maxTurnSpeed = 1f; 
        private const float _turnAccelerationRate = 2f; 
        private const float _turnDecelerationRate = 0.7f;

        // THRUSTER STUFF
        private float _currentThrusterSpeed = 0f;
        private const float _maxThrusterSpeed = 100f;
        private const float _thrusterAcceleration = 100f;
        private const float _thrusterDeceleration = 80f;

        // Toggle movement variables
        private bool _isMovingForward = false;
        private KeyboardState _previousKeyboardState;

        public Vector2 Position { get => _position; set => _position = value; }
        
        public ShipWThrusters(Vector2 initialPosition, int screenWidth, int screenHeight)
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
            _origin = new Vector2(_boatTexture.Width / 100, _boatTexture.Height / 4);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Toggle forward motion
            if (keyboardState.IsKeyDown(_controlKeyMap["Go"]) && _previousKeyboardState.IsKeyUp(_controlKeyMap["Go"]))
            {
                _isMovingForward = !_isMovingForward;
            }

            // Forward acceleration/deceleration
            if (_isMovingForward)
            {
                _currentSpeed = Math.Min(_currentSpeed + _accelerationRate * deltaTime, _maxSpeed);
            }
            else
            {
                _currentSpeed = Math.Max(_currentSpeed - _decelerationRate * deltaTime, 0);
            }


            // Update forward position
            Vector2 newPosition = _position;
            if (_currentSpeed > 0)
            {
                float deltaX = (float)Math.Cos(_rotation) * _currentSpeed * deltaTime;
                float deltaY = (float)Math.Sin(_rotation) * _currentSpeed * deltaTime;

                newPosition.X += deltaX;
                newPosition.Y += deltaY;
            }

            //thrusters
            if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterLeft"]))
            {
                _currentThrusterSpeed = Math.Max(_currentThrusterSpeed - _thrusterAcceleration * deltaTime, -_maxThrusterSpeed);
            }
            else if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterRight"]))
            {
                _currentThrusterSpeed = Math.Min(_currentThrusterSpeed + _thrusterAcceleration * deltaTime, _maxThrusterSpeed);
            }
            else
            {
                //decelerate
                if (_currentThrusterSpeed > 0)
                {
                    _currentThrusterSpeed = Math.Max(_currentThrusterSpeed - _thrusterDeceleration * deltaTime, 0);
                }
                else if (_currentThrusterSpeed < 0)
                {
                    _currentThrusterSpeed = Math.Min(_currentThrusterSpeed + _thrusterDeceleration * deltaTime, 0);
                }
            }

            //move perpendicular
            if (_currentThrusterSpeed != 0)
            {
                float sideDeltaX = (float)Math.Cos(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;
                float sideDeltaY = (float)Math.Sin(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;

                newPosition.X += sideDeltaX;
                newPosition.Y += sideDeltaY;
            }

            //turning accel
            if (keyboardState.IsKeyDown(_controlKeyMap["RudderLeft"]))
            {
                _currentTurnSpeed = Math.Max(_currentTurnSpeed - _turnAccelerationRate * deltaTime, -_maxTurnSpeed);
            }
            else if (keyboardState.IsKeyDown(_controlKeyMap["RudderRight"]))
            {
                _currentTurnSpeed = Math.Min(_currentTurnSpeed + _turnAccelerationRate * deltaTime, _maxTurnSpeed);
            }
            else
            {
                if (_currentTurnSpeed > 0)
                {
                    _currentTurnSpeed = Math.Max(_currentTurnSpeed - _turnDecelerationRate * deltaTime, 0);
                }
                else if (_currentTurnSpeed < 0)
                {
                    _currentTurnSpeed = Math.Min(_currentTurnSpeed + _turnDecelerationRate * deltaTime, 0);
                }
            }

            float newRotation = _rotation + _currentTurnSpeed * deltaTime;


            //sprite front
            float frontOffsetX = (float)Math.Cos(_rotation) * (_frameWidth * 1.0f);
            float frontOffsetY = (float)Math.Sin(_rotation) * (_frameHeight * 1.0f);

            //sprite sides
            float sideOffsetX = (float)Math.Sin(_rotation) * (_frameWidth * 0.6f);
            float sideOffsetY = (float)Math.Cos(_rotation) * (_frameHeight * 0.6f);

            //front
            Vector2 frontPosition = new Vector2(newPosition.X + frontOffsetX, newPosition.Y + frontOffsetY);
            
            //sides
            Vector2 sideLeftPosition = new Vector2(newPosition.X + frontOffsetX - sideOffsetX, newPosition.Y + frontOffsetY - sideOffsetY);
            Vector2 sideRightPosition = new Vector2(newPosition.X + frontOffsetX + sideOffsetX, newPosition.Y + frontOffsetY + sideOffsetY);

            //tile coords
            int tileFrontX = (int)(frontPosition.X / tileMap.TileSize);
            int tileFrontY = (int)(frontPosition.Y / tileMap.TileSize);

            int tileSideLeftX = (int)(sideLeftPosition.X / tileMap.TileSize);
            int tileSideLeftY = (int)(sideLeftPosition.Y / tileMap.TileSize);

            int tileSideRightX = (int)(sideRightPosition.X / tileMap.TileSize);
            int tileSideRightY = (int)(sideRightPosition.Y / tileMap.TileSize);

            //check bounds ALL DIMENSIONS
            if (tileFrontX >= 0 && tileFrontX < tileMap.Width && tileFrontY >= 0 && tileFrontY < tileMap.Height &&
                tileSideLeftX >= 0 && tileSideLeftX < tileMap.Width && tileSideLeftY >= 0 && tileSideLeftY < tileMap.Height &&
                tileSideRightX >= 0 && tileSideRightX < tileMap.Width && tileSideRightY >= 0 && tileSideRightY < tileMap.Height)
            {
                //check if tile is passible for ALL CORNERS
                if (tileMap.Map[tileFrontY, tileFrontX] != 1 && 
                    tileMap.Map[tileSideLeftY, tileSideLeftX] != 1 && 
                    tileMap.Map[tileSideRightY, tileSideRightX] != 1) //only allow movement if tile is passable
                {
                    _position = newPosition; 
                    _rotation = newRotation;
                }
            }

            // Boundary conditions
            float spriteHalfWidth = _frameWidth * 0.5f;
            float spriteHalfHeight = _frameHeight * 0.5f;
            _position = BoundaryManager.ClampToBounds(_position, _screenWidth, _screenHeight, spriteHalfWidth, spriteHalfHeight);

            // Animation
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame++;

                if (_currentFrame >= _numFramesPerRow * _numRows) 
                    _currentFrame = 0;
                
                int row = _currentFrame / _numFramesPerRow;
                int col = _currentFrame % _numFramesPerRow;

                _sourceRectangle.X = col * _frameWidth;
                _sourceRectangle.Y = row * _frameHeight;
            }

            _previousKeyboardState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float scale = 1f;
            spriteBatch.Draw(_boatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
        }
    }
}
