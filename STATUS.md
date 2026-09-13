# Projektstatus

Stand: 2026-09-13

Status dieses Dokuments: Aktuell – System 04 abgeschlossen; MA-01 von System 03 wartet auf Roadmap-Abnahme

## Aktueller Überblick

| Bereich | Aktueller Stand |
|---|---|
| Projektphase | Vorbereitung des nächsten Fachsystems |
| Aktives System | System 03 – Mitarbeitende, Arbeitszeitmodelle und Qualifikationen |
| Aktive Teil-Roadmap | noch nicht abgenommener Entwurf unter `docs/roadmaps/active/EMPLOYEES_WORK_MODELS_QUALIFICATIONS_ROADMAP.md` |
| Aktueller Stand | MA-01 – nach Abschluss von System 04 wieder aufgenommener Roadmap-Entwurf wartet auf Abnahme |
| Zuletzt abgenommener Schritt | ED-10 – System 04 gemeinsam abschließen |
| Funktionsfähige App | Einsatzorte und Diensttyp-Standardzeiten können angezeigt und bearbeitet werden; noch keine Dienstplanfunktion |
| Echte Mitarbeiter- oder Plandaten im Repository | Keine festgestellt; das zuvor vorhandene sensible Beispielbild ist nicht mehr im Arbeitsordner |

## Nachweislich fertig und abgenommen

- Das Grundverständnis und die bestätigten Produktentscheidungen sind dokumentiert.
- Das Projekt ist als Git-Repository eingerichtet und mit dem vorgesehenen GitHub-Repository verbunden.
- Der leicht verständliche Lesebereich für die Service-Leitung ist eingerichtet.
- Zielplattform, Technik und modulare Architektur sind verbindlich festgelegt.
- Clean-Code-, Namespace-, Test- und Qualitätsregeln sind festgelegt.
- Der kleinschrittige Arbeits-, Berichts- und Abnahmeprozess ist in `AGENTS.md` verbindlich geregelt.
- Die abgenommene `MASTER_ROADMAP.md` ordnet 16 geplante Systeme und ihre Abhängigkeiten.
- Die zentrale `STATUS.md` führt aktiven Schritt, offene Entscheidungen, Blockaden und ausstehende Gates zusammen.
- System 01 – Projektgrundlage ist vollständig geprüft, abgenommen und archiviert.
- Die Teil-Roadmap für System 02 ist abgenommen.

## Aktuell

- System 04 – Einsatzorte, Diensttypen und Doppeldienste – ist als erstes Fachsystem ausgewählt, damit System 03 später vorhandene stabile Kennungen zuordnen kann.
- Die beantwortete Fragen-Datei bestätigt Einsatzorte, Diensttypen, Standardzeiten, tatsächliche Bedarfszeiten, Doppeldienst, Springer und den minimalen Bedienumfang.
- Die frühere Annahme einer stets festen Diensttypzeit ist ersetzt: Jeder Bedarf verlangt genau einen Diensttyp, besitzt aber seine tatsächliche, ausdrücklich bearbeitbare Zeit.
- Die Entscheidung und ihre Auswirkungen auf die Systeme 03 bis 12 sind in `docs/decisions/SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md` festgehalten.
- ED-01 und damit der Roadmap-Entwurf sind ausdrücklich abgenommen.
- ED-02 trennt normale Diensttypen von den zusammengesetzten Einsatzmustern `D` und `Spr` und ist ausdrücklich abgenommen.
- ED-03 führt `WorkLocation`, die stark typisierte `WorkLocationId`, geprüfte Namen und erweiterbare semantische Farbkennungen ausschließlich in Domain ein.
- Cafeteria und Restaurant besitzen feste technische Kennungen und die bestätigten Farbkennungen `yellow` beziehungsweise `red`. Weitere Orte und Farbkennungen bleiben ohne feste Aufzählung möglich.
- Änderungen an Name oder Farbe erzeugen ein neues gültiges Einsatzortobjekt und bewahren die stabile Kennung; ungültige Eingaben liefern strukturierte Fehlercodes.
- ED-03 und sein Einsatzortmodell sind ausdrücklich abgenommen.
- ED-04 führt `ShiftType`, die stark typisierte `ShiftTypeId`, geprüfte Bezeichnungen, Einsatzortbezüge, Tabellenanzeigen und getrennte `ShiftStandardTime`-Werte ausschließlich in Domain ein.
- Frühdienst `F`, Spätdienst `S` sowie Cafeteria-Dienst A und B besitzen feste technische Kennungen und die bestätigten Standardzeiten. Cafeteria-Dienste verwenden statt eines Kürzels die spätere tatsächliche Zeitanzeige.
- Standardzeiten akzeptieren nur ganze Minuten auf 30-Minuten-Grenzen, müssen am selben Tag enden und liefern ihre Dauer in ganzen Minuten. Eine Änderung erzeugt eine neue Fassung und bewahrt die Diensttyp-Kennung.
- ED-04 und sein Diensttyp- und Standardzeitmodell sind ausdrücklich abgenommen.
- ED-05 führt die stark typisierte `ShiftPatternId` und einen gemeinsamen Einsatzmustervertrag ausschließlich in Domain ein.
- Der feste Doppeldienst `D` referenziert ausschließlich Früh- und Spätdienst im Restaurant. Seine drei Stunden Unterbrechung zählen nicht zu den zehn Stunden Standardarbeitszeit.
- Der samstägliche Springer `Spr` referenziert Cafeteria-Dienst B und anschließend den Spätdienst. Seine Wechselregel folgt dem tatsächlichen Ende des ersten Bedarfs; eine feste Wechselzeit oder konkrete Zuweisungsabschnitte werden noch nicht gespeichert.
- Die zusätzliche semantische Farbkennung `blue` des Springers wird zusammen mit `Spr` aus Domain an Application übergeben; Text bleibt weiterhin der primäre Informationsträger.
- ED-05 und sein Doppeldienst- und Springer-Modell sind ausdrücklich abgenommen.
- ED-06 führt eine konsistente unveränderliche Katalogabfrage für Einsatzorte, normale Diensttypen, `D` und `Spr` in Application ein und ist ausdrücklich abgenommen.
- Zwei getrennte Commands erlauben ausschließlich die bestätigten Änderungen: Name und Farbkennung eines Einsatzortes sowie die Standardzeit eines Diensttyps. Anlegen, Löschen und freie Musterkombinationen besitzen weiterhin keinen Anwendungsfall.
- Drei kleine Speicherverträge decken Kataloglesen, Einsatzortänderung und Standardzeitänderung ab.
- Validierungs-, Nicht-gefunden- und Änderungskonflikte werden mit stabilen Codes und verständlichen deutschen Meldungen zurückgegeben. Abbruchtoken werden vor und während der Speicheraufrufe weitergereicht.
- Eine Standardzeitänderung von Früh- oder Spätdienst wird vor dem Speichern zusätzlich gegen den festen Doppeldienst geprüft und darf dessen Reihenfolge, Überschneidungsfreiheit und Unterbrechung nicht verletzen.
- ED-07 implementiert diese drei Verträge mit einer lokalen SQLite-Datenbank ausschließlich in Infrastructure und ist ausdrücklich abgenommen.
- Die erste veröffentlichbare Migration legt Einsatzorte, Diensttypen und die Beziehungen von `D` und `Spr` an. Sie stellt Cafeteria, Restaurant und die vier bestätigten Diensttypen mit stabilen Kennungen idempotent bereit.
- Standardzeiten werden als ganze Minuten gespeichert. Fremdschlüssel, Zeitprüfungen sowie eindeutige Musterarten und Anzeigecodes werden durch SQLite selbst abgesichert.
- Die konkrete Adaptererzeugung liegt ausschließlich in `Desktop.Composition`; eine WPF-Fachseite oder produktive Datenbankdatei wird in ED-07 noch nicht gestartet beziehungsweise angelegt.
- Sechs Infrastructure-Tests mit echten temporären SQLite-Dateien prüfen leere Migration, wiederholte Initialisierung, Laden, Ändern, Neustart, Beziehungen, Eindeutigkeit, Konflikte und fehlgeschlagene Schreibvorgänge.
- ED-08 ersetzt das leere Hauptfenster durch eine deutsche Einsatzortverwaltung auf der gemeinsamen Seite „Einsatzorte und Diensttypen“.
- Cafeteria und Restaurant sind mit Name sowie zusätzlicher gelber beziehungsweise roter Text- und Farbkennzeichnung auswählbar. Name und Farbe können gespeichert werden; Anlegen, Löschen, Deaktivieren und Reaktivieren bleiben weiterhin ausgeschlossen.
- Die Oberfläche zeigt getrennte Lade-, Leer-, Arbeits-, Erfolgs-, Validierungs- und technische Fehlerzustände. Unerwartete technische Fehler werden ohne Namen, Eingabewerte oder lokale Pfade protokolliert.
- Die App erzeugt und verwendet ihre lokale Katalogdatenbank unter `%LocalAppData%\Salztal Dienstplanung\dienstplanung.db`; im Repository liegt weiterhin keine Datenbankdatei.
- Acht Desktop-Tests prüfen Laden, Auswahl, Leerzustand, laufende Vorgänge, gültiges Speichern, Validierung und technische Fehler.
- Die sichtbare Einsatzortverwaltung aus ED-08 wurde vom Auftraggeber geprüft und am 2026-09-13 ausdrücklich abgenommen.
- ED-09 zeigt unter jedem gewählten Einsatzort ausschließlich seine zugehörigen normalen Diensttypen. Beginn und Ende der Standardzeit lassen sich in 30-Minuten-Schritten auswählen und getrennt je Diensttyp speichern.
- Cafeteria-Dienste zeigen nach dem Speichern ihre Standardzeit als Anzeige, während Früh- und Spätdienst ihre festen Kürzel `F` und `S` behalten. Ungültige Zeitfolgen sowie eine Verletzung des festen Doppeldienstes werden deutsch gemeldet, ohne die gewählte Eingabe zu verlieren.
- `D` wird mit Restaurant, Früh- und Spätdienst, Unterbrechung ohne Arbeitszeit sowie Standardarbeitszeit angezeigt. `Spr` wird zusätzlich blau und als samstägliche Abfolge von Cafeteria-Dienst B zum Spätdienst dargestellt; eine feste Wechselzeit und freie Musterbearbeitung werden nicht angeboten.
- Die sichtbaren Diensttyp-, Doppeldienst- und Springer-Abläufe aus ED-09 wurden vom Auftraggeber geprüft und am 2026-09-13 ausdrücklich abgenommen.
- ED-10 hat Domain, Application, Infrastructure und Desktop gegen den bestätigten Umfang von System 04 abgeglichen. Die stabilen Kennungen und unveränderlichen Katalogmomentaufnahmen bilden eine eindeutige Übergabe an das spätere System 03.
- Der System-03-Entwurf speichert später nur `WorkLocationId`- und `ShiftTypeId`-Freigaben und übernimmt keine Katalogdaten. `D` und `Spr` erhalten keine eigenen Mitarbeiterfreigaben; ihre Voraussetzungen ergeben sich aus den referenzierten normalen Diensten und Einsatzorten.
- ED-10 und damit System 04 sind am 2026-09-13 ausdrücklich abgenommen. Die Roadmap und ihre beantwortete Fragen-Datei sind unter `docs/roadmaps/completed` archiviert.
- Der weiterhin nicht abgenommene System-03-Entwurf wurde nach dieser Archivierung unter `docs/roadmaps/active` wieder aufgenommen. Noch existiert kein Mitarbeiter-Fachcode.
- 14 Application-Tests, 46 Domain-Tests, 6 Infrastructure-Tests, 13 Desktop-Tests und 13 Architekturtests bestehen. Die vollständige Solution kompiliert mit 0 Warnungen und 0 Fehlern.
- Der noch nicht abgenommene System-03-Entwurf liegt begründet unter `docs/roadmaps/paused` und wird erst nach System 04 erneut geprüft.
- TG-07 mit den sieben getrennten Testprojekten für die sechs Produktionsmodule und die Architektur ist abgenommen.
- TG-08 mit zwölf Architekturtests und den drei nachgewiesenen Fehlermutationen ist abgenommen.
- TG-09 und damit System 02 sind nach dem manuellen sichtbaren Starttest vollständig abgenommen und archiviert.
- TG-08 hat zwölf Architektur-Testfälle gegen reale Projektdateien, Produktionsassemblies und Desktop-Quellen angelegt.
- Die Tests sichern den erwarteten Projektbestand, erlaubte Projektreferenzen, Zyklusfreiheit, Paketgrenzen, Testprojektreferenzen und die besondere `Desktop.Composition`-Grenze.
- Drei kontrollierte Fehlermutationen für Projektreferenz, Paketposition und Namespace-Nutzung wurden zuverlässig erkannt und danach vollständig entfernt.
- Jedes modulspezifische Testprojekt referenziert nur sein Produktionsprojekt; das Architektur-Testprojekt darf alle Produktionsprojekte untersuchen.
- xUnit v3 `4.0.1` und die Microsoft Testing Platform bilden die zentrale .NET-10-Testinfrastruktur.
- Alle sieben Testprojekte besitzen Paket-Lockdateien und sind in der Solution enthalten.
- CommunityToolkit.Mvvm `8.4.2` wird nur in Desktop, Microsoft.EntityFrameworkCore.Sqlite `10.0.12` sowie das private Designtime-Paket Microsoft.EntityFrameworkCore.Design `10.0.12` nur in Infrastructure und Google.OrTools `9.15.6755` nur in Planning direkt referenziert.
- Domain, Application und Excel besitzen weiterhin keine direkte Paketreferenz.
- Paketversionen sind zentral festgelegt; `packages.lock.json` liegt für alle 13 Projekte vor.
- Desktop besitzt nur die vorgesehenen Projektreferenzen auf Application und die technischen Adaptermodule.
- Das Hauptfenster zeigt die deutsche Einsatzort- und Diensttypverwaltung und weist die weiterhin fehlenden Dienstplanfunktionen klar aus.
- Die vollständige Solution mit 13 Projekten kompiliert mit 0 Warnungen und 0 Fehlern.
- Normale und gesperrte Paketwiederherstellung bestehen; die aktuelle NuGet-Sicherheitsprüfung meldet keine bekannten verwundbaren Pakete.
- Der vollständige Testlauf des damaligen technischen Grundgerüsts bestand mit 12 von 12 Architekturtests; System 04 ergänzt einen weiteren Architekturtest.
- TG-09 hat Solution, Projekt- und Testbestand, zentrale Konfiguration, Paketgrenzen und Architekturtests gemeinsam abgeglichen und ist abgenommen.
- Die gesperrte Wiederherstellung, der vollständige Build mit 0 Warnungen und 0 Fehlern sowie aktuell 13 von 13 Architekturtests bestehen erneut.
- Die Desktop-App startete technisch mit reagierendem Hauptfenster und dem Titel „Salztal Dienstplanung“ und wurde regulär mit Exitcode 0 beendet.
- Der Auftraggeber hat den sichtbaren lokalen Start, den Fenstertitel und die beiden deutschen Hinweistexte bestätigt.
- Es wurden noch keine fachlichen Planungs- oder Excel-Abläufe und keine Mitarbeiter- oder Dienstplanbedienung angelegt.

## Noch nicht begonnen

- Die Implementierung von System 03
- Systeme 05 bis 14
- System 15 – Portable Windows-Auslieferung und Endabnahme der Kernversion
- System 16 – Zeitkonten als spätere Ausbaustufe

Die genaue Einordnung steht in der [Master-Roadmap](MASTER_ROADMAP.md). Vor jedem System wird eine eigene Teil-Roadmap erstellt und abgenommen.

## Bestätigte Entscheidungen für System 04

- Cafeteria und Restaurant als bearbeitbare Startwerte; weitere Einträge technisch vorbereitet,
- Cafeteria gelb, Restaurant rot und Springer `Spr` blau, jeweils zusätzlich mit Text,
- normale Diensttypen mit genau einem Einsatzort und bearbeitbarer Standardzeit,
- Doppeldienst `D` und Springer `Spr` als zusammengesetzte Einsatzmuster, die niemals selbst von einem einzelnen Bedarf verlangt werden,
- tatsächlich zu besetzende Zeit im Bedarf mit genau einem verlangten Diensttyp,
- Standardänderungen für zukünftige Vorgaben und getrennte Ausnahmen für ein Datum,
- 30-Minuten-Eingabeschritte, minutengenaue Speicherung und keine Dienste über Mitternacht,
- Doppeldienst `D` ausschließlich als Frühdienst plus Spätdienst im Restaurant,
- samstäglicher Springer `Spr` nur als Notfall mit sichtbar verbleibender Teilunterdeckung vor dem Restaurantwechsel,
- erste Bedienfassung mit Anzeigen und Bearbeiten auf einer gemeinsamen Seite; weitere Stammdatenfunktionen später.

Die Antworten sind in der Fragen-Datei und im Entscheidungsdokument festgehalten. Roadmap, Fachmodell und die Implementierungsschritte ED-01 bis ED-09 sind abgenommen; ED-10 benötigt noch die ausdrückliche Abschlussabnahme von System 04.

## Offene Detailentscheidungen für spätere Schritte

- Verhalten beim späteren Löschen noch aktiv referenzierter Stammdaten,
- genaue Regelpriorität der Springer-Teilunterdeckung.

## Offene Entscheidungen für das zurückgestellte System 03

- Namensaufteilung, optionale Personalnummer und Umgang mit gleichen Namen,
- Löschen oder Deaktivieren von Mitarbeitenden,
- Aufbau und konkrete Werte der Arbeitszeitmodelle,
- konkrete Qualifikationen und notwendige Eigenschaften des Qualifikationskatalogs,
- notwendige Felder und Funktionen der ersten Mitarbeiteroberfläche.

Die Grenze ist bereits bestätigt: System 04 besitzt Einsatzorte und Diensttypen; System 03 ordnet Mitarbeitenden einzelne Einsatzort- und Diensttypfreigaben zu. Die übrigen Fragen stehen im wieder aktivierten Roadmap-Entwurf und werden in MA-02 vor jeder Implementierung von System 03 gemeinsam beantwortet und abgenommen.

## Offene Entscheidungen für spätere Systeme

- weitere, noch nicht bestätigte Personal- und Stundenbedarfe außerhalb der jetzt festgelegten Startwerte,
- vollständige zwingende und priorisierte weiche Regeln,
- Inhalt und Aufbau der noch bereitzustellenden Excel-Vorlage,
- endgültige Bestätigung der Excel-Bibliothek nach dem Vorlagentest,
- praktische Voraussetzungen der portablen Ausgabe auf dem vorgesehenen Windows-11-Rechner.

Diese Entscheidungen sind für den Abschluss der Dokumentationsgrundlage noch nicht erforderlich. Sie werden vor dem jeweils betroffenen System geklärt und nicht vorweggenommen.

## Echte Blockaden

Für MA-01 besteht keine technische Blockade. Der aktualisierte System-03-Roadmap-Entwurf benötigt vor MA-02 und jeder Implementierung die ausdrückliche Abnahme.

Das zuvor unversionierte Beispielbild mit echten Namen und konkreten Plandaten ist nicht mehr im Arbeitsordner vorhanden. Die daraus benötigten Fachinformationen sind nur abstrahiert und ohne personenbezogene Daten dokumentiert.

Die fehlende Excel-Vorlage blockiert später den Excel-Vorlagentest und System 13, aber nicht die gegenwärtige Projektgrundlage.

## Offene Prüf- und Abnahmegates

- Alle 13 Produktions- und Testprojekte kompilieren erfolgreich; 14 von 14 Application-Tests, 46 von 46 Domain-Tests, 6 von 6 Infrastructure-Tests, 13 von 13 Desktop-Tests und 13 von 13 Architekturtests bestehen.
- Der vollständige sichtbare Einsatzort-, Diensttyp-, Doppeldienst- und Springer-Ablauf wurde schrittweise manuell geprüft und durch den Auftraggeber bestätigt.
- Die Paketwiederherstellung und der Build bestätigen noch keinen OR-Tools-Lauf auf einem sauberen Zielsystem. Die Visual-C++-x64-Laufzeitvoraussetzung wird erst bei der portablen Auslieferung praktisch geprüft.
- Die SQLite-Katalogspeicherung ist mit temporären Testdateien geprüft und die lokale App-Datenbank wurde beim sichtbaren Start angelegt. Planungsengine und Excel-Export wurden noch nicht implementiert.
- Portable Windows-Ausgabe und Start auf einem geeigneten Windows-11-System stehen noch aus.
- Die fachliche Endabnahme durch die Service-Leitung steht noch aus.

Keines dieser späteren Gates wird vorzeitig als bestanden geführt.

## Nächster minimaler Schritt

MA-01 mit dem wieder aktivierten System-03-Roadmap-Entwurf ausdrücklich abnehmen oder Änderungswünsche nennen. Danach werden in MA-02 die offenen Mitarbeiterfragen geklärt; System-03-Fachcode beginnt weiterhin erst nach diesen Freigaben.
