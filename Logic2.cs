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
    class Logic2:GameLogic
    {
        public AITYPE move_AI;
        public List<AITYPE> ais = new List<AITYPE>();
        public int start_time;
        public int now_level = 0;
        int phase = 1;
        public List<double> g_sum = new List<double>();

        public Logic2()
        {
            NextAIs();
            NextModules();
        }

        void NextAIs()
        {
            g_sum.Clear();
            for (int i = 0; i < 600; i++) g_sum.Add(0);
            now_level = 0;
            string s = @"AI_Phase1\" + Program.AI1file;
            if (File.Exists(s) && (File.ReadAllText(s) != ""))
            {
                move_AI = new AITYPE(AITYPE.ReadFromFile(s), 1);
            }
            else
            {
                throw new Exception("Файл '" + s + "' не знайдено");
            }
            start_time = Program.Now;
            NextPhase2();
        }

        void NextPhase2()
        {
            bool ok = false;
            string s = @"AI_Phase2\" + Program.AI2file;
            AITYPE file_AI = null;
            ais.Clear();
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
                else ais.Add(new AITYPE(new List<int>() { 7, 4 }));
            }
            if (ok) ais[599] = new AITYPE(file_AI, 1);
            //else ais[599] = new AITYPE(move_AI, 1);
        }

        void NextModules()
        {
            if (now_level != 5)
            {
                phase = 1;
                modules.Clear();
                modules.Add(new LandingModule());
                physics = new MatrixPhysics(modules, 10, Program.fps, 10);
                CreateSurface(Program.levels[4]);
                Find_Landing_Site();
                Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
                now_level++;
                start_time = Program.Now;
                Program.alive_m = modules.Count;
            }
            else
            {
                double m_el = long.MaxValue; int m_id = -1;
                for (int i = 0; i < 600; i++)
                {
                    if (g_sum[i] < m_el)
                    {
                        m_el = g_sum[i];
                        m_id = i;
                    }
                }
                ais[m_id].mygoodness = Math.Max(1 - (g_sum[m_id] / 300), 0);
                ais[m_id].generation++;
                ais[m_id].SaveToFile(@"AI_Phase2\" + Program.AI2file);
                NextAIs();
                NextModules();
            }
        }

        public override void RunLogic()
        {
            Program.press_f = false;
            Program.press_p = false;
            if (phase == 1)
            {
                double dist = Math.Abs(Program.landing_target.X * 10 - 960) / 10d;
                double rtime = 2 * Math.Sqrt(dist) * 60;
                if (Program.Now - start_time > rtime)
                {
                    phase = 2;
                    LandingModule module = (LandingModule)modules[0];
                    modules.Clear();
                    for (int i = 0; i < 600; i++)
                    {
                        modules.Add(new LandingModule(module));
                    }
                    start_time = Program.Now;
                    Program.alive_m = modules.Count;
                }
                else
                {
                    ((LandingModule)modules[0]).Make_AI_params_PHASE1(Program.landing_target.X);
                    ((LandingModule)modules[0]).Run_AI_PHASE1(move_AI);
                    physics.Run();
                }
            }
            else
            {
                if ((Program.Now - start_time > 1200)||(Program.alive_m == 0))
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        LandingModule module = (LandingModule)modules[i];
                        if (module.alive&&module.ever_colided)
                        {
                            g_sum[i] += Math.Min(60, module.AI_PHASE2_Success());
                        }
                        else g_sum[i] += 60;
                    }
                    NextModules();
                }
                else
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        LandingModule module = (LandingModule)modules[i];
                        if (module.alive)
                        {
                            module.Make_AI_params();
                            module.Run_AI_PHASE2(ais[i]);
                            /*if (!module.ever_colided)
                            {
                                
                            }
                            else
                            {
                                module.ResetInputs();
                            }*/
                            if (Math.Abs(module.dif_position.X) > 5) module.Kill();
                        }
                    }
                    physics.Run();
                }
            }
        }
    }
}
