using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;

namespace GameOfLife
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameOfLife : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D aliveCell;
        bool[,] activeCells;
        bool[,] previousCells;
        int height;
        int width;
        bool autoPlay;
        bool wasPressed;
        bool wasReleased;

        public GameOfLife()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize a 2d texture representing an alive cell
            Color[] data = new Color[16 * 16];
            aliveCell = new Texture2D(GraphicsDevice, 16, 16);

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;
            aliveCell.SetData(data);

            // Intialize the arrays holding the current state of the board
            height = Window.ClientBounds.Height / 16;
            width = Window.ClientBounds.Width / 16;
            activeCells = new bool[width , height];
            previousCells = new bool[width , height];
            RandomFill();

            // Set auto play to false
            autoPlay = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Current keyboard state
            KeyboardState state = Keyboard.GetState();

            // Exit if enter is pressed
            if (state.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle auto play if space is pressed
            if (state.IsKeyDown(Keys.Space))
                autoPlay = !autoPlay;
            
            if (state.IsKeyDown(Keys.S))
            {
                wasPressed = true;
                wasReleased = false;
            }

            // Update game if auto play is active or 's' is pressed
            if (autoPlay || (wasPressed && wasReleased))
            {
                UpdateGame();
                Thread.Sleep(10);
                wasPressed = false;
            }

            // If 's' was released set flag to true
            if (state.IsKeyUp(Keys.S))
            {
                wasReleased = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            for (int i = 0; i < activeCells.GetLength(0); i++)
            {
                for (int j = 0; j < activeCells.GetLength(1); j++)
                {
                    spriteBatch.Draw(aliveCell, new Vector2(i * 16, j * 16), activeCells[i, j] ? Color.White : Color.Black);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private int AliveNeighbours(int x, int y)
        {
            int neighbours = 0;
            for (int i = Math.Max(x - 1, 0); i < Math.Min(x + 2, width); i++)
            {
                for (int j = Math.Max(y - 1, 0); j < Math.Min(y + 2, height); j++)
                {
                    neighbours += previousCells[i, j] ? 1 : 0;
                }
            }
            return neighbours - (previousCells[x, y] ? 1 : 0);
        }

        private void UpdateGame()
        {
            bool[,] temp = activeCells;
            activeCells = previousCells;
            previousCells = temp;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int neighbours = AliveNeighbours(i, j);
                    if (neighbours < 2)
                    {
                        activeCells[i, j] = false;
                    }
                    else if (neighbours > 3)
                    {
                        activeCells[i, j] = false;
                    }
                    else if (neighbours == 3)
                    {
                        activeCells[i, j] = true;
                    }
                    else
                    {
                        activeCells[i, j] = previousCells[i, j];
                    }
                }
            }
        }

        private void RandomFill()
        {
            Random random = new Random();
            for (int i = 0; i < activeCells.GetLength(0); i++)
            {
                for (int j = 0; j < activeCells.GetLength(1); j++)
                {
                    activeCells[i, j] = random.NextDouble() < 0.5;
                }
            }
        }
    }
}
