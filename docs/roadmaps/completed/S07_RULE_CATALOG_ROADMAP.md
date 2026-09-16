# Teil-Roadmap System 07 – Regelkatalog und Prioritäten

Status: Abgeschlossen, am 2026-09-16 vollständig geprüft und ausdrücklich abgenommen

Stand: 2026-09-16

## Ziel und Nutzen

Diese Roadmap beschreibt System 07 „Regelkatalog und Prioritäten“.

System 07 führt eine einzige feste, unveränderliche und versionierte fachliche Quelle für alle bestätigten Planungsregeln der ersten Fassung ein. Jede Regel erhält eine stabile Kennung, einen eindeutigen Geltungsbereich, ihre Wirkung in automatischer und manueller Planung, gegebenenfalls eine Priorität, typisierte Parameter und einen neutralen Beschreibungsschlüssel.

Der Katalog soll spätere Systeme in die Lage versetzen, ohne eigene Regelkopien auf dieselben Definitionen zuzugreifen:

- System 08 übernimmt die Regelfassung in eine Planungsmomentaufnahme.
- System 09 übersetzt jede unterstützte automatische Regel in Solver-Bedingungen oder Optimierungsziele.
- System 10 erklärt strukturierte Ergebnisse verständlich.
- System 11 prüft manuelle Änderungen und Bestätigungen.
- System 12 bewahrt die verwendete Regelfassung und bestätigte Abweichungen in unveränderlichen Planversionen.

System 07 erzeugt noch keinen Dienstplan, bewertet noch keinen vollständigen Plan und zeigt keinen Regelkatalog in WPF. Es schafft ausschließlich das fachliche Regelmodell, den vollständigen Startkatalog, gemeinsame Ergebnisbegriffe, einen unveränderlichen Application-Lesevertrag und synthetische Beispielszenarien.

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
- `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md`
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/decisions/S04_SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_QUESTIONS.md`
- die abgeschlossenen Roadmaps der Systeme 03 bis 06
- ausschließlich erfundene Personen und synthetische Planungsdaten in Dokumentation und Tests

Die fachliche Konsolidierung, die Architekturentscheidung und diese Teil-Roadmap für System 07 wurden am 2026-09-16 ausdrücklich abgenommen.

## Abhängigkeiten und Voraussetzungen

- System 03 liefert bindende Mitarbeitertypen, Wochen-Soll, Planungsrollen und strukturierte Einsatzfreigaben.
- System 04 liefert stabile Kennungen und Definitionen für Einsatzorte, normale Diensttypen, `D` und `Spr`.
- System 05 liefert tatsächliche Bedarfszeiten und Bedarfsplätze als spätere Eingabe für Plan und Generierung.
- System 06 liefert `U`, `K`, rote `X` und das wirksame Wochen-Soll.
- Alle vier Systeme sind abgeschlossen und archiviert.
- Es gibt keine offene fachliche Frage, die den Beginn von RK-01 nach Roadmap-Abnahme blockiert.

## Bestätigte fachliche Grundlagen

### Fester Katalog und Aussagegrenze

- Die erste Fassung enthält ausschließlich die bestätigten internen Regeln der Service-Leitung.
- Sie behauptet keine vollständige gesetzliche, tarifliche oder sonstige externe Regelprüfung.
- Regelarten, Grenzwerte, Prioritäten und Aktivierung sind nicht frei bearbeitbar.
- Der Regelkatalog wird nicht als Stammdatum in SQLite gespeichert.
- System 07 besitzt keine WPF-Regelkatalogansicht.
- Eine neue oder geänderte Regel benötigt später eine bewusste Code-, Test-, Dokumentations- und Katalogversionsänderung.

### Drei Wirkungen

| Wirkung | Automatische Planung | Manuelle Bearbeitung |
|---|---|---|
| Nicht übersteuerbare Strukturregel | widersprüchliche Daten werden nicht erzeugt | widersprüchliche Daten werden nicht gespeichert |
| Automatische Hard Rule | darf von der Generierung nicht verletzt werden | darf nur bei ausdrücklich bestätigter Übersteuerbarkeit nach Warnung abweichen |
| Weiche Regel | wird in ihrer festgelegten Prioritätsstufe optimiert | darf nach Warnung und Bestätigung übergangen werden |

Berichtsschwellen erzeugen Hinweise, ohne selbst eine automatische Mindest- oder Höchstgrenze zu erfinden. Stabilitätsziele entscheiden erst nach zwingenden Regeln, Bedarfsdeckung und den drei weichen Prioritätsstufen.

### Hierarchische Reihenfolge

1. nicht übersteuerbare Strukturregeln und automatische Hard Rules einhalten,
2. ungedeckten Bedarf minimieren,
3. weiche Regeln mit Priorität hoch optimieren,
4. weiche Regeln mit Priorität mittel optimieren,
5. weiche Regeln mit Priorität niedrig optimieren,
6. bei ansonsten gleichwertigen Ergebnissen innerhalb des Planungszeitraums stabil und möglichst gleichmäßig verteilen.

Für die erste Fassung ist keine konkrete Regel mit niedriger Priorität bestätigt. Die Prioritätsstufe bleibt dennoch Bestandteil der allgemeinen Regelsprache, damit spätere Ergänzungen keinen Modellumbau benötigen.

### Vorgesehener Startkatalog

Die folgenden stabilen Kennungen werden mit der Abnahme dieser Roadmap verbindlich. Sie sind technische Fachkennungen und keine sichtbaren deutschen Meldungen.

#### Nicht übersteuerbare Strukturregeln

| Regelkennung | Bedeutung |
|---|---|
| `STRUCTURE_KNOWN_REFERENCES` | Nur vorhandene Personen, Dienste, Einsatzmuster und weitere Katalogeinträge dürfen referenziert werden. |
| `STRUCTURE_SINGLE_DAILY_ASSIGNMENT` | Pro Person und Kalendertag ist höchstens eine normale Zuweisung oder ein zusammengesetztes Einsatzmuster zulässig. |
| `STRUCTURE_NO_TIME_OVERLAP` | Tatsächliche Arbeitsabschnitte derselben Person dürfen sich zeitlich nicht überschneiden. |
| `STRUCTURE_BLOCKED_DAY_MARKER` | Auf `U`, `K` oder rotem `X` ist keine direkte Zuweisung zulässig; das Tageskennzeichen muss zuerst bewusst entfernt werden. |
| `STRUCTURE_NORMAL_SLOT_FULL_COVERAGE` | Ein normaler Bedarfsplatz wird vollständig durch eine Person gedeckt oder bleibt vollständig ungedeckt; frei wählbare Teilzuweisungen sind unzulässig. |
| `STRUCTURE_APPROVED_PLAN_IMMUTABLE` | Eine abgenommene Planversion wird nicht verändert; spätere Änderungen erzeugen einen neuen Entwurf. |

#### Für die automatische Planung zwingende Regeln

| Regelkennung | Bedeutung | Manuelle Wirkung |
|---|---|---|
| `ACTIVE_EMPLOYEES_ONLY` | Nur aktive Personen werden in eine neue Planung aufgenommen. | Struktur des späteren Planungsablaufs; keine Zuweisung an unbekannte oder gelöschte Personen. |
| `SHIFT_ELIGIBILITY_REQUIRED` | Automatische Einsätze benötigen die strukturierte Freigabe des Mitarbeitertyps. | Abweichung außerhalb der Freigabe nach Warnung und Bestätigung zulässig. |
| `EXPLICIT_RUN_OPTION_REQUIRED` | Laufabhängige Sonderfreigaben sind standardmäßig aus und gelten nur für den ausdrücklich gestarteten Zeitraum. | Keine dauerhafte Änderung der Einsatzfreigabe. |
| `TYPE1_MANUAL_ONLY` | Typ1 wird niemals automatisch eingeteilt. | Typ1 wird bewusst manuell vorgetragen. |
| `TYPE1_WEEKLY_PREREQUISITE` | In jeder nicht vollständig abwesenden Woche muss vor der Generierung mindestens ein Typ1-Dienst vorgetragen sein. | Der Generierungsstart bleibt bis zur gültigen Eingabe blockiert. |
| `NORMAL_WEEKLY_MAXIMUM` | Normale Typen überschreiten automatisch nicht das wirksame Wochen-Soll plus drei Stunden. | Überschreitung nach Warnung und Bestätigung zulässig. |
| `AH_WEEKLY_MAXIMUM` | AH erhält automatisch höchstens zwölf Wochenstunden. | Mehr als zwölf Stunden nach Warnung und Bestätigung zulässig. |
| `MAX_CONSECUTIVE_WORKDAYS` | Automatisch sind höchstens sieben Kalendertage mit Arbeit in Folge zulässig; erforderliche Vorgeschichte wird einbezogen. | Abweichung nach Warnung und Bestätigung zulässig. |
| `POST_VACATION_WEEKEND_FREE` | Endet `U` am Freitag, hält die Automatik das unmittelbar folgende Wochenende frei. | Abweichung nach Warnung und Bestätigung zulässig. |
| `AUTOMATIC_NO_OVERSTAFFING` | Die Generierung besetzt keinen vollständig gedeckten Dienst zusätzlich. | Manuelle Zusatzbesetzung verändert den Bedarf nicht und benötigt Warnung und Bestätigung. |
| `RELIEF_SHIFT_EMERGENCY_ONLY` | `Spr` ist nur samstags und nur zur Verringerung einer sonst verbleibenden Restaurant-Spätdienst-Unterdeckung ab dem tatsächlichen Wechsel zulässig. | Keine frei erfundene Teilzuweisung; nur das bestätigte Muster ist zulässig. |

#### Weiche Regeln mit Priorität hoch

| Regelkennung | Bedeutung |
|---|---|
| `NORMAL_WEEKLY_MINIMUM` | Normale Typen sollen mindestens das wirksame Wochen-Soll minus drei Stunden erreichen. |
| `WEEKLY_CONSECUTIVE_DAYS_OFF` | Jede Person soll innerhalb jeder Montag-bis-Sonntag-Woche mindestens zwei zusammenhängende regulär freie Tage besitzen. |
| `RED_X_ADJACENT_DAY_OFF` | Mindestens ein rotes `X` einer Woche soll unmittelbar durch ein rotes oder schwarzes `X` zu einem freien Zweierblock ergänzt werden. |
| `PRE_VACATION_WEEKEND_FREE` | Beginnt `U` am Montag, soll das unmittelbar vorherige Wochenende frei bleiben. |
| `SPLIT_SHIFT_WEEKLY_MAXIMUM` | Eine Person soll höchstens einen Doppeldienst `D` pro Woche erhalten. |

#### Weiche Regeln mit Priorität mittel

| Regelkennung | Bedeutung |
|---|---|
| `THREE_WEEK_FREE_WEEKEND` | Innerhalb des konkret geplanten Drei-Wochen-Zeitraums soll jede Person mindestens ein vollständiges regulär freies Wochenende besitzen. |
| `MINIMIZE_SPLIT_SHIFTS` | Innerhalb der hoch priorisierten Doppeldienstgrenze soll die Gesamtzahl der Doppeldienste weiter minimiert werden. |
| `AH_WEEKLY_TARGET` | AH soll innerhalb der nachgelagerten AH-Phase möglichst zehn Wochenstunden erreichen. |

#### Berichtsschwellen und Stabilität

| Regelkennung | Wirkung |
|---|---|
| `AH_WEEKLY_LOW_NOTICE` | Weniger als sechs AH-Wochenstunden erzeugen einen Hinweis, blockieren die Generierung aber nicht. |
| `AH_WEEKLY_HIGH_NOTICE` | Mehr als zehn bis höchstens zwölf automatisch geplante AH-Wochenstunden erzeugen einen Hinweis. |
| `CURRENT_PERIOD_FAIR_DISTRIBUTION` | Bei ansonsten gleichwertigen Ergebnissen werden ungünstige Dienste, Doppeldienste und Wochenenden innerhalb der aktuell geplanten drei Wochen möglichst gleichmäßig verteilt. |

Die Katalogdefinitionen halten außerdem fest:

- rote und schwarze `X` zählen als regulär freie Tage,
- `U`, `K`, rote und schwarze `X` unterbrechen eine Folge tatsächlicher Arbeitstage,
- `U` und `K` zählen nicht als das Paar regulär freier Tage,
- ein vollständiges freies Wochenende verlangt Samstag und Sonntag ohne Arbeitsabschnitt,
- der bestätigte `Spr`-Sonderfall lässt den Restaurant-Zeitraum vor dem Wechsel sichtbar ungedeckt,
- fehlende Vorgeschichte für `MAX_CONSECUTIVE_WORKDAYS` erzeugt „nicht vollständig prüfbar“ statt einer falschen Vollständigkeitsbehauptung.

## Umfang dieser Roadmap

- eigener fachlicher Bereich `Domain/Rules`,
- stabile, validierte Regelkennung und Katalogversion,
- eindeutige Regelfamilie und Geltungsbereiche,
- getrennte automatische und manuelle Wirkung,
- Prioritätsstufen hoch, mittel und niedrig sowie nachgelagerte Stabilität,
- typisierte Parameter ohne frei interpretierte Wörterbücher oder `object`-Werte,
- feste unveränderliche Version 1 des vollständigen Startkatalogs,
- Ergebniszustände `erfüllt`, `verletzt`, `nicht anwendbar` und `nicht vollständig prüfbar` als strukturierte Fachwerte,
- stabile fachliche Parameter und Ursachecodes ohne fertige deutsche Konfliktsätze,
- Katalogprüfung auf eindeutige Kennungen, gültige Kombinationen, vollständige Parameter und deterministische Reihenfolge,
- unveränderlicher Application-Lesevertrag für den gesamten Katalog,
- dokumentierte synthetische Beispielszenarien mit stabilen Szenariokennungen,
- fokussierte Domain-, Application- und Architekturtests,
- wahrheitsgemäße Aktualisierung von Roadmap, Master-Roadmap, Status und Service-Leitungsdokumentation.

## Nicht-Umfang

- vollständiges Planmodell, Planungszeiträume, Zuweisungen oder Planpersistenz,
- tatsächliche fachliche Bewertung eines vollständigen Dienstplans,
- OR-Tools-Variablen, Solver-Bedingungen oder Prioritätsoptimierung,
- automatische Plangenerierung,
- konkrete Berechnung von Bedarfsdeckung oder manueller Überbesetzung,
- manuelle Planbearbeitung, Warnungsdialoge oder Bestätigungsabläufe,
- deutsche Konflikttexte und Lösungsvorschläge,
- Speicherung des Regelkatalogs in SQLite oder eine Datenbankmigration,
- WPF-Regelkatalogansicht oder andere sichtbare Bedienoberfläche,
- Abnahme und Versionierung von Plänen,
- Excel-Export,
- frei bearbeitbare Grenzwerte, Prioritäten oder Regelaktivierung,
- gesetzliche, tarifliche oder sonstige externe Compliance-Prüfung,
- Pausen, allgemeine tägliche Höchstarbeitszeit, Ruhezeiten, Ersatzruhetage, Zuschläge, Nachtarbeit oder besondere Personengruppen,
- neue NuGet-Pakete oder externe Werkzeuge.

## Architekturgrenzen und geplante Code-Anker

### Domain

Der neue Fachbereich liegt unter `src/Salztal.Dienstplanung.Domain/Rules`.

Geplant sind kleine, klar benannte Fachtypen für:

- `RuleId` und `RuleCatalogVersion`,
- Regelfamilie und `RuleScope`,
- automatische Wirkung, manuelle Wirkung und `RulePriority`,
- neutrale Beschreibungsschlüssel,
- typisierte Parameter der bestätigten Regelfamilien,
- unveränderliche Regeldefinitionen,
- fachliche Bewertungszustände und strukturierte Ergebnisparameter,
- den festen `InitialRuleCatalog`.

Die genauen C#-Typnamen dürfen innerhalb von RK-01 begründet präzisiert werden. Fachbedeutung, stabile Regelkennungen, Katalogvollständigkeit und Modulgrenze dürfen sich dadurch nicht ändern.

Domain referenziert weder Application noch WPF, EF Core, SQLite oder OR-Tools. Sichtbare Typcodes und deutsche Texte sind keine Regelschalter. Bestehende Fachtypen wie `WeeklyWorkTarget`, `EffectiveWeeklyWorkTarget`, `EmployeeTypePlanningPolicy`, Einsatzfreigaben sowie Kennungen für Diensttypen und Einsatzmuster werden referenziert oder in späteren Planmomentaufnahmen verwendet, aber nicht als zweite Regelkopie dupliziert.

System 07 definiert die Regelsprache und das Ergebnisvokabular. Die Bewertung eines vollständigen Plans wird erst ergänzt, wenn System 08 das Planmodell bereitstellt. Dadurch entstehen in System 07 keine vorgezogenen Ersatztypen für Plan, Zuweisung oder Bedarfsdeckung.

### Application

Der neue Bereich liegt unter `src/Salztal.Dienstplanung.Application/Rules`.

- Ein schmaler Leseanwendungsfall liefert den vollständigen festen Katalog als unveränderliche Momentaufnahme.
- Die Momentaufnahme enthält Katalogversion, stabile Regelkennung, Geltungsbereich, automatische und manuelle Wirkung, Priorität, typisierte Parameter und neutralen Beschreibungsschlüssel.
- Die Reihenfolge ist deterministisch und unabhängig von Hashing, Reflection oder Dateisystemreihenfolge.
- Es gibt keinen Schreibanwendungsfall und keinen Speichervertrag für Regeln.
- Application dupliziert keine Schwellenwerte und enthält keine eigene Katalogdefinition.
- Deutsche Konfliktsätze bleiben Eigentum von System 10; System 07 liefert dafür nur stabile strukturierte Grundlagen.

### Planning, Infrastructure, Desktop und Excel

- `Salztal.Dienstplanung.Planning` bleibt unverändert; es werden noch keine OR-Tools-Übersetzer angelegt.
- `Salztal.Dienstplanung.Infrastructure` bleibt unverändert; es gibt keine Regel-Entitäten, Tabellen oder Migrationen.
- `Salztal.Dienstplanung.Desktop` bleibt unverändert; es gibt keine Regelansicht und keine vorgezogenen Warnungsdialoge.
- `Salztal.Dienstplanung.Excel` bleibt unverändert.
- Die Composition wird nicht erweitert, weil der feste Katalog ohne technischen Adapter gelesen wird.

### Tests

- `tests/Salztal.Dienstplanung.Domain.Tests/Rules` prüft Regelsprache, Parameter, vollständigen Katalog und fachliche Invarianten.
- `tests/Salztal.Dienstplanung.Application.Tests/Rules` prüft den unveränderlichen Lesevertrag, vollständiges Mapping und deterministische Reihenfolge.
- `tests/Salztal.Dienstplanung.Architecture.Tests` schützt die bestehenden Modulgrenzen und stellt sicher, dass System 07 keine technischen Abhängigkeiten oder UI-Regellogik einführt.
- Synthetische Szenariokennungen aus der Dokumentation werden in den fokussierten Tests wiederverwendet und später von Planning-Tests übernommen.

## Offene Entscheidungen

Für den Beginn von RK-01 bestehen keine offenen fachlichen Entscheidungen.

Folgende technische Details dürfen innerhalb der beschriebenen Grenzen im jeweiligen Schritt präzisiert werden:

- Aufteilung der Regelfamilien auf einzelne kleine C#-Dateien,
- konkrete Namen interner Hilfstypen,
- Darstellung typisierter Parameter durch geschlossene Records oder vergleichbar sichere Fachwerte,
- konkrete Form der deterministischen Katalogreihenfolge.

Eine Präzisierung darf weder neue Regeln hinzufügen noch bestätigte Regelkennungen, Grenzwerte, Prioritäten, manuelle Wirkungen oder Systemgrenzen verändern. Eine solche Änderung wäre fachlicher Handlungsbedarf und stoppt den Ablauf.

## Nummerierte Schritte

Für diese Roadmap gilt nach ihrer ausdrücklichen Abnahme die bedingte automatische Weiterführung: Ein planmäßig und fehlerfrei abgeschlossener Schritt darf direkt in den nächsten übergehen, wenn weder eine unerwartete Entscheidung oder kritische Frage noch ein externes, manuelles oder fachliches Gate besteht. Bei Handlungsbedarf wird gestoppt. Nach jedem abgeschlossenen Schritt ertönt ein Abschlusston; bei Handlungsbedarf zusätzlich ein unterscheidbarer Hinweiston.

### RK-01 – Fachliche Regelsprache und Ergebnisvokabular

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- stabile und validierte `RuleId` einführen,
- positive versionierte `RuleCatalogVersion` einführen,
- geschlossene Werte für Regelfamilie, Geltungsbereich, automatische Wirkung, manuelle Wirkung und Priorität definieren,
- gültige und ungültige Kombinationen fachlich absichern,
- typisierte Parameterbasis ohne frei interpretierte Schlüssel-Wert-Wörterbücher schaffen,
- Bewertungszustände erfüllt, verletzt, nicht anwendbar und nicht vollständig prüfbar definieren,
- strukturierte Ergebnisparameter von deutschen Meldungstexten trennen.

Prüfung:

- fokussierte Domain-Tests für gültige und ungültige Kennungen und Versionen,
- Tests aller zulässigen und unzulässigen Wirkungs-/Prioritätskombinationen,
- Tests der Unveränderlichkeit und der wertbasierten Gleichheit,
- Architekturprüfung: keine technische Bibliothek und kein anderes Projektmodul in Domain.

Abnahmebedingung:

- Die Regelsprache kann jede bestätigte Katalogzeile eindeutig ausdrücken, ohne Planmodell, UI-Text, Datenbank- oder Solvertyp vorwegzunehmen.

### RK-02 – Strukturregeln und automatische Hard Rules

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- alle sechs bestätigten Strukturregeln mit ihren stabilen Kennungen definieren,
- alle elf automatischen Hard Rules mit ihrer manuellen Wirkung definieren,
- Grenzwerte und Geltungsbereiche typisiert festhalten,
- Wochenkorridor-Obergrenze, AH-Obergrenze, sieben Arbeitstage und Urlaubswochenende eindeutig parametrisieren,
- Einsatzfreigaben, Laufoption, Typ1-Voraussetzung, Überbesetzungsverbot und `Spr`-Notfallbedingung ohne sichtbare Codes modellieren.

Prüfung:

- je Regel mindestens ein gültiger Definitionstest,
- Grenzwerttests für drei Stunden, zwölf Stunden und sieben Arbeitstage,
- Tests der korrekten automatischen und manuellen Wirkung,
- Tests, dass Strukturregeln niemals als manuell übersteuerbar definiert werden,
- Tests, dass kein bestätigter Parameter aus Anzeigenamen oder Typcodes abgeleitet wird.

Abnahmebedingung:

- Jede bestätigte Strukturregel und automatische Hard Rule besitzt genau eine valide Domain-Definition mit stabiler Kennung und ohne technische Abhängigkeit.

### RK-03 – Weiche Regeln, Berichtsschwellen und Stabilität

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- alle fünf hoch priorisierten Regeln definieren,
- alle drei mittel priorisierten Regeln definieren,
- die leere, aber zulässige niedrige Prioritätsstufe im Modell absichern,
- beide AH-Berichtsschwellen definieren,
- das nachgelagerte Stabilitätsziel für den aktuellen Planungszeitraum definieren,
- Bedeutung von `U`, `K`, roten und schwarzen `X` für Arbeitsfolgen und regulär freie Tage als typisierte Parameter festhalten.

Prüfung:

- exakte Priorität jeder weichen Regel prüfen,
- Berichtsschwellen unter sechs und über zehn bis höchstens zwölf Stunden prüfen,
- Wochen- und Drei-Wochen-Geltungsbereiche prüfen,
- freie-Wochenend- und Urlaubsblockdefinitionen prüfen,
- sicherstellen, dass Stabilität keine höhere Prioritätsstufe erhält.

Abnahmebedingung:

- Alle bestätigten weichen Regeln, Hinweise und Stabilitätsziele sind vollständig und widerspruchsfrei definiert.

### RK-04 – Vollständiger Startkatalog und Katalogversion 1

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- alle 28 Regeldefinitionen in einem unveränderlichen `InitialRuleCatalog` zusammenführen,
- Katalogversion 1 festlegen,
- eindeutige Kennungen und deterministische Reihenfolge garantieren,
- Vollständigkeit nach Regelfamilie, Priorität und Wirkung prüfen,
- Nachschlagen nach `RuleId` ohne stilles Zurückfallen auf einen Standard ermöglichen,
- unbekannte Kennung oder nicht unterstützte Katalogversion als strukturierten Fehler behandeln,
- keine automatische Aktivierung, Deaktivierung oder Parameteränderung bereitstellen.

Prüfung:

- exakt 28 erwartete eindeutige Kennungen,
- exakte Zuordnung aller Kennungen zu Wirkung, Priorität, Geltungsbereich und Parametern,
- wiederholtes Lesen liefert dieselbe Reihenfolge und dieselben unveränderlichen Werte,
- unbekannte Kennung und unbekannte Version werden sichtbar abgelehnt,
- kein Regelwert stammt aus Datenbank, Konfiguration, UI oder Reflection.

Abnahmebedingung:

- Katalogversion 1 ist vollständig, deterministisch, unveränderlich und maschinell gegen die bestätigte Matrix geprüft.

### RK-05 – Unveränderlicher Application-Lesevertrag

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- eigenen Application-Bereich `Rules` anlegen,
- schmalen Query-Ablauf für den vollständigen Katalog bereitstellen,
- Domain-Definitionen vollständig und typisiert in unveränderliche Snapshots abbilden,
- stabile Reihenfolge und Katalogversion erhalten,
- unbekannte interne Katalogdaten nicht stillschweigend auslassen,
- keinen Store, Command, technischen Adapter oder Composition-Eintrag hinzufügen.

Prüfung:

- Application-Tests für einen vollständigen Lesestand ohne fehlende Einträge mit allen 28 Regeln,
- exaktes Mapping von Kennung, Wirkung, Priorität, Geltungsbereich, Beschreibungsschlüssel und Parametern,
- Unveränderlichkeit und deterministische Reihenfolge,
- Abbruchverhalten vor Rückgabe,
- Architekturtests für unveränderte Referenzrichtungen und das Fehlen technischer Typen.

Abnahmebedingung:

- Spätere Systeme können den vollständigen Katalog über Application lesen, ohne Domain-Werte zu duplizieren oder einen technischen Speicher vorauszusetzen.

### RK-06 – Gemeinsame synthetische Beispielszenarien

Status: `[x]` – umgesetzt, automatisch geprüft und am 2026-09-16 fachlich ausdrücklich abgenommen

Umfang:

- `docs/decisions/S07_RULE_CATALOG_EXAMPLES.md` mit stabilen Szenariokennungen erstellen,
- für jede Regel erfüllt, verletzt und nicht anwendbar beschreiben,
- für `MAX_CONSECUTIVE_WORKDAYS` zusätzlich den Fall nicht vollständig prüfbarer Vorgeschichte beschreiben,
- Kombinationen für Wochenkorridor gegen Überbesetzungsverbot, Urlaub gegen Bedarfsdeckung, rotes `X` plus schwarzes `X`, AH-Phasen, Typ1-Voraussetzung, `D`, `Spr`, manuelle Abweichung und Strukturblockade aufnehmen,
- ausschließlich erfundene Personen und synthetische Daten verwenden,
- die Szenariokennungen in fokussierten Domain- und Application-Tests nachvollziehbar referenzieren,
- die Beispiele in einfacher Sprache der Service-Leitung zur fachlichen Prüfung vorlegen.

Prüfung:

- jede der 28 Regeln ist mindestens durch die vereinbarten Grundfälle abgedeckt,
- jede erwartete Wirkung, Priorität und Schwelle stimmt mit Katalogversion 1 überein,
- Kombinationsszenarien widersprechen weder Fragenkatalog noch Architekturentscheidung,
- keine echten Namen, Dienstpläne, Krankheitsgründe oder anderen personenbezogenen Daten,
- Dokumentprüfung durch die Service-Leitung.

Abnahmebedingung:

- Die Service-Leitung bestätigt die verständlichen synthetischen Beispiele ausdrücklich. Dieses fachliche Dokument-Gate stoppt den Ablauf vor RK-07.

### RK-07 – System-07-Gesamtnachweis und Übergaben

Status: `[x]` – Gesamtnachweis vollständig grün und am 2026-09-16 ausdrücklich abgenommen

Umfang:

- Domain-Katalog, Application-Lesevertrag und Beispielszenarien gemeinsam abgleichen,
- Übergaben an Systeme 08 bis 12 dokumentieren,
- Roadmap, Master-Roadmap, Status und Service-Leitungsdokumente wahrheitsgemäß aktualisieren,
- Fragen- und Roadmap-Datei erst nach ausdrücklicher Systemabnahme nach `docs/roadmaps/completed` verschieben,
- Datenschutz und Repository-Hygiene abschließend prüfen.

Prüfung:

- `dotnet restore Salztal.Dienstplanung.sln --locked-mode`,
- `dotnet build Salztal.Dienstplanung.sln --no-restore`,
- fokussierte Domain-, Application- und Architekturtests,
- vollständiger Testlauf mit der im Repository bestätigten Behandlung leerer Planning- und Excel-Testprojekte,
- `dotnet format Salztal.Dienstplanung.sln --verify-no-changes --no-restore`,
- Katalogvollständigkeit, Szenarioabdeckung, Pfade, Datenschutz und `git diff --check`,
- bestätigtes fachliches Dokument-Gate aus RK-06.

Nachweis am 2026-09-16:

- gesperrte Paketwiederherstellung erfolgreich,
- alle 13 Projekte mit 0 Warnungen und 0 Fehlern gebaut,
- 87 fokussierte Domain-, 7 fokussierte Application- und 18 Architekturtests bestanden,
- alle 590 vorhandenen Tests bestanden; die leeren Planning- und Excel-Testprojekte wurden mit der bestätigten Exitcode-8-Behandlung einbezogen,
- Formatierung, Katalog- und Szenarioabdeckung, Architekturgrenzen, Dokumentpfade, Datenschutz, Artefaktsuche und `git diff --check` ohne Befund,
- bestätigtes fachliches Dokument-Gate aus RK-06 nachgewiesen.

Abnahmebedingung:

- System 07 ist fachlich, technisch und dokumentarisch vollständig geprüft und ausdrücklich abgenommen. Erst dann werden Roadmap und Fragenkatalog archiviert und System 08 kann vorbereitet werden.

## Übergaben an spätere Systeme

| Übergabe | Verbindlicher Inhalt | Eigentümer |
|---|---|---|
| Planungsmomentaufnahme | Katalogversion, vollständige Regeldefinitionen, erforderliche Vorgeschichte und Laufoptionen als unveränderliche Eingabe | System 08 |
| Planmodell | Strukturregeln, normale Vollbesetzung, `Spr`-Teildeckungsfall und manuelle Zusatzbesetzung fachlich ausdrücken | System 08 |
| Solverübersetzung | jede automatische Hard Rule und jedes Ziel eindeutig übersetzen; unbekannte Regel blockiert den Lauf | System 09 |
| Optimierung | Bedarfsdeckung, hoch, mittel, niedrig und Stabilität hierarchisch absichern | System 09 |
| Ergebnisbewertung | erfüllt, verletzt, nicht anwendbar und nicht vollständig prüfbar mit stabiler Regelkennung und Parametern liefern | Systeme 09 und 10 |
| Verständliche Meldungen | strukturierte Ergebnisse in sachliche deutsche Erklärungen und Lösungsmöglichkeiten übersetzen | System 10 |
| Manuelle Bearbeitung | Strukturverstöße blockieren; übersteuerbare Planungsregeln und Zusatzbesetzungen warnen und bestätigen | System 11 |
| Planversion | Katalogversion, verwendete Parameter, Laufoptionen und bestätigte Abweichungen unveränderlich bewahren | System 12 |

## Echte externe und manuelle Gates

- Die fachliche Konsolidierung und die Architekturentscheidung wurden am 2026-09-16 ausdrücklich abgenommen; dieses Gate ist bestanden.
- Diese Teil-Roadmap wurde am 2026-09-16 ausdrücklich abgenommen; dieses Gate ist bestanden.
- Die synthetischen Beispielszenarien aus RK-06 wurden am 2026-09-16 fachlich ausdrücklich abgenommen; dieses Gate ist bestanden.
- System 07 wurde nach dem vollständigen RK-07-Gesamtnachweis am 2026-09-16 ausdrücklich abgenommen; dieses Gate ist bestanden.
- System 07 besitzt bewusst kein WPF-Gate, keine Datenbankmigration und keinen OR-Tools-Lauf.
- Ein erfolgreicher Build ersetzt weder die fachliche Prüfung der Beispiele noch die abschließende Systemabnahme.
- Portable Windows-Ausgabe, echter Solverbetrieb, Planbearbeitung, Excel und fachliche Endabnahme der Gesamtanwendung bleiben spätere Gates.

## Risiken und Schutzmaßnahmen

| Risiko | Schutzmaßnahme |
|---|---|
| Regelwerte werden in späteren Modulen dupliziert | eine Domain-Quelle, Application-Lesevertrag und Architekturtests |
| automatische und manuelle Wirkung werden vermischt | getrennte geschlossene Fachwerte und Kombinationstests |
| scheinbar vollständige rechtliche Prüfung | dokumentierte Aussagegrenze und neutrale interne Regelbezeichnungen |
| unbekannte Regeln werden ignoriert | strukturierte Ablehnung unbekannter Kennungen oder Versionen |
| vorgezogenes Plan- oder Solvermodell | strikter Nicht-Umfang und unveränderte Module Planning, Infrastructure, Desktop und Excel |
| Beispiel und spätere Solverregel driften auseinander | stabile Szenariokennungen und verpflichtende Wiederverwendung in späteren Planning-Tests |
| reale Personaldaten gelangen in Tests | ausschließlich klar erfundene Namen und synthetische Planwerte |

## Berichtsschema nach jedem Schritt

Nach jedem Schritt werden kurz genannt:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. kritische Meldungen und konkreter Handlungsbedarf,
5. offene Gates, Risiken oder Blockaden,
6. Git-Status ohne automatisches Staging, Commit oder Push,
7. nächster minimaler Schritt,
8. Bitte um ausdrückliche Abnahme, wenn ein Gate oder Handlungsbedarf besteht.

## Nächster minimaler Schritt

System 07 und alle Schritte RK-01 bis RK-07 sind vollständig geprüft, ausdrücklich abgenommen und archiviert. Der nächste mögliche Arbeitsschritt ist die Vorbereitung einer eigenen Teil-Roadmap für System 08; System 08 wurde noch nicht begonnen.
