using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace Stromrallye
{
    public class CustomRectangle
    {
        //Attribute eines Rechtecks auf dem Spielfeld
        public Rectangle rect;
        public Color color;
        public int charge;
        public States state;
        public Point position;
        public List<Neighbour> neighbours;
        public Label text;
        public int size = 50;

        public CustomRectangle(Point p, int ch, States s, int z)
        {
            //initialisiert die Attribute mit den genannten Parametern
            size = z;
            rect = new Rectangle(p.X * size, p.Y * size, size, size);
            position = p;
            charge = ch;
            state = s;

            //ändert die Farbe, basierend auf der Rolle des Rechteckss
            switch (s)
            {
                case States.Robot:
                    color = Color.Red;
                    break;
                case States.Batterie:
                    color = Color.Green;
                    break;
                case States.Normal:
                    color = Color.Black;
                    break;
                default:
                    color = Color.Black;
                    break;
            }

            neighbours = new List<Neighbour>();
            text = new Label();

            RealignText();
                     
        }

        //passt die Position und Größe des Textelements an
        public void RealignText()
        {
            text.Text = charge.ToString();
            text.Width = rect.Width / 2;
            text.Height = rect.Height / 2;
            text.TextAlign = ContentAlignment.MiddleCenter;
            text.Location = new Point(rect.X + rect.Width / 4, rect.Y + rect.Width / 4);
            text.Font = new Font("Arial", (float)Math.Ceiling(rect.Height / 5f));
            text.ForeColor = Color.Black;
            text.Enabled = false;
        }
    }
}
