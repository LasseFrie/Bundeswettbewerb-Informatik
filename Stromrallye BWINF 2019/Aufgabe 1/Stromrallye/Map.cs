using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Stromrallye
{
    public partial class Map : Form
    {
        //Definiert alle wichtigen Bestandteile eines Spielszenarios
        public CustomRectangle[,] all_rects;
        public List<CustomRectangle> all_batteries = new List<CustomRectangle>();
        public CustomRectangle robot;
        public Solvable box;

        //Das erste Spielstadium, wird von der Eingabe definiert
        public Gamestate start_state;

        //gibt einen Pfad in Form von einer Zahlenfolge an
        List<int> path = new List<int>();

        //definiert die Bewegungsrichtungen bei der Pfadfindung
        int[] rowNum = { -1, 0, 0, 1 };
        int[] colNum = { 0, -1, 1, 0 };

        //1 = begehbar; 0 = nicht begehbar
        int[,] mat;

        //Spielfeldgröße
        public static int field_size;

        //Anpassbare Parameter
        public int rect_size = 50;

        public Map(int size)
        {
            //Winform spezifische Einstellungen
            InitializeComponent();
            DoubleBuffered = true;

            //bestimmt die Spielfeldgröße und die daraus resultierende Rechtecksgröße
            field_size = size;
            rect_size = (GetScreen().Height / field_size) - (60/size);

            //initialisiert diese Felder mit der Größe des Spielfelds
            all_rects = new CustomRectangle[field_size, field_size];
            mat = new int[field_size, field_size];
        }

        //erzeugt ein Spielfeld
        public void CreateMatchfield()
        {
            //skaliert die Applikationsfenster Größe
            this.Size = new Size(field_size * rect_size + 17, field_size * rect_size + 40);

            //geht durch jede Reihe und Spalte und setzt in diese ein leeres REchteck oder eine Batterie
            int xKord = 0;
            int yKord = 0;
            for (int i = 0; i < field_size; i++)
            {
                xKord = 0;
                for (int j = 0; j < field_size; j++)
                {
                    all_rects[j, i] = new CustomRectangle(new Point(xKord/rect_size, yKord/rect_size),  0,  States.Normal, rect_size);
                    //setzt diese Position als begehbar
                    mat[j, i] = 1;
                    //probiert alle Batterien aus
                    for (int h = 0; h < all_batteries.Count; h++)
                    {
                        //wenn eine Batterie auf der besagten Position ist
                        if (all_batteries[h].position == new Point(j, i))
                        {
                            all_rects[j, i] = new CustomRectangle(new Point(xKord / rect_size, yKord / rect_size), all_batteries[h].charge, all_batteries[h].state, rect_size);
                            //setzt diese Position als nicht begehbar
                            mat[j, i] = 0;
                        }
                    }
                    xKord += rect_size;
                }
                yKord += rect_size;
            }
        }

        //definiert die minimale Distanz zwischen dem Roboter, allen Batterien und zwischen den Batterien untereinander
        public void GetDistances()
        {
            //löscht alle nachbarn des Robotors
            robot.neighbours.Clear();
            mat[robot.position.X, robot.position.Y] = 0;

            //geht alle Batterien durch (erste Schleife)
            foreach (CustomRectangle batterie in all_batteries)
            {
                //setzt die Positionen beider Rechtecke auf begehbar
                mat[batterie.position.X, batterie.position.Y] = 1;
                mat[robot.position.X, robot.position.Y] = 1;

                //kalkuliert die Distanz zwischen diesen beiden Rechtecken
                int bfs = PathBFS(mat, batterie.position, robot.position);

                //setzt die Positionen wieder auf nicht beghbar
                mat[batterie.position.X, batterie.position.Y] = 0;
                mat[robot.position.X, robot.position.Y] = 0;

                //falls es zu keinen Fehler gekommen ist, fügt man dieses Rechteck zu den Nachbarn hinzu
                if (bfs > 0)
                {
                    robot.neighbours.Add(new Neighbour(batterie, bfs, path));
                }

                //vergleicht die Batterie aus der ersten Schleife mit allen anderen Batterien (zweite Schleife)
                foreach (CustomRectangle compare_batterie in all_batteries)
                {
                    //setzt die Positionen der beiden Batterien als begehbar
                    mat[batterie.position.X, batterie.position.Y] = 1;
                    mat[compare_batterie.position.X, compare_batterie.position.Y] = 1;

                    //berechnet die kleinste Entfernung zwischen diesen beiden Batterien
                    bfs = PathBFS(mat, compare_batterie.position, batterie.position);

                    //setzt die Positionen wieder zurück auf nicht begehebar
                    mat[batterie.position.X, batterie.position.Y] = 0;
                    mat[compare_batterie.position.X, compare_batterie.position.Y] = 0;

                    //falls es nicht die beiden selben Batterien sind und die Distanz größer 0 ist
                    if (batterie != compare_batterie &&  bfs > 0)
                    {
                        //fügt die Batterie als nachbar der Batterie aus der ersten Schleife hinzu
                        batterie.neighbours.Add(new Neighbour(compare_batterie, bfs, path));
                    }
                }
            }
        }

        //https://www.geeksforgeeks.org/shortest-path-in-a-binary-maze/
        //Implementierung des Mazerunner Algorithmus
        int PathBFS(int[,] mat, Point src, Point dest)
        {
            //überprüft ob der Start und das Ende begehbar sind
            if (mat[src.X, src.Y] != 1 || mat[dest.X, dest.Y] != 1)
                return -1;

            bool[,] visited = new bool[field_size, field_size];

            // markiert die Anfangszelle als besucht
            visited[src.X, src.Y] = true;

            // erstellt eine Queue für die Breitensuche
            Queue<QueueNode> q = new Queue<QueueNode>();

            // die Distanz zur Anfangszelle beträgt 0
            QueueNode source = new QueueNode(src, 0, null);
            q.Enqueue(source); // fügt die Anfangszelle zur Queue hinzu

            // Breitensuche, startend von der Anfangszelle
            while (q.Count != 0)
            {
                QueueNode curr = q.Peek();
                Point pt = curr.pt;

                //falls man das Ende erreicht hat bricht man ab
                if (pt.X == dest.X && pt.Y == dest.Y)
                {
                    GetPath(curr);
                    return curr.dist;
                }

                //sonst nimmt man das oberste Element und fügt dessen Adjazenz Nachbarn hinzu
                q.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int row = pt.X + rowNum[i];
                    int col = pt.Y + colNum[i];

                    //falls die zurzeitige Zelle noch nicht besucht wurde und innerhalb der Matrix liegt,
                    //fügt man diese zur Queue hinzu
                    if (isValid(row, col) && mat[row, col] == 1 && !visited[row, col])
                    {
                        //markiert diese Zelle als besucht und fügt sie zur Queue hinzu
                        visited[row, col] = true;
                        QueueNode adjacenzcell = new QueueNode(new Point(row, col), curr.dist + 1, curr);
                        q.Enqueue(adjacenzcell);
                    }
                }
            }
            //Falls es zu keinem Ergebnis kommt, ist die Distanz -1
            return -1;
        }

        //verfolgt die Bewegungen die man nehmen muss, um von einem gewissen Punkt zu dessen Nachbarn zu kommen
        public void GetPath(QueueNode end_node)
        {
            QueueNode curr = end_node;

            //macht den zurzeitigen Pfad leer
            path.Clear();

            //fügt jeweils den Schritt zu dem Parent der zurzeitigen Position hinzu
            while(curr.parent != null)
            {
                //wenn die x Position der beiden gleich ist, gibt es eine y Verschiebung
                if(curr.pt.X == curr.parent.pt.X)
                {
                    //Multiplikation mit 2 um zu bestimmen, dass es sich hier um eine y Verschiebung handelt
                    path.Add((curr.parent.pt.Y - curr.pt.Y) * 2);
                }

                //wenn die y Koordinate gleich ist gibt es eine x Verschiebung
                else if(curr.pt.Y == curr.parent.pt.Y)
                {
                    //keine Multiplikation und somit eine x Verschiebung
                    path.Add((curr.parent.pt.X - curr.pt.X));
                }
                curr = curr.parent;
            }
        }

        //löst den Baum in Form einer Tiefensuche durch alle möglichen Szenarien
        public void SolveDFS()
        {
            //zurzeitige Situation
            Gamestate state = null;

            //Stack zur Tiefensuche
            Stack<Gamestate> s = new Stack<Gamestate>();

            //Liste an Nachbarn einer Situation 
            List<Gamestate> neighbours = new List<Gamestate>();

            //fügt die Anfangssituation zu dem Stack hinzu
            s.Push(start_state);
            
            //testet, ob die Anfangssituation schon fertig ist
            bool isDone = IsDone(start_state);

            //Tiefensuche durch alle Szenarien durch
            while(!isDone && s.Count != 0)
            {
                //nimmt das oberste Element 
                state = s.Pop();
                
                //macht die zurzeitgen Nachbarn leer und fügt die Nachbarn dieser Situation dem Stack hinzu
                neighbours.Clear();
                neighbours = GetFollowingStates(state);
                for (int i = 0; i < neighbours.Count; i++)
                {
                  s.Push(neighbours[i]);
                }

                //überprüft ob man fertig ist
                isDone = IsDone(state);
                if (!isDone) state = null;
            }

            if (state == null) state = start_state;

            //ruft das Finale Anzeigefenster auf
            box = new Solvable(state, this);

            //setzt die Batterien zurück
            for (int i = 0; i < start_state.batteries.Count; i++)
            {
                all_batteries[i].charge = start_state.batteries[i].charge;
            }

            //falls es eine Lösung gibt, zeigt man die Anfangssituation mit den Bewegungen zur Endsituation an
            if (isDone && LastSteps(state))
            {
                box.IsSolved(true);
            }
            box.Show();
        }

        //Fügt die Schritte hinzu, die benötigt werden um den Roboter am Ende zu entladen
        public bool LastSteps(Gamestate end_state)
        {
            //falls der Roboter keine Ladung mehr hat passt alles
            if (end_state.robot.charge == 0)
            {
                return true;
            }

            //falls der Roboter noch eine Ladung hat muss er sich in iergendeine Richtung bewegen
            else if (end_state.robot.charge == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (isValid(end_state.robot.position.X + rowNum[i], end_state.robot.position.Y + colNum[i]))
                    {
                        int xMove = rowNum[i];
                        int yMove = colNum[i] * 2;
                        if (xMove != 0) end_state.moves.Add(xMove);
                        else if (yMove != 0) end_state.moves.Add(yMove);
                        return true;
                    }
                }
            }

            //falls der Roboter mehr Ladung hat muss er schauen ob es zwei freie Felder gibt auf welchem er diese ablaufen kann
            else if (end_state.robot.charge > 1)
            {
                Point curr = new Point(end_state.robot.position.X, end_state.robot.position.Y);
                int steps_taken = 0;
                for(int i = 0; i < end_state.robot.charge; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (isValid(curr.X + rowNum[j], curr.Y + colNum[j])
                            && all_rects[curr.X + rowNum[j], curr.Y + colNum[j]].state == States.Normal)
                        {
                            curr.X = curr.X + rowNum[j];
                            curr.Y = curr.Y + colNum[j];

                            int xMove = rowNum[j];
                            int yMove = colNum[j] * 2;

                            if (xMove != 0)
                            {
                                end_state.moves.Add(xMove);
                            }
                            else if (yMove != 0)
                            {
                                end_state.moves.Add(yMove);
                            }

                            steps_taken++;
                        }
                    }
                }
                if (steps_taken >= end_state.robot.charge) return true;
            }                              
            return false;
        }

        //Hilfsmethode, um alle Nachbarsituationen einer gewissen Situation zu bekommen
        public List<Gamestate> GetFollowingStates(Gamestate s)
        {
            //leere Liste an Nachbarsituationen
            List<Gamestate> neighbour_states = new List<Gamestate>();

            //leere Liste an Bewegungen
            List<int> moves = new List<int>();

            //erstellt eine Kopie des Roboters der zurzeitigen Situation
            CustomRectangle r = new CustomRectangle(s.robot.position, s.robot.charge, States.Robot, robot.size);
            
            //fügt dieser Kopie Standardmäßig alle Nachbarn des Anfangsroboters hinzu
            robot.neighbours.ForEach(i => r.neighbours.Add(i));

            //kopiert die Ladung der Batterien aus der zurzeitigen Situation
            for (int i = 0; i < s.batteries.Count; i++)
            {
                all_batteries[i].charge = s.batteries[i].charge;

                //falls der Roboter auf iergeneiner Batterie stehen sollte, fügt man die Nachbarn der Batterie zu den Nachbarn des Roboters hinzu
                if(r.position == all_batteries[i].position)
                {
                    r.neighbours.Clear();
                    all_batteries[i].neighbours.ForEach(j => r.neighbours.Add(j));
                }
            }

            //fügt sich selbst als Nachbar mit der distanz 2 hinzu, wird aber seperat berechnet
            if(SelfNeighbourState(s) != null && r.charge >= 2) neighbour_states.Add(SelfNeighbourState(s));

            //geht alle Nachbarn des zurzeitgen Roboters durch
            foreach (Neighbour neighbour in r.neighbours)
            {
                //falls die Ladung des Roboter ausreicht um zu dem Nachbarn zu gehen, und dieser nicht eine Ladung von 0 besitzt
                if(r.charge >= neighbour.distance && neighbour.rect.charge > 0)
                {
                    //subtrahiert die Distanz zum nächsten Nachbarn von der Ladung des Roboters
                    r.charge -= neighbour.distance;

                    //setzt die Position des Roboters mit der Positon des Nachbars gleich
                    r.position = neighbour.rect.position;

                    //wechselt die Ladungen der beiden, da sie nun "aufeinander" stehen
                    int transfer_charge = r.charge;
                    r.charge = neighbour.rect.charge;
                    neighbour.rect.charge = transfer_charge;

                    //fügt zuerst alle Züge hinzu, die gebraucht werden, um zu der übergebenen Situation zu kommen
                    moves.Clear();
                    s.moves.ForEach(i => moves.Add(i));
                    neighbour.moves.ForEach(i => moves.Add(i));

                    //erstellt eine neue Nachbarsituation, mit dem "neuen" Roboter, den Batterien und den dazugehörigen Zügen
                    neighbour_states.Add(new Gamestate(r, all_batteries, moves));
                                                            
                    //setzt die Ladung und Position zurück, für den nächsten Schleifendurchgang
                    r.charge = s.robot.charge;
                    r.position = s.robot.position;

                    //setzt die Ladung aller Batterien für den nächsten Schleifendurchgang zurück
                    for (int i = 0; i < all_batteries.Count; i++)
                    {
                        all_batteries[i].charge = s.batteries[i].charge;
                    }
                }
            }
            
            //gibt alle Nachbarszenarien zurück
            return SortList(r, neighbour_states);
        }
        
        //findet die "beste" Nachbarsituation heraus, um diese zuerst Nachzuverfolgen
        //die beste Situation ist die, in der der Roboter auf der Batterie mit der höchsten Ladung steht
        List<Gamestate> SortList(CustomRectangle r, List<Gamestate> states)
        {
            List<Gamestate> neighbour_states = states;

            int best_move = 0;
            Point best_position = new Point();

            //geht durch alle Nachbarn des Roboters durch 
            for (int i = 0; i < r.neighbours.Count; i++)
            {
                //falls die Ladung des Nachbarn größer ist als die zurzeitg Beste
                if (r.neighbours[i].rect.charge > best_move)
                {
                    //setzt die zurzeitig beste Ladung gleich der Ladung des Nachbarn und setzt die beste Position gleich der Postion des Nachbarn
                    best_move = r.neighbours[i].rect.charge;
                    best_position = r.neighbours[i].rect.position;
                }
            }

            //geht jetzt durch alle Nachbar Szenarien durch
            for (int j = 0; j < neighbour_states.Count; j++)
            {
                //Falls der Roboter in diesem Szenario auf der oben besagten Position steht, wechselt man das letzte Element der Liste mit dem zurzeitigen
                if (neighbour_states[j].robot.position == best_position)
                {
                    Swap(neighbour_states, neighbour_states.IndexOf(neighbour_states[j]), neighbour_states.Count - 1);
                }
            }
  
            neighbour_states = CutList(neighbour_states);

            return neighbour_states;
        }

        //erstellt ein Szenanrio bei dem der Roboter von der Batterie runter und wieder drauf geht
        Gamestate SelfNeighbourState(Gamestate s)
        {
            //Rückgabewert
            Gamestate neighbourstate = null;

            //Feld mit den möglichen Positionen die in der Entfernung eins vom Roboter liegen
            List<Point> possible_moves = new List<Point>();

            //die letzten Endes ausgeführten Bewegungen
            List<int> moves = new List<int>(s.moves);

            //Kopie des Roboters des zurzeitigen Szenarios
            CustomRectangle r = new CustomRectangle(s.robot.position, s.robot.charge, States.Robot, robot.size);

            //geht alle vier Richtungen durch
            for (int i = 0; i < 4; i++)
            {
                //überprüft ob sich diese Position noch im Spielfeld befindet
                if(isValid(s.robot.position.X + rowNum[i], s.robot.position.Y + colNum[i]))
                {
                    //fügt diese Nachbarposition den möglichen moves hinzu
                    possible_moves.Add(new Point(s.robot.position.X + rowNum[i], s.robot.position.Y + colNum[i]));
                }
            }
            if(possible_moves.Count > 0)
            {
                //entfernt alle Positionen auf denen schon Batterien liegen von den möglichen Bewegungen
                for (int j = 0; j < s.batteries.Count; j++)
                {
                   if(possible_moves.Contains(s.batteries[j].position))
                   {
                        possible_moves.Remove(s.batteries[j].position);
                   }
                }
            }

            //überprüft ob es überhaupt noch eine valide Bewegung gibt, sonst wird null zurück gegeben
            if (possible_moves.Count > 0)
            {
                int difference = 0;
                if(possible_moves[0].X == s.robot.position.X)
                {
                    difference = possible_moves[0].Y + s.robot.position.Y;
                    moves.Add(difference * 2);
                    moves.Add(difference * (-2));
                }
                else if (possible_moves[0].Y == s.robot.position.Y)
                {
                    difference = possible_moves[0].X - s.robot.position.X;
                    moves.Add(difference);
                    moves.Add(difference * (-1));
                }
                r.charge -= 2;

                foreach (CustomRectangle batterie in all_batteries)
                {
                    if(batterie.position == r.position)
                    {
                        SwitchCharge(r, batterie);
                    }
                }

                neighbourstate = new Gamestate(r, all_batteries, moves);
                for (int i = 0; i < all_batteries.Count; i++)
                {
                    all_batteries[i].charge = s.batteries[i].charge;
                }
                return neighbourstate;
            }
            else
            {
                return null;
            }
        }

        //verkleinert die möglichen Nachbarn, indem man Graphen die nicht voll begehbar sind entfernt
        private List<Gamestate> CutList(List<Gamestate> states)
        {
            List<Gamestate> return_states = new List<Gamestate>();
                       
            foreach (Gamestate state in states)
            {
                bool[,] mat = state.TransformToMatrix();
                
                if (IsConnected(mat, mat.GetLength(0)))
                {
                    return_states.Add(state);
                }
            }

            return return_states;
        }

        //durchläuft den zurzeitigen Graphen und prüft ob dieser voll begehbar ist
        private bool IsConnected(bool[,] adj, int verteces, int start = 0)
        {
            //Falls es nurnoch den Robobter gibt, gibt es keinen Graphen
            if (verteces == 0) return true;

            //Stapel zur Tiefensuche durch die Adjazenzmatrix
            Stack<int> next_vertex = new Stack<int>();

            //Knoten die schon besucht wurden
            List<int> cur_visited = new List<int>();

            //Schleife durch die gesamte Adjazenzmatrix
            for (int i = 0; i < verteces; i++)
            {
                //löscht die besuchten Knoten für diesen durchlauf
                cur_visited.Clear();

                //packt den Anfangsknoten auf den Stapel
                next_vertex.Push(i);

                //Iterativer durchlau durch den Stapel
                while(next_vertex.Count != 0)
                {
                    //nimmt das oberste Element des Stapels
                    int current_vertex = next_vertex.Pop();

                    //durchsucht alle Nachbarn des obersten Elements
                    for (int j = 0; j < verteces; j++)
                    {
                        //Falls man zu diesem Nachbarn eine Verbindung hat und man diesen noch nicht besucht hat fügt man diesen zum Stapel hinzu
                        if(adj[current_vertex, j] && !cur_visited.Contains(j))
                        {
                            next_vertex.Push(j);
                        }
                    }

                    //Fügt das zurzeitige Element der besuchten Knoten liste hinzu, falls dieses nicht schon vorhanden ist
                    if(!cur_visited.Contains(current_vertex))
                    {
                        cur_visited.Add(current_vertex);
                    }
                }

                //Falls man insgesamt genauso viele Knoten besucht hat, wie es auch gibt, ist der Graph voll begehbar
                if (cur_visited.Count == verteces) return true;
            }

            return false;
        }
                
        //Hilfsmethode zur Skalierung der Applikation
        public Rectangle GetScreen()
        {
            return Screen.FromControl(this).Bounds;
        }

        //gibt wahr zurück. wenn alle Batterien in einer Situation entladen sind
        bool IsDone(Gamestate s)
        {
            if (s.BatterieCharge() == 0) return true;
            else return false;
        }

        //Hilfsmethode für die Pfadfindung
        bool isValid(int row, int col)
        {
            //ist wahr, wenn die Koordinaten sich im Rahmen der Spielfeldgröße befinden
            return (row >= 0) && (row < field_size) && (col >= 0) && (col < field_size);
        }

        //wechselt zwei Elemente einer Liste
        public void Swap(List<Gamestate> list, int indexA, int indexB)
        {
            Gamestate tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;

        }

        //wechselt die Ladung zweier Rechtecke
        public void SwitchCharge(CustomRectangle first, CustomRectangle second)
        {
            int transfer_charge = first.charge;
            first.charge = second.charge;
            second.charge = transfer_charge;
        }

        //fügt Batterien aufgrund ihrer Position und Ladung der aktuellen Situation hinzu
        public void AddBatterie(int xKor, int yKor,int charge)
        {
            all_batteries.Add(new CustomRectangle(new Point(xKor, yKor),  charge, States.Batterie, rect_size));

            //passt zudem den Schriftzug an die aktuelle Position an und rendert den besagten Schriftzug
            foreach (CustomRectangle batterie in all_batteries)
            {
                if (batterie == null) return;
                Controls.Add(batterie.text);
                batterie.RealignText();
            }
        }

        //fügt einen Roboter aufgrund seiner Position und Ladung der aktuellen Situation hinzu
        public void AddRobot(int xKor, int yKor, int charge)
        {
            //falls es schon einen Roboter geben sollte, wird dieser gelöscht
            robot = null;
            robot = new CustomRectangle(new Point(xKor, yKor),  charge, States.Robot, rect_size);

            //passt den Schriftzug der aktuellen Position an und rendert diesen
            Controls.Add(robot.text);
            robot.RealignText();
        }

        //bewegt den roboter um einen Schritt in eine der vier Richtungen
        public void MoveDir(Movements dir)
        {
            //prüft ob es einen Roboter gibt
            if (robot != null)
            {
                //switch statement mit den vier verschiedenen Bewegungsoptionen
                switch (dir)
                {
                    case Movements.Up:
                        //falls er am Ende der Karte ist oder er keine Ladung mehr hat
                        if (robot.position.Y <= 0 || robot.charge == 0) return;
                        //verschiebt das gezeichnete Rechteck um genau ein Feld
                        robot.rect.Offset(0, -rect_size);
                        //passt die position des Roboters selber an
                        robot.position.Y--;
                        AdjustParams();
                        break;
                    //selbes Vorgehen wie oben nur mit anderen Vorzeichen    
                    case Movements.Down:
                        if (robot.position.Y >= field_size - 1 || robot.charge == 0) return;
                        robot.rect.Offset(0, rect_size);
                        robot.position.Y++;
                        AdjustParams();
                        break;
                    //selbes Vorgehen wie oben nur mit anderer Achse und Vorzeichen
                    case Movements.Right:
                        if (robot.position.X >= field_size - 1 || robot.charge == 0) return;
                        robot.rect.Offset(rect_size, 0);
                        robot.position.X++;
                        AdjustParams();
                        break;
                    //selbes Vorgehen wie oben nur mit anderer Achse   
                    case Movements.Left:
                        if (robot.position.X <= 0 || robot.charge == 0) return;
                        robot.rect.Offset(-rect_size, 0);
                        robot.position.X--;
                        AdjustParams();
                        break;
                    //falls keiner dieser Fälle eintritt, bricht man ab
                    default: return;
                }
            }
        }
        
        //liest Tastatur inputs
        private void Map_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    MoveDir(Movements.Left);
                    break;
                case Keys.S:
                    MoveDir(Movements.Down);
                    break;
                case Keys.D:
                    MoveDir(Movements.Right);
                    break;
                case Keys.W:
                    MoveDir(Movements.Up);
                    break;
                case Keys.Left:
                    MoveDir(Movements.Left);
                    break;
                case Keys.Down:
                    MoveDir(Movements.Down);
                    break;
                case Keys.Right:
                    MoveDir(Movements.Right);
                    break;
                case Keys.Up:
                    MoveDir(Movements.Up);
                    break;
                default: return;
            }
        }

        //passt Grundparameter an die im obigen switch alle gleich angepasst werden müssen
        private void AdjustParams()
        {
            //reduziert die roboter Ladung um eins
            robot.charge--;
            //passt das Textelement auf die zurzeitige position an
            robot.RealignText();
            //überprüft ob er auf einer Batterie steht
            CheckForBatterie();
            //malt das Feld neu um die obigen Änderungen zu übernehmen
            Invalidate();
        }

        //überprüft ob der Roboter auf einer Batterie steht
        private void CheckForBatterie()
        {
            foreach (CustomRectangle batterie in all_batteries)
            {
                //falls die position dieser Batterie mit der des Roboters übereinstimmt, wechselt man diese
                if (batterie.position == robot.position)
                {
                    //temporäre Zahl um eine der beiden Ladungen zu speichern
                    int transfer_charge = batterie.charge;
                    batterie.charge = robot.charge;
                    robot.charge = transfer_charge;
                    //übernimmt die Zahlenänderungen auf dem Zeichenelement
                    batterie.RealignText();
                }
            }
        }
        
        //malt die gegebenen Rechtecke in ein neues Fenster
        private void Form1_Paint_1(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //zuerst alle Rechtecke, auf denen es keine Batterie oder Roboter gibt
            foreach (CustomRectangle rectangle in all_rects)
            {
                Pen pen = new Pen(rectangle.color, 3f);
                g.DrawRectangle(pen, rectangle.rect);
                pen.Dispose();
            }

            //im Anschluss werden alle Batterien gemalt
            foreach (CustomRectangle batterie in all_batteries)
            {
                Pen pen = new Pen(batterie.color, 3f);
                g.DrawRectangle(pen, batterie.rect);
                pen.Dispose();
            }

            //falls es einen Roboter gibt, malt man diesen zum Schluss
            if(robot != null)
            {
                g.DrawRectangle(new Pen(robot.color, 3f), robot.rect);
            }
        }

        //beendet das Programm, wenn ein Fenster geschlossen wird
        private void Matchfieldpage_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(1);
        }
       
    }
}
