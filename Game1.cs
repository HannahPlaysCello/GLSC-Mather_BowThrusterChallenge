﻿using Microsoft.Xna.Framework;
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


//game setup
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // window setup
        Window.AllowUserResizing = true;
        Window.Position = new Point(0, 0); // top-left corner

        _graphics.ApplyChanges();
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

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
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
