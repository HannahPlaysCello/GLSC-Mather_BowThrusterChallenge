using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BowThrust_MonoGame
{
    //i hope this works
    public interface IGameState
    {
        void Enter();
        void Update(GameTime gameTime, KeyboardState keyboardState);
        void Draw(SpriteBatch spriteBatch);
        void Exit();
    }

    public class GameStateManager
    {
        public IGameState CurrentState { get; private set; }

        public GameStateManager(IGameState initialState)
        {
            CurrentState = initialState;
            CurrentState.Enter();
        }

        public void ChangeState(IGameState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            CurrentState.Update(gameTime, keyboardState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            CurrentState.Draw(spriteBatch);
        }
    }



    //--------------------- state classes ----------------------

    #region StartScreenState
    //start screen state: waits for player input (space bar atm)
    public class StartScreenState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public StartScreenState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter()
        {
            _game.inGame = false;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _game.inGame = true;
                _stateManager.ChangeState(new MenuState(_stateManager, _game));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_game.Font, "Press Space to Start", new Vector2(_game.screenWidth / 2 - 275, _game.screenHeight / 2 - 100), Color.White);
        }

        public void Exit() { }
    }
    #endregion

    #region MenuState
    //Menu State: handles menu update logic
    public class MenuState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public MenuState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            bool startGame = _game.MenuManager.Update(keyboardState, _game.ControlKeyMap);
            if (startGame)
            {
                int selectedOption = _game.MenuManager.GetSelectedOption();
                if (selectedOption == 0) //normal mode
                {
                    _game._useThrusters = false;
                    _game._ship = new Ship(new Vector2(0, _game.screenHeight / 2), _game.screenWidth, _game.screenHeight, _game._scoreManager);
                    _game._ship.LoadContent(_game.BoatTexture, _game.GraphicsDevice);
                    _stateManager.ChangeState(new PracticeState(_stateManager, _game, useThrusters: false));
                }
                else if (selectedOption == 1) //thruster mode
                {
                    _game._useThrusters = true;
                    _game._shipWThrusters = new ShipWThrusters(new Vector2(0, _game.screenHeight / 2), _game.screenWidth, _game.screenHeight, _game._scoreManager);
                    _game._shipWThrusters.LoadContent(_game.BoatTexture, _game.GraphicsDevice);
                    _stateManager.ChangeState(new PracticeState(_stateManager, _game, useThrusters: true));
                }
                else if (selectedOption == 2) //challenge mode
                {
                    _game._challengePhaseOne = true;
                    _game._challengeComplete = false;
                    _game._scoreManager.ResetScore();
                    _game._ship = new Ship(new Vector2(0, _game.screenHeight / 2), _game.screenWidth, _game.screenHeight, _game._scoreManager);
                    _game._ship.LoadContent(_game.BoatTexture, _game.GraphicsDevice);
                    _stateManager.ChangeState(new ChallengeState(_stateManager, _game));
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.MenuManager.Draw(spriteBatch);
        }

        public void Exit() { }
    }
    #endregion

    #region PracticeState
    //practice state: updates the selected ship and checks for end tile collisions
    public class PracticeState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;
        private bool _useThrusters;

        public PracticeState(GameStateManager stateManager, Game1 game, bool useThrusters)
        {
            _stateManager = stateManager;
            _game = game;
            _useThrusters = useThrusters;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {

            //update the active ship
            if (_useThrusters && _game._shipWThrusters != null)
            {
                _game._shipWThrusters.Update(gameTime, keyboardState, _game.ControlKeyMap, _game.TileMap);
                _game._camera.Follow(_game._shipWThrusters.Position, _game.TileMap);
                if (_game._shipWThrusters.IsEndTileAtPosition(_game._shipWThrusters.Position, _game.TileMap))
                {
                    _stateManager.ChangeState(new PracticeGameOverState(_stateManager, _game));
                }
            }
            else if (!_useThrusters && _game._ship != null)
            {
                _game._ship.Update(gameTime, keyboardState, _game.ControlKeyMap, _game.TileMap);
                _game._camera.Follow(_game._ship.Position, _game.TileMap);
                if (_game._ship.IsEndTileAtPosition(_game._ship.Position, _game.TileMap))
                {
                    _stateManager.ChangeState(new PracticeGameOverState(_stateManager, _game));
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.TileMap.Draw(spriteBatch, _game._camera);
            if (_useThrusters && _game._shipWThrusters != null)
                _game._shipWThrusters.Draw(spriteBatch, _game._camera);
            else if (!_useThrusters && _game._ship != null)
                _game._ship.Draw(spriteBatch, _game._camera);

            string practiceModeLabel = _useThrusters ? "Practice: With Thrusters" : "Practice: Without Thrusters";
            Color labelColor = _useThrusters ? Color.YellowGreen : Color.GreenYellow;
            spriteBatch.DrawString(_game.Font, practiceModeLabel, new Vector2(50, 15), labelColor);

            Vector2 scorePosition = new Vector2(_game.screenWidth - 400, -20);
            Vector2 collisionPosition = new Vector2(_game.screenWidth - 400, 15);
            _game._scoreManager.Draw(spriteBatch, scorePosition, collisionPosition);
        }

        public void Exit() { }
    }
    #endregion

    #region ChallengeState
    //challenge state 
    public class ChallengeState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public ChallengeState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            if (!_game._challengeComplete)
            {
                if (_game._challengePhaseOne)
                {
                    _game._ship.Update(gameTime, keyboardState, _game.ControlKeyMap, _game.TileMap);
                    _game._camera.Follow(_game._ship.Position, _game.TileMap);
                    if (_game._ship != null && _game._ship.IsEndTileAtPosition(_game._ship.Position, _game.TileMap))
                    {
                        _game._challengeCollisionNoThrusters = _game._scoreManager.Collisions;
                        _game._scoreManager.ResetScore();
                        _game._transitionMessage = $"You finished round 1 with {_game._challengeCollisionNoThrusters} collisions!\nPress space to continue";
                        _stateManager.ChangeState(new ChallengeTransitionState(_stateManager, _game));
                    }
                }
                else //thruster round
                {
                    _game._shipWThrusters.Update(gameTime, keyboardState, _game.ControlKeyMap, _game.TileMap);
                    _game._camera.Follow(_game._shipWThrusters.Position, _game.TileMap);
                    if (_game._shipWThrusters != null && _game._shipWThrusters.IsEndTileAtPosition(_game._shipWThrusters.Position, _game.TileMap))
                    {
                        _game._challengeCollisionWThrusters = _game._scoreManager.Collisions;
                        _game._challengeComplete = true;
                        _stateManager.ChangeState(new ChallengeGameOverState(_stateManager, _game));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.TileMap.Draw(spriteBatch, _game._camera);
            if (_game._challengePhaseOne && _game._ship != null)
                _game._ship.Draw(spriteBatch, _game._camera);
            else if (!_game._challengePhaseOne && _game._shipWThrusters != null)
                _game._shipWThrusters.Draw(spriteBatch, _game._camera);

            Vector2 scorePosition = new Vector2(_game.screenWidth - 400, -20);
            Vector2 collisionPosition = new Vector2(_game.screenWidth - 400, 15);
            _game._scoreManager.Draw(spriteBatch, scorePosition, collisionPosition);

            spriteBatch.DrawString(_game.Font, "Challenge Mode!", new Vector2(100, 15), Color.Yellow);
            spriteBatch.DrawString(_game.Font, "Phase: " + (_game._challengePhaseOne ? "1, No Thrusters" : "2, With Thrusters"), 
                new Vector2(100, 65), Color.White);
        }

        public void Exit() { }
    }
    #endregion

    #region ChallengeTransitionState
    //challenge transition state
    public class ChallengeTransitionState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public ChallengeTransitionState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            _game._overlayAlpha = Math.Min(_game._overlayAlpha + (_game._overlayFadeSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), 1f);
            if (keyboardState.IsKeyDown(_game.ControlKeyMap["Go"]))
            {
                _game._overlayAlpha = 0f;
                _game._challengePhaseOne = false;
                _game._shipWThrusters = new ShipWThrusters(new Vector2(0, _game.screenHeight / 2), _game.screenWidth, _game.screenHeight, _game._scoreManager);
                _game._shipWThrusters.LoadContent(_game.BoatTexture, _game.GraphicsDevice);
                _game._ship = null;
                _game._inputDelayTimer = _game._inputDelayDuration;
                _stateManager.ChangeState(new ChallengeState(_stateManager, _game));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.TileMap.Draw(spriteBatch, _game._camera);
            if (_game._challengePhaseOne && _game._ship != null)
                _game._ship.Draw(spriteBatch, _game._camera);
            else if (!_game._challengePhaseOne && _game._shipWThrusters != null)
                _game._shipWThrusters.Draw(spriteBatch, _game._camera);

            //draw overlay
            Texture2D overlayTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            overlayTexture.SetData(new[] { new Color(0, 0, 0, (int)(200 * _game._overlayAlpha)) });
            spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, _game.screenWidth, _game.screenHeight), Color.White);

            //draw transition message.
            Vector2 messageSize = _game.Font.MeasureString(_game._transitionMessage);
            spriteBatch.DrawString(_game.Font, _game._transitionMessage,
                new Vector2((_game.screenWidth - messageSize.X) / 2, (_game.screenHeight - messageSize.Y) / 2),
                Color.Yellow);
        }

        public void Exit() { }
    }
    #endregion

    //game over states
    #region PracticeGameOverState
    public class PracticeGameOverState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public PracticeGameOverState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_game.Font, "Practice Round Complete!", new Vector2(300, 150), Color.LimeGreen);
            spriteBatch.DrawString(_game.Font, $"Collisions: {_game._scoreManager.Collisions}", new Vector2(300, 200), Color.White);
            spriteBatch.DrawString(_game.Font, "Press R to return to menu", new Vector2(300, 250), Color.Gray);
        }

        public void Exit() { }
    }
    #endregion

    #region ChallengeGameOverState
    public class ChallengeGameOverState : IGameState
    {
        private GameStateManager _stateManager;
        private Game1 _game;

        public ChallengeGameOverState(GameStateManager stateManager, Game1 game)
        {
            _stateManager = stateManager;
            _game = game;
        }

        public void Enter() { }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_game.Font, "Challenge Mode Complete!", new Vector2(300, 150), Color.LimeGreen);
            spriteBatch.DrawString(_game.Font, $"Collisions without Thrusters: {_game._challengeCollisionNoThrusters}", new Vector2(300, 200), Color.White);
            spriteBatch.DrawString(_game.Font, $"Collisions with Thrusters: {_game._challengeCollisionWThrusters}", new Vector2(300, 250), Color.White);

            string resultMessage;
            if (_game._challengeCollisionWThrusters == 0 && _game._challengeCollisionNoThrusters == 0)
            {
                resultMessage = "No Collisions! You're a navigation pro!";
            }
            else
            {
                resultMessage = _game._challengeCollisionWThrusters < _game._challengeCollisionNoThrusters
                    ? "Thrusters improved your navigation!"
                    : "Try again, thrusters should help!";
            }

            spriteBatch.DrawString(_game.Font, resultMessage, new Vector2(300, 300), Color.Yellow);
            spriteBatch.DrawString(_game.Font, "Press R to return to menu", new Vector2(300, 350), Color.Gray);
        }

        public void Exit() { }
    }
    #endregion
}
