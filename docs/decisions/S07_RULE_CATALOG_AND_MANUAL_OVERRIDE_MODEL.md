# Entscheidung: Regelkatalog und manuelle Abweichungen

Status: Mit System 07 am 2026-09-16 fachlich, architektonisch und insgesamt ausdrücklich abgenommen und archiviert

Stand: 2026-09-16

## Anlass

Die bisherigen Grundlagen unterschieden nur zwischen zwingenden und weichen Regeln. Die vollständige Befragung für System 07 hat zusätzlich ergeben, dass die Service-Leitung bestimmte für die Automatik zwingende Planungsregeln später bewusst manuell übergehen darf, während widersprüchliche Plandaten weiterhin unmöglich bleiben müssen.

Diese Entscheidung konsolidiert die beantworteten Fragen aus `docs/roadmaps/completed/S07_RULE_CATALOG_QUESTIONS.md`. Sie wurde am 2026-09-16 ausdrücklich abgenommen. Die System-07-Teil-Roadmap wurde anschließend vollständig umgesetzt, abgenommen und archiviert.

## Entscheidung

### Eine fachliche Regelquelle mit Ausführungskontext

Jede Regel wird genau einmal als unveränderliche Definition in `Domain` beschrieben. Neben stabiler Regelkennung, Art, Geltungsbereich, Parametern und Priorität legt die Definition fest, wie sie in den folgenden Kontexten wirkt:

- automatische Generierung,
- fachliche Bewertung eines vorhandenen Plans,
- manuelle Bearbeitung und Bestätigung,
- Konflikt- und Berichtsausgabe.

Eine manuelle Abweichung erzeugt keine zweite Regeldefinition. Sie ist ein strukturiertes Ergebnis der gemeinsamen Regelbewertung und bleibt mit Regelkennung, Auswirkung und Bestätigung nachvollziehbar.

### Drei Durchsetzungsstufen

1. **Für die Automatik zwingend:** Die Generierung verletzt diese Regeln niemals. Kann Bedarf deshalb nicht besetzt werden, bleibt er sichtbar ungedeckt.
2. **Manuell übersteuerbare Planungsregeln:** Die Service-Leitung darf in einem späteren Bearbeitungsablauf bewusst abweichen. Die Abweichung benötigt eine sichtbare Warnung und ausdrückliche Bestätigung und bleibt im Entwurf und in der späteren Planversion nachvollziehbar.
3. **Nicht übersteuerbare Strukturregeln:** Die Anwendung speichert keinen widersprüchlichen Plan. Zeitliche Überschneidungen, mehr als eine Zuweisung oder ein zusammengesetztes Muster pro Person und Tag, unbekannte Katalogbezüge, eine direkte Zuweisung auf `U`, `K` oder rotes `X` sowie das Verändern einer abgenommenen Planversion bleiben ausgeschlossen.

Die dritte Stufe ist keine frei konfigurierbare Priorität, sondern schützt die Gültigkeit des Fachmodells. Ein vorhandenes Tageskennzeichen muss bewusst entfernt werden, bevor für diesen Tag eine Zuweisung möglich ist.

### Automatische Prioritätsreihenfolge

Die Generierung arbeitet hierarchisch:

1. alle für die Automatik zwingenden Regeln einhalten,
2. ungedeckten Bedarf minimieren,
3. weiche Regeln mit Priorität hoch optimieren,
4. weiche Regeln mit Priorität mittel optimieren,
5. weiche Regeln mit Priorität niedrig optimieren,
6. bei ansonsten gleichwertigen Ergebnissen stabil und innerhalb der drei Planungswochen möglichst gleichmäßig verteilen.

Viele Ziele einer niedrigeren Stufe dürfen kein Ziel einer höheren Stufe überstimmen. Innerhalb derselben Priorität werden zuerst Zahl beziehungsweise Ausmaß der Verletzungen minimiert und danach Belastungen möglichst gleichmäßig verteilt.

Die vollständige fachliche Prioritätsmatrix steht im konsolidierten Abschnitt des Fragenkatalogs und wird in der späteren System-07-Roadmap in einzelne Regeldefinitionen und synthetische Beispielszenarien überführt.

### Korridor und Sonderrollen

- Der normale Wochenkorridor wird für jede Person und Montag-bis-Sonntag-Woche gegen das nach `U` und `K` wirksame Soll geprüft.
- Die Untergrenze von Soll minus drei Stunden ist weich mit Priorität hoch.
- Die Obergrenze von Soll plus drei Stunden ist für die Automatik zwingend.
- Beide Grenzen sind bei der manuellen Bearbeitung nach Warnung und Bestätigung übersteuerbar.
- AH wird erst nach allen automatisch planbaren Nicht-AH-Typen eingesetzt. Zehn Stunden sind ein mittleres Ziel, unter sechs und über zehn Stunden entstehen Hinweise, und automatisch sind höchstens zwölf Stunden zulässig. Mehr als zwölf Stunden ist nur als bestätigte manuelle Abweichung möglich.
- Typ1 bleibt vollständig außerhalb der automatischen Einteilung. Die umfassende Einsatzberechtigung hebt die allgemeinen Tages- und Überschneidungsgrenzen nicht auf.

### Bedarf, Teildeckung und Überbesetzung

- Die Automatik erzeugt keine Überbesetzung.
- Eine normale Zuweisung deckt den vollständigen tatsächlichen Zeitraum genau eines Bedarfsplatzes. Frei wählbare Teilzuweisungen gehören nicht zur ersten Fassung.
- `Spr` ist die einzige bestätigte Ausnahme: Der Restaurant-Spätdienst wird erst ab dem tatsächlichen Wechsel aus der Cafeteria gedeckt; der frühere Zeitraum bleibt sichtbar ungedeckt.
- Die Service-Leitung darf später manuell eine zusätzliche Person auf einen bereits vollständig gedeckten vorhandenen Dienst setzen. Der Bedarf wird dadurch nicht verändert, die tatsächlichen Stunden zählen vollständig und die Überbesetzung benötigt eine sichtbare Bestätigung.
- System 08 muss diese manuelle Zusatzzuweisung unabhängig von noch freien Bedarfsplätzen modellieren.

### Urlaubs- und Erholungsregeln

- Rote und schwarze `X` zählen als die regulären freien Tage. `U` und `K` unterbrechen eine Arbeitsfolge, zählen aber nicht als das geforderte Paar regulär freier Tage.
- Leere Wochenend- oder Feiertagsfelder werden nicht automatisch gesperrt. Ein rotes `X` garantiert einen von der Service-Leitung vorgegebenen freien Tag.
- Beginnt Urlaub am Montag, wird das unmittelbar vorherige Wochenende mit Priorität hoch freigehalten.
- Endet Urlaub am Freitag, hält die Automatik das unmittelbar folgende Wochenende zwingend frei und lässt erforderlichenfalls Bedarf ungedeckt. Manuell darf diese Regel nach Warnung übergangen werden.
- Die Regel zu höchstens sieben aufeinanderfolgenden Arbeitstagen ist die einzige bestätigte Regel mit Datenbedarf vor dem Planungszeitraum. Langfristige Fairness wird in der ersten Fassung nur innerhalb der aktuell geplanten drei Wochen bewertet.

### Regelumfang und Aussagegrenze

Die erste Fassung enthält nur die bestätigten internen Regeln der Service-Leitung. Sie prüft insbesondere keine vollständige gesetzliche oder tarifliche Konformität. Pausen, allgemeine tägliche Höchstarbeitszeit, Ruhezeiten, Ersatzruhetage, Zuschläge und weitere externe Anforderungen sind nicht Bestandteil dieses Katalogs. Sichtbare Meldungen und Dokumentation dürfen keinen umfassenden Compliance-Nachweis behaupten.

Regelarten, Grenzwerte, Prioritäten und Aktivierung sind nicht frei bearbeitbar und werden nicht als Stammdaten in SQLite gespeichert. System 07 erhält keine WPF-Regelkatalogansicht. Der feste Domain-Katalog, ein Application-Lesevertrag, Dokumentation und gemeinsam bestätigte synthetische Beispiele bilden seinen vorgesehenen Umfang.

## Architekturfolgen

- `Domain` besitzt die einzige Regeldefinition und die gemeinsame fachliche Bewertung.
- `Application` koordiniert die Bewertung, erzeugt verständliche Meldungen aus strukturierten Ergebnissen und verlangt später die nötigen Bestätigungen.
- `Planning` übersetzt jede unterstützte automatische Regel in Solver-Bedingung oder Optimierungsziel. Eine unbekannte oder nicht übersetzte Regel blockiert die Generierung sichtbar.
- `Desktop` stellt Regeln, Warnungen und Bestätigungen dar, enthält aber keine eigene Regel- oder Prioritätslogik.
- `Infrastructure` speichert in System 07 keinen bearbeitbaren Regelkatalog. Spätere Planversionen dürfen jedoch die verwendete Katalogversion, Parameter und bestätigten Abweichungen als Momentaufnahme bewahren.
- Fachliche Prüfung und Solver-Übersetzung verwenden dieselben synthetischen Beispielszenarien.

## Übergaben an spätere Systeme

- System 08 definiert Planungsmomentaufnahme, Regelkatalogversion, erforderliche Arbeitstagshistorie, Laufoptionen und manuelle Zusatzzuweisungen.
- System 09 setzt die automatische Hierarchie, vollständige normale Bedarfsdeckung und den `Spr`-Sonderfall um.
- System 10 definiert strukturierte Ergebnis- und Ursachecodes sowie verständliche Konflikt- und Berichtstexte.
- System 11 setzt manuelle Prüfung, blockierte Strukturverletzungen und bestätigte Planungsabweichungen um.
- System 12 bewahrt verwendete Regeln und bestätigte Abweichungen in unveränderlichen Planversionen.

## Bewusst nicht entschieden oder nicht freigegeben

- keine Implementierungsstruktur einzelner Domain-Typen,
- keine konkrete WPF-Bedienung für Warnungs- und Bestätigungsdialoge,
- keine OR-Tools-Modellierung,
- keine Datenbankmigration,
- keine Erweiterung um externe gesetzliche oder tarifliche Regeln,
- keine System-07-Implementierung vor einer eigenen abgenommenen Teil-Roadmap.

## Abnahme

Der Auftraggeber hat am 2026-09-16 bestätigt, dass diese Entscheidung und der konsolidierte Abschnitt des Fragenkatalogs das fachliche Verständnis vollständig und widerspruchsfrei wiedergeben. Auf dieser Grundlage wurde `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md` vollständig umgesetzt, geprüft, ausdrücklich abgenommen und archiviert.
