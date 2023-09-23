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
using System.Runtime.ConstrainedExecution;
using System.Linq;
using Newtonsoft.Json;

namespace AIlanding
{
    class Logic1AI5 : GameLogic
    {
        int npos = 70 - 178;
        public List<AITYPE> ais = new List<AITYPE>();
        public List<AITYPE> prevais = new List<AITYPE>();
        double bestgoodness = 0;
        public int start_time;
        AI5Visualizer visualizer = null;

        public Logic1AI5()
        {
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(100);
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            string s = @"AI5\PHASE1\" + Program.AI1file;
            if (File.Exists(s) && (File.ReadAllText(s) != ""))
            {
                ais = MHeleper.ReadFromBinaryFile<List<AITYPE>>(s);
                NextAIs();
            }
            else
            {
                for (int i = 0; i < 500 + 1; i++)
                {
                    AITYPE current_ai = new AITYPE();
                    current_ai.MutateAI();
                    current_ai.temp_goodness = 0;
                    ais.Add(current_ai);
                }
                Program.alive_m = 500 + 1;
                visualizer = new MyAIVisualizer(ais[0]);
            }
            NextModules();
        }

        void NextAIs()
        {
            int max_storage = 1500;
            npos = 70 - 178;
            foreach (var now_ai in ais)
            {
                now_ai.ApplyGoodness();
            }
            string s = @"AI5\PHASE1\" + Program.AI1file;
            ais.RemoveAt(ais.Count - 1);
            ais.AddRange(prevais);
            ais = ais.OrderBy(t => t.res_goodness).ToList();
            ais = ais.GetRange(0, Math.Min(ais.Count, max_storage));
            MHeleper.WriteToBinaryFile(s, ais);
            bestgoodness = ais[0].res_goodness;
            double min_score = ais[Math.Min(ais.Count-1, 500)].res_goodness;
            double score_sum = 0;
            //visiualize
            Color[] cd = new Color[504 * 24];
            Program.boxtexture.GetData(cd);
            int pixel = 0;
            for (int i = 0; i < Math.Min(ais.Count, 500); i++)
            {
                score_sum += min_score - ais[i].res_goodness;
                int g = (int)(255 * (min_score - ais[i].res_goodness) / (double)(min_score - ais[0].res_goodness + 1));
                for (int j = 0; j < 20; j++)
                {
                    cd[(j + 2) * 504 + i + 2] = new Color(0, g, 0);
                }            
            }
            Program.boxtexture.SetData(cd);
            prevais = ais.CreateDeepCopy();
            //!visiualize
            ais.Clear();
            int max_kc = 0;
            for (int i = 0; i < Math.Min(500, prevais.Count); i++) 
            {
                int kids_count = (int)Math.Round((min_score - prevais[i].res_goodness) / (double)score_sum * 500);
                if (kids_count > max_kc) max_kc = kids_count;
                for (int j = 0; j < kids_count; j++)
                {
                    AITYPE current_ai = (AITYPE)prevais[i].GetCopy();
                    current_ai.MutateAI();
                    current_ai.temp_goodness = 0;
                    ais.Add(current_ai);
                }
            }
            prevais[0].Reset();
            ais.Add(prevais[0].CreateDeepCopy());
            visualizer = new MyAIVisualizer(ais[ais.Count - 1]);
            if (JsonConvert.SerializeObject(ais[ais.Count - 1]) != JsonConvert.SerializeObject(prevais[0]))
            {

            }
        }

        void NextModules()
        {
            if (npos == 1850)
            {
                NextAIs();
                NextModules();
            }
            else
            {
                npos += 178;
                if (npos == 960) npos += 178;
                modules.Clear();
                for (int i = 0; i < ais.Count; i++)
                {
                    modules.Add(new LandingModule());
                }
                for (int i = 0; i < ais.Count; i++)
                {
                    ais[i].Reset();
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
            visualizer.Check(Mouse.GetState(), Keyboard.GetState());    
            Program.press_f = false;
            Program.press_p = false;
            double dist = Math.Abs(npos - 960) / 10d;
            double rtime = 2 * Math.Sqrt(dist) * 60;
            if ((Program.Now - start_time > rtime) || (Program.alive_m == 0))
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    ais[i].temp_goodness += ((LandingModule)modules[i]).ai_succ;
                }
                NextModules();
            }
            else
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    if (modules[i].alive)
                    {
                        ((LandingModule)modules[i]).Make_AI_params();
                        ((LandingModule)modules[i]).Run_AI5_PHASE1(ais[i]);
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

        public override void DrawLogic(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Program.boxtexture, new Vector2(350, 1050), Color.White);
            Game1.generationlabel.text = "Generation: " + ais[0].generation;
            Game1.maxscorelabel.text = "Max. Score: " + ((float)bestgoodness).ToString();
            visualizer.drawposition = new Vector2(850, 850);
            visualizer.DrawNetwork(spriteBatch);
        }
    }
}