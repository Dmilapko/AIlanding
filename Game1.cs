using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using Physics;
using MonoHelper;
using System.Timers;
using MonogameButton;
using MonogameTextBoxLib;
using MonogameLabel;
using FormElementsLib;

namespace AIlanding
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        bool pf = false, pg = false, pp = false, pt = false;
        int fpscnt = 0;
        List<FormElement> elements = new List<FormElement>();
        static public Label fpslabel, maxglabel, vxlabel, vylabel, masslabel, fuellabel, glabel, ai1label, ai2label, generationlabel, maxscorelabel, modelabel, levellabel, AIdifx, AIdify, AIdifa, pointslabel, forceslabel, targetlabel, graphicslabel;
        Button reset_button;
        TextBox modetext, ai1text, ai2text, leveltext;
        GameLogic logic;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            this.IsFixedTimeStep = true;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / Program.fps); //60);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.Position = new Point(0, 0);
            graphics.ApplyChanges();
            Program.my_device = GraphicsDevice;
            base.Initialize();
        }

        private void Fpstimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            fpslabel.text = "FPS " + fpscnt.ToString();
            fpscnt = 0;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Program.font20 = Content.Load<SpriteFont>(@"font20");
            Program.font10 = Content.Load<SpriteFont>(@"font10");
            Program.font15 = Content.Load<SpriteFont>(@"font15");
            Program.AI_params = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/AI_params.png", FileMode.Open));
            Program.marstexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/Mars.png", FileMode.Open));
            Program.flametexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/EngineFlame.png", FileMode.Open));
            Program.moduletexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/LandingModule.png", FileMode.Open));
            Program.gforcetexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/gforce_meter.png", FileMode.Open));
            Program.deadmoduletexture = Program.moduletexture.Clone(GraphicsDevice);
            Program.vectortexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/Vector.png", FileMode.Open));
            Program.displaytexture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/Module_display.png", FileMode.Open));
            Program.deadmoduletexture.FillNot(new Vector2(25, 11), Color.Transparent, Color.Red);
            Program.recttexture = new Texture2D(GraphicsDevice, 2, 2);
            Program.attidude_indicator_texture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/attitude_indicator.png", FileMode.Open));
            Program.attidude_indicator_line_texture = Texture2D.FromStream(GraphicsDevice, new FileStream("MyContent/attitude_indicator_line.png", FileMode.Open));
            Program.boxtexture = new Texture2D(GraphicsDevice, 504, 24);
            Color[] cd = new Color[504 * 24];
            for (int i = 0; i < 504 * 24; i++)
            {
                cd[i] = Color.Black;
            }
            Program.boxtexture.SetData(cd);
            Color[] ncd = new Color[4];
            ncd[0] = ncd[1] = ncd[2] = ncd[3] = Color.White;
            Program.recttexture.SetData(ncd);
            Program.vectortexture.MakeTransparent(Color.White);
            Program.surfacetexture = new Texture2D(GraphicsDevice, 1920, 1080);
            //form elements
            fpslabel = new Label(1820, 20, -1, -1, "FPS", Program.font20, 20, 20);
            elements.Add(fpslabel);
            vxlabel = new Label(1630, 920, -1, -1, "Vx: 5m/s", Program.font20, 20, 10);
            elements.Add(vxlabel);
            vylabel = new Label(1630, 940, -1, -1, "Vy: 5m/s", Program.font20, 20, 10);
            elements.Add(vylabel);
            fuellabel = new Label(1630, 960, -1, -1, "Fuel: 9879kg(98%)", Program.font20, 20, 10);
            elements.Add(fuellabel);
            masslabel = new Label(1630, 980, -1, -1, "Mass: 19879kg", Program.font20, 20, 10);
            elements.Add(masslabel);
            glabel = new Label(1815, 918, 30, -1, "1g", Program.font10, 20, 13);
            glabel.textcolor = Color.Yellow;
            elements.Add(glabel);
            maxglabel = new Label(1730, 1050, 190, -1, "Max. g: 100.51g", Program.font20, 20, 17);
            elements.Add(maxglabel);
            AIdifx = new Label(1427, 990, -1, -1, "Dif. x: 456.81m", Program.font20, 20, 17);
            elements.Add(AIdifx);
            AIdify = new Label(1427, 1020, -1, -1, "Dif. x: 456.81m", Program.font20, 20, 17);
            elements.Add(AIdify);
            AIdifa = new Label(1427, 1050, -1, -1, "Dif. a: 132.72deg", Program.font20, 20, 17);
            elements.Add(AIdifa);
            //--------
            modelabel = new Label(10, 900, -1, -1, "Mode:", Program.font20, 20, 20);
            elements.Add(modelabel);
            ai1label = new Label(10, 1005, -1, -1, "Phase 1 AI:", Program.font20, 20, 20);
            elements.Add(ai1label);
            ai2label = new Label(10, 1040, -1, -1, "Phase 2 AI:", Program.font20, 20, 20);
            elements.Add(ai2label);
            generationlabel = new Label(10, 970, -1, -1, "Generation:", Program.font20, 20, 20);
            elements.Add(generationlabel);
            maxscorelabel = new Label(10, 935, -1, -1, "Max. score:", Program.font20, 20, 20);
            elements.Add(maxscorelabel);
            reset_button = new Button(GraphicsDevice, 10, 860, 100, 35, "RESET", Program.font20, 20, 20);
            elements.Add(reset_button);
            modetext = new TextBox(GraphicsDevice, 90, 900, 25, -1, Program.font20, 20, 17);
            modetext.text = Program.mode.ToString();
            elements.Add(modetext);
            ai1text = new TextBox(GraphicsDevice, 160, 1005, 170, -1, Program.font20, 20, 17);
            ai1text.text = Program.AI1file;
            elements.Add(ai1text);
            ai2text = new TextBox(GraphicsDevice, 160, 1040, 170, -1, Program.font20, 20, 17);
            ai2text.text = Program.AI2file;
            elements.Add(ai2text);
            reset_button.Click += Reset_button_Click;
            levellabel = new Label(130, 900, -1, -1, "Level:", Program.font20, 20, 20);
            elements.Add(levellabel);
            leveltext = new TextBox(GraphicsDevice, 205, 900, 25, -1, Program.font20, 20, 17);
            leveltext.text = "1";
            elements.Add(leveltext);
            pointslabel = new Label(10, 10, -1, -1, "Points: True", Program.font20, 20, 10);
            elements.Add(pointslabel);
            forceslabel = new Label(110, 10, -1, -1, "Forces: True", Program.font20, 20, 10);
            elements.Add(forceslabel);
            targetlabel = new Label(210, 10, -1, -1, "Target: True", Program.font20, 20, 10);
            elements.Add(targetlabel);
            graphicslabel = new Label(310, 10, -1, -1, "Graphics: True", Program.font20, 20, 10);
            elements.Add(graphicslabel);
            //---------
            Program.gforceballtexture = new Texture2D(GraphicsDevice, 10, 10);
            Program.gforceballtexture.Fill(Color.Transparent);
            Program.gforceballtexture.DrawCircle(new Vector2(4.5f, 4.5f), 0, 5, Color.Yellow);
            Program.landing_texture = new Texture2D(GraphicsDevice, 86, 4);
            Program.landing_texture.Fill(Color.Blue);
            SetMode(Program.mode);
            Timer fpstimer = new Timer();
            fpstimer.Elapsed += Fpstimer_Elapsed;
            fpstimer.Interval = 1000;
            fpstimer.Enabled = true;
            // TODO: use this.Content to load your game content here
            /* test = new Texture2D(GraphicsDevice, 100, 100);
             test.Fill(Color.Transparent);
             test.DrawCircle(new Vector2(50, 50), 40, 1.25, Color.Green);
             test.DrawCircle(new Vector2(50, 50), 30, 1.25, Color.Green);
             test.DrawCircle(new Vector2(50, 50), 20, 1.25, Color.Green);
             test.DrawCircle(new Vector2(50, 50), 10, 1.25, Color.Green);
             test.SaveAsPng(new FileStream("test.png", FileMode.Create), 100, 100);*/
        }

        private void Reset_button_Click(object sender, ClickEventArgs e)
        {
            Program.AI1file = Program.setupProp["AI1file"] = ai1text.text;
            Program.AI2file = Program.setupProp["AI2file"] = ai2text.text;
            Program.ChangeSetup();
            switch (modetext.text)
            {
                case "0":
                    SetMode(0);
                    break;
                case "1":
                    SetMode(1);
                    break;
                case "2":
                    SetMode(2);
                    break;
                case "3":
                    SetMode(3);
                    break;
                case "4":
                    SetMode(4);
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                if (!pf) Program.press_f = !Program.press_f;
                pf = true;
            }
            else pf = false;
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                if (!pg) Program.press_g = !Program.press_g;
                pg = true;
            }
            else pg = false;
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                if (!pp) Program.press_p = !Program.press_p;
                pp = true;
            }
            else pp = false;
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                if (!pt) Program.press_t = !Program.press_t;
                pt = true;
            }
            else pt = false;
            foreach (var element in elements)
            {
                element.Check(Mouse.GetState(), Keyboard.GetState());
            }
            logic.RunLogic();
            // TODO: Add your update logic here
            base.Update(gameTime);
            fpscnt++;
            Program.Now++;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatch.Draw(Program.marstexture, new Vector2(0, 0), Color.White);
            spriteBatch.Draw(Program.surfacetexture, new Vector2(0, 0), null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.FlipVertically, 1);
            foreach (LandingModule module in logic.modules)
            {
                if (!module.alive) module.Draw(spriteBatch);
            }
            foreach (LandingModule module in logic.modules)
            {
                if (module.alive) module.Draw(spriteBatch);
            }
            if (Program.press_t) spriteBatch.Draw(Program.landing_texture, new Vector2((float)Program.landing_target.X * 10, 1080 - (float)Program.landing_target.Y * 10), null, Color.White, (float)Program.landing_angle, new Vector2(43, 2), 1f, SpriteEffects.None, 1);
            if (Program.show_display)
            {
                spriteBatch.Draw(Program.displaytexture, new Vector2(1620, 910), Color.White);
                vxlabel.text = "Vx: " + Math.Round(logic.modules[0].speed.X, 1).ToString() + " m/s";
                vylabel.text = "Vy: " + Math.Round(logic.modules[0].speed.Y, 1).ToString() + " m/s";
                fuellabel.text = "Fuel: " + ((int)Math.Round(((LandingModule)logic.modules[0]).fuelmass)).ToString() + " kg (" + ((int)Math.Round(((LandingModule)logic.modules[0]).fuelmass / 80d)).ToString() + "%" + ")";
                masslabel.text = "Mass: " + ((int)Math.Round(logic.modules[0].mass)).ToString() + " kg";
                double g = Math.Min(16, ((LandingModule)logic.modules[0]).gforce);
                double g_a = ((LandingModule)logic.modules[0]).gforce_dir;
                maxglabel.text = "MAX gforce: " + Math.Round(((LandingModule)logic.modules[0]).maxg, 1).ToString() + "g";
                glabel.text = Math.Round(g, 1).ToString() + "g";
                //glabel.text = "10.2g";
                spriteBatch.Draw(Program.gforcetexture, new Vector2(1830, 964), null, Color.White, 0, new Vector2(50, 50), 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(Program.gforceballtexture, new Vector2((float)(1830 + Math.Sqrt(g) * 10 * Math.Sin(g_a)), (float)(964 - Math.Sqrt(g) * 10 * Math.Cos(g_a))), null, Color.White, 0, new Vector2(4.5f, 4.5f), 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(Program.attidude_indicator_texture, new Vector2(1680, 1035), null, Color.White, 0, new Vector2(42, 38), 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(Program.attidude_indicator_line_texture, new Vector2(1680, 1035), null, Color.White, (float)logic.modules[0].rotation, new Vector2(29, 9), 1f, SpriteEffects.None, 1);
                //---------------------------------------------------------------------------------------------------
                spriteBatch.Draw(Program.AI_params, new Vector2(1422, 960), Color.White);
                AIdifx.text = "Dif. x[m]: " + Math.Round(((LandingModule)logic.modules[0]).dif_position.X, 2).ToString();
                AIdify.text = "Dif. y[m]: " + Math.Round(((LandingModule)logic.modules[0]).dif_position.Y, 2).ToString();
                AIdifa.text = "Dif. a[rad]: " + Math.Round(((LandingModule)logic.modules[0]).dif_angle, 3).ToString();
            }
            if (Program.mode ==  1)
            {
                /*generationlabel.text = "Generation: " + ((Logic1)logic).ais[0].generation;
                float gd = (float)((1 - ((Logic1)logic).ais[0].mygoodness) * 2400);
                maxscorelabel.text = "Max. Score: " + gd.ToString();*/
            }
            if (Program.mode == 2)
            {
          /*      generationlabel.text = "Generation: " + ((Logic2)logic).ais[0].generation;
                float gd = (float)((1 - ((Logic2)logic).ais[0].mygoodness) * 300) / 5;
                maxscorelabel.text = "Max. Score: " + gd.ToString();*/
            }
            pointslabel.text = "Points: " + Program.press_p.ToString();
            forceslabel.text = "Forces: " + Program.press_f.ToString();
            graphicslabel.text = "Graphics: " + Program.press_g.ToString();
            targetlabel.text = "Target: " + Program.press_t.ToString();
            logic.DrawLogic(spriteBatch);
            foreach (var element in elements)
            {
                element.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        int GetSize()
        {
            switch (leveltext.text)
            {
                case "2":
                    return Program.levels[1];
                case "3":
                    return Program.levels[2];
                case "4":
                    return Program.levels[3];
                case "5":
                    return Program.levels[4];
                default:
                    return Program.levels[0];
            }
        }

        void SetMode(int mode)
        {
            Program.mode = mode;
            switch (Program.mode)
            {
                case 0:
                    logic = new Logic0(GetSize());
                    break;
                case 1:
                    logic = new Logic1AI5();
                    break;
                case 2:
                    logic = new Logic2AI5();
                    break;
                case 3:
                    logic = new Logic3AI5(GetSize());
                    break;
                case 4:
                    logic = new Logic4(GetSize());
                    break;
                default:
                    break;
            }
        }
    }
}
