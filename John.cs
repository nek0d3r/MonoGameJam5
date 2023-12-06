using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Content;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace MonoGameJam5;

public class John : Game
{
    protected enum GameState {
        MainMenu,
        GameBegin,
        Playing,
        Paused,
        GameOverBegin, // For animating to a game over screen.
        GameOver
    }

    // Handles some animation durations
    private const int _fadeFrames = 90;
    protected int FadeFrame { get; set; } = _fadeFrames;

    // Handles graphics, drawing, and rendering
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _render;

    // Once we have menus and stuff, this should probably go to it's own class.
    private Song _backgroundMusic, _titleMusic;

    // Game state variable.
    private GameState _gameState;
    private Texture2D _mainMenuScreen;

    // Input handling and listening
    KeyboardState currentKey = new KeyboardState(), prevKey;

    // Map and level resources and rendering
    public static TiledMap _tiledMap;
    TiledMapRenderer _tiledMapRenderer;

    // Spritesheet
    SpriteSheet _spriteSheet;

    // Entities like player, boxes, etc
    List<Entity> _entities;

    // Collision component for collider handling
    private CollisionComponent _collisionComponent;

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
        Window.Title = "Under Pressure";

        // Start at the main menu
        _gameState = GameState.MainMenu;

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

    // Called on inital load and after every loss
    // in order to reset the map.
    private void Reset()
    {
        // Create game objects
        _entities = Entity.CreateEntities(_tiledMap, _spriteSheet);

        _entities.ForEach(entity =>
        {
            // Look for NPC/Enemy actions and populate entity with properties
            Entity.ParseActions(_tiledMap, entity);

            // Force the first frame of the animation to play.
            // Without this, idling at the game start will only draw the first sprite in the sheet.
            if (entity.Sprite != null)
            {
                entity.Sprite.Update(0);
            }
            
            // Add entity as a collider
            _collisionComponent.Insert(entity);
        });

        MediaPlayer.Play(_titleMusic);
        // This should be a setting in an options menu eventually.
        MediaPlayer.Volume = 0.1f;
        MediaPlayer.IsRepeating = true;
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

        // Create collider area
        _collisionComponent = new CollisionComponent(
            new RectangleF(
                0,
                0,
                _tiledMap.WidthInPixels,
                _tiledMap.HeightInPixels
            )
        );

        // Load music
        _titleMusic = Content.Load<Song>("Music/Escape");
        _backgroundMusic = Content.Load<Song>("Music/Sneak");

        // Load the main menu screen.
        _mainMenuScreen = Content.Load<Texture2D>("pixel/title");

        Reset();

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

        
        if (_gameState == GameState.MainMenu) 
        {
            // TODO: Tell the player to press any key to begin.
            if (currentKey.GetPressedKeyCount() > 0)
            {
                // GameBegin is an animation over the title screen before the game begins.
                _gameState = GameState.GameBegin;
            }
        }
        else if (_gameState == GameState.GameBegin)
        {
            --FadeFrame;
            if (FadeFrame <= 0)
            {
                MediaPlayer.Play(_backgroundMusic);
                _gameState = GameState.Playing;
                // Reset the animation counter
                FadeFrame = _fadeFrames;
            }
        }
        else if (_gameState == GameState.GameOverBegin)
        {
            --FadeFrame;
            if (FadeFrame <= 0)
            {
                _gameState = GameState.GameOver;
                // Reset the animation counter
                FadeFrame = _fadeFrames;
            }
        }
        else if (_gameState == GameState.GameOver)
        {
            // TODO: Have some sort of animation effect before returning to the main menu
            if (currentKey.GetPressedKeyCount() > 0)
            {
                // Reset game state
                Reset();
                _gameState = GameState.MainMenu;
            }
        }
        else
        {
            // Handles any animated tiles in Tiled map
            _tiledMapRenderer.Update(gameTime);

            // Update entities
            _entities.ForEach(entity => entity.Update(gameTime));

            // Update collisions
            _collisionComponent.Update(gameTime);

            Player pl = (Player)_entities.Where(entity => entity.GetType() == typeof(Player)).FirstOrDefault();

            // Updates camera to player position
            Camera.MoveCamera(gameTime, pl);

            // Check player for game loss. If yes, change game state.
            if (pl.LostGame)
            {
                _gameState = GameState.GameOverBegin;
                // TODO: Switch to game over music
            }
        }
        base.Update(gameTime);
    }

    // Called repeatedly until game ends, handles all drawing and rendering
    protected override void Draw(GameTime gameTime)
    {
        // Render drawing to sprite buffer
        GraphicsDevice.SetRenderTarget(_render);
        GraphicsDevice.Clear(Color.Black);

        // Start point clamped drawing based on camera view
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.ViewMatrix);

        if (_gameState == GameState.MainMenu) {
            Camera._camera.LookAt(new Vector2(TileRender.DEFAULT_WINDOW_SIZE.X / 2, TileRender.DEFAULT_WINDOW_SIZE.Y / 2));
            _spriteBatch.Draw(_mainMenuScreen, Vector2.Zero, Color.White);
        }
        else if (_gameState == GameState.GameOverBegin || _gameState == GameState.GameBegin)
        {           
            if (_gameState == GameState.GameOverBegin)
            {
                // Handles drawing map based on camera's view
                _tiledMapRenderer.Draw(Camera.ViewMatrix);
            }
            else if (_gameState == GameState.GameBegin)
            {
                _spriteBatch.Draw(_mainMenuScreen, Vector2.Zero, Color.White);
            }

            int upperLeftRectX = (int)Camera.Position.X - TileRender.DEFAULT_WINDOW_SIZE.X / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int upperLeftRectY = (int)Camera.Position.Y - TileRender.DEFAULT_WINDOW_SIZE.Y / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int width = TileRender.DEFAULT_WINDOW_SIZE.X * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int height = TileRender.DEFAULT_WINDOW_SIZE.Y * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            _spriteBatch.FillRectangle(new RectangleF(upperLeftRectX, upperLeftRectY, width, height), Color.Black);
        }
        else if (_gameState == GameState.GameOver) {
            // TODO: Not null for the font
            //_spriteBatch.DrawString(null, "YOU DIED", new Vector2(TileRender.DEFAULT_WINDOW_SIZE.X / 2, TileRender.DEFAULT_WINDOW_SIZE.Y / 2), Color.LightSalmon);
        }
        else
        {
            // Handles drawing map based on camera's view
            _tiledMapRenderer.Draw(Camera.ViewMatrix);
            
            // Sort objects in the layer by draw priority, then Y position
            // This allows sprites to draw over each other based on which one "looks" in front
            _entities = _entities.OrderBy(entity => entity.DrawPriority)
                                .ThenBy(entity => entity.Position.Y)
                                .ToList();

            // Draw each entity
            _entities.ForEach(entity => { entity.Draw(_spriteBatch, true); });

        }
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
