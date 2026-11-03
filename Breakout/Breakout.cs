using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace breakout_clone.Breakout;

public static class Globals
{
    public static float Time { get; private set; }
    public static ContentManager ContentManager { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static Point GameWindow { get; private set; }

    public static void Initialize(ContentManager contentManager, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        ContentManager = contentManager;
        SpriteBatch = spriteBatch;
        GraphicsDevice = graphicsDevice;
        GameWindow = new(600, 900);
    }

    public static void Update(GameTime gameTime) =>
        Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
}

public class InputManager
{
    public static InputManager Instance { get; private set; }
    private KeyboardState _currentKeyboardState;

    private InputManager() { }

    public static void Initialize()
    {
        if (Instance is null) Instance = new();
    }

    public static void Update()
    {
        if (Instance is null) return;
        Instance._currentKeyboardState = Keyboard.GetState();
    }

    public static bool IsKeyDown(Keys key)
    {
        if (Instance is null) return false;
        return Instance._currentKeyboardState.IsKeyDown(key);
    }
}

public class GameManager
{
    private Player _player;
    private Ball _ball;
    private List<Brick> Bricks;

    public GameManager()
    {
        InputManager.Initialize();
        _player = new(new(Globals.GameWindow.X / 2, Globals.GameWindow.Y - 100));
        _ball = new(new(Globals.GameWindow.X / 2, Globals.GameWindow.Y / 2), new(0, 1));
        Bricks = new();
        InitBricks();
    }

    public void Update()
    {
        InputManager.Update();
        _player.Update();
        _ball.Update();

        foreach (Brick brick in Bricks)
            brick.Update();

        CheckCollisions();
    }

    public void Draw()
    {
        _player.Draw();
        _ball.Draw();

        foreach (Brick brick in Bricks)
            brick.Draw();
    }

    private void CheckCollisions()
    {
        if (_ball.Bounds.Intersects(_player.Bounds))
        {
            float hitFactor = (_ball.Position.X - _player.Position.X) / (_player.Width / 2f);
            hitFactor = Math.Clamp(hitFactor, -1, 1);

            Vector2 direction = new(hitFactor, -1f);
            direction.Normalize();

            _ball.Direction = direction;
            _ball.Speed += 10f;
        }

        for (int i = 0; i < Bricks.Count; i++)
        {
            Brick brick = Bricks[i];

            if (_ball.Bounds.Intersects(brick.Bounds))
            {
                Bricks.RemoveAt(i);
                _ball.Direction = new(_ball.Direction.X, -_ball.Direction.Y);   
                _ball.Speed += 10f;
            }
        }
    }

    private void InitBricks()
    {
        int columns = 7;
        int rows = 6;

        int brickWidth = 70;
        int brickHeight = 20;
        int spacing = 10;

        int startX = 60;
        int startY = 50;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                int x = startX + i * (brickWidth + spacing);
                int y = startY + j * (brickHeight + spacing);

                Brick brick = new(new(x, y));
                Bricks.Add(brick);
            }
        }
    }
}

public class Player
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position;
    public Vector2 Direction { get; private set; }
    public Rectangle Bounds { get; private set; }
    public float Speed { get; private set; }
    public int Width => _width;

    private int _width = 100;
    private int _height = 20;

    public Player(Vector2 position)
    {
        Texture = new(Globals.GraphicsDevice, 1, 1);
        Texture.SetData([Color.White]);
        Position = position;
        Direction = Vector2.Zero;
        Speed = 700f;
    }

    public void Update()
    {
        HandleMovement();
        Position += Direction * Speed * Globals.Time;

        UpdateBounds();
        CheckCollisions();
    }

    public void Draw() =>
        Globals.SpriteBatch.Draw(Texture, Position, Bounds, Color.White, 0f, new(Bounds.Width / 2, Bounds.Height / 2), 1f, SpriteEffects.None, 0f);

    private void UpdateBounds() =>
        Bounds = new((int)Position.X - _width / 2, (int)Position.Y - _height / 2, _width, _height);

    private void HandleMovement()
    {
        Direction = Vector2.Zero;

        if (InputManager.IsKeyDown(Keys.Left)) Direction = new(-1, 0);
        if (InputManager.IsKeyDown(Keys.Right)) Direction = new(1, 0);
        if (Direction != Vector2.Zero) Direction.Normalize();
    }

    private void CheckCollisions()
    {
        if (Position.X + Bounds.Width / 2 >= Globals.GameWindow.X) Position.X = Globals.GameWindow.X - Bounds.Width / 2;
        if (Position.X - Bounds.Width / 2 <= 0) Position.X = Bounds.Width / 2;
    }
}

public class Ball
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector2 Direction { get; set; }
    public Rectangle Bounds { get; private set; }
    public float Speed { get; set; }

    private int _width = 20;
    private int _height = 20;

    public Ball(Vector2 position, Vector2 direction)
    {
        Texture = new(Globals.GraphicsDevice, 1, 1);
        Texture.SetData([Color.White]);
        Position = position;
        Direction = direction;
        Speed = 400f;
    }

    public void Update()
    {
        Position += Direction * Speed * Globals.Time;
        UpdateBounds();
        CheckCollisions();
    }

    public void Draw() =>
        Globals.SpriteBatch.Draw(Texture, Position, Bounds, Color.White, 0f, new(Bounds.Width / 2, Bounds.Height / 2), 1f, SpriteEffects.None, 0f);

    private void UpdateBounds() =>
        Bounds = new((int)Position.X - _width / 2, (int)Position.Y - _height / 2, _width, _height);

    private void CheckCollisions()
    {
        if (Position.Y - Bounds.Height / 2 <= 0) Direction = new(Direction.X, -Direction.Y);
        if (Position.X + Bounds.Width / 2 >= Globals.GameWindow.X) Direction = new(-Direction.X, Direction.Y);
        if (Position.X - Bounds.Width / 2 <= 0) Direction = new(-Direction.X, Direction.Y);
    }
}

public class Brick
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position { get; private set; }
    public Rectangle Bounds { get; private set; }

    private int _width = 70;
    private int _height = 20;

    public Brick(Vector2 position)
    {
        Texture = new(Globals.GraphicsDevice, 1, 1);
        Texture.SetData([Color.White]);
        Position = position;
        Bounds = new((int)Position.X - _width / 2, (int)Position.Y - _height / 2, _width, _height);
    }

    public void Update()
    {
        
    }

    public void Draw() =>
        Globals.SpriteBatch.Draw(Texture, Position, Bounds, Color.White, 0f, new(Bounds.Width / 2, Bounds.Height / 2), 1f, SpriteEffects.None, 0f);
}