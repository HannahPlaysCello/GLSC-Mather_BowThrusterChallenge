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


//game setup

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        LoadSettings();
        base.Initialize();

    }


    private void LoadSettings()
    {
        try
        {
            string jsonString = File.ReadAllText("appsettings.json");
            _settings = JsonSerializer.Deserialize<Settings>(jsonString);
        
            if (_settings == null)
            {
                throw new System.Exception("settings could not be loaded.");
            }
        
        //settings from config
        _graphics.PreferredBackBufferWidth = _settings.Window.Width; 
        _graphics.PreferredBackBufferHeight = _settings.Window.Height;
        _graphics.ApplyChanges();  

        Console.WriteLine($"Window Width: {_settings.Window.Width}, Height: {_settings.Window.Height}");
        
        Window.Title = _settings.Window.Title;    
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config file {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Using default settings.");
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
    }


//game content
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
