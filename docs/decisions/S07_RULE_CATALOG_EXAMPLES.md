# System 07 – Gemeinsame synthetische Beispielszenarien

Status: Vollständig umgesetzt, automatisch geprüft und am 2026-09-16 fachlich ausdrücklich abgenommen

Stand: 2026-09-16

## Zweck und Leseschlüssel

Dieses Dokument erklärt Katalogversion 1 mit einfachen, gemeinsamen Beispielen. Es legt keine neue Regel fest und nimmt weder das spätere Planmodell noch die technische Regelauswertung vorweg. Maßgeblich bleiben die bestätigte Regelmatrix und die Architekturentscheidung.

Alle Personen, Wochen, Dienste und Uhrzeiten in diesem Dokument sind ausdrücklich erfunden. `Testperson A`, `Testperson B` und `Testperson AH` sind keine echten Beschäftigten. `U`, `K`, rotes `X` und schwarzes `X` werden nur als bereits bestätigte Tageskennzeichen verwendet; es werden keine Abwesenheitsgründe beschrieben.

Jede Szenariokennung ist stabil aufgebaut:

- `...-SATISFIED`: Die Regel ist im beschriebenen Prüfgegenstand erfüllt.
- `...-VIOLATED`: Die Regel ist im beschriebenen Prüfgegenstand verletzt oder löst ihren vorgesehenen Hinweis aus.
- `...-NOT_APPLICABLE`: Im beschriebenen Prüfgegenstand gibt es kein Objekt, auf das die Regel angewendet werden kann.
- `...-NOT_FULLY_EVALUABLE`: Nur bei der Sieben-Tage-Regel fehlt die benötigte Vorgeschichte; das Ergebnis darf deshalb nicht als vollständig geprüft gelten.

Bei Strukturregeln bedeutet eine Verletzung eine nicht speicherbare Struktur. Bei automatischen Hard Rules darf die Generierung die Verletzung nicht erzeugen. Weiche Regeln werden in der angegebenen Prioritätsstufe optimiert. Hinweise berichten einen Zustand, ohne ihn zu verbieten. Das Stabilitätsziel entscheidet erst zwischen ansonsten gleichwertigen Plänen.

## Strukturregeln

Alle sechs Strukturregeln blockieren widersprüchliche Daten sowohl automatisch als auch manuell. Sie haben keine Priorität, weil sie nicht gegeneinander abgewogen werden.

### `STRUCTURE_KNOWN_REFERENCES`

- `S07-STRUCTURE_KNOWN_REFERENCES-SATISFIED`: Der synthetische Plan verweist nur auf vorhandene Testpersonen, Dienste und Einsatzmuster.
- `S07-STRUCTURE_KNOWN_REFERENCES-VIOLATED`: Eine Zuweisung verweist auf die nicht vorhandene Dienstkennung `TEST-UNBEKANNT`; der Plan ist strukturell ungültig.
- `S07-STRUCTURE_KNOWN_REFERENCES-NOT_APPLICABLE`: Der leere Planentwurf enthält noch keine Referenz, die geprüft werden könnte.

### `STRUCTURE_SINGLE_DAILY_ASSIGNMENT`

- `S07-STRUCTURE_SINGLE_DAILY_ASSIGNMENT-SATISFIED`: Testperson A besitzt am Dienstag genau eine normale Zuweisung.
- `S07-STRUCTURE_SINGLE_DAILY_ASSIGNMENT-VIOLATED`: Testperson A erhält am Dienstag zwei voneinander unabhängige Dienste. Ein zusammengesetztes Muster wäre eine einzelne Zuweisung, diese beiden Dienste sind es nicht.
- `S07-STRUCTURE_SINGLE_DAILY_ASSIGNMENT-NOT_APPLICABLE`: Für Testperson A gibt es am Dienstag keine Zuweisung.

### `STRUCTURE_NO_TIME_OVERLAP`

- `S07-STRUCTURE_NO_TIME_OVERLAP-SATISFIED`: Die beiden Abschnitte eines erfundenen zusammengesetzten Musters liegen nacheinander und überschneiden sich nicht.
- `S07-STRUCTURE_NO_TIME_OVERLAP-VIOLATED`: Zwei Arbeitsabschnitte derselben Testperson überschneiden sich am Mittwoch von 12:00 bis 12:30 Uhr.
- `S07-STRUCTURE_NO_TIME_OVERLAP-NOT_APPLICABLE`: Die Zuweisung besitzt nur einen Arbeitsabschnitt; es gibt kein Abschnittspaar zu vergleichen.

### `STRUCTURE_BLOCKED_DAY_MARKER`

- `S07-STRUCTURE_BLOCKED_DAY_MARKER-SATISFIED`: Ein mit `U`, `K` oder rotem `X` markierter Tag enthält keine Arbeitszuweisung.
- `S07-STRUCTURE_BLOCKED_DAY_MARKER-VIOLATED`: Auf einem Tag mit rotem `X` wird gleichzeitig ein Dienst eingetragen; die Zuweisung wird blockiert.
- `S07-STRUCTURE_BLOCKED_DAY_MARKER-NOT_APPLICABLE`: Der betrachtete Tag besitzt weder `U` noch `K` noch ein rotes `X`.

### `STRUCTURE_NORMAL_SLOT_FULL_COVERAGE`

- `S07-STRUCTURE_NORMAL_SLOT_FULL_COVERAGE-SATISFIED`: Eine normale Zuweisung deckt den vollständigen tatsächlichen Zeitraum genau eines synthetischen Bedarfsplatzes.
- `S07-STRUCTURE_NORMAL_SLOT_FULL_COVERAGE-VIOLATED`: Eine normale Zuweisung soll nur einen frei gewählten Teil eines Bedarfsplatzes abdecken; diese freie Teildeckung ist nicht zulässig.
- `S07-STRUCTURE_NORMAL_SLOT_FULL_COVERAGE-NOT_APPLICABLE`: Der Bedarfsplatz bleibt vollständig ungedeckt; es gibt keine normale Zuweisung, deren Deckungsumfang geprüft werden könnte.

### `STRUCTURE_APPROVED_PLAN_IMMUTABLE`

- `S07-STRUCTURE_APPROVED_PLAN_IMMUTABLE-SATISFIED`: Eine abgenommene synthetische Planversion bleibt unverändert; eine spätere Bearbeitung erzeugt eine neue Version.
- `S07-STRUCTURE_APPROVED_PLAN_IMMUTABLE-VIOLATED`: Eine bereits abgenommene Planversion soll direkt überschrieben werden; die Änderung wird blockiert.
- `S07-STRUCTURE_APPROVED_PLAN_IMMUTABLE-NOT_APPLICABLE`: Der betrachtete Planentwurf wurde noch nicht abgenommen und ist noch keine unveränderliche Planversion.

## Automatische Hard Rules

Diese elf Regeln sind für die automatische Planung zwingend. Eine manuelle Abweichung ist nur dort möglich, wo die jeweilige Regelwirkung ausdrücklich eine Warnung und Bestätigung vorsieht.

### `ACTIVE_EMPLOYEES_ONLY`

- `S07-ACTIVE_EMPLOYEES_ONLY-SATISFIED`: Eine Zuweisung verwendet eine im Planungszeitraum aktive Testperson.
- `S07-ACTIVE_EMPLOYEES_ONLY-VIOLATED`: Eine Zuweisung verwendet eine im Planungszeitraum inaktive Testperson; dies wird auch manuell blockiert.
- `S07-ACTIVE_EMPLOYEES_ONLY-NOT_APPLICABLE`: Es liegt keine Zuweisung vor.

### `SHIFT_ELIGIBILITY_REQUIRED`

- `S07-SHIFT_ELIGIBILITY_REQUIRED-SATISFIED`: Testperson A besitzt die bestätigte Einsatzfreigabe für den zugewiesenen Dienst.
- `S07-SHIFT_ELIGIBILITY_REQUIRED-VIOLATED`: Testperson A besitzt diese Einsatzfreigabe nicht. Automatisch ist die Zuweisung ausgeschlossen; manuell wäre sie nur nach Warnung und Bestätigung möglich.
- `S07-SHIFT_ELIGIBILITY_REQUIRED-NOT_APPLICABLE`: Es liegt keine Zuweisung vor.

### `EXPLICIT_RUN_OPTION_REQUIRED`

- `S07-EXPLICIT_RUN_OPTION_REQUIRED-SATISFIED`: Eine besondere, laufabhängige Einsatzfreigabe wurde für genau diesen Planungslauf ausdrücklich aktiviert.
- `S07-EXPLICIT_RUN_OPTION_REQUIRED-VIOLATED`: Die Generierung soll eine besondere Einsatzfreigabe nutzen, obwohl die Laufoption für diesen Planungslauf ausgeschaltet ist.
- `S07-EXPLICIT_RUN_OPTION_REQUIRED-NOT_APPLICABLE`: Der Lauf benötigt keine besondere laufabhängige Einsatzfreigabe.

### `TYPE1_MANUAL_ONLY`

- `S07-TYPE1_MANUAL_ONLY-SATISFIED`: Der Typ1-Dienst wurde vor dem Lauf manuell eingetragen und bleibt während der Generierung geschützt.
- `S07-TYPE1_MANUAL_ONLY-VIOLATED`: Die Generierung versucht, Testperson Typ1 selbst einen neuen Dienst zuzuteilen; der Lauf darf dies nicht tun.
- `S07-TYPE1_MANUAL_ONLY-NOT_APPLICABLE`: Im Planungszeitraum gibt es keine Testperson mit der Planungsrolle Typ1.

### `TYPE1_WEEKLY_PREREQUISITE`

- `S07-TYPE1_WEEKLY_PREREQUISITE-SATISFIED`: In einer nicht vollständig abwesenden Montag-bis-Sonntag-Woche ist für Testperson Typ1 mindestens ein manueller Dienst vorgetragen.
- `S07-TYPE1_WEEKLY_PREREQUISITE-VIOLATED`: Die Woche ist nicht vollständig abwesend, aber es fehlt der notwendige vorgetragene Typ1-Dienst; die Generierung startet nicht.
- `S07-TYPE1_WEEKLY_PREREQUISITE-NOT_APPLICABLE`: Testperson Typ1 ist die gesamte Woche abwesend; die bestätigte Ausnahme wird mit Hinweis behandelt.

### `NORMAL_WEEKLY_MAXIMUM`

- `S07-NORMAL_WEEKLY_MAXIMUM-SATISFIED`: Das wirksame Wochen-Soll von Testperson A beträgt synthetisch 30 Stunden; automatisch werden höchstens 33 Stunden geplant.
- `S07-NORMAL_WEEKLY_MAXIMUM-VIOLATED`: Bei demselben wirksamen Soll soll die Automatik 33 Stunden und 15 Minuten planen. Automatisch ist das unzulässig; manuell wäre es nur bestätigt möglich.
- `S07-NORMAL_WEEKLY_MAXIMUM-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die normale Planungsrolle.

### `AH_WEEKLY_MAXIMUM`

- `S07-AH_WEEKLY_MAXIMUM-SATISFIED`: Testperson AH erhält in der Woche höchstens 12 tatsächliche Arbeitsstunden.
- `S07-AH_WEEKLY_MAXIMUM-VIOLATED`: Testperson AH soll automatisch 12 Stunden und 15 Minuten erhalten. Automatisch ist das unzulässig; manuell wäre es nur bestätigt möglich.
- `S07-AH_WEEKLY_MAXIMUM-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die Planungsrolle AH.

### `MAX_CONSECUTIVE_WORKDAYS`

- `S07-MAX_CONSECUTIVE_WORKDAYS-SATISFIED`: Unter Einbeziehung der unmittelbar vorhergehenden Arbeitstage arbeitet Testperson A höchstens sieben Kalendertage in Folge. `U`, `K`, rotes und schwarzes `X` unterbrechen die Folge.
- `S07-MAX_CONSECUTIVE_WORKDAYS-VIOLATED`: Der Montag im Plan wäre unter Einbeziehung der Vorgeschichte der achte Arbeitstag in Folge. Automatisch bleibt er frei; manuell wäre eine Abweichung nur bestätigt möglich.
- `S07-MAX_CONSECUTIVE_WORKDAYS-NOT_APPLICABLE`: Im betrachteten Ausschnitt gibt es für die Testperson keinen tatsächlichen Arbeitstag und damit keine Arbeitsfolge.
- `S07-MAX_CONSECUTIVE_WORKDAYS-NOT_FULLY_EVALUABLE`: Am Beginn des Planungszeitraums fehlt die benötigte Vorgeschichte. Die Planung darf mit Hinweis fortfahren, aber keine vollständige Sieben-Tage-Prüfung behaupten.

### `POST_VACATION_WEEKEND_FREE`

- `S07-POST_VACATION_WEEKEND_FREE-SATISFIED`: Ein ausdrücklich mit `U` gekennzeichneter Urlaubsblock endet am Freitag; der unmittelbar folgende Samstag und Sonntag bleiben ohne Arbeitsabschnitt.
- `S07-POST_VACATION_WEEKEND_FREE-VIOLATED`: Nach einem am Freitag endenden Urlaubsblock soll Testperson A am Samstag arbeiten. Die Automatik lässt erforderlichenfalls Bedarf ungedeckt; manuell wäre die Abweichung nur bestätigt möglich.
- `S07-POST_VACATION_WEEKEND_FREE-NOT_APPLICABLE`: Der Urlaubsblock endet nicht am Freitag oder es gibt keinen Urlaubsblock.

### `AUTOMATIC_NO_OVERSTAFFING`

- `S07-AUTOMATIC_NO_OVERSTAFFING-SATISFIED`: Die Automatik besetzt einen Bedarfsplatz mit genau einer zulässigen Person und fügt keine weitere Person hinzu.
- `S07-AUTOMATIC_NO_OVERSTAFFING-VIOLATED`: Die Automatik versucht, eine zusätzliche Person auf einen bereits vollständig gedeckten Bedarfsplatz zu setzen. Nur eine spätere manuelle, sichtbar bestätigte Zusatzzuweisung ist erlaubt.
- `S07-AUTOMATIC_NO_OVERSTAFFING-NOT_APPLICABLE`: Es gibt im betrachteten Ausschnitt keinen Bedarfsplatz mit einer Zuweisung.

### `RELIEF_SHIFT_EMERGENCY_ONLY`

- `S07-RELIEF_SHIFT_EMERGENCY_ONLY-SATISFIED`: Am Samstag bleibt der Restaurant-Spätdienst trotz regulärer Planung teilweise ungedeckt. `Spr` wechselt nach dem tatsächlichen Ende des Cafeteria-Einsatzes und deckt erst ab diesem Zeitpunkt.
- `S07-RELIEF_SHIFT_EMERGENCY_ONLY-VIOLATED`: `Spr` wird an einem anderen Tag, ohne verbleibende Restaurant-Unterdeckung oder nur zur Stundenauffüllung eingesetzt; die Zuweisung wird blockiert.
- `S07-RELIEF_SHIFT_EMERGENCY_ONLY-NOT_APPLICABLE`: Im betrachteten Samstag wird kein `Spr`-Muster verwendet.

## Weiche Regeln

Eine Verletzung bleibt automatisch zulässig, wird aber in der bestätigten Prioritätsstufe verschlechtert. Manuell erfordert eine Abweichung später eine sichtbare Warnung und Bestätigung.

### `NORMAL_WEEKLY_MINIMUM` – Priorität hoch

- `S07-NORMAL_WEEKLY_MINIMUM-SATISFIED`: Bei einem wirksamen Wochen-Soll von 30 Stunden erhält Testperson A mindestens 27 Stunden.
- `S07-NORMAL_WEEKLY_MINIMUM-VIOLATED`: Trotz fehlenden zulässigen Bedarfs erhält Testperson A nur 25 Stunden. Der Plan bleibt zulässig und berichtet die Unterschreitung, statt Überbesetzung zu erzeugen.
- `S07-NORMAL_WEEKLY_MINIMUM-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die normale Planungsrolle.

### `WEEKLY_CONSECUTIVE_DAYS_OFF` – Priorität hoch

- `S07-WEEKLY_CONSECUTIVE_DAYS_OFF-SATISFIED`: Testperson A besitzt in einer Montag-bis-Sonntag-Woche zwei direkt aufeinanderfolgende rote oder schwarze `X`.
- `S07-WEEKLY_CONSECUTIVE_DAYS_OFF-VIOLATED`: Die regulären freien Tage liegen nicht zusammen. `U` und `K` ersetzen dieses gewünschte Paar nicht.
- `S07-WEEKLY_CONSECUTIVE_DAYS_OFF-NOT_APPLICABLE`: Im Prüfgegenstand liegt keine Mitarbeiterwoche vor.

### `RED_X_ADJACENT_DAY_OFF` – Priorität hoch

- `S07-RED_X_ADJACENT_DAY_OFF-SATISFIED`: Neben dem garantierten roten `X` am Mittwoch liegt am Dienstag oder Donnerstag ein weiteres rotes oder schwarzes `X`.
- `S07-RED_X_ADJACENT_DAY_OFF-VIOLATED`: Das rote `X` am Mittwoch bleibt ohne direkt angrenzendes rotes oder schwarzes `X`.
- `S07-RED_X_ADJACENT_DAY_OFF-NOT_APPLICABLE`: In der betrachteten Woche gibt es kein rotes `X`.

### `PRE_VACATION_WEEKEND_FREE` – Priorität hoch

- `S07-PRE_VACATION_WEEKEND_FREE-SATISFIED`: Ein ausdrücklich markierter Urlaubsblock beginnt am Montag; der unmittelbar vorherige Samstag und Sonntag bleiben ohne Arbeitsabschnitt.
- `S07-PRE_VACATION_WEEKEND_FREE-VIOLATED`: Vor einem am Montag beginnenden Urlaubsblock arbeitet Testperson A am vorherigen Samstag oder Sonntag.
- `S07-PRE_VACATION_WEEKEND_FREE-NOT_APPLICABLE`: Der Urlaubsblock beginnt nicht am Montag oder es gibt keinen Urlaubsblock.

### `SPLIT_SHIFT_WEEKLY_MAXIMUM` – Priorität hoch

- `S07-SPLIT_SHIFT_WEEKLY_MAXIMUM-SATISFIED`: Testperson A erhält in der Montag-bis-Sonntag-Woche höchstens einen Doppeldienst `D`.
- `S07-SPLIT_SHIFT_WEEKLY_MAXIMUM-VIOLATED`: Testperson A erhält in derselben Woche zwei Doppeldienste `D`.
- `S07-SPLIT_SHIFT_WEEKLY_MAXIMUM-NOT_APPLICABLE`: Im Prüfgegenstand liegt keine Mitarbeiterwoche vor.

### `THREE_WEEK_FREE_WEEKEND` – Priorität mittel

- `S07-THREE_WEEK_FREE_WEEKEND-SATISFIED`: Testperson A besitzt innerhalb der drei Planungswochen mindestens ein vollständiges Wochenende aus Samstag und Sonntag ohne Arbeitsabschnitt; rote oder schwarze `X` zählen.
- `S07-THREE_WEEK_FREE_WEEKEND-VIOLATED`: Testperson A arbeitet an jedem der drei Wochenenden mindestens an einem der beiden Tage. `U` oder `K` werden nicht als regulär freies Wochenende gezählt.
- `S07-THREE_WEEK_FREE_WEEKEND-NOT_APPLICABLE`: Im Prüfgegenstand liegt kein Drei-Wochen-Planungszeitraum vor.

### `MINIMIZE_SPLIT_SHIFTS` – Priorität mittel

- `S07-MINIMIZE_SPLIT_SHIFTS-SATISFIED`: Von zwei ansonsten gleichwertigen zulässigen Plänen wird der mit weniger Doppeldiensten `D` bevorzugt.
- `S07-MINIMIZE_SPLIT_SHIFTS-VIOLATED`: Ein ansonsten gleichwertiger Plan mit mehr Doppeldiensten wird ausgewählt.
- `S07-MINIMIZE_SPLIT_SHIFTS-NOT_APPLICABLE`: Es gibt keine zulässige alternative Verteilung, durch die sich die Zahl der Doppeldienste unterscheidet.

### `AH_WEEKLY_TARGET` – Priorität mittel

- `S07-AH_WEEKLY_TARGET-SATISFIED`: Nach der Nicht-AH-Planung erhält Testperson AH bei vorhandenem zulässigem Restbedarf möglichst 10 Wochenstunden.
- `S07-AH_WEEKLY_TARGET-VIOLATED`: Zulässiger Restbedarf und Kapazität bis 10 Stunden wären vorhanden, Testperson AH erhält aber nur 7 Stunden.
- `S07-AH_WEEKLY_TARGET-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die Planungsrolle AH.

## Berichtshinweise

Die beiden Regeln erzeugen nur strukturierte Hinweise. Der Begriff `VIOLATED` kennzeichnet hier genau den Zustand, für den der Hinweis ausgegeben wird; er macht den Plan nicht unzulässig.

### `AH_WEEKLY_LOW_NOTICE`

- `S07-AH_WEEKLY_LOW_NOTICE-SATISFIED`: Testperson AH erhält mindestens 6 Wochenstunden; der Unter-Sechs-Stunden-Hinweis wird nicht ausgelöst.
- `S07-AH_WEEKLY_LOW_NOTICE-VIOLATED`: Testperson AH erhält weniger als 6 Wochenstunden; der Bericht weist darauf hin, ohne künstlich Bedarf zu erzeugen.
- `S07-AH_WEEKLY_LOW_NOTICE-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die Planungsrolle AH.

### `AH_WEEKLY_HIGH_NOTICE`

- `S07-AH_WEEKLY_HIGH_NOTICE-SATISFIED`: Testperson AH erhält höchstens 10 Wochenstunden; der Über-Zehn-Stunden-Hinweis wird nicht ausgelöst.
- `S07-AH_WEEKLY_HIGH_NOTICE-VIOLATED`: Testperson AH erhält mehr als 10 und höchstens 12 Wochenstunden; der Bericht weist auf den hohen Wert hin, obwohl er automatisch noch zulässig ist.
- `S07-AH_WEEKLY_HIGH_NOTICE-NOT_APPLICABLE`: Die betrachtete Person besitzt nicht die Planungsrolle AH. Mehr als 12 AH-Stunden wird zusätzlich durch die separate Hard Rule behandelt.

## Stabilitätsziel

### `CURRENT_PERIOD_FAIR_DISTRIBUTION`

- `S07-CURRENT_PERIOD_FAIR_DISTRIBUTION-SATISFIED`: Zwischen ansonsten gleichwertigen Plänen verteilt die Auswahl ungünstige Dienste, Doppeldienste und Wochenenden innerhalb der aktuellen drei Wochen möglichst gleichmäßig.
- `S07-CURRENT_PERIOD_FAIR_DISTRIBUTION-VIOLATED`: Ein ansonsten gleichwertiger Plan konzentriert diese Belastungen unnötig auf eine Testperson.
- `S07-CURRENT_PERIOD_FAIR_DISTRIBUTION-NOT_APPLICABLE`: Es gibt keinen gleichwertigen Alternativplan, zwischen dem das nachgelagerte Stabilitätsziel entscheiden könnte.

## Gemeinsame Kombinationsszenarien

Diese Fälle verbinden mehrere Regeln, ändern aber keine ihrer Einzelwirkungen.

### `S07-COMB-WEEKLY-CORRIDOR-NO-OVERSTAFFING`

Testperson A hat ein wirksames Wochen-Soll von 30 Stunden, aber nur 25 Stunden zulässigen ungedeckten Bedarf. Die Automatik bleibt unter der weichen Untergrenze von 27 Stunden, nennt Testperson A im Bericht und erzeugt keine Überbesetzung. Das Überbesetzungsverbot steht über der weichen Untergrenze.

### `S07-COMB-VACATION-COVERAGE`

Der Urlaubsblock von Testperson A endet am Freitag. Am folgenden Wochenende besteht synthetischer Bedarf, der ohne diese Person nicht vollständig gedeckt werden kann. Die Automatik hält Samstag und Sonntag trotzdem frei und weist die Lücke sichtbar aus. Erst eine spätere manuelle Zuweisung dürfte nach Warnung und Bestätigung abweichen.

### `S07-COMB-RED-X-BLACK-X`

Am Mittwoch ist ein rotes `X` vorgegeben. Die Automatik ergänzt am Donnerstag ein schwarzes `X`. Mittwoch und Donnerstag bilden die gewünschten zwei zusammenhängenden regulären freien Tage; das rote `X` bleibt unverändert garantiert.

### `S07-COMB-AH-PHASES`

Zuerst werden alle automatisch planbaren Nicht-AH-Testpersonen innerhalb der zwingenden Regeln eingeplant. Nur danach füllt Testperson AH verbleibende zulässige Lücken. Sie verdrängt niemanden, überschreitet automatisch nie 12 Stunden und strebt mit mittlerer Priorität 10 Stunden an; unter 6 oder über 10 Stunden entsteht der passende Berichtshinweis.

### `S07-COMB-TYPE1-PREREQUISITE`

Für eine nicht vollständig abwesende Typ1-Woche ist ein synthetischer Dienst manuell vorgetragen. Die Generierung schützt ihn und teilt Typ1 nichts zusätzlich automatisch zu. Fehlt der vorgetragene Dienst, startet die Generierung nicht; bei vollständiger Wochenabwesenheit greift die bestätigte Ausnahme mit Hinweis.

### `S07-COMB-SPLIT-SHIFT-D`

Der Doppeldienst `D` besteht aus Restaurant-Frühdienst und Restaurant-Spätdienst. Die Unterbrechung zählt nicht als Arbeitszeit; beide Abschnitte bilden zusammen genau eine Tageszuweisung, einen Arbeitstag und synthetisch 10 tatsächliche Arbeitsstunden. Höchstens ein `D` pro Woche ist ein hohes Ziel, zusätzlich wird die Gesamtzahl mit mittlerer Priorität minimiert.

### `S07-COMB-RELIEF-SPR`

Am Samstag endet ein erfundener Cafeteria-Abschnitt um 17:00 Uhr, während der Restaurant-Spätdienst bereits um 16:30 Uhr beginnt und regulär nicht vollständig besetzt werden kann. `Spr` deckt erst ab dem tatsächlichen Wechsel um 17:00 Uhr. Die Zeit von 16:30 bis 17:00 Uhr bleibt sichtbar ungedeckt; das Dienstkürzel darf sie nicht verbergen.

### `S07-COMB-MANUAL-DEVIATION`

Die Service-Leitung setzt später manuell eine zusätzliche zulässige Testperson auf einen bereits vollständig gedeckten Dienst. Der gespeicherte Bedarf bleibt unverändert, die tatsächlichen Stunden zählen vollständig und die Überbesetzung bleibt bis zur ausdrücklichen Bestätigung sichtbar. Die Bestätigung ändert weder den Katalog noch zukünftige Planungsläufe.

### `S07-COMB-STRUCTURE-BLOCK`

Auf einem Tag mit rotem `X` soll manuell ein Dienst eingetragen werden. Dies ist keine bestätigbare Planungsabweichung, sondern eine Strukturverletzung und wird blockiert. Soll die Person stattdessen arbeiten, muss die Service-Leitung zuerst das Tageskennzeichen bewusst entfernen und anschließend die Zuweisung neu prüfen.

## Fachliches Prüfgate – bestanden

Die Service-Leitung hat am 2026-09-16 ausdrücklich bestätigt:

1. Die Beispiele sind in einfacher Sprache verständlich.
2. Erfüllte, verletzte und nicht anwendbare Fälle entsprechen der gemeinsam bestätigten Bedeutung.
3. Die neun Kombinationsszenarien sind fachlich richtig, besonders Wochenkorridor, Urlaub, Typ1, `D`, `Spr` und manuelle Abweichungen.

Damit ist das fachliche Dokument-Gate vor RK-07 bestanden.
