using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace PrisonStep
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrisonGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        /// <summary>
        /// This graphics device we are drawing on in this assignment
        /// </summary>
        GraphicsDeviceManager graphics;


        private PSLineDraw lineDraw;
        public PSLineDraw LineDraw { get { return lineDraw; } }

        /// <summary>
        /// The camera we use
        /// </summary>
        private Camera camera;

        /// <summary>
        /// The player in your game is modeled with this class
        /// </summary>
        private Player player;

        /// <summary>
        /// This is the actual model we are using for the prison
        /// </summary>
        public List<PrisonModel> phibesModel = new List<PrisonModel>();

        #endregion

        #region Properties

        /// <summary>
        /// The game camera
        /// </summary>
        public Camera Camera { get { return camera; } }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public PrisonGame()
        {
            // XNA startup
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create objects for the parts of the ship
            for(int i=1;  i<=6;  i++)
            {
                phibesModel.Add(new PrisonModel(this, i));
            }

            // Create a player object
            player = new Player(this);

            // Some basic setup for the display window
            this.IsMouseVisible = true;
			this.Window.AllowUserResizing = true;
			this.graphics.PreferredBackBufferWidth = 1024;
			this.graphics.PreferredBackBufferHeight = 728;

            // Basic camera settings
            camera = new Camera(graphics);
            camera.FieldOfView = MathHelper.ToRadians(60);
            camera.Eye = new Vector3(800, 180, 1053);
            camera.Center = new Vector3(275, 90, 1053);
            lineDraw = new PSLineDraw(this, Camera);
            this.Components.Add(lineDraw);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
            player.LoadContent(Content);

            foreach (PrisonModel model in phibesModel)
            {
                model.LoadContent(Content);
            }
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

            lineDraw.Clear();
       
             // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            //
            // Update game components
            //

            player.Update(gameTime);

            //done in player class
            //foreach (PrisonModel model in phibesModel)
            //{
            //    model.Update(gameTime);
            //}

            camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            foreach (PrisonModel model in phibesModel)
            {
                model.Draw(graphics, gameTime);
            }

            player.Draw(graphics, gameTime);

            base.Draw(gameTime);
            player.SpitDraw(graphics, gameTime);
        }
    }
}
