using System;
using Microsoft.Xna.Framework;

namespace Lucky_Fighters
{
    public abstract class Interactive : Tile
    {
        public bool IsEnabled { get; protected set; } = true;

        protected Interactive(string tileSheetName, int tileSheetIndex) : base(tileSheetName,
            tileSheetIndex, TileCollision.Passable)
        {
        }

        internal abstract void ApplyEffect(Player player);
    }
}