# Fragen und Verständnisabgleich: Regelkatalog und Prioritäten

Status: Entwurf – noch nicht beantwortet oder als Grundlage für eine Roadmap freigegeben

Stand: 2026-09-15

## Zweck

Dieses Dokument sammelt die fachlichen und übergeordneten technischen Fragen für System 07 „Regelkatalog und Prioritäten“.

Die Fragen können anhand ihrer Kennung im Chat oder direkt in diesem Dokument beantwortet werden. Jede Frage enthält eine Empfehlung. Eine Empfehlung ist noch keine bestätigte Fachregel. Erst nach der Beantwortung werden die Angaben auf Widersprüche, Vollständigkeit, gesetzliche beziehungsweise betriebliche Quellen und Systemgrenzen geprüft.

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
- `docs/decisions/EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`
- `docs/roadmaps/completed/AVAILABILITY_ABSENCE_QUESTIONS.md`
- `docs/roadmaps/completed/AVAILABILITY_ABSENCE_ROADMAP.md`
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

Antwort:

### H-02 – Regeln innerhalb derselben Priorität

Wie werden mehrere Regeln derselben Priorität gegeneinander abgewogen?

**Empfehlung:** Jede Regel erhält ein fachlich nachvollziehbares Erfüllungsmaß. Zuerst wird die Zahl beziehungsweise das Ausmaß der Verletzungen innerhalb der Stufe minimiert; anschließend wird eine gleichmäßige Verteilung zwischen Personen bevorzugt. Keine Personennamen oder sichtbaren Typcodes als Tie-Breaker verwenden.

Antwort:

### H-03 – Personengerechte Verteilung

Soll die stabile Schlussstufe vermeiden, dass dieselbe Person bei gleichwertigen Plänen wiederholt die ungünstigeren Dienste, Doppeldienste oder Wochenenden erhält?

**Empfehlung:** Ja, soweit dafür ausreichende strukturierte Historie vorhanden ist. Für die erste Fassung darf diese Stabilitätsregel keine höhere Priorität überstimmen.

Antwort:

### H-04 – Konflikt zwischen zwingenden Regeln

Was geschieht, wenn eine gesperrte manuelle Zuweisung einer anderen zwingenden Regel widerspricht?

**Empfehlung:** Die Generierung nicht starten und den konkreten Widerspruch strukturiert melden. Eine Sperre darf weder stillschweigend entfernt noch eine andere Hard Rule verletzt werden.

Antwort:

### H-05 – Manuelle Verletzung weicher Regeln

Darf die Service-Leitung später eine weiche Regel bewusst verletzen und den geänderten Entwurf speichern?

**Empfehlung:** Ja, wie bereits grundsätzlich bestätigt. Vor der Abnahme bleibt die Abweichung mit Regelkennung und verständlicher Erklärung sichtbar.

Antwort:

### H-06 – Manuelle Verletzung zwingender Regeln

Darf die Service-Leitung später eine zwingende Regel manuell übergehen?

**Empfehlung:** Nein. Wenn ein realer betrieblicher Ausnahmefall erforderlich ist, benötigt er eine eigene ausdrücklich definierte und dokumentierte Ausnahmeart; ein allgemeiner „trotzdem speichern“-Schalter wäre zu riskant.

Antwort:

### H-07 – Regelverletzung und Berichtshinweis

Soll System 07 für jede Regel bereits stabile Ergebnisarten wie erfüllt, verletzt, nicht anwendbar und nicht vollständig prüfbar definieren?

**Empfehlung:** Ja. Zusätzlich werden stabile Ursachecodes und fachliche Parameter vorbereitet. Deutsche Sätze formuliert später Application beziehungsweise System 10.

Antwort:

### H-08 – Unbekannte oder nicht übersetzte Regel

Blockiert eine unbekannte oder in System 09 noch nicht unterstützte Regel die Generierung sichtbar?

**Empfehlung:** Ja, zwingend. Andernfalls könnte die App einen scheinbar gültigen Plan erzeugen, obwohl eine bestätigte Regel ignoriert wurde.

Antwort:

## I – Regelfassungen, Wirksamkeit und Nachvollziehbarkeit

### I-01 – Wirkung einer späteren Regeländerung

Soll eine spätere Änderung nur neue Generierungen und noch nicht abgenommene Entwürfe betreffen?

**Empfehlung:** Ja. Bereits abgenommene Planversionen behalten die damals verwendete Regelfassung unverändert.

Antwort:

### I-02 – Regelmomentaufnahme

Soll System 08 jede verwendete Regel mit Kennung, Typ, Priorität und Parametern in die Planungsmomentaufnahme übernehmen?

**Empfehlung:** Ja. Nur so bleiben Generierung, manuelle Prüfung und spätere Planversion nachvollziehbar, auch wenn sich der aktuelle Katalog ändert.

Antwort:

### I-03 – Wirksamkeitsdatum

Benötigen Regeln oder Prioritäten ein eigenes Wirksamkeitsdatum?

**Empfehlung:** In der ersten Fassung nein, solange der Katalog nicht in der App bearbeitbar ist. Die verwendete Katalogfassung wird beim Planungslauf festgehalten. Bei späterer Bearbeitbarkeit wäre ein Wirksamkeits- und Revisionsmodell erforderlich.

Antwort:

### I-04 – Katalogversion

Soll der gesamte Startkatalog eine stabile Fassung beziehungsweise Versionskennung besitzen?

**Empfehlung:** Ja. Eine technische Katalogversion erleichtert Migration, Diagnose und reproduzierbare Tests, ohne sichtbare Fachregeln an Dateinamen oder Programmversionen zu koppeln.

Antwort:

### I-05 – Datenschutz in Regelmeldungen

Dürfen technische Protokolle Namen oder vollständige Planinhalte enthalten, wenn eine Regel verletzt wird?

**Empfehlung:** Nein. Technische Protokolle verwenden Regelkennungen und synthetisch beziehungsweise intern referenzierbare Kennungen. Personenbezogene Erläuterungen erscheinen nur lokal in der erforderlichen Bedienansicht.

Antwort:

## J – Gemeinsame Beispielszenarien und Abschluss

### J-01 – Beispiel je Regel

Ist für jede Regel mindestens je ein synthetischer Fall für erfüllt, verletzt und nicht anwendbar ausreichend?

**Empfehlung:** Als Mindestumfang ja. Zusätzlich braucht jede Regel Grenzwertfälle unmittelbar unter, auf und über ihrem Zahlenwert.

Antwort:

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

Antwort:

### J-03 – Anonymisierte betriebliche Beispiele

Kann die Service-Leitung für ungewöhnliche, aber wichtige Fälle kurze anonymisierte Beispiele liefern?

**Empfehlung:** Ja, ausschließlich mit Bezeichnungen wie „Person A, Typ30a“ und erfundenen Daten. Besonders wertvoll sind Fälle, in denen heute bewusst zwischen zwei nicht gleichzeitig erfüllbaren Wünschen entschieden wird.

Antwort:

### J-04 – Vollständigkeitsbestätigung

Wer bestätigt am Ende, dass der Fragenkatalog alle für die erste Fassung benötigten betrieblichen Planungsregeln enthält?

**Empfehlung:** Die Service-Leitung bestätigt den praktischen Regelbestand. Gesetzliche, tarifliche und betriebsverfassungsrechtliche Einordnungen werden zusätzlich durch die dafür zuständige fachkundige Stelle bestätigt.

Antwort:

### J-05 – Freigabe für den Roadmap-Entwurf

Wann darf aus den Antworten eine Teil-Roadmap für System 07 erstellt werden?

**Empfehlung:** Erst wenn:

- A-01 bis A-08 die Systemgrenze eindeutig festlegen,
- B-01 bis B-12 den anwendbaren und bestätigbaren Rechts- und Betriebsrahmen klären,
- jede konkrete Regel eine eindeutige Wirkung, Priorität, Parameter und Ausnahmen besitzt,
- die kritischen möglichen Widersprüche bei Pausen, Wochenkorridor und Urlaubswochenende geklärt sind,
- ausschließlich synthetische Beispiele verwendet werden,
- der Auftraggeber den beantworteten Katalog ausdrücklich als Roadmap-Grundlage freigibt.

Antwort:

## Besonders kritische Fragen vor einer Roadmap

Die folgenden Punkte können Umfang oder Architektur von System 07 wesentlich verändern und sollten zuerst beantwortet werden:

1. A-02 bis A-06: Sind Regeln oder Prioritäten in der App bearbeitbar und benötigen sie deshalb UI, Speicherung und Historie?
2. B-01 und B-02: Welche tariflichen, betrieblichen oder sonstigen verbindlichen Quellen gelten und wer bestätigt ihre Auslegung?
3. B-07: Besitzen die bisherigen siebenstündigen Dienste eine echte Ruhepause, die bisher noch nicht im Dienstmodell abgebildet ist?
4. D-03: Bleibt die Mindestseite des Wochenkorridors trotz möglicher Kollision mit fehlendem Bedarf zwingend?
5. E-09: Bleibt das freie Wochenende nach Urlaub wirklich zwingend oder wird es weich mit Priorität hoch?
6. B-09 und G-06: Welche langfristigen Sonn-/Feiertags- und Ausgleichsregeln soll die erste Fassung trotz begrenzter Historie zuverlässig prüfen?

## Nächster Schritt nach der Beantwortung

Nach den Antworten werden:

1. bestätigte und noch offene Punkte getrennt,
2. Widersprüche zwischen Regeln und bereits bestehenden Fachmodellen aufgezeigt,
3. rechtlich begründete Regeln nur mit bestätigter Quelle als zwingend übernommen,
4. Regelarten, Geltungsbereiche, Parameter, Prioritäten und Beispielszenarien konsolidiert,
5. die Grenzen zu den Systemen 08 bis 12 nochmals geprüft,
6. erst danach eine eigene kleinschrittige Teil-Roadmap für System 07 entworfen.
