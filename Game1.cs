using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Text.Json;
using BowThrusterChallenge.Settings;
using System;
using System.Collections.Generic;

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

    private Color _backgroundColor;

    private Dictionary<String, Keys> _controlKeyMap;

    private TileMap _tileMap;

    private ScoreManager scoreManager;

 

    //game setup
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true; 

        // window setup
        Window.AllowUserResizing = false;
        Window.Position = new Point(0, 0); // why does this not go to the top-left corner lol

    }

    //
    protected override void Initialize()
    {
        LoadSettings();
        
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;
        
        // ship placement on screen
        _ship = new Ship(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);
        _shipWThrusters = new ShipWThrusters(new Vector2(0, screenHeight / 2), screenWidth, screenHeight);

        _tileMap = new TileMap();
        _tileMap.LoadFromJson(Content, "Content/tilemap.json", "Content/Tiles.json", 64);
        
        base.Initialize();

        _graphics.ApplyChanges();  
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
        _backgroundColor = new Color(
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
    
    //if missing/error with config
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

    //game content
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //boat texture (boat sprite sheet) from ship.cs
        _boatTexture = Content.Load<Texture2D>("MatherV2-NoBack-WithCorners"); 
        _font = Content.Load<SpriteFont>("MenuFont");

        scoreManager = new ScoreManager(_font);

    }

    //
    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(_controlKeyMap["Close"]))
            Exit();

        if (_currentState == GameState.Menu)
        {
            if (keyboardState.IsKeyDown(_controlKeyMap["MenuUp"]) && !_isKeyPressed)
            {
                _selectedOption = 0;  //normal Mode
                _isKeyPressed = true;
            }
            if (keyboardState.IsKeyDown(_controlKeyMap["MenuDown"]) && !_isKeyPressed)
            {
                _selectedOption = 1;  //thruster Mode
                _isKeyPressed = true;
            }  
        
            if (keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Down))
                _isKeyPressed = false;

            
            if (keyboardState.IsKeyDown(_controlKeyMap["Select"]))
            {
                _useThrusters = (_selectedOption == 1);
                int screenWidth = _graphics.PreferredBackBufferWidth;
                int screenHeight = _graphics.PreferredBackBufferHeight;

                // make the correct ship
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
        }

        base.Update(gameTime);
    }

    //
    protected override void Draw(GameTime gameTime)
    {
        
        GraphicsDevice.Clear(_backgroundColor);

        _spriteBatch.Begin();

        if (_currentState == GameState.Playing)
        {
            for (int y = 0; y < _tileMap.Height; y++)
            {
                for (int x = 0; x < _tileMap.Width; x++)
                {
                    int tileID = _tileMap.Map[y, x];
                    Tiles tile = _tileMap.GetTile(tileID);

                    _spriteBatch.Draw(tile.TileTexture, new Vector2(x * _tileMap.TileSize, y * _tileMap.TileSize), Color.White);
                }
            }
        }
        else if (_currentState == GameState.Menu)
        {
            GraphicsDevice.Clear(Color.Blue);
        }

        

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
            
            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;
            

            //position for instrucitons
            Vector2 menuPosition = new Vector2(screenWidth - textSize.X - 10, screenHeight - textSize.Y - 10);
            Vector2 controlsPosition1 = new Vector2(screenWidth - controlsText1Size.X - 10, menuPosition.Y - controlsText1Size.Y - 5);
            Vector2 controlsPosition2 = new Vector2(screenWidth - controlsText2Size.X - 10, controlsPosition1.Y - controlsText2Size.Y - 5);
            
            //draw instructions
            _spriteBatch.DrawString(_font, menuText, menuPosition, Color.LightGray);
            _spriteBatch.DrawString(_font, controlsText1, controlsPosition1, Color.LightGray);
            _spriteBatch.DrawString(_font, controlsText2, controlsPosition2, Color.LightGray);

        }
         
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
