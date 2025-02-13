using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BowThrust_MonoGame
{
    public class ShipWThrusters : ShipBase
    {
        private float _currentThrusterSpeed = 0f;
        private const float _maxThrusterSpeed = 100f;
        private const float _thrusterAcceleration = 100f;
        private const float _thrusterDeceleration = 80f;

        public ShipWThrusters(Vector2 initialPosition, int screenWidth, int screenHeight)
            : base(initialPosition, screenWidth, screenHeight) { }

        //main update loop
        public override void Update(GameTime gameTime, KeyboardState keyboardState, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            base.Update(gameTime, keyboardState, _controlKeyMap, tileMap);
            HandleThrusterMovement(keyboardState, gameTime, _controlKeyMap, tileMap);
        }

        private void HandleThrusterMovement(KeyboardState keyboardState, GameTime gameTime, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterLeft"]))
                _currentThrusterSpeed = Math.Max(_currentThrusterSpeed - _thrusterAcceleration * deltaTime, -_maxThrusterSpeed);
            else if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterRight"]))
                _currentThrusterSpeed = Math.Min(_currentThrusterSpeed + _thrusterAcceleration * deltaTime, _maxThrusterSpeed);
            else
            {
                if (_currentThrusterSpeed > 0)
                    _currentThrusterSpeed = 0;
                else if (_currentThrusterSpeed < 0)
                    _currentThrusterSpeed = 0;
            }

            if (_currentThrusterSpeed != 0)
            {
                float sideDeltaX = (float)Math.Cos(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;
                float sideDeltaY = (float)Math.Sin(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;

                Vector2 newPosition = _position + new Vector2(sideDeltaX, sideDeltaY);

                //assign hitbox corners
                Vector2 topLeft = _hitboxCorners[0];
                Vector2 topRight = _hitboxCorners[1];
                Vector2 bottomLeft = _hitboxCorners[2];
                Vector2 bottomRight = _hitboxCorners[3];

                // Print corner positions
                Console.WriteLine($"TopLeft: {topLeft}, TopRight: {topRight}, BottomLeft: {bottomLeft}, BottomRight: {bottomRight}");

                // Check if each individual corner is colliding
                bool topLeftCollision = IsCollisionAtPosition(new Vector2(topLeft.X, topLeft.Y - 5), tileMap);
                bool topRightCollision = IsCollisionAtPosition(new Vector2(topRight.X, topRight.Y - 5), tileMap);
                bool bottomLeftCollision = IsCollisionAtPosition(new Vector2(bottomLeft.X, bottomLeft.Y + 5), tileMap);
                bool bottomRightCollision = IsCollisionAtPosition(new Vector2(bottomRight.X, bottomRight.Y + 5), tileMap);

                // Print collision results for each corner
                Console.WriteLine($"TopLeft Collision: {topLeftCollision}, TopRight Collision: {topRightCollision}, BottomLeft Collision: {bottomLeftCollision}, BottomRight Collision: {bottomRightCollision}");


                // Check collision per side
                bool topCollision = topLeftCollision && topRightCollision; // Both top corners must hit
                bool bottomCollision = bottomLeftCollision && bottomRightCollision; // Both bottom corners must hit

                Console.WriteLine($"Thruster Speed: {_currentThrusterSpeed}, Left Collision: {topCollision}, Right Collision: {bottomCollision}");

                // Allow movement unless that side is colliding
                if (!topCollision && _currentThrusterSpeed < 0)
                {
                    _position.Y = newPosition.Y;
                    Console.WriteLine("Moving Up");
                }
                if (!bottomCollision && _currentThrusterSpeed > 0)
                {
                    _position.Y = newPosition.Y;
                    Console.WriteLine("Moving Down");
                }

                // Disable ONLY the thruster pushing into the wall
                if (topCollision && keyboardState.IsKeyDown(_controlKeyMap["ThrusterLeft"]))
                {
                    _currentThrusterSpeed = 0;
                    Console.WriteLine("Left Thruster Disabled Due to Collision");
                }
                else if (bottomCollision && keyboardState.IsKeyDown(_controlKeyMap["ThrusterRight"]))
                {
                    _currentThrusterSpeed = 0;
                    Console.WriteLine("Right Thruster Disabled Due to Collision");
                }

                // Stop thrusters completely ONLY if both directions are blocked
                if ((topCollision && keyboardState.IsKeyDown(_controlKeyMap["ThrusterLeft"])) &&
                    (bottomCollision && keyboardState.IsKeyDown(_controlKeyMap["ThrusterRight"])))
                {
                    _currentThrusterSpeed = 0;
                    Console.WriteLine("Both Thrusters Disabled - Fully Blocked");
                }
            }
        }


        /*
        //thruster movement and collisions
        private void HandleThrusterMovement(KeyboardState keyboardState, GameTime gameTime, Dictionary<string, Keys> _controlKeyMap, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterLeft"]))
                _currentThrusterSpeed = Math.Max(_currentThrusterSpeed - _thrusterAcceleration * deltaTime, -_maxThrusterSpeed);
            else if (keyboardState.IsKeyDown(_controlKeyMap["ThrusterRight"]))
                _currentThrusterSpeed = Math.Min(_currentThrusterSpeed + _thrusterAcceleration * deltaTime, _maxThrusterSpeed);
            else
            {
                if (_currentThrusterSpeed > 0)
                        _currentThrusterSpeed = Math.Max(_currentThrusterSpeed - _thrusterDeceleration * deltaTime, 0);
                    else if (_currentThrusterSpeed < 0)
                        _currentThrusterSpeed = Math.Min(_currentThrusterSpeed + _thrusterDeceleration * deltaTime, 0);
            }

            if (_currentThrusterSpeed != 0)
            {
                float sideDeltaX = (float)Math.Cos(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;
                float sideDeltaY = (float)Math.Sin(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime;

                Vector2 newPosition = new Vector2(_position.X + sideDeltaX, _position.Y + sideDeltaY);

                //check for collisions before applying movement
                if (!IsCollisionAtPosition(newPosition, tileMap))
                {
                    _position = newPosition; // Move only if no collision
                }
                else
                {
                    _currentThrusterSpeed = 0; // Stop thrusters if collision occurs
                }
            }
        } */
    }
}