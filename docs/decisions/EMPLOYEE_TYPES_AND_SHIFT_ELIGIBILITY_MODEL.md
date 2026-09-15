# Entscheidung: Bindende Mitarbeitertypen und Einsatzfreigaben

Status: Fachlich bestätigt und mit MA-01 am 2026-09-14 ausdrücklich abgenommen; Erweiterung für System 06 am 2026-09-15 bestätigt

Stand: 2026-09-15

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
| `Typ20` | 20 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ20a` | 20 Stunden | alle Dienste und Muster |
| `Typ25` | 25 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ25a` | 25 Stunden | alle Dienste und Muster |
| `Typ30` | 30 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ30a` | 30 Stunden | alle Dienste und Muster |
| `Typ35` | 35 Stunden | Restaurant-Frühdienst, Restaurant-Spätdienst und `D` |
| `Typ35a` | 35 Stunden | alle Dienste und Muster |
| `TypAH1` | 10 Stunden | regulär Restaurant-Spätdienst; Frühdienst nur als manuelle Lösungsmöglichkeit |
| `TypAH2` | 10 Stunden | Restaurant-Spätdienst, Cafeteria-Dienst B und `D`; einzelner Frühdienst nur als manuelle Lösungsmöglichkeit; `Spr` optional |

Die elf Typen sind stabile Startwerte. Das Modell bleibt datengetrieben. Vor der System-06-Abwesenheitserfassung erhält die App eine eigene Mitarbeitertypenpflege für Anlegen, Bearbeiten und das sichere Entfernen noch nie verwendeter normaler Typen.

Ein Typ kann nur entfernt werden, wenn ihm keine aktive oder deaktivierte Person und keine andere Fachinformation zugeordnet ist. Die Anwendung nennt die blockierenden Bezüge; eine automatische Umstellung findet nicht statt. Die für Typ1 und AH benötigten Sondertypen bleiben erhalten, weil neue Typen nur mit normaler Planungsrolle angelegt werden können.

### Pflege der Mitarbeitertypen

- Beim Anlegen werden ein eindeutiger sichtbarer Code, ein verständlicher Name, Wochen-Soll, `U`-/`K`-Regel und Einsatzfreigaben festgelegt. Der Code bleibt anschließend stabil.
- Bearbeitbar sind Name, Wochen-Soll, Zulässigkeit und Tageswert für `U` und `K`, die Freigaben aller normalen Dienste sowie die Berechtigungen für `D` und `Spr`.
- Änderungen wirken für alle aktuell zugeordneten Mitarbeitenden und nachfolgende Planungen. Abgenommene Planversionen bleiben unverändert; vorhandene Entwürfe werden nicht stillschweigend umgeschrieben.
- Neu angelegte Typen sind normale automatisch planbare Typen mit einem zwingenden Wochenkorridor von minus drei bis plus drei Stunden.
- Die genannten Werte und Einsatzberechtigungen von `Typ1`, `TypAH1` und `TypAH2` sind ebenfalls bearbeitbar. Ihre besonderen Planungsrollen bleiben geschützt und können in der ersten Pflegeoberfläche weder entfernt noch neu vergeben werden.
- `Typ20`, `Typ20a` und `Typ25a` erhalten denselben normalen Wochenkorridor. Ohne `a` sind Frühdienst, Spätdienst und `D` erlaubt; mit `a` zusätzlich beide Cafeteria-Dienste und `Spr`.

### Abwesenheits-Tageswert

Der Typ enthält zusätzlich die strukturierte Zulässigkeit und den minutengenauen gemeinsamen Tageswert für `U` und `K`. Die bestätigten Startwerte lauten:

| Typen | Tageswert für `U` und `K` |
|---|---:|
| `Typ20`, `Typ20a` | 4 Stunden |
| `Typ25`, `Typ25a` | 5 Stunden |
| `Typ30`, `Typ30a` | 6 Stunden |
| `Typ35`, `Typ35a` | 7 Stunden |
| `Typ1` | 8 Stunden |
| `TypAH1`, `TypAH2` | nicht zulässig |

System 06 berechnet daraus je Montag-bis-Sonntag-Woche das wirksame Soll. Eine spätere Typänderung verändert keine bereits abgenommene Planversion.

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
- Ist Typ1 eine ganze Woche durch `U`, `K` oder rote `X` abwesend, darf ohne vorgetragenen Typ1-Dienst generiert werden und die App zeigt einen deutlichen Hinweis. Bei nur teilweiser Abwesenheit bleibt mindestens ein vorgetragener Typ1-Dienst je Woche erforderlich.
- Ein vorgetragener Früh- oder Spätdienst kann als Bürozeit `B` markiert werden. Seine tatsächliche Dauer zählt zu den Typ1-Wochenstunden, deckt aber keinen Personalbedarf. Nach der Planabnahme bleibt der zugrunde liegende Dienst sichtbar und nur `B` wird in Plan und Export ausgeblendet; die strukturierte Büroinformation bleibt in der unveränderlichen Planversion erhalten.

### Wochen-Soll und spätere Generierung

- Die Bewertung erfolgt für jede Person und jede Montag-bis-Sonntag-Woche einzeln.
- Für alle normalen Typen einschließlich Typ20, Typ20a und Typ25a ist minus drei bis plus drei Stunden um das Soll eine zwingende äußere Grenze.
- Bei Typ1 dient dieser Korridor nur der Bewertung und Meldung; vorgetragene Dienste außerhalb des Korridors bleiben zulässig und unverändert.
- Innerhalb aller zwingenden Grenzen wird zuerst ungedeckter Bedarf minimiert und danach die Sollabweichung minimiert.
- AH besitzt ein Soll von zehn Stunden und eine zwingende Obergrenze von zwölf Stunden.
- Zuerst werden alle automatisch planbaren Nicht-AH-Typen verteilt. AH wird danach nur für verbleibende zulässige Lücken verwendet und verdrängt keine bereits geplante Nicht-AH-Person.
- Unter sechs AH-Stunden bleibt ein Ergebnis zulässig und erzeugt eine Meldung. Mehr als zehn AH-Stunden erzeugen ebenfalls eine Meldung; mehr als zwölf Stunden bleiben unzulässig.
- Das wegen `U` oder `K` reduzierte Soll wird in System 06 je Woche als ungekürztes Soll minus der Summe der Typ-Tageswerte berechnet, mindestens null. Rote und schwarze `X` reduzieren es nicht.
- Tatsächliche Zuweisungszeiten bestimmen die Stunden. Bei `D` zählt die Unterbrechung nicht; bei `Spr` zählt die bestätigte zusammenhängende Einsatzzeit.

### Bericht

Jedes spätere Planungsergebnis bewahrt je Person und Woche mindestens Typ, wirksames Soll, geplante Stunden, vorzeichenbehaftete Abweichung sowie AH- und Typ1-Hinweise als unveränderliche Momentaufnahme. Die Zusammenfassung wird zusätzlich direkt nach der Generierung angezeigt.

## Keine zusätzlichen Qualifikationen

Für die erste Fassung wird kein eigener Qualifikationskatalog benötigt. Die erforderlichen Einsatzmöglichkeiten werden durch die Mitarbeitertypen ausgedrückt. Zusätzliche Qualifikationen wären eine spätere, vor ihrer Umsetzung gesondert zu bestätigende Erweiterung.

## Auswirkungen auf die Systeme

- Der Vorbereitungsteil vor System 06 erweitert System 03 um die Mitarbeitertypenpflege, die drei zusätzlichen Starttypen und den Abwesenheits-Tageswert.
- System 04 bleibt Eigentümer der referenzierten Einsatzorte, Diensttypen und Muster.
- System 06 speichert `U`, `K` und rote `X`, berechnet die bestätigte Sollreduktion und behandelt die vollständig abwesende Typ1-Woche.
- System 07 definiert Korridore, AH-Nachrang und Typ1-Verhalten als zentrale Regeln.
- System 08 bewahrt vorgetragene Typ1-Zuweisungen einschließlich Bürokennzeichnung und die verwendete Typmomentaufnahme.
- System 09 setzt die zweiphasige Nicht-AH-/AH-Generierung und die optionale AH2-Springerfreigabe um.
- System 10 erzeugt Lösungsvorschläge und den Wochenstundenbericht.
- Systeme 11 und 12 behandeln manuelle Sonderzuweisungen und unveränderliche Planversionen.

## Verbindliche Übergaben an spätere Systeme

| Übergabe aus System 03 | Verbindlicher Inhalt | Späterer Eigentümer |
|---|---|---|
| Ungekürztes Wochen-Soll und Abwesenheitswert | `WeeklyWorkTarget` bleibt ein positiver, minutengenauer Wert des gemeinsam referenzierten Mitarbeitertyps. Zulässigkeit und Tageswert für `U` und `K` sind getrennte strukturierte Typwerte. | System 06 berechnet die Abwesenheitsreduktion; System 07 definiert den wirksamen Korridor. |
| Typ1-Richtlinie | Keine automatische Einteilung, mindestens eine manuelle Wochenzuweisung, Schutz manueller Zuweisungen und letzte Vorschlagspriorität sind strukturierte Eigenschaften der Typdefinition. | Systeme 07 bis 10 setzen Prüfung, Planung und Vorschlagsreihenfolge um. |
| AH-Freigaben und Nachrang | Reguläre, nur vorschlagsfähige und kontextabhängige Freigaben bleiben getrennte strukturierte Werte. AH folgt erst nach der Nicht-AH-Planung und füllt nur verbleibende zulässige Lücken. | Systeme 07, 09 und 10 setzen Obergrenze, Berichte, Generierung und Lösungsvorschläge um. |
| Typ1-Bürozeit | Eine Bürokennzeichnung ist nur für vorgetragenen Früh- oder Spätdienst zulässig, zählt Stunden und deckt keinen Bedarf. | Systeme 08 bis 13 bewahren, prüfen, planen, zeigen und exportieren diese Information. |
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

- konkrete Darstellung des Wochenstundenberichts,
- genaue spätere Bedienung der Typ1-Dienste und Bürokennzeichnung in der gemeinsamen Planansicht,
- endgültige Excel-Zuordnung des ausgeblendeten `B` nach Charakterisierung der echten Vorlage.

Diese Punkte blockieren die aktuellen System-03-Schritte und die Mitarbeiterstammdaten nicht. Sie werden vor dem jeweils betroffenen späteren Schritt festgelegt und abgenommen.
