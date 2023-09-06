using System;
using System.Collections.Generic;
using System.Text;
using AILib;
using Microsoft.Xna.Framework.Content;
using MonoHelper;

namespace AIlanding
{
    [Serializable]
    public class MyAI5 : AI5
    {
        public double temp_goodness = 0;
        public double res_goodness = 0;
        public int total_runs = 0;

        public void ResetGoodness()
        {
            temp_goodness = 0;
            res_goodness = 0;
            total_runs = 0;
        }

        public MyAI5() : base(6, 3, 5)
        {
        }

        public MyAI5(bool phase2) : base(7, 3, 4)
        {
        }

        public override void MutateAI()
        {
            ResetGoodness();
            base.MutateAI();
        }

        public void ApplyGoodness()
        {
            res_goodness = (res_goodness * total_runs + temp_goodness) / (total_runs + 1);
            total_runs++;
            temp_goodness = 0;
        }
    }
}
