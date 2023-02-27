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
    class FighterSelection
    {
        Color color1 = Color.Blue;
        Color color2 = Color.Red;
        Color color3 = Color.Yellow;
        Color color4 = Color.Orange;
        Color[] alt;
        int numOfPlayers;
        Rectangle[] fighters;
        string[] fighterNames;
        Rectangle[] players;
        Rectangle[] playerOneOptions;
        Rectangle[] playerTwoOptions;
        Rectangle[] playerThreeOptions;
        Rectangle[] playerFourOptions;
        Rectangle[] hover;
        int[] selections;
        string[] selectedFighters;
        bool[] ready;
        int screenWidth;
        int screenHeight;
        Texture2D blank;
        SpriteFont font;
        GamePadState[] gp;
        GamePadState[] oldGP;

        public ContentManager Content { get; }

        public FighterSelection (IServiceProvider _serviceProvider, int sw, int sh, int nOfPlayers)
        {
            numOfPlayers = nOfPlayers;
            selections = new int[numOfPlayers];
            gp = new GamePadState[numOfPlayers];
            oldGP = new GamePadState[numOfPlayers];
            alt = new Color[numOfPlayers];
            if (numOfPlayers < 2)
                throw new Exception("Need at least two players to play");
            Content = new ContentManager(_serviceProvider, "Content");
            screenWidth = sw;
            screenHeight = sh;
            players = new Rectangle[numOfPlayers];
            fighters = new Rectangle[5];
            playerOneOptions = new Rectangle[fighters.Length];
            playerTwoOptions = new Rectangle[fighters.Length]; 
            playerThreeOptions = new Rectangle[fighters.Length];
            playerFourOptions = new Rectangle[fighters.Length];
            hover = new Rectangle[numOfPlayers];
            fighterNames = new string[] { "SwordFighter", "Archer", "Ninja", "Wizard", "Muscleman" };
            selectedFighters = new string[players.Length];
            selectedFighters[0] = fighterNames[1];
            int playerCardSize = (sw - 50 * (4 + 1)) / 4;
            for (int x = 0; x < players.Length; x++)
            {
                players[x] = new Rectangle(50 * (x + 1) + playerCardSize * x, sh - 50 - playerCardSize, playerCardSize, playerCardSize);
            }
            int fighterCardSize = (sw - 50 * (fighters.Length + 1)) / fighters.Length;
            for (int x = 0; x < fighters.Length; x++)
            {
                fighters[x] = new Rectangle(50 * (x + 1) + fighterCardSize * x, 50, fighterCardSize, fighterCardSize);
                int displacement = fighterCardSize / 4;
                playerOneOptions[x] = new Rectangle(fighters[x].X, fighters[x].Bottom - displacement, displacement, displacement);
                playerTwoOptions[x] = new Rectangle(fighters[x].X + displacement, fighters[x].Bottom - displacement, displacement, displacement);
                playerThreeOptions[x] = new Rectangle(fighters[x].X + displacement * 2, fighters[x].Bottom - displacement, displacement, displacement);
                playerFourOptions[x] = new Rectangle(fighters[x].X + displacement * 3, fighters[x].Bottom - displacement, displacement, displacement);
            }
        }

        public void LoadContent()
        {
            blank = Content.Load<Texture2D>("blank");
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        public void Update()
        {
            GetInput();
        }

        public void GetInput()
        {
            //get player one input
            gp[0] = GamePad.GetState(PlayerIndex.One);
            gp[1] = GamePad.GetState(PlayerIndex.Two);
            if (numOfPlayers >= 3)
                gp[2] = GamePad.GetState(PlayerIndex.Three);
            if (numOfPlayers == 4)
                gp[3] = GamePad.GetState(PlayerIndex.Four);
            for (int x = 0; x < numOfPlayers; x++)
            {
                if (gp[x].DPad.Right == ButtonState.Pressed && !(oldGP[x].DPad.Right == ButtonState.Pressed))
                {
                    selections[x]++;
                    if (selections[x] >= fighters.Length)
                        selections[x] = 0;
                }
                if (gp[x].DPad.Left == ButtonState.Pressed && !(oldGP[x].DPad.Left == ButtonState.Pressed))
                {
                    selections[x]--;
                    if (selections[x] < 0)
                        selections[x] = fighters.Length - 1;
                }
                selectedFighters[x] = fighterNames[selections[x]];
                
                oldGP[x] = gp[x];
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int y = 0;
            foreach (Rectangle r in players)
            {
                y++;
                spriteBatch.Draw(blank, r, Color.Red);
                spriteBatch.DrawString(font, "Player " + y +"\n" + selectedFighters[y-1], new Vector2(r.X, r.Y), Color.White);
            }
            for (int x = 0; x < fighters.Length; x++)
            {
                spriteBatch.Draw(blank, fighters[x], Color.White);
                spriteBatch.DrawString(font, fighterNames[x], new Vector2(fighters[x].X, fighters[x].Y), Color.Red);
                //only draw in colors if that fighter is selected
                if (selections[0] == x)
                    spriteBatch.Draw(blank, playerOneOptions[x], color1);
                else
                    spriteBatch.Draw(blank, playerOneOptions[x], Color.White);
                hover[0] = playerOneOptions[x];
                spriteBatch.Draw(blank, hover[0], alt[0]);
                
                if (selections[1] == x)
                    spriteBatch.Draw(blank, playerTwoOptions[x], color2);
                else
                    spriteBatch.Draw(blank, playerTwoOptions[x], Color.White);
                hover[1] = playerTwoOptions[x];
                spriteBatch.Draw(blank, hover[1], alt[1]);

                if (numOfPlayers > 2)
                {
                    if (selections[2] == x)
                        spriteBatch.Draw(blank, playerThreeOptions[x], color3);
                    else
                        spriteBatch.Draw(blank, playerThreeOptions[x], Color.White);
                    hover[2] = playerThreeOptions[x];
                    spriteBatch.Draw(blank, hover[2], alt[2]);

                    if (selections[3] == x)
                        spriteBatch.Draw(blank, playerFourOptions[x], color4);
                    else
                        spriteBatch.Draw(blank, playerFourOptions[x], Color.White);
                    hover[3] = playerFourOptions[x];
                    spriteBatch.Draw(blank, hover[3], alt[3]);
                }
            }
        }

    }
}
