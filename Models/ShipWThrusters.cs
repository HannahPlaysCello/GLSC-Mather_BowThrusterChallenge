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

        public ShipWThrusters(Vector2 initialPosition, int screenWidth, int screenHeight, ScoreManager scoreManager)
            : base(initialPosition, screenWidth, screenHeight, scoreManager) { }

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
                Vector2 thrusterMovement = new Vector2(
                    (float)Math.Cos(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime,
                    (float)Math.Sin(_rotation + MathHelper.PiOver2) * _currentThrusterSpeed * deltaTime
                );

                Vector2 testPosition = _position + thrusterMovement;

                // Only apply thruster movement if it doesn't collide
                if (!IsSATCollision(tileMap))
                {
                    _position = testPosition;
                }
                else
                {
                    _currentThrusterSpeed = 0; // Stop thruster movement
                }
            }
        }
    }
}