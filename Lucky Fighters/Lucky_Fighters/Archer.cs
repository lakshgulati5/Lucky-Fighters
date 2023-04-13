using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class Archer : Player
    {
        public Archer(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 5, "archersheet", playerIndex, teamId)
        {
            attacking = false;
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0, 1 }, 4, true));
            SpriteAnimations.Add("Running", new Animation(new int[] { 2, 3, 4, 5 }, 14, true));
            SpriteAnimations.Add("Sprinting", new Animation(new int[] { 2, 3, 4, 5 }, 18, true));
            SpriteAnimations.Add("Jumping", new Animation(new int[] { 6, 7, 8 }, 10, false));
        }

        public override void Attack()
        {
            throw new NotImplementedException();
        }

        public override void SpecialAttack()
        {
            throw new NotImplementedException();
        }

        public override void Ultimate()
        {
            throw new NotImplementedException();
        }
    }
}
