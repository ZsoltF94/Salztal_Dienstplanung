# Aktueller Stand

Stand: 13. September 2026

## Wo steht das Projekt?

Das erste fachliche System mit Einsatzorten und Diensttypen ist abgeschlossen. Als Nächstes wird die Grundlage für Mitarbeitende, Arbeitszeitmodelle und Qualifikationen vorbereitet; eine eigentliche Dienstplanung wurde noch nicht gebaut.

Die Wünsche und wichtigsten Grundlagen der Service-Leitung wurden gemeinsam besprochen, aufgeschrieben und abgenommen. Das Projekt ist außerdem mit einem GitHub-Repository verbunden, damit der Entwicklungsstand nachvollziehbar gespeichert werden kann.

## Fertig und abgenommen

- Das grundlegende Ziel der App ist geklärt.
- Die App ist für eine Service-Leitung und einen Klinikstandort vorgesehen.
- Die wichtigsten Entscheidungen zur Wochenplanung sind dokumentiert.
- Änderungen am Projekt können nachvollziehbar gespeichert werden.
- Das Projekt ist mit dem vorgesehenen GitHub-Repository verbunden.
- Der leicht verständliche Lesebereich für die Service-Leitung ist eingerichtet.
- Die technische Architektur für die Windows-11-App ist festgelegt.
- Oberfläche, lokale Datenspeicherung, automatische Planung und Excel-Ausgabe werden als getrennte Bausteine entwickelt.
- Die App soll zuerst als entpackbarer Ordner bereitgestellt werden. Ob auf dem Klinikrechner noch eine kleine Windows-Zusatzkomponente benötigt wird, muss später praktisch geprüft werden.
- Verbindliche Qualitätsregeln für verständlichen, wartbaren und prüfbaren Programmcode sind festgelegt.
- Klare Arbeitsanweisungen schreiben kleine einzeln abnehmbare Schritte, ehrliche Prüfberichte und den Schutz vorhandener Änderungen vor.
- Eine abgenommene Gesamtübersicht ordnet alle geplanten Systeme und zeigt ihre wichtigsten Vorarbeiten.
- Für jedes System muss vor seiner Umsetzung zusätzlich eine eigene kleinschrittige Roadmap erstellt und abgenommen werden.
- Eine zentrale Statusseite zeigt den aktiven Arbeitsschritt, spätere offene Entscheidungen, Blockaden und ausstehende Prüfungen an einer Stelle.
- Alle Leitdokumente wurden gemeinsam geprüft und kleinere Unklarheiten berichtigt.
- Die gesamte Projektgrundlage ist abgenommen und abgeschlossen.
- Die kleinschrittige Roadmap für das technische App-Grundgerüst ist geprüft und bestätigt.
- Die zentrale technische Grundlage mit leerer Projektmappe und gemeinsamen Entwicklungsregeln ist bestätigt.
- Die beiden inneren, noch funktionslosen App-Bausteine für spätere Fachregeln und Anwendungsabläufe sind bestätigt.
- Die drei getrennten, noch funktionslosen technischen Bausteine für Planung, Speicherung und Excel-Verarbeitung sind bestätigt.
- Die leere Windows-Oberfläche mit dem Namen „Salztal Dienstplanung“ ist bestätigt. Sie enthält noch keine Dienstplanfunktion.
- Die vorgesehenen technischen Zusatzpakete sind geprüft, ihren Bausteinen zugeordnet und bestätigt.
- Für jeden technischen App-Baustein sowie für die Architektur ist ein eigener Testbereich angelegt und bestätigt.
- Zwölf automatische Architekturprüfungen und ihre Erkennung von drei kontrollierten Fehlerarten sind bestätigt.
- Das gesamte technische App-Grundgerüst einschließlich des sichtbaren lokalen Starttests ist bestätigt und abgeschlossen.
- Die sichtbare Einsatzortverwaltung mit Auswahl, Name und Farbkennzeichnung ist geprüft und bestätigt.
- Die sichtbare Verwaltung der Diensttyp-Standardzeiten sowie die Erklärungen für Doppeldienst und Springer sind geprüft und bestätigt.
- System 04 mit Einsatzorten, normalen Diensttypen, Doppeldienst und Springer ist vollständig geprüft, bestätigt und archiviert.

## System 04 – umgesetzt und abgenommen

- Als erstes Fachsystem wurden Einsatzorte, Diensttypen und Doppeldienste umgesetzt.
- Der kleinschrittige Roadmap-Entwurf sowie das geprüfte Fachmodell und der erste Bedienumfang sind bestätigt.
- ED-03 mit Cafeteria und Restaurant als ersten internen Einsatzorten ist bestätigt.
- Weitere Einsatzorte und Farben bleiben technisch möglich. Leere Namen, fehlende Farben und ungültige Kennungen werden als verständlich übersetzbare Fachfehler abgelehnt.
- ED-04 mit Frühdienst, Spätdienst sowie Cafeteria-Dienst A und B als normalen Diensttypen ist bestätigt. Sie besitzen feste technische Kennungen, ihren Einsatzort und die bestätigten bearbeitbaren Standardzeiten.
- Früh- und Spätdienst verwenden `F` beziehungsweise `S`. Bei den Cafeteria-Diensten wird später im Plan die tatsächliche Zeit statt eines festen Kürzels angezeigt.
- Eine Standardzeit muss auf einer halben Stunde beginnen und enden, darf keine Sekunden enthalten und nicht über Mitternacht reichen. Die tatsächliche Bedarfszeit bleibt weiterhin eine getrennte spätere Angabe.
- Frühdienst, Spätdienst sowie Cafeteria-Dienst A und B sind die normalen Diensttypen. Ein konkreter Personalbedarf verlangt genau einen davon, enthält aber seine tatsächlich benötigte Zeit. Dadurch kann eine einmalige Zeitänderung vorgenommen werden, ohne den zukünftigen Standard oder frühere Pläne zu verändern.
- `D` und `Spr` sind keine weiteren Diensttypen für einen einzelnen Bedarf, sondern Muster, die jeweils zwei normale Dienste verbinden.
- Der Doppeldienst bleibt Frühdienst plus Spätdienst im Restaurant. Der samstägliche Springer `Spr` ist ein eigener Notfalleinsatz: Er wechselt von der Cafeteria ins Restaurant, und eine vorher fehlende Restaurant-Besetzung bleibt sichtbar.
- ED-05 mit diesen beiden internen Mustern ist bestätigt. Beim Doppeldienst zählen die drei Stunden zwischen 13:30 und 16:30 nicht als Arbeitszeit; Früh- und Spätdienst ergeben zusammen zehn Stunden Standardarbeitszeit.
- Beim Springer ist bewusst keine feste Wechselzeit hinterlegt. Der Wechsel folgt später dem tatsächlichen Ende des zweiten Cafeteria-Bedarfs; konkrete Bedarfe, Besetzungen und Teilunterdeckungen werden in diesem Schritt noch nicht erzeugt.
- ED-06 ist bestätigt: Die spätere Oberfläche kann über einen vorbereiteten Ablauf alle Einsatzorte, normalen Diensttypen sowie `D` und `Spr` gemeinsam lesen. Die gelbe, rote und blaue Kennzeichnung wird dabei zusammen mit den Textangaben übergeben.
- Zwei getrennte Bearbeitungsabläufe erlauben später genau die bestätigten Änderungen: Name und Farbe eines Einsatzortes sowie die Standardzeit eines Diensttyps. Anlegen, Löschen und frei zusammengestellte Doppeldienste sind weiterhin nicht freigegeben.
- Ungültige Eingaben, nicht mehr vorhandene Einträge und zwischenzeitlich geänderte Daten werden verständlich gemeldet. Eine geänderte Früh- oder Spätdienstzeit darf außerdem den festen Doppeldienst nicht ungültig machen.
- Die benötigten Schnittstellen zur lokalen Datenbank sind bestätigt und automatisch geprüft.
- ED-07 ist bestätigt: Die lokale SQLite-Speicherung mit ihrer ersten Migration legt Cafeteria, Restaurant, die vier normalen Diensttypen sowie `D` und `Spr` als Startwerte an.
- Änderungen an Einsatzortname, Farbe und Diensttyp-Standardzeit bleiben nach einem Neustart erhalten. Die Datenbank schützt außerdem die Zuordnungen und verhindert doppelte Muster.
- Diese Speicherung wurde mit automatisch erzeugten temporären Testdatenbanken geprüft. Im Projekt liegt keine produktive Datenbankdatei.
- ED-08 mit der ersten sichtbaren Einsatzortverwaltung ist bestätigt. Cafeteria und Restaurant können ausgewählt werden; Name und Farbkennzeichnung lassen sich bearbeiten und speichern.
- Gelb und Rot werden zusätzlich immer als Text genannt. Lade-, Leer-, Speicher-, Erfolgs- und Fehlerzustände sind sichtbar vorbereitet.
- Neue Einsatzorte anlegen sowie Einträge löschen oder deaktivieren ist weiterhin nicht möglich.
- Für ED-09 sind die zugehörigen normalen Diensttypen unter jedem Einsatzort sichtbar. Beginn und Ende ihrer Standardzeit können in halbstündigen Schritten gewählt und gespeichert werden.
- Ungültige Zeiten werden verständlich abgelehnt und bleiben zur Korrektur ausgewählt. Eine Änderung von Früh- oder Spätdienst darf den festen Doppeldienst nicht überschneiden oder seine Unterbrechung entfernen.
- Der Doppeldienst `D` und der Springer `Spr` werden getrennt von den normalen Diensttypen verständlich erklärt. Beide sind feste Muster; freie neue Kombinationen oder eine feste Springer-Wechselzeit werden nicht angeboten.
- Die Service-Leitung hat Diensttyp-Zuordnung, Zeitänderung mit erneutem Laden sowie die Darstellung von `D` und `Spr` bestätigt.
- Cafeteria wird zusätzlich zum Text gelb, Restaurant rot und `Spr` blau dargestellt.
- Der noch nicht bestätigte Entwurf für Mitarbeitende, Arbeitszeitmodelle und Qualifikationen war während System 04 pausiert, damit keine Namen, Zeiten oder Kennungen doppelt angelegt werden.
- Das gesamte technische Grundgerüst wurde noch einmal gemeinsam abgeglichen und abgenommen.
- Alle 13 App- und Testbausteine lassen sich ohne Warnungen oder Fehler technisch erstellen; alle 13 Architekturprüfungen bestehen.
- Die leere Windows-App startete technisch mit einem reagierenden Hauptfenster und wurde danach regulär beendet.
- Fenstertitel und Hinweistexte wurden beim sichtbaren lokalen Start bestätigt.
- Die internen Fachmodelle, die Anwendungsabläufe aus ED-06, die SQLite-Speicherung aus ED-07, die Einsatzortverwaltung aus ED-08 und die Diensttyp- und Einsatzmusteransicht aus ED-09 sind bestätigt.
- Die gemeinsame Abschlussprüfung von ED-10 hat die technischen Bausteine, Dokumente und die spätere Übergabe an die Mitarbeiterverwaltung abgeglichen. System 04 ist ausdrücklich abgenommen und archiviert.
- Die spätere Mitarbeiterverwaltung verwendet die vorhandenen Kennungen der Einsatzorte und normalen Diensttypen. Sie kopiert keine Namen, Farben oder Standardzeiten und führt keine eigenen Freigaben für `D` oder `Spr` ein.
- Die App besitzt noch keine Mitarbeiter- oder Dienstplanbedienung, automatische Planung oder Excel-Verarbeitung.
- Das zuvor unversionierte Beispielbild mit echten Namen und konkreten Plandaten ist nicht mehr im Projektordner vorhanden. Die benötigten Fachinformationen wurden nur ohne personenbezogene Daten übernommen.

## Gerade in Arbeit

- Der aktualisierte Roadmap-Entwurf für Mitarbeitende, Arbeitszeitmodelle und Qualifikationen ist wieder aktiv und wartet auf gemeinsame Prüfung und Abnahme.
- Mitarbeiterdaten, Mitarbeiteroberfläche und Mitarbeiter-Datenbanktabellen werden vor dieser Roadmap- und Fachmodellabnahme noch nicht programmiert.

## Noch nicht gebaut

- die fachlich nutzbare Windows-App,
- die Mitarbeiterverwaltung,
- das Anlegen, Löschen oder Deaktivieren von Einsatzorten und Diensttypen,
- die Eingabe von Urlaub, Krankheit und Verfügbarkeit,
- die automatische Erstellung eines Wochenplans,
- die Erklärung nicht besetzbarer Dienste,
- die manuelle Bearbeitung eines Plans,
- die Abnahme und das Aufbewahren früherer Fassungen fertiger Pläne,
- der Export in die Excel-Vorlage,
- die Datensicherung,
- die spätere Verwaltung von Über- und Minusstunden.

## Nächster geplanter Schritt

Als Nächstes wird der wieder aktivierte Roadmap-Entwurf für Mitarbeitende, Arbeitszeitmodelle und Qualifikationen gemeinsam geprüft und bestätigt oder angepasst. Anschließend werden die offenen Fachfragen organisiert beantwortet; Mitarbeiter- und Dienstplanfunktionen existieren weiterhin nicht.
