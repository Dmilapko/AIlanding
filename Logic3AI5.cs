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
using AILib;
using AITYPE = AIlanding.MyAI5;
using System.Xml.Linq;
using System.Linq;

namespace AIlanding
{
    class Logic3AI5 : GameLogic
    {
        AITYPE aim, ail;
        AI5Visualizer visualizerm;
        AI5Visualizer visualizerl;

        int start_time = Program.Now;
        int phase = 1;
        string s1 = @"AI5\PHASE1\" + Program.AI1file;
        string s2 = @"AI5\PHASE2\" + Program.AI2file;
        int surf_level = 0;
        bool p_e = false;
        bool running = false;
        double wind_accelration = 0;
        double wind_accelrationc = 0;
        double stc = 0;

        public Logic3AI5(int size)
        {
            surf_level = size;
            modules.Add(new LandingModule());
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(size);
            Find_Landing_Site();
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            Program.alive_m = 1;
            if (File.Exists(s1) && (File.ReadAllText(s1) != ""))
            {
                aim = MHeleper.ReadFromBinaryFile<List<AITYPE>>(s1)[0];
                aim.Reset();
            }
            else
            {
                throw new Exception("Файл '" + s1 + "' не знайдено");
            }
            if (File.Exists(s2) && (File.ReadAllText(s2) != ""))
            {
                ail = MHeleper.ReadFromBinaryFile<List<AITYPE>>(s2)[0];
                ail.Reset();
            }
            else
            {
                throw new Exception("Файл '" + s2 + "' не знайдено");
            }

            visualizerm = new AI5Visualizer(Program.my_device, aim, 450, 200, Program.font10, Color.Blue, Color.Gray);
            visualizerl = new AI5Visualizer(Program.my_device, ail, 450, 200, Program.font10, Color.Blue, Color.Gray);
        }
        int st = 50;
        double rst = 50;


        public override void RunLogic()
        {
            visualizerm.Check(Mouse.GetState(), Keyboard.GetState());
            visualizerl.Check(Mouse.GetState(), Keyboard.GetState());
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                wind_accelrationc += 0.001;
                wind_accelration += wind_accelrationc;
            }
            else if (wind_accelrationc > 0) wind_accelrationc = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                wind_accelrationc -= 0.001;
                wind_accelration += wind_accelrationc;
            }
            else if (wind_accelrationc < 0) wind_accelrationc = 0;
            if (!running)
            {
                bool need_re = false;
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    stc -= 0.01;
                    rst += stc;
                    st = (int)rst;
                    if (st < 50) st = 50;
                    need_re = true;
                }
                else if (stc < 0) stc = 0;
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    stc += 0.01;
                    rst += stc;
                    st = (int)rst;
                    if (st > 1800) st = 1800;
                    need_re = true;
                }
                else if (stc > 0) stc = 0;
                if (need_re)
                {
                    int fn = st;
                    double length = new PointD(fn - st, physics.matrix[fn] - physics.matrix[st]).Length();
                    while (length > 81)
                    {
                        fn--;
                        length = new PointD(fn - st, physics.matrix[fn] - physics.matrix[st]).Length();
                    }
                    while (length < 81)
                    {
                        fn++;
                        length = new PointD(fn - st, physics.matrix[fn] - physics.matrix[st]).Length();
                    }
                    double d = Math.Atan2(fn - st, physics.matrix[fn] - physics.matrix[st]);
                    Program.landing_target = new PointD((st + fn) / 2d, (physics.matrix[st] + physics.matrix[fn]) / 2d);
                    Program.landing_target /= 10;
                    Program.landing_angle = d - Math.PI / 2d;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (!p_e)
                {
                    p_e = true;
                    running = !running;
                    if (!running)
                    {
                        phase = 1;
                        ail.Reset();
                        aim.Reset();
                        modules.Clear();
                        modules.Add(new LandingModule());
                        CreateSurface(surf_level);
                        Find_Landing_Site();
                        Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
                    }
                    else start_time = Program.Now;
                }
            }
            else p_e = false;
            if (running)
            {
                modules[0].speed.X += wind_accelration / Program.fps;
                if (phase == 1)
                {
                    double dist = Math.Abs(Program.landing_target.X * 10 - 960) / 10;
                    double rtime = 2 * Math.Sqrt(dist) * 60;
                    if (Program.Now - start_time > rtime)
                    {
                        phase = 2;
                    }
                    else
                    {
                        ((LandingModule)modules[0]).Make_AI_params_PHASE1(Program.landing_target.X);
                        ((LandingModule)modules[0]).Run_AI5_PHASE1(aim);
                        physics.Run();
                    }
                }
                else
                {
                    LandingModule module = (LandingModule)modules[0];
                    if (module.alive)
                    {
                        module.Make_AI_params();
                        module.Run_AI5_PHASE2(ail);
                        if (module.ever_collided[0] && module.ever_collided[1]) module.ResetInputs();
                    }
                    physics.Run();
                }
            }
        }

        public override void DrawLogic(SpriteBatch spriteBatch)
        {
            visualizerm.drawposition = new Vector2(500, 860);
            visualizerm.DrawNetwork(spriteBatch);
            visualizerl.drawposition = new Vector2(950, 860);
            visualizerl.DrawNetwork(spriteBatch);
            spriteBatch.DrawString(Program.font20, "Wind: " + ((float)wind_accelration).ToString() + "m/s", new Vector2(1430, 920), Color.Black);
        }
    }
}
