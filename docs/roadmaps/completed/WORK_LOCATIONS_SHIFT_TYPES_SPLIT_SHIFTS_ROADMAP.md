# Teil-Roadmap: Einsatzorte, Diensttypen und Doppeldienste

Status: Abgeschlossen und archiviert am 2026-09-13

Stand: 2026-09-13

## Ziel und Nutzen

Diese Roadmap beschreibt System 04 der `MASTER_ROADMAP.md`. Am Ende sollen erweiterbare Einsatzorte, Diensttypen mit bearbeitbaren Standardzeiten, der feste Restaurant-Doppeldienst und der samstägliche Springer-Einsatz `Spr` fachlich gültig angezeigt, bearbeitet und lokal gespeichert werden können.

System 04 schafft damit die eindeutige Quelle für Einsatzorte und Diensttypen. Das anschließend fortgesetzte System 03 kann Mitarbeitenden diese vorhandenen Einsatzorte und gegebenenfalls Diensttypen über stabile Kennungen zuordnen, ohne Namen oder Zeiten zu duplizieren.

## Verbindliche Grundlagen

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, abgenommen am 2026-09-13
- `ARCHITECTURE.md`, abgenommen am 2026-09-13
- `CLEANCODE.md`, abgenommen am 2026-09-13
- `AGENTS.md` mit dem verbindlichen kleinschrittigen Abnahmeprozess
- `MASTER_ROADMAP.md`, System 04 – Einsatzorte, Diensttypen und Doppeldienste
- System 02 – Technisches App-Grundgerüst, abgeschlossen und archiviert am 2026-09-13
- ausschließlich synthetische Daten in Quellcode, Tests, Dokumentation und Screenshots

## Bestätigte fachliche Grundlagen

- Die zunächst bekannten Einsatzorte sind Cafeteria und Restaurant.
- Weitere Einsatzorte müssen später ergänzt werden können.
- Einsatzorte besitzen keine eigenen Öffnungs- oder Betriebszeiten. Zeitangaben gehören zu Diensttyp-Standardzeiten oder zu den tatsächlichen Bedarfen aus System 05.
- Es gibt Diensttypen mit bearbeitbaren Standardzeiten.
- Frühdienst, Spätdienst, Cafeteria-Dienst A und Cafeteria-Dienst B sind die vier bestätigten normalen Diensttypen. `D` und `Spr` sind zusammengesetzte Einsatzmuster und werden nicht von einem einzelnen Bedarf verlangt.
- Jeder Bedarf verlangt genau einen Diensttyp und besitzt die tatsächliche Zeit. Eine ausdrücklich erfasste Bedarfszeit darf von der Standardzeit abweichen.
- Die automatische Planung darf später ausschließlich vorher definierte Diensttypen und ausdrücklich erfasste Zeitabweichungen verwenden.
- Ein Doppeldienst besteht aus zwei getrennten, bereits definierten Diensten am selben Tag.
- Die Unterbrechung zwischen den beiden Teilen eines Doppeldienstes zählt nicht als Arbeitszeit.
- Beide Teile eines Doppeldienstes finden am selben Einsatzort statt.
- Doppeldienste sind nach aktuellem Stand ausschließlich im Restaurant zulässig.
- Der bestätigte Doppeldienst ist ausschließlich Frühdienst plus Spätdienst und wird als `D` angezeigt.
- Der samstägliche Springer `Spr` kombiniert Cafeteria-Dienst B mit einem anschließenden Restaurant-Spätdienst und ist kein Doppeldienst.
- Vor dem tatsächlichen Wechsel des Springers bleibt eine Restaurant-Teilunterdeckung sichtbar.
- Cafeteria wird zusätzlich zum Text gelb, Restaurant rot und `Spr` blau gekennzeichnet.
- Kalendertage werden ohne Uhrzeit gespeichert; Dienstzeiten werden als lokale Uhrzeiten und Arbeitsdauern als ganze Minuten abgebildet.
- Falls Dienste über Mitternacht benötigt werden, muss diese Regel vor ihrer Umsetzung ausdrücklich ergänzt werden.
- Einsatzorte, Diensttypen und ihre Kennungen dürfen nicht in Mitarbeiter-, UI- oder späterem Solver-Code als zweite fachliche Quelle dupliziert werden.

## Technischer Ausgangspunkt

- Das technische Grundgerüst mit sechs Produktionsprojekten und sieben Testprojekten ist vorhanden.
- `Domain` enthält die abgenommenen Einsatzorte, normalen Diensttypen sowie Doppeldienst- und Springer-Muster. `Application` enthält die abgenommenen Katalog-, Bearbeitungs- und Speicherverträge aus ED-06.
- `Infrastructure` enthält die abgenommene SQLite-Speicherung und erste Migration aus ED-07.
- `Desktop` enthält die abgenommene Einsatzort- und Diensttypverwaltung sowie die feste Anzeige von Doppeldienst und Springer.
- Die dreizehn Architekturtests bestehen nach der Ergänzung der System-04-Module.
- Der Entwurf für System 03 wurde vor seiner Abnahme pausiert, damit Einsatzorte und Diensttypen zuerst eine stabile fachliche Grundlage erhalten.

## Umfang dieser Roadmap

- fachliche Identität und Bezeichnung erweiterbarer Einsatzorte
- fachliche Identität, Bezeichnung, Einsatzort und bearbeitbare Standardzeit der Diensttypen
- bestätigte Zuordnung von Diensttypen zu Einsatzorten
- fachliches Modell für den festen Doppeldienst aus Frühdienst und Spätdienst
- fachliche Form des samstäglichen Springer-Einsatzes aus Cafeteria-Dienst B und Spätdienst
- notwendige Anwendungsfälle und unveränderliche Ein- und Ausgabeverträge
- lokale SQLite-Speicherung mit Migration und echten temporären SQLite-Tests
- deutsche WPF-Bedienung zum Anzeigen und Bearbeiten der bestätigten Startwerte
- erweiterbares Datenmodell ohne hart codierte Aufzählungen, damit sichtbares Anlegen und Löschen später ergänzt werden kann
- fachliche Validierungen und verständliche Fehlermeldungen
- stabile Kennungen, die spätere Systeme ohne technische Abhängigkeit verwenden können
- passende Domain-, Application-, Infrastructure-, Desktop- und Architekturprüfungen
- wahrheitsgemäße Aktualisierung von Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung`

## Nicht Bestandteil dieser Roadmap

- echte Mitarbeiter-, Dienstplan- oder Betriebsdaten
- Zuordnung von Einsatzorten, Qualifikationen oder Diensttypen zu Mitarbeitenden
- Mitarbeiterverwaltung, Arbeitszeitmodelle, Verfügbarkeiten oder Abwesenheiten
- konkreter Personalbedarf nach Wochentag und Dienst
- Umsetzung von Standardbedarfen, tatsächlichen Bedarfszeiten, Personenzahlen und Datums-Ausnahmen; diese gehört zu System 05
- gesetzliche oder betriebliche Arbeitszeit- und Ruhezeitregeln
- automatische Planerzeugung, Optimierung oder OR-Tools-Modellierung
- automatische Notfallentscheidung für `Spr` und Priorisierung seiner Teilunterdeckung; diese gehören zu den späteren Regel- und Planungssystemen
- konkrete Planungsregeln für die Zulässigkeit einer Person in einem Dienst
- Planansichten, Planversionen, Excel-Export oder Zeitkonten
- Sicherung und Wiederherstellung
- Dienste über Mitternacht ohne vorherige ausdrückliche Fachentscheidung
- Benutzerkonten, Cloud-Synchronisierung oder Netzwerkzugriffe
- Commit, Push oder externe Veröffentlichung

## Architekturgrenzen

### Domain

- Einsatzorte und Diensttypen werden in klaren fachlichen Bereichen des Domain-Projekts modelliert.
- `WorkLocationId` und `ShiftTypeId` sind stabile stark typisierte Kennungen.
- Zeitwerte verwenden `TimeOnly`; Standard- und tatsächliche Zeiten bleiben fachlich unterscheidbar und Arbeitsdauern werden als ganze Minuten dargestellt.
- Doppeldienstbedingungen werden an genau einer fachlichen Stelle geprüft.
- Der standortübergreifende Springer ist ein eigenes Muster aus zwei referenzierten Diensttypen. Die konkreten tatsächlichen Abschnitte entstehen erst aus Bedarfen und Planungszuweisungen späterer Systeme.
- Domain besitzt keine WPF-, EF-Core-, SQLite-, OR-Tools- oder Excel-Abhängigkeit.

### Application

- Application koordiniert getrennte Commands und Queries für die bestätigten Bedienfälle.
- Speicherverträge werden nach konkreten Anwendungsfällen benannt; eine generische CRUD-Schnittstelle wird nicht eingeführt.
- Ein- und Ausgaben sind unveränderliche Momentaufnahmen ohne EF-Core- oder WPF-Typen.
- Application verwendet ausschließlich Domain.

### Infrastructure

- EF-Core-, SQLite-, `DbContext`- und Migrationscode bleibt in `Salztal.Dienstplanung.Infrastructure.Persistence`.
- `IQueryable` und veränderliche Persistenzobjekte verlassen Infrastructure nicht.
- Zusammengehörige Änderungen werden transaktional gespeichert.
- Tests verwenden ausschließlich temporäre synthetische SQLite-Datenbanken.

### Desktop

- Ansichten und ViewModels liegen in fachlich benannten Feature-Bereichen für Einsatzorte und Dienste.
- ViewModels verwenden nur Application-Verträge und UI-eigene Typen.
- Nur `Salztal.Dienstplanung.Desktop.Composition` erzeugt und verdrahtet die konkrete SQLite-Implementierung.
- Fachlogik und Datenbankzugriff werden nicht in XAML, Code-behind oder ViewModels dupliziert.

### Übergabe an spätere Systeme

- System 03 referenziert später vorhandene `WorkLocationId`- und `ShiftTypeId`-Werte, verwaltet aber nicht deren Bezeichnungen oder Standardzeiten.
- System 05 ordnet jeden Bedarf genau einem Einsatzort und einem Diensttyp zu und besitzt dessen tatsächliche Zeit, Personenzahl und Datums-Ausnahmen.
- Ein Bedarf verlangt niemals `D` oder `Spr`; diese Muster verbinden später zwei getrennte Bedarfe beziehungsweise Zuweisungen ihrer referenzierten Diensttypen.
- Planning erhält diese Daten erst über eine vollständige unveränderliche Planungsmomentaufnahme aus Application.
- `Planning` und `Excel` werden in dieser Roadmap nicht fachlich erweitert.
- Neue Pakete sind nach aktuellem Stand nicht vorgesehen.

## Bestätigte Entscheidungen für diesen Entwurf

Die begleitende Datei [Fragen zu Einsatzorten, Diensttypen, Doppeldiensten und Belegung](WORK_LOCATIONS_SHIFT_TYPES_SPLIT_SHIFTS_QUESTIONS.md) wurde am 2026-09-13 fachlich beantwortet. Die begründete Änderung des bisherigen Zeitmodells steht in `docs/decisions/SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`.

- Cafeteria und Restaurant werden als Startwerte angelegt; weitere Einträge bleiben durch ein erweiterbares Datenmodell möglich.
- Einsatzorte besitzen keine eigenen Arbeits-, Öffnungs- oder Betriebszeiten.
- Cafeteria wird gelb, Restaurant rot und der Springer `Spr` blau gekennzeichnet. Text bleibt immer zusätzlich sichtbar.
- Frühdienst, Spätdienst, Cafeteria-Dienst A und Cafeteria-Dienst B sind normale Diensttypen. Sie gehören genau zu einem Einsatzort und besitzen bearbeitbare Standardzeiten.
- `D` und `Spr` sind zusammengesetzte Einsatzmuster und keine weiteren, von einem einzelnen Bedarf verlangten Diensttypen.
- Jeder Bedarf verlangt genau einen Diensttyp, besitzt aber seine eigene tatsächliche Zeit. Standardänderungen und einmalige Datums-Ausnahmen bleiben getrennt.
- Mitarbeitende erhalten später Freigaben für einzelne Diensttypen; Cafeteria A und B sind getrennte Freigaben.
- Zeiten werden in 30-Minuten-Schritten eingegeben und in ganzen Minuten gespeichert. Dienste über Mitternacht sind nicht vorgesehen.
- Innerhalb einzelner Dienste gibt es keine Unterbrechung, die nicht als Arbeitszeit zählt.
- Der Doppeldienst ist ausschließlich Frühdienst plus Spätdienst im Restaurant und wird als `D` angezeigt.
- `Spr` ist ein samstägliches Notfallmuster aus Cafeteria-Dienst B und anschließendem Spätdienst. Es darf automatisch nur verwendet werden, wenn sonst ein Spätdienst unbesetzt bleibt.
- Die Springer-Wechselzeit folgt dem tatsächlichen Ende des zweiten Cafeteria-Bedarfs. Eine vorherige Restaurant-Teilunterdeckung bleibt sichtbar.
- Bereits gespeicherte Planversionen behalten ihre damaligen Bezeichnungen, Farben und tatsächlichen Zeiten, auch wenn ein Stammdateneintrag später gelöscht wird.
- Die erste Bedienfassung zeigt und bearbeitet die vorbereiteten Startwerte auf einer gemeinsamen Einsatzort- und Diensttypseite. Sichtbares Anlegen, Löschen, Deaktivieren und Reaktivieren wird nur technisch vorbereitet und später umgesetzt.

## Bewusst noch offene Detailfragen

- genaue barrierearme Rot-, Gelb- und Blautöne,
- konkrete Anordnung und vollständige Feldauswahl der gemeinsamen WPF-Seite,
- Verhalten einer späteren Löschfunktion bei noch aktiven zukünftigen Bedarfen oder nicht abgenommenen Entwürfen,
- genaue Konfliktpriorisierung der Springer-Teilunterdeckung in den späteren Systemen 07 und 09.

Diese Detailfragen ändern nicht das bestätigte Fachmodell. Sie werden vor dem jeweils betroffenen Implementierungsschritt geklärt und abgenommen.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Schritte

### ED-01 – Teil-Roadmap entwerfen

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Ziel, Umfang, Nicht-Umfang, bestätigte Fachentscheidungen, bewusst offene Details, Architekturgrenzen, Schritte und echte Gates von System 04 sind festgehalten.
- Der noch nicht abgenommene System-03-Entwurf ist mit Begründung pausiert.
- `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` verweisen auf diesen aktiven Entwurf.
- Es werden noch keine Fachtypen, Datenbankobjekte oder Bedienoberflächen angelegt.

Prüfung:

- Der Entwurf stimmt mit den abgenommenen Grundlagen, der Architektur, den Clean-Code-Regeln und System 04 der Master-Roadmap überein.
- Bestätigte Antworten und bewusst offene Details werden klar voneinander getrennt.
- Die Eigentumsgrenze zu den Mitarbeiter-Einsatzfreigaben ist eindeutig beschrieben.
- Jeder spätere Schritt besitzt ein kleines, getrennt prüfbares Ergebnis.
- Lokale Markdown-Verweise, Statusangaben und `git diff --check` sind fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt den Roadmap-Entwurf oder nennt Änderungswünsche.

### ED-02 – Fachmodell und Bedienumfang bestätigen

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Die bereits beantworteten Fragen zu Einsatzorten, Diensttypen, Standard- und Bedarfszeiten, Doppeldienst, Springer, Lebenszyklus und minimaler Bedienung sind gemeinsam auf Widersprüche geprüft.
- Die Grenze zu Mitarbeiter-Dienstfreigaben, späterem Bedarf und automatischer Springer-Verwendung ist widerspruchsfrei festgelegt.
- Bestätigte Entscheidungen sind in allen betroffenen Grundlagen- und Statusdokumenten nachgeführt.
- Die späteren Schritte dieser Roadmap sind anhand der Entscheidungen präzisiert, ohne Fachcode anzulegen.

Prüfung:

- Keine konkrete Zeit, Pause, Abkürzung, Farbe oder betriebliche Regel ist über die bestätigten Antworten hinaus erfunden.
- Grundlagen, Roadmap, Master-Roadmap und Service-Leitungsdokumentation widersprechen einander nicht.
- Veraltete Formulierungen und Pfade werden gesucht; `git diff --check` bleibt fehlerfrei.

Abnahmebedingung:

- Der Auftraggeber bestätigt das Fachmodell und den Bedienumfang ausdrücklich.

### ED-03 – Einsatzorte fachlich modellieren

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Stabile Einsatzort-Kennung, bestätigte Bezeichnung und zusätzliche Farbkennung sind als gültige Domain-Typen umgesetzt.
- Cafeteria und Restaurant liegen als fachlich bestätigte, bearbeitbare Startwerte vor; weitere Einsatzorte sind nicht durch eine feste Code-Aufzählung ausgeschlossen.
- Die bestätigten Invarianten und Validierungsfehler liegen ausschließlich in Domain.
- Es existiert noch keine Speicherung oder WPF-Bedienung.

Prüfung:

- Domain-Tests decken gültige Werte, leere beziehungsweise ungültige Eingaben und die bestätigte Farbkennung gemäß ED-02 ab.
- Tests verwenden nur erfundene zusätzliche Einsatzorte; bestätigte Startwerte werden getrennt geprüft, falls sie vorgesehen sind.
- Domain bleibt frei von technischen Abhängigkeiten.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Fachliche Bedeutung, Validierungen und Testfälle der Einsatzorte werden bestätigt.

### ED-04 – Diensttypen und Standardzeiten fachlich modellieren

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Stabile Diensttyp-Kennung, bestätigte Bezeichnung beziehungsweise Tabellenanzeige, Einsatzortbezug und bearbeitbare Standardzeit sind in Domain umgesetzt.
- Die Standardzeit ist ausdrücklich von der späteren tatsächlichen Bedarfs- und Zuweisungszeit getrennt.
- Ungültige oder nicht unterstützte Zeitkombinationen können kein gültiges Fachobjekt erzeugen.
- Frühdienst, Spätdienst sowie Cafeteria-Dienst A und B liegen als bestätigte Startwerte vor; weitere Diensttypen bleiben technisch ergänzbar.

Prüfung:

- Domain-Tests decken Beginn, Ende, Standarddauer, Minutengenauigkeit, 30-Minuten-Eingabewerte, Einsatzortbezug und bestätigte Grenzfälle ab.
- Dienste über Mitternacht werden entsprechend ED-02 sichtbar abgelehnt.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Diensttyp, Zeitdarstellung und Trennung von Standard- und tatsächlicher Zeit werden bestätigt.

### ED-05 – Doppeldienst und Springer fachlich modellieren

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Der bestätigte Doppeldienst `D` besteht ausschließlich aus Frühdienst und Spätdienst.
- Beide Teile liegen im Restaurant, überschneiden sich nicht und bleiben als getrennte Abschnitte erhalten.
- Die Unterbrechung zwischen den Standardzeiten wird nachvollziehbar als nicht zur Arbeitszeit gehörend behandelt.
- Der Springer `Spr` ist eine getrennte samstägliche Musterdefinition aus Cafeteria-Dienst B und anschließendem Spätdienst. Sie enthält keine fest programmierte Wechselzeit.
- `Spr` verweist auf beide Einsatzorte und Diensttypen, besitzt keine Unterbrechung und beschreibt die später benötigten Freigaben. Konkrete Bedarfs-, Zuweisungs- und Teilunterdeckungszeiten werden noch nicht erzeugt.

Prüfung:

- Domain-Tests decken gültigen Doppeldienst, falschen Einsatzort, Überschneidung, vertauschte Reihenfolge und die bestätigte Unterbrechung ab.
- Springer-Tests decken Samstag, Reihenfolge, zwei Einsatzorte, die beiden referenzierten Diensttypen und das Fehlen einer festen Wechselzeit ab.
- Die Standarddauer des Doppeldiensts enthält die Unterbrechung nicht. Tatsächliche Doppeldienst- und Springer-Arbeitsdauern werden erst aus späteren Zuweisungsabschnitten berechnet.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Doppeldienst, Springer, Fehlermeldungen und Grenzfälle werden bestätigt.

### ED-06 – Anwendungsfälle und Speicherverträge umsetzen

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Die bestätigten Commands und Queries zum Anzeigen und Bearbeiten der vorbereiteten Einsatzorte und normalen Diensttypen sind in Application umgesetzt. `D` und `Spr` werden als feste Musterdefinitionen lesbar bereitgestellt, aber nicht frei umkombiniert.
- Verträge und Kennungen sind so erweiterbar, dass sichtbares Anlegen und Löschen später ohne fest programmierte Sonderfälle ergänzt werden kann; diese Bedienfälle werden noch nicht freigegeben.
- Kleine anwendungsfallbezogene Speicherverträge ersetzen eine generische CRUD-Schnittstelle.
- Ein- und Ausgaben sind unveränderliche Momentaufnahmen mit verständlichen Validierungsergebnissen.

Prüfung:

- Application-Tests prüfen Erfolgs-, Validierungs-, Nicht-gefunden-, Konflikt- und Abbruchfälle mit synthetischen Daten.
- Application verwendet nur Domain und enthält keine WPF-, EF-Core- oder SQLite-Typen.
- Vollständiger Build und betroffene Architekturtests bestehen.

Abnahmebedingung:

- Anwendungsabläufe, Ergebnisse und Fehlerfälle werden bestätigt.

### ED-07 – Lokale SQLite-Speicherung und Migration umsetzen

Status: `[x]` – am 2026-09-13 ausdrücklich abgenommen

Geplantes Ergebnis:

- Infrastructure implementiert ausschließlich die in ED-06 bestätigten Speicherverträge.
- Eine erste veröffentlichbare Migration bildet Einsatzorte, Diensttypen, Standardzeiten, Doppeldienst und Springer-Muster ab.
- Cafeteria, Restaurant und die bestätigten Diensttypen werden idempotent als Startwerte bereitgestellt.
- Mapping und Speicherung bewahren Kennungen, Farben, Standardzeiten, Beziehungen und Muster ohne Verlust.
- Die konkrete Implementierung wird ausschließlich in `Desktop.Composition` verdrahtet.

Prüfung:

- Infrastructure-Tests verwenden echte temporäre SQLite-Dateien.
- Migration von leerer Datenbank, Speichern, Laden, Ändern und Neustart-Round-trip bestehen.
- Beziehungen und Eindeutigkeitsregeln werden mit dem tatsächlichen SQLite-Verhalten geprüft.
- Fehlgeschlagene Schreibvorgänge hinterlassen keinen teilweise gültigen Zustand.
- Vollständiger Build, Infrastructure-Tests und Architekturtests bestehen.

Abnahmebedingung:

- Datenmodell, Migration und lokale Round-trips werden technisch bestätigt.

### ED-08 – Einsatzortverwaltung in WPF umsetzen

Status: `[x]` – WPF-Einsatzortverwaltung sichtbar geprüft und am 2026-09-13 abgenommen

Geplantes Ergebnis:

- Eine gemeinsame deutsche Einsatzort- und Diensttypseite zeigt die Einsatzorte und ermöglicht die bestätigte Bearbeitung.
- Laden, leerer Zustand, Arbeitszustand und technische Fehler werden verständlich dargestellt.
- Sichtbares Anlegen, Deaktivieren, Reaktivieren und Löschen ist in dieser ersten Bedienfassung noch nicht enthalten.
- ViewModel und View greifen ausschließlich über Application auf Daten zu.

Prüfung:

- Desktop-Tests prüfen Zustände, Validierungen und Benutzerbefehle des ViewModels.
- Architekturtests sichern die `Desktop.Composition`-Grenze.
- Die WPF-App wird manuell gestartet; Übersicht, Eingabe, Fehlerdarstellung, Lesbarkeit und deutsche Texte werden sichtbar geprüft.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbare Einsatzortverwaltung und ihre Bedienbarkeit.

### ED-09 – Diensttypen, Doppeldienst und Springer in WPF umsetzen

Status: `[x]` – sichtbare Diensttypverwaltung und feste Einsatzmuster am 2026-09-13 abgenommen

Geplantes Ergebnis:

- Diensttypen und ihre Standardzeiten können im bestätigten Umfang angezeigt und bearbeitet werden.
- Der feste Doppeldienst `D` und das Springer-Muster `Spr` werden verständlich angezeigt; freie neue Kombinationen werden nicht angeboten.
- Standardzeiten, Einsatzort, Doppeldienst-Unterbrechung und die Regel zur später bedarfsabhängigen Springer-Wechselzeit werden nachvollziehbar unterschieden. Eine feste Springer-Wechselzeit wird nicht bearbeitet.
- Validierungsfehler erscheinen deutsch und ohne Verlust bereits eingegebener Werte.

Prüfung:

- Desktop- und Application-Tests prüfen erfolgreiche sowie ungültige Eingaben und Abbruch ohne Speicherung.
- Ein manueller WPF-Ablauf mit klar synthetischen Daten prüft Anzeigen, Bearbeiten, Neustart und erneutes Laden.
- Doppeldienste und Springer-Einsätze außerhalb der bestätigten Grenzen werden sichtbar abgelehnt.
- Es findet kein Netzwerkzugriff statt; keine Testdaten oder lokale Datenbank werden versioniert.

Abnahmebedingung:

- Der Auftraggeber bestätigt die sichtbaren Diensttyp-, Doppeldienst- und Springer-Abläufe.

### ED-10 – System 04 gemeinsam abschließen

Status: `[x]` – Systemprüfung abgenommen und Archivierung am 2026-09-13 freigegeben

Geplantes Ergebnis:

- Domain, Application, Infrastructure und Desktop bilden den bestätigten Umfang von System 04 widerspruchsfrei ab.
- Roadmap, `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` zeigen denselben tatsächlichen Stand.
- Der pausierte System-03-Entwurf wird gegen die entstandenen Kennungen und Systemgrenzen geprüft, aber nicht ohne eigene Freigabe umgesetzt.
- System 04 wird erst nach ausdrücklicher Abschlussabnahme als abgeschlossen archiviert.

Prüfung:

- Gesperrte Paketwiederherstellung, vollständiger Build und alle automatischen Tests bestehen ohne neue Warnungen.
- Der vollständige Einsatzort-, Diensttyp-, Doppeldienst- und Springer-Ablauf wird mit synthetischen Daten manuell in WPF geprüft.
- Eine Suche findet keine veralteten Roadmap-Pfade oder widersprüchlichen Statusangaben.
- `git diff --check` meldet keine Whitespace-Fehler.
- Die Dateiliste enthält keine echten oder sensiblen Daten und keine Datenbank-, Sicherungs-, Export- oder Buildartefakte.

Prüfnachweis vom 2026-09-13:

- Die gesperrte Wiederherstellung für alle 13 Projekte und der vollständige Build mit 0 Warnungen und 0 Fehlern bestehen.
- 46 Domain-, 14 Application-, 6 Infrastructure-, 13 Desktop- und 13 Architekturtests bestehen; Planning und Excel enthalten in diesem System weiterhin keine eigenen Testfälle.
- Die sichtbaren Abläufe wurden über ED-08 und ED-09 schrittweise mit synthetischen beziehungsweise lokalen Katalogdaten geprüft und vom Auftraggeber bestätigt.
- Status- und Pfadsuche sowie `git diff --check` sind fehlerfrei. Unter 158 versionierbaren Dateien befinden sich keine Datenbank-, Sicherungs-, Export-, Build- oder Bilddateien.
- Der System-03-Entwurf ist gegen `WorkLocationId`, `ShiftTypeId` und die unveränderliche Katalogabfrage geprüft und bleibt bis zu seiner eigenen Roadmap-Abnahme ohne Fachcode.

Abnahmebedingung:

- Der Auftraggeber bestätigt System 04 und erlaubt die Archivierung dieser Roadmap sowie die erneute Vorbereitung von System 03.

## Echte externe und manuelle Gates

- Vor jeder Abnahme und vor einem beauftragten Commit wird die Dateiliste erneut auf echte Mitarbeiter- und Plandaten geprüft.
- Die Roadmap und jede fachliche Entscheidung benötigen ausdrückliche Abnahme; Schweigen ist keine Zustimmung.
- Konkrete Diensttypen, Uhrzeiten, Pausen und betriebliche Regeln werden ausschließlich nach Bestätigung der Service-Leitung umgesetzt.
- Die sichtbaren WPF-Abläufe müssen zusätzlich zu automatischen Tests manuell geprüft werden.
- Alle manuellen Beispiele verwenden ausschließlich klar erfundene Daten.
- Ein lokaler Build ersetzt weder den sichtbaren WPF-Test noch die spätere portable Windows-11-Prüfung.
- Portable Veröffentlichung und Start auf einem sauberen Windows-11- beziehungsweise Klinikrechner bleiben bis System 15 offen.
- Die fachliche Endabnahme der gesamten Dienstplanung bleibt offen; System 04 allein erzeugt noch keinen Dienstplan.

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

System 03 mit MA-01 fortsetzen: den wieder aktivierten, noch nicht abgenommenen Roadmap-Entwurf gemeinsam prüfen und bestätigen oder Änderungswünsche festhalten. Mitarbeiter-Fachcode beginnt erst nach der Roadmap- und späteren Fachmodellabnahme.
