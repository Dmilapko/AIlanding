using AILib;
using Microsoft.Xna.Framework;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIlanding
{
    internal class MyAIVisualizer:AI5Visualizer
    {
        public MyAIVisualizer(AI5 ai):base(Program.my_device, ai, 550, 230, Program.font10, Color.Blue, Color.Gray)
        {

        }
    }
}
