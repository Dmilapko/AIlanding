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

namespace AIlanding
{
    class Logic3AI5:GameLogic
    {
        AITYPE aim, ail;
        MyAIVisualizer visualizer;

        int start_time = Program.Now;
        int phase = 1;
        string s1 = @"AI5\PHASE1\" + Program.AI1file;
        string s2 = @"AI5\PHASE2\" + Program.AI2file;
        int surf_level = 0;

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
            visualizer = new MyAIVisualizer(aim);
        }

        public override void RunLogic()
        {
            visualizer.Check(Mouse.GetState(), Keyboard.GetState());
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                start_time = Program.Now;
                phase = 1;
                ail.Reset();
                aim.Reset();
                modules.Clear();
                modules.Add(new LandingModule());
                CreateSurface(surf_level);
                Find_Landing_Site();
                Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            }
            if (phase == 1)
            {
                double dist = Math.Abs(Program.landing_target.X * 10 - 960) / 10;
                double rtime = 2 * Math.Sqrt(dist) * 60;
                if (Program.Now - start_time > rtime)
                {
                    phase = 2;
                    visualizer = new MyAIVisualizer(ail);
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

        public override void DrawLogic(SpriteBatch spriteBatch)
        {
            visualizer.drawposition = new Vector2(850, 850);
            visualizer.DrawNetwork(spriteBatch);
        }
    }
}
