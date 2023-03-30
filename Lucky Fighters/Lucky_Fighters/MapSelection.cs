using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class MapSelection : Screen
    {
        const int PADDING = 5;
        int timer;
        int sw;
        int sh;
        string[] levelPaths;
        Rectangle[] levels;
        Texture2D[] images;
        Rectangle[] options;
        Texture2D blank;
        int selection;
        Color[] alt;
        GamePadState[] oldGP;
        GamePadState[] gp;
        bool ready;
        
        public Direction direction { get; private set; }
        public ContentManager Content { get; }
        public MapSelection (IServiceProvider _serviceProvider, int sw, int sh, int numOfPlayers)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
            selection = 0;
            timer = 0;
            oldGP = new GamePadState[numOfPlayers];
            gp = new GamePadState[numOfPlayers];
            ready = false;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        { 
            for (int x = 0; x < levels.Length; x++)
            {
                if (x == selection)
                    spriteBatch.Draw(blank, options[selection], alt[selection]);
                spriteBatch.Draw(images[x], levels[x], Color.White);
            }

        }

        public override void LoadContent()
        {
            //get level paths to pass into gameplay in next screen
            string path = Directory.GetCurrentDirectory() + "/Content/Maps";
            levelPaths = Directory.GetFiles(path);
            
            //get images for level selection display
            path = Directory.GetCurrentDirectory() + "/Content/MapImages";
            string[] paths = Directory.GetFiles(path);
            images = new Texture2D[levelPaths.Length];
            for (int x = 0; x < paths.Length; x++)
            {
                try
                {
                    int start = paths[x].LastIndexOf("Map");
                    int length = paths[x].LastIndexOf(".") - start;

                    images[x] = Content.Load<Texture2D>(paths[x].Substring(start, length));
                }
                catch (IndexOutOfRangeException e) //for levels that don't have an image yet
                {
                    images[x] = Content.Load<Texture2D>("blank");
                }
            }

            //create box to display level in
            levels = new Rectangle[levelPaths.Length];
            options = new Rectangle[levels.Length];
            alt = new Color[levels.Length];
            for (int x = 0; x < alt.Length; x++)
            {
                alt[x] = GetColor();
            }
            int rectWidth = (sw - 200 - levels.Length * 20) / levels.Length;
            int rectHeight = rectWidth * sh / sw; //proportional to level dimensions with 100 px padding on each side
            for (int x = 0; x < levels.Length; x++)
            {
                levels[x] = new Rectangle(100 + rectWidth * x + 20* x, 100, rectWidth, rectHeight);
                options[x] = new Rectangle(levels[x].X - PADDING, levels[x].Y - PADDING, levels[x].Width + 2 * PADDING, levels[x].Height + 2 * PADDING);
            }
            blank = Content.Load<Texture2D>("blank");
        }

        private void Hover()
        {
            if (alt[selection] == Color.Transparent)
                alt[selection] = Color.White;
            else
                alt[selection] = Color.Transparent;
        }

        public override bool ReadyForNextScreen()
        {
            return ready;
        }

        public override void Update(GameTime gameTime)
        {
            timer++;
            if (timer > 10) //avoid taking input from previous screen
                GetInput();
            if (timer % 10 == 0)
                Hover();
        }

        private void GetInput()
        {
            for (int x = 0; x < gp.Length; x++)
            {
                gp[x] = GamePad.GetState((PlayerIndex)x);
                if (gp[x].DPad.Left == ButtonState.Pressed && !(oldGP[x].DPad.Left == ButtonState.Pressed))
                    selection--;
                if (gp[x].DPad.Right == ButtonState.Pressed && !(oldGP[x].DPad.Right == ButtonState.Pressed))
                    selection++;
                if (gp[x].ThumbSticks.Left.X < 0 && !(oldGP[x].ThumbSticks.Left.X < 0))
                    selection--;
                if (gp[x].ThumbSticks.Left.X > 0 && !(oldGP[x].ThumbSticks.Left.X > 0))
                    selection++;
            }
            if (selection < 0)
                selection = levels.Length - 1;
            if (selection > levels.Length - 1)
                selection = 0;
            for (int x = 0; x < gp.Length; x++)
            {
                if (gp[x].Buttons.A == ButtonState.Pressed)
                {
                    direction = Direction.Forward;
                    ready = true;
                }
                else if (gp[x].Buttons.Back == ButtonState.Pressed)
                {
                    direction = Direction.Backward;
                    ready = true;
                }
                oldGP[x] = gp[x];
            }
        }

        public int getMap()
        {
            return selection;
        }

        public override Color GetColor()
        {
            return Color.DimGray;
        }
    }
}
