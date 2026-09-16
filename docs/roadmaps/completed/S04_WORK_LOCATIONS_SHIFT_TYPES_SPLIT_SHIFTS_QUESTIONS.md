# Fragen zu Einsatzorten, Diensttypen, Doppeldiensten und Belegung

Status: Fachlich beantwortet und als Grundlage des am 2026-09-13 abgeschlossenen Systems 04 archiviert

Stand: 2026-09-13

## Zweck

Diese Datei sammelt die offenen Fragen zur aktiven Teil-Roadmap für System 04. Zusätzlich trennt sie Angaben, die erst in System 05 – Personal-, Schicht- und Stundenbedarf – umgesetzt werden.

Die Antworten werden nach ihrer gemeinsamen Bestätigung in die verbindlichen Grundlagen und Roadmaps übernommen. Bis dahin sind sie noch keine Umsetzungsfreigabe.

## So kann die Datei beantwortet werden

- Bei Auswahlfragen bitte das passende Feld mit `[x]` markieren.
- Freitext kann direkt hinter `Antwort:` ergänzt werden.
- Wenn eine Frage noch mit der Service-Leitung geklärt werden muss, genügt zunächst `Antwort: noch zu klären`.
- Nicht zutreffende Fragen können mit `Antwort: entfällt` beantwortet werden.
- Die Empfehlungen sind Vorschläge und noch nicht automatisch bestätigt.

## A – Übermittelte Ausgangsdaten prüfen

### A-01 – Einsatzorte und allgemeine Arbeitszeiten

| Einsatzort | Übermittelte allgemeine Arbeitszeiten |
|---|---|
| Cafeteria | Montag bis Sonntag, 13:30–20:30 Uhr |
| Restaurant | Montag bis Sonntag, 06:30–13:30 Uhr und 16:30–19:30 Uhr |

Sind diese Angaben vollständig und richtig?

Antwort: Ja

### A-02 – Diensttypen und zusammengesetzte Einsatzmuster

| Dienst oder Muster | Kategorie | Abkürzung beziehungsweise Anzeige | Zeit | Einsatzort |
|---|---|---|---|---|
| Frühdienst | Diensttyp | `F` | 06:30–13:30 Uhr | Restaurant |
| Spätdienst | Diensttyp | `S` | 16:30–19:30 Uhr | Restaurant |
| Doppeldienst | Einsatzmuster | `D` | Frühdienst und Spätdienst | Restaurant |
| Cafeteria-Dienst A | Diensttyp | Zeitangabe statt fester Abkürzung | 13:30–20:30 Uhr | Cafeteria |
| Cafeteria-Dienst B | Diensttyp | Zeitangabe statt fester Abkürzung | 13:30–17:30 Uhr | Cafeteria |
| Springer-Einsatz | Einsatzmuster | `Spr` | Beginn 13:30 Uhr, Wechsel bedarfsabhängig, Ende regulär 19:30 Uhr | zuerst Cafeteria, danach Restaurant |

Sind Namen, Abkürzungen, Zeiten und Zuordnungen in dieser Tabelle richtig?

Antwort: Ja

Bestätigte Einordnung aus ED-02: Frühdienst, Spätdienst sowie Cafeteria-Dienst A und B sind die normalen Diensttypen. Ein einzelner Bedarf verlangt genau einen dieser Diensttypen. `D` und `Spr` verbinden jeweils zwei Diensttypen und sind deshalb zusammengesetzte Einsatzmuster, keine zusätzlichen von einem einzelnen Bedarf verlangten Diensttypen.

### A-03 – Reguläre benötigte Belegung

| Einsatzort | Tage | Übermittelte Belegung |
|---|---|---|
| Cafeteria | Montag bis Freitag | eine Person von 13:30–20:30 Uhr |
| Cafeteria | Samstag und Sonntag | eine Person von 13:30–20:30 Uhr und eine zweite Person von 13:30–17:30 Uhr |
| Restaurant | Montag bis Sonntag | vier Personen von 06:30–13:30 Uhr und vier Personen von 16:30–19:30 Uhr |

Ist diese reguläre Grundbelegung vollständig und richtig?

Antwort: ja

## B – Einsatzorte in System 04

### B-01 – Startwerte

Sollen Cafeteria und Restaurant beim ersten Start der App bereits angelegt sein?

- [x] Ja, beide werden als Startwerte angelegt.
- [ ] Nein, die Service-Leitung legt beide selbst an.
- [ ] Andere Lösung.

Antwort:

Empfehlung: Beide bestätigten Einsatzorte als Startwerte anlegen und trotzdem weitere Einsatzorte erlauben.

### B-02 – Bedeutung der allgemeinen Arbeitszeiten

Sind die unter A-01 genannten Zeiten eigenständige Öffnungs- beziehungsweise Betriebszeiten des Einsatzortes, oder ergeben sie sich nur aus den dort verwendeten Diensten und der benötigten Belegung?

- [ ] Eigenständige, bearbeitbare Zeiten des Einsatzortes.
- [x] Keine eigenen Einsatzortzeiten; die Zeiten ergeben sich aus Diensttypen und Belegung.
- [ ] Beides wird benötigt und muss gegeneinander geprüft werden.

Antwort: Die genannten Zeiten sind keine Öffnungs- oder Betriebszeiten des Einsatzortes. Sie beschreiben die Zeiten, während derer eine bestimmte Anzahl von Mitarbeitenden eingeplant werden muss. Diese Belegungszeiten müssen bearbeitbar sein.

Empfehlung: Eigene Einsatzortzeiten nur speichern, wenn sie unabhängig von Diensten und Belegung eine fachliche Bedeutung haben. Andernfalls würden dieselben Zeiten unnötig doppelt gepflegt.

### B-03 – Name und Abkürzung eines Einsatzortes

1. Müssen Einsatzortnamen eindeutig sein?
2. Benötigen Einsatzorte eine kurze Abkürzung für spätere Tabellen?
3. Falls ja: Welche Abkürzungen sollen Cafeteria und Restaurant erhalten?

Antwort: Die Einsatzorte werden zusätzlich zum sichtbaren Text farblich gekennzeichnet. Cafeteria wird gelb und das vorerst zusammengefasste Restaurant rot dargestellt. Kinzigstube und Spessarteck werden in der ersten Fassung nicht getrennt. Die konkrete Farbgestaltung bleibt zusätzlich durch Text erkennbar und wird später in der UI-Abnahme geprüft.

Empfehlung: Eindeutige Namen; Abkürzungen erst ergänzen, wenn sie tatsächlich in einer Ansicht oder Excel-Vorlage gebraucht werden.

### B-04 – Nicht mehr verwendete Einsatzorte

Wie soll mit einem Einsatzort umgegangen werden, der nicht mehr verwendet wird?

- [ ] Nur deaktivieren; frühere Verwendungen bleiben erhalten.
- [ ] Löschen erlauben, solange er noch nie verwendet wurde; sonst deaktivieren.
- [X] Immer endgültig löschen können.
- [ ] Andere Regel.

Antwort: Einsatzorte sollen später immer aus dem aktuellen Stammdatenkatalog gelöscht werden können. Bereits gespeicherte unveränderliche Planversionen behalten Namen, Farbe und sonstige damalige Anzeigedaten als eigene Momentaufnahme. Die Löschfunktion gehört noch nicht zur ersten Bedienfassung.

Bestätigte Einordnung: Das spätere Löschen entfernt den Eintrag aus dem aktuellen Stammdatenkatalog. Unveränderliche frühere Planversionen behalten ihre damalige Momentaufnahme.

## C – Einzelne Diensttypen in System 04

### C-01 – Unbezahlte Pause innerhalb eines einzelnen Dienstes

Enthält einer der einzelnen Dienste eine Unterbrechung, die nicht als Arbeitszeit zählt, oder zählt jeweils die gesamte Zeit zwischen Beginn und Ende als Arbeitszeit?

Antwort: Innerhalb der einzelnen Dienste gibt es keine unbezahlte Pause. Nur der Doppeldienst besitzt zwischen Früh- und Spätdienst eine Unterbrechung. Die App verwaltet keine Entlohnung, muss aber die tatsächliche Arbeitsdauer ohne diese Unterbrechung berechnen können.

Falls solche Unterbrechungen vorkommen: Bei welchen Diensten und mit welcher Dauer beziehungsweise Lage?

Antwort: Beim Doppeldienst liegt die nicht als Arbeitszeit zählende Unterbrechung regulär zwischen 13:30 und 16:30 Uhr.

### C-02 – Cafeteria-Dienst B am Samstag

In der Belegung werden samstags und sonntags zwei Personen benötigt. Beim Cafeteria-Dienst B wurde dagegen ausdrücklich „Sonntag“ genannt. Was gilt regulär?

- [x] Cafeteria-Dienst B gilt regulär am Samstag und Sonntag von 13:30–17:30 Uhr.
- [ ] Cafeteria-Dienst B gilt nur sonntags; samstags wird die zweite Person anders eingeplant.
- [ ] Andere Regel.

Antwort: Regulär gilt der Cafeteria-Dienst B am Samstag und Sonntag. Falls an einem Samstag sonst ein Restaurant-Spätdienst unbesetzt bleibt, kann Cafeteria-Dienst B mit dem anschließenden Spätdienst zum Springer-Einsatz kombiniert werden. Die Zeit vor dem tatsächlichen Wechsel ins Restaurant bleibt dabei als sichtbare Teilunterdeckung des Restaurantbedarfs erhalten. Das kann nur an einem Samstag passieren.

### C-03 – Anzeige der Cafeteria-Dienste

Wie genau soll die Zeit statt einer festen Abkürzung angezeigt werden?

Beispiele:

- [X] `13:30-20:30`
- [ ] `13:30–20:30`
- [ ] `13.30-20.30`
- [ ] Andere Darstellung.

Antwort:

Soll der Name „Cafeteria-Dienst A/B“ zusätzlich sichtbar bleiben oder nur die Zeit?

Antwort: nur die Zeit in der tabelle.

### C-04 – Zuordnung zu Einsatzorten

Soll jeder normale Diensttyp genau einem Einsatzort gehören?

- [X] Ja; Früh- und Spätdienst gehören zum Restaurant, Cafeteria-Dienst A und B zur Cafeteria.
- [ ] Ein normaler Diensttyp darf mehreren Einsatzorten zugeordnet werden.
- [ ] Andere Regel.

Antwort: deine Empfehlung

Empfehlung: Jeder normale Diensttyp gehört genau zu einem Einsatzort. Der Springer wird getrennt als standortübergreifendes Einsatzmuster behandelt.

### C-05 – Kleinste eingebbare Zeiteinheit

In welchen Schritten sollen Zeiten bearbeitet werden können?

- [ ] minutengenau
- [ ] in 5-Minuten-Schritten
- [ ] in 15-Minuten-Schritten
- [X] in 30-Minuten-Schritten
- [ ] andere Schrittweite

Antwort:

Die technische Speicherung erfolgt unabhängig davon minutengenau in ganzen Minuten.

### C-06 – Dienste über Mitternacht

Kann ein Dienst jetzt oder absehbar zukünftig über Mitternacht hinausgehen?

- [X] Nein, das wird in der aktuellen App nicht benötigt.
- [ ] Ja, folgende Dienste können über Mitternacht gehen.
- [ ] Noch zu klären.

Antwort:

### C-07 – Änderung einer Dienstzeit

Was bedeutet „Dienstzeiten sollen bearbeitbar sein“?

- [ ] Eine Änderung verändert den allgemeinen Diensttyp für alle zukünftigen Wochen.
- [ ] Die Zeit soll zusätzlich nur für ein bestimmtes Datum oder eine bestimmte Woche geändert werden können.
- [x] Beide Möglichkeiten werden benötigt.

Antwort: Wir brauchen bearbeitbare Standardzeiten. Neue Bedarfsvorgaben verwenden diese Werte, solange keine ausdrückliche Abweichung erfasst wurde. Zusätzlich müssen Zeiten nur für ein bestimmtes Datum geändert werden können, ohne den zukünftigen Standard zu verändern.
Frühere und bereits abgenommene Pläne behalten ihre damaligen Zeiten als Momentaufnahme.

Empfehlung: Allgemeine Änderungen wirken nur für die Zukunft. Frühere und bereits abgenommene Pläne behalten ihre damaligen Zeiten als Momentaufnahme.

### C-08 – Nicht mehr verwendete Diensttypen

Wie soll mit einem Diensttyp umgegangen werden, der nicht mehr verwendet wird?

- [ ] Nur deaktivieren.
- [ ] Löschen erlauben, solange er noch nie verwendet wurde; sonst deaktivieren.
- [X] Immer endgültig löschen können.
- [ ] Andere Regel.

Antwort: Diensttypen sollen später immer aus dem aktuellen Stammdatenkatalog gelöscht werden können. Bereits gespeicherte unveränderliche Planversionen behalten Bezeichnung, Kürzel, tatsächliche Zeit und sonstige damalige Anzeigedaten als eigene Momentaufnahme. Die Löschfunktion gehört noch nicht zur ersten Bedienfassung.

Bestätigte Einordnung: Das spätere Löschen entfernt den Eintrag aus dem aktuellen Stammdatenkatalog. Unveränderliche frühere Planversionen behalten ihre damalige Momentaufnahme.

## D – Doppeldienst im Restaurant

### D-01 – Erlaubte Kombination

Ist der Doppeldienst ausschließlich die feste Kombination aus Frühdienst und Spätdienst?

- [X] Ja, nur Frühdienst plus Spätdienst ist erlaubt.
- [ ] Später sollen weitere ausdrücklich festgelegte Kombinationen möglich sein.
- [ ] Jede zeitlich passende Kombination zweier Restaurant-Dienste ist erlaubt.

Antwort:

### D-02 – Name und Abkürzung

Soll die Kombination als eigener Eintrag „Doppeldienst“ mit der Abkürzung `D` geführt und angezeigt werden?

Antwort: Ja

### D-03 – Arbeitszeit und Unterbrechung

Bitte bestätigen oder korrigieren:

- Frühdienst: 06:30–13:30 Uhr, sieben Arbeitsstunden
- nicht als Arbeitszeit zählende Unterbrechung: 13:30–16:30 Uhr, drei Stunden
- Spätdienst: 16:30–19:30 Uhr, drei Arbeitsstunden
- Doppeldienst insgesamt: zehn Arbeitsstunden

Antwort: ja

### D-04 – Einsatzortgrenze

Bleibt verbindlich, dass ein Doppeldienst ausschließlich im Restaurant und niemals über zwei Einsatzorte hinweg stattfindet?

Antwort: ja

## E – Springer-Dienst als eigener Sonderfall

Der Springer-Dienst widerspricht nicht zwingend dem Doppeldienst, ist aber etwas anderes: Er wechselt den Einsatzort und besitzt nach bisherigem Verständnis keine unbezahlte Unterbrechung.

### E-01 – Exakte Abschnitte

Bitte bestätigen oder korrigieren:

- Cafeteria: ab 13:30 Uhr bis zum Ende des tatsächlichen Bedarfs für Cafeteria-Dienst B
- unmittelbarer Wechsel nach dem Cafeteria-Bedarf
- Restaurant: ab dem tatsächlichen Wechsel bis zum Ende des Spätdienstbedarfs

Antwort: Der Wechsel erfolgt nicht zu einer festen Uhrzeit, sondern am Ende des für diesen Samstag eingestellten Bedarfs der zweiten Cafeteria-Person. Danach wechselt der Springer ins Restaurant. Die Zeit des Restaurant-Spätdienstbedarfs vor seiner tatsächlichen Ankunft bleibt sichtbar teilweise unbesetzt.

### E-02 – Bezahlte Arbeitszeit

Zählt die gesamte Zeit vom Beginn des Cafeteria-Abschnitts bis 19:30 Uhr als Arbeitszeit und gibt es keine dazwischenliegende Unterbrechung?

Antwort: ja

Falls es eine nicht als Arbeitszeit zählende Unterbrechung gibt: wann und wie lange?

Antwort:

### E-03 – Anrechnung auf die Belegung

Bitte bestätigen oder korrigieren:

- Der Springer zählt bis zum Wechsel als zweite Cafeteria-Person.
- Ab seinem tatsächlichen Wechsel zählt er als eine der vier benötigten Personen im Restaurant-Spätdienst.
- Im Restaurant müssen dann nur noch drei weitere Personen für den Spätdienst eingeplant werden.

Antwort: Ja, allerdings zählt der Springer erst ab seinem tatsächlichen Wechsel als anwesende Restaurant-Person. Bis dahin bleibt der vierte Restaurant-Platz sichtbar unbesetzt. Springer gibt es ausschließlich samstags.

### E-04 – Cafeteria-Bedarf am Springer-Samstag

Wenn der Springer die Cafeteria bereits um 16:30 Uhr verlässt, endet dann an diesem konkreten Samstag auch der Bedarf für die zweite Cafeteria-Person um 16:30 Uhr?

- [X] Ja, der zweite Bedarf wird für diesen Samstag auf 13:30–16:30 Uhr geändert.
- [ ] Nein, von 16:30–17:30 Uhr muss eine andere zweite Person übernehmen.
- [ ] Andere Regel.

Antwort: Die Abhängigkeit geht vom Bedarf aus, nicht vom Springer. Die Service-Leitung stellt die tatsächliche Endzeit des zweiten Cafeteria-Bedarfs ein; erst daraus ergibt sich die Wechselzeit des Springers.

### E-05 – Erlaubte Tage

Ist der Springer-Dienst ausschließlich samstags möglich?

- [X] Ja, ausschließlich samstags.
- [ ] Nein, bei Bedarf auch an folgenden Tagen.
- [ ] Die Tage sollen frei bearbeitbar sein.

Antwort:

### E-06 – Bedeutung von „personeller Notfall“

Wie soll entschieden werden, wann der Springer verwendet wird?

- [ ] Ausschließlich manuell durch die Service-Leitung.
- [X] Die automatische Planung darf ihn nur verwenden, wenn sonst ein Dienst unbesetzt bleibt.
- [ ] Die automatische Planung darf ihn verwenden, soll ihn aber möglichst vermeiden.
- [ ] Andere Regel.

Antwort:

Hinweis: Die endgültige Priorisierung der automatischen Verwendung gehört später in den Regelkatalog und die Planungsengine. System 04 muss zunächst nur die fachliche Form des Springer-Einsatzes eindeutig beschreiben.

### E-07 – Darstellung des Springers

Soll `Spr` als ein benanntes Einsatzmuster aus zwei Abschnitten angezeigt werden, während beide Abschnitte intern ihren jeweiligen Einsatzort behalten?

- [x] Ja, ein Muster `Spr` mit Cafeteria- und Restaurant-Abschnitt.
- [ ] Nein, zwei völlig getrennte Einteilungen ohne gemeinsamen Springer-Eintrag.
- [ ] Andere Darstellung.

Antwort: Der Springer wird als `Spr` bezeichnet und blau hinterlegt. Intern bleiben Cafeteria- und Restaurant-Abschnitt mit ihren tatsächlichen Zeiten und Einsatzorten getrennt erkennbar.

Empfehlung: Ein sichtbares Muster `Spr` aus zwei getrennten Abschnitten. Dadurch bleibt die Einsatzortzuordnung korrekt und der Sonderfall ist trotzdem verständlich auswählbar.

### E-08 – Voraussetzungen der eingesetzten Person

Muss die als Springer eingesetzte Person für Cafeteria und Restaurant freigegeben sein?

Antwort: ja

Werden darüber hinaus bestimmte Qualifikationen benötigt?

Antwort: nein nur die Freigabe

Hinweis: Die konkrete Zuordnung dieser Voraussetzungen zu Mitarbeitenden gehört später zu System 03 und zum Regelkatalog.

## F – Belegung und Änderungen für System 05

Diese Fragen werden jetzt dokumentiert, aber erst in System 05 umgesetzt. System 04 definiert die Dienste; System 05 definiert Anzahl und zeitliche Geltung der benötigten Plätze.

### F-01 – Dauer einer Änderung

Wenn die Service-Leitung beispielsweise den zweiten Cafeteria-Bedarf eines Samstags auf 13:30–16:30 Uhr ändert, worauf wirkt diese Änderung?

- [ ] Nur auf das ausgewählte konkrete Datum.
- [ ] Auf die gesamte ausgewählte Planungswoche.
- [ ] Auf alle zukünftigen Samstage als neuer Standard.
- [x] Beim Speichern soll zwischen einmaliger Ausnahme und neuem Standard gewählt werden.

Antwort: Deine Empfehlung. Es soll möglich sein nur ein Datum zu ändern, aber auch den Standart zu ändern.

Empfehlung: Bewusste Auswahl zwischen einmaliger Datums-Ausnahme und Änderung des zukünftigen Standards.

### F-02 – Frei eingegebene Bedarfszeiten

Darf die Service-Leitung für eine einzelne Woche eine Bedarfszeit eingeben, für die noch kein allgemeiner Diensttyp vorhanden ist?

- [ ] Nein; zuerst muss ein passender Diensttyp angelegt werden.
- [ ] Ja; daraus entsteht ein ausdrücklich angelegter einmaliger Sonderdienst für dieses Datum.
- [ ] Ja; Bedarf und Diensttyp dürfen dauerhaft voneinander abweichende Zeiten haben.
- [x] Andere Regel.

Antwort: Jeder Bedarf verlangt genau einen vorhandenen Diensttyp. Der Bedarf besitzt die tatsächlich zu besetzende Anfangs- und Endzeit. Der Diensttyp besitzt eine bearbeitbare Standardzeit, die für neue Bedarfsvorgaben als Ausgangswert dient und ansonsten informativ zeigt, wann dieser Dienst normalerweise stattfindet. Eine ausdrücklich geänderte Bedarfszeit darf von der Standardzeit abweichen, bleibt aber demselben Diensttyp zugeordnet. Die automatische Planung erfindet weder den Diensttyp noch die Abweichung.

Beispiele:

- Restaurantbedarf 06:30–12:30 Uhr verlangt weiterhin den Diensttyp Frühdienst, obwohl dessen Standardzeit 06:30–13:30 Uhr ist.
- Cafeteria-Dienst A übernimmt den ersten Cafeteria-Bedarf, Cafeteria-Dienst B den zweiten.
- Falls samstags sonst ein Restaurant-Spätdienst unbesetzt bleibt, können Cafeteria-Dienst B und Spätdienst zum Springer-Einsatz `Spr` kombiniert werden.

Mitarbeitende erhalten später getrennte Freigaben für einzelne Diensttypen. Eine Person kann beispielsweise Cafeteria-Dienst A übernehmen dürfen, Cafeteria-Dienst B aber nicht.



Bestätigte Einordnung: Der Bedarf verlangt genau einen vorhandenen Diensttyp, besitzt aber seine eigene tatsächliche Zeit. Eine ausdrückliche Zeitabweichung erzeugt keinen neuen Diensttyp und wird niemals automatisch erfunden.

### F-03 – Bearbeitung der Personenzahl

Soll die Service-Leitung die benötigte Personenzahl sowohl einmalig für ein Datum als auch dauerhaft für einen Wochentag ändern können?

Antwort: ja

### F-04 – Keine automatische Überbesetzung

Bleibt verbindlich, dass die automatische Planung nie mehr Personen als den für diesen Tag und Dienst gespeicherten Bedarf einplant?

Antwort: ja

## G – Minimale Bedienung

### G-01 – Aufteilung der Verwaltung

Welche Aufteilung ist verständlicher?

- [ ] Getrennte Übersichten für Einsatzorte und Diensttypen.
- [X] Eine gemeinsame Seite: Einsatzort auswählen und zugehörige Dienste darunter bearbeiten.
- [ ] Kombination aus Gesamtübersicht und Detailseite je Einsatzort.
- [ ] Andere Aufteilung.

Antwort:

### G-02 – Sofort sichtbare Angaben

Welche Angaben sollen in der Einsatzortübersicht sofort sichtbar sein?

Antwort: Die genaue Feldauswahl wird erst im abgenommenen UI-Schritt festgelegt. Fest steht bereits, dass Einsatzortname und seine zusätzliche Farbkennzeichnung sichtbar sein müssen.

Welche Angaben sollen in der Diensttypübersicht sofort sichtbar sein?

Antwort: Die genaue Feldauswahl wird erst im abgenommenen UI-Schritt festgelegt. Fest steht bereits, dass Bezeichnung beziehungsweise Tabellenanzeige, Standardzeit und Einsatzort verständlich erkennbar sein müssen.

### G-03 – Benötigte Funktionen der ersten Fassung

Bitte markieren:

- [X] Anzeigen
- [ ] Anlegen
- [X] Bearbeiten
- [ ] Deaktivieren
- [ ] Reaktivieren
- [ ] Unbenutzte Fehleingabe löschen
- [ ] Suchen
- [ ] Filtern
- [ ] Eigene Sortierreihenfolge festlegen
- [ ] Andere Funktion

Antwort:

Für die erste Fassung werden nur Anzeigen und Bearbeiten umgesetzt. Das Datenmodell wird ohne fest programmierte Aufzählungen so vorbereitet, dass Anlegen, Löschen, Deaktivieren und Reaktivieren später ergänzt werden können.

## H – Abschluss der Fragenrunde

### H-01 – Weitere Einsatzorte oder Dienste

Fehlt ein derzeit verwendeter Einsatzort, Diensttyp oder Sonderdienst vollständig in dieser Datei?

Antwort: Nein.

### H-02 – Weitere betriebliche Besonderheiten

Gibt es zu Einsatzorten, Diensten, Pausen oder Belegung eine wichtige Ausnahme, nach der noch nicht gefragt wurde?

Antwort: Nein.

### H-03 – Fachliche Bestätigung

Dieser Punkt wird erst nach gemeinsamer Durchsicht ausgefüllt.

- [x] Die Antworten wurden mit der Service-Leitung abgestimmt.
- [x] Offene beziehungsweise noch unsichere Antworten sind ausdrücklich als solche markiert.
- [x] Die beantwortete Datei darf als Grundlage für die Aktualisierung der Roadmap und Leitdokumente verwendet werden.

Bestätigt am: 2026-09-13

Zusätzliche Anmerkung:

## Nächster Schritt nach der Beantwortung

Die Antworten werden zunächst auf Widersprüche und Systemgrenzen geprüft. Anschließend werden ausschließlich die betroffenen Grundlagen, die aktive Teil-Roadmap, `STATUS.md` und die verständliche Service-Leitungsdokumentation angepasst. Erst nach einer getrennten Abnahme dieser Entscheidungen kann der erste Fachcode-Schritt beginnen.
