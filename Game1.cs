using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using Physics;
using MonoHelper;

namespace AIlanding
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        List<MatrixPhysics.MP_Object> modules = new List<MatrixPhysics.MP_Object>();
        MatrixPhysics physics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.Position = new Point(0, 0);
            graphics.ApplyChanges();
            modules.Add(new LandingModule());
            physics = new MatrixPhysics(modules, new List<List<bool>>(), 10);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Program.marstexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/Mars.png", FileMode.Open));
            Program.flametexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/EngineFlame.png", FileMode.Open));
            Program.moduletexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/LandingModule.png", FileMode.Open));
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            physics.Run();
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatch.Draw(Program.marstexture, new Vector2(0, 0), Color.White);
            foreach (var module in modules)
            {
                ((LandingModule)module).Draw(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
