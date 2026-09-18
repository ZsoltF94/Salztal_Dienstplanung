# S09B – Planungsqualität und Generierungsphasen nachvollziehbar berichten

Status: Fragenkatalog, Roadmap sowie QB-01 bis QB-05 am 2026-09-18 ausdrücklich abgenommen; QB-06 technisch umgesetzt und vollständig automatisch geprüft; QB-06A am 2026-09-18 ausdrücklich zur schrittweisen Umsetzung freigegeben, QB-06A.1 bis QB-06A.4 abgenommen, QB-06A.5 technisch umgesetzt und gezielt geprüft, sichtbare Abnahme offen

Stand: 2026-09-19

## Ziel und Nutzen

S09B macht die Qualität des fachlich unveränderten automatischen Planungsergebnisses nachvollziehbar. Die Service-Leitung erhält einen Planungsbericht über Bedarfsdeckung, Wochenarbeitszeiten und Dienstverteilung sowie einen getrennten Generierungsbericht über die nacheinander ausgeführten Optimierungsphasen.

Der Bericht soll insbesondere sichtbar machen, wie `F` und `S` pro Person und Woche verteilt sind und wie viele `D` die aktuelle Berechnung verwendet. Er ist die Messgrundlage für einen danach gesondert zu planenden Optimierungs- oder Rückfallschritt. S09B selbst verbessert den Generator noch nicht.

Die verbindlichen Detailentscheidungen stehen im vollständig beantworteten und konsolidierten Fragenkatalog `S09B_PLANNING_QUALITY_REPORT_QUESTIONS.md`. Diese Roadmap setzt sie um, benötigt aber vor jeder Codeänderung ihre eigene ausdrückliche Abnahme.

## Voraussetzung und Freigabegrenze

- S09A ist vollständig umgesetzt, automatisch geprüft, sichtbar abgenommen und archiviert.
- Der aktuelle Generator ist technisch funktionsfähig, seine fachliche Ergebnisqualität aber ausdrücklich noch nicht angenommen.
- Solver, Zielmatrix, Regelkatalog, Generierungsregeln und Auswahlverhalten bleiben während S09B fachlich unverändert.
- Der Fragenkatalog ist vollständig beantwortet und konsolidiert.
- Diese Roadmap ist an die Antworten angepasst und wird vor jeder Codeänderung ausdrücklich abgenommen.
- Bestehende Benutzeränderungen im Arbeitsbaum werden nicht überschrieben. Es werden keine echten Mitarbeiter- oder Plandaten in Repository, Tests, Screenshots, Diagnose oder Exporte übernommen.

## Geplantes Zielbild

```text
Hauptfenster: Dienstplan                 Eigenes nicht-modales Berichtsfenster
├─ Wochenplanung                         ├─ Drei-Wochen-Zusammenfassung
├─ Generierung / Vorschau               ├─ Planung
│  └─ Bericht anzeigen ────────────────────────▶ │  ├─ Bedarfe und Deckung je Woche
└─ bleibt parallel bedienbar              │  ├─ Soll, wirksames Soll, Planzeit
                                             │  └─ Dienste je Person: F / S / D / Spr
                                             └─ Generierung
                                                ├─ Status, Phasen und Laufzeiten
                                                ├─ Spr- und D-Minimierung
                                                ├─ Fairness- und Regelwerte
                                                └─ technische Details oder Fehlerdetails
```

Das Berichtsfenster blockiert das Hauptfenster nicht und kann daneben angeordnet werden. Es zeigt höchstens den aktuellen flüchtigen Vorschlag beziehungsweise den dazu gehörenden übernommenen automatischen Lauf; es entsteht keine Berichtshistorie. Ein erneuter Aufruf für denselben Stand aktiviert das vorhandene Fenster statt ein Duplikat zu öffnen. Wenn der zugrunde liegende Vorschlag oder Lauf nicht mehr aktuell ist, wird der alte Inhalt nicht still als aktueller Bericht weitergeführt.

Bei einem blockierten, abgebrochenen, zeitüberschrittenen oder technisch fehlgeschlagenen Generierungsversuch zeigt das Fenster nur den Generierungs- beziehungsweise Fehlerbericht. Der nicht vorhandene Planungsbericht wird eindeutig als nicht verfügbar gekennzeichnet.

## Umfang

- vollständige Bedarfsdeckungswerte pro Woche einschließlich vollständiger und `Spr`-Teilabdeckung,
- gut lesbare Personenliste mit Wochen-Soll, wirksamem Soll, geplanter Arbeitszeit und bestätigter Abweichungsdarstellung,
- Dienstanzahlen je Person und Woche mit ausdrücklicher Behandlung von `D`, `Spr`, Typ1-Bürozeit und späteren manuellen Zusatzbesetzungen,
- `U`- und `K`-Anzahlen im Personendetail als Erklärung des wirksamen Solls; keine Zählspalten für rote oder schwarze `X` in der Haupttabelle,
- neutrale Diagnosewerte für die beobachtete `F`-/`S`-Verteilung mit Einzelwerten, Minimum, Maximum und Spannweite sowie `D`- und `Spr`-Gesamtanzahl,
- geordneter Generierungsbericht über Planungsphasen, erreichte Zielwerte, Status und belastbar messbare Laufzeiten für jeden Generierungsversuch,
- Fehlerbericht ohne erfundene Planwerte für blockierte, abgebrochene, zeitüberschrittene und technisch fehlgeschlagene Versuche,
- eigenes nicht-modales WPF-Berichtsfenster, das neben dem weiterhin bedienbaren Hauptfenster offen bleiben kann,
- Application-Verträge für unveränderliche Berichtsmomentaufnahmen,
- ausschließlich erforderliche strukturierte Planning-Ergebniswerte ohne deutsche Sätze,
- Desktop-Darstellung und sichtbare Abnahme mit synthetischen Daten,
- automatische Fach-, Mapping-, ViewModel-, WPF- und Architekturprüfungen im tatsächlich betroffenen Umfang.

## Nicht-Umfang

- keine Änderung von Solverbedingungen, Zielreihenfolge, Gewichten, Fairnessformel, `Spr`-/`D`-Regeln oder Zeitbudget,
- keine Verbesserung der `F`-/`S`-Verteilung und keine zusätzliche `D`-Minimierung in S09B,
- keine Behauptung eines einzelnen menschlichen oder kausalen Grundes für jede konkrete Personenzuweisung,
- keine vollständigen Ausschlussursachen und Lösungsvorschläge aus System 10,
- keine allgemeine manuelle Planbearbeitung aus System 11,
- keine unveränderlichen Planversionen oder Berichtshistorie aus System 12,
- keine parallelen Berichtskopien oder Vergleiche mehrerer vergangener Läufe,
- kein Excel-, PDF-, Druck-, Zwischenablage- oder sonstiger Diagnoseexport,
- keine Cloud, Telemetrie oder externe Datenübertragung,
- kein vorgezogener AG-15-Nachweis für einen noch nicht endgültig angenommenen Algorithmus.

## Architekturgrenzen

- `Domain` beziehungsweise `Application` berechnen stabile fachliche Berichtskennzahlen aus vorhandenen unveränderlichen Momentaufnahmen.
- `Planning` liefert nur die tatsächlich benötigten strukturierten Phasenwerte, Zielwerte, Regelbewertungen und Laufzeiten. OR-Tools-Typen und deutsche Berichtstexte verlassen `Planning` nicht.
- Der Bericht wird aus dem bereits erzeugten Ergebnis aufgebaut und startet keinen zweiten versteckten Solverlauf.
- `Desktop` zeigt vorbereitete Werte, filtert und navigiert. ViewModels oder XAML berechnen weder Bedarfsdeckung noch Wochenminuten, Dienstanzahlen oder Fairness.
- Das eigene WPF-Berichtsfenster bleibt nicht modal. Fenstererzeugung, Aktivierung eines bereits geöffneten Berichts und Lebensdauer werden von der Desktop-Schicht koordiniert; das Berichts-ViewModel erzeugt kein `Window` und kennt keine konkrete Fensterinstanz.
- Ein Fehlerbericht verwendet ausschließlich strukturierte Status-, Phasen- und Fehlerwerte. Ungefilterte Ausnahmetexte, lokale Pfade, Namen und Planinhalte werden nicht in die sichtbare technische Diagnose oder das Fehlerprotokoll übernommen.
- Falls neutrale Phasenwerte für den nach Übernahme weiterhin identischen Bericht gespeichert werden müssen, geschieht dies über Application-Ports und Infrastructure-Implementierungen; EF-/SQLite-Typen überschreiten die Modulgrenze nicht.
- Nur `Desktop.Composition` verdrahtet konkrete Adapter.
- Berichtswerte werden unabhängig gegen Vorschlag und Planstand validiert. Ein Widerspruch ist ein technischer Fehler und kein anzeigbarer Qualitätswert.

## Prüfstrategie ab QB-04

- Nach jedem Schritt laufen nur noch Prüfungen, die den neu geschriebenen Code oder sein Zusammenwirken mit bestehendem Code abdecken.
- Prüfungen, die ausschließlich unveränderten älteren Code betreffen, werden nicht nach jedem Einzelschritt wiederholt.
- Nach jeweils drei abgeschlossenen Schritten folgt ein vollständiger Projektcheck. Der vollständige Lauf nach QB-03 bildete den Ausgangspunkt; nach QB-06 bestanden 1027 Prüfungen. Der vereinbarte vollständige Check nach QB-06A.3 ist mit 1037 bestandenen Prüfungen abgeschlossen. QB-06A.4, QB-06A.5 und der abschließende QB-07 bilden die nächsten drei Schritte.
- Ein konkreter Fehlerverdacht, eine berührte Architekturgrenze oder ein unerwartetes Verhalten darf jederzeit zusätzliche gezielte Prüfungen auslösen.

## Vorhandene Anker vor QB-02 erneut prüfen

- `src/Salztal.Dienstplanung.Application/Scheduling/AutomaticScheduleProposal.cs`
- `src/Salztal.Dienstplanung.Application/Scheduling/AutomaticScheduleRunMetadata.cs`
- `src/Salztal.Dienstplanung.Application/Scheduling/AutomaticScheduleRunRecord.cs`
- `src/Salztal.Dienstplanung.Application/Scheduling/ScheduleWorkspaceSnapshot.cs`
- `src/Salztal.Dienstplanung.Application/Scheduling/ScheduleDemandSlotSnapshot.cs`
- `src/Salztal.Dienstplanung.Application/Scheduling/ScheduleAssignmentSnapshot.cs`
- `src/Salztal.Dienstplanung.Application/Availabilities/AvailabilityPeriodEmployeeSnapshot.cs`
- `src/Salztal.Dienstplanung.Domain/Scheduling/Optimization/ScheduleObjectiveVector.cs`
- `src/Salztal.Dienstplanung.Planning/Optimization/DemandCoverageOptimizer.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/AutomaticSchedulePreviewViewModel.cs`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewView.xaml`
- `src/Salztal.Dienstplanung.Desktop/Features/Scheduling/ScheduleOverviewViewModel.cs`
- `src/Salztal.Dienstplanung.Desktop/MainWindow.xaml.cs`
- `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowComposition.cs`
- zugehörige Tests in Application, Planning, Infrastructure, Desktop und Architecture

Die Liste ist eine Prüfroute, keine vorsorgliche Freigabe, alle genannten Dateien zu ändern. QB-02 grenzt die kleinste tatsächlich erforderliche Vertragsänderung ein.

## Umsetzungsschritte

### QB-01 – Fragenkatalog konsolidieren und Roadmap abnehmen

Status: `[x]` – Fragenkatalog und angepasste Roadmap am 2026-09-18 ausdrücklich abgenommen

Umfang:

- die bereits bestätigten drei Planungsbericht-Inhalte und den Generierungsbericht festhalten,
- bestätigte Aussagegrenze zwischen Phasenbilanz und unbelegbarer Einzelbegründung dokumentieren,
- Soll-/Planzeit, `D`-/`Spr`-Zählung, `F`-/`S`-Vergleich, Fehlerbericht, Persistenz und eigenes Berichtsfenster konsolidieren,
- vollständig beantworteten Fragenkatalog gegen diese Roadmap prüfen,
- diese Roadmap ausdrücklich abnehmen.

Prüfung:

- alle Kernfragen besitzen eine eindeutige bestätigte Antwort,
- S09B bleibt messend; der Generator und System 10 werden nicht vorgezogen,
- Leitdokumente nennen denselben nächsten Schritt und dieselbe Freigabegrenze,
- `git diff --check` und gezielte Pfad-/Verweisprüfung sind grün.

Abnahmebedingung:

- Fragenkatalog und angepasste Roadmap sind vollständig verstanden und ausdrücklich freigegeben. Erst danach beginnt QB-02.

### QB-02 – Unveränderliche Berichtsverträge und Referenzrechnung

Status: `[x]` – umgesetzt, vollständig automatisch geprüft und am 2026-09-18 ausdrücklich abgenommen

Umfang:

- kleinste erforderliche Application-Berichtsmomentaufnahme für erfolgreiche und erfolglose Generierungsversuche festlegen,
- Bedarfsdeckungs-, Wochenminuten- und Dienstzählregeln zentral implementieren,
- strukturierte Planning-Phasenwerte nur dort ergänzen, wo vorhandene Endwerte keine wahrheitsgemäße Aussage erlauben,
- Bericht gegen Vorschlag, Eingabemomentaufnahme und gegebenenfalls übernommenen Lauf eindeutig binden,
- Phasenfortschritt und strukturierte Fehlerdetails bei einem erfolglosen Versuch verlustfrei bis zum Generierungsbericht transportieren, ohne einen Plan vorzutäuschen,
- unabhängige synthetische Referenzfälle für Normaldienst, `D`, `Spr`, Typ1-Bürozeit, Abwesenheit und offene Deckung erstellen.

Prüfung:

- Domain-/Application-Vertragstests für Validierung, Sortierung, Unveränderlichkeit und exakte Minuten,
- Planning-Tests für vollständige Phasenreihenfolge und unveränderten Zielvektor,
- Vertragsprüfungen für blockiert, abgebrochen, Zeitgrenze ohne Vorschlag und technischen Fehler einschließlich letzter erreichter Phase,
- Architekturtests gegen OR-Tools-, EF-, UI- und Sprachtext-Leaks,
- Reproduzierbarkeitstest: Berichtserfassung verändert den erzeugten Vorschlag nicht.

Abnahmebedingung:

- Verträge und Referenzrechnung sind vollständig, unabhängig geprüft und enthalten noch keine vorsorgliche Export-, Historien- oder System-10-Struktur.

### QB-03 – Planungsbericht: Bedarfe und Deckung

Status: `[x]` – umgesetzt, mit vollständigem Build, Formatprüfung und allen 1017 automatischen Prüfungen grün; am 2026-09-18 ausdrücklich abgenommen

Umfang:

- alle wirksamen Bedarfsplätze den drei Wochen zuordnen,
- benötigte, gedeckte und offene Personen beziehungsweise Minuten berechnen,
- vollständige, teilweise `Spr`-gedeckte und ungedeckte Intervalle unterscheiden,
- bestätigte Wochen- und Gesamtsummen bereitstellen,
- vollständige Liste und bestätigte Filterdarstellung im Desktop umsetzen.

Prüfung:

- exakt gedeckter, vollständig offener und teilweise durch `Spr` gedeckter synthetischer Bedarf,
- mehrere gleiche Dienste/Plätze, abweichende tatsächliche Zeit und mehrere Einsatzorte,
- Summen stimmen exakt mit den Einzelzeilen überein,
- vollständig gedeckte Bedarfe verschwinden nicht durch die Standarddarstellung.

Abnahmebedingung:

- jeder Bedarf ist pro Woche vollständig und verständlich prüfbar; Teildeckung und offene Zeiträume werden nicht beschönigt.

### QB-04 – Planungsbericht: Personen, Wochenzeiten und Dienste

Status: `[x]` – umgesetzt, gezielt automatisch geprüft und am 2026-09-18 ausdrücklich abgenommen; die gemeinsame sichtbare Darstellung folgt wahrheitsgemäß erst mit dem Berichtsfenster in QB-06

Umfang:

- alle bestätigten Personen pro Woche in stabiler Gruppen- und Namensreihenfolge zeigen,
- bestätigte Soll-, wirksame Soll-, Planzeit- und Differenzwerte darstellen,
- jeden relevanten Dienst stabil zählen,
- die bestätigte Zählweise für `D`, `Spr`, Bürozeit und manuelle Zusatzbesetzung umsetzen,
- `U`- und `K`-Anzahlen nur im Personendetail zeigen und rote beziehungsweise schwarze `X` aus den Dienstzählspalten heraushalten,
- neutrale `F`-/`S`-Einzelwerte, Minimum, Maximum und Spannweite unter geeigneten und in der Woche grundsätzlich planbaren Personen sowie `D`-/`Spr`-Gesamtwerte bereitstellen, ohne eine neue Optimierungsregel zu erfinden.

Prüfung:

- Personen mit null Diensten, voller Abwesenheit, Typ1-, regulärer und AH-Rolle,
- exakte Wochenabgrenzung und Arbeitsminuten zusammengesetzter Muster,
- stabile Nullspalten für relevante, aber nicht zugewiesene Dienste,
- Summen der Personenzählung stimmen mit den Zuweisungen überein,
- sichtbare Vergleichbarkeit von `F`, `S` und `D` bei vielen Personen,
- fachlich ungeeignete oder in der Woche nicht grundsätzlich planbare Personen verfälschen Minimum, Maximum und Spannweite nicht.

Abnahmebedingung:

- die Service-Leitung kann pro Person und Woche Arbeitszeitabweichung und Dienstverteilung ohne eigene Nebenrechnung erkennen.

### QB-05 – Generierungsbericht: Phasenbilanz

Status: `[x]` – technisch umgesetzt, gezielt automatisch geprüft und am 2026-09-18 ausdrücklich abgenommen; die sichtbare gemeinsame Prüfung erfolgt im QB-06-Berichtsfenster

Umfang:

- jede bestätigte Planungsphase in tatsächlicher Reihenfolge strukturiert berichten,
- Ziel, Status, erreichbare und festgeschriebene Werte, Fälle, Ausmaß und belastbar messbare Laufzeit darstellen,
- reguläre Deckung ohne `Spr` und die danach unter dem bestätigten `Spr`-Notfall erreichbare zusätzliche Deckung getrennt ausweisen,
- `Spr`- und `D`-Minimierung als eigene sichtbare Phasen ausweisen,
- AH-Mindestintegration, relative Wochenzielannäherung, Regelprioritäten und Stabilität getrennt halten,
- Optimalität, Zeitgrenze und nicht anwendbare Phasen wahrheitsgemäß kennzeichnen,
- technische Reproduzierbarkeitswerte in einen nachgeordneten Detailbereich legen,
- bei jedem erfolglosen Generierungsversuch Status, abgeschlossene Phasen, zuletzt erreichte Phase, Laufzeiten und vorhandene strukturierte Fehlerdetails berichten; Planungswerte bleiben nicht verfügbar.

Prüfung:

- vollständige und stabile Phasenfolge,
- Endwerte stimmen mit Zielvektor, Regelbewertungen und Vorschlag überein,
- `D`-/`Spr`-Anzahl und Wochenzielwerte stimmen mit dem Planungsbericht überein,
- optimaler und nur zulässiger, nicht nachweislich optimaler Lauf,
- blockierter, abgebrochener, zeitüberschrittener und technisch fehlgeschlagener Lauf ohne erfundene Planwerte,
- keine deutschen Texte in Planning und keine unbelegte Einzelursache.

Abnahmebedingung:

- die Phasenwirkung ist nachvollziehbar, ohne einen nicht vorhandenen menschlichen Entscheidungsweg oder System-10-Ursachen vorzutäuschen.

Technisch umgesetzt und gezielt geprüft am 2026-09-18:

- Der Application-Bericht führt immer alle 15 bestätigten Phasen in stabiler Reihenfolge. Nicht erreichte und nicht anwendbare Phasen, abgebrochene Läufe sowie nachgewiesene beziehungsweise nicht vollständig nachgewiesene Optimalität bleiben unterscheidbar.
- Vorher-, Ergebnis- und festgeschriebene Werte werden nur aus bereits vorhandenen Solver- und Zielwerten gebildet. Reguläre Deckung, zusätzliche `Spr`-Deckung, `Spr`-/`D`-Minimierung, AH-Mindestintegration, relative Wochenzielabweichung, Regelstufen, Stabilität und technischer Gleichstand bleiben getrennt. Es gibt keinen zweiten Solverlauf.
- Erfolgreiche Berichte werden gegen Zielvektor und Planungsbericht geprüft. Erfolgslose Versuche behalten letzte erreichte Phase, Laufzeiten und strukturierte Fehlerdetails, erzeugen aber keine Planungswerte.
- Desktop bereitet Status, Phasen, Messwerte, Regelzeilen und technische Details bereits anzeigefertig auf. Das eigentliche nicht-modale Fenster und damit die sichtbare Prüfung bleiben ausschließlich QB-06.
- Nach einer Übernahme bleibt die neutrale Phasenbilanz am aktuellen automatischen Lauf erhalten. Die Migration `20260918164237_PersistAutomaticSchedulePhases` ergänzt dafür ausschließlich die strukturierte Phasenfolge; ältere gespeicherte Läufe erhalten eine leere Folge und bleiben lesbar. Eine Berichtshistorie entsteht nicht.
- Der Generator, seine Zielreihenfolge, Regeln, Auswahl und sein Zeitbudget wurden fachlich nicht verändert.

### QB-06 – Eigenes WPF-Berichtsfenster und Bedienprüfung

Status: `[~]` – technisch umgesetzt; vollständiger Solution-Build, Formatprüfung und alle 1027 automatischen Prüfungen grün; sichtbare Bedien- und Lesbarkeitsabnahme offen

Umfang:

- bestätigten Einstieg aus Vorschau und gegebenenfalls übernommenem Lauf umsetzen,
- eigenes nicht-modales Berichtsfenster öffnen beziehungsweise bei erneutem Aufruf für denselben Stand aktivieren,
- Haupt- und Berichtsfenster gleichzeitig sichtbar und bedienbar halten; Schließen, Aktivieren, Minimieren, Wiederherstellen und Hauptfensterende eindeutig behandeln,
- veraltete oder nicht mehr vorhandene Berichtsquellen nicht still als aktuell weiteranzeigen,
- Planungs- und Generierungsbereich klar trennen,
- Drei-Wochen-Navigation, Tabellen, Filter, Scrollen, Fokus und Tastaturbedienung umsetzen,
- Null-, Leer-, Teildeckungs- und Nicht-optimal-Zustände textlich verständlich darstellen,
- bei vielen Personen und Bedarfen lesbar bleiben, ohne den Dienstplan dauerhaft zu verkleinern.

Prüfung:

- ViewModel- und WPF-Strukturtests,
- Fokus-, Tastatur-, Scroll- und Filtertests,
- Fensterlebenszyklus-, Mehrfachaufruf-, Nebenherbedienungs- und Aktualitätstests,
- automatischer Rendercheck in repräsentativen Fenstergrößen,
- gemeinsame sichtbare Prüfung mit einem ausschließlich synthetischen Drei-Wochen-Fall.

Abnahmebedingung:

- die Service-Leitung bestätigt Lesbarkeit, Vollständigkeit, Bedienung beider Berichtsteile und das parallele Arbeiten mit Haupt- und Berichtsfenster. Automatische Tests oder Screenshots ersetzen dieses Gate nicht.

Technisch umgesetzt und automatisch geprüft am 2026-09-18:

- Vorschau und aktueller übernommener Lauf stellen denselben Berichtsvertrag über „Bericht anzeigen“ bereit. Das Fenster ist nicht modal; ein zweiter Aufruf aktiviert und restauriert dieselbe Instanz statt ein weiteres Fenster zu öffnen.
- Planungs- und Generierungsbericht sind getrennt. Bedarfe besitzen Wochen- und Offenfilter, Personen Wochen- und Gruppenfilter; Tabellen bleiben horizontal und vertikal scrollbar. Die drei Wochen, Null- und Leerstände, Teildeckung, Fehlerläufe und nicht nachgewiesene Optimalität werden textlich ausgewiesen.
- Der Bericht ist an Snapshot, Entwurf und Version gebunden. Nach einer späteren Entwurfsänderung werden die früheren Inhalte im bereits offenen Fenster sofort durch einen sichtbaren Veraltet-Hinweis ersetzt. Ältere Läufe ohne vollständige Phasenfolge werden nicht als aktueller Vollbericht ausgegeben.
- Das Hauptfenster bleibt während des Berichts bedienbar. Minimieren, Wiederherstellen, Schließen, erneutes Öffnen sowie das gemeinsame Ende mit dem Hauptfenster sind automatisch geprüft.
- 17 fokussierte Application- und 17 fokussierte Desktop-Prüfungen sichern Projektion, Aktualität, Einstieg, Filter, repräsentative Fenstergrößen, Struktur, Tastaturschließen, Fehlerzustand und Fensterlebenszyklus. Der vereinbarte vollständige Check umfasst 359 Domain-, 292 Application-, 132 Planning-, 81 Infrastructure-, 144 Desktop- und 19 Architekturtests, insgesamt 1027 bestandene Prüfungen. Das unverändert leere Excel-Testprojekt meldet den dokumentierten Exitcode 8. Solution-Build und Formatprüfung sind grün.
- Der Generator, Zielvektor, Solveraufruf und Zeitbudget blieben fachlich unverändert. Die gemeinsame sichtbare Prüfung mit synthetischen Daten ist weiterhin ein echtes manuelles Gate.

Korrigierter sichtbarer Befund am 2026-09-18:

- Beim ersten manuellen Wechsel in „Generierung“ beendete eine reproduzierbare `XamlParseException` die App. Die drei `Run.Text`-Bindungen der Phasenüberschrift verwendeten ohne ausdrücklichen Modus den schreibenden WPF-Standard und versuchten deshalb, die schreibgeschützten Eigenschaften `Sequence`, `Name` und `StatusDisplay` zurückzuschreiben.
- Die Anzeige-Bindungen sind nun ausdrücklich `OneWay`. Der WPF-Regressionstest wechselt in beiden repräsentativen Fenstergrößen tatsächlich vom Planungs- in den Generierungsbereich und hätte den ursprünglichen Fehler ausgelöst. Die frühere Strukturprüfung hatte nur die beiden Planungs-Untertabs aktiviert und konnte den verzögert aufgebauten Generierungsinhalt deshalb nicht prüfen.
- QB-06 bleibt trotz technischer Korrektur auf `[~]`, bis der korrigierte Tabwechsel und die übrige Bedienung erneut sichtbar bestätigt sind.

### QB-06A – Vollständige Herkunfts- und Unterbrechungsberichte

Status: `[~]` – bestätigter Befund und fünfteiliger Umsetzungsplan am 2026-09-18 dokumentiert und ausdrücklich zur schrittweisen Umsetzung freigegeben; QB-06A.1 bis QB-06A.4 ausdrücklich abgenommen, QB-06A.5 technisch umgesetzt und gezielt geprüft, sichtbare Abnahme offen

Bestätigter sichtbarer und technischer Befund:

- Die Eingangs- und Strukturprüfung wird vor dem Planning-Engine-Aufruf tatsächlich ausgeführt und im flüchtigen Planungsergebnis als abgeschlossen vorangestellt. Die Vorschlagsmetadaten werden jedoch vorher aus der erst mit dem Modellaufbau beginnenden Engine-Phasenfolge erzeugt. Bei der Übernahme wird diese unvollständige Metadatenfolge gespeichert; der erneut geladene Bericht deutet die fehlende Eingangsprüfung deshalb sachlich falsch als „Nicht erreicht“.
- Die reguläre Bedarfsdeckung verwendet dasselbe gemeinsame Solver-Zeitbudget wie die übrigen Optimierungsphasen. Erreicht der Solver dort innerhalb der 120 Sekunden nur eine zulässige Auswahl ohne Optimalitätsbeweis, wird die laufende Phase unterbrochen, die letzte zulässige Auswahl als Rückfallergebnis übernommen und die gesamte weitere Optimierungsfolge nicht mehr begonnen.
- Der heutige Phasenschnappschuss speichert bei einer Unterbrechung nur Status und Laufzeit. Konkreter Abbruchgrund, gerade aktives Teilziel, Zeitbudget und belastbare Zwischenwerte werden verworfen. Deshalb kann der sichtbare Bericht weder die Zeitgrenze als Grund nennen noch erklären, warum die Folgephasen nicht begonnen wurden.
- Für den bereits gespeicherten Lauf lassen sich weder die ursprüngliche Dauer der Eingangsprüfung noch das bei Ablauf aktive der beiden Teilziele der regulären Bedarfsdeckung nachträglich belegen. QB-06A erfindet diese Werte nicht. Vorhandene unvollständige Läufe werden ausdrücklich als ältere Aufzeichnung mit fehlenden Detaildaten gekennzeichnet.

Ziel und Abnahmegrenze:

- Vorschau und erneut geladener aktueller Lauf verwenden dieselbe vollständige kanonische Phasenfolge einschließlich Eingangs- und Strukturprüfung.
- Eine unterbrochene Phase nennt aus strukturierten Laufdaten mindestens den bestätigten Abbruchgrund, das aktive Teilziel, Zeitgrenze und verbrauchte Zeit sowie vorhandene belastbare Werte der behaltenen zulässigen Auswahl.
- Nachfolgende Phasen werden als nicht begonnen mit Verweis auf die vorherige Unterbrechung ausgewiesen, statt ohne Zusammenhang nur „Nicht erreicht“ zu zeigen.
- Ältere Aufzeichnungen bleiben lesbar, werden aber nicht durch erfundene Laufzeiten, Teilziele oder Zwischenwerte vervollständigt.
- Solverbedingungen, Zielreihenfolge, Auswahl, Zeitbudget und Rückfallverhalten bleiben unverändert. Die Korrektur startet keinen zweiten Solverlauf und zieht weder personenbezogene System-10-Ursachen noch Lösungsvorschläge vor.

#### QB-06A.1 – Strukturierter Phasenherkunfts- und Abbruchvertrag

Status: `[x]` – technisch umgesetzt, gezielt geprüft und vor dem Start von QB-06A.2 ausdrücklich abgenommen

Umfang:

- den kleinsten unveränderlichen Application-Vertrag für Phasenherkunft, Detailverfügbarkeit, Abbruchgrund, aktives Teilziel, Zeitbudget und bekannte Zwischenwerte festlegen,
- Zeitgrenze mit zulässiger Auswahl, Zeitgrenze ohne zulässige Auswahl, Benutzerabbruch und technisches Scheitern unterscheidbar halten,
- ausschließlich stabile Codes und primitive Werte über Modulgrenzen geben; OR-Tools-Typen und deutsche Texte bleiben außerhalb des Vertrags,
- ältere Phasendaten ohne neue Details ausdrücklich repräsentieren, statt fehlende Werte als fachliches Ergebnis umzudeuten.

Prüfung:

- gezielte Application-Vertragstests für Validierung, Unveränderlichkeit, vollständige Zustandsunterscheidung und ältere Aufzeichnungen,
- Architekturprüfung für technische Typ- und Sprachgrenzen im unmittelbar betroffenen Umfang.

Technisch umgesetzt und gezielt geprüft am 2026-09-18:

- Erfolgreiche Laufmetadaten unterscheiden nun eine vollständige Phasenaufzeichnung, eine vorhandene Aufzeichnung ohne Eingangsprüfung und einen Lauf ohne aufgezeichnete Phasen. Die Kennzeichnung wird aus der vorhandenen unveränderlichen Phasenfolge abgeleitet und benötigt keine zweite Wahrheitsquelle.
- `AutomaticSchedulePhaseTerminationSnapshot` trägt ausschließlich stabile Codes und primitive Werte für Zeitgrenze mit oder ohne zulässige Auswahl, Benutzerabbruch oder technischen Fehler, das gegebenenfalls aktive Optimierungsziel sowie gemeinsames Zeitbudget und bereits verstrichene Budgetzeit.
- Unterbrochene und fehlgeschlagene ältere Phasen ohne den neuen Abschlussdatensatz bleiben gültig, kennzeichnen ihre Abschlussdetails aber ausdrücklich als nicht aufgezeichnet. Vorhandene Phasenwerte können bei späterer Befüllung als belastbarer Zwischenstand erhalten bleiben.
- Der Vertrag weist widersprüchliche Kombinationen aus Phasenstatus, Abbruchgrund und aktivem Optimierungsziel sowie unvollständige oder ungültige Budgetwerte zurück. OR-Tools-Typen und deutsche Anzeigetexte wurden nicht aufgenommen.
- In diesem Teilabschnitt erzeugte Planning die neuen Abschlussdetails noch nicht; Persistenz und sichtbare Berichte wurden ebenfalls nicht verändert. Diese Grenzen blieben ausdrücklich QB-06A.2 bis QB-06A.5 vorbehalten.
- 19 fokussierte Application-Vertragstests und alle 19 Architekturtests bestehen. Die direkt nachgelagerten Planning- und Infrastructure-Projekte bauen jeweils ohne Warnungen oder Fehler.

#### QB-06A.2 – Vollständige Planning-Aufzeichnung ohne Generatoränderung

Status: `[x]` – technisch umgesetzt, gezielt geprüft und vor dem Start von QB-06A.3 ausdrücklich abgenommen

Umfang:

- die Eingangs- und Strukturprüfung vor Abschluss des erfolgreichen Vorschlags in dieselbe kanonische Phasenfolge aufnehmen, die Vorschau und Persistenz verwenden,
- bei jeder Solverunterbrechung Grund, aktive Optimierung, gemeinsames Zeitbudget, verbrauchte Zeit und vorhandene belastbare Zwischenwerte verlustfrei an den neuen Vertrag übergeben,
- bei regulärer Bedarfsdeckung die beiden tatsächlich vorhandenen Teilziele „gedeckte reguläre Minuten“ und „vollständig berührte Bedarfsplätze“ strukturiert unterscheidbar machen,
- die Ursache der Unterbrechung erfassen, ohne eine unbelegte Erklärung für die Schwierigkeit des Optimalitätsbeweises zu behaupten,
- nachweisen, dass Zielvektor, Vorschlag, deterministische Auswahl, 120-Sekunden-Budget und Anzahl der Solverläufe unverändert bleiben.

Prüfung:

- gezielte Planning-Tests für vollständige erfolgreiche Phasenfolge einschließlich Eingangsprüfung,
- kontrollierte kurze Zeitgrenzen für zulässige Zwischenlösung, fehlende zulässige Lösung und Abbruch,
- Reproduzierbarkeitsvergleich von Vorschlag und Zielvektor vor und nach der reinen Instrumentierung.

Technisch umgesetzt und gezielt geprüft am 2026-09-19:

- Das Voranstellen der erfolgreichen Eingangs- und Strukturprüfung erzeugt bei einem erfolgreichen Ergebnis nun zugleich neue unveränderliche Vorschlagsmetadaten. Äußeres Planungsergebnis und Vorschlag verwenden damit dieselbe kanonische Phasenfolge; die bestehende Vorschlagssignatur und alle fachlichen Vorschlagsdaten bleiben unverändert.
- Das gemeinsame Solverbudget hält vor jedem bereits vorhandenen Optimierungsaufruf ausschließlich das aktive strukturierte Teilziel fest. Für die reguläre Bedarfsdeckung sind „gedeckte reguläre Minuten“ und „vollständig berührte Bedarfsplätze“ getrennt nachweisbar. Weitere Solveraufrufe, neue Bedingungen oder eine veränderte Zielreihenfolge wurden nicht ergänzt.
- Zeitgrenzen mit oder ohne zulässige Auswahl erfassen Ziel, konfiguriertes Budget und bereits verbrauchte Budgetzeit unmittelbar beim festgestellten Abbruch. Abbruch und technischer Fehler werden ebenfalls strukturiert an die aktive Phase übergeben.
- Bei einer Zeitgrenze mit zulässiger Auswahl werden die für die aktive Phase belastbaren Kennzahlen ausschließlich aus der bereits gehaltenen Auswahl berechnet. Dafür wird kein zweiter Solverlauf gestartet; der vorhandene Rückfall und die nachfolgenden nicht begonnenen Optimierungsphasen bleiben unverändert.
- In QB-06A.2 selbst wurden Persistenz, erneutes Laden, Application-Berichtsaussagen und sichtbare Darstellung bewusst nicht geändert; diese Grenzen blieben den nachfolgenden Teilen vorbehalten.
- 20 fokussierte Application-Vertragstests und 23 fokussierte Planning-/Integrationstests bestehen. Darin enthalten sind die vollständige Erfolgsfolge einschließlich Eingangsprüfung, kontrollierte Zeitgrenzen beim ersten und zweiten regulären Teilziel, Zeitgrenze ohne Auswahl, Abbruch sowie der bestehende reproduzierbare Vorschlagsfingerabdruck. Das produktive 120-Sekunden-Limit bleibt unverändert. Der vereinbarte vollständige Projektcheck wurde nach QB-06A.3 ausgeführt.

#### QB-06A.3 – Verlustfreie Speicherung und erneutes Laden

Status: `[x]` – technisch umgesetzt, gezielt und vollständig geprüft und vor dem Start von QB-06A.4 ausdrücklich abgenommen

Umfang:

- die vollständige kanonische Phasenfolge und die neuen strukturierten Unterbrechungsdetails über den bestehenden Application-Port beim aktuellen automatischen Lauf speichern,
- eine SQLite-Modelländerung und Migration nur dann abwärtsverträglich ergänzen, wenn die neuen strukturierten Werte nicht im vorhandenen Phasenmodell verlustfrei gespeichert werden können,
- ältere Läufe ohne diese Daten lesbar halten und ihre Detailgrenze ausdrücklich kennzeichnen,
- Vorschau, übernommener Lauf und nach Neustart geladener Bericht auf identische vorhandene Phasen- und Abbruchwerte prüfen; eine Berichtshistorie entsteht weiterhin nicht.

Prüfung:

- gezielte Infrastructure-Rundreisetests mit echter temporärer SQLite-Datenbank für neuen und älteren Datensatz,
- Integrationstest von Generierung, Übernahme und erneutem Workspace-Laden,
- anschließend der vereinbarte vollständige Projektcheck, weil mit QB-06A.1 bis QB-06A.3 drei weitere Schritte abgeschlossen wären.

Technisch umgesetzt und geprüft am 2026-09-19:

- Der bestehende aktuelle Lauf speichert die strukturierten Abschlussdetails innerhalb des bereits vorhandenen Phasen-JSON. Gesichert werden Abbruchgrund, aktives Optimierungsziel, Zeitgrenze und verbrauchte Budgetzeit; Phasenstatus, Laufzeit und Zwischenwerte bleiben unverändert Teil desselben Datensatzes.
- Eine neue Datenbankspalte oder Migration war nicht erforderlich. Das neue `Termination`-Objekt ist optional, sodass ältere Phasen-JSONs ohne dieses Feld und bereits migrierte Datensätze mit leerer Phasenfolge weiterhin gelesen werden können.
- Eine ältere unterbrochene Phase ohne gespeicherte Abschlussdetails bleibt als unterbrochen erhalten und kennzeichnet ihre Detailgrenze ausdrücklich als nicht aufgezeichnet. Fehlende Werte werden weder ergänzt noch aus Laufzeit oder Ergebnisstatus geschätzt.
- Ein echter SQLite-Integrationslauf belegt Generierung, Vorschau, Übernahme und erneutes Workspace-Laden nach Neustart mit identischer kanonischer Phasenfolge und identischen Abbruchdetails. Der Laufdatensatz bleibt weiterhin auf genau einen aktuellen Lauf je Entwurf begrenzt; eine Berichtshistorie wurde nicht eingeführt.
- Drei gezielte SQLite-Nachweise sowie alle 83 Infrastructure-Tests bestehen. Der vereinbarte vollständige Projektcheck ist mit Solution-Build ohne Warnungen oder Fehler, grüner Formatprüfung sowie 359 Domain-, 297 Application-, 135 Planning-, 83 Infrastructure-, 144 Desktop- und 19 Architekturtests grün, insgesamt 1037 bestandene Prüfungen. Das unverändert leere Excel-Testprojekt meldet den dokumentierten Exitcode 8.

#### QB-06A.4 – Wahrheitsgemäße Application-Berichtsaussagen

Status: `[x]` – technisch umgesetzt, gezielt geprüft und vor dem Start von QB-06A.5 ausdrücklich abgenommen

Umfang:

- aus den strukturierten Daten verständliche deutsche Aussagen zu Zeitgrenze, fehlendem Optimalitätsbeweis und behaltenem zulässigem Zwischenstand bilden,
- jede nicht begonnene Folgephase eindeutig auf die vorherige Unterbrechung und deren bestätigten Grund beziehen,
- vorhandene Zwischenwerte ausdrücklich als nicht optimal bewiesenen Zwischenstand kennzeichnen und nicht als festgeschriebenes Optimum ausgeben,
- ältere unvollständige Aufzeichnungen als „Detaildaten nicht gespeichert“ statt als „Nicht erreicht“ darstellen,
- weiterhin keine personenbezogenen Ursachen, hypothetischen Alternativen oder System-10-Lösungsvorschläge erzeugen.

Prüfung:

- gezielte Application-Berichtstests für optimalen Lauf, Zeitgrenze mit und ohne zulässige Auswahl, Abbruch, technischen Fehler und ältere gespeicherte Aufzeichnung,
- Konsistenzprüfung zwischen Planungsbericht, Zielvektor, Phasenbericht und gespeichertem Lauf.

Technisch umgesetzt und gezielt geprüft am 2026-09-19:

- Jeder Phasenbericht enthält nun zusätzlich eine bereits in Application gebildete deutsche Erklärung. Eine optimal abgeschlossene Optimierungsphase weist den belegten Optimalitätsnachweis aus; eine unterbrochene Phase benennt ausschließlich den strukturiert gespeicherten Abbruchgrund.
- Bei einer Zeitgrenze werden konfiguriertes Budget, verbrauchte Budgetzeit und das aktive Teilziel verständlich benannt. Nur beim bestätigten Fall mit zulässiger Auswahl wird erklärt, dass der vorhandene Zwischenstand behalten wurde und seine Optimalität nicht nachgewiesen ist.
- Aufgezeichnete Werte einer unterbrochenen oder fehlgeschlagenen Phase werden getrennt als Teil- beziehungsweise Zwischenstand eingeordnet und niemals als bewiesenes Optimum bezeichnet.
- Jede nach einer belegten Unterbrechung fehlende Folgephase erklärt, dass sie deshalb nicht begonnen wurde, und verweist auf die konkrete vorherige Phase und den bestätigten Grund. Fehlen bei einem älteren Lauf die Abschlussdetails, nennt der Bericht stattdessen ausdrücklich „Detaildaten nicht gespeichert“ und erfindet weder Zeitgrenze noch Teilziel.
- Application deckt optimalen Lauf, Zeitgrenze mit und ohne zulässige Auswahl, Benutzerabbruch, technischen Fehler, ältere unvollständige Aufzeichnung und einen erneut geladenen übernommenen Lauf mit sieben gezielten grünen Testfällen ab. Formatprüfung und Desktop-Build sind ohne Warnungen oder Fehler grün. Die vorhandene WPF-Darstellung wurde noch nicht verändert; das bleibt ausschließlich QB-06A.5 vorbehalten.

#### QB-06A.5 – Sichtbare Darstellung und erneute Abnahme

Status: `[~]` – technisch umgesetzt und gezielt geprüft; sichtbare Abnahme offen

Umfang:

- Eingangsprüfung, konkrete Unterbrechungsursache, aktives Teilziel, Zeitangaben, bekannte Zwischenwerte und Grund der nicht begonnenen Folgephasen im vorhandenen Generierungsbereich anzeigen,
- die Darstellung in normaler und kleiner Fenstergröße lesbar und unabhängig von Farbe verständlich halten,
- den bereits gespeicherten unvollständigen Lauf ohne erfundene Werte als ältere Detailaufzeichnung kennzeichnen,
- QB-06 erst nach der erneuten sichtbaren Prüfung beider Berichtsteile zur Abnahme vorlegen.

Prüfung:

- gezielte Desktop- und WPF-Prüfungen für alle neuen Texte, Leerzustände, Scrollbarkeit und den echten Tabwechsel,
- `git diff --check`, Datenschutz- und Artefaktprüfung,
- gemeinsame sichtbare Prüfung mit synthetischen Daten: vollständige Phase 1, Zeitgrenze in Phase 4, erklärter Abbruch der Folgephasen und ältere Detailaufzeichnung.

Technisch umgesetzt und gezielt geprüft am 2026-09-19:

- Der vorhandene Generierungsbereich übernimmt die in Application vorbereitete Phasenerklärung unverändert. Desktop leitet weder Unterbrechungsursache noch aktives Teilziel oder Zeitgrenze selbst her.
- Die bisher missverständliche Überschrift „Nicht erreicht“ heißt bei fehlender Aufzeichnung neutral „Keine aufgezeichnete Ausführung“. Der Erklärungstext unterscheidet darunter belegte nicht begonnene Folgephasen von älteren Läufen, deren Abschlussdetails nicht gespeichert wurden.
- Jede aufgeklappte Phase besitzt einen farbunabhängig beschrifteten Bereich „Einordnung dieser Phase“. Vorhandene Teilwerte erhalten zusätzlich „Einordnung der Werte“ und werden dort ausdrücklich als nicht optimal bewiesener Zwischenstand bezeichnet. Die bereits vorhandenen Kennzahlen bleiben separat sichtbar.
- Der Generierungsbereich ist horizontal und vertikal scrollbar. Automatische WPF-Prüfungen wechseln tatsächlich in den Reiter „Generierung“, öffnen die unterbrochene und die folgende Phase und prüfen die Texte in 860 × 560 sowie 1180 × 780 Pixeln.
- Sieben gezielte Desktop-Prüfungen decken den vollständigen Lauf, die Zeitgrenze mit behaltenem zulässigem Zwischenstand, die deshalb nicht begonnene Folgephase und einen älteren unvollständigen Lauf ohne erfundene Zeitgrenze ab. Die gemeinsame sichtbare Prüfung durch die Service-Leitung bleibt das offene Gate.

Abnahmebedingung für QB-06A:

- Die Service-Leitung kann im Bericht ohne eigene technische Schlussfolgerung erkennen, dass die Eingangsprüfung tatsächlich durchgeführt wurde, warum der Optimalitätsbeweis beendet wurde und weshalb die nachfolgenden Phasen nicht begonnen wurden. Nicht mehr rekonstruierbare Altdaten bleiben sichtbar unbekannt. Erst danach kann die offene QB-06-Sichtabnahme abgeschlossen werden.

### QB-07 – Regression, Dokumentation und Optimierungsübergabe

Status: `[ ]`

Umfang:

- vollständige betroffene Regression und Architekturprüfung ausführen,
- nachweisen, dass Vorschlag, Zielvektor und Reproduzierbarkeit gegenüber dem eingefrorenen Ausgangsstand unverändert sind,
- Status, Master-Roadmap, System-09-Roadmap und Service-Leitungsstand wahrheitsgemäß aktualisieren,
- anhand des sichtbar beurteilten Berichts konkrete Messbefunde dokumentieren, ohne lokale Personendaten zu übernehmen,
- einen eigenen kleinen Folgeplan für bestätigte Optimierungen oder die dokumentierte Rückfalllösung vorbereiten.

Prüfung:

- betroffene und vollständige automatische Tests grün,
- Release-/Debug-Build im erforderlichen Umfang grün,
- `git diff --check`, Pfad-, Datenschutz- und Artefaktprüfung grün,
- keine echte Berichtsausgabe, Datenbank, Sicherung oder Exportdatei im Repository,
- sichtbare S09B-Gesamtabnahme protokolliert.

Abnahmebedingung:

- S09B ist technisch und sichtbar abgenommen; die Qualitätsbefunde sind neutral dokumentiert. Erst danach darf ein eigener Optimierungs- oder Rückfallplan freigegeben werden. AG-15 bleibt bis zur Umsetzung und Annahme des endgültig vorgesehenen Algorithmus gesperrt.

## Echte manuelle Gates

- vollständige Beantwortung und ausdrückliche Abnahme des Fragenkatalogs,
- ausdrückliche Abnahme der daran angepassten Roadmap vor QB-02,
- ausdrückliche QB-04-Abnahme ist erfolgt; die tatsächliche sichtbare Prüfung der Planungsbericht-Tabellen bleibt Bestandteil des in QB-06 erstmals vorhandenen Berichtsfensters,
- sichtbare Prüfung der verständlichen Aussagegrenze des Generierungsberichts im QB-06-Berichtsfenster,
- ausdrückliche Abnahme des QB-06A-Plans vor seiner Implementierung sowie sichtbare Prüfung der korrigierten Herkunfts- und Unterbrechungsberichte,
- gemeinsame Bedien- und Gesamtabnahme in QB-06/QB-07,
- eigene Freigabe jedes danach geplanten Optimierungs- oder Rückfallschritts.

## Risiken und Schutzmaßnahmen

| Risiko | Schutzmaßnahme |
|---|---|
| Bericht verändert den Generator | fachliche Generierungsstrecke einfrieren; Ergebnis und Reproduzierbarkeit vor/nach Instrumentierung vergleichen |
| Endergebnis wird als menschliche Begründung ausgegeben | Phasenbilanz klar von Kausal- und Gegenfaktualanalyse trennen |
| `D` oder `Spr` wird doppelt gezählt | Zählsemantik vor QB-02 bestätigen und mit Segmentgrenzfällen testen |
| Sollzeit und Planzeit werden verwechselt | Wochen-Soll, wirksames Soll, geplante Arbeitszeit und Differenz eindeutig benennen |
| `F`-/`S`-Spannweite erzeugt eine neue versteckte Fairnessregel | Kennzahl nur beschreibend verwenden; Optimierungsformel erst im eigenen Folgeplan entscheiden |
| Teildeckung wird in Summen beschönigt | Zeitintervalle und Minuten unabhängig aus Bedarf und Deckung nachrechnen |
| Desktop dupliziert Fachlogik | fertige Application-Berichtsmomentaufnahme; Architektur- und ViewModel-Tests |
| Phasenmessung verfälscht das Zeitbudget | monotone Messung außerhalb des Solverbudgets beziehungsweise ohne zusätzlichen Solverlauf; Laufzeitvergleich |
| Bericht wird bei manuellen Änderungen fälschlich dem alten Lauf zugeschrieben | Planstand und Generierungslauf eindeutig binden; abweichenden Stand klar kennzeichnen |
| Personenbezogene Berichte gelangen in Dateien oder Logs | kein Export, keine Telemetrie, datensparsame Fehlerdiagnose, synthetische Tests und Screenshots |
| Große Tabellen werden unlesbar | Wochen-/Bereichsnavigation, stabile Köpfe, gezielte Filter und sichtbares WPF-Gate |
| Nicht-modales Fenster zeigt nach einer Änderung einen veralteten Stand | Bericht eindeutig an Vorschlag beziehungsweise Lauf binden; Aktualitätswechsel sichtbar behandeln und keinen alten Inhalt still umdeuten |
| Mehrfaches Öffnen erzeugt viele widersprüchliche Fenster | für denselben aktuellen Stand vorhandenes Berichtsfenster aktivieren statt duplizieren |
| Fehlerbericht verliert die eigentliche Fehlerstelle | abgeschlossene und zuletzt erreichte Phase sowie strukturierte Fehlerdetails bereits am Modulübergang sichern |
| Eingangsprüfung geht zwischen Vorschau und gespeichertem Lauf verloren | eine einzige kanonische Phasenfolge bis in Vorschlagsmetadaten, Persistenz und erneutes Laden verwenden |
| „Nicht optimal bewiesen“ bleibt ohne belegten Abbruchgrund | Zeitgrenze, aktives Teilziel und vorhandenen zulässigen Zwischenstand strukturiert am Solverübergang erfassen |
| Alte Läufe werden rückwirkend mit erfundenen Details ergänzt | fehlende historische Detaildaten ausdrücklich kennzeichnen; Laufzeit, Teilziel und Zwischenwerte niemals rekonstruieren oder schätzen |

## Definition des abgeschlossenen Zwischenschritts

S09B ist erst zur Abnahme bereit, wenn:

- Fragenkatalog und Roadmap ausdrücklich abgenommen wurden,
- alle Bedarfe und ihre Deckung pro Woche vollständig und nachrechenbar erscheinen,
- alle bestätigten Personen pro Woche mit den bestätigten Soll- und Planzeitwerten erscheinen,
- jeder relevante Dienst je Person und Woche nach der bestätigten Semantik gezählt wird,
- `F`-/`S`-Verteilung und `D`-Anzahl sichtbar beurteilbar sind,
- der Generierungsbericht jede tatsächliche Phase wahrheitsgemäß und in richtiger Reihenfolge bilanziert,
- jeder erfolglose Generierungsversuch einen wahrheitsgemäßen Fehlerbericht ohne erfundene Planwerte liefert,
- das eigene nicht-modale Berichtsfenster neben dem bedienbaren Hauptfenster genutzt werden kann und keinen veralteten Stand als aktuell ausgibt,
- der Bericht keine unbelegten Einzelursachen oder System-10-Lösungen erfindet,
- die Berichtserfassung Auswahlverhalten, Zielvektor, Zeitbudget und Reproduzierbarkeit nicht verändert,
- automatische Prüfungen, Builds, Architektur-, Format- und Datenschutzprüfungen im betroffenen Umfang grün sind,
- die sichtbare WPF-Gesamtabnahme erfolgt ist,
- Dokumentation und Roadmap-Status den tatsächlichen Stand wiedergeben.

Abgeschlossen und `[x]` wird S09B erst nach ausdrücklicher Abnahme. Der Bericht entscheidet nicht selbst, welche Optimierung folgt.

## Nächster minimaler Schritt

QB-01 bis QB-05 sind ausdrücklich abgenommen. QB-06 ist technisch umgesetzt; das eigene nicht-modale Berichtsfenster liegt vor. Die sichtbare Prüfung hat jedoch die zwei in QB-06A dokumentierten Berichtslücken bestätigt. Der QB-06A-Plan ist ausdrücklich freigegeben, QB-06A.1 bis QB-06A.4 sind abgenommen. QB-06A.5 zeigt die vollständige Eingangsprüfung, belegte Unterbrechungsdaten, die Einordnung vorhandener Zwischenwerte und den Grund nicht begonnener Folgephasen im Generierungsbereich und ist gezielt geprüft. Der nächste minimale Schritt ist die gemeinsame sichtbare Prüfung und ausdrückliche Abnahme von QB-06A.5 und damit der offenen QB-06-/QB-06A-Anzeige. Erst danach beginnt QB-07 mit Regression, Dokumentation und Optimierungsübergabe; dabei folgt vereinbarungsgemäß der nächste vollständige Projektcheck.
