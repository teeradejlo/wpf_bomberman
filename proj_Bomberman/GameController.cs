using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Input;

namespace proj_Bomberman
{
    public class GameController {
        private Canvas? _MainCanvas;
        private Player _Player;

        private Dictionary<int, MapObject> _itemCollection;
        private Dictionary<int, Bomb> _bombCollection;
        private Dictionary<int, Blast> _blastCollection;
        private List<Mobs> _mobsCollection;

        private MapObject?[,] _Map;
        private MapObject? _key;
        private MapObject? _door;
        private int _keyPos;
        private int _doorPos;

        private DispatcherTimer mobs_tim;

        private PlayerDel _updatePlayerStats;
        private StringDel _updateGameState;

        private readonly int _mapDimRow;
        private readonly int _mapDimCol;
        private readonly int _itemCount;
        private readonly int _wallCount;
        private readonly int _mobsCount;

        private bool _isPlaying;

        public GameController(Canvas? canvas, int tot_row, int tot_col, int itemCount, int mobCount, PlayerDel playerUpdate, StringDel gameUpdate) {
            if (tot_row < 6 || tot_col < 6) throw new Exception("Invalid Map Size...");

            if (tot_row % 2 == 0) tot_row++;
            if (tot_col % 2 == 0) tot_col++;

            _updatePlayerStats = playerUpdate;
            _updateGameState = gameUpdate;

            _mapDimRow = tot_row;
            _mapDimCol = tot_col;

            _MainCanvas = canvas;
            _Player = new Player(1, 1);
            _Map = new MapObject[_mapDimRow, _mapDimCol];
            _itemCollection = new Dictionary<int, MapObject>();
            _bombCollection = new Dictionary<int, Bomb>();
            _blastCollection = new Dictionary<int, Blast>();
            _mobsCollection = new List<Mobs>();
            _key = null;
            _door = null;
            _keyPos = -1;
            _doorPos = -1;
            _itemCount = itemCount;
            _mobsCount = mobCount;
            _wallCount = (int)(0.4 * (((_mapDimCol - 2) * ((_mapDimRow - 1) / 2)) + (((_mapDimCol - 1) / 2) * ((_mapDimRow - 1) / 2 - 1))));

            _isPlaying = false;

            mobs_tim = new DispatcherTimer();
            mobs_tim.Tick += new EventHandler(GameController_Mobs_Tick);
            mobs_tim.Interval = new TimeSpan(0, 0, 1);
            mobs_tim.Start();
        }

        public void InitGame() {

            Rectangle rec = new Rectangle()
            {
                Width = GlobalVar.BLOCK_SIZE * _mapDimCol,
                Height = GlobalVar.BLOCK_SIZE * _mapDimRow,
                Fill = Brushes.LightGray,
            };
            _MainCanvas?.Children.Add(rec);
            Canvas.SetTop(rec, GlobalVar.CANVAS_PADDING_TOP);
            Canvas.SetLeft(rec, GlobalVar.CANVAS_PADDING_LEFT);

            for (int i = 0; i < _mapDimRow; i++)
            {
                for (int j = 0; j < _mapDimCol; j++)
                {
                    if ((i == 0 || i == _mapDimRow - 1) || (i % 2 == 0 && j % 2 == 0) || (j == 0 || j == _mapDimCol - 1))
                    {
                        _Map[i, j] = new MapObject("unbreakwall");
                        AddToCanvas(_Map[i, j], i, j);
                    }
                }
            }

            Random rnd = new Random();
            int item_added = 0;
            int wall_added = 0;
            int mobs_added = 0;
            int row = -1;
            int col = -1;

            while (wall_added < _wallCount || mobs_added < _mobsCount)
            {
                row = rnd.Next(1, _mapDimRow - 1);
                col = rnd.Next(1, _mapDimCol - 1);

                if (row > 3 - col)
                {
                    if (_Map[row, col] == null)
                    {
                        if (wall_added < _wallCount)
                        {
                            if (_key == null)
                            {
                                _key = new MapObject("key");
                                _keyPos = 100 * row + col;
                                AddToCanvas(_key, row, col);
                            }
                            else if (_door == null)
                            {
                                _door = new MapObject("door");
                                _doorPos = 100 * row + col;
                                AddToCanvas(_door, row, col);
                            }
                            else if (item_added < _itemCount)
                            {
                                int rnd_num = rnd.Next(1, 3);

                                if (rnd_num == 1)
                                {
                                    _itemCollection[100 * row + col] = new MapObject("extraBomb");
                                }
                                else
                                {
                                    _itemCollection[100 * row + col] = new MapObject("extraRange");
                                }
                                AddToCanvas(_itemCollection[100 * row + col], row, col);
                                item_added++;
                            }

                            _Map[row, col] = new MapObject("wall");
                            AddToCanvas(_Map[row, col], row, col);
                            wall_added++;
                        } else
                        {
                            if (row > 6 - col)
                            {
                                bool vert_free = false;
                                bool hor_free = false;
                                bool prioritize_vert = true;
                                bool prioritize_hor = true;

                                if (_Map[row + 1, col] == null || _Map[row - 1, col] == null)
                                {
                                    vert_free = true;
                                } else
                                {
                                    if (_Map[row + 1, col].Type == "unbreakwall" && _Map[row - 1, col].Type == "unbreakwall")
                                    {
                                        prioritize_vert = false;
                                    }
                                }

                                if (_Map[row, col + 1] == null || _Map[row, col - 1] == null)
                                {
                                    hor_free = true;
                                } else
                                {
                                    if (_Map[row, col + 1].Type == "unbreakwall" && _Map[row, col - 1].Type == "unbreakwall")
                                    {
                                        prioritize_hor = false;
                                    }
                                }

                                Mobs mob = new Mobs(row, col, vert_free, hor_free, prioritize_vert, prioritize_hor);
                                _mobsCollection.Add(mob);
                                AddToCanvas(mob, row, col);
                                mobs_added++;
                            }
                        }
                    }
                }
            }

            AddToCanvas(_Player, _Player.Row, _Player.Col, 1);
            _updatePlayerStats(_Player);
            _isPlaying = true;
        }

        private void StopTimers()
        {
            _isPlaying = false;
            mobs_tim.Stop();
            foreach(Bomb bomb in _bombCollection.Values)
            {
                bomb.StopTimer();
            }
            foreach(Blast blast in _blastCollection.Values)
            {
                blast.StopTimer();
            }
        }

        private void AddToCanvas(MapObject obj, int row, int col) {
            _MainCanvas?.Children.Add(obj.img);

            Canvas.SetTop(obj.img, GlobalVar.CANVAS_PADDING_TOP + GlobalVar.BLOCK_SIZE * row);
            Canvas.SetLeft(obj.img, GlobalVar.CANVAS_PADDING_LEFT + GlobalVar.BLOCK_SIZE * col);
        }

        private void AddToCanvas(MapObject obj, int row, int col, int height)
        {
            AddToCanvas(obj, row, col);
            Canvas.SetZIndex(obj.img, height);
        }

        private void DisposeFromMap(MapObject obj, int row, int col)
        {
            if (_Map[row, col] == obj)
            {
                _MainCanvas?.Children.Remove(obj.img);
                _Map[row, col] = null;
            }
        }

        public void ReadInput(bool up, bool down, bool left, bool right) {
            if (up)
            {
                MovePlayer(_Player.Row - 1, _Player.Col);
            }
            else if (down)
            {
                MovePlayer(_Player.Row + 1, _Player.Col);
            }
            else if (left)
            {
                MovePlayer(_Player.Row, _Player.Col - 1);
            }
            else if (right)
            {
                MovePlayer(_Player.Row, _Player.Col + 1);
            }
        }

        public void ReadInput(bool space) {
            AddBomb(_Player.Row, _Player.Col);
        }

        public void MovePlayer(int row, int col)
        {
            if (IsValidSpot(row, col))
            {
                Canvas.SetTop(_Player.img, GlobalVar.CANVAS_PADDING_TOP + GlobalVar.BLOCK_SIZE * row);
                Canvas.SetLeft(_Player.img, GlobalVar.CANVAS_PADDING_LEFT + GlobalVar.BLOCK_SIZE * col);

                _Player.Row = row;
                _Player.Col = col;

                if (_key != null && _keyPos == 100 * row + col)
                {
                    _Player.HasKey = true;
                    DisposeItem(_key, row, col);
                }
                if (_itemCollection.ContainsKey(100 * row + col))
                {
                    if (_itemCollection[100 * row + col].Type == "extraRange")
                    {
                        _Player.BombRange++;
                    } else
                    {
                        _Player.MaxBomb++;
                    }
                    DisposeItem(_itemCollection[100 * row + col], row, col);
                }
                if (_Player.HasKey && _doorPos == 100 * row + col)
                {
                    StopTimers();
                    _updateGameState("You Win!!!");
                    return;
                }
                foreach (int val in _blastCollection.Keys)
                {
                    if (_Player.Row == ((val/100) % 100) && _Player.Col == (val % 100)) {
                        StopTimers();
                        _updateGameState("Game Over...");
                        return;
                    }
                }
                foreach (Mobs mob in _mobsCollection)
                {
                    if (_Player.Row == mob.Row && _Player.Col == mob.Col)
                    {
                        StopTimers();
                        _updateGameState("Game Over...");
                        return;
                    }
                }

                _updatePlayerStats(_Player);
            }
        }

        public bool IsValidSpot(int row, int col)
        {
            if (_Map[row, col] == null && !_bombCollection.ContainsKey(100 * row + col))
                return true;
            return false;
        }

        public void AddBomb(int row, int col)
        {
            if (!_bombCollection.ContainsKey(100 * row + col) && _Player.BombPlaced < _Player.MaxBomb)
            {
                _bombCollection[100 * row + col] = new Bomb(row, col, _Player.BombRange, _Player.NextBombIndex, DisposeBomb);
                AddToCanvas(_bombCollection[100 * row + col], row, col);

                _Player.UpdateBombIndex();
                _Player.BombPlaced++;
            }
        }

        private void DisposeBomb(Bomb obj, int row, int col)
        {
            if (_bombCollection.ContainsKey(100 * row + col))
            {
                _MainCanvas?.Children.Remove(obj.img);
                _bombCollection.Remove(100 * row + col);
                GenerateBlastEffect(obj, row, col);

                _Player.BombPlaced--;
            }
        }

        private void GenerateBlastEffect(Bomb obj, int row, int col)
        {
            int[] dirX = { 0, 1, 0, -1 };
            int[] dirY = { 1, 0, -1, 0 };
            bool[] dirValid = { true, true, true, true };

            //blast at the bomb
            AddBlast(10000 * obj.BombIndex + 100 * row + col);

            MapObject? dummy = null;
            int curr_range = 1;
            while (curr_range <= obj.Range)
            {
                for (int i = 0; i < dirX.Length; i++)
                {
                    if (dirValid[i])
                    {
                        int temp_row = row + curr_range * dirY[i];
                        int temp_col = col + curr_range * dirX[i];
                        dummy = _Map[temp_row, temp_col];
                        if (dummy != null)
                        {
                            if (dummy.Type == "unbreakwall")
                            {
                                dirValid[i] = false;
                            } else if (dummy.Type == "wall")
                            {
                                dirValid[i] = false;
                                int index = 10000 * obj.BombIndex + 100 * temp_row + temp_col;
                                AddBlast(index);
                                DisposeFromMap(dummy, temp_row, temp_col);
                                _blastCollection[index].Accelerate();
                            }
                        }
                        else
                        {
                            if (_bombCollection.ContainsKey(100 * temp_row + temp_col))
                            {
                                _bombCollection[100 * temp_row + temp_col].InstantDetonate();
                            }

                            if (_itemCollection.ContainsKey(100 * temp_row + temp_col))
                            {
                                DisposeItem(_itemCollection[100 * temp_row + temp_col], temp_row, temp_col);
                            }

                            for (int j = _mobsCollection.Count - 1; j >= 0; j--)
                            {
                                if (_mobsCollection[j].Row == temp_row && _mobsCollection[j].Col == temp_col)
                                {
                                    DisposeMob(_mobsCollection[j], j);
                                }
                            }

                            int index = 10000 * obj.BombIndex + 100 * temp_row + temp_col;
                            AddBlast(index);
                        }
                    }
                }
                curr_range++;
            }

            if (_isPlaying)
            {
                foreach (int key in _blastCollection.Keys)
                {
                    if ((key / 100) % 100 == _Player.Row && key % 100 == _Player.Col)
                    {
                        StopTimers();
                        _updateGameState("Game Over...");
                        return;
                    }
                }
            }
        }

        private void AddBlast(int index)
        {
            _blastCollection[index] = new Blast(index, DisposeBlast);
            AddToCanvas(_blastCollection[index], (index/100) % 100, index % 100);
        }

        private void DisposeBlast(Blast obj, int index)
        {
            if (_blastCollection.ContainsKey(index) && _blastCollection[index] == obj)
            {
                _blastCollection.Remove(index);
            }
            _MainCanvas?.Children.Remove(obj.img);
        }

        private void DisposeItem(MapObject obj, int row, int col)
        {
            if (obj.Type == "key")
            {
                _MainCanvas?.Children.Remove(obj.img);
                _key = null;
                _keyPos = -1;
            } else
            {
                _MainCanvas?.Children.Remove(obj.img);
                _itemCollection.Remove(100 * row + col);
            }
        }

        private void GameController_Mobs_Tick(object? sender, EventArgs e)
        {
            UpdateMobsDir();

            for (int i = _mobsCollection.Count - 1; i >= 0; i--)
            {
                Mobs mob = _mobsCollection[i];
                int temp_row = mob.Row + mob.DirY;
                int temp_col = mob.Col + mob.DirX;

                if (_Map[temp_row, temp_col] != null || _bombCollection.ContainsKey(100 * temp_row + temp_col) || !IsNoMob(mob, temp_row, temp_col))
                {
                    mob.DirY *= -1;
                    mob.DirX *= -1;

                    temp_row = mob.Row + mob.DirY;
                    temp_col = mob.Col + mob.DirX;

                    //if flip dir and no obstacle, move the mob; else do nothing
                    if (_Map[temp_row, temp_col] == null && !_bombCollection.ContainsKey(100 * temp_row + temp_col) && IsNoMob(mob, temp_row, temp_col))
                    {
                        MoveMobs(mob, temp_row, temp_col);
                    }
                }
                else
                {
                    MoveMobs(mob, temp_row, temp_col);
                }

                foreach (int blast_key in _blastCollection.Keys)
                {
                    if (((blast_key / 100) % 100) == mob.Row && (blast_key % 100) == mob.Col)
                    {
                        DisposeMob(mob, i);
                        break;
                    }
                }
            }
        }

        private void UpdateMobsDir()
        {
            foreach (Mobs mob in _mobsCollection)
            {
                bool vert_free = false;
                bool hor_free = false;
                int row = mob.Row;
                int col = mob.Col;

                if (_Map[row + 1, col] == null || _Map[row - 1, col] == null) vert_free = true;
                if (_Map[row, col + 1] == null || _Map[row, col - 1] == null) hor_free = true;

                mob.UpdateDir(vert_free, hor_free);
            }
        }

        private bool IsNoMob(Mobs mob, int row, int col)
        {
            foreach (Mobs comp_mob in _mobsCollection)
            {
                if (mob != comp_mob && comp_mob.Row == row && comp_mob.Col == col)
                {
                    return false;
                }
            }
            return true;
        }

        private void MoveMobs(Mobs mob, int row, int col)
        {
            Canvas.SetTop(mob.img, GlobalVar.CANVAS_PADDING_TOP + GlobalVar.BLOCK_SIZE * row);
            Canvas.SetLeft(mob.img, GlobalVar.CANVAS_PADDING_LEFT + GlobalVar.BLOCK_SIZE * col);

            mob.Row = row;
            mob.Col = col;

            if (_Player.Row == mob.Row && _Player.Col == mob.Col)
            {
                StopTimers();
                _updateGameState("Game Over...");
                return;
            }
        }

        private void DisposeMob(Mobs mob, int index)
        {
            if (_mobsCollection[index] == mob)
            {
                _MainCanvas?.Children.Remove(mob.img);
                _mobsCollection.RemoveAt(index);
            }
        }

        public void ClearAll()
        {
            StopTimers();

            if (_MainCanvas != null)
            {
                List<Rectangle> rec_list = new List<Rectangle>();

                IEnumerable<Rectangle>? rec_enum = _MainCanvas.Children.OfType<Rectangle>();
                foreach (var rec in rec_enum)
                {
                    rec_list.Add(rec);
                }

                for (int i = rec_list.Count - 1; i >= 0; i--)
                {
                    _MainCanvas.Children.Remove(rec_list[i]);
                }

                List<Image> img_list = new List<Image>();
                IEnumerable<Image> img_enum = _MainCanvas.Children.OfType<Image>();
                foreach (var img in img_enum)
                {
                    img_list.Add(img);
                }

                for (int i = img_list.Count - 1; i >= 0; i--)
                {
                    _MainCanvas.Children.Remove(img_list[i]);
                }
            }

            _MainCanvas = null;
            _itemCollection.Clear();
            _bombCollection.Clear();
            _blastCollection.Clear();
            _mobsCollection.Clear();

            _key = null;
            _door = null;

            for (int i = 0; i < _mapDimRow; i++)
            {
                for (int j = 0; j < _mapDimCol; j++)
                {
                    _Map[i, j] = null;
                }
            }
        }
    }
}
