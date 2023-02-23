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
    abstract class Player
    {
        // constants
        const float MoveAcceleration = 500f;
        const float MaxMoveSpeed = 200f;
        const float DragFactor = .6f;
        const float Gravity = 300f;

        const float MaxHealth = 100f;
        const float AdditionalHealthRegen = 1f;

        const float DodgeDuration = .3f;
        const float DodgeCooldown = .8f;
        const float BlockDuration = .3f;
        const float BlockCooldown = .8f;

        // player identifiers
        PlayerIndex playerIndex;
        int teamId;
        GamePadState oldGamePad;

        // combat and movement info
        public Vector2 Position;
        public Vector2 Velocity;
        float movement;
        public float Health { get; private set; }
        public float AdditionalHealth { get; private set; }
        public bool IsDead
        {
            get => Health <= 0;
        }
        public bool IsCompletelyDead { get; private set; }
        public float Luck { get; private set; }
        
        bool sprinting;
        bool ducking;
        
        bool IsDodging
        {
            get => dodgingTime > 0;
        }
        float dodgingTime;
        float dodgingCooldown;
        
        bool IsBlocking
        {
            get => blockingTime > 0;
        }
        float blockingTime;
        float blockingCooldown;

        /// <summary>
        /// This will prevent the player from sending inputs due to the player being hit
        /// </summary>
        public float DisabledTime;

        public Rectangle Hitbox
        {
            get
            {
                // TODO implement
                return new Rectangle();
            }
        }

        // for subclasses and rendering
        Texture2D spriteSheet;
        Rectangle SourceRectangle
        {
            get
            {
                return new Rectangle(frameIndex % framesPerRow * frameWidth, frameIndex / framesPerRow * frameHeight, frameWidth, frameHeight);
            }
        }
        Rectangle Rectangle
        {
            get
            {
                // TODO implement
                return new Rectangle((int)Position.X, (int)Position.Y, frameWidth, frameHeight);
            }
        }
        Vector2 Origin
        {
            get => new Vector2(frameWidth, frameHeight) / 2;
        }
        SpriteEffects flip;
        int frameWidth;
        int frameHeight;
        int framesPerRow;
        int frameIndex;

        public Player(Map map, Vector2 start, int frameWidth, int frameHeight, int framesPerRow, string spriteSheetName, PlayerIndex playerIndex, int teamId)
        {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.framesPerRow = framesPerRow;
            this.frameIndex = 0;
            spriteSheet = map.Content.Load<Texture2D>(@"Fighters\" + spriteSheetName);
            this.playerIndex = playerIndex;
            this.teamId = teamId;

            oldGamePad = GamePad.GetState(playerIndex);
            Position = start;
            Velocity = Vector2.Zero;

            Health = MaxHealth;
            AdditionalHealth = MaxHealth;
            Luck = 0f;

            sprinting = false;
            ducking = false;
            dodgingTime = 0f;
            dodgingCooldown = 0f;
            blockingTime = 0f;
            blockingCooldown = 0f;
            DisabledTime = 0f;
        }

        public bool IsPlayerFriendly(Player other)
        {
            return teamId == other.teamId;
        }

        /// <summary>
        /// Make the player attack
        /// </summary>
        public abstract void Attack();
        
        /// <summary>
        /// Make the player use a special attack
        /// </summary>
        public abstract void SpecialAttack();

        /// <summary>
        /// Make the player block an attack. If timed right, take decreased damage and do a counter attack
        /// </summary>
        private void Block()
        {
            // TODO implement
        }

        /// <summary>
        /// Make the player dodge an attack. If timed right, completely avoid the attack.
        /// </summary>
        private void Dodge()
        {
            // TODO implement
        }

        /// <summary>
        /// Make the player jump
        /// </summary>
        private void Jump()
        {
            // TODO implement
        }

        private void GetInput()
        {
            GamePadState gamePad = GamePad.GetState(playerIndex);
            movement = gamePad.ThumbSticks.Left.X;

            // TODO check for attacks and other input

            oldGamePad = gamePad;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            DoPhysics(elapsed);

            AdditionalHealth = Math.Min(MaxHealth, AdditionalHealth + AdditionalHealthRegen);
        }

        /// <summary>
        /// Handle the player's movement and physics
        /// </summary>
        private void DoPhysics(float elapsed)
        {
            // do the x velocity
            Velocity.X += movement * MoveAcceleration * elapsed;
            Velocity.X *= 1 - DragFactor * elapsed;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            Position += Velocity * elapsed;

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(spriteSheet, Rectangle, SourceRectangle, Color.White, 0f, Origin, flip, 0f);
        }

        public void OnKilled()
        {
            
        }

        public void Reset(Vector2 start)
        {

        }
    }
}
