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
    class Logic1TTL:GameLogic
    {
        public List<AITYPE> ais = new List<AITYPE>();
        public int start_time;
        int pos = 760;
        List<double> letsgo = new List<double>();

        public Logic1TTL()
        {
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(100);
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            NextAIs();
        }

        void NextAIs()
        {
            pos = 710;
            ais.Clear();
            letsgo.Clear();
            bool ok = false;
            string s = @"AI_Phase1\" + Program.AI1file;
            AITYPE file_AI = null;
            if (File.Exists(s) && (File.ReadAllText(s) != ""))
            {
                ok = true;
                file_AI = AITYPE.ReadFromFile(s);
            }
            for (int i = 0; i < 600; i++)
            {
                letsgo.Add(0);
                if (ok)
                {
                    ais.Add(new AITYPE(file_AI, file_AI.mygoodness));
                    ais[ais.Count - 1].mygoodness = file_AI.mygoodness;
                }
                else ais.Add(new AITYPE(new List<int>() { 6, 3 }));
            }
            if (ok) ais[599] = new AITYPE(file_AI, 1);
            NextModules();
        }

        bool lg = false;

        void NextModules()
        {
            if (pos == 1085)
            {
                double min = int.MaxValue; int minid = -1;
                for (int i = 0; i < 600; i++)
                {
                    if (letsgo[i] < min)
                    {
                        min = letsgo[i];
                        minid = i;
                    }
                }
                //min -= 20000;
                ais[minid].mygoodness = Math.Max(1 - (min / 2200), 0);
                ais[minid].generation++;
                ais[minid].SaveToFile(@"AI_Phase1\" + Program.AI1file);
                NextAIs();
            }
            else
            {
                pos += 125;
                if (pos != 960) lg = true;
                else lg = false;
                start_time = Program.Now;
                modules.Clear();
                for (int i = 0; i < 600; i++) modules.Add(new LandingModule());
                Program.landing_target = new PointD(pos, modules[0].position.Y * 10 - 33);
                Program.landing_angle = 0d;
            }
        }

        public override void RunLogic()
        {
            Program.press_f = false;
            Program.press_p = false;
            if (Program.Now - start_time < 400) 
            {
                int c = 0;
                foreach (LandingModule module in modules)
                {
                    module.Make_AI_params();
                    module.Run_AI_PHASE1(ais[c]);
                    if (!lg) module.AITTL_Success();
                    else
                    {
                        if (Program.Now - start_time > 200) module.AITTL_Success(); 
                    }
                    if (Math.Abs(module.dif_position.Y) > 10) module.Kill();
                    if (Math.Abs(module.dif_position.X) > 15) module.Kill();
                    c++;
                }
                physics.Run();
            }
            else
            {
                for (int i = 0; i < 600; i++)
                {
                    letsgo[i] += ((LandingModule)modules[i]).ai_succ;
                }
                NextModules();
            }
        }
    }
}
