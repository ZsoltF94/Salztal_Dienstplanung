# Teil-Roadmap: Personal-, Schicht- und Stundenbedarf

Status: Abgeschlossen und archiviert am 2026-09-15

Stand: 2026-09-15

## Ziel und Nutzen

Diese Roadmap beschreibt System 05 der `MASTER_ROADMAP.md`. Am Ende soll die Service-Leitung wiederkehrende Standardbedarfe und ausdrücklich datumsbezogene Ausnahmen je Einsatzort und normalem Diensttyp erfassen, ändern und nachvollziehbar prüfen können.

Jeder wirksame Bedarf enthält die tatsächlich zu besetzende Zeit und eine konstante benötigte Personenzahl. Die App berechnet daraus die Bedarfsdauer und die benötigten Mitarbeiterstunden. Diese Daten bilden später die verbindliche Bedarfsseite für Planmodell, automatische Generierung, Konflikterklärung und Planansichten; System 05 teilt noch keine Mitarbeitenden ein und erzeugt noch keinen Dienstplan.

## Verbindliche Grundlagen

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`
- `ARCHITECTURE.md`
- `CLEANCODE.md`
- `AGENTS.md`
- `MASTER_ROADMAP.md`
- `STATUS.md`
- `docs/decisions/SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/SHARED_SQLITE_MIGRATION_BOUNDARY.md`
- `docs/roadmaps/completed/WORK_LOCATIONS_SHIFT_TYPES_SPLIT_SHIFTS_ROADMAP.md`
- `docs/roadmaps/completed/EMPLOYEES_EMPLOYEE_TYPES_SHIFT_ELIGIBILITY_ROADMAP.md`

## Bestätigte fachliche Grundlagen

### Bedeutung eines Bedarfs

- Ein Bedarf gehört zu genau einem Einsatzort und verlangt genau einen vorhandenen normalen Diensttyp dieses Einsatzortes.
- Doppeldienst `D` und Springer `Spr` sind zusammengesetzte Einsatzmuster und niemals auswählbare Diensttypen eines einzelnen Bedarfs.
- Ein wirksamer Bedarf besitzt genau einen zusammenhängenden tatsächlichen Zeitraum innerhalb desselben Kalendertags.
- Innerhalb dieses Zeitraums bleibt die benötigte Personenzahl konstant und ganzzahlig.
- Je Einsatzort, Datum und normalem Diensttyp existiert höchstens ein wirksamer Bedarfsblock.
- Verschiedene normale Diensttypen dürfen am selben Einsatzort und Datum gleichzeitig beziehungsweise überlappend benötigt werden. Dadurch bleiben insbesondere Cafeteria-Dienst A und Cafeteria-Dienst B am Wochenende getrennte Bedarfe.
- Beginn und Ende werden in der Bedienung in 30-Minuten-Schritten eingegeben und technisch minutengenau gespeichert.
- Das Ende muss nach dem Beginn liegen. Dienste über Mitternacht gehören nicht zum Umfang.
- Die tatsächliche Bedarfszeit darf nach einer ausdrücklichen Eingabe von der aktuellen Standardzeit des verlangten Diensttyps abweichen.
- Eine fachliche Höchstpersonenzahl wurde nicht festgelegt und wird nicht willkürlich erfunden. Die Zahl muss positiv, ganzzahlig und technisch sicher berechenbar sein.

### Berechnete Bedarfswerte

- Die Bedarfsdauer wird ausschließlich aus tatsächlichem Beginn und tatsächlichem Ende berechnet.
- Die benötigten Mitarbeiterstunden ergeben sich aus Personenzahl mal tatsächlicher Bedarfsdauer.
- Die Service-Leitung gibt keinen zweiten unabhängigen Stundenwert ein.
- Dauer und Gesamtbedarf werden intern in ganzen Minuten berechnet; `double` und `float` sind dafür unzulässig.
- Eine positive Personenzahl erzeugt einen wirksamen Bedarf. „Kein Bedarf“ wird fachlich als fehlender beziehungsweise ausdrücklich aufgehobener Bedarf und nicht als scheinbar wirksamer Null-Personen-Bedarf behandelt.
- Die erste Oberfläche zeigt mindestens den berechneten Wert je Bedarf, Tagessummen je Einsatzort, Wochensummen je Einsatzort und die Gesamtsumme der ausgewählten Woche.
- Die unveränderlichen Leseergebnisse enthalten die atomaren Bedarfswerte und berechneten Minuten so vollständig, dass später weitere abgestimmte Summen oder Darstellungen ergänzt werden können, ohne das Fachmodell umzubauen.

### Wiederkehrende Standardbedarfe

- Standardbedarfe bilden eine wiederkehrende Wochenvorlage von Montag bis Sonntag.
- Eine Standardänderung gilt ab einer bewusst ausgewählten Planungswoche und damit ab deren Montag.
- Frühere Wochen bleiben unverändert auflösbar.
- Eine Änderung darf Personenzahl und tatsächliche Zeit ersetzen, einen bisher fehlenden Standardbedarf ergänzen oder einen Standardbedarf ab der ausgewählten Woche aufheben.
- Standardänderungen werden deshalb als zeitlich wirksame Revisionen erhalten und nicht als rückwirkendes Überschreiben derselben Zeile modelliert.
- Derselbe Standardbedarf darf am selben Wirksamkeitsmontag wiederholt korrigiert werden. Jede Speicherung bleibt als eigene unveränderliche Fassung nachvollziehbar; für diesen Montag ist die Fassung mit der höchsten Korrekturfolge fachlich maßgeblich.
- Die Standardzeit des Diensttyps ist der Ausgangswert für einen neu angelegten Bedarf. Ein bereits gespeicherter Standardbedarf besitzt jedoch seine eigene tatsächliche Zeit und wird durch eine spätere Änderung der Diensttyp-Standardzeit nicht stillschweigend verändert.
- Die erste Grundbelegung gilt als Ausgangsstand für alle Wochen, für die keine spätere Standardrevision greift.

### Datumsbezogene Ausnahmen

- Eine Datumsausnahme gilt ausschließlich für ein ausgewähltes Kalenderdatum und einen normalen Diensttyp an dessen Einsatzort.
- Sie darf Personenzahl und tatsächliche Zeit vollständig ersetzen.
- Sie darf einen an diesem Datum sonst geltenden Standardbedarf vollständig aufheben.
- Sie darf für dieses Datum einen Bedarf ergänzen, für den an diesem Wochentag kein Standardbedarf gilt.
- Eine Ausnahme ist eine vollständige fachliche Momentaufnahme. Spätere Änderungen am Standardbedarf oder an der Diensttyp-Standardzeit verändern sie nicht.
- Wird eine Ausnahme entfernt, gilt für dieses Datum wieder der zu diesem Zeitpunkt auflösbare Standardbedarf.
- Feiertage und andere besondere Tage werden in der ersten Fassung bewusst manuell als Datumsausnahmen erfasst. Eine automatische Feiertagserkennung gehört nicht zum Umfang.

### Bestätigte vollständige Startbedarfe

Die folgenden Werte sind für die erste Fassung vollständig. Cafeteria-Dienst A und B bleiben auch bei zeitlicher Überlappung zwei getrennte normale Diensttypen.

| Einsatzort | Wochentage | Diensttyp | Personen | Tatsächliche Zeit |
|---|---|---|---:|---|
| Cafeteria | Montag bis Freitag | Cafeteria-Dienst A | 1 | 13:30–20:30 Uhr |
| Cafeteria | Samstag und Sonntag | Cafeteria-Dienst A | 1 | 13:30–20:30 Uhr |
| Cafeteria | Samstag und Sonntag | Cafeteria-Dienst B | 1 | 13:30–17:30 Uhr |
| Restaurant | Montag bis Sonntag | Frühdienst | 4 | 06:30–13:30 Uhr |
| Restaurant | Montag bis Sonntag | Spätdienst | 4 | 16:30–19:30 Uhr |

Als nach Wochentagen aufgelöster Ausgangskatalog entstehen daraus 23 einzelne Standardbedarfe. Die tatsächlichen Zeiten werden bei der erstmaligen Anlage aus den dann vorhandenen normalen Diensttypen übernommen, damit bereits bewusst geänderte Diensttyp-Standardzeiten nicht durch historische Startwerte überschrieben werden. Personenzahlen und die vorhandenen beziehungsweise fehlenden Kombinationen folgen der bestätigten Tabelle.

## Umfang dieser Roadmap

- stark typisierte und unveränderliche Fachwerte für Bedarfskennung, Personenzahl, tatsächliche Zeit und berechnete Minuten,
- wiederkehrende Standardbedarfe je Wochentag mit ab Montag wirksamen Revisionen,
- vollständige Datumsausnahmen zum Ersetzen, Ergänzen und Aufheben eines Bedarfs,
- deterministische Auflösung einer ausgewählten Montag-bis-Sonntag-Woche,
- Startbedarfe für Cafeteria und Restaurant,
- getrennte Lese- und Schreibanwendungsfälle mit verständlichen deutschen Ergebnissen,
- atomare Speicherung und Konflikterkennung bei zwischenzeitlichen Änderungen,
- Erweiterung der gemeinsamen lokalen SQLite-Datenbank durch eine neue Migration,
- Upgrade-Prüfung sowohl einer leeren Datenbank als auch des aktuellen System-03-Datenbankstands,
- eine eigene WPF-Bedarfsansicht für Wochenübersicht, Summen, Standardänderungen und Datumsausnahmen,
- Lade-, Leer-, Validierungs-, Speicher-, Erfolgs-, Konflikt- und Fehlerzustände,
- wahrheitsgemäße Aktualisierung von Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung`.

## Nicht Bestandteil dieser Roadmap

- Auswahl oder Einteilung von Mitarbeitenden,
- Verfügbarkeiten, Urlaub, Krankheit, Fortbildung oder Wunschfrei,
- zwingende und weiche Planungsregeln,
- automatische Plangenerierung mit OR-Tools,
- Erkennung von Über- oder Unterbesetzung eines erzeugten Plans,
- konkrete Springer-Zuweisung oder Bewertung seiner späteren Teilunterdeckung,
- Konflikterklärungen und Lösungsvorschläge,
- Planmodell, Planungszeiträume, manuelle Planbearbeitung und Sperren,
- Planversionen, Abnahme, Excel-Export oder Zeitkonten,
- automatische Feiertags- oder Kalenderlogik,
- mehrere getrennte Zeitblöcke desselben Diensttyps an demselben Ort und Datum,
- Dienste über Mitternacht,
- Anlegen, Löschen oder Deaktivieren von Einsatzorten und Diensttypen,
- neue NuGet-Pakete oder ein zweiter `DbContext`.

## Bewusst noch offene Detailfragen

- Die genaue visuelle Anordnung der ersten Bedarfsübersicht wird mit dem sichtbaren Entwurf in BE-08 gemeinsam beurteilt. Der bestätigte Informationsumfang bleibt dabei verbindlich.
- Zusätzliche Summen oder Filter über die bestätigten Mindestangaben hinaus werden erst nach der ersten sichtbaren Gestaltung mit der Service-Leitung priorisiert.
- Für BE-06 wurde vor der Umsetzung festgelegt: Das Entfernen einer bereits fehlenden Datumsausnahme ist ein idempotenter Erfolg. Eine inzwischen unter demselben Schlüssel gespeicherte andere Ausnahme bleibt dagegen ein sichtbarer Konflikt.
- Mehrere getrennte Zeitblöcke desselben Diensttyps an demselben Ort und Datum benötigen bei einem späteren realen Bedarf eine eigene Fachentscheidung und gehören nicht zu dieser Roadmap.
- Das spätere Löschen eines Einsatzortes oder Diensttyps mit noch referenzierten zukünftigen Bedarfen bleibt eine gesonderte Entscheidung des Stammdaten-Lebenszyklus.

Keine dieser Detailfragen blockiert BE-02. Sie darf nicht stillschweigend zu einer neuen Funktion, einer Architekturänderung oder einer Erweiterung des bestätigten Umfangs führen.

## Architekturgrenzen

### Domain

- Der neue Fachbereich liegt unter `Salztal.Dienstplanung.Domain.StaffingDemands`.
- Domain besitzt die einzige fachliche Definition der Bedarfswerte, Standardrevisionen, Datumsausnahmen und Berechnungen.
- Domain referenziert keine technischen Projekte und keine EF-Core-, WPF- oder OR-Tools-Typen.
- System 05 referenziert `WorkLocationId` und `ShiftTypeId`; Namen, Farben, Anzeigen und Standardzeiten werden nicht als zweite Stammdatenkopie in den Bedarfsdefinitionen geführt.
- Eine tatsächliche Bedarfszeit bleibt dennoch ein eigener historisch relevanter Fachwert und ist keine dynamische Referenz auf `ShiftStandardTime`.
- Lokale Invarianten werden bei der Erzeugung geprüft. Die Prüfung, ob Diensttyp und Einsatzort im aktuellen System-04-Katalog zusammengehören, erfolgt beim Anwendungsfall mit den gemeinsam geladenen Katalogdaten.

### Application

- Der neue Fachbereich liegt unter `Salztal.Dienstplanung.Application.StaffingDemands`.
- Queries liefern unveränderliche Wochen- und Bearbeitungsmomentaufnahmen.
- Commands für Standardrevision, Datumsausnahme und Entfernen einer Ausnahme bleiben getrennt.
- Speicherverträge werden nach diesen Anwendungsfällen benannt; ein generisches CRUD-Repository ist nicht zulässig.
- Application lädt Bedarfs- und Katalogdaten vollständig, prüft die referenzierten Kennungen und erzeugt verständliche deutsche Validierungs- und Konfliktmeldungen.
- WPF-, EF-Core-, SQLite- und OR-Tools-Typen überschreiten diese Grenze nicht.

### Infrastructure

- Der neue technische Unterbereich liegt unter `Persistence/StaffingDemands`.
- Ein eigener `SqliteStaffingDemandStore` implementiert ausschließlich die von Application benötigten Bedarfsverträge; `SqliteServiceCatalogStore` wird nicht zu einem fachübergreifenden Sammelspeicher erweitert.
- `ServiceCatalogDbContext` bleibt trotz seines historischen Namens der einzige Context der gemeinsamen lokalen Datenbank.
- Neue Tabellen, Konfigurationen und Migrationen werden in derselben Assembly, demselben Migrationsordner und derselben `__EFMigrationsHistory` fortgeführt.
- Die veröffentlichten Migrationen `20260913202156_InitialServiceCatalog` und `20260914005039_AddEmployeesAndEmployeeTypes` werden nicht verändert.
- Fremdschlüssel und eindeutige Indizes schützen gültige Katalogreferenzen sowie höchstens eine Revisionsfolge pro Bedarfsschlüssel, Wirksamkeitsmontag und Korrekturfolge beziehungsweise höchstens eine Ausnahme pro Datumsschlüssel.
- Datums-, Uhrzeit- und Minutenwerte erhalten SQLite-Round-trip-Tests.

### Desktop

- Der neue UI-Fachbereich liegt unter `Salztal.Dienstplanung.Desktop.Features.StaffingDemands`.
- Die Bedarfsansicht wird als eigener Reiter eingebunden und bleibt von den ViewModels für Servicekatalog und Mitarbeitende getrennt.
- ViewModels verwenden ausschließlich Application-Verträge und UI-eigene Typen.
- Nur `Salztal.Dienstplanung.Desktop.Composition` kennt und verdrahtet den konkreten SQLite-Adapter.
- Farben sind nicht zur fachlichen Unterscheidung erforderlich. Quelle und Zustand eines Bedarfs werden zusätzlich durch verständlichen Text gekennzeichnet.
- Die erste konkrete Oberflächengestaltung wird in BE-08 sichtbar geprüft und darf danach innerhalb des bestätigten Funktionsumfangs gemeinsam angepasst werden.

### Unberührte Module

- `Salztal.Dienstplanung.Planning` wird nicht um Bedarfsübersetzung oder Solverbedingungen erweitert.
- `Salztal.Dienstplanung.Excel` bleibt unverändert.
- Mitarbeiter-, Mitarbeitertyp- und Einsatzfreigabelogik bleibt unverändert.
- System 05 prüft noch keine Planbesetzung und erzeugt keine Konflikte aus fehlenden Mitarbeitenden.

## Verbindliche Code-Anker

### Vorhandene Anker

- `src/Salztal.Dienstplanung.Domain/WorkLocations/WorkLocationId.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/ShiftTypeId.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/ShiftType.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/ShiftStandardTime.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/InitialShiftTypeCatalog.cs`
- `src/Salztal.Dienstplanung.Application/ServiceCatalog/IServiceCatalogReader.cs`
- `src/Salztal.Dienstplanung.Application/ServiceCatalog/ShiftTypeSnapshot.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContext.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContextFactory.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/SharedDatabaseMigrationBoundary.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations/ServiceCatalogDbContextModelSnapshot.cs`
- `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowComposition.cs`
- `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowDependencies.cs`
- `src/Salztal.Dienstplanung.Desktop/MainWindowViewModel.cs`
- `src/Salztal.Dienstplanung.Desktop/MainWindow.xaml`
- `tests/Salztal.Dienstplanung.Architecture.Tests/InfrastructurePersistenceBoundaryTests.cs`
- `tests/Salztal.Dienstplanung.Architecture.Tests/DesktopCompositionBoundaryTests.cs`

### Vorgesehene neue Anker

Die endgültige Aufteilung darf innerhalb eines freigegebenen Schritts kleiner werden, die Verantwortungen und Namespace-Grenzen bleiben jedoch verbindlich.

- `src/Salztal.Dienstplanung.Domain/StaffingDemands/StaffingDemandId.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/RequiredEmployeeCount.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/StaffingDemandTime.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/StaffingDemand.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/StandardStaffingDemandRevision.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/StaffingDemandDateException.cs`
- `src/Salztal.Dienstplanung.Domain/StaffingDemands/InitialStaffingDemandCatalog.cs`
- `src/Salztal.Dienstplanung.Application/StaffingDemands/GetStaffingDemandWeekQuery.cs`
- `src/Salztal.Dienstplanung.Application/StaffingDemands/StaffingDemandWeekSnapshot.cs`
- `src/Salztal.Dienstplanung.Application/StaffingDemands/SaveStandardStaffingDemandCommand.cs`
- `src/Salztal.Dienstplanung.Application/StaffingDemands/SaveStaffingDemandDateExceptionCommand.cs`
- `src/Salztal.Dienstplanung.Application/StaffingDemands/RemoveStaffingDemandDateExceptionCommand.cs`
- schmale Lese- und Schreibverträge im selben Application-Fachbereich,
- Entities, EF-Konfigurationen und `SqliteStaffingDemandStore` unter `src/Salztal.Dienstplanung.Infrastructure/Persistence/StaffingDemands`,
- eine neue generierte Migration unter `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations`,
- `StaffingDemandOverviewView.xaml` und zugehörige ViewModels unter `src/Salztal.Dienstplanung.Desktop/Features/StaffingDemands`,
- gespiegelte Tests unter den vorhandenen Domain-, Application-, Infrastructure- und Desktop-Testprojekten.

Die Namen `StaffingDemand`, `RequiredEmployeeCount`, `StandardStaffingDemandRevision` und `StaffingDemandDateException` sind die bestätigten fachlichen Richtungsanker. Falls ein Implementierungsschritt eine präzisere Zerlegung benötigt, wird sie vor dem Code im jeweiligen Schritt berichtet und darf keine andere Fachbedeutung einführen.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Schritte

### BE-01 – Teil-Roadmap mit Fachentscheidungen und Code-Ankern entwerfen

Status: `[x]` – Entwurf erstellt, geprüft und am 2026-09-14 ausdrücklich abgenommen

Umfang:

- Ziel, Nutzen, bestätigte Bedarfsregeln, Umfang, Nicht-Umfang, Architekturgrenzen, Code-Anker, kleine Schritte und echte Gates festhalten.
- Die bestätigte vollständige Grundbelegung und die präzisierte Cafeteria-Wochenendbelegung dokumentieren.
- Die bestätigten Regeln für Wirksamkeitsmontag, vollständige Datumsausnahmen, manuelle Feiertage, einen Bedarfsblock je Schlüssel und erweiterbare Summen festhalten.
- `MASTER_ROADMAP.md`, `STATUS.md` und den Bereich `Service-Leitung` auf den Roadmap-Entwurf abstimmen.
- Noch keinen Fachcode, kein Datenbankschema und keine WPF-Bedarfsansicht anlegen.

Prüfung:

- Der Entwurf stimmt mit Grundlagen, Architektur, Clean-Code-Regeln, System 04 und der gemeinsamen SQLite-Migrationsgrenze überein.
- Alle vorhandenen und vorgesehenen Code-Anker zeigen auf die richtige Modulverantwortung.
- Eine Suche findet keine widersprüchliche Aussage zur Cafeteria-Wochenendbelegung, Systemauswahl oder aktiven Roadmap.
- `git diff --check` meldet keine Whitespace-Fehler.

Abnahmebedingung:

- Der Auftraggeber bestätigt den Roadmap-Entwurf einschließlich Fachmodell, Code-Ankern und Schrittreihenfolge oder nennt Änderungswünsche.

### BE-02 – Bedarfsgrundwerte und Stundenberechnung fachlich modellieren

Status: `[x]` – am 2026-09-14 ausdrücklich abgenommen

Umfang:

- Stark typisierte Kennung, positive ganzzahlige Personenzahl und tatsächliche Bedarfszeit einführen.
- Einen unveränderlichen einzelnen Bedarf mit `WorkLocationId`, `ShiftTypeId`, tatsächlicher Zeit und Personenzahl modellieren.
- Bedarfsdauer und Mitarbeiterbedarf in ganzen Minuten deterministisch und überlaufsicher berechnen.
- Lokale Fehlercodes für leere Kennungen, fehlende Referenzen, ungültige Zeiten und ungültige Personenzahlen bereitstellen.
- Noch keine Wochenstandards, Datumsausnahmen, Application-Abläufe oder Speicherung umsetzen.

Prüfung:

- Domain-Tests decken gültige Werte, alle lokalen Fehler, 30-Minuten-Grenzen, Ende-vor-Beginn, denselben Zeitpunkt, ganze Minuten und die bestätigten Rechenbeispiele 28 beziehungsweise 24 Stunden ab.
- Tests bestätigen, dass kein unabhängiger Stundenwert übergeben werden kann.
- Domain kompiliert ohne neue Abhängigkeiten oder Warnungen.

Abnahmebedingung:

- Der Auftraggeber bestätigt Fachwerte, Invarianten und Berechnung als Grundlage für wiederkehrende Standards.

Nachweis vom 2026-09-14:

- `StaffingDemandId`, `RequiredEmployeeCount`, `StaffingDemandTime` und `StaffingDemand` bilden Kennung, Datum, stabile Katalogreferenzen, tatsächliche Zeit und positive Personenzahl als unveränderliche Domain-Werte ab.
- `StaffingDemand.Create` sammelt strukturierte Fehler für leere Kennungen, nicht positive Personenzahl, Sekunden, nicht halbstündige Eingaben sowie leere oder über Mitternacht reichende Zeiträume.
- Bedarfsdauer wird als `int` in Minuten und der gesamte Mitarbeiterbedarf überlaufsicher als `long` in Minuten berechnet. Ein unabhängiger Stundenparameter existiert nicht.
- 18 neue fokussierte Tests bestehen. Insgesamt bestehen 113 Domain-Tests und 16 Architekturtests; der vollständige Lauf aller sieben Testprojekte besteht mit 239 von 239 Tests.
- Der Domain-Build und der vollständige Solution-Build bestehen mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` besteht.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat die Bedarfsgrundwerte, Invarianten und Minutenberechnung ausdrücklich bestätigt.

### BE-03 – Wiederkehrende Standardbedarfe und Wirksamkeitsrevisionen modellieren

Status: `[x]` – am 2026-09-14 ausdrücklich abgenommen

Umfang:

- Den eindeutigen Standardschlüssel aus Wochentag, Einsatzort und normalem Diensttyp modellieren.
- Eine unveränderliche Standardrevision mit Wirksamkeit ab einem Montag modellieren.
- Ergänzen, Ersetzen und Aufheben eines Standards ab einer ausgewählten Woche fachlich unterscheiden.
- Den vollständigen initialen Bedarfskatalog mit 23 Wochentagswerten bereitstellen.
- Verhindern, dass zwei Revisionen desselben Schlüssels mit demselben Wirksamkeitsmontag gleichzeitig gültig sind.
- Noch keine Datumsausnahmen, Application-Abläufe oder Speicherung umsetzen.

Prüfung:

- Domain-Tests decken Montagspflicht, frühere und spätere Revisionen, Ergänzen, Ersetzen, Aufheben und Eindeutigkeit ab.
- Katalogtests prüfen alle bestätigten Kombinationen, Personenzahlen und die Trennung von Cafeteria-Dienst A und B.
- Tests belegen, dass `D` und `Spr` nicht als einzelner Bedarfsdiensttyp in den Startwerten vorkommen.

Abnahmebedingung:

- Der Auftraggeber bestätigt Wochenvorlage, Wirksamkeitsprinzip und Startkatalog.

Nachweis vom 2026-09-14:

- `StandardStaffingDemandKey` bildet den eindeutigen Standardschlüssel aus Wochentag, Einsatzort- und normaler Diensttypkennung unveränderlich ab.
- `StandardStaffingDemandRevision` unterscheidet Ergänzen, Ersetzen und Aufheben ausdrücklich. Jede Revision gilt ab einem geprüften Montag; aufgehobene Standards besitzen weder eine scheinbare Null-Personenzahl noch eine tatsächliche Zeit.
- `StandardStaffingDemandRevisionSet` bewahrt frühere und spätere Revisionen, löst den für ein Datum wirksamen Stand auf und lehnt doppelte Schlüssel am selben Wirksamkeitsmontag sowie fachlich unmögliche Revisionsfolgen ab.
- `InitialStaffingDemandCatalog` enthält alle 23 bestätigten Wochentagswerte. Die tatsächlichen Zeiten werden aus den übergebenen aktuellen normalen Diensttypen übernommen; `D` und `Spr` können nicht als Bedarfsdiensttyp erscheinen.
- 15 neue fokussierte BE-03-Tests bestehen. Insgesamt bestehen 128 Domain-Tests; der vollständige Lauf aller sieben Testprojekte besteht mit 254 von 254 Tests.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` und `git diff --check` bestehen.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat Wochenvorlage, Wirksamkeitsprinzip und Startkatalog ausdrücklich bestätigt.

### BE-04 – Datumsausnahmen und wirksame Wochenauflösung modellieren

Status: `[x]` – umgesetzt, automatisch geprüft und ausdrücklich abgenommen

Umfang:

- Vollständige Datumsausnahmen für Ersetzen, Ergänzen und Aufheben modellieren.
- Entfernen einer Ausnahme als Rückkehr zum aktuell auflösbaren Standard definieren.
- Eine ausgewählte Montag-bis-Sonntag-Woche deterministisch aus Standardrevisionen und Datumsausnahmen auflösen.
- Je Einsatzort, Datum und normalem Diensttyp höchstens einen wirksamen Bedarfsblock sichern.
- Bedarfs-, Tages-, Einsatzort- und Wochensummen aus den wirksamen atomaren Bedarfen berechnen.
- Feiertage ausschließlich als gewöhnliche manuelle Datumsausnahmen behandeln.

Prüfung:

- Domain-Tests decken unveränderten Standard, ersetzte, ergänzte, aufgehobene und wieder entfernte Ausnahmen ab.
- Tests bestätigen, dass eine bestehende Ausnahme von späteren Standardrevisionen und Diensttyp-Standardzeitänderungen unberührt bleibt.
- Tests decken überlappende verschiedene Diensttypen und die verbotene doppelte Belegung desselben Bedarfsschlüssels ab.
- Summen werden für die vollständige bestätigte Startwoche und für synthetische Ausnahmewochen geprüft. Bei den bestätigten Ausgangszeiten ergeben sich 57 Cafeteria-Stunden, 280 Restaurant-Stunden und insgesamt 337 Mitarbeiterstunden pro Woche.

Abnahmebedingung:

- Der Auftraggeber bestätigt Ausnahmevorrang, Rückkehr zum Standard und berechnete Wochenansicht.

Nachweis vom 2026-09-14:

- `StaffingDemandDateException` bildet Ergänzen, Ersetzen und Aufheben als getrennte vollständige Datumsmomentaufnahmen ab. Eine Aufhebung enthält weder eine scheinbare Null-Personenzahl noch eine tatsächliche Zeit.
- `StaffingDemandDateExceptionSet` bewahrt eine unveränderliche Ausnahmemenge und lehnt mehr als eine Ausnahme je Datum, Einsatzort und normalem Diensttyp ab.
- `StaffingDemandWeek` löst ausschließlich vollständige Montag-bis-Sonntag-Wochen auf, wendet Datumsausnahmen nach dem wirksamen Standard an und liefert atomare Bedarfe mit Quelle sowie Tages-, Einsatzort- und Wochensummen in ganzen Minuten.
- Das Entfernen einer vorhandenen Ausnahme wird fachlich als Auflösung ohne diese Ausnahme geprüft; dadurch gilt wieder die dann wirksame Standardrevision. Das noch offene Ergebnis beim Entfernen einer bereits fehlenden Ausnahme bleibt unverändert BE-06 vorbehalten.
- 16 neue fokussierte BE-04-Tests bestehen. Die bestätigte Ausgangswoche ergibt 57 Cafeteria-Stunden, 280 Restaurant-Stunden und insgesamt 337 Mitarbeiterstunden.
- Insgesamt bestehen 144 Domain-Tests; der vollständige Lauf aller sieben Testprojekte besteht mit 270 von 270 Tests.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` und `git diff --check` bestehen.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat BE-04 ausdrücklich abgenommen und BE-05 freigegeben.

### BE-05 – Leseanwendungsfall und unveränderliche Bedarfsansicht umsetzen

Status: `[x]` – umgesetzt, automatisch geprüft und ausdrücklich abgenommen

Umfang:

- Einen Lesevertrag für Standardrevisionen, Datumsausnahmen und den System-04-Katalog definieren.
- `GetStaffingDemandWeekQuery` für einen ausdrücklich übergebenen Wochenmontag umsetzen.
- Eine unveränderliche Wochenmomentaufnahme mit Quelle jedes Werts, tatsächlicher Zeit, Personenzahl, Minuten und bestätigten Summen liefern.
- Fehlende Katalogreferenzen und widersprüchliche gespeicherte Daten sichtbar ablehnen, statt sie zu ignorieren.
- Leeren Bestand und Abbruch vor beziehungsweise während des Lesens behandeln.

Prüfung:

- Application-Tests verwenden synthetische Daten für Standard, Ausnahme, Aufhebung, leere Ansicht, unbekannte Referenz und Abbruch.
- Tests bestätigen, dass Application keine WPF-, EF-Core-, SQLite- oder OR-Tools-Typen verwendet.
- Momentaufnahmen enthalten keine veränderlichen oder verzögert geladenen Collections.

Abnahmebedingung:

- Der Auftraggeber bestätigt Lesevertrag, Wochenmomentaufnahme und Fehlergrenzen als Grundlage der Speicherung und UI.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- `IStaffingDemandReader` liefert Standardrevisionen, Datumsausnahmen und den Dienstkatalog als einen unveränderlichen Lesestand an genau einen Leseanwendungsfall.
- `GetStaffingDemandWeekQuery` prüft Abbruch vor und nach dem Laden, validiert gespeicherte Revisionsfolgen sowie Katalogreferenzen und löst anschließend genau eine ausdrücklich übergebene Montag-bis-Sonntag-Woche auf.
- Die unveränderliche Wochenmomentaufnahme enthält je Bedarf Quelle, Einsatzort- und Diensttypanzeige, tatsächliche Zeit, Personenzahl, Dauer und Mitarbeiterbedarf in Minuten sowie Tages-, Einsatzort- und Gesamtsummen.
- Ungültiger Wochenbeginn, doppelte oder widersprüchliche gespeicherte Werte, unbekannte Kennungen und eine falsche Zuordnung von Diensttyp zu Einsatzort liefern strukturierte deutsche Fehler statt stiller Auslassungen.
- 16 neue fokussierte BE-05-Application-Tests bestehen. Sie decken Standard, Ersetzen, Aufheben, leeren Bestand, unveränderliche Collections, Katalog- und Bestandsfehler sowie Abbruch vor und während des Lesens ab.
- Insgesamt bestehen 71 Application-Tests und alle 286 vorhandenen Tests. Die noch leeren Planning- und Excel-Testprojekte werden beim Gesamtlauf mit der vorgesehenen Ausnahme für Exitcode 8 behandelt; dort ist noch kein fachlicher Testumfang vorhanden.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern. Application referenziert weiterhin keine WPF-, EF-Core-, SQLite- oder OR-Tools-Typen.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat BE-05 ausdrücklich abgenommen und BE-06 freigegeben.

### BE-06 – Schreibanwendungsfälle und Speicherverträge umsetzen

Status: `[x]` – am 2026-09-14 ausdrücklich abgenommen

Umfang:

- Getrennte Commands für Standardrevision, Datumsausnahme und Entfernen einer Ausnahme umsetzen.
- Aktuelle Katalogreferenzen laden und Diensttyp sowie Einsatzort vor jedem Schreiben gemeinsam prüfen.
- Zwischenzeitlich geänderte Daten durch erwartete Vorgängerwerte beziehungsweise einen klaren Versionsvertrag erkennen.
- Fachliche Validierung, nicht gefunden, Konflikt, Abbruch und Erfolg in verständliche deutsche Ergebnisse übersetzen.
- Schmale atomare Speicherverträge für die drei Vorgänge definieren.
- Noch keinen SQLite-Adapter und keine WPF-Oberfläche umsetzen.

Prüfung:

- Application-Tests decken gültige Änderungen, alle bestätigten Ausnahmearten, falschen Wochenbeginn, unbekannte Kennungen, falsche Standortzuordnung, Konflikt und Abbruch ab.
- Abgelehnte Änderungen lösen keinen Schreibaufruf aus.
- Entfernen einer nicht vorhandenen Ausnahme liefert ein ausdrücklich definiertes idempotentes oder Nicht-gefunden-Ergebnis; das Verhalten wird vor Implementierung dieses Details im Schritt berichtet.

Abnahmebedingung:

- Der Auftraggeber bestätigt Schreibabläufe, Meldungen und atomare Speichergrenzen.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- `ChangeStandardStaffingDemandCommand` bildet Ergänzen, Ersetzen und Aufheben als neue unveränderliche, ab einem Montag wirksame Revision ab. Die erwartete letzte Revision desselben Standards wird vorab geprüft und dem atomaren Speichervertrag vollständig übergeben.
- `SaveStaffingDemandDateExceptionCommand` bildet Ergänzen, Ersetzen und Aufheben eines Bedarfs für genau ein Datum ab. Eine vorhandene Ausnahme wird nur gegen ihre erwartete vollständige Momentaufnahme ersetzt.
- `RemoveStaffingDemandDateExceptionCommand` entfernt eine vorhandene Ausnahme gegen ihre erwartete Momentaufnahme. Fehlt sie bereits, ist das Ergebnis idempotent erfolgreich und es erfolgt kein unnötiger Schreibaufruf; eine inzwischen andere Ausnahme erzeugt einen Konflikt.
- Jeder tatsächliche Schreibversuch lädt vorher den aktuellen Bedarfs- und Dienstkatalogstand. Unbekannte Kennungen, eine falsche Diensttyp-Einsatzort-Zuordnung, widersprüchliche Bestandsdaten und fachlich unpassende Änderungsarten werden vor dem Speicheraufruf verständlich abgelehnt.
- Drei schmale Speicherverträge halten Anhängen einer Standardrevision, Speichern einer Datumsausnahme und Entfernen einer Datumsausnahme getrennt. Sie erhalten jeweils den erwarteten Vorgänger und liefern Erfolg, Nicht gefunden oder Konflikt zurück; SQLite ist noch nicht angebunden.
- Abbruch wird vor dem Lesen und nach einem Schreibaufruf erneut geprüft, sodass nach einer Abbruchanforderung kein Erfolg gemeldet wird.
- 26 neue fokussierte BE-06-Application-Tests bestehen. Abgelehnte Änderungen lösen nachweislich keinen Schreibaufruf aus.
- Insgesamt bestehen 97 Application-Tests und alle 312 vorhandenen Tests. Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat BE-06 ausdrücklich abgenommen und BE-07 freigegeben.

### BE-07 – SQLite-Speicherung, Startwerte und Migration umsetzen

Status: `[x]` – am 2026-09-14 ausdrücklich abgenommen

Umfang:

- Entities und EF-Konfigurationen für Standardrevisionen und Datumsausnahmen im Unterbereich `Persistence/StaffingDemands` anlegen.
- `ServiceCatalogDbContext` ausschließlich um die benötigten `DbSet`- und Konfigurationsbindungen ergänzen.
- `SqliteStaffingDemandStore` als getrennten Adapter umsetzen.
- Eine neue Migration in der bestehenden gemeinsamen Migrationsfolge generieren.
- Die bestätigten initialen Standardbedarfe einmalig anhand der vorhandenen Diensttypen und ihrer aktuellen Standardzeiten anlegen.
- Eindeutige Schlüssel, Fremdschlüssel, Zeit-, Personenzahl- und Ausnahmebedingungen soweit sinnvoll zusätzlich in SQLite absichern.
- Leere Datenbank sowie Upgrade vom aktuellen System-03-Stand mit synthetisch geänderten Katalog- und Mitarbeiterdaten prüfen.

Prüfung:

- Infrastructure-Tests prüfen Speichern, Laden, Revision, Ausnahme, Aufhebung, Entfernen, Konflikt, Transaktionsgrenze und Round-trip aller Datums-, Zeit- und Minutenwerte.
- Migrationstests starten bei der ältesten veröffentlichten Migration und beim aktuellen System-03-Stand; bestehende Katalog-, Typ- und Mitarbeiterdaten bleiben unverändert lesbar.
- Eine zuvor geänderte Diensttyp-Standardzeit wird bei der erstmaligen Bedarfsanlage übernommen und nicht auf den historischen Startwert zurückgesetzt.
- Architekturtests bestätigen weiterhin genau einen `DbContext`, einen Migrationsordner und die erlaubten Modulreferenzen.

Abnahmebedingung:

- Der Auftraggeber bestätigt Migration, Startwerte und gemeinsame Datenbankgrenze nach bestandenem Upgrade-Nachweis.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- `StandardStaffingDemandRevisionEntity` und `StaffingDemandDateExceptionEntity` liegen mit ihren EF-Konfigurationen ausschließlich unter `Persistence/StaffingDemands`. Der bestehende `ServiceCatalogDbContext` erhielt nur die beiden erforderlichen `DbSet`- und Konfigurationsbindungen.
- Die generierte Migration `20260914144923_AddStaffingDemands` ergänzt die zwei Bedarfstabellen in der bestehenden gemeinsamen Migrationsfolge. Eindeutige Fachschlüssel, restriktive Beziehungen sowie Bedingungen für Änderungsart, Montag, tatsächliche halbstündige Zeit und positive Personenzahl werden zusätzlich durch SQLite geschützt.
- `SqliteStaffingDemandStore` liefert Bedarf und System-04-Katalog in einem gemeinsamen Lesetransaktionsstand. Standardrevisionen werden ausschließlich angehängt; Datumsausnahmen werden gegen die vollständige erwartete Momentaufnahme gespeichert, ersetzt oder entfernt.
- Ein erzwungener Fehler zwischen Löschen und Neuanlegen einer Datumsausnahme weist nach, dass die gemeinsame Transaktion den ursprünglichen Stand vollständig wiederherstellt. Veraltete erwartete Momentaufnahmen überschreiben keine inzwischen gespeicherten Werte.
- Auf einer leeren Datenbank werden die 23 bestätigten Standardbedarfe genau einmal angelegt. Die Werte entstehen nach der Migration aus den zu diesem Zeitpunkt gespeicherten normalen Diensttypen; eine synthetisch geänderte Standardzeit wird deshalb übernommen.
- Die Upgrade-Tests starten getrennt bei der ältesten veröffentlichten System-04-Migration und beim aktuellen System-03-Stand. Synthetisch geänderte Katalogwerte und vorhandene synthetische Mitarbeiterdaten bleiben erhalten und lesbar.
- 9 neue fokussierte BE-07-Infrastructure-Tests bestehen. Insgesamt bestehen 32 Infrastructure-Tests, 16 Architekturtests und alle 321 vorhandenen Tests.
- Gesperrte Paketwiederherstellung, vollständiger Solution-Build mit 0 Warnungen und 0 Fehlern sowie `dotnet format --verify-no-changes` bestehen. Die Architekturprüfung bestätigt weiterhin genau einen `DbContext`, einen Migrationsordner und die erlaubten Modulreferenzen.
- WPF, sichtbare Bedarfsübersicht und Bearbeitung wurden nicht verändert; diese blieben den nachfolgenden UI-Schritten vorbehalten.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat BE-07 ausdrücklich abgenommen und BE-08 freigegeben.

### BE-08 – Wochenübersicht und erste Oberflächengestaltung in WPF umsetzen

Status: `[x]` – umgesetzt, automatisch geprüft und sichtbar abgenommen

Umfang:

- Einen eigenen Reiter „Bedarf“ mit ausgewählter Montag-bis-Sonntag-Woche bereitstellen.
- Bedarfe nach Einsatzort und Wochentag mit Diensttyp, tatsächlicher Zeit, Personenzahl, Quelle und berechneten Stunden anzeigen.
- Tagessummen je Einsatzort, Wochensummen je Einsatzort und die Gesamtsumme der Woche darstellen.
- Lade-, Leer- und technische Fehlerzustände verständlich anzeigen.
- Die atomaren Daten im ViewModel so behalten, dass zusätzliche abgestimmte Summen später ohne Domain-Umbau darstellbar sind.
- Noch keine Bearbeitung ermöglichen.

Prüfung:

- Desktop-Tests prüfen Laden, Wochenwechsel, Gruppierung, Quellenkennzeichnung, alle Summen, leeren Zustand, Fehler und Abbruch.
- Architekturtests bestätigen die Trennung zu Servicekatalog- und Mitarbeiter-ViewModels sowie die Composition-Grenze.
- Die Anwendung kompiliert ohne Warnungen; anschließend wird die erste Gestaltung mit ausschließlich synthetischen Bedarfen sichtbar geprüft.

Abnahmebedingung:

- Der Auftraggeber bestätigt Verständlichkeit, Anordnung und Summen der ersten sichtbaren Bedarfsübersicht oder nennt gezielte Gestaltungsänderungen.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- Der neue Reiter „Bedarf“ zeigt eine ausgewählte Montag-bis-Sonntag-Woche. Ein im Kalender gewähltes Datum wird auf den zugehörigen Montag normalisiert; Schaltflächen wechseln jeweils genau eine Woche vor oder zurück.
- Die Ansicht gruppiert die atomaren Bedarfe zuerst nach Einsatzort und darunter in sieben Tageszeilen. Jeder Bedarf zeigt normalen Diensttyp, tatsächliche Zeit, Personenzahl, berechnete Mitarbeiterstunden und seine Quelle als „Wochenstandard“ oder „Datumsausnahme“.
- Cafeteria und Restaurant behalten ihre gelbe beziehungsweise rote Kennzeichnung und nennen die Farbe zusätzlich als Text. Datumsausnahmen werden neben dem Quellentext zusätzlich visuell hervorgehoben.
- Tagesbedarf je Einsatzort, Wochenbedarf je Einsatzort und der Gesamtbedarf der Woche werden aus der Application-Momentaufnahme dargestellt. Die bestätigte Ausgangswoche zeigt 57 Cafeteria-Stunden, 280 Restaurant-Stunden und insgesamt 337 Stunden.
- `StaffingDemandOverviewViewModel` behält zusätzlich zur Gruppierung alle atomaren Kennungen, Datums-, Zeit-, Personen- und Minutenwerte. Spätere abgestimmte Summen oder die Bearbeitung benötigen dadurch keinen Domain-Umbau.
- Ladezustand, erfolgreicher Leerzustand, strukturierte Application-Fehler, unerwartete technische Fehler, Wiederholen und Abbruch sind verständlich behandelt. Während des Ladens sind widersprüchliche Wochenaktionen deaktiviert.
- Nur `Desktop.Composition` kennt den konkreten `SqliteStaffingDemandStore`, initialisiert ihn und übergibt den Application-Lesevertrag an die Ansicht. Servicekatalog-, Mitarbeiter- und Bedarfs-ViewModels bleiben getrennte Fachbereiche.
- 8 neue fokussierte BE-08-Desktop-Tests bestehen. Insgesamt bestehen 40 Desktop-Tests, 16 Architekturtests und alle 329 vorhandenen Tests.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` besteht.
- Die verfügbare Computer-Use-Anbindung stellt in dieser Sitzung keine nativen Windows-Apps bereit. Der sichtbare WPF-Prüflauf und die Beurteilung der ersten Gestaltung bleiben deshalb ein ausdrücklich offenes manuelles Abnahmegate.
- Standardbedarfe und Datumsausnahmen können in diesem Schritt noch nicht bearbeitet werden; dies blieb den nachfolgenden UI-Schritten vorbehalten.

Abnahme vom 2026-09-14:

- Der Auftraggeber hat die sichtbare Wochenübersicht ausdrücklich bestätigt, BE-08 abgenommen und BE-09 freigegeben.

### BE-09 – Standardbedarfe in WPF bearbeiten

Status: `[x]` – ursprünglicher sichtbarer Entwurf geprüft und verworfen; korrigierter Zielablauf durch BE-09A und BE-09B am 2026-09-15 abgenommen

Umfang:

- Für einen Wochentag und normalen Diensttyp Personenzahl und tatsächliche Zeit bearbeiten.
- Den Wirksamkeitsmontag vor dem Speichern sichtbar auswählen und erklären.
- Fehlenden Standard ergänzen und bestehenden Standard ab der gewählten Woche aufheben können.
- Diensttypen nur innerhalb ihres zugehörigen Einsatzortes anbieten; `D` und `Spr` nicht anbieten.
- Validierungs-, Speicher-, Erfolgs- und Konfliktzustände anzeigen und Eingaben bei einer Ablehnung erhalten.
- Nach erfolgreichem Speichern die ausgewählte Woche aus der gespeicherten Quelle neu laden.

Prüfung:

- Desktop-Tests decken Bearbeiten, Ergänzen, Aufheben, ungültige Zeit, ungültige Personenzahl, falschen Montag, Konflikt, technischen Fehler und erneutes Laden ab.
- Ein sichtbarer synthetischer Ablauf prüft eine zukünftige Revision und die unveränderte frühere Woche.
- Der Build ersetzt den sichtbaren Bediennachweis nicht.

Abnahmebedingung:

- Der Auftraggeber bestätigt den vollständigen sichtbaren Standardänderungsablauf einschließlich Wirksamkeitswoche und Summenaktualisierung.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- Der Reiter „Bedarf“ besitzt nun einen ausdrücklich gestarteten und gespeicherten Bearbeitungsbereich für regelmäßige Wochenstandards. Wirksamkeitsmontag, Einsatzort, Wochentag, normaler Diensttyp, tatsächlicher Beginn, tatsächliches Ende und Personenzahl sind vor dem Speichern sichtbar.
- Die gewählte Wirksamkeitswoche wird bewusst geladen und zugleich in der Wochenübersicht angezeigt. Eine gültige Änderung wird als neue unveränderliche Revision gespeichert; frühere Wochen werden weder überschrieben noch ungefragt neu erzeugt.
- Die Application-Wochenmomentaufnahme enthält für alle normalen Diensttypen eine unveränderliche Bearbeitungsmomentaufnahme. Sie liefert auch bei einem aktuell fehlenden Standard den letzten historischen Revisionsstand einschließlich einer Aufhebung, damit Ergänzen, Ersetzen und Aufheben denselben Konfliktschutz wie BE-06 verwenden.
- Für einen fehlenden Standard werden die aktuellen Diensttyp-Standardzeiten als Ausgangswerte und eine Person als korrigierbarer Startwert angeboten. Diensttypen werden nach Einsatzort gefiltert; `D` und `Spr` sind keine auswählbaren Bedarfsdiensttypen.
- Validierungs-, Speicher-, Erfolgs-, Konflikt- und unerwartete Fehlerzustände werden deutsch angezeigt. Abgelehnte Eingaben bleiben erhalten; nach erfolgreichem Speichern wird die ausgewählte Woche vollständig aus SQLite neu geladen und alle Summen werden aus der gespeicherten Quelle aktualisiert.
- Mehrere Änderungen am selben Wirksamkeitsmontag werden nicht rückwirkend überschrieben. Nach einer gespeicherten Revision fordert die Oberfläche für eine weitere Änderung einen späteren Montag.
- 12 neue fokussierte BE-09-Desktop-Tests bestehen. Insgesamt bestehen 52 Desktop-Tests, 16 Architekturtests und alle 341 vorhandenen Tests.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` und `git diff --check` bestehen.
- Die damalige Zuordnung der Datumsausnahmen zu BE-10 und das manuelle BE-09-Abnahmegate wurden durch den nachfolgenden Sichtprüfungsbefund verworfen und in BE-09A neu geordnet.

Sichtprüfungsbefund vom 2026-09-14:

- Der Auftraggeber hat BE-09 nicht abgenommen. Die Wochenübersicht bleibt bestätigt, der Bearbeitungsablauf wird jedoch neu geordnet.
- Regelmäßige Standards sollen nicht im Reiter „Bedarf“, sondern ausschließlich im Reiter „Einsatzorte und Dienste“ bearbeitet werden.
- Im Reiter „Bedarf“ soll der bisherige Standard-Aufhebungsablauf vollständig durch eine wiederholt bearbeitbare einmalige Änderung eines konkreten Datums ersetzt werden.
- Ein geänderter Tagesbedarf soll in der Wochenansicht verständlich als „Einmalige Änderung“ gekennzeichnet sein.

### BE-09A – Standard- und Einmaländerungsbedienung nach Sichtprüfung neu ordnen

Status: `[x]` – umgesetzt, automatisch geprüft und als Bestandteil des korrigierten Gesamtbereichs am 2026-09-15 sichtbar abgenommen

Ziel und Nutzen:

- Die Service-Leitung findet regelmäßige Grundlagen bei den Einsatzorten und Diensttypen und bearbeitet im Reiter „Bedarf“ nur noch konkrete Kalendertage.
- Die Begriffe „Diensttyp-Standardzeit“, „regelmäßiger Personalbedarf“ und „einmalige Änderung“ bleiben fachlich und sichtbar getrennt.
- Eine einmalige Änderung kann jederzeit erneut bearbeitet oder auf den regelmäßigen Standard zurückgesetzt werden; die bisherige sperrende Meldung über eine unveränderliche Änderung entfällt aus diesem Ablauf.

Bestätigte Bedienstruktur:

- Der Hauptreiter „Einsatzorte und Dienste“ erhält eine übersichtliche Unterteilung in „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ oder eine gleichwertig klare Navigation.
- „Einsatzorte“ behält Name und Farbe des Einsatzortes. „Diensttypen“ behält Bezeichnung, Anzeige und die allgemeine Diensttyp-Standardzeit.
- „Regelmäßiger Bedarf“ bearbeitet getrennt davon den wiederkehrenden Personalbedarf mit Einsatzort, Wochentag, normalem Diensttyp, tatsächlicher Bedarfszeit, Personenzahl und „Wirksam ab“.
- „Wirksam ab“ akzeptiert ausschließlich einen Montag. Frühere Wochen bleiben unverändert; `D` und `Spr` werden weiterhin nicht als einzelne Bedarfsdiensttypen angeboten.
- Der vorhandene Button „Standardzeit speichern“ ändert weiterhin nur die allgemeine Diensttyp-Standardzeit. Änderungen am regelmäßigen Personalbedarf besitzen einen eigenen eindeutig beschrifteten Speichervorgang und werden nicht stillschweigend mit der Diensttyp-Standardzeit gekoppelt.
- Im Hauptreiter „Bedarf“ entfällt der vollständige Editor für regelmäßige Standards. Jede sichtbare Bedarfskarte kann stattdessen den Ablauf „Nur diesen Tag ändern“ öffnen.
- Für eine an diesem Tag fehlende Kombination kann ein einmaliger Bedarf ergänzt werden. Eine vorhandene einmalige Änderung kann beliebig oft erneut bearbeitet und konfliktgeschützt ersetzt werden.
- Innerhalb der einmaligen Änderung bleiben die drei fachlichen Ergebnisse möglich: Zeit oder Personenzahl ändern, für diesen Tag ausdrücklich „kein Bedarf“ festlegen und die einmalige Änderung zurücksetzen, sodass wieder der dann wirksame regelmäßige Standard gilt.
- Eine gespeicherte Datumsausnahme wird in der rechten Wochenansicht mit dem sichtbaren Text „Einmalige Änderung“ gekennzeichnet. Farbe ist weiterhin nicht der einzige Informationsträger.
- Nach jedem erfolgreichen Speichern oder Zurücksetzen wird die ausgewählte Woche aus SQLite neu geladen; Tages-, Einsatzort- und Gesamtsummen werden ausschließlich aus dieser gespeicherten Momentaufnahme aktualisiert.

Verbindliche Architektur- und Codegrenzen:

- Die vorhandenen Domain-Modelle für Standardrevisionen und Datumsausnahmen bleiben die einzige fachliche Definition. Eine neue Migration oder ein zweites Datenmodell ist für diese Bedienkorrektur nicht vorgesehen.
- `ChangeStandardStaffingDemandCommand` bleibt der getrennte Anwendungsfall für regelmäßige Bedarfe. `SaveStaffingDemandDateExceptionCommand` und `RemoveStaffingDemandDateExceptionCommand` bleiben die getrennten Anwendungsfälle für einmalige Änderungen und deren Rücksetzung.
- `MainWindow.xaml` darf beide UI-Fachbereiche anordnen, ohne dass Servicekatalog- und Bedarfs-ViewModels einander direkt referenzieren. Konkrete SQLite-Typen bleiben ausschließlich in `Desktop.Composition`.
- Der vorhandene `StandardStaffingDemandEditorViewModel` wird aus der Wochenansicht herausgelöst und als eigener Bereich für regelmäßige Bedarfe eingebunden; es entsteht keine zweite widersprüchliche Standardbearbeitung.
- `StaffingDemandOverviewViewModel` erhält ausschließlich den Editorzustand für ein konkretes Datum und die Wochenansicht. Technische Begriffe wie „Datumsausnahme“ werden in sichtbaren Texten durch „Einmalige Änderung“ ersetzt, bleiben intern aber eindeutige Codebegriffe.

Bewusst nicht Bestandteil:

- keine automatische Feiertagserkennung,
- keine Änderung an Mitarbeitenden, Planung, Regeln, Excel oder Zeitkonten,
- keine Kopplung, durch die eine Änderung der Diensttyp-Standardzeit ungefragt vorhandene regelmäßige oder einmalige Bedarfe umschreibt,
- keine automatische Speicherung, keine ungefragte Neugenerierung und keine echten Klinikdaten.

Umsetzungsreihenfolge:

1. Die bestehende BE-09-Standardbearbeitung aus dem Reiter „Bedarf“ herauslösen und die neue Unterteilung im Reiter „Einsatzorte und Dienste“ ohne Feature-zu-Feature-Abhängigkeit vorbereiten.
2. Den regelmäßigen Bedarfseditor dort mit Montagspflicht, gefilterten normalen Diensttypen, Personenzahl, tatsächlicher Zeit, Ergänzen, Ändern und eindeutig formulierter dauerhafter Nichtbedarfs-Option anbinden.
3. Im Reiter „Bedarf“ den Ablauf „Nur diesen Tag ändern“ für vorhandene und fehlende Bedarfe einbauen; erneutes Bearbeiten, „kein Bedarf“ und Zurücksetzen über die vorhandenen Application-Befehle anbinden.
4. Quellenanzeige und Summen nach jedem erfolgreichen Neuladen auf „Regelmäßiger Standard“ beziehungsweise „Einmalige Änderung“ abgleichen.
5. Automatische Prüfungen, Dokumentation und anschließend den vollständigen sichtbaren Ablauf mit ausschließlich synthetischen Daten durchführen.

Prüfung:

- Desktop-Tests bestätigen, dass im Reiter „Bedarf“ keine regelmäßige Standardbearbeitung mehr angeboten wird und der neue Bereich im Reiter „Einsatzorte und Dienste“ erreichbar ist.
- Desktop-Tests decken regelmäßiges Ergänzen und Ändern ab einem Montag, Einsatzortfilter, ungültigen Montag, ungültige Zeit und Personenzahl, Konflikt, Fehler und gespeichertes Neuladen ab.
- Desktop-Tests decken eine erste und eine wiederholte einmalige Änderung, das Ergänzen eines fehlenden Tagesbedarfs, „kein Bedarf“, Zurücksetzen, Eingabeerhalt bei Ablehnung, Quellenanzeige „Einmalige Änderung“ und aktualisierte Summen ab.
- Architekturtests sichern die Trennung der UI-Fachbereiche und die alleinige konkrete Adapterverdrahtung in `Desktop.Composition`.
- Vollständiger Solution-Build, relevante Domain-, Application-, Infrastructure-, Desktop- und Architekturtests, `dotnet format --verify-no-changes` und `git diff --check` müssen bestehen.
- Der Build ersetzt nicht den sichtbaren Bediennachweis.

Abnahmebedingung:

- Der Auftraggeber bestätigt zuerst diesen Änderungsplan. Nach der späteren Umsetzung bestätigt er die übersichtliche Unterteilung, die ausschließlich montags wirksame Standardbearbeitung und den vollständigen, wiederholt bearbeitbaren Ablauf „Nur diesen Tag ändern“ einschließlich „Einmalige Änderung“, „kein Bedarf“, Zurücksetzen und Summenaktualisierung.

Umsetzungs- und Prüfnachweis vom 2026-09-14:

- Der Auftraggeber hat den Änderungsplan ausdrücklich bestätigt. Unter „Einsatzorte und Dienste“ stehen nun die getrennten Bereiche „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ zur Auswahl.
- Die vorhandene Schaltfläche „Standardzeit speichern“ bleibt bei den Diensttypen. Der regelmäßige Personalbedarf besitzt einen eigenen Editor mit Einsatzort, Wochentag, normalem Diensttyp, tatsächlicher Zeit, Personenzahl und ausschließlich montags zulässigem „Wirksam ab“.
- Der Reiter „Bedarf“ enthält keine Bearbeitung regelmäßiger Standards mehr. Ein Tag oder eine vorhandene Bedarfskarte öffnet stattdessen „Nur diesen Tag ändern“.
- Eine einmalige Änderung kann einen vorhandenen Bedarf ersetzen, einen sonst fehlenden Bedarf ergänzen, nur an diesem Tag „kein Bedarf“ festlegen und wieder auf den wirksamen regelmäßigen Standard zurückgesetzt werden. Nach jedem Speichern oder Zurücksetzen wird die Woche aus der gespeicherten Quelle neu geladen.
- Eine vorhandene einmalige Änderung bleibt erneut bearbeitbar. Der jeweils zuletzt geladene Ausnahmestand wird als Konfliktschutz verwendet; die frühere sperrende Bedienmeldung gehört nicht mehr zu diesem Ablauf.
- Bedarfskarten und auch Tage mit einer aufhebenden Einmaländerung zeigen den Text „Einmalige Änderung“. Vollständig leere Bedarfswochen behalten ihre Einsatzorte und Tage, damit ein fehlender Bedarf ergänzt werden kann.
- Die vorhandenen Domain-Modelle, Application-Befehle und SQLite-Tabellen werden weiterverwendet. Es wurde keine Migration und kein zweites Datenmodell angelegt.
- 8 neue fokussierte Desktop-Tests und 1 zusätzlicher Architekturtest bestehen. Insgesamt bestehen 60 Desktop-Tests, 17 Architekturtests und alle 350 vorhandenen Tests.
- Der vollständige Solution-Build besteht mit 0 Warnungen und 0 Fehlern; `dotnet format --verify-no-changes` und `git diff --check` bestehen. Die sichtbare WPF-Bedienprüfung bleibt als eigenes Gate offen.

Sichtprüfungsnachtrag vom 2026-09-14:

- Nach einer gespeicherten Änderung eines regelmäßigen Bedarfs sperrt die Oberfläche für denselben Wirksamkeitsmontag jede weitere Änderung. Die Meldung fordert stattdessen einen späteren Montag.
- Der Auftraggeber möchte regelmäßige Bedarfe auch am selben Wirksamkeitsmontag beliebig oft bewusst korrigieren können.
- Im Bereich „Regelmäßiger Bedarf“ soll die rechte Seite den gewählten Einsatzort vollständig von Montag bis Sonntag zeigen. Der Einsatzort soll wie in den Bereichen „Einsatzorte“ und „Diensttypen“ über die linke Einsatzortliste gewählt werden und nicht zusätzlich im rechten Editor.

### BE-09B – Regelmäßigen Wochenbedarf konsistent anzeigen und am selben Montag wiederholt korrigieren

Status: `[x]` – implementiert, automatisch geprüft und am 2026-09-15 sichtbar abgenommen

Ziel und Nutzen:

- Der regelmäßige Bedarf eines links ausgewählten Einsatzortes ist als vollständige Wochenansicht von Montag bis Sonntag unmittelbar erfassbar.
- Die Einsatzortauswahl verhält sich in „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ gleich; eine zweite Einsatzortauswahl im rechten Bedarfsbereich entfällt.
- Ein bereits gespeicherter regelmäßiger Bedarf kann für denselben Wirksamkeitsmontag erneut korrigiert, auf „kein regelmäßiger Bedarf“ gesetzt oder wieder ergänzt werden.
- Frühere Fassungen bleiben unverändert nachvollziehbar. Für jeden Wirksamkeitsmontag gilt stets die zuletzt gespeicherte Korrekturfassung.

Bestätigter Ausgangsbefund vor BE-09B:

- `StandardStaffingDemandEditorViewModel` setzt `HasRevisionAtEffectiveMonday`, zeigt die sperrende Meldung und schließt diesen Zustand in `CanSave` und damit auch in `CanRemove` aus.
- `StaffingDemandWeekSnapshotProjector` kennzeichnet eine bereits vorhandene Revision am gewählten Montag ausdrücklich für diese Sperre.
- `ChangeStandardStaffingDemandCommand` lehnt eine zweite Revision desselben Bedarfsschlüssels und Wirksamkeitsmontags als Konflikt ab.
- `StandardStaffingDemandRevisionSet` bewertet doppelte Schlüssel am selben Wirksamkeitsmontag als ungültig. Der eindeutige SQLite-Index und `SqliteStaffingDemandStore` sichern dieselbe Einschränkung zusätzlich technisch ab.
- Die gewünschte Änderung benötigt daher eine abgestimmte Anpassung in Domain, Application, Infrastructure und Desktop; das Entfernen der sichtbaren Sperre allein wäre fachlich und technisch unvollständig.

Empfohlenes fachliches und technisches Modell:

- `StandardStaffingDemandRevision` erhält eine positive, pro Bedarfsschlüssel und Wirksamkeitsmontag fortlaufende Korrekturfolge. Vorhandene Revisionen werden bei der Migration mit Folge `1` übernommen.
- Jede weitere bewusste Speicherung für denselben Montag hängt eine neue unveränderliche Revision mit der nächsten Folge an. Vorherige Fassungen werden weder aktualisiert noch gelöscht.
- Für die fachliche Wochenauflösung wird je Bedarfsschlüssel und Wirksamkeitsmontag ausschließlich die höchste Korrekturfolge berücksichtigt. Danach gelten weiterhin die zeitlichen Revisionen der aufeinanderfolgenden Montage.
- Der Konfliktschutz bleibt erhalten: Der Schreibablauf erwartet die zuletzt geladene aktuelle Revision. Wurde sie zwischenzeitlich verändert, wird weiterhin ein verständlicher Konflikt mit anschließendem Neuladen angezeigt; dies ist keine dauerhafte Bearbeitungssperre.
- Eine Korrektur an einem früheren Montag verändert keine bereits gespeicherte spätere Korrektur. Die gesamte daraus entstehende zeitliche Folge wird vor dem Speichern fachlich validiert; widersprüchliche spätere Übergänge werden sichtbar abgelehnt und nicht stillschweigend umgeschrieben.
- Der vorhandene veröffentlichte Migrationsstand wird nicht verändert. Eine neue Migration erweitert die Tabelle, übernimmt bestehende Daten verlustfrei und ersetzt den bisherigen eindeutigen Index durch einen eindeutigen Schlüssel aus Bedarfsschlüssel, Wirksamkeitsmontag und Korrekturfolge.

Verbindliche Bedienstruktur:

- Die linke Einsatzortliste bleibt in allen drei Unterbereichen sichtbar und bestimmt den aktuell bearbeiteten Einsatzort.
- Beim Wechsel des Einsatzortes lädt „Regelmäßiger Bedarf“ dessen sieben Tageszeilen Montag bis Sonntag für den gewählten Wirksamkeitsmontag neu.
- Die rechte Ansicht besitzt keine zweite Einsatzortauswahl. Jede Tageszeile zeigt die normalen Diensttypen des links gewählten Einsatzortes, tatsächliche Zeit, Personenzahl und den aktuellen Zustand.
- Tage ohne regelmäßigen Bedarf bleiben sichtbar, damit dort ein Bedarf ergänzt werden kann. Mehrere normale Diensttypen desselben Tages bleiben getrennte Bedarfe.
- Speichern und „Ab diesem Montag kein regelmäßiger Bedarf“ bleiben nach erfolgreichem Neuladen erneut verfügbar. Eine vorhandene Fassung darf beliebig oft weiterbearbeitet werden.
- Die bisherige sperrende Meldung entfällt. Stattdessen darf ein neutraler Hinweis erklären, dass eine weitere Speicherung die wirksame Fassung ab diesem Montag korrigiert und die ältere Fassung intern erhalten bleibt.
- Erfolgreiches Speichern lädt Wochenansicht und Summen aus SQLite neu. Validierungsfehler erhalten die Eingaben; ein echter Nebenläufigkeitskonflikt verlangt ein bewusstes Neuladen.

Verbindliche Code-Anker und umgesetzte Änderungen:

- Domain: `StandardStaffingDemandRevision`, `StandardStaffingDemandRevisionSet` und ihre Validierung um Korrekturfolge, Eindeutigkeit und Auflösung der jeweils neuesten Montagsfassung erweitern.
- Application: `ChangeStandardStaffingDemandCommand`, Schreibanforderung, Bearbeitungsmomentaufnahme und `StaffingDemandWeekSnapshotProjector` auf den erwarteten aktuellen Revisionsstand statt einer generellen Montagssperre umstellen.
- Infrastructure: Entität, EF-Konfiguration, Model-Snapshot und `SqliteStaffingDemandStore` erweitern; eine neue Migration nach `20260914144923_AddStaffingDemands` erzeugen und Upgradepfade prüfen.
- Desktop: `StandardStaffingDemandEditorViewModel` und `StandardStaffingDemandEditorView` auf wiederholtes Speichern sowie die durch die linke Auswahl gesteuerte Montag-bis-Sonntag-Ansicht umstellen. Die vorhandene Trennung der ViewModels und die Adapterverdrahtung ausschließlich in `Desktop.Composition` bleiben bestehen.

Bewusst nicht Bestandteil:

- keine Änderung der bestätigten Cafeteria- und Restaurant-Ausgangsbedarfe,
- keine Änderung an einmaligen Tagesänderungen im Reiter „Bedarf“,
- keine automatische Feiertagserkennung, automatische Speicherung oder ungefragte Neugenerierung,
- keine neuen Einsatzorte, Diensttypen oder frei zusammengestellten Zeitblöcke,
- keine Änderungen an Mitarbeitenden, Planung, Regeln, Excel oder Zeitkonten,
- keine echten Klinikdaten und kein Umschreiben veröffentlichter Migrationen.

Umsetzungsreihenfolge:

1. Domain-Modell und Tests für fortlaufende Korrekturfassungen desselben Wirksamkeitsmontags ergänzen; die wirksame Montagsfassung deterministisch auflösen.
2. Application-Schreib- und Leseverträge auf die erwartete aktuelle Revision umstellen und wiederholtes Ändern, Aufheben und Ergänzen sowie echte Nebenläufigkeitskonflikte testen.
3. Neue SQLite-Migration und Speicherlogik ergänzen; leere sowie bestehende synthetische Datenbanken verlustfrei aktualisieren und mehrere Fassungen desselben Montags dauerhaft laden.
4. Die linke Einsatzortauswahl mit dem regelmäßigen Bedarfsbereich verbinden und rechts eine vollständige Montag-bis-Sonntag-Ansicht ohne zweite Einsatzortauswahl darstellen.
5. Sperrlogik und Sperrmeldung entfernen, wiederholte Bedienabläufe anbinden und nach jedem Erfolg aus der gespeicherten Quelle neu laden.
6. Vollständige automatische Prüfungen und anschließend einen sichtbaren synthetischen Bediennachweis durchführen; Dokumentation auf den geprüften Stand bringen.

Prüfung:

- Domain-Tests decken mehrere Korrekturfolgen desselben Schlüssels und Montags, die Auswahl der neuesten Fassung, frühere und spätere Montage sowie widersprüchliche Folgen ab.
- Application-Tests decken wiederholtes Ändern, „kein regelmäßiger Bedarf“ und erneutes Ergänzen am selben Montag sowie einen echten Konflikt bei veraltetem erwarteten Revisionsstand ab.
- Migrationstests prüfen eine leere Datenbank und das Upgrade vorhandener synthetischer System-05-Daten. Bestehende Revisionen, Datumsausnahmen, Katalog- und Mitarbeiterdaten bleiben erhalten.
- Infrastructure-Tests sichern fortlaufende Folgen, den neuen eindeutigen Index, atomare Speicherung und Rollback bei Fehlern.
- Desktop-Tests prüfen die linke Einsatzortauswahl, den Wechsel zwischen Cafeteria und Restaurant, sieben sichtbare Wochentage, fehlende Bedarfe, gefilterte Diensttypen und mehrfaches Speichern desselben Montags.
- Architekturtests sichern die bestehenden Modulgrenzen. Vollständiger Solution-Build, alle relevanten Tests, `dotnet format --verify-no-changes` und `git diff --check` müssen bestehen.
- Der Auftraggeber prüft sichtbar beide Einsatzorte, die vollständige Woche und mindestens zwei aufeinanderfolgende Korrekturen desselben Bedarfs am selben Wirksamkeitsmontag einschließlich Neuladen.

Abnahmebedingung:

- Der Auftraggeber hat diesen BE-09B-Plan einschließlich der neuen Korrekturfolge und Migration vor der Implementierung ausdrücklich bestätigt.
- Der Auftraggeber hat am 2026-09-15 bestätigt, dass die konsistente linke Einsatzortführung, die vollständige Montag-bis-Sonntag-Ansicht und die wiederholte Änderung desselben Wirksamkeitsmontags zunächst passend sind. Spätere Änderungswünsche der Service-Leitung werden als neuer abgestimmter Umfang behandelt.

Umsetzungsnachweis vom 2026-09-14:

- `StandardStaffingDemandRevision` besitzt nun eine positive Korrekturfolge. Die Revisionsmenge erlaubt mehrere unveränderliche Fassungen desselben Bedarfsschlüssels und Wirksamkeitsmontags, verlangt lückenlose Folgen und löst stets die höchste Folge als wirksam auf.
- `ChangeStandardStaffingDemandCommand` und der SQLite-Speicher vergleichen weiterhin die zuletzt geladene aktuelle Revision. Eine weitere bewusste Speicherung am selben Montag erhält die nächste Folge; ein tatsächlich zwischenzeitlich geänderter Stand bleibt ein sichtbarer Konflikt.
- Die neue Migration `20260914185047_AddStandardDemandCorrectionSequence` übernimmt bestehende Revisionen mit Folge `1`, ergänzt die Datenbankbedingung für positive Folgen und erweitert den eindeutigen Index verlustfrei.
- Der regelmäßige Bedarfsbereich zeigt für den links gewählten Einsatzort Montag bis Sonntag mit allen normalen Diensttypen, Zeiten, Personenzahlen und fehlenden Bedarfen. Die frühere zweite Einsatzortauswahl im rechten Editor ist entfernt; eine Tages-/Diensttypzeile kann direkt zur Bearbeitung gewählt werden.
- Die automatische Prüfung umfasst 148 Domain-, 98 Application-, 34 Infrastructure-, 64 Desktop- und 17 Architekturtests. Insgesamt bestehen 361 Tests; Build, Format-, Migrations- und Diff-Prüfung werden im Abschlussbericht getrennt ausgewiesen.
- Die sichtbare Bedienprüfung wurde am 2026-09-15 ausdrücklich abgenommen.

### BE-10 – System 05 gemeinsam abschließen

Status: `[x]` – Abschlussprüfung bestanden und Roadmap am 2026-09-15 archiviert

Umfang:

- Domain, Application, Infrastructure, Desktop und Architekturtests im vollständigen System-05-Umfang gemeinsam prüfen.
- Die Übergaben an System 08, 09, 10, 11 und 12 dokumentarisch abgleichen, ohne diese Systeme zu implementieren.
- Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` auf denselben tatsächlichen Stand bringen.
- Datenschutz-, Artefakt-, Pfad- und Whitespace-Prüfung durchführen.
- Roadmap erst nach ausdrücklicher Abnahme nach `docs/roadmaps/completed` verschieben und alle direkten Verweise im selben Schritt aktualisieren.

Prüfung:

- `dotnet restore Salztal.Dienstplanung.sln --locked-mode`
- `dotnet build Salztal.Dienstplanung.sln --no-restore`
- vollständige relevante Domain-, Application-, Infrastructure-, Desktop- und Architekturtests
- Suche nach veralteten Roadmap-Pfaden, widersprüchlichen Statusangaben, echten Daten und unerwünschten Build- oder Datenbankartefakten
- `git diff --check`
- Die sichtbare Wochenübersicht aus BE-08 und die korrigierten Abläufe aus BE-09A und BE-09B wurden schrittweise vom Auftraggeber bestätigt.

Abnahmebedingung:

- Der Auftraggeber bestätigt System 05 und erlaubt die Archivierung dieser Roadmap sowie die Auswahl des nächsten Systems.

Abschlussnachweis vom 2026-09-15:

- Der Auftraggeber hat die korrigierten sichtbaren Abläufe aus BE-09A und BE-09B ausdrücklich abgenommen und den Abschluss der Roadmap freigegeben.
- `dotnet restore Salztal.Dienstplanung.sln --locked-mode` ist erfolgreich.
- Der vollständige Solution-Build ist mit 0 Warnungen und 0 Fehlern erfolgreich.
- Alle 361 vorhandenen Tests bestehen: 148 Domain-, 98 Application-, 34 Infrastructure-, 64 Desktop- und 17 Architekturtests. Die noch leeren Planning- und Excel-Testprojekte werden im Gesamtlauf wie vorgesehen behandelt.
- `dotnet format --verify-no-changes`, der EF-Abgleich ohne ausstehende Modelländerung und `git diff --check` bestehen.
- Die Suche fand keine Datenbanken, Sicherungen, Exporte oder auffälligen Bilddateien im Repository. Es wurden keine echten Mitarbeiter- oder Plandaten hinzugefügt.
- Die Übergaben an Systeme 08 bis 12 bleiben unverändert dokumentiert; Planning, Regeln, Planmodell, Konflikterklärung, Planansichten und Planversionen wurden nicht vorgezogen.

## Übergaben an spätere Systeme

### System 08 – Planmodell und Planungszeiträume

- erhält die wirksamen atomaren Bedarfe einer ausgewählten Woche mit Datum, Einsatzort, normalem Diensttyp, tatsächlicher Zeit, Personenzahl und berechneten Minuten,
- erhält keine EF-Entities, Datenbankabfragen oder veränderlichen UI-Objekte,
- kann den damaligen Bedarf später als Planmomentaufnahme übernehmen, ohne zukünftige Standardrevisionen nachzuladen.

### System 09 – Automatische Plangenerierung

- verwendet ausschließlich vorher aufgelöste normale Bedarfe und darf weder Diensttypen noch Zeiten erfinden,
- behandelt die Personenzahl als Obergrenze; automatische Überbesetzung bleibt unzulässig,
- verbindet Doppeldienst und Springer später aus getrennten normalen Bedarfen, ohne System-05-Daten umzudeuten.

### System 10 und 11 – Konflikte und Planansichten

- können vollständig oder teilweise ungedeckte Zeiträume gegen die tatsächlichen Bedarfszeiten erklären und darstellen,
- können die in System 05 vorbereiteten atomaren Werte und Summen für weitere abgestimmte Ansichten nutzen.

### System 12 – Planversionen

- bewahrt den für eine Abnahme verwendeten Bedarf als unveränderliche Momentaufnahme,
- bleibt unabhängig von späteren Standardrevisionen und Datumsausnahmen.

## Echte externe und manuelle Gates

- BE-01 und jeder spätere Schritt benötigen ausdrückliche Abnahme; Schweigen ist keine Zustimmung.
- Vor jeder Abnahme und vor einem beauftragten Commit wird die Dateiliste auf echte Mitarbeiter-, Plan- und andere sensible Klinikdaten geprüft.
- Migration und Datenbanktests verwenden ausschließlich temporäre synthetische SQLite-Dateien.
- Die sichtbare WPF-Wochenübersicht aus BE-08 sowie die korrigierten Standard- und Einmaländerungsabläufe aus BE-09A und BE-09B wurden manuell geprüft und ausdrücklich abgenommen.
- Ein erfolgreicher Build oder ViewModel-Test ersetzt keinen sichtbaren WPF-Test.
- Ein lokaler Start ersetzt nicht die spätere portable Windows-11-Prüfung.
- OR-Tools, Excel-Vorlage, portable Ausgabe und fachliche Endabnahme sind für System 05 nicht betroffen und bleiben spätere offene Gates.

## Berichtsschema nach jedem Schritt

Der Abschlussbericht nennt:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. kritische Meldungen mit Auswirkung und Dringlichkeit; wenn keine bestehen, wird dies ausdrücklich genannt,
5. konkreten Handlungsbedarf des Auftraggebers; wenn keiner besteht, wird dies ausdrücklich genannt,
6. offene Gates, Risiken oder Blockaden,
7. Git-Status; Commit und Push nur nach ausdrücklichem Auftrag,
8. nächsten minimalen Schritt,
9. Bitte um ausdrückliche Abnahme.

## Nächster minimaler Schritt

System 05 ist abgeschlossen. Als nächstes System wird gemäß Master-Roadmap System 06 „Verfügbarkeiten und Abwesenheiten“ empfohlen. Vor jeder Implementierung wird dafür zuerst eine eigene Teil-Roadmap mit den noch offenen Fachentscheidungen erstellt und ausdrücklich abgenommen.
