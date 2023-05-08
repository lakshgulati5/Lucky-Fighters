using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class Map : Screen
    {
        string[] teamColorStrings = new string[] { "Blue", "Red", "Green", "Yellow" };
        private Tile[,] tiles;
        public Dictionary<string, Texture2D> tileSheets { get; }
        public List<Rectangle> TileDefinitions;

        SpriteFont font;

        public Dictionary<Vector2, Interactive> Interactives { get; } = new Dictionary<Vector2, Interactive>();

        // Any powerup that can spawn (non hard-coded)
        enum PowerupTypes
        {
            Shield,
            Wings
        }

        Player[] players;
        string[] fighters;
        int[] teams;
        bool ready;
        public Player winner;
        public bool quitting;
        Mode mode;
        KeyboardState oldKb;

        // holds the starting point for the level for each player
        private Vector2[] starts;
        public Dictionary<int, Rectangle> TileSourceRecs;

        // contains AnimatedSprites other than the player (e.g. arrows, tundra breeze)
        List<AnimatedSprite> otherSprites;

        SoundEffectInstance backgroundMusic;

        private const int TileWidth = 96;
        private const int TileHeight = 96;
        private const int TilesPerRow = 5;
        private const int NumRowsPerSheet = 5;

        private Random random = new Random(1337);

        public bool paused;
        public PlayerIndex pausedBy;

        List<Task> tasks;

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        public string winningTeam
        {
            get;
            private set;
        }

        public Map(IServiceProvider _serviceProvider, string path, string[] fighters, int[] teams, Mode mode) : base(_serviceProvider)
        {

            // load the textures
            tileSheets = new Dictionary<string, Texture2D>();
            tileSheets.Add("Blocks", Content.Load<Texture2D>("Tiles/Blocks"));
            tileSheets.Add("Platforms", Content.Load<Texture2D>("Tiles/Platforms"));

            //TODO: Make 1 sprite sheet for interactives
            tileSheets.Add("Flower", Content.Load<Texture2D>("Interactives/Flower"));
            tileSheets.Add("Shield", Content.Load<Texture2D>("Interactives/Shield"));
            tileSheets.Add("Wings", Content.Load<Texture2D>("Interactives/Wings"));

            this.fighters = fighters;
            players = new Player[fighters.Length];
            starts = new Vector2[fighters.Length];
            this.teams = teams;
            otherSprites = new List<AnimatedSprite>();
            this.mode = mode;

            tasks = new List<Task>();

            // lives = new int[fighters.Length];
            // create a collection of source rectangles.
            TileSourceRecs = new Dictionary<int, Rectangle>();
            for (int i = 0; i < TilesPerRow * NumRowsPerSheet; i++)
            {
                Rectangle rectTile = new Rectangle(
                    (i % TilesPerRow) * TileWidth,
                    (i / TilesPerRow) * TileHeight,
                    TileWidth,
                    TileHeight);
                TileSourceRecs.Add(i, rectTile);
            }

            LoadTiles(path);
        }

        public override void LoadContent()
        {
            font = this.Content.Load<SpriteFont>("Big");
            backgroundMusic = Content.Load<SoundEffect>("Sound/luckyfighterstheme").CreateInstance();
            backgroundMusic.IsLooped = true;
            backgroundMusic.Play();
        }

        private void LoadTiles(string path)
        {
            // Load the level and ensure all of the lines are the same length.
            int numOfTilesAcross = 0;
            List<string> lines = new List<string>();
            try
            {
                // Create an instance of StreamReader to read form a file.
                // The using statement also closes the StreamReader.
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = reader.ReadLine();
                    numOfTilesAcross = line.Length;
                    while (line != null)
                    {
                        lines.Add(line);
                        int nextLineWidth = line.Length;
                        if (nextLineWidth != numOfTilesAcross)
                            throw new Exception(String.Format(
                                "The length of line {0} is different from all preceeding lines.",
                                lines.Count));
                        line = reader.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }

            // Allocate the tile grid.
            tiles = new Tile[numOfTilesAcross, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    string currentRow = lines[y];
                    char tileType = currentRow[x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }
        }

        private Tile LoadTile(char _tileType, int _x, int _y)
        {
            switch (_tileType)
            {
                // Blank space
                case '.':
                    return Tile.Empty;

                // blocks and platforms will have this format:
                // e.g. platforms: q is [], w is [=, e is ==, r is =], where [ is a border and = is open

                // Platform blocks range from 'q' to 'r'
                case 'q':
                    return LoadVarietyTile("Platforms", 0);
                case 'w':
                    return LoadVarietyTile("Platforms", 1);
                case 'e':
                    return LoadVarietyTile("Platforms", 3);
                case 'r':
                    return LoadVarietyTile("Platforms", 2);

                // Impassable block
                case 'b':
                    return LoadVarietyTile("Blocks", 0);

                // player start point
                case '1':
                    return LoadStartTile(_x, _y, PlayerIndex.One);
                case '2':
                    return LoadStartTile(_x, _y, PlayerIndex.Two);
                case '3':
                    return LoadStartTile(_x, _y, PlayerIndex.Three);
                case '4':
                    return LoadStartTile(_x, _y, PlayerIndex.Four);

                // interactives
                // powerups (touching it automatically uses, randomly generated)
                case 'i':
                    return LoadPowerupTile(_x, _y);
                // still interactives (same position, regenerate after time)
                case 'f':
                    return LoadInteractiveTile(_x, _y, new Flower());

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format(
                        "Unsupported tile type character {0} at position {1}, {2},", _tileType, _x, _y));
            }
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int _x, int _y, PlayerIndex index)
        {
            if ((int)index >= fighters.Length)
                return Tile.Empty;
            if (players[(int)index] != null)
                throw new NotSupportedException("A level may only have one starting point");

            Vector2 start = new Vector2((_x * Tile.Width) + 48, (_y * Tile.Height) + 16);
            starts[(int)index] = start;
            switch (fighters[(int)index])
            {
                case "swordfighter":
                    players[(int)index] = new SwordFighter(this, start, index, teams[(int)index]);

                    break;
                case "archer":
                    players[(int)index] = new Archer(this, start, index, teams[(int)index]);
                    break;
                /*
                case "ninja":
                    players[(int)index] = new Ninja(this, start, index, 0);
                    break;
                case "wizard":
                    players[(int)index] = new Wizard(this, start, index, 0);
                    break;
                case "muscleman":
                    players[(int)index] = new Muscleman(this, start, index, 0);
                    break;
                */
            }

            // lives[(int)index] = 3;
            // players[(int)index].lives = 3;

            return Tile.Empty;
        }

        private Tile LoadVarietyTile(string _tileSheetName, int index)
        {
            // get index on tile to rect dictionary
            switch (_tileSheetName)
            {
                case "Blocks":
                    return new Tile(_tileSheetName, index, TileCollision.Impassable);
                case "Platforms":
                    return new Tile(_tileSheetName, index, TileCollision.Platform);
            }

            return new Tile(_tileSheetName, index, TileCollision.Passable);
        }

        /// <summary>
        /// Load a tile for interactives that require a keypress
        /// </summary>
        private Interactive LoadInteractiveTile(int x, int y, Interactive interactive)
		{
            return Interactives[new Vector2(x, y) * Tile.Size] = interactive;
        }

        public Interactive LoadPowerupTile(int x, int y)
		{
            return LoadInteractiveTile(x, y, GetRandomPowerup());
		}

        /// <summary>
        /// Gets a random powerup interactive
        /// </summary>
        public Interactive GetRandomPowerup()
		{
            switch ((PowerupTypes)SafeRandom.Next(0, Enum.GetValues(typeof(PowerupTypes)).Length))
            {
                case PowerupTypes.Shield:
                    return new Shield();
                case PowerupTypes.Wings:
                    return new Wings();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RegeneratePowerup(Vector2 key)
		{
            Interactives.Remove(key);
            int tileX = (int)key.X / 96, tileY = (int)key.Y / 96;
            tiles[tileX, tileY] = Tile.Empty;
            AddTask(new Task(random.Next(20, 30), () =>
            {
                tiles[tileX, tileY] = LoadPowerupTile(tileX, tileY);
            }));
        }

        public TileCollision GetCollision(int _x, int _y)
        {
            if (_x < 0 || _x >= Width)
                return TileCollision.Impassable;
            if (_y < 0 || _y >= Height)
                return TileCollision.Passable;

            return tiles[_x, _y].Collision;
        }

        public Rectangle GetBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
            //if (tiles[x, y].Collision == TileCollision.Platform)
            //    return new Rectangle(x * Tile.Width, (y * Tile.Height) + 20, Tile.Width, Tile.Height - 20);
            return new Rectangle(x * Tile.Width, (y * Tile.Height) + 5, Tile.Width, Tile.Height - 5);
        }

        public List<Player> GetCollidingPlayers(Rectangle hitbox)
        {
            List<Player> touchingPlayers = new List<Player>();
            foreach (Player player in players)
            {
                if (player.Hitbox.Intersects(hitbox))
                {
                    touchingPlayers.Add(player);
                }
            }

            return touchingPlayers;
        }

        public void AddSprite(AnimatedSprite sprite)
        {
            otherSprites.Add(sprite);
        }

        public override void Update(GameTime _gameTime)
        {
            bool someoneAlive = false;
            bool multipleAlive = false;
            int x = 0;
            KeyboardState kb = Keyboard.GetState();

            if (!paused)
            {
                foreach (AnimatedSprite sprite in otherSprites)
                {
                    sprite.Update(_gameTime);
                }

                otherSprites.RemoveAll(s => s.ShouldRemove);

                // Update tasks (moved from Player class to Map)
                float elapsed = _gameTime.GetElapsedSeconds();
                for (int i = 0; i < tasks.Count; i++)
                {
                    Task task = tasks[i];
                    task.Update(elapsed);
                    if (task.IsCompleted)
                    {
                        task.WhenCompleted();
                        // check again to make sure all Then() connected tasks are done
                        if (task.IsCompleted)
                        {
                            tasks.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            for (; x < players.Length; x++)
            {
                Player player = players[x];
                PlayerIndex playerIndex = player.playerIndex;
                GamePadState gamePad = player.GetGamePad();
                GamePadState oldGamePad = player.oldGamePad;
                //pause game
                if (paused)
                {
                    //can only be unpaused by the player who paused the game
                    if (pausedBy == playerIndex)
                    {
                        //resume game
                        if (gamePad.Buttons.Start == ButtonState.Pressed && oldGamePad.Buttons.Start == ButtonState.Released ||
                            playerIndex == PlayerIndex.One && kb.IsKeyDown(Keys.Escape) && oldKb.IsKeyUp(Keys.Escape))
                        {
                            paused = false;
                            quitting = false;
                        }

                        //quit game
                        if (gamePad.Buttons.RightShoulder == ButtonState.Pressed &&
                            gamePad.Buttons.LeftShoulder == ButtonState.Pressed)
                        {
                            InitializeQuit();
                        }

                        if (quitting)
                        {
                            if (gamePad.Buttons.A == ButtonState.Pressed)
                                Quit();
                            else if (gamePad.Buttons.B == ButtonState.Pressed)
                                CancelQuit();
                        }
                    }
                    player.oldGamePad = gamePad;
                }
                else
                {
                    player.Update(_gameTime);
                    if (player.IsCompletelyDead && player.lives > 0)
                        player.Reset(starts[x]);
                    if (player.IsDead)
                        OnPlayerKilled(x);
                    

                    //pause only by players that are still in the game
                    if (gamePad.Buttons.Start == ButtonState.Pressed && oldGamePad.Buttons.Start == ButtonState.Released ||
                        playerIndex == PlayerIndex.One && kb.IsKeyDown(Keys.Escape) && oldKb.IsKeyUp(Keys.Escape))
                    {
                        paused = true;
                        pausedBy = playerIndex;
                    }
                }

                // Check if players are alive
                if (mode == Mode.Solo)
                {
                    if (player.lives > 0)
                    {
                        if (someoneAlive)
                            multipleAlive = true;
                        else
                        {
                            someoneAlive = true;
                            winner = player;
                        }
                    }
                }
            }


            if (mode == Mode.Team)
            {
                int[] amtAlive = new int[4];
                for (x = 0; x < teams.Length; x++)
                {
                    if (teams[x] != -1)
                        amtAlive[teams[x]]++;
                }
                for (x = 0; x < amtAlive.Length; x++)
                {
                    if (amtAlive[x] > 0)
                    {
                        if (someoneAlive)
                            multipleAlive = true;
                        else
                        {
                            someoneAlive = true;
                            winningTeam = teamColorStrings[x];
                        }
                    }
                }
            }

            if (!multipleAlive)
            {
                ready = true;
                end = GameEnd.Win;
            }

            oldKb = kb;
        }

        public void AddTask(Task task)
        {
            tasks.Add(task);
        }


        public void OnPlayerKilled(int index)
        {
            Player player = players[index];
            if (player.StartedRespawning)
                return;

            player.OnKilled();
            // lives[index] -= 1;
            player.lives -= 1;
            player.Stats.LivesLost++;
            if (player.lives < 1)
                teams[index] = -1;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
			DrawTiles(spriteBatch);
			foreach (Player player in players)
            {
                // if (lives[(int)player.playerIndex] > 0) player.Draw(spriteBatch, gameTime);
                if (player.lives > 0 || !player.IsCompletelyDead)
                    player.Draw(spriteBatch, gameTime);
            }

            foreach (AnimatedSprite sprite in otherSprites)
            {
                sprite.Draw(spriteBatch, gameTime);
            }

            if (paused)
            {
                spriteBatch.DrawString(font, "Paused by Player " + pausedBy + "\nQuit- [LB] + [RB]",
                    new Vector2(300, 200), Color.White);
                if (quitting)
                    spriteBatch.DrawString(font, "Quit?\nYes- A\nNo- B", new Vector2(300, 375), Color.White);
            }
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    if (!tileSheets.TryGetValue(tiles[x, y].TileSheetName, out var tileSheet)) continue;


                    // Draw it in screen space.
                    var position = new Vector2(x, y) * Tile.Size;


					if (Interactives.TryGetValue(position, out var interactive))
					{
						if (!interactive.IsEnabled) continue;
					}


					spriteBatch.Draw
                    (
                        tileSheet,
                        position,
                        TileSourceRecs[tiles[x, y].TileSheetIndex],
                        Color.White
                    );
                }
            }
        }

        public void InitializeQuit()
        {
            quitting = true;
        }

        public void Quit()
        {
            ready = true;
            end = GameEnd.Quit;
        }

        public void CancelQuit()
        {
            quitting = false;
        }

        // used to store how the game was ended and determine what the result screen displays
        public enum GameEnd
        {
            Quit,
            Win
        }

        public GameEnd end { get; private set; }

        public override bool ReadyForNextScreen()
        {
            return ready;
        }

        public override Color GetColor()
        {
            return Color.CornflowerBlue;
        }

        public Player.CombatStats[] GetCombatStats()
		{
            Player.CombatStats[] stats = new Player.CombatStats[players.Length];
            for (int i = 0; i < players.Length; i++)
                stats[i] = players[i].Stats;
            return stats;
		}
    }
}