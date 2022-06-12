using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AILib;
using MonoHelper;
using Physics;

namespace AIlanding
{
    class Engine
    {
        public Vector2 drawingpos;
        public Vector2 pos;
        public bool enabled = false;
        public int dir_of_force;

        public System.Drawing.PointF GetPosition()
        {
            return new System.Drawing.PointF(pos.X - 43, pos.Y - 28);
        }

        public Engine(Vector2 _pos, Vector2 _drawingpos, int _dir_of_force)
        {
            pos = _pos;
            drawingpos = _drawingpos;
            dir_of_force = _dir_of_force;
        }
    }

    public class LandingModule : MatrixPhysics.MP_Object
    {
        /// <summary>
        /// Thrust of engines in H[si]
        /// </summary>
        const float thrust = 10;
        private int direction = 1;
        List<Engine> engines = new List<Engine>();
        public float maxg = 0;
        public bool alive = true;

        public virtual void Run()
        {
            ApplyForce(new System.Drawing.PointF(0, 0), 180, mass * 3.721f / 60f);
            foreach (var engine in engines)
            {
                if (engine.enabled)
                {
                    ApplyForce()
                }
            }
        }

        public LandingModule() : base(1f / 12f * 20000f * (8.6f * 8.6f * 6.3f * 6.3f), 20000f, new System.Drawing.PointF(960, 100), new List<System.Drawing.PointF>())
        {
            x = 960;
            y = 100;
            Initialize();
        }

        void Initialize()
        {
            engines.Add(new Engine(new Vector2(14, 16), new Vector2(14, 16), 0));
            engines.Add(new Engine(new Vector2(4, 8), new Vector2(5, 8), 90));
            engines.Add(new Engine(new Vector2(14, 0), new Vector2(15, 1), 180));
            engines.Add(new Engine(new Vector2(71, 0), new Vector2(72, 1), 180));
            engines.Add(new Engine(new Vector2(81, 8), new Vector2(81, 9), 270));
            engines.Add(new Engine(new Vector2(71, 16), new Vector2(71, 16), 0));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var engine in engines)
            {
                if (engine.enabled)
                {
                    float rx = engine.drawingpos.X - 43;
                    float ry = engine.drawingpos.Y - 28;
                    float cx = 0, cy = 0;
                    cx += (float)Math.Sin(dir.ToRadians()) * ry;
                    cy += (float)Math.Cos(dir.ToRadians()) * ry;
                    cx += (float)Math.Sin((dir + 90).ToRadians()) * rx;
                    cy += (float)Math.Cos((dir + 90).ToRadians()) * rx;
                    spriteBatch.Draw(Program.flametexture, new Vector2(x + cx, y + cy), null, Color.White, (dir + engine.dir_of_force).ToRadians(), new Vector2(3, 0), 1f, SpriteEffects.None, 1);
                }
            }
            if (alive) spriteBatch.Draw(Program.moduletexture, new Vector2(x, y), null, Color.White, dir.ToRadians(), new Vector2(43, 28), 1f, SpriteEffects.None, 1);
            else spriteBatch.Draw(Program.moduletexture, new Vector2(x, y), null, Color.White, dir.ToRadians(), new Vector2(43, 28), 1f, SpriteEffects.None, 1);
        }
    }
}
