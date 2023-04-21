using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class Map : Screen, IDisposable
    {
        private Tile[,] tiles;
        public Dictionary<string, Texture2D> tileSheets { get; }
        public List<Rectangle> TileDefinitions;

        public Dictionary<Vector2, Interactive> Interactives { get; } = new Dictionary<Vector2, Interactive>();
        Player[] players;
        string[] fighters;
        bool ready;
        public Player winner;

        // holds the starting point for the level for each player
        private Vector2[] starts;
        public Dictionary<int, Rectangle> TileSourceRecs;

        public ContentManager Content { get; }

        private const int TileWidth = 96;
        private const int TileHeight = 96;
        private const int TilesPerRow = 5;
        private const int NumRowsPerSheet = 5;

        private Random random = new Random(1337);

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        public Map(IServiceProvider _serviceProvider, string path, string[] fighters)
        {
            // Create a new content manager to load content used just by this level.
            Content = new ContentManager(_serviceProvider, "Content");

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

                case 'f':
                case 's':
                case 'n':
                    return LoadInteractiveTile(_x, _y, _tileType);


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
                    players[(int)index] = new SwordFighter(this, start, index, (int)index);

                    break;
                /*
            case "archer":
                players[(int)index] = new Archer(this, start, index, 0);
                break;
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

        private Interactive LoadInteractiveTile(int x, int y, char type)
        {
            switch (type)
            {
                case 'f':
                {
                    var flowerTile = new Flower();
                    Interactives[new Vector2(x, y) * Tile.Size] = flowerTile;
                    return flowerTile;
                }
                case 's':
                {
                    var shieldTile = new Shield();
                    Interactives[new Vector2(x, y) * Tile.Size] = shieldTile;
                    return shieldTile;
                }
                case 'n':
                {
                    var wingTile = new Wings();
                    Interactives[new Vector2(x, y) * Tile.Size] = wingTile;
                    return wingTile;
                }
            }

            return new Flower();
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

        public override void Update(GameTime _gameTime)
        {
            bool someoneAlive = false;
            bool multipleAlive = false;
            int x = 0;


            foreach (Player player in players)
            {
                player.Update(_gameTime);
                if (player.IsCompletelyDead && player.lives > 0) player.Reset(starts[x]);
                if (player.IsDead) OnPlayerKilled(x);
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

                x++;
            }

            if (!multipleAlive)
                ready = true;
        }


        public void OnPlayerKilled(int index)
        {
            Player player = players[index];
            if (player.StartedRespawning)
                return;

            player.OnKilled();
            // lives[index] -= 1;
            player.lives -= 1;
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

        public void Dispose()
        {
            Content.Unload();
        }

        public override bool ReadyForNextScreen()
        {
            return ready;
        }

        public override Color GetColor()
        {
            return Color.CornflowerBlue;
        }
    }
}