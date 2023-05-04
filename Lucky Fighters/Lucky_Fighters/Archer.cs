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
        const float BasicAttackCooldown = .18f;
        const float BasicAttackForeswing = .12f;
        const float BasicAttackDamage = 5f;

        Rectangle attackRectangle;

        const float SpecialAttackCooldown = 1.5f;
        const float SpecialAttackForeswing = .55f;
        const float SpecialAttackDamage = 20f;
        const float SpecialAttackBackswing = .1f;
        // Angle of the arrow is 18 degrees above parallel
        const double SpecialAttackAngle = Math.PI / 10;

        Texture2D arrowTex;

        public Archer(Map map, Vector2 start, PlayerIndex playerIndex, int teamId) : base(map, start, 1f, 96, 128, 5, "archersheet", playerIndex, teamId)
        {
            attacking = false;
            arrowTex = map.Content.Load<Texture2D>("Fighters/archer_arrow");
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0, 1 }, 4, true));
            SpriteAnimations.Add("Running", new Animation(new int[] { 2, 3, 4, 5 }, 14, true));
            SpriteAnimations.Add("Sprinting", new Animation(new int[] { 2, 3, 4, 5 }, 18, true));
            SpriteAnimations.Add("Jumping", new Animation(new int[] { 6, 7, 8 }, 10, false));
            SpriteAnimations.Add("Attacking", new Animation(new int[] { 17, 18, 19, 19, 19 }, 16, false));
            SpriteAnimations.Add("SpecialAttacking", new Animation(new int[] { 9, 10, 11, 12, 13, 13, 14, 15, 16 }, 16, false));
            SpriteAnimations.Add("Blocking", new Animation(new int[] { 22, 23, 24 }, 16, false));
            SpriteAnimations.Add("Hurt", new Animation(new int[] { 20, 21 }, 10, false));
        }

        public override void Attack()
        {
            if (AttackCooldown > 0f || attacking)
                return;

            attacking = true;
            SetAndPlayAnimation("Attacking");

            AddTask(new Task(BasicAttackForeswing, () =>
            {
                Rectangle attackHitbox = GetAdjustedAttackHitbox(new Rectangle(10, -120, 60, 80));
                Point center = attackHitbox.Center;
                foreach (Player otherPlayer in Map.GetCollidingPlayers(attackHitbox))
                {
                    if (!IsPlayerFriendly(otherPlayer))
                    {
                        OnDamageDealt(otherPlayer.TakeDamage(BasicAttackDamage));
                    }
                }
                //attackRectangle = attackHitbox;
                attacking = false;
            })
            //.Then(.1f, () =>
            //{
            //    attackRectangle = new Rectangle();
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
                float direction = flip == SpriteEffects.FlipHorizontally ? -1 : 1;
                Map.AddSprite(new Arrow(Map, arrowTex, Position + new Vector2(30f * direction, -4f), new Vector2((float)Math.Cos(SpecialAttackAngle) * direction, (float)-Math.Sin(SpecialAttackAngle)) * 1600f, .3f, this, SpecialAttackDamage));
            })
            .Then(SpecialAttackBackswing, () =>
            {
                attacking = false;
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
