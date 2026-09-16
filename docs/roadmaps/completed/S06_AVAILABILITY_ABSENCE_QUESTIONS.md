# Fragen und Verständnisabgleich: Verfügbarkeiten und Abwesenheiten

Status: Fachlich beantwortet und als Grundlage für den Roadmap-Entwurf freigegeben

Stand: 2026-09-15

## Zweck

Dieses Dokument sammelt den abgeschlossenen Verständnisabgleich und die beantworteten Fachfragen für System 06 „Verfügbarkeiten und Abwesenheiten“.

Die Antworten können anhand ihrer Kennung im Chat oder direkt in diesem Dokument ergänzt werden. Erst danach werden sie auf Widersprüche und Systemgrenzen geprüft und als Grundlage für eine eigene Teil-Roadmap verwendet. Dieses Dokument ist noch keine Teil-Roadmap und gibt weder Implementierung noch Datenbank-, Bedien- oder Planungscode frei.

Alle Beispiele und späteren Tests verwenden ausschließlich erfundene Personen und synthetische Daten. Echte Namen, Krankheitsdetails, Dienstpläne oder andere personenbezogene Unterlagen werden für die Klärung nicht benötigt.

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
- die abgeschlossenen Fragen- und Roadmap-Dateien der Systeme 03 bis 05
- die am 2026-09-15 beschriebene abstrahierte Struktur der bisherigen Drei-Wochen-Tabelle; das lokale Referenzbild wird nicht als Projektdokumentation übernommen

## Bereits bestätigtes fachliches Verständnis

### 1. Zweck von System 06

Die bisherige Master-Roadmap nennt für einzelne Mitarbeitende und konkrete Planungszeiträume:

- Verfügbarkeit,
- Urlaub,
- Krankheit,
- Fortbildung,
- Wunschfrei,
- zeitliche Einschränkungen.

Die konkrete Beschreibung bestätigt davon den Ablauf mit `U`, `K` und einem von der Service-Leitung vorgegebenen roten `X`. Wunschfrei, Fortbildung und zeitliche Einschränkungen werden für die erste Fassung nicht benötigt. Ein gewünschter freier Tag wird stattdessen als verbindliches rotes `X` eingetragen. Ein schwarzes `X` ist keine Eingabe der Service-Leitung, sondern wird später von der Generierung als frei geplanter Tag gesetzt. Die bestätigten Angaben werden als strukturierte Planungsdaten verwendet. System 06 erzeugt noch keinen Dienstplan und enthält noch keine OR-Tools-Logik.

### 2. Trennung von Mitarbeiterstammdaten und Zeitraumdaten

Mitarbeitertyp, ungekürztes Wochen-Soll und Einsatzfreigaben bleiben Eigenschaften des gemeinsam referenzierten Mitarbeitertyps. Individuelle Verfügbarkeiten und Abwesenheiten werden davon getrennt für Mitarbeitende und Zeiträume gespeichert.

Eine Abwesenheit ändert deshalb weder den Mitarbeitertyp noch dauerhaft dessen ungekürztes Wochen-Soll. Das für eine konkrete Montag-bis-Sonntag-Woche wirksame Soll wird später aus dem ungekürzten Soll und der bestätigten Abwesenheitsregel berechnet.

### 3. Mitarbeiterhistorie

Sobald eine Person in einer Verfügbarkeit oder Abwesenheit verwendet wurde, darf sie nicht mehr endgültig gelöscht werden. Sie kann weiterhin deaktiviert werden; ihre historischen Fachdaten bleiben erhalten.

### 4. Datenschutz

Alle produktiven Verfügbarkeits-, Abwesenheits- und Krankheitsdaten bleiben lokal. Technische Protokolle enthalten keine Namen, Krankheitsgründe oder vollständigen Planinhalte. Für Krankheit ist keine Diagnose oder medizinische Erläuterung vorgesehen.

### 5. Grenze zu späteren Systemen

- System 06 definiert und speichert die Zeitraumdaten und klärt die fachliche Berechnung des wegen Abwesenheit wirksamen Wochen-Solls.
- System 07 bildet die bestätigten zwingenden und weichen Regeln zentral ab.
- System 08 übernimmt die benötigten Daten später in eine unveränderliche Planungsmomentaufnahme.
- System 09 verwendet diese Momentaufnahme bei der automatischen Generierung.
- System 10 erklärt daraus entstehende Konflikte und Lösungsmöglichkeiten.

System 06 muss die später benötigten Informationen eindeutig und strukturiert bereitstellen, nimmt die spätere Regel-, Solver- oder Konfliktlogik aber nicht vorweg.

### 6. Bestätigtes Zielbild vor der Generierung

Die Service-Leitung möchte die Eingaben unmittelbar in einer zusammenhängenden Ansicht für die drei zu generierenden Wochen vornehmen:

- ganz links steht je Zeile der Name und der bindende Mitarbeitertyp einer Person,
- rechts folgen 21 Tagesfelder für Montag bis Sonntag über drei aufeinanderfolgende Wochen,
- jedes Tagesfeld gehört eindeutig zu einer Person und einem konkreten Kalenderdatum,
- vor der Generierung trägt die Service-Leitung dort `U`, `K` oder ein rot markiertes `X` ein,
- in derselben Ansicht trägt sie die Arbeitszeiten beziehungsweise Dienste der Typ1-Person vor,
- `U`, `K`, das rote `X` und die Typ1-Zuweisungen sind für die automatische Generierung vollständig gesperrt und dürfen von ihr nicht verändert oder überschrieben werden,
- die Generierung kennzeichnet alle übrigen von ihr bestimmten freien Tage mit einem schwarzen `X`.

Ein vor der Generierung leeres Tagesfeld einer aktiven Person bedeutet nach der markierten Antwort A-01 grundsätzlich, dass die Person verfügbar ist. Nach der Generierung enthält ein solches Feld entweder einen Dienst oder ein von der Generierung gesetztes schwarzes `X`. Die Sollwirkung ist in Abschnitt C und im klärenden Abschluss bestätigt.

### 7. Bestätigte Tageskennzeichen

| Kennzeichen | Sichtbare Bedeutung | Wirkung auf die Generierung | Bestätigte Zusatzregel |
|---|---|---|---|
| `U` | Urlaub | Eingabe der Service-Leitung; an diesem ganzen Tag keine Einteilung | Tageswert des Mitarbeitertyps reduziert das Wochensoll, auch an Wochenenden und Feiertagen |
| `K` | krank | Eingabe der Service-Leitung; an diesem ganzen Tag keine Einteilung | gleicher Tageswert wie bei `U`; bei AH nicht auswählbar |
| `X`, rot markiert | fest vorgegebener freier Tag | Eingabe der Service-Leitung; vollständig für die Generierung gesperrt | keine Sollreduzierung; bewusste Korrektur durch die Service-Leitung bleibt möglich |
| `X`, schwarz | von der Generierung bestimmter freier Tag | keine Vorabeingabe und keine dauerhafte Sperre; Teil des erzeugten Plans | spätere Regelprioritäten und manuelle Planbearbeitung |

Die interne Bedeutung der roten Markierung muss das System nicht kennen. Es benötigt nur ein eigenes strukturiertes Merkmal, damit ein rotes `X` zuverlässig gespeichert, angezeigt und vom später generierten schwarzen `X` unterschieden werden kann. Die Unterscheidung darf technisch nicht ausschließlich aus einer Farbe oder einem formatierten Text abgeleitet werden.

Die Typ1-Eingabe in derselben Ansicht ist als Produktziel bestätigt. Fachlich bleibt sie eine vorgetragene und geschützte Zuweisung. Ihr Planmodell gehört gemäß Master-Roadmap zu System 08. Die System-06-Roadmap muss deshalb später ausdrücklich festlegen, welcher Teil der gemeinsamen Ansicht bereits in System 06 entsteht und welche Planfunktion erst System 08 ergänzt.

## Fragen zur fachlichen Bestätigung

## A – Grundmodell der Verfügbarkeit

### A-01 – Grundannahme für aktive Mitarbeitende

Was gilt, wenn für eine aktive Person in einem Planungszeitraum keine Verfügbarkeit und keine Abwesenheit eingetragen wurde?

- [X] Empfohlen: Die Person gilt grundsätzlich als verfügbar; nur Abweichungen werden eingetragen.
- [ ] Die Person gilt erst als verfügbar, wenn sie für den Zeitraum ausdrücklich ausgewählt wurde.
- [ ] Vor jeder Generierung gibt es zusätzlich eine eigene Auswahl der teilnehmenden Mitarbeitenden.
- [ ] Andere Regel.

Antwort: Bestätigt durch die markierte Auswahl und die neue Ablaufbeschreibung: Ein vor der Generierung leeres Tagesfeld einer aktiven Person gilt grundsätzlich als verfügbar. Die Generierung trägt dort entweder einen Dienst oder ein schwarzes `X` als generierten freien Tag ein.

### A-02 – Einmalige oder wiederkehrende Verfügbarkeit

Die Service-Leitung trägt `U`, `K` und rote `X` direkt für konkrete Kalendertage der drei zu generierenden Wochen ein. Werden daneben wiederkehrende persönliche Regeln benötigt?

- [X] Direkte Angaben für konkrete Kalenderdaten der drei angezeigten Wochen.
- [ ] Zusätzlich persönliche wiederkehrende Wochenregeln, beispielsweise „montags erst ab 10:00 Uhr“.
- [ ] Zusätzlich ein für einen begrenzten Datumsbereich wiederkehrendes Wochenmuster.
- [ ] Andere Form.

Antwort: Die direkte Eingabe für konkrete Kalenderdaten ist bestätigt. Ein rot markiertes `X` wird für jeden konkreten Tag neu eingetragen und nicht in einen folgenden Drei-Wochen-Zeitraum übernommen. Schwarze `X` werden nie vorgetragen, sondern von jeder Generierung neu bestimmt.

### A-03 – Ganze Tage und Zeitfenster

Sind `U`, `K` und das rote `X` jeweils ganztägig, oder werden dafür zusätzlich konkrete Zeitfenster benötigt?

Antwort: Ganztägig. Für diese Eingaben werden keine Teilzeiten benötigt. Auch das später generierte schwarze `X` gilt für den ganzen Tag.

### A-04 – Mehrere Zeitfenster an einem Tag

Werden neben den ganztägigen Kennzeichen überhaupt noch eingeschränkte Verfügbarkeiten mit einem oder mehreren Zeitfenstern benötigt?

Antwort: Für die erste Version nicht nötig.

### A-05 – Zeitraster

Falls zeitliche Einschränkungen entgegen dem aktuell beschriebenen ganztägigen Modell doch benötigt werden: In welcher Genauigkeit dürfen Anfangs- und Endzeiten eingegeben werden?

- [ ] Empfohlen passend zu den aktuellen Dienstzeiten: 30-Minuten-Schritte.
- [ ] 15-Minuten-Schritte.
- [ ] Minutengenau.
- [ ] Andere Genauigkeit.

Antwort: Für `U`, `K` und rotes `X` nicht erforderlich. Die Darstellungsform der vorgetragenen Typ1-Arbeitszeiten wird getrennt unter D-04 geklärt.

### A-06 – Zeitraum über Mitternacht

Dürfen mögliche spätere zeitliche Einschränkungen über Mitternacht reichen? Die ganztägigen Kennzeichen selbst benötigen keine Uhrzeit.

Empfehlung: Für die erste Fassung nein; ein Zeitraum gehört vollständig zu einem Kalendertag.

Antwort: Für den bestätigten ganztägigen Ablauf nicht erforderlich. Nur zu beantworten, falls A-04 zusätzliche Zeitfenster bestätigt.

### A-07 – Bezug zu Einsatzorten und Diensttypen

Sollen außer den bestätigten Tageseingaben individuelle Ausschlüsse einzelner Einsatzorte oder Diensttypen möglich sein?

Empfehlung: Verfügbarkeit beschreibt nur die mögliche Zeit. Einsatzorte und Diensttypen bleiben über die bindenden Einsatzfreigaben des Mitarbeitertyps geregelt, damit dieselbe Regel nicht an zwei Stellen gepflegt wird.

Antwort: Im beschriebenen Zielbild nicht vorgesehen. Einsatzfreigaben des Mitarbeitertyps genügen

### A-08 – Teilnahme an einem mehrwöchigen Planungslauf

Genügt es, eine Person bei Bedarf in allen 21 Tagesfeldern mit `U`, `K` oder roten `X` zu sperren, oder wird zusätzlich eine Aktion „für alle drei Wochen nicht einplanen“ benötigt?

Antwort: manuelle Kennzeichnung reicht erstmal.

## B – Abwesenheitsarten und ihre Bedeutung

### B-01 – Benötigte Arten

Für die erste Fassung sind folgende sichtbare Tageswerte bestätigt:

- [X] `U` – Urlaub
- [X] `K` – krank
- [X] rotes `X` – von der Service-Leitung fest vorgegebener und für die Generierung gesperrter freier Tag
- [X] schwarzes `X` – von der Generierung bestimmter freier Tag

Fortbildung und zeitliche Einschränkungen werden in der ersten Fassung nicht benötigt.

Antwort: Bestätigt. Wunschfrei wurde nachträglich aus der ersten Fassung entfernt. Ein gewünschter verbindlicher freier Tag wird als rotes `X` eingetragen. Das schwarze `X` ist keine Eingabe, sondern ein Ergebnis der Generierung.

### B-02 – Kein gesondertes Wunschfrei

Wird neben dem roten `X` ein gesondertes weiches Wunschfrei benötigt?

Antwort: Nein. Wunschfrei gehört nicht zur ersten Fassung. Die Service-Leitung verwendet für einen gewünschten, verbindlichen freien Tag das rote `X`.

### B-03 – Zwingende Wirkung

Die neue Beschreibung korrigiert die frühere Annahme, alle sichtbaren Kennzeichen seien gesperrt:

| Art | Zwingend nicht einplanen? | Anmerkung |
|---|---|---|
| `U` | Ja | Urlaub; keine Minusstunden wegen dieses Tages |
| `K` | Ja | krank |
| rotes `X` | Ja | fest vorgegebener freier Tag; wird von der Service-Leitung eingetragen |
| schwarzes `X` | Nein, nicht als Vorabeingabe vorhanden | wird erst von der Generierung als freier Tag gesetzt |

Antwort: `U`, `K`, rote `X` und vorgetragene Typ1-Dienste darf die automatische Generierung nicht überschreiben. Schwarze `X` werden von ihr erzeugt.

### B-04 – Ganztägige und teilweise Abwesenheit

Sind `U`, `K`, rotes `X` und schwarzes `X` ausschließlich für ganze Kalendertage vorgesehen?

Antwort: Ja, ganztägig.

### B-05 – Mehrtägige Erfassung

Die fachlichen Daten liegen pro Person und konkretem Tagesfeld vor. Soll die Bedienung trotzdem mehrere aufeinanderfolgende Tagesfelder in einem Vorgang mit demselben Kennzeichen füllen können?

Antwort: Nein, erstmal brauchen wir das nicht. Einfache bedienung pro tag reicht für den anfang.

### B-06 – Wochenenden innerhalb eines Zeitraums

Können `U`, `K` und rote `X` genauso an Samstag und Sonntag eingetragen werden wie an Werktagen? Kann die Generierung dort ebenso schwarze `X` setzen?

Antwort: ja

### B-07 – Zusätzliche Notiz

Wird neben dem Tageskennzeichen eine optionale fachliche Notiz benötigt?

Bestätigt ist bereits: Die interne Zusatzinformation hinter einem roten `X` ist für das System nicht wichtig und wird nicht erfasst. Auch für `K` werden keine medizinischen Diagnosen oder detaillierten Krankheitsgründe gespeichert.

Antwort: keine weitere notizen für den anfang.

### B-08 – Sichtbare Bezeichnung einer Krankheit

Soll Krankheit in der Drei-Wochen-Ansicht ausschließlich als `K` erscheinen oder zusätzlich ausgeschrieben erklärt werden?

Antwort: Das sichtbare Kennzeichen `K` ist bestätigt. Kein weiterer hinweis erstmal

## C – Reduktion des Wochen-Solls

### C-01 – Welche Angaben reduzieren das Soll?

Für `U` und `K` ist ein gemeinsamer ganztägiger Abwesenheitswert je Mitarbeitertyp vorgegeben. Dieser Wert soll am Mitarbeitertyp einstellbar sein. Das ungekürzte Wochen-Soll des Typs bleibt bestehen; für die konkrete Woche wird daraus ein entsprechend reduziertes wirksames Soll berechnet.

| Tageswert | Wirkung auf das wirksame Wochen-Soll |
|---|---|
| `U` | Abwesenheits-Tageswert des Mitarbeitertyps abziehen; dadurch entstehen wegen dieses Urlaubstags keine Minusstunden. |
| `K` | denselben Abwesenheits-Tageswert des Mitarbeitertyps abziehen |
| rotes `X` | freier Tag, keine Sollreduktion |
| schwarzes `X` | von der Generierung geplanter freier Tag, keine Sollreduktion |

Antwort: Für `U` und `K` gelten dieselben einstellbaren Mitarbeitertyp-Tageswerte. Rote und schwarze `X` reduzieren das Wochen-Soll nicht; schwarze `X` sind Teil der normalen Generierung.

### C-02 – Einstellbarer Abwesenheits-Tageswert je Mitarbeitertyp

Die Service-Leitung hat folgende Werte für einen ganzen `U`- oder `K`-Tag genannt:

| Mitarbeitertyp | Tageswert für `U` und `K` |
|---|---:|
| `Typ20` | 4 Stunden |
| `Typ20a` | 4 Stunden |
| `Typ25` | 5 Stunden |
| `Typ25a` | 5 Stunden |
| `Typ30` | 6 Stunden |
| `Typ30a` | 6 Stunden |
| `Typ35` | 7 Stunden |
| `Typ35a` | 7 Stunden |
| `Typ1` | 8 Stunden |
| `TypAH1` | kein Tageswert; `U` und `K` sind nicht auswählbar |
| `TypAH2` | kein Tageswert; `U` und `K` sind nicht auswählbar |

Der Wert wird als strukturierter, bearbeitbarer Wert des gemeinsam referenzierten Mitarbeitertyps geführt und nicht aus dessen sichtbarem Code berechnet. Eine spätere Änderung gilt für nachfolgende Berechnungen aller zugeordneten Mitarbeitenden; bereits abgenommene Planversionen bleiben unverändert.

Die aktuell vorhandenen Starttypen enthalten `Typ20`, `Typ20a` und `Typ25a` noch nicht. Ihre Ergänzung benötigt neben dem Wochen-Soll auch Namen, Einsatzfreigaben und Planungsrichtlinie und darf nicht allein aus dem Buchstaben `a` abgeleitet werden.

Antwort: Die Tabelle und die Einstellbarkeit je Mitarbeitertyp sind bestätigt. Die AH-Bedeutung, die zusätzlichen Typdefinitionen und alle Randfälle sind in Abschnitt H abschließend geklärt.

### C-03 – Teil eines Tages

Die bestätigten Eingaben sind ganztägig. Werden für die erste Fassung dennoch teilweise Abwesenheiten oder stundenweise Einschränkungen benötigt?

Antwort: Im jetzt beschriebenen Ablauf nicht vorgesehen. A-04 bestätigt, dass keine zusätzlichen Zeitfenster benötigt werden.

### C-04 – Wochentage ohne regelmäßige Fünf-Tage-Woche

Bestätigt ist ein eigener Tageswert je Mitarbeitertyp. Wird dieser Wert an jedem zulässigen `U`- oder `K`-Tag gleich angewendet, unabhängig davon, an welchen Wochentagen die Person normalerweise arbeitet?

Antwort: Der Tageswert ist je Mitarbeitertyp festgelegt und nicht von einem regelmäßig gearbeiteten Wochentag abgeleitet. Er gilt gemäß H-02 auch an Wochenenden und Feiertagen.

### C-05 – Samstag, Sonntag und Feiertag

Wird der Mitarbeitertyp-Tageswert für `U` und `K` auch an Samstag, Sonntag und Feiertagen abgezogen? Rote und schwarze `X` werden als freie Tage eingeordnet; die genaue Bestätigung steht in H-02 und H-03.

Antwort: In H-02 zusammengeführt.

### C-06 – Mehrwöchige Abwesenheit

Soll das wirksame Soll in der Drei-Wochen-Ansicht für jede Montag-bis-Sonntag-Woche getrennt berechnet werden?

Empfehlung: Ja, weil Wochen-Soll und spätere Stundenbewertung bereits je einzelner Planungswoche definiert sind.

Antwort: Ja. Das wirksame Soll wird entsprechend der bestätigten Montag-bis-Sonntag-Bewertung für jede der drei Wochen getrennt berechnet.

### C-07 – Untergrenze und Rundung

Soll das wirksame Wochen-Soll mindestens null Minuten betragen, falls die Summe mehrerer `U`- und `K`-Tageswerte das ungekürzte Wochen-Soll erreicht oder überschreitet?

Empfehlung: Mindestens null Minuten. Bei den aktuell genannten vollen Stunden ist keine Rundung erforderlich; spätere einstellbare Werte sollten minutengenau gespeichert werden.

Antwort: In H-02 zusammengeführt; empfohlen ist eine Untergrenze von null Minuten.

### C-08 – Bereits vorgetragener oder gespeicherter Dienst

Falls in einem Tagesfeld bereits ein vorgetragener Typ1-Dienst steht und anschließend `U`, `K` oder ein rotes `X` gewählt wird: Soll die Änderung blockiert werden oder erst nach ausdrücklicher Bestätigung den Dienst ersetzen?

Hinweis: Die endgültige Behandlung gespeicherter Plan-Zuweisungen gehört zu den späteren Plan- und Bearbeitungssystemen. System 06 muss aber festlegen, welche strukturierten Daten und Hinweise dafür übergeben werden.

Antwort: Die App zeigt den vorhandenen Typ1-Dienst und den neuen Tageswert. Erst nach ausdrücklicher Bestätigung wird der Dienst ersetzt; Abbrechen lässt alles unverändert. Beide Einträge dürfen niemals gleichzeitig wirksam sein.

### C-09 – Anonymisierte Rechenbeispiele

Bitte möglichst drei erfundene Beispiele angeben:

1. `Typ25` mit einem `U`-Tag: vom ungekürzten Soll von 25 Stunden verbleiben 20 zu planende Stunden,
2. `Typ30` mit zwei `K`-Tagen: vom ungekürzten Soll von 30 Stunden verbleiben 18 zu planende Stunden,
3. ein rotes `X` und später von der Generierung gesetzte schwarze `X`: die genaue Stundenwirkung des roten `X` wird in H-03 bestätigt.

Diese Beispiele folgen der naheliegenden Auslegung der neuen Tageswerte. Bitte in H-02 bestätigen, dass genau so gerechnet wird.

Antwort: Die Beispiele werden mit H-02 und H-03 bestätigt.

## D – Sonderfall Typ1

Die Typ1-Person trägt ihre Arbeitszeiten beziehungsweise Dienste vor der Generierung in derselben Drei-Wochen-Ansicht ein. Diese Einträge sind vollständig geschützt und werden von der Generierung weder verändert noch überschrieben.

### D-01 – Vollständig abwesende Woche

Wenn die einzige aktive Typ1-Person eine vollständige Woche abwesend ist: Darf für diese Woche ausnahmsweise eine Generierung ohne vorgetragenen Typ1-Dienst gestartet werden?

- [X] Empfohlen: Ja, wenn die vollständige Abwesenheit bestätigt ist; die App zeigt einen deutlichen Hinweis.
- [ ] Nein, die Generierung bleibt auch dann blockiert.
- [ ] Die Service-Leitung muss die Ausnahme vor jedem Lauf zusätzlich bestätigen.
- [ ] Andere Regel.

Antwort: Ja. Die markierte Empfehlung gilt: Bei einer vollständig bestätigten Abwesenheitswoche darf ohne vorgetragenen Typ1-Dienst generiert werden; die App zeigt einen deutlichen Hinweis.

### D-02 – Teilweise abwesende Woche

Was gilt, wenn Typ1 nur an einzelnen ganzen Tagen mit `U`, `K` oder einem roten `X` gesperrt ist? Bleibt die Voraussetzung „mindestens ein vorgetragener Typ1-Dienst je Woche“ unverändert bestehen?

Antwort: Ja.

### D-03 – Widerspruch mit vorgetragenem Typ1-Dienst

Was soll geschehen, wenn ein neues Tageskennzeichen einen bereits vorgetragenen und automatisch geschützten Typ1-Dienst im selben Tagesfeld ersetzen würde?

Antwort: Die App zeigt deutlich den vorhandenen Typ1-Dienst und das neue Kennzeichen. Nach ausdrücklicher Bestätigung wird der Dienst entfernt und beispielsweise K eingetragen. Abbrechen lässt alles unverändert.

### D-04 – Inhalt eines vorgetragenen Typ1-Tagesfelds

Was trägt die Service-Leitung für Typ1 konkret ein?

- [X] Auswahl eines bereits definierten normalen Diensttyps beziehungsweise Einsatzmusters; dessen tatsächliche Zeit wird übernommen.
- [ ] Freie Eingabe einer Anfangs- und Endzeit ohne Diensttyp.
- [ ] Auswahl eines Diensttyps mit ausdrücklich anpassbarer tatsächlicher Zeit.
- [ ] Andere Eingabe.

Empfehlung: Einen vorhandenen Diensttyp oder ein vorhandenes Einsatzmuster auswählen und nur bei einem bestätigten Bedarf die tatsächliche Zeit ausdrücklich anpassen. Dadurch bleiben Bedarf, Einsatzort und bestehende Dienstdefinitionen nachvollziehbar.

Antwort: Die markierte Auswahl gilt: Die Service-Leitung wählt einen bereits definierten normalen Diensttyp beziehungsweise ein Einsatzmuster; dessen tatsächliche Zeit wird übernommen.

### D-05 – Typ1-Voraussetzung für alle drei Wochen

Muss vor der gemeinsamen Generierung in jeder der drei angezeigten Wochen mindestens ein Typ1-Dienst eingetragen sein, sofern Typ1 in der betreffenden Woche nicht vollständig abwesend ist?

Antwort: Ja

### D-06 – Technische Systemgrenze der gemeinsamen Ansicht

Die gemeinsame Drei-Wochen-Ansicht ist als Produktziel bestätigt. Das fachliche Planmodell für vorgetragene Typ1-Zuweisungen gehört laut Master-Roadmap zu System 08. Ist folgende Aufteilung richtig?

- System 06 baut die Drei-Wochen-Erfassung für `U`, `K` und rote `X` und bereitet die gemeinsame Zellstruktur beziehungsweise Übergabe vor.
- System 08 ergänzt in derselben Ansicht die vorgetragenen Typ1-Dienste und die späteren Plan-Zuweisungen.
- Die Service-Leitung erhält spätestens vor der ersten Generierung die vollständig gemeinsame Ansicht; es entsteht keine dauerhaft getrennte zweite Bedienoberfläche.

Antwort: Ja, die vorgeschlagene Aufteilung ist richtig.

## E – Überschneidungen, Korrekturen und Historie

### E-01 – Überschneidende Angaben

Dürfen für dasselbe Tagesfeld nacheinander unterschiedliche Eingaben gespeichert werden, beispielsweise zuerst `U` und später `K`, oder ersetzt die neue Auswahl immer den bisherigen, noch nicht verwendeten Wert?

Antwort: Pro Person und Kalendertag gilt immer genau eine Eingabe. Eine neue Auswahl ersetzt den vorherigen Wert. Vor dem Ersetzen zeigt die App deutlich an, was geändert wird.

### E-02 – Vorrang bei Überschneidungen

Falls eine Änderung von `U` zu `K` oder zu einem roten `X` erfolgt: Gilt unmittelbar nur die zuletzt gespeicherte Auswahl? Muss die vorherige Auswahl bereits vor einer Generierung historisch erhalten bleiben?

Antwort: Nur die letzte Kennzeichnung gilt.

### E-03 – Korrektur einer Abwesenheitsart

Soll beispielsweise `U` nachträglich in `K` geändert werden können? Muss eine frühere Fassung erst dann nachvollziehbar erhalten bleiben, wenn sie bereits Bestandteil eines erzeugten oder abgenommenen Plans war?

Antwort: ja

### E-04 – Löschen oder Aufheben

Unter welchen Bedingungen darf ein noch nicht verwendeter Eintrag vollständig gelöscht werden? Soll ein bereits in einer Planmomentaufnahme verwendeter Eintrag nur noch durch eine neue Korrektur aufgehoben werden können?

Empfehlung: Noch nicht fachlich verwendete Fehleingaben dürfen mit Bestätigung gelöscht werden; verwendete Daten bleiben für die historische Nachvollziehbarkeit erhalten.

Antwort: deine empfehlung

### E-05 – Vergangene Zeiträume

Dürfen Verfügbarkeiten und Abwesenheiten für vergangene Kalenderdaten neu angelegt oder verändert werden? Falls ja: bis zu welchem fachlichen Abschlusszustand?

Antwort: Die spätere Korrektur vergangener Zeiträume bleibt ohne feste zeitliche Grenze möglich. Eine bereits abgenommene Version wird dabei niemals überschrieben; die Bearbeitung erzeugt einen neuen Entwurf und nach erneuter Abnahme eine neue Version.

### E-06 – Deaktivierte Mitarbeitende

Sollen für deaktivierte Mitarbeitende neue zukünftige Einträge verhindert werden, während frühere Angaben weiterhin angezeigt werden?

Empfehlung: Ja. Eine spätere Reaktivierung verwendet weiterhin dieselbe stabile Mitarbeiterkennung.

Antwort: ja

## F – Bedienung und Darstellung

### F-01 – Bestätigte Hauptansicht

- [X] Eine zusammenhängende Tabelle zeigt alle Mitarbeitenden als Zeilen.
- [X] Ganz links stehen Name und bindender Mitarbeitertyp.
- [X] Danach folgen 21 Tagesspalten für drei vollständige Wochen von Montag bis Sonntag.
- [X] Die Service-Leitung trägt `U`, `K` und rote `X` vor der Generierung direkt in die Tagesfelder ein.
- [X] Die Typ1-Arbeitszeiten beziehungsweise Dienste erscheinen im selben Raster.

Antwort: Bestätigt. Das bereitgestellte Bild dient ausschließlich als visuelle Orientierung für diese abstrahierte Tabellenstruktur; es wird nicht als Fachdatenquelle oder Projektdokumentation übernommen.

### F-02 – Auswahl und Wechsel des Drei-Wochen-Zeitraums

Beginnt die Ansicht immer mit einem ausgewählten Montag und zeigt ab dort genau 21 Kalendertage? Soll mit „Vorherige drei Wochen“, „Nächste drei Wochen“ und einer Datumsauswahl gewechselt werden können?

Antwort:ja

### F-03 – Bedienung eines einzelnen Tagesfelds

Wie soll ein Tagesfeld bearbeitet werden?

- [ ] Tageswert direkt über die Tastatur eingeben.
- [ ] Zelle auswählen und `U`, `K`, rotes `X`, Typ1-Dienst oder „leer“ aus einer Liste wählen.
- [X] Beide Bedienwege.
- [ ] Andere Bedienung.

Empfehlung: Auswahl aus klaren Aktionen plus gut sichtbare Tastaturkürzel, damit keine unbekannten Freitexte als Planungsdaten entstehen.

Antwort: deine empfehlung

### F-04 – Mehrere Tagesfelder füllen

Soll dasselbe Kennzeichen für mehrere markierte Tage derselben Person in einem Vorgang gesetzt werden können, beispielsweise `U` für fünf aufeinanderfolgende Tage?

Antwort: keine gleichzeitige bearbeitung für die erste version. 5 aufeinanderfolgende tage müssen 5 mal separat pro tag bearbeitet werden

### F-05 – Sofort sichtbare Informationen

Bestätigt sichtbar sind Name, Mitarbeitertyp, Wochentag, Datum und das jeweilige Tageskennzeichen beziehungsweise der Typ1-Dienst. Soll zusätzlich je Person und Woche das ungekürzte und das wirksame Wochen-Soll unmittelbar angezeigt werden?

Antwort: ja

### F-06 – Warnungen vor dem Speichern

Welche Situationen sollen bereits beim Speichern verständlich gewarnt oder blockiert werden?

- [X] Ersetzen eines anderen Kennzeichens im selben Tagesfeld.
- [X] Ersetzen eines bereits vorgetragenen Typ1-Dienstes.
- [X] Eintrag für eine deaktivierte Person.
- [X] Eingabe eines unbekannten Kürzels.
- [ ] Andere Situation.

Antwort: Eingaben eines unbekannten Kürzels sind nicht möglich. In der Eingabeansicht erscheinen nur aktive Mitarbeitende. Für deaktivierte Mitarbeitende kann dort weder ein Dienst noch `U`, `K` oder rotes `X` eingetragen werden.

### F-07 – Bestätigung beim Entfernen

Soll das Leeren eines noch nicht verwendeten Tagesfelds sofort möglich sein oder eine Bestätigung mit Person, Kennzeichen und Datum verlangen?

Antwort: Die App sollte eine Bestätigung verlangen

### F-08 – Darstellung des roten X

Wie soll der Unterschied sichtbar werden?

- [X] Roter Buchstabe `X` auf normalem Hintergrund.
- [ ] Normales `X` auf rotem Hintergrund.
- [ ] Roter Buchstabe und roter beziehungsweise deutlich abgesetzter Hintergrund.
- [ ] Andere Darstellung.

Unabhängig von der Farbe speichert das System den Unterschied als eigenes Merkmal. Dadurch bleibt er auch bei Darstellungsänderungen und späteren Exporten eindeutig.

Antwort: Die markierte Auswahl gilt: Das rote `X` erscheint als roter Buchstabe auf normalem Hintergrund.

### F-09 – Gemeinsame Ansicht vor und nach der Generierung

Soll dieselbe Drei-Wochen-Tabelle nach der Generierung die erzeugten Dienste und schwarzen `X` in den zuvor verfügbaren Feldern zeigen, während `U`, `K`, rote `X` und die Typ1-Dienste unverändert stehen bleiben?

Antwort: ja. diese soll danach noch optional bearbeitet werden können. Erst nach bestätigung soll das der feste Plan für die nächsten 3 wochen sein.

### F-10 – Sichtbarer Name der Ansicht

Soll diese Ansicht in der App „Wochenbericht“, „Drei-Wochen-Plan“, „Dienstplan“ oder anders heißen?

Hinweis: In der vorhandenen Projektdokumentation bezeichnet „Wochenstundenbericht“ bereits die spätere Zusammenfassung von Soll, geplanten Stunden und Abweichung je Person. Die Eingabeansicht sollte davon sprachlich eindeutig unterscheidbar bleiben.

Antwort: So wie auch hier im Beispiel "Dienstplan SER"

## G – Fachliche Beispiele und Vollständigkeit

### G-01 – Typische Beispielwoche

Bitte beschreibe mit erfundenen Personen eine typische Woche, in der mindestens folgende Fälle vorkommen:

- `U`,
- mehrere aufeinanderfolgende `K`-Tage,
- ein von der Service-Leitung eingetragenes rotes `X`,
- ein von der Generierung gesetztes schwarzes `X`,
- mindestens ein vorgetragener Typ1-Dienst.

Antwort: Für den Roadmap-Entwurf ist kein zusätzliches Praxisbeispiel erforderlich. Die bestätigten Einzelbeispiele und Regeln werden mit synthetischen Testfällen kombiniert.

### G-02 – Ungewöhnlicher, aber wichtiger Grenzfall

Welcher in der Praxis vorkommende Sonderfall darf auf keinen Fall vergessen werden?

Antwort: Kein weiterer System-06-Grenzfall wurde genannt. Die anschließend unter G-03 beschriebenen Planungsregeln bleiben als Übergaben an spätere Systeme erhalten.

### G-03 – Weitere betriebliche Regeln

Gibt es zu Verfügbarkeiten, Abwesenheiten oder Sollreduktionen eine wichtige Regel, nach der dieses Dokument noch nicht fragt?

Antwort:
Wenn vor oder nach dem Urlaub ein Wochenende kommt, soll - wenn möglich - Auch das komplette Wochenende vor oder nach dem Urlaub als freier Tag geplant werden.
Es sollen generell 2 zusammenhängende Tage pro Woche frei gegeben werden.
Die 2 freien Tag sollen - wenn möglich - mit den roten X Tagen verbunden werden. Also unmittelbar vorher oder nachher.
Eine person soll maximal 7 Arbeitstage am stück geplant werden können.
Mindestens einmal innerhalb der geplanten 3 Wochen soll jeder Mitarbeiter 1 komplettes Wochenende frei bekommen - wenn möglich.
Doppeldienste sollen auf ein Minimum gehalten werden, maximal 1x Pro Woche pro person.

Da sind sicher Antworten dabei die nicht hier in dieses System gehören, bitte halte sie aber trotzdem schonmal fest, damit wir sie in den nächsten Systemen und bei der Generierung nicht vergessen.

## Klärender Abgleich der Antworten und neuen Informationen

### Gehört unmittelbar zu System 06

- `U` und `K` sind ganztägige, von der Service-Leitung eingetragene und für die Generierung zwingend gesperrte Abwesenheiten.
- Für `U` und `K` gilt derselbe am Mitarbeitertyp einstellbare Abwesenheits-Tageswert.
- Rote `X` sind ganztägige, von der Service-Leitung vorgegebene freie Tage und für die Generierung zwingend gesperrt.
- Wunschfrei gehört nicht zur ersten Fassung; ein verbindlicher freier Wunsch wird als rotes `X` eingetragen.
- Fortbildung, individuelle Zeitfenster, Mehrfachbearbeitung mehrerer Tagesfelder und zusätzliche Notizen gehören nicht zur ersten Fassung.
- In der Eingabeansicht erscheinen nur aktive Mitarbeitende.
- Die Drei-Wochen-Ansicht, die Korrektur einzelner Tagesfelder und die lokale Speicherung dieser Eingaben gehören in die System-06-Roadmap.

### Wird vorbereitet, aber erst in späteren Systemen vollständig umgesetzt

| Fachliche Vorgabe | Eigentümer gemäß Master-Roadmap |
|---|---|
| Schwarze `X` als von der Generierung bestimmte freie Tage | Systeme 08 und 09 |
| zwei zusammenhängende freie Tage pro Woche | Systeme 07 und 09 |
| freie Tage möglichst an rote `X` oder an ein Urlaubswochenende anschließen | Systeme 07 und 09 |
| mindestens ein vollständiges freies Wochenende innerhalb der drei Wochen | Systeme 07 und 09 |
| höchstens sieben Arbeitstage in Folge | Systeme 07 bis 09; benötigte Historie in System 08 |
| Doppeldienste minimieren und höchstens einmal pro Person und Woche verwenden | Systeme 07 und 09 |
| optionale manuelle Planbearbeitung und anschließende feste Abnahme | Systeme 11 und 12 |

Diese späteren Regeln bleiben ausdrücklich festgehalten. Sie werden nicht versehentlich als bereits implementierter Bestandteil von System 06 behandelt.

## H – Abgeschlossene Folgefragen

### H-01 – Bedeutung der AH-Ausnahme

Was bedeutet „`TypAH1` und `TypAH2` bekommen keinen Urlaub und keine Krankheitstage“ genau?

- [X] `U` und `K` dürfen für AH nicht eingetragen werden.
- [ ] `K` darf als zwingende Nichtverfügbarkeit eingetragen werden, reduziert das Soll aber um null Stunden; `U` ist nicht zulässig.
- [ ] `U` und `K` dürfen als zwingende Nichtverfügbarkeit eingetragen werden, reduzieren das Soll aber jeweils um null Stunden.
- [ ] Andere Regel.

Empfehlung: Mindestens `K` muss als Nichtverfügbarkeit erfassbar bleiben, damit eine erkrankte AH-Person niemals eingeplant wird. Ob `U` fachlich zulässig ist, muss die Service-Leitung bestätigen.

Antwort: bei krankheit wird von der Serviceleitung ein rotes X eingetragen, das möchte sie so machen.

### H-02 – Exakte Formel sowie Wochenenden und Feiertage

Ist folgende Berechnung richtig?

`Wirksames Wochen-Soll = ungekürztes Wochen-Soll − Summe aller U- und K-Tageswerte dieser Woche`, mindestens jedoch null Stunden.

Beispiele:

- `Typ25` mit einem `U`: 25 − 5 = 20 zu planende Stunden.
- `Typ30` mit zwei `K`: 30 − 6 − 6 = 18 zu planende Stunden.

Gilt der jeweilige Tageswert immer dann, wenn `U` oder `K` eingetragen wurde, also auch an Samstag, Sonntag und Feiertagen?

Antwort: richtig

### H-03 – Stundenwirkung und Korrektur eines roten X

Bitte bestätigen oder korrigieren:

- Ein rotes `X` ist ein fest vorgegebener freier Tag, verändert aber das Wochen-Soll nicht.
- Die automatische Generierung darf ein rotes `X` niemals verändern.
- Die Service-Leitung darf eine eigene Fehleingabe vor der Planabnahme nach der bereits bestätigten Sicherheitsabfrage korrigieren oder entfernen.
- Nach der Planabnahme bleibt der damalige Stand über die unveränderliche Planversion nachvollziehbar.
- Rote `X` gelten nur für konkrete Kalenderdaten. Bitte zusätzlich angeben, ob sie für jeden neuen Drei-Wochen-Zeitraum erneut eingetragen werden oder auf Wunsch aus dem vorherigen Zeitraum übernommen werden können.

Antwort: Alle Aussagen sind bestätigt. Rote `X` werden für jeden konkreten Kalendertag einzeln eingetragen und nicht in einen späteren Drei-Wochen-Zeitraum übernommen.

### H-04 – Neue Mitarbeitertypen und Wochen-Soll

Bitte bestätigen:

- `Typ20` besitzt 20 Wochenstunden.
- `Typ20a` besitzt 20 Wochenstunden.
- `Typ25a` besitzt 25 Wochenstunden.

Antwort: richtig. Wir machen es so, dass die Typen ohne a nicht in der Cafeteria eingeplant werden können. Die Typen mit einem a sind für Cafeteria einplanbar.

### H-05 – Einsatzfreigaben und Zeitpunkt der neuen Typen

Welche Einsatzfreigaben und Planungsbesonderheiten besitzen `Typ20`, `Typ20a` und `Typ25a`?

Die vorhandene Namenslogik legt nahe, dass Typen ohne `a` nur Restaurant-Frühdienst, Restaurant-Spätdienst und `D` und Typen mit `a` alle Dienste und Muster erlauben. Diese Zuordnung wird nicht ohne ausdrückliche Bestätigung übernommen.

Bitte außerdem bestätigen, ob für diese drei Typen später derselbe zwingende Stundenkorridor von minus drei bis plus drei Stunden gilt wie für die bisherigen Typen 25 bis 35.

Sollen die drei neuen Typen als notwendige Ergänzung vor beziehungsweise innerhalb von System 06 umgesetzt werden, oder erst in einem späteren eigenen Erweiterungsschritt? Der einstellbare Abwesenheits-Tageswert kann unabhängig davon bereits datengetrieben vorbereitet werden.

Antwort: Die Einsatzfreigaben sind bestätigt: Typen ohne `a` dürfen Restaurant-Frühdienst, Restaurant-Spätdienst und `D`; Typen mit `a` dürfen zusätzlich beide Cafeteria-Dienste und `Spr`. Für `Typ20`, `Typ20a` und `Typ25a` gilt derselbe zwingende Korridor von minus drei bis plus drei Stunden. Die drei Typen werden in einem Vorbereitungsteil vor dem ersten eigentlichen System-06-Schritt ergänzt.

### H-06 – Wunschfrei entfällt

Wird für die erste Fassung ein eigenes Wunschfrei-Kürzel oder eine weiche Wunschfrei-Regel benötigt?

Antwort: Nein. Wunschfrei wird entfernt. Die Service-Leitung trägt den gewünschten verbindlichen freien Tag als rotes `X` ein.

### H-07 – Bedeutung „eine Woche in der Vergangenheit“

Bezieht sich die erlaubte rückwirkende Bearbeitung auf die unmittelbar vorherige Kalenderwoche relativ zum aktuellen Datum?

Bitte bestätigen: Auch innerhalb dieses Zeitfensters wird eine bereits abgenommene Planversion niemals nachträglich verändert. Eine Korrektur erzeugt später einen neuen nachvollziehbaren Stand.

Antwort: Die rückwirkende Bearbeitung soll immer möglich sein. Ein abgenommener Plan wird dabei nicht überschrieben: Die Bearbeitung führt zu einem neuen Entwurf, die frühere unveränderliche Planversion bleibt erhalten und nach einer erneuten Abnahme entsteht eine neue Version.

### H-08 – Zwingend oder weich bei den späteren Generierungsregeln

Bitte jede Regel einordnen. „Zwingend“ bedeutet, dass sie selbst bei ungedecktem Bedarf niemals verletzt werden darf. „Weich“ bedeutet, dass die Generierung sie möglichst erfüllt und eine Abweichung sichtbar erklärt.

| Regel | Zwingend oder weich? | Falls weich: Priorität hoch, mittel oder niedrig? |
|---|---|---|
| zwei zusammenhängende freie Tage pro Person und Woche |  weich   |  hoch |
| freie Tage möglichst direkt vor oder nach roten `X` | weich | hoch |
| freies Wochenende direkt vor Urlaub | weich | hoch |
| freies Wochenende direkt nach Urlaub | zwingend |  |
| mindestens ein vollständiges freies Wochenende innerhalb der drei Wochen | weich | mittel |
| höchstens sieben Arbeitstage in Folge | zwingend |  |
| höchstens ein Doppeldienst pro Person und Woche | weich | hoch |
| Doppeldienste innerhalb dieser Grenze zusätzlich minimieren | weich | mittel |

Zählt ein rotes `X` selbst bereits als einer der zwei zusammenhängenden freien Tage, sodass die Generierung möglichst ein schwarzes `X` unmittelbar davor oder danach setzt?

Antwort: ja

### H-09 – Zeitraumgrenze bei höchstens sieben Arbeitstagen

Muss die Prüfung „höchstens sieben Arbeitstage in Folge“ auch Dienste unmittelbar vor und nach den angezeigten drei Wochen berücksichtigen? Ohne diese Randdaten könnte eine scheinbar zulässige Folge innerhalb der Ansicht insgesamt länger als sieben Tage sein.

Empfehlung: Ja; System 08 stellt dafür die notwendige angrenzende Historie als Teil der Planungsmomentaufnahme bereit.

Antwort: Ja.

### H-10 – AH erst nach allen anderen Typen

Wie wird AH in der späteren Generierung behandelt?

Antwort:

- Zuerst werden alle automatisch planbaren Nicht-AH-Typen geplant.
- Danach werden AH-Personen ausschließlich für noch ungedeckte, für sie zulässige Dienste eingeplant.
- Bereits durch Nicht-AH-Personen gedeckte Dienste werden nicht wieder freigeräumt, um AH-Stunden zu erzeugen.
- Das AH-Wochen-Soll bleibt zehn Stunden.
- Weniger als sechs geplante AH-Stunden sind zulässig und werden im Generierungsbericht gemeldet.
- Mehr als zehn AH-Stunden werden weiterhin gemeldet.
- Mehr als zwölf AH-Stunden bleiben zwingend unzulässig.

Diese Regel ersetzt die bisherige zwingende AH-Untergrenze von sieben Stunden. Sie gehört in die zentralen Regeln und die spätere Generierung der Systeme 07, 09 und 10; System 06 implementiert sie nicht.

### H-11 – Typ1-Bürokennzeichnung

Wie wird eine von Typ1 vorgetragene Bürozeit behandelt?

Antwort:

- Nur ein manuell vorgetragener Restaurant-Frühdienst oder Restaurant-Spätdienst kann zusätzlich als Büro gekennzeichnet werden.
- In der Bearbeitungsansicht wird dafür die Abkürzung `B` sichtbar.
- Die vollständige tatsächliche Dienstzeit zählt zu den geplanten Typ1-Wochenstunden.
- Die Zuweisung deckt keinen Platz des zugrunde liegenden Personalbedarfs.
- Nach der Planabnahme bleibt der zugrunde liegende Dienst sichtbar; nur die zusätzliche sichtbare Kennzeichnung `B` verschwindet.
- Die strukturierte Büroinformation bleibt in der unveränderlichen Planversion erhalten, damit Stunden- und Bedarfsberechnung nachvollziehbar bleiben. Im abgenommenen Plan und späteren Excel-Export wird sie nicht als `B` angezeigt.

Die gemeinsame Eingabe wird in System 06 nur vorbereitet. Das Planmodell und die vorgetragene Typ1-Zuweisung gehören zu System 08, die Bedarfswirkung zur Planung beziehungsweise Prüfung und die abgenommene Darstellung zu den Systemen 11 bis 13.

### H-12 – Mitarbeitertypen vor System 06 pflegen

Welche Typdaten soll die Service-Leitung vor dem ersten eigentlichen System-06-Schritt selbst verwalten können?

Antwort:

- Ein eigener Mitarbeitertypen-Tab erlaubt Anlegen und Bearbeiten.
- Bearbeitbar sind verständlicher Name, Wochen-Soll, Zulässigkeit und Tageswert für `U` und `K`, jede normale Dienstfreigabe sowie die Berechtigungen für `D` und `Spr`.
- Der eindeutige sichtbare Typcode wird beim Anlegen vergeben und bleibt danach stabil.
- Neu angelegte Typen sind normale automatisch planbare Typen mit dem zwingenden Korridor von minus drei bis plus drei Stunden.
- Auch `Typ1`, `TypAH1` und `TypAH2` dürfen in den genannten Stammdaten und Einsatzberechtigungen bearbeitet werden. Ihre besonderen Planungsrollen bleiben strukturierte, geschützte Eigenschaften: Typ1 wird nicht automatisch geplant; AH wird erst nach Nicht-AH für verbleibende Lücken geplant.
- Ein noch nie verwendeter Typ darf nach Sicherheitsabfrage endgültig gelöscht werden. Ein bereits einer aktiven oder deaktivierten Person oder anderen Fachdaten zugeordneter Typ darf nicht gelöscht werden.
- Die besonderen Rollen für Typ1 und AH können in dieser ersten Pflegeoberfläche weder neu vergeben noch entfernt werden. Deshalb bleiben die dafür benötigten Starttypen selbst erhalten; normale neue Typen erhalten keine solche Sonderrolle.

Die Typen `Typ20`, `Typ20a` und `Typ25a` werden über denselben datengetriebenen Vertrag als Startwerte ergänzt. Die Pflegeoberfläche ist ein verbindlicher Vorbereitungsteil des Roadmap-Entwurfs vor der Abwesenheitserfassung.

## I – Abschluss der Fragenrunde

### I-01 – Fachliche Bestätigung

Dieser Punkt wird erst nach gemeinsamer Durchsicht der Antworten ausgefüllt.

- [X] Die Antworten wurden mit der Service-Leitung abgestimmt.
- [X] Es bestehen keine offenen fachlichen Fragen für den Roadmap-Entwurf.
- [X] Die Antworten enthalten keine echten Mitarbeiter-, Krankheits- oder Plandaten.
- [X] Die beantwortete Datei darf als Grundlage für die Teil-Roadmap von System 06 und die betroffenen Leitdokumente verwendet werden.

Bestätigt am: 2026-09-15

Zusätzliche Anmerkung: Der Mitarbeitertypen-Tab ist ein verbindlicher Vorbereitungsteil vor dem ersten eigentlichen System-06-Implementierungsschritt. Typ1-Bürozeiten und die AH-Reihenfolge werden bereits fachlich festgehalten, aber erst in ihren späteren Planungs-, Berichts- und Versionssystemen umgesetzt.

## Nächster Schritt nach der Beantwortung

Die Antworten wurden auf Widersprüche, Datenschutz, Überschneidungen mit den Systemen 03 bis 05 und die Grenzen zu den Systemen 07 bis 13 geprüft. Die bestätigten Entscheidungen werden in den betroffenen Grundlagen nachvollziehbar festgehalten und eine eigene Teil-Roadmap für System 06 mit Vorbereitungsteil, kleinen nummerierten Schritten, Code-Ankern, Prüfungen und echten Abnahmegates entworfen.

Erst nach ausdrücklicher Abnahme dieser Teil-Roadmap kann der erste Implementierungsschritt beginnen.
