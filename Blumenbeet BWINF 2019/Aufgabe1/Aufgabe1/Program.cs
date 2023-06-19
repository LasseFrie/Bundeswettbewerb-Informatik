//Modulimport
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aufgabe1
{
    static class Program
    {
        #region Utility und Eingabe Parameter
        //Eingabe Parameter
        public static int farben_anzahl = 0;
        public static int kombinationen_anzahl = 0;

        //Blumenbeet Parameter
        static int nknoten = 9;
        static int nkanten = 16;

        //Utility Parameter
        static int count = 0;
        static int maximaleBewertung = 0;
        static int max_kombinations_bewertung = 0;
        static bool has_changed = false;
        static string max_farbe = null;
        static string[] max_kombination = new string[2];

        //Speicher für die Kombinations Eingaben
        static string[] inputs;
        public static string[,] kombinationen;

        //Feld mit allen möglichen Verbindungen zwischen allen Knoten
        static int[,] kanten_nachbarn = { { 0, 1 }, { 0, 2 }, { 1, 2 }, { 1, 3 }, { 1, 4 }, { 2, 4 }, { 2, 5 }, { 3, 4 }, { 4, 5 }, { 3, 6 }, { 4, 6 }, { 4, 7 }, { 5, 7 }, { 6, 7 }, { 6, 8 }, { 7, 8 } };
        #endregion
        //Alle möglichen Farben
        static List<string> alle_farben = new List<string>() { "gruen", "gelb", "rot", "blau", "orange", "tuerkis", "rosa" };
        //Die später gestellte Farbenliste
        static List<string> farbenliste = new List<string>();

        //Liste aller Knoten (Blumentöpfe)
        public static List<KNOTEN> knotenpunkte = new List<KNOTEN>();
        static List<KNOTEN> zubesetzen = new List<KNOTEN>();

        //Liste aller Kanten (Verbindung zweier Blumentöpfe)
        static List<KANTE> kanten = new List<KANTE>();

        //Aufzählung der Farben aus dem Input mit ihrem Farbenwert
        static Dictionary<string, int> farbenwerte = new Dictionary<string, int>();

        //Start
        static void Main(string[] args)
        {
            GetIntInput();
        }

        //nimmt den Input der Gegebenen Zahlen
        #region UserInputs
        private static void GetIntInput()
        {
            //Bedingungen zur Überprüfung des Wertes "input_farben"
            Console.WriteLine("Anzahl der Farben: ");
            string input_farben = Console.ReadLine();
            if (Int32.TryParse(input_farben, out farben_anzahl) && farben_anzahl >= 0 && farben_anzahl <= 7)
            {
                Console.WriteLine("Farben: " + farben_anzahl);
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Anzahl der Kombinationen: ");
                string input_kombinationen = Console.ReadLine();
                if (Int32.TryParse(input_kombinationen, out kombinationen_anzahl) && kombinationen_anzahl >= 0 && kombinationen_anzahl <= 7)
                {
                    Console.WriteLine("Kombinationen: " + kombinationen_anzahl);
                    GetStringInput();
                }
                else
                {
                    Console.WriteLine("Bitte gebe eine Zahl an die zwischen 0 und 7 liegt.");
                    GetIntInput();
                }
            }
            else
            {
                Console.WriteLine("Bitte gebe eine Zahl an die zwischen 0 und 7 liegt.");
                GetIntInput();
            }
            Console.ReadKey();
        }

        //nimmt den Input der einzelnen kombinationen
        private static void GetStringInput()
        {
            inputs = new string[kombinationen_anzahl];

            for (int i = 0; i < kombinationen_anzahl; i++)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Vorliebe Nummer " + i + ": ");
                inputs[i] = Console.ReadLine();
            }
            InputAufteilen();
        }
        #endregion

        //zerteilt den gegebenen Input und übergibt diesen den kombinationen
        private static void InputAufteilen()
        {

            kombinationen = new string[kombinationen_anzahl, 4];

            for (int i = 0; i < inputs.Length; i++)
            {
                int wichtigkeit = 0;

                //speichert die Zahl der Kombination und löscht diese im Original
                string[] reg_nummern = Regex.Split(inputs[i], @"\D+");
                foreach (string value in reg_nummern)
                {

                    if (!string.IsNullOrEmpty(value))
                    {
                        wichtigkeit = int.Parse(value);
                        inputs[i] = inputs[i].Trim(value[0]);
                    }
                }

                //Aufteilung der zwei Farben in einer Kombination
                string farbe1 = inputs[i].Substring(0, inputs[i].IndexOf(' ')).Trim(' ');
                string farbe2 = inputs[i].Substring(farbe1.Length, inputs[i].Length - farbe1.Length).Trim(' ');

                //speichern der aufgeteilten Kombination im 2D-Feld
                kombinationen[i, 0] = farbe1;
                kombinationen[i, 1] = farbe2;
                kombinationen[i, 2] = wichtigkeit.ToString();

                FarbenWerteBestimmen(farbe1, farbe2, wichtigkeit);
            }
            KnotenKantenErstellen();

        }

        //addiert die Wertigkeiten für jede Farbe aus allen kombinationen um eine "beste" Farbe zu bestimmen
        private static void FarbenWerteBestimmen(string farbe1, string farbe2, int wichtigkeit)
        {
            if (farbenwerte.ContainsKey(farbe1))
            {
                farbenwerte[farbe1] += wichtigkeit;
            }
            else
            {
                farbenwerte.Add(farbe1, wichtigkeit);
            }
            if (farbenwerte.ContainsKey(farbe2))
            {
                farbenwerte[farbe2] += wichtigkeit;
            }
            else
            {
                farbenwerte.Add(farbe2, wichtigkeit);
            }
        }

        //erstellt alle Knoten und Kanten und gibt die nötigen Nachbarschafts Verweise
        private static void KnotenKantenErstellen()
        {
            for (int i = 0; i < nknoten; i++)
            {
                knotenpunkte.Add(new KNOTEN());
            }
            for (int i = 0; i < nkanten; i++)
            {
                kanten.Add(new KANTE(kombinationen));
                kanten[i].KnotenSetzen(kanten_nachbarn[i, 0], kanten_nachbarn[i, 1]);
            }
            
            FarbenKominationsWerteBestimmen();
        }
       
        private static void FarbenKominationsWerteBestimmen()
        {
            //addiert immer zwei Farbwerte aus einer Kombination um dann einen Kombinationswert zu bestimmen
            for (int i = 0; i < kombinationen_anzahl; i++)
            {
                int kombinations_bewertung = farbenwerte[kombinationen[i, 0]] + farbenwerte[kombinationen[i, 1]];
                kombinationen[i, 3] = kombinations_bewertung.ToString();
                if (kombinations_bewertung > max_kombinations_bewertung) max_kombinations_bewertung = kombinations_bewertung;
            }
            FarbenListeStellen();
        }

        private static void FarbenListeStellen()
        {
            //Entfernt eine Farbe falls es weniger Farben geben soll als in den kombinationen angegeben
            for (int e = farben_anzahl; e < farbenwerte.Keys.Count; e++)
            {
                int min_farbe_wert = 9999;
                string min_farbe = null;
                foreach (string f in farbenwerte.Keys)
                {
                    if (farbenwerte[f] < min_farbe_wert)
                    {
                        min_farbe_wert = farbenwerte[f];
                        min_farbe = f;
                    }
                }
                farbenwerte.Remove(min_farbe);
            }
            //fügt die farben der Kombinationen hinzu
            foreach (string k in farbenwerte.Keys)
            {
                for (int i = 0; i < alle_farben.Count; i++)
                {
                    if (alle_farben[i] == k)
                    {
                        alle_farben.Remove(k);
                    }
                }
                farbenliste.Add(k);
            }
            //fügt andere Farben hinzu, wenn die farben anzahl noch nicht erreicht wurde
            for (int j = 0; j < farben_anzahl - farbenliste.Count; j++)
            {
                int r = j;
                farbenliste.Add(alle_farben[r]);
                j--;
                alle_farben.Remove(alle_farben[r]);
            }
                    
            //speichert die beste Farbe aus farbenwerte
            max_farbe = farbenwerte.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

            //fügt die besten Farben so lange hinzu bis alle Blumentöpfe eine Farbe haben
            while (farbenliste.Count < knotenpunkte.Count)
            {
                //Wenn mehr als 2 Farben zu füllen sind, immer die beste Kombination nochmal hinzufügen
                if (knotenpunkte.Count - farbenliste.Count >= 2)
                {
                    for (int b = 0; b < kombinationen.GetLength(0); b++)
                    {
                        if (int.Parse(kombinationen[b, 3]) == max_kombinations_bewertung && farbenliste.Count < knotenpunkte.Count)
                        {
                                farbenliste.Add(kombinationen[b, 0]);
                                farbenliste.Add(kombinationen[b, 1]);
                                max_kombination[0] = kombinationen[b, 0];
                                max_kombination[1] = kombinationen[b, 1];
                        }
                    }
                }
                //sonst nur die beste Farbe hinzufügen
                else
                {
                    int max_farbe_wert = 0;
                    foreach (string f in farbenwerte.Keys)
                    {
                        if (farbenwerte[f] > max_farbe_wert)
                        {
                            max_farbe_wert = farbenwerte[f];
                            max_farbe = f;
                        }
                    }
                    farbenliste.Add(max_farbe);
                }
            }
            ListenVorbereiten();
        }
        //bereitet die Listen zum Permutieren vor
        private static void ListenVorbereiten()
        {
            //entfernt die beste Kombination doppelt da dessen position bekannt ist
            for (int i = 0; i < 2; i++)
            {
                farbenliste.Remove(max_kombination[0]);
                farbenliste.Remove(max_kombination[1]);
            }

            //weißt bekannte Farben Knoten zu
            BekannteKnotenZuweisen();

            //erstellt eine Liste mit allen noch zu besetzenden Knoten (isLocked = ob dieser knoten noch zu besetzen ist)
            
            foreach (KNOTEN kNOTEN in knotenpunkte)
            {
                if (!kNOTEN.isLocked)
                {
                    zubesetzen.Add(kNOTEN);
                }
            }

            BesteKombinationErrechnen();
        }
        //Grundgerüst von Stackoverflow über Permutation
        private static void BesteKombinationErrechnen()
        {
            
            //Alle Permutationen von der gestellten Farbenliste durchprobieren
            List<string> zurzeitige_list = new List<string>();
            List<string> perm_list = farbenliste;
            IList<IList<string>> perms = Permutationen(perm_list);
            foreach (IList<string> perm in perms)
            {
                foreach (string s in perm)
                {
                    zurzeitige_list.Add(s);
                }
                for (int i = 0; i < zubesetzen.Count; i++)
                {
                    zubesetzen[i].farbe = zurzeitige_list[i];
                }                               
                zurzeitige_list.Clear();
                count++;
                //Wenn es eine neue beste Möglichkeit gibt, diese Ausgeben
                if (BewertungBestimmen() > maximaleBewertung)
                {
                    maximaleBewertung = BewertungBestimmen();
                    Ausgabe();
                    Console.WriteLine("Bewertung: " + maximaleBewertung + "  " + "Counter: " + count);
                }
            }
            //probiert die Farben der besten Kombination umgekehrt
            if(! has_changed)
            {
                for (int j = 0; j < max_kombination.Length; j++)
                {
                    if (max_kombination[j] != max_farbe)
                    {
                        knotenpunkte[4].farbe = max_kombination[j];
                        knotenpunkte[0].farbe = max_kombination[j];
                    }
                }
                knotenpunkte[1].farbe = max_farbe;
                knotenpunkte[2].farbe = max_farbe;

                has_changed = true;

                BesteKombinationErrechnen();
                Console.Write("Drücke Enter zum schließen.....");
            }
        }
        //ändert die Position der einzelnen Elemente innerhalb einer Liste
        private static IList<IList<T>> Permutationen<T>(IList<T> list)
        {
            List<IList<T>> perms = new List<IList<T>>();
            if (list.Count == 0)
                return perms;
            int factorial = 1;
            for (int i = 2; i <= list.Count; i++)
                factorial *= i;
            for (int v = 0; v < factorial; v++)
            {
                List<T> s = new List<T>(list);
                int k = v;
                for (int j = 2; j <= list.Count; j++)
                {
                    int other = (k % j);
                    T temp = s[j - 1];
                    s[j - 1] = s[other];
                    s[other] = temp;
                    k = k / j;
                }
                perms.Add(s);
            }
            return perms;
        }

        //weißt bekannten Knoten ihre Farbe zu 
        private static void BekannteKnotenZuweisen()
        {
            knotenpunkte[4].farbe = max_farbe;
            knotenpunkte[4].isLocked = true;

            knotenpunkte[0].farbe = max_farbe;
            knotenpunkte[0].isLocked = true;

            for (int i = 1; i < 3; i++)
            {
                knotenpunkte[i].isLocked = true;
                for (int j = 0; j < max_kombination.Length; j++)
                {
                    if(max_kombination[j] != max_farbe || farben_anzahl == 1)
                    {
                        knotenpunkte[i].farbe = max_kombination[j];
                    }
                }
            } 
        }

        //addiert das Gewicht aller Kanten und gibt diese zurück
        private static int BewertungBestimmen()
        {
            int score = 0;
            foreach (KANTE k in kanten)
            {
                k.gewicht = 0;
                k.GewichtBestimmen();
                score += k.gewicht;
            }
            return score;
        }

        //gibt alle Farben der zurzeitigen Knoten zurück
        private static void Ausgabe()
        {
            Console.WriteLine("--------------------------------");
            string field = @"
                   {0}
                {1}   {2}
             {3}   {4}   {5}
                {6}   {7}
                   {8}";
            string[] farben = new string[9];
            for (int i = 0; i < knotenpunkte.Count; i++)
            {
                farben[i] = knotenpunkte[i].farbe;
            }
            Console.WriteLine(string.Format(field, farben));
        }
    }
}

