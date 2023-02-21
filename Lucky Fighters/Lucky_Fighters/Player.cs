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
        PlayerIndex playerIndex;
        GamePadState oldGamePad;
        
        // combat and movement info
        SpriteEffects flip;
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

        float health;
        float additionalHealth;
        int teamId;

        // for subclasses
        Texture2D spriteSheet;
        Rectangle SourceRectangle
        {
            get
            {
                return new Rectangle(frameIndex % framesPerRow * frameWidth, frameIndex / framesPerRow * frameHeight, frameWidth, frameHeight);
            }
        }
        int frameWidth;
        int frameHeight;
        int framesPerRow;
        int frameIndex;

        public Player(Map map, int frameWidth, int frameHeight, string spriteSheetName, PlayerIndex playerIndex, int teamId)
        {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            spriteSheet = map.Content.Load<Texture2D>(@"Fighters\" + spriteSheetName);
            this.playerIndex = playerIndex;
            this.teamId = teamId;
        }
        public abstract void Attack();
        public abstract void SpecialAttack();

        public void Jump()
        {
            // TODO implement
        }

        public void Update(GameTime gameTime)
        {
            // TODO implement
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // TODO implement
        }
    }
}
