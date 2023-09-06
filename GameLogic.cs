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

namespace AIlanding
{
    class GameLogic
    {
        public List<MatrixPhysics.MP_Object> modules = new List<MatrixPhysics.MP_Object>();
        public MatrixPhysics physics;

        public double Surface_func(double pos)
        {
            double to_bound = Math.Min(91 - pos, pos);
            if (to_bound < 14)
            {
                return to_bound + 1;
            }
            else return 15;
        }

        public void Find_Landing_Site()
        {
            double min_el = int.MaxValue;
            int st, fn = 50;
            for (st = 50; fn <= 1870; st++)
            {
                double a1 = 0, a2 = 0;
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
                for (int x = st - 1; x <= st + 1; x++)
                {
                    a1 = Math.Max(a1, Math.Abs(physics.GetSurfaceAngle(x) - Math.PI / 2d));
                }
                for (int x = fn - 1; x <= fn + 1; x++)
                {
                    a2 = Math.Max(a2, Math.Abs(physics.GetSurfaceAngle(x) - Math.PI / 2d));
                }
                double d = Math.Atan2(fn - st, physics.matrix[fn] - physics.matrix[st]);
                if (Math.Max(a1, a2) < Math.PI / 4d)
                {
                    double res = a1 + a2 + 2 * Math.Abs(d - Math.PI / 2d);
                    if (res < min_el)
                    {
                        bool ok = true;
                        double min_h = int.MaxValue;
                        for (int c = st; c < fn; c++)
                        {
                            double coef = (c - st) / (double)(fn - st);
                            double p_a = (physics.matrix[st] + coef * (physics.matrix[fn] - physics.matrix[st]));
                            double a = physics.matrix[c] - p_a;
                            min_h = Math.Min(min_h, physics.matrix[c]);
                            if (a > 0)
                            {
                                double h = a * Math.Sin(d);
                                double l = a * Math.Cos(d);
                                double pos = coef * length + l;
                                if (Surface_func(pos) < h)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                        if (ok)
                        {
                            min_el = res;
                            Program.landing_target = new PointD((st + fn) / 2d, (physics.matrix[st] + physics.matrix[fn]) / 2d);
                            Program.landing_target /= 10;
                            Program.landing_angle = d - Math.PI / 2d;
                        }
                    }
                }

            }
        }

        public void CreateSurface(int diapason)
        {
            /*   List<Vector2> points = new List<Vector2>();
                int rd = diapason;
                float prev = 300;
                for (int x = 0; x - rd < 1920; x+= rd)
                {
                    float totop = 400 - prev;
                    float tobottom = prev - 100;
                    float koef = 0;
                    if (totop < 100) koef = (-1 + (totop / 100f)) / 2;
                    if (tobottom < 100) koef = (1 - (tobottom / 100f)) / 2;
                    float height = (float)(prev + (MHeleper.RandomDouble() - 0.5 + koef) * 200);
                    if (height < 100) height = 100;
                    if (height > 400) height = 400;
                    prev = height;
                    points.Add(new Vector2(x, height));
                }
                Program.surfacetexture = MHeleper.CreateCurve(GraphicsDevice, 1920, 1080, points, Color.Black);
                Program.surfacetexture.Fill(new Vector2(0, 0), Color.Transparent, new Color(206, 101, 36));
                physics.SetMatrix(Program.surfacetexture, new List<Color>() { new Color(206, 101, 36), Color.Black });*/
            physics.SetMatrix(MatrixPhysics.CreateSurface(1920, diapason, diapason, 200, 500));
            /*   Program.surfacetexture.Dispose();
               Program.surfacetexture = MHeleper.CreateCurve(GraphicsDevice, 1920, 1080, new List<Vector2>() { new Vector2(0, 300), new Vector2(1920, 300) }, Color.Black);
               Program.surfacetexture.Fill(new Vector2(0, 0), Color.Transparent, new Color(206, 101, 36));
               physics.SetMatrix(Program.surfacetexture.ToBoolMatrix(new List<Color>() { new Color(206, 101, 36), Color.Black }));*/
        }

        public virtual void RunLogic()
        {

        }
        
        public virtual void DrawLogic(SpriteBatch spriteBatch)
        {

        }
    }
}
