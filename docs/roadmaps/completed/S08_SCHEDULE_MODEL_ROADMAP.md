# Teil-Roadmap System 08 – Planmodell und Planungszeiträume

Status: System 08 und PM-01 bis PM-10 am 2026-09-16 ausdrücklich abgenommen und archiviert

Stand: 2026-09-16

## Ziel und Nutzen

Diese Roadmap beschreibt System 08 „Planmodell und Planungszeiträume“.

System 08 schafft den fachlichen und technischen Rahmen, in dem ein Dienstplan für genau drei vollständige Montag-bis-Sonntag-Wochen vorbereitet, als ein aktueller Entwurf lokal gespeichert und später von der automatischen Planung verwendet werden kann. Es verbindet die bereits vorhandenen Mitarbeitenden, Typen, Einsatzfreigaben, tatsächlichen Bedarfe, Tageskennzeichen und den festen Regelkatalog zu einer bewusst erzeugten unveränderlichen Planungsmomentaufnahme.

Die Service-Leitung kann in der bestehenden Ansicht „Dienstplan SER“ vor der Generierung Typ1-Dienste einschließlich der strukturierten Bürokennzeichnung `B` eintragen. Der Vorbereitungsstand wird sichtbar. Geänderte Eingangsdaten ersetzen eine vorhandene Momentaufnahme nicht stillschweigend, sondern machen sie nach einem strukturierten Vergleich als veraltet kenntlich und verlangen eine bewusste Aktualisierung.

System 08 liefert damit eine modulare Übergabe:

- System 09 erhält ein unveränderliches, vollständig strukturiertes Planungsinput und ein Planmodell, ohne UI- oder Datenbankobjekte zu kennen.
- System 10 kann später auf denselben stabilen Fachkennungen und Deckungszuständen aufbauen.
- System 11 ergänzt allgemeine manuelle Bearbeitung, bedienbare Einzelsperren und bestätigte Abweichungen, ohne das Grundmodell neu zu erfinden.
- System 12 überführt einen Entwurf bei bewusster Abnahme in unveränderliche Planversionen.

System 08 erzeugt noch keinen automatischen Plan, formuliert noch keine Konflikterklärungen und nimmt keinen Plan ab.

## Verbindliche Grundlagen

- `AGENTS.md`
- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`
- `ARCHITECTURE.md`
- `CLEANCODE.md`
- `MASTER_ROADMAP.md`
- `STATUS.md`
- `Service-Leitung/README.md`
- `Service-Leitung/AKTUELLER_STAND.md`
- `Service-Leitung/GRUNDLAGEN_UND_ENTSCHEIDUNGEN.md`
- `docs/roadmaps/completed/S08_SCHEDULE_MODEL_QUESTIONS.md`
- `docs/roadmaps/completed/S03_EMPLOYEES_EMPLOYEE_TYPES_SHIFT_ELIGIBILITY_ROADMAP.md`
- `docs/roadmaps/completed/S05_STAFFING_DEMAND_ROADMAP.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_ROADMAP.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md`
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/decisions/S04_SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_EXAMPLES.md`
- ausschließlich erfundene Personen und synthetische Planungsdaten in Dokumentation und Tests

Die zehn Fachfragen für System 08 sind beantwortet. Es besteht keine offene fachliche Frage, die den Roadmap-Entwurf verhindert. Diese Roadmap wurde am 2026-09-16 ausdrücklich abgenommen.

## Abhängigkeiten und Voraussetzungen

- System 03 liefert stabile Mitarbeitenden- und Mitarbeitertypkennungen, Planungsrollen und strukturierte Einsatzfreigaben.
- System 04 liefert stabile Einsatzort-, Diensttyp- und Einsatzmusterkennungen sowie die Definitionen von `D` und `Spr`.
- System 05 liefert tatsächliche Bedarfszeiten und positive Personenzahlen.
- System 06 liefert `U`, `K`, rote `X`, die 21-Tage-Ansicht und das wirksame Wochen-Soll.
- System 07 liefert Katalogversion 1 mit allen 28 unveränderlichen Regeldefinitionen und dem strukturierten Ergebnisvokabular.
- Die Systeme 03 bis 07 sind abgeschlossen und archiviert.
- Die bestehende gemeinsame SQLite-Migrationsfolge und der einzige gemeinsame `ServiceCatalogDbContext` bleiben verbindlich.

## Bestätigte fachliche Grundlagen

### Planungszeitraum und Entwurf

- Ein Planungszeitraum umfasst immer genau 21 Kalendertage.
- Er beginnt an einem Montag und endet am dritten folgenden Sonntag.
- Für einen Zeitraum existiert genau ein aktueller bearbeitbarer Entwurf.
- Ein neuer Zeitraum darf keinen bereits gespeicherten Zeitraum auch nur teilweise überschneiden.
- Wird genau ein vorhandener Zeitraum ausgewählt, wird dessen Entwurf geöffnet; es entsteht kein zweiter Plan.
- Zwischenspeicherungen verändern denselben Entwurf und erzeugen keine Planversion.
- System 08 löscht Entwürfe weder automatisch noch aufgrund einer Aufbewahrungsfrist.

### Bedarfsplätze und tatsächliche Zeiten

- Jeder aufgelöste Bedarf wird entsprechend seiner positiven Personenzahl deterministisch in einzelne Bedarfsplätze zerlegt.
- Jeder Bedarfsplatz behält Datum, Einsatzort, normalen Diensttyp und die tatsächliche Anfangs- und Endzeit des aufgelösten Bedarfs.
- Ein normaler Dienst deckt genau einen Bedarfsplatz vollständig oder gar nicht.
- Frei erfundene Dienstzeiten und frei wählbare Teildeckungen sind unzulässig.
- Nur das bestätigte `Spr`-Muster darf den Restaurant-Spätdienst ab dem tatsächlichen Wechselzeitpunkt teilweise decken. Der frühere Zeitraum bleibt strukturiert ungedeckt.
- Eine manuelle Zusatzbesetzung ist als eigener später nutzbarer Zuweisungsfall modellierbar, verbraucht aber keinen zusätzlichen Bedarfsplatz und verändert den gespeicherten Bedarf nicht.

### Typ1 und Bürokennzeichnung

- Typ1 wird ausschließlich bewusst vorgetragen und niemals automatisch eingeplant.
- Eine normale Typ1-Zuweisung referenziert genau einen vorhandenen Bedarfsplatz und übernimmt dessen vollständige tatsächliche Zeit.
- `D` und `Spr` referenzieren ihre benötigten vorhandenen Bedarfsplätze beziehungsweise Teilabschnitte als ein Tagesmuster.
- `B` ist nur die strukturierte Kennzeichnung eines vorgetragenen Restaurant-Früh- oder Restaurant-Spätdienstes. Es ist weder Diensttyp noch Bedarf.
- Eine mit `B` gekennzeichnete Typ1-Zuweisung zählt mit ihrer tatsächlichen Zeit zu den Typ1-Stunden, deckt den referenzierten Bedarfsplatz aber nicht.
- Ein Typ1-Dienst ohne entsprechenden vorhandenen Bedarf ist unzulässig.
- In jeder Typ1-Woche, die nicht vollständig durch zulässige Abwesenheiten blockiert ist, muss vor der Generierung mindestens ein gültiger normaler Dienst, ein gültiges Muster `D` oder `Spr` oder ein gültiger `B`-Dienst vorgetragen sein.
- `U`, `K`, ein rotes `X` und ein leeres Feld erfüllen diese Voraussetzung nicht.

### Planungsmomentaufnahme

- Die Service-Leitung erzeugt die Momentaufnahme ausdrücklich mit „Planung vorbereiten“.
- Die Momentaufnahme enthält die für diesen Zeitraum verwendeten Mitarbeitenden, Typfassungen, Einsatzfreigaben, tatsächlichen Bedarfsplätze, Tageskennzeichen, Typ1-Zuweisungen, Regelkatalogversion, vollständigen Regeldefinitionen und Laufoptionen.
- Sie enthält außerdem bis zu sieben unmittelbar vorhergehende Kalendertage aus lokal gespeicherten Plänen.
- Fehlende Vorgeschichte blockiert die Vorbereitung nicht. Umfang und Lücken werden strukturiert festgehalten, damit `MAX_CONSECUTIVE_WORKDAYS` später ehrlich „nicht vollständig prüfbar“ liefern kann.
- Die erzeugte Momentaufnahme wird nicht nachträglich verändert.
- Aktuelle Eingangsdaten werden mit ihr nach fachlichen Komponenten verglichen. Änderungen werden sichtbar, aber nicht automatisch übernommen.
- Eine bewusste Aktualisierung erzeugt eine neue Momentaufnahme für denselben Entwurf. Noch gültige Typ1-Zuweisungen bleiben erhalten; ungültig gewordene Zuweisungen blockieren die Aktualisierung mit einer strukturierten Liste und werden nicht stillschweigend gelöscht oder übernommen.

## Umfang dieser Roadmap

- fachlicher Bereich `Domain/Scheduling` für Zeitraum, atomare Bedarfsplätze, Arbeitsabschnitte, Zuweisungen, Deckungswirkung, Entwurf und zukünftige Planzustände,
- exakt drei Wochen und fachliche Überschneidungsprüfung,
- deterministische Zerlegung tatsächlicher Bedarfe in einzelne Plätze,
- fachlich ausdrückbare normale Vollbesetzung, `Spr`-Teildeckung, schwarze `X`, Zuweisungssperren und manuelle Zusatzbesetzung,
- vollständige Typ1-Zuweisungen einschließlich `D`, `Spr` und `B`,
- ein aktueller Entwurf je Zeitraum mit optimistischer Änderungskontrolle,
- unveränderliche Application-Verträge für Arbeitsansicht und Planungsmomentaufnahme,
- schmale, an Anwendungsfällen ausgerichtete Lese- und Schreibports,
- koordinierte und atomare Wechsel zwischen Typ1-Zuweisung und `U`, `K` oder rotem `X`,
- strukturierte Erkennung veralteter Eingangsdaten nach betroffenen Kategorien,
- bis zu sieben Tage gespeicherte Vorgeschichte mit Vollständigkeitsangabe,
- lokale Speicherung in der gemeinsamen SQLite-Datenbank mit einer neuen Migration,
- modulare Weiterentwicklung der bestehenden Ansicht „Dienstplan SER“,
- fokussierte Domain-, Application-, Infrastructure-, Desktop- und Architekturtests,
- Gesamtnachweis und klare Übergaben an Systeme 09 bis 12.

## Nicht-Umfang

- OR-Tools-Variablen, Solverbedingungen oder automatische Plangenerierung,
- automatisch erzeugte Dienste oder schwarze `X`,
- tatsächliche Optimierung oder Bewertung aller 28 Regeln gegen einen vollständigen Plan,
- strukturierte Konfliktdiagnose, deutsche Konflikterklärungen oder Lösungsvorschläge,
- allgemeiner manueller Bearbeitungsmodus für normale Mitarbeitende,
- bedienbares Setzen oder Lösen von Einzelsperren,
- bedienbare manuelle Zusatzbesetzungen,
- Warnungs- und Bestätigungsabläufe für allgemeine manuelle Regelabweichungen,
- unveränderliche Planversionen, Abnahme oder Wiederherstellung älterer Fassungen,
- Excel-Export,
- automatische Löschung oder einstellbare Aufbewahrungsfrist,
- Zeitkonten,
- frei einstellbare Zeitraumlänge,
- manuelle Eingabe einer Vorgeschichte,
- neue NuGet-Pakete oder externe Werkzeuge.

Schwarze `X`, Sperren und Zusatzbesetzungen werden in System 08 nur so fachlich modelliert, dass spätere Systeme sie ohne Modellbruch verwenden können. Es gibt dafür in System 08 noch keinen sichtbaren Schreibablauf.

## Modularitätsvertrag

System 08 wird nicht als ein großer „Dienstplan-Manager“ umgesetzt. Die Verantwortungen bleiben getrennt:

| Baustein | Eine Verantwortung | Darf nicht übernehmen |
|---|---|---|
| `Domain/Scheduling` | Planungsinvarianten und wertbasierte Fachmodelle | Datenbank, UI-Texte, Solver oder Anwendungsabläufe |
| `Application/Scheduling` | Anwendungsfälle, Momentaufnahme, Vergleich und schmale Ports | EF-Entitäten, WPF-Zustand oder OR-Tools-Typen |
| `Infrastructure/Persistence/Scheduling` | atomare SQLite-Lese- und Schreibvorgänge | fachliche Entscheidungen oder deutsche Meldungen |
| `Desktop/Features/Scheduling` | Darstellung und Bedienzustand der Ansicht „Dienstplan SER“ | direkte Datenbankzugriffe oder eigene Planungsregeln |
| `Desktop/Composition` | konkrete Verdrahtung | fachliche Logik |
| `Planning` | bleibt in System 08 unverändert | vorgezogene Solverübersetzung |

Zusätzlich gelten folgende Schutzregeln:

- Es entsteht kein allgemeines `ISchedulingRepository` mit beliebigem CRUD. Jeder Port beschreibt genau einen Lese- oder Schreibzweck.
- Ein konkreter SQLite-Adapter darf mehrere schmale Ports implementieren, wenn die Application-Verträge getrennt bleiben.
- Der 21-Tage-Arbeitsstand, der gespeicherte Entwurf und das Solver-Input sind getrennte unveränderliche Modelle. UI-Snapshots werden nicht als Solver-Input wiederverwendet.
- `EffectiveStaffingDemand.RequiredEmployeeCount` wird beim Aufbau des Planungsinputs einmal deterministisch in atomare Plätze zerlegt; UI und spätere Solverübersetzung erfinden keine eigenen Platznummern.
- Bestehende Fachwerte für Personen, Typen, Diensttypen, Muster, Bedarfe, Abwesenheiten und Regeln werden referenziert oder unveränderlich kopiert, nicht in `Scheduling` neu definiert.
- Die vollständige Planungsmomentaufnahme wird in einer konsistenten SQLite-Lesetransaktion aufgebaut. Der feste Regelkatalog wird anschließend in Application aus seiner einzigen Domain-Quelle ergänzt.
- Veränderungsvergleiche liefern strukturierte Kategorien wie Mitarbeitende/Typen, Einsatzfreigaben, Bedarfe, Tageskennzeichen, Typ1-Zuweisungen, Regeln und Laufoptionen. Ein bloßes unsichtbares Hash-Ergebnis genügt nicht.
- Das WPF-Feature wird in kleine ViewModels für Zeitraum, Rasterzelle, Typ1-Auswahl und Vorbereitungsstatus zerlegt. Das Übersichts-ViewModel koordiniert, dupliziert aber keine Fachvalidierung.
- Nach der Überführung der vorhandenen WPF-Dateien gibt es keine parallelen alten und neuen Ansichten oder Namespaces.
- Architekturtests entdecken Desktop-Featuregrenzen vollständig; die Prüfung bleibt nicht auf eine handgepflegte Teilmenge bestehender Features beschränkt.

## Explizite Code-Anker des aktuellen Codes

### Vorhandene Domain-Anker

| Aktueller Code-Anker | Verwendung in System 08 |
|---|---|
| `src/Salztal.Dienstplanung.Domain/StaffingDemands/EffectiveStaffingDemand.cs` | liefert Datum, Einsatzort, normalen Diensttyp, tatsächliche Zeit und Personenzahl für atomare Bedarfsplätze |
| `src/Salztal.Dienstplanung.Domain/StaffingDemands/StaffingDemandWeek.cs` | bleibt die einzige Auflösung von Standardrevisionen und Datumsausnahmen; Scheduling dupliziert diese Logik nicht |
| `src/Salztal.Dienstplanung.Domain/Availabilities/AvailabilityEntry.cs` | liefert strukturierte `U`-, `K`- und rote-X-Einträge |
| `src/Salztal.Dienstplanung.Domain/Availabilities/AvailabilityEntrySet.cs` | schützt die Eindeutigkeit je Person und Datum |
| `src/Salztal.Dienstplanung.Domain/Availabilities/EffectiveWeeklyWorkTarget.cs` | bleibt Quelle des wirksamen Wochen-Solls |
| `src/Salztal.Dienstplanung.Domain/Employees/Employee.cs` | liefert stabile Personenkennung und Typbezug |
| `src/Salztal.Dienstplanung.Domain/Employees/EmployeeTypePlanningPolicy.cs` | unterscheidet normale, Typ1- und AH-Planungsrollen ohne Namens- oder Codevergleich |
| `src/Salztal.Dienstplanung.Domain/Employees/EmployeeTypeShiftEligibility.cs` | liefert die strukturierte Freigabe und Aktivierungsart je Dienst oder Muster |
| `src/Salztal.Dienstplanung.Domain/ShiftPatterns/SplitShiftPattern.cs` | definiert `D` aus Restaurant-Früh- und Spätdienst |
| `src/Salztal.Dienstplanung.Domain/ShiftPatterns/ReliefShiftPattern.cs` | definiert `Spr`, Samstag und den Wechsel am Ende des ersten tatsächlichen Bedarfs |
| `src/Salztal.Dienstplanung.Domain/Rules/RuleCatalog.cs` und `InitialRuleCatalog.cs` | bleiben die einzige Quelle der vollständigen Katalogversion 1 |

### Vorhandene Application-Anker

| Aktueller Code-Anker | Verwendung oder bewusste Ablösung |
|---|---|
| `src/Salztal.Dienstplanung.Application/Availabilities/GetAvailabilityPeriodQuery.cs` | liefert heute die 21-Tage-Projektion; die neue Scheduling-Arbeitsansicht übernimmt die Zeitraumskonstante und die bestätigte Sollberechnung, verwendet aber einen eigenen konsistenten Gesamtlesevertrag |
| `src/Salztal.Dienstplanung.Application/Availabilities/SaveAvailabilityEntryCommand.cs` | bleibt fachlich für reine Tageseinträge gültig; die Dienstplan-UI verwendet künftig den koordinierten Scheduling-Ablauf, sobald Typ1 betroffen sein kann |
| `src/Salztal.Dienstplanung.Application/Availabilities/RemoveAvailabilityEntryCommand.cs` | bleibt schmal; das Entfernen im gemeinsamen Raster wird über den Scheduling-Anwendungsfall koordiniert |
| `src/Salztal.Dienstplanung.Application/StaffingDemands/GetStaffingDemandWeekQuery.cs` | bleibt die Bedarfsansicht für eine Woche; Planvorbereitung erhält einen eigenen Drei-Wochen-Leseablauf und ruft nicht drei UI-Queries zusammen |
| `src/Salztal.Dienstplanung.Application/StaffingDemands/StaffingDemandWeekSnapshotProjector.cs` | zeigt das bestehende Projektionsmuster; atomare Bedarfsplätze werden separat für Scheduling projiziert |
| `src/Salztal.Dienstplanung.Application/Employees/EmployeeReadData.cs` | vorhandenes unveränderliches Rohdatenmuster für Personen, Typen und Katalogbezüge |
| `src/Salztal.Dienstplanung.Application/Rules/GetRuleCatalogQuery.cs` und `RuleCatalogSnapshot.cs` | vorhandenes vollständiges Regel-Mapping; Planvorbereitung nutzt dieselbe Domain-Quelle ohne Regelkopie |

### Vorhandene Infrastructure-Anker

| Aktueller Code-Anker | Verwendung in System 08 |
|---|---|
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContext.cs` | wird um die Scheduling-Tabellen erweitert; es entsteht kein zweiter `DbContext` |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContextFactory.cs` | bleibt die einzige SQLite-Konfiguration für produktive und Migrationszugriffe |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/SharedDatabaseMigrationBoundary.cs` | bleibt Grenze der gemeinsamen Migrationshistorie |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/Availabilities/SqliteAvailabilityStore.cs` | liefert das Muster einer konsistenten Lesetransaktion; Typ1-/Tageswechsel werden jedoch atomar im Scheduling-Adapter koordiniert |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations` | erhält genau eine neue generierte System-08-Migration; veröffentlichte Migrationen bleiben unverändert |

### Vorhandene Desktop- und Architekturanker

| Aktueller Code-Anker | Geplante Änderung |
|---|---|
| `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewView.xaml` | enthält nach der vollständigen Überführung die Ansicht für Tageskennzeichen, Typ1 und Vorbereitung |
| `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewViewModel.cs` | koordiniert ausschließlich die sichtbaren Anwendungsabläufe und delegiert Zelle, Typ1 und Vorbereitung an getrennte ViewModels |
| `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleCellViewModel.cs` | projiziert Tageskennzeichen und strukturierte Typ1-Zustände ohne eigene Fachentscheidung |
| `src/Salztal.Dienstplanung.Desktop/MainWindow.xaml` | behält den Reiter „Dienstplan SER“ und bindet danach das Scheduling-Feature |
| `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowDependencies.cs` | erhält eine gruppierte, schmale `SchedulingDependencies`-Abhängigkeit statt vieler ungeordneter Einzelparameter |
| `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowComposition.cs` | bleibt der einzige Ort für die konkrete SQLite-Verdrahtung |
| `tests/Salztal.Dienstplanung.Architecture.Tests/DesktopCompositionBoundaryTests.cs` | wird so erweitert, dass alle Featureordner automatisch auf Querverweise geprüft werden |
| `tests/Salztal.Dienstplanung.Architecture.Tests/InfrastructurePersistenceBoundaryTests.cs` | schützt weiterhin genau einen `DbContext` und genau ein Migrationsverzeichnis |
| `tests/Salztal.Dienstplanung.Architecture.Tests/ProductionProjectBoundaryTests.cs` | schützt die Referenzrichtung und die Trennung vom Planning-Modul |

## Geplante neue Code-Anker

Die folgenden Namen sind verbindliche Richtungsanker. Kleine interne Typnamen dürfen innerhalb des jeweiligen Schritts begründet präzisiert werden, solange Verantwortung und Modulgrenze unverändert bleiben.

### Domain

Neuer Fachbereich `src/Salztal.Dienstplanung.Domain/Scheduling`:

- `SchedulePeriod` für Montag, 21 Tage, Endsonntag und Überschneidung,
- `ScheduleDraftId` und `ScheduleDraftVersion` für stabile Identität und optimistische Änderungskontrolle,
- `DemandSlotId` und `DemandSlot` für deterministische einzelne Plätze mit tatsächlicher Zeit,
- `AssignmentSegment` für einen tatsächlichen ununterbrochenen Arbeitsabschnitt,
- `ScheduleAssignment` mit strukturierter Herkunft, Zielbezug und Deckungswirkung,
- `DemandCoverage` für vollständig gedeckt, vollständig ungedeckt und ausschließlich beim bestätigten `Spr`-Fall teilweise gedeckt,
- `GeneratedDayOffMarker` für ein später erzeugbares schwarzes `X`; `U`, `K` und rote `X` bleiben ausschließlich vorhandene `AvailabilityEntry`-Werte,
- `AssignmentLock` als modellierter, in System 08 noch nicht bedienbarer Einzelschutz,
- `ScheduleDraft` als fachlicher aktueller Entwurf,
- kleine Ergebnis- und Fehlerwerte für Zeitraum, Platzaufbau, Zuweisung und Entwurfsvalidierung.

Es entsteht kein einzelner allwissender Typ. Zeitraum, Platz, Segment, Zuweisung und Entwurf besitzen getrennte Invarianten und getrennte Tests.

### Application

Neuer Fachbereich `src/Salztal.Dienstplanung.Application/Scheduling` mit getrennten Unterverantwortungen:

- `GetScheduleWorkspaceQuery` und unveränderliche `ScheduleWorkspaceSnapshot`-Typen für die sichtbare 21-Tage-Arbeitsansicht,
- `OpenOrCreateScheduleDraftCommand` für vorhandenen Zeitraum oder überschneidungsfreien neuen Entwurf,
- `SetServiceManagementAssignmentCommand` und `RemoveServiceManagementAssignmentCommand` für Typ1,
- `ChangeScheduleDayEntryCommand` für den atomar bestätigten Wechsel zwischen Typ1 und `U`, `K` oder rotem `X`,
- `PreparePlanningInputCommand` für die erste Momentaufnahme und ihre bewusste Aktualisierung,
- `PlanningInputSnapshot` als eigenständige unveränderliche Übergabe an System 09,
- `PlanningInputComparison` und strukturierte Änderungskategorien für einen veralteten Vorbereitungsstand,
- `PlanningHistorySnapshot` mit bis zu sieben Tagen und expliziter Vollständigkeit,
- `PlanningRunOptions` für die nur in diesem Zeitraum bewusst aktivierten Einsatzfreigaben,
- schmale Ports wie `IScheduleWorkspaceReader`, `IOpenScheduleDraftStore`, `IChangeScheduleDayStore`, `IPlanningInputReader` und `IPreparePlanningSnapshotStore`.

Die genaue Zahl kleiner Snapshot-Dateien folgt dem vorhandenen Stil. Fachlich verschiedene Rohdaten, UI-Projektionen und Solver-Input werden nicht in einem Sammelrecord vermischt.

### Infrastructure

Neuer Bereich `src/Salztal.Dienstplanung.Infrastructure/Persistence/Scheduling`:

- `SqliteScheduleStore` als konkreter Adapter hinter den schmalen Application-Ports,
- eine interne `SchedulePeriodDayEntity` je geplantem Kalendertag mit eindeutiger Datumsspalte, damit auch parallele Schreibzugriffe keinen überlappenden Zeitraum anlegen können,
- getrennte Entities und EF-Konfigurationen für Entwurf, Zuweisung, Arbeitsabschnitt, Bedarfsplatzbezug, Momentaufnahme und ihre Komponenten,
- atomare Transaktionen für Entwurfserstellung, Typ1-/Tageswechsel und Momentaufnahme-Aktualisierung,
- relationale Eindeutigkeits-, Bereichs-, Fremdschlüssel- und Änderungsversionregeln,
- eine generierte Migration in `Persistence/ServiceCatalog/Migrations`.

Die Datenbank speichert die fachlich benötigten Momentaufnahmewerte verlustfrei. Sie speichert keine WPF-Modelle, keine `IQueryable`-Grenzübergaben und keine OR-Tools-Typen.

### Desktop

Neuer beziehungsweise vollständig überführter Bereich `src/Salztal.Dienstplanung.Desktop/Features/Scheduling`:

- `ScheduleOverviewView.xaml` und `ScheduleOverviewViewModel` als schlanke Koordination,
- `ScheduleEmployeeRowViewModel` und `ScheduleDayCellViewModel` für das Raster,
- `ServiceManagementAssignmentViewModel` für Typ1-Auswahl einschließlich `B`,
- `PlanningPreparationViewModel` für Laufoptionen, Vorbereitungsstatus, erkannte Änderungskategorien und bewusste Aktualisierung,
- unveränderte Einbindung in den Reiter „Dienstplan SER“ über `Desktop.Composition`.

Die früheren Dateien des eigenständigen Availability-Features und ihre gespiegelten Desktop-Tests sind vollständig in `Features/Scheduling` überführt. Es bestehen keine doppelten Ansichten, Dateinamen oder Namespaces.

### Tests

- `tests/Salztal.Dienstplanung.Domain.Tests/Scheduling`
- `tests/Salztal.Dienstplanung.Application.Tests/Scheduling`
- `tests/Salztal.Dienstplanung.Infrastructure.Tests/Scheduling`
- `tests/Salztal.Dienstplanung.Desktop.Tests/Features/Scheduling`
- Erweiterungen unter `tests/Salztal.Dienstplanung.Architecture.Tests`

Gemeinsame synthetische Testaufbauten dürfen gezielt geteilt werden. Produktionslogik wird nicht in Test-Buildern nachgebaut.

## Ablauf und Datenfluss

1. Die Service-Leitung wählt ein Datum; Application normalisiert es auf den Montag eines Drei-Wochen-Zeitraums.
2. `OpenOrCreateScheduleDraftCommand` öffnet den exakt vorhandenen Entwurf oder legt nur bei vollständig überschneidungsfreiem Zeitraum einen neuen an.
3. `GetScheduleWorkspaceQuery` lädt den konsistenten 21-Tage-Arbeitsstand mit aktiven Personen, Tageskennzeichen, tatsächlichen Bedarfsplätzen, Typ1-Zuweisungen und Vorbereitungsstatus.
4. Typ1- und Tagesänderungen laufen über koordinierte Commands und atomare Store-Operationen. Kein ViewModel kombiniert zwei unabhängige Speicheraufrufe.
5. `PreparePlanningInputCommand` lädt alle aktuellen Eingaben konsistent, ergänzt den festen Regelkatalog, validiert Typ1, zerlegt Bedarfe deterministisch, übernimmt Vorgeschichte und Laufoptionen und speichert eine neue unveränderliche Momentaufnahme.
6. Beim späteren Lesen wird ein aktueller Kandidat gegen die gespeicherte Momentaufnahme verglichen. Abweichungen machen den Stand sichtbar veraltet; erst eine bewusste Aktualisierung ersetzt die verwendete Momentaufnahme.
7. System 09 erhält ausschließlich `PlanningInputSnapshot` und liefert später ein strukturiertes Ergebnis zurück. WPF und SQLite sind an dieser Grenze nicht sichtbar.

## Offene Entscheidungen

Für die Abnahme dieses Roadmap-Entwurfs bestehen keine offenen fachlichen Entscheidungen.

Folgende technische Details dürfen im jeweiligen Schritt innerhalb der festgelegten Grenzen präzisiert werden:

- Aufteilung kleiner Ergebnis-, Fehler- und Snapshot-Typen auf einzelne Dateien,
- konkrete interne Namen der schmalen Store-Ports,
- konkrete relationale Aufteilung der unveränderlichen Momentaufnahmekomponenten,
- genaue Anordnung der Typ1-Auswahl und des Vorbereitungsbereichs innerhalb der bestehenden Ansicht,
- technische Umsetzung des deterministischen Komponentenvergleichs.

Eine Präzisierung darf weder Zeitraumlänge, Überschneidungsverbot, Typ1-/`B`-Wirkung, Momentaufnahmeinhalt, Systemgrenzen noch die modulare Verantwortungsverteilung ändern. Eine solche Änderung wäre fachlicher oder architektonischer Handlungsbedarf und stoppt den Ablauf.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Nummerierte Schritte

Nach ausdrücklicher Abnahme dieser Roadmap gilt die bedingte automatische Weiterführung: Ein planmäßig und fehlerfrei abgeschlossener Schritt darf direkt in den nächsten übergehen, wenn weder eine unerwartete Entscheidung oder kritische Frage noch ein externes, manuelles, visuelles oder fachliches Gate besteht. Bei Handlungsbedarf wird gestoppt. Nach jedem abgeschlossenen Schritt ertönt ein Abschlusston; bei Handlungsbedarf zusätzlich ein unterscheidbarer Hinweiston.

### PM-01 – Detailroadmap mit aktuellen Code-Ankern entwerfen

Status: `[x]` – Roadmap am 2026-09-16 ausdrücklich abgenommen

Umfang:

- bestätigte Fachentscheidungen, Umfang, Nicht-Umfang und Übergaben festhalten,
- vorhandene und neue Code-Anker bis auf Klassen- beziehungsweise Dateiebene benennen,
- Planmodell, Application-Abläufe, Persistenz und WPF als getrennte Module planen,
- kleine Implementierungsschritte, Prüfungen und echte Gates festlegen,
- `MASTER_ROADMAP.md`, `STATUS.md`, Fragenkatalog und Service-Leitungsstand auf den Entwurf abstimmen,
- noch keinen Fachcode, kein Datenbankschema und keine WPF-Funktion für System 08 anlegen.

Prüfung:

- Abgleich mit den Systemen 03 bis 07, Architektur und Clean-Code-Regeln,
- gezielte Prüfung aller genannten aktuellen Pfade und Klassen,
- Suche nach widersprüchlichen Aussagen zu Zeitraum, Überschneidung, Typ1, `B`, Momentaufnahme und aktivem Roadmap-Stand,
- `git diff --check` und Prüfung der Markdown-Zeilenenden.

Abnahmebedingung:

- Der Auftraggeber bestätigt Fachumfang, Modularitätsvertrag, Code-Anker und Schrittreihenfolge ausdrücklich oder nennt Änderungswünsche. Vor dieser Abnahme beginnt PM-02 nicht.

### PM-02 – Drei-Wochen-Zeitraum und atomare Bedarfsplätze in Domain

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- `SchedulePeriod` mit Montag, exakt 21 Tagen und Endsonntag einführen,
- wertbasierte Überschneidungsprüfung einschließlich gemeinsamer Randtage definieren,
- jeden `EffectiveStaffingDemand` deterministisch in so viele `DemandSlot`-Werte zerlegen, wie `RequiredEmployeeCount` verlangt,
- stabile Platzidentität aus Bedarfsquelle, Datum, Einsatzort, Diensttyp und Platzfolge bilden,
- tatsächliche Zeit unverändert übernehmen,
- doppelte Platzidentitäten, unbekannte Referenzen und ungültige Zeiträume strukturiert ablehnen,
- noch keine Zuweisung, Application oder Speicherung umsetzen.

Prüfung:

- Domain-Tests für Montag, Sonntag, 21 Tage, Jahreswechsel und ungültige Starttage,
- Überschneidungstests für identisch, teilweise, vollständig enthalten, direkt angrenzend und getrennt,
- Platztests für Personenzahl eins und größer eins, deterministische Reihenfolge und unveränderte tatsächliche Zeit,
- bestehende Bedarfsauflösungs-Tests bleiben grün,
- keine neue technische Abhängigkeit in Domain.

Abnahmebedingung:

- Drei-Wochen-Zeitraum und einzelne Bedarfsplätze sind vollständig wertbasiert, deterministisch und ohne UI-, Datenbank- oder Solverabhängigkeit nachgewiesen.

Nachweis am 2026-09-16:

- `SchedulePeriod` bildet ausschließlich Montag bis zum dritten Sonntag ab und prüft Zeitraumgrenzen sowie Überschneidungen wertbasiert,
- `DemandSlotSet` zerlegt aufgelöste Bedarfe deterministisch in einzelne Plätze mit stabiler Identität und tatsächlicher Zeit,
- unbekannte Einsatzort- oder Diensttypbezüge, Bedarfe außerhalb des Zeitraums und doppelte Platzidentitäten liefern strukturierte Fehler,
- 18 fokussierte Scheduling-Tests, alle 285 Domain-Tests und alle 18 Architekturtests bestanden,
- der vollständige Solution-Build bestand mit 0 Warnungen und 0 Fehlern,
- `dotnet format --verify-no-changes` bestand; Application, Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert.

### PM-03 – Zuweisungs-, Segment- und Deckungsmodell in Domain

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- tatsächliche Arbeitsabschnitte und normale Zuweisungen modellieren,
- vollständige Deckung genau eines normalen Platzes absichern,
- `D` als ein Tagesmuster mit zwei getrennten Restaurant-Segmenten und zwei passenden Plätzen ausdrücken,
- `Spr` mit Cafeteria-Segment, tatsächlichem Wechsel und zulässiger Restaurant-Teildeckung ausdrücken,
- Typ1-Herkunft und geschützte Wirkung strukturiert modellieren,
- `B` auf Restaurant-Früh-/Spätdienst begrenzen, Arbeitszeit zählen und Deckungswirkung auf null setzen,
- schwarze `X`, Einzelsperren und manuelle Zusatzbesetzung als geschlossene spätere Planfälle modellieren, ohne Schreibablauf,
- frei erfundene Teildeckung, Dienst ohne Platz, falsche Musterbestandteile und Zeitüberlappung verhindern.

Prüfung:

- Domain-Tests für normalen Dienst, `B`, `D`, `Spr`, ungedeckten Platz und allein zulässige Teildeckung,
- Tests für falschen Ort, falschen Dienst, fehlenden Platz, abweichende Zeit und doppelte Platzbelegung,
- Tests für wertbasierte Gleichheit, Unveränderlichkeit und deterministische Deckungsberechnung,
- Wiederverwendung der relevanten synthetischen Szenarien aus `S07_RULE_CATALOG_EXAMPLES.md`.

Abnahmebedingung:

- Das Modell kann alle bestätigten Planstrukturen ausdrücken und verhindert widersprüchliche Strukturen, ohne automatische Planung oder allgemeine Bearbeitung vorwegzunehmen.

Nachweis am 2026-09-16:

- normale Zuweisung, `D`, `Spr`, Typ1-Herkunft, `B`, schwarze `X`, Zuweisungssperre und manuelle Zusatzbesetzung besitzen getrennte strukturierte Fachwerte,
- normale Dienste und `D` decken nur vollständige Plätze; `B` und Zusatzbesetzung erzeugen keine Deckung; ausschließlich `Spr` erzeugt die bestätigte Restaurant-Teildeckung ab tatsächlichem Wechsel,
- falsche Musterplätze, doppelte Plätze, verschiedene Tage, überlappende `D`-Zeiten, unzulässige `Spr`-Tage beziehungsweise Wechsel und unzulässige Büroplätze werden strukturiert abgelehnt,
- 35 fokussierte Scheduling-Tests, alle 302 Domain-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern,
- Application, Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert.

### PM-04 – Aktueller Entwurf und Typ1-Bereitschaft in Domain

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- `ScheduleDraft` mit stabiler Kennung, Zeitraum und positiver Änderungsversion einführen,
- genau einen Tageszustand und höchstens eine reguläre Zuweisung beziehungsweise ein Muster je Person und Datum absichern,
- Zuweisungen gegen `U`, `K` und rote `X` ausschließen,
- vorgetragene Typ1-Zuweisungen schützen und ihre Wochenstunden aus tatsächlichen Segmenten berechnen,
- die Typ1-Voraussetzung für jede nicht vollständig abwesende Woche strukturiert prüfen,
- vollständig abwesende Woche von nur teilweise abwesender Woche unterscheiden,
- Entwurf und spätere unveränderliche Planversion fachlich getrennt halten.

Prüfung:

- Domain-Tests für alle drei Wochen, leere und teilweise/vollständig abwesende Typ1-Wochen,
- `B`, `D` und `Spr` zählen jeweils korrekt zur Voraussetzung und zu den tatsächlichen Stunden,
- `U`, `K`, rote `X` und leere Tage zählen nicht,
- Tests für doppelte Tageszuweisung, kollidierende Tageszustände und optimistische Versionswerte,
- keine Planversion oder Abnahmelogik in Domain.

Abnahmebedingung:

- Ein aktueller Entwurf schützt die bestätigten Strukturregeln und kann seine Typ1-Bereitschaft je Woche eindeutig melden.

Nachweis am 2026-09-16:

- `ScheduleDraft` besitzt stabile Kennung, positive Änderungsversion und unveränderliche geordnete Sammlungen,
- doppelte Personen-/Tageszuweisungen, Zuweisungen auf `U`/`K`/rotem `X`, doppelte Bedarfsdeckung, kollidierende schwarze `X` und ungültige Sperren werden strukturiert abgelehnt,
- `ServiceManagementReadiness` unterscheidet je Woche bereit, vollständig abwesend und fehlende Typ1-Zuweisung und summiert ausschließlich tatsächliche geschützte Typ1-Arbeitsminuten,
- normale Typ1-Dienste, `B`, `D` und `Spr` erfüllen die Wochenvoraussetzung; fremde automatische Zuweisungen und Tageskennzeichen tun dies nicht,
- 48 fokussierte Scheduling-Tests, alle 315 Domain-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern,
- Application, Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert.

### PM-05 – Modulare Application-Leseverträge und Zeitraumöffnung

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- `Application/Scheduling` mit getrennten Rohdaten-, Workspace- und Ergebnisverträgen anlegen,
- `GetScheduleWorkspaceQuery` für 21 Tage, aktive Personen, Wochen-Soll, Tageskennzeichen, Bedarfsplätze, Typ1 und Vorbereitungsstatus umsetzen,
- `OpenOrCreateScheduleDraftCommand` für exaktes Öffnen oder überschneidungsfreies Anlegen bereitstellen,
- jeden teilüberlappenden neuen Zeitraum mit stabilem Code und verständlicher deutscher Meldung ablehnen,
- schmale Reader- und Store-Ports definieren,
- keine vorhandenen UI-Queries dreifach aufrufen und keine technischen Objekte über die Application-Grenze reichen.

Prüfung:

- Application-Tests für vorhandenen, neuen, identischen, angrenzenden und überlappenden Zeitraum,
- Tests für exakt 21 Tage, drei getrennte Wochenwerte, nur aktive Personen und deterministische Plätze,
- Tests für fehlende oder widersprüchliche Katalogreferenzen, Abbruch und Store-Konflikt,
- Vertragstests bestätigen unveränderliche Snapshots ohne EF-, WPF- oder OR-Tools-Typen.

Abnahmebedingung:

- Die Arbeitsansicht ist über einen kohärenten, modularen Application-Vertrag vollständig lesbar und es kann fachlich nie ein zweiter überlappender Entwurf entstehen.

Nachweis am 2026-09-16:

- `OpenOrCreateScheduleDraftCommand` normalisiert ein gewähltes Datum auf Montag, öffnet einen exakt passenden Entwurf oder legt genau einen neuen 21-Tage-Entwurf an,
- vorhandene Teilüberschneidungen sowie beim Speichern erkannte parallele Überschneidungen und Versionskonflikte liefern stabile strukturierte Ergebnisse,
- `GetScheduleWorkspaceQuery` liefert über genau einen Reader-Aufruf 21 Tage, drei getrennte Wochenwerte, nur aktive Personen, 195 deterministische Bedarfsplätze, strukturierte Typ1-Zuweisungen, Typ1-Wochenbereitschaft und Vorbereitungsstatus,
- getrennte Reader-, Store-, Rohdaten-, Workspace- und Ergebnisverträge halten EF-, WPF- und OR-Tools-Typen außerhalb der Application-Grenze,
- fehlende oder doppelte Mitarbeiter-, Typ-, Einsatzort- und Diensttyp-Referenzen sowie widersprüchliche gespeicherte Daten werden kontrolliert abgelehnt,
- 19 fokussierte Scheduling-Application-Tests, alle 179 Application-Tests, alle 315 Domain-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern,
- Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert.

### PM-06 – Typ1- und Tagesänderungen als koordinierte Application-Abläufe

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- Typ1-Dienst oder Muster anhand vorhandener Plätze, tatsächlicher Zeit, Planungsrolle und Einsatzfreigabe setzen,
- `B` nur für die bestätigten Restaurant-Dienste zulassen,
- Typ1-Zuweisung entfernen,
- Eintrag auf `U`, `K` oder rotes `X` bei vorhandener Typ1-Zuweisung nur nach sichtbarer Bestätigung atomar ersetzen,
- Typ1 auf einem blockierten Tag erst nach bewusster Entfernung des Tageskennzeichens zulassen,
- keine halbfertige Kombination aus entferntem Eintrag und fehlgeschlagener Zuweisung speichern,
- jede Änderung mit erwarteter Entwurfsversion gegen paralleles Überschreiben schützen,
- einen vorhandenen Vorbereitungsstand nach tatsächlicher Eingabeänderung als potenziell veraltet behandeln, ohne ihn still zu ersetzen.

Prüfung:

- Application-Tests für normalen Typ1-Dienst, `B`, `D`, `Spr`, Entfernen und Abbruch,
- Negativtests für Nicht-Typ1, inaktive Person, fehlenden Bedarf, falsche Zeit, fehlende Freigabe und unzulässiges `B`,
- Tests beider Wechselrichtungen zwischen Typ1 und `U`/`K`/rotem `X`, jeweils für Bestätigen, Abbrechen und Store-Fehler,
- vor Fehler oder abgelehnter Bestätigung erfolgt keine Teiländerung,
- bestehende Availability-Commands und ihre Tests bleiben gültig; die gemeinsame UI verwendet danach nur den koordinierten Ablauf.

Abnahmebedingung:

- Jeder sichtbare Tageswechsel besitzt genau einen atomaren Application-Ablauf mit stabilen Ergebnissen und ohne Regelduplikation im Desktop.

Nachweis am 2026-09-16:

- `SetServiceManagementAssignmentCommand` setzt oder ersetzt normale Typ1-Dienste, `B`, `D` und `Spr` ausschließlich anhand vorhandener Bedarfsplätze und ihrer tatsächlichen Zeiten,
- aktive Typ1-Rolle, Dienst- beziehungsweise Musterfreigabe, Bedarfsidentität, tatsächliche Zeit und die besondere `B`-Beschränkung werden vor jedem Schreibaufruf geprüft,
- `RemoveServiceManagementAssignmentCommand` entfernt ausschließlich geschützte Typ1-Zuweisungen,
- `ChangeScheduleDayEntryCommand` ersetzt Typ1 atomar durch `U`, `K` oder rotes `X`; die Gegenrichtung entfernt ein vorhandenes Tageskennzeichen nur nach bewusster Bestätigung,
- Entwurfsversion und Tagesänderungsversion schützen vor parallelem Überschreiben; abgebrochene, ungültige oder konfliktbehaftete Abläufe verändern den geladenen Entwurf nicht,
- jeder tatsächliche Schreibvorgang wird als eine gemeinsame Store-Änderung mit potenzieller Veraltung eines vorhandenen Vorbereitungsstands übergeben,
- 23 fokussierte PM-06-Tests beziehungsweise 42 Scheduling-Application-Tests, alle 202 Application-Tests, alle 315 Domain-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern,
- Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert; die transaktionale SQLite-Umsetzung folgt planmäßig in PM-08.

### PM-07 – Unveränderliche Planungsmomentaufnahme und Änderungsvergleich

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- `PlanningInputSnapshot` getrennt vom Workspace-Snapshot definieren,
- alle bestätigten Eingabekomponenten und den vollständigen Regelkatalog versioniert übernehmen,
- explizite Laufoptionen standardmäßig ausgeschaltet halten und nur für den konkreten Zeitraum aufnehmen,
- bis zu sieben vorhergehende Kalendertage aus gespeicherten Plänen übernehmen,
- fehlende Historientage und Vollständigkeit strukturiert darstellen,
- erste Vorbereitung und bewusste Aktualisierung als denselben klaren Anwendungsfall mit unterschiedlichem erwarteten Zustand ausführen,
- aktuelle Daten komponentenweise mit der gespeicherten Momentaufnahme vergleichen,
- veraltete Kategorien sichtbar liefern, ohne den Snapshot zu verändern,
- bei Aktualisierung gültige Typ1-Zuweisungen erhalten und ungültige mit konkreten stabilen Codes blockieren.

Nach einer erfolgreichen Aktualisierung verweist der Entwurf genau auf die neue unveränderliche Momentaufnahme. Die abgelöste technische Vorbereitung wird atomar entfernt und bildet keine verdeckte Planversionshistorie; gespeicherte Pläne oder spätere abgenommene Planversionen werden dadurch nicht gelöscht.

Prüfung:

- Application-Tests für vollständige und teilweise/komplett fehlende Vorgeschichte,
- Tests aller Snapshot-Komponenten einschließlich 28 Regeln und Katalogversion 1,
- Vergleichstests je Änderungskategorie sowie unveränderter Eingaben,
- Tests für standardmäßig ausgeschaltete und bewusst aktivierte Laufoption,
- Tests für erhaltene gültige und gemeldete ungültige Typ1-Zuweisungen,
- wiederholtes Lesen verändert weder Entwurf noch Momentaufnahme.

Abnahmebedingung:

- System 09 kann einen vollständigen unveränderlichen Input erhalten, und die App kann wahrheitsgemäß unterscheiden, ob dieser Input noch dem aktuellen Datenstand entspricht.

Nachweis am 2026-09-16:

- `PlanningInputSnapshot` hält aktive Personen, verwendete Typfassungen und Einsatzfreigaben, Dienstkatalog, Tageskennzeichen, 195 tatsächliche Bedarfsplätze, geschützte Typ1-Zuweisungen, alle 28 Regeldefinitionen der Katalogversion 1, Laufoptionen und Vorgeschichte unveränderlich fest,
- `PreparePlanningInputCommand` verwendet für erste Vorbereitung und bewusste Aktualisierung denselben Ablauf mit ausdrücklich unterschiedlichem erwarteten Vorbereitungszustand,
- die AH2-`Spr`-Laufoption ist standardmäßig ausgeschaltet und wird nur durch den ausdrücklichen Anforderungswert in die Momentaufnahme aufgenommen,
- sieben unmittelbar vorhergehende Kalendertage werden als vollständig, teilweise vorhanden oder vollständig fehlend strukturiert abgebildet; fehlende Vorgeschichte blockiert die Vorbereitung nicht,
- gültige Typ1-Zuweisungen behalten bei einer Aktualisierung ihre Kennungen; ungültige Zuweisungen blockieren mit Zuweisungskennung und stabilem Ursachencode,
- `PlanningInputComparison` meldet Mitarbeitende/Typen, Einsatzfreigaben, Dienstkatalog, Bedarfe, Tageskennzeichen, Typ1, Regeln, Laufoptionen und Vorgeschichte getrennt,
- wiederholtes Lesen verändert weder Entwurf noch gespeicherte Momentaufnahme; der Workspace meldet vorbereitet oder veraltet samt Änderungskategorien,
- 20 fokussierte PM-07-Tests beziehungsweise 62 Scheduling-Application-Tests, alle 222 Application-Tests, alle 315 Domain-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern,
- Infrastructure, Desktop, Planning und Datenbankschema blieben fachlich unverändert.

### PM-08 – Entwurf und Momentaufnahme dauerhaft in SQLite speichern

Status: `[x]` – am 2026-09-16 ausdrücklich abgenommen

Umfang:

- `Persistence/Scheduling` mit getrennten Entities, Konfigurationen und `SqliteScheduleStore` anlegen,
- `ServiceCatalogDbContext` erweitern und den einzigen Migrationsverlauf fortführen,
- Datenbankregeln für Zeitraum, Eindeutigkeit, Referenzen, Versionen und zulässige strukturierte Werte ergänzen,
- alle 21 Zeitraumtage innerhalb derselben Transaktion mit einem eindeutigen Datumswert belegen und dadurch auch parallele Überlappungen datenbankseitig verhindern,
- Typ1-/Tageswechsel und Snapshot-Aktualisierung jeweils atomar speichern,
- Momentaufnahmekomponenten verlustfrei und nach Neustart unverändert rekonstruieren,
- eine neue generierte Migration erstellen, ohne frühere Migrationen zu ändern,
- keine produktive Datenbank, Sicherung oder Exportdatei verwenden.

Prüfung:

- temporäre SQLite-Tests für Neuaufbau und Upgrade vom System-06-Datenbankstand,
- vorhandene synthetische Katalog-, Mitarbeiter-, Bedarfs- und Verfügbarkeitsdaten bleiben erhalten,
- Neustarttests für Entwurf, Typ1, `B`, Segmente, Plätze, Regeln, Laufoptionen und Vorgeschichte,
- Parallelitäts-, Eindeutigkeits-, Fremdschlüssel-, Überlappungs- und Rollbacktests,
- künstlicher Fehler hinterlässt weder Teiländerung noch ersetzten Snapshot,
- `dotnet ef migrations has-pending-model-changes` meldet keinen ausstehenden Modellunterschied,
- Architekturtest bestätigt weiterhin einen `DbContext` und ein Migrationsverzeichnis.

Abnahmebedingung:

- Ein aktueller Entwurf und jede bewusst erzeugte Momentaufnahme bleiben lokal, atomar und verlustfrei erhalten; vorhandene Datenbanken können sicher migriert werden.

Ergebnis und Nachweis:

- `Persistence/Scheduling` enthält getrennte EF-Entitäten, Konfigurationen und den gemeinsamen `SqliteScheduleStore`; der bestehende `ServiceCatalogDbContext` bleibt der einzige `DbContext`,
- Entwurf, alle 21 eindeutigen Zeitraumtage, Bedarfsplätze, Tageskennzeichen, Typ1-Zuweisungen, Segmente, Deckungen, Sperren und Vorbereitungsstand werden lokal rekonstruiert; überschneidende Zeiträume werden auch durch den eindeutigen Datumsschlüssel verhindert,
- Typ1-/Tageswechsel und die zugehörige Verfügbarkeitsänderung werden in einer Transaktion gespeichert; eine fehlgeschlagene kombinierte Änderung hinterlässt weder einen neuen Dienst noch ein teilweise geändertes Tageskennzeichen,
- eine neue Vorbereitung ersetzt die vorige Momentaufnahme atomar; ein künstlich ausgelöster Speicherfehler stellt die vorige Momentaufnahme samt Entwurfsverweis vollständig wieder her,
- unveränderliche Application-Momentaufnahmen werden über Infrastructure-eigene Speicherformen verlustfrei serialisiert und nach einem Neustart einschließlich `B`, `D`, `Spr`, Segmenten, Regeln, Laufoptionen und Vorgeschichte rekonstruiert,
- die generierte Migration `20260916163137_AddScheduling` führt ausschließlich die vorhandene gemeinsame Migrationsfolge fort; der Upgrade-Test vom System-06-Stand erhält bestehende synthetische Mitarbeitenden-, Bedarfs- und Verfügbarkeitsdaten,
- acht fokussierte Scheduling-Persistenztests, alle 65 Infrastructure-Tests, alle 315 Domain-Tests, alle 222 Application-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build und Formatprüfung bestanden mit 0 Warnungen und 0 Fehlern; `dotnet ef migrations has-pending-model-changes` meldete keinen Modellunterschied,
- ausschließlich temporäre synthetische SQLite-Datenbanken wurden verwendet; Desktop und Planning blieben unverändert.

### PM-09 – Modulare Ansicht „Dienstplan SER“ für Typ1 und Vorbereitung

Status: `[x]` – sichtbare Bedienung am 2026-09-16 ausdrücklich abgenommen

Umfang:

- das frühere eigenständige Availability-Feature vollständig in `Features/Scheduling` überführen,
- bestehende 21-Tage-Navigation, `U`, `K`, rotes `X` und Wochen-Soll erhalten,
- Typ1-Dienst beziehungsweise Muster aus den tatsächlich vorhandenen zulässigen Bedarfsplätzen auswählen und entfernen,
- `B` strukturiert setzen und eindeutig von Diensttyp und Bedarf unterscheiden,
- erforderliche Bestätigung bei Wechsel zwischen Typ1 und Tageskennzeichen anzeigen,
- Zeitraum, vorhandenen Entwurf, Vorbereitung, veraltete Kategorien, unvollständige Vorgeschichte und Laufoptionen verständlich darstellen,
- „Planung vorbereiten“ und bewusste Aktualisierung anbieten,
- Übersichts-, Zell-, Typ1- und Vorbereitungs-ViewModel getrennt halten,
- `SchedulingDependencies` in Composition gruppieren und konkrete Adapter nur dort verdrahten,
- alte Availability-View-/ViewModel-Dateien und gespiegelte Testpfade nach vollständiger Überführung entfernen.

Prüfung:

- Desktop-ViewModel-Tests für Laden, Navigation, Typ1-Auswahl, `B`, Tageswechsel, Vorbereitung, Veraltet-Status, Aktualisierung, Laufoptionen und Fehlerzustände,
- WPF-Konstruktions- und Render-Tests für auflösbare Ressourcen, Tastaturbedienung und nicht nur farblich vermittelte Zustände,
- Architekturtests verhindern Feature-ViewModel-Querverweise und konkrete Adapter außerhalb Composition,
- Pfadsuche findet keine veralteten Produktions- oder Testverweise auf das frühere Feature,
- manueller sichtbarer Test ausschließlich mit synthetischen Daten auf der vorgesehenen Windows-Darstellung.

Abnahmebedingung:

- Die Service-Leitung bestätigt die Lesbarkeit und Bedienung von Zeitraum, vorhandenen Tageskennzeichen, Typ1 einschließlich `B`, Vorbereitungsstatus und bewusster Aktualisierung. Dieses visuelle Gate stoppt den Ablauf vor PM-10.

Technisches Ergebnis und automatischer Nachweis:

- die frühere Availability-Ansicht wurde vollständig durch `Features/Scheduling` ersetzt; alte Produktions- und Testdateien beziehungsweise Namespaces bestehen nicht parallel fort,
- `ScheduleOverviewViewModel`, `ScheduleCellViewModel`, `ServiceManagementAssignmentEditorViewModel` und `PlanningPreparationViewModel` trennen Zeitraumkoordination, Zellenprojektion, Typ1-Auswahl und Vorbereitungszustand,
- die Application liefert fachlich gefilterte Typ1-Optionen für normale Dienste, `B`, `D` und `Spr`; der Desktop leitet ihre Zulässigkeit nicht aus Namen, Farben oder freien Texten ab,
- `U`, `K`, rotes `X`, Typ1 und deren bestätigungspflichtige Wechsel verwenden ausschließlich koordinierte Scheduling-Abläufe; auch das Leeren eines Tageskennzeichens aktualisiert Entwurf und Verfügbarkeit atomar,
- Zeitraum, drei Wochenziele, Typ1-Wochenbereitschaft, vorbereiteter beziehungsweise veralteter Stand, Änderungskategorien, unvollständige Vorgeschichte und die bestätigte AH-`Spr`-Laufoption sind sichtbar,
- `SchedulingDependencies` bündelt die schmalen Ports; nur Composition verdrahtet den konkreten `SqliteScheduleStore`,
- 65 fokussierte Scheduling-Application-Tests, 11 fokussierte Scheduling-Desktop-/Render-Tests, alle 225 Application-Tests, alle 89 Desktop-Tests, alle 315 Domain-Tests, alle 65 Infrastructure-Tests und alle 18 Architekturtests bestanden,
- vollständiger Solution-Build bestand mit 0 Warnungen und 0 Fehlern; die abschließende Format-, Pfad- und Diff-Prüfung ist erfolgt,
- der sichtbare WPF-Bedientest mit synthetischen Daten wurde von der Service-Leitung am 2026-09-16 ausdrücklich bestätigt; damit ist das manuelle Gate bestanden.

### PM-10 – System-08-Gesamtnachweis und Übergabe

Status: `[x]` – Gesamtnachweis und System 08 am 2026-09-16 ausdrücklich abgenommen

Umfang:

- Domain, Application, Infrastructure, Desktop und Composition gemeinsam abgleichen,
- Modularitätsvertrag und alle Systemgrenzen gezielt prüfen,
- Übergaben an Systeme 09 bis 12 dokumentieren,
- Roadmap, Master-Roadmap, Status und Service-Leitungsdokumente wahrheitsgemäß aktualisieren,
- Fragenkatalog und Roadmap erst nach ausdrücklicher Systemabnahme nach `docs/roadmaps/completed` verschieben,
- Datenschutz und Repository-Hygiene abschließend prüfen.

Prüfung:

- `dotnet restore Salztal.Dienstplanung.sln --locked-mode`,
- `dotnet build Salztal.Dienstplanung.sln --no-restore`,
- fokussierte Domain-, Application-, Infrastructure-, Desktop- und Architekturtests,
- vollständiger Testlauf mit der bestätigten Behandlung weiterhin leerer Testprojekte,
- `dotnet format Salztal.Dienstplanung.sln --verify-no-changes --no-restore`,
- EF-Migrationsprüfung, Pfadprüfung, Datenschutz- und Artefaktsuche,
- `git diff --check`,
- bestätigtes sichtbares WPF-Gate aus PM-09.

Nachweis am 2026-09-16:

- Wiederherstellung im gesperrten Modus war vollständig aktuell.
- Der vollständige Release-Build bestand mit 0 Warnungen und 0 Fehlern. Der zusätzlich gestartete Debug-Build wurde ausschließlich durch die noch geöffnete, sichtbar geprüfte Desktop-App beim Kopieren einer bereits geladenen DLL blockiert; die getrennte Release-Ausgabe war davon unabhängig und vollständig erfolgreich.
- Die fokussierten Prüfungen bestanden mit 48 Domain-, 65 Application-, 8 Infrastructure-, 11 Desktop- und 18 Architekturtests.
- Der vollständige Regressionstest bestand mit 315 Domain-, 225 Application-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests, insgesamt 712 Tests ohne Fehler oder Überspringen.
- Die weiterhin absichtlich leeren Planning- und Excel-Testprojekte meldeten jeweils wie bestätigt Exitcode 8 mit 0 ausgeführten und 0 fehlgeschlagenen Tests.
- Formatprüfung und EF-Migrationsprüfung bestanden; das aktuelle Modell besitzt keine ausstehende Migration.
- Architekturtests und Projektverweise bestätigten die Modulgrenzen. Planning referenziert nur Application und Domain; konkrete Adapter bleiben in Infrastructure beziehungsweise in Desktop.Composition verdrahtet.
- Es besteht genau eine Migrationsfolge am einzigen gemeinsamen `ServiceCatalogDbContext`. Veraltete Desktop-Featurepfade wurden nicht gefunden.
- Repository-Suche fand keine Datenbanken, Sicherungen, Exporte, temporären Ausgaben oder Zugangsdatenmuster. Neue Tests verwenden ausschließlich klar erfundene Personen.
- `git diff --check` meldete keine Fehler; lediglich die erwartete Git-Zeilenendeninformation für drei Dokumente wurde ausgegeben.
- Das sichtbare WPF-Gate aus PM-09 wurde von der Service-Leitung ausdrücklich bestätigt.

Abnahmebedingung:

- System 08 ist fachlich, technisch, modular, persistent und sichtbar vollständig geprüft und ausdrücklich abgenommen. Erst dann werden Roadmap und Fragenkatalog archiviert und System 09 darf vorbereitet werden.

## Übergaben an spätere Systeme

| Übergabe | Verbindlicher Inhalt | Eigentümer |
|---|---|---|
| Solver-Input | unveränderliche `PlanningInputSnapshot` mit Plätzen, Personen, Typen, Freigaben, Tageszuständen, Typ1, Regeln, Laufoptionen und Vorgeschichte | System 09 |
| Generierungsergebnis | automatische Zuweisungen und schwarze `X` nur gegen den vorbereiteten Snapshot erzeugen; geschützte Typ1-Zuweisungen nicht verändern | System 09 |
| Regelbewertung | normale Vollbesetzung, ausschließlich bestätigte `Spr`-Teildeckung und fehlende Vorgeschichte strukturiert bewerten | Systeme 09 und 10 |
| Konflikterklärung | stabile Ursache- und Lösungscodes in verständliche deutsche Texte übersetzen | System 10 |
| Allgemeine Bearbeitung | normale Dienste, schwarze `X`, Einzelsperren, Zusatzbesetzungen und bestätigte Abweichungen bedienen | System 11 |
| Planversion | aktuellen Entwurf, verwendeten Snapshot, Laufoptionen und spätere Abweichungen unveränderlich abnehmen | System 12 |
| Historie | abgenommene Fassungen werden bevorzugte Quelle der Vorgeschichte; System 08 kann bereits vorhandene lokal gespeicherte Planstände strukturiert lesen | System 12 |

## Echte externe und manuelle Gates

- Die Teil-Roadmap wurde vor Implementierungsbeginn am 2026-09-16 ausdrücklich abgenommen.
- Der sichtbare WPF-Bedientest und die ausdrückliche Bestätigung der Service-Leitung für PM-09 wurden am 2026-09-16 erbracht.
- Die ausdrückliche Gesamtabnahme von PM-10 und System 08 wurde am 2026-09-16 erteilt.
- Ein erfolgreicher Build ersetzt weder SQLite-Migrationstests noch den WPF-Bedientest.
- Temporäre synthetische Datenbanken ersetzen keine spätere Sicherungs- und Wiederherstellungsprüfung.
- OR-Tools-Lauf, Konflikterklärung, allgemeine Planbearbeitung, Planabnahme, Excel und portable Windows-Ausgabe bleiben spätere Gates.

## Risiken und Schutzmaßnahmen

| Risiko | Schutzmaßnahme |
|---|---|
| ein monolithischer Scheduling-Bereich vermischt alles | verbindlicher Modularitätsvertrag, kleine Fachtypen, schmale Ports und Feature-Untermodelle |
| Bedarf wird in UI und Solver unterschiedlich in Plätze zerlegt | einmalige deterministische Domain-Zerlegung mit stabiler Platzidentität |
| `B` wird irrtümlich als eigener Bedarf oder Dienst gespeichert | strukturierte Kennzeichnung an einer vorhandenen Typ1-Zuweisung und Negativtests |
| veraltete Eingaben werden unbemerkt geplant | unveränderlicher Snapshot, komponentenweiser Vergleich und bewusste Aktualisierung |
| Typ1 geht bei Aktualisierung verloren | gültige Zuweisungen erhalten; ungültige blockieren mit strukturierter Liste statt stiller Löschung |
| Tageskennzeichen und Typ1 werden in zwei Aufrufen inkonsistent | ein koordinierter Application-Command und eine SQLite-Transaktion |
| zwei überlappende Pläne entstehen bei parallelem Zugriff | Domain-Prüfung, Transaktionsprüfung, Versionierung und Konkurrenztests |
| UI-Snapshot wird zum Solververtrag | getrennte `ScheduleWorkspaceSnapshot`- und `PlanningInputSnapshot`-Modelle |
| neue Tabellen teilen die Datenbankarchitektur | vorhandener gemeinsamer `ServiceCatalogDbContext`, eine Migrationsfolge und Architekturtests |
| System 09 bis 12 werden vorweggenommen | expliziter Nicht-Umfang und Tests auf unverändertes Planning-Modul |
| echte Personaldaten gelangen in Tests oder Repository | ausschließlich klar erfundene Namen und synthetische Planwerte |

## Berichtsschema nach jedem Schritt

Nach jedem Schritt werden kurz genannt:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. kritische Meldungen mit Auswirkung und Dringlichkeit,
5. konkreter Handlungsbedarf oder ausdrücklich keiner,
6. offene Gates, Risiken oder Blockaden,
7. Git-Status ohne automatisches Staging, Commit oder Push,
8. nächster minimaler Schritt,
9. Bitte um ausdrückliche Abnahme, wenn ein Gate oder Handlungsbedarf besteht.

## Nächster minimaler Schritt

PM-01 bis PM-10 und damit System 08 sind vollständig geprüft, am 2026-09-16 ausdrücklich abgenommen und unter `docs/roadmaps/completed` archiviert. Der nächste mögliche Arbeitsschritt ist die Vorbereitung der Fachfragen und einer eigenen Teil-Roadmap für System 09; System 09 wurde noch nicht begonnen.
