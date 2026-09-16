# Aktueller Stand

Stand: 16. September 2026

## Wo steht das Projekt?

Vier fachliche Systeme sind abgeschlossen: Einsatzorte und Diensttypen, Mitarbeitende und Mitarbeitertypen, Personal-, Schicht- und Stundenbedarf sowie Verfügbarkeiten und Abwesenheiten. System 06 ist fachlich, technisch und sichtbar vollständig geprüft, ausdrücklich abgenommen und archiviert.

Für System 07 sind alle Fachfragen und Folgefragen zum zentralen Regelkatalog beantwortet. Die Antworten wurden in einer Prioritätsmatrix und einer Architekturentscheidung zusammengeführt und am 16. September 2026 ausdrücklich bestätigt. Eine kleinschrittige Roadmap mit sieben Schritten ist jetzt als Entwurf vorbereitet; programmiert wurde System 07 noch nicht.

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
- System 05 mit regelmäßigen Bedarfen, einzelnen Tagesänderungen, berechneten Stunden und lokaler Speicherung ist vollständig geprüft, bestätigt und archiviert.

## Bestätigt: erster Mitarbeitertypen-Schritt

- Die fachliche Grundlage kennt nun alle elf bestätigten Starttypen, darunter neu `Typ20`, `Typ20a` und `Typ25a`.
- Für Urlaub `U` und Krankheit `K` besitzt jeder dafür zugelassene Typ einen minutengenauen Tageswert. Die bestätigten Startwerte von vier bis acht Stunden sind hinterlegt; für AH bleiben `U` und `K` unzulässig.
- Normale Typen, Typ1 und AH sind durch geschützte Rollen eindeutig unterschieden. Namen oder sichtbare Typcodes entscheiden nicht über diese Sonderrollen.
- Bei einer späteren Bearbeitung bleiben die feste Kennung, der Typcode und die Sonderrolle erhalten. Name, Wochen-Soll, Abwesenheitsregel und Einsatzberechtigungen können über den vorbereiteten Fachvertrag geändert werden.
- Die lokale Datenbank und die sichtbare App sind in MT-01 bewusst noch nicht erweitert worden. Die Speicherung wurde anschließend in MT-03 umgesetzt; die sichtbare App folgt erst in MT-04.
- Der vollständige technische Testlauf umfasst 371 erfolgreiche Prüfungen; der Build enthält keine Warnungen oder Fehler.
- Der erste Mitarbeitertypen-Schritt ist am 15. September 2026 ausdrücklich bestätigt worden.

## Bestätigt: Abläufe der Mitarbeitertypenpflege

- Die App-Grundlage kann künftig alle Angaben eines Mitarbeitertyps vollständig und eindeutig an die Bedienoberfläche übergeben: Code, Name, Wochen-Soll, Urlaubs- und Krankheitsregel, Tageswert, geschützte Rolle und jede Einsatzberechtigung.
- Beim Anlegen wird immer ein normal automatisch planbarer Typ vorbereitet. Ein bereits verwendeter Typcode wird verständlich abgelehnt.
- Beim Bearbeiten bleiben die feste Kennung, der Typcode und eine besondere Typ1- oder AH-Rolle unverändert. Die freigegebenen Stammdaten und Einsatzberechtigungen können geändert werden.
- Endgültiges Löschen verlangt eine ausdrückliche Bestätigung. Typ1- und AH-Typen sowie Typen, die einer aktiven oder deaktivierten Person oder anderen Fachdaten zugeordnet sind, bleiben geschützt erhalten.
- Wird ein Typ zwischen Lesen und Speichern verändert, überschreibt die App-Grundlage diesen neueren Stand nicht, sondern fordert zum erneuten Laden auf.
- Die Abläufe und verständlichen Fehlermeldungen sind automatisch geprüft. Insgesamt bestehen nun 393 Prüfungen; der vollständige Build enthält keine Warnungen oder Fehler.
- Dauerhafte Datenbankspeicherung und sichtbare Bedienoberfläche sind bewusst noch nicht in MT-02 enthalten. Die Speicherung wurde anschließend in MT-03 umgesetzt; die Bedienoberfläche folgt erst in MT-04.
- Die vorbereiteten Abläufe und Fehlermeldungen sind am 15. September 2026 ausdrücklich bestätigt worden.

## Bestätigt: dauerhafte Speicherung der Mitarbeitertypen

- Die gemeinsame lokale Datenbank enthält nun alle elf Starttypen samt Wochen-Soll, Urlaubs- und Krankheitsregel, minutengenauem Tageswert, geschützter Planungsrolle und jeder Einsatzberechtigung.
- Eine vorhandene Datenbank aus System 05 wird in derselben nachvollziehbaren Folge erweitert. Synthetische Tests belegen, dass bereits vorhandene Mitarbeiter-, Katalog- und Bedarfsdaten dabei erhalten bleiben.
- Neue normale Typen können dauerhaft angelegt und erlaubte Angaben vollständig geändert werden. Die Werte bleiben nach dem Schließen und erneuten Öffnen korrekt erhalten.
- Typcodes sind auch bei unterschiedlicher Groß- und Kleinschreibung eindeutig. Ungültige Abwesenheits- oder Rollenwerte werden zusätzlich direkt von der Datenbank abgelehnt.
- Ein noch nicht verwendeter normaler Typ kann samt Einsatzberechtigungen vollständig gelöscht werden. Ein einer Person zugeordneter Typ bleibt durch den Referenzschutz erhalten.
- Ein technischer Fehler oder ein zwischenzeitlich geänderter Stand führt nicht zu einer Teiländerung. Der vorherige Datenstand bleibt vollständig erhalten.
- Insgesamt bestehen nun 404 automatische Prüfungen; der vollständige Build enthält keine Warnungen oder Fehler. Der sichtbare WPF-Tab ist weiterhin nicht Bestandteil dieses Schritts.
- Die dauerhafte Speicherung ist am 15. September 2026 ausdrücklich bestätigt worden.

## Bestätigt: Mitarbeitertypen-Tab

- Ein eigener Reiter „Mitarbeitertypen“ zeigt Typcode, Namen, Wochen-Soll, U-/K-Regel, Tageswert, geschützte Planungsrolle und alle Einsatzberechtigungen.
- Reguläre Planbarkeit, ein rein manueller Vorschlag und eine nur nach bewusster Laufoption aktive Berechtigung bleiben getrennt erkennbar und bearbeitbar.
- Beim Anlegen entsteht immer ein normaler Mitarbeitertyp. Beim Bearbeiten bleiben Kennung, Typcode und besondere Typ1- oder AH-Rolle geschützt.
- Vor einer Änderung weist die App darauf hin, dass sie für alle zugeordneten Mitarbeitenden und künftigen Planungen gilt, während abgenommene Planversionen unverändert bleiben.
- Endgültiges Löschen ist nur für normale Typen erreichbar und verlangt eine deutliche Bestätigung. Verwendete Typen bleiben erhalten und werden verständlich gemeldet.
- Ungültige Eingaben und technische Fehler lassen die eingegebenen Werte zur Korrektur stehen. Auch ein laufender Speichervorgang kann ohne Verlust der Eingaben abgebrochen werden.
- In der Detailansicht wird jede Einsatzberechtigung mit dem zugehörigen Dienst- oder Musternamen und ihrem Status angezeigt. Eine automatische Ansichtsprüfung schützt diese sichtbare Zuordnung vor einem Rückfall.
- Grüne Erfolgsfelder sowie rote Eingabe- und technische Fehlerfelder zeigen ihren vollständigen Rückmeldungstext an, sobald sie eingeblendet werden. Die automatische Prüfung deckt diese gemeinsame Anzeigelogik ab.
- Zwölf automatische Ablaufprüfungen und zwei zusätzliche Prüfungen des fehlerfreien Ansichtsstarts einschließlich geladener Typdaten bestehen. Insgesamt sind 418 Prüfungen grün; der vollständige Build enthält keine Warnungen oder Fehler.
- Ein unerwarteter technischer Oberflächenfehler beendet die App künftig nicht mehr kommentarlos, sondern wird verständlich gemeldet.
- Die sichtbare Bedienprüfung mit ausschließlich synthetischen Angaben wurde am 15. September 2026 bestätigt.

## Abgenommen: Mitarbeitertypen-Vorbereitung

- Fachmodell, Abläufe, lokale Speicherung und sichtbare Bedienoberfläche wurden gemeinsam geprüft.
- Alle 418 automatischen Prüfungen bestehen. Der vollständige Build enthält keine Warnungen oder Fehler.
- Die lokale Datenbank besitzt alle erwarteten Migrationen; das aktuelle Datenmodell stimmt mit dem Migrationsstand überein.
- Die technischen und verständlichen Leitdokumente beschreiben denselben Stand.
- Das unversionierte Referenzbild wurde als reines Beispiel ohne personenbezogene oder reale Planungsdaten bestätigt. Es bleibt unversioniert und wird nicht als Fachdatenquelle verwendet.
- Der Gesamtstand wurde am 15. September 2026 ausdrücklich abgenommen. VA-01 darf damit beginnen.

## Fachlich umgesetzt und geprüft: Tageseinträge und Wochen-Soll

- `U`, `K` und ein von der Service-Leitung gesetztes rotes `X` sind drei eindeutig getrennte Tageseinträge für eine Person und ein konkretes Datum.
- Pro Person und Tag kann nur ein aktueller Wert gelten. Eine neue Auswahl ersetzt den bisherigen Wert; der unveränderte Ausgangsstand bleibt bei der fachlichen Berechnung erhalten.
- Das wirksame Soll wird für jede Woche von Montag bis Sonntag in Minuten berechnet. `U` und `K` ziehen den Tageswert des Mitarbeitertyps ab, auch an Wochenenden und Feiertagen. Ein rotes `X` zieht nichts ab; weniger als null Minuten sind ausgeschlossen.
- Bei AH werden `U` und `K` abgelehnt, ein rotes `X` bleibt zulässig. Für Typ1 wird eindeutig erkannt, ob wirklich alle sieben Tage einer Woche gesperrt sind oder noch mindestens ein Tag verfügbar ist.
- 22 neue Prüfungen decken diese Fälle ab. Zusammen mit allen bisherigen Bereichen bestehen nun 440 automatische Prüfungen; der vollständige Build enthält keine Warnungen oder Fehler.
- Oberfläche und lokale Speicherung waren bewusst noch nicht Teil dieses Schritts. VA-01 wurde am 15. September 2026 zusammen mit dem vollständig funktionierenden Stand bis VA-05 ausdrücklich abgenommen.

## Fachlich umgesetzt und geprüft: Drei-Wochen-Lesestand

- Ein beliebig ausgewähltes Datum wird eindeutig auf den zugehörigen Montag bezogen. Der Lesestand enthält von dort genau 21 Tage bis zum dritten Sonntag.
- Angezeigt werden später nur aktive Personen. Jede Zeile enthält Name, aktuellen Mitarbeitertyp, vorhandene Tageseinträge sowie ungekürztes und wirksames Soll für jede der drei Wochen.
- Die Übergabe ist unveränderlich und enthält keine Datenbank- oder Oberflächenobjekte. Abbruch wird sauber weitergegeben.
- Fehlende oder doppelte Personen- und Typzuordnungen, doppelte aktuelle Tageseinträge und ein unzulässiges `U` oder `K` bei AH führen zu verständlichen Fehlern statt zu einem unvollständigen Stand.
- 14 neue Prüfungen decken diesen Lesevertrag ab. Insgesamt sind 454 automatische Prüfungen grün; der Build enthält keine Warnungen oder Fehler.
- Die tatsächliche Datenbankspeicherung und die sichtbare Tabelle sind weiterhin bewusst spätere Schritte.

## Fachlich umgesetzt und geprüft: Tageseinträge ändern

- Ein leeres Tagesfeld kann mit `U`, `K` oder rotem `X` belegt werden. Ein anderer bestehender Wert wird erst nach einer ausdrücklichen Ersetzungsbestätigung geändert.
- Das Entfernen eines Eintrags verlangt ebenfalls eine ausdrückliche Sicherheitsbestätigung.
- Jeder gelesene Eintrag besitzt einen positiven Änderungsstand. Wurde das Feld zwischen Lesen und Speichern verändert, bleibt der neuere Stand erhalten und die App kann zum erneuten Laden auffordern.
- Für deaktivierte Personen werden keine neuen Änderungen gespeichert. Fehlende Personen oder Typen sowie bei AH unzulässige `U`- und `K`-Einträge werden verständlich abgelehnt.
- Validierungs-, Bestätigungs- und Konfliktfehler rufen die Speicherung nicht auf. Die spätere Historie abgenommener Pläne gehört nicht zum aktuellen Eintrag und wird durch diese Verträge nicht verändert.
- 19 neue Prüfungen decken diese Schreibabläufe ab. Insgesamt sind 473 automatische Prüfungen grün; der Build enthält keine Warnungen oder Fehler.

## Technisch umgesetzt und geprüft: Tageseinträge lokal speichern

- Pro Person und Datum speichert die gemeinsame lokale Datenbank genau einen aktuellen Wert: Urlaub, Krankheit oder ein fest gesetztes rotes `X`.
- Jede Änderung besitzt einen positiven Änderungsstand. Ein inzwischen veränderter Wert wird nicht unbemerkt überschrieben; die App kann stattdessen zum erneuten Laden auffordern.
- Solange ein Tageseintrag besteht, schützt die Datenbank die zugehörige Person vor dem Löschen. Ungültige Eintragsarten und Änderungsstände werden zusätzlich direkt abgelehnt.
- Die neue sechste Migration erweitert vorhandene Datenbanken ohne Änderung früherer Migrationen. Tests mit ausschließlich erfundenen Daten bestätigen Neuaufbau, Aktualisierung, Neustart, Eindeutigkeit, Fremdschlüssel sowie den unveränderten vorherigen Stand bei einem künstlich ausgelösten Schreibfehler.
- Zwölf neue Prüfungen decken die lokale Speicherung ab. Insgesamt sind 485 automatische Prüfungen grün; der vollständige Build enthält keine Warnungen oder Fehler.
- Die lokale Speicherung bildet die technische Grundlage für die anschließend umgesetzte Drei-Wochen-Tabelle aus VA-05.

## Sichtbar umgesetzt und automatisch geprüft: „Dienstplan SER“

- Der neue Tab zeigt alle aktiven Mitarbeitenden als Zeilen. Links stehen Name und bindender Mitarbeitertyp, danach folgen genau 21 Tage von Montag bis zum dritten Sonntag.
- Rechts werden für jede Person und jede der drei Wochen das normale und das durch `U` oder `K` verringerte Wochen-Soll angezeigt.
- Ein Tagesfeld wird einzeln ausgewählt. Urlaub, Krankheit, festes Frei und Leeren stehen als klare Schaltflächen bereit; zusätzlich funktionieren die Kürzel `U`, `K`, `X` und `Entf`.
- Ein rotes `X` erscheint als roter Buchstabe auf normalem Hintergrund und wird zusätzlich verständlich als fest freier Tag erklärt. Bei AH sind Urlaub und Krankheit nicht auswählbar.
- Ein vorhandener anderer Wert und das Leeren werden erst nach einer sichtbaren Bestätigung geändert. Gleichzeitiges Bearbeiten mehrerer Felder ist bewusst nicht enthalten.
- Zehn neue Desktop-Prüfungen decken Laden, Zeitraumwechsel, Aktionen, Tastatur, Bestätigen und Abbrechen, AH, Konflikte, Fehlerzustände sowie die gerenderte Darstellung ab. Insgesamt sind 495 automatische Prüfungen grün; der Build enthält keine Warnungen oder Fehler.
- Vor dem sichtbaren Start wurde die vorhandene lokale Datenbank im lokalen Sicherungsordner kopiert. Die App bleibt nach der Aktualisierung stabil geöffnet.
- Die Service-Leitung hat am 15. September 2026 bestätigt, dass die vollständige Ansicht und alle vereinbarten sichtbaren Abläufe funktionieren.

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
- die Eingabe von Urlaub, Krankheit und Verfügbarkeit,
- die automatische Erstellung eines Wochenplans,
- die Erklärung nicht besetzbarer Dienste,
- die manuelle Bearbeitung eines Plans,
- die Abnahme und das Aufbewahren früherer Fassungen fertiger Pläne,
- der Export in die Excel-Vorlage,
- die Datensicherung,
- die spätere Verwaltung von Über- und Minusstunden.

## System 05 – umgesetzt und abgenommen

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
- Die Korrektur ist programmiert, automatisch geprüft und sichtbar abgenommen.
- Der weitere Änderungsbedarf aus der Sichtprüfung ist programmiert: Ein bereits geänderter regelmäßiger Bedarf kann nun erneut für denselben Wirksamkeitsmontag gespeichert, auf „kein regelmäßiger Bedarf“ gesetzt oder wieder ergänzt werden.
- Jede weitere Speicherung erzeugt intern eine neue unveränderliche Korrekturfassung. Ältere Fassungen bleiben erhalten; sichtbar und fachlich gilt für diesen Montag immer die zuletzt gespeicherte Fassung.
- Der Einsatzort wird auch beim regelmäßigen Bedarf über die linke Einsatzortliste gewählt. Die Bedarfsansicht zeigt anschließend alle sieben Tage von Montag bis Sonntag sowie vorhandene und fehlende Bedarfe der normalen Diensttypen.
- BE-09B ist programmiert, automatisch geprüft und sichtbar abgenommen. Damit ist System 05 vollständig abgeschlossen.

## Abschließender System-06-Nachweis

- Alle 13 Projekte lassen sich ohne Warnung und ohne Fehler erstellen.
- Alle 495 vorhandenen automatischen Prüfungen sind grün. Die später benötigten Planning- und Excel-Prüfprojekte sind noch leer und melden dies erwartungsgemäß.
- Datenbankmigrationen, Architekturgrenzen, Formatierung und Dokumentpfade sind geprüft.
- Im Projektordner liegen keine Datenbanken, Sicherungen oder Exporte mit Anwendungsdaten. Das bestätigte Beispielbild bleibt unversioniert.
- Die späteren Übergaben für Regelkatalog, Planmomentaufnahme, Generierung, Berichte, Bearbeitung, Planversionen und Excel sind klar getrennt dokumentiert.

## Nächster geplanter Schritt

Als Nächstes wird die Teil-Roadmap für System 07 gemeinsam geprüft und abgenommen. Erst danach darf der erste kleine Implementierungsschritt für die fachliche Regelsprache beginnen.
