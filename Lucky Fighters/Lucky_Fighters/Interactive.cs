using System;
using Microsoft.Xna.Framework;

namespace Lucky_Fighters
{
    public abstract class Interactive : Tile
    {
        public bool IsEnabled { get; protected set; } = true;

        public bool RequiresKeypress { get; }

        protected Interactive(string tileSheetName, int tileSheetIndex, bool requiresKeypress = true) : base(tileSheetName,
            tileSheetIndex, TileCollision.Passable)
        {
            RequiresKeypress = requiresKeypress;
        }

        internal abstract void ApplyEffect(Player player);
    }
}