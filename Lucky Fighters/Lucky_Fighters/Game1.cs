using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucky_Fighters
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const int GameWidth = 1344;
        public const int GameHeight = 768;

        public static readonly Color[] DefaultColors = new Color[] { Color.Blue, Color.Red, Color.Green, Color.Yellow };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Screen screen;
        int numOfPlayers;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = GameWidth;
            graphics.PreferredBackBufferHeight = GameHeight;
            // allows for 14x8 map
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            int sw = graphics.PreferredBackBufferWidth;
            int sh = graphics.PreferredBackBufferHeight;
			SetScreen(new NumberOfPlayerSelection(Services, sw, sh));
			// testing
			SetScreen(new Map(Services, @"Content\Maps\map1.txt", new string[] { "swordfighter", "swordfighter" }));
		}

		public void SetScreen (Screen screen)
        {
            this.screen = screen;
            screen.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            // TODO: Add your update logic here
            //map.Update(gameTime);
            screen.Update(gameTime);
            if (screen.ReadyForNextScreen() && screen is NumberOfPlayerSelection)
            {
                NumberOfPlayerSelection alt = (NumberOfPlayerSelection)screen;
                numOfPlayers = alt.Num;
                SetScreen(new MapSelection(Services, GameWidth, GameHeight));
            }
            if (screen.ReadyForNextScreen() && screen is MapSelection)
            {
                MapSelection alt = (MapSelection)screen;
                SetScreen(new FighterSelection(Services, GameWidth, GameHeight, numOfPlayers));
            }
            if (screen.ReadyForNextScreen() && screen is FighterSelection)
            {
                FighterSelection alt = (FighterSelection)screen;
                if (alt.direction == Screen.Direction.Forward)
                    SetScreen(new Map(Services, @"Content\Maps\map1.txt", alt.SelectedFighters()));
                else
                    SetScreen(new NumberOfPlayerSelection(Services, GameWidth, GameHeight));
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(screen.GetColor());

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            screen.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
