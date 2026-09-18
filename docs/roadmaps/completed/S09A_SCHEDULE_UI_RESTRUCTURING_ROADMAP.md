# S09A – Dienstplan-Oberfläche neu ordnen und Bedienung vereinheitlichen

Status: Abgeschlossen; UI-01 bis UI-09 am 2026-09-18 vollständig geprüft und ausdrücklich abgenommen

Stand: 2026-09-18

## Zweck und Nutzen

Dieser kleine Zwischenschritt ordnet den vorhandenen WPF-Tab „Dienstplan“ so neu, dass die eigentliche Drei-Wochen-Planung möglichst viel sichtbaren Raum erhält. Häufige Rückmeldungen erscheinen platzneutral am oberen Rand. Bestätigungen blockieren als modales Panel vorübergehend die übrige Bedienung, statt dauerhaft Platz im Seitenaufbau zu belegen. Die bereits vorhandene manuelle Typ1-Eingabe wird direkt an das angeklickte Tagesfeld verlegt.

Der Schritt liegt innerhalb des noch offenen Systems 09 zwischen dem technisch funktionsfähigen, fachlich noch nicht angenommenen Generierungsstand und dem späteren S09B-Planungsqualitätsbericht. Er verändert weder die automatische Planung noch den Funktionsumfang des Qualitätsberichts, der späteren Konflikterklärung oder der manuellen Planbearbeitung.

## Voraussetzung und Freigabegrenze

- Der aktuelle System-09-Ausgangsstand muss technisch stabil sein: Die erneute sichtbare Generierung funktioniert, der vollständige Build und 982 automatische Prüfungen sind grün. Die Ergebnisqualität und die Primärlösung sind damit ausdrücklich noch nicht fachlich abgenommen.
- Der Generator wird während S09A als unveränderter Ausgangsstand eingefroren. S09A ändert weder Solver, Zielmatrix, Regelkatalog, Planning-Verträge, Persistenz noch fachliche Generierungsregeln.
- Der tatsächlich vorhandene Stand von `ScheduleOverviewView`, `ScheduleOverviewViewModel`, Generierungsansicht und Verwerfen-Ansicht wird vor der ersten Codeänderung erneut gelesen.
- Diese Roadmap wird vor jeder Implementierung ausdrücklich abgenommen.
- Bis diese Voraussetzungen erfüllt sind, werden keine Produktions- oder Testdateien dieses UI-Umbaus geändert.
- Die laufenden, noch nicht zusammengeführten Änderungen des aktuellen System-09-Arbeitsstands werden nicht überschrieben oder vorweggenommen.
- In UI-01 wird nur festgelegt, an welcher Stelle ein späterer S09B-Bericht grundsätzlich erreichbar sein kann. Berichtsinhalte, Berichtsverträge und Auswertungen werden nicht vorgezogen.

## Bestätigtes Zielbild

### Sichtbare Reihenfolge

```text
Dienstplan-Tab
├─ Kopfbereich und Zeitraumsteuerung
├─ ausgewählte Person / ausgewählter Tag
│  └─ Urlaub · Krankheit · Festes Frei · Leeren
├─ Drei-Wochen-Planung mit höchstmöglicher sichtbarer Höhe
├─ Planungsvorbereitung
│  ├─ Status, Bereitschaft, Historie und Änderungen
│  └─ AH-Laufoption und Vorbereiten/Aktualisieren
├─ automatische Plangenerierung und Vorschau
└─ vollständiges Verwerfen eines übernommenen automatischen Plans

Darüber, ohne eigenen Platz im Layout:
├─ vorübergehende Meldung am oberen Rand
└─ bei Bestätigungen ein modales Panel mit gesperrtem Hintergrund
```

### Priorität der Wochenplanung

- Die Drei-Wochen-Planung steht unmittelbar unter der Auswahl- und Aktionszeile.
- Sie wird nicht auf eine willkürlich kleine feste Höhe abgeschnitten.
- Bei der vorgesehenen Fenstergröße erhält sie so viel Höhe wie praktisch möglich.
- Vorbereitung und Generierung bleiben darunter erreichbar, dürfen der Wochenplanung aber nicht dauerhaft unnötig Höhe wegnehmen.
- Horizontales oder vertikales Scrollen bleibt erlaubt, wenn Fenstergröße oder Anzahl der Mitarbeitenden es erfordern. Scrollen darf keine Zeilen, Eingaben oder Aktionen unerreichbar machen.
- Die endgültige Verteilung von Abständen, Zeilenhöhen und Scrollbereichen wird schrittweise an der sichtbaren WPF-Anwendung optimiert. Ein automatischer Render-Test ersetzt dieses manuelle Gate nicht.

### Typ1-Dienst direkt am Tagesfeld

- Ausschließlich bei der vorhandenen Typ1-Person öffnet ein Klick auf ein bearbeitbares Tagesfeld eine am Feld verankerte aufklappbare Auswahl.
- Die Auswahl enthält ausschließlich die bereits durch die vorhandenen Anwendungsdaten zugelassenen Dienste aus tatsächlichem Bedarf.
- Die Auswahl eines Dienstes speichert ihn unmittelbar über den bestehenden Anwendungsablauf.
- Ein bereits eingetragener Typ1-Dienst wird in der Auswahl erkennbar vorausgewählt.
- Das getrennte Feld „Typ1-Dienst aus vorhandenem Bedarf“ sowie „Typ1 speichern“ und „Typ1 entfernen“ entfallen.
- Entfernen erfolgt einheitlich über „Leeren“ beziehungsweise die Taste `Entf` und benötigt das bestätigte modale Panel.
- Für andere Mitarbeitertypen öffnet sich keine Dienstauswahl. Deren manuelle Dienstplanung bleibt ausdrücklich System 11.
- `U`, `K`, rotes `X`, Bürokennzeichnung, vorhandene Sperren und automatisch erzeugte Inhalte behalten ihre bisherigen Regeln.

### Platzneutrale Meldungen

- Erfolgs- und Informationsmeldungen erscheinen oben im Dienstplan als kleine überlagernde Meldung und verschieben keine darunterliegenden Inhalte.
- Erfolgs- und Informationsmeldungen werden grün dargestellt, bleiben zunächst ungefähr vier Sekunden vollständig lesbar und blenden danach weich aus.
- Die genaue Dauer und Überblendzeit dürfen im sichtbaren Abnahmeschritt angepasst werden, ohne das fachliche Verhalten zu ändern.
- Eine neuere Meldung ersetzt eine noch sichtbare ältere Meldung. Ein älterer Ablauf darf die neuere Meldung anschließend nicht versehentlich ausblenden.
- Fehler erscheinen am selben platzneutralen Ort in eindeutig roter Darstellung, verschwinden aber nicht automatisch. Sie bleiben bis zur nächsten erfolgreichen Korrektur, bewussten Schließung oder einem passenden Zustandswechsel sichtbar.
- Meldungen sind zusätzlich textlich verständlich; Farbe allein vermittelt keine Bedeutung.
- Der Ablauf verwendet kein blockierendes Warten und blockiert den WPF-UI-Thread nicht.

### Modales Bestätigungspanel

- Alle Bestätigungen zu zerstörerischen Aktionen innerhalb des Dienstplan-Tabs verwenden dieselbe modale Darstellung.
- Dazu gehören mindestens das Leeren eines Tageswerts, das Entfernen eines Typ1-Dienstes und das vollständige Verwerfen eines übernommenen automatischen Plans.
- Das Panel erscheint über dem Dienstplan. Der Hintergrund wird visuell zurückgenommen und nimmt bis zur Entscheidung keine Maus-, Tastatur- oder Befehlsaktionen an.
- Das Panel nennt die konkrete Aktion und ihre Auswirkung verständlich und bietet „Abbrechen“ und „Bestätigen“.
- Der anfängliche Fokus liegt auf der sicheren Aktion „Abbrechen“.
- `Escape` bricht ab. `Enter` löst nur die aktuell fokussierte Schaltfläche aus.
- Der Tastaturfokus bleibt während der Bestätigung im Panel und kehrt danach sinnvoll in den Dienstplan zurück.
- Abbrechen verändert keine Daten. Fehler beim bestätigten Speichern oder Verwerfen werden anschließend als nicht automatisch verschwindende Fehlermeldung angezeigt.

## Umfang

- sichtbare Neuordnung des bestehenden Dienstplan-Tabs,
- schrittweise Verdichtung zugunsten der Wochenplanung,
- feldgebundene Typ1-Dienstauswahl,
- kleine platzneutrale Meldungsdarstellung für den Dienstplan,
- einheitliches modales Bestätigungspanel für alle dortigen zerstörerischen Aktionen,
- sichtbare Wochen- und Mitarbeitergruppierung anhand der bereits vorhandenen fachlichen Planungsrolle,
- minimale Ergänzung der Application-Anzeige-Momentaufnahme um diese vorhandene Planungsrolle,
- notwendige kleine Desktop-ViewModel- und Darstellungsbausteine,
- angepasste Application-, Desktop- und WPF-Renderprüfungen,
- abschließende sichtbare Prüfung mit ausschließlich synthetischen Daten.

## Nicht-Umfang

- keine neue fachliche Domain-, Application-, Planning-, Infrastructure- oder Datenbankfunktion; die vorhandene Planungsrolle wird nur zusätzlich in der Anzeige-Momentaufnahme bereitgestellt,
- keine Änderung fachlicher Zulässigkeits-, Bedarfs-, Typ1-, Abwesenheits- oder Generierungsregeln,
- keine neue Migration und kein neues Paket,
- keine manuelle Dienstzuweisung für Nicht-Typ1-Personen; diese gehört zu System 11,
- keine Konflikterklärung oder Lösungsvorschläge aus System 10,
- kein Planungsqualitätsbericht aus S09B und keine vorsorgliche Berichtsdatenstruktur,
- keine Änderung oder Optimierung des eingefrorenen Generators,
- keine Planversion, Abnahme oder Excel-Funktion aus den Systemen 12 und 13,
- kein anwendungsweites Redesign aller Tabs,
- keine automatische Neugenerierung nach einer manuellen Eingabe,
- keine Änderung der bestehenden atomaren Übernahme oder des vollständigen Verwerfens eines automatischen Plans.

## Architektur- und Dateigrenzen

- Die Änderung bleibt grundsätzlich in `Salztal.Dienstplanung.Desktop.Features.Scheduling` und den zugehörigen Desktop-Tests. Für die bestätigte Mitarbeitergruppierung darf die vorhandene Planungsrolle zusätzlich über `AvailabilityPeriodEmployeeSnapshot` projiziert und mit fokussierten Application-Tests abgesichert werden.
- `ScheduleOverviewView.xaml` beschreibt Reihenfolge, Überlagerungen, Darstellung und Bindings.
- ViewModels koordinieren ausschließlich UI-Zustand und vorhandene Application-Aktionen. Fachlogik oder Zulässigkeitsregeln werden nicht in die Oberfläche kopiert.
- Die vorhandene Typ1-Optionsquelle bleibt maßgeblich; sichtbare Namen oder Texte steuern keine fachliche Auswahl.
- Ein kleiner eigener Desktop-Baustein für vorübergehende Meldungen oder Bestätigungen ist zulässig, wenn er Duplikation verhindert und im Scheduling-Bereich bleibt.
- Rein visuelle Animation, Fokusübergabe oder Panel-Verhalten darf in eng begrenztem Code-behind liegen, sofern dort keine Anwendungs- oder Fachlogik entsteht.
- Zeitgesteuerte Meldungen müssen abbrechbar und deterministisch testbar sein; `.Wait()`, `.Result`, `Thread.Sleep` und blockierende Dispatcher-Aufrufe bleiben verboten.
- Konkrete Planning- oder Infrastructure-Adapter bleiben ausschließlich in `Desktop.Composition`. Für diesen Umbau wird dort voraussichtlich keine neue Abhängigkeit benötigt.

## Vorhandene Anker vor der S09A-Umsetzung erneut prüfen

- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewView.xaml`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewView.xaml.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewViewModel.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleCellViewModel.cs`
- `src/Salztal.Dienstplanung.Application/Availabilities/AvailabilityPeriodEmployeeSnapshot.cs`
- `src/Salztal.Dienstplanung.Application/Availabilities/AvailabilityPeriodSnapshotProjector.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ServiceManagementAssignmentEditorViewModel.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/AutomaticScheduleGenerationView.xaml`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/AutomaticScheduleResetView.xaml`
- zugehörige Dateien unter `tests/Salztal.Dienstplanung.Desktop.Tests/Features/Scheduling`
- `tests/Salztal.Dienstplanung.Application.Tests/Availabilities/GetAvailabilityPeriodQueryTests.cs`
- betroffene Architekturtests nur, falls sich öffentliche Desktop-Verträge oder Verdrahtung tatsächlich ändern

Die Liste ist keine Freigabe, derzeit laufende System-09-Dateien parallel zu bearbeiten. Der tatsächliche Stand nach der System-09-Abnahme ist verbindlich.

## Umsetzungsschritte

### UI-01 – Abgenommene System-09-Ausgangslage und Roadmap-Freigabe

Status: `[x]` – am 2026-09-17 ausdrücklich freigegeben, geprüft und abgenommen

Umfang:

- den dokumentierten technisch funktionsfähigen System-09-Ausgangsstand und die weiterhin offene Qualitätsabnahme prüfen,
- Arbeitsbaum und tatsächlichen Scheduling-Dateistand lesen,
- diese Roadmap gegen den abgeschlossenen Generierungs-, Vorschau- und Verwerfen-Ablauf prüfen,
- Überschneidungen oder inzwischen geänderte UI-Verträge vor Codeänderungen dokumentieren,
- die grundsätzliche spätere Einordnung des S09B-Berichts im Bedienablauf festlegen, ohne Inhalt oder Verträge vorwegzunehmen,
- ausdrücklich bestätigen, dass keine S09B-, System-10- oder System-11-Funktion vorgezogen und der Generator nicht verändert wird.

Prüfung:

- System-09-Roadmap und Fragenkatalog dokumentieren den technisch funktionsfähigen, fachlich noch nicht angenommenen Ausgangsstand und die bestätigte Reihenfolge S09A, S09B, gezielte Optimierung, AG-15 und AG-16,
- `MASTER_ROADMAP.md`, `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` nennen denselben tatsächlichen Übergang,
- `git status` und `git diff` sind gelesen; fremde oder nicht zusammengehörige Änderungen bleiben unangetastet,
- Ausgangs-Build und betroffene Desktop-Tests sind grün oder vorhandene Abweichungen sind konkret dokumentiert.

Abnahmebedingung:

- Der technisch stabile Ausgangsstand, die eingefrorene Generierung und die reine UI-Grenze sind bestätigt; der Auftraggeber gibt UI-02 ausdrücklich frei. Vorher wird kein Produktionscode dieses Zwischenschritts verändert.

Abgenommen und geprüft am 2026-09-17:

- Der Auftraggeber hat den Start dieser Roadmap sowie die Umsetzung von UI-01 und bei korrektem Ausgangsstand unmittelbar anschließend UI-02 ausdrücklich freigegeben.
- Leitdokumente, System-09-Roadmap, Fragenkatalog, Arbeitsbaum und der tatsächliche Stand von Dienstplan-, Generierungs-, Vorschau- und Verwerfen-Ansicht wurden vor der UI-Änderung gelesen.
- Der dokumentierte Übergang stimmt überein: 982 automatische Ausgangsprüfungen und die erneute sichtbare Generierung waren bestätigt; die Ergebnisqualität und weitere AG-14F-Gates bleiben offen.
- Ein isolierter vollständiger Solution-Build bestand mit 0 Warnungen und 0 Fehlern; alle 106 damaligen Desktop-Tests waren grün. Der normale Ausgabeordner war ausschließlich durch die bereits laufende Desktop-App gesperrt und wurde nicht verändert oder beendet.
- Der Generator, Planning-Verträge, Zielmatrix, Regelkatalog, Persistenz und fachliche Generierungsregeln bleiben während S09A unverändert. S09B, System 10 und System 11 werden nicht vorgezogen.
- Der spätere S09B-Bericht wird grundsätzlich direkt nach automatischer Generierung beziehungsweise Vorschau und vor dem vollständigen Verwerfen eines übernommenen automatischen Plans erreichbar sein. UI-01 führt dafür weder Platzhalter noch Berichtsinhalt oder technische Verträge ein.

### UI-02 – Reihenfolge und sichtbare Höhe der Wochenplanung

Status: `[x]` – am 2026-09-17 sichtbar geprüft und ausdrücklich abgenommen

Umfang:

- Auswahl- und Aktionszeile unmittelbar vor der Wochenplanung anordnen,
- Wochenplanung unmittelbar danach anzeigen,
- Planungsvorbereitung als eine gemeinsame Karte unterhalb der Wochenplanung anordnen,
- automatische Plangenerierung, Vorschau und vollständiges Verwerfen darunter erhalten,
- unnötige dauerhafte vertikale Flächen oberhalb der Wochenplanung reduzieren,
- Scroll- und Größenverhalten so gestalten, dass die Wochenplanung nicht willkürlich abgeschnitten wird.

Prüfung:

- WPF-Konstruktions- und Renderprüfungen für die neue Reihenfolge und vollständig auflösbare Bindings,
- sichtbare Prüfung bei der aktuellen Standardgröße `1160 × 760`, der vorhandenen Mindestgröße `900 × 600` und maximiertem Fenster,
- bei vielen synthetischen Personen bleiben alle Zeilen über nachvollziehbares Scrollen erreichbar,
- Vorbereitung und Generierung bleiben unterhalb der Tabelle erreichbar,
- keine überdeckten Schaltflächen, abgeschnittenen Texte oder unbedienbaren Scrollbereiche.

Abnahmebedingung:

- Die Service-Leitung bestätigt, dass die Wochenplanung sichtbar klar im Mittelpunkt steht und in der Höhe bereits sinnvoll verbessert ist. Weitere reine Abstands- oder Größenoptimierungen dürfen innerhalb UI-06 folgen.

Umgesetzt und automatisch geprüft am 2026-09-17:

- Zeitraumsteuerung und vorhandene Meldungen bleiben oben; Auswahl, Tagesaktionen und der noch bis UI-03 bestehende Typ1-Editor bilden direkt davor einen gemeinsamen Auswahlblock.
- Die Drei-Wochen-Planung folgt unmittelbar auf diesen Auswahlblock. Planungsvorbereitung, automatische Generierung samt Vorschau und das vollständige Verwerfen stehen danach in der bestätigten Reihenfolge.
- Ein gemeinsamer vertikaler Arbeitsbereich macht alle unteren Karten erreichbar. Die Tabelle besitzt nur noch ihren notwendigen horizontalen Scrollbereich; ihre Personenzeilen werden nicht durch eine willkürlich kleine feste Höhe oder einen zweiten konkurrierenden vertikalen Scrollbereich abgeschnitten.
- Drei neue WPF-Renderfälle prüfen `1160 × 760`, `900 × 600` und `1920 × 1080` mit 20 zusätzlichen synthetischen Personen. Sie sichern Reihenfolge, auflösbare Bindings, vollständige Personenzeilen und Erreichbarkeit der unteren Generierungskarte durch Scrollen.
- Der fokussierte Desktop-Build bestand mit 0 Warnungen und 0 Fehlern; alle 109 Desktop-Tests waren grün.
- Der vollständige Solution-Build bestand ebenfalls mit 0 Warnungen und 0 Fehlern. Alle 985 vorhandenen Tests aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; das weiterhin leere Excel-Testprojekt meldete wie dokumentiert den erwarteten Exitcode 8.
- Der neue Build wurde sichtbar gestartet. Eine automatisierte Erfassung der nativen WPF-Oberfläche war in dieser Umsetzungssitzung technisch nicht verfügbar; deshalb blieben Sichtprüfung und Abnahme der tatsächlichen Abstände, Größen und Bedienbarkeit zunächst offen.
- Die Service-Leitung hat die sichtbare Zwischenprüfung am 2026-09-17 ausdrücklich abgenommen.

### UI-03 – Typ1-Dienstauswahl direkt am Feld

Status: `[x]` – umgesetzt, automatisch und sichtbar geprüft sowie am 2026-09-17 ausdrücklich abgenommen

Umfang:

- vorhandene Typ1-Optionen am ausgewählten Tagesfeld aufklappbar anzeigen,
- vorhandene Auswahl kenntlich machen,
- ausgewählten Dienst unmittelbar über den bestehenden Speichervorgang sichern,
- getrennte Typ1-Auswahlkarte und ihre Speicher-/Entfernen-Schaltflächen entfernen,
- „Leeren“ und `Entf` auf das einheitliche Bestätigungspanel aus UI-05 vorbereiten,
- Nicht-Typ1-Felder von dieser Dienstauswahl ausschließen.

Prüfung:

- ViewModel-Tests für zulässige Optionen, vorhandene Auswahl, unmittelbares Speichern, Fehler und Neuladen,
- Gegenprüfung: Nicht-Typ1, gesperrte, automatisch geschützte oder nicht bearbeitbare Zellen öffnen keine unzulässige Auswahl,
- WPF-Render- und Bedienprüfung mit Maus und Tastatur einschließlich Öffnen, Navigation, Auswahl und Abbruch,
- bestehende `U`-/`K`-/`X`- und Typ1-Regressionsprüfungen bleiben grün.

Abnahmebedingung:

- Ein vorhandener zulässiger Typ1-Dienst lässt sich direkt am Tagesfeld auswählen und speichern; andere Mitarbeitende erhalten dadurch keine vorgezogene manuelle Dienstplanung.

Umgesetzt und automatisch geprüft am 2026-09-17:

- Ein Klick auf ein bearbeitbares Typ1-Tagesfeld öffnet die vorhandenen zulässigen Dienstoptionen direkt unter diesem Feld. Die bisherige getrennte Typ1-Auswahlkarte samt eigener Speichern- und Entfernen-Schaltfläche ist entfernt.
- Jede Option speichert unmittelbar über den unveränderten Application-Speicherbefehl. Nach dem Neuladen wird die gespeicherte Option im Feldmenü als aktuell ausgewählt markiert.
- `U`, `K` oder rotes `X` werden weiterhin erst nach dem bestehenden sichtbaren Bestätigungsschritt durch einen Typ1-Dienst ersetzt. `Leeren` und `Entf` verwenden unverändert den vorhandenen Bestätigungsablauf als Vorbereitung für UI-05.
- Nicht-Typ1-Felder, automatisch erzeugte schwarze `X`, automatisch erzeugte oder sonst nicht über den Typ1-Ablauf bearbeitbare Einteilungen und global gesperrte Zustände öffnen keine unzulässige Dienstauswahl.
- Neue ViewModel- und WPF-Prüfungen decken zulässige Optionen, aktuelle Auswahl, Sofortspeicherung, Neuladen, Konfliktmeldung, Bestätigung bei vorhandenem Tageswert, Feldanker, direkte Auswahl und Abbruch mit `Escape` ab. Die vorhandenen Render-, Größen-, `U`-/`K`-/`X`- und Typ1-Prüfungen bleiben grün.
- Der vollständige Solution-Build bestand mit 0 Warnungen und 0 Fehlern. Alle 989 vorhandenen Tests aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; das weiterhin leere Excel-Testprojekt meldete den dokumentierten Exitcode 8. Die 113 Desktop-Tests sind grün.
- Die geänderten C#-Dateien bestehen die fokussierte Formatprüfung. Die sichtbare Bedienprüfung des tatsächlichen Popups mit Maus und Tastatur bleibt als Abnahmegate offen.
- Die Service-Leitung hat die sichtbare Bedienung am 2026-09-17 ausdrücklich abgenommen.

### UI-04 – Platzneutrale Meldungen

Status: `[x]` – umgesetzt, automatisch und sichtbar geprüft sowie am 2026-09-17 ausdrücklich abgenommen

Umfang:

- bisherige platzbelegende Erfolgs- und Fehlermeldungsfelder im Dienstplan ersetzen,
- Erfolgs- und Informationsmeldungen oben grün überlagert anzeigen und weich ausblenden,
- Fehler am selben Ort rot und dauerhaft anzeigen,
- neuere Meldungen sicher vor verspätetem Ausblenden durch ältere Zeitabläufe schützen,
- sinnvolle Automation-Namen und textliche Statusbedeutung ergänzen.

Prüfung:

- deterministische Tests für Anzeigen, Ersetzen, Zeitablauf, Abbruch eines alten Ablaufs und dauerhaft sichtbare Fehler,
- keine Meldung verschiebt Wochenplanung, Vorbereitung oder Generierung,
- schneller Folgeablauf mehrerer gespeicherter Tageswerte lässt stets die neueste Meldung korrekt stehen,
- sichtbare Prüfung von Einblenden, Lesedauer, Ausblenden, Farbe, Kontrast und Textverständlichkeit,
- kein blockierendes Warten auf dem UI-Thread.

Abnahmebedingung:

- Erfolgsmeldungen sind gut lesbar, verschwinden anschließend weich und verbrauchen keinen dauerhaften Platz; Fehler gehen nicht durch automatisches Ausblenden verloren.

Umgesetzt und automatisch geprüft am 2026-09-17:

- Erfolgs- und Informationsmeldungen erscheinen als grüne, nicht klickblockierende Überlagerung oben im Dienstplan. Sie bleiben vier Sekunden lesbar und werden anschließend über 350 Millisekunden weich ausgeblendet, ohne Wochenplanung oder nachfolgende Karten zu verschieben.
- Fehler verwenden denselben Platz mit rotem Hintergrund und bleiben sichtbar, bis eine neue Aktion die Rückmeldung ersetzt oder bewusst entfernt. Die sichtbaren Wörter `Hinweis` und `Fehler`, der vollständige Meldungstext sowie ein passender Automation-Name vermitteln den Zustand unabhängig von der Farbe.
- Eine eigene abbrechbare Zeitsteuerung mit Meldungsfolge schützt neuere Rückmeldungen davor, durch einen verspätet beendeten älteren Ablauf ausgeblendet zu werden. Der Ablauf verwendet ausschließlich asynchrones Warten und blockiert den UI-Thread nicht.
- Deterministische ViewModel-Tests prüfen Lesedauer, Ausblendphase, Ersetzen, einen absichtlich verspätet abgeschlossenen älteren Ablauf und dauerhaft sichtbare Fehler. Ein WPF-Test prüft Overlay-Ebene, fehlende Klickblockade, grüne und rote Zustände, Automation-Live-Meldung sowie unveränderte Position des Arbeitsbereichs bei sichtbarer, verborgener und fehlerhafter Rückmeldung.
- Der vollständige Solution-Build im separaten Ausgabeverzeichnis bestand mit 0 Warnungen und 0 Fehlern. Alle 993 vorhandenen Tests aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; davon sind 117 Desktop-Tests grün. Das weiterhin leere Excel-Testprojekt enthält unverändert keine Tests.
- Die geänderten C#-Dateien bestehen die fokussierte Formatprüfung. Die sichtbare Prüfung von Lesbarkeit, Farbe, Kontrast, tatsächlicher Ausblendbewegung und Verhalten bei den unterstützten Fenstergrößen bleibt als Abnahmegate offen.
- Die Service-Leitung hat die sichtbare Darstellung am 2026-09-17 ausdrücklich abgenommen.

### UI-05 – Einheitliches modales Bestätigungspanel

Status: `[x]` – umgesetzt, automatisch und sichtbar geprüft sowie am 2026-09-18 ausdrücklich abgenommen

Umfang:

- eine gemeinsame modale Bestätigungsdarstellung über dem Dienstplan bereitstellen,
- bestehende Bestätigungen für Tageswert, Typ1-Dienst und vollständiges automatisches Verwerfen darauf umstellen,
- Hintergrundinteraktion, Tastenkürzel und widersprüchliche Befehle während der Bestätigung sperren,
- sicheren Anfangsfokus, `Escape`, Fokusbindung und Fokuswiederherstellung umsetzen,
- bestehende fachliche Meldungstexte und Folgen der jeweiligen Aktion erhalten.

Prüfung:

- ViewModel-Tests für Öffnen, Bestätigen, Abbrechen, Fehler und unveränderten Zustand nach Abbruch,
- Tests, dass Hintergrundbefehle während des Panels nicht ausgeführt werden,
- WPF-Render- und Tastaturtests für Fokus, `Escape`, Schaltflächen, Automation-Namen und nicht nur farblich vermittelte Modalität,
- vollständiges Verwerfen nennt weiterhin betroffene automatische Einteilungen und schwarze `X` sowie erhaltene Inhalte,
- Zellleeren und Typ1-Entfernen verändern vor der Bestätigung nichts.

Abnahmebedingung:

- Alle zerstörerischen Bestätigungen im Dienstplan erscheinen einheitlich als modales Panel und lassen bis zur Entscheidung keine widersprüchliche Bedienung zu.

Umgesetzt und automatisch geprüft am 2026-09-17:

- Änderungen und Löschungen an Tagesfeldern, das Ersetzen und Entfernen von Typ1-Diensten sowie das vollständige Verwerfen eines übernommenen automatischen Plans verwenden dieselbe zentrale Overlay-Darstellung über dem gesamten Dienstplan. Die beiden früheren eingebetteten Bestätigungsfelder sind entfernt.
- Das Panel benennt die notwendige Bestätigung ausdrücklich, zeigt unverändert den jeweiligen fachlichen Warntext und verwendet für das vollständige Verwerfen weiterhin die genaue Zahl automatischer Einteilungen und schwarzer `X` sowie die Liste der erhaltenen Inhalte.
- Der abgedunkelte Hintergrund fängt Mausinteraktionen ab. Zeitraum, Tagesaktionen, Planungsvorbereitung, Generierung und Verwerfen erhalten während einer Entscheidung keinen ausführbaren widersprüchlichen Befehl; vorhandene Tastenkürzel bleiben ebenfalls gesperrt.
- `Abbrechen` erhält beim Öffnen den sicheren Anfangsfokus. Der Panelbereich führt Tabulator- und Richtungstasten zyklisch, `Escape` bricht ab und nach Schließen wird der vorherige Fokus wiederhergestellt. Logischer und realer WPF-Fokus werden gemeinsam gesetzt, damit das Verhalten auch bei geöffneten Popups und im sichtbaren Fenster stabil bleibt.
- ViewModel-Tests prüfen beide zentral weitergeleiteten Bestätigungsarten, Abbruch ohne Schreibzugriff, gesperrte Hintergrundbefehle, unveränderte Tageswerte und Typ1-Zuweisungen sowie den unveränderten Warntext des vollständigen Verwerfens. Ein gemeinsamer WPF-Test prüft dieselbe Overlay-Instanz, sicheren Fokus, Fokusbindung, `Escape`, Fokuswiederherstellung, Schaltflächen, Automation-Namen und die textlich erklärte Modalität.
- Der vollständige Solution-Build im separaten Ausgabeverzeichnis bestand mit 0 Warnungen und 0 Fehlern. Alle 997 vorhandenen Tests aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; davon sind 121 Desktop-Tests grün. Das weiterhin leere Excel-Testprojekt enthält unverändert keine Tests.
- Die geänderten C#-Dateien bestehen die fokussierte Formatprüfung.
- Die Service-Leitung hat die sichtbare Bedienung aller Bestätigungsarten mit Maus und Tastatur am 2026-09-18 ausdrücklich bestätigt.

### UI-06 – Vertikale Grenzen der drei Kalenderwochen

Status: `[x]` – technisch, automatisch und sichtbar geprüft sowie am 2026-09-18 ausdrücklich abgenommen

Umfang:

- eine stärkere vertikale Linie vor dem ersten Montag, zwischen erstem Sonntag und zweitem Montag, zwischen zweitem Sonntag und drittem Montag sowie nach dem dritten Sonntag darstellen,
- alle vier Grenzen durch Tabellenkopf und sämtliche Mitarbeiterzeilen führen,
- vorhandene Tagesbreiten, Kalenderzuordnung, horizontales Scrollen und Wochensummen unverändert lassen.

Prüfung:

- WPF-Renderprüfungen finden genau die vier Grenzen vor Tag 1, zwischen Tag 7 und 8, zwischen Tag 14 und 15 sowie nach Tag 21,
- Tabellenkopf und jede synthetische Mitarbeiterzeile verwenden dieselben Grenzpositionen,
- Standardgröße, Mindestgröße und maximiertes Fenster behalten ausgerichtete Spalten und den vorhandenen horizontalen Scrollbereich.

Umgesetzt und automatisch geprüft am 2026-09-18:

- Vier nicht interaktive, drei Pixel starke Linien überlagern die unveränderte Tabellenstruktur gemeinsam über Kopf und sämtliche Mitarbeiterzeilen. Sie liegen vor Tag 1, nach Tag 7, nach Tag 14 und nach Tag 21; Zellbreiten, Auswahlrahmen, Wochensummen und Scrolllogik bleiben unverändert.
- Ein WPF-Render-Test prüft die exakt vier Grenzpositionen `226`, `604`, `982` und `1360`, ihre vollständige Tabellenhöhe und den unveränderten horizontalen Scrollbereich bei `900 × 600`, `1160 × 760` und `1920 × 1080`.
- Der vollständige Solution-Build bestand mit 0 Warnungen und 0 Fehlern. Alle 1001 automatischen Prüfungen aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; davon sind 125 Desktop-Tests grün.
- Die vollständige Formatprüfung und `git diff --check` sind grün. Die gemeinsame sichtbare Bewertung von Stärke und Farbe erfolgte wie vorgesehen in UI-09.
- Die Service-Leitung hat die vier sichtbaren Wochengrenzen nach einem aktuellen Debug-Build am 2026-09-18 ausdrücklich bestätigt. Die gemeinsame Feinbeurteilung mit den weiteren Tabellenänderungen wurde in UI-09 ebenfalls abgenommen.

Abnahmebedingung:

- Die vier Wochengrenzen sind technisch durchgängig umgesetzt, automatisch geprüft und in UI-09 gemeinsam sichtbar abgenommen.

### UI-07 – Typ1 immer als erste Mitarbeiterzeile

Status: `[x]` – technisch, automatisch und sichtbar geprüft sowie am 2026-09-18 ausdrücklich abgenommen

Umfang:

- die vorhandene fachliche Planungsrolle über die unveränderliche Application-Anzeige-Momentaufnahme bereitstellen,
- die aktive Person mit der Rolle `ServiceManagement` immer als erste Mitarbeiterzeile darstellen,
- ohne aktive Typ1-Person die vorhandene Namenssortierung der übrigen Personen beibehalten,
- weder Typcode noch Name in der UI zur Erkennung von Typ1 auswerten.

Prüfung:

- Application-Tests sichern die unveränderte Übergabe der Planungsrolle,
- ViewModel-Tests sichern Typ1 an erster Stelle unabhängig vom Namen sowie den Fall ohne Typ1,
- bestehende Typ1-Auswahl, Tageswerte, Wochensummen und fachliche Befehle bleiben unverändert.

Umgesetzt und automatisch geprüft am 2026-09-18:

- Die vorhandene fachliche Planungsrolle wird über `AvailabilityPeriodEmployeeSnapshot` als unveränderlicher `EmployeeTypePlanningRoleKind` bereitgestellt. Ein gemeinsamer Application-Mapper verhindert doppelte Übersetzungslogik zwischen Domain- und Anzeige-Rolle.
- Der Dienstplan ordnet ausschließlich Personen mit `ServiceManagement` stabil vor die bereits nach Nachname, Vorname und Kennung sortierten übrigen Personen. Typcode und sichtbarer Name werden dafür nicht ausgewertet; ohne aktive Typ1-Person bleibt die vorhandene Namensreihenfolge unverändert.
- Fokussierte Application-Tests prüfen die Projektion von `Normal`, `ServiceManagement` und `Auxiliary`. Desktop-ViewModel-Tests prüfen Typ1 vor einer alphabetisch früheren Person und den unveränderten Fall ohne Typ1.
- Release- und Debug-Build bestanden mit 0 Warnungen und 0 Fehlern. Alle 1004 automatischen Prüfungen bestanden; davon sind 127 Desktop-Tests grün. Die vollständige Formatprüfung und `git diff --check` sind grün.
- Die Service-Leitung hat Typ1 als erste Mitarbeiterzeile am 2026-09-18 ausdrücklich bestätigt. Die gemeinsame sichtbare Beurteilung aller Gruppen wurde in UI-09 ebenfalls abgenommen.

Abnahmebedingung:

- Eine vorhandene aktive Typ1-Person steht technisch immer in der ersten Zeile; die gemeinsame sichtbare Beurteilung ist in UI-09 erfolgt.

### UI-08 – Reguläre Personen vor AH und horizontale AH-Grenze

Status: `[x]` – technisch, automatisch und sichtbar geprüft sowie am 2026-09-18 ausdrücklich abgenommen

Umfang:

- nach Typ1 zuerst Personen mit der Rolle `Normal` und danach Personen mit der Rolle `Auxiliary` anzeigen,
- innerhalb der regulären und der AH-Gruppe die vorhandene Sortierung nach Nachname, Vorname und stabiler Kennung beibehalten,
- den Beginn des AH-Blocks mit einer stärkeren horizontalen Linie über Namensspalte, 21 Tagesfelder und drei Wochensummen kennzeichnen,
- die Linie auch zwischen Typ1 und AH anzeigen, wenn keine reguläre Person vorhanden ist; ohne AH-Gruppe keine leere Trennlinie darstellen,
- weder Typcodes noch Namen in der UI zur Erkennung von AH auswerten.

Prüfung:

- ViewModel-Tests sichern die Reihenfolge `Typ1 → regulär → AH` und die vorhandene Namenssortierung innerhalb der beiden Gruppen,
- Grenzfälle ohne reguläre Personen, ohne Typ1 beziehungsweise ohne AH sind abgedeckt,
- WPF-Renderprüfungen sichern genau eine durchgängige AH-Grenze an der korrekten Zeile.

Umgesetzt und automatisch geprüft am 2026-09-18:

- Der Dienstplan ordnet stabil nach der vorhandenen fachlichen Rolle in der Reihenfolge `ServiceManagement → Normal → Auxiliary`. Die bereits in Application festgelegte Sortierung nach Nachname, Vorname und Kennung bleibt innerhalb der regulären und AH-Gruppe erhalten; Typcodes und Namen steuern die Gruppierung nicht.
- Ausschließlich die erste AH-Zeile trägt ein eigenes Darstellungsmerkmal. XAML legt dort eine nicht interaktive, drei Pixel starke Linie über die vollständigen 1690 Pixel aus Namensspalte, 21 Tagesfeldern und drei Wochensummen; Zeilenhöhe, Zellbedienung, Wochenlinien und Scrollbereich bleiben unverändert.
- ViewModel-Tests prüfen die vollständige Gruppen- und Namensreihenfolge sowie die Fälle ohne reguläre Personen, ohne Typ1 und ohne AH. WPF-Renderprüfungen sichern genau eine sichtbare Linie an der ersten AH-Zeile beziehungsweise keine Linie ohne AH.
- Release- und Debug-Build bestanden mit 0 Warnungen und 0 Fehlern. Alle 1010 automatischen Prüfungen bestanden; davon sind 133 Desktop-Tests grün. Die vollständige Formatprüfung und `git diff --check` sind grün.
- Die Service-Leitung hat die Reihenfolge `Typ1 → regulär → AH` und die durchgängige horizontale Grenze vor dem AH-Block im Rahmen der gemeinsamen Sichtprüfung am 2026-09-18 ausdrücklich bestätigt.

Abnahmebedingung:

- Gruppenreihenfolge und AH-Grenze sind technisch umgesetzt, automatisch geprüft und gemeinsam sichtbar abgenommen.

### UI-09 – Gemeinsame Sichtprüfung, Höhenoptimierung und Abschluss

Status: `[x]` – vollständig umgesetzt, automatisch geprüft und am 2026-09-18 gemeinsam sichtbar abgenommen

Umfang:

- alle neuen UI-Bausteine gemeinsam mit dem abgeschlossenen System-09-Ablauf prüfen,
- vertikale Mausradbewegungen über dem ausschließlich horizontal scrollbaren Wochenplan an den gemeinsamen vertikalen Arbeitsbereich weitergeben,
- die Ergebnisse aus UI-06 bis UI-08 gemeinsam sichtbar beurteilen,
- Abstände, Linienwirkung, Zeilenhöhen und verfügbare Planhöhe in kleinen sichtbaren Iterationen optimieren,
- ausschließlich visuelle Feinabstimmungen innerhalb des bestätigten Umfangs vornehmen,
- Dokumentation und Projektstatus nach tatsächlichem Ergebnis aktualisieren,
- Roadmap erst nach ausdrücklicher Abnahme nach `docs/roadmaps/completed` verschieben.

Prüfung:

- vollständiger Build ohne neue Warnungen oder Fehler,
- betroffene Application-, Desktop- und Architekturtests sowie vollständiger Regressionstest,
- `dotnet format ... --verify-no-changes --no-restore` und `git diff --check`,
- sichtbarer Gesamtablauf mit synthetischen Daten: Zeitraum öffnen, Zelle auswählen, `U/K/X` speichern, Typ1-Dienst direkt wählen, Leeren abbrechen und bestätigen, Planung vorbereiten, Plan erzeugen, Vorschlag behandeln und vollständiges Verwerfen abbrechen und bestätigen,
- Wochengrenzen, Typ1-Erstplatzierung, regulärer Block und AH-Block bei Standardgröße, Mindestgröße und maximiertem Fenster prüfen,
- Mausrad über Tabelle und übriger Seite, horizontalen Tabellen-Scrollbalken und eigenen Scrollbereich des Typ1-Popups gegenprüfen,
- Datenschutzprüfung: keine echten Namen, Pläne, Datenbanken, Sicherungen oder Exporte im Repository.

Vorgezogene Scrollkorrektur umgesetzt, automatisch und sichtbar geprüft am 2026-09-18:

- Der gemeinsame vertikale Arbeitsbereich und der ausschließlich horizontale Scrollbereich der Drei-Wochen-Planung besitzen eindeutige Namen. Vertikale Mausradbewegungen über Tabellenzellen, Tabellenköpfen und Summen verwenden nun den äußeren Arbeitsbereich; der horizontale Tabellenversatz bleibt unverändert.
- Die Weiterleitung gilt nur für den Wochenplan. Das getrennte Typ1-Popup behält seinen eigenen vertikalen Scrollbereich. Die Windows-Einstellung für Zeilen- beziehungsweise Seitenscrollen wird verwendet; feste Pixelabstände wurden nicht eingeführt.
- Ein neuer WPF-Ereignistest prüft beide Scrollrichtungen, unveränderten horizontalen und inneren vertikalen Offset sowie obere und untere Grenze. Die bestehenden Renderfälle für `900 × 600`, `1160 × 760` und `1920 × 1080` bleiben grün.
- Der vollständige Solution-Build bestand mit 0 Warnungen und 0 Fehlern. Alle 998 vorhandenen Tests aus Domain, Application, Planning, Infrastructure, Desktop und Architektur bestanden; davon sind 122 Desktop-Tests grün. Das weiterhin leere Excel-Testprojekt meldete den dokumentierten Exitcode 8.
- Die Service-Leitung hat das vertikale Scrollen des vollständigen Tabs über der Wochenplanung am 2026-09-18 sichtbar bestätigt. Der horizontale Tabellen-Scrollbalken bleibt erhalten.

Gemeinsame Sichtprüfung und Abschluss am 2026-09-18:

- Die Service-Leitung hat die gesamte Sichtprüfung ausdrücklich abgenommen. Bestätigt sind damit die Wochengrenzen, die Typ1-Erstplatzierung, die Reihenfolge der regulären und AH-Personen, die horizontale AH-Grenze sowie das durchgängige Scrollverhalten des Dienstplan-Tabs.
- Die bereits sichtbaren UI-01- bis UI-05-Ergebnisse, der horizontale Tabellen-Scrollbalken und der eigene Scrollbereich der Typ1-Auswahl bleiben Bestandteil der abgenommenen Gesamtdarstellung. Weitere Höhen-, Linien- oder Abstandskorrekturen waren nicht erforderlich.
- Der reine UI-Zwischenschritt S09A ist damit abgeschlossen. Der Generator und seine Ergebnisqualität wurden in S09A nicht verändert oder fachlich abgenommen; System 09 bleibt bis S09B, dem daraus abgeleiteten Folgeplan sowie AG-15 und AG-16 offen.

Abnahmebedingung:

- Die Service-Leitung bestätigt Reihenfolge, Wochengrenzen, AH-Grenze, möglichst große sichtbare Wochenplanung, feldgebundene Typ1-Auswahl, platzneutrale Meldungen und alle modalen Bestätigungen. Erst danach wird der Zwischenschritt abgeschlossen; anschließend wird der Fragenkatalog für den S09B-Planungsqualitätsbericht erstellt. System 09 bleibt bis Bericht, gezielter Optimierung, AG-15 und AG-16 offen.

## Echte manuelle Gates

- ausdrückliche Abnahme dieser Roadmap vor UI-01 beziehungsweise jeder Codeänderung,
- nachweislich technisch stabiler und während S09A unveränderter Generierungsstand vor UI-02,
- sichtbare Zwischenabnahme der Tabellenhöhe und Reihenfolge in UI-02,
- sichtbare Bedienabnahme des Gesamtumbaus in UI-09,
- eigener Fragenkatalog und danach eigener Roadmap-Entwurf für S09B nach Abschluss dieses Zwischenschritts.

## Risiken und Schutzmaßnahmen

| Risiko | Schutzmaßnahme |
|---|---|
| Laufende System-09-Änderungen werden überschrieben | vor UI-02 Ausgangslage und Arbeitsbaum erneut lesen; S09A strikt auf die bestätigten Desktop-Dateien begrenzen |
| UI-Umbau verändert unbeabsichtigt die Ergebnisqualität | Generator, Planning-Verträge, Zielmatrix, Regeln und Persistenz während S09A einfrieren; vollständige Regression ausführen |
| Der spätere Bericht erzwingt einen zweiten UI-Umbau | in UI-01 nur seine grundsätzliche Position festlegen; Inhalte und vorsorgliche Abstraktionen ausdrücklich vermeiden |
| Die Tabelle erhält durch eine feste Höhe zu wenig Raum | keine willkürlich kleine feste Höhe; Prüfung in drei Fensterzuständen und sichtbare Iteration |
| Untere Karten werden durch eine sehr lange Tabelle unerreichbar | bewusstes, getestetes Scrollkonzept für den gesamten Inhalt und die Tabelle |
| Typ1-Auswahl dupliziert Fachregeln | ausschließlich vorhandene strukturierte Optionen und bestehende Application-Aktion verwenden |
| Alte Zeitsteuerung blendet eine neue Meldung aus | jeden alten Ablauf abbrechen beziehungsweise mit einer Meldungsgeneration absichern |
| Fehler verschwinden unbemerkt | Fehler nicht automatisch ausblenden und textlich eindeutig kennzeichnen |
| Modales Panel wirkt nur optisch modal | Befehle und Eingaben zusätzlich im ViewModel sperren; Fokus und Tastatur testen |
| `Popup` verliert Fokus oder bleibt offen | Öffnen, Auswahl, Abbruch, Zellwechsel und Neuladen mit WPF-Bedientests absichern |
| System 11 wird vorgezogen | feldgebundene Dienstauswahl strikt auf Typ1 begrenzen |
| System 10 wird vorgezogen | keine neuen Konfliktursachen oder Lösungsvorschläge in diesem Schritt |

## Definition des abgeschlossenen Zwischenschritts

Der UI-Umbau ist erst zur Abnahme bereit, wenn:

- der technisch funktionsfähige System-09-Ausgangsstand bestätigt und während S09A fachlich unverändert geblieben ist,
- alle neun Schritte im freigegebenen Umfang umgesetzt und geprüft sind,
- die Wochenplanung in der bestätigten Reihenfolge möglichst viel sichtbare Höhe erhält,
- Typ1-Dienste ausschließlich aus zulässigen vorhandenen Optionen direkt am Feld gespeichert werden,
- Meldungen keinen dauerhaften Platz belegen und Fehler nicht automatisch verloren gehen,
- alle zerstörerischen Bestätigungen tatsächlich modal und tastaturbedienbar sind,
- die vier Wochengrenzen und der Beginn des AH-Blocks über die vollständige Tabelle klar erkennbar sind,
- Typ1, reguläre Personen und AH in der bestätigten Gruppenreihenfolge erscheinen und die Namenssortierung innerhalb der regulären und AH-Gruppe erhalten bleibt,
- keine Fachlogik in XAML, Code-behind oder ViewModels dupliziert wurde,
- Build, relevante Tests, Format- und Datenschutzprüfung grün sind,
- das sichtbare WPF-Gate ausdrücklich bestätigt wurde,
- Dokumentation und Roadmap-Status den tatsächlichen Stand wiedergeben.

Abgeschlossen und archiviert wird die Roadmap erst nach ausdrücklicher Abnahme. Ein erfolgreicher Build oder Render-Test ersetzt die sichtbare Bedienprüfung nicht.

## Nächster minimaler Schritt

S09A ist vollständig umgesetzt, geprüft, sichtbar abgenommen und archiviert. Der nächste minimale Schritt innerhalb des weiterhin offenen Systems 09 ist ein eigener Fragenkatalog für S09B. Er klärt Inhalt und Abnahmebedingungen des neutralen Planungsqualitätsberichts, bevor eine S09B-Roadmap oder weitere Implementierung beginnt.
