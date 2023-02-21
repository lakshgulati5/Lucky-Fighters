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
    class Map : IDisposable
    {
        private Tile[,] tiles;
        private Dictionary<string, Texture2D> tileSheets;
        public List<Rectangle> TileDefinitions;

        
        List<Player> players;

        // holds the starting point for the level
        private List<Vector2> starts;
        public Dictionary<int, Rectangle> TileSourceRecs;

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private const int TileWidth = 64;
        private const int TileHeight = 64;
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

        public Map(IServiceProvider _serviceProvider, string path)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(_serviceProvider, "Content");

            // load the textures
            tileSheets = new Dictionary<string, Texture2D>();
            //tileSheets.Add("Blocks", Content.Load<Texture2D>("Tiles/Blocks"));
            //tileSheets.Add("Platforms", Content.Load<Texture2D>("Tiles/Platforms"));


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
                    return new Tile(String.Empty, 0, TileCollision.Passable);

                // Platform blocks
                case 'p':
                    return LoadVarietyTile("Platforms", 0, 5);

                // Impassable block
                case 'b':
                    return LoadVarietyTile("Blocks", 0, 5);

                // player start point
                case '1':
                    return LoadStartTile(_x, _y, 1);
                case '2':
                    return LoadStartTile(_x, _y, 2);
                case '3':
                    return LoadStartTile(_x, _y, 3);
                case '4':
                    return LoadStartTile(_x, _y, 4);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format(
                        "Unsupported tile type character {0} at position {1}, {2},", _tileType, _x, _y));
            }
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int _x, int _y, int index)
        {
            if (players[index] != null)
                throw new NotSupportedException("A level may only have one starting point");

            starts[index] = new Vector2((_x * 64) + 48, (_y * 64) + 16);
            player[index] = new Player(this, starts[index]);

            return new Tile(String.Empty, 0, TileCollision.Passable);
        }

        private Tile LoadVarietyTile(string _tileSheetName, int _colorRow, int _variationCount)
        {
            int index = random.Next(_variationCount);
            // get index on tile to rect dicitonary
            int tileSheetIndex = _colorRow + index;
            switch (_tileSheetName)
            {
                case "Blocks":
                    return new Tile(_tileSheetName, tileSheetIndex, TileCollision.Impassable);
                case "Platforms":
                    return new Tile(_tileSheetName, tileSheetIndex, TileCollision.Platform);
            }
            return new Tile(_tileSheetName, tileSheetIndex, TileCollision.Passable);
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

        public void Update(GameTime _gameTime)
        {
            player.Update(_gameTime);
            if (player.IsCompletelyDead)
                player.Reset(start);
        }


        public void OnPlayerKilled(int index)
        {
            players[index].OnKilled();
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawTiles(spriteBatch);
            foreach (Player player in players)
                player.Draw(gameTime, spriteBatch);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    if (tileSheets.ContainsKey(tiles[x, y].TileSheetName))
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(tileSheets[tiles[x, y].TileSheetName],
                            position,
                            TileSourceRecs[tiles[x, y].TileSheetIndex],
                            Color.White);
                    }
                }
            }
        }

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
