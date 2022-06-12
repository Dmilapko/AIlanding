using System;
using System.Collections.Generic;
using System.Text;
using AILib;

namespace AIlanding
{
    class LandingModule_AI_Move:LandingModule
    {
        public AI ai;
        public LandingModule_AI_Move(AI _ai):base()
        {
            ai = _ai;
        }

        public override void Run()
        {
            base.Run();   
        }
    }
}
