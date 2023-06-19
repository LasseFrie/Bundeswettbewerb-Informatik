using System.Collections.Generic;

namespace Stromrallye
{
    //Klasse die eine Kante in einem Graphen wiederspiegelt
    //beinhaltet die Entfernung, Bewegung und Referenz zu einem anderen Knoten
    public class Neighbour
    {
        public CustomRectangle rect;
        public int distance;
        public List<int> moves = new List<int>();

        public Neighbour(CustomRectangle rect, int distance, List<int> m)
        {
            m.ForEach(i => moves.Add(i));
            this.rect = rect;
            this.distance = distance;
        }
    }
}
