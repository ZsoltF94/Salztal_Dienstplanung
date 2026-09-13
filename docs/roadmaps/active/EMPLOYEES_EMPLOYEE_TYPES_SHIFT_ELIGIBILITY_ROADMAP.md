# Teil-Roadmap: Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben

Status: Aktive, abgenommene Teil-Roadmap – MA-02A in Bearbeitung

Stand: 2026-09-14

## Ziel und Nutzen

Diese Roadmap beschreibt System 03 der `MASTER_ROADMAP.md`. Am Ende sollen Mitarbeitende mit Vorname, Nachname, Aktivstatus und genau einem gemeinsam definierten Mitarbeitertyp erfasst, bearbeitet und lokal gespeichert werden können.

Ein Mitarbeitertyp ist eine bindende, gemeinsam referenzierte Fachdefinition. Er bündelt ein Wochen-Soll und die für diesen Typ geltenden Einsatzfreigaben. Ändert sich eine Typdefinition später, gilt die neue Definition für alle aktuell zugeordneten Mitarbeitenden und für nachfolgende Planungen. Mitarbeitende erhalten keine davon losgelösten Kopien dieser Werte.

System 03 legt die Mitarbeiter- und Typstammdaten sicher an. Die automatische Verwendung der Typen, die Wochenstundenoptimierung, vorgetragene Typ1-Zuweisungen, AH-Notfallvorschläge und der Abweichungsbericht werden erst in den dafür vorgesehenen späteren Systemen umgesetzt.

## Verbindliche Grundlagen

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`
- `ARCHITECTURE.md`
- `CLEANCODE.md`
- `AGENTS.md` mit dem verbindlichen kleinschrittigen Abnahmeprozess
- `MASTER_ROADMAP.md`, System 03 – Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben
- `docs/decisions/SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- die vollständig beantwortete Fragen-Datei `EMPLOYEES_EMPLOYEE_TYPES_SHIFT_ELIGIBILITY_QUESTIONS.md`
- System 02 – Technisches App-Grundgerüst, abgeschlossen und archiviert am 2026-09-13
- System 04 – Einsatzorte, Diensttypen und Doppeldienste, abgeschlossen und archiviert am 2026-09-13
- ausschließlich synthetische Mitarbeiterdaten in Quellcode, Tests, Dokumentation und Screenshots

## Bestätigtes Fachmodell

### Mitarbeiter

- Jeder Mitarbeiter besitzt eine stabile, unsichtbare `EmployeeId`.
- Vorname und Nachname werden getrennt gespeichert und angezeigt.
- Gleiche menschliche Namen sind zulässig; Namen sind keine technischen Kennungen.
- Eine betriebliche Personalnummer wird in der ersten Fassung nicht benötigt.
- Jeder Mitarbeiter verweist auf genau einen Mitarbeitertyp.
- Mitarbeitende können deaktiviert werden. Historische Planmomentaufnahmen bleiben später unverändert.
- Ein endgültiges Löschen ist höchstens vor der ersten späteren Verwendung zulässig und gehört noch nicht zur ersten Bedienfassung.
- Übersicht, Anlegen und Bearbeiten einschließlich Typwechsel und Deaktivierung gehören zur ersten Fassung. Suche, Filter und freie Sortierung gehören nicht dazu.

### Bindende Mitarbeitertypen

- Mitarbeitertypen sind gemeinsam referenzierte, bindende Fachobjekte und keine einmalig kopierten Eingabevorlagen.
- Ein Typ besitzt eine stabile `EmployeeTypeId`, einen stabilen sichtbaren Code, einen verständlichen Namen, ein Wochen-Soll in ganzen Minuten und ausdrückliche Einsatzfreigaben.
- Mitarbeitende speichern nur die Typzuordnung. Wochen-Soll und Freigaben werden nicht zusätzlich als unabhängige Mitarbeiterwerte dupliziert.
- Eine spätere Änderung einer Typdefinition wirkt sofort auf die aktuellen Stammdaten aller diesem Typ zugeordneten Mitarbeitenden und auf nachfolgende Generierungen.
- Bereits abgenommene Planversionen bleiben unveränderliche Momentaufnahmen.
- Ein bestehender Planentwurf wird durch eine Typänderung nicht stillschweigend umgeschrieben; eine spätere Planprüfung muss die geänderte Grundlage sichtbar machen.
- Individuelle Abweichungen werden durch die bewusste Zuordnung eines anderen beziehungsweise später neu angelegten Typs abgebildet.
- System 03 startet mit acht festen Typdefinitionen. Das Modell bleibt datengetrieben und bereitet späteres Anlegen, Bearbeiten und Entfernen vor; diese Katalogpflege wird noch nicht bedient.
- Ein verwendeter Mitarbeitertyp kann später erst entfernt werden, nachdem alle zugeordneten aktiven und deaktivierten Mitarbeitenden bewusst einem anderen Typ zugeordnet wurden. Eine Meldung nennt die betroffenen Mitarbeitenden.

### Initialer Typenkatalog

| Typ | Wochen-Soll | Regulär zulässige Dienste und Muster | Besondere Planungsbedeutung |
|---|---:|---|---|
| `Typ1` | 40 Stunden | alle vier normalen Diensttypen, `D` und `Spr` | genau eine aktive Service-Leitung; nur manuell vorgetragen, niemals automatisch verteilt; letzte manuelle Lösungsmöglichkeit |
| `Typ25` | 25 Stunden | Frühdienst, Spätdienst und `D` im Restaurant | keine zusätzliche Besonderheit |
| `Typ30` | 30 Stunden | Frühdienst, Spätdienst und `D` im Restaurant | keine zusätzliche Besonderheit |
| `Typ30a` | 30 Stunden | alle vier normalen Diensttypen, `D` und `Spr` | keine zusätzliche Besonderheit |
| `Typ35` | 35 Stunden | Frühdienst, Spätdienst und `D` im Restaurant | keine zusätzliche Besonderheit |
| `Typ35a` | 35 Stunden | alle vier normalen Diensttypen, `D` und `Spr` | keine zusätzliche Besonderheit |
| `TypAH1` | 10 Stunden | regulär nur Spätdienst im Restaurant | Frühdienst nur als manuelle Lösung bei offener Besetzung vorschlagen |
| `TypAH2` | 10 Stunden | Spätdienst im Restaurant, Cafeteria-Dienst B und `D` | einzelner Frühdienst nur als manuelle Lösung; `Spr` nur bei ausdrücklicher Planungslaufoption |

Der sichtbare stabile Code bleibt erhalten. Zusätzlich erhält jeder Typ einen verständlichen Namen beziehungsweise eine kurze verständliche Beschreibung. Diese kann später pflegbar gemacht werden, ohne die technische Kennung zu verändern.

### Einsatzfreigaben und System-04-Grenze

- System 04 bleibt die einzige Quelle für Einsatzorte, normale Diensttypen und Einsatzmuster.
- Mitarbeitertypen referenzieren ausschließlich vorhandene `WorkLocationId`-, `ShiftTypeId`- und, wo eine kontextabhängige Musterfreigabe erforderlich ist, `ShiftPatternId`-Werte.
- Namen, Farben, Standardzeiten und die fachliche Zusammensetzung von `D` und `Spr` werden nicht in System 03 dupliziert.
- Eine Freigabe für Cafeteria-Dienst A erzeugt keine automatische Freigabe für Cafeteria-Dienst B.
- Freigaben für `D` und `Spr` werden je Mitarbeitertyp ausdrücklich festgelegt und nicht allein aus den beteiligten Einzeldiensten abgeleitet. `Typ25`, `Typ30` und `Typ35` dürfen `D`, aber nicht `Spr`; `Typ30a`, `Typ35a` und `Typ1` dürfen beide Muster.
- `TypAH2` benötigt eine ausdrückliche kontextabhängige Ausnahme: Frühdienst ist innerhalb von `D` regulär zulässig, als einzelner Dienst aber niemals automatisch.
- `TypAH2` darf nur dann als `Spr` berücksichtigt werden, wenn die Service-Leitung vor dem jeweiligen Planungslauf die standardmäßig ausgeschaltete Option aktiviert. Die allgemeine `Spr`-Notfallbedingung bleibt zusätzlich bestehen.
- System 03 speichert die fachlich notwendige Typ- und Freigabestruktur. Die Auswertung im Solver und die Planungslaufoption gehören nicht zu System 03.

### Typ1 als Service-Leitung

- Es darf höchstens eine aktive Typ1-Person in den Stammdaten geben. Zielzustand vor einer Planung ist genau eine aktive Typ1-Person.
- Da das Repository und die Migration keine reale Person vorbefüllen dürfen, ist während der erstmaligen Stammdateneingabe vorübergehend keine Typ1-Person möglich. Eine spätere Generierung muss den fehlenden Zielzustand blockieren.
- Vor jeder Generierung muss in jeder zu generierenden Woche mindestens ein Dienst für Typ1 manuell vorgetragen sein.
- Urlaub, Krankheit und sonstige Abwesenheiten werden später ebenfalls im Plan sichtbar eingetragen. Die genaue Ausnahme für eine vollständig abwesende Typ1-Woche wird mit System 06 und System 07 festgelegt.
- Typ1 bleibt vollständig außerhalb der automatischen Verteilung.
- Alle vorhandenen Typ1-Zuweisungen wirken ohne zusätzliche Einzelaktion wie gesperrte Zuweisungen und werden bei einer Neugenerierung weder verändert noch entfernt.
- Typ1 darf bei weiterhin ungedecktem Bedarf für jeden normalen Dienst als letzte manuelle Lösung vorgeschlagen werden, wird aber niemals automatisch eingetragen.
- Bei einem offenen Frühdienst werden zunächst die besonderen AH-Möglichkeiten genannt; Typ1 folgt erst danach.
- Eine angenommene zusätzliche Typ1-Zuweisung bleibt sichtbar und bei späteren Generierungen geschützt.
- Liegen die vorgetragenen Typ1-Stunden außerhalb des normalen Korridors, darf die Generierung fortfahren, verändert Typ1 nicht und meldet die Abweichung deutlich.

### Wochen-Soll und spätere Optimierungsregel

- Das ungekürzte Wochen-Soll wird im Mitarbeitertyp in ganzen Minuten gespeichert.
- Die Wochenregel gilt je Mitarbeiter und je einzelner Woche von Montag bis Sonntag. Mehrere gemeinsam geplante Wochen werden nicht miteinander verrechnet.
- Für Typ25, Typ30, Typ30a, Typ35 und Typ35a ist der äußere Korridor von minus drei bis plus drei Stunden eine zwingende Grenze der späteren automatischen Planung.
- Für Typ1 dient derselbe Korridor nur der Bewertung und Meldung. Vorgetragene Typ1-Dienste außerhalb des Korridors bleiben zulässig und unverändert.
- Innerhalb des erlaubten Korridors wird zunächst ungedeckter Bedarf minimiert und danach die absolute Abweichung vom Wochen-Soll möglichst klein gehalten.
- Für TypAH1 und TypAH2 gilt ein besonderer Korridor von mindestens sieben bis höchstens zwölf Stunden.
- Für AH wird zuerst möglichst genau zehn Stunden angestrebt. Wenn zehn Stunden nicht erreichbar sind, wird eine Lösung über zehn bis höchstens zwölf Stunden gegenüber einer Lösung unter zehn Stunden bevorzugt. Erst wenn das nicht sinnvoll möglich ist, sind sieben bis unter zehn Stunden zulässig.
- Jede AH-Planung über zehn Stunden erzeugt eine sichtbare Meldung.
- Bei Abwesenheiten wird das wirksame Wochen-Soll später reduziert. System 03 speichert nur das ungekürzte Soll; die Berechnungsformel wird erst mit System 06 festgelegt und in System 07 als zentrale Regel definiert.
- Stunden folgen den tatsächlichen Zeiten der geplanten Zuweisungen. Bei `D` zählen Früh- und Spätdienst ohne Unterbrechung; bei `Spr` zählt die bestätigte tatsächliche zusammenhängende Einsatzzeit.

### Späterer Stundenbericht

Nach jeder Generierung wird für jede Person und jede einzelne Woche ein kurzer Bericht erzeugt. Er enthält mindestens:

- Mitarbeitertyp,
- ungekürztes beziehungsweise später wirksames Wochen-Soll,
- geplante Stunden,
- vorzeichenbehaftete Abweichung,
- AH-Warnung bei mehr als zehn Stunden,
- Hinweis auf eine unverändert gebliebene Typ1-Abweichung.

Der Bericht wird als unveränderlicher Bestandteil des jeweiligen Planungsergebnisses gespeichert und zusätzlich unmittelbar nach der Generierung angezeigt. System 03 bereitet nur die dafür benötigten stabilen Typ- und Sollwerte vor.

### Keine zusätzlichen Qualifikationen

- Für die erste Fassung existiert kein zusätzlicher Qualifikationskatalog.
- Die fachlich benötigten Einsatzmöglichkeiten werden vollständig über die bindenden Mitarbeitertypen und deren Einsatzfreigaben abgebildet.
- Dokumente, Nachweise, Qualifikationsstufen oder Ablaufdaten gehören nicht zu System 03.
- Falls später echte zusätzliche Qualifikationen benötigt werden, entsteht dafür vor der Umsetzung eine eigene bestätigte Erweiterung.

## Technischer Ausgangspunkt

- Das technische Grundgerüst mit sechs Produktionsprojekten und sieben Testprojekten ist vorhanden.
- System 04 stellt stabile `WorkLocationId`-, `ShiftTypeId`- und `ShiftPatternId`-Werte bereit.
- Domain und Application enthalten den abgenommenen Einsatzort-, Diensttyp- und Einsatzmusterkatalog. Mitarbeiterfunktionen existieren noch nicht.
- Infrastructure enthält die abgenommene gemeinsame SQLite-Datenbank, den bestehenden `ServiceCatalogDbContext` und die erste veröffentlichte Migration aus System 04. Mitarbeiterdaten sind nicht enthalten.
- Desktop enthält die abgenommene Einsatzort- und Diensttypverwaltung. Eine Mitarbeiteransicht existiert nicht.
- Planning und Excel bleiben in System 03 fachlich unverändert.
- System 04 ist abgeschlossen. Veraltete offene Gates aus dem früheren System-03-Entwurf sind entfernt.

## Umfang dieser Roadmap

- stabile Mitarbeiteridentität, getrennte Namen und Aktivstatus
- bindende Mitarbeitertypen mit stabilem Code, verständlichem Namen und Wochen-Soll
- acht datengetriebene Starttypen
- Typzuordnung mit höchstens einer aktiven Typ1-Person
- reguläre, kontextabhängige und nur vorschlagsfähige Einsatzfreigaben als Fachinformation
- lesbarer Typenkatalog für die Mitarbeiterbedienung
- Mitarbeiterübersicht sowie Anlegen, Bearbeiten, Typwechsel und Deaktivieren
- notwendige Application-Queries, Commands und unveränderliche Ein- und Ausgabeverträge
- eine koordinierte Fortführung der bestehenden SQLite-Migrationsfolge
- echte temporäre SQLite-Tests mit synthetischen Personen
- deutsche WPF-Bedienung für den bestätigten Umfang
- passende Domain-, Application-, Infrastructure-, Desktop- und Architekturprüfungen
- wahrheitsgemäße Aktualisierung von Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung`

## Nicht Bestandteil dieser Roadmap

- echte Namen, Personaldaten oder Mitarbeiterlisten
- freie Pflege, Anlegen oder Entfernen von Mitarbeitertypen in der Oberfläche
- individuelle Abweichung von Wochen-Soll oder Einsatzfreigaben innerhalb eines Mitarbeitertyps
- zusätzliche Qualifikationen oder ein Qualifikationskatalog
- Verfügbarkeiten, Urlaub, Krankheit, Fortbildung, Wunschfrei oder sonstige Abwesenheiten
- Berechnung eines wegen Abwesenheit reduzierten Wochen-Solls
- manuelles Vortragen von Typ1-Diensten in einem Plan
- automatische Planerzeugung, OR-Tools-Modellierung oder Planungslaufoptionen
- automatische Auswertung der Wochenstundenkorridore
- AH- oder Typ1-Lösungsvorschläge und deren manuelle Übernahme
- Generierungs- und Stundenberichte
- Personal- und Stundenbedarf
- Zeitkonten, Überstunden oder Minusstunden
- Planansichten, Planversionen, Excel-Export oder Sicherung und Wiederherstellung
- Benutzerkonten, Rollen, Cloud-Synchronisierung oder Netzwerkzugriffe
- Import aus Excel oder anderen Personalsystemen
- Commit, Push oder externe Veröffentlichung

## Architekturgrenzen

### Domain

- `Salztal.Dienstplanung.Domain.Employees` besitzt Mitarbeiter, Mitarbeitertypen, Kennungen, Namen, Wochen-Soll und Einsatzfreigaben.
- `Employee` referenziert genau eine `EmployeeTypeId`; er kopiert Wochen-Soll oder Freigaben nicht.
- Mitarbeitertypen referenzieren vorhandene stark typisierte Kennungen aus System 04 und duplizieren keine Katalogdaten.
- Kontextabhängige Freigaben werden als fachliche Werte modelliert und nicht durch Abfragen nach Typcodes oder deutsche Namen ersetzt.
- Fachobjekte sind nach erfolgreicher Erzeugung gültig; Benutzerfehler liefern strukturierte Validierungsergebnisse.
- Domain besitzt keine WPF-, EF-Core-, SQLite-, OR-Tools- oder Excel-Abhängigkeit.

### Application

- `Salztal.Dienstplanung.Application.Employees` koordiniert getrennte Queries und Commands.
- Application prüft referenzierte Typ-, Einsatzort-, Diensttyp- und Musterkennungen gegen konsistente Katalogmomentaufnahmen.
- Der schreibende Ablauf sichert, dass höchstens eine aktive Person Typ1 besitzt.
- Ein fehlender aktiver Typ1 darf während der Ersteinrichtung als sichtbarer unvollständiger Stammdatenzustand bestehen; spätere Planung erhält eine eindeutige Sperrursache.
- Speicherverträge werden nach Anwendungsfällen benannt. Eine generische CRUD-Schnittstelle wird nicht eingeführt.
- Ein- und Ausgaben sind unveränderliche Momentaufnahmen und enthalten keine EF-Core- oder WPF-Typen.
- Application verwendet ausschließlich Domain.

### Infrastructure

- EF-Core-, SQLite-, `DbContext`- und Migrationscode bleibt in `Salztal.Dienstplanung.Infrastructure.Persistence`.
- System 03 setzt die bereits veröffentlichte Migration aus System 04 unverändert fort.
- Vor einer Mitarbeitermigration wird in einem eigenen Schritt nachgewiesen, wie der bisher eng benannte `ServiceCatalogDbContext` ohne zweite unkoordinierte Migrationshistorie sicher erweitert oder abgelöst werden kann.
- Es gibt weiterhin genau eine koordinierte Migrationsfolge für die gemeinsame Datenbankdatei.
- Fremdschlüssel schützen Typzuordnungen und Referenzen auf die vorhandenen System-04-Katalogtabellen.
- Das Entfernen eines verwendeten Typs wird durch Anwendungslogik und referenzielle Integrität verhindert.
- `IQueryable` und veränderliche Persistenzobjekte verlassen Infrastructure nicht.
- Tests verwenden ausschließlich temporäre synthetische SQLite-Datenbanken.

### Desktop

- Mitarbeiteransichten und ViewModels liegen unter `Salztal.Dienstplanung.Desktop.Features.Employees`.
- Die neue Bedienung wird als getrenntes Feature an die bestehende Oberfläche angebunden und nicht in das `ServiceCatalogViewModel` hineingemischt.
- ViewModels verwenden nur Application-Verträge und UI-eigene Typen.
- Nur `Salztal.Dienstplanung.Desktop.Composition` erzeugt und verdrahtet die konkrete SQLite-Implementierung.
- Fachlogik und Datenbankzugriff werden nicht in XAML, Code-behind oder ViewModels dupliziert.

### Unberührte Module

- Planning und Excel werden in System 03 nicht fachlich erweitert.
- Neue Pakete sind nicht vorgesehen.
- OR-Tools-, Excel- und Planungsberichte bleiben außerhalb dieses Systems.

## Verbindliche Code-Anker

| Anker | Bedeutung für System 03 |
|---|---|
| `src/Salztal.Dienstplanung.Domain/WorkLocations/WorkLocationId.cs` | einzige stark typisierte Einsatzortkennung |
| `src/Salztal.Dienstplanung.Domain/WorkLocations/InitialWorkLocationCatalog.cs` | stabile Startkennungen für Cafeteria und Restaurant |
| `src/Salztal.Dienstplanung.Domain/ShiftTypes/ShiftTypeId.cs` | einzige stark typisierte Kennung normaler Diensttypen |
| `src/Salztal.Dienstplanung.Domain/ShiftTypes/InitialShiftTypeCatalog.cs` | stabile Kennungen der vier normalen Diensttypen |
| `src/Salztal.Dienstplanung.Domain/ShiftPatterns/ShiftPatternId.cs` | einzige stark typisierte Kennung zusammengesetzter Einsatzmuster |
| `src/Salztal.Dienstplanung.Domain/ShiftPatterns/InitialShiftPatternCatalog.cs` | stabile Definitionen von `D` und `Spr` |
| `src/Salztal.Dienstplanung.Application/ServiceCatalog/IServiceCatalogReader.cs` | konsistente fachliche Katalogquelle für Validierung |
| `src/Salztal.Dienstplanung.Application/ServiceCatalog/ServiceCatalogData.cs` | unveränderliche Domain-Momentaufnahme des Katalogs |
| `src/Salztal.Dienstplanung.Application/ServiceCatalog/GetServiceCatalogQuery.cs` | vorhandene UI-Momentaufnahme für lesbare Auswahlwerte |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContext.cs` | bestehende EF- und Migrationsgrenze, vor Schemaänderung zu charakterisieren |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContextFactory.cs` | bestehende SQLite-Konfiguration der gemeinsamen Datenbank |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations/` | unverändert fortzuführende veröffentlichte Migrationsfolge |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/SqliteServiceCatalogStore.cs` | bestehender Katalogadapter; Mitarbeiteradapter bleiben verantwortlich getrennt |
| `src/Salztal.Dienstplanung.Desktop/Composition/ServiceCatalogComposition.cs` | aktueller Composition-Anker, nicht Ort neuer Featurelogik |
| `src/Salztal.Dienstplanung.Desktop/App.xaml.cs` | vorhandener Startablauf und äußerste Fehlergrenze |
| `tests/Salztal.Dienstplanung.Architecture.Tests/ProductionProjectBoundaryTests.cs` | bestehende Produktionsprojektgrenzen |
| `tests/Salztal.Dienstplanung.Architecture.Tests/DesktopCompositionBoundaryTests.cs` | Schutz der einzigen konkreten Adapterverdrahtung |

Die Roadmap legt bewusst keine neuen endgültigen Typnamen fest, bevor der jeweilige Implementierungsschritt freigegeben ist.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Schritte

### MA-01 – Teil-Roadmap und geänderte Systemgrenzen überarbeiten

Status: `[x]` – am 2026-09-14 ausdrücklich abgenommen

Geplantes Ergebnis:

- Ziel, Fachmodell, Umfang, Nicht-Umfang, Architekturgrenzen, Code-Anker, Schritte und echte Gates von System 03 sind festgehalten.
- Die frühere Vorlage aus Arbeitszeitmodell und Qualifikationen ist durch gemeinsam referenzierte bindende Mitarbeitertypen ersetzt.
- Die kontextabhängige AH2-Doppeldienstfreigabe und alle Übergaben an spätere Planungssysteme sind dokumentiert.
- Roadmap und Fragen-Datei sind passend zum neuen Systemnamen umbenannt.
- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, `ARCHITECTURE.md`, `CLEANCODE.md`, `MASTER_ROADMAP.md`, `STATUS.md`, die betroffenen Entscheidungen und `Service-Leitung` sind widerspruchsfrei nachgeführt.
- Es werden noch keine Fachtypen, Datenbankobjekte oder Bedienoberflächen angelegt.

Prüfung:

- Der Entwurf bildet alle beantworteten Fragen ohne erfundene Regeln ab.
- Frühere Aussagen zu frei kombinierbaren Mitarbeiterwerten und fehlenden Musterfreigaben sind nachvollziehbar ersetzt oder als historisch überholt gekennzeichnet.
- Jeder spätere Schritt besitzt ein kleines, getrennt prüfbares Ergebnis.
- Alle Code-Anker existieren; keine reale Person oder realer Plan ist enthalten.
- Lokale Markdown-Verweise, Statusangaben, veraltete Pfade und `git diff --check` sind fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt den überarbeiteten Roadmap- und Entscheidungsentwurf oder nennt Änderungswünsche.

### MA-02 – Mitarbeitertyp-Grundwerte fachlich modellieren

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-14 ausdrücklich abgenommen

Geplantes Ergebnis:

- Stabile Mitarbeitertypkennung, sichtbarer Code, verständlicher Name und Wochen-Soll in ganzen Minuten sind als gültige Domain-Werte umgesetzt.
- Gemeinsam referenzierte Mitarbeitertypen sind unveränderliche Fachobjekte.
- Ungültige Kennungen, Codes, Namen und Sollwerte können kein gültiges Objekt erzeugen.
- Das Wochen-Soll muss positiv sein und darf die mathematische Länge einer vollständigen Woche von 10.080 Minuten nicht überschreiten; daraus wird bewusst keine arbeitsrechtliche Höchstgrenze abgeleitet.
- Es existiert noch kein Startkatalog, Mitarbeiterobjekt, Speicheradapter oder WPF-Code.

Prüfung:

- Domain-Tests decken gültige Werte, leere Eingaben, Grenzen, Minutengenauigkeit und Gleichheit ab.
- Wochenstunden verwenden weder `double` noch `float`.
- Domain bleibt frei von technischen Abhängigkeiten.
- Vollständiger Build und betroffene Architekturtests bestehen.

Automatischer Nachweis vom 2026-09-14:

- vollständiger Build: 13 von 13 Projekten, 0 Warnungen, 0 Fehler,
- Domain-Tests: 63 von 63 bestanden,
- Architekturtests: 13 von 13 bestanden.

Abnahmebedingung:

- Bedeutung und Invarianten der Mitarbeitertyp-Grundwerte werden bestätigt.

### MA-02A – Repository-Zeilenenden konsolidieren

Status: `[~]` – umgesetzt und automatisch geprüft; wartet auf Abnahme

Geplantes Ergebnis:

- `.gitattributes` macht die bereits in `.editorconfig` festgelegten CRLF-Zeilenenden für alle relevanten Repository-Textdateien unabhängig von der lokalen Git-Konfiguration verbindlich.
- Bestehende versionierte und neue Textdateien werden einmalig auf die festgelegten Zeilenenden normalisiert.
- Shell-Skripte bleiben für eine spätere plattformübergreifende Nutzung ausdrücklich auf LF festgelegt.
- Fachlogik, Daten, Projektabhängigkeiten und Roadmap-Umfang ändern sich nicht.

Prüfung:

- Ein vor und nach der Normalisierung berechneter, zeilenendenunabhängiger Inhaltsfingerabdruck bleibt identisch.
- Git meldet für die geregelten Dateien keine gemischten oder abweichenden Arbeitsbaum-Zeilenenden.
- `dotnet format Salztal.Dienstplanung.sln --no-restore --verify-no-changes` besteht repositoryweit.
- `git diff --check`, vollständiger Build und vollständiger automatischer Testlauf bestehen.

Abnahmebedingung:

- Die repositoryweit reproduzierbare Zeilenendenregel und ihre rein formale Normalisierung werden bestätigt.

Automatischer Nachweis vom 2026-09-14:

- 169 geregelte Textdateien geprüft; 128 Dateien benötigten die einmalige Zeilenenden-Normalisierung,
- identischer zeilenendenunabhängiger SHA-256-Inhaltsfingerabdruck vor und nach der Normalisierung,
- repositoryweite `dotnet format`-Prüfung bestanden; dabei wurde zusätzlich eine bereits vorhandene `using`-Reihenfolge rein formatierend korrigiert,
- vollständiger Build: 13 von 13 Projekten, 0 Warnungen, 0 Fehler,
- vollständiger Testlauf: 109 von 109 Tests bestanden,
- `git diff --check` bestanden.

### MA-03 – Einsatzfreigaben und initialen Typenkatalog modellieren

Status: `[ ]`

Geplantes Ergebnis:

- Reguläre, kontextabhängige und nur vorschlagsfähige Freigaben sind in Domain ohne Abfragen nach sichtbaren Codes modelliert.
- Die acht bestätigten Starttypen referenzieren ausschließlich die stabilen System-04-Kennungen.
- `TypAH2` bildet `D`, den nur vorgeschlagenen einzelnen Frühdienst und die spätere optionale `Spr`-Teilnahme widerspruchsfrei ab.
- Typ1 trägt seine besondere spätere Planungsbedeutung als strukturierte Fachinformation, ohne Planungslogik auszuführen.

Prüfung:

- Domain-Tests decken die vollständige bestätigte Typenmatrix ab.
- Tests beweisen, dass Namen, Farben, Standardzeiten sowie die Zusammensetzung von `D` und `Spr` nicht dupliziert werden.
- Unbekannte oder formal ungültige Katalogkennungen werden abgelehnt.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Typenmatrix, Freigabekontexte und System-04-Grenze werden bestätigt.

### MA-04 – Mitarbeiteridentität und Lebenszyklus modellieren

Status: `[ ]`

Geplantes Ergebnis:

- Stabile Mitarbeiterkennung, Vorname, Nachname, Aktivstatus und genau eine Typzuordnung sind in Domain umgesetzt.
- Typwechsel und Deaktivierung erzeugen neue gültige Fassungen und bewahren die Mitarbeiterkennung.
- Wochen-Soll und Freigaben werden nicht in `Employee` kopiert.
- Domain bildet die lokale Mitarbeiterinvariante ab; die katalogübergreifende Typ1-Eindeutigkeit bleibt Aufgabe des Application-Ablaufs.

Prüfung:

- Domain-Tests decken gültige und ungültige Namen, gleiche Anzeigenamen, Kennungsgleichheit, Typwechsel und Deaktivierung ab.
- Tests belegen, dass genau eine Typreferenz vorhanden ist und keine unabhängigen Stunden- oder Freigabekopien entstehen.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Mitarbeiteridentität, Namensdarstellung und Lebenszyklus werden bestätigt.

### MA-05 – Leseanwendungsfälle und unveränderliche Momentaufnahmen umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Application liefert getrennte Abfragen für Mitarbeiterübersicht, Mitarbeiterdetails und den lesbaren Mitarbeitertypkatalog.
- Momentaufnahmen enthalten die für die UI benötigten deutschen Anzeigewerte, aber keine EF-Core- oder WPF-Typen.
- Typwerte werden aktuell aus der gemeinsam referenzierten Typdefinition aufgelöst.
- Leerer Zustand und Abbruch sind ausdrücklich behandelt.

Prüfung:

- Application-Tests prüfen gefüllte und leere Kataloge, aktuelle Typauflösung, gleiche Namen und Abbruch.
- Collections sind nur lesbare Momentaufnahmen.
- Application verwendet nur Domain.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Abfragen, Momentaufnahmen und sichtbare Angaben werden bestätigt.

### MA-06 – Schreibanwendungsfälle und Typ1-Grenze umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Getrennte Commands ermöglichen Anlegen, Bearbeiten, Typwechsel und Deaktivierung im bestätigten Umfang.
- Referenzierte Mitarbeitertypen und deren Katalogbezüge werden vor dem Speichern geprüft.
- Höchstens eine aktive Typ1-Person wird transaktional abgesichert.
- Ein fehlender aktiver Typ1 wird als noch unvollständiger Einrichtungszustand sichtbar, aber System 03 benötigt noch keinen Planungsstart.
- Validierungs-, Nicht-gefunden-, Typ1-Konflikt- und Änderungskonflikte besitzen stabile Codes und verständliche deutsche Meldungen.

Prüfung:

- Application-Tests prüfen Erfolg, ungültige Eingaben, unbekannte Typen, zweite aktive Typ1-Person, Typwechsel, Deaktivierung, Konflikt und Abbruch.
- Speicherverträge sind anwendungsfallbezogen und keine generische CRUD-Schnittstelle.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Schreibabläufe und die Typ1-Grenze werden bestätigt.

### MA-07 – Gemeinsame Persistenz- und Migrationsgrenze absichern

Status: `[ ]`

Geplantes Ergebnis:

- Die bestehende `ServiceCatalogDbContext`- und Migrationsstruktur ist durch einen Charakterisierungstest gegen eine Erweiterung um Mitarbeiterdaten geprüft.
- Die kleinste sichere Fortführung als eine gemeinsame Migrationsfolge ist dokumentiert und umgesetzt, ohne die veröffentlichte System-04-Migration umzuschreiben.
- Ein zweiter unkoordinierter Migrationsverlauf für dieselbe Datenbank ist ausgeschlossen.
- Es entstehen in diesem Schritt noch keine Mitarbeiter-Tabellen.

Prüfung:

- Eine leere temporäre Datenbank migriert weiterhin exakt über die veröffentlichte System-04-Migration.
- Eine bereits auf System-04-Stand befindliche temporäre Datenbank wird erkannt und bleibt unverändert lesbar.
- Architektur- und Infrastructure-Tests sichern die gewählte Kontextgrenze.
- Vollständiger Build besteht ohne neue Warnungen.

Abnahmebedingung:

- Die technische Migrationsfortführung wird vor der ersten Mitarbeiter-Schemaänderung bestätigt.

### MA-08 – SQLite-Speicherung und Mitarbeitermigration umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Infrastructure implementiert ausschließlich die in MA-05 und MA-06 bestätigten Speicherverträge.
- Eine neue Migration ergänzt Typkatalog, Einsatzfreigaben, Mitarbeitende und Typzuordnungen.
- Die acht Starttypen werden idempotent mit stabilen Kennungen bereitgestellt; es werden keine realen Mitarbeiter vorbefüllt.
- Fremdschlüssel und geeignete Eindeutigkeitsregeln schützen Typ-, Dienst-, Orts- und Musterbezüge.
- Das Entfernen verwendeter Typen wird verhindert.

Prüfung:

- Infrastructure-Tests verwenden echte temporäre SQLite-Dateien.
- Migration von leerer Datenbank sowie Upgrade vom unveränderten System-04-Stand bestehen.
- Initialisierung ist wiederholbar; Speichern, Laden, Typwechsel, Deaktivierung und Neustart-Round-trip bestehen.
- Eine zweite aktive Typ1-Person und ungültige Referenzen werden zuverlässig verhindert.
- Fehlgeschlagene Schreibvorgänge hinterlassen keinen teilweise gültigen Zustand.
- Vollständiger Build, Infrastructure-Tests und Architekturtests bestehen.

Abnahmebedingung:

- Datenmodell, Migration und lokale Round-trips werden technisch bestätigt.

### MA-09 – Mitarbeiterübersicht in WPF umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Eine getrennte deutsche Mitarbeiteransicht zeigt Vorname, Nachname, Aktivstatus, Typcode, verständlichen Typnamen, Wochen-Soll und zusammengefasste Einsatzmöglichkeiten.
- Laden, leerer Zustand, Arbeitszustand und technische Fehler werden verständlich dargestellt.
- Das Feature wird modular an die bestehende Einsatzort- und Diensttypseite angebunden.
- ViewModel und View greifen ausschließlich über Application auf Daten zu.

Prüfung:

- Desktop-Tests prüfen Zustände, Auswahl und Benutzerbefehle des ViewModels.
- Architekturtests sichern die `Desktop.Composition`-Grenze und die Trennung vom `ServiceCatalogViewModel`.
- Die WPF-App wird manuell gestartet; Übersicht, leerer Zustand, Lesbarkeit und deutsche Texte werden sichtbar geprüft.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbare Übersicht und ihre Bedienbarkeit.

### MA-10 – Anlegen, Bearbeiten, Typwechsel und Deaktivieren in WPF umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Mitarbeitende können mit Vorname, Nachname und genau einem vorhandenen Typ angelegt werden.
- Name, Typzuordnung und Aktivstatus können im bestätigten Umfang bearbeitet werden.
- Die Bedienung verhindert eine zweite aktive Typ1-Person und erklärt den Konflikt verständlich.
- Typcode, verständlicher Name, Wochen-Soll und Einsatzmöglichkeiten sind vor der Auswahl erkennbar.
- Validierungsfehler werden deutsch, feldbezogen und ohne Datenverlust angezeigt.
- Suche, Filter, freie Sortierung, Mitarbeitertyp-Katalogpflege und endgültiges Löschen bleiben ausgeschlossen.

Prüfung:

- Desktop- und Application-Tests prüfen erfolgreiche sowie ungültige Eingaben und Abbruch ohne Speicherung.
- Ein manueller WPF-Ablauf mit klar synthetischen Daten prüft Anlegen, Bearbeiten, Typwechsel, Deaktivieren, Neustart und erneutes Laden.
- Es findet kein Netzwerkzugriff statt; keine Testdaten oder lokale Datenbank werden versioniert.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbaren Eingabe- und Bearbeitungsabläufe.

### MA-11 – System 03 gemeinsam abschließen

Status: `[ ]`

Geplantes Ergebnis:

- Domain, Application, Infrastructure und Desktop bilden den bestätigten Umfang von System 03 widerspruchsfrei ab.
- Die Übergaben für Typ1, AH, Wochen-Soll, Abwesenheitsreduktion, Planungslaufoption und Stundenbericht sind für die späteren Systeme eindeutig dokumentiert.
- Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` zeigen denselben tatsächlichen Stand.
- System 03 wird erst nach ausdrücklicher Abschlussabnahme als abgeschlossen archiviert.

Prüfung:

- Gesperrte Paketwiederherstellung, vollständiger Build und alle automatischen Tests bestehen ohne neue Warnungen.
- Der vollständige Mitarbeiterablauf wird mit synthetischen Daten manuell in WPF geprüft.
- Eine Suche findet keine veralteten Roadmap-Pfade oder widersprüchlichen Statusangaben.
- `git diff --check` meldet keine Whitespace-Fehler.
- Die Dateiliste enthält keine echten oder sensiblen Daten und keine Datenbank-, Sicherungs-, Export- oder Buildartefakte.

Abnahmebedingung:

- Der Auftraggeber bestätigt System 03 und erlaubt die Archivierung dieser Roadmap sowie den Übergang zum nächsten System.

## Übergaben an spätere Systeme

### System 06 – Verfügbarkeiten und Abwesenheiten

- sichtbare Eintragung von Urlaub, Krankheit und sonstigen Abwesenheiten
- genaue Regel für das reduzierte Wochen-Soll
- Sonderfall einer vollständig abwesenden Typ1-Person

### System 07 – Regelkatalog und Prioritäten

- zwingende Wochenstundenkorridore
- asymmetrische AH-Zielreihenfolge und Warnschwelle
- Typ1-Ausschluss aus der automatischen Planung
- strukturierte Regeldefinitionen ohne Abfragen nach sichtbaren Typcodes

### System 08 – Planmodell und Planungszeiträume

- manuell vorgetragene Typ1-Zuweisungen
- automatischer Schutz dieser Zuweisungen als Sperren
- unveränderliche Typ-, Soll- und Freigabemomentaufnahme im Planungseingang

### System 09 – Automatische Plangenerierung

- wöchentliche statt planungszeitraumweite Stundenbewertung
- Bedarfsdeckung vor Sollabweichung innerhalb aller zwingenden Grenzen
- standardmäßig ausgeschaltete Planungslaufoption für TypAH2 als `Spr`
- keine automatische AH-Frühdienst- oder Typ1-Zuweisung

### System 10 – Konflikte und Lösungsvorschläge

- AH-Frühdienstvorschlag bei weiter ungedecktem Frühdienst
- Typ1 erst als letzte manuelle Lösungsmöglichkeit
- strukturierte Ursache- und Lösungscodes
- unveränderlicher Wochenstundenbericht je Planungsergebnis

### System 11 und 12 – Manuelle Bearbeitung und Planversionen

- bewusste Bestätigung vorgeschlagener Sonderzuweisungen
- sichtbare Markierung ohne Änderung der Typfreigaben
- Schutz bei späterer Neugenerierung
- unveränderte historische Planversionen nach Typänderungen

## Echte externe und manuelle Gates

- Die Roadmap und jede spätere Fach- oder Architekturänderung benötigen ausdrückliche Abnahme; Schweigen ist keine Zustimmung.
- Die beantwortete Fragen-Datei bestätigt das Fachmodell, ersetzt aber nicht die Abnahme dieser überarbeiteten Roadmap.
- Die sichtbaren WPF-Abläufe müssen zusätzlich zu automatischen Tests manuell geprüft werden.
- Alle manuellen Beispiele verwenden ausschließlich klar erfundene Daten.
- Die gemeinsame SQLite-Migrationsfolge muss vor der Mitarbeitermigration praktisch charakterisiert werden.
- Ein lokaler Build ersetzt weder den sichtbaren WPF-Test noch die spätere portable Windows-11-Prüfung.
- Portable Veröffentlichung und Start auf einem sauberen Windows-11- beziehungsweise Klinikrechner bleiben bis System 15 offen.
- Die fachliche Endabnahme der gesamten Dienstplanung bleibt offen; System 03 allein erzeugt noch keinen Dienstplan.

## Berichtsschema nach jedem Schritt

Nach jedem Schritt werden kurz genannt:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und ihr Ergebnis,
4. offene Gates, Risiken oder Blockaden,
5. Git-Status ohne automatischen Commit oder Push,
6. nächster minimaler Schritt,
7. Bitte um ausdrückliche Abnahme.

## Nächster minimaler Schritt

MA-02A mit der repositoryweiten Zeilenendenregel ausdrücklich abnehmen oder Änderungswünsche nennen. Erst danach beginnt MA-03 mit Einsatzfreigaben und dem initialen Typenkatalog. Mitarbeiterobjekt, Datenbankänderungen und WPF-Bedienung bleiben weiterhin unverändert.
