# Projektstatus

Stand: 2026-09-13

Status dieses Dokuments: Aktuell – System 02 abgeschlossen und archiviert

## Aktueller Überblick

| Bereich | Aktueller Stand |
|---|---|
| Projektphase | Übergang vom technischen Grundgerüst zum ersten Fachsystem |
| Aktives System | Keines; Auswahl zwischen System 03 und System 04 steht aus |
| Aktive Teil-Roadmap | Keine |
| Aktueller Stand | System 02 vollständig geprüft, abgenommen und archiviert |
| Zuletzt abgenommener Schritt | TG-09 – Technisches Grundgerüst gemeinsam abschließen |
| Funktionsfähige App | Nur leere Startanwendung; noch keine Dienstplanfunktion |
| Echte Mitarbeiter- oder Plandaten im Repository | Keine |

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

- TG-07 mit den sieben getrennten Testprojekten für die sechs Produktionsmodule und die Architektur ist abgenommen.
- TG-08 mit zwölf Architekturtests und den drei nachgewiesenen Fehlermutationen ist abgenommen.
- TG-09 und damit System 02 sind nach dem manuellen sichtbaren Starttest vollständig abgenommen und archiviert.
- TG-08 hat zwölf Architektur-Testfälle gegen reale Projektdateien, Produktionsassemblies und Desktop-Quellen angelegt.
- Die Tests sichern den erwarteten Projektbestand, erlaubte Projektreferenzen, Zyklusfreiheit, Paketgrenzen, Testprojektreferenzen und die besondere `Desktop.Composition`-Grenze.
- Drei kontrollierte Fehlermutationen für Projektreferenz, Paketposition und Namespace-Nutzung wurden zuverlässig erkannt und danach vollständig entfernt.
- Jedes modulspezifische Testprojekt referenziert nur sein Produktionsprojekt; das Architektur-Testprojekt darf alle Produktionsprojekte untersuchen.
- xUnit v3 `4.0.1` und die Microsoft Testing Platform bilden die zentrale .NET-10-Testinfrastruktur.
- Alle sieben Testprojekte besitzen Paket-Lockdateien und sind in der Solution enthalten.
- CommunityToolkit.Mvvm `8.4.2` wird nur in Desktop, Microsoft.EntityFrameworkCore.Sqlite `10.0.12` nur in Infrastructure und Google.OrTools `9.15.6755` nur in Planning direkt referenziert.
- Domain, Application und Excel besitzen weiterhin keine direkte Paketreferenz.
- Paketversionen sind zentral festgelegt; `packages.lock.json` liegt für alle 13 Projekte vor.
- Desktop besitzt nur die vorgesehenen Projektreferenzen auf Application und die technischen Adaptermodule.
- Das Hauptfenster zeigt einen deutschen Hinweis auf das vorhandene Grundgerüst und die noch fehlenden Dienstplanfunktionen.
- Die vollständige Solution mit 13 Projekten kompiliert mit 0 Warnungen und 0 Fehlern.
- Normale und gesperrte Paketwiederherstellung bestehen; die aktuelle NuGet-Sicherheitsprüfung meldet keine bekannten verwundbaren Pakete.
- Der vollständige Testlauf besteht mit 12 von 12 Architekturtests.
- TG-09 hat Solution, Projekt- und Testbestand, zentrale Konfiguration, Paketgrenzen und Architekturtests gemeinsam abgeglichen und ist abgenommen.
- Die gesperrte Wiederherstellung, der vollständige Build mit 0 Warnungen und 0 Fehlern sowie 12 von 12 Architekturtests bestehen erneut.
- Die Desktop-App startete technisch mit reagierendem Hauptfenster und dem Titel „Salztal Dienstplanung“ und wurde regulär mit Exitcode 0 beendet.
- Der Auftraggeber hat den sichtbaren lokalen Start, den Fenstertitel und die beiden deutschen Hinweistexte bestätigt.
- Es wurden noch keine Adapterimplementierung, Fachtests, Excel-Bibliothek oder Fachfunktionen angelegt.

## Noch nicht begonnen

- Systeme 03 bis 14
- System 15 – Portable Windows-Auslieferung und Endabnahme der Kernversion
- System 16 – Zeitkonten als spätere Ausbaustufe

Die genaue Einordnung steht in der [Master-Roadmap](MASTER_ROADMAP.md). Vor jedem System wird eine eigene Teil-Roadmap erstellt und abgenommen.

## Offene Entscheidungen für spätere Systeme

- konkrete Arbeitszeitmodelle, Qualifikationen und Einsatzfreigaben,
- konkrete Diensttypen, Dienstzeiten und Doppeldienste,
- tatsächlicher Personal- und Stundenbedarf,
- vollständige zwingende und priorisierte weiche Regeln,
- Inhalt und Aufbau der noch bereitzustellenden Excel-Vorlage,
- endgültige Bestätigung der Excel-Bibliothek nach dem Vorlagentest,
- praktische Voraussetzungen der portablen Ausgabe auf dem vorgesehenen Windows-11-Rechner.

Diese Entscheidungen sind für den Abschluss der Dokumentationsgrundlage noch nicht erforderlich. Sie werden vor dem jeweils betroffenen System geklärt und nicht vorweggenommen.

## Echte Blockaden

Aktuell besteht keine technische oder fachliche Blockade für die Vorbereitung von System 03 oder System 04.

Die fehlende Excel-Vorlage blockiert später den Excel-Vorlagentest und System 13, aber nicht die gegenwärtige Projektgrundlage.

## Offene Prüf- und Abnahmegates

- Alle 13 Produktions- und Testprojekte kompilieren erfolgreich; 12 von 12 Architekturtests bestehen. Fachtests entstehen erst mit den späteren Fachsystemen.
- Die leere WPF-App wurde lokal technisch gestartet und geschlossen; Fenstertitel und Hinweistexte wurden manuell bestätigt. Fachliche Bedienabläufe existieren noch nicht.
- Die Paketwiederherstellung und der Build bestätigen noch keinen OR-Tools-Lauf auf einem sauberen Zielsystem. Die Visual-C++-x64-Laufzeitvoraussetzung wird erst bei der portablen Auslieferung praktisch geprüft.
- Datenbank, Planungsengine und Excel-Export wurden noch nicht implementiert oder praktisch geprüft.
- Portable Windows-Ausgabe und Start auf einem geeigneten Windows-11-System stehen noch aus.
- Die fachliche Endabnahme durch die Service-Leitung steht noch aus.

Keines dieser späteren Gates wird vorzeitig als bestanden geführt.

## Nächster minimaler Schritt

Entscheiden, ob als Nächstes System 03 – Mitarbeitende, Arbeitszeitmodelle und Qualifikationen – oder System 04 – Einsatzorte und Diensttypen – vorbereitet wird. Danach wird zunächst nur die zugehörige kleinschrittige Teil-Roadmap entworfen und zur Abnahme vorgelegt.
