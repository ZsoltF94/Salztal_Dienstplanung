# Fragen und Verständnisabgleich: S09B Planungs- und Generierungsbericht

Status: Vollständig beantwortet, konsolidiert und gemeinsam mit der angepassten Roadmap am 2026-09-18 ausdrücklich abgenommen

Stand: 2026-09-18

## Zweck und Grenze

Dieser Fragenkatalog klärt den S09B-Planungsqualitätsbericht. Er soll den fachlich unveränderten Generator so sichtbar machen, dass die Service-Leitung den erzeugten Plan pro Woche beurteilen und die Wirkung der einzelnen Generierungsphasen nachvollziehen kann.

Der Bericht besteht nach dem bisherigen Auftrag aus zwei klar getrennten Teilen:

1. **Planungsbericht:** Bedarfe und Deckung, Wochenarbeitszeiten je Person sowie Anzahl der Dienste je Person und Woche.
2. **Generierungsbericht:** neutrale, strukturierte Bilanz der nacheinander ausgeführten Planungsphasen und ihrer erreichten Werte.

Die derzeit größten bekannten Qualitätsbaustellen sind eine noch nicht ausreichend faire Verteilung von `F` und `S` sowie eine noch nicht ausreichende Minimierung von `D`. S09B soll diese Beobachtungen messbar machen, den Generator aber noch nicht optimieren.

Dieser Katalog und die daran angepasste Roadmap wurden am 2026-09-18 ausdrücklich als Implementierungsgrundlage abgenommen. Solver, Zielmatrix, Regelkatalog und Generierungsregeln bleiben während S09B fachlich unverändert. Jede aus dem Bericht abgeleitete Optimierung erhält danach einen eigenen kleinen Folgeplan.

S09B erklärt keine personenbezogenen Ausschlussursachen und macht keine Lösungsvorschläge. Diese vollständige Konflikterklärung bleibt System 10. Alle Repository-Beispiele und Tests verwenden ausschließlich synthetische Daten.

## Gelesene und berücksichtigte Grundlagen

- `AGENTS.md`
- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`
- `ARCHITECTURE.md`
- `CLEANCODE.md`
- `MASTER_ROADMAP.md`
- `STATUS.md`
- `Service-Leitung/AKTUELLER_STAND.md`
- `docs/roadmaps/active/S09_AUTOMATIC_SCHEDULE_GENERATION_QUESTIONS.md`
- `docs/roadmaps/active/S09_AUTOMATIC_SCHEDULE_GENERATION_ROADMAP.md`
- `docs/roadmaps/completed/S09A_SCHEDULE_UI_RESTRUCTURING_ROADMAP.md`
- aktuelle Application-, Planning- und Desktop-Verträge für Vorschlag, Laufmetadaten, Zielvektor, Bedarfe, Zuweisungen und Wochenziele

## Bereits bestätigte Mindestinhalte

- Alle wirksamen Bedarfe und ihre Deckung werden pro Woche gezeigt.
- Jede Person erhält pro Woche eine gut lesbare Gegenüberstellung von Soll- und tatsächlicher geplanter Arbeitszeit.
- Für jede Person wird pro Woche gezeigt, welchen Dienst sie wie oft ausührt.
- Ein eigener Generierungsbericht beschreibt die Entscheidungen beziehungsweise erreichten Ergebnisse der einzelnen Planungsphasen so nachvollziehbar und übersichtlich wie seriös möglich.
- Die Darstellung muss insbesondere die Beurteilung der `F`-/`S`-Verteilung und der `D`-Minimierung unterstützen.

## A – Gegenstand, Zeitpunkt und Reichweite

### A-01 – Zwei Berichtsteile

Sollen Planungsbericht und Generierungsbericht als zwei klar benannte Bereiche derselben S09B-Ansicht erscheinen?

**Empfehlung:** Ja. Beide gehören zum selben Lauf, beantworten aber unterschiedliche Fragen. Eine gemeinsame Ansicht mit zwei Bereichen oder Reitern verhindert, dass fachliche Planwerte und technische Phasenwerte vermischt werden.

Antwort: Ja. Planungsbericht und Generierungsbericht erscheinen als getrennte Bereiche eines eigenen Berichtsfensters. Das Berichtsfenster ist nicht modal und kann neben dem Hauptfenster geöffnet bleiben.

### A-02 – Vorschau oder übernommener Plan

Auf welchen Stand bezieht sich der Bericht?

**Empfehlung:** Der Bericht ist bereits für die flüchtige Generierungsvorschau erreichbar. Nach der Übernahme bleibt derselbe Bericht beim aktuellen automatischen Ergebnis erreichbar. Spätere manuelle Änderungen erzeugen keinen angeblich unveränderten Generierungsbericht, sondern werden klar vom ursprünglichen Laufstand getrennt.

Antwort: deine empfehlung

### A-03 – Berichtszeitraum

Soll der Bericht immer den gesamten Drei-Wochen-Zeitraum umfassen und innerhalb dessen nach Kalenderwochen gegliedert sein?

**Empfehlung:** Ja. Oben steht eine kompakte Gesamtsicht; darunter werden Woche 1, 2 und 3 jeweils vollständig gezeigt.

Antwort: deine empfehlung

### A-04 – Bericht ohne erfolgreichen Vorschlag

Soll bei blockierten, abgebrochenen oder technisch fehlgeschlagenen Läufen ein Qualitätsbericht erscheinen?

**Empfehlung:** Jeder Generierungsversuch erhält einen Generierungsbericht. Ohne zulässigen Vorschlag entfällt nur der Planungsbericht. Der Generierungsbericht zeigt dann Abbruch- oder Fehlerstatus, abgeschlossene Phasen, die zuletzt erreichte Phase, Laufzeiten und vorhandene strukturierte Fehlerdetails, erfindet aber keine fehlenden Planwerte.

Antwort: Ja. Jeder Generierungsversuch erhält einen Generierungsbericht. Wenn kein zulässiger Vorschlag entstanden ist, entfällt nur der Planungsbericht. Stattdessen zeigt der Generierungsbericht den Abbruch- oder Fehlerstatus, alle bereits abgeschlossenen Phasen, die zuletzt erreichte Phase, Laufzeiten und sämtliche vorhandenen strukturierten Fehlerdetails. Er erfindet keine Plan- oder Qualitätswerte für nicht erzeugte Ergebnisse.

### A-05 – Historie und Vergleich

Müssen mehrere frühere Berichte gespeichert und nebeneinander verglichen werden?

**Empfehlung:** Noch nicht. S09B zeigt die aktuelle Vorschau beziehungsweise den dazu gehörenden übernommenen Lauf. Eine dauerhafte personenbezogene Berichtshistorie oder ein Export wird nicht vor System 12/13 eingeführt. Verbesserungen werden zunächst mit versionierten synthetischen Vergleichsfällen nachgewiesen.

Antwort: deine empfehlung

## B – Bedarfe und Deckung pro Woche

### B-01 – Vollständige Bedarfsliste

Soll jeder wirksame Bedarfsplatz einzeln erscheinen, auch wenn er vollständig gedeckt ist?

**Bereits verbindliche Grundlage:** Ja. Die Liste enthält alle Bedarfe und unterscheidet vollständig gedeckt, beim bestätigten `Spr`-Sonderfall teilweise gedeckt und ungedeckt.

Antwort: Ja, alle Bedarfe und deren Deckung pro Woche.

### B-02 – Angaben je Bedarf

Welche Werte soll eine Bedarfszeile mindestens zeigen?

**Empfehlung:** Datum und Wochentag, Einsatzort, Dienst, tatsächliche Bedarfszeit, benötigte Personenzahl und Minuten, gedeckte Personenzahl und Minuten, offene Personenzahl und Minuten sowie Deckungsstatus. Bei `Spr` wird der konkret gedeckte und offene Zeitraum sichtbar.

Antwort: deine empfehlung

### B-03 – Wochensummen

Welche Zusammenfassung soll oberhalb jeder Bedarfsliste stehen?

**Empfehlung:** Gesamtzahl und Gesamtminuten aller Bedarfsplätze, davon vollständig gedeckt, teilweise gedeckt und ungedeckt; zusätzlich die Deckungsquote in Prozent. Personenzahlen und Minuten werden getrennt ausgewiesen und nicht zu einer uneindeutigen Punktzahl vermischt.

Antwort: deine empfehlung

### B-04 – Gruppierung und Sortierung

Wie soll die Bedarfsliste innerhalb einer Woche geordnet werden?

**Empfehlung:** Standardmäßig nach Datum, Einsatzort, Beginn, Dienst und Platznummer. Ein zusätzlicher Filter „nur offene/teilweise offene“ darf die Prüfung erleichtern, aber die vollständige Liste bleibt jederzeit erreichbar.

Antwort: deine empfehlung

### B-05 – Typ1 und manuelle Werte

Soll die Deckung alle zum betrachteten Plan gehörenden Zuweisungen einbeziehen, unabhängig davon, ob sie von Typ1, der Automatik oder später aus einer manuellen Bearbeitung stammen?

**Empfehlung:** Ja. Der Planungsbericht beschreibt den tatsächlich betrachteten Planstand. Die Herkunft darf kenntlich gemacht werden, darf die Deckungsrechnung aber nicht verfälschen. Der Generierungsbericht bleibt dagegen an den ursprünglichen automatischen Lauf gebunden.

Antwort: deine empfehlung

## C – Personen, Sollzeit und tatsächliche Planzeit

### C-01 – Personenkreis

Welche Personen erscheinen in der Wochenliste?

**Empfehlung:** Alle im Planungszeitraum aktiven Personen einschließlich Typ1, regulärer Personen und AH, auch wenn ihre geplante Arbeitszeit in einer Woche null ist. Die sichtbare Reihenfolge entspricht dem Dienstplan: Typ1, regulär, AH; innerhalb der Gruppen gilt die vorhandene Namenssortierung.

Antwort: deine empfehlung

### C-02 – Bedeutung von „Soll“

Soll pro Woche nur das ursprüngliche Wochen-Soll oder auch das durch Urlaub/Krankheit angepasste wirksame Soll gezeigt werden?

**Empfehlung:** Beide Werte zeigen: `Wochen-Soll`, `wirksames Soll` und `geplante Arbeitszeit`. Dadurch ist erkennbar, ob eine Abweichung durch Abwesenheit oder durch die Planung entsteht. Die zentrale Vergleichsdifferenz verwendet das wirksame Soll.

Antwort: deine empfehlung

### C-03 – Bedeutung von „tatsächliche Arbeitszeit“

Ist mit der tatsächlichen Arbeitszeit die Summe der im Plan zugewiesenen Arbeitsminuten gemeint und nicht eine spätere Zeiterfassung real geleisteter Stunden?

**Empfehlung:** Ja. Die Bezeichnung im Bericht lautet eindeutig `geplante Arbeitszeit`, weil die App in diesem System noch keine Ist-Zeiterfassung besitzt.

Antwort: ja deine empfehlug

### C-04 – Abweichung zum Soll

Soll zusätzlich die Differenz zwischen geplanter Arbeitszeit und wirksamem Soll gezeigt werden?

**Empfehlung:** Ja, in Stunden und Minuten mit Vorzeichen. Farben können unterstützen, die Bedeutung bleibt aber immer auch textlich sichtbar.

Antwort: ja

### C-05 – Urlaub, Krankheit und freie Tage

Wie werden `U`, `K`, rote und schwarze `X` behandelt?

**Empfehlung:** `U` und `K` wirken nur über das bereits berechnete wirksame Soll und zählen nicht als geplante Arbeitszeit. Ihre Anzahl erscheint im Personendetail als Erklärung des wirksamen Solls. Rote und schwarze `X` zählen weder als Arbeitszeit noch als Dienst und erscheinen nicht als Zählspalten der Haupttabelle.

Antwort: Ja, diese Empfehlung.

### C-06 – Lesbarkeit bei vielen Personen

Welche verdichtete Darstellung ist gewünscht?

**Empfehlung:** Eine Zeile je Person und Woche mit Name, Rolle/Typ, Wochen-Soll, wirksamem Soll, Planzeit und Differenz. Details zu Diensten stehen direkt daneben oder in einem aufklappbaren Unterbereich; horizontales Springen zwischen getrennten Tabellen wird vermieden.

Antwort: deine empfehlung

## D – Dienstanzahlen je Person und Woche

### D-01 – Vollständige Dienstzählung

Soll für jede Person und Woche die Anzahl jedes tatsächlich vorkommenden Dienstes gezeigt werden?

Antwort: Ja. Es soll sichtbar sein, welche Dienste eine Person wie oft pro Woche macht.

### D-02 – Dienste ohne Vorkommen

Sollen Dienste mit Anzahl null sichtbar bleiben?

**Empfehlung:** Die für den Zeitraum relevanten Dienste bleiben als stabile Spalten sichtbar, auch bei null. Dadurch lassen sich Personen direkt vergleichen und fehlende `F`- oder `S`-Zuweisungen fallen auf.

Antwort: ja deine empfehlung

### D-03 – Zählweise von `D`

Wie soll ein Doppeldienst gezählt werden?

**Empfehlung:** In der Zählung einmal als `D`, nicht zusätzlich als je ein `F` und `S`. Es gibt keine zusätzliche Segmentzählung für `F` und `S`. So bleibt die `D`-Minimierung direkt messbar und die Summe der Einsätze wird nicht doppelt gezählt.

Antwort: Ja. `D` wird einmal als `D` gezählt; eine zusätzliche Segmentzählung als `F` und `S` wird nicht benötigt.

### D-04 – Zählweise von `Spr`

Wie soll der Springer-Einsatz gezählt werden?

**Empfehlung:** Einmal als `Spr`; die beiden gedeckten Bedarfsanteile erscheinen zusätzlich in der Bedarfsdeckung, nicht als zwei weitere Dienste der Person.

Antwort: deine empfehlung

### D-05 – Typ1-Bürozeit und zusätzliche manuelle Besetzung

Wie werden Bürozeit und spätere manuelle Zusatzbesetzungen gezählt?

**Empfehlung:** Bürozeit erhält eine eigene sichtbare Kategorie und wird nicht als Bedarfsdienst ausgegeben. Eine manuelle Zusatzbesetzung zählt als der tatsächlich zugewiesene Dienst und wird als manuell zusätzlich gekennzeichnet.

Antwort: deine empfehlung

### D-06 – Diagnose für `F`, `S` und `D`

Welche zusätzlichen Kennzahlen sollen die bekannten Baustellen direkt sichtbar machen?

**Empfehlung:** Pro Woche Gesamtanzahl `F`, `S`, `D` und `Spr`; je geeigneter Person deren Einzelanzahl; für `F` und `S` jeweils kleinster und größter Wert sowie Spannweite. Diese Werte sind zunächst Beobachtungen und noch kein neues Fairnessziel.

Antwort: Pro Woche werden Gesamtanzahl `F`, `S`, `D` und `Spr`, die Einzelanzahl je geeigneter und in dieser Woche grundsätzlich planbarer Person sowie für `F` und `S` jeweils Minimum, Maximum und Spannweite gezeigt. Diese Kennzahlen beschreiben den aktuellen Stand, bewerten ihn aber noch nicht mit einer neuen Fairnessformel.

### D-07 – Vergleichsgruppe für Fairness

Welche Personen dürfen bei der `F`-/`S`-Verteilung miteinander verglichen werden?

**Empfehlung:** Nur Personen, die für den jeweiligen Dienst fachlich geeignet sind und in der betrachteten Woche grundsätzlich planbar waren. Typ1, reguläre Personen und AH werden nicht unbesehen in eine einzige Spannweite geworfen. Die genaue Normalisierung nach Verfügbarkeit oder geplanter Dienstzahl bleibt eine fachliche Entscheidung für den späteren Optimierungsplan.

Antwort: deine empfehlung

## E – Generierungsbericht und Phasenentscheidungen

### E-01 – Aussage des Generierungsberichts

Soll der Bericht je Planungsphase das Ziel, den erreichten Wert und die danach für nachrangige Phasen festgeschriebene Grenze zeigen?

**Empfehlung:** Ja. Das ist die belastbare Form von „Entscheidungen der Planungsphasen“. Der Bericht behauptet nicht, der Solver habe einen menschlichen Gedankengang oder eine einzelne Person aus genau einem Grund ausgewählt.

Antwort: Ja. Der Bericht zeigt Ziel, erreichten Wert und festgeschriebene Grenze jeder Phase so übersichtlich wie möglich. Er behauptet keinen menschlichen Gedankengang und keine unbelegte einzelne Ursache für die Auswahl einer Person.

### E-02 – Aufzuführende Phasen

Welche Phasen müssen mindestens erscheinen?

**Empfehlung:** Eingangs- und Strukturprüfung, Hard Rules, bestmögliche reguläre Deckung ohne `Spr`, zusätzliche Deckung unter Zulassung des `Spr`-Notfalls, hohe Regeln, `Spr`-Minimierung, `D`-Minimierung, AH-Mindestintegration, relative Wochenzielannäherung, mittlere Regeln, niedrige Regeln, Stabilität und technischer Gleichstandsentscheid. Die Deckungsphasen zeigen jeweils offene Mitarbeiterminuten und vollständig offene Bedarfsplätze. Nicht ausgeführte oder nicht anwendbare Phasen werden ausdrücklich so gekennzeichnet.

Antwort: deine empfehlung

### E-03 – Werte je Phase

Welche Angaben sollen je Phase erscheinen?

**Empfehlung:** Reihenfolge, deutscher Name, fachliches Ziel, Status, Ausgangswert soweit sinnvoll, bester erreichter Wert, festgeschriebener Wert für folgende Phasen, Zahl der betroffenen Fälle, Ausmaß, Laufzeit sowie Kennzeichnung `optimal bewiesen` oder `zulässig, nicht nachweislich optimal`.

Antwort: deine empfehlung

### E-04 – Personenbezogene Einzelentscheidungen

Soll S09B für jede einzelne Zuweisung behaupten, warum genau diese Person gewählt und eine andere nicht gewählt wurde?

**Empfehlung:** Nein. Der aktuelle CP-SAT-Lauf liefert dafür keinen eindeutigen einzelnen Kausalgrund; mehrere Bedingungen und nachrangige Gleichstandsentscheidungen wirken zusammen. S09B zeigt stattdessen Phasenergebnisse und die daraus entstandene Verteilung. Kontrollierte personenbezogene Ursachen für offene Bedarfe bleiben System 10. Falls später echte Gegenfaktualanalysen je Zuweisung gewünscht sind, benötigen sie einen eigenen Folgeplan und Laufzeitnachweis.

Antwort: deine empfehlung

### E-05 – Regelverletzungen

Wie detailliert werden weiche Regelverletzungen dargestellt?

**Empfehlung:** Pro Prioritätsphase eine Summe und darunter je Regelkennung die Zahl der Fälle und das Ausmaß. Application übersetzt bekannte Regelkennungen in kurze deutsche Bezeichnungen. Vollständige Ursachen- und Lösungstexte bleiben System 10.

Antwort: deine empfehlung

### E-06 – Laufzeiten

Sollen neben der Gesamtlaufzeit die Laufzeiten der einzelnen Phasen erscheinen?

**Empfehlung:** Ja, soweit sie verlässlich gemessen werden. Modellaufbau, einzelne Optimierungsphasen, Ergebnisabbildung und Gesamtzeit werden getrennt gezeigt. Messaufwand und Berichtserzeugung dürfen das Solver-Zeitbudget nicht verfälschen.

Antwort:ja deine empfehlung

### E-07 – Technische Details

Sollen Solvername, Version, Zeitgrenze und deterministische Einstellungen sichtbar sein?

**Empfehlung:** In einem aufklappbaren technischen Abschnitt ja. Die normale Service-Leitungsansicht beginnt mit fachlichen Werten; technische Reproduzierbarkeitsdaten bleiben für Vergleiche erreichbar.

Antwort: deine empfehlung

### E-08 – `F`-/`S`-Fairness im Phasenbericht

Wie wird sichtbar, ob die aktuelle Stabilitäts-/Fairnessphase `F` und `S` tatsächlich verbessert hat?

**Empfehlung:** Der Generierungsbericht zeigt für diese Phase die Verteilung vor und nach der Phase nur dann, wenn der Optimierer beide belastbar als strukturierte Zwischenwerte liefern kann. Mindestens zeigt der Planungsbericht die Endverteilung. Eine neue Fairnessformel wird nicht stillschweigend in S09B erfunden.

Antwort: deine empfehlung

### E-09 – `D`-Minimierung im Phasenbericht

Welche Aussage muss die `D`-Phase liefern?

**Empfehlung:** Anzahl `D` vor der Phase, kleinste gefundene und für nachrangige Phasen festgeschriebene Anzahl, Optimalitätsstatus und Laufzeit. So ist unterscheidbar, ob zu viele `D` aus der Zielreihenfolge, einer Zeitgrenze oder fehlenden Alternativen stammen; eine vollständige Einzelfallursache wird noch nicht behauptet.

Antwort: deine empfehlung

## F – Darstellung und Bedienung

### F-01 – Einstiegspunkt

Wo soll der Bericht erreichbar sein?

**Empfehlung:** Direkt in der Generierungsvorschau über `Bericht anzeigen` und nach der Übernahme im Generierungsbereich des Dienstplan-Tabs. Der Bericht öffnet sich in einem eigenen nicht-modalen Fenster, sodass Hauptfenster und Berichtsfenster nebeneinander sichtbar und bedienbar bleiben. Die Dienstplantabelle wird dadurch nicht verkleinert.

Antwort: Ja. Der Bericht soll in einem eigenen Fenster geöffnet werden, sodass Dienstplan und Bericht nebeneinander offen sein können.

### F-02 – Navigation im Bericht

**Empfehlung:** Das eigene Berichtsfenster zeigt oben eine kompakte Drei-Wochen-Zusammenfassung, darunter die Umschaltung zwischen `Planung` und `Generierung`, innerhalb des Planungsberichts Woche 1 bis 3. Tabellenkopf und Personenname bleiben beim Scrollen sichtbar, soweit WPF dies robust erlaubt.

Antwort: erstmal deine empfehlung

### F-03 – Filter

Welche Filter sind für die erste Fassung erforderlich?

**Empfehlung:** Woche, alle/offene Bedarfe und Personengruppe. Keine frei konfigurierbare Analyseoberfläche und keine versteckten Filter, die einen unvollständigen Bericht wie einen vollständigen wirken lassen.

Antwort: erstmal deine empfehlung

### F-04 – Export oder Kopieren

Soll S09B den Bericht exportieren, drucken oder in die Zwischenablage kopieren?

**Empfehlung:** Noch nicht. Personenbezogene Daten bleiben in der lokalen App; Excel-Export gehört zu System 13. Falls ein späterer Diagnoseexport gewünscht wird, benötigt er ein eigenes Datenschutz- und Dateiformat-Gate.

Antwort: nein noch nicht

### F-05 – Leere und besondere Zustände

**Empfehlung:** Nullwerte, vollständig abwesende Personen, Wochen ohne zulässigen Bedarf, teilweise `Spr`-Deckung und ein nicht nachweislich optimales Ergebnis erhalten eindeutige Texte. Farbe allein vermittelt keinen Status.

Antwort: deine empgehlung

## G – Architektur, Speicherung und Wahrheitsgrenze

### G-01 – Berechnungsort

**Empfehlung:** Stabile fachliche Berichtswerte werden aus unveränderlichen Momentaufnahmen in Domain/Application berechnet. Planning liefert nur strukturierte primitive Phasen- und Ergebniswerte. Desktop formatiert und zeigt, berechnet aber weder Deckung noch Arbeitszeit, Dienstanzahl oder Fairness neu.

Antwort: deine empgehlung

### G-02 – Einfluss auf den Generator

**Bereits verbindliche Grundlage:** Die Erfassung von Berichtsdaten darf Auswahl, Zielreihenfolge, Solverbedingungen, Zeitbudget und Reproduzierbarkeit nicht verändern. Abweichungen zwischen Bericht und tatsächlichem Vorschlag blockieren die Anzeige als technischer Fehler; sie werden niemals stillschweigend gerundet oder geschönt.

Antwort: Der Generator soll in S09B fachlich unverändert bleiben.

### G-03 – Persistenz

**Empfehlung:** Der bereits gespeicherte aktuelle automatische Lauf darf um die zwingend benötigten neutralen Phasenwerte ergänzt werden, wenn Vorschau und übernommener Bericht sonst nicht wahrheitsgemäß identisch bleiben. Keine Berichtshistorie, keine echten Daten in Logs und keine eigenständigen Berichtsexporte.

Antwort: deine empgehlung

### G-04 – Datenschutz

**Empfehlung:** Produktive Berichtsdaten bleiben ausschließlich lokal. Technische Fehlerprotokolle enthalten keine Namen, Dienstpläne oder vollständigen Berichte. Tests, Screenshots und Dokumentation verwenden klar erfundene Daten.

Antwort: deine empgehlung

## H – Prüfung und Abnahme

### H-01 – Referenzrechnung

**Empfehlung:** Bedarfsdeckung, Wochenminuten und Dienstanzahlen werden in Application-Tests aus kleinen synthetischen Plänen unabhängig nachgerechnet. `D`, `Spr`, Typ1-Bürozeit, Abwesenheit und Teildeckung besitzen eigene Grenzfälle.

Antwort: deine empgehlung

### H-02 – Phasenbericht

**Empfehlung:** Planning-Tests erzwingen vollständige Phasenreihenfolge, korrekte Werte, Status und Laufzeitgrenzen. Eine absichtlich verfälschte Phase muss von der unabhängigen Ergebnisprüfung erkannt werden.

Antwort: deine empgehlung

### H-03 – Sichtbare Abnahme

**Empfehlung:** Die Service-Leitung prüft einen synthetischen Drei-Wochen-Fall sichtbar: alle Bedarfsstatus, drei Wochen je Person, `F`/`S`/`D`/`Spr`, Soll-/Planzeit, Phasenfolge, Scrollen, Filter und Lesbarkeit. Ein Build oder ViewModel-Test ersetzt dieses Gate nicht.

Antwort: deine empgehlung

### H-04 – Abschluss des Fragenkatalogs

Wann darf die Roadmap zur Implementierung freigegeben werden?

**Empfehlung:** Erst wenn insbesondere A-02, C-02/C-03, D-03/D-04/D-07, E-01 bis E-09, F-01/F-04 und G-03 beantwortet sind und die Grenze zu System 10 ausdrücklich bestätigt wurde.

Antwort: Ja. Der vollständig beantwortete und konsolidierte Fragenkatalog sowie die daran angepasste Roadmap werden vor QB-02 ausdrücklich abgenommen. Die Grenze zu System 10 bleibt bestehen.

## Abschließende Konsolidierung der bestätigten Kernentscheidungen

1. Bericht bereits in der Vorschau und nach Übernahme identisch erreichbar.
2. Wochen-Soll, wirksames Soll, geplante Arbeitszeit und Differenz gemeinsam zeigen.
3. `D` und `Spr` jeweils einmal als eigenes Muster zählen, nicht zusätzlich als normale Teildienste.
4. `F`-/`S`-Vergleich nur unter fachlich geeigneten und in der Woche planbaren Personen; die genaue spätere Fairnessformel noch nicht vorwegnehmen.
5. Generierungsbericht als Phasenbilanz statt unbelegbarer Begründung jeder einzelnen Personenzuweisung.
6. Keine Berichtshistorie und kein Export in S09B.
7. Phasenwerte nur strukturiert und ohne Einfluss auf das Solverergebnis erfassen.
8. Jeder Generierungsversuch erhält auch ohne zulässigen Vorschlag einen Generierungs- beziehungsweise Fehlerbericht; ohne Plan entfallen ausschließlich die Planungswerte.
9. `U`- und `K`-Anzahlen erklären im Personendetail das wirksame Soll; rote und schwarze `X` erhalten keine Zählspalten der Haupttabelle.
10. Der Bericht öffnet in einem eigenen nicht-modalen Fenster, das neben dem Hauptfenster sichtbar und bedienbar bleibt.

## Ergebnisstand des Verständnisabgleichs

- Alle Fragen A-01 bis H-04 sind beantwortet und inhaltlich konsolidiert.
- Die Aussagegrenze zwischen Planungsbericht, Generierungsbericht und späterer Konflikterklärung ist bestätigt.
- Die zusätzlichen Entscheidungen zum Fehlerbericht, zu den `F`-/`S`-Vergleichswerten, zum `U`-/`K`-Kontext und zum eigenen Berichtsfenster sind bestätigt.
- Der Generator bleibt während S09B fachlich unverändert.
- Der Fragenkatalog ist damit bereit, zusammen mit der angepassten Roadmap ausdrücklich abgenommen zu werden.

## Nächster minimaler Schritt

Der Fragenkatalog und die angepasste S09B-Roadmap sind ausdrücklich abgenommen. QB-02 ist der freigegebene nächste Implementierungsschritt.
