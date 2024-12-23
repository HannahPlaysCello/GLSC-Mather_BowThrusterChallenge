using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Text.Json;
using BowThrusterChallenge.Settings;
using System;  

using System.Windows.Forms; //for the screen



namespace BowThrust_MonoGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Settings _settings;
    private Ship _ship;
 

    //game setup
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true; // <- change this after

        // window setup
        Window.AllowUserResizing = true;
        Window.Position = new Point(0, 0); // top-left corner

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        LoadSettings();

        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;
        //initialize the ship at the center of the screen, will need to place at the edge for actual gameplay 
        _ship = new Ship(new Vector2(screenWidth / 2, screenHeight / 2), screenWidth, screenHeight);

        base.Initialize();
    }





    //settings from jsons. maybe should handle totally differently
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
        
        //settings from config
        _graphics.PreferredBackBufferWidth = _settings.Window.Width; 
        _graphics.PreferredBackBufferHeight = _settings.Window.Height;
        Window.Title = _settings.Window.Title;

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
            Window = new WindowSettings { Width = 800, Height = 600, Title = "Bow Thruster Challenge" },
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
        Texture2D boatTexture = Content.Load<Texture2D>("testingGrid"); 
        _ship.LoadContent(boatTexture);

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            Exit();

        KeyboardState keyboardState = Keyboard.GetState();
        _ship.Update(gameTime, keyboardState);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        
        GraphicsDevice.Clear(new Color(0, 69, 255, 0));

        _spriteBatch.Begin();

        _ship.Draw(_spriteBatch); //draw the right frame from ship.cs




        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
