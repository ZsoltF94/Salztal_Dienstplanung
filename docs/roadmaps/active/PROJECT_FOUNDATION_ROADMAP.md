# Teil-Roadmap: Projektgrundlage

Status: Aktiv – PF-04 umgesetzt und geprüft, wartet auf Abnahme

## Ziel

Diese Roadmap bereitet das Projekt so vor, dass alle späteren Systeme nachvollziehbar, modular und in kleinen abnehmbaren Schritten entwickelt werden können. Am Ende dieser Roadmap besitzt das Projekt eine dokumentierte Architektur, verbindliche Arbeits- und Qualitätsregeln, eine Master-Roadmap sowie einen wahrheitsgemäßen Projektstatus.

Diese Roadmap erzeugt noch keine funktionsfähige Dienstplan-App.

## Verbindliche Grundlage

- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`, abgenommen am 2026-09-13
- Zielplattform: Windows 10 und Windows 11
- Einzelbenutzer-Anwendung für die Service-Leitung
- vollständig offline und lokal gespeichert
- bevorzugt portable Auslieferung
- deutsche Bedienoberfläche
- Excel-Ausgabe nach einer später gelieferten Vorlage
- kleinschrittige Umsetzung mit Abnahme nach jedem Schritt

## Umfang dieser Roadmap

- Versionsverwaltung und minimale Dokumentationsstruktur vorbereiten
- technische Architektur begründet auswählen und dokumentieren
- Regeln für sauberen, wartbaren Code definieren
- verbindliche Agenten- und Arbeitsanweisungen festhalten
- die Systeme der App in einer Master-Roadmap ordnen
- einen zentralen, wahrheitsgemäßen Projektstatus einführen
- Dokumente gegenseitig verknüpfen und auf Widersprüche prüfen

## Nicht Bestandteil dieser Roadmap

- konkrete Mitarbeiter- oder Arbeitszeitmodelle
- echte Diensttypen und Dienstzeiten
- konkrete Bedarfs- und Planungsregeln
- Benutzeroberfläche der App
- Planungsalgorithmus
- Datenbankimplementierung
- Excel-Exportimplementierung
- produktive Windows-Builds

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` begonnen oder wartet auf Abnahme
- `[x]` geprüft und abgenommen
- `[!]` blockiert; Ursache muss direkt beim Schritt stehen

Ein Schritt gilt erst nach Prüfung und ausdrücklicher Abnahme als abgeschlossen.

## Schritte

### PF-01 – Roadmap für die Projektgrundlage erstellen

Status: `[x]` – abgenommen am 2026-09-13

Ergebnis:

- Diese Teil-Roadmap beschreibt Umfang, Reihenfolge, Prüfungen und Abnahmebedingungen der Projektgrundlage.
- Die abgenommenen Grundlagen sind als verbindlicher Ausgangspunkt referenziert.
- Implementierungsarbeit ist ausdrücklich ausgeschlossen.

Prüfung:

- Alle vom Auftraggeber verlangten Leitdokumente sind als eigene spätere Schritte enthalten.
- Jeder Schritt besitzt ein klar begrenztes Ergebnis.
- Nach jedem Schritt ist eine Abnahme vorgesehen.

Abnahmebedingung:

- Der Auftraggeber bestätigt diese Roadmap oder nennt gewünschte Änderungen.

### PF-02 – Versionsverwaltung und Dokumentationsstruktur vorbereiten

Status: `[x]` – abgenommen am 2026-09-13

Geplantes Ergebnis:

- Der Projektordner wird als lokales Git-Repository initialisiert.
- Eine für das gewählte Projekt geeignete `.gitignore` wird angelegt.
- Die minimale Ordnerstruktur für aktive, pausierte und abgeschlossene Teil-Roadmaps wird festgelegt.
- Es werden noch keine App-Projekte, Frameworks oder Abhängigkeiten installiert.

Prüfung:

- `git status` funktioniert im Projektordner.
- Nur die vereinbarte Grundstruktur und Dokumente werden erfasst.
- Temporäre Dateien und lokale Build-Artefakte sind ausgeschlossen.

Tatsächlich umgesetzt:

- Lokales Git-Repository mit Hauptbranch `main` initialisiert.
- Allgemeine `.gitignore` für Windows-, Editor-, Build-, Test- und temporäre Dateien angelegt.
- Lokale Mitarbeiter- und Plandaten, Datenbanken, Sicherungen und erzeugte Exporte werden ignoriert.
- Excel-Vorlagen bleiben absichtlich versionierbar.
- Ordner für aktive, pausierte und abgeschlossene Teil-Roadmaps angelegt.
- Noch kein Commit und keine Verbindung zu einem entfernten Repository erstellt.

### PF-03 – Projekt mit GitHub verbinden

Status: `[x]` – abgenommen am 2026-09-13

Bestätigtes Repository:

- `https://github.com/ZsoltF94/Salztal_Dienstplanung.git`

Geplantes Ergebnis:

- Name und Sichtbarkeit des GitHub-Repositorys werden vor dem externen Anlegen bestätigt.
- Wegen der späteren Verarbeitung von Mitarbeiter- und Plandaten wird ein privates Repository empfohlen.
- Das GitHub-Repository wird im Konto des Auftraggebers erstellt und als `origin` verbunden.
- Die vorhandene Dokumentationsgrundlage wird in einem ersten Commit gesichert und auf den Branch `main` übertragen.
- Lokale Anwendungsdaten, Sicherungen und Exporte bleiben durch die `.gitignore` vom Repository ausgeschlossen.

Prüfung:

- `origin` zeigt auf das bestätigte GitHub-Repository.
- Der lokale Branch `main` verfolgt den entfernten Branch `origin/main`.
- Der erste Commit ist lokal und auf GitHub vorhanden.
- Es wurden keine lokalen Mitarbeiter-, Planungs-, Datenbank- oder Sicherungsdaten übertragen.

Tatsächlich umgesetzt:

- Das leere GitHub-Repository wurde unter der bestätigten Adresse als `origin` verbunden.
- Die geprüfte Dokumentationsgrundlage und `.gitignore` wurden in einem ersten Commit gesichert.
- Der Branch `main` wurde auf GitHub veröffentlicht und verfolgt `origin/main`.
- Es wurden keine lokalen Anwendungs-, Mitarbeiter-, Planungs-, Datenbank-, Sicherungs- oder Exportdaten übertragen.

### PF-04 – Lesebereich für die Service-Leitung anlegen

Status: `[~]` – umgesetzt und geprüft, wartet auf Abnahme

Geplantes Ergebnis:

- Im Ordner `Service-Leitung` entsteht eine leicht verständliche Startseite.
- Der aktuelle Projektstand wird ohne technische Fachsprache zusammengefasst.
- Bestätigte Grundlagen und Entscheidungen werden für die Service-Leitung verständlich erklärt.
- Die Dokumente enthalten keine echten Mitarbeiter-, Planungs- oder Gesundheitsdaten.
- Der Lesebereich wird nach jedem abgenommenen Projektschritt wahrheitsgemäß aktualisiert.

Prüfung:

- Die Service-Leitung findet von der Startseite direkt zum aktuellen Stand und zu den Grundlagen.
- Technische Begriffe werden vermieden oder einfach erklärt.
- Der dargestellte Stand stimmt mit der aktiven Roadmap überein.
- Noch nicht entwickelte Funktionen sind eindeutig als geplant gekennzeichnet.

Tatsächlich umgesetzt:

- `Service-Leitung/README.md` dient als kurze Startseite und verlinkt alle verständlichen Informationen.
- `Service-Leitung/AKTUELLER_STAND.md` trennt fertig abgenommene, aktuell bearbeitete und noch nicht gebaute Inhalte.
- `Service-Leitung/GRUNDLAGEN_UND_ENTSCHEIDUNGEN.md` erklärt die bestätigte Funktionsweise ohne technische Architekturdetails.
- Der Lesebereich enthält keine echten Mitarbeiter-, Planungs-, Gesundheits- oder Zugangsdaten.

### PF-05 – Technische Architektur auswählen und `ARCHITECTURE.md` anlegen

Status: `[ ]`

Geplantes Ergebnis:

- Geeignete, aktuell unterstützte Technologien werden anhand offizieller Quellen verglichen.
- Bewertet werden mindestens Windows-10/11-Unterstützung, Offlinebetrieb, portable Auslieferung, lokale Datenbank, Excel-Erzeugung, automatische Planung, Wartbarkeit und Testbarkeit.
- Die empfohlene Lösung wird vor einer Festlegung verständlich mit Vor- und Nachteilen vorgestellt.
- Nach Zustimmung wird `ARCHITECTURE.md` mit Systemgrenzen, Modulen, Datenfluss, Speicherstrategie, Sicherheitsgrenzen und Erweiterungspunkten angelegt.
- Zeitkonten bleiben als nachrangiges Modul vorgesehen.

Prüfung:

- Jede wesentliche Technologieentscheidung besitzt eine nachvollziehbare Begründung.
- Die Architektur erfüllt die abgenommenen Grundlagen.
- Spätere Einsatzorte, Diensttypen und Regeln können ergänzt werden, ohne Kernbereiche neu zu bauen.
- Die Planungslogik bleibt von Benutzeroberfläche, Speicherung und Excel-Export getrennt.

### PF-06 – Qualitätsregeln in `CLEANCODE.md` festlegen

Status: `[ ]`

Geplantes Ergebnis:

- `CLEANCODE.md` definiert verständliche Benennung, kleine Verantwortungsbereiche, Modulgrenzen und Fehlerbehandlung.
- Regeln für Tests, Datenmigrationen, Protokollierung und technische Dokumentation werden passend zur gewählten Architektur festgelegt.
- Fachbegriffe der Anwendung werden einheitlich auf Deutsch oder technisch begründet auf Englisch verwendet.
- Vermeidbare Abhängigkeiten und übergroße Klassen beziehungsweise Module werden untersagt.

Prüfung:

- Alle Regeln sind konkret überprüfbar.
- Das Dokument enthält keine allgemeinen Floskeln ohne praktische Bedeutung.
- Es widerspricht nicht der Architektur.

### PF-07 – Arbeitsanweisungen in `AGENTS.md` festlegen

Status: `[ ]`

Geplantes Ergebnis:

- `AGENTS.md` macht den kleinschrittigen Roadmap- und Abnahmeprozess verbindlich.
- Vor jedem System wird eine eigene Teil-Roadmap verlangt.
- Pro freigegebenem Schritt werden Umfang, Prüfung, Ergebnis, Risiken und nächster Minimalschritt berichtet.
- Nicht bestätigte Gates dürfen nicht als abgeschlossen markiert werden.
- Bestehende Benutzeränderungen müssen erhalten bleiben.
- Architektur- und Clean-Code-Regeln werden als verbindliche Referenzen eingebunden.
- Der aktuelle Stand im Lesebereich für die Service-Leitung muss nach jedem abgenommenen Schritt mitgeführt werden.

Prüfung:

- Die Anweisungen sind eindeutig und im Projektalltag ausführbar.
- Eigenmächtige Erweiterungen über einen freigegebenen Schritt hinaus werden verhindert.
- Dokumentationsstatus und tatsächlicher Entwicklungsstand müssen übereinstimmen.

### PF-08 – Systemübersicht in `MASTER_ROADMAP.md` anlegen

Status: `[ ]`

Geplantes Ergebnis:

- `MASTER_ROADMAP.md` ordnet die geplanten Systeme und ihre Abhängigkeiten.
- Jedes System verweist auf eine eigene Teil-Roadmap, sobald diese begonnen wird.
- Die Master-Roadmap bleibt bewusst grob und enthält keine versteckte Detailimplementierung.
- Zeitkonten werden sichtbar als spätere, nicht vorrangige Ausbaustufe eingeordnet.

Voraussichtliche Systembereiche:

1. Projektgrundlage
2. Mitarbeiter-, Arbeitszeitmodell- und Qualifikationsverwaltung
3. Einsatzorte, Diensttypen und Doppeldienste
4. Personal- und Schichtbedarf
5. Verfügbarkeiten und Abwesenheiten
6. Regelkatalog mit zwingenden und priorisierten weichen Regeln
7. automatische Plangenerierung
8. Konflikterklärung und Lösungsvorschläge
9. Planansicht, manuelle Bearbeitung und Sperren
10. Planhistorie, Versionen und Abnahme
11. Excel-Export nach Vorlage
12. lokale Datensicherung und Wiederherstellung
13. Zeitkonten als spätere Ausbaustufe
14. portable Windows-Auslieferung und Endabnahme

Prüfung:

- Alle bestätigten Funktionsbereiche sind enthalten.
- Abhängigkeiten und Reihenfolge sind plausibel.
- Kein System wird ohne eigene freigegebene Teil-Roadmap zur Umsetzung freigegeben.

### PF-09 – Zentralen Stand in `STATUS.md` anlegen

Status: `[ ]`

Geplantes Ergebnis:

- `STATUS.md` zeigt den aktuellen Projektstand in kompakter Form.
- Enthalten sind die aktive Roadmap, der zuletzt abgenommene Schritt, der nächste vorgeschlagene Minimalschritt, offene Entscheidungen und echte Blockaden.
- Ausstehende Tests, Builds oder Abnahmen bleiben ausdrücklich offen.
- `STATUS.md` und `Service-Leitung/AKTUELLER_STAND.md` müssen inhaltlich denselben tatsächlichen Projektstand zeigen.

Prüfung:

- Der Status lässt sich mit Roadmaps und vorhandenen Dateien abgleichen.
- Es werden keine geplanten Funktionen als umgesetzt dargestellt.
- Das Dokument nennt genau einen nächsten Minimalschritt.

### PF-10 – Leitdokumente gemeinsam prüfen und Projektgrundlage abschließen

Status: `[ ]`

Geplantes Ergebnis:

- `AGENTS.md`, `CLEANCODE.md`, `ARCHITECTURE.md`, `MASTER_ROADMAP.md`, `STATUS.md` und der Lesebereich `Service-Leitung` werden auf widersprüchliche Aussagen und ungültige Verweise geprüft.
- Die aktive Teil-Roadmap und der zentrale Status werden wahrheitsgemäß aktualisiert.
- Die Projektgrundlage wird erst nach abschließender Abnahme als abgeschlossen archiviert.

Prüfung:

- Alle lokalen Markdown-Verweise zeigen auf vorhandene Dateien.
- Eine Suche findet keine veralteten Dateinamen oder Statusangaben.
- `git diff --check` meldet keine Whitespace-Fehler.
- Es existiert weiterhin noch keine fachliche Implementierung der App.

## Berichtsschema nach jedem Schritt

Nach jedem einzelnen Schritt enthält der Bericht:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. durchgeführte Prüfungen und deren Ergebnis,
4. offene Punkte oder Risiken,
5. nächster minimaler Schritt,
6. ausdrückliche Bitte um Abnahme.

## Nächster minimaler Schritt

Nach Abnahme von PF-04: PF-05 – Technische Architektur auswählen und `ARCHITECTURE.md` anlegen.
