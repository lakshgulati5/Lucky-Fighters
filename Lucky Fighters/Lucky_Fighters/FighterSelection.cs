using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class FighterSelection
    {
        Rectangle[] fighters;
        string[] fighterNames;
        Rectangle[] players;
        string[] selectedFighters;
        int screenWidth;
        int screenHeight;
        Texture2D blank;
        SpriteFont font;

        public ContentManager Content { get; }

        public FighterSelection (IServiceProvider _serviceProvider, int sw, int sh)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            screenWidth = sw;
            screenHeight = sh;
            players = new Rectangle[4];
            fighters = new Rectangle[5];
            fighterNames = new string[] { "SwordFighter", "Archer", "Ninja", "Wizard", "Muscleman" };
            selectedFighters = new string[players.Length];
            selectedFighters[0] = fighterNames[1];
            int playerCardSize = (sw - 50 * (players.Length + 1)) / players.Length;
            for (int x = 0; x < players.Length; x++)
            {
                players[x] = new Rectangle(50 * (x + 1) + playerCardSize * x, sh - 50 - playerCardSize, playerCardSize, playerCardSize);
            }
            int fighterCardSize = (sw - 50 * (fighters.Length + 1)) / fighters.Length;
            for (int x = 0; x < fighters.Length; x++)
            {
                fighters[x] = new Rectangle(50 * (x + 1) + fighterCardSize * x, 50, fighterCardSize, fighterCardSize);
            }
        }

        public void LoadContent()
        {
            blank = Content.Load<Texture2D>("blank");
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int x = 0;
            foreach (Rectangle r in players)
            {
                x++;
                spriteBatch.Draw(blank, r, Color.Red);
                spriteBatch.DrawString(font, "Player " + x +"\n" + selectedFighters[x-1], new Vector2(r.X, r.Y), Color.White);
            }
            x = 0;
            foreach (Rectangle r in fighters)
            {
                spriteBatch.Draw(blank, r, Color.White);
                spriteBatch.DrawString(font, fighterNames[x], new Vector2(r.X, r.Y), Color.Red);
                x++;
            }
        }

    }
}
