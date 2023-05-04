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
        const float JoystickTolerance = .5f;

        string[] teamColorStrings = new string[] { "Blue", "Red", "Green", "Yellow" };
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
        KeyboardState oldKb;
        public bool started;
        Mode mode;
        Color[] teamOptions;
        int[] selectedTeam;
        const int implementedFighters = 2;
        public Direction direction { get; private set; }

        public int[] SelectedTeams { get { return selectedTeam; } }

        public FighterSelection (IServiceProvider _serviceProvider, int sw, int sh, int nOfPlayers, Mode mode) : base(_serviceProvider)
        {
            timer = 0;
            numOfPlayers = nOfPlayers;
            selections = new int[numOfPlayers];
            gp = new GamePadState[numOfPlayers];
            oldGP = new GamePadState[numOfPlayers];
            oldKb = Keyboard.GetState();
            alt = new Color[numOfPlayers];
            if (numOfPlayers < 2)
                throw new Exception("Need at least two players to play");
            screenWidth = sw;
            screenHeight = sh;
            players = new Rectangle[numOfPlayers];
            borders = new Rectangle[numOfPlayers];
            fighters = new Rectangle[5];
            playerOneOptions = new Rectangle[fighters.Length];
            playerTwoOptions = new Rectangle[fighters.Length]; 
            playerThreeOptions = new Rectangle[fighters.Length];
            playerFourOptions = new Rectangle[fighters.Length];
            fighterNames = new string[] { "swordfighter", "archer", "ninja", "wizard", "muscleman" };
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
            this.mode = mode;
            teamOptions = new Color[] { Game1.DefaultColors[0], Game1.DefaultColors[1], Game1.DefaultColors[2], Game1.DefaultColors[3] };
            selectedTeam = new int[numOfPlayers];
            for (int x = 0; x < numOfPlayers; x++)
            {
                selectedTeam[x] = x;
            }
        }

        public override void LoadContent()
        {
            blank = Content.Load<Texture2D>("blank");
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        public override void Update(GameTime gameTime)
        {
            timer++;
            if (timer > 10) //avoid taking input from previous screen
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
            for (int x = 0; x < numOfPlayers; x++)
            {
                gp[x] = GamePad.GetState((PlayerIndex)x);
            }
            KeyboardState kb = Keyboard.GetState();
            for (int x = 0; x < numOfPlayers; x++)
            {
                if (!ready[x])
                {
                    //select by dpad, thumbstick, or keyboard
                    if (gp[x].DPad.Right == ButtonState.Pressed && !(oldGP[x].DPad.Right == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X > JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X > JoystickTolerance) ||
                        x == 1 && kb.MenuRightPressed() && !oldKb.MenuRightPressed())
                        selections[x]++;
                    if (gp[x].DPad.Left == ButtonState.Pressed && !(oldGP[x].DPad.Left == ButtonState.Pressed) ||
                        gp[x].ThumbSticks.Left.X < -JoystickTolerance && !(oldGP[x].ThumbSticks.Left.X < -JoystickTolerance) ||
                        x == 1 && kb.MenuLeftPressed() && !oldKb.MenuLeftPressed())
                        selections[x]--;

                    //keep in bounds
                    if (selections[x] >= implementedFighters)
                        selections[x] = 0;
                    if (selections[x] < 0)
                        selections[x] = implementedFighters - 1;

                    selectedFighters[x] = fighterNames[selections[x]];

                    //change team
                    if (gp[x].Buttons.RightShoulder == ButtonState.Pressed && !(oldGP[x].Buttons.RightShoulder == ButtonState.Pressed))
                        selectedTeam[x]++;
                    if (gp[x].Buttons.LeftShoulder == ButtonState.Pressed && !(oldGP[x].Buttons.LeftShoulder == ButtonState.Pressed))
                        selectedTeam[x]--;

                    //keep in bounds
                    if (selectedTeam[x] < 0)
                        selectedTeam[x] = 3;
                    if (selectedTeam[x] >= 4)
                        selectedTeam[x] = 0;

                    //declare that the player is ready
                    if (gp[x].IsButtonDown(Buttons.A) ||
                        x == 1 && kb.GamePadAPressed() && !oldKb.GamePadAPressed())
                    {
                        ready[x] = true;
                    }
                }
                //unready
                if (gp[x].IsButtonDown(Buttons.B) ||
                        x == 1 && kb.GamePadBPressed() && !oldKb.GamePadBPressed())
                {
                    ready[x] = false;
                }

                if (ReadyToStart())
                {
                    if (gp[x].IsButtonDown(Buttons.Start) && oldGP[x].IsButtonUp(Buttons.Start) ||
                        x == 1 && kb.StartKeyPressed() && !oldKb.StartKeyPressed())
                    {
                        direction = Direction.Forward;
                        started = true;
                    }
                }
                if (gp[x].Buttons.Back == ButtonState.Pressed && oldGP[x].Buttons.Back == ButtonState.Released ||
                    x == 1 && kb.BackKeyPressed() && !oldKb.BackKeyPressed())
                {
                    direction = Direction.Backward;
                    started = true;
                }

                oldGP[x] = gp[x];
            }
            oldKb = kb;
        }

        private bool ReadyToStart()
        {
            //players all ready
            foreach (bool r in ready)
            {
                if (!r)
                    return false;
            }

            //valid teams
            if (mode == Mode.Team) {
                bool[] selected = new bool[4];
                foreach (int selection in selectedTeam)
                {

                    selected[selection] = true;
                }
                int count = 0;
                foreach (bool s in selected)
                {
                    if (s)
                        count++;
                }
                if (count < 2)
                    return false;
            }
            return true;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw cards
            for (int x = 0; x < players.Length; x++)
            {
                if (ready[x])
                    spriteBatch.Draw(blank, borders[x], Color.White);
                if (mode == Mode.Solo)
                    spriteBatch.Draw(blank, players[x], Game1.DefaultColors[x]);
                else 
                    spriteBatch.Draw(blank, players[x], teamOptions[selectedTeam[x]]);
                if (mode == Mode.Solo)
                {
                    if (ready[x])
                        spriteBatch.DrawString(font, "Player " + (x + 1) + "\n" + selectedFighters[x] + "\nReady", new Vector2(players[x].X + 10, players[x].Y + 10), Color.White);
                    else
                        spriteBatch.DrawString(font, "Player " + (x + 1) + "\n" + selectedFighters[x], new Vector2(players[x].X + 10, players[x].Y + 10), Color.LightGray);
                }
                else //team mode
                {
                    string c = teamColorStrings[selectedTeam[x]];
                    if (ready[x])
                        spriteBatch.DrawString(font, "Player " + (x + 1) + "\n[LB] " + c + " Team [RB]\n" + selectedFighters[x] + "\nReady", new Vector2(players[x].X + 10, players[x].Y + 10), Color.White);
                    else
                        spriteBatch.DrawString(font, "Player " + (x + 1) + "\n[LB] " + c + " Team [RB]\n" + selectedFighters[x], new Vector2(players[x].X + 10, players[x].Y + 10), Color.LightGray);
                }
            }
            for (int x = 0; x < fighters.Length; x++)
            {
                Color c = Color.White;
                if (x >= implementedFighters)
                    c = Color.SlateGray;
                spriteBatch.Draw(blank, fighters[x], c);
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
                    spriteBatch.Draw(blank, playerOneOptions[x], c);

                //player 2
                if (selections[1] == x)
                {
                    if (ready[1])
                        spriteBatch.Draw(blank, playerTwoOptions[x], Game1.DefaultColors[1]);
                    else
                        spriteBatch.Draw(blank, playerTwoOptions[x], alt[1]);
                }
                else
                    spriteBatch.Draw(blank, playerTwoOptions[x], c);

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
                        spriteBatch.Draw(blank, playerThreeOptions[x], c);   
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
                        spriteBatch.Draw(blank, playerFourOptions[x], c);
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

        public string[] SelectedFighters()
        {
            return selectedFighters;
        }

        public override Color GetColor()
        {
            return Color.DimGray;
        }
    }
}
