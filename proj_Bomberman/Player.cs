using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proj_Bomberman
{
    public class Player : MapObject
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int MaxBomb { get; set; }
        public int BombPlaced { get; set; }
        public int BombRange { get; set; }
        public int NextBombIndex { get; private set; }
        public bool HasKey { get; set; }

        public Player(int bomb_cnt, int bomb_range) : base("player") {
            Row = 1;
            Col = 1;

            MaxBomb = bomb_cnt;
            BombRange = bomb_range;
            NextBombIndex = 1;
            HasKey = false;
        }

        public void UpdateBombIndex()
        {
            NextBombIndex = NextBombIndex % MaxBomb + 1;
        }
    }
}
