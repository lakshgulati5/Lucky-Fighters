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
        private const float BasicAttackCooldown = .3f;
        private const float BasicAttackDamage = 5f;

        private const float SpecialAttackCooldown = 2f;

        private bool attacking;

		public override bool CanMove => !attacking && base.CanMove;

		public SwordFighter(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 4, "swordfightersheet", playerIndex, teamId)
		{
            attacking = false;
		}

        public override void Attack()
        {
            // TODO implement
            if (AttackCooldown > 0f || attacking)
                return;

            attacking = true;

            AddTask(new Task(.2f, () =>
            {
                // TODO implement
                Rectangle attackHitbox = GetAdjustedAttackHitbox(new Rectangle(30, -100, 30, 100));
                Point center = attackHitbox.Center;
                foreach (Player otherPlayer in Map.GetCollidingPlayers(attackHitbox))
				{
                    if (!IsPlayerFriendly(otherPlayer))
					{
                        otherPlayer.TakeDamage(BasicAttackDamage);
					}
				}

                attacking = false;
            }));

            AttackCooldown = BasicAttackCooldown;
            Console.WriteLine("Used Attack");
        }

        public override void SpecialAttack()
        {
            // TODO implement
            if (SpecialCooldown > 0f)
                return;



            SpecialCooldown = SpecialAttackCooldown;
            Console.WriteLine("Used Special Attack");
        }

        public override void Ultimate()
        {
            // TODO implement
            if (Luck < 1f)
                return;

            Console.WriteLine("Used Ultimate");
        }
    }
}
