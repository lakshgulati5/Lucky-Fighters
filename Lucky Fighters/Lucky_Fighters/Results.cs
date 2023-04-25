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
    class Results : Screen
    {
        bool ready;
        SpriteFont font;
        int sw;
        int sh;
        Screen.Mode mode;
        Player winner;
        string winningTeam;
        int numOfPlayers;
        GamePadState[] gp;
        Map.GameEnd end;
        public ContentManager Content { get; }
        public Results(IServiceProvider _serviceProvider, int sw, int sh, int num, Screen.Mode mode, Player winner, Map.GameEnd end)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
            this.winner = winner;
            numOfPlayers = num;
            gp = new GamePadState[numOfPlayers];
            this.end = end;
            this.mode = mode;
        }

        //no winner (quit game)
        public Results(IServiceProvider _serviceProvider, int sw, int sh, int num, Screen.Mode mode, string winner, Map.GameEnd end)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
            winningTeam = winner;
            numOfPlayers = num;
            gp = new GamePadState[numOfPlayers];
            this.end = end;
            this.mode = mode;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (end == Map.GameEnd.Win)
            {
                if (mode == Mode.Solo)
                    spriteBatch.DrawString(font, "Player " + winner.playerIndex + " wins!\nPress start to continue.", new Vector2(50, 50), Color.White);
                else
                    spriteBatch.DrawString(font, winningTeam + " Team wins!\nPress start to continue.", new Vector2(50, 50), Color.White);
            }
            else
                spriteBatch.DrawString(font, "No contest.\nPress start to continue.", new Vector2(50, 50), Color.White);
        }

        public override Color GetColor()
        {
            return Color.DimGray;
        }

        public override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Big");
        }

        public override bool ReadyForNextScreen()
        {
            return ready;
        }

        public override void Update(GameTime gameTime)
        {
            getInput();
        }

        private void getInput()
        {
            for (int x = 0; x < gp.Length; x++)
            {
                gp[x] = GamePad.GetState((PlayerIndex)x);
            }
            for (int x = 0; x < gp.Length; x++)
            {
                if (gp[x].Buttons.Start == ButtonState.Pressed)
                {
                    ready = true;
                }
            }
        }
    }
}
