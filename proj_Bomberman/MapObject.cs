using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace proj_Bomberman
{
    public class MapObject {
        public readonly Image img;
        public readonly string Type;

        public MapObject (string type) {
            Type = type;

            img = new Image
            {
                Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Resources/" + Type + ".png"))),
            };
        }
    }
}