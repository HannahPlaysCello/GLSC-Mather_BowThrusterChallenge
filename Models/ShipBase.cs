using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BowThrust_MonoGame
{
    //base class for all ships
    public abstract class ShipBase
    {
        protected Texture2D _boatTexture;
        protected Vector2 _position;  //ship position
        protected Vector2 _origin;    //for rotation (gotta seperate this into two things, one for rotation, one for position on the screen)

        //for boundary manager
        protected int _screenWidth;
        protected int _screenHeight;

        //Sprite sheet
        protected Rectangle _sourceRectangle; 
        protected int _frameWidth = 160; 
        protected int _frameHeight = 160; 
        protected int _currentFrame = 0;
        protected double _frameTime = 0.15f;  //time between frames, change this to change animation speed
        protected double _elapsedTime = 0;
        protected int _numFramesPerRow = 2;
        protected int _numRows = 2; 

        //motion vars for the ship
        protected float _rotation;  //in radians
        protected float _currentSpeed = 0f;
        protected float _currentTurnSpeed = 0f;

        //accel/decel
        protected const float _maxSpeed = 100f;
        protected const float _accelerationRate = 100f;
        protected const float _decelerationRate = 30f;

        protected const float _maxTurnSpeed = 1f;
        protected const float _turnAccelerationRate = 2f;
        protected const float _turnDecelerationRate = 0.7f;

        // Toggle movement state
        protected bool _isMovingForward = false;
        protected KeyboardState _previousKeyboardState;

        public Vector2 Position { get => _position; set => _position = value; }

        //Constructor
        public ShipBase(Vector2 initialPosition, int screenWidth, int screenHeight)
        {
            _position = initialPosition;
            _rotation = 0f;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        //loat boat texture/sprite sheet setup
        public void LoadContent(Texture2D boatTexture)
        {
            _boatTexture = boatTexture;
            _sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight);
            _origin = new Vector2(_boatTexture.Width / 100, _boatTexture.Height / 4);
        }

        //movement and animation
        public virtual void Update(GameTime gameTime, KeyboardState keyboardState, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //toggle forward movement
            if (keyboardState.IsKeyDown(_controlKeyMap["Go"]) && _previousKeyboardState.IsKeyUp(_controlKeyMap["Go"]))
                _isMovingForward = !_isMovingForward;

            HandleForwardMovement(deltaTime);
            HandleTurning(keyboardState, deltaTime, _controlKeyMap);

            // calculate new position and check for collisions
            Vector2 newPosition = CalculateNewPosition(deltaTime);
            HandleCollisions(newPosition, tileMap);

            //keep ship within screen boundaries!
            _position = BoundaryManager.ClampToBounds(_position, _screenWidth, _screenHeight, _frameWidth * 0.5f, _frameHeight * 0.5f);

            //update animation frames
            UpdateAnimation(gameTime);

            //have to store previous key state for toggling
            _previousKeyboardState = keyboardState;
        }

        //forward acceleration and deceleration
        protected void HandleForwardMovement(float deltaTime)
        {
            if (_isMovingForward)
                _currentSpeed = Math.Min(_currentSpeed + _accelerationRate * deltaTime, _maxSpeed);
            else
                _currentSpeed = Math.Max(_currentSpeed - _decelerationRate * deltaTime, 0);
        }

        //ship turning logic
        protected void HandleTurning(KeyboardState keyboardState, float deltaTime, Dictionary<string, Keys> _controlKeyMap)
        {
            if (keyboardState.IsKeyDown(_controlKeyMap["RudderLeft"]))
                _currentTurnSpeed = Math.Max(_currentTurnSpeed - _turnAccelerationRate * deltaTime, -_maxTurnSpeed);
            else if (keyboardState.IsKeyDown(_controlKeyMap["RudderRight"]))
                _currentTurnSpeed = Math.Min(_currentTurnSpeed + _turnAccelerationRate * deltaTime, _maxTurnSpeed);
            else
            {
                //decelerat if no turn key is pressed
                if (_currentTurnSpeed > 0)
                    _currentTurnSpeed = Math.Max(_currentTurnSpeed - _turnDecelerationRate * deltaTime, 0);
                else if (_currentTurnSpeed < 0)
                    _currentTurnSpeed = Math.Min(_currentTurnSpeed + _turnDecelerationRate * deltaTime, 0);
            }

            //apply turn speed to rotation
            _rotation += _currentTurnSpeed * deltaTime;
        }

        //calculate ship's next position
        protected Vector2 CalculateNewPosition(float deltaTime)
        {
            float deltaX = (float)Math.Cos(_rotation) * _currentSpeed * deltaTime;
            float deltaY = (float)Math.Sin(_rotation) * _currentSpeed * deltaTime;
            return new Vector2(_position.X + deltaX, _position.Y + deltaY);
        }

        //stop if collided
        protected void HandleCollisions(Vector2 newPosition, TileMap tileMap)
        {
            if (!tileMap.IsCollisionTile(newPosition))
                _position = newPosition;
            else
                _currentSpeed = 0;
        }

        //sprite sheet animation
        protected void UpdateAnimation(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame = (_currentFrame + 1) % (_numFramesPerRow * _numRows);
                int row = _currentFrame / _numFramesPerRow;
                int col = _currentFrame % _numFramesPerRow;
                _sourceRectangle = new Rectangle(col * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            }
        }

        //draw sprite :)
        public void Draw(SpriteBatch spriteBatch)
        {
            float scale = 1f;
            spriteBatch.Draw(_boatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
        }
    }
}
