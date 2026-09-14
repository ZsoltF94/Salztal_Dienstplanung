# Aktueller Stand

Stand: 14. September 2026

## Wo steht das Projekt?

Die beiden ersten fachlichen Systeme sind abgeschlossen: Einsatzorte und Diensttypen sowie Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben. Die vollständige Mitarbeiterbedienung mit Anlegen, Bearbeiten, Typwechsel, Deaktivieren, Reaktivieren und sicherem endgültigem Löschen ist programmiert, automatisch geprüft und sichtbar bestätigt. Beim nächsten System für Personal-, Schicht- und Stundenbedarf sind Bedarfsgrundwerte, Wochenvorlage, einzelne Datumsausnahmen, Lese- und Schreibabläufe sowie die lokale Datenbankspeicherung intern programmiert und bestätigt. Die sichtbare Wochenübersicht ist ebenfalls bestätigt. Die nach der ersten Sichtprüfung gewünschte klarere Trennung ist nun programmiert und automatisch geprüft; ihre sichtbare Abnahme steht noch aus.

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
- Der frühere Entwurf für Mitarbeitende war während System 04 pausiert, damit keine Einsatzorte, Dienste oder Kennungen doppelt angelegt werden.
- Das gesamte technische Grundgerüst wurde noch einmal gemeinsam abgeglichen und abgenommen.
- Alle 13 App- und Testbausteine lassen sich ohne Warnungen oder Fehler technisch erstellen; alle 13 Architekturprüfungen bestehen.
- Die leere Windows-App startete technisch mit einem reagierenden Hauptfenster und wurde danach regulär beendet.
- Fenstertitel und Hinweistexte wurden beim sichtbaren lokalen Start bestätigt.
- Die internen Fachmodelle, die Anwendungsabläufe aus ED-06, die SQLite-Speicherung aus ED-07, die Einsatzortverwaltung aus ED-08 und die Diensttyp- und Einsatzmusteransicht aus ED-09 sind bestätigt.
- Die gemeinsame Abschlussprüfung von ED-10 hat die technischen Bausteine, Dokumente und die spätere Übergabe an die Mitarbeiterverwaltung abgeglichen. System 04 ist ausdrücklich abgenommen und archiviert.
- Die spätere Mitarbeiterverwaltung verwendet die vorhandenen Kennungen der Einsatzorte, normalen Diensttypen und Einsatzmuster. Sie kopiert keine Namen, Farben, Standardzeiten oder Musterbestandteile. Eine kontextabhängige Freigabe kann aber festhalten, dass TypAH2 den Frühdienst innerhalb von `D`, nicht jedoch automatisch als einzelnen Frühdienst übernehmen darf.
- Die App besitzt jetzt eine Mitarbeiterbedienung, aber noch keine Dienstplanbedienung, automatische Planung oder Excel-Verarbeitung.
- Das zuvor unversionierte Beispielbild mit echten Namen und konkreten Plandaten ist nicht mehr im Projektordner vorhanden. Die benötigten Fachinformationen wurden nur ohne personenbezogene Daten übernommen.

## System 03 – umgesetzt und abgenommen

- Die Fachfragen zu Mitarbeitenden und Mitarbeitertypen sind beantwortet. Die App startet später mit acht gemeinsam verwendeten Typen und ihren bestätigten Wochenstunden und Einsatzmöglichkeiten.
- Die überarbeitete Roadmap für „Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben“ ist bestätigt.
- Der erste Domain-Schritt ist bestätigt. Er bildet eine feste interne Typkennung, einen sichtbaren Code, einen verständlichen Namen und ein minutengenaues Wochen-Soll ab. Leere Angaben, eine leere Kennung und unmögliche Wochenwerte werden abgelehnt.
- Die Textdateien des Projekts besitzen nun bestätigte, einheitlich durch Git abgesicherte Zeilenenden. Diese technische Bereinigung hat keine Fachfunktion verändert.
- Die acht Starttypen besitzen ihre bestätigten Wochenstunden und Einsatzmöglichkeiten. TypAH1 und TypAH2 schlagen einen einzelnen Frühdienst nur als manuelle Notfalllösung vor; TypAH2 kann für den Springer nur nach bewusster Aktivierung berücksichtigt werden.
- Typ1 bleibt außerhalb der späteren automatischen Einteilung. Seine vorhandenen manuellen Dienste werden geschützt, und er wird erst als letzte manuelle Lösung vorgeschlagen.
- Jeder Mitarbeiter besitzt eine unsichtbare stabile Kennung, einen getrennten Vor- und Nachnamen, einen Aktivstatus und genau einen Verweis auf einen Mitarbeitertyp. Gleiche Namen sind zulässig.
- Änderungen am Namen oder Typ sowie eine Deaktivierung bewahren die frühere Fassung unverändert. Wochenstunden und Einsatzmöglichkeiten werden nicht beim Mitarbeiter kopiert.
- Die vorbereiteten Leseabläufe liefern Übersicht, Details und Typauswahl als unveränderliche Momentaufnahmen. Sie zeigen aktuelle Typnamen, Wochenstunden und Einsatzmöglichkeiten in verständlicher Form.
- Ein leerer Mitarbeiterbestand und ein Abbruch vor dem Lesen sind berücksichtigt. Die Leseabläufe sind inzwischen mit der gemeinsamen lokalen Mitarbeiterdatenbank verbunden.
- Die vorbereiteten Schreibabläufe prüfen Namen, Mitarbeiter- und Typkennungen sowie die Einsatzfreigaben eines Typs, bevor eine Speicherung verlangt wird. Fehler werden mit verständlichen deutschen Meldungen zurückgegeben.
- Das Speichern führt Anlegen und Typwechsel gemeinsam mit der Prüfung aus, dass nie zwei aktive Typ1-Personen entstehen. Die letzte aktive Typ1-Person darf während der Ersteinrichtung deaktiviert werden; vor einer Planung muss später wieder genau eine aktive Typ1-Person vorhanden sein.
- Die vorhandene lokale Datenbank erhält nur eine gemeinsame Folge nummerierter Änderungen. Die bereits veröffentlichte erste Änderung wurde nicht umgeschrieben; eine leere und eine bereits vorhandene synthetische Testdatenbank bleiben korrekt lesbar.
- Die acht Mitarbeitertypen und ihre Einsatzmöglichkeiten werden beim Einrichten der lokalen Datenbank vollständig angelegt. Es werden keine Personen vorbefüllt.
- Mitarbeitende können technisch gespeichert, umbenannt, einem anderen Typ zugeordnet und deaktiviert werden. Diese Änderungen bleiben nach dem erneuten Öffnen erhalten.
- Die Datenbank verhindert eine zweite aktive Serviceleitung, unbekannte Zuordnungen und das Entfernen eines Typs, der noch von Mitarbeitenden verwendet wird.
- Ein eigener Reiter „Mitarbeitende“ zeigt vorhandene Personen mit Aktivstatus, Typ, Wochenstunden und Einsatzmöglichkeiten. Laden, leerer Bestand, technische Fehler, erneutes Laden und Auswahl sind vorbereitet und automatisch geprüft.
- Der Auftraggeber hat den Reiter „Mitarbeitende“, den verständlichen leeren Zustand und den Button „Aktualisieren“ sichtbar bestätigt.
- Neue Mitarbeitende können mit Vorname, Nachname und einem der acht Typen angelegt werden. Name und Typ werden bewusst getrennt bearbeitet, damit eine abgelehnte Änderung keinen anderen Wert teilweise speichert.
- Vor der Typauswahl sind Typcode, Name, Wochenstunden und Einsatzmöglichkeiten erkennbar. Leere Pflichtfelder und eine zweite aktive Typ1-Person werden verständlich abgelehnt, ohne die Eingaben zu verlieren.
- Deaktivieren löscht keine Person und verlangt vorher eine ausdrückliche Bestätigung. Der vollständige sichtbare Ablauf einschließlich erneutem Laden ist bestätigt.
- Deaktivierte Mitarbeitende können wieder aktiviert werden. Kennung, Name und Typ bleiben erhalten; eine zweite aktive Serviceleitung wird weiterhin verhindert.
- Nur deaktivierte Personen, die noch in keinem Plan und keinen anderen Fachdaten verwendet werden, können endgültig gelöscht werden. Verwendete Personen bleiben deaktiviert erhalten.
- Vor dem endgültigen Löschen zeigt die App den vollständigen Namen und warnt deutlich, dass die Aktion nicht rückgängig gemacht werden kann. Erst ein erfolgreicher Speichervorgang entfernt die Person aus der Übersicht.

## Noch nicht gebaut

- die fachlich nutzbare Windows-App,
- das Anlegen, Löschen oder Deaktivieren von Einsatzorten und Diensttypen,
- die sichtbare Bearbeitung einzelner Datumsausnahmen,
- die Eingabe von Urlaub, Krankheit und Verfügbarkeit,
- die automatische Erstellung eines Wochenplans,
- die Erklärung nicht besetzbarer Dienste,
- die manuelle Bearbeitung eines Plans,
- die Abnahme und das Aufbewahren früherer Fassungen fertiger Pläne,
- der Export in die Excel-Vorlage,
- die Datensicherung,
- die spätere Verwaltung von Über- und Minusstunden.

## System 05 – derzeit in Arbeit

- Ein einzelner Bedarf kann intern mit Datum, Einsatzort, normalem Diensttyp, tatsächlicher Zeit und positiver Personenzahl gültig beschrieben werden.
- Die App-Grundlage berechnet daraus minutengenau die Dauer und die insgesamt benötigten Mitarbeiterstunden. Ein zusätzlicher, möglicherweise widersprüchlicher Stundenwert wird nicht eingegeben.
- Ungültige Kennungen, Personenzahlen und Zeiten werden strukturiert abgelehnt.
- Die bestätigten Beispiele mit vier Personen ergeben bei 06:30 bis 13:30 Uhr 28 Stunden und bei 06:30 bis 12:30 Uhr 24 Stunden.
- Dieser erste interne Schritt ist automatisch geprüft und abgenommen.
- Die regelmäßige Wochenvorlage enthält alle 23 bestätigten Bedarfe für Montag bis Sonntag. Die beiden Cafeteria-Dienste am Wochenende bleiben dabei getrennt.
- Eine spätere Änderung kann einen Standard ab einer ausgewählten Woche ergänzen, ersetzen oder aufheben. Sie gilt ab dem Montag dieser Woche; frühere Stände bleiben erhalten.
- Bereits geänderte Diensttyp-Standardzeiten werden bei der erstmaligen Anlage übernommen und nicht durch alte Ausgangszeiten überschrieben.
- Dieser zweite interne Schritt ist automatisch geprüft und bestätigt.
- Für ein einzelnes Datum kann ein Bedarf intern vollständig ersetzt, ergänzt oder aufgehoben werden. Eine solche Ausnahme behält ihre eigene tatsächliche Zeit und Personenzahl.
- Wird eine vorhandene Ausnahme entfernt, verwendet die Berechnung wieder den zu diesem Zeitpunkt geltenden Wochenstandard.
- Eine ausgewählte Woche wird von Montag bis Sonntag eindeutig berechnet. Die Ausgangswoche umfasst 57 Cafeteria-Stunden, 280 Restaurant-Stunden und insgesamt 337 benötigte Mitarbeiterstunden.
- Feiertage besitzen keine versteckte Sonderautomatik, sondern werden wie vereinbart als normale manuelle Datumsausnahme behandelt.
- Dieser dritte interne Schritt ist automatisch geprüft und bestätigt.
- Der neue Leseablauf sammelt Wochenstandards, Datumsausnahmen und den Dienstkatalog in einem festen Lesestand und berechnet daraus genau eine Montag-bis-Sonntag-Woche.
- Die vorbereitete Wochenansicht enthält für jeden Bedarf seine Herkunft, Einsatzort, Diensttyp, tatsächliche Zeit, Personenzahl und benötigte Minuten. Dazu kommen Tages-, Einsatzort- und Gesamtsummen.
- Fehlende oder falsch zugeordnete Katalogeinträge und widersprüchliche gespeicherte Werte werden verständlich gemeldet und nicht stillschweigend ausgelassen. Auch ein Abbruch vor oder während des Lesens ist geprüft.
- Dieser vierte interne Schritt ist automatisch geprüft und bestätigt.
- Drei getrennte Schreibabläufe bereiten Änderungen an Wochenstandards, das Speichern einer Datumsausnahme und das Entfernen einer Datumsausnahme vor.
- Vor dem Speichern werden der aktuelle Stand, Einsatzort, Diensttyp und ihre richtige Zuordnung geprüft. Wenn der Stand zwischenzeitlich geändert wurde, erscheint ein verständlicher Konflikt statt einer unbemerkten Überschreibung.
- Ergänzen, Ersetzen und Aufheben sind bei Wochenstandard und Datumsausnahme getrennt erkennbar. Ungültige Eingaben und nicht mehr vorhandene Werte lösen keinen Schreibversuch aus.
- Ist eine zu entfernende Datumsausnahme bereits weg, gilt das als erfolgreich erreicht. Wurde sie inzwischen durch eine andere Ausnahme ersetzt, muss dagegen neu geladen und bewusst entschieden werden.
- Dieser fünfte interne Schritt ist automatisch geprüft und bestätigt.
- Die regelmäßigen Bedarfe und einzelnen Datumsausnahmen können nun in derselben lokalen Datenbank wie Einsatzorte, Diensttypen und Mitarbeitende gespeichert werden.
- Beim ersten Einrichten werden die 23 bestätigten regelmäßigen Bedarfe genau einmal angelegt. Dabei verwendet die App die dann aktuellen Diensttyp-Standardzeiten und setzt eine bereits bewusst geänderte Zeit nicht zurück.
- Frühere Wochenstände bleiben als aufeinanderfolgende Änderungen erhalten. Einzelne Datumsausnahmen werden vollständig und gemeinsam gespeichert, ersetzt oder entfernt; ein zwischenzeitlich geänderter Stand wird nicht unbemerkt überschrieben.
- Automatische Prüfungen haben sowohl eine neue Datenbank als auch vorhandene ältere synthetische Datenbanken mit geänderten Dienstzeiten und erfundenen Mitarbeiterdaten erfolgreich aktualisiert. Die vorhandenen Werte blieben erhalten.
- Die Datenbank weist doppelte Bedarfe, unbekannte Zuordnungen, ungültige Zeiten und ungültige Personenzahlen zusätzlich selbst zurück. Auch ein absichtlich ausgelöster Fehler während des Speicherns hat den vorherigen Stand vollständig erhalten.
- Dieser sechste interne Schritt ist automatisch geprüft und bestätigt.
- Ein neuer Reiter „Bedarf“ zeigt die ausgewählte Woche immer von Montag bis Sonntag. Eine Woche kann über ein Datum gewählt oder mit „Vorherige Woche“ und „Nächste Woche“ gewechselt werden.
- Cafeteria und Restaurant werden getrennt dargestellt. Darunter zeigt jede Tageszeile den Diensttyp, die tatsächlich benötigte Zeit, die Personenzahl, die daraus berechneten Mitarbeiterstunden und die Herkunft als Wochenstandard oder Datumsausnahme.
- Die Ansicht nennt zusätzlich die Tagessumme je Einsatzort, die Wochensumme jedes Einsatzortes und die Gesamtsumme. Mit den bestätigten Ausgangswerten sind dies 57 Cafeteria-Stunden, 280 Restaurant-Stunden und insgesamt 337 Stunden.
- Gelb und Rot werden weiterhin zusätzlich als Text genannt. Eine einzelne Datumsausnahme ist gegenüber dem Wochenstandard sowohl beschriftet als auch gestalterisch hervorgehoben.
- Während des Ladens sowie bei leerem Bestand oder einem Fehler erscheinen eigene verständliche Zustände. Die automatische Prüfung umfasst auch Wochenwechsel, erneutes Laden und Abbruch.
- Dieser siebte interne Schritt ist programmiert, automatisch geprüft und nach dem sichtbaren Test ausdrücklich bestätigt.
- Ein neuer Bearbeitungsbereich erlaubt nun, einen regelmäßigen Wochenstandard ab einem bewusst gewählten Montag zu ersetzen, zu ergänzen oder aufzuheben. Frühere Wochen bleiben dabei unverändert.
- Einsatzort, Wochentag, normaler Diensttyp, tatsächliche Zeit und Personenzahl sind vor dem Speichern sichtbar. Zum gewählten Einsatzort werden nur dessen normale Diensttypen angeboten; `D` und `Spr` sind keine auswählbaren Bedarfe.
- Ungültige Eingaben und zwischenzeitliche Änderungen werden verständlich gemeldet und bleiben zur Korrektur stehen. Nach erfolgreichem Speichern lädt die App die Woche erneut aus der lokalen Datenbank und berechnet die sichtbaren Summen neu.
- Dieser achte interne Schritt ist programmiert und automatisch geprüft. Bei der sichtbaren Beurteilung wurde er in dieser Form nicht abgenommen: Regelmäßige Standards sollen nicht in der Wochenübersicht bearbeitet werden.
- Die Bedienung ist nun klarer getrennt. Unter „Einsatzorte und Dienste“ sind die drei Bereiche „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ auswählbar.
- Im Reiter „Bedarf“ wird ausschließlich ein konkreter Kalendertag über „Nur diesen Tag ändern“ angepasst. Diese „Einmalige Änderung“ darf erneut bearbeitet, auf „kein Bedarf“ gesetzt oder wieder auf den regelmäßigen Standard zurückgesetzt werden.
- Die allgemeine Standardzeit eines Diensttyps und der regelmäßige Personalbedarf bleiben getrennt. Ein regelmäßiger Bedarf erhält weiterhin einen bewusst gewählten Wirksamkeitsmontag, damit frühere Wochen unverändert bleiben.
- Nach jedem Speichern oder Zurücksetzen wird die Woche aus der lokalen Datenbank neu geladen und alle sichtbaren Summen werden neu berechnet. Auch ein zuvor fehlender Tagesbedarf kann ergänzt werden.
- Die Korrektur ist programmiert und automatisch geprüft. Als offener Nachweis bleibt die gemeinsame sichtbare Bedienprüfung.
- Der weitere Änderungsbedarf aus der Sichtprüfung ist programmiert: Ein bereits geänderter regelmäßiger Bedarf kann nun erneut für denselben Wirksamkeitsmontag gespeichert, auf „kein regelmäßiger Bedarf“ gesetzt oder wieder ergänzt werden.
- Jede weitere Speicherung erzeugt intern eine neue unveränderliche Korrekturfassung. Ältere Fassungen bleiben erhalten; sichtbar und fachlich gilt für diesen Montag immer die zuletzt gespeicherte Fassung.
- Der Einsatzort wird auch beim regelmäßigen Bedarf über die linke Einsatzortliste gewählt. Die Bedarfsansicht zeigt anschließend alle sieben Tage von Montag bis Sonntag sowie vorhandene und fehlende Bedarfe der normalen Diensttypen.
- BE-09B ist programmiert und automatisch geprüft. Offen ist die sichtbare Prüfung beider Einsatzorte und zweier aufeinanderfolgender Korrekturen desselben Bedarfs am selben Montag.

## Nächster geplanter Schritt

Als Nächstes werden die einheitliche linke Einsatzortauswahl, die vollständige Wochenansicht und zwei aufeinanderfolgende Korrekturen desselben regelmäßigen Bedarfs sichtbar geprüft. Danach können BE-09A und BE-09B gemeinsam abgenommen werden.
