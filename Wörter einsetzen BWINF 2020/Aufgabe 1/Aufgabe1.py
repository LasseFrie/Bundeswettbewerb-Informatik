import sys
import operator
from itertools import count
alles klar bei dieser Tastatur, ich hoffe schon sonst wäre das sehr blöd wenn sie nicht richtibg funktioniert, scheint aber alles zu passen
#Einstiegspunkt des Programms
def main():
    path = sys.argv[1]
    ProcessInput(path)

#wandelt den Input in eine Lücken Liste und eine Wörter Liste um   
def ProcessInput(path):

    #UTF-8 wird benutzt um Umlaute anzeigen zu können
    file = open(path, "r", 1, encoding="utf-8")

    if file.mode == "r":
        contents = file.readlines()

        available_gaps = contents[0].split()        
        words = contents[1].split()

        gaps = []

        #speichert, ob die Lücke ein Satzzeichen hat und wenn ja, dann welches
        for gap in available_gaps:
            if gap[len(gap) - 1] is ".":   
                gaps.append(Gap(gap, True, '.'))
            elif gap[len(gap) - 1] is "!":
                gaps.append(Gap(gap, True, '!'))
            elif gap[len(gap) - 1] is "?":
                gaps.append(Gap(gap, True, '?'))
            elif gap[len(gap) - 1] is ",":
                gaps.append(Gap(gap, True, ','))
            else:
                gaps.append(Gap(gap, False, ''))

    gaps = sorted(gaps, key=operator.attrgetter('length','has_letter'))

    evaluated_gaps = 0

    #Schleife die jede Lücke dazu aufruft zu überprüfen, ob sie mit den zurzeitigen Wörter
    #ein eindeutiges Wort herausfiltern können
    while evaluated_gaps is not len(gaps):

        evaluated_gaps = 0

        #entfernt alle Wörter die schon sicher vergeben sind aus der Wörter Liste
        for gap in gaps:
            evaluated_word = gap._evaluate_(words)
            if evaluated_word is not None:
                words.remove(evaluated_word)
        
        for gap in gaps:
            if gap.has_finalword:
                evaluated_gaps += 1

        print("Gaps filled: ", evaluated_gaps, " / ", len(gaps))
  
    #Ausgabe, wenn das Programm eine Lösung gefunden hat
    result = sorted(gaps, key=operator.attrgetter('position'))

    print()
    print("---Found Solution!---")
    print()

    for r in result:
        print(r.finalword, end = r.punctuation + ' ')

    print()

#eine Lückenklasse die für jede zu besetzende Lücke erschaffen wird   
class Gap():

    counter = count(0)

    def __init__(self, word, has_punctuation, punctuation):

        self.finalword = word
        self.length = len(self.finalword)
        self.position = next(self.counter)

        self.has_finalword = False

        self.letter = None
        self.letterIndex = None
        
        self.possible_words = []

        #hat die Lücke schon einen Buchstaben, wenn ja wo?
        for num, letter in enumerate(word):
            if letter is not "_" and letter is not "." and letter is not "!" and letter is not "?" and letter is not ",":
               self.letter = letter
               self.letterIndex = num

        if self.letter is not None:
            self.has_letter = True
        else :
            self.has_letter = False

        #hat die Lücke ein Satzzeichen, wenn ja welches?
        self.has_punctuation = has_punctuation
        self.punctuation = punctuation

        if self.has_punctuation:
            self.length -= 1
    
    #Methode die potentielle Wörter für eine Lücke durchsucht
    def _evaluate_(self, current_allwords):

        #wenn die Lücke schon besetzt ist
        if self.has_finalword:
            return None

        #Pool an möglichen Wörtern
        self.possible_words = []

        #Wenn es ein Wort gibt mit einem Buchstaben an der selben Stelle wie die Lücke
        #fügt man es, wenn es auch die selbe Länge hat zu einem Pool hinzu
        for word in current_allwords:
            if self.length is len(word):
               if self.letter is not None:
                   if self.letter is word[self.letterIndex]:
                       self.possible_words.append(word)
               else:
                   self.possible_words.append(word)

        #Wenn es nur ein mögliches Wort gibt, ist dass, das finale Wort für diese Lücke
        if len(self.possible_words) is 1:
           self.finalword = self.possible_words[0]
           self.has_finalword = True
           return self.finalword

        self.counter = 1
        self.duplicate = ""

        #überprüft ob es duplikate gibt, die sich an mehreren Stellen einsetzen liesen
        self.dupes = [x for n, x in enumerate(self.possible_words) if x in self.possible_words[:n]]
        if len(self.dupes) is not 0:
            self.duplicate = self.dupes[0]

        for pos in self.possible_words:
            if pos is self.duplicate:
                self.counter += 1
        
        #Wenn der Pool nurnoch Duplikate eines Wortes enthält und es sonst keine möglichen Wörter gibt
        #gibt man der Lücke dieses Duplikat als finales Wort
        if len(self.dupes) is 1 and self.counter is len(self.possible_words):
           self.finalword = self.dupes[0]
           self.has_finalword = True
           return self.finalword

        return None
            
main()
