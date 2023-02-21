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
    class SwordFighter : Player
    {
        public SwordFighter(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 96, 128, "swordfightersheet", playerIndex, teamId)
        {
            
        }

        public override void Attack()
        {
            // TODO implement
            Console.WriteLine("Used Attack");
        }

        public override void SpecialAttack()
        {
            // TODO implement
            Console.WriteLine("Used Special Attack");
        }
    }
}
