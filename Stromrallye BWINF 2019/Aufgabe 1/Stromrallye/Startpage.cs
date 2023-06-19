using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;

namespace Stromrallye
{
    public partial class Stromrallye : Form
    {
        public Stromrallye()
        {
            InitializeComponent();
        }

        //Event, wenn man auf Lösen klickt
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                //überprüft den Pfad und ob dieser gültig ist
                string path = @textBox1.Text;
                if(!textBox1.Text.Contains(@"\"))
                {
                    textBox1.Text = "Ungültiger Pfad!";
                    return;
                }

                //liest alle Zeilen ein
                string[] file = File.ReadAllLines(path);

                //initialisiert das erste Spielszenario
                Map m1 = new Map(int.Parse(file[0]));

                //liest ab Zeile drei, die einzelnen Batterien ein
                int curline = 3;
                for (int i = 0; i < int.Parse(file[2]); i++)
                {
                    string line = file[curline];
                    int[] values;
                    values = StringToIntArray(line);
                    m1.AddBatterie(values[0] - 1, values[1] - 1, values[2]);
                    curline++;
                }
                
                //liest den Roboter un seine Werte ein
                int[] rvalues;
                rvalues = StringToIntArray(file[1]);
                              
                m1.AddRobot(rvalues[0] -1, rvalues[1] -1 , rvalues[2]);

                //fängt an eine Lösung zu finden, falls es eine gibt
                m1.CreateMatchfield();
                m1.GetDistances();

                m1.start_state = new Gamestate(m1.robot, m1.all_batteries, new List<int>());
                
                m1.SolveDFS();

                //lässt dieses Menü verschwinden und zeigt das nächste
                m1.Show();
                this.Hide();
            }
        }
        //https://stackoverflow.com/questions/1763613/convert-comma-separated-string-of-ints-to-int-array
        //transformiert einen String mit Zahlen zu einem Integer Array
        private int[] StringToIntArray(string myNumbers)
        {
            List<int> myIntegers = new List<int>();
            Array.ForEach(myNumbers.Split(",".ToCharArray()), s =>
            {
                int currentInt;
                if (Int32.TryParse(s, out currentInt))
                    myIntegers.Add(currentInt);
            });
            return myIntegers.ToArray();
        }

        //Event, wenn man auf Generieren klickt
        private void button2_Click(object sender, EventArgs e)
        {
            MapGenerator map = new MapGenerator();
            this.Hide();
        }
    }
}
