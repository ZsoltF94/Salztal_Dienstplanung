# Fragen und Verständnisabgleich: Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben

Status: Vollständig beantwortet und am 2026-09-14 mit System 03 archiviert

Stand: 2026-09-14

## Zweck

Dieses Dokument hält den Verständnisabgleich zu den Mitarbeitertypen und die vollständig beantworteten Fragen fest, auf deren Grundlage die Teil-Roadmap überarbeitet wurde.

Die Antworten bilden die fachliche Grundlage der abgenommenen System-03-Roadmap. Die Umsetzung erfolgt weiterhin ausschließlich in den dort einzeln freigegebenen und abgenommenen Schritten.

Alle Beispiele verwenden ausschließlich Typbezeichnungen und keine echten Mitarbeiternamen oder Plandaten.

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
- `docs/decisions/S01_ARCHITECTURE_PROPOSAL.md`
- `docs/decisions/S04_SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md`
- `docs/decisions/S02_TECHNICAL_PACKAGE_BASELINE.md`
- die inzwischen abgeschlossene Teil-Roadmap `S03_EMPLOYEES_EMPLOYEE_TYPES_SHIFT_ELIGIBILITY_ROADMAP.md`
- die abgeschlossene Roadmap und Fragen-Datei von System 04
- die vorhandenen Domain-, Application-, Infrastructure- und Composition-Verträge aus System 04

## Aktuelles fachliches Verständnis

### 1. Mitarbeitertypen als betriebliche Profile

Die Bezeichnungen `Typ1`, `Typ25`, `Typ30`, `Typ30a`, `Typ35`, `Typ35a`, `TypAH1` und `TypAH2` beschreiben nach aktuellem Verständnis wiederkehrende betriebliche Profile. Ein Profil bündelt mindestens:

- ein Wochen-Soll,
- regulär automatisch zulässige Einsatzorte und normale Diensttypen,
- gegebenenfalls besondere Regeln für die automatische Planung,
- gegebenenfalls nur vorschlagsfähige Notfallabweichungen.

Die bisher abgenommene Architektur hält Wochenstunden, Qualifikationen, Einsatzorte und Diensttypfreigaben getrennt und erlaubt Profile nur als Vorlagen für diese getrennten Eigenschaften. Ob die neuen Mitarbeitertypen solche Vorlagen oder eine dauerhaft bindende Klassifikation sein sollen, ist deshalb die wichtigste offene Grundsatzfrage.

### 2. Vorläufig verstandene Typenmatrix

Die folgende Tabelle gibt die neue Vorgabe wieder. Annahmen und Widersprüche sind ausdrücklich als offen markiert.

| Profil | Wochen-Soll | Regulär automatisch zulässig | Besonderheit | Noch offen |
|---|---:|---|---|---|
| `Typ1` | 40 Stunden | alle Dienste | Service-Leitung; wird vorgetragen und von der Generierung nicht verändert | Bedeutung von „vorgetragen“; Verhalten ohne Vorbelegung; Verhältnis zur Stundenabweichung |
| `Typ25` | 25 Stunden | Restaurant-Frühdienst und Restaurant-Spätdienst; daraus grundsätzlich Doppeldienst möglich | keine weitere Besonderheit genannt | Bestätigung, dass `D` nur abgeleitet und keine eigene Freigabe ist |
| `Typ30` | 30 Stunden | Restaurant-Frühdienst und Restaurant-Spätdienst; daraus grundsätzlich Doppeldienst möglich | keine weitere Besonderheit genannt | wie `Typ25` |
| `Typ30a` | vermutlich 30 Stunden | alle Dienste | keine weitere Besonderheit genannt | Wochen-Soll und Bedeutung von „alle Dienste“ bestätigen |
| `Typ35` | laut neuer Angabe 25 Stunden | Restaurant-Frühdienst und Restaurant-Spätdienst; daraus grundsätzlich Doppeldienst möglich | Typname und Stundenwert passen nicht zusammen | 25 oder 35 Stunden bestätigen |
| `Typ35a` | vermutlich derselbe Stundenwert wie `Typ35` | alle Dienste | keine weitere Besonderheit genannt | Wochen-Soll und Bedeutung von „alle Dienste“ bestätigen |
| `TypAH1` | 10 Stunden | Restaurant-Spätdienst | Frühdienst nur als Lösung bei offenem Bedarf vorschlagen, niemals automatisch eintragen | zulässiger Stundenkorridor und Annahme eines Vorschlags |
| `TypAH2` | 10 Stunden | Restaurant-Spätdienst, Doppeldienst und Cafeteria-Dienst B | Frühdienst nur als Lösung bei offenem Bedarf vorschlagen, niemals automatisch als einzelnen Frühdienst eintragen | Doppeldienst enthält selbst einen Frühdienst; Springer-Berechtigung ist offen |

### 3. Vorhandene Dienst- und Einsatzortgrenzen

System 04 stellt bereits folgende stabile fachliche Quellen bereit:

- `WorkLocationId` für Cafeteria und Restaurant,
- `ShiftTypeId` für Frühdienst, Spätdienst, Cafeteria-Dienst A und Cafeteria-Dienst B,
- `D` als festes Einsatzmuster aus Frühdienst und Spätdienst im Restaurant,
- `Spr` als samstägliches Notfallmuster aus Cafeteria-Dienst B und anschließendem Spätdienst.

Nach der bestätigten System-04-Grenze erhalten Mitarbeitende keine eigenen Freigaben für `D` oder `Spr`. Die Zulässigkeit wird aus den Freigaben für die beteiligten normalen Diensttypen und Einsatzorte abgeleitet. Diese Grenze passt ohne weitere Klärung zu `Typ25`, `Typ30` und `Typ35`, erzeugt aber bei `TypAH2` einen Widerspruch: `D` soll zulässig sein, obwohl ein einzelner Frühdienst nicht automatisch zulässig ist.

### 4. Vorläufig verstandene Wochenstundenregel

Als noch zu bestätigende Auslegung wird verstanden:

- Das Wochen-Soll gilt je Mitarbeiter und je Planungswoche von Montag bis Sonntag.
- Die Generierung soll die Abweichung vom Wochen-Soll möglichst klein halten.
- Für normale Profile darf das Ergebnis höchstens drei Stunden unter oder über dem Soll liegen.
- Für AH-Profile gilt ein Soll von zehn Stunden und eine besondere Obergrenze von zwölf Stunden; eine Planung über zehn Stunden erzeugt eine Meldung.
- Berechnet wird mit den tatsächlichen Zeiten der Zuweisungen. Beim Doppeldienst zählen die beiden Arbeitsabschnitte, nicht die Unterbrechung.
- Nach jeder Generierung soll ein kurzer Bericht pro Mitarbeiter und Woche mindestens Soll, geplante Stunden und die vorzeichenbehaftete Abweichung zeigen.

Diese Auslegung ist noch nicht bestätigt. Insbesondere ist offen, ob der Korridor eine zwingende Grenze oder ein Optimierungsziel ist und wie Typ1, AH-Profile und Abwesenheiten behandelt werden.

### 5. Trennung der späteren Systeme

Nicht jede neue Regel gehört in die Implementierung von System 03. Nach der vorhandenen Master-Roadmap wäre folgende Aufteilung sicher:

- System 03 speichert Mitarbeiteridentität, Profil beziehungsweise Profilvorlage, Wochen-Soll und reguläre Einsatzfreigaben.
- System 07 definiert die Wochenstundenregel, die besondere Behandlung von Typ1 und die AH-Ausnahmen als zentrale Fachregeln.
- System 08 stellt gesperrte beziehungsweise vorgetragene Zuweisungen als Planungsdaten bereit.
- System 09 setzt die automatische Generierung und die Optimierung der Wochenstunden um.
- System 10 erzeugt strukturierte Notfallvorschläge und den verständlichen Bericht.
- System 11 ermöglicht die bewusst bestätigte manuelle Übernahme eines Vorschlags, ohne Stammdaten stillschweigend zu ändern.

Die Roadmap für System 03 sollte diese Übergaben verbindlich verankern, aber noch keine OR-Tools-, Plan- oder Konfliktlogik implementieren.

## Fragen zur fachlichen Bestätigung

Die Fragen können im Dokument oder im Chat anhand ihrer Kennung beantwortet werden.

### A – Bedeutung und Lebenszyklus der Mitarbeitertypen

#### A-01 – Bindender Typ oder Eingabevorlage

Was soll die Auswahl eines Mitarbeitertyps fachlich bedeuten?

- [X] Empfohlen: Der Typ ist ein vordefiniertes Profil. Er setzt Wochen-Soll und Freigaben als getrennte Eigenschaften; begründete individuelle Änderungen bleiben möglich.
- [ ] Der Typ ist dauerhaft bindend. Wochen-Soll und Freigaben können nur durch Wechsel des gesamten Typs geändert werden.
- [ ] Andere Regel.

Antwort:

#### A-02 – Zuordnung pro Mitarbeiter

Besitzt jeder Mitarbeiter genau einen dieser Typen oder darf eine Person mehrere Typen beziehungsweise gar keinen Typ besitzen?

Empfehlung: Genau ein Profil ist für die normale Eingabe ausgewählt; die fachlich wirksamen Einzelwerte bleiben trotzdem getrennt nachvollziehbar.

Antwort: Jedem Mitarbeiter wird genau einer dieser Typen zugeordnet

#### A-03 – Änderung einer Profildefinition

Wenn die Definition eines Typs später geändert wird, sollen bereits angelegte Mitarbeitende automatisch mitgeändert werden?

- [ ] Empfohlen: Nein. Bestehende Mitarbeiter behalten ihre bestätigten Werte; eine Übernahme der neuen Profilfassung erfolgt bewusst.
- [X] Ja. Alle zugeordneten Mitarbeitenden ändern sich automatisch.
- [ ] Profile dürfen nach Auslieferung überhaupt nicht geändert werden.

Antwort: Wenn sich ein Typ Ändert, ändert das natürlich auch den Typ des mitarbeiters, da das genau der selbe Typ ist.
Beispiel:
Mitarbeiter A ist Typ25.
Mitarbeiter B ist Typ25.
Mitarbeiter C ist Typ30.
Ändert sich jetzt die Zuständigkeit für Typ25, beispielsweise War typ25 vorher nicht freigegeben für Cafeteria und wird jetzt dafür freigegeben, bedeutet dass, das MA A und B für die Cafeteria freigegeben sind.
Im realistischen nutzen der App wird MA A und B einfach ein neuer Typ zugewiesen, um die Qualifikatioon zu ändern.
Später sollen wir noch Typen hinzufügen und löschen können, wir starten aber mit den gegebenen.

#### A-04 – Pflege des Typenkatalogs

Sind die acht genannten Typen feste Startwerte oder soll die Service-Leitung Typen später selbst anlegen, umbenennen und bearbeiten können?

Empfehlung: Zunächst feste, stabile Startprofile; Erweiterung oder Bearbeitung erst nach einem konkret bestätigten Bedienbedarf.

Antwort: Deine empfehlung. Wir starten mit denen, schreiben den Code aber so, dass wir erweiterung und entfernen neuer Typen vorbereiten und später einfach implementieren können.

#### A-05 – Sichtbare Bezeichnung

Sollen die Codes exakt als `Typ1`, `Typ25`, `Typ30`, `Typ30a`, `Typ35`, `Typ35a`, `TypAH1` und `TypAH2` in der App angezeigt werden, oder werden zusätzlich verständliche Namen benötigt?

Antwort: Ja

### B – Stundenwerte und Dienstfreigaben

#### B-01 – Stundenwert von Typ35

Ist `Typ35 = 25 Stunden/Woche` bewusst so gemeint oder soll `Typ35` 35 Stunden/Woche besitzen?

Antwort: Typ35 = 35 Stunden/Woche

#### B-02 – Stundenwert der a-Varianten

Gilt für `Typ30a` ebenfalls 30 Stunden/Woche und für `Typ35a` derselbe bestätigte Stundenwert wie für `Typ35`?

Antwort: ja

#### B-03 – Bedeutung von „alle Dienste“

Bedeutet „alle Dienste“ die Freigabe für alle vier normalen Diensttypen und beide Einsatzorte, sodass `D` und `Spr` aus diesen Freigaben ebenfalls möglich sind?

- Frühdienst
- Spätdienst
- Cafeteria-Dienst A
- Cafeteria-Dienst B
- daraus Doppeldienst `D`
- daraus bei erfüllter Notfallregel Springer `Spr`

Antwort: Ja

#### B-04 – Doppeldienst bei TypAH2

`TypAH2` darf laut Vorgabe den Doppeldienst, aber nicht automatisch als einzelnen Frühdienst geplant werden. Welche Regel ist gemeint?

- [X] Der Frühdienst ist für `TypAH2` innerhalb von `D` regulär erlaubt, als einzelner Frühdienst aber nur ein manueller Lösungsvorschlag.
- [ ] `TypAH2` darf `D` doch nicht automatisch übernehmen.
- [ ] `TypAH2` darf auch einen einzelnen Frühdienst automatisch übernehmen.
- [ ] Andere Regel.

Antwort:

#### B-05 – Springer bei TypAH2

Darf `TypAH2` wegen der regulären Freigaben für Cafeteria-Dienst B und Spätdienst automatisch für `Spr` berücksichtigt werden, wenn dessen bereits bestätigte Notfallbedingung erfüllt ist?

Antwort: Ja, aber lass uns das optional machen.

#### B-06 – Notfallvorschlag Frühdienst für AH

Soll der Frühdienstvorschlag für `TypAH1` und `TypAH2` ausschließlich dann erscheinen, wenn nach der regulären Generierung ein Frühdienst ganz oder teilweise ungedeckt bleibt?

Empfehlung: Ja. Der Solver trägt AH dort nie automatisch ein; die Konflikterklärung nennt die Person nur als ausdrücklich zu bestätigende manuelle Möglichkeit.

Antwort: Ja

#### B-07 – Wirkung eines angenommenen AH-Vorschlags

Wenn die Service-Leitung einen vorgeschlagenen Frühdienst manuell übernimmt:

- bleibt die Einteilung als sichtbare Sonderabweichung markiert?
- bleibt die normale Dienstfreigabe der Person unverändert?
- muss die Übernahme jedes Mal ausdrücklich bestätigt werden?

Empfehlung: dreimal ja, entsprechend der bereits bestätigten Regel für manuelle Sonderzuweisungen.

Antwort: dreimal ja

### C – Besondere Behandlung von Typ1

#### C-01 – Bedeutung von „wird immer vorgetragen“

Woher kommen die vorgetragenen Einteilungen von Typ1?

- [X] Sie werden für den neuen Zeitraum von der Service-Leitung vor der Generierung manuell eingetragen.
- [ ] Sie werden aus der vorherigen Woche nach einem noch zu definierenden Wiederholungsmuster übernommen.
- [ ] Sie stammen aus einer dauerhaft hinterlegten persönlichen Wochenvorlage.
- [ ] Andere Herkunft.

Antwort:

#### C-02 – Verhalten ohne vorgetragene Einteilung

Wenn für Typ1 an einem Tag oder in einer ganzen Woche nichts vorgetragen ist, darf die Generierung dort selbst einen Dienst eintragen oder bleibt Typ1 vollständig außerhalb der automatischen Verteilung?

Antwort: Wenn Typ1 gar nicht eingetragen ist, kann keine generierung erfolgen. Typ1 bleibt vollständig außerhalb der generierung, darf aber als Lösungsvorschlag zur allerletzten Rettung eines Dienstes vorgeschlagen werden.

#### C-03 – Umfang des Schutzes

Werden alle vorhandenen Einteilungen von Typ1 automatisch wie gesperrte Zuweisungen behandelt, oder muss die Service-Leitung sie einzeln sperren?

Empfehlung: Wenn „wird nicht durch die Generierung geändert“ ausnahmslos gilt, sollte der Schutz aus dem Typ1-Profil folgen und nicht von versehentlich fehlenden Einzelsperren abhängen.

Antwort: Alle vorhandene eintragungen von Typ1 werden automatisch wie gesperrte Zuweisungen behandelt

#### C-04 – Typ1 und Stundenkorridor

Was geschieht, wenn die vorgetragenen Typ1-Dienste außerhalb von 37 bis 43 Stunden liegen?

- [X] Die Generierung bleibt erlaubt, verändert Typ1 nicht und meldet die Abweichung deutlich.
- [ ] Die Generierung wird vor dem Start blockiert, bis die Vorbelegung korrigiert ist.
- [ ] Der allgemeine Korridor gilt für Typ1 nicht.
- [ ] Andere Regel.

Antwort:

#### C-05 – Anzahl Typ1-Mitarbeitende

Kann es genau eine Person vom Typ1 geben, oder könnten mehrere Mitarbeitende dieses Profil besitzen?

Antwort: genau eine Person.

### D – Wochen-Soll, Toleranz und Bericht

#### D-01 – Bezugszeitraum

Gilt der Korridor von plus/minus drei Stunden für jede Person in jeder einzelnen Woche von Montag bis Sonntag, auch wenn mehrere Wochen gemeinsam generiert werden?

Empfehlung: Ja; keine Durchschnittsbildung über mehrere Wochen.

Antwort: Ja

#### D-02 – Zwingende Grenze oder weiches Ziel

Soll die Generierung eine Person niemals außerhalb des erlaubten Korridors einplanen, selbst wenn dadurch Bedarf ungedeckt bleibt?

Empfehlung: Der äußere Korridor ist eine zwingende Grenze. Innerhalb des Korridors wird die absolute Abweichung vom Soll minimiert. Ungedeckter Bedarf bleibt entsprechend der bestehenden Architektur sichtbar.

Antwort: deine Empfehlung. Wenn Bedarf ungedeckt bleibt, werden Lösungsvorschläge gegeben und der Plan kann manuell angepasst/ neu generiert werden

#### D-03 – Rangfolge gegenüber Bedarfsdeckung

Welche Reihenfolge ist gewünscht, solange alle zwingenden Grenzen eingehalten werden?

- [X] Empfohlen gemäß bestehender Architektur: zuerst ungedeckten Bedarf minimieren, danach die Abweichung der Wochenstunden minimieren.
- [ ] Zuerst Wochenstunden möglichst exakt treffen, danach Bedarfsdeckung optimieren.
- [ ] Andere Priorisierung.

Antwort:

#### D-04 – AH-Stundenkorridor

Welche genaue Grenze gilt für `TypAH1` und `TypAH2`?

- [X] Soll 10 Stunden; automatisch 7 bis 12 Stunden zulässig; über 10 bis 12 Stunden mit Meldung.
- [ ] Soll 10 Stunden; automatisch höchstens 10 Stunden; 10 bis 12 Stunden nur nach manueller Bestätigung.
- [ ] Allgemein plus/minus drei Stunden, also 7 bis 13 Stunden; ab mehr als 10 Stunden mit Meldung.
- [ ] Andere Regel.

Antwort: Hier zuerst versuchen mindestens 10 zu kriegen, maximal aber 12 stunden. nur wenn unbedingt sein muss weniger als 10

#### D-05 – Abwesenheit und anteiliges Soll

Bleibt das volle Wochen-Soll auch bei Urlaub, Krankheit oder nur teilweiser Verfügbarkeit bestehen, oder wird es für die Planungsoptimierung reduziert? Diese Entscheidung würde später mit System 06 und System 07 umgesetzt.

Antwort: wird reduziert

#### D-06 – Berechnung der geplanten Stunden

Bitte bestätigen: Es zählen die tatsächlichen Zeiten der geplanten Zuweisungen. Bei `D` werden Früh- und Spätdienst addiert, die Unterbrechung zählt nicht. Bei `Spr` zählt die bereits bestätigte tatsächliche zusammenhängende Einsatzzeit.

Antwort: bestätigt

#### D-07 – Inhalt des kurzen Berichts

Soll der Bericht für jede Person und jede Woche folgende Werte zeigen?

- Mitarbeitertyp beziehungsweise Profil,
- Wochen-Soll,
- geplante Stunden,
- Abweichung mit Vorzeichen, beispielsweise `−1:30 Stunden`,
- Warnung bei einer besonderen AH-Überschreitung,
- Hinweis bei einer unveränderten Typ1-Abweichung.

Antwort: ja

#### D-08 – Bericht bei mehreren Generierungen

Soll der Bericht als Bestandteil jedes gespeicherten Planungsergebnisses erhalten bleiben, damit die damalige Abweichung später nachvollziehbar ist, oder genügt eine nur unmittelbar sichtbare Zusammenfassung?

Empfehlung: Als unveränderlicher Bestandteil des Planungsergebnisses speichern; die Werte können zusätzlich direkt nach der Generierung kurz angezeigt werden.

Antwort: deine empfehlung

### E – Qualifikationen und Mitarbeiterstammdaten

#### E-01 – Zusätzliche Qualifikationen

Gibt es neben den Einsatzort- und Diensttypfreigaben weitere echte Qualifikationen, die in System 03 benötigt werden? Beispiele werden bewusst nicht als Anforderungen vorausgesetzt.

- [X] Nein. Für die erste Fassung genügen Profil, Wochen-Soll und Einsatzfreigaben.
- [ ] Ja. Folgende Qualifikationen werden benötigt.

Antwort:

#### E-02 – Name und interne Identität

Welche Namensform soll die Mitarbeiterverwaltung verwenden?

- [X] Empfohlen: Vorname und Nachname getrennt; unsichtbare stabile `EmployeeId`; keine Personalnummer ohne konkreten Bedarf; gleiche Namen sind erlaubt.
- [ ] Ein gemeinsames sichtbares Namensfeld.
- [ ] Zusätzlich wird eine betriebliche Personalnummer benötigt.
- [ ] Andere Regel.

Antwort:

#### E-03 – Ausscheiden von Mitarbeitenden

Wie soll mit Mitarbeitenden umgegangen werden, die nicht mehr beschäftigt sind?

- [X] Empfohlen: deaktivieren; historische Pläne bleiben unverändert. Endgültiges Löschen höchstens vor der ersten Verwendung.
- [ ] Immer endgültig löschen können.
- [ ] Andere Regel.

Antwort:

#### E-04 – Erste Mitarbeiteroberfläche

Reichen für System 03 zunächst eine Übersicht sowie getrennte Vorgänge für Anlegen und Bearbeiten, ohne Suche, Filter und freie Sortierung?

Antwort: Ja

### F – Folgefragen nach Durchsicht der Antworten

#### F-01 – Gemeinsamer, sofort wirksamer Mitarbeitertyp

Die Antworten A-01 bis A-03 werden zusammen so verstanden:

- Jeder Mitarbeiter verweist auf genau einen gemeinsam definierten Mitarbeitertyp.
- Wochen-Soll und reguläre Freigaben sind fachlich getrennte Bestandteile dieses Typs.
- Sie werden nicht als unabhängige Kopien beim Mitarbeiter gespeichert.
- Eine Änderung des Typs wirkt sofort auf alle ihm zugeordneten Mitarbeitenden.
- Individuelle Abweichungen werden nicht innerhalb des Mitarbeiters gepflegt; stattdessen wird ein anderer oder später ein neuer Typ zugewiesen.

Ist dieses Verständnis vollständig richtig? Damit wäre der Typ keine bloße Eingabevorlage, sondern eine gemeinsam referenzierte, bindende Profildefinition. Die bisherige Architekturformulierung müsste entsprechend präzisiert werden.

Antwort: Ja das ist richtig

#### F-02 – Sichtbarer Typname

Die Antwort „Ja“ zu A-05 ist noch mehrdeutig. Soll in der App ausschließlich der exakte Code wie `Typ30a` sichtbar sein, oder zusätzlich ein verständlicher Name wie beispielsweise `Typ30a – alle Dienste`?

Empfehlung: Der stabile Code bleibt sichtbar; ein zusätzlicher verständlicher Name kann später pflegbar ergänzt werden, ohne die Kennung zu verändern.

Antwort: deine empfehlung

#### F-03 – Optionaler Springer für TypAH2

Wo soll die Option „TypAH2 darf für `Spr` berücksichtigt werden“ eingestellt werden?

- [X] Empfohlen: vor jeder Generierung als ausdrückliche Option für den gesamten Planungslauf; standardmäßig ausgeschaltet.
- [ ] dauerhaft als globale Einstellung der App.
- [ ] individuell je TypAH2-Mitarbeiter.
- [ ] andere Regel.

Wenn die Option eingeschaltet ist, bleibt zusätzlich die bereits bestätigte allgemeine `Spr`-Notfallbedingung bestehen.

Antwort:

#### F-04 – Genaue Generierungsvoraussetzung für Typ1

Was muss vor einer Generierung für die genau eine Typ1-Person vorhanden sein?

- [ ] Es genügt, dass genau eine aktive Typ1-Person in den Mitarbeiterstammdaten existiert.
- [X] Zusätzlich muss in jeder zu generierenden Woche mindestens ein Typ1-Dienst manuell vorgetragen sein.
- [ ] Alle Typ1-Dienste des gesamten Planungszeitraums müssen vorab vollständig manuell eingetragen sein.
- [ ] andere, konkret prüfbare Voraussetzung.

Wie gilt diese Voraussetzung, wenn Typ1 für eine ganze Woche als Urlaub, Krankheit oder sonstige Abwesenheit erfasst ist?

Antwort: Urlaub, krankheit und sonstige abwesenheit wird auch in den Plan eingetragen. Diese regeln werden wir später noch miteinander ausarbeiten

#### F-05 – Typ1 als „allerletzte Rettung“

Bitte bestätigen oder korrigieren:

- Typ1 wird niemals automatisch auf einen offenen Dienst gesetzt.
- Typ1 darf für jeden weiterhin ungedeckten normalen Dienst als manuelle Lösung vorgeschlagen werden.
- Dieser Vorschlag erscheint erst, wenn keine regulär automatisch zulässige Lösung gefunden wurde.
- Bei einem offenen Frühdienst werden zuerst die besonderen AH-Lösungsmöglichkeiten genannt; Typ1 folgt erst danach als letzte Möglichkeit.
- Eine angenommene Typ1-Sonderzuweisung bleibt wie andere manuelle Sonderentscheidungen sichtbar und wird bei einer späteren Generierung automatisch geschützt.

Antwort: Ja, aber was bedeutet "automatisch geschützt"?

#### F-06 – Rangfolge innerhalb des AH-Stundenkorridors

Die Antwort zu D-04 wird vorläufig so verstanden:

1. möglichst genau 10 Stunden,
2. falls 10 Stunden nicht erreichbar sind, bevorzugt mehr als 10 bis höchstens 12 Stunden,
3. erst wenn das nicht sinnvoll möglich ist, weniger als 10 bis mindestens 7 Stunden,
4. jede Überschreitung über 10 Stunden wird gemeldet.

Beispiel: Wenn ansonsten gleichwertige Pläne 9 oder 11 Stunden ergeben, soll 11 Stunden bevorzugt werden. Ist diese asymmetrische Rangfolge richtig?

Antwort: Richtig

#### F-07 – Reduktion des Wochen-Solls bei Abwesenheit

Nach welcher Regel wird das Wochen-Soll bei Urlaub, Krankheit oder teilweiser Verfügbarkeit reduziert? Ohne bestätigte Verteilung der Vertragsstunden auf Wochentage lässt sich die Reduktion noch nicht eindeutig berechnen.

Empfehlung: System 03 speichert zunächst nur das ungekürzte Wochen-Soll. Die konkrete Reduktionsformel wird gemeinsam mit Abwesenheiten und Verfügbarkeiten in System 06 festgelegt und anschließend als zentrale Regel in System 07 verwendet. Die neue Roadmap hält diese Übergabe ausdrücklich offen und erfindet noch keine Formel.

Ist dieses Aufschieben der Berechnungsformel in Ordnung?

Antwort: Ja korrekt.

#### F-08 – Späteres Entfernen eines verwendeten Mitarbeitertyps

Wie soll ein Mitarbeitertyp später entfernt werden, wenn ihm noch aktive oder deaktivierte Mitarbeitende zugeordnet sind?

- [X] Empfohlen: Entfernen ist erst möglich, nachdem alle Mitarbeitenden bewusst einem anderen Typ zugeordnet wurden; historische Planmomentaufnahmen bleiben unverändert.
- [ ] Der Typ wird nur deaktiviert und bleibt für bestehende Zuordnungen erhalten.
- [ ] Beim Entfernen werden Mitarbeitende automatisch auf einen anderen Typ umgestellt.
- [ ] andere Regel.

Die eigentliche Löschfunktion wird noch nicht in System 03 umgesetzt; die bestätigte Regel bestimmt aber Fremdschlüssel und Erweiterungsgrenzen.

Antwort: wenn man einen Mitarbeitertyp entfernen möchte, gibt es eine Meldung, welche alle Mitarbeiter mit dem zu entfernenden Typen nennt. Es wird gesagt dass der Tyo nicht entfernd werden kann, da noch X, Y, ... Mitarbeiter diesen Typs sind.

#### F-09 – Genau eine aktive Typ1-Person

Soll die Regel „genau eine Person vom Typ1“ nur für aktive Mitarbeitende gelten? Dann dürfte eine ausgeschiedene, deaktivierte frühere Typ1-Person historisch erhalten bleiben, während genau eine andere aktive Person Typ1 besitzt.

Empfehlung: Ja; genau eine aktive Typ1-Person, beliebig viele historisch deaktivierte frühere Zuordnungen.

Antwort: Ja.

#### F-10 – Wirkung einer Typänderung auf Pläne

Bitte bestätigen oder korrigieren:

- Eine Änderung an einem Mitarbeitertyp wirkt auf die Stammdaten aller zugeordneten Mitarbeitenden und auf nachfolgende Generierungen.
- Bereits abgenommene Planversionen bleiben als Momentaufnahme unverändert.
- Bereits vorhandene, noch nicht abgenommene Planentwürfe werden nicht stillschweigend umgeschrieben; sie werden bei der nächsten Prüfung als möglicherweise nicht mehr passend gekennzeichnet.

Antwort: Ja

#### F-11 – Name von System 03

Da für die erste Fassung keine zusätzlichen Qualifikationen benötigt werden und die Einsatzmöglichkeiten über Mitarbeitertypen geregelt werden, passt der bisherige Name „Mitarbeitende, Arbeitszeitmodelle und Qualifikationen“ nicht mehr vollständig.

Soll System 03 bei der Roadmap-Überarbeitung in „Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben“ umbenannt werden, einschließlich Roadmap-Dateiname und aller Verweise?

Empfehlung: Ja. Dadurch entspricht der Name dem bestätigten Fachmodell; es bleiben keine leeren Qualifikationsschritte in der Roadmap.

Antwort: Ja, bitte.

## Vorgesehene Code-Anker für die spätere Roadmap-Überarbeitung

Nach der fachlichen Klärung sollte die Roadmap mindestens folgende vorhandene Anker ausdrücklich nennen:

### Domain

- `src/Salztal.Dienstplanung.Domain/WorkLocations/WorkLocationId.cs`
- `src/Salztal.Dienstplanung.Domain/WorkLocations/InitialWorkLocationCatalog.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/ShiftTypeId.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftTypes/InitialShiftTypeCatalog.cs`
- `src/Salztal.Dienstplanung.Domain/ShiftPatterns/InitialShiftPatternCatalog.cs`

Neue Mitarbeiter- und Profiltypen gehören unter `Salztal.Dienstplanung.Domain.Employees`. Sie sollen die vorhandenen stabilen Kennungen referenzieren und weder Einsatzortnamen noch Dienstzeiten duplizieren. `D` und `Spr` bleiben Muster aus System 04.

### Application

- `src/Salztal.Dienstplanung.Application/ServiceCatalog/IServiceCatalogReader.cs`
- `src/Salztal.Dienstplanung.Application/ServiceCatalog/ServiceCatalogData.cs`
- `src/Salztal.Dienstplanung.Application/ServiceCatalog/GetServiceCatalogQuery.cs`

Die vorhandene Katalogquelle kann die Auswahl und Existenz von Einsatzort- und Diensttypkennungen absichern. Neue Mitarbeiter-Anwendungsfälle erhalten eigene kleine Queries, Commands und Speicherverträge unter `Application.Employees`; eine generische CRUD-Schnittstelle ist weiterhin ausgeschlossen.

### Infrastructure und Migrationen

- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContext.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/ServiceCatalogDbContextFactory.cs`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations/`
- `src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/SqliteServiceCatalogStore.cs`

System 04 besitzt bereits die erste Migration und eine gemeinsame lokale Datenbankdatei. Vor Mitarbeiter-Tabellen muss die Roadmap einen eigenen kleinen technischen Schritt festlegen, der eine einzige koordinierte Migrationsfolge und echte Fremdschlüssel auf die vorhandenen Katalogtabellen sicherstellt. Die veröffentlichte Migration aus System 04 wird nicht nachträglich umgeschrieben.

### Desktop Composition

- `src/Salztal.Dienstplanung.Desktop/Composition/MainWindowComposition.cs`
- `src/Salztal.Dienstplanung.Desktop/App.xaml.cs`

Nur `Desktop.Composition` darf neue konkrete SQLite-Adapter verdrahten. Mitarbeiter-ViewModels dürfen weiterhin nur Application-Verträge verwenden.

## Durch die Roadmap-Überarbeitung berichtigte Dokumentationswidersprüche

Die folgenden Punkte wurden durch die Antworten fachlich geklärt und im MA-01-Dokumentationsschritt berichtigt:

1. Die Leitdokumente sagten zuvor, dass ein Mitarbeiter kein fest programmierter Mitarbeitertyp ist. Die neue bindende Typdefinition ist jetzt in Grundlagen, Architektur und einer eigenen Entscheidung dokumentiert.
2. `STATUS.md` führt System 03 korrekt als aktiv, enthält aber noch eine ältere Zeile, nach der derselbe Entwurf unter `docs/roadmaps/paused` liege.
3. `STATUS.md` bezeichnet ED-10 an einer Stelle als abgenommen, später jedoch noch als offene Abschlussabnahme.
4. Die aktive Roadmap nennt im Abschnitt der manuellen Gates noch eine ausstehende Abschlussabnahme von System 04, obwohl System 04 laut Master-Roadmap und aktuellem Projektstand bereits abgeschlossen ist.
5. Die Typbezeichnung `Typ35` steht in der neuen Vorgabe neben einem Wochen-Soll von 25 Stunden.
6. Die reguläre Doppeldienstfreigabe für `TypAH2` passt nicht ohne Sonderregel zur fehlenden automatischen Frühdienstfreigabe.

Diese Widersprüche sind im überarbeiteten MA-01-Entwurf und den betroffenen aktuellen Dokumenten beseitigt. MA-01 wurde am 2026-09-14 ausdrücklich abgenommen.

## Nächster Schritt

Die Antworten sind vollständig in die abgenommene Roadmap eingearbeitet. MA-02 bis MA-10A einschließlich des Hygiene-Schritts MA-02A sind abgenommen:

- MA-09 wurde am 2026-09-14 sichtbar abgenommen.
- MA-10 mit Anlegen, Bearbeiten, Typwechsel und Deaktivieren ist umgesetzt, automatisch geprüft und sichtbar abgenommen.
- Die neue Anforderung zum Reaktivieren und endgültigen Löschen deaktivierter Mitarbeitender ist als MA-10A ergänzt. Beide Empfehlungen aus `docs/roadmaps/completed/S03_EMPLOYEES_REACTIVATION_DELETION_QUESTIONS.md` sind bestätigt; MA-10A ist umgesetzt, automatisch geprüft und gemeinsam mit MA-10 sichtbar abgenommen.
- MA-11 hat den vollständigen System-03-Umfang und die Übergaben an spätere Systeme abgeglichen. System 03 wurde anschließend ausdrücklich abgenommen und archiviert.
- Planning bleibt weiterhin unverändert.
