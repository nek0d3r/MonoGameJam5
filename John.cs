using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
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
        GameOver,
        GameOverEnd, // For animating between game over and the menu screen.
        VictoryBegin, // Animating to a victory screen
        Victory, // Explaining how your work isn't done from stealing one CEO-only toilet
        VictoryCredits, // Roll the credits.
        VictoryEnd // Animating back to the main menu.
    }

    // Handles some animation durations
    // TODO: use time instead of frames
    private const int _fadeFrames = 90;
    protected int FadeFrame { get; set; } = _fadeFrames;

    // Handles graphics, drawing, and rendering
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _render;
    private Camera _gameCamera;
    private Camera _staticCamera;

    // Once we have menus and stuff, this should probably go to it's own class.
    private Song _backgroundMusic, _titleMusic, _gameOverMusic, _victoryMusic;

    private string _creditsText;
    private Size2 _creditsTextSize;
    private int _creditsScroll;
    private float _creditsScale;

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

    // Dogica font
    BitmapFont _bitmapFont;

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
        _gameCamera = new Camera(new BoxingViewportAdapter(
            Window,
            GraphicsDevice,
            TileRender.DEFAULT_WINDOW_SIZE.X,
            TileRender.DEFAULT_WINDOW_SIZE.Y
        ));
        _staticCamera = new Camera(new BoxingViewportAdapter(
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

        // Load music
        _titleMusic = Content.Load<Song>("Music/Escape");
        _backgroundMusic = Content.Load<Song>("Music/Sneak");
        _gameOverMusic = Content.Load<Song>("Music/Ded");
        _victoryMusic = Content.Load<Song>("Music/YoureWinner");

        // Load the main menu screen.
        _mainMenuScreen = Content.Load<Texture2D>("pixel/title");
        
        // Load the bitmap font
        _bitmapFont = Content.Load<BitmapFont>("fonts/dogica");

        // Load the credits information.
        // Despite the potential for people to make the game say silly things
        // by loading a raw text file, I *really* don't feel like running
        // it through the content pipeline.
        _creditsText = File.ReadAllText("CREDITS");
        _creditsTextSize = _bitmapFont.MeasureString(_creditsText);
        _creditsScale = 0.8f;

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
        else if (_gameState == GameState.GameOverEnd || _gameState == GameState.VictoryEnd)
        {
            --FadeFrame;
            if (FadeFrame <= 0)
            {
                _gameState = GameState.MainMenu;
                // Reset the animation counter
                FadeFrame = _fadeFrames;
            }
        }
        else if (_gameState == GameState.VictoryBegin)
        {
            --FadeFrame;
            if (FadeFrame <= 0)
            {
                _gameState = GameState.Victory;
                // Reset the animation counter
                FadeFrame = _fadeFrames;
            }
        }
        else if (_gameState == GameState.Victory) 
        {
            if (currentKey.GetPressedKeyCount() > 0)
            {
                // Reset game state
                Reset();
                // Start the animation before returning to the main menu.
                _gameState = GameState.VictoryCredits;
                _creditsScroll = 0;
            }
        }
        else if (_gameState == GameState.GameOver)
        {
            if (currentKey.GetPressedKeyCount() > 0)
            {
                // Reset game state
                Reset();
                // Start the animation before returning to the main menu.
                _gameState = GameState.GameOverEnd;
            }
        }
        else if (_gameState == GameState.VictoryCredits)
        {
            _creditsScroll++;
            if (_creditsScroll > (TileRender.BUFFER_SIZE.Y + _creditsTextSize.Height) * _creditsScale + 100)
            {
                _gameState = GameState.VictoryEnd;
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
            _gameCamera.MoveCamera(gameTime, pl);

            // Prioritize winning over losing
            if (pl.WonGame)
            {
                _gameState = GameState.VictoryBegin;
                MediaPlayer.Play(_victoryMusic);
                MediaPlayer.IsRepeating = false;
            }
            // Check player for game loss. If yes, change game state.
            else if (pl.LostGame)
            {
                _gameState = GameState.GameOverBegin;
                MediaPlayer.Play(_gameOverMusic);
                MediaPlayer.IsRepeating = false;
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

        if (_gameState == GameState.MainMenu)
        {
            // Start point clamped drawing based on game camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

            _spriteBatch.Draw(_mainMenuScreen, Vector2.Zero, Color.White);
            _spriteBatch.DrawString(
                _bitmapFont,                                // The bitmap font
                "Press any key to begin",                   // Text to display
                new Vector2(                                // Position
                    TileRender.BUFFER_SIZE.X / 2 - _bitmapFont.MeasureString("Press any key to begin").Width * 0.3f / 2,
                    TileRender.BUFFER_SIZE.Y - _bitmapFont.MeasureString("Press any key to begin").Height * 0.3f - 12
                ),
                Color.LightSalmon,                          // Text color/alpha
                0,                                          // Rotation
                Vector2.Zero,                               // Origin
                0.3f,                                       // Scale
                SpriteEffects.None,                         // Sprite effects
                0                                           // Layer depth
            );

            _spriteBatch.End();
        }
        else if (_gameState == GameState.GameOverBegin || _gameState == GameState.GameBegin || _gameState == GameState.VictoryBegin)
        {           
            if (_gameState == GameState.GameOverBegin || _gameState == GameState.VictoryBegin)
            {
                // Start point clamped drawing based on game camera view
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _gameCamera.ViewMatrix);

                // Handles drawing map based on camera's view
                _tiledMapRenderer.Draw(_gameCamera.ViewMatrix);

                _spriteBatch.End();
            }
            else if (_gameState == GameState.GameBegin)
            {
                // Start point clamped drawing based on game camera view
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

                _spriteBatch.Draw(_mainMenuScreen, Vector2.Zero, Color.White);

                _spriteBatch.End();
            }

            
            // Start point clamped drawing based on game camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

            int upperLeftRectX = (int)_staticCamera.Position.X - TileRender.BUFFER_SIZE.X / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int upperLeftRectY = (int)_staticCamera.Position.Y - TileRender.BUFFER_SIZE.Y / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int width = TileRender.BUFFER_SIZE.X * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            int height = TileRender.BUFFER_SIZE.Y * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
            _spriteBatch.FillRectangle(new RectangleF(upperLeftRectX, upperLeftRectY, width, height), Color.Black);

            _spriteBatch.End();
        }
        else if (_gameState == GameState.GameOver || _gameState == GameState.GameOverEnd)
        {
            // Start point clamped drawing based on static camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

            _spriteBatch.DrawString(
                _bitmapFont,                                // The bitmap font
                "YOU DIED",                                 // Text to display
                new Vector2(                                // Position
                    TileRender.BUFFER_SIZE.X / 2 - _bitmapFont.MeasureString("YOU DIED").Width / 2,
                    TileRender.BUFFER_SIZE.Y / 2 - _bitmapFont.MeasureString("YOU DIED").Height / 2
                ),
                Color.LightSalmon,                          // Text color/alpha
                0,                                          // Rotation
                Vector2.Zero,                               // Origin
                1f,                                         // Scale
                SpriteEffects.None,                         // Sprite effects
                0                                           // Layer depth
            );

            if (_gameState == GameState.GameOverEnd)
            {
                int upperLeftRectX = (int)_staticCamera.Position.X - TileRender.BUFFER_SIZE.X / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int upperLeftRectY = (int)_staticCamera.Position.Y - TileRender.BUFFER_SIZE.Y / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int width = TileRender.BUFFER_SIZE.X * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int height = TileRender.BUFFER_SIZE.Y * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                _spriteBatch.FillRectangle(new RectangleF(upperLeftRectX, upperLeftRectY, width, height), Color.Gray);
            }

            _spriteBatch.End();
        }
        else if (_gameState == GameState.Victory)
        {
            string exposition = "But alas, simply taking\n" +
                                "John Bozos's toilet is not enough.\n" +
                                "The Gammazon employees'\n" +
                                "productivity quotas still\n" +
                                "prevent them from having\n" +
                                "the chance to go to the\n" + 
                                "bathroom. To stop this,\n" +
                                "you must find John Bozos\n" +
                                "himself and stop him from\n" + 
                                "encouraging such brutality.\n" + 
                                "Maybe the next warehouse\n" + 
                                "will hold clues as to\n"+
                                "his whereabouts...";
            Size2 sz = _bitmapFont.MeasureString(exposition);
            float scale = 0.5f;
            // Start point clamped drawing based on static camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

            _spriteBatch.DrawString(
                _bitmapFont,                                // The bitmap font
                exposition,                                 // Text to display
                new Vector2(                                // Position
                    TileRender.BUFFER_SIZE.X / 2 - sz.Width * scale / 2,
                    TileRender.BUFFER_SIZE.Y / 2 - sz.Height * scale / 2
                ),
                Color.LightGreen,                           // Text color/alpha
                0,                                          // Rotation
                Vector2.Zero,                               // Origin
                scale,                                       // Scale
                SpriteEffects.None,                         // Sprite effects
                0                                           // Layer depth
            );

            _spriteBatch.End();
        }
        else if (_gameState == GameState.VictoryCredits || _gameState == GameState.VictoryEnd)
        {
            float scale = 0.8f;
            // Start point clamped drawing based on static camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _staticCamera.ViewMatrix);

            _spriteBatch.DrawString(
                _bitmapFont,                                // The bitmap font
                _creditsText,                               // Text to display
                new Vector2(                                // Position
                    TileRender.BUFFER_SIZE.X / 2 - _creditsTextSize.Width * scale / 2,
                    TileRender.BUFFER_SIZE.Y - _creditsScroll
                ),
                Color.LightSteelBlue,                       // Text color/alpha
                0,                                          // Rotation
                Vector2.Zero,                               // Origin
                scale,                                      // Scale
                SpriteEffects.None,                         // Sprite effects
                0                                           // Layer depth
            );

            if (_gameState == GameState.VictoryEnd)
            {
                int upperLeftRectX = (int)_staticCamera.Position.X - TileRender.BUFFER_SIZE.X / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int upperLeftRectY = (int)_staticCamera.Position.Y - TileRender.BUFFER_SIZE.Y / 2 * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int width = TileRender.BUFFER_SIZE.X * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                int height = TileRender.BUFFER_SIZE.Y * (_fadeFrames - FadeFrame) / (_fadeFrames - 10);
                _spriteBatch.FillRectangle(new RectangleF(upperLeftRectX, upperLeftRectY, width, height), Color.LightGray);
            }

            _spriteBatch.End();
        }
        else
        {
            // Start point clamped drawing based on game camera view
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _gameCamera.ViewMatrix);

            // Handles drawing map based on camera's view
            _tiledMapRenderer.Draw(_gameCamera.ViewMatrix);
            
            // Sort objects in the layer by draw priority, then Y position
            // This allows sprites to draw over each other based on which one "looks" in front
            _entities = _entities.OrderBy(entity => entity.DrawPriority)
                                .ThenBy(entity => entity.Position.Y)
                                .ToList();

            // Draw each entity
            _entities.ForEach(entity => { entity.Draw(_spriteBatch, true); });

            // End drawing
            _spriteBatch.End();
        }


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
