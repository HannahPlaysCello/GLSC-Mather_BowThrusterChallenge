using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Linq;

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
        protected Vector2[] _hitboxCorners = new Vector2[5]; //make pentagon for pointy ship bow

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

        //boat edges
        private List<Vector2> GetEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            for (int i = 0; i < _hitboxCorners.Length; i++)
            {
                //current corner to next one
                Vector2 start = _hitboxCorners[i];
                Vector2 end = _hitboxCorners[(i + 1) % _hitboxCorners.Length];
                //edge vector by subtracting 
                Vector2 edge = end - start;
                edges.Add(edge);
            }
            return edges;
        }

        //boat normal axes
        private List<Vector2> GetProjectionAxes()
        {
            List<Vector2> axes = new List<Vector2>();
            foreach (var edge in GetEdges())
            {
                //get normal (gotta tell dr han)
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                //unit vector (gotta tell dr meyer)
                normal.Normalize();
                //store
                axes.Add(normal);
            }
            return axes;
        }

        private void ProjectOntoAxis(Vector2 axis, Vector2[] corners, out float min, out float max)
        {
            min = max = Vector2.Dot(corners[0], axis);
            for (int i = 1; i < corners.Length; i++)
            {
                float projection = Vector2.Dot(corners[i], axis);
                if (projection < min)
                    min = projection;
                if (projection > max)
                    max = projection;
            }
        }

        //get tiles corners
        private Vector2[] GetTileCorners(Vector2 tilePosition, TileMap tileMap)
        {
            int tileX = (int)(tilePosition.X / tileMap.TileSize);
            int tileY = (int)(tilePosition.Y / tileMap.TileSize);

            Vector2 topLeft = new Vector2(tileX * tileMap.TileSize, tileY * tileMap.TileSize);
            Vector2 topRight = topLeft + new Vector2(tileMap.TileSize, 0);
            Vector2 bottomLeft = topLeft + new Vector2(0, tileMap.TileSize);
            Vector2 bottomRight = topLeft + new Vector2(tileMap.TileSize, tileMap.TileSize);

            return new Vector2[] { topLeft, topRight, bottomRight, bottomLeft };
        }

        private List<Vector2> GetTileProjectionAxes(Vector2[] tileCorners)
        {
            List<Vector2> axes = new List<Vector2>();
            for (int i = 0; i < tileCorners.Length; i++)
            {
                Vector2 edge = tileCorners[(i + 1) % tileCorners.Length] - tileCorners[i];
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                normal.Normalize();
                axes.Add(normal);
            }
            return axes;
        }

        //move with the ship sprite to detect collisions
        public void UpdateHitbox()
        {
            float width = _frameWidth;
            float halfHeight = _frameHeight / 5;

            Vector2 backLeft = new Vector2(0, -halfHeight);
            Vector2 backRight = new Vector2(0, halfHeight);
            Vector2 frontLeft = new Vector2(width * 0.8f, -halfHeight);
            Vector2 frontRight = new Vector2(width * 0.8f, halfHeight);
            Vector2 frontCenter = new Vector2(width, 0);

            Matrix rotationMatrix = Matrix.CreateRotationZ(_rotation);
            _hitboxCorners[0] = Vector2.Transform(backLeft, rotationMatrix) + _position;
            _hitboxCorners[4] = Vector2.Transform(backRight, rotationMatrix) + _position;
            _hitboxCorners[1] = Vector2.Transform(frontLeft, rotationMatrix) + _position;
            _hitboxCorners[3] = Vector2.Transform(frontRight, rotationMatrix) + _position;
            _hitboxCorners[2] = Vector2.Transform(frontCenter, rotationMatrix) + _position;
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

            // calculate new position, apply mtv if colliding
            Vector2 newPosition = CalculateNewPosition(deltaTime);
            _position = newPosition;
            UpdateHitbox();

            //check and resolve collisions
            if (ComputeMTV(tileMap, out Vector2 mtv))
            {
                if (!_wasPreviouslyColliding && _hasStartedMoving)
                {
                    _scoreManager.AddCollisionPoints();
                    _wasPreviouslyColliding = true;
                }
                //apply MTV to reposition the ship so that it no longer collides
                _position += mtv;
                _currentSpeed = 0;
            }
            else
            {
                //flag for score counter
                _wasPreviouslyColliding = false;
            }

            UpdateHitbox();

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

        //new test for mtC logic to make boat "slide" out of collision instead of turning into blocked tiles
        protected bool ComputeMTV(TileMap tileMap, out Vector2 mtv)
        {
            mtv = Vector2.Zero;
            const float epsilon = 0.001f; //tolerance
            const float pushBuffer = 2f;

            List<Vector2[]> potentialCollidingTiles = new List<Vector2[]>();

            //collect nearby blocked tiles
            int tileRadius = (int)Math.Ceiling(_frameWidth / (float)tileMap.TileSize);
            int centerTileX = (int)(_position.X / tileMap.TileSize);
            int centerTileY = (int)(_position.Y / tileMap.TileSize);

            for (int x = centerTileX - tileRadius; x <= centerTileX + tileRadius; x++)
            {
                for (int y = centerTileY - tileRadius; y <= centerTileY + tileRadius; y++)
                {
                    if (x >= 0 && x < tileMap.Width && y >= 0 && y < tileMap.Height)
                    {
                        if (!tileMap.GetTile(tileMap.Map[y, x]).IsPassable)
                        {
                            potentialCollidingTiles.Add(GetTileCorners(new Vector2(x * tileMap.TileSize, y * tileMap.TileSize), tileMap));
                        }
                    }
                }
            }

            if (potentialCollidingTiles.Count == 0)
                return false; //if no potential collisions

            //get the projection axes for the ship
            List<Vector2> shipAxes = GetProjectionAxes();

            bool collisionDetected = false;
            float smallestOverlap = float.MaxValue;
            Vector2 finalMTV = Vector2.Zero;

            //;oop thru potential collisions
            foreach (var tileCorners in potentialCollidingTiles)
            {
                bool tileCollides = true;
                float candidateOverlap = float.MaxValue;
                Vector2 candidateMTV = Vector2.Zero;

                //get projection axes for the tile
                List<Vector2> tileAxes = GetTileProjectionAxes(tileCorners);

                //check both ship and tile axes
                foreach (Vector2 axis in shipAxes.Concat(tileAxes))
                {
                    ProjectOntoAxis(axis, _hitboxCorners, out float minA, out float maxA);
                    ProjectOntoAxis(axis, tileCorners, out float minB, out float maxB);

                    //if gap, no collision
                    if (maxA < minB - epsilon || maxB < minA - epsilon)
                    {
                        tileCollides = false;
                        break;
                    }
                    else
                    {
                        //calculate overlap
                        float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
                        //track overlap -- this is waht will make MTV work
                        if (overlap < candidateOverlap)
                        {
                            candidateOverlap = overlap;
                            candidateMTV = axis * overlap;

                            //adjust MTV direction so it pushes the ship out
                            Vector2 tileCenter = (tileCorners[0] + tileCorners[2]) / 2;
                            if (Vector2.Dot(_position - tileCenter, axis) < 0)
                            {
                                candidateMTV = -candidateMTV;
                            }
                        }
                    }
                }

                if (tileCollides && candidateOverlap < smallestOverlap)
                {
                    smallestOverlap = candidateOverlap;
                    finalMTV = candidateMTV;
                    collisionDetected = true;
                }
            }

            //push ship back a little
            if (collisionDetected && finalMTV != Vector2.Zero)
            {
                finalMTV += Vector2.Normalize(finalMTV) * pushBuffer;
            }

            mtv = finalMTV;
            return collisionDetected;
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
        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            float scale = 1f;
            //crashes here
            //spriteBatch.Draw(BoatTexture, _position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(BoatTexture, _position - camera.Position, _sourceRectangle, Color.White, _rotation, _origin, scale, SpriteEffects.None, 0f);

            //temporary draw hitbox for debug
            for (int i = 0; i < _hitboxCorners.Length; i++)
            {
                Vector2 start = _hitboxCorners[i] - camera.Position;
                Vector2 end = _hitboxCorners[(i + 1) % _hitboxCorners.Length] - camera.Position; //loop back to first point

                DrawLine(spriteBatch, start, end, Color.Red);
            }
        }
    }
}
