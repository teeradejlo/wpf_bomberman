using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace proj_Bomberman
{
    public class Bomb : MapObject
    {
        private DispatcherTimer detonate_tim;
        private DisposeBombDel dispose_self;
        public int Row { get; set; }
        public int Col { get; set; }
        public int Range { get; set; }
        public int BombIndex { get; set; }

        public Bomb(int row, int col, int range, int index, DisposeBombDel dispose_self) : base("bomb")
        {
            detonate_tim = new DispatcherTimer();
            detonate_tim.Tick += new EventHandler(Bomb_Detonate_Tick);
            detonate_tim.Interval = new TimeSpan(0, 0, 2);
            detonate_tim.Start();
            Row = row;
            Col = col;
            Range = range;
            BombIndex = index;
            this.dispose_self = dispose_self;
        }

        public void InstantDetonate()
        {
            Bomb_Detonate_Tick(this, EventArgs.Empty);
        }

        private void Bomb_Detonate_Tick(object sender, EventArgs e)
        {
            detonate_tim.Stop();

            dispose_self(this, Row, Col);
        }

        public void StopTimer()
        {
            detonate_tim.Stop();
        }
    }
}
