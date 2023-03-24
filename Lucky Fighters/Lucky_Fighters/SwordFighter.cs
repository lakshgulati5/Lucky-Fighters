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

        Rectangle attackRectangle;

        private const float SpecialAttackCooldown = 2f;

		public SwordFighter(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 4, "swordfightersheet", playerIndex, teamId)
		{
            attacking = false;
            attackRectangle = new Rectangle();
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0, 5 }, 4, true));
            SpriteAnimations.Add("Running", new Animation(new int[] { 1, 4, 3, 2 }, 14, true));
            SpriteAnimations.Add("Sprinting", new Animation(new int[] { 1, 4, 3, 2 }, 18, true));
            SpriteAnimations.Add("Jumping", new Animation(new int[] { 6, 7, 8 }, 10, false));
        }

        public override void Attack()
        {
            if (AttackCooldown > 0f || attacking)
                return;

            attacking = true;

            AddTask(new Task(.1f, () =>
            {
                Rectangle attackHitbox = GetAdjustedAttackHitbox(new Rectangle(Hitbox.Width / 3, -150, 80, 120));
                Point center = attackHitbox.Center;
                foreach (Player otherPlayer in Map.GetCollidingPlayers(attackHitbox))
				{
                    if (!IsPlayerFriendly(otherPlayer))
					{
                        otherPlayer.TakeDamage(BasicAttackDamage);
					}
				}
                attackRectangle = attackHitbox;
                attacking = false;
            }).Then(.1f, () =>
            {
                attackRectangle = new Rectangle();
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

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
		{
            spriteBatch.Draw(blank, attackRectangle, new Color(1, 0, 0, .5f));
            base.Draw(spriteBatch, gameTime);
		}
    }
}
