# Teil-Roadmap: Mitarbeitende, Arbeitszeitmodelle und Qualifikationen

Status: Aktiver, noch nicht abgenommener Entwurf – MA-01 nach Abschluss von System 04 wieder aufgenommen

Stand: 2026-09-13

## Ziel und Nutzen

Diese Roadmap beschreibt System 03 der `MASTER_ROADMAP.md`. Am Ende sollen Mitarbeitende mit ihrem Namen, ihren Vertragsstunden beziehungsweise ihrem Arbeitszeitmodell, ihren Qualifikationen, ihren zulässigen Einsatzorten und ihren Freigaben für einzelne Diensttypen getrennt erfasst, bearbeitet und lokal gespeichert werden können.

Die getrennte Modellierung verhindert starre Mitarbeitertypen. Wochenstunden, Qualifikationen und Einsatzfreigaben bleiben unabhängig kombinierbar und können später von Verfügbarkeiten, Regeln und der automatischen Planung verwendet werden.

## Verbindliche Grundlagen

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, abgenommen am 2026-09-13
- `ARCHITECTURE.md`, abgenommen am 2026-09-13
- `CLEANCODE.md`, abgenommen am 2026-09-13
- `AGENTS.md` mit dem verbindlichen kleinschrittigen Abnahmeprozess
- `MASTER_ROADMAP.md`, System 03 – Mitarbeitende, Arbeitszeitmodelle und Qualifikationen
- System 02 – Technisches App-Grundgerüst, abgeschlossen und archiviert am 2026-09-13
- ausschließlich synthetische Mitarbeiterdaten in Quellcode, Tests, Dokumentation und Screenshots

## Bestätigte fachliche Grundlagen

- Mitarbeitende werden mit ihrem Namen erfasst.
- Arbeitszeitmodell, Qualifikationen und zulässige Einsatzorte werden fachlich getrennt modelliert.
- Freigaben für einzelne Diensttypen werden ebenfalls getrennt modelliert. Eine Freigabe für Cafeteria-Dienst A schließt Cafeteria-Dienst B nicht automatisch ein.
- Ein Mitarbeiter ist kein fest programmierter „Mitarbeitertyp“.
- Wochenstunden beziehungsweise Arbeitszeitmodelle dürfen unabhängig mit Qualifikationen und Einsatzfreigaben kombiniert werden.
- Individuelle Verfügbarkeiten und Abwesenheiten sind ein getrenntes späteres System.
- Arbeitsdauern werden in ganzen Minuten gespeichert und berechnet; `double` und `float` werden dafür nicht verwendet.
- Die Anwendung arbeitet vollständig lokal und offline.
- Sichtbare Texte der App sind deutsch; Codebezeichner verwenden die festgelegten englischen Begriffe wie `Employee`, `WorkModel` und `WorkLocation`.
- Ungültige Eingaben werden als verständliche fachliche Validierungsergebnisse und nicht als technische Abstürze behandelt.

## Technischer Ausgangspunkt

- Das technische Grundgerüst mit sechs Produktionsprojekten und sieben Testprojekten ist vorhanden.
- `Domain` und `Application` enthalten die abgenommenen Einsatzorte, Diensttypen, Einsatzmuster sowie Katalog-, Bearbeitungs- und Speicherverträge aus System 04. Mitarbeiterfunktionen existieren weiterhin nicht.
- `Infrastructure` enthält die abgenommene SQLite-Speicherung und erste Migration für System 04; Mitarbeiterdaten sind darin weiterhin nicht enthalten.
- `Desktop` enthält die abgenommene Einsatzort- und Diensttypverwaltung sowie die feste Anzeige von Doppeldienst und Springer; eine Mitarbeiteransicht existiert weiterhin nicht.
- Die dreizehn Architekturtests bestehen nach der Ergänzung der System-04-Module.
- System 04 ist mit ED-01 bis ED-10 abgenommen und unter `docs/roadmaps/completed/WORK_LOCATIONS_SHIFT_TYPES_SPLIT_SHIFTS_ROADMAP.md` archiviert.

## Umfang dieser Roadmap

- fachliche Identität und Namensdarstellung von Mitarbeitenden
- fachliches Modell für Vertragsstunden beziehungsweise Arbeitszeitmodelle
- fachliches Modell für Qualifikationen und deren Zuordnung zu Mitarbeitenden
- Zuordnung zulässiger Einsatzorte aus System 04
- Zuordnung einzelner zulässiger Diensttypen aus System 04
- notwendige Anwendungsfälle und unveränderliche Ein- und Ausgabeverträge
- lokale SQLite-Speicherung mit Migration und echten temporären SQLite-Tests
- deutsche WPF-Bedienung zum Anzeigen, Anlegen und Bearbeiten der bestätigten Mitarbeiterdaten
- fachliche Validierungen und verständliche Fehlermeldungen
- passende Domain-, Application-, Infrastructure-, Desktop- und Architekturprüfungen
- wahrheitsgemäße Aktualisierung von Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung`

## Nicht Bestandteil dieser Roadmap

- echte Namen, Personaldaten oder reale Mitarbeiterlisten
- Verfügbarkeiten, Urlaub, Krankheit, Fortbildung, Wunschfrei oder andere Abwesenheiten
- Einsatzorte, Diensttypen, Standardzeiten sowie Doppeldienst- und Springer-Muster
- konkrete Planungsregeln oder arbeitsrechtliche Grenzwerte
- automatische Planerzeugung, Konfliktdiagnose oder OR-Tools-Modellierung
- Zeitkonten, Überstunden oder Minusstunden
- Planansichten, Planversionen, Excel-Export oder Sicherung und Wiederherstellung
- Benutzerkonten, Rollen, Cloud-Synchronisierung oder Netzwerkzugriffe
- Import aus Excel oder anderen Personalsystemen
- Commit, Push oder externe Veröffentlichung

## Architekturgrenzen

### Domain

- `Salztal.Dienstplanung.Domain.Employees` besitzt die fachlichen Typen und Invarianten.
- Technische Speicher-, WPF- und EF-Core-Typen bleiben vollständig außerhalb von Domain.
- Fachlich wichtige Kennungen und Arbeitsdauern werden stark typisiert.
- Fachobjekte sind nach erfolgreicher Erzeugung gültig; änderbare Abläufe liefern explizite Ergebnisse.

### Application

- `Salztal.Dienstplanung.Application.Employees` koordiniert Commands und Queries für die bestätigten Bedienfälle.
- Speicherverträge werden nach den benötigten Anwendungsfällen benannt; eine generische CRUD-Schnittstelle wird nicht eingeführt.
- Ein- und Ausgaben sind unveränderliche Momentaufnahmen und enthalten keine EF-Core- oder WPF-Typen.
- Application verwendet ausschließlich Domain.

### Infrastructure

- EF-Core-, SQLite-, `DbContext`- und Migrationscode bleibt in `Salztal.Dienstplanung.Infrastructure.Persistence`.
- `IQueryable` und veränderliche Persistenzobjekte verlassen Infrastructure nicht.
- Datenänderungen, die gemeinsam gelten müssen, werden transaktional gespeichert.
- Tests verwenden ausschließlich temporäre synthetische SQLite-Datenbanken.

### Desktop

- Mitarbeiteransichten und ViewModels liegen unter `Salztal.Dienstplanung.Desktop.Features.Employees`.
- ViewModels verwenden nur Application-Verträge und UI-eigene Typen.
- Nur `Salztal.Dienstplanung.Desktop.Composition` erzeugt und verdrahtet die konkrete SQLite-Implementierung.
- Fachlogik und Datenbankzugriff werden nicht in XAML, Code-behind oder ViewModels dupliziert.

### Unberührte Module

- `Planning` und `Excel` werden in diesem System nicht fachlich erweitert.
- Neue Pakete sind nach aktuellem Stand nicht vorgesehen.
- Die bestehenden Architekturgrenzen bleiben unverändert.

## Offene Entscheidungen vor der Implementierung

Die folgenden Punkte sind noch nicht bestätigt. Sie werden in MA-02 geklärt und danach in den Grundlagen beziehungsweise in dieser Roadmap verbindlich festgehalten.

### 1. Name und interne Identität

- Reicht ein gemeinsames sichtbares Namensfeld oder sollen Vor- und Nachname getrennt erfasst werden?
- Wird zusätzlich eine betriebliche Personalnummer benötigt oder genügt eine unsichtbare, von der App erzeugte Kennung?
- Müssen Anzeigenamen eindeutig sein, oder dürfen zwei Mitarbeitende denselben Namen tragen?

Empfohlener Ausgangspunkt: getrennte Felder für Vor- und Nachname, eine unsichtbare stabile `EmployeeId`, keine Personalnummer ohne betrieblichen Bedarf und keine erzwungene Eindeutigkeit menschlicher Namen.

### 2. Lebenszyklus von Mitarbeitenden

- Sollen Mitarbeitende gelöscht werden können oder nur auf „nicht aktiv“ gesetzt werden?
- Falls Löschen erlaubt ist: nur solange noch keine abhängigen Daten existieren?

Empfohlener Ausgangspunkt: Mitarbeitende werden deaktiviert statt historienzerstörend gelöscht; ein endgültiges Löschen ist höchstens vor der ersten späteren Verwendung erlaubt.

### 3. Arbeitszeitmodelle und Vertragsstunden

- Sind Arbeitszeitmodelle wiederverwendbare benannte Vorlagen, beispielsweise „Vollzeit 39 Stunden“, oder wird nur eine individuelle Wochenstundenzahl je Mitarbeiter benötigt?
- Welche konkreten Modelle beziehungsweise Wochenstunden kommen im Betrieb vor?
- Dürfen beliebige Minutenwerte eingetragen werden oder nur festgelegte Schritte?
- Gehört eine Verteilung der Wochenstunden auf einzelne Wochentage bereits dazu?

Empfohlener Ausgangspunkt: wiederverwendbarer Name plus Wochen-Soll in ganzen Minuten; keine Tagesverteilung in System 03, solange dafür kein bestätigter fachlicher Bedarf besteht.

### 4. Qualifikationen

- Welche Qualifikationen werden tatsächlich benötigt?
- Darf die Service-Leitung Qualifikationen selbst anlegen und umbenennen?
- Genügt „vorhanden/nicht vorhanden“ oder werden Stufen, Ablaufdaten oder Nachweise benötigt?

Empfohlener Ausgangspunkt: frei pflegbarer Qualifikationskatalog mit Name und Mehrfachzuordnung; keine Stufen, Ablaufdaten oder Dokumente ohne bestätigten Bedarf.

### 5. Zulässige Einsatzorte, Diensttypen und Grenze zu System 04

- System 04 wird vor System 03 umgesetzt und besitzt den Einsatzort- sowie Diensttyp-Katalog.
- System 03 ordnet Mitarbeitenden vorhandene Einsatzorte und einzelne vorhandene Diensttypen über stabile Kennungen zu.
- Eine Person kann für Cafeteria-Dienst A freigegeben sein, ohne automatisch Cafeteria-Dienst B übernehmen zu dürfen.
- Der Springer benötigt Freigaben für Cafeteria, Restaurant, Cafeteria-Dienst B und Spätdienst, aber keine zusätzliche Qualifikation.
- Die tatsächlich vorhandenen Domain-Verträge `WorkLocationId` und `ShiftTypeId` stellen diese stabilen Kennungen bereit. Namen, Farben, Anzeigen und Standardzeiten bleiben ausschließlich im Katalog von System 04.
- Die vorhandene Application-Katalogabfrage liefert Einsatzorte und normale Diensttypen als unveränderliche Momentaufnahme für Auswahl und Anzeige.
- `D` und `Spr` erhalten keine eigenen Mitarbeiterfreigaben. Der Doppeldienst setzt die Freigaben für Restaurant, Frühdienst und Spätdienst voraus; für den Springer gelten die bereits bestätigten vier Freigaben.
- Ob eine referenzierte Kennung aktuell im Katalog existiert, wird später im Application-Ablauf gegen System 04 geprüft und durch Fremdschlüssel in Infrastructure abgesichert. Domain prüft die Form der Kennung und die Mitarbeiterzuordnung, greift aber nicht auf die Datenbank zu.

Diese Systemgrenze ist am 2026-09-13 bestätigt und in ED-10 gegen die tatsächlichen Verträge aus System 04 geprüft worden. Vor der Mitarbeiter-Migration ist noch festzulegen, wie ihre Tabellen in die bestehende gemeinsame SQLite-Migrationsfolge aufgenommen werden; eine unkoordinierte zweite Migrationsfolge darf die vorhandenen Katalogtabellen nicht verändern.

### 6. Minimale Bedienung

- Welche Felder müssen in der Mitarbeiterübersicht sofort sichtbar sein?
- Reichen für die erste Fassung Übersicht, Anlegen und Bearbeiten oder werden Suche, Filter und Sortierung bereits benötigt?

Empfohlener Ausgangspunkt: eine übersichtliche Liste sowie getrennte Vorgänge für Anlegen und Bearbeiten; keine Suche oder Spezialfilter ohne nachgewiesenen Bedarf.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Schritte

### MA-01 – Teil-Roadmap entwerfen

Status: `[~]` – nach Abschluss von System 04 wieder aufgenommen, aktualisiert und zur Roadmap-Abnahme vorgelegt

Geplantes Ergebnis:

- Ziel, Umfang, Nicht-Umfang, offene Entscheidungen, Architekturgrenzen, Schritte und echte Gates von System 03 sind festgehalten.
- `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` verweisen auf den aktiven Entwurf.
- Es werden noch keine Fachtypen, Datenbankobjekte oder Bedienoberflächen angelegt.

Prüfung:

- Der Entwurf stimmt mit den abgenommenen Grundlagen, der Architektur, den Clean-Code-Regeln und System 03 der Master-Roadmap überein.
- Offene fachliche Punkte werden als Fragen geführt und nicht als fertige Regeln dargestellt.
- Jeder spätere Schritt besitzt ein kleines, getrennt prüfbares Ergebnis.
- Lokale Markdown-Verweise, Statusangaben und `git diff --check` sind fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt den Roadmap-Entwurf oder nennt Änderungswünsche.

### MA-02 – Fachmodell und Systemgrenzen bestätigen

Status: `[ ]`

Geplantes Ergebnis:

- Die offenen Fragen zu Namen, Lebenszyklus, Arbeitszeitmodellen, Qualifikationen, Einsatzfreigaben und minimaler Bedienung sind beantwortet.
- Die Grenze zwischen System 03 und System 04 ist widerspruchsfrei festgelegt.
- Bestätigte Entscheidungen sind in allen betroffenen Grundlagen- und Statusdokumenten nachgeführt.
- Die späteren Schritte dieser Roadmap sind anhand der Entscheidungen präzisiert, ohne Fachcode anzulegen.

Prüfung:

- Keine konkrete Mitarbeiter-, Arbeitszeit-, Qualifikations- oder Einsatzregel ist erfunden.
- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, Roadmap, Master-Roadmap und Service-Leitungsdokumentation widersprechen einander nicht.
- Veraltete Formulierungen und Pfade werden gesucht; `git diff --check` bleibt fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt das Fachmodell und die Systemgrenzen ausdrücklich.

### MA-03 – Mitarbeiteridentität und Namen fachlich modellieren

Status: `[ ]`

Geplantes Ergebnis:

- Stabile Mitarbeiterkennung und bestätigte Namensdarstellung sind als gültige Domain-Typen umgesetzt.
- Die bestätigten Invarianten und Validierungsfehler liegen ausschließlich in Domain.
- Es existiert noch keine Speicherung oder WPF-Bedienung.

Prüfung:

- Domain-Tests decken gültige Werte, leere beziehungsweise ungültige Eingaben, Grenzwerte und Gleichheit der Kennung ab.
- Domain bleibt frei von technischen Abhängigkeiten.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Fachliche Bedeutung, Fehlermeldungen und Testfälle werden bestätigt.

### MA-04 – Arbeitszeitmodelle fachlich modellieren

Status: `[ ]`

Geplantes Ergebnis:

- Das bestätigte Arbeitszeitmodell und die Vertragszeit in ganzen Minuten sind als getrennt kombinierbare Domain-Typen umgesetzt.
- Ungültige Werte können kein gültiges Fachobjekt erzeugen.
- Es werden keine gesetzlichen oder betrieblichen Grenzwerte erfunden.

Prüfung:

- Domain-Tests decken gültige Modelle, bestätigte Grenzwerte, ungültige Werte und unveränderte Minutengenauigkeit ab.
- Ein Mitarbeiter kann das Arbeitszeitmodell unabhängig von Qualifikation und Einsatzfreigabe besitzen.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Das Arbeitszeitmodell und seine fachlichen Grenzen werden bestätigt.

### MA-05 – Qualifikationen fachlich modellieren

Status: `[ ]`

Geplantes Ergebnis:

- Der bestätigte Qualifikationskatalog und die Zuordnung zu Mitarbeitenden sind in Domain umgesetzt.
- Doppelte oder ungültige Zuordnungen werden entsprechend der bestätigten Regeln behandelt.
- Qualifikationen bleiben unabhängig von Arbeitszeitmodell und Einsatzfreigaben.

Prüfung:

- Domain-Tests decken Anlegen, Ändern und Zuordnen gemäß dem bestätigten Umfang ab.
- Tests belegen, dass keine starre Mitarbeitertyp-Kombination entsteht.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Qualifikationsmodell und Zuordnungsregeln werden bestätigt.

### MA-06 – Einsatzort- und Diensttypfreigaben fachlich integrieren

Status: `[ ]`

Voraussetzung:

- Die in MA-02 festgelegte Grenze zu System 04 ist erfüllt; `WorkLocationId`, `ShiftTypeId` und die Katalogabfrage sind nach der Abschlussabnahme von System 04 abgenommen vorhanden.

Geplantes Ergebnis:

- Zulässige Einsatzorte und einzelne Diensttypen können einem Mitarbeiter über stabile Kennungen zugeordnet und wieder entfernt werden.
- Die Mitarbeiterdomäne übernimmt keine Verwaltung oder Sonderlogik konkreter Einsatzorte.
- Die Mitarbeiterdomäne übernimmt keine Bezeichnungen, Farben oder Standardzeiten aus System 04 als zweite Quelle.
- Doppeldienst und Springer werden aus den Freigaben ihrer normalen Diensttypen und Einsatzorte beurteilt; es entstehen keine parallelen Freigabekennungen für `D` oder `Spr`.

Prüfung:

- Domain-Tests decken Zuordnung, Entfernung, Doppelzuordnung und formal ungültige Kennungen für Einsatzorte und Diensttypen gemäß der bestätigten Grenze ab.
- Application-Tests lehnen nicht im aktuellen Katalog vorhandene Kennungen ab; Infrastructure-Tests sichern die referenzielle Integrität in der gemeinsamen lokalen Datenbank.
- Cafeteria, Restaurant oder andere Einsatzorte sind nicht im Mitarbeitercode hart codiert.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Die fachliche Zuordnung und die tatsächliche Trennung zu System 04 werden bestätigt.

### MA-07 – Anwendungsfälle und Speicherverträge umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Die bestätigten Commands und Queries zum Anzeigen, Anlegen und Bearbeiten der Mitarbeiterdaten sind in Application umgesetzt.
- Kleine anwendungsfallbezogene Speicherverträge ersetzen eine generische CRUD-Schnittstelle.
- Ein- und Ausgaben sind unveränderliche Momentaufnahmen mit verständlichen Validierungsergebnissen.

Prüfung:

- Application-Tests prüfen Erfolgs-, Validierungs-, Nicht-gefunden- und Abbruchfälle mit synthetischen Daten.
- Application verwendet nur Domain und enthält keine WPF-, EF-Core- oder SQLite-Typen.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Anwendungsabläufe, Ergebnisse und Fehlerfälle werden bestätigt.

### MA-08 – Lokale SQLite-Speicherung und Migration umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Infrastructure implementiert ausschließlich die in MA-07 bestätigten Speicherverträge.
- Eine erste veröffentlichbare Migration bildet die bestätigten Mitarbeiterdaten und Beziehungen ab.
- Mapping und Speicherung bewahren die fachlichen Werte ohne Rundungs- oder Zuordnungsverlust.
- Die konkrete Anwendung verdrahtet die Implementierung ausschließlich in `Desktop.Composition`.

Prüfung:

- Infrastructure-Tests verwenden echte temporäre SQLite-Dateien.
- Migration von leerer Datenbank, Speichern, Laden, Ändern und Neustart-Round-trip bestehen.
- Fehlgeschlagene Schreibvorgänge hinterlassen keinen teilweise gültigen Zustand.
- Vollständiger Build, Infrastructure-Tests und Architekturtests bestehen.

Abnahmebedingung:

- Datenmodell, Migration und lokale Round-trips werden technisch bestätigt.

### MA-09 – Mitarbeiterübersicht in WPF umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Eine deutsche Mitarbeiterübersicht zeigt die in MA-02 bestätigten wichtigsten Angaben.
- Laden, leerer Zustand, Arbeitszustand und technische Fehler werden verständlich dargestellt.
- ViewModel und View greifen ausschließlich über Application auf Daten zu.

Prüfung:

- Desktop-Tests prüfen Zustände und Benutzerbefehle des ViewModels.
- Architekturtests sichern die `Desktop.Composition`-Grenze.
- Die WPF-App wird manuell gestartet; Übersicht, leerer Zustand, Lesbarkeit und deutsche Texte werden sichtbar geprüft.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbare Übersicht und ihre Bedienbarkeit.

### MA-10 – Anlegen und Bearbeiten in WPF umsetzen

Status: `[ ]`

Geplantes Ergebnis:

- Mitarbeitende können im bestätigten Umfang angelegt und bearbeitet werden.
- Arbeitszeitmodell, Qualifikationen und Einsatzfreigaben bleiben in der Bedienung erkennbar getrennt.
- Validierungsfehler werden deutsch, feldbezogen und ohne Datenverlust angezeigt.
- Löschen oder Deaktivieren wird ausschließlich gemäß MA-02 umgesetzt.

Prüfung:

- Desktop- und Application-Tests prüfen erfolgreiche sowie ungültige Eingaben und Abbruch ohne Speicherung.
- Ein manueller WPF-Ablauf mit klar synthetischen Daten prüft Anlegen, Bearbeiten, Neustart und erneutes Laden.
- Es findet kein Netzwerkzugriff statt; keine Testdaten oder lokale Datenbank werden versioniert.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbaren Eingabe- und Bearbeitungsabläufe.

### MA-11 – System 03 gemeinsam abschließen

Status: `[ ]`

Geplantes Ergebnis:

- Domain, Application, Infrastructure und Desktop bilden den bestätigten Umfang von System 03 widerspruchsfrei ab.
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

## Echte externe und manuelle Gates

- Die Roadmap und jede fachliche Entscheidung benötigen ausdrückliche Abnahme; Schweigen ist keine Zustimmung.
- Die sichtbaren WPF-Abläufe müssen zusätzlich zu automatischen Tests manuell geprüft werden.
- Alle manuellen Beispiele verwenden ausschließlich klar erfundene Daten.
- Die technischen Verträge für Einsatzfreigaben sind vorhanden; ihre Nutzung bleibt bis zur Abschlussabnahme von System 04 und der eigenen Freigabe von MA-02 gesperrt.
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

MA-01 mit dem aktualisierten Roadmap-Entwurf ausdrücklich abnehmen oder Änderungswünsche nennen. Danach werden in MA-02 die weiterhin offenen Mitarbeiterfragen organisiert beantwortet; Fachcode, Datenbank und WPF-Bedienung für System 03 bleiben bis zu diesen Abnahmen unverändert.
