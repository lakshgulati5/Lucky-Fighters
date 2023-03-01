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
    // a task that needs to be completed after a delay
    class Task
    {
        private float toWait;
        public TaskAction OnCompleted;
        private LinkedList<Task> taskList;

        public delegate void TaskAction();

        public bool IsCompleted => toWait < 0f;

        public Task(float delay, TaskAction action)
        {
            toWait = delay;
            OnCompleted = action;
            taskList = new LinkedList<Task>();
        }

        public Task Then(float delay, TaskAction action)
        {
            taskList.AddLast(new Task(delay, action));
            return this;
        }

        public void Update(float elapsed)
        {
            toWait -= elapsed;
        }

        public void WhenCompleted()
        {
            OnCompleted();
            LinkedListNode<Task> first = taskList.First;
            if (first != null)
            {
                Task next = taskList.First.Value;
                toWait += next.toWait;
                OnCompleted = next.OnCompleted;
                taskList.RemoveFirst();
            }
        }
    }

    abstract class Player
    {
        // constants
        const float MoveAcceleration = 4000f;
        const float MaxMoveSpeed = 300f;
        const float DragFactor = 10f;
        const float Gravity = 2000f;
        const float JumpPower = 800f;
        const float JumpTolerance = .1f;
        const float SprintingMultiplier = 1.5f;

        const float MaxHealth = 100f;
        const float AdditionalHealthRegen = 1f;

        // cooldowns will run down during the duration
        // therefore, cooldowns should be >= the duration
        const float DodgeDuration = .3f;
        const float DodgeCooldown = .8f;
        const float BlockDuration = .5f;
        const float BlockCooldown = 1f;
        const float BlockDamageFactor = .5f;
        const float TriggerTolerance = .9f;

        // player identifiers
        PlayerIndex playerIndex;
        int teamId;
        GamePadState oldGamePad;
        protected Map Map;

        // combat and movement info
        public Vector2 Position;
        public Vector2 Velocity;
        float movement;
        // TODO implement
        float weightMultiplier;
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
        float jumpingInput;

        public float AttackCooldown { get; protected set; }
        public float SpecialCooldown { get; protected set; }
        
        bool IsDodging => dodgingTime > 0;
        
        float dodgingTime;
        float dodgingCooldown;
        
        bool IsBlocking => blockingTime > 0;
        float blockingTime;
        float blockingCooldown;

        public bool IsOnGround { get; private set; }
        float previousBottom;

        /// <summary>
        /// This will prevent the player from sending inputs due to the player being hit
        /// </summary>
        public float DisabledTime;
        public virtual bool CanMove => DisabledTime <= 0;

        List<Task> tasks;

        // virtual allows this to be overridden by fighter subclasses
        public virtual Rectangle Hitbox
        {
            get
            {
                // TODO implement
                Rectangle rect = Rectangle;
                int paddingX = 10, paddingY = 2;
                return new Rectangle((int)(rect.X - Origin.X + paddingX), (int)(rect.Y - Origin.Y + paddingY), rect.Width - paddingX * 2, rect.Height);
            }
        }

        // for subclasses and rendering
        Texture2D spriteSheet;
        /// <summary>
        /// The source rectangle, given the current frame index
        /// </summary>
        Rectangle SourceRectangle
        {
            get
            {
                int x = frameIndex % framesPerRow, y = frameIndex / framesPerRow;
                return new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
            }
        }
        /// <summary>
        /// The rectangle to be used for drawing
        /// </summary>
        Rectangle Rectangle
        {
            get
            {
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

        public Player(Map map, Vector2 start, float weightMultiplier, int frameWidth, int frameHeight, int framesPerRow, string spriteSheetName, PlayerIndex playerIndex, int teamId)
        {
            Map = map;
            this.weightMultiplier = weightMultiplier;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.framesPerRow = framesPerRow;
            frameIndex = 0;
            spriteSheet = map.Content.Load<Texture2D>(@"Fighters\" + spriteSheetName);
            this.playerIndex = playerIndex;
            this.teamId = teamId;

            tasks = new List<Task>();

            oldGamePad = GamePad.GetState(playerIndex);
            Reset(start);
        }

        public void AddTask(Task task)
        {
            tasks.Add(task);
        }

        public bool IsPlayerFriendly(Player other)
        {
            return teamId == other.teamId;
        }

        public void OnKilled()
        {
            // TODO implement
            
        }

        public void Reset(Vector2 start)
        {
            // TODO implement
            Position = start;
            Velocity = Vector2.Zero;

            Health = MaxHealth;
            AdditionalHealth = MaxHealth;
            IsCompletelyDead = false;
            Luck = 0f;

            sprinting = false;
            ducking = false;
            jumpingInput = 0f;
            dodgingTime = 0f;
            dodgingCooldown = 0f;
            blockingTime = 0f;
            blockingCooldown = 0f;
            DisabledTime = 0f;
        }

        /// <summary>
        /// Called when this player is hit by an enemy player's attack
        /// </summary>
        /// <param name="damage">Ranges from 0 to 1</param>
        public void TakeDamage(float damage)
        {
            if (damage == 0)
                return;

            if (IsDodging)
                return;

            if (IsBlocking)
            {
                damage *= BlockDamageFactor;
            }

            // damage to deal to the second health bar
            float additionalHealthDamage = Math.Min(damage, AdditionalHealth);
            AdditionalHealth -= additionalHealthDamage;
            Health -= damage - additionalHealthDamage;

            Console.WriteLine("Took " + damage + " damage");
        }

        /// <summary>
        /// Get an adjusted rectangle for the given hitbox (the hitbox x = 0 is at the player's center x, y = 0 i at the feet)
        /// </summary>
        /// <param name="attackHitbox">The offsetted hitbox rectangle from the player</param>
        /// <returns></returns>
        public Rectangle GetAdjustedAttackHitbox(Rectangle attackHitbox)
        {
            // TODO implement
            int centerX = Rectangle.X;
            Point startingPoint = new Point(centerX, Hitbox.Bottom);
            if (flip == SpriteEffects.FlipHorizontally)
            {
                attackHitbox.X = - attackHitbox.X - attackHitbox.Width;
            }
            attackHitbox.Offset(startingPoint);
            return attackHitbox;
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
        /// Make the player use the ultimate (requires luck bar to be full)
        /// </summary>
        public abstract void Ultimate();

        /// <summary>
        /// Make the player block an attack. If timed right, take decreased damage and do a counter attack
        /// </summary>
        private void Block()
        {
            if (blockingCooldown > 0f)
                return;

            Console.WriteLine("Blocked");
            blockingTime = BlockDuration;
            blockingCooldown = BlockCooldown;
        }

        /// <summary>
        /// Make the player dodge an attack. If timed right, completely avoid the attack.
        /// </summary>
        private void Dodge()
        {
            if (dodgingCooldown > 0f)
                return;

            Console.WriteLine("Dodged");
            dodgingTime = DodgeDuration;
            dodgingCooldown = DodgeCooldown;
        }

        /// <summary>
        /// Tell the game the player wants to jump
        /// </summary>
        private void SendJump()
        {
            jumpingInput = JumpTolerance;
        }

        /// <summary>
        /// Actually make the player jump if conditions are met
        /// </summary>
        private void DoJump()
        {
            if (IsOnGround)
            {
                Velocity.Y = -JumpPower;
            }
        }

        private void GetInput()
        {

            GamePadState gamePad = GamePad.GetState(playerIndex);
            if (CanMove)
			{
                movement = gamePad.ThumbSticks.Left.X;
                if (Math.Abs(movement) < .1f)
                    movement = 0f;

                // TODO check for attacks and other input
                if (gamePad.Buttons.X == ButtonState.Pressed && oldGamePad.Buttons.X == ButtonState.Released ||
                    gamePad.Buttons.Y == ButtonState.Pressed && oldGamePad.Buttons.Y == ButtonState.Released)
                {
                    SendJump();
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
                sprinting = gamePad.Triggers.Left >= TriggerTolerance;
                ducking = gamePad.Buttons.LeftShoulder == ButtonState.Pressed;
			}
            else
			{
                movement = 0f;
			}

            oldGamePad = gamePad;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = gameTime.GetElapsedSeconds();

            if (!IsDead)
            {
                AttackCooldown = Math.Max(AttackCooldown - elapsed, 0f);
                SpecialCooldown = Math.Max(SpecialCooldown - elapsed, 0f);
                GetInput();
                DoPhysics(elapsed);
                for (int i = 0; i < tasks.Count; i++)
                {
                    Task task = tasks[i];
                    task.Update(elapsed);
                    if (task.IsCompleted)
                    {
                        task.WhenCompleted();
                        // check again to make sure all Then() connected tasks are done
                        if (task.IsCompleted)
                        {
                            tasks.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            AdditionalHealth = Math.Min(MaxHealth, AdditionalHealth + AdditionalHealthRegen * elapsed);
        }

        /// <summary>
        /// Handle the player's movement and physics
        /// </summary>
        private void DoPhysics(float elapsed)
        {
            Vector2 previousPosition = Position;

            // check if the player wants to jump
            if (jumpingInput > 0f)
            {
                DoJump();
            }
            jumpingInput -= elapsed;

            // do the x velocity
            Velocity.X += movement * MoveAcceleration * elapsed;
            if (movement == 0)
                Velocity.X *= 1 - DragFactor * elapsed;
            float maxSpeed = MaxMoveSpeed;
            if (sprinting)
                maxSpeed *= SprintingMultiplier;
            Velocity.X = MathHelper.Clamp(Velocity.X, -maxSpeed, maxSpeed);

            Velocity.Y += Gravity * elapsed;

            Position += Velocity * elapsed;

            HandleCollisions();


            if (Position.X == previousPosition.X)
                Velocity.X = 0;

            if (Position.Y == previousPosition.Y || IsOnGround)
                Velocity.Y = 0;

            if (movement < 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (movement > 0)
                flip = SpriteEffects.None;
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
            DrawHealthBar();
        }

        public void DrawHealthBar()
        {
            // TODO implement

        }
    }
}
