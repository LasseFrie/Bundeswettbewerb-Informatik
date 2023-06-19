using System.Drawing;

namespace Stromrallye
{
    //Wird zur Breitensuche zwischen den Batterien benötigt
    public class QueueNode
    {
        // Koordinaten der Zelle
        public Point pt;

        // Entfernung zum Anfang
        public int dist;

        //Vorgänger der Zelle
        public QueueNode parent;
        public QueueNode(Point pt, int dist, QueueNode p)
        {
            this.parent = p;
            this.pt = pt;
            this.dist = dist;
        }
    }

}
