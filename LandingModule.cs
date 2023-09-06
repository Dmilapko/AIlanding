using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AILib;
using MonoHelper;
using Physics;
using AITYPE = AILib.Clever_AI;

namespace AIlanding
{
    class Engine
    {
        public Vector2 drawingpos;
        public PointD pos;
        public bool enabled = false;
        public double dir_of_force;

        public Engine(PointD _pos, Vector2 _drawingpos, double _dir_of_force)
        {
            pos = _pos;
            drawingpos = _drawingpos;
            dir_of_force = _dir_of_force;
        }
    }

    public class LandingModule : MatrixPhysics.MP_Object
    {
        const double gravity_force = 3.72076;
        /// <summary>
        /// Empty mass of  module
        /// </summary>
        const double initialmass = 12000;
        const double initialfuelmass = 8000;
        public double fuelmass = initialfuelmass;
        /// <summary>
        /// Thrust of engines in H[si]
        /// </summary>
        const double thrust = 55000;
        List<Engine> engines = new List<Engine>();
        public double maxg = 0;
        public double gforce = 0;
        public double gforce_dir = 0;
        private PointD needbe = new PointD(0, 0);
        List<PointD> speed_list = new List<PointD>();
        public PointD dif_position;
        public double dif_angle = 0;
        public AITYPE ai_move;
        public AITYPE ai_land;
        public double ai_succ = int.MaxValue;
        public bool collided = false;
        public bool ever_colided = false;
        public double beforefuel;
        public int beforetime = -1;

        public override double mass { get => initialmass + fuelmass; set => base.mass = value; }

        public LandingModule() : base(1f / 12f * (8.6f * 8.6f + 6.3f * 6.3f), (initialmass + initialfuelmass), 0.45, new PointD(96, 90))
        {
            on_death += LandingModule_on_death;
            Initialize();
        }

        public LandingModule(LandingModule module) : base(1f / 12f * (8.6f * 8.6f + 6.3f * 6.3f), (initialmass + initialfuelmass), 0.45, new PointD(96, 90))
        {
            fuelmass = module.fuelmass;
            position = module.position;
            angularvelocity = module.angularvelocity;
            rotation = module.rotation;
            speed = module.speed;
            on_death += LandingModule_on_death;
            Initialize();
        }

        public bool AI_Finished()
        {
            return ((Math.Abs(dif_angle) < 0.02) && (Math.Abs(dif_position.X) < 0.1) && (Math.Abs(dif_position.Y) < 0.1));
        }

        public void AI_Success()
        {
            if (alive)
                ai_succ = Math.Min(ai_succ, Math.Abs(dif_position.X * 7) + Math.Abs(dif_position.Y * 3.5) + Math.Abs(dif_angle * 100) + Math.Abs(speed.X * 10) + Math.Abs(speed.Y * 10) + Math.Abs(angularvelocity * 200));
        }

        public double AI_PHASE2_Success()
        {
            //return maxg * 4 + Math.Abs(dif_position.X * 10) + Math.Abs(speed.X * 10) * +Math.Abs(dif_position.Y * 10) + Math.Abs(speed.Y * 10) + (beforefuel - fuelmass) / (double)(Program.Now - beforetime) * 15;
            return maxg * 4 + Math.Abs(dif_position.X * 10) + Math.Abs(speed.X * 10) * +Math.Abs(dif_position.Y * 10) + Math.Abs(speed.Y * 10);
            //return maxg * 2 + Math.Abs(dif_position.X * 10) + Math.Abs(speed.X * 10) * +Math.Abs(dif_position.Y * 10) + Math.Abs(speed.Y * 10);
        }

        public void AITTL_Success()
        {
            ai_succ += Math.Abs(dif_position.X * 10) + Math.Abs(dif_position.Y) + Math.Abs(dif_angle * 10) + Math.Abs(speed.X) + Math.Abs(speed.Y) + Math.Abs(angularvelocity * 10);
        }

        public void ResetInputs()
        {
            engines[0].enabled = engines[1].enabled = engines[2].enabled = engines[3].enabled = engines[4].enabled = engines[5].enabled = false;
        }

        public PointD GetPosition(PointD pos, double translate_dir)
        {
            Vector2 v = new Vector2((float)(pos.X - 43), (float)(28 - pos.Y));
            double l = (double)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            double d = translate_dir + (double)Math.Atan2(v.X, v.Y);
            return new PointD((float)(Math.Sin(d) * l / 10f), (float)(Math.Cos(d) * l / 10f));
        }

        public override void InitializePoints()
        {
            base.InitializePoints();
            hitpoints.Add(GetPosition(new PointD(2, 61), 0));
            hitpoints.Add(GetPosition(new PointD(83, 61), 0));
        }

        public override void InitializeDeathPoints()
        {
            base.InitializeDeathPoints();
            deathpoints.Add(GetPosition(new PointD(5, 57), 0));
            deathpoints.Add(GetPosition(new PointD(7, 51), 0));
            deathpoints.Add(GetPosition(new PointD(13, 47), 0));
            deathpoints.Add(GetPosition(new PointD(18, 45), 0));
            deathpoints.Add(GetPosition(new PointD(24, 44), 0));
            deathpoints.Add(GetPosition(new PointD(31, 44), 0));
            deathpoints.Add(GetPosition(new PointD(39, 44), 0));
            deathpoints.Add(GetPosition(new PointD(2, 55), 0));
            deathpoints.Add(GetPosition(new PointD(4, 46), 0));
            deathpoints.Add(GetPosition(new PointD(7, 36), 0));
            deathpoints.Add(GetPosition(new PointD(19, 23), 0));
            deathpoints.Add(GetPosition(new PointD(14, 15), 0));
            deathpoints.Add(GetPosition(new PointD(5, 8), 0));
            deathpoints.Add(GetPosition(new PointD(14, 1), 0));
            deathpoints.Add(GetPosition(new PointD(21, 0), 0));
            deathpoints.Add(GetPosition(new PointD(36, 0), 0));
            deathpoints.Add(GetPosition(new PointD(39, 44), 0));
            int leftcount = deathpoints.Count;
            for (int i = 0; i < leftcount; i++)
            {
                deathpoints.Add(new PointD(-1 * deathpoints[i].X, deathpoints[i].Y));
            }
        }

        public void Think()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad1)) engines[0].enabled = true;
            else engines[0].enabled = false;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad4)) engines[1].enabled = true;
            else engines[1].enabled = false;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad7)) engines[2].enabled = true;
            else engines[2].enabled = false;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad3)) engines[5].enabled = true;
            else engines[5].enabled = false;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad6)) engines[4].enabled = true;
            else engines[4].enabled = false;
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad9)) engines[3].enabled = true;
            else engines[3].enabled = false;
        }

        public override void AfterRun()
        {
            base.AfterRun();
            int cnt_interval = (int)(Program.fps / 10);
            speed_list.Add(speed);
            if (speed_list.Count == (cnt_interval + 2))
            {
                speed_list.RemoveAt(0);
                PointD needbe = speed_list[0];
                needbe.Y -= (cnt_interval / Program.fps) * gravity_force;
                PointD dif = (needbe - speed_list[cnt_interval]) * (Program.fps / cnt_interval);
                gforce_dir = Math.Atan2(needbe.X - speed_list[cnt_interval].X, needbe.Y - speed_list[cnt_interval].Y) - rotation;
                gforce = Math.Sqrt(dif.X * dif.X + dif.Y * dif.Y) / 9.81d;
                if (gforce > maxg) maxg = gforce;
            }
        }

        public void Make_AI_params()
        {
            PointD lp = Program.landing_target + new PointD(0, 3.3).Turn(Program.landing_angle);
            dif_position = position - lp;
            dif_angle = Program.landing_angle - rotation;
        }

        public void Make_AI_params_PHASE1(double pos_x)
        {
            dif_position.Y = position.Y - 90;
            dif_position.X = position.X - pos_x;
            dif_angle = -1 * rotation;
        }

        public void Run_AI_PHASE1(AITYPE ai)
        {
            // ВХІДНІ НЕЙРОНИ ФАЗА 1
            ai.inputs[0].value = speed.X / 50d;
            ai.inputs[1].value = speed.Y / 10d;
            ai.inputs[2].value = dif_position.X / 200d;
            ai.inputs[3].value = dif_position.Y / 40d;
            ai.inputs[4].value = angularvelocity / Math.PI * 2;
            ai.inputs[5].value = dif_angle / Math.PI * 2;
            // РОЗРАХУНОК
            ai.Run();
            // ВИХІДНІ НЕЙРОНИ 
            engines[0].enabled = ai.outputs[0].value > 0.5;
            engines[2].enabled = ai.outputs[0].value < 0.5;
            engines[1].enabled = ai.outputs[1].value > 0.5;
            engines[4].enabled = ai.outputs[1].value < 0.5;
            engines[5].enabled = ai.outputs[2].value > 0.5;
            engines[3].enabled = ai.outputs[2].value > 0.6;
        }

        public void Run_AI5_PHASE1(AI5 ai)
        {
            // ВХІДНІ НЕЙРОНИ ФАЗА 1
            ai.SetInput(0, speed.X / 50d);
            ai.SetInput(1, speed.Y / 10d);
            ai.SetInput(2, dif_position.X / 200d);
            ai.SetInput(3, dif_position.Y / 40d);
            ai.SetInput(4, angularvelocity / Math.PI * 2);
            ai.SetInput(5, dif_angle / Math.PI * 2);
            // РОЗРАХУНОК
            ai.Run();
            // ВИХІДНІ НЕЙРОНИ 
            engines[0].enabled = ai.GetOutput(0) > 0;
            engines[2].enabled = ai.GetOutput(0) < 0;
            engines[1].enabled = ai.GetOutput(1) > 0;
            engines[4].enabled = ai.GetOutput(1) < 0;
            engines[5].enabled = ai.GetOutput(2) > 0;
            engines[3].enabled = ai.GetOutput(2) < 0;
        }

        public void Run_AI_PHASE2(AITYPE ai)
        {
            // ВИХІДНІ НЕЙРОНИ ФАЗА 2
            ai.inputs[0].value = speed.X / 4d;
            ai.inputs[1].value = speed.Y / 10d;
            ai.inputs[2].value = dif_position.X / 5d;
            ai.inputs[3].value = dif_position.Y / 100d;
            ai.inputs[4].value = angularvelocity / Math.PI * 2;
            ai.inputs[5].value = dif_angle / Math.PI * 2;
            ai.inputs[6].value = rotation / Math.PI * 2;
            // РОЗРАХУНОК
            ai.Run();
            // ВИХІДНІ НЕЙРОНИ
            engines[0].enabled = ai.outputs[0].value > 0.5;
            engines[2].enabled = ai.outputs[0].value < 0.5;
            engines[1].enabled = ai.outputs[1].value > 0.5;
            engines[4].enabled = ai.outputs[1].value < 0.5;
            engines[5].enabled = ai.outputs[2].value > 0.5;
            engines[3].enabled = ai.outputs[2].value < 0.5;
            if (ai.outputs[3].value > 0.5) ResetInputs();
        }

        public void Run_AI5_PHASE2(AI5 ai)
        {
            // ВИХІДНІ НЕЙРОНИ ФАЗА 2
            ai.SetInput(0, speed.X / 4d * 2);
            ai.SetInput(1, speed.Y / 10d * 2);
            ai.SetInput(2, dif_position.X / 5d * 2);
            ai.SetInput(3, dif_position.Y / 100d * 2);
            ai.SetInput(4, angularvelocity / Math.PI * 2 * 2);
            ai.SetInput(5, dif_angle / Math.PI * 2 * 2);
            ai.SetInput(6, rotation / Math.PI * 2);
            //ai.SetInput(7, Convert.ToInt32(now_collided[0]) * 2 - 1);
            //ai.SetInput(8, Convert.ToInt32(now_collided[1]) * 2 - 1);
            // РОЗРАХУНОК
            ai.Run();
            // ВИХІДНІ НЕЙРОНИ
            engines[0].enabled = ai.GetOutput(0) > 0;
            engines[2].enabled = ai.GetOutput(0) < 0;
            engines[1].enabled = ai.GetOutput(1) > 0;
            engines[4].enabled = ai.GetOutput(1) < 0;
            engines[5].enabled = ai.GetOutput(2) > 0;
            engines[3].enabled = ai.GetOutput(2) < 0;
            //if (ai.GetOutput(3) > 0) ResetInputs();
        }

        public override void Run()
        {
            collided = false;
            if (alive)
            {
                //Think();
                ApplyForce(new PointD(0, 0), 180.ToRadians(), mass * gravity_force / Program.fps);
                needbe = position + (speed / Program.fps);
                foreach (var engine in engines)
                {
                    if (engine.enabled)
                    {
                        if (fuelmass != 0)
                        {
                            fuelmass -= 60 / Program.fps;
                            ApplyForce(GetPosition(engine.pos, (float)rotation), engine.dir_of_force + rotation, thrust / Program.fps);
                        }
                    }
                }
            }
        }

        private void LandingModule_on_death(object sender, EventArgs e)
        {
            forces.Clear();
            Program.alive_m--;
            foreach (Engine engine in engines)
            {
                engine.enabled = false;
            }
        }

        void Initialize()
        {
            on_collision += LandingModule_on_collision;
            engines.Add(new Engine(new PointD(14, 16), new Vector2(14, 16), 0));
            engines.Add(new Engine(new PointD(4, 8), new Vector2(5, 8), 90.ToRadians()));
            engines.Add(new Engine(new PointD(14, 0), new Vector2(15, 1), 180.ToRadians()));
            engines.Add(new Engine(new PointD(71, 0), new Vector2(72, 1), 180.ToRadians()));
            engines.Add(new Engine(new PointD(81, 8), new Vector2(81, 9), 270.ToRadians()));
            engines.Add(new Engine(new PointD(71, 16), new Vector2(71, 16), 0.ToRadians()));
        }

        private void LandingModule_on_collision(object sender, EventArgs e)
        {
            collided = true;
            if (!ever_colided)
            {
                beforefuel = fuelmass;
                beforetime = Program.Now;
            }
            ever_colided = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (alive)
            {
                foreach (var engine in engines)
                {
                    if (engine.enabled && (fuelmass != 0))
                    {
                        double rx = engine.drawingpos.X - 43;
                        double ry = 28 - engine.drawingpos.Y;
                        PointD c = new PointD(0, 0);
                        c.X += (float)Math.Sin(rotation) * ry;
                        c.Y += (float)Math.Cos(rotation) * ry;
                        c.X += (float)Math.Sin(rotation + 90.ToRadians()) * rx;
                        c.Y += (float)Math.Cos(rotation + 90.ToRadians()) * rx;
                        spriteBatch.Draw(Program.flametexture, (position + c * 0.1).ToPixel(), null, Color.White, (float)(rotation + engine.dir_of_force), new Vector2(3, 0), 1f, SpriteEffects.None, 1);
                    }
                }
                spriteBatch.Draw(Program.moduletexture, position.ToPixel(), null, Color.White, (float)rotation, new Vector2(43, 28), 1f, SpriteEffects.None, 1);
                if (Program.press_f)
                {
                    foreach (var force in forces)
                    {
                        spriteBatch.Draw(Program.vectortexture, (force.pos + position).ToPixel(), null, Color.White, (float)force.angle, new Vector2(5, 30), (float)force.force / 1240f, SpriteEffects.None, 1);
                    }
                }
                if (Program.press_p)
                {
                    foreach (var point in deathpoints)
                    {
                        spriteBatch.Draw(Program.recttexture, (point.Turn(rotation) + position).ToPixel(), null, Color.Red, 0f, new Vector2(0.5f, 0.5f), 1, SpriteEffects.None, 1);
                    }
                    foreach (var point in hitpoints)
                    {
                        spriteBatch.Draw(Program.recttexture, (point.Turn(rotation) + position).ToPixel(), null, Color.Black, 0f, new Vector2(0.5f, 0.5f), 1, SpriteEffects.None, 1);
                    }
                }
            }
            else spriteBatch.Draw(Program.deadmoduletexture, position.ToPixel(), null, Color.White, (float)rotation, new Vector2(43, 28), 1f, SpriteEffects.None, 1);
        }
    }
}
