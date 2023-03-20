using Microsoft.Xna.Framework;
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
        Rectangle instructions;
        Texture2D blank;
        int num;
        int sw;
        int sh;
        GamePadState oldGP;
        GamePadState gp;

        public NumberOfPlayerSelection(IServiceProvider _serviceProvider, int sw, int sh)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
            instructions = new Rectangle(100, 100, sw - 200, sh / 3);
            num = 1;
            oldGP = GamePad.GetState(PlayerIndex.One);
        }

        public ContentManager Content { get; }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(blank, instructions, Color.Red);
            spriteBatch.DrawString(font, "Select the number of players.\nPress A to continue.", new Vector2(instructions.X + 10, instructions.Y + 10), Color.White);
            spriteBatch.DrawString(font, "" + num, new Vector2(instructions.X + instructions.Width / 2, instructions.Bottom - 30), Color.White);
        }

        public override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            blank = Content.Load<Texture2D>("blank");
        }

        public override bool ReadyForNextScreen()
        {
            return false;
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
            if (num < 1)
                num = 4;
            if (num > 4)
                num = 1;
            oldGP = gp;
        }
    }
}
