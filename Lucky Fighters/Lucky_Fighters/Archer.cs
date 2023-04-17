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
        private const float BasicAttackCooldown = .8f;
        private const float BasicAttackForeswing = .55f;
        private const float BasicAttackDamage = 15f;
        private const float BasicAttackBackswing = .1f;

        public Archer(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 5, "archersheet", playerIndex, teamId)
        {
            attacking = false;
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0, 1 }, 4, true));
            SpriteAnimations.Add("Running", new Animation(new int[] { 2, 3, 4, 5 }, 14, true));
            SpriteAnimations.Add("Sprinting", new Animation(new int[] { 2, 3, 4, 5 }, 18, true));
            SpriteAnimations.Add("Jumping", new Animation(new int[] { 6, 7, 8 }, 10, false));
            SpriteAnimations.Add("Attacking", new Animation(new int[] { 9, 10, 11, 12, 13, 13, 14, 15, 16 }, 16, false));
        }

        public override void Attack()
        {
            if (SpecialCooldown > 0f || attacking) return;

            attacking = true;
            SetAndPlayAnimation("Attacking");

            AddTask(new Task(BasicAttackForeswing, () =>
            {

            })
            .Then(BasicAttackBackswing, () =>
            {
                attacking = false;
            })
            );

            AttackCooldown = BasicAttackCooldown;
        }

        public override void SpecialAttack()
        {
            // TODO implement
        }

        public override void Ultimate()
        {
            // TODO implement
            if (Luck < 100f)
                return;
        }
    }
}
