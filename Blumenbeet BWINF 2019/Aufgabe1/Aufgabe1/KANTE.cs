
namespace Aufgabe1
{
    //befindet sich immer zwischen zwei Knoten um mögliche kombinationen zu finden
    class KANTE
    {
        public int gewicht { get; set; }

        private KNOTEN[] knoten = new KNOTEN[2];

        public static string[,] kombinationen;
        public KANTE(string[,] newkombinationen)
        {
            kombinationen = newkombinationen;
        }
        public void KnotenSetzen(int kn1, int kn2)
        {
            knoten[0] = Program.knotenpunkte[kn1];
            knoten[1] = Program.knotenpunkte[kn2];
        }

        public void GewichtBestimmen()
        {
            if (knoten[0] != null && knoten[1] != null && kombinationen != null)
            {
                for (int i = 0; i < kombinationen.GetLength(0); i++)
                {
                    if (kombinationen[i, 0] == knoten[0].farbe && kombinationen[i, 1] == knoten[1].farbe)
                    {
                        gewicht += int.Parse(kombinationen[i, 2]);
                    }
                    else if (kombinationen[i, 0] == knoten[1].farbe && kombinationen[i, 1] == knoten[0].farbe)
                    {
                        gewicht += int.Parse(kombinationen[i, 2]);
                    }
                }
            }
        }
    }
}

