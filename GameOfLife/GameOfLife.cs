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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D aliveCell;
        private bool[,] activeCells;
        private bool[,] previousCells;
        private int height;
        private int width;
        private int offScreenBuffer;
        private int offsetY;
        private int offsetX;
        private int cellSize;
        private bool autoPlay;
        private KeyboardState prevKeyboardState;
        private MouseState prevMouseState;

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
            // Set size for a cell
            cellSize = 16;

            // Set the amount of cells exceeding the screen
            offScreenBuffer = 60;

            // Initialize a 2d texture representing an alive cell
            Color[] data = new Color[cellSize * cellSize];
            aliveCell = new Texture2D(GraphicsDevice, cellSize, cellSize);

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;
            aliveCell.SetData(data);

            // Intialize the arrays holding the current state of the board
            // Array bounds exceed the visible screen so the game behaves properly
            // at the borders of the screen
            height = Window.ClientBounds.Height / cellSize;
            width = Window.ClientBounds.Width / cellSize;
            activeCells = new bool[width + offScreenBuffer, height + offScreenBuffer];
            previousCells = new bool[width + offScreenBuffer, height + offScreenBuffer];

            // Calculate offsets so the arrays are centered in the screen
            offsetY = offScreenBuffer / 2;
            offsetX = offScreenBuffer / 2;

            // Set auto play to false
            autoPlay = false;

            // Read first keyboard state
            prevKeyboardState = Keyboard.GetState();

            // Make mouse visible
            IsMouseVisible = true;

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
            // Get current mouse state
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.X / cellSize;
            int mouseY = mouseState.Y / cellSize;

            // Get current keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            // Invert cell if left mouse button is clicked
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                if (0 <= mouseX && mouseX < width && 0 <= mouseY && mouseY < height)
                activeCells[mouseX + offsetX, mouseY + offsetY] = !activeCells[mouseX + offsetX, mouseY + offsetY];

            // Exit if enter is pressed
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle auto play if space is pressed
            if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
                autoPlay = !autoPlay;

            // If 'r' is pressed  random fill the board
            if (keyboardState.IsKeyDown(Keys.R) && prevKeyboardState.IsKeyUp(Keys.R))
                RandomFill();

            // If 'x' is pressed call kill all cells
            if (keyboardState.IsKeyDown(Keys.X) && prevKeyboardState.IsKeyUp(Keys.X))
                Exterminate();

            // Update game if auto play is active or 's' is pressed
            if (autoPlay || (keyboardState.IsKeyDown(Keys.S) && prevKeyboardState.IsKeyUp(Keys.S)))
            {
                UpdateGame();
                Thread.Sleep(10);
            }

            // Save keyboard and mouse state for the next update
            prevKeyboardState = keyboardState;
            prevMouseState = mouseState;
 
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    spriteBatch.Draw(aliveCell, new Vector2(i * cellSize, j * cellSize),
                                     activeCells[i + offsetX, j + offsetY] ? Color.White : Color.Black);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private int AliveNeighbours(int x, int y)
        {
            // Return the amount of living cells next to the given cell
            int neighbours = 0;
            for (int i = Math.Max(x - 1, 0); i < Math.Min(x + 2, previousCells.GetLength(0)); i++)
            {
                for (int j = Math.Max(y - 1, 0); j < Math.Min(y + 2, previousCells.GetLength(1)); j++)
                {
                    neighbours += previousCells[i, j] ? 1 : 0;
                }
            }
            return neighbours - (previousCells[x, y] ? 1 : 0);
        }

        private void UpdateGame()
        {
            // Execute a game step according to Conway's rules
            bool[,] temp = activeCells;
            activeCells = previousCells;
            previousCells = temp;

            for (int i = 0; i < activeCells.GetLength(0); i++)
            {
                for (int j = 0; j < activeCells.GetLength(1); j++)
                {
                    int neighbours = AliveNeighbours(i, j);
                    if (neighbours < 2)
                        activeCells[i, j] = false;
                    else if (neighbours > 3)
                        activeCells[i, j] = false;
                    else if (neighbours == 3)
                        activeCells[i, j] = true;
                    else
                        activeCells[i, j] = previousCells[i, j];
                }
            }
        }

        private void RandomFill()
        {
            // Random fill the board with living cells
            Random random = new Random();
            for (int i = 0; i < activeCells.GetLength(0); i++)
            {
                for (int j = 0; j < activeCells.GetLength(1); j++)
                {
                    if (random.NextDouble() < 0.5)
                        activeCells[i, j] = true;
                }
            }
        }

        private void Exterminate()
        {
            // Kill every cell
            for (int i = 0; i < activeCells.GetLength(0); i++)
            {
                for (int j = 0; j < activeCells.GetLength(1); j++)
                {
                    activeCells[i, j] = false;
                }
            }
        }
    }
}
