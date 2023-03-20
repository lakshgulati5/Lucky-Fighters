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
    class FighterSelection : Screen
    {
        int timer;
        Color[] alt;
        int numOfPlayers;
        Rectangle[] fighters;
        string[] fighterNames;
        Rectangle[] players;
        Rectangle[] borders;
        Rectangle[] playerOneOptions;
        Rectangle[] playerTwoOptions;
        Rectangle[] playerThreeOptions;
        Rectangle[] playerFourOptions;
        Rectangle startStrip;
        int[] selections;
        string[] selectedFighters;
        bool[] ready;
        int screenWidth;
        int screenHeight;
        Texture2D blank;
        SpriteFont font;
        GamePadState[] gp;
        GamePadState[] oldGP;
        public bool started;

        public ContentManager Content { get; }

        public FighterSelection (IServiceProvider _serviceProvider, int sw, int sh, int nOfPlayers)
        {
            timer = 0;
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
            borders = new Rectangle[numOfPlayers];
            fighters = new Rectangle[5];
            playerOneOptions = new Rectangle[fighters.Length];
            playerTwoOptions = new Rectangle[fighters.Length]; 
            playerThreeOptions = new Rectangle[fighters.Length];
            playerFourOptions = new Rectangle[fighters.Length];
            fighterNames = new string[] { "SwordFighter", "Archer", "Ninja", "Wizard", "Muscleman" };
            selectedFighters = new string[players.Length];
            ready = new bool[numOfPlayers];
            startStrip = new Rectangle(0, 320, sw, 50);
            int playerCardSize = (sw - 50 * (4 + 1)) / 4;
            for (int x = 0; x < players.Length; x++)
            {
                players[x] = new Rectangle(50 * (x + 1) + playerCardSize * x, sh - 50 - playerCardSize, playerCardSize, playerCardSize);
                selectedFighters[x] = "Select Fighter";
                borders[x] = new Rectangle(players[x].X - 10, players[x].Y - 10, players[x].Width + 20, players[x].Height + 20);
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
            started = false;
        }

        public override void LoadContent()
        {
            blank = Content.Load<Texture2D>("blank");
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        public override void Update(GameTime gameTime)
        {
            timer++;
            GetInput();
            if (timer % 10  == 0)
                Hover();
        }

        private void Hover()
        {
            for (int x = 0; x < ready.Length; x++)
            {
                if (!ready[x])
                {
                    if (alt[x] == Game1.DefaultColors[x])
                        alt[x] = Color.White;
                    else
                        alt[x] = Game1.DefaultColors[x];
                }
            }
        }

        private void GetInput()
        {
            //get player input
            gp[0] = GamePad.GetState(PlayerIndex.One);
            gp[1] = GamePad.GetState(PlayerIndex.Two);
            if (numOfPlayers >= 3)
                gp[2] = GamePad.GetState(PlayerIndex.Three);
            if (numOfPlayers == 4)
                gp[3] = GamePad.GetState(PlayerIndex.Four);
            for (int x = 0; x < numOfPlayers; x++)
            {
                if (!ready[x])
                {
                    //select by dpad
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

                    //select by left thumbstick
                    if (gp[x].ThumbSticks.Left.X > 0 && !(oldGP[x].ThumbSticks.Left.X > 0))
                    {
                        selections[x]++;
                        if (selections[x] >= fighters.Length)
                            selections[x] = 0;
                    }
                    if (gp[x].ThumbSticks.Left.X < 0 && !(oldGP[x].ThumbSticks.Left.X < 0))
                    {
                        selections[x]--;
                        if (selections[x] < 0)
                            selections[x] = fighters.Length - 1;
                    }
                    selectedFighters[x] = fighterNames[selections[x]];
                    
                    //declare that the player is ready
                    if (gp[x].IsButtonDown(Buttons.A))
                    {
                        ready[x] = true;
                    }
                }
                //unready
                if (gp[x].IsButtonDown(Buttons.B))
                {
                    ready[x] = false;
                }

                if (ReadyToStart())
                {
                    if (gp[0].IsButtonDown(Buttons.Start))
                        started = true;
                }

                oldGP[x] = gp[x];
            }
        }

        private bool ReadyToStart()
        {
            foreach (bool r in ready)
            {
                if (!r)
                    return false;
            }
            return true;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < players.Length; x++)
            {
                if (ready[x])
                    spriteBatch.Draw(blank, borders[x], Color.White);
                spriteBatch.Draw(blank, players[x], Game1.DefaultColors[x]);
                if (ready[x])
                    spriteBatch.DrawString(font, "Player " + (x + 1) + "\n" + selectedFighters[x] + "\nReady", new Vector2(players[x].X + 10, players[x].Y + 10), Color.White);
                else
                    spriteBatch.DrawString(font, "Player " + (x + 1) + "\n" + selectedFighters[x], new Vector2(players[x].X + 10, players[x].Y + 10), Color.LightGray);
            }
            for (int x = 0; x < fighters.Length; x++)
            {
                spriteBatch.Draw(blank, fighters[x], Color.White);
                spriteBatch.DrawString(font, fighterNames[x], new Vector2(fighters[x].X + 10, fighters[x].Y + 10), Color.Red);
                //only draw in colors if that fighter is selected

                //player 1
                if (selections[0] == x)
                {
                    if (ready[0])
                        spriteBatch.Draw(blank, playerOneOptions[x], Game1.DefaultColors[0]);
                    else
                        spriteBatch.Draw(blank, playerOneOptions[x], alt[0]);
                }
                else
                    spriteBatch.Draw(blank, playerOneOptions[x], Color.White);

                //player 2
                if (selections[1] == x)
                {
                    if (ready[1])
                        spriteBatch.Draw(blank, playerTwoOptions[x], Game1.DefaultColors[1]);
                    else
                        spriteBatch.Draw(blank, playerTwoOptions[x], alt[1]);
                }
                else
                    spriteBatch.Draw(blank, playerTwoOptions[x], Color.White);

                //player 3
                if (numOfPlayers > 2)
                {
                    if (selections[2] == x)
                    {
                        if (ready[2])
                            spriteBatch.Draw(blank, playerThreeOptions[x], Game1.DefaultColors[2]);
                        else
                            spriteBatch.Draw(blank, playerThreeOptions[x], alt[2]);
                    }
                    else
                        spriteBatch.Draw(blank, playerThreeOptions[x], Color.White);   
                }

                //player 4
                if (numOfPlayers > 3)
                {
                    if (selections[3] == x)
                    {
                        if (ready[3])
                            spriteBatch.Draw(blank, playerFourOptions[x], Game1.DefaultColors[3]);
                        else
                            spriteBatch.Draw(blank, playerFourOptions[x], alt[3]);
                    }
                    else
                        spriteBatch.Draw(blank, playerFourOptions[x], Color.White);
                }
            }
            if (ReadyToStart())
            {
                spriteBatch.Draw(blank, startStrip, Color.Orange);
                spriteBatch.DrawString(font, "Press Start to begin.", new Vector2(startStrip.X + 10, startStrip.Y + 10), Color.White);
            }
        }

        public override bool ReadyForNextScreen()
        {
            return started;
        }
    }
}
