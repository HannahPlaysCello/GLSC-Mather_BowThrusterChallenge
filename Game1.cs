using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.IO;
using System.Text.Json;
using BowThrusterChallenge.Settings;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
//using System.Drawing;

namespace BowThrust_MonoGame;

public class Game1 : Game
{
    //get classes from other files
    public GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Settings _settings;
    public Ship _ship;
    public ShipWThrusters _shipWThrusters;
    public Texture2D BoatTexture;
    public Camera _camera; //test! camera for side scroll

    //window set-up
    public int screenWidth;
    public int screenHeight;
    private Microsoft.Xna.Framework.Color _backgroundColor;

    //game modes
    public GameStateManager StateManager { get; private set; }
    private enum GameState 
    {
        StartScreen,
        Menu,
        Practice, //right now, this contains both normal and thruster. might change
        Challenge,
        ChallengeTransition,
        GameOver
    }
    //private GameState CurrentState = GameState.StartScreen; //state to load on start-up
    public bool _useThrusters = false;

    //inputs
    public Dictionary<String, Keys> ControlKeyMap;

    //tile map set-up
    public TileMap TileMap { get; private set; }
    //private TileMap _tileMap;
    private int desiredTileSize = 32;

    //score
    public ScoreManager _scoreManager;

    //menu
    public MenuManager MenuManager { get; private set; }
    //private MenuManager _menuManager;

    //timeout 
    private float idleTime = 0f;
    private const float timeOutLen = 45f;
    public bool inGame = false;

    //keep track of challenge Mode
    public bool _challengePhaseOne = true; //start in normal mode without thrsuters
    public int _challengeCollisionNoThrusters = 0; //score for no thrusters
    public int _challengeCollisionWThrusters = 0; //score for thrusters
    public bool _challengeComplete = false; //results screen
    public string _transitionMessage = ""; //to store different strings based on different phases
    public float _overlayAlpha = 0f; //transparency level for phase transition
    public float _overlayFadeSpeed = 1.5f; //change this value for how fast transition screen fades in
    
    //input debouncer. Used only in challenge transition state right now
    public float _inputDelayTimer = 0f;
    public float _inputDelayDuration = 0.2f;

    //fonts
    public SpriteFont Font { get; private set; }
    //private SpriteFont _font;

    //sounds
    private Song _gameplayMusic;
    //private Song _manuMusic; //not going to implement this one right exactly now
    private bool _isPlayingGameMusic = false;



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

        ControlKeyMap = new Dictionary<string, Keys>
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

    //restart game
    private void ResetGame()
    {
        _scoreManager.ResetScore();
        _ship = null;
        _shipWThrusters = null;
        
        // Reset any challenge-related variables if necessary.
        _challengePhaseOne = true;
        _challengeComplete = false;
        _challengeCollisionNoThrusters = 0;
        _challengeCollisionWThrusters = 0;
        _overlayAlpha = 0f;
        _inputDelayTimer = 0f;
        
        // Switch back to the Menu state.
        StateManager.ChangeState(new MenuState(StateManager, this));
    }

    //control tile map
    private void ResetTileMap()
    {
        TileMap = new TileMap();
        TileMap.LoadFromJson(Content, "Content/TileMapSimple.json", "Content/Tiles.json", desiredTileSize);
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

        ResetTileMap();

        StateManager = new GameStateManager(new StartScreenState(null, this));
        StateManager.ChangeState( new StartScreenState(StateManager, this));
        
        base.Initialize();

        _camera = new Camera(screenWidth, screenHeight); //test camera for side scroll

        _graphics.ApplyChanges();  


    }

    //game content
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //boat texture (boat sprite sheet) from ship.cs
        BoatTexture = Content.Load<Texture2D>("MatherV2-NoBack-WithCorners"); 
        Font = Content.Load<SpriteFont>("MenuFont");

        _scoreManager = new ScoreManager(Font);

        MenuManager = new MenuManager(Font);

        _gameplayMusic = Content.Load<Song>("Riverboat_Shuffle_-_Bix_Beiderbecke_and_Wolverine_Orchestra_1924");

    }

    //
    protected override void Update(GameTime gameTime)
    {
        //debounce
        if (_inputDelayTimer > 0)
        {
            _inputDelayTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            return; //skip input processing until delay expires
        }

        KeyboardState keyboardState = Keyboard.GetState();
        //close game from any screen!
        if (keyboardState.IsKeyDown(ControlKeyMap["Close"]))
            Exit();

        //rest the game from any screen
        if (keyboardState.IsKeyDown(ControlKeyMap["Restart"]))
            {
                ResetGame();
                return;
            }


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
            StateManager.ChangeState(new StartScreenState(StateManager, this));
            idleTime = 0f;
        }

        //play music!
        if (StateManager.CurrentState is PracticeState 
            || StateManager.CurrentState is ChallengeState 
            || StateManager.CurrentState is ChallengeTransitionState 
            || StateManager.CurrentState is PracticeGameOverState
            || StateManager.CurrentState is ChallengeGameOverState)
        {
            if (!_isPlayingGameMusic)
            {
                MediaPlayer.Stop(); // Stop any previous music (optional)
                MediaPlayer.Play(_gameplayMusic);
                MediaPlayer.IsRepeating = true;
                _isPlayingGameMusic = true;
            }
        }
        else // For states like StartScreen or Menu
        {
            if (_isPlayingGameMusic)
            {
                MediaPlayer.Stop();
                _isPlayingGameMusic = false;
            }
        }

        StateManager.Update(gameTime, keyboardState);
    }

    //
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_backgroundColor);

        _spriteBatch.Begin();

        StateManager.Draw(_spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
