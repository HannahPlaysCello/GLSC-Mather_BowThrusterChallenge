using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Text.Json;
using BowThrusterChallenge.Settings;
using System;


namespace BowThrust_MonoGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Settings _settings;

    private enum GameState { Menu, Playing }
    private GameState _currentState = GameState.Menu;

    private SpriteFont _font;
    private int _selectedOption = 0; //0 is normal, 1 is thruster mode
    private bool _isKeyPressed = false;

    private Ship _ship;
    private ShipWThrusters _shipWThrusters;
    private bool _useThrusters = false;
    private Texture2D _boatTexture;
 

    //game setup
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false; 

        // window setup
        Window.AllowUserResizing = true;
        Window.Position = new Point(0, 0); // not the top-left corner lol

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        LoadSettings();

        
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;
        
        // ship placement on screen
        _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);
        _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);


        base.Initialize();
    }




    //settings from jsons. maybe should handle totally differently idk
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
        
        _graphics.ApplyChanges();  
        
        Window.Title = _settings.Window.Title;    
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config file {ex.Message}");
            SetDefaultSettings();
        }
    }

    //if missing/error with config
    private void SetDefaultSettings()
    {
        _settings = new Settings
        {
            Window = new WindowSettings { Width = 1920, Height = 1080, Title = "Bow Thruster Challenge" },
            Controls = new ControlSettings { Forward = "W", RudderLeft = "A", RudderRight = "D", ThrusterLeft = "Q", ThrusterRight = "E", Restart = "R" }
        };

        _graphics.PreferredBackBufferWidth = _settings.Window.Width;
        _graphics.PreferredBackBufferHeight = _settings.Window.Height;
        Window.Title = _settings.Window.Title;

        _graphics.ApplyChanges();
    }





//game content
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //boat texture (boat sprite sheet) from ship.cs
        _boatTexture = Content.Load<Texture2D>("MatherV2"); 
        _font = Content.Load<SpriteFont>("MenuFont");

    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        if (_currentState == GameState.Menu)
        {
            if (keyboardState.IsKeyDown(Keys.Up) && !_isKeyPressed)
            {
                _selectedOption = 0;  // Normal Mode
                _isKeyPressed = true;
            }
            if (keyboardState.IsKeyDown(Keys.Down) && !_isKeyPressed)
            {
                _selectedOption = 1;  // Thruster Mode
                _isKeyPressed = true;
            }  
        
            if (keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Down))
                _isKeyPressed = false;

            
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                _useThrusters = (_selectedOption == 1);
                int screenWidth = _graphics.PreferredBackBufferWidth;
                int screenHeight = _graphics.PreferredBackBufferHeight;

                // Instantiate the correct ship
                if (_useThrusters)
                    _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);
                else
                    _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);

                // Load texture into ship
                if (_useThrusters)
                    _shipWThrusters.LoadContent(_boatTexture);
                else
                    _ship.LoadContent(_boatTexture);

                _currentState = GameState.Playing;
            }
        }

        else if (_currentState == GameState.Playing)
        {
            // Update the selected ship
            if (_useThrusters && _shipWThrusters != null)
                _shipWThrusters.Update(gameTime, keyboardState);
            else if (!_useThrusters && _ship !=null)
                _ship.Update(gameTime, keyboardState);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        
        GraphicsDevice.Clear(new Color(0, 69, 255, 0));

        _spriteBatch.Begin();

        if (_currentState == GameState.Menu)
        {
            // Draw menu options
            string option1 = "Normal Mode";
            string option2 = "Thruster Mode";
            Color option1Color = (_selectedOption == 0) ? Color.Yellow : Color.White;
            Color option2Color = (_selectedOption == 1) ? Color.Yellow : Color.White;

            _spriteBatch.DrawString(_font, "Choose Boat Mode:", new Vector2(300, 200), Color.White);
            _spriteBatch.DrawString(_font, option1, new Vector2(300, 300), option1Color);
            _spriteBatch.DrawString(_font, option2, new Vector2(300, 350), option2Color);
            _spriteBatch.DrawString(_font, "Press ENTER to select", new Vector2(300, 450), Color.Gray);
        }
        else if (_currentState == GameState.Playing)
        {
            // Draw the correct ship
            if (_useThrusters && _shipWThrusters != null)
                _shipWThrusters.Draw(_spriteBatch);
            else if (!_useThrusters && _ship != null)
                _ship.Draw(_spriteBatch);
        }



        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
