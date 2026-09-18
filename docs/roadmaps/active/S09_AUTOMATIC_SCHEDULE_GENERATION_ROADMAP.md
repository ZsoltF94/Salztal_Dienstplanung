# Teil-Roadmap System 09 – Automatische Plangenerierung

Status: AG-14B ausdrücklich abgenommen; AG-14C bis AG-14E umgesetzt und automatisch geprüft; technisch funktionsfähiger AG-14F-Ausgangsstand bestätigt; S09B-QB-06A freigegeben, QB-06A.1 bis QB-06A.4 abgenommen und QB-06A.5 technisch geprüft; Qualitätsentscheidung weiterhin offen

Stand: 2026-09-17

## Ziel und Nutzen

Diese Roadmap beschreibt System 09 „Automatische Plangenerierung“.

System 09 erzeugt aus genau einer vorbereiteten und unveränderten `PlanningInputSnapshot` einen zulässigen, bestmöglichen und reproduzierbaren Drei-Wochen-Plan. Für die Automatik zwingende Regeln bleiben unverletzt. Kann ein Bedarf nicht zulässig besetzt werden, bleibt er vollständig oder ausschließlich beim bestätigten `Spr`-Sonderfall teilweise sichtbar ungedeckt. Automatische Überbesetzung ist ausgeschlossen.

Ein Planungslauf verändert den aktuellen Entwurf noch nicht. Ein zulässiges Ergebnis wird zuerst als flüchtige Vorschau angezeigt und erst nach einer bewussten, erneut versionsgeprüften und atomaren Übernahme gespeichert. Abbruch, Zeitablauf ohne zulässiges Ergebnis, nicht unterstützte Regeln und technische Fehler lassen den vorhandenen Entwurf unverändert.

Die Umsetzung wird wegen der besonders hohen Korrektheitsanforderung nicht allein durch einzelne Solver-Tests abgesichert. Gemeinsame Regelszenarien, vollständige Übersetzungsmatrizen der jeweiligen Regelkatalogversion, unabhängige Kleinstfallvergleiche, generierte Invariantentests, kontrollierte Mutationsnachweise, Transaktionsprüfungen, reproduzierbare Lastfälle und ein sichtbares WPF-Gate gehören ausdrücklich zum Systemumfang.

System 09 liefert damit die belastbare automatische Planung. Vor ihrem endgültigen Korrektheitsnachweis wurde der technisch funktionsfähige Generator unverändert durch den abgeschlossenen UI-Zwischenschritt S09A begleitet und wird nun mit dem S09B-Planungsqualitätsbericht messbar beurteilt. Der Bericht dient der Qualitätsanalyse und enthält noch nicht die ausführliche Konflikterklärung aus System 10, die allgemeine manuelle Bearbeitung aus System 11 oder die unveränderlichen Planversionen und Abnahme aus System 12.

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
- `docs/roadmaps/active/S09_AUTOMATIC_SCHEDULE_GENERATION_QUESTIONS.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md`
- `docs/roadmaps/completed/S08_SCHEDULE_MODEL_ROADMAP.md`
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/decisions/S04_SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md`
- `docs/decisions/S09_JOINT_OPTIMIZATION_ACCEPTANCE_SCENARIOS.md`
- `docs/decisions/S07_RULE_CATALOG_EXAMPLES.md`
- ausschließlich erfundene Personen und synthetische Planungsdaten in Dokumentation und Tests

Der ursprüngliche System-09-Fragenkatalog ist vollständig beantwortet und am 2026-09-17 als Grundlage für diesen Roadmap-Entwurf bestätigt worden. Die fünf Ergänzungsfragen zum nachträglich gewünschten Zwischenplanungsschritt AG-14A sind ebenfalls vollständig beantwortet und in diesen Planungsentwurf konsolidiert. Der ergänzte Planungsabschnitt wurde am 2026-09-17 ausdrücklich zur Implementierung freigegeben.

Vor AG-15 wurde ein weiterer fachlicher Änderungswunsch eingebracht. Die Antworten L-01 bis L-10 im Fragenkatalog bestätigen als Primärlösung eine gemeinsame globale Optimierung von Nicht-AH und AH, ein neues erreichbares AH-Mindestziel von 180 Minuten je Person und Woche, die vorrangige Vermeidung von `Spr` und `D` vor der exakten Zielstundenannäherung sowie eine relative faire Annäherung aller Personen an ihr Wochenziel. Die frühere eingefrorene Nicht-AH-/AH-Phasenregel wird damit für den geplanten Zielstand ersetzt. Die bisherige fünfstufige Wunschabfolge bleibt nur als ausdrücklich abnahmebedürftiger Rückfall dokumentiert und wird nicht parallel implementiert.

## Abhängigkeiten und Voraussetzungen

- System 03 liefert stabile Mitarbeitenden- und Typkennungen, Planungsrollen und strukturierte Einsatzfreigaben.
- System 04 liefert die normalen Diensttypen und die zusammengesetzten Muster `D` und `Spr`.
- System 05 liefert die tatsächlichen Bedarfszeiten und deterministisch auflösbaren Personenzahlen.
- System 06 liefert `U`, `K` und rote `X` sowie das nach Abwesenheiten wirksame Wochen-Soll.
- System 07 liefert Katalogversion 1 mit sechs Strukturregeln, elf automatischen Hard Rules, fünf hohen und drei mittleren weichen Regeln, zwei Hinweisen und einem Stabilitätsziel.
- System 08 liefert den aktuellen Entwurf, atomare Bedarfsplätze, geschützte Typ1-Zuweisungen, schwarze `X` als Fachwert, Sperren als Modellwert und die unveränderliche `PlanningInputSnapshot`.
- Die Systeme 03 bis 08 sind abgeschlossen und archiviert.
- `Google.OrTools` ist bereits ausschließlich im Planning-Projekt zentral referenziert. System 09 plant kein weiteres NuGet-Paket und kein externes Testwerkzeug ein.
- Die gemeinsame SQLite-Migrationsfolge und der einzige `ServiceCatalogDbContext` bleiben verbindlich.

## Bestätigte fachliche Grundlagen

### Eingabe und Startschutz

- Ein Lauf verwendet ausschließlich eine vorhandene, aktuelle und unveränderliche `PlanningInputSnapshot`.
- Fehlt die Vorbereitung oder ist sie gegenüber den aktuellen Eingaben veraltet, startet keine Generierung.
- Planning prüft seinen Eingang zusätzlich selbst. Application-Prüfungen ersetzen keinen Schutz des öffentlichen Planning-Vertrags.
- Unbekannte Katalogversionen, unbekannte Regelkennungen und bekannte Regeln ohne technische Übersetzung besitzen getrennte stabile Fehlercodes.
- Alle vor dem Solverstart erkennbaren Regelprobleme werden gesammelt und in stabiler Reihenfolge gemeldet; keine Regel wird stillschweigend ignoriert.
- Eine widersprüchliche geschützte Typ1-Zuweisung oder spätere wirksame Sperre blockiert den Lauf und wird nicht verändert.
- Im bestätigten Einzelbenutzerbetrieb läuft höchstens eine Generierung gleichzeitig.

### Kandidaten und Deckung

- Normale Kandidaten verbinden genau eine aktive automatisch planbare Person mit genau einem vorhandenen zulässigen Bedarfsplatz.
- Planning erfindet keine Mitarbeitenden, Plätze, Diensttypen, Zeiten oder Einsatzfreigaben.
- Ein normaler Bedarfsplatz ist vollständig gedeckt oder vollständig ungedeckt.
- `D` verbindet für dieselbe Person und denselben Tag genau einen vorhandenen Restaurant-Frühdienstplatz und einen vorhandenen Restaurant-Spätdienstplatz. Die Unterbrechung zählt nicht als Arbeitszeit.
- `Spr` ist nur samstags, nur bei aktivierter Laufoption, nur mit den bestätigten Plätzen und nur als Notfall bei sonst verbleibender Restaurant-Unterdeckung zulässig.
- `Spr` deckt den Restaurant-Spätdienst erst ab dem tatsächlichen Wechsel. Jede frühere ungedeckte Minute bleibt sichtbar und zählt in der Bedarfsoptimierung als ungedeckte Mitarbeiterzeit.
- Geschützte Typ1-Zuweisungen verbrauchen genau ihre gespeicherten Deckungen. Ein Typ1-Dienst mit `B` zählt Stunden, deckt aber keinen Bedarf.
- System 09 erzeugt keine `ManualAdditional`-Zuweisung und keine automatische Überbesetzung.
- Jede Person erhält pro Tag höchstens eine normale Zuweisung oder genau ein zusammengesetztes Muster.
- Jeder ansonsten leere Tag einer aktiven Person erhält im Generierungsergebnis genau ein schwarzes `X`. `U`, `K` und rote `X` bleiben unverändert und erhalten kein zusätzliches schwarzes `X`.

### Lauf, Vorschau und Übernahme

- Der produktive Lauf besitzt zunächst eine zentral konfigurierte Obergrenze von 120 Sekunden. Sie ist keine frei bearbeitbare Benutzereinstellung und keine Garantie für beliebig große zukünftige Datenbestände.
- Ungefähr 17 bis 20 aktive Mitarbeitende, davon bis zu fünf AH, bilden die unverbindliche realitätsnahe Testgröße und keine programmierte Obergrenze.
- Ein nachweislich optimales und ein zulässiges, aber noch nicht nachweislich optimales Ergebnis werden zuerst nur als Vorschau angeboten.
- Nach einem ausdrücklichen Abbruch wird auch ein intern bereits gefundener Zwischenstand nicht angeboten.
- `TimedOutWithoutFeasibleResult`, `BlockedByInput`, `UnsupportedRule` und `TechnicalFailure` verändern den Entwurf nicht.
- Eine nicht übernommene Vorschau muss keinen App-Neustart überstehen.
- Vor der Übernahme werden Entwurfskennung, erwartete Entwurfsversion und Snapshot-Kennung erneut geprüft. Bei Abweichung wird das Ergebnis vollständig als veraltet abgelehnt.
- Die Übernahme ersetzt ausschließlich ersetzbare automatisch erzeugte Zuweisungen und schwarze `X`. Geschützte Typ1-Zuweisungen und spätere wirksame Sperren bleiben erhalten.
- Entfernen des alten Generierungsergebnisses, Speichern des neuen Ergebnisses, Speichern der aktuellen Laufmetadaten und Erhöhen der Entwurfsversion bilden genau eine Transaktion.
- System 09 führt keine Vorschlags- oder Snapshot-Historie. Eine aktualisierte Vorbereitung erzeugt einen neuen unveränderlichen Snapshot und ersetzt die vorherige technische Vorbereitung. Unveränderliche fachliche Planversionen entstehen erst in System 12.

### Wiederholbarkeit und Fehlerdiagnose

- Derselbe Snapshot erzeugt mit derselben Solver-Version und denselben Einstellungen dasselbe wertgleiche Ergebnis.
- Der letzte technische Gleichstand verwendet ausschließlich stabile technische Kennungen und Ordnungen, niemals Namen, sichtbare Typcodes oder die aktuelle UI-Reihenfolge.
- Kennungen automatisch erzeugter Zuweisungen werden deterministisch aus Snapshot-Kennung und stabiler fachlicher Zuweisungskombination abgeleitet. Die Erzeugung liegt hinter einem kleinen gekapselten Vertrag und verwendet keine zufällige Laufzeitreihenfolge.
- Technische Fehler liefern einen stabilen Fehlercode und einen Korrelationswert für die Fehlersuche. Strukturierte Logs dürfen Exception-Typ und technischen Kontext enthalten, aber keine Namen, Krankheitsangaben oder vollständigen Planinhalte.
- Planning liefert Codes und fachliche Parameter, keine deutschen Sätze. Application formuliert den minimalen verständlichen System-09-Text; die umfassende Ursachen- und Lösungserklärung bleibt System 10.

## Bisher umgesetzte Optimierungs- und Prioritätsmatrix

Die folgende Reihenfolge beschreibt den automatisch geprüften Stand aus AG-02 bis AG-10. Sie bleibt als historischer Nachweis erhalten, wird aber durch den noch abzunehmenden Änderungsblock AG-14B bis AG-14F ersetzt, bevor AG-15 beginnt.

| Rang | Stufe | Bewertungsmaß und Regeln |
|---:|---|---|
| 0 | Eingabegültigkeit und Struktur | alle sechs Strukturregeln; ungültiger oder nicht unterstützter Input blockiert vor dem Solver beziehungsweise wird konstruktiv ausgeschlossen |
| 1 | Automatische Hard Rules | alle elf Hard Rules; eine Verletzung ist kein zulässiges Solverergebnis |
| 2a | Ungedeckte Mitarbeiterzeit | Summe der tatsächlich ungedeckten Minuten aller Bedarfsplätze; alle Einsatzorte und normalen Diensttypen sind gleich gewichtet; die frühe `Spr`-Lücke bleibt enthalten |
| 2b | Vollständig ungedeckte Plätze | bei gleicher offener Zeit die Zahl vollständig ungedeckter normaler Bedarfsplätze minimieren |
| 3 | Hohe weiche Regeln | `NORMAL_WEEKLY_MINIMUM`, `WEEKLY_CONSECUTIVE_DAYS_OFF`, `RED_X_ADJACENT_DAY_OFF`, `PRE_VACATION_WEEKEND_FREE`, `SPLIT_SHIFT_WEEKLY_MAXIMUM` |
| 4 | Mittlere weiche Regeln | `THREE_WEEK_FREE_WEEKEND`, `MINIMIZE_SPLIT_SHIFTS`, in der getrennten AH-Phase `AH_WEEKLY_TARGET` |
| 5 | Niedrige weiche Regeln | Katalogversion 1 enthält keine niedrige Regel; die Stufe bleibt ausdrücklich vorhanden und leer |
| 6 | Stabilität und faire Verteilung | `CURRENT_PERIOD_FAIR_DISTRIBUTION`: `D`, `Spr`, Spät-, Früh- und Wochenenddienste nur unter vergleichbar geeigneten Personen im aktuellen Zeitraum möglichst gleichmäßig verteilen |
| 7 | Technischer Gleichstand | deterministische stabile Kennungs- und Kandidatenordnung ohne fachliche Zusatzpriorität |

Innerhalb einer hohen, mittleren oder später niedrigen Prioritätsstufe gilt:

1. Zuerst wird die Gesamtzahl verletzter Regelfälle dieser Stufe minimiert.
2. Das Ausmaß wird nur zwischen Verletzungen derselben Regelart verglichen, beispielsweise 60 statt 120 fehlende Minuten oder ein statt zwei zusätzliche `D`.
3. Unterschiedliche Regelarten derselben Priorität werden weder durch Minutenumrechnung noch durch Punkte oder versteckte Gewichte gegeneinander bewertet.
4. Wenn Ausmaße verschiedener Regelarten gegeneinander laufen, bleiben die Lösungen in dieser Stufe fachlich gleichwertig; nachfolgende bestätigte Stufen und zuletzt der technische Gleichstand entscheiden.
5. Vor der Solverumsetzung wird dieser Vergleich als benannter Zielvektor und als ausführbare Vergleichsfunktion festgelegt. Ein unabhängiger Kleinstfall-Referenzlöser und gezielte Gegentests müssen beweisen, dass die CP-SAT-Umsetzung exakt dieselbe Ordnung verwendet. Falls diese Semantik mit dem vorgesehenen Modell nicht ohne versteckte Unterpriorität abbildbar ist, ist dies Handlungsbedarf und kein Anlass für einen stillen Ersatzalgorithmus.

`Spr` wird bereits durch seine zwingende Notfallbedingung besonders vermieden. `D` wird durch das hohe Wochenmaximum und die mittlere Minimierung besonders vermieden. System 09 fügt dafür kein weiteres verborgenes Gewicht hinzu. Bedarfsdeckung bleibt wichtiger als jedes zulässige Vermeidungs- oder Fairnessziel.

## Bisher umgesetzte Nicht-AH- und AH-Phasen

1. Die Nicht-AH-Phase plant ausschließlich automatisch planbare Nicht-AH-Personen und optimiert ihre vollständige Hierarchie.
2. Das danach erreichte Nicht-AH-Ergebnis wird eingefroren.
3. Die AH-Phase sieht ausschließlich noch offene, für AH zulässige Plätze und darf keine Nicht-AH-Zuweisung verdrängen.
4. Innerhalb der AH-Phase gelten alle Struktur- und Hard Rules, insbesondere höchstens zwölf Stunden je AH-Person und Woche.
5. Danach wird die gemeinsame Abweichung vom Zehn-Stunden-Ziel minimiert und die verbleibende Abweichung unter geeigneten AH-Personen möglichst gleichmäßig verteilt.
6. Unter sechs beziehungsweise über zehn bis höchstens zwölf Stunden werden mit `AH_WEEKLY_LOW_NOTICE` oder `AH_WEEKLY_HIGH_NOTICE` strukturiert berichtet und blockieren die Generierung nicht.

## Geplante verbindliche Zielmatrix nach AG-14B

Die Primärlösung optimiert alle automatisch planbaren Nicht-AH- und AH-Personen in einem gemeinsamen globalen Modell. `D` und `Spr` sind von Beginn an zulässige Kandidaten, werden jedoch durch eigene frühere Zielstufen vermieden. Es entstehen keine unumkehrbaren Zwischenpläne.

| Rang | Stufe | Bewertungsmaß und Regeln |
|---:|---|---|
| 0 | Eingabegültigkeit und Struktur | alle nicht übersteuerbaren Strukturregeln; unbekannte oder nicht übersetzte Regeln blockieren |
| 1 | Automatische Hard Rules | alle automatischen Obergrenzen und Verbote bleiben unverletzbar, insbesondere zwölf AH-Stunden je Woche |
| 2a | Ungedeckte Mitarbeiterzeit | tatsächlich offene Minuten minimieren; die frühe `Spr`-Lücke bleibt enthalten |
| 2b | Vollständig ungedeckte Plätze | bei gleicher offener Zeit vollständig offene normale Plätze minimieren |
| 3 | Bestehende hohe Regeln | die fünf bestätigten hohen Regeln unverändert optimieren; insbesondere darf AH den normalen Mindestkorridor nicht verschlechtern |
| 4a | `Spr` vermeiden | bei identischen früheren Werten die Zahl gewählter `Spr` minimieren; die Notfallbedingung bleibt zusätzlich zwingend |
| 4b | `D` vermeiden | danach die Gesamtzahl gewählter Doppeldienste minimieren; das hohe Wochenmaximum bleibt bereits in Rang 3 enthalten |
| 5 | AH-Mindestintegration | je AH-Person und Woche eine Unterschreitung von 180 Minuten bei vorhandenem zulässigem Bedarf vermeiden; zuerst betroffene AH-Wochen, danach fehlende Minuten minimieren |
| 6 | Relative Wochenzielannäherung | Nicht-AH gegen das wirksame Wochensoll und AH gegen 600 Minuten vergleichen; zuerst die größte relative Abweichung verkleinern, danach verbleibende Abweichungen fair ausgleichen |
| 7 | Verbleibende mittlere Regeln | das freie Drei-Wochen-Wochenende und weitere nicht ersetzte mittlere Katalogregeln optimieren; `MINIMIZE_SPLIT_SHIFTS` und das bisher getrennte `AH_WEEKLY_TARGET` gehen nicht doppelt ein |
| 8 | Niedrige Regeln | Katalogversion 2 enthält zunächst weiterhin keine niedrige Regel |
| 9 | Einsatzfairness | Früh-, Spät-, Wochenend-, `D`- und `Spr`-Belastung unter vergleichbar geeigneten Personen ausgleichen, ohne frühere Ränge zu verschlechtern |
| 10 | Technischer Gleichstand | stabile technische Kennungen und reproduzierbare Ordnung ohne Namen, Typcodes oder UI-Reihenfolge |

Für die relative Zielannäherung werden keine frei geschätzten Strafpunkte verwendet. Die technische Darstellung muss eine nachvollziehbare exakte oder nachweislich verlustfreie ganzzahlige Vergleichssemantik erhalten. Falls das nicht ohne Rundungs- oder Gewichtungsfehler möglich ist, wird vor der Solveränderung gestoppt und Handlungsbedarf gemeldet.

Das 180-Minuten-Ziel gilt nur bei vorhandenem zulässigem Bedarf und darf keinen früheren Rang verschlechtern. Eine AH-Person mit 180 bis unter 360 Minuten erfüllt das neue Mindestziel, erhält aber weiterhin den bestehenden Unter-Sechs-Stunden-Hinweis. Die Grenze von zwölf Stunden bleibt unverletzbar.

Die gemeinsame Optimierung darf normale Dienste zwischen Nicht-AH und AH umverteilen. Sie darf jedoch weder den normalen Mindestkorridor zugunsten von AH verschlechtern noch `D` oder `Spr` nur zur Stundenauffüllung erzeugen. Früh und Spät werden weiterhin nur unter vergleichbar geeigneten Personen fair verteilt.

## Dokumentierte Rückfalllösung

Falls das fachliche Gate in AG-14F die gemeinsame Optimierung ausdrücklich ablehnt, wird nicht automatisch umgeschaltet. Zuerst entsteht ein eigener abgenommener Folgeplan für diese fünf Stufen:

1. Nicht-AH ohne `D` und `Spr`, mit fairer Früh-/Spätverteilung.
2. AH füllt verbleibende normale Lücken, ebenfalls ohne `D` und `Spr`.
3. Nicht-AH reduziert verbleibende Lücken mit zulässigen `D`- und `Spr`-Mustern.
4. AH reduziert danach verbleibende Lücken mit zulässigen `D`- und `Spr`-Mustern.
5. Normale AH-Dienste werden bei Bedarf an unter Soll liegende Nicht-AH-Personen umverteilt, ohne das erreichbare AH-Mindestziel von 180 Minuten oder frühere höhere Ziele zu verschlechtern.

Diese Rückfalllösung wird in AG-14B bis AG-14F weder implementiert noch als ungenutzte Strategieabstraktion vorbereitet.

## Ergebnisstatus und Ergebnisinhalt

Die öffentlichen Application-Verträge unterscheiden mindestens:

- `Optimal`,
- `FeasibleNotProvenOptimal`,
- `Cancelled`,
- `TimedOutWithoutFeasibleResult`,
- `BlockedByInput`,
- `UnsupportedRule`,
- `TechnicalFailure`.

Ein Planning-Ergebnis enthält mindestens Snapshot-Kennung, Entwurfskennung und erwartete Version, Status, Solvername und -version, relevante deterministische Einstellungen, Zeitgrenze und Laufzeiten, automatische Zuweisungen, schwarze `X`, vollständig und teilweise offene Bedarfe, Zielvektor, strukturierte Regelbewertungen, Hinweise und technische Fehlercodes. OR-Tools-Objekte überschreiten die Planning-Grenze nicht.

## Umfang dieser Roadmap

- unveränderliche Application-Verträge für Planungslauf, Vorschau, Status, Zielvektor, Regelbewertungen und Übernahme,
- fachlich eindeutige Vergleichssemantik für alle Optimierungsstufen,
- vollständiges prüfbares Übersetzungsverzeichnis der historischen Katalogversion 1 und der geplanten Katalogversion 2,
- Eingangsvalidierung vor dem Solverstart,
- deterministische Kandidaten für normale Dienste, `D` und `Spr`,
- CP-SAT-Modell für Struktur, automatische Hard Rules, Bedarfsdeckung und hierarchische weiche Ziele,
- gemeinsame globale Nicht-AH-/AH-Optimierung mit AH-Mindestintegration und relativer Wochenzielannäherung,
- deterministische Rückübersetzung in vorhandene Domain-Werte,
- unabhängige fachliche Nachprüfung jedes Vorschlags vor seiner Anzeige,
- Erzeugung schwarzer `X` für alle sonst freien Tage,
- kontrollierter Abbruch, Zeitgrenze, stabile Laufstatus und hilfreiche datensparsame technische Diagnose,
- flüchtige Ergebnisvorschau sowie bewusstes Verwerfen oder Übernehmen,
- atomare Speicherung des übernommenen Generierungsergebnisses und genau eines aktuellen Laufmetadatensatzes,
- minimale Erweiterung der bestehenden Ansicht „Dienstplan SER“ um Start, Arbeitszustand, Abbruch, Vorschau und Übernahme,
- umfangreiche Domain-, Application-, Planning-, Infrastructure-, Desktop- und Architekturprüfungen,
- fachliche Prüfung ausgewählter Kombinationsfälle und sichtbares WPF-Gate,
- wiederholbarer synthetischer Größen- und Laufzeitnachweis.

## Nicht-Umfang

- ausführliche Ursachenanalyse je ausgeschlossener Person,
- vollständige deutsche Konflikterklärungen und Lösungsvorschläge aus System 10,
- allgemeine manuelle Planbearbeitung, manuelle Zusatzbesetzung oder bedienbare Sperren aus System 11,
- bestätigte manuelle Regelabweichungen,
- unveränderliche Planversionen, Abnahme, Wiederherstellung oder Vorschlagshistorie aus System 12,
- Excel-Export,
- portable Windows-Veröffentlichung oder Test auf einem sauberen Windows-11-System,
- Zeitkonten oder langfristige Fairness vor dem aktuellen Drei-Wochen-Zeitraum,
- externe gesetzliche oder tarifliche Regeln,
- frei einstellbare Laufzeit, frei konfigurierbare Prioritäten oder Punktwerte,
- eine sichtbare oder implementierte Funktion „Alternative Planung erzeugen“,
- zufällige Ergebnisvarianten,
- Performanceoptimierung über die bestätigte erste Größenordnung hinaus,
- zusätzliche NuGet-Pakete, allgemeine Plugin-Infrastruktur oder ein zweiter Solver.

## Modularitätsvertrag

| Baustein | Eine Verantwortung | Darf nicht übernehmen |
|---|---|---|
| `Domain/Rules` und `Domain/Scheduling` | Regeldefinition, fachlicher Zielvergleich, planunabhängige Invarianten und unabhängige Ergebnisbewertung | OR-Tools, Datenbank, WPF oder deutsche UI-Sätze |
| `Application/Scheduling` | Lauf koordinieren, unveränderliche Modulverträge, Vorschauzustand und atomare Übernahme anfordern | Solvervariablen, EF-Entitäten oder eigene Regelwerte |
| `Planning/Validation` | vollständigen Planning-Eingang und unterstützte Katalogversion prüfen | Daten nachladen oder Benutzermeldungen formulieren |
| `Planning/Candidates` | deterministische zulässige Kandidaten beschreiben | Optimierungsrangfolge oder Persistenz |
| `Planning/ModelBuilding` und `Planning/Rules` | Kandidaten und jede Regelart in CP-SAT-Bedingungen oder Ziele übersetzen | Application-Abläufe oder deutsche Texte |
| `Planning/Optimization` | benannte lexikografische Stufen, Nicht-AH-/AH-Phasen und feste Solver-Einstellungen koordinieren | Regelparameter duplizieren oder UI-Zustand verwalten |
| `Planning/Results` | Solverentscheidung zurückübersetzen und gegen fachliche Invarianten prüfen | Entwürfe speichern oder Konfliktsätze erfinden |
| `Infrastructure/Persistence/Scheduling` | aktuellen Entwurf und Laufmetadaten atomar ersetzen | Planung ausführen oder fachliche Prioritäten bestimmen |
| `Desktop/Features/Scheduling` | Arbeitszustand und Vorschau darstellen, Benutzeraktionen auslösen | Solver, Datenbank oder Regelbewertung direkt verwenden |
| `Desktop/Composition` | konkrete Planning- und SQLite-Adapter verdrahten | Fachlogik enthalten |

Zusätzliche Schutzregeln:

- Planning liest nie aus SQLite und speichert nie selbst.
- Application referenziert Planning nicht konkret, sondern nur einen eigenen schmalen Port.
- Ein Regelübersetzer behandelt genau eine Regelart oder eine eng zusammengehörige Regelfamilie.
- Alle unterstützten Regelkennungen werden in einem vollständigen Registry-Vertrag explizit klassifiziert: Vorprüfung, Hard Constraint, Optimierungsziel, Stabilitätsziel, Hinweis oder im aktuellen Entwurf nicht anwendbare Strukturprüfung.
- Gemeinsame technische Modellbausteine dürfen wiederverwendet werden; Regelgrenzen und Prioritäten werden nicht kopiert.
- Der Optimierungsorchestrator kennt die Stufen, aber nicht die fachlichen Grenzwerte einzelner Regeln.
- Der bestehende `ScheduleOverviewViewModel` bleibt Koordinator und erhält kein Solver- oder Persistenzwissen. Der Generierungszustand liegt in einem eigenen kleinen ViewModel.
- Es entsteht kein `PlanningManager`, `SchedulingHelper`, allgemeines Repository oder öffentliches ungenutztes Strategieframework.

## Explizite Code-Anker des aktuellen Codes

### Vorhandene Domain-Anker

| Aktueller Code-Anker | Verwendung in System 09 |
|---|---|
| `src/Salztal.Dienstplanung.Domain/Rules/InitialRuleCatalog.cs` | einzige Quelle der 28 Regeln und Katalogversion 1 |
| `src/Salztal.Dienstplanung.Domain/Rules/InitialStructureRuleDefinitions.cs` | sechs strukturelle Schutzregeln |
| `src/Salztal.Dienstplanung.Domain/Rules/InitialAutomaticHardRuleDefinitions.cs` | elf unverletzbare automatische Regeln |
| `src/Salztal.Dienstplanung.Domain/Rules/InitialSoftRuleDefinitions.cs` | fünf hohe und drei mittlere Ziele; niedrige Stufe bleibt leer |
| `src/Salztal.Dienstplanung.Domain/Rules/InitialStabilityRuleDefinitions.cs` | Stabilitäts- und Fairnessziel des aktuellen Zeitraums |
| `src/Salztal.Dienstplanung.Domain/Rules/InitialNoticeRuleDefinitions.cs` | beide nicht blockierenden AH-Hinweise |
| `src/Salztal.Dienstplanung.Domain/Scheduling/ScheduleAssignment.cs` | erzeugt validierte normale, `D`- und `Spr`-Zuweisungen mit tatsächlichen Zeiten |
| `src/Salztal.Dienstplanung.Domain/Scheduling/DemandCoverage.cs` | bildet Voll- und ausschließlich bestätigte `Spr`-Teildeckung ab |
| `src/Salztal.Dienstplanung.Domain/Scheduling/ScheduleDraft.cs` | bewahrt Entwurfsversion, geschützte Zuweisungen und generierte Ergebnisse |
| `src/Salztal.Dienstplanung.Domain/Scheduling/GeneratedDayOffMarker.cs` | vorhandener Fachwert für ein schwarzes `X` |

### Vorhandene Application-Anker

| Aktueller Code-Anker | Verwendung in System 09 |
|---|---|
| `src/Salztal.Dienstplanung.Application/Scheduling/PlanningInputSnapshot.cs` | vollständige unveränderliche Eingabe; wird nicht durch ein zweites Solver-Inputmodell in der UI ersetzt |
| `src/Salztal.Dienstplanung.Application/Scheduling/PlanningHistorySnapshot.cs` | liefert die vorhandene Vorgeschichte und ihre Vollständigkeit |
| `src/Salztal.Dienstplanung.Application/Scheduling/ScheduleAssignmentSnapshot.cs` | bestehende technische Übergabeform für geschützte Zuweisungen und Deckung |
| `src/Salztal.Dienstplanung.Application/Scheduling/ScheduleDemandSlotSnapshot.cs` | stabile atomare Plätze mit tatsächlicher Zeit und Ordnung |
| `src/Salztal.Dienstplanung.Application/Scheduling/GetScheduleWorkspaceQuery.cs` | lädt den sichtbaren Entwurf nach einer Übernahme neu; führt keinen Solver aus |
| `src/Salztal.Dienstplanung.Application/Scheduling/PreparePlanningInputCommand.cs` | bleibt Eigentümer der bewussten Vorbereitung und Snapshot-Aktualisierung |

### Vorhandene technische und UI-Anker

| Aktueller Code-Anker | Verwendung in System 09 |
|---|---|
| `src/Salztal.Dienstplanung.Planning/Salztal.Dienstplanung.Planning.csproj` | kapselt das bereits referenzierte `Google.OrTools`; derzeit nur Gerüst |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/Scheduling/SqliteScheduleStore.cs` | konkreter Adapter für den neuen schmalen Übernahme-Port und konsistente Leseverträge |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/Scheduling/ScheduleEntities.cs` | wird um genau die aktuellen Laufmetadaten ergänzt; keine Vorschlagshistorie |
| `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContext.cs` | bleibt einziger `DbContext` und setzt die gemeinsame Migration fort |
| `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewViewModel.cs` | koordiniert Laden und den neuen untergeordneten Generierungszustand |
| `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewView.xaml` | erhält den minimalen sichtbaren Generierungsablauf |
| `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowComposition.cs` | bleibt einziger Ort für den konkreten Planning-Adapter |
| `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowDependencies.cs` | erweitert die gruppierten Scheduling-Abhängigkeiten ohne konkrete Adapter im Feature |
| `tests/Salztal.Dienstplanung.Architecture.Tests/ProductionProjectBoundaryTests.cs` | schützt Projektreferenzen und Planning-Grenze |
| `tests/Salztal.Dienstplanung.Architecture.Tests/DesktopCompositionBoundaryTests.cs` | schützt die alleinige konkrete Verdrahtung in `Desktop.Composition` |

## Geplante neue Code-Anker

Die folgenden Namen sind verbindliche Richtungsanker. Kleine interne Typnamen dürfen innerhalb des jeweiligen Schritts begründet präzisiert werden, solange Verantwortung, Sichtbarkeit und Modulgrenze unverändert bleiben.

### Domain

- kleine unveränderliche Werte für Zielstufe, Zielwert und Zielvektor,
- ein expliziter Vergleichsvertrag für die bestätigte lexikografische Reihenfolge ohne versteckte Gewichte,
- ein schmaler fachlicher Auswertungskontext für einen erzeugten Plan,
- getrennte Auswerter für Struktur, Hard Rules, weiche Regeln, Stabilität und Hinweise,
- eine atomare Fachoperation am `ScheduleDraft`, welche ausschließlich ersetzbare automatische Zuweisungen und schwarze `X` austauscht und geschützte Werte bewahrt.

### Application

- `IAutomaticSchedulePlanner` als einziger öffentlicher Planning-Port,
- unveränderliche Request-, Status-, Ergebnis-, Ziel- und Fehlerwerte,
- `GenerateAutomaticScheduleCommand` für Start, Vorprüfung und flüchtige Vorschau,
- `AcceptAutomaticScheduleProposalCommand` für erneute Versionsprüfung und bewusste Übernahme,
- ein schmaler `IAcceptAutomaticScheduleProposalStore` für genau die atomare Speicherung,
- aktuelle Laufmetadaten ohne Vorschlags- oder Planversionshistorie,
- kurze deutsche System-09-Meldungen für Startblockade, Status, Abbruch, Versionskonflikt und technischen Fehler.

### Planning

Interne Bereiche unter `Salztal.Dienstplanung.Planning`:

- `Validation` für Eingang, Katalogversion, Regelvollständigkeit und geschützte Zuweisungen,
- `Candidates` für stabile normale, `D`- und `Spr`-Kandidaten,
- `Rules` für das vollständige Übersetzungsverzeichnis und kleine Regelübersetzer,
- `ModelBuilding` für gemeinsame Variablen, Deckung und konstruktive Strukturgrenzen,
- `Optimization` für Zielvektor, gemeinsame lexikografische Personaloptimierung und deterministische Einstellungen,
- `Results` für Rückübersetzung, schwarze `X`, offene Bedarfe, Hinweise und unabhängige Nachprüfung,
- ein kleiner `AutomaticSchedulePlanner` als öffentliche Adapterimplementierung, der diese Verantwortungen nur koordiniert.

### Infrastructure

- eine aktuelle Laufmetadaten-Entity je Entwurf,
- Implementierung des atomaren Übernahme-Ports in `SqliteScheduleStore`,
- relationale Eindeutigkeits- und Fremdschlüsselregeln für genau einen aktuellen Metadatensatz,
- eine neue generierte Migration in der bestehenden gemeinsamen Migrationsfolge.

### Desktop

- ein eigenes `AutomaticScheduleGenerationViewModel` für Start, Abbruch, Laufzeit, Vorschau, Verwerfen und Übernehmen,
- kleine unveränderliche Anzeigeobjekte für Ergebnisstatus und offene Bedarfe,
- Einbindung in `ScheduleOverviewViewModel` und `ScheduleOverviewView.xaml`,
- Verdrahtung ausschließlich in `Desktop.Composition`.

### Tests

- neue Planning-Tests gegliedert wie das Produktionsmodul,
- ergänzte Domain- und Application-Tests unter `Scheduling`,
- temporäre SQLite-Tests unter `Infrastructure.Tests/Scheduling`,
- ViewModel- und Renderprüfungen unter `Desktop.Tests/Features/Scheduling`,
- erweiterte Architekturtests,
- gemeinsame synthetische Szenarien mit stabilen Kennungen,
- ein ausschließlich im Testprojekt liegender erschöpfender Kleinstfall-Referenzlöser,
- deterministisch erzeugte kleine Invariantenfälle ohne neues Testpaket.

## Ablauf und Datenfluss

1. Desktop fordert über `GenerateAutomaticScheduleCommand` einen Lauf für den sichtbaren Entwurf und die sichtbare Vorbereitung an.
2. Application prüft Entwurfskennung, Version, Snapshot-Kennung, Aktualität, Typ1-Bereitschaft und die globale Laufkoordination.
3. Application übergibt ausschließlich die vollständige unveränderliche Momentaufnahme und kontrollierte Laufparameter an `IAutomaticSchedulePlanner`.
4. Planning validiert die im Snapshot enthaltene Katalogversion und jede Regelzuordnung, bildet stabile Kandidaten und startet erst danach CP-SAT.
5. Das gemeinsame Modell optimiert Nicht-AH und AH in der geplanten Zielreihenfolge. Nach jeder Stufe wird der erreichte Wert vor der Folgestufe fixiert; keine Rollen-Zwischenlösung wird vorzeitig eingefroren.
6. Planning übersetzt die Entscheidung in vorhandene fachliche Zuweisungen und schwarze `X`, berechnet offene Zeiträume, Musteranzahlen, AH-Mindestwerte, relative Wochenzielwerte und Hinweise und führt eine unabhängige fachliche Nachprüfung durch.
7. Application hält einen zulässigen Vorschlag nur flüchtig. Verwerfen, Abbruch und Fehler schreiben nichts.
8. Bei „Plan übernehmen“ prüft Application Entwurfskennung, Version und Snapshot-Kennung erneut.
9. Infrastructure ersetzt in genau einer Transaktion die ersetzbaren automatischen Zuweisungen, schwarzen `X` und aktuellen Laufmetadaten und erhöht die Entwurfsversion.
10. Desktop lädt den Entwurf neu. Die verwendete Vorbereitung ist wegen der neuen Entwurfsversion anschließend sichtbar veraltet; eine spätere Neugenerierung verlangt eine bewusste Aktualisierung.

## Teststrategie und Korrektheitsnachweise

### Gemeinsame Regelmatrix

- Jede Regel der historischen Katalogversion 1 und der neuen Katalogversion 2 besitzt eine explizite Zuordnung zu Vorprüfung, Solverbedingung, Optimierungsziel, Hinweis oder derzeit nicht anwendbarer Strukturprüfung.
- Für jede automatische Regel werden erfüllte, verletzte, Grenzwert- und nicht anwendbare Fälle geprüft.
- Jede der elf Hard Rules besitzt zusätzlich einen Fall, in dem der Solver Bedarf offenlassen muss, statt die Regel zu verletzen.
- Jede weiche Regel besitzt zwei ansonsten vergleichbare zulässige Pläne; der fachlich bessere muss gewinnen.
- Die Szenarien aus `S07_RULE_CATALOG_EXAMPLES.md` bleiben fachlich unverändert und werden für Domain-Auswertung und Solverübersetzung gemeinsam verwendet.

### Mehrere unabhängige Orakel

- Domain-Nachprüfung validiert das zurückübersetzte Ergebnis ohne OR-Tools-Objekte.
- Ein einfacher erschöpfender Referenzlöser bewertet bewusst kleine Fälle ohne produktiven Solvercode und vergleicht Zulässigkeit und vollständigen Zielvektor mit CP-SAT.
- Deterministisch erzeugte kleine Fälle prüfen allgemeine Invarianten und Wiederholbarkeit.
- Kontrollierte Mutationen müssen nachweislich von den Tests erkannt werden; die mutierten Produktionsstände werden danach vollständig entfernt.

### Verbindliche Mutationsfälle

- automatische Überbesetzung zugelassen,
- Tagesblockade ignoriert,
- normales oder AH-Wochenmaximum entfernt,
- achte aufeinanderfolgende Arbeit zugelassen,
- Nicht-AH-Ergebnis durch AH verdrängt,
- höhere Optimierungsstufe durch viele niedrigere Verbesserungen überstimmt,
- `Spr` ohne Notfall oder mit verborgener früher Teilunterdeckung zugelassen,
- Ergebnisnachprüfung oder Versionskontrolle vor Übernahme umgangen.

### Wiederholbarkeit und Last

- Derselbe Snapshot wird mehrfach im selben Prozess, nach neuem Adapteraufbau und in getrennten Testläufen gelöst.
- Zuweisungen, Kennungen, schwarze `X`, Status und Zielvektor müssen wertgleich sein.
- Ein synthetischer Lastfall mit ungefähr 20 aktiven Personen, bis zu fünf AH, 21 Tagen, aktuellem vollständigem Startbedarf und gezielten Engpässen misst Modellaufbau, Nicht-AH-Solverzeit, AH-Solverzeit, Rückübersetzung und Gesamtzeit getrennt.
- Ein zusätzlicher größerer synthetischer Schutzfall beweist, dass keine feste Obergrenze von 20 Personen einprogrammiert wurde.
- Die Zwei-Minuten-Grenze ist zunächst ein wiederholbarer, separat dokumentierter Nachweis auf festgehaltener Hardware und kein flakyanfälliger normaler Unit-Test. Eine deutliche Überschreitung ist dennoch Handlungsbedarf vor Systemabschluss.

## Offene Entscheidungen

Für die Abnahme dieses Roadmap-Entwurfs bestehen keine offenen fachlichen Entscheidungen.

Folgende technische Details dürfen innerhalb der festgelegten Grenzen präzisiert werden:

- Dateiaufteilung kleiner unveränderlicher Request-, Ergebnis- und Zielwerte,
- konkrete interne Namen einzelner Regelübersetzer,
- CP-SAT-Variablenform und Modellbausteine,
- konkrete Abbruchkopplung an die verwendete OR-Tools-Version,
- konkrete relationale Spaltenaufteilung der aktuellen Laufmetadaten,
- konkrete barrierearme Anordnung des Generierungsbereichs in der bestehenden Ansicht,
- technische Implementierung der deterministischen Kennungsableitung.

Eine Präzisierung darf keine Regel, Prioritätsstufe, Phasenreihenfolge, Teildeckungsgrenze, Atomaritätsgrenze oder Modulabhängigkeit verändern. Ist die bestätigte Behandlung gleichrangiger Regelarten technisch nicht ohne versteckte Unterpriorität abbildbar, wird vor dem Solverausbau gestoppt und gemeinsam entschieden.

## Statuskennzeichnung

- `[ ]` noch nicht begonnen
- `[~]` in Bearbeitung oder wartet auf Abnahme
- `[x]` geprüft und ausdrücklich abgenommen
- `[!]` blockiert; der konkrete Grund steht direkt beim Schritt

Ein Schritt wird erst nach seinem vereinbarten Nachweis und der ausdrücklichen Abnahme als `[x]` markiert.

## Nummerierte Schritte

Nach ausdrücklicher Abnahme dieser Roadmap gilt die bedingte automatische Weiterführung: Ein planmäßig und fehlerfrei abgeschlossener Schritt darf direkt in den nächsten übergehen, wenn weder eine unerwartete Entscheidung oder kritische Frage noch ein externes, manuelles, visuelles oder fachliches Gate besteht. Bei Handlungsbedarf wird gestoppt. Nach jedem abgeschlossenen Schritt ertönt ein Abschlusston; bei Handlungsbedarf zusätzlich ein unterscheidbarer Hinweiston.

### AG-01 – Detailroadmap und beantworteten Fragenkatalog konsolidieren

Status: `[x]` – Roadmap am 2026-09-17 ausdrücklich abgenommen

Umfang:

- alle bestätigten Antworten einschließlich der Folgeentscheidungen zu `Spr`, Snapshot-Lebensdauer, gleichrangigen Regeln und versteckten Gewichten konsolidieren,
- Fachumfang, Nicht-Umfang, Prioritätsmatrix und Phasen festhalten,
- aktuelle und geplante Code-Anker bis auf Datei- oder Verantwortungsniveau benennen,
- Korrektheitsstrategie, kleine Implementierungsschritte und echte Gates festlegen,
- `MASTER_ROADMAP.md`, `STATUS.md` und den verständlichen Service-Leitungsstand auf den Entwurf abstimmen,
- noch keinen Produktionscode, Solver, Datenbankschema oder sichtbare System-09-Funktion ändern.

Prüfung:

- vollständiger Abgleich mit den Systemen 07 und 08 sowie Architektur und Clean-Code-Regeln,
- gezielte Prüfung aller genannten aktuellen Code-Anker,
- Suche nach widersprüchlichen Aussagen zu `Spr`, Snapshot-Ersetzung, Prioritätsreihenfolge, AH-Phase, Übernahme und Systemgrenzen,
- `git diff --check`, Pfadprüfung und Kontrolle der Markdown-Zeilenenden.

Abnahmebedingung:

- Der Auftraggeber bestätigt Fachumfang, Prioritätsmatrix, Modularitätsvertrag, Testtiefe und Schrittreihenfolge ausdrücklich oder nennt Änderungswünsche. Vor dieser Abnahme beginnt AG-02 nicht.

Nachweis am 2026-09-17:

- Fachumfang, Prioritätsmatrix, Modularitätsvertrag, Testtiefe und Schrittreihenfolge wurden gegen den vollständig beantworteten Fragenkatalog, die Systeme 07 und 08 sowie Architektur und Clean Code geprüft.
- Alle benannten vorhandenen Code-Anker sind vorhanden; Pfade, Querverweise, CRLF-Zeilenenden und `git diff --check` sind ohne Fehler geprüft.
- Der Auftraggeber hat den Roadmap-Entwurf ausdrücklich freigegeben und den Beginn von AG-02 beauftragt.

### AG-02 – Zielvektor, gemeinsame Regelauswertung und Modulverträge

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- kleine unveränderliche Domain-Werte für Zielstufe, Regelverletzungsfall, Ausmaß und vollständigen Zielvektor einführen,
- die bestätigte Vergleichssemantik ohne regelübergreifende versteckte Gewichte ausführbar machen,
- den schmalen fachlichen Auswertungskontext und die getrennten Regelprüfungen als OR-Tools-unabhängige Nachprüfung vorbereiten,
- Application-Verträge für Request, Ergebnis, Status, Fehler, Vorschau und Planning-Port einführen,
- deterministische Zuweisungskennung als gekapselten Vertrag festlegen,
- gemeinsame synthetische Szenarien und den Kleinstfall-Referenzrahmen anlegen, noch ohne produktiven Solver.

Prüfung:

- Domain-Tests für jede Zielstufe, gleichrangige gleiche und unterschiedliche Regelarten, Dominanzgrenzen und stabilen Gleichstand,
- Vertrags- und Unveränderlichkeitstests für alle öffentlichen Application-Werte,
- Gegentests, die eine versteckte Unterpriorität oder fachfremde Punkte erkennen,
- Build sowie alle Domain-, Application- und Architekturtests.

Abnahmebedingung:

- Zielvergleich und Modulverträge sind eindeutig, unabhängig von OR-Tools testbar und verändern keine vorhandene Fachregel.

Nachweis am 2026-09-17:

- `ScheduleObjectiveVector` und `ScheduleObjectiveComparer` bilden Bedarfsminuten, vollständig offene Plätze, hohe, mittlere und niedrige Regeln, Stabilität und technischen Gleichstand in der bestätigten Reihenfolge ohne Strafpunkte ab.
- Die Fallzahl entscheidet innerhalb einer Prioritätsstufe zuerst. Ausmaße werden ausschließlich je derselben Regelart summiert und verglichen; gegenläufige Ausmaße verschiedener Regelarten bleiben in dieser Stufe gleichwertig.
- Ein OR-Tools-unabhängiger `ScheduleEvaluationContext`, einzelne Regelprüfer-Verträge und ein vollständiger, nach Regelfamilien getrennter Auswertungssatz bereiten die spätere fachliche Nachprüfung vor.
- Der öffentliche Application-Vertrag umfasst unveränderlichen Request, alle sieben Laufstatus, Fehler, Vorschau, Ergebnisinhalt, Laufmetadaten und den schmalen `IAutomaticSchedulePlanner`-Port. Eine gekapselte SHA-256-Ableitung erzeugt Zuweisungskennungen reproduzierbar aus ausschließlich stabilen technischen Werten.
- Ein ausschließlich im Planning-Testprojekt liegender erschöpfender Referenzrahmen verwendet gemeinsame synthetische Dominanz- und Gleichrangigkeitsszenarien; ein produktiver Solver wurde noch nicht implementiert.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 240 Application-, 6 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Es wurden keine Pakete, Datenbankmigrationen, WPF-Funktionen, echten Namen oder produktiven Plandaten ergänzt.

### AG-03 – Planning-Eingangsvalidierung und vollständiges Regelverzeichnis

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- unterstützte Katalogversion, Zeitraum, Kennungen, Typbezüge, tatsächliche Zeiten, geschützte Zuweisungen und Typ1-Bereitschaft am Planning-Eingang prüfen,
- jede der 28 Regeln genau einer unterstützten technischen Wirkung zuordnen,
- unbekannte Version, unbekannte Regel und bekannte nicht übersetzte Regel getrennt melden,
- sämtliche vor dem Solver erkennbaren Regelprobleme in stabiler Reihenfolge sammeln,
- den Solver bei jeder Blockade nachweislich nicht starten.

Prüfung:

- Vollständigkeitstest exakt gegen `InitialRuleCatalog`,
- Negativtests für fehlende, doppelte, unbekannte und falsch klassifizierte Regeln,
- Tests der drei Fehlercodegruppen sowie stabiler Parameterreihenfolge,
- Typ1- und geschützte-Zuweisungsfälle aus den bestätigten Szenarien,
- Planning-, Application- und Architekturtests.

Abnahmebedingung:

- Keine Katalogregel kann stillschweigend ignoriert werden; blockierter Input erreicht keinen Solver.

Nachweis am 2026-09-17:

- Ein explizites technisches Register ordnet jede der 28 Definitionen aus `InitialRuleCatalog` genau einer Struktur-, Hard-Rule-, Optimierungs-, Hinweis- oder Stabilitätswirkung zu. Vollständigkeit, Eindeutigkeit, unbekannte Kennungen und falsche Zuordnungen werden unabhängig geprüft.
- Die defensive Planning-Eingangsvalidierung prüft Katalogversion und Definitionen, Zeitraum und technische Kennungen, Typ- und Katalogbezüge, tatsächliche Bedarfszeiten, Vorgeschichte, geschützte Typ1-Zuweisungen sowie die Typ1-Wochenbereitschaft. Alle Probleme werden als unveränderliche Werte in stabiler Reihenfolge gesammelt.
- Unbekannte Katalogversion, unbekannte Regel und bekannte nicht übersetzte Regel bleiben getrennte öffentliche Fehlercodes mit Katalogversion und Regelkennung. Weitere Eingabefehler blockieren mit technischen Kennungen und Datum, ohne Namen oder Planinhalte in die Diagnose zu übernehmen.
- Der zentrale Planning-Einstieg übergibt ausschließlich vollständig validierte Eingaben an die nachfolgende Engine. Negativtests belegen für Regel-, Zeitraum-, Schutz- und Typ1-Blockaden, dass die Engine nicht aufgerufen wird.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 23 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Es wurden keine Solverbedingungen, Kandidatenbildung, Pakete, Datenbankmigrationen oder WPF-Funktionen ergänzt. Ausschließlich synthetische Testdaten werden verwendet.

### AG-04 – Deterministische Kandidaten und konstruktive Strukturgrenzen

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- stabile normale Kandidaten ausschließlich aus vorhandenen Personen, Freigaben und Bedarfsplätzen bilden,
- `D` aus genau zwei passenden Restaurant-Plätzen und `Spr` aus den bestätigten Samstagsteilen bilden,
- geschützte Typ1-Deckungen vorab verbrauchen und `B` bedarfsneutral behandeln,
- Doppelzuweisung, Zeitüberschneidung, belegten Sperrtag, unbekannte Referenz, normale Teildeckung und Überbesetzung bereits durch die Modellform ausschließen,
- stabile Variablen- und Kandidatennamen ausschließlich aus technischen Kennungen erzeugen.

Prüfung:

- Kandidatentests für gültige und jede einzeln ungültige Kombination,
- Voll- und `Spr`-Teildeckungsfälle mit tatsächlichen statt Standardzeiten,
- mehrere gleichartige Plätze mit stabiler Ordnung,
- kein Kandidat für inaktive, Typ1-, gesperrte oder nicht freigegebene Personen,
- generierte kleine Invariantenfälle und erste Referenzlöservergleiche.

Abnahmebedingung:

- Der Kandidatenraum enthält alle und nur die bestätigten Möglichkeiten; Strukturverstöße sind konstruktiv unmöglich.

Nachweis am 2026-09-17:

- Der unveränderliche Kandidatenraum bildet normale Zuweisungen nur aus automatisch planbaren Personen mit regulärer Dienstfreigabe und genau einem vorhandenen vollständigen Bedarfsplatz. `ManualSuggestion`, Typ1, fehlende Freigaben und belegte Sperrtage erzeugen keine automatischen Kandidaten.
- `D` entsteht ausschließlich aus einer strukturiert freigegebenen Früh-/Spät-Kombination desselben Restauranttages mit echter Unterbrechung. `Spr` entsteht ausschließlich samstags aus Cafeteria B und dem überlappenden Restaurant-Spätdienst, verwendet den tatsächlichen Wechselzeitpunkt und beachtet eine gegebenenfalls erforderliche Laufoption.
- Geschützte Typ1-Deckungen werden vor der Kandidatenbildung minuten- und platzgenau verbraucht. Voll gedeckte Plätze erzeugen keine Kandidaten; ein geschützter `Spr` lässt nur die Zeit vor dem Wechsel offen. Ein geschütztes `B` besitzt keine Deckung und lässt den Bedarf vollständig verfügbar.
- Stabile Kandidaten- und CP-SAT-Variablennamen bestehen ausschließlich aus technischen Kennungen, Datum, Platzordnung und technischem Kandidatentyp. Eingabereihenfolge, Namen und sichtbare Typcodes beeinflussen weder Schlüssel noch Reihenfolge.
- Die CP-SAT-Struktur enthält höchstens eine Auswahl je Person und Tag sowie höchstens eine Deckung je atomarem Bedarfszeitraum. Solver-Gegentests beweisen, dass Doppelzuweisung und Überbesetzung unzulässig sind; normale Teildeckung kann bereits durch den Kandidatenwert nicht dargestellt werden.
- Deterministisch erzeugte Permutationsfälle, getrennte Negativfälle und ein unabhängiger erschöpfender Kleinstfallvergleich prüfen Kandidatenmenge, tatsächliche Zeiten, gleichartige Plätze, `D`, `Spr`, Typ1-Vorverbrauch und Strukturgrenzen.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 44 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Es wurden keine fachlichen Hard-Rule-Solverbedingungen aus AG-05, Datenbankmigrationen, WPF-Funktionen oder neuen Pakete ergänzt. Ausschließlich synthetische Testdaten werden verwendet.

### AG-05 – Alle automatischen Hard Rules als unverletzbare Solverbedingungen

Status: `[~]` – umgesetzt und geprüft; wartet auf Abnahme

Umfang:

- die elf Hard Rules in kleine Regelübersetzer oder eng zusammengehörige Familien überführen,
- wirksame Wochenziele nach `U` und `K`, normale und AH-Obergrenzen, Vorgeschichte, Urlaubsgrenze und `Spr`-Notfallbedingung korrekt einbeziehen,
- fehlende Vorgeschichte ausschließlich bei `MAX_CONSECUTIVE_WORKDAYS` als nicht vollständig prüfbar kennzeichnen,
- bei fehlender zulässiger Besetzung Bedarf offenlassen statt eine Hard Rule zu verletzen.

Prüfung:

- je Hard Rule erfüllt, verletzt, Grenzwert, nicht anwendbar und relevante Kombination,
- je Hard Rule ein Solverfall „Bedarf offen statt Regelbruch“,
- vollständige S07-Szenarien für Wochenmaximum, Urlaub, Typ1, `D` und `Spr`,
- kontrollierte Mutation von Tagesblockade, Wochenmaximum, Arbeitstagen und `Spr`-Notfallgrenze,
- Vergleich mit Domain-Nachprüfung und Kleinstfall-Referenzlöser.

Abnahmebedingung:

- Kein zulässiges Planning-Ergebnis verletzt eine Struktur- oder automatische Hard Rule; Mutationen werden sicher erkannt.

Umsetzungs- und Prüfstand vom 2026-09-17:

- Alle elf automatischen Hard Rules besitzen genau einen expliziten Übersetzer. Bereits durch Eingangsprüfung, Kandidatenbildung oder Strukturmodell garantierte Regeln bleiben konstruktiv erzwungen; Wochenobergrenzen, Arbeitsfolgen, Urlaubsgrenze und `Spr`-Notfallfreigabe ergänzen echte CP-SAT-Bedingungen.
- Das normale Wochenmaximum verwendet das nach `U` und `K` wirksame Wochen-Soll plus 180 Minuten. AH bleibt bei höchstens 720 Minuten. Tatsächliche Kandidaten-Arbeitsminuten werden gezählt; rote `X` verändern das Wochen-Soll nicht.
- Acht aufeinanderfolgende bekannte Arbeitstage sind unzulässig. Vorhandene sieben Tage Vorgeschichte werden einbezogen; eine fehlende Geschichtsangabe wird weder als Arbeit noch als frei erfunden und ausschließlich für `MAX_CONSECUTIVE_WORKDAYS` als nicht vollständig prüfbar ausgewiesen.
- Endet ein Urlaubsblock am Freitag, sperrt das Modell den unmittelbar folgenden Samstag und Sonntag. Unzulässige Kandidaten dürfen nicht gewählt werden; der zugehörige Bedarf bleibt offen.
- `Spr` ist im Hard-Rule-Modell standardmäßig gesperrt. Erst eine ausdrücklich typisierte, bedarfsbezogene Notfallfreigabe aus der regulären Nicht-`Spr`-Planung darf genau den bestätigten Restaurant-Spätbedarf öffnen. AG-06 erzeugt diese Freigabe aus der Bedarfsoptimierung; dadurch kann `Spr` nicht zur bloßen Stundenauffüllung verwendet werden.
- Eine OR-Tools-unabhängige fachliche Nachprüfung bewertet alle elf Regeln in Katalogreihenfolge. Kontrollierte Fälle ohne Wochenmaximum oder ohne `Spr`-Grenze werden als ungültig erkannt. Ein erschöpfender Kleinstfall-Referenzlöser und CP-SAT erreichen denselben zulässigen Deckungswert von 1.380 Minuten.
- Erfüllt-, Verletzt-, Grenzwert-, Nicht-anwendbar-, Vorgeschichts-, Urlaubs-, Typ1-, `D`-, `Spr`-, Tagesblockade- und Überbesetzungsfälle werden gemeinsam durch die fokussierten Validierungs-, Kandidaten-, Struktur- und Hard-Rule-Tests abgedeckt. Ausschließlich synthetische Daten werden verwendet.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 61 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Datenbank, Migrationen, WPF-Oberfläche, Rückübersetzung, schwarze `X` und weiche Optimierungsstufen wurden nicht geändert.

### AG-06 – Bedarfsoptimierung, Rückübersetzung und schwarze X

Status: `[~]` – umgesetzt und geprüft; wartet auf Abnahme

Umfang:

- zuerst ungedeckte Mitarbeiterzeit und danach vollständig ungedeckte Plätze minimieren,
- alle Einsatzorte und Diensttypen innerhalb dieser Stufe gleich behandeln,
- die frühe `Spr`-Lücke als tatsächlich ungedeckte Zeit erhalten,
- Solverentscheidungen in vorhandene `ScheduleAssignment`-, `DemandCoverage`- und `GeneratedDayOffMarker`-Werte zurückübersetzen,
- für jeden sonst leeren aktiven Personentag genau ein schwarzes `X` erzeugen,
- das vollständige Ergebnis unabhängig fachlich nachprüfen und bei Abweichung als technisch ungültig verwerfen.

Prüfung:

- sieben offene Stunden gegen zwei mal drei offene Stunden,
- gleiche offene Minuten mit unterschiedlicher Zahl vollständig offener Plätze,
- abweichende tatsächliche Bedarfszeiten, gleiche Plätze, `D`, `Spr`, `B` und keine Überbesetzung,
- schwarze `X` bei Arbeit, leeren Tagen, `U`, `K`, rotem `X` und vollständig abwesender Woche,
- deterministische Kennungen und wertgleiche Rückübersetzung,
- Mutation von Überbesetzung und verborgener `Spr`-Lücke.

Abnahmebedingung:

- Deckungsziel, offene Zeiträume, Zuweisungen und schwarze `X` entsprechen exakt dem bestätigten Modell und bestehen die unabhängige Nachprüfung.

Umsetzungs- und Prüfstand vom 2026-09-17:

- Die Bedarfsoptimierung minimiert lexikografisch zuerst ungedeckte Mitarbeiterzeit und danach vollständig ungedeckte Plätze. Einsatzort, Diensttyp, sichtbarer Name und Farbe erhalten in dieser Stufe kein Gewicht.
- Eine reguläre Optimierung ohne `Spr` fixiert zuerst ihre bestmöglichen offenen Minuten und Plätze. Erst danach darf `Spr` zusätzliche Restaurant-Unterdeckung reduzieren; die erreichte reguläre Deckung darf dabei nicht verdrängt werden.
- Normale Kandidaten werden vollständig gedeckt oder bleiben vollständig offen. Beim bestätigten `Spr` bleibt der Zeitraum vor dem tatsächlichen Wechsel als eigenes teilweise offenes Intervall erhalten und fließt minutengetreu in den Zielvektor ein.
- Gewählte normale, `D`- und `Spr`-Kandidaten werden wertgleich in vorhandene `ScheduleAssignmentSnapshot`-, Segment- und Deckungswerte mit deterministischen Kennungen zurückübersetzt. Die Übergabeform entspricht den bestehenden `ScheduleAssignment`-/`DemandCoverage`-Fachwerten; die atomare Übernahme in den Entwurf bleibt AG-12.
- Für jeden ansonsten leeren Tag jeder aktiven Person entsteht genau ein Vorschlagswert für ein schwarzes `X`. Arbeit, geschützte Typ1-Arbeit, `U`, `K` und rote `X` erhalten kein zusätzliches schwarzes `X`; eine vollständig abwesende Woche bleibt ohne schwarze `X`.
- Die unabhängige Ergebnisprüfung vergleicht Kennungen, Zuweisungsart, tatsächliche Segmente und Deckungen, offene Intervalle, schwarze `X`, Zielwerte und alle Hard-Rule-Bewertungen. Veränderte Arbeitsminuten, fehlende schwarze `X` und eine verborgene frühe `Spr`-Lücke werden verworfen.
- Ein OR-Tools-unabhängiger erschöpfender Kleinstfalllöser bestätigt die Reihenfolge aus offenen Minuten und vollständig offenen Plätzen. Geprüft sind insbesondere sieben offene Stunden gegen zweimal drei, gleicher Minutenwert mit unterschiedlicher Platzanzahl, abweichende tatsächliche Zeiten, `D`, `Spr`, vollständig offener Bedarf und reguläre Besetzung vor `Spr`.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 78 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Datenbank, Migrationen, WPF-Oberfläche, Übernahme sowie hohe, mittlere und Stabilitätsziele wurden nicht geändert.

### AG-07 – Hohe weiche Regeln

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- alle fünf hohen Regeln als eigene nachvollziehbare Optimierungsbeiträge umsetzen,
- zuerst Fälle und nur innerhalb derselben Regel das Ausmaß berücksichtigen,
- die erreichten Zielwerte als feste Grenze für nachfolgende Stufen bewahren,
- rote und schwarze `X` als reguläre freie Tage behandeln; `U` und `K` nur als Unterbrechung von Arbeitsfolgen verwenden.

Prüfung:

- pro Regel ein Paar ansonsten vergleichbarer zulässiger Pläne,
- Kombinationen aus Wochenminimum, zusammenhängenden freien Tagen, rotem `X`, Urlaub und `D`,
- jede Grenze gegen viele mittlere Verbesserungen absichern,
- Referenzlöservergleich für kleine kombinierte Fälle,
- kontrollierte Mutation der hohen Dominanzgrenze.

Abnahmebedingung:

- Jede hohe Regel beeinflusst nachweislich die Lösung, ohne Hard Rules oder Bedarfsdeckung zu überstimmen.

Umgesetzt und geprüft am 2026-09-17:

- Alle fünf hohen Regeln besitzen eigene CP-SAT-Fälle mit stabilen fachlichen Fallschlüsseln. Zuerst wird die Gesamtzahl verletzter Fälle minimiert und als Gleichheitsgrenze fixiert.
- Das Ausmaß wird pro Regelart getrennt geführt. Die anschließende Dominanzsuche akzeptiert ausschließlich Lösungen, die bei keiner hohen Regel schlechter und bei mindestens einer besser sind; Minuten, freie Tage und zusätzliche `D` erhalten weder gemeinsame Punkte noch eine versteckte Reihenfolge.
- `NORMAL_WEEKLY_MINIMUM` verwendet das wirksame Wochen-Soll abzüglich 180 Minuten. `WEEKLY_CONSECUTIVE_DAYS_OFF` zählt rote und erzeugte schwarze `X`, aber weder `U` noch `K`. `RED_X_ADJACENT_DAY_OFF`, `PRE_VACATION_WEEKEND_FREE` und `SPLIT_SHIFT_WEEKLY_MAXIMUM` verwenden die bestätigten Grenzen und tatsächlichen Arbeitstage beziehungsweise `D`-Kandidaten.
- Ein Urlaubsstart am ersten Planmontag mit unbekanntem vorherigem Wochenende wird nicht erfunden, sondern in der unabhängigen Auswertung als nicht vollständig prüfbar ausgewiesen.
- Eine OR-Tools-unabhängige Nachprüfung berechnet Fallzahl und Ausmaß erneut. Der Optimierer verwirft Ergebnisse, wenn diese Werte vom CP-SAT-Modell abweichen. Vorschlagsziel und Regelstatus enthalten die fünf hohen Regeln; eine kontrolliert entfernte hohe Zielgrenze wird von der unabhängigen Vorschlagsprüfung erkannt.
- Pro hoher Regel belegt ein fokussierter Auswahltest die Wirkung ohne Verschlechterung von Hard Rules oder Bedarfsdeckung. Ergänzend bestehen Gleichrangigkeits-, `U/K`-, Vorgeschichts- und erschöpfende Kleinstfalltests.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 87 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Datenbank, Migrationen, WPF-Oberfläche, Übernahme, mittlere Ziele, AH-Phase und Stabilitätsziele wurden nicht geändert.

### AG-08 – Mittlere Ziele, faire Verteilung und technischer Gleichstand

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- `THREE_WEEK_FREE_WEEKEND` und `MINIMIZE_SPLIT_SHIFTS` in der Nicht-AH-Phase umsetzen,
- die leere niedrige Stufe ausdrücklich und ohne Scheinregel durchlaufen,
- Früh-, Spät-, Wochenend-, `D`- und zulässige `Spr`-Belastung nur unter vergleichbar geeigneten Personen ausgleichen,
- `Spr` und `D` ausschließlich über ihre bestehenden bestätigten Regeln besonders vermeiden,
- den letzten rein technischen Gleichstand stabil auflösen.

Prüfung:

- mittlere Regeln gegen viele Stabilitätsverbesserungen,
- gleiche Eignung sowie bewusst unterschiedliche Eignung und Sperrtage,
- faire Früh-/Spät-, Wochenend-, `D`- und `Spr`-Verteilung im aktuellen Zeitraum,
- keine Bevorzugung über Namen, Typcodes oder UI-Reihenfolge,
- wiederholte Lösungen in geänderter Eingabereihenfolge mit wertgleichem Ergebnis.

Abnahmebedingung:

- Mittlere Regeln bleiben dominant; Stabilität ist fair, lokal auf drei Wochen begrenzt und frei von versteckten Fachgewichten.

Umgesetzt und geprüft am 2026-09-17:

- `THREE_WEEK_FREE_WEEKEND` bewertet je Person genau einen Drei-Wochen-Fall; rote und schwarze `X` bilden ein freies Wochenende, `U` und `K` nicht. `MINIMIZE_SPLIT_SHIFTS` führt die Gesamtzahl gewählter `D` als eigenes mittleres Regelausmaß.
- Die mittlere Fallzahl wird erst unter nicht dominierten hohen Lösungen minimiert. Vor der Stabilitätsstufe bleiben gegenläufige Ausmaße verschiedener hoher und mittlerer Regelarten gleichwertig; gefundene dominierte Bereiche werden ohne Punkte und ohne feste Regelreihenfolge ausgeschlossen.
- Die niedrige Stufe der Katalogversion 1 bleibt ausdrücklich leer. Im Zielvektor entstehen keine erfundenen niedrigen Fälle.
- Die faire Verteilung betrachtet Früh-, Spät-, Wochenend-, `D`- und zulässige `Spr`-Zuweisungen getrennt. Verglichen werden ausschließlich Personengruppen mit derselben Zahl zulässiger Kandidatentage für das jeweilige Thema; Personen ohne Freigabe oder mit abweichenden Sperrtagen werden nicht künstlich gleichgesetzt. Gemessen wird je Gruppe nur die Differenz zwischen höchster und niedrigster Anzahl im aktuellen Drei-Wochen-Zeitraum.
- AH-Kandidaten sind in dieser Nicht-AH-Phase ausdrücklich auf null gesetzt. Die eigentliche nachgelagerte AH-Optimierung bleibt AG-09.
- Der technische Gleichstand verwendet die bereits stabil sortierten technischen Kandidatenschlüssel, einen einzelnen Solver-Worker, festen Seed und eine feste Suchstrategie. Namen, Typcodes, UI- und Eingabereihenfolge gehen nicht in die Entscheidung ein.
- OR-Tools-unabhängige Nachprüfungen berechnen mittlere Verletzungen und Fairness erneut. Geprüft sind freies Wochenende, `D`-Minimierung, Früh-, Spät-, Wochenend-, `D`- und `Spr`-Verteilung, unterschiedliche Kandidatenmöglichkeiten, AH-Ausschluss, Namens- und Typcode-Unabhängigkeit sowie umgeordnete Eingaben.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 332 Domain-, 241 Application-, 98 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt meldet erwartungsgemäß Exitcode 8.
- Datenbank, Migrationen, WPF-Oberfläche, Übernahme, AH-Ziel und AH-Hinweise wurden nicht geändert.

### AG-09 – Nachgelagerte AH-Phase und Hinweise

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- das vollständige Nicht-AH-Ergebnis vor der AH-Phase einfrieren,
- AH ausschließlich auf verbleibende zulässige Plätze anwenden,
- zwölf Stunden als unverletzbare Wochenobergrenze und zehn Stunden als mittleres Ziel anwenden,
- Abweichungen mehrerer AH-Personen möglichst klein und anschließend möglichst gleichmäßig verteilen,
- strukturierte niedrige und hohe AH-Hinweise erzeugen.

Prüfung:

- AH verdrängt keine Nicht-AH-Zuweisung,
- unter sechs, genau sechs, genau zehn, über zehn und genau zwölf Stunden,
- mehr als zwölf Stunden bleibt automatisch unmöglich,
- mehrere AH-Personen bei genügendem und knappem Restbedarf,
- Mutation der Phasenreihenfolge und der AH-Obergrenze,
- gemeinsamer Zielvektor und Referenzlöservergleich kleiner Fälle.

Abnahmebedingung:

- Die AH-Phase ist fachlich und technisch nachgelagert, fair und vollständig strukturiert ausgewertet.

Umgesetzt und geprüft am 2026-09-17:

- Nach der vollständig optimierten Nicht-AH-Phase werden alle Nicht-AH-Kandidatenentscheidungen wertgleich eingefroren. Die zweite Modellphase kann ausschließlich noch AH-Kandidaten auf nicht belegten zulässigen Restplätzen wählen; eine unabhängige Nachprüfung verwirft jede veränderte Nicht-AH-Auswahl.
- Auch innerhalb der AH-Phase wird reguläre Deckung vor einem zulässigen `Spr` fixiert. Restdeckung bleibt wichtiger als das Zehn-Stunden-Ziel; alle Struktur- und Hard Rules werden erneut angewendet und unabhängig nachgeprüft.
- `AH_WEEKLY_MAXIMUM` begrenzt jede AH-Person unverletzbar auf 720 Minuten je Montag-bis-Sonntag-Woche. `AH_WEEKLY_TARGET` minimiert danach die gesamte absolute Abweichung von 600 Minuten. Unter vergleichbar geeigneten AH-Personen wird anschließend die verbleibende Abweichung ohne Namens-, Typcode- oder Eingabereihenfolge-Einfluss möglichst gleichmäßig verteilt.
- `AH_WEEKLY_LOW_NOTICE` meldet ausschließlich Werte unter 360 Minuten, `AH_WEEKLY_HIGH_NOTICE` ausschließlich Werte über 600 bis einschließlich 720 Minuten. Beide Ergebnisse enthalten Regelkennung, technische Personenkennung, Wochenmontag und Minutenwert; sie blockieren die Generierung nicht und enthalten keinen deutschen UI-Satz.
- Ziel-, Fairness- und Hinweiswerte werden nach der Solverentscheidung OR-Tools-unabhängig erneut berechnet und in Zielvektor und Regelbewertungen übernommen. Kontrollierte Gegenprüfungen erkennen sowohl eine veränderte Nicht-AH-Auswahl als auch eine entfernte AH-Wochenobergrenze.
- Geprüft sind 300, 360, 600, 660, 720 und mehr als 720 Minuten, genügend und knapper Restbedarf bei mehreren AH-Personen, unterschiedliche Eignung, Namens- und Eingabereihenfolge-Unabhängigkeit, strukturierte Vorschlagshinweise und ein unabhängiger erschöpfender Kleinstfallvergleich.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 335 Domain-, 241 Application-, 113 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt liefert im Gesamtlauf den erwarteten Exitcode 8.
- Datenbank, Migrationen, WPF-Oberfläche, Übernahme, Laufstatus, Zeitgrenze und Abbruchbehandlung wurden nicht geändert.

### AG-10 – Solverorchestrierung, Status, Abbruch und Wiederholbarkeit

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- benannte lexikografische Läufe mit nach jeder Stufe fixiertem Zielwert koordinieren,
- deterministische Solver-Einstellungen, Zeitgrenze und relevante Version erfassen,
- alle bestätigten Laufstatus sauber unterscheiden,
- Abbruch kontrolliert bis an die Solvergrenze weiterreichen,
- technische Ausnahmen an der äußeren Planning-Grenze datensparsam in stabile Fehlerwerte übersetzen,
- keine zweite parallele Planung zulassen.

Prüfung:

- optimal vor Zeitgrenze, zulässig aber nicht nachweislich optimal, Zeitablauf ohne zulässiges Ergebnis,
- Abbruch vor Start und während der Lösung,
- unerwarteter Solverstatus, Mappingfehler und technische Ausnahme,
- unveränderter Entwurf bei jedem nicht übernommenen Ergebnis,
- Wiederholung im selben Prozess, mit neuem Adapter und in getrennten Testläufen,
- keine Namen oder vollständigen Planinhalte in protokolliertem Fehlerkontext.

Abnahmebedingung:

- Status und Abbruch sind wahrheitsgemäß, Ergebnisse reproduzierbar und technische Fehler für die Fehlersuche hilfreich, ohne Datenschutzgrenzen zu verletzen.

Umgesetzt und geprüft am 2026-09-17:

- Die Nicht-AH- und AH-Phase werden als benannte lexikografische Stufen ausgeführt. Jeder bestätigte Zielwert wird vor der Folgestufe im Modell fixiert; die Metadaten halten Ergebnisstatus, Stufenfolge, einen Worker, Startwert `0`, den technischen Gleichstand, Solvername und geladene OR-Tools-Version fest.
- Eine zentrale Laufzeitgrenze gilt über alle CP-SAT-Aufrufe eines Optimierungslaufs. Jeder Solver erhält nur die verbleibende Zeit. Ein zulässiger Zwischenstand wird unabhängig nachgeprüft und als `FeasibleNotProvenOptimal` zurückgegeben; ohne zulässigen Stand entsteht `TimedOutWithoutFeasibleResult` ohne Vorschau.
- Der Abbruch wird bis `CpSolver.StopSearch()` weitergereicht. Ein Abbruch vor dem Start oder nach einem intern gefundenen Zwischenstand liefert ausschließlich `Cancelled`; ein Zwischenstand wird nach ausdrücklichem Abbruch nicht angeboten.
- Eine prozessweite nicht wartende Einzellauf-Sperre verhindert einen zweiten parallelen Rechenlauf. Der abgelehnte Start erhält den stabilen Fehlercode `ConcurrentRun` und startet keinen zweiten Solver.
- Unerwartete Solverstatus, Rückübersetzungsfehler und sonstige technische Ausnahmen werden an der Planning-Grenze in stabile Fehlercodes mit Korrelationskennung und Ausnahmetyp übersetzt. Ausnahmetexte, Namen und vollständige Planinhalte werden weder im Ergebnis noch in einem neu eingeführten Log gespeichert; ein automatischer Wiederholungsversuch findet nicht statt.
- Reproduzierbarkeit ist im selben Prozess, mit neuer Engine und durch einen fest erwarteten SHA-256-Fingerabdruck des vollständigen synthetischen Vorschlags geprüft. Zuweisungskennungen, schwarze `X`, offene Bedarfe, Status und Zielwerte bleiben wertgleich.
- Gezielte Tests decken optimales Ergebnis, zulässigen nicht nachweislich optimalen Vorschlag, echten Zeitablauf ohne zulässigen Stand, Abbruch während und nach der Lösung, parallelen Start, unerwarteten Solverstatus, Mappingfehler, äußere technische Ausnahme, Datenschutz und unveränderte Eingabemomentaufnahme ab.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 335 Domain-, 241 Application-, 126 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt liefert im Gesamtlauf den erwarteten Exitcode 8.
- Datenbank, Migrationen, Application-Generierungsablauf, WPF-Oberfläche und Übernahme wurden nicht geändert; sie beginnen frühestens mit AG-11.

### AG-11 – Application-Ablauf für Generierung und flüchtige Vorschau

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- `GenerateAutomaticScheduleCommand` mit aktueller Vorbereitung, Typ1-Schutz und globaler Laufkoordination umsetzen,
- Planning asynchron außerhalb des WPF-UI-Threads aufrufen,
- zulässige Ergebnisse als flüchtige Vorschau halten,
- Verwerfen, Abbruch und Fehler ohne Speichervorgang behandeln,
- kurze deutsche System-09-Meldungen und nächste Bedienungsschritte liefern,
- System-10-Diagnostik nicht vorwegnehmen.

Prüfung:

- fehlende, aktuelle und veraltete Vorbereitung,
- paralleler Start, Abbruch und erneuter Start,
- jeder Laufstatus und korrekte Vorschauwerte,
- Verwerfen und App-Neustart ohne gespeicherten Vorschlag,
- Testdoubles beweisen genau einen Planning-Aufruf und keinen Store-Aufruf vor bewusster Übernahme,
- Application-, Planning- und Architekturtests.

Abnahmebedingung:

- Ein Lauf kann den aktuellen Entwurf weder direkt noch indirekt verändern; nur zulässige Ergebnisse werden als Vorschau angeboten.

Umgesetzt und geprüft am 2026-09-17:

- `GenerateAutomaticScheduleCommand` prüft Entwurfskennung, erwartete Version, Zeitraum und erwartete Snapshot-Kennung. Er liest Entwurf und gespeicherte Vorbereitung erneut und startet Planning nur bei einem eindeutig passenden aktuellen Stand.
- Aus den aktuellen Lesedaten wird mit denselben Application-Bausteinen wie bei der bewussten Vorbereitung ein neuer Vergleichsstand gebildet. Damit werden die Typ1-Wochenvoraussetzung, geschützte Typ1-Zuweisungen, Mitarbeitende, Typen, Berechtigungen, Katalog, Bedarfe, Abwesenheiten, Regeln, Laufoptionen und Vorgeschichte unmittelbar vor Planning erneut geprüft.
- Die produktive Zeitgrenze von 120 Sekunden ist zentral im Planning-Request festgelegt und wird vom Application-Ablauf ohne sichtbare oder benutzerseitige Änderungsmöglichkeit verwendet. Der konkrete Planning-Adapter führt die Solverarbeit bereits außerhalb des WPF-UI-Threads aus.
- Eine prozessweite, nicht wartende Application-Sperre verhindert einen zweiten parallelen Start bereits vor einem weiteren Planning-Aufruf. Nach Abbruch oder Abschluss kann ein neuer Lauf bewusst gestartet werden.
- Nur `Optimal` und `FeasibleNotProvenOptimal` werden als flüchtige `AutomaticSchedulePreview` gehalten. Ein neuer Lauf, Abbruch, Zeitablauf, Blockade, nicht unterstützte Regel oder technischer Fehler entfernt eine frühere Vorschau. `DiscardPreview` und eine neue Command-Instanz verwerfen sie ohne Speicherung.
- Jeder bestätigte Planning-Status wird in einen eindeutigen Application-Status und einen kurzen deutschen Text mit nächstem Bedienungsschritt übersetzt. Technische Fehler übernehmen ausschließlich datensparsame Fehlerwerte und eine vorhandene Korrelationskennung; eine unerwartete Ausnahme wird genau einmal ohne automatischen Wiederholungsversuch übersetzt.
- Der Command besitzt ausschließlich Lese- und Planning-Abhängigkeiten. Er kennt keinen Schreib-Port. Testdoubles belegen genau einen Planning-Aufruf bei Erfolg sowie unveränderte Speicheraufrufzahl bei Erfolg, Verwerfen, Abbruch und allen Fehlerpfaden.
- 18 neue Application-Testfälle decken beide zulässigen Ergebnisstatus, alle fünf Planning-Fehlerstatus, fehlende und veraltete Vorbereitung, Snapshot- und Versionskonflikt, veränderte geschützte Typ1-Zuweisung, Abbruch vor und während Planning, parallelen Start, erneuten Start, Verwerfen, neue Command-Instanz, technischen Fehler, Datenschutz und widersprüchliche Vorschlagskennungen ab.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 335 Domain-, 259 Application-, 126 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt liefert im Gesamtlauf den erwarteten Exitcode 8.
- Datenbank, Migrationen, atomare Übernahme, WPF-ViewModel und sichtbare Oberfläche wurden nicht geändert. AG-12 beginnt erst nach Abnahme dieses reinen Application-Schritts.

### AG-12 – Fachlich atomare Übernahme und Versionsschutz

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- eine Domain-Operation zum vollständigen Austausch ersetzbarer automatischer Zuweisungen und schwarzer `X` einführen,
- geschützte Typ1-Zuweisungen und spätere wirksame Sperren bewahren,
- `AcceptAutomaticScheduleProposalCommand` mit erneuter Prüfung von Entwurf, Version und Snapshot umsetzen,
- doppelte Übernahme, veraltete Vorschau und automatische Zusammenführung ablehnen,
- erfolgreiche Übernahme erhöht die Entwurfsversion genau einmal.

Prüfung:

- Domain-Invarianten für vollständigen Austausch und Schutzwerte,
- erfolgreiche, doppelte und veraltete Übernahme,
- abweichende Entwurfskennung, Version und Snapshot-Kennung jeweils getrennt,
- neue Zuweisungen, schwarze `X`, Metadaten und Versionswert als ein unteilbarer Änderungsauftrag,
- kein Application-Pfad lädt technische Zusatzdaten aus Planning oder Infrastructure nach.

Abnahmebedingung:

- Die fachliche Änderung ist vollständig beschrieben und kein Versionskonflikt kann teilweise übernommen oder automatisch zusammengeführt werden.

Umgesetzt und geprüft am 2026-09-17:

- `ScheduleDraft.ReplaceAutomaticGeneration` bildet den Austausch als eine unveränderliche Domain-Operation ab. Sie entfernt ausschließlich ersetzbare automatische Zuweisungen, ersetzt den vollständigen Bestand schwarzer `X` und erzeugt erst nach erfolgreicher Gesamtvalidierung einen neuen Entwurf mit genau einer höheren Version.
- Geschützte Typ1-Zuweisungen, nicht automatische Zuweisungen und durch `AssignmentLock` gesperrte automatische Zuweisungen bleiben erhalten. Ungültige Herkunft, Konflikte mit Schutzwerten und eine nicht mehr erhöhbare Version liefern strukturierte Domain-Fehler; der Ausgangsentwurf bleibt wertgleich unverändert.
- `AcceptAutomaticScheduleProposalCommand` liest den aktuellen Entwurf und die gespeicherte Vorbereitung erneut. Entwurfskennung, erwartete Version und Snapshot-Kennung werden getrennt geprüft; ein abweichender oder bereits übernommener Vorschlag wird ohne Zusammenführung und ohne Schreibauftrag abgelehnt.
- Vorschlagszuweisungen werden ausschließlich aus den vollständigen Vorschlagswerten, dem aktuellen Domain-Entwurf und dem vorhandenen fachlichen Dienstkatalog rekonstruiert. Normale, `D`- und `Spr`-Zuweisungen müssen danach segment-, deckungs-, zeit- und kennungsgenau dem Vorschlag entsprechen. Planning oder Infrastructure werden nicht für technische Zusatzdaten aufgerufen.
- Der neue Schreib-Port erhält genau einen `AcceptAutomaticScheduleProposalChange`. Darin sind der vollständig validierte Ersatzentwurf einschließlich neuer Zuweisungen, schwarzer `X` und genau einmal erhöhter Version sowie erwartete Ausgangsversion, Snapshot-Kennung und Laufmetadaten gemeinsam enthalten. Ein Store-Konflikt liefert keinen Teilstand und löst keinen automatischen Wiederholungsversuch aus.
- Fünf neue Domain-Testfälle prüfen vollständigen Austausch, Schutzwerte, Sperren, unveränderten Fehlerfall und Versionsgrenze. Elf neue Application-Testfälle prüfen Erfolg, doppelte Übernahme, Entwurfs-, Versions- und Snapshot-Konflikt, atomaren Store-Konflikt, manipulierte Werte, Schutzkonflikt, Abbruch sowie normale, `D`- und `Spr`-Zuweisungen.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 340 Domain-, 270 Application-, 126 Planning-, 65 Infrastructure-, 89 Desktop- und 18 Architekturtests. Das weiterhin leere Excel-Testprojekt liefert den erwarteten Exitcode 8.
- Datenbank, Migrationen, produktive Persistenzverdrahtung, WPF-ViewModel und sichtbare Oberfläche wurden nicht geändert. Die echte SQLite-Transaktion und der persistente Laufmetadatensatz folgen erst in AG-13.

### AG-13 – SQLite-Transaktion, aktuelle Laufmetadaten und Migration

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Umfang:

- den atomaren Übernahme-Port im bestehenden `SqliteScheduleStore` implementieren,
- ersetzbare automatische Zuweisungen, Deckungen und schwarze `X` austauschen,
- genau einen aktuellen Laufmetadatensatz speichern und einen vorherigen ersetzen,
- Entwurf, erwartete Version und Snapshot-Kennung innerhalb derselben Transaktion prüfen,
- eine neue generierte Migration in der bestehenden gemeinsamen Folge erstellen,
- bei jedem Schreib- oder Prüfungsfehler vollständig zurückrollen.

Prüfung:

- echte temporäre SQLite-Dateien für Erfolg, Fehler in der Transaktionsmitte, Versionskonflikt, Snapshot-Konflikt und doppelte Übernahme,
- Neustart und vollständiger Round-trip aller Metadaten,
- alter Entwurf bleibt nach jedem Fehler wertgleich erhalten,
- Migration von leerer Datenbank und Upgrade vom System-08-Stand mit synthetischen Bestandsdaten,
- genau ein `DbContext`, ein Migrationsverzeichnis und keine Planning-/OR-Tools-Typen in Infrastructure-Verträgen.

Abnahmebedingung:

- Übernahme und Migration sind atomar, verlustfrei und durch reale SQLite-Prüfungen belegt.

Umgesetzt und geprüft am 2026-09-17:

- `SqliteScheduleStore` implementiert den schmalen Übernahme-Port mit genau einem `ServiceCatalogDbContext` und einer expliziten SQLite-Transaktion. Entwurfskennung, erwartete Ausgangsversion und vorbereitete Snapshot-Kennung werden vor jeder Mutation innerhalb dieser Transaktion erneut geprüft.
- Bei Erfolg werden ausschließlich nicht gesperrte automatische Zuweisungen samt Segmenten und Deckungen sowie der vollständige Bestand schwarzer `X` ersetzt. Typ1-, manuelle und gesperrte automatische Zuweisungen bleiben erhalten; die Entwurfsversion wird genau einmal auf den bereits fachlich validierten Folgewert gesetzt.
- Genau ein aktueller Laufdatensatz je Entwurf speichert Snapshot-Kennung, Solvername und -version, deterministische Einstellungen, Zeitgrenze, gemessene Laufzeiten, Ergebnisstatus und den vollständigen lexikografischen Zielvektor. Eine spätere Übernahme ersetzt diesen Datensatz; eine Vorschlags- oder Planversionshistorie wurde nicht eingeführt.
- Die generierte Migration `20260917125952_AddAutomaticScheduleRuns` erweitert die einzige gemeinsame Migrationsfolge. Leere Datenbanken und eine synthetische Datenbank auf dem System-08-Schemastand werden verlustfrei bis zum aktuellen Modell migriert.
- Reale temporäre SQLite-Dateien belegen Erfolg und vollständigen Metadaten-Round-trip nach Neustart, den Schutz gesperrter automatischer Zuweisungen, das Ersetzen eines vorherigen Laufs, getrennte Versions- und Snapshot-Konflikte, die abgelehnte doppelte Übernahme sowie vollständiges Rollback bei einem künstlich ausgelösten Fehler in der Transaktionsmitte. In allen Fehlerfällen bleibt der alte Entwurf erhalten.
- Die Application-Verträge enthalten ausschließlich fachliche und primitive unveränderliche Werte. Infrastructure übernimmt keine Planning- oder OR-Tools-Typen; Architekturtests sichern weiterhin genau einen gemeinsamen `DbContext` und ein Migrationsverzeichnis ab.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 340 Domain-, 272 Application-, 126 Planning-, 73 Infrastructure-, 89 Desktop- und 18 Architekturtests, insgesamt 918 Tests. Das weiterhin leere Excel-Testprojekt liefert den erwarteten Exitcode 8.
- WPF-ViewModel und sichtbare Oberfläche wurden nicht geändert. Die sichtbare Bedienung und ihr manuelles Gate folgen in AG-14.

### AG-14 – Minimaler WPF-Generierungsablauf

Status: `[~]` – umgesetzt und automatisch geprüft; sichtbare Abnahme offen

Umfang:

- Startaktion nur bei aktuellem Snapshot und erfüllten Voraussetzungen anbieten,
- deaktivierte Aktion mit verständlichem Grund erklären,
- laufenden Zustand, verstrichene Zeit, maximale Laufzeit und „Abbrechen“ ohne erfundene Prozentzahl zeigen,
- Status, Laufzeit, offene Plätze und Minuten, erzeugte Zuweisungen sowie Hinweis auf System 10 als Vorschau anzeigen,
- „Plan übernehmen“ und „Vorschlag verwerfen“ anbieten,
- übernommene Dienstkennzeichnungen und schwarze `X` in der bestehenden 21-Tage-Ansicht lesbar darstellen,
- Versionskonflikt und technische Fehler verständlich mit Code anzeigen.

Prüfung:

- ViewModel-Tests für alle Start-, Lauf-, Abbruch-, Vorschau-, Verwerfungs-, Übernahme- und Fehlerzustände,
- WPF-Konstruktions- und Renderprüfungen für Bindings, Ressourcen, Tastaturbedienung und nicht nur farblich vermittelte Zustände,
- gezielte Reaktionsfähigkeitsprüfung ohne `.Wait()`, `.Result` oder unkontrolliertes `Task.Run` im ViewModel,
- Architekturtests für konkrete Adapter ausschließlich in `Desktop.Composition`,
- manueller sichtbarer Test mit ausschließlich synthetischen Daten.

Abnahmebedingung:

- Die Service-Leitung bestätigt Start, Arbeitszustand, Abbruch, beide zulässigen Ergebnisstatus, Verwerfen, Übernehmen, schwarze `X`, offene Bedarfe, Versionskonflikt und technischen Fehler sichtbar. Nach dem späteren Änderungswunsch wird dieses weiterhin offene Gate mit dem neuen Algorithmus in AG-14F durchgeführt und stoppt bis dahin vor AG-15.

Umgesetzt und automatisch geprüft am 2026-09-17:

- Der Generierungsablauf ist in kleine getrennte Bausteine für Aktionen, Kontext, Darstellung, Vorschau, ViewModel und View gegliedert. Das vorhandene Dienstplan-ViewModel koordiniert diese Bausteine, ohne Planning-, Infrastructure- oder OR-Tools-Typen zu übernehmen.
- „Plan erzeugen“ ist nur bei einem aktuellen vorbereiteten Snapshot, erfüllter Typ1-Wochenvoraussetzung und unveränderten Laufoptionen aktiv. Ein verständlicher Text nennt den jeweiligen Sperrgrund. Während Lauf, Übernahme oder bewusster Vorschauentscheidung bleiben widersprüchliche Zeitraum-, Typ1-, AH- und Entwurfsaktionen gesperrt.
- Der laufende Zustand zeigt verstrichene und maximale Laufzeit mit unbestimmtem Fortschritt ohne erfundene Prozentzahl. „Abbrechen“ reicht das Abbruchsignal bis zum bestehenden Application- und Planning-Ablauf weiter; die Oberfläche blockiert den UI-Thread nicht.
- Optimale und zulässige nicht nachweislich optimale Vorschläge zeigen Laufzeit, vollständig und teilweise offene Bedarfe, offene Minuten, Anzahl der Einteilungen und schwarzen `X` sowie den Hinweis auf System 10. Übernehmen und Verwerfen sind bewusste getrennte Aktionen.
- Die 21-Tage-Ansicht zeigt automatisch erzeugte normale Dienste entsprechend ihrer Diensttypdarstellung, außerdem `D`, `Spr`, `B` und schwarze `X`. Rote feste `X` und schwarze erzeugte `X` bleiben farblich und durch ausgeschriebene Bedeutung unterscheidbar. Allgemeine manuelle Bearbeitung automatisch erzeugter Werte bleibt System 11.
- Versionskonflikte und technische Fehler werden mit verständlichem Text und stabilem Code angezeigt. Der konkrete Planning-Adapter und der SQLite-Schreibadapter werden ausschließlich in `Desktop.Composition` verdrahtet.
- Sechs neue ViewModel-Tests, drei echte WPF-Konstruktions- und Renderprüfungen, zwei Anzeigeprüfungen und erweiterte Architekturprüfungen decken Startgründe, Arbeitszustand, Abbruch, beide Vorschlagsarten, Verwerfen, Übernehmen, Neuladen, schwarze `X`, Konflikte, technische Fehler, Tastaturbedienung und UI-Thread-Schutz ab.
- Solution-Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 340 Domain-, 273 Application-, 126 Planning-, 73 Infrastructure-, 100 Desktop- und 19 Architekturtests, insgesamt 931 Tests. Das weiterhin leere Excel-Testprojekt liefert den erwarteten Exitcode 8.
- Das verpflichtende sichtbare WPF-Gate ist noch offen und wird nach dem Änderungsblock gemeinsam in AG-14F durchgeführt. Die bisherige automatische Prüfung bleibt historischer Nachweis des Bedienablaufs.

### AG-14A – Übernommenen automatischen Plan vollständig verwerfen und neu beginnen

Status: `[~]` – Planungsentwurf abgenommen, umgesetzt und automatisch geprüft; sichtbare Abnahme offen

Ziel und Abgrenzung:

- Nach der bewussten Übernahme eines automatisch erzeugten Vorschlags soll die Service-Leitung den aktuellen automatischen Plan mit einer eigenen, deutlich bestätigten Aktion vollständig verwerfen können.
- Die bereits vorhandene Aktion „Vorschlag verwerfen“ bleibt ausschließlich für die noch nicht gespeicherte Vorschau zuständig. AG-14A betrifft dagegen einen bereits übernommenen und in der lokalen Datenbank gespeicherten automatischen Plan.
- Der Zeitraum, seine Bedarfe und die fachlichen Eingaben werden nicht durch eine technisch unnötige Löschung des gesamten Entwurfs gefährdet. Die zu erhaltenden nicht automatisch erzeugten Planwerte sind in den bestätigten Entscheidungen dieses Schritts ausdrücklich festgelegt.
- Das Verwerfen erzeugt keinen neuen Plan und startet keine automatische Generierung. Nach dem erfolgreichen Zurücksetzen beginnt die Service-Leitung bewusst wieder mit Vorbereitung und Generierung.
- Abgenommene unveränderliche Planversionen aus System 12 dürfen später niemals durch diese Funktion verändert oder gelöscht werden. AG-14A betrifft ausschließlich den aktuellen noch nicht abgenommenen Arbeitsentwurf.

Geprüfte Umsetzungsoptionen:

| Option | Umsetzung | Vorteile | Nachteile und Risiken | Bewertung |
|---|---|---|---|---|
| A – gezieltes Zurücksetzen des automatischen Ergebnisses | Im vorhandenen Entwurf ausschließlich die festgelegten automatisch erzeugten Zuweisungen, schwarzen `X`, zugehörigen Sperren und aktuellen Laufmetadaten atomar entfernen; Entwurfskennung behalten und Version erhöhen | kleinster fachlicher Eingriff; Zeitraum, Bedarfe, Abwesenheiten und ausdrücklich zu erhaltende Eingaben bleiben sicher; bestehende Herkunftskennzeichen können verwendet werden; keine neue Historie nötig | Erhalt oder Entfernung nicht automatischer Einträge und der Vorbereitung musste ausdrücklich festgelegt werden und ist nun geklärt | **ausgewählt** |
| B – gesamten Entwurf löschen und für denselben Zeitraum neu anlegen | Entwurf mit allen Kindern entfernen und anschließend neu erzeugen | wirkt auf den ersten Blick wie ein vollständiger Neustart | gefährdet Typ1-, manuelle und andere bewusste Eingaben; neue Entwurfskennung; Überschneidungs- und Fehlerfenster; spätere Referenzen werden unnötig kompliziert | nicht empfohlen |
| C – technischen Vorher-Zustand als Rücksetzpunkt speichern | Vor jeder Übernahme eine vollständige zusätzliche Entwurfskopie speichern und bei Bedarf wiederherstellen | exakte technische Rückkehr zum Vorher-Zustand | führt vor System 12 eine neue Historie ein, benötigt zusätzliche Persistenz und Migration und verdoppelt Zustandsfragen | nicht empfohlen |
| D – nur erneut generieren | Vorhandenen Ablauf erneut starten und das automatische Ergebnis bei Übernahme ersetzen | bereits weitgehend vorhanden; kein zusätzlicher Löschablauf | zeigt keinen wirklich leeren Neustand und erfüllt den Wunsch nach bewusstem vollständigem Verwerfen nicht | keine ausreichende Lösung |

Empfohlener technischer Zuschnitt für Option A:

- **Domain:** eine kleine eigene Operation am `ScheduleDraft`, die den bestätigten Rücksetzumfang validiert, einen neuen widerspruchsfreien Entwurf mit genau einmal erhöhter Version erzeugt und keine technischen Abhängigkeiten kennt.
- **Application:** ein eigener Command mit Anforderung aus Entwurfskennung, erwarteter Version und Zeitraum. Er liest den aktuellen Stand erneut, lehnt fehlenden automatischen Inhalt, veraltete Versionen und Abbruch verständlich ab und übergibt genau einen atomaren Änderungsauftrag an einen schmalen Schreib-Port.
- **Infrastructure:** eine SQLite-Transaktion prüft Entwurf, Version und Zeitraum erneut und entfernt gemeinsam die bestätigten automatischen Zuweisungen samt Segmenten und Deckungen, schwarze `X`, zugehörige Sperren, den aktuellen Laufdatensatz und – falls bestätigt – die bisherige Planungsvorbereitung. Bei jedem Fehler bleibt der vorherige Stand vollständig erhalten.
- **Desktop:** eine getrennte Aktion „Automatischen Plan vollständig verwerfen“ ist nur sichtbar beziehungsweise aktiv, wenn ein übernommenes automatisches Ergebnis existiert. Eine deutliche Bestätigung nennt die tatsächlich betroffenen Inhalte. Abbruch ändert nichts; Erfolg lädt den Zeitraum vollständig neu.
- **Composition:** ausschließlich `Desktop.Composition` verdrahtet den konkreten SQLite-Adapter. View und ViewModel kennen nur Application-Verträge.
- **Modularität:** Command, Ergebnisverträge, Desktop-Aktionen und Bestätigungsdarstellung bleiben kleine getrennte Bausteine; der bestehende Generierungsablauf wird nicht zu einer Sammelklasse erweitert.

Vorgesehene Prüfungen nach Klärung und Implementierungsfreigabe:

- Domain-Tests für den exakt bestätigten Erhalt und die exakt bestätigte Entfernung aller Eintragsarten, Versionsgrenze, leeren Ausgangszustand und unveränderten Fehlerfall,
- Application-Tests für Erfolg, Abbruch, fehlenden automatischen Plan, Entwurfs-, Versions- und Zeitraumkonflikt, genau einen Schreibauftrag und keine automatische Neugenerierung,
- echte temporäre SQLite-Tests für atomare Entfernung, Neustart-Round-trip, vollständiges Rollback bei einem Fehler in der Transaktionsmitte und unveränderte nicht betroffene Werte,
- Desktop- und WPF-Renderprüfungen für Sichtbarkeit, Aktivierung, verständliche Warnung, Bestätigen, Abbrechen, Erfolg, Konflikt, technischen Fehler und Tastaturbedienung,
- Architektur-, Format-, Build- und vollständige Regressionstests,
- manueller sichtbarer Test mit ausschließlich synthetischen Daten: Vorschlag erzeugen, übernehmen, vollständiges Verwerfen abbrechen, erneut bestätigen, leeren Neustand prüfen, neu vorbereiten und erneut generieren.

Bestätigte fachliche Entscheidungen:

1. Typ1-Zuweisungen, `U`, `K`, rote `X` und später mögliche manuelle Zuweisungen bleiben erhalten. Zeitraum, Bedarfe, Mitarbeitende und sonstige fachliche Eingaben werden nicht gelöscht.
2. Sämtliche automatisch erzeugten Zuweisungen werden entfernt, auch wenn sie später gesperrt oder vor automatischer Ersetzung geschützt wurden. Zugehörige Sperren werden gemeinsam entfernt. Dadurch bedeutet „vollständig“ tatsächlich alle automatischen Ergebnisse.
3. Alle schwarzen `X`, der aktuelle Laufmetadatensatz und die bisherige Planungsvorbereitung werden entfernt. Vor einer neuen Generierung muss ausdrücklich erneut „Planung vorbereiten“ gewählt werden; es gibt keine versteckte automatische Vorbereitung oder Generierung.
4. Eine deutliche Bestätigung ohne zusätzliche Texteingabe genügt. Sie nennt die Anzahl automatisch erzeugter Einteilungen und schwarzer `X`, erklärt den Erhalt bewusster Eingaben und weist auf die im aktuellen Ausbauzustand fehlende Rückgängig-Funktion hin.
5. Die Funktion gilt ausschließlich für den aktuellen, noch nicht abgenommenen Arbeitsentwurf. Spätere unveränderliche Planversionen aus System 12 werden niemals gelöscht oder verändert.

Abnahmebedingung des Planungsentwurfs:

- Die fünf Ergänzungsfragen sind beantwortet und Option A ist fachlich ausgewählt. Der genaue Erhaltungs- und Löschumfang ist widerspruchsfrei dokumentiert. Vor der Implementierung ist dieser vollständige AG-14A-Planungsentwurf ausdrücklich abzunehmen; vorher wird kein Fachcode für AG-14A geändert.

Abnahmebedingung der späteren Umsetzung:

- Der bestätigte Umfang wird atomar, versionsgeschützt und ohne automatische Neugenerierung zurückgesetzt; alle automatischen und manuellen Prüfungen sind grün; die Service-Leitung bestätigt den sichtbaren Ablauf. Das sichtbare Gate wird nach dem Änderungsblock gemeinsam in AG-14F durchgeführt.

Umgesetzt und automatisch geprüft am 2026-09-17:

- `ScheduleDraft.DiscardAutomaticGeneration` entfernt alle automatischen Zuweisungen unabhängig von einer Sperre, alle schwarzen `X` und ausschließlich die zu diesen automatischen Zuweisungen gehörenden Sperren. Typ1-, manuelle und andere nicht automatische Zuweisungen sowie `U`, `K`, rote `X`, Bedarfe, Zeitraum und Entwurfskennung bleiben erhalten; die Version steigt genau einmal.
- Ein eigener Application-Command liest Entwurf und Laufzustand erneut, prüft Kennung, Version und Zeitraum, verlangt einen tatsächlich übernommenen automatischen Lauf und übergibt genau einen schmalen atomaren Schreibauftrag. Abbruch, fehlender Lauf, Konflikt und technische Fehler erzeugen verständliche getrennte Ergebnisse und starten niemals Vorbereitung oder Generierung.
- Die SQLite-Transaktion prüft Version, Zeitraum, Vorbereitung und vorhandenen Lauf erneut. Sie entfernt automatische Zuweisungen samt Segmenten und Deckungen, zugehörige Sperren, schwarze `X`, Laufmetadaten und Planungsvorbereitung gemeinsam. Ein echter SQLite-Triggerfehler nach bereits erfolgten Löschungen beweist das vollständige Rollback; ein Neustart-Round-trip beweist den dauerhaften Erfolg. Eine neue Migration war nicht erforderlich, weil nur vorhandene Daten gezielt entfernt werden.
- Eine getrennte kleine WPF-Komponente zeigt die Aktion nur für einen übernommenen automatischen Plan. Die Bestätigung nennt die tatsächliche Zahl der automatischen Einteilungen und schwarzen `X`, die erhaltenen Eingaben, die entfernte Vorbereitung und die fehlende Rückgängig-Funktion. Abbrechen schreibt nichts; Erfolg lädt den Zeitraum neu und verlangt danach eine bewusste neue Vorbereitung. Tastenkürzel und Automation-Namen sind vorhanden.
- Build und Formatprüfung sind mit 0 Warnungen und 0 Fehlern grün. Bestanden haben 342 Domain-, 279 Application-, 126 Planning-, 76 Infrastructure-, 106 Desktop- und 19 Architekturtests, insgesamt 948 Tests. Das leere Excel-Testprojekt wird weiterhin mit dem vereinbarten Exitcode behandelt. `git diff --check` ist grün.
- Das verpflichtende gemeinsame sichtbare WPF-Gate für AG-14 und AG-14A ist noch offen. Es wird mit der fachlichen Ergebnisprüfung des neuen Algorithmus in AG-14F verbunden; erst dessen Annahme kann AG-15 freigeben.

### AG-14B – Änderungsplan, Regelversion und neue Zielverträge

Status: `[x]` – am 2026-09-17 ausdrücklich abgenommen

Ziel und Abgrenzung:

- die Antworten L-01 bis L-10 und die neue Zielmatrix verbindlich konsolidieren,
- die bisherige eingefrorene Nicht-AH-/AH-Phasenregel durch die gemeinsame Primäroptimierung ersetzen,
- eine neue Regelkatalogversion 2 planen, ohne gespeicherte Version-1-Snapshots still umzudeuten,
- die fachlichen Grenzen für spätere kleine Domain-Verträge zu AH-Mindestintegration, Mustervermeidung und relativer Wochenzielannäherung festlegen,
- die bisherige fünfstufige Vorgehensweise ausschließlich als nicht implementierten Rückfall bewahren,
- die fachlich bestätigten Zielaussagen in Grundlagen, Architektur, Master-Roadmap, Status und Service-Leitungsdokumentation als noch nicht implementierten Zielstand konsistent markieren; nach Abnahme dieses Plans den versionierten Regelentscheid und die vollständigen Beispiele in AG-14C ergänzen.

Prüfung:

- jede neue Zielstufe besitzt eine eindeutige fachliche Bedeutung und Reihenfolge,
- das normale Wochenminimum liegt nachweislich vor der AH-Mindestintegration,
- Bedarfsdeckung bleibt vor `Spr`-/`D`-Vermeidung,
- `Spr` liegt vor `D`, beide liegen vor der exakten Stundenannäherung,
- keine vorhandene Version-1-Momentaufnahme erhält still eine neue Bedeutung,
- keine zweite Implementierung, kein versteckter Umschalter und kein neues Paket werden vorgezogen,
- Dokument-, Pfad-, CRLF- und Diff-Prüfung.

Abnahmebedingung:

- Der Auftraggeber bestätigt Zielmatrix, Katalogversion 2, Primärlösung, Rückfallgrenze und die Schritte AG-14C bis AG-14F ausdrücklich. Vorher wird kein Produktions- oder Testcode für die neue Optimierung geändert.

Umgesetzt und geprüft am 2026-09-17:

- Die Antworten L-01 bis L-10 sind in einer eindeutigen Zielmatrix konsolidiert: Bedarfsdeckung und hohe Schutzregeln bleiben vorrangig, danach werden zuerst `Spr` und dann `D` minimiert, anschließend folgen AH-Mindestintegration, relative Wochenzielannäherung und vergleichbare Einsatzfairness.
- Die neue Regelkatalogversion 2 ist als notwendige Versionsgrenze festgelegt. Vorhandene Version-1-Momentaufnahmen behalten ihre bisherige Bedeutung und müssen für den neuen Algorithmus sichtbar neu vorbereitet werden.
- Die gemeinsame globale Optimierung ist die einzige geplante Primärlösung. Die fünfstufige Vorgehensweise bleibt ausschließlich dokumentierter Rückfall nach dem sichtbaren Entscheidungsgate in AG-14F und einem neu abzunehmenden Folgeplan.
- Grundlagen, Architektur, Master-Roadmap, Projektstatus, Fragenkatalog und Service-Leitungsdokumentation unterscheiden den bereits umgesetzten Stand vom noch nicht implementierten Zielstand.
- Produktions- und Testcode, Regelkatalog, Solver, Persistenz und Oberfläche wurden in AG-14B nicht geändert. Neue Domain-Verträge und vollständige versionierte Beispiele gehören erst zu AG-14C.
- Dokumentreihenfolge, referenzierte Pfade, UTF-8 ohne BOM, CRLF-Zeilenenden, nachgestellte Leerzeichen und `git diff --check` wurden geprüft.

### AG-14C – Regelkatalog 2 und unabhängiger Zielvergleich

Status: `[~]` – umgesetzt und automatisch geprüft; ausdrückliche Einzelabnahme offen

Umfang:

- Katalogversion 1 unverändert lesbar halten und Katalogversion 2 als neuen aktuellen Zielkatalog einführen,
- das AH-Mindestziel von 180 Minuten je Person-Woche strukturiert definieren,
- `Spr`- und `D`-Minimierung als getrennte benannte Zielstufen ohne doppelte Regelwirkung modellieren,
- eine gemeinsame relative Wochenzielannäherung für Nicht-AH und AH mit klarer Behandlung von Ziel null definieren,
- `AH_WEEKLY_TARGET` und `MINIMIZE_SPLIT_SHIFTS` in Version 2 ersetzen oder eindeutig neu einordnen, damit keine doppelte Bewertung entsteht,
- Zielvektor, Vergleichsfunktion, Application-Snapshots und Laufmetadaten um die neuen benannten Werte erweitern,
- bestehende Version-1-Vorbereitungen sichtbar als veraltet behandeln und eine bewusste neue Vorbereitung mit Katalogversion 2 verlangen.

Prüfung:

- vollständige Katalog- und Registry-Tests für Version 1 und 2,
- Domain-Tests für jede neue Ranggrenze und kontrollierte Gegentests gegen vertauschte Stufen,
- relative Zielvergleiche für 10-, 20-, 25-, 30-, 35- und 40-Stunden-Ziele sowie durch `U/K` reduzierte Ziele und Ziel null,
- 179, 180, 359, 360, 600, 720 und mehr als 720 AH-Minuten,
- Unveränderlichkeit, serialisierbare Laufmetadaten und bewusste Neu-Vorbereitung statt stiller Snapshot-Änderung,
- Build, Domain-, Application-, Infrastructure- und Architekturtests.

Abnahmebedingung:

- Katalogversion 2 und der vollständige Zielvergleich sind OR-Tools-unabhängig eindeutig und geprüft; Version 1 bleibt historisch lesbar und kein alter Snapshot wird umgedeutet.

Umgesetzt und geprüft am 2026-09-17:

- Katalogversion 2 enthält 30 Regeln; Version 1 mit 28 Regeln bleibt unverändert lesbar und gespeicherte Version-1-Momentaufnahmen behalten ihre historische Bedeutung.
- `Spr`, `D`, AH-Mindestintegration und relative Wochenzielannäherung sind getrennte benannte Zielstufen. `AH_WEEKLY_TARGET` wirkt in Version 2 nicht parallel weiter.
- Das AH-Mindestziel von 180 Minuten und die relative Zielannäherung für normale wirksame Wochenziele, AH mit 600 Minuten sowie Ziel null sind als kleine unveränderliche Domain-Verträge umgesetzt.
- Der Zielvektor, der unabhängige Vergleich und die serialisierbare Application-Momentaufnahme enthalten alle neuen Werte ohne OR-Tools-Abhängigkeit.
- Grenz-, Katalog-, Registry-, Unveränderlichkeits- und Versionsprüfungen decken unter anderem 179/180, 359/360, 600, 720 und mehr als 720 Minuten sowie 10-, 20-, 25-, 30-, 35- und 40-Stunden-Ziele ab.

### AG-14D – Gemeinsame globale Nicht-AH-/AH-Optimierung

Status: `[~]` – umgesetzt und automatisch geprüft; ausdrückliche Einzelabnahme offen

Umfang:

- Nicht-AH- und AH-Kandidaten gleichzeitig in einem gemeinsamen CP-SAT-Modell zulassen,
- die bisherige eingefrorene Nicht-AH-Auswahl und die nachgelagerte AH-Solverphase entfernen,
- alle Zielstufen der neuen Matrix nacheinander optimieren und jeden erreichten Wert vor der Folgestufe fixieren,
- normale Dienste zwischen Nicht-AH und AH umverteilbar halten,
- `Spr` nur als bedarfsbezogenen Notfall zulassen und vor `D` minimieren,
- AH-Mindestintegration nur nach allen früheren Rängen anwenden,
- relative Zielannäherung und anschließende Früh-/Spät-, Wochenend-, `D`- und `Spr`-Fairness unter vergleichbar geeigneten Personen umsetzen,
- Solverzeit, Abbruch, Reproduzierbarkeit, Ergebnisrückübersetzung und unabhängige Nachprüfung erhalten.

Prüfung:

- Nicht-AH bleibt über der hohen Untergrenze, obwohl AH normale Dienste erhält,
- jede geeignete AH-Person erreicht bei ausreichendem Bedarf mindestens 180 Minuten je Woche,
- knapper Bedarf mit mehreren AH-Personen wird relativ fair verteilt,
- weniger `Spr` schlägt bei gleicher Deckung und gleichen hohen Regeln mehr `Spr`; danach gilt dasselbe für `D`,
- kein `D` oder `Spr` nur zur Stundenauffüllung,
- ein globales Umverteilungsszenario, in dem die frühere eingefrorene Phasenlösung nachweislich schlechter ist,
- Früh-/Spät-Fairness bei gleicher und unterschiedlicher Eignung,
- Referenzlöservergleich, deterministische Permutationen und kontrollierte Mutation des alten Phasen-Freeze,
- Planning-, Domain- und vollständige Regressionstests.

Abnahmebedingung:

- Das gemeinsame Modell entspricht in allen Kleinstfällen exakt dem unabhängigen Zielvergleich, erfüllt die neue AH-Mindestintegration und verwendet weder versteckte Punkte noch eine zweite parallele Planungsstrategie.

Umgesetzt und geprüft am 2026-09-17:

- Nicht-AH und AH werden in genau einem gemeinsamen CP-SAT-Modell optimiert; die frühere eingefrorene Nicht-AH-Auswahl, die nachgelagerte AH-Phase und ihre ungenutzten Übersetzer wurden entfernt.
- Erreichte Werte werden in der bestätigten Reihenfolge fixiert: Deckung, hohe Regeln, `Spr`, `D`, AH-Mindestintegration, relative Wochenziele, verbleibende mittlere Regeln, vergleichbare Einsatzfairness und technischer Gleichstand.
- Die relative Zielstufe vergleicht exakte gekürzte Brüche ohne Rundung oder versteckte Punkte.
- Ein vollständig enumerierter Kleinstfall weist nach, dass die gemeinsame Lösung bei gleicher früherer Ranglage einen normalen Dienst an AH umverteilt, Nicht-AH an der hohen Untergrenze hält und die simulierte alte Freeze-Auswahl auf der AH-Mindeststufe schlägt.
- Planning prüft zusätzlich AH-Grenzen, knappe faire Verteilung, hohe Vorrangregeln, Früh-/Spät-, Wochenend-, `D`- und `Spr`-Fairness, Eingabepermutationen, Abbruch, Rückübersetzung und unabhängige Nachprüfung.

### AG-14E – Persistenz-, Laufmetadaten- und Bedienanpassung

Status: `[~]` – umgesetzt und automatisch geprüft; ausdrückliche Einzelabnahme offen

Umfang:

- Laufmetadaten von den alten Bezeichnungen für getrennte Nicht-AH-/AH-Laufzeiten auf neutrale benannte Optimierungsstufen umstellen,
- den vollständigen neuen Zielvektor verlustfrei speichern und nach Neustart lesen,
- erforderliche Schemaänderungen ausschließlich als neue Migration in der gemeinsamen Folge umsetzen; falls das vorhandene Payloadformat ohne Bedeutungsänderung genügt, den begründeten Verzicht auf eine Migration nachweisen,
- Vorbereitung mit Katalogversion 1 sichtbar als veraltet melden und die bewusste Aktualisierung auf Version 2 anbieten,
- Vorschau und vorhandenen Bedienablauf mit neutralen Texten auf die gemeinsame Optimierung abstimmen,
- AG-14A weiterhin unabhängig und atomar funktionsfähig halten.

Prüfung:

- echte temporäre SQLite-Dateien für neuen Lauf, Neustart, Ersatz eines alten Laufs und vollständiges Rollback,
- Upgrade vom aktuellen AG-14A-Schemastand ohne Verlust von Entwurf, Typ1, `U`, `K`, roten `X` oder Bedarfen,
- veraltete Version-1-Vorbereitung startet keinen Solver und wird erst nach bewusster Aktualisierung ersetzt,
- ViewModel-, WPF-Render-, Architektur-, Format-, Build- und vollständige Regressionstests,
- keine deutschen Sätze aus Planning und keine echten Namen oder Plandaten in Metadaten oder Logs.

Abnahmebedingung:

- Katalogwechsel, Laufmetadaten, Vorschau, Übernahme und vollständiges Verwerfen funktionieren verlustfrei und wahrheitsgemäß mit dem gemeinsamen Optimierungsmodell.

Umgesetzt und geprüft am 2026-09-17:

- Application und Planning führen nur noch eine neutrale `OptimizationDuration`; die dokumentierte Stufenfolge beschreibt wahrheitsgemäß die gemeinsame Optimierung.
- Die neue Migration `20260917173925_RenameJointOptimizationDurations` benennt beide historischen Phasenspalten verlustfrei um. Beim Lesen werden beide Altanteile zur neutralen Optimierungsdauer addiert; neue Läufe schreiben nur den aktuellen Anteil.
- Das vorhandene JSON-Payload wurde rückwärtskompatibel um `Spr`-/`D`-Anzahlen, AH-Mindestfälle und relative Wochenzielfälle erweitert. Alte Payloads bleiben mit leeren neuen Werten lesbar.
- Echte temporäre SQLite-Dateien belegen Neustart, Ersatz, Rollback, vollständigen neuen Zielvektor und das Upgrade des AG-14A-Schemas ohne Verlust der historischen Laufzeiten.
- Eine Version-1-Vorbereitung wird als veraltet erkannt und blockiert den Solver bis zur bewussten neuen Vorbereitung mit Version 2.
- Vorschau, Übernahme und vollständiges Verwerfen bleiben unabhängig und verwenden keine alten Phasenbegriffe.

### AG-14F – Fachlicher Vergleich und Entscheidung über den Rückfall

Status: `[~]` – technisch funktionsfähiger Ausgangsstand sichtbar bestätigt; fachliche Qualitätsentscheidung bis nach S09A und S09B zurückgestellt

Umfang:

- die Primärlösung mit verständlichen synthetischen Drei-Wochen-Fällen sichtbar prüfen,
- mindestens vollständig deckbaren, knappen und teilweise nicht deckbaren Bedarf vergleichen,
- mehrere Nicht-AH-Vertragsumfänge und mehrere AH-Personen mit 180-Minuten-Mindestziel abbilden,
- Fälle mit und ohne erforderliches `D` beziehungsweise Notfall-`Spr` zeigen,
- absolute Stunden und relative Zielerfüllung jeder Person-Woche, Früh/Spät, Wochenenden, `D`, `Spr` und offene Bedarfe nachvollziehbar zusammenfassen,
- die Service-Leitung ausdrücklich zwischen Annahme der Primärlösung und Beauftragung eines eigenen Rückfallplans entscheiden lassen.

Prüfung:

- automatischer Vergleich jedes Beispiels gegen Zielvektor und unabhängigen Referenzlöser,
- manuelle fachliche Prüfung der lesbaren Ergebniszusammenfassung,
- gemeinsames sichtbares WPF-Gate aus AG-14/AG-14A mit dem neuen Algorithmus,
- kein stiller Wechsel und keine bereits implementierte zweite Strategie.

Abnahmebedingung:

- **Zwischenstand bestätigt:** Die App erzeugt sichtbar einen zulässigen Vorschlag; der Generator bleibt während S09A und S09B unverändert. Dies ist noch keine fachliche Annahme seiner Ergebnisqualität.
- **Primärlösung nach dem S09B-Bericht angenommen und gegebenenfalls gezielt verbessert:** AG-15 darf beginnen.
- **Primärlösung nach dem S09B-Bericht abgelehnt:** AG-15 bleibt gesperrt; vor jedem Umbau wird ein eigener kleiner Folgeplan für die dokumentierte fünfstufige Rückfalllösung erstellt und ausdrücklich abgenommen.

Automatisch vorbereitet und geprüft am 2026-09-17:

- Fünf vollständig erfundene Vergleichsfälle decken normale AH-Umverteilung an der hohen Nicht-AH-Untergrenze, knappen Bedarf mit zwei AH-Personen, unzulässig großen offenen Bedarf, erforderliches `D` und Notfall-`Spr` mit sichtbarer früher Lücke ab.
- Jeder Fall wird gegen die vollständige Enumeration aller zulässigen Kandidatenauswahlen geprüft. Rein symmetrische Zuordnungen dürfen nur im technischen Gleichstandsschlüssel abweichen; alle fachlichen Stufen müssen übereinstimmen.
- `docs/decisions/S09_JOINT_OPTIMIZATION_ACCEPTANCE_SCENARIOS.md` fasst Ausgangslage, absolute Minuten, `D`, `Spr` und offene Bedarfe verständlich zusammen.
- Der erste sichtbare Lauf hat einen technischen Fehler offengelegt. Die anschließende datensparsame Diagnose zeigte eine falsche unabhängige Nachprüfung der Dienstberechtigung: Bei einem zulässigen `D`-Muster wurde zusätzlich eine nicht erforderliche Freigabe für jeden einzelnen Teildienst verlangt. Die Nachprüfung unterscheidet nun normale Dienste, `D` und `Spr` entsprechend ihren bestätigten Berechtigungsregeln; ein gezielter Regressionstest schützt diesen Fall.
- Technische Planungsfehler werden lokal und begrenzt unter einer Korrelationskennung mit Stufe, Ausnahmetyp, Fehlerwert und ausschließlich geprüftem technischen Kontext protokolliert. Ausnahmetexte, Dateipfade, Namen und Planinhalte werden nicht gespeichert; ein Protokollierungsfehler verändert das Planungsergebnis nicht.
- Der zuvor fehlschlagende vorbereitete lokale Fall wurde ausschließlich lesend erneut ausgeführt und liefert nach der Korrektur innerhalb der verkürzten Diagnosegrenze einen regulären zulässigen Status `FeasibleNotProvenOptimal` statt `TechnicalFailure`. Der vollständige Solution-Build ist ohne Warnungen und Fehler grün; 359 Domain-, 285 Application-, 132 Planning-, 81 Infrastructure-, 106 Desktop- und 19 Architekturtests, insgesamt 982 Tests, bestehen.
- Die Service-Leitung hat den erneuten sichtbaren Generierungslauf am 2026-09-17 bestätigt: Die App kann nach der Korrektur einen Plan erzeugen. Das Ergebnis wurde dabei ausdrücklich noch nicht als optimal oder fachlich gut bewertet. Diese Qualitätsbewertung soll in den weiteren Schritten verbessert und geprüft werden.
- Am 2026-09-17 wurde deshalb die Reihenfolge geändert: Zuerst folgte der reine UI-Umbau S09A bei eingefrorenem Generator; er ist inzwischen vollständig abgenommen und archiviert. Der S09B-Fragenkatalog, die Roadmap und QB-01 bis QB-05 sind am 2026-09-18 ausdrücklich abgenommen. QB-06 mit eigenem nicht-modalem Berichtsfenster ist technisch umgesetzt und vollständig automatisch geprüft. Die sichtbare Prüfung hat jedoch eine beim Speichern verlorene Eingangsphase sowie fehlende Angaben zur Zeitgrenze und zu deshalb nicht begonnenen Folgephasen bestätigt. QB-06A ist dafür als reiner Berichtskorrekturschritt freigegeben. QB-06A.1 bis QB-06A.4 sind abgenommen; QB-06A.5 zeigt die vollständige Phasenfolge, die belegten Solverunterbrechungsdaten und die daraus gebildeten verständlichen Aussagen nun im Generierungsbereich, ohne ältere Details oder personenbezogene Ursachen zu erfinden. Erst auf Grundlage des fertig sichtbar abgenommenen Berichts entsteht ein eigener abgenommener Plan für gezielte Optimierungen oder den dokumentierten Rückfall.
- Offen bleiben die noch nicht einzeln bestätigten Teile des gemeinsamen WPF-Gates aus AG-14/AG-14A, S09B und die anschließende ausdrückliche Entscheidung über die endgültige Primärlösung.

## Bestätigte Zwischenreihenfolge vor AG-15

1. Der aktuelle Generator bleibt als technisch funktionsfähiger, automatisch geprüfter und fachlich noch nicht angenommener Ausgangsstand unverändert.
2. S09A hat ausschließlich die vorhandene Dienstplanoberfläche neu geordnet und ist vollständig abgenommen und archiviert. Der Generator blieb dabei unverändert.
3. `S09B_PLANNING_QUALITY_REPORT_QUESTIONS.md` und `S09B_PLANNING_QUALITY_REPORT_ROADMAP.md` sowie QB-01 bis QB-05 sind ausdrücklich abgenommen. QB-06 stellt Planungs- und Generierungsbericht im eigenen nicht-modalen Einzelfenster bereit. Vor seiner erneuten sichtbaren Abnahme korrigiert der freigegebene QB-06A-Plan die vollständige Phasenherkunft und belegte Unterbrechungsangaben; QB-06A.1 bis QB-06A.4 sind abgenommen, QB-06A.5 stellt die daraus abgeleiteten wahrheitsgemäßen Application-Aussagen im Generierungsbereich dar und wartet auf die sichtbare Abnahme.
4. S09B bleibt von System 10 getrennt: Es liefert nachvollziehbare Qualitätswerte zur Beurteilung des Generators, aber noch keine vollständigen deutschen Ursachenanalysen oder Lösungsvorschläge.
5. Erst der Bericht bestimmt, ob Laufzeit, Regelübersetzung, Zielmatrix, gemeinsame Optimierung oder die dokumentierte Rückfalllösung geändert werden sollen. Jede fachliche oder technische Optimierung erhält vor ihrer Umsetzung einen eigenen kleinen, ausdrücklich abgenommenen Folgeplan.
6. AG-15 prüft erst den danach als endgültig vorgesehenen Algorithmus mit Referenzlöser, Invarianten, Mutationen und Lastfällen. AG-16 schließt System 09 anschließend insgesamt ab.

Während S09A und S09B werden Solver, Zielmatrix und Generierungsregeln nicht parallel verändert. Synthetische Fälle bilden den versionierten Vergleichsmaßstab; lokale Anwendungsdaten dürfen sichtbar beurteilt, aber niemals in Repository, Tests oder Berichtsexporte übernommen werden.

### AG-15 – Unabhängiger Korrektheits-, Mutations- und Leistungsnachweis

Status: `[ ]`

Umfang:

- sämtliche gemeinsamen S07-Szenarien gegen Domain-Auswertung und Planning ausführen,
- CP-SAT für viele kleine Fälle mit dem erschöpfenden Referenzlöser vergleichen,
- deterministisch erzeugte Invariantenfälle ausführen,
- alle verbindlichen Mutationen einzeln durchführen, ihr erwartetes Testversagen dokumentieren und die Mutation wieder entfernen,
- den realitätsnahen sowie den größeren synthetischen Lastfall wiederholt messen,
- die wichtigsten Kombinationsfälle in einfacher Sprache für die fachliche Prüfung zusammenfassen.

Prüfung:

- vollständige Regel- und Szenariomatrix ohne ausgelassene Regel,
- wertgleiche Zulässigkeit und Zielvektoren zwischen Referenzlöser und CP-SAT in allen unterstützten Kleinstfällen,
- jede Mutation wird mindestens von einem dafür vorgesehenen Test erkannt,
- dokumentierte Hardware, Solver-Version, Einstellungen und getrennte Zeitanteile des Lastfalls,
- keine echten Namen, realen Pläne oder produktiven Daten in Szenarien, Ausgaben oder Messungen,
- fachliches Gate zu offene Zeit gegen offene Plätze, gleichrangigen Regeln, `D`, `Spr`, Früh-/Spätfairness und mehreren AH-Personen.

Abnahmebedingung:

- S09A und S09B sind abgeschlossen, die daraus abgeleitete Optimierung oder Rückfallentscheidung wurde in einem eigenen Folgeplan umgesetzt und AG-14F hat den endgültig vorgesehenen Algorithmus ausdrücklich bestätigt. Die automatischen Nachweise sind vollständig grün, alle Mutationen wurden erkannt und entfernt, der Lastfall liegt in der bestätigten Größenordnung ohne kritische Auffälligkeit und die Service-Leitung bestätigt die fachlichen Kombinationsbeispiele. Dieses Gate stoppt vor AG-16.

### AG-16 – System-09-Gesamtnachweis und Übergabe

Status: `[ ]`

Umfang:

- alle Schritte und offenen Gates gegen Roadmap, Fragenkatalog, Architektur und Clean Code prüfen,
- vollständigen Build und vollständigen Regressionstest aller Projekte ausführen,
- Architektur, Paketgrenzen, Formatierung, Migrationen, Pfade und Datenschutz prüfen,
- `MASTER_ROADMAP.md`, `STATUS.md` und Service-Leitungsdokumentation wahrheitsgemäß aktualisieren,
- Roadmap und beantworteten Fragenkatalog erst nach ausdrücklicher Gesamtabnahme gemeinsam nach `docs/roadmaps/completed` verschieben,
- Übergaben an Systeme 10 bis 12 festhalten.

Prüfung:

- vollständiger Build ohne neue Warnungen oder Fehler,
- alle Domain-, Application-, Planning-, Infrastructure-, Desktop- und Architekturtests,
- `git diff --check`, Prüfung der CRLF-Vorgabe und Suche nach veralteten aktiven Pfaden,
- Suche nach Datenbanken, Sicherungen, Exporten, Logs, echten Namen, Zugangsdaten und anderen unbeabsichtigten Artefakten,
- bestätigte Gates aus AG-14 und AG-15,
- keine Behauptung einer vollständigen gesetzlichen oder tariflichen Konformität.

Abnahmebedingung:

- Der Auftraggeber bestätigt den vollständigen System-09-Umfang ausdrücklich. Erst danach werden Roadmap und Fragenkatalog archiviert und System 09 in der Master-Roadmap als abgeschlossen markiert.

## Echte externe und manuelle Gates

- AG-01: ausdrückliche Abnahme dieses Roadmap-Entwurfs vor jedem Fachcode.
- AG-14: sichtbare WPF-Abnahme des minimalen Generierungsablaufs.
- AG-14A: ausdrückliche Abnahme des Planungsentwurfs vor der Implementierung und sichtbare Abnahme des vollständigen Verwerfens vor AG-15.
- AG-14B: ausdrückliche Abnahme des neuen Optimierungsplans vor jeder Codeänderung.
- S09A: sichtbare Abnahme des reinen UI-Umbaus bei unverändertem Generator.
- S09B: ausdrückliche Abnahme des Fragenkatalogs, der daran angepassten Roadmap und der anschließenden neutralen Ausgangsmessung.
- AG-14F: fachliche Annahme des nach Bericht und gegebenenfalls gezielter Optimierung endgültig vorgesehenen Algorithmus oder ausdrückliche Entscheidung für einen eigenen Rückfallplan; vorher beginnt AG-15 nicht.
- AG-15: fachliche Abnahme der besonders wichtigen Kombinationsfälle und Bewertung des dokumentierten Performance-Nachweises.
- AG-16: ausdrückliche Gesamtabnahme von System 09.
- Portable `win-x64`-Ausgabe und OR-Tools-Start auf einem sauberen Windows-11-System bleiben System 15 und werden hier nicht als bestanden behauptet.

## Risiken und Schutzmaßnahmen

| Risiko | Schutzmaßnahme |
|---|---|
| Eine Regel wird im Solver vergessen oder still ignoriert | vollständiges Registry-Verzeichnis gegen alle 28 Katalogdefinitionen; unbekannt oder unübersetzt blockiert vor Solverstart |
| Solverbedingung und fachliche Bedeutung driften auseinander | gemeinsame S07-Szenarien, OR-Tools-unabhängige Domain-Nachprüfung und Kleinstfall-Referenzlöser |
| Niedrige Ziele überstimmen höhere Ziele | getrennte lexikografische Stufen, fixierte Zielwerte und Gegentests an jeder Grenze |
| Gleichrangige Regeln erhalten unbemerkt Gewichte | ausführbarer Zielvergleich, kein regelübergreifendes Ausmaß, Referenzvergleich und explizites Stop-Gate bei technischer Nichtabbildbarkeit |
| `Spr` verbirgt die Zeit vor dem tatsächlichen Wechsel | Teildeckung bleibt eigener Fachwert; Deckungs-, Ergebnis- und Mutationstests |
| AH verdrängt Nicht-AH unter den Mindestkorridor | normale hohe Untergrenze vor AH-Mindestziel fixieren; gemeinsame Nachprüfung und gezielte Mutation |
| Alte Version-1-Snapshots erhalten still neue Regeln | neue Katalogversion 2; alte Vorbereitung sichtbar veraltet; bewusste erneute Vorbereitung |
| Relative Fairness wird durch Rundung oder Punkte verzerrt | OR-Tools-unabhängige Vergleichssemantik vor Solvercode; Referenzfälle für alle Starttypen und reduzierte Ziele |
| Rückfall erzeugt vorsorglich einen zweiten Algorithmus | nur Dokumentation; eigener Folgeplan und ausdrückliche Entscheidung erst nach AG-14F |
| Reproduzierbarkeit hängt von Hash-, Thread- oder Eingabereihenfolge ab | feste Solver-Einstellungen, stabile Kandidatenordnung, deterministische IDs und Wiederholung in mehreren Aufbauten |
| Ein Fehler überschreibt den vorhandenen Entwurf teilweise | flüchtige Vorschau, erneute Versionsprüfung und eine echte SQLite-Transaktion mit Fehler-in-der-Mitte-Tests |
| Ein technischer Fehler ist nicht untersuchbar oder loggt sensible Daten | stabiler Fehlercode und Korrelation mit begrenztem strukturiertem Kontext; Negativprüfung auf Namen und Planinhalte |
| Große Klassen koppeln Regelbau, Optimierung und Mapping | getrennte interne Bereiche und kleine Übersetzer; Architektur- und Review-Gates je Schritt |
| Laufzeittest wird flakyanfällig | getrenntes manuell bewertetes Performance-Gate auf dokumentierter Hardware; Korrektheit bleibt hartes automatisches Gate |
| System 10 bis 12 werden vorweggenommen | expliziter Nicht-Umfang und schmale Ergebnis-, Vorschau- und Übernahmeverträge |
| echte Mitarbeiterdaten gelangen in Tests oder Repository | ausschließlich klar erfundene synthetische Daten und Abschlussprüfung der Dateiliste |

## Übergaben an spätere Systeme

- System 10 erhält die in S09B bereits neutral dargestellten wirksamen Bedarfe, Zuweisungen und stabilen offenen Zeitanteile sowie Regelbewertungen, Hinweis-, Ursache- und technische Grundcodes und fachliche Parameter. Es dupliziert die Bedarfsrechnung nicht, sondern ergänzt für teilweise oder vollständig offene Bedarfe vollständige Konflikterklärungen und Lösungsvorschläge.
- System 11 verwendet den aktuellen Entwurf, ergänzt allgemeine manuelle Bearbeitung, bedienbare Einzelsperren und bestätigte Abweichungen. Es darf keine automatische Neugenerierung durch eine manuelle Änderung auslösen.
- System 12 übernimmt einen fachlich geprüften aktuellen Entwurf in unveränderliche Planversionen und bewahrt ältere Fassungen.
- System 15 prüft die portable Windows-Ausgabe einschließlich nativer OR-Tools-Laufzeit auf einem geeigneten sauberen Windows-11-System.
- Eine spätere alternative gleichwertige Planung darf an den gekapselten deterministischen Einstellungen und einer dann ausdrücklich definierten Suchabsicht ansetzen; System 09 stellt dafür keine sichtbare oder ungenutzte allgemeine Strategieinfrastruktur bereit.
- Ein späteres Punktesystem wäre eine neue fachliche Entscheidung mit eigener Prioritätsmatrix, Dominanznachweis, Tests und ausdrücklicher Abnahme.

## Berichtsschema nach jedem Schritt

Der Abschlussbericht nennt jeweils:

1. tatsächlich geändert,
2. bewusst nicht geändert,
3. ausgeführte Prüfungen und Ergebnis,
4. kritische Meldungen mit Auswirkung und Dringlichkeit oder ausdrücklich keine,
5. konkreten Handlungsbedarf oder ausdrücklich keinen,
6. offene Gates, Risiken oder Blockaden,
7. Git-Status ohne Commit oder Push,
8. nächsten minimalen Roadmap-Schritt,
9. Bitte um ausdrückliche Abnahme, wenn ein Gate oder Handlungsbedarf besteht.

## Nächster minimaler Schritt

AG-07 bis AG-14B wurden am 2026-09-17 ausdrücklich abgenommen. AG-14C bis AG-14E sind umgesetzt und automatisch geprüft; ihre ausdrückliche Einzelabnahme ist offen. Die erneute sichtbare Generierung wurde bestätigt, die Ergebnisqualität aber ausdrücklich noch nicht. S09A ist abgeschlossen und archiviert. Der S09B-Fragenkatalog, die Roadmap und QB-01 bis QB-05 sind abgenommen. QB-06 mit eigenem nicht-modalem Berichtsfenster und Aktualitätsbindung ist technisch umgesetzt. Die sichtbare Prüfung hat zwei Berichtslücken bestätigt; QB-06A ist für ihre Korrektur in fünf kleinen Teilen freigegeben. QB-06A.1 bis QB-06A.4 sind abgenommen. QB-06A.5 ist technisch umgesetzt und zeigt die vollständige Eingangsprüfung, belegte Unterbrechungsangaben, die Zwischenstandseinordnung und den Grund nicht begonnener Folgephasen; sieben gezielte Fälle sind grün. Der nächste minimale Schritt ist die sichtbare Prüfung und ausdrückliche Abnahme von QB-06A.5 und der korrigierten QB-06-Anzeige. Danach beginnt QB-07 mit dem nächsten vollständigen Projektcheck. AG-15 bleibt bis zu Bericht, gezielter Folgeentscheidung und ausdrücklicher Annahme des endgültigen Algorithmus gesperrt.
