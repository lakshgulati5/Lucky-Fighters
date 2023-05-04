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
        const float JoystickTolerance = .5f;

        int timer;
        SpriteFont font;
        SpriteFont bigFont;
        Rectangle instructions;
        Texture2D blank;
        Texture2D arrowTexture;
        Rectangle arrow;
        Rectangle arrow1;
        Rectangle team;
        Rectangle solo;
        int num;
        int sw;
        int sh;
        GamePadState[] oldGP;
        GamePadState[] gp;
        KeyboardState oldKb;
        bool ready;
        bool top;
        bool soloSelected;
        Color arrowColor;
        Color modeColor;
        static readonly Color selectedColor = Color.DarkRed;
        public Direction direction { get; private set; }


        public NumberOfPlayerSelection(IServiceProvider _serviceProvider, int sw, int sh) : base(_serviceProvider)
        {
            timer = 0;
            this.sw = sw;
            this.sh = sh;
            instructions = new Rectangle(100, 100, sw - 200, sh / 2);
            int h = instructions.Height / 4;
            int w = instructions.Width / 15;
            arrow = new Rectangle(instructions.X + instructions.Width / 2 - w - 40, instructions.Y + instructions.Height / 4 - h / 4, w, h);
            arrow1 = new Rectangle(instructions.Right - instructions.Width / 2 + w - 40, arrow.Y, w, h);
            solo = new Rectangle(instructions.X + instructions.Width / 4, arrow.Y + arrow.Height + 50, instructions.Width / 4, h);
            team = new Rectangle(instructions.X + instructions.Width / 2, solo.Y, solo.Width, solo.Height);
            num = 2;
            oldGP = new GamePadState[4];
            for (int x = 0; x < 4; x++)
                oldGP[x] = GamePad.GetState((PlayerIndex)x);
            ready = false;
            gp = new GamePadState[4];
            oldKb = Keyboard.GetState();
            top = true;
            soloSelected = true;
            modeColor = selectedColor;
        }

        public int Num { get { return num; } }

        public Mode getMode 
        { 
            get 
            { 
                if (soloSelected) 
                    return Mode.Solo; 
                else 
                    return Mode.Team;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(blank, instructions, Color.Red);
            spriteBatch.DrawString(font, "Select the number of players and game mode.\nPress A to continue.", new Vector2(instructions.X + 10, instructions.Y + 10), Color.White);
            spriteBatch.DrawString(bigFont, "" + num, new Vector2(instructions.X + instructions.Width / 2 - 20, arrow.Y), Color.White);
            spriteBatch.Draw(arrowTexture, arrow, new Rectangle(0, 0, 1800, 1570), arrowColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(arrowTexture, arrow1, arrowColor);
            if (soloSelected)
            {
                spriteBatch.Draw(blank, solo, modeColor);
                spriteBatch.Draw(blank, team, GetColor());
            }
            else 
            {
                spriteBatch.Draw(blank, solo, GetColor());
                spriteBatch.Draw(blank, team, modeColor);
            }
            spriteBatch.DrawString(bigFont, "Solo", new Vector2(solo.X + solo.Width / 2 - 70, solo.Y + solo.Height/2 - 48), Color.White);
            spriteBatch.DrawString(bigFont, "Team", new Vector2(team.X + team.Width / 2 - 80, solo.Y + team.Height / 2 - 48), Color.White);
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
            timer++;
            GetInput();
            if (timer % 15 == 0)
            {
                if (top)
                {
                    ArrowHover();
                    modeColor = selectedColor;
                }
                else
                {
                    ModeHover(soloSelected);
                    arrowColor = Color.Red;
                }

            }
        }

        public void GetInput()
        {
            KeyboardState kb = Keyboard.GetState();
            for (int x = 0; x < gp.Length; x++)
            {
                gp[x] = GamePad.GetState((PlayerIndex)x);
                if (top)
                {
                    if (gp[x].DPad.Left == ButtonState.Pressed && !(oldGP[x].DPad.Left == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X < -JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X < -JoystickTolerance) ||
                        x == 1 && kb.MenuLeftPressed() && !oldKb.MenuLeftPressed())
                        num--;
                    if (gp[x].DPad.Right == ButtonState.Pressed && !(oldGP[x].DPad.Right == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X > JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X > JoystickTolerance) ||
                        x == 1 && kb.MenuRightPressed() && !oldKb.MenuRightPressed())
                        num++;
                }
                else
                {
                    if (gp[x].DPad.Left == ButtonState.Pressed && !(oldGP[x].DPad.Left == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X < -JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X < -JoystickTolerance) ||
                        x == 1 && kb.MenuLeftPressed() && !oldKb.MenuLeftPressed())
                        soloSelected = true;
                    if (gp[x].DPad.Right == ButtonState.Pressed && !(oldGP[x].DPad.Right == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X > JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X > JoystickTolerance) ||
                        x == 1 && kb.MenuRightPressed() && !oldKb.MenuRightPressed())
                        soloSelected = false;
                }

                if (gp[x].DPad.Up == ButtonState.Pressed && !(oldGP[x].DPad.Up == ButtonState.Pressed) ||
                    gp[x].ThumbSticks.Left.Y > JoystickTolerance && !(oldGP[x].ThumbSticks.Left.Y > JoystickTolerance) ||
                    x == 1 && kb.MenuUpPressed() && !oldKb.MenuUpPressed())
                    top = true;
                if (gp[x].DPad.Down == ButtonState.Pressed && !(oldGP[x].DPad.Down == ButtonState.Pressed) ||
                    gp[x].ThumbSticks.Left.Y < -JoystickTolerance && !(oldGP[x].ThumbSticks.Left.Y < -JoystickTolerance) ||
                    x == 1 && kb.MenuDownPressed() && !oldKb.MenuDownPressed())
                    top = false;
            }
            if (num < 2)
                num = 4;
            if (num > 4)
                num = 2;
            for (int x = 0; x < gp.Length; x++)
            {
                if (gp[x].Buttons.A == ButtonState.Pressed ||
                    x == 1 && kb.GamePadAPressed())
                {
                    direction = Direction.Forward;
                    ready = true;
                }
                oldGP[x] = gp[x];
            }
            oldKb = kb;
        }

        private void ArrowHover()
        {
            if (arrowColor == Color.Red)
                arrowColor = Color.DarkRed;
            else
                arrowColor = Color.Red;
        }

        private void ModeHover(bool soloSelected)
        {
            if (modeColor == GetColor())
                modeColor = selectedColor;
            else
                modeColor = GetColor();
        }

        public override Color GetColor()
        {
            return Color.DimGray;
        }
    }
}
