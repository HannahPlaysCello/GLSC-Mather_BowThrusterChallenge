using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Text.Json;
using BowThrusterChallenge.Settings;
using System;
using System.Collections.Generic;
//using System.Drawing;

namespace BowThrust_MonoGame;

public class Game1 : Game
{
    //get files
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Settings _settings;
    private Ship _ship;
    private ShipWThrusters _shipWThrusters;
    private Texture2D _boatTexture;

    //window set-up
    private int screenWidth;
    private int screenHeight;
    private Microsoft.Xna.Framework.Color _backgroundColor;

    //game modes
    private enum GameState 
    {
        StartScreen,
        Menu,
        Practice, //right now, this contains both normal and thruster. might change
        Challenge,
        ChallengeTransition,
        GameOver
    }
    private GameState _currentState = GameState.StartScreen; //state to load on start-up
    private bool _useThrusters = false;

    //inputs
    private Dictionary<String, Keys> _controlKeyMap;

    //tile map set-up
    private TileMap _tileMap;
    private int desiredTileSize = 32;

    //score
    private ScoreManager _scoreManager;

    //menu
    private MenuManager _menuManager;

    //timeout 
    private float idleTime = 0f;
    private const float timeOutLen = 45f;
    private bool inGame = false;

    //keep track of challenge Mode
    private bool _challengePhaseOne = true; //start in normal mode without thrsuters
    private int _challengeCollisionNoThrusters = 0; //score for no thrusters
    private int _challengeCollisionWThrusters = 0; //score for thrusters
    private bool _challengeComplete = false; //results screen
    private string _transitionMessage = ""; //to store different strings based on different phases
    private float _overlayAlpha = 0f; //transparency level for phase transition
    private const float _overlayFadeSpeed = 1.5f; //change this value for how fast transition screen fades in

    //fonts
    private SpriteFont _font;


    //game setup
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true; 

        // window setup
        Window.AllowUserResizing = false;
        Window.Position = new Microsoft.Xna.Framework.Point(0, 0); // why does this not go to the top-left corner lol
    }

    //settings from json
    private void LoadSettings()
    {
        try
        {
            // try to load settings from appsettings.json
            string jsonString = File.ReadAllText("appsettings.json");
            _settings = JsonSerializer.Deserialize<Settings>(jsonString);
        
            if (_settings == null)
            {
                throw new System.Exception("settings could not be loaded.");
            }
            
        _graphics.IsFullScreen = true;
        _graphics.HardwareModeSwitch = false;
        _graphics.PreferredBackBufferWidth = _settings.Window.Width; //make window size so boat goes all the way to the edge
        _graphics.PreferredBackBufferHeight = _settings.Window.Height;
        //Window.IsBorderless = false;
        
        Window.Title = _settings.Window.Title; 

        //apply background color from JSON
        _backgroundColor = new Microsoft.Xna.Framework.Color(
            _settings.BackgroundColor.R, 
            _settings.BackgroundColor.G, 
            _settings.BackgroundColor.B, 
            _settings.BackgroundColor.A
        );

        _controlKeyMap = new Dictionary<string, Keys>
        {
            { "Go", ParseKeyFromString(_settings.Controls.Go) },
            { "RudderLeft", ParseKeyFromString(_settings.Controls.RudderLeft) },
            { "RudderRight", ParseKeyFromString(_settings.Controls.RudderRight) },
            { "ThrusterLeft", ParseKeyFromString(_settings.Controls.ThrusterLeft) },
            { "ThrusterRight", ParseKeyFromString(_settings.Controls.ThrusterRight) },
            { "Restart", ParseKeyFromString(_settings.Controls.Restart) },
            { "Menu", ParseKeyFromString(_settings.Controls.Menu) },
            { "Close", ParseKeyFromString(_settings.Controls.Close) },
            { "Select", ParseKeyFromString(_settings.Controls.Select) },
            { "MenuUp", ParseKeyFromString(_settings.Controls.MenuUp) },
            { "MenuDown", ParseKeyFromString(_settings.Controls.MenuDown) }
        };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config file {ex.Message}");
            SetDefaultSettings();
        }
    }

    //for key input control from json!!!!!!!!!
    private Keys ParseKeyFromString(string keyName)
    {
        try
        {
            //make string to a Keys enum value
            return (Keys)Enum.Parse(typeof(Keys), keyName);
        }
        catch (ArgumentException)
        {
            //fallback
            Console.WriteLine($"Invalid key string: {keyName}");
            return Keys.None;
        }
    }

    //if missing/error with config PLEASE FINISH THIS ROUTINE THIS HAS TO HAPPEN SERIOUSLY
    private void SetDefaultSettings()
    {
        _settings = new Settings
        {
           Window = new WindowSettings 
            { 
                Width = 1920, 
                Height = 1080, 
                Title = "Bow Thruster Challenge" 
            },
            BackgroundColor = new ColorSettings
            { 
                R = 0, 
                G = 69, 
                B = 255, 
                A = 255  // Default to fully opaque blue
            },

            Controls = new ControlSettings //fill these out!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                Go = "Space", 
                RudderLeft = "A", 
                RudderRight = "D", 
                ThrusterLeft = "Left", 
                ThrusterRight = "Right", 
                Menu = "M" }
        };
    } 

    //control tile map
    private void ResetTileMap()
    {
        _tileMap = new TileMap();
        _tileMap.LoadFromJson(Content, "Content/TileMapSimple.json", "Content/Tiles.json", desiredTileSize);
    }

    //
    protected override void Initialize()
    {
        LoadSettings();

        screenWidth = _graphics.PreferredBackBufferWidth;
        screenHeight = _graphics.PreferredBackBufferHeight;

        // ship placement on screen
        _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
        _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
        
        Console.WriteLine($"Initial Ship Position: {_ship.Position}");
        Console.WriteLine($"Initial ShipWThrusters Position: {_shipWThrusters.Position}");
        
        ResetTileMap();
        
        base.Initialize();

        _graphics.ApplyChanges();  
    }

    //game content
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //boat texture (boat sprite sheet) from ship.cs
        _boatTexture = Content.Load<Texture2D>("MatherV2-NoBack-WithCorners"); 
        _font = Content.Load<SpriteFont>("MenuFont");

        _scoreManager = new ScoreManager(_font);

        _menuManager = new MenuManager(_font);

    }

    //
    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        //idle timer
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (Keyboard.GetState().GetPressedKeys().Length > 0)
        {
            idleTime = 0f; //reset timer
        }
        else
        {
            idleTime += deltaTime; //increment timer if no input
        }
        if (idleTime >= timeOutLen && inGame)
        {
            _currentState = GameState.StartScreen;
        }

        //close game from any screen!
        if (keyboardState.IsKeyDown(_controlKeyMap["Close"]))
            Exit();
        //reset button -- gotta add this to everywhere instead of the stupid menu button  
        if (keyboardState.IsKeyDown(_controlKeyMap["Restart"]))
            {
                _currentState = GameState.Menu;

                if (_ship != null) _ship = null;
                if (_shipWThrusters != null) _shipWThrusters = null;

                _challengeComplete = false;
                _challengePhaseOne = true;

                _scoreManager.ResetScore();
                _challengeCollisionNoThrusters = 0;
                _challengeCollisionWThrusters = 0;

                ResetTileMap();
            }

        //game states
        if (_currentState == GameState.StartScreen)
        {
            inGame = false;
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _currentState = GameState.Menu;
                inGame = true;
            }
        }

        else if (_currentState == GameState.Menu)
        {
            bool startGame = _menuManager.Update(keyboardState, _controlKeyMap);

            if (startGame)
            {
                //modified to include more than 2 options (why did i make this difficult for myself)
                if (_menuManager.GetSelectedOption() == 0) //normal mode
                {
                    _useThrusters = false;
                    _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
                    _ship.LoadContent(_boatTexture, GraphicsDevice);
                    _currentState = GameState.Practice;
                }
                else if (_menuManager.GetSelectedOption() == 1) //thrusters
                {
                    _useThrusters = true;
                    _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
                    _shipWThrusters.LoadContent(_boatTexture, GraphicsDevice);
                    _currentState = GameState.Practice;
                }
                else if (_menuManager.GetSelectedOption() == 2) //challenge
                {
                    _currentState = GameState.Challenge;
                    _challengePhaseOne = true; //start in normal mode
                    _challengeComplete = false;
                    _scoreManager.ResetScore();

                    //load ship for normal mode
                    _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
                    _ship.LoadContent(_boatTexture, GraphicsDevice);
                }
            }
        }

        else if (_currentState == GameState.Practice)
        {

            if (keyboardState.IsKeyDown(_controlKeyMap["Menu"]))
            {
                _currentState = GameState.Menu;

                _ship = null;
                _shipWThrusters = null;
            }
            // Update the selected ship
            if (_useThrusters && _shipWThrusters != null)
                _shipWThrusters.Update(gameTime, keyboardState, _controlKeyMap, _tileMap);
            else if (!_useThrusters && _ship !=null)
                _ship.Update(gameTime, keyboardState, _controlKeyMap, _tileMap);
        
            //send to end screen
            if (_useThrusters && _shipWThrusters != null && _shipWThrusters.IsEndTileAtPosition(_shipWThrusters.Position, _tileMap))
            {
                _currentState = GameState.GameOver;
            }
            else if (!_useThrusters && _ship != null && _ship.IsEndTileAtPosition(_ship.Position, _tileMap))
            {
                _currentState = GameState.GameOver;
            }
        }

        else if (_currentState == GameState.Challenge)
        {
            Console.WriteLine("challenge mode phase " + (_challengePhaseOne ? "1" : "2"));

            if (!_challengeComplete) //allow gameplay if not done
            {
                if (_challengePhaseOne)
                {
                    Console.WriteLine("update _ship");

                    //_ship.Update(gameTime, keyboardState, _controlKeyMap, _tileMap);
                    if (_ship == null)
                        Console.WriteLine("_ship is null before update");
                    else
                        _ship.Update(gameTime, keyboardState, _controlKeyMap, _tileMap);


                    if (_ship != null && _ship.IsEndTileAtPosition(_ship.Position, _tileMap))
                    {
                        Console.WriteLine("ship reached end tile");
                        _challengeCollisionNoThrusters = _scoreManager.Collisions;
                        _scoreManager.ResetScore();
                        //_challengePhaseOne = false;
                        if (_scoreManager == null)
                            Console.WriteLine("SCORE MANAGER IS NULL");
                        else 
                            Console.WriteLine("score manager exists");
                        
                        if (_boatTexture == null)
                            Console.WriteLine("boattexture is null");
                        else
                            Console.WriteLine("boat texture still exists");

                        _transitionMessage = ($"You finished round 1 with {_challengeCollisionNoThrusters}collisions!\nPress space to continue");
                        _currentState = GameState.ChallengeTransition;
                    }
                }

                else //thrusters
                {
                    _shipWThrusters.Update(gameTime, keyboardState, _controlKeyMap, _tileMap);
                    if (_shipWThrusters != null && _shipWThrusters.IsEndTileAtPosition(_shipWThrusters.Position, _tileMap))
                    {
                        _challengeCollisionWThrusters = _scoreManager.Collisions;
                        _challengeComplete = true; //go to resutls screen
                    }
                }
            }



            //i want this to work at all times, altho this will have to stay on backend or add reset button. will have to remove this block in subroutines so im not duplicating
            /*else 
            {
                if (keyboardState.IsKeyDown(_controlKeyMap["Restart"]))
                {
                    _currentState = GameState.Menu;
                    _ship = null;
                    _shipWThrusters = null;
                    _scoreManager.ResetScore();
                }
            }*/
        }

        else if (_currentState == GameState.ChallengeTransition)
        {
            _overlayAlpha = Math.Min(_overlayAlpha + (_overlayFadeSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), 1f);
            if (keyboardState.IsKeyDown(_controlKeyMap["Go"])) //make this read from key dictionary 
            {
                _overlayAlpha = 0f; //rest overlay for next time (duh im so good at making the same mistake over and over again)
                _currentState = GameState.Challenge; //resume game
                _challengePhaseOne = false; //resume in trhuster mode
                //_scoreManager.ResetScore(); didn't work
                _ship = null;
                _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight, _scoreManager);
                _shipWThrusters.LoadContent(_boatTexture, GraphicsDevice);
            }
        }

        else if (_currentState == GameState.GameOver)
        {
            if (keyboardState.IsKeyDown(_controlKeyMap["Restart"]))
            {
                _scoreManager.ResetScore(); //reset score
                _ship = null; //clear the ships
                _shipWThrusters = null;
                _currentState = GameState.Menu; //go to menu
            }
        }

        base.Update(gameTime);
    }

    //
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_backgroundColor);

        _spriteBatch.Begin();

        if (_currentState == GameState.StartScreen)
        {
            string message = "Press spacebar to start";
            Vector2 textSize = _font.MeasureString(message);
            Vector2 textPosition = new Vector2(
                (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (GraphicsDevice.Viewport.Height - textSize.Y) / 2
            );
            _spriteBatch.DrawString(_font, message, textPosition, Color.White);
        }
        else if (_currentState == GameState.Menu)
        {
            _menuManager.Draw(_spriteBatch);
            //GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Blue);
        }
        else if (_currentState == GameState.Practice)
        {
            //draw the background
            _tileMap.Draw(_spriteBatch);
            //draw the correct ship
            if (_useThrusters && _shipWThrusters != null)
                _shipWThrusters.Draw(_spriteBatch);
            else if (!_useThrusters && _ship != null)
                _ship.Draw(_spriteBatch);
            //drawing stuff on the playing screen TEMP THIS WILL GO IN ANOTHER FILE 
            //instructions
            string menuText = "Press M to return to menu";
            string controlsText1 = "SPACE: Start/Stop | A: Left | D: Right";
            string controlsText2 = "<-: Left Thruster | ->: Right Thruster";
            //size for instructions
            Vector2 textSize = _font.MeasureString(menuText);
            Vector2 controlsText1Size = _font.MeasureString(controlsText1);
            Vector2 controlsText2Size = _font.MeasureString(controlsText2);
            //position for instrucitons
            Vector2 menuPosition = new Vector2(screenWidth - textSize.X - 10, screenHeight - textSize.Y - 10);
            Vector2 controlsPosition1 = new Vector2(screenWidth - controlsText1Size.X - 10, menuPosition.Y - controlsText1Size.Y - 5);
            Vector2 controlsPosition2 = new Vector2(screenWidth - controlsText2Size.X - 10, controlsPosition1.Y - controlsText2Size.Y - 5);
            //draw instructions
            _spriteBatch.DrawString(_font, menuText, menuPosition, Microsoft.Xna.Framework.Color.LightGray);
            _spriteBatch.DrawString(_font, controlsText1, controlsPosition1, Microsoft.Xna.Framework.Color.LightGray);
            _spriteBatch.DrawString(_font, controlsText2, controlsPosition2, Microsoft.Xna.Framework.Color.LightGray);
            //position for score counter
            Vector2 scorePosition = new Vector2(_graphics.PreferredBackBufferWidth - 400, -20); //will figure out how i want to score this later, then move this into the visible area of teh screen lmao!
            Vector2 collisionPosition = new Vector2(_graphics.PreferredBackBufferWidth - 400, 15);
            //draw collision counter
            _scoreManager.Draw(_spriteBatch, scorePosition, collisionPosition);
        }
        else if (_currentState == GameState.Challenge)
        {
            if (!_challengeComplete)
            {
                _tileMap.Draw(_spriteBatch);

                if (_challengePhaseOne && _ship != null)
                {
                    _ship.Draw(_spriteBatch);
                }
                else if (!_challengePhaseOne && _shipWThrusters != null)
                {
                    _shipWThrusters.Draw(_spriteBatch);
                }

                /*
                if (_challengePhaseOne)
                    _ship.Draw(_spriteBatch);
                else
                    _shipWThrusters.Draw(_spriteBatch);

                */
                _spriteBatch.DrawString(_font, "Challenge Mode!", new Vector2(100, 15), Color.Yellow);
                _spriteBatch.DrawString(_font, "Phase: " + (_challengePhaseOne ? "1, No Thrusters" : "2, With Thrusters"), new Vector2(100, 65), Color.White);

                //position for score counter
                Vector2 scorePosition = new Vector2(_graphics.PreferredBackBufferWidth - 400, -20); //will figure out how i want to score this later, then move this into the visible area of teh screen lmao!
                Vector2 collisionPosition = new Vector2(_graphics.PreferredBackBufferWidth - 400, 15);
                //draw collision counter
                _scoreManager.Draw(_spriteBatch, scorePosition, collisionPosition);
            }
            else 
            {
                //results
                _spriteBatch.DrawString(_font, "Challenge Mode Complete!", new Vector2(300, 150), Color.LimeGreen);
                _spriteBatch.DrawString(_font, $"Collisions without Thrusters: {_challengeCollisionNoThrusters}", new Vector2(300, 200), Color.White);
                _spriteBatch.DrawString(_font, $"Collisions with Thrusters: {_challengeCollisionWThrusters}", new Vector2(300, 250), Color.White);

                string resultMessage = _challengeCollisionWThrusters < _challengeCollisionNoThrusters
                    ? "Thrusters improved your navigation!" 
                    : "Try again, thrusters should help!";

                _spriteBatch.DrawString(_font, resultMessage, new Vector2(300, 300), Color.Yellow);
                _spriteBatch.DrawString(_font, "Press R to return to menu", new Vector2(300, 350), Color.Gray);
            }
        }
        else if (_currentState == GameState.ChallengeTransition)
        {
            //keep map and boat
            _tileMap.Draw(_spriteBatch);
            if (_challengePhaseOne && _ship != null)
                _ship.Draw(_spriteBatch);
            else if (!_challengePhaseOne && _shipWThrusters != null)
                _shipWThrusters.Draw(_spriteBatch);
            //overlay for message
            Texture2D overlayTexture = new Texture2D(GraphicsDevice, 1, 1);
            overlayTexture.SetData(new[] { new Color(0, 0, 0, (int)(200 * _overlayAlpha)) });
            _spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            //write message
            string message = _transitionMessage;
            Vector2 textSize = _font.MeasureString(message);
            Vector2 textPosition = new Vector2(
                (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (GraphicsDevice.Viewport.Height - textSize.Y) / 2
            );
            _spriteBatch.DrawString(_font, message, textPosition, Color.Yellow);
        }
        else if (_currentState == GameState.GameOver)
        {
            _spriteBatch.DrawString(_font, "Congratulations! You've reached the goal!", new Vector2(300, 200), Color.LimeGreen);
            _spriteBatch.DrawString(_font, "Press R to restart", new Vector2(300, 250), Color.White);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
