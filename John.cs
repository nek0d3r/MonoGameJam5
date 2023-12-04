﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Content;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace MonoGameJam5;

public enum Facing
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public class John : Game
{
    // Handles graphics, drawing, and rendering
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _render;

    // Once we have menus and stuff, this should probably go to it's own class.
    private Song _backgroundMusic;

    // Input handling and listening
    KeyboardState currentKey = new KeyboardState(), prevKey;

    // Map and level resources and rendering
    public static TiledMap _tiledMap;
    TiledMapRenderer _tiledMapRenderer;

    // Spritesheet
    SpriteSheet _spriteSheet;

    // Entities like player, boxes, etc
    List<Entity> entities;

    // Main constructor, called when program starts
    public John()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Allow user to resize window, add event handler
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += TileRender.WindowChanged;
    }

    // Called once after main constructor finishes
    protected override void Initialize()
    {
        // Set buffer size to default
        _graphics.PreferredBackBufferWidth = TileRender.DEFAULT_WINDOW_SIZE.X;
        _graphics.PreferredBackBufferHeight = TileRender.DEFAULT_WINDOW_SIZE.Y;
        _graphics.ApplyChanges();

        // Update window handler to reflect buffer change
        TileRender.WindowChanged(Window, null);

        // Create new camera
        Camera.Initialize(new BoxingViewportAdapter(
            Window,
            GraphicsDevice,
            TileRender.DEFAULT_WINDOW_SIZE.X,
            TileRender.DEFAULT_WINDOW_SIZE.Y
        ));
        
        base.Initialize();
    }

    // Called once after initialization
    protected override void LoadContent()
    {
        // Create drawing and buffer objects
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _render = new RenderTarget2D(GraphicsDevice, TileRender.BUFFER_SIZE.X, TileRender.BUFFER_SIZE.Y);

        // Load level and create Tiled map renderer
        _tiledMap = Content.Load<TiledMap>("maps/map");
        _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

        // Load spritesheet
        _spriteSheet = Content.Load<SpriteSheet>("pixel/spritesheet-animations.sf", new JsonContentLoader());

        entities = new List<Entity>();

        // For every object layer in the map
        foreach(TiledMapObjectLayer layer in _tiledMap.ObjectLayers)
        {
            // For every object in the layer
            foreach(TiledMapObject tiledObject in layer.Objects)
            {
                switch (tiledObject.Type)
                {
                    case "box":
                        entities.Add(new Box()
                        {
                            Position = tiledObject.Position,
                            Sprite = new AnimatedSprite(_spriteSheet),
                            Animation = tiledObject.Properties["texture"]
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        // Create new player
        entities.Add(new Player()
        {
            Speed = 70,
            Position = new Vector2(
                TileRender.BUFFER_SIZE.X / 2,
                TileRender.BUFFER_SIZE.Y / 2
            ),
            realPos = new Vector2(
                TileRender.BUFFER_SIZE.X / 2,
                TileRender.BUFFER_SIZE.Y / 2
            ),
            Sprite = new AnimatedSprite(_spriteSheet),
            Animation = "playerDown"
        });
        // This will force the first frame of the animation to play.
        // Without this, idling at the game start will only draw the first sprite in the sheet
        entities.ForEach(entity => entity.Sprite.Update(0));

        // Load music
        _backgroundMusic = Content.Load<Song>("Music/Sneak");
        MediaPlayer.Play(_backgroundMusic);
        // This should be a setting in an options menu eventually.
        MediaPlayer.Volume = 0.1f;
        MediaPlayer.IsRepeating = true;
    }

    // Called repeatedly until game ends, handles logic updates (e.g. object positions, game state)
    protected override void Update(GameTime gameTime)
    {
        // Set previous key state and update current state
        prevKey = currentKey;
        currentKey = Keyboard.GetState();

        // Default exit on escape
        if (currentKey.IsKeyDown(Keybindings.Exit))
        {
            Exit();
        }

        // Window is not supporting Alt + Enter despite Window.AllowAltF4, so this handles shortcut fullscreen toggle
        if ((currentKey.IsKeyDown(Keys.LeftAlt) || currentKey.IsKeyDown(Keys.RightAlt)) && currentKey.IsKeyDown(Keys.Enter) && !prevKey.IsKeyDown(Keys.Enter))
        {
            // If not currently fullscreen, handle window changes before fullscreen
            if (!_graphics.IsFullScreen)
            {
                // Capture current window size
                TileRender.previousWindowSize = TileRender.currentWindowSize;
                // Set graphics buffer to native resolution and apply
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.ApplyChanges();
            }
            _graphics.ToggleFullScreen();
            // If not currently fullscreen, handle window changes after exiting fullscreen
            if (!_graphics.IsFullScreen)
            {
                // Set graphics buffer to previously captured window size and apply
                _graphics.PreferredBackBufferWidth = TileRender.previousWindowSize.X;
                _graphics.PreferredBackBufferHeight = TileRender.previousWindowSize.Y;
                _graphics.ApplyChanges();
            }
            // Trigger window change
            TileRender.WindowChanged(Window, null);
        }

        // Handles any animated tiles in Tiled map
        _tiledMapRenderer.Update(gameTime);

        // Update entities
        entities.ForEach(entity => entity.Update(gameTime));

        // Updates camera to player position
        Camera.MoveCamera(gameTime, (Player)entities.Where(entity => entity.GetType() == typeof(Player)).FirstOrDefault());

        base.Update(gameTime);
    }

    // Called repeatedly until game ends, handles all drawing and rendering
    protected override void Draw(GameTime gameTime)
    {
        // Render drawing to sprite buffer
        GraphicsDevice.SetRenderTarget(_render);
        GraphicsDevice.Clear(Color.Black);

        // Handles drawing map based on camera's view
        _tiledMapRenderer.Draw(Camera.ViewMatrix);

        // Start point clamped drawing based on camera view
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.ViewMatrix);

        // Sort objects in the layer by Y position
        // This allows sprites to draw over each other based on which one "looks" in front
        entities.Sort(new DrawComparer());

        // Draw each entity
        entities.ForEach(entity => {
            _spriteBatch.Draw(entity.Sprite, entity.Position);
        });

        // End drawing
        _spriteBatch.End();

        // Set render target to device back buffer and clear
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        // Draw sprite buffer to back buffer
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        _spriteBatch.Draw(_render, TileRender.renderDims, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
