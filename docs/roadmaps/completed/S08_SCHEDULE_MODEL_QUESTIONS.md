# Fragenkatalog System 08 – Planmodell und Planungszeiträume

Status: Alle Ausgangsfragen beantwortet; System 08 am 2026-09-16 ausdrücklich abgenommen und archiviert

## Zweck und Grenze

Dieser Fragenkatalog hält die fachlichen Entscheidungen fest, die vor dem Entwurf der Teil-Roadmap für System 08 benötigt werden. Er ergänzt die bereits abgenommenen Grundlagen und Übergaben aus den Systemen 03 bis 07.

Der Fragenkatalog ist weder eine Teil-Roadmap noch eine Freigabe für Domain-, Application-, Infrastructure-, Desktop- oder Datenbankänderungen. Erst eine eigene, ausdrücklich abgenommene System-08-Teil-Roadmap darf den ersten Implementierungsschritt freigeben.

## Gelesene und berücksichtigte Grundlagen

- `AGENTS.md`
- `GRUNDLAGEN_FRAGEN_UND_ENTSCHEIDUNGEN.md`
- `ARCHITECTURE.md`
- `CLEANCODE.md`
- `MASTER_ROADMAP.md`
- `STATUS.md`
- `Service-Leitung/README.md`
- `Service-Leitung/AKTUELLER_STAND.md`
- `Service-Leitung/GRUNDLAGEN_UND_ENTSCHEIDUNGEN.md`
- `docs/roadmaps/completed/S03_EMPLOYEES_EMPLOYEE_TYPES_SHIFT_ELIGIBILITY_ROADMAP.md`
- `docs/roadmaps/completed/S05_STAFFING_DEMAND_ROADMAP.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_QUESTIONS.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_ROADMAP.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_QUESTIONS.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md`
- `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_EXAMPLES.md`

## Bereits bestätigter Ausgangsrahmen

- Eine Planungswoche läuft von Montag bis Sonntag.
- Die bestehende Ansicht „Dienstplan SER“ zeigt 21 Tage ab einem ausgewählten Montag.
- Ein leeres Tagesfeld einer aktiven Person bedeutet grundsätzlich verfügbar. Eine zusätzliche Auswahl teilnehmender Mitarbeitender vor jedem Lauf gibt es nicht.
- `U`, `K` und rote `X` bleiben geschützte Eingaben der Service-Leitung. Schwarze `X` entstehen erst durch die spätere Generierung.
- Typ1 wird vor der Generierung manuell eingetragen, niemals automatisch geplant und durch die Generierung nicht verändert.
- In jeder nicht vollständig abwesenden Typ1-Woche muss mindestens ein Typ1-Dienst vorgetragen sein.
- Ein normaler Bedarfsplatz wird vollständig durch eine Person gedeckt oder bleibt vollständig ungedeckt. Nur das bestätigte `Spr`-Muster darf eine Teildeckung erzeugen.
- Manuelle Zusatzbesetzungen werden unabhängig von freien Bedarfsplätzen modelliert und verändern den Bedarf nicht.
- System 08 stellt Planmodell, Planungszeitraum, Planungsmomentaufnahme und lokale Speicherung bereit. Automatische Generierung, Konflikterklärung, allgemeine manuelle Planbearbeitung und unveränderliche Planversionen bleiben Eigentum der Systeme 09 bis 12.

## A – Planungszeitraum

### S08-01 – Länge des Planungszeitraums

Soll die erste Fassung immer genau drei Wochen planen oder bereits eine einstellbare Zahl vollständiger Wochen unterstützen?

Antwort:

- Die erste Fassung plant immer genau drei vollständige Wochen.
- Der Zeitraum beginnt an einem ausgewählten Montag und endet mit dem dritten Sonntag.
- Eine einstellbare Zahl von Planungswochen gehört nicht zu System 08.

### S08-02 – Drei-Wochen-Regel bei anderer Zeitraumlänge

Wie soll die Regel zu mindestens einem vollständig freien Wochenende bei einem kürzeren oder längeren Zeitraum wirken?

Antwort:

- Für System 08 ist keine zusätzliche Entscheidung erforderlich, weil immer genau drei Wochen geplant werden.
- Die bestätigte Regel wird genau gegen diesen vollständigen Drei-Wochen-Zeitraum bewertet.

### S08-03 – Überschneidende Planungszeiträume

Darf ein bereits geplanter Zeitraum ganz oder teilweise in einen neuen Planungszeitraum aufgenommen werden?

Antwort:

- Nein. Ein bereits gespeicherter Planungszeitraum darf nicht ganz oder teilweise in einen neuen Planungszeitraum aufgenommen werden.
- Arbeit an denselben Kalendertagen erfolgt am vorhandenen Plan und erzeugt keinen zweiten überlappenden Plan.
- Die spätere Roadmap muss eine eindeutige strukturierte Ablehnung jeder Überschneidung vorsehen.

Beispiel:

Ein gespeicherter Plan reicht vom 5. bis 25. Oktober. Ein neuer Plan vom 12. Oktober bis 1. November ist unzulässig, weil sich beide Zeiträume überschneiden. Der nächste überschneidungsfreie Zeitraum kann frühestens am 26. Oktober beginnen.

## B – Planungsmomentaufnahme und Vorgeschichte

### S08-04 – Zeitpunkt und Aktualisierung der Momentaufnahme

Wann werden Mitarbeitende, Typwerte, Einsatzfreigaben, Abwesenheiten, Bedarfe, Regelkatalog und Laufoptionen für einen Planungslauf festgehalten?

Antwort:

- Die Service-Leitung wählt den Drei-Wochen-Zeitraum und trägt Typ1-Dienste ein.
- Mit einer ausdrücklichen Aktion „Planung vorbereiten“ entsteht eine unveränderliche Planungsmomentaufnahme.
- Ändern sich anschließend Stammdaten, Bedarfe, Abwesenheiten oder andere Eingangsdaten, wird die Momentaufnahme nicht unbemerkt aktualisiert.
- Die App verlangt eine bewusste Aktualisierung und macht den veralteten Vorbereitungsstand sichtbar.
- Noch gültige Typ1-Zuweisungen bleiben bei der Aktualisierung erhalten. Ungültig gewordene Einträge werden nicht stillschweigend übernommen.

### S08-05 – Vorgeschichte für höchstens sieben Arbeitstage

Woher stammt die unmittelbar vor dem Planungszeitraum benötigte Arbeitstagshistorie?

Antwort:

- Die App verwendet bis zu sieben unmittelbar vorhergehende Kalendertage aus vorhandenen lokal gespeicherten Plänen.
- Eine getrennte manuelle Eingabe früherer Arbeitstage gehört nicht zur ersten Fassung.
- Fehlt die Vorgeschichte ganz oder teilweise, bleibt die Planung erlaubt.
- Die betroffene Regel wird dann als nicht vollständig prüfbar gekennzeichnet; die App behauptet keine vollständige Regelprüfung.

## C – Vorgetragene Typ1-Zuweisungen

### S08-06 – Bedarfsbezug und Bürokennzeichnung

Wie wird ein vorgetragener Typ1-Dienst einem Bedarf zugeordnet und wie wirkt die Bürokennzeichnung `B`?

Antwort:

- Ein Typ1-Dienst belegt genau einen vorhandenen Bedarfsplatz.
- Die Zuweisung übernimmt die tatsächliche Zeit dieses Bedarfs und erfindet weder einen Dienst noch eine Zeitabweichung.
- Ist der vorgetragene Restaurant-Frühdienst oder Restaurant-Spätdienst zusätzlich mit `B` gekennzeichnet, zählt seine vollständige tatsächliche Zeit zu den Typ1-Wochenstunden, deckt den Bedarfsplatz aber nicht.
- Es gibt keinen eigenen oder ausdrücklich eingetragenen Bürobedarf.
- Ein Dienst ohne entsprechenden vorhandenen Bedarf ist unzulässig.

Folge für die Bedarfsabbildung:

- Eine normale Typ1-Zuweisung kann genau einen vorhandenen Platz decken.
- Eine Typ1-Zuweisung mit `B` bleibt mit dem vorhandenen Bedarf und dessen tatsächlicher Zeit verbunden, lässt den Platz aber für die spätere automatische Besetzung offen.
- Die Bürokennzeichnung ist kein eigener Diensttyp und kein Bedarfsmerkmal.

### S08-07 – Typ1-Voraussetzung je Woche

Welche Einträge erfüllen die Voraussetzung „mindestens ein vorgetragener Typ1-Dienst je nicht vollständig abwesender Woche“?

Antwort:

- Ein gültiger normaler Typ1-Dienst zählt.
- Ein gültiges zusammengesetztes Einsatzmuster `D` oder `Spr` zählt jeweils als ein Tagesmuster.
- Ein gültiger Früh- oder Spätdienst mit Bürokennzeichnung `B` zählt, weil ihm weiterhin eine strukturierte Typ1-Zuweisung zugrunde liegt.
- `U`, `K`, ein rotes `X` und ein leeres Tagesfeld zählen nicht.

## D – Entwurf, Speicherung und Aufbewahrung

### S08-08 – Aktueller Entwurf und Zwischenhistorie

Welche Zwischenstände werden vor der späteren Planabnahme gespeichert?

Antwort:

- Für einen Planungszeitraum existiert genau ein aktueller bearbeitbarer Entwurf.
- Der Entwurf und seine verwendete Planungsmomentaufnahme werden lokal gespeichert.
- Zwischenspeicherungen erzeugen noch keine unveränderliche Planversion.
- Die Versionshistorie entsteht erst durch den bewussten Abnahmeschritt in System 12.

### S08-09 – Aufbewahrung und automatische Löschung

Setzt System 08 bereits eine einstellbare Aufbewahrungs- oder Löschfrist um?

Antwort:

- Nein. System 08 löscht gespeicherte Pläne nicht automatisch.
- Eine einstellbare Aufbewahrung und eine sichere Bereinigung gehören nicht zum Umfang von System 08.
- Diese Verantwortung wird erst gemeinsam mit Planversionen beziehungsweise Datensicherung in System 12 oder 14 festgelegt.

## E – Sichtbarer Umfang und Systemgrenzen

### S08-10 – Sichtbarer Umfang von System 08

Welche sichtbaren Funktionen gehören bereits zu System 08 und welche bleiben späteren Systemen vorbehalten?

Antwort:

- Die bestehende Ansicht „Dienstplan SER“ wird um vorgetragene Typ1-Dienste und die strukturierte Bürokennzeichnung `B` ergänzt.
- Zeitraum, Vorbereitung und gespeicherter Vorbereitungsstand werden sichtbar und verständlich dargestellt.
- Planmodell, Planungsmomentaufnahme und lokale Speicherung werden in System 08 vollständig fachlich und technisch geprüft.
- Automatisch erzeugte Dienste und schwarze `X` beginnen mit System 09.
- Strukturierte Konfliktdiagnose und verständliche Konflikterklärungen beginnen mit System 10.
- Allgemeine manuelle Planänderungen, bedienbare Einzelsperren, Warnungen und Bestätigungen beginnen mit System 11.
- Unveränderliche Planversionen und die Abnahme beginnen mit System 12.

## Konsolidierte Übergaben an die System-08-Teil-Roadmap

Die Teil-Roadmap muss mindestens folgende fachliche Ergebnisse in kleine, einzeln prüfbare Schritte zerlegen:

1. genau drei vollständige Montag-bis-Sonntag-Wochen als Planungszeitraum,
2. eindeutige Ablehnung überlappender gespeicherter Planungszeiträume,
3. fachliches Planmodell für Bedarfsplätze, Zuweisungen, vollständig ungedeckte Plätze, `Spr`-Teildeckung, schwarze `X`, Einzelsperren und manuelle Zusatzbesetzungen,
4. vorgetragene und automatisch geschützte Typ1-Zuweisungen einschließlich `B`,
5. unveränderliche Planungsmomentaufnahme mit Katalogversion, vollständigen Regeldefinitionen, Typfassungen, Einsatzfreigaben, tatsächlichen Bedarfen, Tageseingaben, Laufoptionen und benötigter Vorgeschichte,
6. sichtbare Erkennung veralteter Eingangsdaten und bewusste Aktualisierung ohne stilles Ersetzen,
7. ein aktueller lokal gespeicherter Entwurf je Planungszeitraum ohne vorgezogene Planversionen,
8. Erweiterung der bestehenden Ansicht „Dienstplan SER“ nur im bestätigten System-08-Umfang,
9. SQLite-Migration und Migrationstests in der bestehenden gemeinsamen Migrationsfolge,
10. klare Übergaben an Systeme 09 bis 12 ohne vorgezogene Solver-, Konflikt-, allgemeine Bearbeitungs- oder Versionslogik.

## Ergebnis des Verständnisabgleichs

- Alle zehn Ausgangsfragen sind beantwortet.
- Es bestehen keine offenen fachlichen Fragen, die den Entwurf der System-08-Teil-Roadmap verhindern.
- Die früher allgemein vorgesehene einstellbare Anzahl von Planungswochen wird für die erste Fassung durch genau drei vollständige Wochen ersetzt.
- Die vollständig umgesetzte und abgenommene Teil-Roadmap liegt unter `docs/roadmaps/completed/S08_SCHEDULE_MODEL_ROADMAP.md`.
- PM-01 bis PM-10 und damit System 08 wurden am 2026-09-16 ausdrücklich abgenommen und archiviert.
