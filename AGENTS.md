# Arbeitsanweisungen für dieses Repository

## Geltungsbereich

Diese Datei gilt für das gesamte Repository `Salztal_Dienstplanung` und für alle darunterliegenden Dateien und Ordner.

Alle Bearbeiter und automatisierten Agenten müssen diese Regeln vor einer Änderung lesen und einhalten. Untergeordnete `AGENTS.md`-Dateien dürfen nur für einen klar begrenzten Teilbereich zusätzliche Regeln festlegen und den Root-Regeln nicht widersprechen.

## Verbindliche Projektquellen

Vor einer Aufgabe sind die dafür relevanten Dokumente zu lesen:

1. `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md` für bestätigte Produktgrundlagen,
2. `ARCHITECTURE.md` für Zielplattform, Module und Abhängigkeitsgrenzen,
3. `CLEANCODE.md` für Code-, Namespace-, Test- und Qualitätsregeln,
4. die aktive Teil-Roadmap unter `docs/roadmaps/active`,
5. nach ihrer Erstellung `MASTER_ROADMAP.md` und `STATUS.md`,
6. `Service-Leitung` für den einfach verständlichen, fachlichen Projektstand.

Eine aktuelle ausdrückliche Anweisung des Auftraggebers kann eine frühere Projektentscheidung ändern. Die Änderung muss dann im selben Schritt in allen betroffenen Dokumenten nachvollziehbar festgehalten werden.

## Projektziel in Kürze

- klassische Offline-App für Windows 11 x64,
- Einzelbenutzer: Service-Leitung einer Rehaklinik,
- gastronomische Einsatzorte zunächst Cafeteria und Restaurant,
- automatische mehrwöchige Dienstplanung aus normalen Diensttypen mit bearbeitbaren Standardzeiten, zusammengesetzten Einsatzmustern und ausdrücklich festgelegten tatsächlichen Bedarfszeiten,
- zwingende Regeln und priorisierte weiche Regeln,
- verständliche Konflikte und Lösungsmöglichkeiten,
- manuelle Bearbeitung ohne ungefragte Neugenerierung,
- unveränderliche Planversionen bei jeder Abnahme,
- Excel-Export erst nach Abnahme und exakt nach bereitgestellter Vorlage,
- lokale Datenbank, lokale Sicherungen und keine Cloud,
- Zeitkonten erst in einer späteren Ausbaustufe.

Windows 10, Mehrbenutzerbetrieb, Login, Cloud-Synchronisierung und PDF-Export gehören nicht zum aktuellen Umfang.

## Sprache

- Die Zusammenarbeit mit dem Auftraggeber erfolgt auf Deutsch.
- Roadmaps, Statusberichte, Entscheidungsdokumente und der Bereich `Service-Leitung` werden auf Deutsch geschrieben.
- Sichtbare Texte der App sind deutsch.
- Codebezeichner und technische Testnamen sind entsprechend `CLEANCODE.md` englisch.
- Fachbegriffe werden einheitlich nach dem Glossar in `CLEANCODE.md` verwendet.

## Arbeitsprinzip: lesen, eingrenzen, umsetzen, prüfen, berichten

Vor jeder Änderung:

1. aktuellen Auftrag und Freigabe bestimmen,
2. relevante Dateien gezielt lesen,
3. vorhandene Änderungen mit `git status` und `git diff` prüfen,
4. aktiven Roadmap-Schritt und dessen Grenzen feststellen,
5. nur den kleinsten freigegebenen Umfang bearbeiten.

Nach jeder Änderung:

1. passende automatische Prüfungen ausführen,
2. Status und Dokumentation wahrheitsgemäß aktualisieren,
3. Änderungen gegen den freigegebenen Umfang prüfen,
4. einen kurzen Abschlussbericht geben,
5. vor dem nächsten Roadmap-Schritt auf Abnahme warten.

## Roadmap-Pflicht

### Vor einem neuen System

Kein neues fachliches oder technisches System wird ohne eigene Teil-Roadmap implementiert.

Eine Teil-Roadmap enthält mindestens:

- Ziel und Nutzen,
- bestätigte fachliche Grundlagen,
- Umfang und Nicht-Umfang,
- offene Entscheidungen,
- Architekturgrenzen,
- kleine nummerierte Schritte,
- Prüfung je Schritt,
- Abnahmebedingung,
- echte externe oder manuelle Gates.

Die Roadmap wird zuerst als Entwurf erstellt und abgenommen. Erst danach beginnt die Implementierung ihres ersten Schritts.

### Roadmap-Ablage

- aktiv: `docs/roadmaps/active`
- pausiert: `docs/roadmaps/paused`
- abgeschlossen: `docs/roadmaps/completed`

Eine Roadmap wird nur nach ausdrücklicher Statusentscheidung verschoben. Verweise in `MASTER_ROADMAP.md`, `STATUS.md` und anderen Dokumenten werden im selben Schritt aktualisiert. Danach wird nach veralteten Pfaden gesucht.

### Statuszeichen

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

`[x]` bedeutet nicht nur „Code geschrieben“. Der vereinbarte Nachweis und die Abnahme müssen erfolgt sein.

## Abnahmeprozess

- Pro Turn wird grundsätzlich nur der aktuell freigegebene minimale Roadmap-Schritt abgeschlossen.
- Nach dem Bericht wird nicht selbstständig mit dem nächsten Schritt begonnen.
- Eine ausdrückliche Abnahme oder eine klare Aufforderung wie „es kann weitergehen“ gilt als Freigabe des zuvor berichteten Schritts.
- Neue Rückfragen oder Änderungswünsche werden dem laufenden Schritt zugeordnet, sofern sie ihn ergänzen.
- Ändert eine Rückmeldung die Architektur oder den Umfang, wird zuerst die Dokumentation angepasst.
- Mehrere Schritte dürfen nur zusammen umgesetzt werden, wenn der Auftraggeber dies ausdrücklich verlangt.
- Eine Abnahme darf niemals vorweggenommen oder aus Schweigen abgeleitet werden.

## Verhalten bei Prüf-, Ideen- und Verständnisfragen

Wenn der Auftraggeber nur prüfen, verstehen, vergleichen oder Ideen sammeln möchte:

- zuerst lesen und Belege nennen,
- keine Dateien verändern,
- Befund, Risiken und Optionen erklären,
- auf eine ausdrückliche Änderungsanweisung warten.

Formulierungen wie „ändere noch nichts“, „nur Ideen“, „überprüfe“ oder „lass uns abgleichen“ sind verbindliche Schreibstopps.

## Umgang mit Unklarheiten

- Kleine, reversible Detailannahmen dürfen getroffen und im Bericht genannt werden.
- Eine Annahme darf keine neue Funktion, externe Datenübertragung oder Architekturänderung erzeugen.
- Bei mehreren fachlich unterschiedlichen Ergebnissen wird gezielt nachgefragt.
- Konkrete Regeln für Mitarbeiter, Dienste, Einsatzorte und Bedarfe werden nicht erfunden.
- Gesetzliche oder betriebliche Regeln werden erst nach bestätigter Quelle und fachlicher Einordnung als zwingend behandelt.

## Architekturgrenzen

Die vollständigen Regeln stehen in `ARCHITECTURE.md`. Besonders verbindlich sind:

- `Domain` besitzt keine technischen Abhängigkeiten.
- `Application` verwendet nur `Domain`.
- Planning, Infrastructure und Excel implementieren technische Adapter hinter Anwendungsschnittstellen.
- OR-Tools-Typen bleiben in Planning.
- Entity-Framework- und SQLite-Typen bleiben in Infrastructure.
- ClosedXML- und Open-XML-Typen bleiben in Excel.
- WPF-Fenster und ViewModels verwenden keine konkreten technischen Adapter.
- Nur `Desktop.Composition` verdrahtet konkrete Implementierungen.
- Modulübergaben sind unveränderliche Momentaufnahmen.
- `DbContext`, `IQueryable`, UI-, Solver- und Excel-Bibliotheksobjekte überschreiten keine Modulgrenze.
- Architekturtests erzwingen diese Regeln, sobald das Projektgerüst existiert.

Eine Änderung dieser Grenzen benötigt vor ihrer Umsetzung einen dokumentierten und abgenommenen Architekturentscheid.

## Fachregeln

- Jede Regel besitzt genau eine unveränderliche fachliche Definition in Domain.
- Regelkennung, Parameter, Geltungsbereich und Priorität werden nicht in UI, Datenbankabfragen oder Solver-Code dupliziert.
- Planning übersetzt bestätigte Regelarten in Solver-Bedingungen.
- Eine unbekannte oder nicht übersetzte Regel blockiert die Generierung sichtbar und wird niemals ignoriert.
- Fachliche Prüfung und Solver-Übersetzung verwenden dieselben Beispielszenarien.
- Zwingende Regeln bleiben unverletzt; bei fehlender zulässiger Besetzung bleibt der betroffene Zeitraum vollständig oder teilweise sichtbar ungedeckt.
- Automatische Überbesetzung ist nicht erlaubt.
- Optimierung erfolgt hierarchisch: ungedeckter Bedarf, hoch, mittel, niedrig, anschließend Stabilität.

## Konflikterklärung

- Planning liefert strukturierte Ursachecodes, Lösungscodes und fachliche Parameter.
- Planning formuliert keine deutschen UI-Sätze.
- Application übersetzt strukturierte Konflikte in verständliche deutsche Meldungen.
- Desktop stellt Meldungen dar, erfindet aber keine Ursachen oder Lösungsvorschläge.
- Vorschläge verändern niemals automatisch Regeln, Abwesenheiten, Sperren oder Stammdaten.
- Konflikte werden sachlich beschrieben und geben keiner Person die Schuld.

## Datenschutz und reale Daten

Das Repository darf niemals enthalten:

- echte Mitarbeiternamen,
- echte Dienstpläne,
- Krankheits- oder Abwesenheitsgründe,
- echte Verfügbarkeiten oder Zeitkonten,
- produktive Datenbanken,
- Sicherungen oder Exporte,
- Passwörter, Tokens oder andere Zugangsdaten.

Tests, Screenshots und Beispiele verwenden ausschließlich klar erfundene synthetische Daten. Vor jedem Commit oder externen Upload wird die Dateiliste auf unbeabsichtigte Daten geprüft.

Die produktive App arbeitet vollständig lokal und sendet keine Mitarbeiter- oder Plandaten über das Netzwerk.

## Service-Leitungsdokumentation

Der Ordner `Service-Leitung` ist ein verbindlicher Teil der Projektdokumentation.

- `Service-Leitung/README.md` bleibt die einfache Startseite.
- `Service-Leitung/AKTUELLER_STAND.md` zeigt den tatsächlichen Projektstand.
- `Service-Leitung/GRUNDLAGEN_UND_ENTSCHEIDUNGEN.md` erklärt bestätigte Grundlagen in einfacher Sprache.
- Nach jedem abgenommenen Schritt wird geprüft, ob der verständliche Stand aktualisiert werden muss.
- Noch nicht gebaute Funktionen werden klar als geplant bezeichnet.
- Technische Details werden nur aufgenommen, wenn sie für eine fachliche Entscheidung wichtig sind, und dann einfach erklärt.
- Widersprüche zwischen technischer Dokumentation und Service-Leitungsbereich sind vor Abnahme zu beseitigen.

## Git-Regeln

- Der Auftraggeber erstellt Commits und Pushes selbst oder beauftragt sie ausdrücklich.
- Ohne ausdrücklichen Auftrag werden weder `git commit` noch `git push` ausgeführt.
- Änderungen werden nicht automatisch gestaged.
- `git status`, `git diff`, `git diff --check`, `git log` und andere unverändernde Prüfungen sind erlaubt.
- Kein `git reset --hard`, `git checkout --`, Force-Push oder Umschreiben der Historie ohne ausdrücklichen Auftrag.
- Benutzeränderungen im Arbeitsordner werden erhalten und nicht überschrieben.
- Unzusammenhängende vorhandene Änderungen werden nicht bereinigt oder in den eigenen Umfang aufgenommen.
- Vor einem beauftragten Commit werden Dateiliste, Diff, sensible Daten und Prüfstatus kontrolliert.
- Commit-Nachrichten beschreiben die tatsächliche Änderung und behaupten keine noch offene Abnahme.

## Dateiänderungen und Repository-Hygiene

- Zuerst mit `rg` beziehungsweise `rg --files` gezielt suchen.
- Dateien werden mit kleinen nachvollziehbaren Patches geändert.
- Bestehender Stil und Zeilenenden werden respektiert.
- Generierte Dateien werden nicht manuell editiert.
- Build-, Test-, Datenbank-, Sicherungs- und Exportartefakte bleiben außerhalb von Git.
- Keine destruktiven Dateioperationen ohne eindeutigen Auftrag und überprüftes Ziel.
- Neue Root-Dateien oder Root-Ordner benötigen einen dokumentierten Zweck.
- Keine parallelen alten und neuen Dateinamen nach einer Umbenennung.
- Nach Verschiebungen oder Umbenennungen wird nach veralteten Verweisen gesucht.

## Namespace- und Sichtbarkeitsregeln

Die Details stehen in `CLEANCODE.md`.

- Der Root-Namespace entspricht dem Projektnamen.
- Ordner und Namespaces stimmen grundsätzlich überein.
- Fachliche Projekte werden nach Fachbereichen gegliedert.
- Technische Projekte werden nach gekapselten Verantwortungen gegliedert.
- Keine globalen Sammelbereiche wie `Helpers`, `Managers`, `Common`, `Misc` oder `Data`.
- Typen bleiben standardmäßig `internal`.
- Nur notwendige Modulverträge werden `public`.
- `Desktop.Composition` ist der einzige UI-Namespace mit konkreten Adapterreferenzen.

## Abhängigkeiten und Werkzeuge

- Neue Pakete, SDKs oder Tools werden nur innerhalb eines freigegebenen Roadmap-Schritts hinzugefügt.
- Vor Aufnahme werden Zweck, Lizenz, Wartungsstatus, .NET-10-Kompatibilität und Windows-11-x64-Auslieferung geprüft.
- Es werden nur stabile Versionen verwendet.
- Paketversionen werden zentral verwaltet.
- Ein Paket darf keine Architekturgrenze umgehen.
- Downloads und Installationen außerhalb des Repositorys benötigen vorherige Zustimmung, wenn sie den Rechner verändern.
- Externe technische Aussagen mit möglicher Versionsänderung werden anhand offizieller Primärquellen geprüft.

## Test- und Validierungsgates

Prüfungen werden passend zum geänderten Umfang gewählt und getrennt berichtet:

1. statische Format- und Diff-Prüfung,
2. Kompilierung,
3. Domain- und Application-Tests,
4. Planning- und Konflikttests,
5. Architekturtests,
6. Datenbank- und Migrationstests,
7. Excel-Struktur- und Vorlagentests,
8. WPF-Bedienprüfung,
9. portable Veröffentlichung,
10. Start auf einem sauberen Windows-11-System,
11. fachliche Abnahme durch die Service-Leitung.

Nur betroffene Gates werden ausgeführt. Ein nicht betroffener oder noch nicht möglicher Nachweis bleibt ausdrücklich offen. Kein Gate ersetzt ein anderes.

Insbesondere gilt:

- Ein erfolgreicher Build ist kein bestandener UI-Test.
- Automatische Tests sind keine Abnahme der Excel-Darstellung.
- Ein lokaler Start ist kein Nachweis der portablen Ausgabe.
- Ein Testrechner ist keine fachliche Abnahme durch die Service-Leitung.
- Ein fehlendes Tool oder eine fehlende Vorlage wird als offenes Gate und nicht als Erfolg dokumentiert.

## Datenbankänderungen

- Datenmodelländerungen benötigen Migration und Migrationstest.
- Veröffentlichte Migrationen werden nicht nachträglich umgeschrieben.
- Vor produktiven Migrationen wird eine Sicherung erstellt.
- Fehlgeschlagene Migrationen dürfen die letzte funktionsfähige Datenbank nicht zerstören.
- Tests verwenden temporäre synthetische Datenbanken.
- Datenbankobjekte überschreiten Infrastructure nicht.

## Excel-Arbeit

- Die bereitgestellte Vorlage bleibt unverändert erhalten.
- Exporte arbeiten ausschließlich mit einer Kopie.
- ClosedXML wird erst nach einem Charakterisierungstest mit der echten Vorlage endgültig bestätigt.
- Der Test umfasst Werte, Format, Druckbereich, Seitenaufteilung, verbundene Zellen, Formeln und benannte Bereiche.
- Drei Wochen auf einer Seite sind ein manuell abzunehmendes Gate.
- Eine teilweise oder technisch ungültige Datei wird nicht als erfolgreicher Export gemeldet.
- Excel-Ausgaben werden nicht committed.

## Fehler und Blockaden

- Erwartbare fachliche Konflikte sind reguläre Ergebnisse und keine technischen Abstürze.
- Wiederholte oder unklare Fehler werden bis zur Ursache untersucht.
- Workarounds werden nicht als endgültige Lösung versteckt.
- Ein echter Blocker nennt Ursache, bereits geprüfte Alternativen und die konkret benötigte Entscheidung.
- Ohne benötigte Freigabe für eine externe oder destruktive Aktion wird gestoppt.
- Fehlende manuelle Abnahme ist ein offenes Gate, kein technischer Fehler.

## Delegation

- Aufgaben werden nicht ohne ausdrücklichen Wunsch des Auftraggebers an weitere Agenten delegiert.
- Bei ausdrücklich gewünschter Delegation bleiben Roadmap, Datenschutz, Dateigrenzen und Abnahmeprozess für alle Beteiligten verbindlich.
- Ein Hauptbearbeiter prüft zusammengeführte Ergebnisse vor dem Bericht.

## Abschlussbericht je Schritt

Der Bericht enthält kurz und konkret:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. offene Gates, Risiken oder Blockaden,
5. Git-Status; Commit und Push nur wenn ausdrücklich beauftragt,
6. nächster minimaler Roadmap-Schritt,
7. Bitte um Abnahme.

Der Bericht behauptet niemals, dass ein offenes Gate bestanden wurde.

## Definition eines abgeschlossenen Schritts

Ein Implementierungsschritt ist erst zur Abnahme bereit, wenn:

- sein freigegebener Umfang vollständig umgesetzt ist,
- keine unbestätigte Erweiterung enthalten ist,
- Architektur- und Clean-Code-Regeln eingehalten sind,
- passende Tests bestehen,
- relevante manuelle Gates wahrheitsgemäß ausgewiesen sind,
- Dokumentation, Roadmap, Status und Service-Leitungsstand übereinstimmen,
- keine echten oder sensiblen Daten ins Repository gelangt sind,
- der nächste minimale Schritt benannt ist.

Abgeschlossen und `[x]` wird der Schritt erst nach der Abnahme.
