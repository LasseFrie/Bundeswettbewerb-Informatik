using System.Collections.Generic;

namespace Stromrallye
{
    public class Gamestate
    {
        //Werte die gespeichert werden sollen (die eine Situation ausmachen)
        public StateVector robot;
        public List<StateVector> batteries = new List<StateVector>();
        public List<int> moves = new List<int>();
        public Map map;

        public Gamestate(CustomRectangle bot,  List<CustomRectangle> bats, List<int> ms)
        {
            //bildet einen Abdruck der zurzeitigen Situation
            robot = new StateVector(bot.position, bot.charge);
            ms.ForEach(i => moves.Add(i));

            for (int i = 0; i < bats.Count; i++)
            {
                batteries.Add(new StateVector(bats[i].position, bats[i].charge));
            

            //kopiert die Nachbarn (Kanten) für jede Batterie
            for (int i = 0; i < batteries.Count; i++)
            {
                for (int j = 0; j < bats[i].neighbours.Count; j++)
                {
                    for (int h = 0; h < batteries.Count; h++)
                    {
                        if(bats[i].neighbours[j].rect.position == batteries[h].position && batteries[h] != batteries[i] && batteries[i].charge >= bats[i].neighbours[j].distance)
                        {
                            batteries[i].neighbours.Add(batteries[h]);
                        }
                    }
                }
            }

            //kopiert alle Nachbarn des Roboters 
            for (int i = 0; i < bot.neighbours.Count; i++)
            {
                for (int j = 0; j < batteries.Count; j++)
                {
                    if(bot.neighbours[i].rect.position == batteries[j].position && robot.charge >= bot.neighbours[i].distance)
                    {
                        robot.neighbours.Add(batteries[j]);
                    }
                }
            }
        }
       
        //gibt die insgesamte Batterieladung der Situation zurück
        public int BatterieCharge()
        {
            int charge = 0;
            batteries.ForEach(i => charge += i.charge);
            return charge;
        }

        //gibt eine Situation (Graphen) als Adjazenzmatrix wieder
        public bool[,] TransformToMatrix()
        {
            List<StateVector> current_batteries = new List<StateVector>();

            //lässt nur Batterien, die nicht leer sind zu
            foreach (StateVector batterie in batteries)
            {
                if (batterie.charge > 0)
                {
                    current_batteries.Add(batterie);
                }
            }

            //bildet die tatsächliche Matrix
            bool[,] return_matrix = new bool[current_batteries.Count, current_batteries.Count];

            for (int i = 0; i < return_matrix.GetLength(0); i++)
            {
                for (int j = 0; j < return_matrix.GetLength(1); j++)
                {
                    return_matrix[i, j] = false;
                    if (i != j)
                    {
                        foreach (StateVector neighbour in current_batteries[i].neighbours)
                        {
                            if (neighbour.position == current_batteries[j].position)
                            {
                                 return_matrix[i, j] = true;
                            }
                        }
                    }
                }
            }

            return return_matrix;
        }

        //zeigt eine Situation als Grafisches Element an indem man eine neue Map erschafft
        public Map ShowState()
        {
            int size = Map.field_size;
            map = new Map(size);

            foreach (StateVector batterie in batteries)
            {
               map.AddBatterie(batterie.position.X, batterie.position.Y, batterie.charge);
            }

            map.CreateMatchfield();
            map.Show();
            map.AddRobot(robot.position.X, robot.position.Y, robot.charge);

            return map;
        }
    }
}
