using System;
using System.Collections.Generic;
using System.Drawing;

namespace Stromrallye
{
    public class MapGenerator
    {
        //Anpassbare Parameter
        int[] map_size_range = new int[] { 5, 15};
        int[] batterie_charge_range = new int[] { 1, 6 };

        //Größe des Spielfeldes
        int map_size;
        Map map;

        //Anfangsladung des Roboters
        int robot_charge;

        //Anzahl aller benötigten und tatsächlichen batterien
        int batterie_amount;
        public List<StateVector> all_batteries = new List<StateVector>();

        //Roboter und dessen Startposition
        public StateVector robot;
        public Point startpos;
        
        //Liste an besuchten Positionen
        List<Point> visited = new List<Point>();

        //Zufallszahl
        Random rnd = new Random();
        
        //setzt die Anfangsparameter des Spielbretts
        public MapGenerator()
        {
            //zufällige Spielbrettgröße
            map_size = rnd.Next(map_size_range[0], map_size_range[1]);
            map = new Map(map_size);

            //zufällige Robobterladung
            robot_charge = rnd.Next(map_size_range[0], map_size_range[1]);

            //Anzahl der zu platzierenden Batterien
            batterie_amount = map_size * map_size / 4;

            //Anfangsposition des Robobters
            startpos = new Point(map_size / 2, map_size / 2);
            robot = new StateVector(startpos, robot_charge);
            map.AddRobot(robot.position.X, robot.position.Y, robot.charge);
            visited.Add(robot.position);

            GenerateBatteries();
        }
        
        //geht zufällige Schritte und plaziert Batterien, bis alle Batterien platziert wurden
        void GenerateBatteries()
        {
            //macht do viele Bewegungen bis alle Batterien platziert wurden
            while(all_batteries.Count < batterie_amount)
            {
                MoveDir((Movements)rnd.Next(-2, 3));
              
                //falls der Roboter keine Ladung hat und man auf dieser Position noch nicht war, platziert man eine Batterie
                if (robot.charge == 0)
                {
                    if(!visited.Contains(robot.position))
                    {
                        int next_charge = rnd.Next(batterie_charge_range[0], batterie_charge_range[1]);
                        robot.charge = next_charge;
                        all_batteries.Add(new StateVector(robot.position, next_charge));
                    }
                    else
                    {
                        robot.charge++;
                        if(all_batteries.Count != 0) all_batteries[all_batteries.Count - 1].charge++;
                    }
                }
                visited.Add(robot.position);
            }

            //erstellt ein Spielbrett mit den gegebenen Batterien
            all_batteries.ForEach(i => map.AddBatterie(i.position.X, i.position.Y, i.charge));
            map.start_state = new Gamestate(map.robot, map.all_batteries, new List<int>());
            map.CreateMatchfield();
            map.GetDistances();

            //zeigt das besagte Spielbrett an
            map.start_state.ShowState();
        }

        //bewegt den Roboter in eine Richtung und passt seine Ladung an
        private void MoveDir(Movements dir)
        {
            //nur falls es zurzeit einen Roboter gibt
            if (robot != null)
            {
                switch (dir)
                {
                    //Anpassen von Position und Ladung
                    case Movements.Up:
                        if (robot.position.Y <= 0 || robot.charge == 0) return;
                        robot.position.Y--;
                        robot.charge--;
                        break;
                    case Movements.Down:
                        if (robot.position.Y >= map_size - 1 || robot.charge == 0) return;
                        robot.position.Y++;
                        robot.charge--;
                        break;
                    case Movements.Right:
                        if (robot.position.X >= map_size - 1 || robot.charge == 0) return;
                        robot.position.X++;
                        robot.charge--;
                        break;
                    case Movements.Left:
                        if (robot.position.X <= 0 || robot.charge == 0) return;
                        robot.position.X--;
                        robot.charge--;
                        break;
                    default: return;
                }
            }
        }
     
    }


}
