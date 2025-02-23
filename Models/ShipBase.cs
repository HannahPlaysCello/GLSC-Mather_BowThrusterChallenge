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
        //temporary debug for hitbox
        protected Texture2D _pixelTexture;

        //permanent starts here
        protected Texture2D BoatTexture;
        protected Vector2 _position;  //ship position
        protected Vector2 _origin;    //what boat rotates around :)

        //score manager
        protected ScoreManager _scoreManager;

        //hitbox!
        protected Vector2[] _hitboxCorners = new Vector2[4]; //:)))))))))))))

        //this needs work
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

        //for score tracking
        private bool _hasStartedMoving = false; //literally just to prevent the score from loading wrong
        private bool _wasPreviouslyColliding = false; //to prevent score from continually incrementing if boat stays still

        //where the boat appears on the screen
        public Vector2 Position { get => _position; set => _position = value; }

        //Constructor
        public ShipBase(Vector2 initialPosition, int screenWidth, int screenHeight, ScoreManager scoreManager)
        {
            _position = initialPosition;
            _rotation = 0f;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _origin = new Vector2(0, _frameWidth / 2);
            _scoreManager = scoreManager;

            UpdateHitbox(); //calculated hitbox upon load so no garbage values decide to appear
        }

        private void UpdateHitbox()
        {
            float Width = _frameWidth;
            float halfHeight = _frameHeight / 5;

            Vector2 topLeft = new Vector2(0, -halfHeight);
            Vector2 topRight = new Vector2(Width, -halfHeight);
            Vector2 bottomLeft = new Vector2(0, halfHeight);
            Vector2 bottomRight = new Vector2(Width, halfHeight);

            Matrix rotationMatrix = Matrix.CreateRotationZ(_rotation);
            _hitboxCorners[0] = Vector2.Transform(topLeft, rotationMatrix) + _position;
            _hitboxCorners[1] = Vector2.Transform(topRight, rotationMatrix) + _position;
            _hitboxCorners[2] = Vector2.Transform(bottomLeft, rotationMatrix) + _position;
            _hitboxCorners[3] = Vector2.Transform(bottomRight, rotationMatrix) + _position;
        }

        //boat texture/sprite sheet setup
        public void LoadContent(Texture2D boatTexture, GraphicsDevice graphicsDevice)
        {
            BoatTexture = boatTexture;
            _sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight);
            
            //temporary rectangle for debug
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        //move the ship based on helper classes
        public virtual void Update(GameTime gameTime, KeyboardState keyboardState, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //toggle forward movement
            if (keyboardState.IsKeyDown(_controlKeyMap["Go"]) && _previousKeyboardState.IsKeyUp(_controlKeyMap["Go"]))
            {
                _isMovingForward = !_isMovingForward;
                _hasStartedMoving = true;
            }

            HandleForwardMovement(deltaTime);
            HandleTurning(keyboardState, deltaTime, _controlKeyMap);

            // calculate new position but stop if collision
            Vector2 newPosition = CalculateNewPosition(deltaTime);
            if (!IsCollisionAtPosition(newPosition, tileMap))
            {
                _position = newPosition;
            }
            else
            {
                _currentSpeed = 0; //stof if collision!
            }

            //keep ship within screen boundaries!
            _position = BoundaryManager.ClampToBounds(_position, _screenWidth, _screenHeight, _frameWidth, _frameHeight);

            //update hitbox
            Vector2 hitboxRotate = _position; // Adjust based on sprite shape
            float Width = _frameWidth;
            float halfHeight = _frameHeight / 5;

            // Define the four corners BEFORE rotation
            Vector2 topLeft = new Vector2(0, -halfHeight);
            Vector2 topRight = new Vector2(Width, -halfHeight);
            Vector2 bottomLeft = new Vector2(0, halfHeight);
            Vector2 bottomRight = new Vector2(Width, halfHeight);

            // Rotate each point around the hitbox axis
            Matrix rotationMatrix = Matrix.CreateRotationZ(_rotation);
            _hitboxCorners[0] = Vector2.Transform(topLeft, rotationMatrix) + hitboxRotate;
            _hitboxCorners[1] = Vector2.Transform(topRight, rotationMatrix) + hitboxRotate;
            _hitboxCorners[2] = Vector2.Transform(bottomLeft, rotationMatrix) + hitboxRotate;
            _hitboxCorners[3] = Vector2.Transform(bottomRight, rotationMatrix) + hitboxRotate;

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
        protected bool IsCollisionAtPosition(Vector2 testPosition, TileMap tileMap)
        {
            if (!_hasStartedMoving) //ignore collisions before the boat starts moving
                return false;

            bool isColliding = false;


            
            foreach (Vector2 corner in _hitboxCorners)
            {
                if (tileMap.IsCollisionTile(corner))
                {
                    isColliding = true;
                    break;
                }
            }

            if (isColliding && !_wasPreviouslyColliding)
            {
                _scoreManager?.AddCollisionPoints();
            }

            _wasPreviouslyColliding = isColliding;
            return isColliding;
        }

        //send to end screen if end tile hit
        public bool IsEndTileAtPosition(Vector2 position, TileMap tileMap)
        {
            //check all four corners of the ship’s hitbox (you dummy)
            Vector2[] corners = new Vector2[]
            {
                _hitboxCorners[0], //top left
                _hitboxCorners[1], //top right
                _hitboxCorners[2], //bottom left
                _hitboxCorners[3]  //bottom right
            };

            foreach (Vector2 corner in corners)
            {
                int tileX = (int)(corner.X / tileMap.TileSize);
                int tileY = (int)(corner.Y / tileMap.TileSize);

                if (tileX < 0 || tileX >= tileMap.Width || tileY < 0 || tileY >= tileMap.Height)
                    continue; //skip out-of-bounds corners

                int tileID = tileMap.Map[tileY, tileX];
                bool isEnd = tileMap.GetTile(tileID).IsEndTile;

                if (isEnd)
                    return true; //if one corner is on the end tile
            }

            return false;
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

        //temporary for debugging the hitbox! 
        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);


            spriteBatch.Draw(_pixelTexture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        //draw sprite :)
        public void Draw(SpriteBatch spriteBatch)
        {
            float scale = 1f;
            //crashes here
            spriteBatch.Draw(BoatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
        
            //temporary draw hitbox for debug
            for (int i = 0; i < 4; i++)
            {
                Vector2 start = _hitboxCorners[i];
                Vector2 end = _hitboxCorners[(i + 1) % 4]; // Connect to the next point

                DrawLine(spriteBatch, start, end, Color.Red);
            }
        }
    }
}
