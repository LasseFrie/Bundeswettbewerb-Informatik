using System.Drawing;
using System.Collections.Generic;

namespace Stromrallye
{
    //abgespeckte Version der CustomRectangle Klasse 
    //Rendert kein Display Element und ist somit besser zum Speichern geeignet
    public class StateVector
    {
        public Point position;
        public int charge;
        public List<StateVector> neighbours = new List<StateVector>();

        public StateVector(Point p, int c)
        {
            position = p;
            charge = c;
        }
    }
}
