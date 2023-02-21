using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Lucky_Fighters
{
    /// <summary>
    /// Controls the collision detection and the response behavior of a tile.
    /// </summary>
    public enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through 
        /// it at all. It is completely solid 
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the 
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but cannot fall down through the top of it
        /// </summary>
        Platform = 2,
    }
    class Tile
    {
        // name of the sprite sheet
        public string TileSheetName;
        // the index of the rectangle for sourceRect for drawing.
        public int TileSheetIndex;

        public TileCollision Collision;

        public const int Width = 96;
        public const int Height = 96;
        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(string tileSheetName, int tileSheetIndex, TileCollision collision)
        {
            TileSheetName = tileSheetName;
            TileSheetIndex = tileSheetIndex;
            Collision = collision;
        }
    }
}
