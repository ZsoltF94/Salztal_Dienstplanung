# Fragen und Verständnisabgleich: Automatische Plangenerierung

Status: Ursprünglicher Fragenkatalog, AG-14A und Änderungsfragen vor AG-15 vollständig beantwortet; Antworten L-01 bis L-10 in AG-14B konsolidiert und ausdrücklich abgenommen; spätere Reihenfolge S09A, S09B, gezielte Optimierung und AG-15 bestätigt

Stand: 2026-09-17

## Zweck und Grenze

Dieses Dokument sammelt die fachlichen, technischen und prüfbezogenen Fragen für System 09 „Automatische Plangenerierung“. Es übernimmt die bereits abgenommenen Regeln und die unveränderliche Planungsmomentaufnahme aus den Systemen 07 und 08 und klärt ausschließlich, wie daraus ein zulässiger, bestmöglicher und reproduzierbarer Plan erzeugt werden soll.

Der Fragenkatalog ist weder eine Teil-Roadmap noch eine Implementierungsfreigabe. Er erlaubt insbesondere noch keine Änderung an Domain, Application, Planning, Infrastructure, Desktop, Datenbank oder Paketbestand. Erst nach vollständiger Beantwortung, Konsolidierung und ausdrücklicher Abnahme dieses Verständnisabgleichs wird eine eigene kleinschrittige System-09-Teil-Roadmap entworfen und zur Abnahme vorgelegt.

System 09 erzeugt automatische Zuweisungen und schwarze `X`, bewertet seine Optimierungsziele strukturiert und kann einen erzeugten Vorschlag atomar in den aktuellen Entwurf übernehmen. Ausführliche deutsche Konflikterklärungen und Lösungsvorschläge bleiben System 10. Allgemeine manuelle Bearbeitung, bedienbare Sperren und bestätigte manuelle Abweichungen bleiben System 11. Unveränderliche Planversionen und Abnahme bleiben System 12.

Alle Beispiele und späteren Tests verwenden ausschließlich erfundene Personen und synthetische Planungsdaten. Für diesen Fragenkatalog werden keine echten Namen, Dienstpläne, Abwesenheitsdaten oder sonstigen personenbezogenen Informationen benötigt.

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
- `docs/roadmaps/completed/S04_WORK_LOCATIONS_SHIFT_TYPES_SPLIT_SHIFTS_ROADMAP.md`
- `docs/roadmaps/completed/S05_STAFFING_DEMAND_ROADMAP.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_ROADMAP.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_QUESTIONS.md`
- `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md`
- `docs/roadmaps/completed/S08_SCHEDULE_MODEL_QUESTIONS.md`
- `docs/roadmaps/completed/S08_SCHEDULE_MODEL_ROADMAP.md`
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/decisions/S04_SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md`
- `docs/decisions/S07_RULE_CATALOG_EXAMPLES.md`
- aktueller Codebestand der Planungsmomentaufnahme, des Regelkatalogs, des Planmodells und des noch leeren Planning-Testprojekts

## Bereits bestätigter Ausgangsrahmen

### Fachliche Eingabe

- Ein Planungslauf verwendet ausschließlich eine ausdrücklich vorbereitete, unveränderliche `PlanningInputSnapshot`.
- Der Zeitraum umfasst genau 21 Kalendertage von Montag bis zum dritten Sonntag.
- Die Momentaufnahme enthält aktive Mitarbeitende, verwendete Typfassungen, strukturierte Einsatzfreigaben, Dienstkatalog, tatsächliche Bedarfsplätze, `U`, `K`, rote `X`, geschützte Typ1-Zuweisungen, alle 28 Regeldefinitionen der Katalogversion 1, Laufoptionen und bis zu sieben Tage Vorgeschichte.
- Geänderte Eingangsdaten verändern eine vorbereitete Momentaufnahme nicht stillschweigend. Ein veralteter Vorbereitungsstand muss vor einer Generierung bewusst aktualisiert werden.
- Typ1 wird niemals automatisch eingeplant. Vorgetragene Typ1-Zuweisungen einschließlich `B`, `D` oder `Spr` bleiben geschützt.
- Ein normaler Bedarfsplatz wird vollständig besetzt oder bleibt vollständig ungedeckt. Nur der bestätigte `Spr`-Sonderfall darf eine Teildeckung erzeugen.
- Automatische Überbesetzung ist verboten.
- Jeder nach der Generierung freie Tag, der nicht bereits `U`, `K` oder rotes `X` ist, erhält ein von der Generierung erzeugtes schwarzes `X`.

### Optimierung

- Die Reihenfolge lautet zwingende Regeln, ungedeckten Bedarf minimieren, hohe weiche Regeln, mittlere weiche Regeln, niedrige weiche Regeln und zuletzt stabile faire Verteilung.
- Innerhalb derselben Priorität wird zuerst die Anzahl der verletzten Regelfälle minimiert. Bei derselben Regel wird anschließend das Ausmaß der Verletzung minimiert. Unterschiedliche Regeln derselben Priorität erhalten zunächst keine versteckten Punktwerte oder Unterprioritäten.
- Ein späteres frei konfigurierbares Punktesystem wird als möglicher Erweiterungsgedanke festgehalten, gehört aber nicht zur ersten Umsetzung.
- `Spr`, `D` und Spätdienste gelten als ungünstige Einsätze. `Spr` und `D` sollen am stärksten vermieden werden. Früh- und Spätdienste sollen zwischen dafür geeigneten Personen fair verteilt werden.
- Die bestätigte fachliche Notfallwirkung von `Spr` und die Bedarfsdeckung bleiben wichtiger als seine Einordnung als ungünstiger Einsatz.
- Nicht-AH-Personen werden zuerst geplant. AH darf danach nur verbleibende zulässige Lücken füllen und keine bereits geplante Nicht-AH-Person verdrängen.

### Lauf und Größenordnung

- Als unverbindliche Orientierung werden ungefähr 17 bis 20 aktive Mitarbeitende angenommen, darunter bis zu fünf AH-Personen. Diese Zahlen sind keine Fachgrenze und dürfen nicht als feste Obergrenze programmiert werden.
- Ein produktiver Lauf darf in der ersten Fassung bis ungefähr zwei Minuten benötigen. Eine kürzere Laufzeit ist erwünscht. Spätere Performanceverbesserungen bleiben ausdrücklich möglich.
- Derselbe Input soll mit derselben Solver-Version und denselben Einstellungen reproduzierbar denselben Plan liefern.
- Eine spätere bewusste Funktion für einen alternativen gleichwertigen Plan soll architektonisch möglich bleiben, wird in System 09 aber nicht implementiert und nicht sichtbar angeboten.
- Ein innerhalb der Zeitgrenze gefundener zulässiger, aber noch nicht nachweislich optimaler Vorschlag darf angezeigt werden. Er wird eindeutig gekennzeichnet und ersetzt den aktuellen Entwurf nur nach bewusster Übernahme.
- Ein technisch abgebrochener oder fehlerhafter Lauf verändert den vorhandenen Entwurf nicht.

### Sichtbarer Mindestumfang

- System 09 erhält einen kleinen, vollständig bedienbaren Ablauf zum Starten und Abbrechen der Generierung, zur Anzeige des Arbeits- und Ergebnisstatus und zur bewussten Übernahme eines Vorschlags.
- Die Oberfläche zeigt mindestens, ob ein Ergebnis optimal, zulässig aber nicht nachweislich optimal, abgebrochen oder technisch fehlgeschlagen ist und wie viele Bedarfsplätze beziehungsweise Bedarfszeiten offen bleiben.
- Ausführliche Ursachen, Lösungsvorschläge und allgemeine manuelle Planänderungen werden nicht vorgezogen.

## A – Startvoraussetzungen und Laufgrenze

### A-01 – Aktuelle Planungsmomentaufnahme

Darf eine Generierung ausschließlich mit einer vorhandenen und gegenüber den aktuellen Eingaben unveränderten Planungsmomentaufnahme starten?

**Empfehlung:** Ja. Fehlt die Vorbereitung oder ist sie veraltet, startet kein Lauf. Die Oberfläche verweist auf „Planung vorbereiten“ beziehungsweise „Vorbereitung aktualisieren“. Ein alter Entwurf bleibt unverändert.

Antwort: deine empfehlung

### A-02 – Unbekannte oder nicht übersetzte Regel

Wie reagiert System 09, wenn die Momentaufnahme eine unbekannte Regelkennung, eine unbekannte Katalogversion oder eine bekannte, aber noch nicht technisch übersetzte Regel enthält?

**Bereits verbindliche Grundlage:** Die Generierung wird sichtbar blockiert. Keine Regel darf stillschweigend ignoriert werden. Der vorhandene Entwurf bleibt unverändert.

Offen ist nur noch, welche strukturierten Fehlerwerte System 09 an System 10 und die minimale System-09-Anzeige übergibt.

Antwort: Unbekannte Katalogversionen, unbekannte Regelkennungen und bekannte Regeln ohne technische Übersetzung erhalten getrennte stabile Fehlercodes. Planning prüft vor dem Solverstart alle Regeln und meldet sämtliche gefundenen Probleme in stabiler Reihenfolge mit Katalogversion und Regelkennung. Der Solver wird nicht gestartet und der vorhandene Entwurf bleibt unverändert. Application zeigt bereits in System 09 eine kurze verständliche deutsche Meldung. Ausführliche Ursachen und Lösungsvorschläge bleiben System 10.

### A-03 – Typ1-Voraussetzung vor dem Lauf

Soll System 09 die bereits in System 08 geprüfte Typ1-Wochenbereitschaft zusätzlich als eigene Schutzprüfung am Planning-Eingang verifizieren?

**Empfehlung:** Ja. Application prüft den Ablauf, Planning schützt zusätzlich seinen öffentlichen Modulvertrag. Die zweite Prüfung verwendet dieselben strukturierten Werte und erfindet keine abweichende Typ1-Regel.

Antwort: ja deine empfehlung

### A-04 – Widersprüchliche geschützte Zuweisung

Was geschieht, wenn eine geschützte Typ1- oder spätere gesperrte Zuweisung einer Hard Rule oder den Bedarfsdaten der Momentaufnahme widerspricht?

**Empfehlung:** Der Lauf startet nicht. Die widersprüchliche Zuweisung wird weder entfernt noch verändert. Das Ergebnis nennt mindestens Zuweisungskennung, Regelkennung und den strukturierten Ablehnungsgrund.

Antwort: deine empfehlung

### A-05 – Laufzeitgrenze der ersten Fassung

Wie wird die bestätigte Laufzeit von ungefähr zwei Minuten umgesetzt?

**Empfehlung:** Für die erste Fassung besitzt der produktive Lauf eine intern zentral konfigurierte Obergrenze von 120 Sekunden. Sie ist noch keine frei bearbeitbare Benutzereinstellung. Kürzere Testgrenzen werden über injizierte Laufparameter geprüft.

Antwort: Bis ungefähr zwei Minuten sind für die erste Fassung akzeptabel. Schneller ist besser; weitere Optimierung darf später erfolgen. Die Zahl ist keine Garantie für beliebig große zukünftige Datenbestände.

### A-06 – Größenordnung und Skalierungsgrenze

Welche Datenmenge muss der erste reale Größen- und Laufzeittest mindestens abbilden?

**Empfehlung:** Ein vollständig synthetischer Drei-Wochen-Fall mit ungefähr 20 aktiven Personen, davon bis zu fünf AH, und einer Bedarfsgröße in der Größenordnung der aktuellen 195 Plätze. Zusätzlich ein bewusst größerer synthetischer Schutzfall, damit keine feste Obergrenze von 20 Personen einprogrammiert wird.

Antwort: 17 bis 20 aktive Mitarbeitende und bis zu fünf AH gelten als unverbindliche Orientierung, nicht als feste fachliche Obergrenze.

### A-07 – Parallele Planungsläufe

Darf für denselben oder einen anderen Zeitraum ein zweiter Lauf gestartet werden, solange eine Generierung läuft?

**Empfehlung:** Nein. Im bestätigten Einzelbenutzerbetrieb läuft höchstens eine Generierung gleichzeitig. Andere Stammdatenansichten dürfen lesbar bleiben, widersprüchliche Aktionen für den betroffenen Entwurf werden gezielt deaktiviert.

Antwort: deine empfehlung

### A-08 – Alternative gleichwertige Planung

Wie weit wird eine spätere Funktion „Alternative erzeugen“ in System 09 vorbereitet?

Antwort:

- Der normale Lauf bleibt reproduzierbar und erzeugt bei identischem Input dasselbe Ergebnis.
- Planning darf intern so gegliedert werden, dass später eine ausdrücklich gewählte Alternativstrategie ergänzt werden kann.
- System 09 implementiert weder eine zweite Suchstrategie noch einen sichtbaren Button noch zufällige Varianten.
- Es wird keine ungenutzte allgemeine Plugin- oder Strategie-Infrastruktur ohne konkreten aktuellen Zweck gebaut.

## B – Laufstatus, Abbruch und bewusste Übernahme

### B-01 – Ergebnisstatus

Welche fachlich-technischen Zustände muss ein Planungslauf mindestens unterscheiden?

**Empfehlung:** `Optimal`, `FeasibleNotProvenOptimal`, `Cancelled`, `TimedOutWithoutFeasibleResult`, `BlockedByInput`, `UnsupportedRule` und `TechnicalFailure`. Die genauen englischen Codebezeichner werden in der Roadmap festgelegt; sichtbare deutsche Texte bleiben in Application/Desktop.

Antwort: deine empfehlung

### B-02 – Zulässiger Zwischenstand bei Zeitgrenze

Was geschieht, wenn innerhalb von 120 Sekunden ein zulässiger Plan gefunden wurde, dessen Optimalität aber noch nicht bewiesen ist?

Antwort:

- Der Vorschlag darf angezeigt werden.
- Er wird als „zulässig, aber noch nicht nachweislich optimal“ gekennzeichnet.
- Er ersetzt den aktuellen Entwurf erst nach einer bewussten Übernahme durch die Service-Leitung.
- Zwingende Regeln müssen trotzdem vollständig eingehalten sein; die Kennzeichnung erlaubt keine Hard-Rule-Verletzung.

### B-03 – Optimaler Vorschlag und Übernahme

Soll auch ein nachweislich optimaler Vorschlag zunächst als Ergebnisvorschau erscheinen und erst nach bewusster Übernahme gespeichert werden?

**Empfehlung:** Ja. Damit verwendet jeder erfolgreiche Lauf denselben klaren Ablauf und eine Generierung verändert den Entwurf nicht bereits durch das bloße Starten.

Antwort: deine empfehlung

### B-04 – Abbruch durch die Service-Leitung

Was geschieht bei „Abbrechen“ während eines laufenden Solver-Vorgangs?

**Empfehlung:** Der Lauf endet kontrolliert, meldet keinen Erfolg und verändert weder Entwurf noch vorbereitete Momentaufnahme. Ein bereits intern gefundener Zwischenstand wird nach einem ausdrücklichen Abbruch nicht zur Übernahme angeboten.

Antwort: deine empfehlung

### B-05 – Technischer Fehler

Was geschieht bei einer OR-Tools-Ausnahme, ungültigem Rückgabestatus oder einem unerwarteten Mappingfehler?

**Empfehlung:** `TechnicalFailure` mit stabiler technischer Fehlerkennung. Keine teilweise Übernahme, keine Namen oder vollständigen Planinhalte im Log und kein automatischer Wiederholungsversuch.

Antwort: deine empfehlung. Der technische fehler soll dabei eine hilfe für unsere Fehlerbehebung sein

### B-06 – Veralteter Entwurf vor Übernahme

Was geschieht, wenn sich Entwurfsversion oder Vorbereitung zwischen Start des Laufs und bewusster Übernahme verändert hat?

**Empfehlung:** Die Übernahme wird wegen eines Versionskonflikts abgelehnt. Das Ergebnis darf nicht gegen einen inzwischen veränderten Entwurf gespeichert werden. Ein neuer Lauf benötigt eine aktuelle Vorbereitung.

Antwort: Vor der Übernahme werden Entwurfskennung, Entwurfsversion und Kennung der vorbereiteten Planungsmomentaufnahme erneut geprüft. Stimmen sie nicht mehr mit den Werten beim Start der Generierung überein, wird die Übernahme vollständig abgelehnt. Der aktuelle Entwurf bleibt unverändert und das Ergebnis wird als veraltet gekennzeichnet. Eine automatische Zusammenführung oder automatische Neugenerierung findet nicht statt. Nach einer notwendigen Aktualisierung startet die Service-Leitung die Generierung erneut.

### B-07 – Lebensdauer einer nicht übernommenen Vorschau

Muss ein erzeugter, aber noch nicht übernommener Vorschlag einen App-Neustart überstehen?

**Empfehlung:** Nein. Erst die bewusste atomare Übernahme wird dauerhaft gespeichert. Die unveränderte Planungsmomentaufnahme erlaubt bei Bedarf einen reproduzierbaren neuen Lauf.

Antwort: deine empfehlung

### B-08 – Erneute Generierung

Welche Teile eines vorhandenen Entwurfs ersetzt eine ausdrücklich gestartete und anschließend übernommene Neugenerierung?

Antwort:

- Geschützte Typ1-Zuweisungen und spätere wirksame Einzelsperren bleiben unverändert.
- Nicht gesperrte automatisch erzeugte Zuweisungen und schwarze `X` werden als zusammengehöriges Ergebnis vollständig ersetzt.
- Manuelle Änderungen werden in System 09 noch nicht erzeugt. Ihre spätere Behandlung muss durch die Modulverträge möglich bleiben und wird in System 11 endgültig festgelegt.
- Die Ersetzung erfolgt atomar. Bei Fehler bleibt der bisherige Entwurf vollständig erhalten.

## C – Kandidaten, Zuweisungen und freie Tage

### C-01 – Zulässige normale Kandidaten

Aus welchen Kombinationen darf Planning normale Zuweisungskandidaten bilden?

**Empfehlung:** Nur aus einem aktiven automatisch planbaren Mitarbeiter und genau einem vorhandenen Bedarfsplatz, wenn Tageszustand, Planungsrolle, Einsatzfreigabe, tatsächliche Zeit und alle Strukturvoraussetzungen passen. Planning erfindet weder Plätze noch Zeiten oder Diensttypen.

Antwort: deine empfehlung

### C-02 – Bedarfsplätze mit gleicher Fachinformation

Wie werden mehrere Plätze desselben Bedarfs unterschieden und stabil zugeordnet?

**Empfehlung:** Ausschließlich über die bereits deterministisch vorbereiteten Platzkennungen beziehungsweise Ordnungswerte. Planning erzeugt keine eigene zweite Platznummerierung. Gleichartige Plätze bleiben fachlich gleichwertig, ihre technische Zuordnung ist reproduzierbar.

Antwort: deine empfehlung

### C-03 – Doppeldienst `D`

Wann darf Planning einen Kandidaten für `D` bilden?

**Empfehlung:** Nur für dieselbe Person und denselben Tag aus genau einem vorhandenen Restaurant-Frühdienstplatz und einem vorhandenen Restaurant-Spätdienstplatz, mit der strukturierten Musterfreigabe und ohne Überschneidung. Beide Plätze werden vollständig gedeckt; die Unterbrechung zählt nicht als Arbeitszeit.

Antwort: deine empfehlung

### C-04 – Springer `Spr`

Wann darf Planning einen Kandidaten für `Spr` bilden?

**Empfehlung:** Nur samstags, nur mit aktivierter laufabhängiger Freigabe, nur aus den bestätigten Cafeteria- und Restaurant-Plätzen und nur wenn nach der regulären Nicht-`Spr`-Planung sonst Restaurant-Unterdeckung verbleibt. Der Restaurantabschnitt beginnt am tatsächlichen Wechsel; die frühere Zeit bleibt ungedeckt.

Antwort: deine empfehlung

### C-05 – Typ1-Bedarfswirkung

Wie wirken geschützte Typ1-Zuweisungen auf den verbleibenden Solverbedarf?

**Empfehlung:** Normale Typ1-Zuweisungen sowie Typ1-`D` und Typ1-`Spr` verbrauchen genau ihre gespeicherten Deckungen. Eine Typ1-Zuweisung mit `B` zählt Stunden, verbraucht aber keinen Platz. Planning verändert keine dieser Zuweisungen.

Antwort: deine empfehlung

### C-06 – Manuelle Zusatzbesetzungen

Darf System 09 selbst `ManualAdditional`-Zuweisungen erzeugen oder vorhandene manuelle Zusatzbesetzungen zur Verbesserung seiner Ziele verwenden?

**Empfehlung:** Nein. Automatische Überbesetzung bleibt ausgeschlossen. Die aktive Erzeugung und Bestätigung manueller Zusatzbesetzungen gehört zu System 11.

Antwort: deine empfehlung

### C-07 – Schwarzes `X`

Welche Tage erhalten nach einer übernommenen Generierung ein schwarzes `X`?

Antwort:

- Jeder Kalendertag einer aktiven Person ohne Arbeitszuweisung, `U`, `K` oder rotes `X` erhält genau ein schwarzes `X`.
- Das schwarze `X` gilt ganztägig, reduziert das Wochen-Soll nicht und ist Teil des erzeugten Entwurfs.
- Bei einer bewussten Neugenerierung darf es neu verteilt werden, solange es nicht in einem späteren System ausdrücklich geschützt wurde.

### C-08 – Vollständig abwesende Woche

Erhält eine Person in einer vollständig durch `U` oder `K` belegten Woche zusätzlich schwarze `X`?

**Empfehlung:** Nein. Vorhandene Tageskennzeichen bleiben die alleinige sichtbare und fachliche Tagesinformation. Schwarze `X` werden nur für ansonsten leere freie Tage erzeugt.

Antwort: deine empfehlung

### C-09 – Inaktive Personen

Erzeugt System 09 Zuweisungen oder schwarze `X` für Personen, die nicht in der vorbereiteten aktiven Mitarbeitendenliste enthalten sind?

**Empfehlung:** Nein. Historische oder nachträglich deaktivierte Personen werden nicht eigenständig nachgeladen. Planning arbeitet ausschließlich mit der Momentaufnahme.

Antwort: deine empfehlung

### C-10 – Ein Arbeitstag pro Person

Bleibt `D` oder `Spr` trotz mehrerer Arbeitsabschnitte genau eine Tageszuweisung und ein Arbeitstag?

**Bereits bestätigte Grundlage:** Ja. Eine Person erhält pro Kalendertag höchstens eine normale Zuweisung oder genau ein zusammengesetztes Muster.

Antwort: deine empfehlung

## D – Vollständige Regelübersetzung

### D-01 – Übersetzungsverzeichnis

Soll System 09 ein ausdrücklich prüfbares Verzeichnis besitzen, das jede automatische Regelkennung genau einem Übersetzer, einer Vorprüfung, einem Optimierungsziel oder einem reinen Berichtshinweis zuordnet?

**Empfehlung:** Ja. Ein Vollständigkeitstest vergleicht das Verzeichnis mit allen 28 Definitionen der unterstützten Katalogversion. Keine `switch`-Standardverzweigung darf unbekannte Regeln akzeptieren.

Antwort: deine empfehlung

### D-02 – Strukturregeln

Werden die sechs Strukturregeln nur vor dem Solver geprüft oder zusätzlich durch die Modellform selbst unmöglich gemacht?

**Empfehlung:** Beides, soweit anwendbar. Ungültiger Input wird früh strukturiert abgelehnt. Das Modell verhindert außerdem unbekannte Kandidaten, Doppelzuweisungen, Überschneidungen, belegte Sperrtage und freie normale Teildeckungen konstruktiv.

Antwort: deine empfehlung

### D-03 – Automatische Hard Rules

Muss für jede der elf automatischen Hard Rules mindestens ein Test nachweisen, dass der Solver lieber Bedarf ungedeckt lässt, als die Regel zu verletzen?

**Empfehlung:** Ja. Zusätzlich erhält jede Regel erfüllte, verletzte und nicht anwendbare Fälle aus den bestätigten gemeinsamen Szenarien.

Antwort: deine empfehlung

### D-04 – Weiche Regeln

Wie wird nachgewiesen, dass jede weiche Regel tatsächlich das Solverergebnis beeinflusst und nicht nur nachträglich bewertet wird?

**Empfehlung:** Für jede weiche Regel existiert ein synthetisches Paar zulässiger Pläne, bei dem nur die betreffende Zielerfüllung unterscheidet. Der Solver muss den fachlich besseren Plan wählen.

Antwort:  deine empfehlung

### D-05 – Berichtshinweise für AH

Werden `AH_WEEKLY_LOW_NOTICE` und `AH_WEEKLY_HIGH_NOTICE` bereits in System 09 strukturiert ausgewertet?

**Empfehlung:** Ja. System 09 liefert Regelkennung, betroffene technische Personenkennung, Woche und Minutenwert. Die verständliche deutsche Ausformulierung bleibt System 10.

Antwort: deine empfehlung

### D-06 – Fehlende Vorgeschichte

Wie wirkt unvollständige Vorgeschichte auf `MAX_CONSECUTIVE_WORKDAYS`?

**Bereits bestätigte Grundlage:** Die Generierung bleibt erlaubt und kennzeichnet die Regel strukturiert als nicht vollständig prüfbar. Vorhandene Geschichte wird vollständig berücksichtigt; fehlende Tage werden weder als Arbeit noch als frei erfunden.

Antwort: deine empfehlung

### D-07 – Gemeinsame fachliche Bewertung

Soll der erzeugte Plan nach der Rückübersetzung nochmals unabhängig mit den gemeinsamen fachlichen Regeln bewertet werden, bevor er zur Übernahme angeboten wird?

**Empfehlung:** Ja. Solverbedingungen und fachliche Nachprüfung verwenden dieselben Regeldefinitionen und Beispielszenarien. Eine nachträglich entdeckte Hard- oder Strukturverletzung macht das Ergebnis technisch ungültig und verhindert die Übernahme.

Antwort: deine empfehlung

### D-08 – Aussagegrenze

Soll das Planungsergebnis ausdrücklich festhalten, dass nur der interne Katalog der ersten Fassung geprüft wurde?

**Empfehlung:** Ja. System 09 behauptet keine vollständige gesetzliche, tarifliche oder sonstige externe Regelkonformität.

Antwort: deine empfehlung

## E – Bedarfsdeckung, Prioritäten und faire Verteilung

### E-01 – Messung ungedeckten Bedarfs

Was wird innerhalb der Bedarfsdeckungsstufe zuerst minimiert: Zahl ungedeckter Plätze, ungedeckte Minuten oder benötigte Mitarbeiterstunden?

Beispiel:

- Plan A lässt einen siebenstündigen Cafeteria-Platz ungedeckt.
- Plan B lässt zwei dreistündige Restaurant-Spätdienstplätze ungedeckt.

Plan A hat weniger offene Plätze, Plan B aber weniger offene Mitarbeiterstunden.

**Empfehlung:** Zuerst ungedeckte Mitarbeiterstunden beziehungsweise Minuten minimieren, danach bei gleicher offener Zeit die Zahl vollständig ungedeckter Plätze. Dadurch bleiben Personal- und Stundenbedarf getrennt sichtbar, ohne einen langen Platz allein wegen seiner Stückzahl gleich wie einen kurzen Platz zu behandeln.

Antwort: deine empfehlung

### E-02 – Gewichtung verschiedener Einsatzorte und Dienste

Sind ungedeckte Minuten in Cafeteria und Restaurant sowie in Früh- und Spätdiensten innerhalb der Bedarfsdeckungsstufe gleich wichtig?

**Empfehlung:** Ja. Eine abweichende fachliche Dringlichkeit wurde bisher nicht bestätigt. Einsatzort- oder Dienstprioritäten dürfen nicht aus Farbe, Namen oder Kürzel erfunden werden.

Antwort: deine empfehlung

### E-03 – Bewertung der `Spr`-Teildeckung

Wie wird die verbleibende Teilunterdeckung eines Restaurant-Spätdienstes bewertet?

**Empfehlung:** Jede vor dem tatsächlichen Wechsel ungedeckte Minute bleibt als ungedeckte Mitarbeiterzeit in derselben Bedarfsdeckungsstufe enthalten. Nur die wirklich abgedeckte Zeit reduziert die Lücke.

Antwort: Wir bleiben bei der bereits bestätigten Regel. Jede Minute vor dem tatsächlichen Wechsel bleibt sichtbar ungedeckt und zählt als ungedeckte Mitarbeiterzeit. Nur die wirklich abgedeckte Zeit reduziert die Lücke.

### E-04 – Strikte Prioritätsstufen

Wie wird technisch abgesichert, dass viele niedrigere Ziele niemals ein höheres Ziel überstimmen?

**Empfehlung:** Getrennte lexikografische Optimierungsläufe oder mathematisch nachgewiesene dominante Grenzen. Frei geschätzte Strafpunkte ohne Dominanznachweis sind unzulässig.

Antwort:  deine empfehlung

### E-05 – Regeln innerhalb derselben Priorität

Wie werden mehrere Regeln derselben Priorität verglichen?

Antwort:

- Zuerst wird die Anzahl verletzter Regelfälle minimiert.
- Bei derselben Regel wird anschließend das Ausmaß der Verletzung minimiert, beispielsweise fehlende Minuten oder zusätzliche `D`.
- Unterschiedliche Regeln derselben Priorität erhalten in der ersten Fassung keine versteckten Punktwerte und keine heimliche Unterpriorität.
- Unterschiedliche Regelarten derselben Priorität werden nicht über Stunden, Punkte oder andere Einheiten gegeneinander umgerechnet. Bleiben sie nach der Fallzählung gleichwertig, entscheiden erst die nachfolgenden bestätigten Stufen.
- Ein späteres ausdrücklich bestätigtes Punktesystem bleibt als Erweiterungsgedanke dokumentiert.

### E-06 – Noch gleichwertige Zielkombinationen

Was geschieht, wenn zwei Pläne innerhalb einer Prioritätsstufe unterschiedliche Regelarten verletzen, aber nach der bestätigten Zählung weiterhin gleichwertig sind?

**Empfehlung:** Keine nachträgliche fachliche Rangfolge erfinden. Beide bleiben für diese Stufe gleichwertig; nachfolgende bestätigte Stufen und zuletzt die stabile faire Verteilung entscheiden.

Antwort: Bestätigt wie empfohlen. Insbesondere entsteht keine nachträgliche Rangfolge zwischen verschiedenen Regelarten derselben Priorität.

### E-07 – Bedeutung „ungünstige Dienste“

Welche Einsätze berücksichtigt die Stabilitätsregel als ungünstig?

Antwort:

- Spätdienst und `Spr` gelten als ungünstige Einsätze.
- `D` wird als besonders belastendes zusammengesetztes Muster berücksichtigt.
- `Spr` und `D` sollen am stärksten vermieden werden.
- Früh- und Spätdienste sollen zwischen den jeweils geeigneten Personen fair verteilt werden.
- Ein Frühdienst ist nicht pauschal unerwünscht, darf aber nicht dauerhaft einseitig derselben Personengruppe zugewiesen werden.
- Es entsteht kein zusätzliches verborgenes Gewicht: `Spr` wird durch seine Notfallregel, `D` durch Wochenmaximum und Minimierungsregel vermieden; Fairness entscheidet nur zwischen ansonsten gleichwertigen Möglichkeiten.

### E-08 – Verhältnis von Bedarfsdeckung und ungünstigem Einsatz

Darf Bedarf ungedeckt bleiben, nur um einen zulässigen `D`, Spätdienst oder bestätigten Notfall-`Spr` zu vermeiden?

**Empfehlung:** Nein. Innerhalb der Hard Rules steht Bedarfsdeckung über weichen Vermeidungs- und Fairnesszielen. `Spr` bleibt zusätzlich an seine bestätigte Notfallbedingung gebunden und darf nicht nur zur Stundenauffüllung verwendet werden.

Antwort: deine empfehlung

### E-09 – Verteilung von `D` und `Spr`

Wie wird zwischen mehreren für `D` oder `Spr` geeigneten Personen verteilt?

**Empfehlung:** Zuerst gelten Einsatzfreigaben, Wochenkorridore und alle höheren Regeln. Unter danach gleichwertigen Personen werden bisherige `D`- beziehungsweise `Spr`-Anzahlen innerhalb der drei Wochen möglichst ausgeglichen. Es wird keine längerfristige Belastungshistorie verwendet.

Antwort:  deine empfehlung

### E-10 – Fairness bei Früh- und Spätdiensten

Wird Fairness nur anhand absoluter Dienstzahlen oder relativ zu den tatsächlich möglichen Einsätzen einer Person bewertet?

**Empfehlung:** Nur unter vergleichbar geeigneten Personen. Eine Person ohne Freigabe oder mit vielen Sperrtagen darf nicht als unfair bevorzugt erscheinen. Verglichen werden ausschließlich zulässige Kandidaten innerhalb des aktuellen Drei-Wochen-Zeitraums.

Antwort: deine empfehlung

### E-11 – Nicht-AH- und AH-Phase

Soll die Nicht-AH-Lösung nach Abschluss ihrer Optimierung vollständig eingefroren werden, bevor AH verbleibende Lücken füllt?

**Empfehlung:** Ja. AH verdrängt keine Nicht-AH-Zuweisung. Die AH-Phase darf nur noch offene zulässige Plätze verwenden und innerhalb dieser Grenze ihr Zehn-Stunden-Ziel optimieren.

Antwort:  deine empfehlung

### E-12 – Mehrere AH-Personen

Wie wird bei bis zu ungefähr fünf AH-Personen verteilt, wenn nicht alle ihr Zehn-Stunden-Ziel erreichen können?

**Empfehlung:** Innerhalb der nachgelagerten AH-Phase zunächst die Abweichung aller AH-Personen vom Zehn-Stunden-Ziel minimieren und danach die verbleibende Abweichung möglichst gleichmäßig verteilen. Keine Person wird aufgrund von Name oder Typcode bevorzugt.

Antwort: deine empfehlung

### E-13 – Reproduzierbarer letzter Gleichstand

Wie entscheidet Planning, wenn nach allen fachlichen Zielen weiterhin mehrere Pläne gleichwertig sind?

Antwort:

- Ein stabiler technischer Tie-Breaker verwendet ausschließlich unveränderliche technische Kennungen und stabile Ordnungen.
- Namen, sichtbare Typcodes und aktuelle Reihenfolge in der Oberfläche sind keine fachlichen Bevorzugungskriterien.
- Derselbe Input, dieselbe Solver-Version und dieselben Einstellungen erzeugen denselben Plan.

## F – Planungsergebnis und Speicherung

### F-01 – Inhalt des Planning-Ergebnisses

Welche Informationen muss Planning mindestens strukturiert zurückgeben?

**Empfehlung:** Snapshot-Kennung, Entwurfskennung und erwartete Version, Laufstatus, Solver-Version und Einstellungen, Laufzeit, automatische Zuweisungen, schwarze `X`, vollständige und teilweise offene Bedarfe, Zielwerte je Optimierungsstufe, Regelbewertungen, Hinweise und technische Fehlercodes.

Antwort: deine empfehlung

### F-02 – Keine deutschen Sätze aus Planning

Darf Planning sichtbare deutsche Meldungen formulieren?

**Bereits verbindliche Grundlage:** Nein. Planning liefert stabile Codes und fachliche Parameter. Application und später System 10 formulieren verständliche deutsche Texte.

Antwort: deine empfehlung

### F-03 – Rückübersetzung in das Planmodell

Müssen alle Solverentscheidungen vor der Rückgabe in vorhandene Domain- und Application-Werte zurückübersetzt und validiert werden?

**Empfehlung:** Ja. Keine OR-Tools-Variable und kein Solverobjekt überschreitet die Planning-Grenze. Zuweisungen verwenden vorhandene Personen-, Platz-, Dienst- und Musterkennungen.

Antwort: deine empfehlung

### F-04 – Kennungen automatisch erzeugter Zuweisungen

Wie werden Kennungen für automatisch erzeugte Zuweisungen gebildet?

**Empfehlung:** Deterministisch aus Planungssnapshot und fachlicher Zielkombination oder über eine klar gekapselte Kennungserzeugung bei Übernahme. Die gewählte Lösung muss Wiederholbarkeit, Eindeutigkeit und atomare Speicherung sichern und wird in der Roadmap begründet.

Antwort: deine empfehlung

### F-05 – Atomare Übernahme

Welche Daten gelten bei der Übernahme als eine unteilbare Änderung?

**Empfehlung:** Entfernen der ersetzbaren alten Generierung, Speichern aller neuen automatischen Zuweisungen, Speichern aller schwarzen `X`, Aktualisieren der Entwurfsversion und Speichern der Laufmetadaten erfolgen in einer Transaktion. Kein Teilergebnis wird sichtbar.

Antwort: Die Übernahme eines Generierungsergebnisses erfolgt in genau einer atomaren Transaktion. Innerhalb dieser Transaktion werden Entwurfskennung, Entwurfsversion und Planungsmomentaufnahme erneut geprüft, alle ersetzbaren alten automatischen Zuweisungen und schwarzen X entfernt, alle neuen Zuweisungen und schwarzen X gespeichert, die Laufmetadaten festgehalten und die Entwurfsversion erhöht. Geschützte Typ1-Zuweisungen und spätere wirksame Sperren bleiben unverändert. Scheitert eine Prüfung oder ein Schreibvorgang, wird die gesamte Übernahme zurückgenommen und der bisherige Entwurf bleibt vollständig erhalten.

### F-06 – Laufmetadaten

Welche Solverinformationen werden mit dem übernommenen Entwurf gespeichert?

**Empfehlung:** Mindestens Solvername und -version, relevante deterministische Einstellungen, Zeitgrenze, tatsächliche Laufzeit, Ergebnisstatus, Snapshot-Kennung und Zielwerte. Es werden keine OR-Tools-Objekte serialisiert.

Antwort: deine empfehlung

### F-07 – Frühere Generierungsergebnisse

Werden frühere automatisch erzeugte Vorschläge als eigene Historie aufbewahrt?

**Empfehlung:** Nein. System 09 verwaltet den aktuellen Entwurf, keine Vorschlagshistorie. Unveränderliche fachliche Versionen entstehen erst durch System 12. Technische Logs ersetzen keine Planhistorie.

Antwort:  deine empfehlung

### F-08 – Bedarf und Snapshot bleiben unverändert

Darf die Übernahme eines Ergebnisses Bedarfe, Einsatzfreigaben, Tageskennzeichen der Service-Leitung, Regeln oder die verwendete Momentaufnahme verändern?

**Bereits verbindliche Grundlage:** Nein. Gespeichert werden ausschließlich das neue Generierungsergebnis und seine Metadaten gegen den vorbereiteten Input.

Antwort: Planning und die Übernahme eines Generierungsergebnisses dürfen ausschließlich automatische Zuweisungen, schwarze X, Laufmetadaten und die Entwurfsversion verändern. Bedarfe, Dienstkatalog, Mitarbeitende, Typfassungen, Einsatzfreigaben, U, K, rote X, geschützte Typ1-Zuweisungen, Regeln, Laufoptionen und die verwendete `PlanningInputSnapshot` bleiben unverändert. Kann mit diesen Eingaben kein vollständig gedeckter Plan erzeugt werden, bleibt der betreffende Bedarf sichtbar ungedeckt. Eine spätere Änderung der Eingaben erzeugt durch eine bewusste Aktualisierung einen neuen unveränderlichen Snapshot. Dieser ersetzt die vorherige technische Vorbereitung; System 09 führt keine Snapshot-Historie. Unveränderliche fachliche Planversionen entstehen erst in System 12.

## G – Sichtbarer Ablauf in „Dienstplan SER“

### G-01 – Startaktion

Wann ist „Plan erzeugen“ verfügbar?

**Empfehlung:** Nur bei vorhandenem Entwurf, aktueller Vorbereitung, erfüllter Typ1-Voraussetzung und ohne laufenden widersprüchlichen Vorgang. Ein deaktivierter Button besitzt einen verständlichen sichtbaren Grund.

Antwort: deine empfehlung

### G-02 – Arbeitszustand

Was wird während der Generierung angezeigt?

**Empfehlung:** Eindeutiger Arbeitszustand, verstrichene Zeit, bestätigte maximale Laufzeit und „Abbrechen“. Keine erfundene Prozentanzeige, solange der Solver keinen belastbaren Fortschrittswert liefert. Die WPF-Oberfläche bleibt reaktionsfähig.

Antwort:  deine empfehlung

### G-03 – Ergebnisvorschau

Was zeigt System 09 vor der bewussten Übernahme?

**Empfehlung:** Ergebnisstatus, Laufzeit, Zahl vollständig und teilweise offener Bedarfe, offene Mitarbeiterstunden, Zahl erzeugter Zuweisungen und Hinweis, dass ausführliche Erklärungen erst im nachfolgenden Konfliktsystem ergänzt werden.

Antwort: deine empfehlung

### G-04 – Übernahme und Verwerfen

Welche Aktionen gibt es für einen zulässigen Vorschlag?

**Empfehlung:** „Plan übernehmen“ und „Vorschlag verwerfen“. Verwerfen ändert den Entwurf nicht. Übernehmen prüft Entwurfs- und Snapshot-Version erneut und speichert atomar.

Antwort: deine empfehlung

### G-05 – Darstellung des übernommenen Plans

Wie weit wird der erzeugte Plan bereits sichtbar gemacht, obwohl die vollständigen Planansichten erst System 11 gehören?

**Empfehlung:** Die bestehende 21-Tage-Ansicht zeigt übernommene Dienstkennzeichnungen und schwarze `X` lesbar an. Zusätzliche Einsatzortansicht, allgemeiner Bearbeitungsmodus und vollständige Konfliktmarkierung bleiben System 10/11.

Antwort:  deine empfehlung

### G-06 – Minimaler Fehlertext

Welche Fehler müssen bereits vor System 10 verständlich angezeigt werden?

**Empfehlung:** Fehlende oder veraltete Vorbereitung, nicht unterstützte Regel, blockierende geschützte Zuweisung, Abbruch, Zeitablauf ohne zulässiges Ergebnis, Versionskonflikt und technischer Fehler. System 09 nennt den nächsten möglichen Bedienungsschritt, aber noch keine umfassende Konfliktanalyse.

Antwort: deine empfehlung

### G-07 – Alternatives Ergebnis

Wird die spätere Alternative bereits sichtbar angedeutet?

**Empfehlung:** Nein. Kein deaktivierter oder funktionsloser Button. Die Erweiterbarkeit wird nur in Architektur und Roadmap festgehalten.

Antwort: deine empfehlung

### G-08 – Manuelles Sichtgate

Benötigt System 09 vor seinem Abschluss eine sichtbare WPF-Abnahme?

**Empfehlung:** Ja. Start, Arbeitszustand, Abbruch, Ergebnisstatus, Vorschau, Übernahme, schwarze `X`, Dienstanzeige und Fehlerzustände werden mit ausschließlich synthetischen Daten sichtbar geprüft.

Antwort: deine empfehlung

## H – Teststrategie für besonders hohe Korrektheit

### H-01 – Gemeinsame Regelszenarien

Werden die abgenommenen System-07-Beispiele unverändert als gemeinsame Ausgangsszenarien für Fachbewertung und Solverübersetzung verwendet?

**Empfehlung:** Ja. Wo ein Beispiel für das konkrete Planmodell präzisiert werden muss, bleibt die fachliche Aussage gleich und die Ergänzung wird dokumentiert und fachlich abgenommen.

Antwort: deine empfehlung

### H-02 – Mindestmatrix je Regel

Welche Fälle benötigt jede automatische Regel mindestens?

**Empfehlung:** Erfüllt, verletzt, Grenzwert, nicht anwendbar, relevante Kombination mit anderen Regeln, richtige Regelkennung und richtige Prioritätsstufe. `MAX_CONSECUTIVE_WORKDAYS` erhält zusätzlich vollständig und unvollständig prüfbare Vorgeschichte.

Antwort: deine empfehlung

### H-03 – Unabhängiger Kleinstfall-Referenzlöser

Soll für sehr kleine synthetische Fälle ein einfacher erschöpfender Referenzlöser alle zulässigen Kombinationen prüfen und das optimale Ergebnis mit CP-SAT vergleichen?

**Empfehlung:** Ja. Der Referenzlöser bleibt ausschließlich Testcode, unterstützt bewusst nur kleine Fälle und dupliziert nicht die produktive OR-Tools-Implementierung. Er ist ein besonders starker Nachweis gegen fehlerhafte Modellübersetzungen.

Antwort: deine empfehlung

### H-04 – Invariantenbasierte generierte Tests

Sollen zusätzlich viele kleine synthetische Eingaben mit festen Startwerten erzeugt und auf allgemeine Invarianten geprüft werden?

**Empfehlung:** Ja. Geprüft werden mindestens keine Hard-Rule-Verletzung, keine Überbesetzung, keine Doppelzuweisung, vollständige normale Deckung oder vollständige Lücke, korrekte schwarze `X` und deterministische Wiederholung.

Antwort: deine empfehlung

### H-05 – Mutationsnachweise

Soll die Roadmap kontrollierte Fehlermutationen vorsehen, bei denen jeweils eine wichtige Solverbedingung vorübergehend entfernt oder umgekehrt wird und die Tests nachweislich fehlschlagen müssen?

**Empfehlung:** Ja, mindestens für Überbesetzung, Tagesblockade, Wochenmaximum, Nicht-AH-vor-AH, Prioritätsdominanz und `Spr`-Notfallgrenze. Die Mutation wird anschließend vollständig entfernt.

Antwort: deine empfehlung

### H-06 – Prioritäts-Gegentests

Benötigt jede Grenze zwischen zwei Optimierungsstufen einen Fall, in dem viele niedrigere Verbesserungen bewusst gegen genau eine höhere Verschlechterung antreten?

**Empfehlung:** Ja. Der höhere Wert muss immer gewinnen. Damit wird die Hierarchie tatsächlich bewiesen und nicht nur aus gewählten Gewichten angenommen.

Antwort:  deine empfehlung

### H-07 – Deckungs- und Stundenfälle

Welche Bedarfsfälle werden ausdrücklich geprüft?

**Empfehlung:** Vollständig deckbar, teilweise insgesamt deckbar, vollständig ungedeckter normaler Platz, unterschiedliche tatsächliche Dauern, von Standardzeiten abweichende Bedarfe, mehrere gleichartige Plätze, keine Überbesetzung und ausschließlich `Spr` als Teildeckung.

Antwort: deine empfehlung

### H-08 – Sonderrollen und Muster

Welche kombinierten Fälle werden ausdrücklich geprüft?

**Empfehlung:** Typ1 normal, Typ1-`B`, Typ1-`D`, Typ1-`Spr`, fehlende Typ1-Voraussetzung, mehrere AH-Personen, AH unter sechs, über zehn und an zwölf Stunden, `D`-Verteilung, `Spr` mit und ohne Laufoption sowie reguläre Besetzung vor `Spr`.

Antwort: deine empfehlung

### H-09 – Schwarze `X`

Welche schwarzen-X-Fälle werden geprüft?

**Empfehlung:** Jeder sonst leere Tag erhält genau ein schwarzes `X`; keine zusätzlichen `X` auf `U`, `K`, rotem `X` oder Arbeit; Neugenerierung verteilt ersetzbare `X` neu; freie-Tage-Regeln verwenden rote und schwarze `X` korrekt.

Antwort: deine empfehlung

### H-10 – Wiederholbarkeit

Wie wird reproduzierbares Verhalten nachgewiesen?

**Empfehlung:** Derselbe Snapshot wird mehrfach im selben Prozess, nach neuem Adapteraufbau und in getrennten Testläufen gelöst. Ergebniszuweisungen, schwarze `X`, Status und Zielvektor müssen bei gleicher Solver-Version und gleichen Einstellungen wertgleich sein.

Antwort: Die normale Generierung soll reproduzierbar sein. Eine alternative gleichwertige Planung bleibt eine spätere, ausdrücklich gestartete Funktion.

### H-11 – Zeitgrenze und Abbruch

Welche Laufzeitpfade werden automatisiert geprüft?

**Empfehlung:** Optimales Ergebnis vor Grenze, zulässiger nicht nachweislich optimaler Zwischenstand, Zeitablauf ohne zulässigen Plan, Abbruch vor Start, Abbruch während Lösung, technischer Fehler und jeweils unveränderter vorhandener Entwurf bis zur bewussten Übernahme.

Antwort: deine empfehlung

### H-12 – Atomare Speicherung und Konkurrenz

Welche Application- und Infrastructure-Fälle werden geprüft?

**Empfehlung:** Erfolgreiche Übernahme, künstlicher Fehler in der Mitte, veraltete Entwurfsversion, veraltete Snapshot-Kennung, doppelte Übernahme, Neustart nach Erfolg und vollständig erhaltener alter Entwurf nach jedem Fehlerfall.

Antwort: deine empfehlung

### H-13 – Realistische synthetische Last

Welcher Performancefall wird als erster belastbarer Nachweis verwendet?

**Empfehlung:** Ungefähr 20 synthetische Mitarbeitende, bis zu fünf AH, drei Wochen, vollständiger aktueller Startbedarf und mehrere gezielte Engpässe. Der Test erfasst Modellaufbau, Solverzeit, Rückübersetzung und Gesamtzeit getrennt. Er enthält keine echten Planwerte.

Antwort: deine empfehlung

### H-14 – Bedeutung der Zwei-Minuten-Grenze im Test

Ist die Zwei-Minuten-Grenze ein hartes automatisches Build-Gate oder zunächst ein manuell bewerteter Performance-Nachweis auf einem festgehaltenen Referenzrechner?

**Empfehlung:** Zunächst ein wiederholbarer, separat ausgewiesener Performance-Nachweis auf dokumentierter Hardware, kein flakyanfälliger normaler Unit-Test. Korrektheit, Hard Rules und atomare Speicherung bleiben harte Gates. Eine deutliche Überschreitung der bestätigten Größenordnung ist dennoch Handlungsbedarf vor Systemabschluss.

Antwort: deine empfehlung

### H-15 – Architektur- und Paketgrenzen

Welche automatisierten Architekturprüfungen werden ergänzt?

**Empfehlung:** OR-Tools ausschließlich in Planning; Planning referenziert nur Application und Domain; Application enthält keine OR-Tools-Typen; Desktop-Features kennen keinen konkreten Planning-Adapter; nur Composition verdrahtet ihn; keine Datenbank- oder deutschen UI-Texte in Planning.

Antwort: deine empfehlung

### H-16 – Sichtbare Bedienprüfung

Welche manuellen Fälle umfasst das WPF-Gate?

**Empfehlung:** Start mit aktuellem Snapshot, blockierter Start bei veraltetem Snapshot, laufender Zustand, Abbruch, optimaler Vorschlag, zulässiger nicht bewiesen optimaler Vorschlag, Verwerfen, Übernehmen, Versionskonflikt, schwarze `X`, ungedeckter Bedarf und technischer Fehler.

Antwort: deine empfehlung

## I – Modularität und minimale Abhängigkeiten

### I-01 – Aufteilung des Planning-Moduls

Welche internen Verantwortungen werden getrennt?

**Empfehlung:** Mindestens Eingangsvalidierung, Kandidatenerzeugung, Regelübersetzung, Modellaufbau, hierarchische Optimierung, AH-Nachphase, Rückübersetzung und Ergebnisvalidierung. Die Roadmap benennt kleine Code-Anker; es entsteht kein `PlanningManager` oder allwissender Solver-Dienst.

Antwort: deine empfehlung

### I-02 – Regelübersetzer

Wie fein werden Regelübersetzer geschnitten?

**Empfehlung:** Ein Übersetzer behandelt genau eine Regelart oder eine eng zusammengehörige Regelfamilie. Er erhält die typisierte Regeldefinition und liefert keine deutschen Texte. Gemeinsame technische Modellbausteine dürfen gekapselt wiederverwendet werden, Regelwerte aber nicht dupliziert werden.

Antwort: deine empfehlung

### I-03 – Mehrstufige Optimierung

Wer koordiniert die Optimierungsstufen?

**Empfehlung:** Ein klarer interner Orchestrator führt benannte Stufen aus und bewahrt nach jeder Stufe deren nachgewiesenen Zielwert als Grenze für die nächste Stufe. Die einzelnen Regelübersetzer kennen nicht den gesamten Ablauf.

Antwort: deine empfehlung

### I-04 – Nicht-AH und AH

Werden Nicht-AH- und AH-Planung als getrennte fachlich sichtbare Phasen implementiert?

**Empfehlung:** Ja. Gemeinsame Kandidaten- und Regelbausteine dürfen geteilt werden, aber die zweite Phase erhält die eingefrorene Nicht-AH-Lösung und ausschließlich verbleibenden Bedarf.

Antwort: deine empfehlung

### I-05 – Application-Verträge

Welche neuen öffentlichen Modulverträge gehören in Application?

**Empfehlung:** Ein schmaler Planning-Port mit unveränderlichem Request/Result, ein eigener Generate-Anwendungsfall und ein atomarer Übernahme-Port. UI-Vorschau, Speicherentitäten und OR-Tools-Zustände werden nicht in einem Sammelmodell vermischt.

Antwort: deine empfehlung

### I-06 – Alternative später ermöglichen

Welche Vorbereitung für alternative Pläne ist zulässig?

**Empfehlung:** Deterministische Solver-Einstellungen und Tie-Breaker werden zentral gekapselt; der Anwendungsfall kann später eine ausdrücklich definierte Suchabsicht erhalten. Es werden jetzt keine ungenutzten öffentlichen Strategieinterfaces, Zufallsschalter oder zweite Implementierungen angelegt.

Antwort: Die spätere Alternative soll vorbereitet, aber in System 09 noch nicht implementiert werden.

### I-07 – Persistenzgrenze

Darf Planning selbst Entwürfe oder Ergebnisse laden und speichern?

**Bereits verbindliche Grundlage:** Nein. Application stellt den vollständigen Snapshot bereit und koordiniert die atomare Übernahme über einen Infrastructure-Port. Planning kennt weder EF Core noch SQLite.

Antwort: deine empfehlung

### I-08 – Diagnostikgrenze zu System 10

Welche Diagnosedaten erzeugt System 09 bereits?

**Empfehlung:** System 09 liefert offene Plätze und Zeiten, Regelbewertungen, Zielwerte, betroffene technische Kennungen und grundlegende Lauf-/Ablehnungscodes. Kontrollierte Ursachenanalyse je ausgeschlossener Person und ausformulierte Lösungsvorschläge bleiben System 10.

Antwort: deine empfehlung

### I-09 – Neue Abhängigkeiten

Benötigt System 09 zusätzliche Pakete neben dem bereits zentral vorhandenen stabilen `Google.OrTools`?

**Empfehlung:** Nein. Testhilfen, Referenzlöser und Ergebnisvergleiche werden mit vorhandenem .NET- und xUnit-Bestand umgesetzt. Jede unerwartet notwendige neue Abhängigkeit wäre eigener Handlungsbedarf vor der Roadmap-Abnahme.

Antwort: deine empfehlung

## J – Roadmap- und Abnahmegrenzen

### J-01 – Reihenfolge der späteren Implementierung

Soll die Teil-Roadmap die Umsetzung zunächst mit Verträgen, Vorprüfungen und gemeinsamen Testorakeln beginnen, bevor das vollständige CP-SAT-Modell entsteht?

**Empfehlung:** Ja. Dadurch werden Eingabe, Ergebnis, unterstützte Regeln und Korrektheitsnachweise festgelegt, bevor viele Solverbedingungen gleichzeitig entstehen.

Antwort: deine empfehlung

### J-02 – Kleine Regelpakete

Sollen Hard Rules und Optimierungsstufen in kleinen getrennt abnehmbaren Schritten umgesetzt werden?

**Empfehlung:** Ja. Struktur und Kandidaten, Hard Rules, Bedarfsdeckung, hohe Regeln, mittlere Regeln, Stabilität, AH-Phase, Rückübersetzung, Speicherung und UI erhalten getrennte Nachweise.

Antwort: deine empfehlung

### J-03 – Kein Abschluss nur durch grüne Unit-Tests

Welche Gates müssen vor Abschluss von System 09 zusätzlich bestanden sein?

**Empfehlung:** Vollständiger Build, Domain-/Application-/Planning-/Infrastructure-/Desktop- und Architekturtests, Mutationsnachweise, reproduzierbarer realitätsnaher Performancefall, Datenbank-/Atomaritätstest und sichtbare WPF-Abnahme. Portable Windows-Ausgabe bleibt System 15.

Antwort: deine empfehlung

### J-04 – Fachliche Prüfung der Kombinationsfälle

Sollen die wichtigsten neu konkretisierten Solver-Kombinationsfälle vor Abschluss der Roadmap durch die Service-Leitung in einfacher Sprache geprüft werden?

**Empfehlung:** Ja, insbesondere offene Stunden gegen offene Plätze, gleichrangige Regelverletzungen, `D`/`Spr`, Früh-/Spätdienst-Fairness, mehrere AH-Personen und zulässiger nicht nachweislich optimaler Zwischenstand.

Antwort: deine empfehlung

### J-05 – Punktesystem als spätere Erweiterung

Wie wird der bestätigte Gedanke eines späteren Punktesystems festgehalten?

Antwort:

- Die erste Fassung verwendet die feste Hierarchie und die bestätigte Behandlung gleichrangiger Regeln.
- Ein späteres Punktesystem wäre eine neue fachliche Optimierungsentscheidung mit eigenen Beispielen, Dominanznachweis, Tests und ausdrücklicher Abnahme.
- Es wird in System 09 weder sichtbar konfigurierbar noch als ungenutzte technische Parallelstruktur implementiert.

### J-06 – Abschlussbedingung des Fragenkatalogs

Wann darf aus diesem Dokument die System-09-Teil-Roadmap entstehen?

**Empfehlung:** Erst wenn alle offenen Antworten konsolidiert, scheinbare Widersprüche aufgelöst, Prioritäts- und Ergebnisszenarien verständlich bestätigt und die Systemgrenzen zu 10 bis 12 eindeutig sind.

Antwort: deine empfehlung

## Abschließende Konsolidierung der bestätigten Punkte

1. Die Größenordnung von 17 bis 20 Mitarbeitenden und bis zu fünf AH ist nur eine unverbindliche Orientierung für realitätsnahe synthetische Tests.
2. Ein Lauf darf in Version 1 ungefähr bis zu zwei Minuten dauern. Korrekte Generierung hat zunächst Vorrang; Performance darf später verbessert werden.
3. Ein zulässiger, noch nicht nachweislich optimaler Vorschlag wird gekennzeichnet und nur bewusst übernommen.
4. Identischer Input erzeugt bei identischer Solver-Version und identischen Einstellungen reproduzierbar denselben Plan.
5. Eine spätere bewusste Alternativplanung bleibt architektonisch möglich, wird aber noch nicht implementiert.
6. Jeder nach einer Generierung sonst freie Tag erhält ein schwarzes `X`.
7. Innerhalb derselben Priorität werden zuerst Verletzungsfälle und danach das Ausmaß derselben Regel minimiert. Verdeckte Punktwerte werden nicht verwendet.
8. Ein mögliches späteres Punktesystem bleibt nur als Erweiterungsgedanke festgehalten.
9. Spätdienst und `Spr` gelten als ungünstig; `D` und `Spr` sollen besonders vermieden werden. Früh- und Spätdienste werden unter geeigneten Personen fair verteilt.
10. System 09 erhält den empfohlenen minimalen Bedienablauf mit Start, Arbeitszustand, Abbruch, Ergebnisvorschau und bewusster Übernahme.
11. Eine Neugenerierung bewahrt geschützte Zuweisungen und ersetzt nur die ersetzbaren automatischen Ergebnisse atomar.
12. `Spr` deckt den Restaurant-Spätdienst erst ab dem tatsächlichen Wechsel; die frühere Zeit bleibt sichtbar ungedeckt und Teil der Bedarfsoptimierung.
13. Eine aktualisierte Planungsvorbereitung erzeugt einen neuen unveränderlichen Snapshot und ersetzt den vorherigen technischen Snapshot. Eine Snapshot-Historie gehört nicht zu System 09.
14. Unterschiedliche Regelarten derselben Priorität werden nicht durch Punkte oder Einheitenumrechnung gegeneinander gewichtet. Nach gleicher Fallzahl entscheiden die nachfolgenden bestätigten Stufen.
15. `Spr` und `D` werden ausschließlich durch ihre bestätigten Notfall-, Begrenzungs- und Minimierungsregeln besonders vermieden. Es entsteht kein zusätzliches verborgenes Gewicht.
16. Vor der Solverumsetzung entsteht eine vollständige, prüfbare Prioritäts- und Übersetzungsmatrix für alle 28 Regeln sowie eine ausführbare Vergleichssemantik für den Zielvektor.

## Ergebnisstand des Verständnisabgleichs

- Alle 94 Fragen und die anschließenden vier Klärungspunkte sind beantwortet.
- Die zunächst widersprüchliche Antwort zur `Spr`-Teildeckung wurde zugunsten der bereits abgenommenen Regel aus den Systemen 07 und 08 aufgelöst.
- Die Antworten zu Snapshot-Lebensdauer, gleichrangigen Regeln und der Vermeidung von `Spr` und `D` sind ausdrücklich präzisiert.
- Der Auftraggeber hat den vollständig geklärten Fragenstand am 2026-09-17 als Grundlage für den Entwurf der System-09-Teil-Roadmap bestätigt.
- Die Teil-Roadmap liegt unter `docs/roadmaps/active/S09_AUTOMATIC_SCHEDULE_GENERATION_ROADMAP.md` als Entwurf vor und benötigt vor jeder Implementierung ihre eigene ausdrückliche Abnahme.
- Es wurde noch keine automatische Generierung implementiert.

## Historischer nächster Schritt des ursprünglichen Fragenkatalogs

Der ursprüngliche Fragenkatalog wurde abgeschlossen, die System-09-Teil-Roadmap anschließend ausdrücklich abgenommen und bis AG-14 umgesetzt. Der nachfolgende Ergänzungsabschnitt dokumentiert einen später gewünschten Zwischenplanungsschritt und ändert die bereits bestätigten ursprünglichen Antworten nicht rückwirkend.

## Ergänzungsfragen AG-14A – Übernommenen automatischen Plan vollständig verwerfen

Stand: 2026-09-17

Der Wunsch betrifft nicht das bereits vorhandene Verwerfen einer noch flüchtigen Vorschau. Gemeint ist ein bereits mit „Plan übernehmen“ gespeicherter automatischer Plan, der bewusst vollständig aus dem aktuellen Arbeitsentwurf entfernt werden soll, damit die Service-Leitung wieder von vorne beginnen kann. Vor der Beantwortung dieser Fragen besteht keine Implementierungsfreigabe.

### K-01 – Welche fachlichen Eingaben bleiben erhalten?

Sollen Typ1-Zuweisungen, `U`, `K`, rote `X` und später mögliche manuelle Zuweisungen beim vollständigen Verwerfen erhalten bleiben?

**Empfehlung:** Ja. Diese Werte sind bewusste Eingaben und keine automatisch erzeugten Ergebnisse. Entfernt werden automatische Zuweisungen und schwarze `X`; Zeitraum, Bedarfe, Mitarbeitende, Verfügbarkeiten und bewusste Eingaben bleiben erhalten.

Antwort: ja

### K-02 – Gesperrte automatische Zuweisungen

Sollen auch später gesperrte oder anderweitig vor automatischer Ersetzung geschützte automatische Zuweisungen entfernt werden?

**Empfehlung:** Ja. Die ausdrücklich bestätigte Aktion heißt „vollständig verwerfen“. Die Warnung muss deshalb darauf hinweisen, dass auch Schutz und Sperren der automatisch erzeugten Einteilungen aufgehoben werden. Typ1- und manuelle Werte bleiben davon getrennt.

Antwort: ja

### K-03 – Planungsvorbereitung nach dem Zurücksetzen

Soll die bisherige Planungsvorbereitung ebenfalls verworfen werden, sodass vor dem nächsten Lauf erneut bewusst „Planung vorbereiten“ gewählt werden muss?

**Empfehlung:** Ja. Der Entwurf erhält durch das Zurücksetzen eine neue Version. Eine neue bewusste Vorbereitung ist verständlich, versionssicher und entspricht „von vorne anfangen“. Es findet keine versteckte automatische Vorbereitung statt.

Antwort: ja

### K-04 – Sicherheitsbestätigung

Welche Bestätigung ist vor dem unwiderruflichen Zurücksetzen des aktuellen Arbeitsentwurfs erforderlich?

**Empfehlung:** Eine deutliche Bestätigung reicht. Sie nennt die Anzahl automatisch erzeugter Einteilungen und schwarzer `X`, erklärt den Erhalt bewusster Eingaben und weist darauf hin, dass die Aktion im aktuellen Ausbauzustand nicht rückgängig gemacht werden kann. Eine zusätzliche Texteingabe wäre für diesen lokalen Einzelbenutzerablauf unverhältnismäßig.

Antwort: ja, deine Empfehlung

### K-05 – Grenze zu späteren abgenommenen Planversionen

Gilt „Automatischen Plan vollständig verwerfen“ ausschließlich für den aktuellen, noch nicht abgenommenen Arbeitsentwurf?

**Empfehlung:** Ja. Spätere unveränderliche Planversionen aus System 12 werden niemals gelöscht oder verändert. Eine spätere Bearbeitung eines abgenommenen Plans arbeitet auf einem neuen Arbeitsstand und erzeugt nach erneuter Abnahme eine neue Version.

Antwort: ja, deine Empfehlung

## Aktueller nächster Schritt

Die Antworten K-01 bis K-05 sind vollständig und wurden in AG-14A konsolidiert. Der vollständige AG-14A-Planungsentwurf wurde am 2026-09-17 ausdrücklich zur Implementierung freigegeben. Die Umsetzung ist automatisch vollständig geprüft. Vor dem gemeinsamen sichtbaren Gate und vor AG-15 wurde anschließend ein fachlicher Änderungswunsch zur Verteilung von Nicht-AH, AH, `D`, `Spr` und Wochenstunden eingebracht. Die folgenden Fragen L-01 bis L-10 ersetzen ausschließlich die davon betroffenen früheren Antworten; alle nicht betroffenen Entscheidungen bleiben bestehen.

## Ergänzungsfragen vor AG-15 – gemeinsame Personaloptimierung

Stand: 2026-09-17

### L-01 – Hauptziel nach der Bedarfsdeckung

Was soll bei ansonsten gleicher maximaler Bedarfsdeckung Vorrang haben: möglichst wenige `D` und `Spr` oder die exakte Annäherung aller Personen an ihr Wochen-Soll?

Antwort: Möglichst wenige `D` und `Spr` haben Vorrang vor der exakten Annäherung an das Wochen-Soll. Die Bedarfsdeckung bleibt wichtiger. Die bereits bestätigten hohen Schutzregeln einschließlich `NORMAL_WEEKLY_MINIMUM` bleiben ebenfalls vorgeschaltet.

### L-02 – AH-Mindestintegration

Welches neue Mindestziel gilt für AH?

Antwort: Jede automatisch planbare AH-Person soll bei vorhandenem zulässigem Bedarf möglichst mindestens 180 Minuten je Montag-bis-Sonntag-Woche erhalten. Zehn Stunden bleiben das eigentliche Wochenziel und zwölf Stunden die zwingende automatische Obergrenze. Der bestehende Hinweis unter sechs Stunden bleibt erhalten; drei bis unter sechs Stunden erfüllen damit das neue Mindestziel, erzeugen aber weiterhin den Hinweis.

### L-03 – Schutz des Nicht-AH-Mindestkorridors

Darf die AH-Mindestintegration eine Nicht-AH-Person unter `wirksames Wochensoll minus drei Stunden` drängen?

Antwort: Nein. AH darf normale Dienste von Nicht-AH übernehmen und Nicht-AH dadurch unter dem exakten Wochensoll bleiben, aber die bereits bestätigte hohe Nicht-AH-Untergrenze darf dadurch nicht verschlechtert werden.

### L-04 – Gemeinsame statt eingefrorene Planung

Dürfen normale Zuweisungen von Nicht-AH und AH für das bessere Gesamtergebnis gemeinsam umverteilt werden?

Antwort: Ja. Die bisherige vollständige Nicht-AH-Auswahl wird in der neuen Primärlösung nicht mehr vor der AH-Optimierung eingefroren. Alle zulässigen normalen, `D`- und `Spr`-Kandidaten dürfen im selben globalen Modell neu geordnet werden. Frühere Zwischenentscheidungen sind keine Sperren.

### L-05 – Bedeutung von `D` und `Spr`

Werden Dienste für `D` oder `Spr` zeitlich verlängert?

Antwort: Nein. `D` und `Spr` bleiben zusammengesetzte Muster aus vorhandenen Bedarfsplätzen und deren tatsächlichen Zeiten. Planning erfindet weder zusätzliche Arbeitszeit noch neue Bedarfe oder Zeitverlängerungen.

### L-06 – Vermeidung von `Spr` und `D`

Wie werden die beiden Muster unter gleich gut gedeckten und hinsichtlich hoher Regeln gleichwertigen Plänen behandelt?

Antwort: Zuerst wird `Spr` als strenger Notfallfall minimiert, danach die Gesamtzahl der `D`. `Spr` darf weiterhin nur verwendet werden, wenn es eine sonst verbleibende Restaurant-Unterdeckung reduziert. Weder `Spr` noch `D` werden nur zur Stundenauffüllung verwendet.

### L-07 – Faire Annäherung an individuelle Wochenziele

Wie wird verteilt, wenn nach Bedarfsdeckung, hohen Regeln, Mustervermeidung und AH-Mindestintegration nicht alle Personen ihr Ziel erreichen können?

Antwort: Die Annäherung wird relativ zum individuellen wirksamen Wochenziel bewertet. Zuerst wird die größte relative Abweichung einer Person-Woche möglichst verkleinert; danach werden die verbleibenden Abweichungen weiter ausgeglichen. Für Nicht-AH gilt das nach `U` und `K` wirksame Wochensoll, für AH 600 Minuten. Personen-Wochen mit Ziel null werden nicht künstlich in einen Quotienten einbezogen. Es entstehen keine frei geschätzten Punktgewichte.

### L-08 – Früh-/Spät-Fairness

Zwischen welchen Personen werden Früh- und Spätdienste ausgeglichen?

Antwort: Weiterhin nur zwischen vergleichbar geeigneten Personen mit vergleichbaren zulässigen Kandidatenmöglichkeiten im aktuellen Drei-Wochen-Zeitraum. Namen, Typcodes und UI-Reihenfolge bleiben ohne Einfluss.

### L-09 – Primärlösung und Rückfall

Welche technische Vorgehensweise wird zuerst umgesetzt?

Antwort: Zuerst wird die gemeinsame globale Optimierung nach L-01 bis L-08 umgesetzt und durch verständliche synthetische Vergleichsfälle fachlich geprüft. Die nachfolgende fünfstufige Vorgehensweise bleibt ausdrücklich als Rückfall dokumentiert, wird aber nicht parallel oder vorsorglich implementiert:

1. Nicht-AH ohne `D` und `Spr` planen und Früh/Spät gleichmäßig verteilen.
2. AH füllt verbleibende normale Lücken, weiterhin ohne `D` und `Spr`.
3. Verbleibende Bedarfslücken durch zulässige `D`- und `Spr`-Muster von Nicht-AH reduzieren.
4. Danach verbleibende Lücken durch zulässige `D`- und `Spr`-Muster von AH reduzieren.
5. Normale AH-Dienste dürfen für unter Soll liegende Nicht-AH-Personen umverteilt werden, ohne AH unter das erreichbare 180-Minuten-Mindestziel zu verdrängen und ohne frühere höhere Ziele zu verschlechtern.

### L-10 – Entscheidung über den Rückfall

Wann darf von der Primärlösung auf die fünfstufige Vorgehensweise umgebaut werden?

Antwort: Erst wenn die Service-Leitung die Ergebnisse der vereinbarten synthetischen Kombinationsfälle sichtbar prüft und ausdrücklich feststellt, dass die gemeinsame Optimierung nicht das gewünschte Planbild erzeugt. Dann wird vor jedem Umbau ein eigener kleiner Folgeplan erstellt und abgenommen. Ohne diese Entscheidung entsteht kein zweiter Algorithmus und kein versteckter Umschalter.

## Neuer aktueller nächster Schritt

Die Antworten L-01 bis L-10 sind fachlich vollständig und in AG-14B als Zielmatrix, Regelversionsgrenze, Primärlösung und Rückfallgrenze konsolidiert und ausdrücklich abgenommen. AG-14C bis AG-14E setzen Regelkatalogversion 2, Zielvergleich, gemeinsamen Solver, Laufmetadaten und Persistenz um und sind automatisch geprüft. Der erneute sichtbare AG-14F-Lauf bestätigt, dass die App nach der Fehlerkorrektur einen Plan erzeugen kann; die Service-Leitung bewertet das Ergebnis jedoch noch nicht als optimal oder fachlich gut.

Am 2026-09-17 wurde deshalb folgende weitere Reihenfolge bestätigt:

1. Der aktuelle Generator bleibt zunächst fachlich unverändert.
2. S09A ordnet die vorhandene Dienstplanoberfläche neu und wird in einem neuen Arbeitschat vor jeder Codeänderung nochmals geprüft und ausdrücklich abgenommen.
3. Erst nach S09A wird ein eigener Fragenkatalog für den S09B-Planungsqualitätsbericht erstellt. Welche Kennzahlen und Vergleiche der Bericht genau enthält, bleibt bis zu diesem Verständnisabgleich offen.
4. Der Bericht misst den eingefrorenen Ausgangsstand und bleibt von den vollständigen Konflikterklärungen und Lösungsvorschlägen aus System 10 getrennt.
5. Aus dem Bericht folgt ein eigener abgenommener Plan für gezielte Optimierungen oder für die bereits dokumentierte fünfstufige Rückfalllösung.
6. AG-15 führt den unabhängigen Korrektheits-, Mutations- und Leistungsnachweis erst für den danach endgültig vorgesehenen Algorithmus aus.

Der nächste minimale Schritt ist damit die gesonderte Abnahme und Umsetzung von S09A. Dieses Dokument erfindet keine Inhalte für S09B vor dessen späterem Fragenkatalog.
