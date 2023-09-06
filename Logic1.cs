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
    class Logic1 : GameLogic
    {
        int npos = 70 - 178;
        List<double> times = new List<double>();
        public List<AITYPE> ais = new List<AITYPE>();
        public int start_time;

        public Logic1()
        {
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(100);
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            NextAIs();
            NextModules();
        }

        void NextAIs()
        {
            ais.Clear();
            times.Clear();
            npos = 70 - 178;
            for (int i = 0; i < 600; i++)
            {
                times.Add(0);
            }
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
                if (ok)
                {
                    ais.Add(new AITYPE(file_AI, file_AI.mygoodness));
                    ais[ais.Count - 1].mygoodness = file_AI.mygoodness;
                }
                else ais.Add(new AITYPE(new List<int>() { 6, 3 }));
            }
            if (ok) ais[599] = new AITYPE(file_AI, 1);
        }

        void NextModules()
        {
            if (npos == 1850)
            {
                double m_el = long.MaxValue; int m_id = -1;
                for (int i = 0; i < 600; i++)
                {
                    if (times[i] < m_el)
                    {
                        m_el = times[i];
                        m_id = i;
                    }
                }
                ais[m_id].mygoodness = Math.Max(1 - (times[m_id] / 2400), 0);
                ais[m_id].generation++;
                ais[m_id].SaveToFile(@"AI_Phase1\" + Program.AI1file);
                NextAIs();
                NextModules();
            }
            else
            {
                npos += 178;
                if (npos == 960) npos += 178;
                modules.Clear();
                for (int i = 0; i < 600; i++)
                {
                    modules.Add(new LandingModule());
                }
                for (int i = 0; i < 600; i++)
                {
                    double ng = ais[0].mygoodness;
                    ais[i] = new AITYPE(ais[i], 1);
                    ais[i].mygoodness = ng;
                }
                start_time = Program.Now;
                physics.objects = modules;
                Program.landing_angle = 0;
                Program.landing_target = new PointD(npos / 10, modules[0].position.Y - 3.3);
                Program.alive_m = modules.Count;
            }
        }

        public override void RunLogic()
        {
            Program.press_f = false;
            Program.press_p = false;
            double dist = Math.Abs(npos - 960) / 10d;
            double rtime = 2 * Math.Sqrt(dist) * 60;
            if ((Program.Now - start_time > rtime) || (Program.alive_m == 0))
            {
                for (int i = 0; i < 600; i++)
                {
                    times[i] += ((LandingModule)modules[i]).ai_succ;
                }
                NextModules();
            }
            else
            {
                for (int i = 0; i < 600; i++)
                {
                    if (modules[i].alive)
                    {
                        ((LandingModule)modules[i]).Make_AI_params();
                        ((LandingModule)modules[i]).Run_AI_PHASE1(ais[i]);
                        if (Math.Abs(((LandingModule)modules[i]).dif_position.Y) > 10) ((LandingModule)modules[i]).Kill();
                        if (npos < 960)
                        {
                            if (((LandingModule)modules[i]).position.X > 98) ((LandingModule)modules[i]).Kill();
                        }
                        else
                        {
                            if (((LandingModule)modules[i]).position.X < 94) ((LandingModule)modules[i]).Kill();
                        }
                        ((LandingModule)modules[i]).AI_Success();
                    }
                    //((LandingModule)modules[i]).Think();
                }
                physics.Run();
            }
        }
    }
}