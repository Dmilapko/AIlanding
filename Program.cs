using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MonoHelper;

namespace AIlanding
{
    public static class Program
    {
        public static Texture2D marstexture;
        public static Texture2D surfacetexture;
        public static Texture2D flametexture;
        public static Texture2D moduletexture;
        public static Texture2D deadmoduletexture;
        public static Texture2D vectortexture;
        public static Texture2D recttexture;
        public static Texture2D balltexture;
        public static Texture2D displaytexture;
        public static Texture2D gforcetexture;
        public static Texture2D gforceballtexture;
        public static Texture2D attidude_indicator_texture;
        public static Texture2D attidude_indicator_line_texture;
        public static Texture2D boxtexture;
        public static Texture2D landing_texture;
        public static Texture2D AI_params;
        public static SpriteFont font20;
        public static SpriteFont font10;
        public static SpriteFont font15;
        public static int mode = 0;
        public static bool press_f = true;
        public static bool press_g = true;
        public static bool press_p = true;
        public static bool press_t = true;
        public static double fps = 60;
        public static bool show_display = true;
        public static string AI1file = "ai1.txt", AI2file = "ai2.txt";
        static public Dictionary<string, string> setupProp = new Dictionary<string, string>();
        public static string setuppath = "setup";
        public static PointD landing_target;
        public static double landing_angle = 0;
        public static Game1 game1;
        public static GraphicsDevice my_device;
        public static int Now = 0;
        public static int alive_m = 0;
        public static List<int> levels = new List<int> { 150, 100, 75, 60, 50 };
        static public void ChangeSetup()
        {
            string str = JsonSerializer.Serialize(setupProp);
            File.WriteAllText(setuppath, str);
        }

        static public void GetSetup()
        {
            string str = File.ReadAllText(setuppath);
            setupProp = JsonSerializer.Deserialize<Dictionary<string, string>>(str);
        }

        public static Vector2 ToPixel(this System.Drawing.PointF point)
        {
            return new Vector2(point.X * 10, point.Y * 10).FlipY();
        }

        public static Vector2 ToPixel(this PointD point)
        {
            return new Vector2((float)(point.X * 10), (float)(point.Y * 10)).FlipY();
        }

        [STAThread]
        static void Main()
        {
            List<int> ar = new List<int>(new int[10]);
            double sum = 0;
            for (int i = 1; i < 10000; i++)
            {
                sum += i * Math.Pow(0.95d, i);
            }
            if ((File.Exists(setuppath)) && (File.ReadAllText(setuppath) != ""))
            {
                GetSetup();
            }
            else
            {
                setupProp.Add("AI1file", "ai1_0.txt");
                setupProp.Add("AI2file", "ai2_0.txt");
                ChangeSetup();
            }
            AI1file = setupProp["AI1file"];
            AI2file = setupProp["AI2file"];

            game1 = new Game1();
            game1.Run();
           /* using (var game = new Game1())
                game.Run();*/
        }
    }
}
