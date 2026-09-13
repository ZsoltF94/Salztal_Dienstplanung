# Teil-Roadmap: Technisches App-Grundgerüst

Status: Abgeschlossen und archiviert am 2026-09-13

Stand: 2026-09-13

## Ziel und Nutzen

Diese Roadmap legt ein kompilierbares, fachlich noch leeres Grundgerüst der Salztal-Dienstplanung an. Am Ende bestehen die vorgesehene Lösungs- und Modulstruktur, eine leere deutsche WPF-Anwendung, zentrale Build- und Paketregeln sowie automatisierte Prüfungen der wichtigsten Architekturgrenzen.

Das Grundgerüst schafft eine belastbare technische Basis für die späteren Fachsysteme. Es nimmt noch keine Mitarbeiter-, Dienst-, Bedarfs-, Planungs-, Datenbank- oder Excel-Funktion vorweg.

## Verbindliche Grundlagen

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, abgenommen am 2026-09-13
- `ARCHITECTURE.md`, abgenommen am 2026-09-13
- `CLEANCODE.md`, abgenommen am 2026-09-13
- `AGENTS.md` mit dem verbindlichen kleinschrittigen Abnahmeprozess
- `MASTER_ROADMAP.md`, System 02 – Technisches App-Grundgerüst
- ausschließlich Windows 11 x64, C#, .NET 10 LTS und WPF
- vollständig lokaler und offline nutzbarer App-Betrieb
- getrennte Module für Domain, Application, Planning, Infrastructure, Excel und Desktop
- echte Mitarbeiter-, Planungs-, Datenbank-, Sicherungs- und Exportdaten bleiben außerhalb des Repositorys

## Bestätigter technischer Ausgangspunkt

- Im Repository existieren noch keine App- oder Testprojekte.
- Lokal ist das .NET SDK `10.0.102` für `win-x64` vorhanden.
- Die zugehörige .NET-10-Windows-Desktop-Laufzeit ist vorhanden.
- Es ist noch keine `global.json` vorhanden.
- Die Projektgrundlage ist abgeschlossen; im Arbeitsbaum bestanden vor diesem Entwurf keine offenen Änderungen.

Diese Feststellungen beschreiben nur die aktuelle Entwicklungsumgebung. Sie sind noch kein Nachweis für eine portable Auslieferung oder den Start auf dem späteren Klinikrechner.

## Umfang dieser Roadmap

- eine klassische Solution `Salztal.Dienstplanung.sln`
- zentrale SDK-, Compiler-, Formatierungs- und Paketverwaltung
- die sechs in `ARCHITECTURE.md` festgelegten Produktionsprojekte
- die sieben vorgesehenen Testprojekte
- ausschließlich die festgelegten Projektreferenzen
- die bereits architektonisch bestätigten technischen Pakete in ihren zuständigen Modulen
- eine fachlich leere WPF-Startanwendung mit deutschem Hinweis auf den Entwicklungsstand
- ein klar abgegrenzter Namespace `Salztal.Dienstplanung.Desktop.Composition`
- erste automatisierte Architekturtests für Projekt-, Paket- und Namespace-Grenzen
- reproduzierbare Paketwiederherstellung sowie warnungsfreier Build und Testlauf
- wahrheitsgemäße Aktualisierung von Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung`

## Nicht Bestandteil dieser Roadmap

- Mitarbeiter, Arbeitszeitmodelle, Qualifikationen oder Einsatzfreigaben
- Einsatzorte, Diensttypen, Bedarfe oder konkrete Planungsregeln
- Anwendungsfälle oder fachliche Datenmodelle
- produktive Datenbank, `DbContext`, Migrationen oder Sicherungslogik
- ein OR-Tools-Planungsmodell oder eine Konfliktdiagnose
- Excel-Vorlagenzugriff oder die Festlegung auf ClosedXML
- Navigation, Planansichten oder eine gestaltete Fachoberfläche
- Installer, ZIP-Erzeugung, Ein-Datei-Ausgabe oder abschließender portabler Build
- Startnachweis auf einem sauberen Windows-11- oder Klinikrechner
- Commit, Push oder externe Veröffentlichung

## Architekturgrenzen

Die Projektreferenzen werden auf folgende Richtungen begrenzt:

```text
Domain          -> keine Produktionsprojekte
Application     -> Domain
Planning        -> Application, Domain
Infrastructure  -> Application, Domain
Excel           -> Application
Desktop         -> Application, Planning, Infrastructure, Excel
```

Für `Desktop` gilt zusätzlich: Nur der Namespace `Salztal.Dienstplanung.Desktop.Composition` darf konkrete Typen aus Planning, Infrastructure oder Excel zusammensetzen. Views und ViewModels erhalten keinen Zugriff auf diese konkreten Adapter.

Externe Pakete bleiben in ihrem zuständigen Modul:

- `CommunityToolkit.Mvvm` ausschließlich in Desktop,
- Entity Framework Core und SQLite ausschließlich in Infrastructure,
- Google OR-Tools ausschließlich in Planning,
- noch keine Excel-Bibliothek, da der Vorlagentest aussteht,
- Testpakete ausschließlich in Testprojekten.

## Offene Entscheidungen

Vor dem ersten Implementierungsschritt besteht keine offene fachliche Entscheidung.

Die exakten stabilen Paketversionen werden erst in dem dafür vorgesehenen Schritt anhand offizieller Primärquellen geprüft. Dabei müssen Lizenz, Wartungsstatus, .NET-10-Kompatibilität und Windows-11-x64-Bereitstellung bestätigt werden. Ergibt diese Prüfung eine notwendige Abweichung vom abgenommenen Technologierahmen, wird die Roadmap vor jeder Paketaufnahme angepasst und erneut zur Abnahme vorgelegt.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Schritte

### TG-01 – Teil-Roadmap entwerfen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Ziel, Umfang, Nicht-Umfang, Architekturgrenzen, Schritte und echte Gates des technischen Grundgerüsts sind festgelegt.
- Die übergeordneten Statusdokumente verweisen auf diese aktive Roadmap.
- Es werden noch keine Solution, Projekte, Pakete oder App-Dateien angelegt.

Prüfung:

- Der Entwurf stimmt mit den abgenommenen Grundlagen, `ARCHITECTURE.md`, `CLEANCODE.md` und `MASTER_ROADMAP.md` überein.
- Jeder spätere Schritt besitzt ein kleines, eindeutig prüfbares Ergebnis.
- Fachfunktionen und spätere Auslieferungsgates werden nicht vorweggenommen.
- Lokale Markdown-Verweise, Statusangaben und `git diff --check` sind fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt diese Roadmap oder nennt Änderungswünsche.

### TG-02 – Zentrale Lösungs- und Buildgrundlage anlegen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- `Salztal.Dienstplanung.sln` wird als klassische Solution angelegt.
- `global.json` bindet die Entwicklung kontrolliert an das bestätigte .NET-10-SDK.
- `.editorconfig`, `Directory.Build.props` und `Directory.Packages.props` legen die gemeinsamen Regeln fest.
- Nullable Reference Types sind aktiviert, Warnungen im eigenen Produktionscode werden als Fehler behandelt und experimentelle Sprachversionen sind ausgeschlossen.
- Zentrale Paketverwaltung und die spätere reproduzierbare Wiederherstellung sind vorbereitet; es werden noch keine Fach- oder Adapterpakete aufgenommen.

Prüfung:

- SDK-Auswahl und zentrale MSBuild-Eigenschaften werden über die .NET-Werkzeuge ausgegeben und geprüft.
- Die leere Solution lässt sich öffnen und auswerten.
- Format- und Diff-Prüfung bestehen.

Abnahmebedingung:

- Die zentralen Dateien enthalten nur die vereinbarten gemeinsamen Regeln und noch keine versteckte Implementierung.

Tatsächlich umgesetzt:

- `Salztal.Dienstplanung.sln` als klassische, noch leere Solution angelegt.
- `global.json` auf das lokal bestätigte .NET SDK `10.0.102` mit erlaubtem Roll-forward innerhalb der Patchversion und ausgeschlossenen Vorabversionen festgelegt.
- `.editorconfig` mit UTF-8, CRLF, abschließender Leerzeile, Entfernung nachgestellter Leerzeichen sowie grundlegenden C#-, XAML-, XML- und Markdown-Regeln angelegt.
- `Directory.Build.props` mit .NET 10, stabilem aktuellem C#-Sprachstand, Nullable Reference Types, einheitlichen impliziten Usings, Warnungen als Fehlern, aktivierten .NET-Analyzern, Build-Codeformatprüfung und deterministischen Builds angelegt.
- `Directory.Packages.props` mit zentraler Paketversionsverwaltung und Vorbereitung von Paket-Lockdateien angelegt.
- Noch keine Produktions- oder Testprojekte, Paketreferenzen, Paket-Lockdateien oder App-Implementierung erzeugt.

### TG-03 – Domain und Application als innere Module anlegen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- `Salztal.Dienstplanung.Domain` und `Salztal.Dienstplanung.Application` werden unter `src` angelegt.
- Application referenziert ausschließlich Domain; Domain besitzt keine Projektreferenz.
- Beide Projekte enthalten nur minimale interne Assembly-Marker, keine Fachmodelle oder Anwendungsfälle.

Prüfung:

- Beide Projekte kompilieren ohne Warnungen.
- Die Projektreferenzen entsprechen exakt der festgelegten Richtung.

Abnahmebedingung:

- Die beiden inneren Module sind vorhanden, fachlich leer und unabhängig von technischen Bibliotheken.

Tatsächlich umgesetzt:

- `Salztal.Dienstplanung.Domain` und `Salztal.Dienstplanung.Application` als .NET-10-Klassenbibliotheken unter `src` angelegt und in die Solution aufgenommen.
- Beide Projekte beziehen Ziel-Framework, Nullable-, Using-, Analyzer- und Warnungseinstellungen ausschließlich aus `Directory.Build.props`.
- Domain besitzt keine Projektreferenz; Application referenziert ausschließlich Domain.
- Die öffentlichen Vorlagenklassen wurden entfernt und durch je einen internen Assembly-Marker im passenden Root-Namespace ersetzt.
- Beide Projekte erfolgreich mit 0 Warnungen und 0 Fehlern kompiliert.
- Nachgewiesen, dass beide Assemblies keine öffentlichen Typen enthalten.
- Keine Paketreferenzen, Paket-Lockdateien, Fachmodelle oder Anwendungsfälle angelegt.

### TG-04 – Technische Adaptermodule anlegen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Planning, Infrastructure und Excel werden als getrennte Projekte unter `src` angelegt.
- Planning und Infrastructure referenzieren Application und Domain.
- Excel referenziert ausschließlich Application.
- Die Projekte enthalten nur minimale interne Assembly-Marker und noch keine Adapterimplementierung.

Prüfung:

- Alle bis dahin vorhandenen Produktionsprojekte kompilieren ohne Warnungen.
- Kein technisches Modul referenziert ein anderes technisches Modul.

Abnahmebedingung:

- Die drei Adaptergrenzen sind als eigenständige, fachlich leere Assemblies vorhanden.

Tatsächlich umgesetzt:

- `Salztal.Dienstplanung.Planning`, `Salztal.Dienstplanung.Infrastructure` und `Salztal.Dienstplanung.Excel` als .NET-10-Klassenbibliotheken unter `src` angelegt und in die Solution aufgenommen.
- Planning und Infrastructure referenzieren ausschließlich Application und Domain.
- Excel referenziert ausschließlich Application.
- Kein technisches Adaptermodul referenziert ein anderes technisches Adaptermodul.
- Die öffentlichen Vorlagenklassen wurden entfernt und durch je einen internen Assembly-Marker im passenden Root-Namespace ersetzt.
- Alle fünf vorhandenen Produktionsprojekte erfolgreich mit 0 Warnungen und 0 Fehlern kompiliert.
- Nachgewiesen, dass die drei neuen Assemblies keine öffentlichen Typen enthalten.
- Keine Paketreferenzen, Paket-Lockdateien, Adapterimplementierungen oder Fachfunktionen angelegt.

### TG-05 – Leere WPF-Startanwendung und Composition-Grenze anlegen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- `Salztal.Dienstplanung.Desktop` wird als WPF-Projekt unter `src` angelegt.
- Das Desktop-Projekt referenziert Application sowie die drei technischen Adapterprojekte entsprechend der bestätigten Composition-Grenze.
- Die Anwendung zeigt nur einen deutschen Hinweis, dass das technische Grundgerüst vorhanden ist und noch keine Dienstplanfunktion implementiert wurde.
- Der Composition-Bereich wird als eigener Namespace angelegt; es entsteht noch keine fachliche Navigation oder Adapterlogik.

Prüfung:

- Die gesamte Produktionslösung kompiliert ohne Warnungen.
- Die App startet lokal und das leere Hauptfenster kann wieder geschlossen werden.
- Der lokale Start wird getrennt von einer späteren UI-, Portable- oder Zielrechnerabnahme berichtet.

Abnahmebedingung:

- Die leere WPF-App startet lokal, enthält keine Fachfunktion und wahrt die Composition-Grenze.

Tatsächlich umgesetzt:

- `Salztal.Dienstplanung.Desktop` als WPF-Projekt für `net10.0-windows` unter `src` angelegt und in die Solution aufgenommen.
- Desktop referenziert Application, Planning, Infrastructure und Excel; Domain wird nicht direkt referenziert.
- Den Namespace `Salztal.Dienstplanung.Desktop.Composition` mit einem internen Marker als klar abgegrenzten späteren Zusammensetzungsbereich angelegt.
- `App` und `MainWindow` bleiben intern und versiegelt; der öffentliche parameterlose Fensterkonstruktor ist ausschließlich für die technisch notwendige WPF-`StartupUri`-Aktivierung vorhanden.
- Ein leeres deutsches Hauptfenster mit dem Titel „Salztal Dienstplanung“ und den Hinweisen auf das eingerichtete Grundgerüst sowie die noch fehlenden Dienstplanfunktionen angelegt.
- Die gesamte Lösung mit sechs Produktionsprojekten erfolgreich mit 0 Warnungen und 0 Fehlern kompiliert.
- Die lokale EXE erzeugte ein reagierendes Hauptfenster mit dem erwarteten Titel und ließ sich kontrolliert wieder schließen.
- Keine Adapterregistrierung, Navigation, Paketreferenz, Fachfunktion oder Paket-Lockdatei angelegt.

### TG-06 – Technische Pakete kontrolliert aufnehmen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Die exakten stabilen Paketversionen werden anhand offizieller Primärquellen dokumentiert und zentral festgelegt.
- CommunityToolkit.Mvvm wird ausschließlich von Desktop referenziert.
- Entity Framework Core mit SQLite wird ausschließlich von Infrastructure referenziert.
- Google OR-Tools wird ausschließlich von Planning referenziert.
- Excel erhält noch keine Bibliothek.
- Paket-Lockdateien werden als versionierbare Dateien für eine wiederholbare Wiederherstellung erzeugt; Commit und Push bleiben einem ausdrücklichen Auftrag vorbehalten.

Prüfung:

- Lizenz, Wartungsstatus, .NET-10-Kompatibilität und Windows-11-x64-Eignung sind nachvollziehbar belegt.
- Normale und gesperrte Paketwiederherstellung bestehen.
- Die Lösung kompiliert nach der Paketaufnahme ohne Warnungen.
- Die Projektdateien enthalten keine Paketreferenz außerhalb des zuständigen Moduls.

Abnahmebedingung:

- Nur bestätigte stabile Pakete und ihre notwendigen transitiven Abhängigkeiten sind wiederherstellbar und korrekt gekapselt.

Tatsächlich umgesetzt:

- Die Prüfung und Auswahl ist in `docs/decisions/TECHNICAL_PACKAGE_BASELINE.md` mit offiziellen Quellen, Zweck, Lizenz, Wartungsstand, Kompatibilität und offenen Laufzeitgates dokumentiert.
- `CommunityToolkit.Mvvm` `8.4.2` zentral festgelegt und ausschließlich in Desktop direkt referenziert.
- `Microsoft.EntityFrameworkCore.Sqlite` `10.0.12` zentral festgelegt und ausschließlich in Infrastructure direkt referenziert.
- `Google.OrTools` `9.15.6755` zentral festgelegt und ausschließlich in Planning direkt referenziert.
- Domain, Application und Excel besitzen weiterhin keine direkte Paketreferenz; insbesondere wurde noch keine Excel-Bibliothek aufgenommen.
- Für alle sechs Produktionsprojekte wurden versionierbare `packages.lock.json`-Dateien erzeugt.
- Normale und gesperrte Paketwiederherstellung erfolgreich ausgeführt.
- Die gesamte Lösung nach der Paketaufnahme mit 0 Warnungen und 0 Fehlern kompiliert.
- Die aktuelle NuGet-Sicherheitsprüfung meldet keine bekannten verwundbaren direkten oder transitiven Pakete.
- Das praktische OR-Tools-Laufzeitgate für Visual C++ x64 sowie der portable Start auf einem sauberen Windows-11-System bleiben ausdrücklich bis System 15 offen.

### TG-07 – Testprojektstruktur anlegen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Die sechs vorgesehenen modulspezifischen Testprojekte und `Salztal.Dienstplanung.Architecture.Tests` werden unter `tests` angelegt.
- Testpakete und ihre Versionen werden zentral verwaltet und bleiben auf Testprojekte begrenzt.
- Jedes modulspezifische Testprojekt referenziert nur sein Produktionsprojekt; das Architektur-Testprojekt darf alle Produktionsassemblies untersuchen.
- Leere Platzhaltertests werden vermieden; noch nicht vorhandenes Fachverhalten wird nicht vorgetäuscht.

Prüfung:

- Alle Testprojekte werden von der Solution erfasst und kompilieren.
- Testpakete befinden sich nicht in Produktionsprojekten.
- Der Testlauf wird wahrheitsgemäß als Infrastrukturtest und noch nicht als Fachtest ausgewiesen.

Abnahmebedingung:

- Die vorgesehene Teststruktur steht bereit, ohne erfundene Fachtests oder leere Scheinerfolge.

Tatsächlich umgesetzt:

- Die sechs modulspezifischen Testprojekte und `Salztal.Dienstplanung.Architecture.Tests` unter `tests` angelegt und im Solution-Ordner `tests` erfasst.
- Jedes modulspezifische Testprojekt referenziert ausschließlich das gleichnamige Produktionsprojekt; das Architektur-Testprojekt referenziert alle sechs Produktionsprojekte.
- Desktop- und Architekturtests zielen wegen der WPF-Referenz auf `net10.0-windows`; die übrigen Testprojekte übernehmen zentral `net10.0`.
- Das aktuelle stabile `xunit.v3` `4.0.1` zentral festgelegt und ausschließlich in den sieben Testprojekten direkt referenziert.
- Für .NET 10 die Microsoft Testing Platform in `global.json` als Testläufer festgelegt.
- Für alle sieben Testprojekte versionierbare `packages.lock.json`-Dateien erzeugt; die gesperrte Wiederherstellung aller 13 Projekte besteht.
- Die vollständige Solution mit 13 Projekten erfolgreich mit 0 Warnungen und 0 Fehlern kompiliert.
- Alle sieben Testmodule wurden erfolgreich entdeckt und gestartet. Es wurden wahrheitsgemäß 0 Tests ausgeführt, weil TG-07 weder Platzhalter- noch vorgezogene Fach- oder Architekturtests anlegt.
- Die aktuelle NuGet-Sicherheitsprüfung meldet für alle Produktions- und Testprojekte keine bekannten verwundbaren direkten oder transitiven Pakete.

### TG-08 – Erste Architekturtests umsetzen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Automatische Tests prüfen die erlaubte Projektreferenzstruktur.
- Tests verhindern technische Pakete außerhalb ihres zuständigen Moduls.
- Tests sichern die besondere Grenze von `Desktop.Composition` gegenüber Views und ViewModels.
- Tests erkennen unerwartete neue Produktionsprojekte und zirkuläre Projektreferenzen.
- Die Prüfungen arbeiten mit den tatsächlichen Projekt- und Assemblydaten und nicht mit einer duplizierten Erfolgsliste ohne Bezug zur Lösung.

Prüfung:

- Der Architektur-Testlauf besteht im korrekten Grundgerüst.
- Mindestens je eine kontrollierte Testmutation weist nach, dass eine verbotene Projektreferenz, Paketposition und Desktop-Namespace-Nutzung erkannt wird; die Mutationen werden danach vollständig zurückgenommen.
- Anschließend bestehen Build, Gesamttestlauf und `git diff --check` erneut.

Abnahmebedingung:

- Die zentralen Modulgrenzen werden automatisch durchgesetzt und ihre Fehlererkennung ist nachgewiesen.

Tatsächlich umgesetzt:

- Zwölf ausführbare Architektur-Testfälle angelegt, die reale Projektdateien, kompilierte Produktionsassemblies und Desktop-Quelldateien untersuchen.
- Den erwarteten Satz aus sechs Produktionsprojekten sowie deren exakte erlaubte Projektreferenzen geprüft; unerwartete Produktionsprojekte und zirkuläre Referenzen werden erkannt.
- Die Referenzen der kompilierten Produktionsassemblies zusätzlich gegen die erlaubten Modulrichtungen geprüft.
- Den erwarteten Satz aus sieben Testprojekten sowie die zulässigen Referenzen jedes Testprojekts geprüft.
- Die direkte Platzierung von CommunityToolkit.Mvvm, Entity Framework Core/SQLite, OR-Tools, späteren Excel-Paketen und Testpaketen an ihren zuständigen Projektgrenzen geprüft.
- Geprüft, dass `xunit.v3` von allen und ausschließlich den sieben Testprojekten direkt referenziert wird.
- C#- und XAML-Quellen des Desktop-Projekts werden außerhalb von `Salztal.Dienstplanung.Desktop.Composition` auf verbotene Verwendungen von Planning, Infrastructure und Excel geprüft; generierte Builddateien bleiben ausgeschlossen.
- Eine kontrolliert eingefügte verbotene Referenz von Domain auf Infrastructure wurde durch zwei fehlschlagende Tests einschließlich Zykluserkennung nachgewiesen und anschließend vollständig entfernt.
- Eine kontrolliert eingefügte OR-Tools-Paketreferenz in Domain wurde durch zwei fehlschlagende Tests nachgewiesen und anschließend vollständig entfernt.
- Eine kontrolliert eingefügte Planning-Namespace-Verwendung außerhalb von `Desktop.Composition` wurde durch einen fehlschlagenden Test nachgewiesen und anschließend vollständig entfernt.
- Nach Entfernung aller Mutationen bestanden gesperrte Wiederherstellung, Gesamt-Build mit 0 Warnungen und 0 Fehlern sowie der vollständige Lauf mit 12 von 12 bestandenen Architekturtests.
- Keine Fachtests, Fachmodelle, Adapterimplementierungen oder neuen Pakete aufgenommen.

### TG-09 – Technisches Grundgerüst gemeinsam abschließen

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Solution, Produktionsprojekte, Testprojekte, zentrale Konfiguration, Paketgrenzen und Architekturtests werden gemeinsam abgeglichen.
- Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` zeigen denselben tatsächlichen Stand.
- System 02 wird erst nach der ausdrücklichen Abschlussabnahme als abgeschlossen archiviert.

Prüfung:

- Gesperrte Paketwiederherstellung, vollständiger Build und vollständiger automatischer Testlauf bestehen ohne neue Warnungen.
- Der lokale Start der leeren WPF-App ist bestätigt.
- Eine Suche findet keine veralteten Roadmap-Pfade oder widersprüchlichen Statusangaben.
- `git diff --check` meldet keine Whitespace-Fehler.
- Die Dateiliste enthält keine echten oder sensiblen Daten und keine Build-, Datenbank-, Sicherungs- oder Exportartefakte.

Abnahmebedingung:

- Der Auftraggeber bestätigt das technische Grundgerüst und erlaubt die Archivierung der Roadmap sowie den Übergang zu System 03 oder 04.

Tatsächlich umgesetzt:

- Den vollständigen Bestand aus sechs Produktionsprojekten, sieben Testprojekten und 13 Paket-Lockdateien mit Solution und geplanter Struktur abgeglichen.
- Projektreferenzen, zentrale .NET-Konfiguration, direkte Paketzuordnungen und Architekturtests gemeinsam gegen `ARCHITECTURE.md` und `CLEANCODE.md` geprüft.
- Die gesperrte Paketwiederherstellung und den vollständigen Build aller 13 Projekte erfolgreich ausgeführt; der Build meldet 0 Warnungen und 0 Fehler.
- Den vollständigen Testlauf erfolgreich ausgeführt; alle 12 Architekturtests bestehen ohne übersprungene oder fehlgeschlagene Tests.
- Die lokale Desktop-App technisch gestartet: Der Prozess reagierte, besaß ein Hauptfenster mit dem Titel „Salztal Dienstplanung“ und wurde anschließend regulär mit Exitcode 0 beendet.
- Der Auftraggeber hat den sichtbaren lokalen Start, den Fenstertitel und die beiden Hinweistexte am 2026-09-13 manuell bestätigt.
- Roadmap, `MASTER_ROADMAP.md`, `STATUS.md`, Paketentscheidung und `Service-Leitung/AKTUELLER_STAND.md` auf denselben tatsächlichen Stand gebracht.
- Die Suche nach veralteten Roadmap-Pfaden und widersprüchlichen früheren Statusangaben ergab keinen offenen Widerspruch; `git diff --check` meldet keine Whitespace-Fehler.
- Die Repository-Dateiliste enthält weiterhin 13 Projekt- und 13 Paket-Lockdateien, aber keine Build-, Datenbank-, Sicherungs-, Export- oder sonstigen sensiblen Datenartefakte.
- Keine Fachfunktion, Adapterimplementierung, Datenbank, Excel-Bibliothek oder portable Veröffentlichung ergänzt.
- Nach der ausdrücklichen Abschlussabnahme wurden System 02 und diese Roadmap als abgeschlossen gekennzeichnet und die Roadmap nach `docs/roadmaps/completed` verschoben.

## Echte externe und manuelle Gates

- Der lokale Start des leeren WPF-Fensters wurde in TG-05 und TG-09 manuell bestätigt; ein Build allein hätte diesen Nachweis nicht ersetzt.
- Paketinformationen werden erst in TG-06 anhand dann aktueller offizieller Primärquellen bestätigt.
- Eine selbstständige `win-x64`-Veröffentlichung ist in dieser Roadmap bewusst noch kein Abschlussgate; sie gehört zu System 15.
- Der Start auf einem sauberen Windows-11-System und auf dem vorgesehenen Klinikrechner bleibt bis System 15 offen.
- Die Visual-C++-Laufzeitvoraussetzung von OR-Tools wird bei der späteren portablen Veröffentlichung praktisch geprüft und hier nicht als bestanden gemeldet.
- Excel-Bibliothek, Vorlagentreue und Drei-Wochen-Druckbild bleiben bis zur echten Vorlage und System 13 offen.
- Eine fachliche App-Abnahme ist noch nicht möglich, weil dieses Grundgerüst keine Dienstplanfunktion enthält.

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

Das technische App-Grundgerüst ist abgeschlossen. Der nächste Schritt außerhalb dieser Roadmap ist die Entscheidung zwischen System 03 und System 04. Vor der Umsetzung des gewählten Fachsystems wird eine eigene Teil-Roadmap entworfen und abgenommen.
