using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace proj_Bomberman
{
    public class Blast : MapObject
    {
        private DispatcherTimer detonate_tim;
        private DisposeBlastDel dispose_self;

        public int BlastIndex { get; set; }

        public Blast(int index, DisposeBlastDel dispose_self) : base("blast")
        {
            detonate_tim = new DispatcherTimer();
            detonate_tim.Tick += new EventHandler(Blast_Detonate_Tick);
            detonate_tim.Interval = new TimeSpan(0, 0, 1);
            detonate_tim.Start();

            this.dispose_self = dispose_self;
            BlastIndex = index;
        }

        private void Blast_Detonate_Tick(object sender, EventArgs e)
        {
            detonate_tim.Stop();

            dispose_self(this, BlastIndex);
        }

        public void Accelerate()
        {
            detonate_tim.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        public void StopTimer()
        {
            detonate_tim.Stop();
        }
    }
}
