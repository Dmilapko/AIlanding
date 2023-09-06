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
using AITYPE = AILib.Clever_AI;

namespace AIlanding
{
    class Logic4 : GameLogic
    {
        AITYPE aim, ail;
        int start_time = Program.Now;
        int phase = 0;

        public void CalcPos(int pos)
        {
            int startpos = pos, finpos = pos;
            while (new PointD(finpos - startpos, physics.matrix[finpos] - physics.matrix[startpos]).Length() < 81)
            {
                startpos--;
                finpos++;
            }
            Program.landing_target = new PointD((startpos + finpos) / 2d / 10d, (physics.matrix[startpos] + physics.matrix[finpos]) / 2d / 10d);
            Program.landing_angle = new PointD(finpos - startpos, physics.matrix[finpos] - physics.matrix[startpos]).Angle() - Math.PI / 2;
        }

        public Logic4(int size)
        {
            modules.Add(new LandingModule());
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(size);
            Find_Landing_Site();
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            Program.alive_m = 1;
            string s1 = @"AI_Phase1\" + Program.AI1file;
            if (File.Exists(s1) && (File.ReadAllText(s1) != ""))
            {
                aim = new AITYPE(AITYPE.ReadFromFile(s1), 1);
            }
            else
            {
                throw new Exception("Файл '" + s1 + "' не знайдено");
            }
            string s2 = @"AI_Phase2\" + Program.AI2file;
            if (File.Exists(s2) && (File.ReadAllText(s2) != ""))
            {
                ail = new AITYPE(AITYPE.ReadFromFile(s2), 1);
            }
            else
            {
                throw new Exception("Файл '" + s2 + "' не знайдено");
            }
        }

        public override void RunLogic()
        {
            if (phase == 0)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) CalcPos((int)(Program.landing_target.X * 10 - 3));
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) CalcPos((int)(Program.landing_target.X * 10 + 3));
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    phase = 1;
                    start_time = Program.Now;
                }
            }
            else if (phase == 1)
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
                    ((LandingModule)modules[0]).Run_AI_PHASE1(aim);
                    physics.Run();
                }
            }
            else
            {
                LandingModule module = (LandingModule)modules[0];
                if (module.alive)
                {
                    module.Make_AI_params();
                    module.Run_AI_PHASE2(ail);
                    if (module.beforetime != -1) if (Program.Now - module.beforetime > 240) module.ResetInputs();
                }
                physics.Run();
            }
        }
    }
}
