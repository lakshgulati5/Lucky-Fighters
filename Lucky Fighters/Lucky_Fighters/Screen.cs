using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    public abstract class Screen : IDisposable
    {
        public enum Direction
        {
            Forward,
            Backward
        }

        public enum Mode
        {
            Solo,
            Team
        }

        public ContentManager Content { get; private set; }

        public Screen(IServiceProvider serviceProvider)
        {
            Content = new ContentManager(serviceProvider, "Content");
        }

        public abstract void LoadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public abstract bool ReadyForNextScreen();
        public abstract Color GetColor();

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
