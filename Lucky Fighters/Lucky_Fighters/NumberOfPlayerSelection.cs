﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class NumberOfPlayerSelection : Screen
    {
        SpriteFont font;
        SpriteFont bigFont;
        Rectangle instructions;
        Texture2D blank;
        Texture2D arrowTexture;
        Rectangle arrow;
        Rectangle arrow1;
        int num;
        int sw;
        int sh;
        GamePadState oldGP;
        GamePadState gp;
        bool ready;
        public Direction direction { get; private set; }


        public NumberOfPlayerSelection(IServiceProvider _serviceProvider, int sw, int sh)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
            instructions = new Rectangle(100, 100, sw - 200, sh / 3);
            int h = instructions.Height / 2;
            int w = instructions.Width / 15;
            arrow = new Rectangle(instructions.X + instructions.Width / 2 - w - 10, instructions.Y + instructions.Height / 2 - h / 2, w, h);
            arrow1 = new Rectangle(instructions.Right - instructions.Width / 2 + w - 10, arrow.Y, w, h);
            num = 2;
            oldGP = GamePad.GetState(PlayerIndex.One);
            ready = false;
        }

        public int Num { get { return num; } }

        public ContentManager Content { get; }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(blank, instructions, Color.Red);
            spriteBatch.DrawString(font, "Select the number of players.\nPress Start to continue.", new Vector2(instructions.X + 10, instructions.Y + 10), Color.White);
            spriteBatch.DrawString(bigFont, "" + num, new Vector2(instructions.X + instructions.Width / 2, instructions.Bottom - instructions.Height/2 - 60), Color.White);
            spriteBatch.Draw(arrowTexture, arrow, new Rectangle(0, 0, 1800, 1570), Color.Red, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(arrowTexture, arrow1, Color.Red);
        }

        public override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            blank = Content.Load<Texture2D>("blank");
            bigFont = Content.Load<SpriteFont>("Big");
            arrowTexture = Content.Load<Texture2D>("arrow");
        }

        public override bool ReadyForNextScreen()
        {
            return ready;
        }

        public override void Update(GameTime gameTime)
        {
            GetInput();
        }

        public void GetInput()
        {
            gp = GamePad.GetState(PlayerIndex.One);
            if (gp.DPad.Left == ButtonState.Pressed && !(oldGP.DPad.Left == ButtonState.Pressed))
                num--;
            if (gp.DPad.Right == ButtonState.Pressed && !(oldGP.DPad.Right == ButtonState.Pressed))
                num++;
            if (gp.ThumbSticks.Left.X < 0 && !(oldGP.ThumbSticks.Left.X < 0))
                num--;
            if (gp.ThumbSticks.Left.X > 0 && !(oldGP.ThumbSticks.Left.X > 0))
                num++;
            if (num < 2)
                num = 4;
            if (num > 4)
                num = 2;
            if (gp.Buttons.Start == ButtonState.Pressed)
            {
                direction = Direction.Forward;
                ready = true;
            }
            oldGP = gp;
        }
    }
}