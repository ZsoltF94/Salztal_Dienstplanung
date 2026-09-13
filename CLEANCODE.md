# Clean-Code- und Qualitätsregeln

Status: Grundfassung abgenommen am 2026-09-13; Fachbegriffe und reproduzierbare Zeilenenden für System 03 ergänzt am 2026-09-14

Stand: 2026-09-14

## Zweck und Geltung

Dieses Dokument macht überprüfbare Qualitätsregeln für die Salztal-Dienstplanung verbindlich. Es gilt für Produktionscode, Tests, Datenbankmigrationen, technische Skripte und technische Dokumentation.

`ARCHITECTURE.md` bestimmt die System- und Modulgrenzen. Dieses Dokument bestimmt, wie innerhalb dieser Grenzen gearbeitet wird. Bei einem Widerspruch hat `ARCHITECTURE.md` Vorrang. Eine notwendige Abweichung wird vor ihrer Umsetzung dokumentiert und abgenommen.

## Grundsätze

1. Fachliche Richtigkeit ist wichtiger als kurze oder besonders raffinierte Implementierung.
2. Jede Regel besitzt genau eine fachliche Quelle.
3. Abhängigkeiten zeigen nach innen zum Fachmodell.
4. Ein Modul kennt nur Daten und Dienste, die es für seine Verantwortung benötigt.
5. Fehler werden sichtbar und konkret behandelt.
6. Noch nicht geprüfte Ergebnisse werden nicht als fertig gemeldet.
7. Echte Mitarbeiter- und Plandaten gehören niemals in Quellcode, Tests, Logs oder Git.
8. Verständlichkeit für den nächsten Bearbeiter ist ein Qualitätsmerkmal.
9. Kleine, sichere Änderungen werden großen Mischänderungen vorgezogen.
10. Eine neue Abstraktion braucht einen konkreten aktuellen Zweck.

## Sprache und Begriffe

### Sichtbare Sprache

- Alle Texte in der Benutzeroberfläche sind deutsch.
- Konflikte, Hilfen und Fehlermeldungen verwenden die Begriffe der Service-Leitung.
- Technische Fehlercodes dürfen zusätzlich angezeigt werden, ersetzen aber keine verständliche Erklärung.

### Sprache im Code

- Typen, Methoden, Eigenschaften, Namespaces und technische Tests werden auf Englisch benannt.
- Fachbegriffe erhalten eine feste englische Bezeichnung und werden nicht wechselnd übersetzt.
- Eine einmal festgelegte Bezeichnung wird projektweit beibehalten.
- Abkürzungen werden nur verwendet, wenn sie allgemein bekannt oder im Projektglossar definiert sind.

Vorgesehene Begriffe:

| Fachbegriff | Codebegriff |
|---|---|
| Mitarbeiter | `Employee` |
| Mitarbeitertyp | `EmployeeType` |
| Wochen-Soll | `WeeklyWorkTarget` |
| Einsatzfreigabe | `ShiftEligibility` |
| Einsatzort | `WorkLocation` |
| Diensttyp | `ShiftType` |
| Einsatzmuster | `ShiftPattern` |
| Doppeldienst | `SplitShift` |
| Springer-Einsatz | `ReliefShiftPattern` |
| Bedarf | `StaffingDemand` |
| Verfügbarkeit | `Availability` |
| Abwesenheit | `Absence` |
| Plan | `Schedule` |
| Zuweisung | `Assignment` |
| Planversion | `ScheduleVersion` |
| zwingende Regel | `HardRule` |
| weiche Regel | `SoftRule` |
| Konflikt | `PlanningConflict` |

Dieses Glossar darf in einer späteren Fach-Roadmap präzisiert werden. Umbenennungen müssen vollständig und nicht schrittweise halb umgesetzt werden.

## C#-Grundeinstellungen

Beim späteren Projektgerüst gelten mindestens:

- Nullable Reference Types sind aktiviert.
- Compilerwarnungen im eigenen Produktionscode werden als Fehler behandelt.
- Der vom bestätigten .NET-10-SDK unterstützte stabile C#-Sprachstand wird verwendet.
- Vorab-Sprachversionen und experimentelle Compilerfunktionen sind nicht erlaubt.
- Implizite globale `using`-Direktiven werden bewusst und einheitlich konfiguriert.
- Formatierung und grundlegende Stilregeln werden über eine eingecheckte `.editorconfig` erzwungen.
- `.gitattributes` erzwingt für relevante Repository-Textdateien unabhängig von der lokalen Git-Konfiguration die in `.editorconfig` festgelegten CRLF-Zeilenenden; Shell-Skripte bleiben auf LF.
- Gemeinsame Build-Einstellungen liegen zentral in `Directory.Build.props`.
- Paketversionen liegen zentral in `Directory.Packages.props`.
- Reproduzierbare Paketwiederherstellung wird beim Projektgerüst festgelegt und geprüft.

Diese Dateien werden erst in der dafür freigegebenen Roadmap angelegt.

## Namespace-Regeln

### Rolle von Namespaces

- Projektdateien beziehungsweise Assemblies sind die harten Modulgrenzen.
- Namespaces ordnen Verantwortungen innerhalb eines Projekts und ersetzen keine Projektgrenze.
- Ein Namespace darf keine Abhängigkeit erlauben, die nach `ARCHITECTURE.md` verboten ist.
- Architekturtests prüfen sowohl Projektreferenzen als auch besonders geschützte Namespace-Grenzen.
- Der Root-Namespace jedes Projekts entspricht seinem Projektnamen.

Beispiele für Root-Namespaces:

```text
Salztal.Dienstplanung.Domain
Salztal.Dienstplanung.Application
Salztal.Dienstplanung.Planning
Salztal.Dienstplanung.Infrastructure
Salztal.Dienstplanung.Excel
Salztal.Dienstplanung.Desktop
```

### Fachliche Gliederung

Innerhalb von Domain, Application und Desktop werden fachliche Bereiche bevorzugt. Zusammengehörige Typen bleiben dadurch auch dann auffindbar, wenn ein System wächst.

Vorgesehene Beispiele:

```text
Salztal.Dienstplanung.Domain.Employees
Salztal.Dienstplanung.Domain.Scheduling
Salztal.Dienstplanung.Domain.Rules

Salztal.Dienstplanung.Application.Employees
Salztal.Dienstplanung.Application.Scheduling
Salztal.Dienstplanung.Application.Exports

Salztal.Dienstplanung.Desktop.Features.Employees
Salztal.Dienstplanung.Desktop.Features.Scheduling
```

Ein neuer fachlicher Namespace wird nur angelegt, wenn er eine klar benennbare Verantwortung enthält. Eine einzelne Datei rechtfertigt nicht automatisch einen eigenen tiefen Namespace.

### Technische Gliederung

Technische Projekte werden nach ihrer gekapselten technischen Verantwortung gegliedert.

Vorgesehene Beispiele:

```text
Salztal.Dienstplanung.Planning.ModelBuilding
Salztal.Dienstplanung.Planning.Diagnostics

Salztal.Dienstplanung.Infrastructure.Persistence
Salztal.Dienstplanung.Infrastructure.Backup
Salztal.Dienstplanung.Infrastructure.FileSystem
Salztal.Dienstplanung.Infrastructure.Logging

Salztal.Dienstplanung.Excel.Templates
Salztal.Dienstplanung.Excel.Validation

Salztal.Dienstplanung.Desktop.Composition
Salztal.Dienstplanung.Desktop.Shared
```

`Desktop.Composition` ist der einzige Namespace der Oberfläche, der konkrete Typen aus Planning, Infrastructure oder Excel registrieren und erzeugen darf. Views, ViewModels und Feature-Namespaces dürfen diese konkreten Adapter nicht referenzieren.

### Ordner und Namespaces

- Ordnerstruktur und Namespace stimmen grundsätzlich überein.
- Eine Abweichung benötigt einen Framework-, Generator- oder Migrationsgrund und einen kurzen Kommentar in der Projektkonfiguration.
- Namespace-Tiefe wird nicht künstlich durch bedeutungslose Zwischenordner erhöht.
- Typen werden nicht allein wegen ihrer technischen Form global gesammelt.
- Ein projektweites Sammelbecken wie `Models`, `Services`, `ViewModels`, `Helpers`, `Managers`, `Common`, `Misc` oder `Data` ist nicht erlaubt.
- Technische Formbegriffe dürfen innerhalb eines klaren fachlichen Bereichs verwendet werden, beispielsweise `Desktop.Features.Scheduling.ViewModels`, wenn dadurch zusammengehörige Scheduling-Dateien zusammenbleiben.

### Sichtbarkeit

- Typen sind standardmäßig `internal`.
- `public` ist ausschließlich für einen notwendigen Modulvertrag erlaubt.
- Ein öffentlicher Typ liegt in einem Namespace, dessen Rolle als Modulvertrag erkennbar ist.
- Interne Implementierungen werden nicht öffentlich gemacht, nur um Tests oder Dependency Injection zu vereinfachen.
- Falls Tests interne Typen begründet benötigen, wird ein gezielter Testzugang dokumentiert; die Produktions-API wird dafür nicht verbreitert.

### Tests

- Test-Namespaces spiegeln den Produktionsbereich im jeweiligen Testprojekt. Beispiel: `Salztal.Dienstplanung.Domain.Employees` wird durch `Salztal.Dienstplanung.Domain.Tests.Employees` getestet.
- Architekturtests erhalten den eigenen Root-Namespace `Salztal.Dienstplanung.Architecture.Tests`.
- Testhilfen liegen beim betroffenen Fachbereich und nicht in einem globalen unspezifischen Hilfsnamespace.
- Namespace-Tests prüfen mindestens die geschützte `Composition`-Grenze und das Verbot technischer Bibliothekstypen außerhalb ihres Moduls.

### Änderungen an der Namespace-Struktur

- Verschiebungen werden vollständig durchgeführt: Datei, Namespace, Verwendungen, Tests und Dokumentation werden gemeinsam aktualisiert.
- Parallel vorhandene alte und neue Namespaces für denselben Bereich sind nicht erlaubt.
- Ein neuer Root-Namespace oder eine neue projektübergreifende Namespace-Abhängigkeit ist eine Architekturänderung und benötigt vorherige Abnahme.

## Dateien und Typen

- Eine Datei besitzt einen klaren Haupttyp oder eine klar zusammengehörige kleine Typgruppe.
- Dateiname und Haupttyp stimmen überein.
- Pro Datei wird ein file-scoped Namespace verwendet, sofern Generatoren oder Frameworkvorgaben nicht dagegen sprechen.
- Sichtbarkeit ist so klein wie möglich: zuerst `private`, dann `internal`, nur bei einem echten Modulvertrag `public`.
- Klassen werden `sealed`, wenn Vererbung nicht ausdrücklich Teil des Entwurfs ist.
- Unveränderliche Daten werden bevorzugt als immutable records oder Value Objects modelliert.
- Öffentliche veränderliche Felder sind untersagt.
- Statischer veränderlicher globaler Zustand ist untersagt.
- Partielle Klassen werden nur für Framework- oder Generatoranforderungen verwendet.

## Benennung

- Typen, Methoden und öffentliche Eigenschaften verwenden `PascalCase`.
- Parameter und lokale Variablen verwenden `camelCase`.
- Private Felder verwenden `_camelCase`.
- Interfaces verwenden das Präfix `I`, wenn sie eine echte austauschbare Grenze darstellen.
- Asynchrone Methoden tragen das Suffix `Async`.
- Boolesche Namen beginnen nach Möglichkeit mit `Is`, `Has`, `Can`, `Should` oder `Requires`.
- Collections werden im Plural benannt.
- Befehlsmethoden beschreiben eine Handlung; Abfragen beschreiben das gelieferte Ergebnis.
- Namen wie `Manager`, `Helper`, `Util`, `Common`, `Data` oder `Misc` sind ohne engere fachliche Bedeutung nicht zulässig.
- Versionsnummern oder Implementierungsdetails gehören nicht in Fachtypnamen, außer sie sind Teil eines echten externen Vertrags.

## Methoden und Klassen

- Eine Methode erledigt eine klar benennbare Aufgabe auf einem Abstraktionsniveau.
- Ein Typ besitzt genau einen fachlichen oder technischen Änderungsgrund.
- Verschachtelte Bedingungen werden durch frühe Rückgaben oder klar benannte Teilentscheidungen lesbar gehalten.
- Mehr als vier fachlich zusammengehörige Parameter sind ein Signal für ein benanntes Eingabeobjekt.
- Boolesche Steuerparameter werden vermieden, wenn getrennte Methoden die Absicht klarer ausdrücken.
- Methoden mit ungefähr mehr als 30 Zeilen und Klassen mit ungefähr mehr als 250 Zeilen werden überprüft; die Zahlen sind Warnsignale, keine automatischen Fehler.
- Kommentare dürfen keine unverständliche Struktur rechtfertigen. Zuerst wird die Struktur verbessert.
- Kopierter Fachcode wird nicht als schnelle zweite Implementierung akzeptiert.

## Fachmodell

### Invarianten

- Ein ungültiges Fachobjekt darf nach erfolgreicher Erzeugung nicht existieren.
- Konstruktoren oder Fabriken prüfen zwingende lokale Bedingungen.
- Fehlerhafte Benutzereingaben liefern fachliche Validierungsergebnisse und keine technischen Ausnahmefehler.
- Wertobjekte werden für fachlich wichtige Werte verwendet, beispielsweise Kennungen, Kalenderwoche, Arbeitsdauer oder Regelpriorität.
- IDs sind stark typisiert, wenn dadurch Verwechslungen zwischen Mitarbeiter, Dienst, Standort oder Plan verhindert werden.

### Zeit und Dauer

- Kalendertage verwenden `DateOnly`.
- Uhrzeiten ohne Datum verwenden `TimeOnly`.
- Arbeitsdauern werden als ganze Minuten oder durch einen geprüften Werttyp gespeichert.
- Für Arbeitsstunden werden keine `double`- oder `float`-Werte verwendet.
- Umrechnungen in Stunden dienen nur der Anzeige.
- Ein Diensttyp besitzt eine bearbeitbare Standardzeit; ein Bedarf und eine geplante Zuweisung besitzen ihre tatsächlichen Zeiten.
- Ein einzelner Bedarf referenziert genau einen normalen Diensttyp. Doppeldienst und Springer bleiben zusammengesetzte Muster und dürfen nicht als zusätzliche bedarfsfähige Diensttypen modelliert werden.
- Berechnungen verwenden die tatsächlichen Bedarfs- und Zuweisungszeiten und greifen nicht versehentlich auf eine abweichende Standardzeit zurück.
- Eine Datums-Ausnahme bleibt vom zukünftigen Standard getrennt. Abgenommene Planversionen bewahren die damaligen tatsächlichen Zeiten als Momentaufnahme.
- Die aktuelle Zeit wird über eine injizierte Uhr bezogen; Fachcode ruft nicht direkt `DateTime.Now` oder `DateTime.UtcNow` auf.
- Lokale Klinikzeiten und technische Zeitstempel werden ausdrücklich unterschieden.

### Regeldefinitionen

- Jede Regel besitzt eine stabile `RuleId` und einen eindeutigen Regeltyp.
- Parameter und Priorität liegen ausschließlich in der fachlichen Regeldefinition.
- UI, Datenbank und Solver dürfen keine eigenen Kopien derselben Grenzwerte führen.
- Eine unbekannte Regel wird sichtbar abgelehnt und nie ignoriert.
- Jede neue Regel benötigt Beispiele für erfüllt, verletzt und nicht anwendbar.

## Anwendungsschicht

- Jeder Anwendungsfall besitzt eine klare Eingabe, ein klares Ergebnis und eine benennbare Verantwortung.
- Commands ändern Zustand; Queries lesen Zustand. Beide werden nicht unnötig in generischen Allzweckdiensten vermischt.
- Die Anwendungsschicht koordiniert Transaktionen, Berechtigungen des Ablaufs, Planstatus und technische Ports.
- Sie enthält keine WPF-, EF-Core-, OR-Tools-, ClosedXML- oder Open-XML-Typen.
- Schnittstellen werden an der Stelle definiert, die sie benötigt, nicht beim technischen Anbieter.
- Generische Repository-Schnittstellen mit beliebigen CRUD-Operationen werden vermieden. Speicherverträge werden nach den tatsächlichen Anwendungsfällen benannt.
- Ein Anwendungsfall lädt alle benötigten Daten, bevor er Planning oder Excel aufruft.

## Unveränderliche Übergabemodelle

- `PlanningRequest`, `PlanningResult`, `ApprovedPlanExport` und `ExportResult` sind unveränderlich.
- Collections werden als nur lesbare Momentaufnahme übergeben.
- Übergabemodelle enthalten keine verzögerten Datenbankabfragen.
- `DbContext`, `IQueryable`, WPF-Objekte, OR-Tools-Objekte und Excel-Bibliotheksobjekte dürfen keine Modulgrenze überschreiten.
- Ein Adapter lädt keine versteckten Zusatzdaten nach.
- Mapping zwischen Fachmodell und Übergabemodell findet an einer klar benannten Stelle statt und besitzt Tests.

## Planungsmodul und OR-Tools

- OR-Tools wird ausschließlich im Planning-Projekt referenziert.
- Fachregeln werden über explizite Übersetzer in Solver-Bedingungen übertragen.
- Ein Regelübersetzer behandelt genau einen klaren Regeltyp oder eine eng zusammengehörige Regelfamilie.
- Jede Solver-Bedingung bleibt über eine fachliche Regelkennung rückverfolgbar.
- Solver-Variablen erhalten stabile, diagnostisch hilfreiche Namen ohne echte Personennamen.
- Unbesetzte Plätze sind ausdrückliche Modellwerte und keine technische Ausnahme.
- Teilweise gedeckte Bedarfszeiträume behalten den ungedeckten Zeitraum als ausdrückliches fachliches Ergebnis; eine bloße Diensttypzuordnung darf ihn nicht verbergen.
- Überbesetzung wird nicht nachträglich aus einem Ergebnis entfernt, sondern bereits im Modell ausgeschlossen.
- Prioritätsstufen werden hierarchisch abgesichert. Beliebige magische Strafwerte ohne Dominanznachweis sind untersagt.
- Solver-Status, Laufzeit, Version und relevante Einstellungen werden im Ergebnis dokumentiert.
- Abbruch und Zeitüberschreitung sind reguläre Ergebnisse und keine Erfolgsmeldung.
- Planung unterstützt `CancellationToken`, soweit OR-Tools dies technisch zulässt; andernfalls wird eine kontrollierte Abbruchgrenze gekapselt.

## Konflikterklärung

- Planning liefert Ursachecodes, Lösungscodes und fachliche Parameter, keine fertigen deutschen Sätze.
- Die Anwendungsschicht erzeugt daraus die verständliche deutsche Erklärung.
- ViewModels erfinden keine Ursachen oder Lösungsvorschläge.
- Ursachecodes sind stabil und werden nicht für einzelne UI-Formulierungen umbenannt.
- Jede Konfliktart besitzt Tests für strukturierte Daten und deutschen Meldungstext.
- Eine Lösungsmöglichkeit ist immer als Vorschlag gekennzeichnet und verändert keine Daten selbstständig.
- Es werden keine Personen als „Fehlerursache“ formuliert; die Meldung beschreibt Regel, Verfügbarkeit und Bedarf sachlich.

## WPF und MVVM

- XAML beschreibt Darstellung, Bindings und Styles.
- ViewModels beschreiben Zustand und Benutzeraktionen der Ansicht.
- Anwendungs- und Fachlogik gehört nicht in Code-behind.
- Code-behind ist nur für rein visuelles Verhalten zulässig, das nicht sinnvoll bindbar ist.
- ViewModels verwenden ausschließlich Anwendungsschnittstellen und UI-eigene Abstraktionen.
- Konkrete Adapter werden nur im `Composition`-Bereich registriert.
- Der Service-Locator-Ansatz ist untersagt; Abhängigkeiten werden über Konstruktoren übergeben.
- Befehle verwenden `RelayCommand` beziehungsweise `AsyncRelayCommand` nur als UI-Adapter.
- `async void` ist ausschließlich für echte UI-Ereignishandler erlaubt.
- `.Wait()`, `.Result` und blockierendes Warten auf asynchrone Arbeit sind im UI-Pfad untersagt.
- Längere Planung, Sicherung oder Exporte zeigen einen verständlichen Arbeitszustand und blockieren die Oberfläche nicht.
- Benutzeraktionen werden während eines laufenden widersprüchlichen Vorgangs gezielt deaktiviert und nicht global versteckt.
- ViewModels greifen weder auf `Application.Current` noch direkt auf Fenster, MessageBoxen oder Dispatcher zu.

## Entity Framework Core und SQLite

- EF-Core- und SQLite-Abhängigkeiten bleiben in `Infrastructure`.
- Das Fachmodell erhält keine EF-Attribute.
- Tabellen-, Schlüssel-, Index- und Beziehungskonfigurationen liegen in Infrastructure-Mappings.
- Ein `DbContext` ist kurzlebig und wird nicht global gehalten.
- Lazy Loading ist nicht erlaubt.
- Abfragen projizieren nur benötigte Daten und vermeiden versteckte N+1-Zugriffe.
- `IQueryable` verlässt Infrastructure nicht.
- Schreibende Anwendungsfälle verwenden explizite Transaktionen, wenn mehrere Änderungen gemeinsam gelten müssen.
- Migrationen werden niemals manuell nachträglich umgeschrieben, sobald sie in einem verwendeten Stand angekommen sind.
- Vor produktiven Migrationen werden Sicherung und Wiederherstellung getestet.
- Datums-, Uhrzeit- und Dauerabbildungen erhalten round-trip Tests.
- Nebenläufigkeitsannahmen bleiben auf Einzelbenutzerbetrieb begrenzt und werden nicht als Mehrbenutzersicherheit dargestellt.

## Infrastructure-Unterbereiche

Die Bereiche `Persistence`, `Backup`, `FileSystem` und `Logging` bleiben intern getrennt:

- keine gegenseitigen Zugriffe auf interne Implementierungstypen,
- gemeinsame Abläufe nur über Anwendungsschnittstellen oder kleine ausdrücklich freigegebene technische Verträge,
- getrennte Tests je Bereich,
- keine Sammelklasse für alle lokalen Dienste.

Ein eigener Projekt-Split erfolgt erst nach einer dokumentierten Notwendigkeit. Viele Projekte ohne echte Grenze sind ebenso unerwünscht wie ein unstrukturierter Infrastructure-Block.

## Excel-Modul

- ClosedXML oder Open XML SDK werden ausschließlich im Excel-Projekt referenziert.
- Der Adapter erhält nur `ApprovedPlanExport`.
- Zellpositionen, Bereiche, Blattnamen und Vorlagenversion werden in einer zentralen Vorlagenbeschreibung gekapselt.
- Zelladressen werden nicht über mehrere Klassen verteilt als Zeichenketten fest codiert.
- Die Originalvorlage wird niemals überschrieben.
- Export wird zunächst in eine temporäre Zieldatei geschrieben und erst nach erfolgreicher Validierung als Ergebnis bereitgestellt.
- Eine teilweise geschriebene Datei wird nicht als erfolgreicher Export gemeldet.
- Öffnen und unverändertes Speichern der Vorlage ist ein eigener Charakterisierungstest.
- Format, Druckbereich, Seitenaufteilung, verbundene Zellen, Formeln und benannte Bereiche werden separat geprüft.
- Excel-Tests verwenden synthetische Namen und Daten.
- Ein Bibliothekswechsel bleibt innerhalb des Excel-Moduls.

## Fehlerbehandlung

### Fachliche Fehler

- Erwartbare Probleme werden als typisierte Ergebnisse zurückgegeben.
- Jede Meldung besitzt einen stabilen Code und strukturierte Parameter.
- Validierungsfehler können gesammelt angezeigt werden, wenn mehrere Eingaben unabhängig korrigierbar sind.

### Technische Fehler

- Unerwartete technische Fehler werden an einer geeigneten äußeren Grenze protokolliert und in eine verständliche Meldung übersetzt.
- Eine Ausnahme wird nur behandelt, wenn die behandelnde Stelle sinnvoll reagieren, ergänzende Information hinzufügen oder sauber übersetzen kann.
- Leere `catch`-Blöcke und pauschales Verschlucken von Fehlern sind untersagt.
- `catch (Exception)` ist nur an äußersten Prozess- oder Jobgrenzen mit Protokollierung und definiertem Ergebnis erlaubt.
- Ausnahmen werden nicht für normale Verzweigungen verwendet.
- Ressourcen werden mit `using` beziehungsweise `await using` zuverlässig freigegeben.

## Asynchronität und Abbruch

- Datei-, Datenbank- und andere potenziell längere Vorgänge werden asynchron ausgeführt, wenn die verwendete API dies sinnvoll unterstützt.
- Öffentliche asynchrone Anwendungsfälle akzeptieren einen `CancellationToken`.
- Tokens werden bis zur tatsächlichen Operation weitergereicht.
- Nach einer Abbruchanforderung wird kein Erfolg gemeldet.
- Abbruch hinterlässt weder teilweise gespeicherte Pläne noch gültig wirkende Exporte.
- CPU-intensive Planung läuft nicht auf dem WPF-UI-Thread.
- Unkontrolliertes `Task.Run` in ViewModels ist untersagt; Ausführung und Lebensdauer werden in der Anwendungsschicht koordiniert.

## Protokollierung und Datenschutz

- Logs dienen technischer Diagnose und sind kein zweites Facharchiv.
- Keine vollständigen Namen, Krankheitsgründe, Wunschfrei-Texte oder Planinhalte in technischen Logs.
- Wenn eine Zuordnung nötig ist, werden technische IDs oder für die Diagnose erzeugte Korrelationskennungen verwendet.
- Zugangsdaten, Tokens und lokale Pfade mit persönlichen Ordnernamen werden nicht protokolliert.
- Logmeldungen sind strukturiert und besitzen Ereigniskennungen.
- Exceptions werden mit technischem Kontext protokolliert, aber sensible Fachdaten werden vorher entfernt.
- Logdateien besitzen eine begrenzte Größe und Aufbewahrung.
- Eine fehlgeschlagene Protokollierung darf keinen gültigen Fachvorgang beschädigen.

## Tests

### Allgemein

- Ein Test prüft einen klar benannten Sachverhalt.
- Testnamen beschreiben Ausgangslage, Handlung und erwartetes Ergebnis.
- Tests sind unabhängig von Ausführungsreihenfolge und lokaler Uhrzeit.
- Tests verwenden ausschließlich synthetische Daten.
- Zufall wird durch einen festen Startwert kontrolliert.
- Flaky Tests werden nicht einfach wiederholt oder ignoriert, sondern als Fehler untersucht.
- Ein Bugfix erhält zuerst oder gleichzeitig einen Test, der den Fehler reproduziert.
- Interne Implementierungsdetails werden nur getestet, wenn sie ein wichtiger technischer Vertrag sind.
- Mocks werden an externen Grenzen verwendet, nicht für jedes Fachobjekt.

### Fachregeltests

Jede Regel benötigt mindestens:

- einen erfüllten Fall,
- einen verletzten Fall,
- Grenzwerte,
- einen nicht anwendbaren Fall,
- Kombinationen mit relevanten anderen Regeln,
- Prüfung der richtigen Regelkennung und Priorität.

### Planning-Tests

- deckbarer Bedarf ohne Konflikt,
- nicht deckbarer Bedarf mit weiterhin erzeugtem Restplan,
- Bedarf mit einer ausdrücklich von der Diensttyp-Standardzeit abweichenden tatsächlichen Zeit,
- genau ein verlangter Diensttyp je Bedarf,
- samstäglicher Springer-Einsatz nur als Notfall und mit sichtbar verbleibender Teilunterdeckung vor dem Einsatzortwechsel,
- keine Überbesetzung,
- unverletzte Hard Rules,
- Prioritätsreihenfolge hoch vor mittel vor niedrig,
- gesperrte Zuweisungen,
- Doppeldienstbedingungen,
- reproduzierbares Ergebnis bei gleichen Eingaben,
- Zeitgrenze und Abbruchstatus,
- Ursache und Lösungsmöglichkeiten für vollständig oder teilweise ungedeckte Bedarfszeiträume.

### Architekturtests

- Projektreferenzen entsprechen `ARCHITECTURE.md`.
- Verbotene Bibliothekstypen überschreiten keine Modulgrenze.
- ViewModels referenzieren keine konkreten technischen Adapter.
- Nur `Composition` baut konkrete Adapter zusammen.
- Neue Produktionsprojekte müssen in der Architektur freigegeben sein.

### Datenbank- und Excel-Tests

- Datenbanktests verwenden eine echte temporäre SQLite-Datei, wenn SQLite-Verhalten relevant ist.
- Migrationen werden von der ältesten unterstützten Testversion bis zum aktuellen Stand geprüft.
- Sicherung und Wiederherstellung werden als vollständiger Ablauf getestet.
- Excel-Tests vergleichen nicht nur Zellwerte, sondern auch die vereinbarten Format- und Druckmerkmale.
- Temporäre Testdateien werden kontrolliert innerhalb des Testordners angelegt und entfernt.

## Testbarkeit vor Bequemlichkeit

- Aktuelle Zeit, Dateisystem, Solver und Export werden hinter schmalen Schnittstellen gekapselt.
- Reine Fachberechnungen bleiben ohne Mocks testbar.
- Private Methoden werden nicht nur für Tests öffentlich gemacht.
- Ist ein Verhalten schwer testbar, wird zuerst die Verantwortung und Abhängigkeit überprüft.
- Testhilfen erzeugen lesbare synthetische Mitarbeiter, Schichten und Regeln, ohne Produktionslogik zu duplizieren.

## Abhängigkeiten und Pakete

- Ein Paket wird nur aufgenommen, wenn sein Nutzen den Wartungs- und Bereitstellungsaufwand rechtfertigt.
- Vor Aufnahme werden Lizenz, Wartungsstatus, unterstützte .NET-Version und Windows-x64-Bereitstellung geprüft.
- Direkte Paketabhängigkeiten werden zentral dokumentiert und versioniert.
- Transitive Abhängigkeiten werden bei sicherheits- oder deploymentrelevanten Paketen geprüft.
- Paketupdates sind eigene nachvollziehbare Änderungen mit Build und betroffenen Tests.
- Vorabversionen sind in produktiven Abhängigkeiten nicht erlaubt.
- Reflection-basierte oder quellgenerierende Pakete werden auf portable Veröffentlichung geprüft.
- Ein Paket darf die Architekturgrenzen nicht umgehen.

## Kommentare und Dokumentation

- Code erklärt das Was durch Namen und Struktur.
- Kommentare erklären ein nicht offensichtliches Warum, eine fachliche Quelle oder eine technische Einschränkung.
- Auskommentierter Code wird gelöscht; Git bewahrt die Historie.
- TODOs enthalten Grund und zugehörige Roadmap beziehungsweise Aufgabe.
- Öffentliche Modulverträge und komplexe Fachalgorithmen werden knapp dokumentiert.
- Eine Änderung von Verhalten aktualisiert Tests, Fachentscheidung, Roadmap, `STATUS.md` und die betroffene Service-Leitungsinformation im selben abgenommenen Schritt.

## Verbotene Muster

- Fachlogik in WPF-Code-behind oder ViewModels,
- Datenbankzugriff aus UI oder Planning,
- OR-Tools-Typen außerhalb von Planning,
- Excel-Bibliothekstypen außerhalb von Excel,
- Service Locator und globale veränderliche Singletons,
- generische `Manager`- oder `Helper`-Sammelklassen,
- stille Fallbacks bei unbekannten Regeln,
- magische Regelwerte in mehreren Modulen,
- `.Result`, `.Wait()` oder blockierende Sleeps im UI-Pfad,
- leere `catch`-Blöcke,
- Erfolgsmeldungen vor Abschluss von Speicherung oder Export,
- echte Mitarbeiterdaten in Tests, Beispielen oder Git,
- Änderungen außerhalb des freigegebenen Roadmap-Schritts.

## Warnsignale bei Reviews

Folgende Beobachtungen verlangen eine bewusste Prüfung:

- Ein neuer Typ benötigt Abhängigkeiten aus mehreren technischen Modulen.
- Ein ViewModel kennt Datenbank- oder Solverbegriffe.
- Eine neue Regel verändert gleichzeitig Domain, UI-Sonderlogik und Datenbankabfrage.
- Eine Methode erhält mehrere boolesche Steuerparameter.
- Dieselbe fachliche Berechnung erscheint in mehreren Projekten.
- Ein Fehler wird nur geloggt, obwohl die Service-Leitung reagieren muss.
- Ein Test benötigt viele Mocks, um eine kleine Fachentscheidung zu prüfen.
- Eine Klasse wächst weiter, weil „alles zu diesem Thema“ dort gesammelt wird.
- Ein Paket wird nur aufgenommen, um wenige leicht verständliche Zeilen zu vermeiden.
- Ein lokaler erfolgreicher Build wird als vollständige Funktionsabnahme dargestellt.

## Definition of Done für einen Implementierungsschritt

Ein Schritt ist erst bereit zur Abnahme, wenn:

1. ausschließlich der freigegebene Umfang umgesetzt wurde,
2. Architektur- und Modulgrenzen eingehalten sind,
3. neue oder geänderte Regeln eine einzige fachliche Quelle besitzen,
4. relevante Tests ergänzt wurden und bestehen,
5. die Lösung ohne neue Warnungen kompiliert,
6. Fehler- und Abbruchpfade geprüft wurden,
7. keine echten Mitarbeiter- oder Plandaten hinzugefügt wurden,
8. Dokumentation und Status den tatsächlichen Stand zeigen,
9. ausstehende manuelle, Excel- oder Windows-Gates ausdrücklich offen bleiben,
10. der nächste minimale Schritt benannt wird,
11. die Abnahme des Auftraggebers noch nicht vorweggenommen wird.

Welche Prüfungen konkret erforderlich sind, hängt vom Schritt ab. Nicht betroffene Gates müssen nicht künstlich ausgeführt werden; betroffene Gates dürfen nicht ausgelassen oder zusammengefasst werden.

## Abweichungen

Eine Regel dieses Dokuments darf nur abweichen, wenn:

1. ein konkreter technischer oder fachlicher Grund dokumentiert ist,
2. die Auswirkungen auf Wartbarkeit, Tests und Architektur beschrieben sind,
3. eine kleinere Alternative geprüft wurde,
4. die Abweichung vor der Umsetzung abgenommen wurde,
5. dieses Dokument oder ein verlinkter Entscheidungsnachweis aktualisiert wird.
