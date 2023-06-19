using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Stromrallye
{
    public partial class Solvable : Form
    {
        Map map;
        Gamestate endstate;

        Queue<int> moves = new Queue<int>();

        Timer tmr = new Timer();

        public bool solvable = false;

        //initialisiert das Objekt mit den zu laufenden Schritten und einer gewissen Situation
        public Solvable(Gamestate state, Map ma)
        {
            InitializeComponent();
            map = ma;
            endstate = state;
        }

        //ändert den Text basierend darauf, ob es lösbar ist oder nicht
        public void IsSolved(bool value)
        {
            if(value)
            {
                button1.Text = "lösbar";
                solvable = value;
            }
            else
            {
                button1.Text = "nicht lösbar";
                solvable = value;
            }
            Refresh();
        }

        //erzeugt den Timer beim klicken des Knopfs
        private void Button1_Click(object sender, EventArgs e)
        {
            if(solvable)
            {
                endstate.moves.ForEach(i => moves.Enqueue(i));
                tmr.Interval = 500;
                tmr.Enabled = true;
                tmr.Tick += new EventHandler(OnTimerEvent);
                this.Hide();
            }
        }
        
        //wird bei jedem Timer Tick aufgerufen und läuft einen Schritt
        private void OnTimerEvent(object sender, EventArgs e)
        {
            if(moves.Count != 0)
            {
                int move = moves.Dequeue();
                map.MoveDir((Movements)move);
            }
        }

        //schließt das Program, wenn ein Fenster geschlossen wird
        private void Solvable_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(1);
        }
    }
}
