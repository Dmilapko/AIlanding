using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace AIlanding
{
    public static class Program
    {
        public static Texture2D marstexture;
        public static Texture2D flametexture;
        public static Texture2D moduletexture;

        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
