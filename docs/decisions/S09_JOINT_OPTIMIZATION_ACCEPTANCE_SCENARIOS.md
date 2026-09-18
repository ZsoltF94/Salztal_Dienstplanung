# System 09 – Fachliche Vergleichsfälle für die gemeinsame Optimierung

Stand: 2026-09-17

Status: Automatisch geprüft; technisch funktionsfähiger sichtbarer Ausgangsstand bestätigt; endgültige fachliche Entscheidung nach S09A, S09B und gezieltem Folgeplan offen

## Zweck

Diese fünf vollständig erfundenen Kleinstfälle machen die Wirkung der gemeinsamen Nicht-AH-/AH-Optimierung verständlich. Sie enthalten keine echten Namen, Verfügbarkeiten oder Plandaten.

Jeder Fall wird im Planning-Testprojekt mit demselben produktiven Kandidatenbau und Solver ausgeführt. Eine vollständige Enumeration aller zulässigen Kandidatenauswahlen vergleicht das Ergebnis unabhängig über den Domain-Zielvektor. Bei vollkommen symmetrischen Zuordnungen darf ausschließlich der technische Gleichstandsschlüssel abweichen; alle fachlichen Stufen bis einschließlich Einsatzfairness müssen gleich sein. Getrennte Permutationstests sichern die reproduzierbare technische Auswahl.

## Ergebnisübersicht

| Fall | Ausgangslage | Erwartetes und automatisch bestätigtes Ergebnis |
|---|---|---|
| A – normale Untergrenze und AH-Integration | Eine normale Person besitzt bereits 1.020 geschützte Minuten. Eine AH-Person und die normale Person dürfen einen weiteren normalen Dienst mit 180 Minuten übernehmen. | Der Dienst geht an AH. Nicht-AH bleibt genau auf der hohen Untergrenze von 1.020 Minuten, AH erreicht 180 Minuten, der Bedarf ist vollständig gedeckt, `D = 0`, `Spr = 0`. Die frühere eingefrorene Nicht-AH-Auswahl wäre auf der AH-Mindeststufe schlechter. |
| B – knapper AH-Bedarf | Zwei gleich geeignete AH-Personen konkurrieren in einer Woche um zwei normale Dienste mit je 300 Minuten. Für beide zusammen reichen die Dienste nicht bis zu ihren 600-Minuten-Zielen. | Beide erhalten 300 Minuten. Beide überschreiten das Mindestziel von 180 Minuten gleichmäßig; `D = 0`, `Spr = 0`, kein Bedarf bleibt offen. |
| C – nicht zulässig deckbar | Eine AH-Person ist die einzige geeignete Person für einen Dienst mit 721 Minuten. Die unverletzbare AH-Wochengrenze beträgt 720 Minuten. | Es wird keine unzulässige Zuweisung erzeugt. Alle 721 Minuten bleiben vollständig und sichtbar offen. |
| D – `D` erforderlich | Eine normale Person ist die einzige geeignete Person für einen Früh- und einen Spätdienst am selben Tag. Die bestätigte `D`-Kombination deckt beide Dienste. | Genau ein `D` wird verwendet, weil nur so beide Bedarfe gedeckt werden. Es bleibt kein Bedarf offen; `Spr = 0`. |
| E – `Spr` als Notfall | Eine normale Person kann am Samstag einen Cafeteria-Dienst mit 240 Minuten und einen anschließenden Spätdienst mit 180 Minuten nicht als zwei normale Dienste übernehmen. `Spr` ist für den Lauf freigegeben. | Genau ein `Spr` wird verwendet. Die bestätigte frühe Lücke des Spätdienstes von 16:30 bis 17:30 Uhr bleibt mit 60 Minuten sichtbar offen; `D = 0`. |

## Automatische Nachweise

- `JointWeeklyObjectiveOptimizationTests.JointOptimizationMatchesExhaustiveReferenceAndImprovesOldPhaseFreeze` prüft Fall A einschließlich der kontrollierten alten Freeze-Auswahl.
- `JointOptimizationAcceptanceScenarioTests` prüft die Fälle B bis E gegen die vollständige Enumeration.
- Die übrigen Planning-Tests prüfen zusätzlich 179/180, 359/360, 600, 720 und mehr als 720 AH-Minuten, Früh-/Spät-, Wochenend-, `D`- und `Spr`-Fairness, unterschiedliche Eignung, Eingabepermutationen, Zeitgrenze, Abbruch und unabhängige Nachprüfung.
- Domain-Tests prüfen die komplette Zielreihenfolge und relative Ziele für 10, 20, 25, 30, 35 und 40 Stunden, durch `U/K` reduzierte Ziele sowie Ziel null.

## Offenes fachliches und sichtbares Gate

Die Service-Leitung prüft diese Fälle weiterhin vor AG-15:

1. Sind die Ergebnisse A bis E fachlich nachvollziehbar und entsprechen sie dem gewünschten Grundverhalten?
2. Ist insbesondere Fall A die gewünschte Art, AH einzubeziehen, ohne Nicht-AH unter die bestätigte hohe Untergrenze zu drücken?
3. Ist die relative Verteilung in Fall B sinnvoller als eine starre Reihenfolge nach Mitarbeitertyp?
4. Sind `D` in Fall D und die sichtbare Teildeckung bei `Spr` in Fall E verständlich?
5. Zeigt die laufende WPF-App Start, Laufzustand, Abbruch, Vorschau, offene Bedarfe, schwarze `X`, Übernehmen, Verwerfen und vollständiges Zurücksetzen wie erwartet?

Die erneute sichtbare Generierung funktioniert, ihre Ergebnisqualität ist jedoch ausdrücklich noch nicht angenommen. Deshalb bleibt der produktive Generator zunächst unverändert. Der reine UI-Umbau S09A ist abgeschlossen. Der S09B-Fragenkatalog ist vollständig beantwortet und die Roadmap auf die bestätigten Berichtskennzahlen, Fehlerberichte und das eigene nicht-modale Fenster angepasst; ihre gemeinsame ausdrückliche Abnahme steht noch aus. Erst der anschließend am eingefrorenen Ausgangsstand ausgeführte Bericht begründet einen eigenen abgenommenen Plan für gezielte Optimierungen oder den Rückfall.

Danach ist genau eine ausdrückliche Entscheidung über den endgültig vorgesehenen Algorithmus erforderlich:

- **Primärlösung annehmen:** AG-15 darf beginnen.
- **Primärlösung ablehnen:** AG-15 bleibt gesperrt; vor einem Umbau entsteht ein eigener abgenommener Folgeplan für die dokumentierte fünfstufige Rückfalllösung.
