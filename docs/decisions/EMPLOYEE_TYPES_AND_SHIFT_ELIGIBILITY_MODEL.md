# Entscheidung: Bindende Mitarbeitertypen und Einsatzfreigaben

Status: Fachlich bestätigt und mit MA-01 am 2026-09-14 ausdrücklich abgenommen

Stand: 2026-09-14

## Anlass

Die bisherige Grundlage sah getrennt kombinierbare Wochenstunden, Qualifikationen sowie Einsatzort- und Diensttypfreigaben je Mitarbeiter vor. Vordefinierte Profile waren nur als Eingabevorlagen vorgesehen.

Die Service-Leitung hat für den tatsächlichen Betrieb stattdessen acht gemeinsam verwendete Mitarbeitertypen beschrieben. Ein Typ bestimmt das Wochen-Soll und die Einsatzmöglichkeiten aller ihm zugeordneten Mitarbeitenden. Eine spätere Änderung desselben Typs soll unmittelbar für alle diese Mitarbeitenden gelten.

Zusätzlich besitzt `TypAH2` eine kontextabhängige Ausnahme: Frühdienst ist innerhalb des Doppeldienstes zulässig, als einzelner Dienst aber nur eine manuell zu bestätigende Lösungsmöglichkeit.

## Entscheidung

### Gemeinsame bindende Typdefinition

- Jeder Mitarbeiter verweist auf genau einen gemeinsam definierten Mitarbeitertyp.
- Ein Mitarbeitertyp besitzt eine stabile Kennung, einen stabilen sichtbaren Code, einen verständlichen Namen, ein ungekürztes Wochen-Soll in ganzen Minuten und strukturierte Einsatzfreigaben.
- Wochen-Soll und Freigaben bleiben innerhalb der Typdefinition fachlich getrennte Werte, werden aber nicht als unabhängige Kopien beim Mitarbeiter gespeichert.
- Eine Änderung einer Typdefinition wirkt auf alle aktuell zugeordneten Mitarbeitenden und auf nachfolgende Planungen.
- Eine individuelle Abweichung erfolgt durch die bewusste Zuordnung eines anderen oder später neu angelegten Typs.
- Bereits abgenommene Planversionen bleiben unverändert. Vorhandene Entwürfe werden nicht stillschweigend umgeschrieben und müssen später gegen die aktuelle Typdefinition geprüft werden.

Diese Entscheidung ersetzt die frühere Aussage, Mitarbeitertypen seien lediglich Vorlagen für frei kombinierbare Mitarbeiterwerte.

### Initiale Mitarbeitertypen

| Typ | Wochen-Soll | Einsatzmöglichkeiten |
|---|---:|---|
| `Typ1` | 40 Stunden | alle Dienste und Muster; keine automatische Verteilung |
| `Typ25` | 25 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ30` | 30 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ30a` | 30 Stunden | alle Dienste und Muster |
| `Typ35` | 35 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ35a` | 35 Stunden | alle Dienste und Muster |
| `TypAH1` | 10 Stunden | regulär Restaurant-Spätdienst; Frühdienst nur als manuelle Lösungsmöglichkeit |
| `TypAH2` | 10 Stunden | Restaurant-Spätdienst, Cafeteria-Dienst B und `D`; einzelner Frühdienst nur als manuelle Lösungsmöglichkeit; `Spr` optional |

Die acht Typen sind stabile Startwerte. Das Modell bleibt datengetrieben, damit Typen später angelegt, geändert und entfernt werden können. Die erste Bedienfassung enthält diese Katalogpflege noch nicht.

Ein Typ kann später nur entfernt werden, wenn ihm keine aktive oder deaktivierte Person mehr zugeordnet ist. Die Anwendung nennt die noch zugeordneten Personen; eine automatische Umstellung findet nicht statt.

### Mitarbeiterlebenszyklus

- Eine deaktivierte Person kann mit derselben stabilen Kennung, denselben Namen und demselben Mitarbeitertyp reaktiviert werden.
- Die Reaktivierung einer Typ1-Person wird zusammen mit der Speicherung atomar gegen die Grenze von höchstens einer aktiven Typ1-Person geprüft. Bei einem Konflikt bleibt die Person deaktiviert; ein automatischer Typwechsel findet nicht statt.
- Nur eine bereits deaktivierte Person kann endgültig gelöscht werden. Eine aktive Person muss zuerst bewusst deaktiviert werden.
- Endgültiges Löschen ist nur zulässig, solange die Person noch in keinem Plan, keiner Verfügbarkeit, keiner Abwesenheit, keinem Zeitkonto und keinen anderen Fachdaten verwendet wird. Andernfalls bleibt sie deaktiviert erhalten und die Anwendung erklärt den blockierenden Bezug verständlich.
- Vor dem endgültigen Löschen zeigt die Oberfläche den vollständigen Namen, eine deutliche Unwiderruflichkeitswarnung und die getrennten Aktionen „Abbrechen“ und „Endgültig löschen“. Das erneute Eintippen des Namens wird nicht verlangt.
- Eine gelöschte Mitarbeiterkennung wird nicht erneut vergeben. Historische Planmomentaufnahmen werden durch den Vorgang weder verändert noch gelöscht.

### Bezug zu System 04

- System 04 bleibt die einzige Quelle für Einsatzorte, normale Diensttypen sowie die Definitionen von `D` und `Spr`.
- System 03 referenziert ausschließlich die stabilen `WorkLocationId`-, `ShiftTypeId`- und erforderlichen `ShiftPatternId`-Werte.
- Bezeichnungen, Farben, Standardzeiten und Musterbestandteile werden nicht dupliziert.
- Mitarbeitertypen dürfen kontextabhängige Freigaben ausdrücken. Dadurch ist Frühdienst bei `TypAH2` innerhalb von `D` zulässig, ohne einen einzelnen Frühdienst automatisch zu erlauben.
- `TypAH2` wird für `Spr` nur berücksichtigt, wenn eine vor jedem Planungslauf angebotene, standardmäßig ausgeschaltete Option aktiviert ist. Die allgemeine `Spr`-Notfallbedingung bleibt bestehen.

Diese kontextabhängige Regel präzisiert und ersetzt die frühere System-04-Annahme, dass für `D` und `Spr` niemals eine mitarbeiterbezogene Musterfreigabe benötigt wird. Die Muster selbst und ihre fachliche Zusammensetzung bleiben unverändert Eigentum von System 04.

### Typ1

- Zielzustand sind genau eine aktive Typ1-Person und beliebig viele historisch deaktivierte frühere Typ1-Personen.
- Während der ersten Stammdateneinrichtung darf noch keine aktive Typ1-Person existieren, da keine reale Person vorbefüllt wird. Spätere Planung wird ohne genau eine aktive Typ1-Person blockiert.
- Vor jeder Generierung muss in jeder zu generierenden Woche mindestens ein Typ1-Dienst manuell vorgetragen sein.
- Alle Typ1-Zuweisungen sind ohne zusätzliche Bedienhandlung für eine Neugenerierung geschützt.
- Typ1 wird niemals automatisch eingeteilt.
- Typ1 kann für jeden weiterhin ungedeckten normalen Dienst als letzte manuell zu bestätigende Lösung genannt werden. Bei einem offenen Frühdienst werden zuerst AH-Möglichkeiten genannt.
- Ein Stundenwert außerhalb des normalen Korridors verändert die vorgetragenen Dienste nicht und wird deutlich gemeldet.
- Die Behandlung einer vollständig abwesenden Typ1-Person wird erst mit Verfügbarkeiten und Abwesenheiten festgelegt.

### Wochen-Soll und spätere Generierung

- Die Bewertung erfolgt für jede Person und jede Montag-bis-Sonntag-Woche einzeln.
- Für Typ25, Typ30, Typ30a, Typ35 und Typ35a ist minus drei bis plus drei Stunden um das Soll eine zwingende äußere Grenze.
- Bei Typ1 dient dieser Korridor nur der Bewertung und Meldung; vorgetragene Dienste außerhalb des Korridors bleiben zulässig und unverändert.
- Innerhalb aller zwingenden Grenzen wird zuerst ungedeckter Bedarf minimiert und danach die Sollabweichung minimiert.
- AH besitzt ein Soll von zehn Stunden und einen zwingenden Korridor von sieben bis zwölf Stunden.
- AH strebt zuerst zehn Stunden an. Oberhalb von zehn bis zwölf Stunden wird gegenüber unter zehn Stunden bevorzugt; weniger als zehn Stunden wird erst verwendet, wenn es notwendig ist.
- Mehr als zehn AH-Stunden erzeugen eine Meldung.
- Ein wegen Abwesenheit reduziertes Soll wird erst in System 06 fachlich berechnet und in System 07 als zentrale Regel definiert.
- Tatsächliche Zuweisungszeiten bestimmen die Stunden. Bei `D` zählt die Unterbrechung nicht; bei `Spr` zählt die bestätigte zusammenhängende Einsatzzeit.

### Bericht

Jedes spätere Planungsergebnis bewahrt je Person und Woche mindestens Typ, wirksames Soll, geplante Stunden, vorzeichenbehaftete Abweichung sowie AH- und Typ1-Hinweise als unveränderliche Momentaufnahme. Die Zusammenfassung wird zusätzlich direkt nach der Generierung angezeigt.

## Keine zusätzlichen Qualifikationen

Für die erste Fassung wird kein eigener Qualifikationskatalog benötigt. Die erforderlichen Einsatzmöglichkeiten werden durch die Mitarbeitertypen ausgedrückt. Zusätzliche Qualifikationen wären eine spätere, vor ihrer Umsetzung gesondert zu bestätigende Erweiterung.

## Auswirkungen auf die Systeme

- System 03 speichert Mitarbeiter, Typdefinitionen, Typzuordnung, Wochen-Soll und Einsatzfreigaben.
- System 04 bleibt Eigentümer der referenzierten Einsatzorte, Diensttypen und Muster.
- System 06 bestimmt die Reduktion des Wochen-Solls bei Abwesenheiten und die vollständig abwesende Typ1-Woche.
- System 07 definiert Korridore, AH-Rangfolge und Typ1-Verhalten als zentrale Regeln.
- System 08 bewahrt vorgetragene Typ1-Zuweisungen und die verwendete Typmomentaufnahme.
- System 09 setzt die Generierungslogik und die optionale AH2-Springerfreigabe um.
- System 10 erzeugt Lösungsvorschläge und den Wochenstundenbericht.
- Systeme 11 und 12 behandeln manuelle Sonderzuweisungen und unveränderliche Planversionen.

## Verbindliche Übergaben an spätere Systeme

| Übergabe aus System 03 | Verbindlicher Inhalt | Späterer Eigentümer |
|---|---|---|
| Ungekürztes Wochen-Soll | `WeeklyWorkTarget` bleibt ein positiver, minutengenauer Wert des gemeinsam referenzierten Mitarbeitertyps. | System 06 berechnet die Abwesenheitsreduktion; System 07 definiert den wirksamen Korridor. |
| Typ1-Richtlinie | Keine automatische Einteilung, mindestens eine manuelle Wochenzuweisung, Schutz manueller Zuweisungen und letzte Vorschlagspriorität sind strukturierte Eigenschaften der Typdefinition. | Systeme 07 bis 10 setzen Prüfung, Planung und Vorschlagsreihenfolge um. |
| AH-Freigaben | Reguläre, nur vorschlagsfähige und kontextabhängige Freigaben bleiben getrennte strukturierte Werte. Ein einzelner Frühdienst wird für AH nie automatisch freigegeben. | Systeme 07, 09 und 10 setzen Korridor, Generierung und Lösungsvorschläge um. |
| AH2-Springeroption | Die Freigabe für `Spr` trägt `ExplicitPlanningRunOption`; die konkrete Option ist pro Planungslauf standardmäßig ausgeschaltet. | System 09 definiert den Planungslaufeingang und wertet die Option aus. |
| Planungseingang | Mitarbeitende, Typdefinitionen, Wochen-Soll, Planungsrichtlinie und Einsatzfreigaben werden später als unveränderliche Momentaufnahme übergeben; sichtbare Typcodes sind keine Solver-Schalter. | System 08 definiert den vollständigen Planungseingang. |
| Wochenstundenbericht | Je Person und Montag-bis-Sonntag-Woche werden Typ, wirksames Soll, geplante Minuten, vorzeichenbehaftete Abweichung sowie AH- und Typ1-Hinweise unveränderlich bewahrt. | System 10 definiert Bericht und verständliche Darstellung; System 12 bewahrt ihn in Planversionen. |

Diese Übergaben sind Verträge für spätere Roadmaps und noch keine Freigabe, die zugehörige Planungs-, Abwesenheits-, Konflikt- oder Versionslogik in System 03 zu implementieren.

Wichtiger technischer Anker: `Application.Employees.EmployeeTypeSnapshot` und `EmployeeTypeEligibilitySnapshot.AvailabilityDisplay` dienen der aktuellen Mitarbeiteroberfläche. Sie sind kein vollständiger Planungseingang und dürfen später weder über sichtbare Typcodes noch über deutsche Anzeigetexte ausgewertet werden. System 08 definiert dafür einen eigenen unveränderlichen Planungsvertrag, der die strukturierte `EmployeeTypePlanningPolicy`, die Kennungen, das minutengenaue Wochen-Soll sowie Modus und Aktivierung jeder Einsatzfreigabe ausdrücklich übernimmt.

## Architekturfolgen

- `Domain.Employees` besitzt die neuen Fachtypen und referenziert die stabilen Domain-Kennungen aus System 04.
- Planning darf sichtbare Typcodes nicht als versteckte Schalter verwenden. Spätere Regeln erhalten strukturierte fachliche Eigenschaften und stabile Kennungen.
- Application validiert Katalogbezüge und die Grenze von höchstens einer aktiven Typ1-Person.
- Infrastructure führt die bestehende gemeinsame SQLite-Migrationsfolge fort und schreibt die veröffentlichte System-04-Migration nicht um.
- Desktop zeigt deutsche Bezeichnungen, enthält aber keine eigene Typ-, Freigabe- oder Planungslogik.

## Bewusst noch offen

- genaue Reduktionsformel des Wochen-Solls bei Abwesenheit,
- Verhalten der Typ1-Generierungsvoraussetzung bei einer vollständig abwesenden Woche,
- konkrete Pflegeoberfläche für neue oder zu entfernende Mitarbeitertypen,
- konkrete Darstellung des Wochenstundenberichts,
- konkrete UI-Anordnung der Mitarbeiterverwaltung.

Diese Punkte blockieren die aktuellen System-03-Schritte und die Mitarbeiterstammdaten nicht. Sie werden vor dem jeweils betroffenen späteren Schritt festgelegt und abgenommen.
