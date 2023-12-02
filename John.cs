using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace MonoGameJam5;

public class John : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _render;
    
    KeyboardState currentKey = new KeyboardState(), prevKey;
    
    // Test tilemap
    Map testMap;

    TiledMap _tiledMap;
    TiledMapRenderer _tiledMapRenderer;

    // Test sprite
    Texture2D testTile;
    Texture2D testConveyor;

    public John()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Allow user to resize window, add event handler
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += TileRender.WindowChanged;
    }

    protected override void Initialize()
    {
        // TODO: The map will need to be bigger than the viewport eventually.
        // We will make changes here when that happens.
        testMap = new Map(TileRender.BUFFER_TILE_DIMS);

        base.Initialize();

        // Set buffer size to default
        _graphics.PreferredBackBufferWidth = TileRender.DEFAULT_WINDOW_SIZE.X;
        _graphics.PreferredBackBufferHeight = TileRender.DEFAULT_WINDOW_SIZE.Y;
        _graphics.ApplyChanges();

        TileRender.WindowChanged(Window, null);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _render = new RenderTarget2D(GraphicsDevice, TileRender.BUFFER_SIZE.X, TileRender.BUFFER_SIZE.Y);

        testTile = Content.Load<Texture2D>("test");
        testConveyor = Content.Load<Texture2D>("pixel/boxsadnarrow");

        _tiledMap = Content.Load<TiledMap>("testmap");
        _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

        for(var y = 0; y < TileRender.BUFFER_TILE_DIMS.Y; y++)
        {
            for(var x = 0; x < TileRender.BUFFER_TILE_DIMS.X; x++)
            {
                testMap.AddToMap(new TestTile
                {
                    Image = testTile
                }, x, y, Tile.TileLayer.Floor);
                // For testing, one in 8 tiles will now also have a test conveyor
                if ((Random.Shared.Next() & 0x111) == 1)
                {
                    testMap.AddToMap(new Conveyor(testConveyor, (Random.Shared.Next() & 1) == 1, Conveyor.Direction.North, Random.Shared.Next() % 13),
                        x, y, Tile.TileLayer.Active);
                }
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Set previous key state and update current state
        prevKey = currentKey;
        currentKey = Keyboard.GetState();

        // Default exit on escape or back on Xbox controller
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

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

        _tiledMapRenderer.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render drawing to sprite buffer
        GraphicsDevice.SetRenderTarget(_render);
        GraphicsDevice.Clear(Color.Black);

        // Drawing begins here
        // _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

        // for (int l = 0; l < (int)Tile.TileLayer.NumLayers; ++l)
        // {
        //     foreach (Map.TilePos t in testMap.map[l])
        //     {
        //         _spriteBatch.Draw(
        //             t.tile.Image,
        //             new Rectangle(t.loc.X * TileRender.TILE_SIZE, t.loc.Y * TileRender.TILE_SIZE, TileRender.TILE_SIZE, TileRender.TILE_SIZE),
        //             new Rectangle(TileRender.TILE_SIZE * t.tile.X, TileRender.TILE_SIZE * t.tile.Y, TileRender.TILE_SIZE, TileRender.TILE_SIZE),
        //             Color.White
        //         );
        //     }
        // }

        // _spriteBatch.End();

        _tiledMapRenderer.Draw();

        // Set render target to device back buffer and clear
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        // Draw sprite buffer to back buffer
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        _spriteBatch.Draw((Texture2D)_render, TileRender.renderDims, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
