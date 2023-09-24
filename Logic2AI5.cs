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
using System.Linq;
using Newtonsoft.Json;

namespace AIlanding
{
    class Logic2AI5:GameLogic
    {
        public AITYPE aimove;
        public int start_time;
        public int now_level = 0;
        int phase = 1;
        public List<AITYPE> ais = new List<AITYPE>();
        public List<AITYPE> prevais = new List<AITYPE>();
        MyAIVisualizer visualizer;
        double bestgoodness;
        string s1 = @"AI5\PHASE1\" + Program.AI1file;
        string s2 = @"AI5\PHASE2\" + Program.AI2file;

        public Logic2AI5()
        {
            if (File.Exists(s1) && (File.ReadAllText(s1) != ""))
            {
                aimove = MHeleper.ReadFromBinaryFile<List<AITYPE>>(s1)[0];
                aimove.Reset();
            }
            else throw new Exception("Немає файлу " + s1);
            if (File.Exists(s2) && (File.ReadAllText(s2) != ""))
            {
                ais = MHeleper.ReadFromBinaryFile<List<AITYPE>>(s2);
                NextAIs(true);
            }
            else
            {
                for (int i = 0; i < 500; i++)
                {
                    AITYPE current_ai = new AITYPE(true);
                    current_ai.MutateAI();
                    ais.Add(current_ai);
                }
                Program.alive_m = 500;
                visualizer = new MyAIVisualizer(ais[0]);
            }
            NextModules();
        }

        void NextAIs(bool loaded = false)
        {
            int max_storage = 1000;
            if (!loaded)
            {
                ais.RemoveAt(0);
                foreach (var now_ai in ais)
                {
                    now_ai.ApplyGoodness();
                }
                for (int i = 0; i < Math.Min(250, prevais.Count); i++)
                {
                    prevais[i].ApplyGoodness();
                }

                ais.AddRange(prevais);
                ais = ais.OrderBy(t => t.res_goodness).ToList();
                ais = ais.GetRange(0, Math.Min(ais.Count, max_storage));
                MHeleper.WriteToBinaryFile(s2, ais);
            }
            bestgoodness = ais[0].res_goodness;
            double min_score = ais[Math.Min(ais.Count - 1, 500)].res_goodness;
            double score_sum = 0;
            int tot_rs = 0;
            int max_r = 0;
            foreach (var now_ai in ais)
            {
                tot_rs += now_ai.total_runs;
                if (now_ai.total_runs> max_r)
                {
                    max_r = now_ai.total_runs;
                }
            }
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
            ais.Add(prevais[0].CreateDeepCopy());
            ais[0].Reset();
            prevais[0].Reset();
            int max_kc = 0;
            for (int i = 0; i < Math.Min(250, prevais.Count); i++)
            {
                //if (prevais[i].res_goodness < 80)
                if (true)
                {
                    int kids_count = (int)Math.Round((min_score - prevais[i].res_goodness) / (double)score_sum * 250);
                    if (kids_count > max_kc) max_kc = kids_count;
                    for (int j = 0; j < kids_count; j++)
                    {
                        AITYPE current_ai = (AITYPE)prevais[i].GetCopy();
                        current_ai.MutateAI();
                        current_ai.temp_goodness = 0;
                        ais.Add(current_ai);
                    }
                }
                else
                {
                    AITYPE current_ai = new AITYPE(true);
                    current_ai.MutateAI();
                    ais.Add(current_ai);
                }
            }
           /* if (JsonConvert.SerializeObject(ais[0]) == JsonConvert.SerializeObject(prevais[0]))
            {

            }*/
            visualizer = new MyAIVisualizer(ais[0]);
            if (JsonConvert.SerializeObject(ais[0]) != JsonConvert.SerializeObject(prevais[0]))
            {

            }
            phase = 1;
            now_level = 0;
        }

        void NextModules()
        {
            if (now_level != 5)
            {
                foreach (var item in ais)
                {
                    item.Reset();
                }
                for (int i = 0; i < Math.Min(250, prevais.Count); i++)
                {
                    prevais[i].Reset();
                }
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
                NextAIs();
                NextModules();
            }
        }

        public override void RunLogic()
        {
            visualizer.Check(Mouse.GetState(), Keyboard.GetState());
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
                    for (int i = 0; i < ais.Count; i++)
                    {
                        modules.Add(new LandingModule(module));
                    }
                    for (int i = 0; i < Math.Min(250, prevais.Count); i++)
                    {
                        modules.Add(new LandingModule(module));
                    }
                    start_time = Program.Now;
                    Program.alive_m = modules.Count;
                }
                else
                {
                    ((LandingModule)modules[0]).Make_AI_params_PHASE1(Program.landing_target.X);
                    ((LandingModule)modules[0]).Run_AI5_PHASE1(aimove);
                    physics.Run();
                }
            }
            else
            {
                if (Program.Now - start_time == 1200)
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        LandingModule module = (LandingModule)modules[i];
                        if (module.alive && !(module.ever_collided[0] && module.ever_collided[1]))
                        {
                            module.Kill();
                        }
                    }
                }

                if ((Program.Now - start_time > 1600)||(Program.alive_m == 0))
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        LandingModule module = (LandingModule)modules[i];
                        if (module.alive && module.ever_collided[0] && module.ever_collided[1])
                        {
                            if (i < ais.Count) ais[i].temp_goodness += Math.Min(120, module.AI_PHASE2_Success());
                            else prevais[i - ais.Count].temp_goodness += Math.Min(120, module.AI_PHASE2_Success());
                        }
                        else
                        {
                            if (i < ais.Count) ais[i].temp_goodness += 120;
                            else 
                                prevais[i - ais.Count].temp_goodness += 120;
                        }
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
                            /*if (i < ais.Count) module.Run_AI5_PHASE2(ais[0]);
                            else 
                                module.Run_AI5_PHASE2(prevais[0]);*/
                            if (i < ais.Count) module.Run_AI5_PHASE2(ais[i]);
                            else
                                module.Run_AI5_PHASE2(prevais[i-ais.Count]);
                            if (module.ever_collided[0] && module.ever_collided[1])
                            {
                                module.ResetInputs();
                            }
                            if (Math.Abs(module.dif_position.X) > 5) module.Kill();
                        }
                    }
                    physics.Run();
                }
            }
        }

        public override void DrawLogic(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Program.boxtexture, new Vector2(350, 1050), Color.White);
            Game1.generationlabel.text = "Generation: " + ais[0].generation;
            Game1.maxscorelabel.text = "Max. Score: " + ((float)bestgoodness / 5f).ToString();
            visualizer.drawposition = new Vector2(850, 850);
            visualizer.DrawNetwork(spriteBatch);
        }
    }
}
