# Fragen und Verständnisabgleich: Regelkatalog und Prioritäten

Status: Alle Fragen beantwortet, am 2026-09-16 ausdrücklich abgenommen und mit System 07 archiviert

Stand: 2026-09-16

## Zweck

Dieses Dokument sammelt die fachlichen und übergeordneten technischen Fragen für System 07 „Regelkatalog und Prioritäten“.

Alle Fragen und Folgefragen sind beantwortet. Die ursprünglichen Antworten bleiben als Entscheidungsverlauf erhalten. Der Abschnitt „Konsolidiertes Verständnis“ fasst ihre gemeinsame Bedeutung widerspruchsfrei zusammen und wurde am 2026-09-16 ausdrücklich abgenommen.

Dieses Dokument ist noch keine Teil-Roadmap und gibt weder Implementierung noch Datenbank-, Bedien- oder Planungscode frei. System 06 ist abgeschlossen und bleibt davon unberührt.

Alle Beispiele und späteren Tests verwenden ausschließlich erfundene Personen und synthetische Daten. Echte Namen, echte Dienstpläne, Gesundheitsangaben, Personalakten oder andere personenbezogene Unterlagen werden für die Klärung nicht benötigt.

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
- `docs/decisions/S03_EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_QUESTIONS.md`
- `docs/roadmaps/completed/S06_AVAILABILITY_ABSENCE_ROADMAP.md`
- die abgeschlossenen Fragen-, Entscheidungs- und Roadmap-Dateien der Systeme 03 bis 05

## Rechtliche Orientierung für die Fragen

Folgende amtliche Quellen wurden als Orientierung für die Fragen geprüft:

- [§ 2 ArbZG – Begriffsbestimmungen](https://www.gesetze-im-internet.de/arbzg/__2.html)
- [§ 3 ArbZG – Arbeitszeit der Arbeitnehmer](https://www.gesetze-im-internet.de/arbzg/__3.html)
- [§ 4 ArbZG – Ruhepausen](https://www.gesetze-im-internet.de/arbzg/__4.html)
- [§ 5 ArbZG – Ruhezeit](https://www.gesetze-im-internet.de/arbzg/__5.html)
- [§ 6 ArbZG – Nacht- und Schichtarbeit](https://www.gesetze-im-internet.de/arbzg/__6.html)
- [§ 10 ArbZG – Sonn- und Feiertagsbeschäftigung](https://www.gesetze-im-internet.de/arbzg/__10.html)
- [§ 11 ArbZG – Ausgleich für Sonn- und Feiertagsbeschäftigung](https://www.gesetze-im-internet.de/arbzg/__11.html)

Diese Quellen allein legen die für die konkrete Rehaklinik geltenden Regeln nicht abschließend fest. Tarifvertrag, Betriebs- oder Dienstvereinbarung, behördliche Ausnahmen, arbeitsvertragliche Regelungen und die genaue rechtliche Einordnung des gastronomischen Servicebereichs können zusätzliche oder abweichende Vorgaben enthalten. Deshalb wird keine rechtliche Empfehlung in diesem Dokument ohne fachliche Prüfung und Bestätigung als zwingende Projektregel übernommen.

## Aufgabe und Grenze von System 07

System 07 beschreibt jede Planungsregel genau einmal als unveränderliche fachliche Definition. Dazu gehören mindestens eine stabile Regelkennung, Regelart, Geltungsbereich, zwingende oder weiche Wirkung, gegebenenfalls Priorität, fachliche Parameter und ein neutraler Beschreibungsschlüssel.

System 07 erzeugt noch keinen Dienstplan. Es bereitet die Regeln so vor, dass:

- System 08 sie in eine unveränderliche Planungsmomentaufnahme übernehmen kann,
- System 09 jede unterstützte Regel eindeutig in die automatische Generierung übersetzen kann,
- System 10 Regelverletzungen und Hinweise strukturiert erklären kann,
- System 11 weiche Abweichungen bei manueller Bearbeitung sichtbar halten kann,
- System 12 die bei einer Abnahme verwendete Regelfassung bewahren kann.

OR-Tools-Übersetzungen, automatische Generierung, fertige Konfliktsätze, manuelle Planbearbeitung, Planabnahme und Excel-Ausgabe gehören nicht zu System 07.

## Bereits bestätigte Grundlagen

Die folgenden Aussagen werden nicht erneut grundsätzlich infrage gestellt. Detailfragen sind nur dort aufgenommen, wo die spätere Regel technisch und fachlich noch nicht eindeutig genug ist.

### Allgemeine Regelwirkung

- Zwingende Regeln dürfen durch die automatische Planung nicht verletzt werden.
- Kann Bedarf wegen zwingender Regeln nicht vollständig gedeckt werden, bleibt der vollständig oder teilweise ungedeckte Zeitraum sichtbar.
- Weiche Regeln besitzen die Priorität hoch, mittel oder niedrig.
- Viele niedrig priorisierte Regeln dürfen zusammen keine höher priorisierte Regel überstimmen.
- Die Service-Leitung darf bei einer späteren manuellen Bearbeitung eine weiche Regel bewusst übergehen; die Abweichung bleibt sichtbar.
- Eine unbekannte oder technisch noch nicht übersetzte Regel darf bei der späteren Generierung nicht stillschweigend ignoriert werden.
- Automatische Überbesetzung ist nicht erlaubt.

### Bereits eingeordnete Planungsregeln

| Regel | Bestätigte Einordnung |
|---|---|
| normaler Wochenkorridor von Wochen-Soll minus drei bis Wochen-Soll plus drei Stunden | bisher als zwingend bestätigt |
| zwei zusammenhängende freie Tage pro Person und Woche | weich, hoch |
| freie Tage möglichst direkt mit einem roten `X` verbinden | weich, hoch |
| vollständiges freies Wochenende direkt vor Urlaub | weich, hoch |
| vollständiges freies Wochenende direkt nach Urlaub | bisher als zwingend bestätigt |
| mindestens ein vollständiges freies Wochenende innerhalb der drei geplanten Wochen | weich, mittel |
| höchstens sieben Arbeitstage in Folge | zwingend; angrenzende Historie wird berücksichtigt |
| höchstens ein Doppeldienst pro Person und Woche | weich, hoch |
| Doppeldienste innerhalb dieser Grenze zusätzlich minimieren | weich, mittel |

### Bereits bestätigte Sonderregeln

- Leere Tagesfelder aktiver Mitarbeitender gelten vor der Generierung grundsätzlich als verfügbar.
- `U`, `K` und rote `X` sperren den betreffenden ganzen Kalendertag zwingend für automatische Dienste.
- Ein rotes `X` zählt bereits als freier Tag, an den möglichst ein weiterer freier Tag angeschlossen wird.
- `U` und `K` reduzieren das wirksame Wochen-Soll um den Tageswert des verwendeten Mitarbeitertyps; rote und schwarze `X` reduzieren es nicht.
- Typ1 wird nicht automatisch eingeplant. Manuell vorgetragene Typ1-Dienste werden geschützt.
- Bei Typ1 ist in einer nicht vollständig abwesenden Woche mindestens ein vorgetragener Dienst vorgesehen. Bei vollständiger Abwesenheit darf mit deutlichem Hinweis ohne diesen Dienst generiert werden.
- Zuerst werden automatisch planbare Nicht-AH-Typen geplant. AH darf anschließend nur verbleibende zulässige Lücken füllen und keine bereits geplante Nicht-AH-Person verdrängen.
- Für AH bleiben zehn Wochenstunden das Ziel. Weniger als sechs Stunden sind zulässig und werden berichtet; mehr als zehn Stunden werden berichtet; mehr als zwölf Stunden sind zwingend unzulässig.
- Der Doppeldienst `D` besteht aus Restaurant-Frühdienst und Restaurant-Spätdienst. Die Unterbrechung zwischen beiden Abschnitten zählt nicht als Arbeitszeit.
- Der Springer `Spr` ist ein eigener samstäglicher Notfalleinsatz von der Cafeteria zum Restaurant. Eine vorher verbleibende Teilunterdeckung im Restaurant darf nicht verborgen werden.

## Fragen zur fachlichen Bestätigung

## A – Umfang und Bedienbarkeit des Regelkatalogs

### A-01 – Feste Regelarten oder frei anlegbare Regeln

Soll die erste Fassung ausschließlich die gemeinsam bestätigten Regelarten enthalten, oder darf die Service-Leitung völlig neue Regeln selbst formulieren?

**Empfehlung:** In der ersten Fassung nur fest definierte und getestete Regelarten zulassen. Frei formulierbare Regeln wären ohne eigene technische Übersetzung nicht zuverlässig planbar und dürften gemäß Architektur ohnehin nicht ignoriert werden.

Antwort: deine empfehlung

### A-02 – Bearbeitbare Grenzwerte

Darf die Service-Leitung Grenzwerte wie „sieben Arbeitstage“, „ein Doppeldienst“ oder AH-Schwellen in der App verändern?

**Empfehlung:** In der ersten Fassung keine freie Bearbeitung dieser Grenzwerte. Zuerst einen fachlich bestätigten Startkatalog sicher umsetzen. Eine spätere Bearbeitung benötigt eigene Validierung, Wirksamkeitsregeln und Historie.

Antwort: deine empfehlung

### A-03 – Bearbeitbare Prioritäten

Darf die Service-Leitung die Priorität einer weichen Regel zwischen hoch, mittel und niedrig verändern?

**Empfehlung:** Für die erste Fassung die bestätigten Prioritäten festhalten und nicht in der Oberfläche änderbar machen. So bleibt die Generierung reproduzierbar und die erste fachliche Abnahme eindeutig.

Antwort: deine empfehlung

### A-04 – Regeln aktivieren oder deaktivieren

Dürfen weiche Regeln deaktiviert werden? Dürfen zwingende Regeln deaktiviert werden?

**Empfehlung:** Zwingende Regeln niemals in der normalen Bedienung deaktivierbar machen. Für die erste Fassung auch weiche Regeln immer aktiv lassen. Eine spätere optionale Aktivierung sollte nur als versionierte Einstellung mit deutlicher Auswirkung erfolgen.

Antwort: deine empfehlung

### A-05 – Eigene Bedienoberfläche

Benötigt System 07 einen sichtbaren Reiter „Regeln“, obwohl Regeln und Prioritäten zunächst nicht bearbeitet werden?

**Empfehlung:** Nein. In System 07 zunächst Domain-Regelkatalog, Application-Lesevertrag und automatische Tests umsetzen. Eine reine Anzeige ohne Handlungsmöglichkeiten bringt wenig Nutzen und erzeugt ein zusätzliches visuelles Gate.

Antwort: deine empfehlung

### A-06 – Datenbankspeicherung

Müssen die Regeln in SQLite gespeichert werden, oder genügt ein versionierter Startkatalog im Programm?

**Empfehlung:** Solange Regeln nicht durch die Service-Leitung geändert werden, keine zusätzliche Datenbankpflege einführen. Unveränderliche Regeldefinitionen gehören in Domain; veränderliche fachliche Parameter bleiben bei ihren Eigentümern, beispielsweise Wochen-Soll beim Mitarbeitertyp.

Antwort: deine empfehlung

### A-07 – Grenze zwischen Regel und Stammdatum

Soll der Regelkatalog Wochen-Soll, Einsatzfreigaben, Dienstzeiten und Bedarfe kopieren, oder diese über ihre stabilen Kennungen beziehungsweise Momentaufnahmen verwenden?

**Empfehlung:** Nichts kopieren. System 03 bleibt Eigentümer von Mitarbeitertypen und Einsatzfreigaben, System 04 von Diensten und Mustern und System 05 von Bedarfen. System 07 definiert nur, wie diese fachlichen Werte bei der Planung als Regeln wirken.

Antwort: deine empfehlung

### A-08 – Minimaler Abschlussumfang von System 07

Ist System 07 abgeschlossen, wenn der vollständige strukturierte Regelkatalog, seine Validierung, ein Application-Lesevertrag und gemeinsame synthetische Beispielszenarien vorliegen?

**Empfehlung:** Ja. Planmodell und regelbezogene Planbewertung werden nur so weit vorbereitet, wie sie ohne das erst in System 08 entstehende Planmodell sinnvoll sind. Solver-Übersetzungen bleiben System 09.

Antwort: deine empfehlung

## B – Verbindliche Quellen und rechtlicher Geltungsbereich

### B-01 – Anwendbare betriebliche Grundlagen

Welche Grundlagen gelten für den gastronomischen Servicebereich der Rehaklinik?

- Tarifvertrag oder kirchliche Arbeitsvertragsrichtlinie,
- Betriebs- oder Dienstvereinbarung,
- arbeitsvertragliche Sonderregeln,
- behördliche Genehmigungen oder Ausnahmen,
- verbindliche interne Dienstplanrichtlinie.

**Empfehlung:** Bezeichnung, Geltungsbereich und relevante Regelstellen ohne personenbezogene Unterlagen zusammentragen. Keine Regel allein aufgrund einer Vermutung als gesetzlich oder tariflich zwingend einordnen.

Antwort: Das System soll nur unsere Regeln einbeziehen und keine externen regeln.

### B-02 – Fachliche Freigabe rechtlicher Regeln

Wer darf für die Klinik verbindlich bestätigen, dass unsere Auslegung der gesetzlichen, tariflichen und betrieblichen Regeln richtig ist?

**Empfehlung:** Vor der Implementierung gesetzlich oder tariflich begründeter Hard Rules eine dokumentierte Bestätigung durch die dafür zuständige Stelle einholen, beispielsweise Personalabteilung, Arbeitgebervertretung oder fachkundige Rechtsberatung. Die Service-Leitung bestätigt zusätzlich den tatsächlichen Planungsablauf.

Antwort: wir gehen hier erstmal nicht nach tariflichen regeln oder anderen regeln außerhalb unserer regeln. Wir erstellen erstmal eine funkionierende app mit unseren regeln. Später kann das, wenn es muss, erweitert werden.

### B-03 – Minderjährige oder besondere Ausbildungsregeln

Werden mit dieser App Personen unter 18 Jahren, Auszubildende oder Praktikanten geplant, für die abweichende Schutzregeln gelten können?

**Empfehlung:** Falls nein, den Personenkreis für die erste Fassung ausdrücklich ausschließen. Falls ja, vor Umsetzung eine eigene bestätigte Regelgruppe anlegen; normale Erwachsenenregeln dürfen nicht stillschweigend verwendet werden.

Antwort: nein. wir unterscheiden in dieser version auch nicht.

### B-04 – Weitere besonders geschützte Personengruppen

Muss die Planung besondere gesetzliche oder betriebliche Einschränkungen einzelner Personengruppen berücksichtigen?

**Empfehlung:** Nur notwendige Planungsbeschränkungen lokal und strukturiert abbilden, niemals Diagnosen oder Gründe. Wenn solche Fälle relevant sind, benötigen sie vor der Umsetzung eine eigene datenschutzarme Fachentscheidung und bestätigte Quelle.

Antwort: nein

### B-05 – Arbeitszeiten bei weiteren Arbeitgebern

Müssen bekannte Arbeitszeiten einer Person bei einem weiteren Arbeitgeber berücksichtigt werden?

**Empfehlung:** Für die erste Fassung nur dann aufnehmen, wenn dies praktisch vorkommt und die Klinik einen bestätigten Erfassungsprozess besitzt. Andernfalls den Dienstplan nicht als vollständigen gesetzlichen Arbeitszeitnachweis bezeichnen.

Antwort: nein

### B-06 – Tägliche Höchstarbeitszeit und Ausgleichszeitraum

Welche tägliche Höchstarbeitszeit und welcher Ausgleichszeitraum gelten tatsächlich? Nach § 3 ArbZG sind acht Stunden werktäglich der Grundsatz; bis zu zehn Stunden setzen einen Ausgleich im gesetzlichen Zeitraum voraus.

**Empfehlung:** Zehn tatsächliche Arbeitsstunden zunächst als niemals automatisch zu überschreitende Tagesobergrenze behandeln. Ob und wie der mehrmonatige Acht-Stunden-Durchschnitt geprüft werden kann, muss wegen der dafür benötigten langen Historie gesondert entschieden und fachlich bestätigt werden.

Antwort: Das machen wir in einem späteren schritt. wenn momentan eine person (bspw. als springer) für 10h geplant werden kann ist das grundsätzlich zulässig. Unsere Regeln versuchen das zu vermeiden, wenn es aber nicht anders geplant werden darf wird das geplant

### B-07 – Pausen innerhalb der bisherigen Sieben-Stunden-Dienste

Die vorhandenen Einzelzeiten 06:30–13:30 Uhr und 13:30–20:30 Uhr umfassen jeweils sieben Stunden und werden bisher ohne abgezogene Unterbrechung als Arbeitszeit behandelt. Gibt es darin tatsächlich eine im Voraus feststehende Ruhepause? § 4 ArbZG verlangt grundsätzlich bei mehr als sechs Stunden Arbeitszeit mindestens 30 Minuten Ruhepause.

**Empfehlung:** Dies vor der System-07-Roadmap zwingend mit der Klinik klären. Falls eine echte Ruhepause besteht, müssen Dienstabschnitte und Arbeitsminuten in System 04 beziehungsweise den späteren Planungsmomentaufnahmen korrekt getrennt werden. Eine bloße Bereitschaft während einer Mahlzeit sollte nicht ohne fachliche Prüfung als Ruhepause behandelt werden.

Antwort: Es gibt ruhepausen, das machen die kollegen aber unter sich aus, wenn es passt. Wir müssen das nicht berücksichtigen in der ersten version

### B-08 – Ruhezeit zwischen zwei Arbeitstagen

Soll grundsätzlich eine ununterbrochene Ruhezeit von mindestens elf Stunden zwischen dem Ende des letzten Dienstabschnitts und dem Beginn des nächsten gelten? Wird die in § 5 ArbZG genannte mögliche Verkürzung in der Klinik tatsächlich genutzt?

**Empfehlung:** Elf Stunden als zwingende Standardregel verwenden. Eine Verkürzung auf zehn Stunden nur nach bestätigter betrieblicher beziehungsweise tariflicher Grundlage und nur zusammen mit einem nachweisbaren Ausgleich auf mindestens zwölf Stunden abbilden.

Antwort: Tariflich sind hier standartgemäß maximal 10h vorgesehen. Ohne nachweisbaren ausgleich von 12h

### B-09 – Sonn- und Feiertagsarbeit sowie Ersatzruhetage

Welche Regeln gelten für Sonntags- und Feiertagsarbeit im konkreten Servicebereich? Wie werden die gesetzlichen beziehungsweise betrieblichen Ersatzruhetage nachgehalten?

**Empfehlung:** Die Zulässigkeit der Sonn- und Feiertagsarbeit sowie die konkreten Ausgleichsregeln bestätigen lassen. § 10 ArbZG nennt sowohl Einrichtungen zur Behandlung und Betreuung als auch Einrichtungen zur Bewirtung; § 11 enthält unter anderem Ersatzruhetage und eine jährliche Mindestzahl beschäftigungsfreier Sonntage. Für diese längeren Zeiträume reicht eine isolierte Drei-Wochen-Ansicht nicht aus.

Antwort: ersatzruhetage werden hier tariflich bezahlt (zuschläge). müssen wir also nicht beachten

### B-10 – Nachtarbeit

Gibt es gegenwärtig oder absehbar Dienste mit Nachtarbeit im Sinne der anwendbaren Regelung?

**Empfehlung:** Bei den bestätigten Zeiten bis spätestens 20:30 Uhr keine Nachtarbeitsregeln in die erste Fassung aufnehmen. Neue Nachtzeiten würden eine eigene fachliche Erweiterung mit bestätigten Schutzregeln erfordern.

Antwort: keine nachtarbeit

### B-11 – Notfälle und außergewöhnliche Abweichungen

Soll die normale automatische Planung gesetzliche oder betriebliche Notfallausnahmen verwenden dürfen?

**Empfehlung:** Nein. Die reguläre Generierung plant nur den normalen zulässigen Betrieb. Ein realer Ausnahmefall sollte später bewusst manuell dokumentiert werden und darf nicht als normale Solver-Abkürzung dienen.

Antwort: nein

### B-12 – Feiertagsquelle

Wie erkennt die spätere Planung einen Feiertag: weiterhin durch manuelle Kennzeichnung wie in System 05 oder durch einen automatischen Kalender?

**Empfehlung:** In der ersten Fassung bei der bereits bestätigten manuellen Feiertagsbehandlung bleiben. Eine automatische Feiertagsquelle wäre eine eigene Erweiterung und benötigt mindestens das maßgebliche Bundesland sowie eine geprüfte Datenquelle.

Antwort: deine empfehlung.

## C – Zeit-, Tages- und Stundenbegriffe

### C-01 – Definition eines Arbeitstages

Wann zählt ein Kalendertag als Arbeitstag für die Regel „höchstens sieben Arbeitstage in Folge“?

**Empfehlung:** Jeder Kalendertag mit mindestens einem tatsächlichen Arbeitsabschnitt zählt genau einmal als Arbeitstag. Die Dauer des Einsatzes ist dafür unerheblich.

Antwort: deine empfehlung

### C-02 – Doppeldienst und Springer als Arbeitstag

Zählen `D` und `Spr` trotz ihrer zwei Abschnitte jeweils als genau ein Arbeitstag?

**Empfehlung:** Ja. Beide liegen nach aktuellem Modell innerhalb eines Kalendertages; ihre Arbeitsminuten werden aus den Abschnitten summiert, der Tag wird aber nur einmal gezählt.

Antwort: ja

### C-03 – Arbeitszeit eines Einsatzes

Welche Minuten zählen zu den geplanten Wochen- und Tagesstunden?

**Empfehlung:** Ausschließlich die tatsächlichen Arbeitsabschnitte der konkreten Zuweisung. Nicht als Arbeitszeit geltende Unterbrechungen werden abgezogen. Standardzeiten dienen nur als Ausgangswert und dürfen eine konkrete tatsächliche Zeit nicht überschreiben.

Antwort: deine empfehlung

### C-04 – Ein Einsatzmuster pro Tag

Darf eine Person an einem Kalendertag mehr als einen voneinander unabhängigen Dienst beziehungsweise ein zusätzliches Einsatzmuster erhalten?

**Empfehlung:** Nein. Höchstens eine normale Zuweisung oder genau ein zusammengesetztes Muster je Person und Kalendertag. `D` und `Spr` enthalten ihre beiden Abschnitte bereits selbst.

Antwort: deine empfehlung

### C-05 – Überschneidungen

Dürfen sich zwei Arbeitsabschnitte derselben Person zeitlich überschneiden?

**Empfehlung:** Nein, zwingend ausschließen. Diese Regel gilt auch für manuell vorgetragene oder gesperrte Zuweisungen.

Antwort: nein

### C-06 – Wochenbegriff

Gilt für alle wöchentlichen Regeln weiterhin Montag bis Sonntag?

**Empfehlung:** Ja. Wochen-Soll, Wochenkorridor, freie Tage, Doppeldienstgrenze und AH-Schwellen werden jeweils pro Montag-bis-Sonntag-Woche bewertet.

Antwort: ja

### C-07 – Dienste über Mitternacht

Müssen Dienste oder Einsatzmuster über Mitternacht in der ersten Fassung unterstützt werden?

**Empfehlung:** Nein. Das bestehende Modell schließt sie aus. Eine spätere Einführung benötigt eine eigene Fach- und Architekturentscheidung zur Zuordnung von Arbeitstag, Ruhezeit und Wochenstunden.

Antwort: gibt keine dienste über mitternacht

### C-08 – Teilweise Bedarfsdeckung und Mitarbeiterstunden

Kann eine Person nur einen Teil des tatsächlichen Bedarfszeitraums übernehmen, und zählen dann nur diese tatsächlich zugewiesenen Minuten?

**Empfehlung:** Ja, falls das spätere Planmodell Teildeckung durch Personen zulässt. Stunden dürfen nur für den tatsächlich zugewiesenen Arbeitsabschnitt zählen; der Rest bleibt ausdrücklich ungedeckt. System 08 muss dies eindeutig modellieren.

Antwort: nein. Nur der Springer übernimmt sozusagen einen Spätdienst der eigentlich 16:30 anfängt, auch wenn er (bis bspw. 17:00) noch bis ende des Cafeteria Dienst B dort bleibt. Das haben wir schon geklärt.

### C-09 – Bedeutung von `U`, `K` und freien Tagen

Zählen `U`, `K`, rote `X` und schwarze `X` für alle Regeln gleichermaßen als „freier Tag“?

**Empfehlung:** Unterscheiden:

- Für die Arbeitstagefolge unterbrechen alle vier Kennzeichen eine Folge tatsächlicher Arbeitstage.
- Für den Wunsch „zwei zusammenhängende reguläre freie Tage“ zählen rote und schwarze `X`.
- `U` und `K` sollten diesen Erholungswunsch nicht automatisch erfüllen, weil Urlaub beziehungsweise Krankheit einen anderen fachlichen Zweck besitzt.

Antwort: nein.

## D – Wochen-Soll, Wochenkorridore, Typ1 und AH

### D-01 – Geltungsbereich des normalen Wochenkorridors

Gilt Wochen-Soll minus drei bis Wochen-Soll plus drei Stunden für alle normalen automatisch planbaren Typen einschließlich später neu angelegter normaler Typen?

**Empfehlung:** Ja. Typ1 und AH werden wegen ihrer besonderen Planungsrollen getrennt behandelt. Der Korridor wird nicht aus dem sichtbaren Typcode abgeleitet.

Antwort: Ja

### D-02 – Korridor nach `U` und `K`

Bezieht sich der Korridor auf das wegen `U` und `K` bereits reduzierte wirksame Wochen-Soll?

**Empfehlung:** Ja. Beispiel: Wirksames Soll 32 Stunden ergibt einen Korridor von 29 bis 35 Stunden. Andernfalls würde die bestätigte Sollreduzierung ihre Planungswirkung teilweise verlieren.

Antwort: ja

### D-03 – Untergrenze des normalen Wochenkorridors

Soll die Untergrenze wirklich zwingend sein, auch wenn nicht genügend zulässiger Bedarf existiert? Eine zwingende Mindeststundenzahl kann mit dem ebenfalls zwingenden Überbesetzungsverbot kollidieren.

**Empfehlung:** Die Untergrenze als weiche Regel mit Priorität hoch behandeln. Die App soll zu wenig geplante Stunden sichtbar melden, aber niemals unnötige Überbesetzung erzeugen oder die gesamte Generierung unmöglich machen.

Antwort: Wenn beide Grenzen Kollidieren, dann gewinnt das Überbesetzungsverbot über die Untergrenze. Dann werden im Planungsbericht die Mitarbeiter aufgelistet, welche zu wenig stunden bekommen haben und die Serviceleitung kann sie manuell extra einplanen. Die serviceleitung kann mehr personen einplanen als der bedarf vorschreibt. Dann wird es aber auch eine meldung geben. Der Plan ist aber auch bei Überbesetzung - wenn das so von der Serviceleitung bestätigt wurde - abnehmbar.

### D-04 – Obergrenze des normalen Wochenkorridors

Bleibt Wochen-Soll plus drei Stunden eine zwingende Obergrenze?

**Empfehlung:** Ja, sofern keine strengere tägliche, gesetzliche oder betriebliche Grenze greift. Eine Überschreitung darf nicht zur Bedarfsdeckung erzwungen werden.

Antwort: Ja. Wir behandeln das äquivalent zur untergrenze:
Wenn Bedarf nicht gedeckt werden konnte, dann gibt es im Planungsbericht die Meldungen, welche Bedarfe nicht gedackt werden konnten. Die Serviceleitung kann Mitarbeiter über ihrer Obergrenze einplanen. Dann wird es aber auch eine meldung geben. Der Plan ist aber auch bei überschrittener Obergrenzen - wenn das so von der Serviceleitung bestätigt wurde - abnehmbar.

### D-05 – Bewertung jeder einzelnen Woche

Wird der Korridor in einem Drei-Wochen-Plan für jede Woche getrennt geprüft, statt die Stunden über drei Wochen auszugleichen?

**Empfehlung:** Ja. Dies entspricht der bisherigen Übergabe aus System 03 und macht Abweichungen je Woche nachvollziehbar.

Antwort: ja

### D-06 – AH-Ziel von zehn Stunden

Welche Priorität besitzt das weiche Ziel, eine AH-Person möglichst auf zehn Stunden pro Woche zu bringen?

**Empfehlung:** Mittel innerhalb der nachgelagerten AH-Phase. Bedarfsdeckung und zwingende Regeln bleiben wichtiger; AH darf niemals bereits gedeckte Nicht-AH-Zuweisungen verdrängen.

Antwort: deine empfehlung

### D-07 – AH-Bereich zwischen zehn und zwölf Stunden

Darf AH bei verbleibendem zulässigem Bedarf mehr als zehn und höchstens zwölf Stunden erhalten?

**Empfehlung:** Ja. Der Einsatz ist zulässig, wird aber ab mehr als zehn Stunden als strukturierter Hinweis gemeldet. Mehr als zwölf Stunden bleibt zwingend ausgeschlossen.

Antwort: ja. sie dürfen auch mehr, aber nur vond er Serviceleitung manuell eingetragen

### D-08 – AH unter sechs Stunden

Soll eine AH-Person mit weniger als sechs geplanten Stunden nur einen Hinweis erzeugen, ohne dass dadurch andere Personen verdrängt oder Lücken künstlich erzeugt werden?

**Empfehlung:** Ja. Die bestätigte Schwelle ist eine Berichtsschwelle und keine Mindestbedingung.

Antwort: ja. seviceleitung kann entscheiden wie sie damit umgeht

### D-09 – Typ1-Dienst je Woche

Ist „mindestens ein vorgetragener Typ1-Dienst je nicht vollständig abwesender Woche“ eine zwingende Voraussetzung für den Start der Generierung oder lediglich ein deutlicher Hinweis?

**Empfehlung:** Als zwingende Eingabevoraussetzung behandeln. Ohne den benötigten manuellen Typ1-Dienst startet die automatische Generierung nicht; bei vollständiger Abwesenheit greift die bereits bestätigte Ausnahme mit Hinweis.

Antwort: deine empfehlung

### D-10 – Typ1-Bürozeit `B`

Zählt ein als Bürozeit markierter vorgetragener Typ1-Früh- oder Spätdienst vollständig zu den Typ1-Wochenstunden, obwohl er keinen Bedarf deckt?

**Empfehlung:** Ja, wie bereits fachlich beschrieben. Die Regeldefinition sollte Bedarfswirkung und Stundenwirkung getrennt ausdrücken.

Antwort: ja

## E – Freie Tage, Urlaub, Wochenenden und Arbeitsfolgen

### E-01 – Lage der zwei zusammenhängenden freien Tage

Müssen beide freien Tage vollständig innerhalb derselben Montag-bis-Sonntag-Woche liegen?

**Empfehlung:** Ja. Ein Paar Sonntag/Montag darf nicht gleichzeitig die Regel für beide angrenzenden Wochen erfüllen. Es kann aber bei der stabilen Verteilung zusätzlich positiv berücksichtigt werden.

Antwort: ja

### E-02 – Mehr als zwei freie Tage

Ist die Regel erfüllt, sobald mindestens ein zusammenhängendes Paar freier Tage existiert, auch wenn insgesamt mehr freie Tage geplant sind?

**Empfehlung:** Ja. Die Regel verlangt mindestens ein Paar und begrenzt die Gesamtzahl freier Tage nicht.

Antwort: ja

### E-03 – Anschluss an ein rotes `X`

Ist es gleichwertig, ob das zusätzliche schwarze `X` unmittelbar vor oder nach dem roten `X` liegt?

**Empfehlung:** Ja. Beide Varianten erfüllen denselben weichen Wunsch. Weitere Regeln und eine stabile Verteilung entscheiden bei Gleichstand.

Antwort: ja

### E-04 – Mehrere rote `X` in einer Woche

Wie wird priorisiert, wenn mehrere voneinander getrennte rote `X` vorhanden sind?

**Empfehlung:** Die Regel gilt als erfüllt, wenn mindestens eines der roten `X` zu einem zusammenhängenden Paar ergänzt wird. Zusätzliche Anschlüsse dürfen nur bei ansonsten gleichwertiger Planung bevorzugt werden.

Antwort: ja

### E-05 – Definition eines vollständigen freien Wochenendes

Bedeutet ein vollständiges freies Wochenende, dass Samstag und Sonntag beide ohne tatsächlichen Arbeitsabschnitt bleiben?

**Empfehlung:** Ja. Beide Kalendertage müssen frei von normalen Diensten, `D`, `Spr` und Typ1-Arbeitsabschnitten sein.

Antwort: ja

### E-06 – Kennzeichen für ein freies Wochenende

Dürfen rote oder schwarze `X` ein freies Wochenende bilden? Dürfen `U` oder `K` das ebenfalls?

**Empfehlung:** Rote und schwarze `X` zählen. `U` und `K` sollten für die allgemeine Regel „ein regulär freies Wochenende in drei Wochen“ nicht zählen; bei der besonderen Urlaubswochenendregel werden sie zur Erkennung des Urlaubsblocks verwendet, nicht als geplante freie Wochenendtage.

Antwort: deine empfehlung

### E-07 – Definition eines Urlaubsblocks

Was gilt als zusammenhängender Urlaub, für den ein Wochenende davor oder danach betrachtet wird?

**Empfehlung:** Mindestens ein oder mehrere unmittelbar aufeinanderfolgende Kalendertage mit `U`. Wochenende und Feiertage ohne `U` unterbrechen den Block nicht automatisch, müssen aber für die genaue Vorher-/Nachher-Regel ausdrücklich eingeordnet werden.

Antwort: Ein Urlaubsblock besteht aus den ausdrücklich mit U gekennzeichneten Urlaubstagen. Angrenzende Wochenend- oder Feiertage werden nicht automatisch als Urlaub behandelt. Sollen sie mit dem Urlaub einen zusammenhängenden freien Block bilden, trägt die Service-Leitung für diese Tage ausdrücklich ein rotes X ein.
Das rote X bleibt ein fest vorgegebener freier Tag, reduziert das Wochen-Soll nicht und zählt als einer der zwei freien Tage. Die Generierung versucht nach Möglichkeit, unmittelbar davor oder danach einen weiteren freien Tag als schwarzes X einzuplanen.
Beispiel: Eine Person hat Montag und Dienstag U. Mittwoch ist ein Feiertag und wird von der Service-Leitung mit einem roten X gesperrt. Dieses rote X zählt als erster freier Tag. Die Generierung kann Donnerstag als schwarzen freien Tag ergänzen. Leere Wochenend- oder Feiertagsfelder werden nicht automatisch gesperrt.

### E-08 – Welches Wochenende liegt vor oder nach Urlaub?

Ist stets das kalendarisch unmittelbar vorherige beziehungsweise nächste Samstag-Sonntag-Paar gemeint, unabhängig davon, an welchem Wochentag der Urlaub beginnt oder endet?

**Empfehlung:** Ja. Dadurch bleibt die Regel eindeutig und unabhängig von einer angenommenen Fünf-Tage-Woche.

Antwort: Das wochenende davor bekommt eine Person nur frei, wenn der urlaub an einem Montag beginnt. Äquivalent dazu ist das wochenende danach auch nur frei, wenn der urlaub an einem freitag aufhört.
Wenn ein urlaub von montag bis freitag eingetragen wurde, dann bitte versuchen das wochenende vorher und nachher frei zu tragen.

### E-09 – Zwingendes freies Wochenende nach Urlaub

Soll das freie Wochenende nach Urlaub wirklich selbst dann zwingend bleiben, wenn dadurch bestätigter Bedarf ungedeckt bleibt?

**Empfehlung:** Auf „weich, hoch“ ändern, sofern keine verbindliche betriebliche oder rechtliche Grundlage die zwingende Wirkung verlangt. Ein Erholungswunsch ist wichtig, sollte aber nicht ohne bestätigte Grundlage automatisch zu Unterdeckung führen.

Antwort: Ja. bestätigten bedarf der ungedeckt bleibt im Planungsbericht melden.

### E-10 – Freies Wochenende innerhalb von drei Wochen

Wird die Regel ausschließlich innerhalb jedes konkret generierten Drei-Wochen-Zeitraums geprüft oder rollierend über beliebige 21 Tage?

**Empfehlung:** Für die erste Fassung innerhalb des ausdrücklich gewählten Drei-Wochen-Zeitraums. Eine rollierende Prüfung über Planungsgrenzen würde zusätzliche Historie und eine andere fachliche Definition benötigen.

Antwort: innerhalb der gerade zu planenden 3 wochen

### E-11 – Höchstens sieben Arbeitstage in Folge

Unterbrechen `U`, `K`, rote `X` und schwarze `X` jeweils die Folge von Arbeitstagen? Wird jeder noch so kurze tatsächliche Einsatz als Arbeitstag gezählt?

**Empfehlung:** Ja zu beidem. Die Regel zählt Kalendertage mit Arbeit, nicht Arbeitsstunden. Die bereits bestätigte angrenzende Historie vor und nach dem Drei-Wochen-Zeitraum wird einbezogen.

Antwort: ja zu beidem

### E-12 – Priorität bei mehreren freien-Tage-Wünschen

Welche Regel entscheidet, wenn nicht gleichzeitig zwei freie Tage, ein Anschluss an ein rotes `X`, ein Urlaubswochenende und ein vollständiges freies Wochenende möglich sind?

**Empfehlung:** Zuerst zwingende Regeln; danach hoch priorisierte Regeln gemeinsam optimieren; danach mittel. Innerhalb derselben Priorität soll die Lösung möglichst viele vollständig erfüllte Verpflichtungen pro Person erreichen und anschließend gleichmäßig verteilen.

Antwort: deine empfehlung. Wir sollten das am besten noch in einer matrix sichtbar machen.

## F – Dienste, Doppeldienst, Springer und Einsatzfreigaben

### F-01 – Höchstens ein Doppeldienst pro Woche

Darf eine Person einen zweiten Doppeldienst in derselben Woche erhalten, wenn dadurch Bedarf gedeckt wird?

**Empfehlung:** Ja, weil die Grenze bereits als weich mit Priorität hoch bestätigt ist. Die Abweichung muss später strukturiert gemeldet werden.

Antwort: ja

### F-02 – Doppeldienste zusätzlich minimieren

Soll bei ansonsten gleichwertigen Plänen jede geringere Gesamtzahl von Doppeldiensten bevorzugt werden?

**Empfehlung:** Ja, mit Priorität mittel. Zuerst soll möglichst niemand die hoch priorisierte Grenze von einem Doppeldienst überschreiten; danach wird die Gesamtzahl minimiert.

Antwort: ja

### F-03 – Doppeldienst und tägliche Höchstarbeitszeit

Bleibt `D` mit seinen bestätigten zehn Arbeitsstunden zulässig, sofern Ruhezeit, Pausenmodell und alle weiteren Grenzen eingehalten werden?

**Empfehlung:** Ja, aber erst nach Klärung von B-06 bis B-08. Der Doppeldienst darf keine gesetzliche oder strengere betriebliche Grenze umgehen.

Antwort: ja

### F-04 – Exakte Notfallbedingung für `Spr`

Wann darf der Springer eingesetzt werden?

**Empfehlung:** Nur wenn er den ansonsten vollständig oder teilweise ungedeckten tatsächlichen Restaurant-Spätdienst nach dem Ende des zweiten Cafeteria-Bedarfs reduziert. Er darf nicht allein zur Stundenauffüllung oder zur Verbesserung einer niedrigeren weichen Regel eingesetzt werden.

Antwort: ja

### F-05 – Springer und verbleibende Teilunterdeckung

Bleibt der Restaurant-Zeitraum vor dem tatsächlichen Wechsel des Springers immer ausdrücklich ungedeckt?

**Empfehlung:** Ja. Der Springer darf nur die Zeit ab seinem wirklichen Eintreffen decken; die Diensttypbezeichnung darf frühere Unterdeckung nicht verbergen.

Antwort: ja

### F-06 – Konkurrenz zwischen regulärer Besetzung und Springer

Soll eine regulär für den Restaurant-Spätdienst freigegebene Person immer Vorrang vor `Spr` haben?

**Empfehlung:** Ja, sofern dadurch keine höher priorisierte Regel verschlechtert wird. `Spr` bleibt ein nachrangiger Notfalleinsatz.

Antwort: ja

### F-07 – Strukturierte Einsatzfreigaben

Sind reguläre Freigabe, nur manuell vorschlagsfähige Freigabe und nur nach bewusster Laufoption aktive Freigabe zwingende Grenzen der automatischen Planung?

**Empfehlung:** Ja. Der Regelkatalog verwendet die vorhandenen strukturierten Zustände und wertet weder sichtbare Typcodes noch deutsche Anzeigetexte aus. Eine deaktivierte oder nur manuell vorschlagsfähige Freigabe wird nicht automatisch genutzt.

Antwort: ja

### F-08 – Bewusste Laufoptionen

Werden besondere, standardmäßig deaktivierte Einsatzfreigaben vor jedem Planungslauf ausdrücklich aktiviert oder gilt eine Aktivierung dauerhaft?

**Empfehlung:** Pro Planungslauf als ausdrückliche Option in der späteren Planungsmomentaufnahme. Keine dauerhafte unbemerkte Aktivierung.

Antwort: Besondere, nur nach Laufoption zulässige Einsatzfreigaben müssen vor jedem Planungslauf ausdrücklich durch die Service-Leitung aktiviert werden. Sie sind standardmäßig ausgeschaltet und werden nicht dauerhaft für zukünftige Planungen freigegeben. Die Aktivierung gilt nur für den konkret ausgewählten Planungszeitraum und wird mit dessen Planungsmomentaufnahme festgehalten.
Die Aktivierung bedeutet lediglich, dass die Generierung diesen Einsatz bei Bedarf berücksichtigen darf. Sie garantiert keine Einteilung und setzt keine anderen Regeln außer Kraft. Rein manuell vorschlagsfähige Einsätze bleiben von der automatischen Generierung ausgeschlossen.

### F-09 – Typ1 in der automatischen Planung

Bleibt Typ1 ausnahmslos außerhalb der automatischen Einteilung, auch wenn sonst Bedarf ungedeckt wäre?

**Empfehlung:** Ja, zwingend. Typ1-Dienste werden ausschließlich manuell vorgetragen und von der Generierung geschützt.

Antwort: ja

### F-10 – Unabhängige Einsätze an zwei Orten

Darf eine Person außerhalb des bestätigten `Spr`-Musters an einem Tag nacheinander an Cafeteria und Restaurant eingesetzt werden?

**Empfehlung:** Nein. Standortübergreifende Kombinationen sind nur über das fachlich definierte `Spr`-Muster zulässig.

Antwort: nein. nur Typ1 darf überall ohne einschränkungen eingesetzt werden

## G – Verfügbarkeit, Planungsgrenzen und Historie

### G-01 – Aktive Mitarbeitende

Dürfen ausschließlich aktive Mitarbeitende in eine neue Planungsmomentaufnahme aufgenommen und automatisch geplant werden?

**Empfehlung:** Ja. Deaktivierte Personen bleiben nur zur Wahrung vergangener Pläne und Historie erhalten.

Antwort: ja

### G-02 – Leeres Tagesfeld

Bleibt ein leeres Tagesfeld einer aktiven Person grundsätzlich automatisch planbar, sofern keine andere Regel oder Einsatzfreigabe entgegensteht?

**Empfehlung:** Ja, entsprechend der bereits bestätigten System-06-Grundlage.

Antwort: ja

### G-03 – Zwingende Tageskennzeichen

Sind `U`, `K` und rote `X` ausnahmslos harte Sperren für den gesamten Kalendertag?

**Empfehlung:** Ja. Die Generierung darf sie weder überschreiben noch durch einen Dienst ergänzen.

Antwort: ja

### G-04 – Schwarze `X`

Sind schwarze `X` ausschließlich Ergebnisse der jeweiligen Generierung und bei einer bewussten Neugenerierung neu verteilbar, sofern sie nicht später als Zuweisung gesperrt wurden?

**Empfehlung:** Ja. Sie sind keine dauerhafte persönliche Verfügbarkeit und gehören erst ab System 08/09 in den Plan.

Antwort: ja

### G-05 – Benötigte Historie vor und nach dem Planungszeitraum

Welche Regeln benötigen Daten außerhalb der drei Wochen?

**Empfehlung:** Mindestens die Arbeitstagefolge, Ruhezeit am ersten und letzten Randtag sowie mögliche Sonn-/Feiertagsausgleiche strukturiert kennzeichnen. Jede Regel muss selbst angeben, wie viel Vor- und Nachlauf sie benötigt.

Antwort: Wir benötigen nur die Tage für die maximal 7 Tage Arbeitfolge

### G-06 – Fehlende historische Daten

Wie soll die App reagieren, wenn ältere Daten für eine langfristige Regel fehlen? Laut Master-Roadmap dürfen fehlende frühere Planwochen eine neue Generierung nicht grundsätzlich blockieren.

**Empfehlung:** Die Generierung im bestätigten Umfang zulassen, aber jede deshalb nicht vollständig prüfbare Regel mit einem strukturierten Hinweis kennzeichnen. Die App darf dann keine vollständige rechtliche Prüfung behaupten. Für unmittelbar prüfbare harte Grenzen gelten weiterhin keine Ausnahmen.

Antwort: deine Empfehlung

## H – Priorisierung, Verletzungen und manuelle Abweichungen

### H-01 – Hierarchische Prioritäten

Bleibt die Reihenfolge zwingend, ungedeckten Bedarf minimieren, hoch, mittel, niedrig und danach stabile Verteilung verbindlich?

**Empfehlung:** Ja. Die Stufen werden später getrennt oder mit nachweisbar dominanten Grenzen optimiert; bloße frei gewählte Strafpunkte reichen nicht.

Antwort: ja

### H-02 – Regeln innerhalb derselben Priorität

Wie werden mehrere Regeln derselben Priorität gegeneinander abgewogen?

**Empfehlung:** Jede Regel erhält ein fachlich nachvollziehbares Erfüllungsmaß. Zuerst wird die Zahl beziehungsweise das Ausmaß der Verletzungen innerhalb der Stufe minimiert; anschließend wird eine gleichmäßige Verteilung zwischen Personen bevorzugt. Keine Personennamen oder sichtbaren Typcodes als Tie-Breaker verwenden.

Antwort: deine empfehlung

### H-03 – Personengerechte Verteilung

Soll die stabile Schlussstufe vermeiden, dass dieselbe Person bei gleichwertigen Plänen wiederholt die ungünstigeren Dienste, Doppeldienste oder Wochenenden erhält?

**Empfehlung:** Ja, soweit dafür ausreichende strukturierte Historie vorhanden ist. Für die erste Fassung darf diese Stabilitätsregel keine höhere Priorität überstimmen.

Antwort: ja

### H-04 – Konflikt zwischen zwingenden Regeln

Was geschieht, wenn eine gesperrte manuelle Zuweisung einer anderen zwingenden Regel widerspricht?

**Empfehlung:** Die Generierung nicht starten und den konkreten Widerspruch strukturiert melden. Eine Sperre darf weder stillschweigend entfernt noch eine andere Hard Rule verletzt werden.

Antwort: deine empfehlung

### H-05 – Manuelle Verletzung weicher Regeln

Darf die Service-Leitung später eine weiche Regel bewusst verletzen und den geänderten Entwurf speichern?

**Empfehlung:** Ja, wie bereits grundsätzlich bestätigt. Vor der Abnahme bleibt die Abweichung mit Regelkennung und verständlicher Erklärung sichtbar.

Antwort: ja

### H-06 – Manuelle Verletzung zwingender Regeln

Darf die Service-Leitung später eine zwingende Regel manuell übergehen?

**Empfehlung:** Nein. Wenn ein realer betrieblicher Ausnahmefall erforderlich ist, benötigt er eine eigene ausdrücklich definierte und dokumentierte Ausnahmeart; ein allgemeiner „trotzdem speichern“-Schalter wäre zu riskant.

Antwort: ja, die serviceleitung darf bei manueller barebeitung alle regeln brechen. Die manualle bearbeitung danach kann alles machen. es werden entsprechende Meldungen gegeben

### H-07 – Regelverletzung und Berichtshinweis

Soll System 07 für jede Regel bereits stabile Ergebnisarten wie erfüllt, verletzt, nicht anwendbar und nicht vollständig prüfbar definieren?

**Empfehlung:** Ja. Zusätzlich werden stabile Ursachecodes und fachliche Parameter vorbereitet. Deutsche Sätze formuliert später Application beziehungsweise System 10.

Antwort: deine empfehlung

### H-08 – Unbekannte oder nicht übersetzte Regel

Blockiert eine unbekannte oder in System 09 noch nicht unterstützte Regel die Generierung sichtbar?

**Empfehlung:** Ja, zwingend. Andernfalls könnte die App einen scheinbar gültigen Plan erzeugen, obwohl eine bestätigte Regel ignoriert wurde.

Antwort: Ja. Enthält die Planungsmomentaufnahme eine unbekannte oder von der Planungsengine noch nicht unterstützte Regel, darf die automatische Generierung nicht starten beziehungsweise kein Ergebnis übernehmen. Die Regel darf niemals stillschweigend ignoriert werden.
Die App nennt die betroffene Regel anhand ihrer stabilen Kennung und meldet verständlich, dass diese Regel technisch noch nicht verarbeitet werden kann. Ein vorhandener Plan oder Entwurf bleibt unverändert erhalten.
Davon zu unterscheiden ist eine bekannte und korrekt umgesetzte zwingende Regel, durch die ein Bedarf nicht besetzt werden kann. In diesem Fall wird der zulässige Restplan weiterhin erzeugt und der ungedeckte Bedarf sichtbar ausgewiesen.

## I – Regelfassungen, Wirksamkeit und Nachvollziehbarkeit

### I-01 – Wirkung einer späteren Regeländerung

Soll eine spätere Änderung nur neue Generierungen und noch nicht abgenommene Entwürfe betreffen?

**Empfehlung:** Ja. Bereits abgenommene Planversionen behalten die damals verwendete Regelfassung unverändert.

Antwort: deine empfehlung

### I-02 – Regelmomentaufnahme

Soll System 08 jede verwendete Regel mit Kennung, Typ, Priorität und Parametern in die Planungsmomentaufnahme übernehmen?

**Empfehlung:** Ja. Nur so bleiben Generierung, manuelle Prüfung und spätere Planversion nachvollziehbar, auch wenn sich der aktuelle Katalog ändert.

Antwort: ja

### I-03 – Wirksamkeitsdatum

Benötigen Regeln oder Prioritäten ein eigenes Wirksamkeitsdatum?

**Empfehlung:** In der ersten Fassung nein, solange der Katalog nicht in der App bearbeitbar ist. Die verwendete Katalogfassung wird beim Planungslauf festgehalten. Bei späterer Bearbeitbarkeit wäre ein Wirksamkeits- und Revisionsmodell erforderlich.

Antwort: deine empfehlung

### I-04 – Katalogversion

Soll der gesamte Startkatalog eine stabile Fassung beziehungsweise Versionskennung besitzen?

**Empfehlung:** Ja. Eine technische Katalogversion erleichtert Migration, Diagnose und reproduzierbare Tests, ohne sichtbare Fachregeln an Dateinamen oder Programmversionen zu koppeln.

Antwort: deine empfehlung

### I-05 – Datenschutz in Regelmeldungen

Dürfen technische Protokolle Namen oder vollständige Planinhalte enthalten, wenn eine Regel verletzt wird?

**Empfehlung:** Nein. Technische Protokolle verwenden Regelkennungen und synthetisch beziehungsweise intern referenzierbare Kennungen. Personenbezogene Erläuterungen erscheinen nur lokal in der erforderlichen Bedienansicht.

Antwort: deine empfehlung

## J – Gemeinsame Beispielszenarien und Abschluss

### J-01 – Beispiel je Regel

Ist für jede Regel mindestens je ein synthetischer Fall für erfüllt, verletzt und nicht anwendbar ausreichend?

**Empfehlung:** Als Mindestumfang ja. Zusätzlich braucht jede Regel Grenzwertfälle unmittelbar unter, auf und über ihrem Zahlenwert.

Antwort: deine empfehlung

### J-02 – Kombinationsszenarien

Welche Kombinationen müssen zwingend gemeinsam geprüft werden?

**Empfehlung:** Mindestens:

- Wochenkorridor gegen fehlenden Bedarf und Überbesetzungsverbot,
- `U`/`K` gegen reduziertes Wochen-Soll und freie Tage,
- rotes `X` gegen zusammenhängende freie Tage,
- sieben Arbeitstage gegen Planungsgrenzen,
- Doppeldienst gegen Tagesstunden, Pause und Ruhezeit,
- Springer gegen Teilunterdeckung und reguläre Besetzung,
- AH-Ziel gegen Nicht-AH-Vorrang und verbleibenden Bedarf,
- Typ1-Sperre beziehungsweise Bürozeit gegen Stunden- und Bedarfswirkung,
- Sonn-/Feiertagsarbeit gegen Ersatzruhetag, soweit diese Regel in den Umfang aufgenommen wird.

Antwort: deine empfehlung

### J-03 – Anonymisierte betriebliche Beispiele

Kann die Service-Leitung für ungewöhnliche, aber wichtige Fälle kurze anonymisierte Beispiele liefern?

**Empfehlung:** Ja, ausschließlich mit Bezeichnungen wie „Person A, Typ30a“ und erfundenen Daten. Besonders wertvoll sind Fälle, in denen heute bewusst zwischen zwei nicht gleichzeitig erfüllbaren Wünschen entschieden wird.

Antwort: deine empfehlung

### J-04 – Vollständigkeitsbestätigung

Wer bestätigt am Ende, dass der Fragenkatalog alle für die erste Fassung benötigten betrieblichen Planungsregeln enthält?

**Empfehlung:** Die Service-Leitung bestätigt den praktischen Regelbestand. Gesetzliche, tarifliche und betriebsverfassungsrechtliche Einordnungen werden zusätzlich durch die dafür zuständige fachkundige Stelle bestätigt.

Antwort: die serviceleitung bestätigt den kompletten regelkatalog für die erste Fassung. Hier werden erstmal nur Ihre regeln berücksichtigt.

### J-05 – Freigabe für den Roadmap-Entwurf

Wann darf aus den Antworten eine Teil-Roadmap für System 07 erstellt werden?

**Empfehlung:** Erst wenn:

- A-01 bis A-08 die Systemgrenze eindeutig festlegen,
- B-01 bis B-12 den anwendbaren und bestätigbaren Rechts- und Betriebsrahmen klären,
- jede konkrete Regel eine eindeutige Wirkung, Priorität, Parameter und Ausnahmen besitzt,
- die kritischen möglichen Widersprüche bei Pausen, Wochenkorridor und Urlaubswochenende geklärt sind,
- ausschließlich synthetische Beispiele verwendet werden,
- der Auftraggeber den beantworteten Katalog ausdrücklich als Roadmap-Grundlage freigibt.

Antwort: deine empfehlung

## K – Folgefragen aus dem Verständnisabgleich

Die bisherigen Antworten sind vollständig gelesen. Die folgenden Folgefragen betreffen nur noch mehrdeutige Formulierungen, Widersprüche zwischen einzelnen Antworten oder Änderungen gegenüber den bisherigen Architektur- und Produktgrundlagen. Sie müssen vor dem Roadmap-Entwurf eindeutig beantwortet werden.

### K-01 – Abgrenzung von gesetzlichen und tariflichen Regeln

Soll für die erste Fassung ausdrücklich gelten:

> Die App prüft ausschließlich die gemeinsam bestätigten internen Planungsregeln. Sie erhebt keinen Anspruch, die vollständige Einhaltung gesetzlicher, tariflicher, betriebsverfassungsrechtlicher oder arbeitsvertraglicher Arbeitszeitregeln zu bestätigen.

**Empfehlung:** Ja. Externe Regeln dürfen später nur nach bestätigter Quelle und fachlicher Einordnung ergänzt werden. Aussagen wie „gesetzlich zulässig“ oder „tariflich vollständig geprüft“ werden in der ersten Fassung vermieden.

Antwort: ja

### K-02 – Gemeinte Zehn-Stunden-Ruhezeit

Ist mit B-08 gemeint, dass zwischen dem Ende des letzten Arbeitsabschnitts und dem Beginn des nächsten Arbeitsabschnitts mindestens zehn zusammenhängende Stunden liegen müssen?

Soll dies unabhängig von seiner rechtlichen oder tariflichen Einordnung als interne zwingende Regel der automatischen Planung gelten?

**Empfehlung:** Zweimal ja, falls dies die tatsächlich gewünschte interne Planungsregel ist. Die Formulierung lautet „mindestens zehn Stunden Ruhezeit“, nicht „maximal zehn Stunden“. Ohne bestätigte Quelle wird sie nicht als vollständiger Nachweis gesetzlicher oder tariflicher Zulässigkeit bezeichnet.

Antwort: nein. die ruhezeiten sollen nicht extra als regeln gelten. Die Arbeitszeiten sind von Der Serviceleitung generell so gewählt dass 10 stunden immer zutreffen. Wir müssen das nicht weiter kontrollieren.

### K-03 – Keine Ersatzruhetage und Zuschläge in der ersten Fassung

Soll B-09 ausschließlich bedeuten:

- Ersatzruhetage werden in der ersten App-Version weder verwaltet noch geprüft,
- Zuschläge werden ebenfalls nicht verwaltet,
- die App behauptet nicht, dass ein Zuschlag einen möglicherweise außerhalb der App bestehenden Ersatzruhetag rechtlich ersetzt?

**Empfehlung:** Ja. Zuschläge und externe Sonn-/Feiertagsregelungen bleiben vollständig außerhalb der ersten Fassung.

Antwort: ja

### K-04 – Zehn-Stunden-Beispiel und tägliche Höchstgrenze

War bei B-06 mit der beispielhaften zehnstündigen Einteilung der Doppeldienst `D` und nicht der Springer `Spr` gemeint?

Soll System 07 in der ersten Fassung keine allgemeine tägliche Höchstarbeitszeit definieren, während der bestätigte Doppeldienst mit zehn tatsächlichen Arbeitsstunden zulässig bleibt und weiterhin möglichst vermieden wird?

**Empfehlung:** Zweimal ja. `Spr` behält seine tatsächlichen Abschnitte und wird nicht pauschal als zehnstündiger Einsatz behandelt.

Antwort: ja

### K-05 – Fachliche Einordnung der Korridorgrenzen

Soll der normale Wochenkorridor für die automatische Planung wie folgt eingeordnet werden?

- Die Untergrenze ist weich mit Priorität hoch. Die Automatik versucht sie zu erreichen, darf bei fehlendem zulässigem Bedarf darunterbleiben und meldet die Unterschreitung.
- Die Obergrenze ist für die Automatik zwingend und wird automatisch niemals überschritten.
- Beide Grenzen dürfen bei einer späteren manuellen Bearbeitung nach Warnung und ausdrücklicher Bestätigung übergangen werden.

**Empfehlung:** Ja. Eine Untergrenze, die bei einer Kollision mit dem Überbesetzungsverbot verlieren darf, kann nicht zugleich eine ausnahmslos zwingende Regel sein.

Antwort: ja

### K-06 – Drei Stufen für automatische und manuelle Regeln

Soll die App folgende drei Stufen unterscheiden?

1. **Zwingend für die automatische Planung:** Die Generierung darf diese Regeln niemals verletzen.
2. **Manuell übersteuerbare Planungsregeln:** Die Service-Leitung darf beispielsweise Wochenkorridor, AH-Obergrenze, Einsatzfreigaben, freie Tage oder Doppeldienstgrenzen nach sichtbarer Warnung und ausdrücklicher Bestätigung übergehen.
3. **Nicht übersteuerbare Strukturregeln:** Keine zeitlichen Überschneidungen; nur vorhandene Personen, Dienste und Einsatzmuster; keine direkte Einteilung auf `U`, `K` oder rotes `X`; keine Veränderung einer abgenommenen Planversion. Ein vorhandenes Tageskennzeichen muss zuerst bewusst entfernt werden.

**Empfehlung:** Ja. Ein allgemeines „alle Regeln brechen“ darf keine widersprüchlichen oder technisch ungültigen Plandaten erzeugen.

Antwort:ja

### K-07 – Genaue Bedeutung manueller Überbesetzung

Soll manuelle Überbesetzung folgendermaßen funktionieren?

- Die automatische Generierung erzeugt niemals Überbesetzung.
- Die Service-Leitung darf bei der manuellen Bearbeitung zusätzliche Personen auf einen bereits vollständig gedeckten vorhandenen Dienst setzen.
- Der gespeicherte Personalbedarf wird dadurch nicht geändert.
- Jede zusätzliche Besetzung wird als Überbesetzung sichtbar gemeldet und muss vor der Abnahme ausdrücklich bestätigt werden.
- Die zusätzlichen Personen erhalten ihre tatsächlichen Dienststunden vollständig angerechnet.

**Empfehlung:** Ja. System 08 muss solche zusätzlichen manuellen Zuweisungen ausdrücklich modellieren, weil sie keinem noch freien Bedarfsplatz entsprechen.

Antwort: ja

### K-08 – Teildeckung nur als bestätigter Springer-Sonderfall

Soll für die erste Fassung gelten:

- Eine normale Zuweisung deckt immer den vollständigen tatsächlichen Zeitraum eines Bedarfsplatzes.
- Eine Person kann außerhalb eines bestätigten Einsatzmusters nicht nur einen frei gewählten Teil übernehmen.
- `Spr` ist die einzige Ausnahme: Der Springer deckt den Restaurant-Spätdienst erst ab seinem tatsächlichen Wechsel aus der Cafeteria.
- Der Restaurant-Zeitraum davor bleibt ausdrücklich ungedeckt.

**Empfehlung:** Ja. Dadurch wird keine allgemeine frei wählbare Teilzeitzuteilung eingeführt und der bestätigte Springer-Sonderfall bleibt sichtbar.

Antwort:ja

### K-09 – Unterschiedliche Wirkung von `U`, `K` und `X`

Soll C-09 konkret so verstanden werden?

- `U`, `K`, rote `X` und schwarze `X` unterbrechen jeweils eine Folge tatsächlicher Arbeitstage.
- Für die Regel „zwei zusammenhängende freie Tage“ zählen rote und schwarze `X`.
- `U` und `K` zählen nicht als die zwei regulär geplanten freien Tage.

**Empfehlung:** Ja. Die vier Kennzeichen verhindern zwar jeweils einen Arbeitstag, besitzen aber für Sollreduzierung und planbare Erholung unterschiedliche fachliche Bedeutungen.

Antwort: ja

### K-10 – Zusammenspiel von Urlaub, rotem `X` und angrenzenden Wochenenden

Ist die gemeinsame Bedeutung von E-07 bis E-09 folgende?

- Ein rotes `X` garantiert einen von der Service-Leitung vorgegebenen freien Tag.
- Ein leeres Wochenend- oder Feiertagsfeld wird nicht automatisch gesperrt.
- Beginnt ein Urlaub am Montag, versucht die Generierung mit hoher Priorität, das unmittelbar vorherige Wochenende freizuhalten.
- Endet ein Urlaub am Freitag, muss die automatische Planung das unmittelbar folgende Wochenende freihalten; nötigenfalls bleibt bestätigter Bedarf ungedeckt.
- Die Service-Leitung darf diese Wochenendregel bei einer späteren manuellen Bearbeitung nach Warnung übergehen.

**Empfehlung:** Ja. Eine vorherige rote Markierung bleibt die Möglichkeit, einen freien Tag unabhängig vom Optimierungsergebnis sicher vorzugeben.

Antwort: ja

### K-11 – Typ1 und die allgemeinen Tagesgrenzen

Bedeutet „Typ1 darf überall ohne Einschränkungen eingesetzt werden“ nur, dass Typ1 für alle bestätigten Dienste und Einsatzorte berechtigt ist?

Gelten für Typ1 trotzdem höchstens eine Zuweisung beziehungsweise ein zusammengesetztes Einsatzmuster pro Kalendertag und das Verbot zeitlicher Überschneidungen?

**Empfehlung:** Zweimal ja. Die umfassende Einsatzberechtigung darf keine widersprüchlichen gleichzeitigen Zuweisungen erzeugen.

Antwort: ja

### K-12 – Benötigte Historie und faire Verteilung

Soll G-05 zusammen mit B-08 und H-03 folgendermaßen gelten?

- Für höchstens sieben Arbeitstage in Folge werden die unmittelbar vorausgehenden Arbeitstage benötigt.
- Falls die interne Zehn-Stunden-Ruhezeit bestätigt wird, werden zusätzlich das Ende des letzten Arbeitsabschnitts vor dem Zeitraum und gegebenenfalls ein bereits vorhandener erster Arbeitsabschnitt danach benötigt.
- Die personengerechte Verteilung aus H-03 wird in der ersten Fassung nur innerhalb der aktuell geplanten drei Wochen bewertet und verwendet keine längerfristige Belastungshistorie.

**Empfehlung:** Ja. Eine planübergreifende faire Rotation wäre eine zusätzliche Regel mit weiterem Historienbedarf.

Antwort: ja

### K-13 – Prioritätsmatrix und synthetische Beispiele

Ist mit der in E-12 gewünschten Matrix zunächst eine verständliche Tabelle in der System-07-Dokumentation gemeint und keine zusätzliche Bedienoberfläche?

Darf die Entwicklung aus den bestätigten Regeln synthetische Beispielszenarien für J-03 entwerfen und sie anschließend der Service-Leitung zur fachlichen Prüfung vorlegen?

**Empfehlung:** Zweimal ja. Die Matrix wird Bestandteil des Verständnisabgleichs und der späteren Roadmap-Grundlage; dieselben freigegebenen Beispiele werden später für Domain- und Planning-Tests verwendet.

Antwort: ja

### K-14 – Nur dokumentierter oder sichtbarer Regelkatalog

Bei A-05 wurde die Empfehlung ohne eigene Oberfläche übernommen. Im Abschnitt „Besonders kritische Fragen“ steht zusätzlich, dass es einen Regelkatalog zur Ansicht geben kann. Soll eine schreibgeschützte Regelkatalog-Ansicht bereits zu System 07 gehören oder nur als mögliche spätere Erweiterung vorgemerkt werden?

**Empfehlung:** In System 07 zunächst nur den fachlichen Katalog, den Application-Lesevertrag und die dokumentierte Prioritätsmatrix umsetzen. Eine sichtbare WPF-Ansicht erst nach gesonderter Freigabe ergänzen; andernfalls würde System 07 ein zusätzliches visuelles Abnahmegate erhalten.

Antwort: ja. wir brauchen die matrizen erstmal für uns und für die Serviceleitung zur überprüfung vor der implementierung. Eine WPF ansicht brauchen wir noch nicht

## Konsolidiertes Verständnis zur Abnahme

Dieser Abschnitt löst die während des Fragebogens entstandenen scheinbaren Widersprüche auf. Er ersetzt nicht die Einzelantworten, sondern beschreibt ihre gemeinsame fachliche Bedeutung.

### Umfang und Aussagegrenze

- Die erste Fassung bildet ausschließlich die bestätigten internen Planungsregeln der Service-Leitung ab.
- Sie prüft keine vollständige gesetzliche, tarifliche oder sonstige externe Regelkonformität und darf eine solche auch nicht behaupten.
- Pausen, eine allgemeine tägliche Höchstarbeitszeit, Ruhezeiten zwischen Arbeitstagen, Ersatzruhetage, Zuschläge, Nachtarbeit, mehrere Arbeitgeber und besondere Personengruppen gehören nicht zum Regelkatalog der ersten Fassung.
- Feiertage werden weiterhin manuell über die Bedarfe und Tageskennzeichen behandelt; es gibt keine externe Feiertagsquelle.
- Der Regelkatalog ist fest definiert. Regelarten, Grenzwerte, Prioritäten und Aktivierung sind in der App nicht frei bearbeitbar und werden nicht in der Datenbank gepflegt.
- System 07 erhält keine WPF-Regelkatalogansicht. Die Dokumentation, der fachliche Katalog und ein Application-Lesevertrag genügen für diesen Schritt.

### Drei Regelwirkungen

| Wirkung | Bedeutung |
|---|---|
| Für die Automatik zwingend | Die Generierung verletzt die Regel niemals. Ist dadurch keine zulässige Besetzung möglich, bleibt Bedarf sichtbar ungedeckt. |
| Manuell übersteuerbare Planungsregel | Die Service-Leitung darf im Bearbeitungsmodus bewusst abweichen. Die App zeigt Regel, Auswirkung und Warnung und verlangt vor Speicherung beziehungsweise Abnahme eine ausdrückliche Bestätigung. |
| Nicht übersteuerbare Strukturregel | Die App speichert keinen innerlich widersprüchlichen Plan. Dazu gehören zeitliche Überschneidungen, mehr als eine Zuweisung oder ein zusammengesetztes Muster pro Person und Tag, unbekannte Personen, Dienste oder Muster, eine direkte Zuweisung auf `U`, `K` oder rotes `X` sowie die Veränderung einer bereits abgenommenen Planversion. Ein Tageskennzeichen muss vor einer Zuweisung bewusst entfernt werden. |

Die Service-Leitung kann damit fachliche Planungsgrenzen bewusst übergehen, aber keine technisch oder fachlich widersprüchlichen Plandaten erzeugen. Automatische und manuelle Bewertung verwenden dieselbe Regeldefinition; nur der zulässige Umgang mit einer Verletzung unterscheidet sich.

### Prioritätsmatrix der automatischen Planung

| Stufe | Regeln und Ziele der ersten Fassung |
|---|---|
| 1 – zwingend | Strukturregeln; `U`, `K` und rote `X` sperren den Tag; nur aktive Personen; Einsatzfreigaben und Laufoptionen; Typ1 nie automatisch; normaler Wochenkorridor höchstens Soll plus drei Stunden; AH höchstens zwölf Stunden; höchstens sieben Arbeitstage in Folge; Wochenende nach einem am Freitag endenden Urlaub frei; automatische Überbesetzung verboten; unbekannte oder nicht übersetzte Regeln blockieren den Lauf. |
| 2 – Bedarfsdeckung | Innerhalb aller zwingenden Regeln vollständig ungedeckten Bedarf minimieren. Ein regulärer Bedarfsplatz wird vollständig besetzt oder bleibt vollständig ungedeckt. Nur `Spr` darf den bestätigten Restaurant-Spätdienst ab dem tatsächlichen Wechsel teilweise decken; der frühere Zeitraum bleibt sichtbar ungedeckt. |
| 3 – hoch | Normalen Wochenkorridor mindestens Soll minus drei Stunden anstreben; zwei zusammenhängende freie Tage je Woche; ein rotes `X` möglichst zu einem freien Zweierblock ergänzen; Wochenende vor einem am Montag beginnenden Urlaub freihalten; höchstens ein `D` pro Person und Woche. |
| 4 – mittel | Mindestens ein vollständiges freies Wochenende innerhalb der geplanten drei Wochen; Zahl der Doppeldienste zusätzlich minimieren; AH möglichst auf zehn Stunden bringen. |
| 5 – niedrig | Derzeit ist keine eigene Regel mit niedriger Priorität bestätigt. Die Stufe bleibt Bestandteil des allgemeinen Regelmodells. |
| 6 – Stabilität | Bei ansonsten gleichwertigen Plänen Belastungen innerhalb der aktuell geplanten drei Wochen möglichst gleichmäßig verteilen. Es wird noch keine längerfristige Fairnesshistorie verwendet. |

Regeln derselben Priorität werden zunächst nach Zahl beziehungsweise Ausmaß ihrer Verletzungen und anschließend möglichst gleichmäßig zwischen Personen bewertet. Viele Regeln einer niedrigeren Stufe dürfen keine höhere Stufe überstimmen.

### Manuelle Abweichungen

- Manuell übersteuerbar sind insbesondere beide Seiten des normalen Wochenkorridors, die AH-Obergrenze, Einsatzfreigaben, Regeln zu freien Tagen und Urlaubswochenenden sowie die Doppeldienstgrenzen.
- Die Service-Leitung darf zusätzliche Personen auf einen bereits vollständig gedeckten vorhandenen Dienst setzen. Der Bedarf selbst bleibt unverändert, die zusätzlichen Stunden zählen vollständig und die Überbesetzung bleibt bis zur ausdrücklichen Bestätigung sichtbar.
- System 08 muss deshalb manuelle zusätzliche Zuweisungen unabhängig von freien Bedarfsplätzen modellieren.
- Eine gespeicherte Regelabweichung ändert weder die Regeldefinition noch Stammdaten oder zukünftige Planungsläufe.

### Stunden, Dienste und Sonderrollen

- Der normale Wochenkorridor wird je Person und Montag-bis-Sonntag-Woche gegen das nach `U` und `K` wirksame Soll geprüft. Die Untergrenze ist weich mit Priorität hoch; die Obergrenze ist für die Automatik zwingend. Beide sind manuell übersteuerbar.
- AH wird erst nach allen automatisch planbaren Nicht-AH-Typen eingesetzt. Zehn Stunden sind das mittlere Ziel; unter sechs und über zehn Stunden entstehen Hinweise; automatisch sind höchstens zwölf Stunden zulässig. Mehr als zwölf Stunden kann nur manuell und bestätigt eingetragen werden.
- Typ1 muss in jeder nicht vollständig abwesenden Woche mindestens einen geschützten vorgetragenen Dienst besitzen, wird nie automatisch eingeteilt und ist für alle bestätigten Dienste und Einsatzorte berechtigt. Die allgemeinen Tages- und Überschneidungsgrenzen gelten trotzdem.
- `D` bleibt mit zehn tatsächlichen Arbeitsstunden zulässig, soll höchstens einmal pro Woche vorkommen und zusätzlich minimiert werden. Eine allgemeine tägliche Höchstarbeitszeit wird in der ersten Fassung nicht geprüft.
- `Spr` bleibt ein samstäglicher Notfalleinsatz und darf nur eine sonst verbleibende Restaurant-Unterdeckung ab dem tatsächlichen Wechsel reduzieren. Besondere laufabhängige Freigaben sind vor jedem Lauf neu und ausdrücklich zu aktivieren.

### Freie Tage, Urlaub und Historie

- Rote und schwarze `X` zählen als regulär freie Tage und unterbrechen eine Arbeitsfolge. `U` und `K` unterbrechen ebenfalls die Arbeitsfolge, zählen aber nicht als die zwei regulär geplanten freien Tage.
- Ein rotes `X` ist ein von der Service-Leitung garantierter freier Tag. Wochenenden und Feiertage werden nur dann Teil eines freien Urlaubsblocks, wenn sie ausdrücklich mit rotem `X` markiert sind.
- Beginnt `U` am Montag, wird das unmittelbar vorherige Wochenende mit hoher Priorität freigehalten. Endet `U` am Freitag, hält die Automatik das unmittelbar folgende Wochenende zwingend frei und weist nötigenfalls ungedeckten Bedarf aus. Die spätere manuelle Bearbeitung darf davon nach Warnung abweichen.
- Für höchstens sieben Arbeitstage in Folge werden die unmittelbar vorhergehenden Arbeitstage benötigt. Weiterer Vor- oder Nachlauf ist in der ersten Fassung nicht erforderlich. Fehlt diese Historie, läuft die Planung mit einem Hinweis weiter und behauptet keine vollständige Prüfung.

### Übergaben an spätere Systeme

- System 08 übernimmt Regelkatalog, Katalogversion, benötigte Arbeitstagshistorie und Laufoptionen in eine unveränderliche Planungsmomentaufnahme. Es modelliert außerdem manuelle Zusatzbesetzungen.
- System 09 übersetzt jede automatische Regel eindeutig in Solver-Bedingung oder Optimierungsziel. Unbekannte oder nicht übersetzte Regeln blockieren die Generierung sichtbar.
- System 10 verwendet stabile Regel-, Ergebnis- und Ursachecodes für erfüllt, verletzt, nicht anwendbar und nicht vollständig prüfbar und formuliert daraus verständliche Meldungen.
- System 11 prüft manuelle Änderungen, blockiert Strukturverletzungen und verlangt für übersteuerbare Planungsregeln eine sichtbare Bestätigung.
- System 12 bewahrt Regelkatalogversion, verwendete Regelparameter, Laufoptionen und bestätigte manuelle Abweichungen in der unveränderlichen Planversion.

Die dazugehörige Architekturentscheidung ist in `docs/decisions/S07_RULE_CATALOG_AND_MANUAL_OVERRIDE_MODEL.md` dokumentiert und ebenfalls abgenommen.

## Ergebnis des Verständnisabgleichs

- Alle 85 Ausgangsfragen und alle 14 Folgefragen sind beantwortet.
- Es bestehen keine offenen fachlichen Rückfragen, die den Roadmap-Entwurf verhindern.
- Die Konsolidierung und die neue Architekturentscheidung wurden am 2026-09-16 ausdrücklich fachlich und architektonisch abgenommen.
- `docs/roadmaps/completed/S07_RULE_CATALOG_ROADMAP.md` ist die daraus entstandene kleinschrittige Teil-Roadmap und wurde mit System 07 am 2026-09-16 vollständig abgenommen und archiviert.
