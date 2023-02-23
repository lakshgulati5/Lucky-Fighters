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
        const float MoveAcceleration = 6000f;
        const float MaxMoveSpeed = 400f;
        const float DragFactor = 8f;
        const float Gravity = 2000f;
        const float JumpPower = 800f;

        const float MaxHealth = 100f;
        const float AdditionalHealthRegen = 1f;

        const float DodgeDuration = .3f;
        const float DodgeCooldown = .8f;
        const float BlockDuration = .3f;
        const float BlockCooldown = .8f;
        const float TriggerTolerance = .9f;

        // player identifiers
        PlayerIndex playerIndex;
        int teamId;
        GamePadState oldGamePad;
        Map Map;

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

        public bool IsOnGround { get; private set; }
        float previousBottom;

        /// <summary>
        /// This will prevent the player from sending inputs due to the player being hit
        /// </summary>
        public float DisabledTime;

        public Rectangle Hitbox
        {
            get
            {
                // TODO implement
                Rectangle rect = Rectangle;
                return new Rectangle((int)(rect.X - Origin.X), (int)(rect.Y - Origin.Y), rect.Width, rect.Height);
            }
        }

        // for subclasses and rendering
        Texture2D spriteSheet;
        Rectangle SourceRectangle
        {
            get
            {
                int x = frameIndex % framesPerRow, y = frameIndex / framesPerRow;
                return new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
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
            Map = map;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.framesPerRow = framesPerRow;
            frameIndex = 0;
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
            Console.WriteLine("Blocked");
            // TODO implement
        }

        /// <summary>
        /// Make the player dodge an attack. If timed right, completely avoid the attack.
        /// </summary>
        private void Dodge()
        {
            Console.WriteLine("Dodged");
            // TODO implement
        }

        /// <summary>
        /// Make the player jump
        /// </summary>
        private void Jump()
        {
            Console.WriteLine("Jumped");
            Velocity.Y = -JumpPower;
        }

        private void GetInput()
        {
            GamePadState gamePad = GamePad.GetState(playerIndex);
            movement = gamePad.ThumbSticks.Left.X;

            // TODO check for attacks and other input
            if (gamePad.Buttons.X == ButtonState.Pressed && oldGamePad.Buttons.X == ButtonState.Released ||
                gamePad.Buttons.Y == ButtonState.Pressed && oldGamePad.Buttons.Y == ButtonState.Released)
            {
                Jump();
            }
            if (gamePad.Buttons.A == ButtonState.Pressed && oldGamePad.Buttons.A == ButtonState.Released)
            {
                Attack();
            }
            if (gamePad.Buttons.B == ButtonState.Pressed && oldGamePad.Buttons.B == ButtonState.Released)
            {
                // TODO add check for luck bar
                SpecialAttack();
            }
            if (gamePad.Buttons.RightShoulder == ButtonState.Pressed && oldGamePad.Buttons.RightShoulder == ButtonState.Released)
            {
                Dodge();
            }
            if (gamePad.Triggers.Right >= TriggerTolerance && oldGamePad.Triggers.Right < TriggerTolerance)
            {
                Block();
            }

            oldGamePad = gamePad;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GetInput();
            DoPhysics(elapsed);

            AdditionalHealth = Math.Min(MaxHealth, AdditionalHealth + AdditionalHealthRegen);
        }

        /// <summary>
        /// Handle the player's movement and physics
        /// </summary>
        private void DoPhysics(float elapsed)
        {
            Vector2 previousPosition = Position;

            // do the x velocity
            Velocity.X += movement * MoveAcceleration * elapsed;
            Velocity.X *= 1 - DragFactor * elapsed;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            Velocity.Y += Gravity * elapsed;

            Position += Velocity * elapsed;

            HandleCollisions();

            if (Position.X == previousPosition.X)
                Velocity.X = 0;

            if (Position.Y == previousPosition.Y || IsOnGround)
                Velocity.Y = 0;
        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles
            Rectangle bounds = Hitbox;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Height) - 1;

            // reset flag to search for ground collisions
            IsOnGround = false;

            // for each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    // if this tile is collidable
                    TileCollision collision = Map.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // determine collision depth (with direction) and magnitude
                        Rectangle tileBounds = Map.GetBounds(x, y);
                        Vector2 depth = bounds.GetIntersectionDepth(tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // resolve the collision along the shallow axis
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // if we crossed the top of a tile, we are on the ground
                                if (previousBottom <= tileBounds.Top)
                                    IsOnGround = true;

                                // ignore platforms, unless we are on the ground
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // resolve the collision along the y axis
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // perform further collisions with the new bounds
                                    bounds = Hitbox;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // ignore platforms
                            {
                                // resolve the collision along the x axis
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // perform further collisions with the new bounds
                                bounds = Hitbox;
                            }
                        }
                    }
                }
            }

            previousBottom = bounds.Bottom;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(spriteSheet, Rectangle, SourceRectangle, Color.White, 0f, Origin, flip, 0f);
            Console.WriteLine(Hitbox + " " + Rectangle + " " + SourceRectangle);
        }

        public void OnKilled()
        {
            
        }

        public void Reset(Vector2 start)
        {

        }
    }
}
