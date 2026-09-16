# Teil-Roadmap: Mitarbeitertypen-Vorbereitung, Verfügbarkeiten und Abwesenheiten

Status: Abgeschlossen und am 2026-09-15 ausdrücklich abgenommen

Stand: 2026-09-15

## Ziel und Nutzen

Diese Roadmap beschreibt System 06 „Verfügbarkeiten und Abwesenheiten“ einschließlich eines ausdrücklich davor gewünschten Vorbereitungsteils für die Mitarbeitertypen.

Zuerst soll die Service-Leitung Mitarbeitertypen in einem eigenen Tab datengetrieben anlegen und bearbeiten können. Dabei werden auch die drei noch fehlenden Starttypen sowie der für `U` und `K` benötigte Tageswert eingeführt.

Danach entsteht die erste gemeinsame Drei-Wochen-Ansicht „Dienstplan SER“. Dort kann die Service-Leitung für aktive Mitarbeitende `U`, `K` und verbindliche rote `X` je Kalendertag eintragen. Die App zeigt daraus das wirksame Wochen-Soll und bewahrt die Eingaben lokal für die spätere Planung.

System 06 erzeugt noch keinen Dienstplan. Typ1-Dienste, schwarze `X`, automatische Generierung, Konfliktberichte, Planabnahme und Excel-Export werden nur vorbereitet beziehungsweise an die zuständigen späteren Systeme übergeben.

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
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/decisions/S03_SHARED_SQLITE_MIGRATION_BOUNDARY.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_QUESTIONS.md`
- abgeschlossene Systeme 03, 04 und 05 mit ihren Roadmaps und Entscheidungen
- ausschließlich synthetische Mitarbeiter-, Abwesenheits- und Plandaten in Tests und Dokumentation

## Bestätigte fachliche Grundlagen

### Mitarbeitertypen

- Mitarbeitertypen sind gemeinsam referenzierte, bindende Fachdefinitionen.
- Bearbeitbar sind verständlicher Name, Wochen-Soll, Zulässigkeit und gemeinsamer Tageswert für `U` und `K`, jede normale Dienstfreigabe sowie Berechtigungen für `D` und `Spr`.
- Der beim Anlegen vergebene eindeutige sichtbare Typcode bleibt stabil.
- Neu angelegte Typen sind normale automatisch planbare Typen mit einem zwingenden Wochenkorridor von minus drei bis plus drei Stunden.
- Auch die genannten Stammdaten und Einsatzberechtigungen von Typ1 und AH sind bearbeitbar. Ihre besonderen Planungsrollen bleiben geschützte strukturierte Eigenschaften und werden weder aus Code noch Anzeigename abgeleitet.
- Ein noch nie referenzierter normaler Typ darf nach Sicherheitsabfrage gelöscht werden. Verwendete Typen sowie die für Typ1 und AH benötigten Sondertypen bleiben erhalten.
- Eine Änderung wirkt auf alle aktuell zugeordneten Mitarbeitenden und nachfolgende Planungen. Abgenommene Planversionen bleiben unverändert; vorhandene Entwürfe werden später sichtbar gegen die aktuelle Typfassung geprüft.

### Starttypen und Tageswerte

| Typ | Wochen-Soll | Tageswert `U`/`K` | Reguläre Freigaben |
|---|---:|---:|---|
| `Typ1` | 40 Stunden | 8 Stunden | vorhandene Typ1-Freigaben; besondere Rolle geschützt |
| `Typ20` | 20 Stunden | 4 Stunden | Frühdienst, Spätdienst und `D` |
| `Typ20a` | 20 Stunden | 4 Stunden | alle normalen Dienste, `D` und `Spr` |
| `Typ25` | 25 Stunden | 5 Stunden | Frühdienst, Spätdienst und `D` |
| `Typ25a` | 25 Stunden | 5 Stunden | alle normalen Dienste, `D` und `Spr` |
| `Typ30` | 30 Stunden | 6 Stunden | Frühdienst, Spätdienst und `D` |
| `Typ30a` | 30 Stunden | 6 Stunden | alle normalen Dienste, `D` und `Spr` |
| `Typ35` | 35 Stunden | 7 Stunden | Frühdienst, Spätdienst und `D` |
| `Typ35a` | 35 Stunden | 7 Stunden | alle normalen Dienste, `D` und `Spr` |
| `TypAH1` | 10 Stunden | `U`/`K` nicht zulässig | vorhandene AH1-Freigaben; besondere Rolle geschützt |
| `TypAH2` | 10 Stunden | `U`/`K` nicht zulässig | vorhandene AH2-Freigaben; besondere Rolle geschützt |

Die vorhandenen vorschlagsfähigen und nur nach Laufoption aktiven Sonderfreigaben bleiben strukturierte Werte. Die Pflegeoberfläche muss ihren Zustand verständlich anzeigen und darf sie nicht unbemerkt in reguläre Freigaben umwandeln.

### Tageseingaben

- Ein leeres Feld bedeutet vor der Generierung, dass die aktive Person grundsätzlich verfügbar ist.
- `U`, `K` und rotes `X` gelten jeweils für den ganzen Kalendertag.
- Pro Mitarbeiter und Datum ist höchstens eine aktuelle Tageseingabe wirksam.
- `U`, `K` und rote `X` sind zwingende Sperren für die spätere automatische Planung.
- Die Generierung darf sie niemals überschreiben.
- Die Service-Leitung darf eine eigene Eingabe nach einer klaren Sicherheitsabfrage korrigieren oder entfernen.
- Rote `X` werden für jeden konkreten Tag einzeln eingetragen, nicht automatisch in spätere Zeiträume übernommen und reduzieren das Wochen-Soll nicht.
- Für AH sind `U` und `K` zunächst nicht auswählbar. Eine krankheitsbedingte Sperre wird als rotes `X` eingetragen.
- Wunschfrei, Fortbildung, Notizen, Teilzeiten und wiederkehrende persönliche Verfügbarkeitsmuster gehören nicht zur ersten Fassung.

### Wirksames Wochen-Soll

Für jede Woche von Montag bis Sonntag gilt:

`Wirksames Wochen-Soll = ungekürztes Wochen-Soll − Summe der U- und K-Tageswerte`, mindestens null Minuten.

Der Tageswert gilt auch bei einem Eintrag am Samstag, Sonntag oder Feiertag. Rote und später generierte schwarze `X` reduzieren das Soll nicht. Die Berechnung verwendet ausschließlich ganze Minuten.

### Drei-Wochen-Ansicht

- Die Ansicht heißt „Dienstplan SER“.
- Ein ausgewählter Montag beginnt den Zeitraum mit genau 21 Tagen bis zum dritten Sonntag.
- Navigation ist über Datumsauswahl sowie „Vorherige drei Wochen“ und „Nächste drei Wochen“ möglich.
- Eine Zeile gehört zu einer aktiven Person. Links stehen Name und aktueller bindender Mitarbeitertyp; rechts folgen 21 Tagesfelder.
- Zusätzlich werden je Person und Woche ungekürztes und wirksames Wochen-Soll angezeigt.
- Ein einzelnes Feld kann über klar begrenzte Aktionen oder Tastaturkürzel bearbeitet werden. Unbekannter Freitext ist unmöglich.
- Mehrfachauswahl und gleichzeitiges Füllen mehrerer Tage gehören nicht zur ersten Fassung.
- Das Entfernen oder Ersetzen zeigt Person, Datum, bisherigen und neuen Wert und verlangt eine Bestätigung.

### Typ1, Bürozeit und AH als spätere Übergaben

- System 06 bereitet die gemeinsame Zellstruktur vor. Vorgetragene Typ1-Dienste werden erst in System 08 ergänzt.
- Typ1 benötigt in jeder nicht vollständig abwesenden Woche mindestens einen vorgetragenen Dienst. Bei vollständiger Abwesenheit darf mit deutlichem Hinweis ohne Typ1-Dienst generiert werden.
- Nur ein vorgetragener Früh- oder Spätdienst kann als Bürozeit `B` markiert werden. Er zählt vollständig zu den Typ1-Stunden, deckt aber keinen Personalbedarf.
- Nach Planabnahme bleibt der zugrunde liegende Dienst sichtbar; nur `B` wird in Plan und Excel-Ausgabe ausgeblendet. Die strukturierte Büroinformation bleibt in der unveränderlichen Planversion erhalten.
- Die spätere Generierung plant zuerst Nicht-AH. AH füllt danach nur verbleibende zulässige Lücken und verdrängt keine bereits geplante Nicht-AH-Person.
- Zehn AH-Stunden bleiben Ziel, zwölf Stunden zwingende Obergrenze. Weniger als sechs und mehr als zehn Stunden werden im Generierungsbericht gemeldet.

## Umfang

### Verbindlicher Vorbereitungsteil vor System 06

- `EmployeeType` um strukturierte `U`-/`K`-Zulässigkeit und Tageswert erweitern
- `Typ20`, `Typ20a` und `Typ25a` als zusätzliche datengetriebene Startwerte ergänzen
- normale und besondere Planungsrollen strukturiert und codeunabhängig unterscheiden
- Mitarbeitertypen lesen, anlegen, bearbeiten und unter Referenzschutz löschen
- optimistische Änderungskontrolle und verständliche fachliche Fehler
- gemeinsame SQLite-Migration ohne Umschreiben veröffentlichter Migrationen
- eigener deutscher WPF-Tab „Mitarbeitertypen“
- sichtbares manuelles Abnahmegate vor der Abwesenheitsimplementierung

### System 06

- unveränderliche Domain-Werte für Kalendertag und Tageseingabe
- Arten `U`, `K` und rotes `X` als strukturierte Werte, nicht als Farbe oder Freitext
- wöchentliche Sollreduktion in ganzen Minuten
- Application-Abfragen und -Schreibabläufe für einen 21-Tage-Zeitraum
- lokale Speicherung mit genau einem aktuellen Eintrag je Mitarbeiter und Datum
- Referenzschutz für verwendete Mitarbeitende und Mitarbeitertypen
- WPF-Ansicht „Dienstplan SER“ für aktive Mitarbeitende
- Einzelbearbeitung, Tastaturbedienung, Bestätigungen, Wochenwechsel und Sollanzeige
- vorbereitete unveränderliche Übergaben an Systeme 07 bis 13

## Nicht-Umfang

- echte Mitarbeiter-, Krankheits-, Abwesenheits- oder Plandaten
- Diagnosen, Abwesenheitsgründe oder freie Notizen
- Fortbildung, Wunschfrei oder eingeschränkte Zeitfenster
- automatische Feiertagserkennung
- wiederkehrende persönliche Wochenregeln
- Mehrfachauswahl oder Sammelbearbeitung mehrerer Tagesfelder
- schwarze `X` als Eingabe oder Ergebnis in System 06
- tatsächliche Typ1-Dienst- oder Bürozeiteingabe in System 06
- OR-Tools-Modell, automatische Generierung oder AH-Planungsphasen
- vollständiger Regelkatalog und Prioritätsoptimierung
- Konflikt- und Generierungsbericht
- manuelle Bearbeitung eines erzeugten Plans
- Planversionen, Abnahme oder Excel-Export
- Zeitkonten
- Änderung von Einsatzorten, Diensttypen oder der Zusammensetzung von `D` und `Spr`
- frei anlegbare Typ1- oder AH-Sonderrollen
- Löschen der für Typ1 und AH benötigten Sondertypen

## Architekturgrenzen und geplante Code-Anker

### Domain

- `src/Salztal.Dienstplanung.Domain/Employees/EmployeeType.cs`
- `src/Salztal.Dienstplanung.Domain/Employees/EmployeeTypePlanningPolicy.cs`
- `src/Salztal.Dienstplanung.Domain/Employees/InitialEmployeeTypeCatalog.cs`
- neue kleine Fachwerte unter `Domain/Employees` für Abwesenheits-Tageswert und geschützte Planungsrolle
- neuer Fachbereich `Domain/Availabilities` für Tageseingaben und Sollberechnung

Domain kennt weder WPF, Datenbank, deutsche Dialogtexte noch OR-Tools. Die Sollformel und die Zulässigkeit von Tageswerten besitzen genau hier ihre fachliche Definition.

### Application

- vorhandener Bereich `src/Salztal.Dienstplanung.Application/Employees`
- neuer Bereich `src/Salztal.Dienstplanung.Application/Availabilities`
- unveränderliche Anfragen, Ergebnisse und Momentaufnahmen
- getrennte Speicherverträge für Lesen, Speichern/Korrigieren und Entfernen
- Katalog- und Referenzprüfung vor jedem Schreibversuch
- verständliche deutsche Meldungen aus stabilen fachlichen Codes

Application lädt konsistente Datenstände und übergibt keine Datenbankabfrage oder veränderliche UI-Objekte.

### Infrastructure

- bestehender gemeinsamer `ServiceCatalogDbContext`
- vorhandener Bereich `Persistence/Employees`
- neuer Bereich `Persistence/Availabilities`
- Fortführung von `Persistence/ServiceCatalog/Migrations`

Veröffentlichte Migrationen werden nicht verändert. Neue Spalten, Typen und Tabellen erhalten eine neue Migration. Fremdschlüssel verhindern das Löschen verwendeter Mitarbeiter und Typen; Eindeutigkeitsregeln verhindern zwei aktuelle Tageseinträge für dieselbe Person und dasselbe Datum.

### Desktop

- neuer Bereich `Desktop/Features/EmployeeTypes`
- neuer Bereich `Desktop/Features/Availabilities`
- Einbindung über `Desktop/Composition`

ViewModels verwenden ausschließlich Application-Verträge. Farbe ist beim roten `X` nur Darstellung; Art und Sperrwirkung stammen aus den strukturierten Anzeigedaten.

### Planning und Excel

Planning und Excel bleiben in dieser Roadmap unverändert. Die Roadmap dokumentiert nur die später benötigten Übergaben für harte Sperren, Typ1-Bürozeit, AH-Reihenfolge und ausgeblendete Exportkennzeichnung.

## Offene Entscheidungen

Für den Beginn des Roadmap-Entwurfs bestehen keine offenen fachlichen Entscheidungen. Folgende Punkte sind bewusst spätere Gates und keine stillschweigend beantworteten Details:

- genaue sichtbare Zellanordnung und Bedienwirkung auf der realen Bildschirmgröße,
- endgültige Darstellung von Typ1-Diensten und `B` ab System 08,
- technische Regeldefinitionen und Solverübersetzung ab System 07 beziehungsweise 09,
- Excel-Zellzuordnung nach Bereitstellung und Charakterisierung der echten Vorlage.

## Nummerierte Schritte

Für diese Roadmap gilt ab MT-04 die bedingte automatische Weiterführung: Ein planmäßig und fehlerfrei abgeschlossener Schritt darf direkt in den nächsten übergehen, wenn weder eine unerwartete Entscheidung oder kritische Frage noch ein externes, manuelles oder visuelles Gate besteht. Bei Handlungsbedarf wird gestoppt. Nach jedem abgeschlossenen Schritt ertönt ein Abschlusston; bei Handlungsbedarf zusätzlich ein unterscheidbarer Hinweiston.

### MT-01 – Mitarbeitertyp-Vertrag erweitern

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- strukturierte Planungsrolle für normal, Typ1 und AH absichern,
- Zulässigkeit und minutengenauen Tageswert für `U` und `K` ergänzen,
- Bearbeitung unter Beibehaltung von Kennung und Code ermöglichen,
- `Typ20`, `Typ20a` und `Typ25a` ergänzen,
- normale neue Typen mit dem bestätigten Standardkorridor vorbereiten.

Prüfung:

- fokussierte Domain-Tests für gültige und ungültige Tageswerte, alle elf Starttypen, Rollen und Freigaben,
- bestehende System-03-Tests bleiben grün,
- `dotnet build` und `git diff --check`.

Abnahmebedingung:

- Der Auftraggeber bestätigt Fachmodell, neue Starttypen und unveränderte Sonderrollen.

### MT-02 – Application-Verträge für die Typenpflege

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- vollständigen Typkatalog strukturiert lesen,
- normalen Typ anlegen,
- erlaubte Felder eines vorhandenen Typs bearbeiten,
- noch nie referenzierten normalen Typ nach Sicherheitsprüfung löschen,
- unbekannte Katalogbezüge, doppelte Codes, Änderungskonflikte und geschützte Sonderaktionen verständlich ablehnen.

Prüfung:

- Application-Tests für Erfolg, Validierung, Abbruch, parallele Änderung und Referenzkonflikte,
- keine Auswertung sichtbarer Codes oder deutscher Anzeigetexte als Planungslogik.

Abnahmebedingung:

- Der Auftraggeber bestätigt die vorgesehenen Abläufe und Fehlermeldungen.

### MT-03 – Mitarbeitertypen dauerhaft speichern

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- neue Migration in der bestehenden gemeinsamen Folge,
- drei zusätzliche Starttypen idempotent ergänzen,
- Tageswert und Rolle verlustfrei speichern,
- Anlegen, Bearbeiten und referenzgeschütztes Löschen atomar implementieren,
- vorhandene Datenbanken aus dem aktuellen System-05-Stand erhalten.

Prüfung:

- echte temporäre SQLite-Tests für leere und vorhandene synthetische Datenbanken,
- Neustart-, Transaktions-, Eindeutigkeits-, Fremdschlüssel- und Migrationshistorientests,
- EF-Migrationsstand ohne ausstehende Modelländerung.

Abnahmebedingung:

- Alle Typwerte bleiben nach Neustart korrekt und keine vorhandenen synthetischen Daten gehen verloren.

### MT-04 – WPF-Tab „Mitarbeitertypen“

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 sichtbar abgenommen

Umfang:

- Typübersicht und getrennten Bearbeitungsbereich ergänzen,
- Code, Name, Wochen-Soll, `U`-/`K`-Regel und alle Dienst-/Musterberechtigungen verständlich anzeigen,
- Anlegen, Bearbeiten und zulässiges Löschen mit Sicherheitsabfragen anbieten,
- Wirkung auf zugeordnete Mitarbeitende vor einer Änderung deutlich erklären,
- Lade-, Leer-, Arbeits-, Erfolgs-, Validierungs- und technische Fehlerzustände darstellen.

Prüfung:

- Desktop-ViewModel-Tests einschließlich Abbruch, Eingabeerhalt und vollständiger Änderungsbenachrichtigung sichtbarer Rückmeldungstexte,
- Desktop-Konstruktions- und Render-Tests für vollständig auflösbare lokale XAML-Ressourcen, schreibgeschützte Anzeigewerte, gemeinsam sichtbare Dienst-/Musternamen und Berechtigungsstatus sowie den sichtbaren Erfolgstext,
- manueller sichtbarer Test ausschließlich mit synthetischen Typen und Personen.

Abnahmebedingung:

- Die Service-Leitung bestätigt den vollständigen sichtbaren Typenablauf.

### MT-05 – Vorbereitungsteil gemeinsam abschließen

Status: `[x]` – Gesamtprüfung abgeschlossen und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- Domain, Application, Infrastructure und Desktop gemeinsam prüfen,
- Leitdokumente und Service-Leitungsstand aktualisieren,
- Datenschutz-, Migrations-, Architektur- und Repository-Hygiene prüfen.

Prüfung:

- gesperrte Paketwiederherstellung,
- vollständiger Build und alle betroffenen Tests,
- Format-, Architektur-, Migrations- und Diff-Prüfung,
- bestätigter manueller WPF-Test aus MT-04.

Abnahmebedingung:

- Der Vorbereitungsteil ist ausdrücklich abgenommen. Erst danach darf VA-01 beginnen.

### VA-01 – Tageseingaben und Sollberechnung in Domain

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- stabile Tageseingabe mit Mitarbeiterkennung, Datum und Art modellieren,
- `U`, `K` und rotes `X` eindeutig unterscheiden,
- genau eine aktuelle Eingabe pro Person und Datum fachlich absichern,
- `U`-/`K`-Zulässigkeit und wirksames Wochen-Soll berechnen,
- vollständige Typ1-Abwesenheitswoche und relevante Ersetzungsfälle prüfen.

Prüfung:

- Domain-Tests für alle Arten, Wochenränder, Wochenende/Feiertag, Untergrenze null, AH-Ausschluss und Typ1-Beispiele.

Abnahmebedingung:

- Die bestätigte Formel und alle Tageswirkungen sind ohne UI- oder Datenbankabhängigkeit nachgewiesen.

### VA-02 – Application-Lesevertrag für drei Wochen

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- ausgewähltes Datum auf den zugehörigen Montag beziehen,
- genau 21 Tage und nur aktive Mitarbeitende laden,
- Name, Typ, vorhandene Einträge sowie ungekürztes und wirksames Soll je Woche projizieren,
- fehlende oder widersprüchliche Katalogdaten sichtbar ablehnen,
- unveränderliche Übergabemodelle für die spätere gemeinsame Planansicht bereitstellen.

Prüfung:

- Application-Tests für leeren Bestand, Wochenwechsel, drei getrennte Sollberechnungen, Abbruch und fehlerhafte Lesestände.

Abnahmebedingung:

- Der 21-Tage-Lesestand ist vollständig, konsistent und ohne technische Objekte an Desktop übergebbar.

### VA-03 – Application-Schreibabläufe

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- Tageseingabe setzen beziehungsweise ersetzen,
- Tageseingabe nach bestätigter Sicherheitsabfrage entfernen,
- aktive Person, vorhandenen Typ, Typzulässigkeit und Änderungsversion prüfen,
- zukünftige Eingaben für deaktivierte Personen blockieren,
- verwendete historische Planversionen nicht verändern.

Prüfung:

- Application-Tests für jede Art, AH-Ausschluss, Ersetzen, Entfernen, parallele Änderung, deaktivierte Person und Abbruch.

Abnahmebedingung:

- Jeder Ablauf liefert stabile Codes und verständliche deutsche Meldungen ohne Teiländerung.

### VA-04 – Tageseingaben in SQLite speichern

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 ausdrücklich abgenommen

Umfang:

- neue Tabelle und Konfiguration im gemeinsamen Context,
- eindeutiger Schlüssel je Mitarbeiter und Datum,
- restriktive Fremdschlüssel und optimistische Änderungskontrolle,
- atomare Speicherung, Ersetzung und Entfernung,
- neue Migration ohne Änderung früherer Migrationen.

Prüfung:

- temporäre SQLite-Tests für Migration ab leerem und aktuellem Stand, Neustart, Eindeutigkeit, Fremdschlüssel, Konflikt und Rollback,
- keine produktive Datenbank im Repository.

Abnahmebedingung:

- Alle synthetischen Einträge bleiben korrekt erhalten und Fehler beschädigen den vorherigen Stand nicht.

### VA-05 – Drei-Wochen-Ansicht „Dienstplan SER“

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-15 sichtbar abgenommen

Umfang:

- aktive Mitarbeitende als Zeilen und 21 Tage als Spalten zeigen,
- Montag, drei Wochen, Wochentage und konkrete Daten eindeutig darstellen,
- Zeitraum per Datum und Vor-/Zurück-Aktion wechseln,
- `U`, `K` und rotes `X` per Aktionsauswahl und Tastaturkürzel setzen,
- unbekannte Eingaben verhindern,
- roten Buchstaben `X` auf normalem Hintergrund plus nichtfarbliche Bedeutung anzeigen,
- Wochen-Soll und wirksames Soll je Person und Woche zeigen,
- Ersetzen und Leeren mit deutlicher Sicherheitsabfrage bestätigen,
- gemeinsame Zellstruktur für spätere Typ1- und Plandaten vorbereiten, ohne diese vorwegzunehmen.

Prüfung:

- Desktop-Tests für Laden, Navigation, Tastatur, Aktionen, Bestätigen/Abbrechen, AH-Einschränkung und Fehlerzustände,
- manueller sichtbarer Test mit synthetischen Personen auf der vorgesehenen Windows-Darstellung.

Abnahmebedingung:

- Die Service-Leitung bestätigt Lesbarkeit, Navigation, Einzelbearbeitung, rote-X-Darstellung und Sollanzeige.

### VA-06 – System-06-Gesamtnachweis und Übergaben

Status: `[x]` – Gesamtnachweis vollständig grün und System 06 am 2026-09-15 ausdrücklich abgenommen

Umfang:

- alle Schichten und Verträge gemeinsam abgleichen,
- Übergaben an Systeme 07 bis 13 dokumentieren,
- Roadmap, Master-Roadmap, Status und Service-Leitungsdokumente wahrheitsgemäß aktualisieren,
- Datenschutz und Repository-Hygiene abschließend prüfen.

Prüfung:

- gesperrte Paketwiederherstellung,
- vollständiger Build und vollständiger Testlauf,
- Format-, Architektur-, Migrations-, Pfad- und Diff-Prüfung,
- bestätigter manueller WPF-Test aus VA-05.

Abnahmebedingung:

- System 06 ist fachlich, technisch und sichtbar geprüft und ausdrücklich abgenommen. Erst dann wird die Roadmap nach `completed` verschoben.

## Übergaben an spätere Systeme

| Übergabe | Inhalt | Eigentümer |
|---|---|---|
| Zentrale Regeln | normale Wochenkorridore, freie Tage, Urlaubswochenenden, höchstens sieben Arbeitstage, Doppeldienstziele und AH-Grenzen | System 07 |
| Planmomentaufnahme | Typfassungen, Tageseingaben, angrenzende Historie, Typ1-Dienste und strukturierte Bürokennzeichnung | System 08 |
| Generierung | schwarze `X`, gesperrte Eingaben, zuerst Nicht-AH und danach AH für Lücken | System 09 |
| Berichte | ungedeckte Bedarfe, Regelabweichungen, AH unter sechs beziehungsweise über zehn Stunden und Typ1-Hinweise | System 10 |
| Planbearbeitung | Dienste, schwarze `X`, Typ1-Eingaben und sichtbare Entwurfskennzeichnung `B` bearbeiten | System 11 |
| Planversion | frühere Fassungen unverändert erhalten; Büroinformation intern bewahren und sichtbares `B` nach Abnahme ausblenden | System 12 |
| Excel | zugrunde liegenden Typ1-Dienst ohne sichtbares `B` nach bestätigter Vorlage exportieren | System 13 |

## Echte externe und manuelle Gates

- Der Roadmap-Entwurf wurde mit dem Auftrag zum Start von MT-01 am 2026-09-15 ausdrücklich abgenommen; dieses Gate ist bestanden.
- Der Mitarbeitertypen-Tab aus MT-04 wurde am 2026-09-15 durch die Service-Leitung sichtbar geprüft und bestätigt; dieses Gate ist bestanden.
- Die Service-Leitung hat die Drei-Wochen-Ansicht aus VA-05 am 2026-09-15 sichtbar geprüft und vollständig bestätigt.
- Ein Build ersetzt keinen WPF-Bedientest.
- Eine temporäre Testdatenbank ersetzt keine spätere Sicherungs- und Wiederherstellungsprüfung.
- Planung, Planabnahme, Excel und portable Windows-Ausgabe bleiben offene spätere Gates.
- Das lokale Referenzbild `dienstplan beispiel blank.jpeg` wurde am 2026-09-15 als reines Beispiel ohne personenbezogene oder reale Planungsdaten bestätigt. Es bleibt unversioniert und wird nicht als Fachdatenquelle übernommen.

## Berichtsschema nach jedem Schritt

Nach jedem Schritt werden kurz genannt:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. kritische Meldungen und konkreter Handlungsbedarf,
5. offene Gates, Risiken oder Blockaden,
6. Git-Status ohne automatisches Staging, Commit oder Push,
7. nächster minimaler Schritt,
8. Bitte um ausdrückliche Abnahme.

## Nächster minimaler Schritt

System 06 ist vollständig nachgewiesen und am 2026-09-15 ausdrücklich abgenommen: gesperrte Paketwiederherstellung, Build aller 13 Projekte mit 0 Warnungen und 0 Fehlern, 495 von 495 vorhandene Tests, Format-, Architektur-, Migrations-, Datenschutz-, Pfad- und Diff-Prüfung sind grün. Planning und Excel enthalten in diesem Ausbauzustand weiterhin bewusst keine Tests und melden jeweils den erwarteten Exitcode 8. Die Übergaben an Systeme 07 bis 13 sind abgeglichen; keine produktiven Daten-, Sicherungs- oder Exportdateien liegen im Repository. Das bestätigte Referenzbild bleibt unversioniert. Roadmap und Fragen-/Antwortendokument wurden nach `docs/roadmaps/completed` verschoben. Ein weiteres System ist dadurch nicht automatisch zur Implementierung freigegeben.
