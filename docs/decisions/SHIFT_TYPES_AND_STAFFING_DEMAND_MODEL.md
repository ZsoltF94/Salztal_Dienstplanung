# Entscheidung: Diensttypen, tatsächliche Bedarfszeiten und Springer-Einsatz

Status: Fachlich bestätigt am 2026-09-13; ED-01 bis ED-07 abgenommen

## Anlass

Die bisherige Grundlage nahm an, dass ein Diensttyp eine unveränderliche Zeit besitzt und der Bedarf seine Dauer vollständig aus diesem Diensttyp übernimmt. Die Service-Leitung benötigt jedoch sowohl bearbeitbare Standardzeiten als auch abweichende tatsächliche Zeiten für ein einzelnes Datum.

Außerdem gibt es samstags einen Springer-Einsatz, der einen Cafeteria-Dienst mit einem anschließenden Restaurant-Spätdienst verbindet. Dieser Einsatz wechselt den Einsatzort und ist deshalb kein Doppeldienst nach der bestehenden Definition.

## Entscheidung

### Einsatzorte

- Cafeteria und Restaurant sind die Startwerte der ersten Fassung.
- Weitere Einsatzorte werden technisch durch stabile Datenkennungen vorbereitet und nicht als fest programmierte Aufzählung ausgeschlossen.
- Die erste Bedienfassung erlaubt Anzeigen und Bearbeiten; das sichtbare Anlegen und Löschen wird später ergänzt.
- Cafeteria wird zusätzlich zum Text gelb, Restaurant rot gekennzeichnet. Farbe bleibt niemals der einzige Informationsträger.
- Einsatzorte besitzen keine eigenen Öffnungs-, Betriebs- oder Arbeitszeiten. Zeiten werden entweder als Standardzeit eines Diensttyps oder als tatsächliche Zeit eines Bedarfs geführt.

### Diensttypen und Standardzeiten

- Ein Diensttyp besitzt eine stabile Kennung, eine Bezeichnung beziehungsweise Tabellenanzeige, genau einen Einsatzort und eine bearbeitbare Standardzeit.
- Die Standardzeit ist der Ausgangswert für neue Bedarfsvorgaben und zeigt, wann der Dienst normalerweise stattfindet.
- Die Standardzeit ist nicht zwingend die tatsächliche Arbeitszeit jedes einzelnen Bedarfseintrags.
- Mitarbeitende erhalten später getrennte Freigaben für einzelne Diensttypen. Eine Freigabe für Cafeteria-Dienst A erzeugt nicht automatisch eine Freigabe für Cafeteria-Dienst B.

### Bedarf und tatsächliche Zeiten

- Jeder Bedarf gehört zu genau einem Einsatzort und verlangt genau einen vorhandenen normalen Diensttyp.
- Ein Bedarf verlangt niemals den Doppeldienst `D` oder den Springer `Spr`; diese Muster verbinden später zwei getrennte Bedarfe beziehungsweise Zuweisungen.
- Der Bedarf besitzt die tatsächlich zu besetzende Anfangs- und Endzeit sowie die benötigte Personenzahl.
- Seine tatsächliche Zeit darf nach einer ausdrücklichen Eingabe von der Standardzeit des Diensttyps abweichen.
- Eine Standardänderung wirkt auf neue zukünftige Bedarfsvorgaben. Eine Datums-Ausnahme verändert nur das ausgewählte Datum.
- Frühere und abgenommene Planversionen behalten ihre damaligen tatsächlichen Zeiten als unveränderliche Momentaufnahme.
- Arbeits- und Bedarfsstunden werden aus den tatsächlichen Zeiträumen berechnet, nicht aus einer gegebenenfalls abweichenden Standardzeit.
- Die automatische Planung erfindet weder Diensttypen noch Zeitabweichungen.
- Automatische Überbesetzung bleibt unzulässig.

### Bestätigte Diensttypen und Einsatzmuster

| Dienst oder Muster | Kategorie | Anzeige | Standardzeit beziehungsweise Regel | Einsatzort |
|---|---|---|---|---|
| Frühdienst | Diensttyp | `F` | 06:30–13:30 Uhr | Restaurant |
| Spätdienst | Diensttyp | `S` | 16:30–19:30 Uhr | Restaurant |
| Doppeldienst | Einsatzmuster | `D` | Frühdienst und Spätdienst | Restaurant |
| Cafeteria-Dienst A | Diensttyp | tatsächliche Zeit, zum Beispiel `13:30-20:30` | 13:30–20:30 Uhr | Cafeteria |
| Cafeteria-Dienst B | Diensttyp | tatsächliche Zeit, zum Beispiel `13:30-17:30` | 13:30–17:30 Uhr | Cafeteria |
| Springer-Einsatz | Einsatzmuster | `Spr` | Wechselzeit folgt dem tatsächlichen Bedarf | Cafeteria, danach Restaurant |

Nur die vier als Diensttyp gekennzeichneten Einträge besitzen eine eigene `ShiftTypeId` und können von einem einzelnen Bedarf verlangt werden. `D` und `Spr` besitzen eine getrennte Musteridentität und referenzieren ihre jeweiligen Diensttypen.

- Zeiten werden in der Bedienung in 30-Minuten-Schritten eingegeben und technisch in ganzen Minuten gespeichert.
- Dienste über Mitternacht gehören nicht zum aktuellen Umfang.
- Innerhalb eines einzelnen Dienstes gibt es aktuell keine Unterbrechung, die nicht als Arbeitszeit zählt.

### Doppeldienst

- Der Doppeldienst ist ausschließlich die Kombination aus Frühdienst und Spätdienst im Restaurant.
- Beide Teile bleiben getrennte Dienstabschnitte desselben Einsatzortes.
- Bei den Standardzeiten liegt zwischen 13:30 und 16:30 Uhr eine dreistündige Unterbrechung, die nicht als Arbeitszeit zählt.
- Bei Standardzeiten umfasst der Doppeldienst zehn Arbeitsstunden.
- Die tatsächlich anzurechnende Arbeitszeit eines Plans folgt den tatsächlichen Zeiten seiner beiden Abschnitte.

### Springer-Einsatz

- `Spr` ist ein eigenes sichtbares samstägliches Notfallmuster und kein Doppeldienst.
- Es verbindet Cafeteria-Dienst B mit einem anschließenden Restaurant-Spätdienst.
- Der Wechsel erfolgt am tatsächlichen Ende des zweiten Cafeteria-Bedarfs und nicht zu einer fest programmierten Uhrzeit.
- Die Zeit vom Beginn des Cafeteria-Abschnitts bis zum Ende des Restaurant-Abschnitts zählt vollständig als Arbeitszeit; es gibt keine dazwischenliegende Unterbrechung.
- Die eingesetzte Person benötigt die Einsatzfreigabe für Cafeteria und Restaurant sowie die Freigaben für beide Diensttypen, aber keine zusätzliche Springer-Qualifikation.
- Die automatische Planung darf `Spr` nur verwenden, wenn sonst ein Restaurant-Spätdienst unbesetzt bleibt.
- Vor dem tatsächlichen Wechsel ist die Springer-Person nicht im Restaurant anwesend. Eine dadurch entstehende Teilunterdeckung des Restaurantbedarfs bleibt sichtbar und wird nicht fälschlich als vollständige Besetzung gemeldet.
- `Spr` wird zusätzlich zum Text blau gekennzeichnet.

### Löschen und historische Momentaufnahmen

- Einsatzorte und Diensttypen sollen später aus dem aktuellen Stammdatenkatalog gelöscht werden können.
- Das Löschen eines Stammdateneintrags verändert keine frühere unveränderliche Planversion.
- Planversionen bewahren die für ihre Anzeige und Berechnung benötigten damaligen Namen, Farben, Diensttypen und tatsächlichen Zeiten als eigene Momentaufnahme.
- Das genaue Verhalten für noch aktive zukünftige Bedarfe oder Entwürfe wird vor Umsetzung der späteren Löschfunktion gesondert festgelegt.

## Systemgrenzen

- System 03 ordnet Mitarbeitenden Einsatzorte und einzelne zulässige Diensttypen zu.
- System 04 verwaltet Einsatzorte, normale Diensttypen sowie die Musterdefinitionen für Doppeldienst und Springer.
- System 05 verwaltet Standardbedarfe, tatsächliche Bedarfszeiten, Personenzahlen und datumsbezogene Ausnahmen.
- System 07 legt die spätere Regelpriorität und Konfliktbewertung fest.
- System 09 setzt die automatische Verwendung des Springers und die sichtbare Teilunterdeckung um.
- System 11 stellt Plan, tatsächliche Zeiten und verbleibende Unterdeckung dar.
- System 12 bewahrt abgenommene Planversionen als unveränderliche Momentaufnahmen.

## Auswirkungen auf die bisherige Grundlage

Die frühere Aussage „Der feste Diensttyp gibt immer die tatsächliche Arbeitsdauer vor“ ist ersetzt. Künftig gilt:

> Der Diensttyp besitzt eine bearbeitbare Standardzeit. Der konkrete Bedarf besitzt die tatsächliche Zeit und verlangt genau einen Diensttyp. Arbeits- und Bedarfsstunden folgen der tatsächlichen Zeit.

Die Aussage, dass die automatische Planung keine eigenen Diensttypen oder Arbeitszeiten erfindet, bleibt bestehen. Eine abweichende Zeit ist nur zulässig, wenn die Service-Leitung sie zuvor ausdrücklich als Standard oder Datums-Ausnahme festgelegt hat.

## Bewusst noch offen

- genaue Farbtöne und barrierearme Darstellung in WPF,
- Verhalten beim Löschen eines Stammdateneintrags, der noch von zukünftigen Bedarfen oder nicht abgenommenen Entwürfen verwendet wird,
- konkrete UI-Anordnung innerhalb der gemeinsam gewählten Einsatzort- und Diensttypseite,
- genaue Konfliktpriorisierung der Springer-Teilunterdeckung in den späteren Systemen 07 und 09.

Diese Punkte blockieren noch keine Dokumentation des fachlichen Modells. Sie werden vor dem jeweils betroffenen Implementierungsschritt geklärt und abgenommen.
