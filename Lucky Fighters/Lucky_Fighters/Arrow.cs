using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class Arrow : AnimatedSprite
    {
        const float Lifespan = 2f;

        Texture2D tex;
        Map map;

        Vector2 Position;
        Vector2 Velocity;
        float scale;
        Player player;
        float damage;
        float timeAlive;

        public Rectangle Hitbox
        {
            get
            {
                float halfSizeX = tex.Width * scale / 2, halfSizeY = tex.Height * scale / 2;
                return new Rectangle((int)(Position.X - halfSizeX), (int)(Position.Y - halfSizeY), (int)(halfSizeX * 2), (int)(halfSizeY * 2));
            }
        }

        bool shouldRemove;

        public override bool ShouldRemove => shouldRemove;

        public Arrow(Map map, Texture2D tex, Vector2 position, Vector2 velocity, float scale, Player player, float damage) : base(256, 64, 1)
        {
            this.map = map;
            this.tex = tex;
            Position = position;
            Velocity = velocity;
            this.scale = scale;
            this.player = player;
            this.damage = damage;
            timeAlive = 0f;
            shouldRemove = false;
            SpriteAnimations.Add("Idle", new Animation(new int[] { 0 }, 1, false));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsed = gameTime.GetElapsedSeconds();
            Velocity += new Vector2(0, Player.Gravity * elapsed);
            Position += Velocity * elapsed;
            timeAlive += elapsed;
            if (timeAlive >= Lifespan)
                shouldRemove = true;

            // check collisions with players
            foreach (Player otherPlayer in map.GetCollidingPlayers(Hitbox))
            {
                if (!player.IsPlayerFriendly(otherPlayer))
                {
                    player.OnDamageDealt(otherPlayer.TakeDamage(damage));
                    shouldRemove = true;
                    break;
                }
            }

            // check collisions with map
            Rectangle bounds = Hitbox;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Height) - 1;

            // for each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    // if this tile is collidable
                    TileCollision collision = map.GetCollision(x, y);
                    if (collision == TileCollision.Impassable)
                        shouldRemove = true;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            float angle = -(float)Math.Atan2(-Velocity.Y, Velocity.X);
            spriteBatch.Draw(tex, Position, SourceRectangle, player.GetColor(), angle, Origin, scale, SpriteEffects.None, 0);
        }
    }
}
