using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proj_Bomberman
{
    public class Mobs : MapObject
    {
        private readonly int _type;

        public bool IsVerticalMove { get; set; }
        public int DirX { get; set; }
        public int DirY { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }

        public Mobs(int row, int col, bool vert_free, bool hor_free, bool prioritize_vert, bool prioritize_hor) : base("minion")
        {
            Random rnd = new Random();
            int rnd_num = rnd.Next(1, 3);

            Row = row;
            Col = col;

            if (rnd_num == 1)
            {
                _type = 1;
            } else
            {
                _type = 2;
            }

            //if both dir is not free
            if ((!vert_free && !hor_free))
            {
                //only one dir is prioritize => take the prioritize dir
                if (!(prioritize_vert && prioritize_hor)) 
                {
                    if (prioritize_vert)
                    {
                        vert_free = true;
                        hor_free = false;
                    }
                    if (prioritize_hor)
                    {
                        vert_free = false;
                        hor_free = true;
                    }
                } else
                {
                    //all surrounding brick is breakable
                    vert_free = true;
                    hor_free = true;
                }
            }
            if (vert_free && hor_free)
            {
                //random dir
                rnd_num = rnd.Next(1, 3);
                if (rnd_num == 1) {
                    vert_free = true;
                    hor_free = false;
                } else {
                    vert_free = false;
                    hor_free = true;
                }
            }
            if (vert_free)
            {
                DirY = 1;
                DirX = 0;

                IsVerticalMove = true;
            }
            if (hor_free)
            {
                DirY = 0;
                DirX = 1;

                IsVerticalMove = false;
            }
        }

        public void UpdateDir(bool vert_free, bool hor_free)
        {
            if (_type == 2)
            {
                Random rnd = new Random();

                if (rnd.Next(1, 3) == 1)
                {
                    if (IsVerticalMove && hor_free)
                    {
                        DirY = 0;
                        DirX = 1;

                        IsVerticalMove = false;
                        return;
                    }
                    if (!IsVerticalMove && vert_free)
                    {
                        DirY = 1;
                        DirX = 0;

                        IsVerticalMove = true;
                        return;
                    }
                }
            }
        }
    }
}
