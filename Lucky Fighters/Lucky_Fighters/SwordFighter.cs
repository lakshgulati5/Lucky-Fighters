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
        const float BasicAttackCooldown = .25f;
        const float BasicAttackForeswing = .15f;
        const float BasicAttackDamage = 8f;

        Rectangle attackRectangle;

        const float SpecialAttackCooldown = 2f;
        const float SpecialAttackDamage = 25f;
        const float SpecialAttackForeswing = .3f;
        const float SpecialAttackBackswing = .3f;


		public SwordFighter(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 5, "swordfightersheet", playerIndex, teamId)
		{
            attackRectangle = new Rectangle();
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0, 5 }, 4, true));
            SpriteAnimations.Add("Running", new Animation(new int[] { 1, 4, 3, 2 }, 14, true));
            SpriteAnimations.Add("Sprinting", new Animation(new int[] { 1, 4, 3, 2 }, 18, true));
            SpriteAnimations.Add("Jumping", new Animation(new int[] { 6, 7, 8 }, 10, false));
            SpriteAnimations.Add("Attacking", new Animation(new int[] { 9, 10, 12, 11, 13, 13 }, 30, false));
            SpriteAnimations.Add("Blocking", new Animation(new int[] { 14, 15 }, 10, false));
            SpriteAnimations.Add("Hurt", new Animation(new int[] { 16, 17 }, 10, false));
            SpriteAnimations.Add("SpecialAttacking", new Animation(new int[] { 18, 19, 20, 20, 21, 22, 23, 24 }, 18, false));
        }

        public override void Attack()
        {
            if (AttackCooldown > 0f || attacking)
                return;

            attacking = true;
            SetAndPlayAnimation("Attacking");

            AddTask(new Task(BasicAttackForeswing, () =>
            {
                Rectangle attackHitbox = GetAdjustedAttackHitbox(new Rectangle(Hitbox.Width / 3, -150, 80, 120));
                Point center = attackHitbox.Center;
                foreach (Player otherPlayer in Map.GetCollidingPlayers(attackHitbox))
                {
                    if (!IsPlayerFriendly(otherPlayer))
					{
                        OnDamageDealt(otherPlayer.TakeDamage(BasicAttackDamage));
					}
				}
                // attackRectangle = attackHitbox;
                attacking = false;
            })
			//.Then(.1f, () =>
			//{
			//	attackRectangle = new Rectangle();
			//})
			);

            AttackCooldown = BasicAttackCooldown;
        }

        public override void SpecialAttack()
        {
            if (SpecialCooldown > 0f || attacking) return;

            attacking = true;
            SetAndPlayAnimation("SpecialAttacking");

            AddTask(new Task(SpecialAttackForeswing, () =>
            {
                var specialAttackHitbox =
                    GetAdjustedAttackHitbox(new Rectangle(Hitbox.Width / 3, -150, 100, 140));
                foreach (var otherPlayer in Map.GetCollidingPlayers(specialAttackHitbox)
                    .Where(otherPlayer => !IsPlayerFriendly(otherPlayer)))
                {
                    otherPlayer.TakeDamage(SpecialAttackDamage);
                }

                //attackRectangle = specialAttackHitbox;
            })
            .Then(SpecialAttackBackswing, () =>
            {
                attacking = false;
                //attackRectangle = new Rectangle();
            })
            );

            SpecialCooldown = SpecialAttackCooldown;
        }

        public override void Ultimate()
        {
            // TODO implement
            if (Luck < 100f)
                return;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
            //spriteBatch.Draw(blank, attackRectangle, new Color(1, 0, 0, .5f));
        }
    }
}