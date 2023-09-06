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
using AITYPE = AILib.AI4;

namespace AIlanding
{
    class Logic0:GameLogic
    {
        public Logic0(int size)
        {
            modules.Add(new LandingModule());
            physics = new MatrixPhysics(modules, 10, Program.fps, 10);
            CreateSurface(size);
            Find_Landing_Site();
            Program.surfacetexture = physics.GetTexture(Program.my_device, 1080, new Color(206, 101, 36), Color.Black, Program.press_g);
            Program.alive_m = 1;
        }

        public override void RunLogic()
        {
            ((LandingModule)modules[0]).Make_AI_params();
            ((LandingModule)modules[0]).Think();
            physics.Run();
        }
    }
}
