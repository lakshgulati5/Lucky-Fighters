using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    class MapSelection : Screen
    {
        int sw;
        int sh;
        string[] levelPaths;
        Rectangle[] levels;
        Texture2D[] images;

        public ContentManager Content { get; }
        public MapSelection (IServiceProvider _serviceProvider, int sw, int sh)
        {
            Content = new ContentManager(_serviceProvider, "Content");
            this.sw = sw;
            this.sh = sh;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        { 
            for (int x = 0; x < levels.Length; x++)
            {
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
            int rectWidth = (sw - 200 - levels.Length * 20) / levels.Length;
            int rectHeight = rectWidth * sh / sw; //proportional to level dimensions with 100 px padding on each side
            for (int x = 0; x < levels.Length; x++)
            {
                levels[x] = new Rectangle(100 + rectWidth * x + 20* x, 100, rectWidth, rectHeight);
            }
        }

        public override bool ReadyForNextScreen()
        {
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override Color GetColor()
        {
            return Color.DimGray;
        }
    }
}
