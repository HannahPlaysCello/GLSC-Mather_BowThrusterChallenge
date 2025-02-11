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
                if (!tileMap.IsCollisionTile(newPosition))
                {
                    _position = newPosition;  //move if no collision
                }
                else
                {
                    _currentThrusterSpeed = 0;  //stop movement if collision detected
                }
            }
        }
    }
}