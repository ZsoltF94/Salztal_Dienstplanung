# Grundlagen und Entscheidungen

## Wofür ist die App gedacht?

Die App soll die Service-Leitung beim Erstellen von Arbeitsplänen unterstützen. Sie ist für den gastronomischen Servicebereich einer Rehaklinik vorgesehen.

Die Service-Leitung trägt die Mitarbeitenden und ihre Einsatzmöglichkeiten ein. Danach kann die App für eine gewünschte Anzahl von Wochen einen möglichst guten Dienstplan vorschlagen. Normalerweise werden drei Wochen im Voraus geplant.

## Wo läuft die App?

- Die App soll auf Windows 11 funktionieren.
- Sie soll vollständig ohne Internet nutzbar sein.
- Alle Arbeitsdaten werden lokal auf dem Windows-PC gespeichert.
- Es wird zunächst nur ein Benutzer benötigt. Eine Anmeldung ist nicht vorgesehen.
- Wenn möglich, soll die App ohne Installation aus einem entpackten Ordner gestartet werden können.
- Die Bedienung der App wird auf Deutsch sein.

## Mitarbeitende

Für jeden Mitarbeiter werden später der Name und die für die Planung notwendigen Eigenschaften eingetragen. Dazu gehören zum Beispiel:

- die vereinbarten Wochenstunden,
- mögliche Einsatzorte,
- erlaubte Dienste,
- Urlaub,
- Krankheit,
- Fortbildungen,
- Wunschfrei,
- andere Einschränkungen der Verfügbarkeit.

Wochenstunden und Einsatzmöglichkeiten werden getrennt gespeichert. Eine Person mit 35 Wochenstunden kann dadurch unabhängig davon für die Cafeteria, das Restaurant oder beide Bereiche freigegeben werden. Zusätzlich wird später je Diensttyp festgelegt, welche Person ihn übernehmen darf; Cafeteria-Dienst A und B sind dabei getrennte Freigaben.

Die genauen Mitarbeitertypen und Regeln werden erst in einem späteren eigenen Schritt festgelegt.

## Einsatzorte und Dienste

Die zunächst bekannten Einsatzorte sind:

- Cafeteria,
- Restaurant.

Weitere Einsatzorte können später ergänzt werden. In der ersten Bedienfassung werden die vorhandenen Einträge angezeigt und bearbeitet; das Anlegen weiterer Einträge wird technisch vorbereitet und später ergänzt.

Frühdienst, Spätdienst sowie Cafeteria-Dienst A und B sind die normalen Diensttypen. Jeder von ihnen besitzt eine bearbeitbare Standardzeit. Diese zeigt, wann der Dienst normalerweise stattfindet und wird als Ausgangswert für neue Bedarfsvorgaben verwendet.

Die tatsächlich benötigte Zeit kann für ein bestimmtes Datum ausdrücklich geändert werden, ohne den Standard für zukünftige Wochen zu verändern. Der Bedarf verlangt weiterhin genau einen vorhandenen normalen Diensttyp. Die App darf weder einen Diensttyp noch eine Zeitabweichung selbst erfinden.

Der Restaurant-Doppeldienst `D` besteht ausschließlich aus Frühdienst und Spätdienst. Die Unterbrechung zwischen beiden Teilen zählt nicht als Arbeitszeit. Beide Teile bleiben getrennte Dienste am selben Einsatzort.

Der samstägliche Springer `Spr` ist dagegen kein Doppeldienst. Er arbeitet zunächst den Cafeteria-Dienst B bis zum Ende des tatsächlichen Bedarfs und wechselt anschließend ins Restaurant. Die App darf ihn automatisch nur als Notfall verwenden, wenn sonst ein Spätdienst unbesetzt bleibt. Die vor seinem Wechsel fehlende Restaurant-Besetzung bleibt sichtbar.

`D` und `Spr` sind zusammengesetzte Einsatzmuster und keine zusätzlichen Diensttypen für einen einzelnen Bedarf. Ein einzelner Bedarf verlangt niemals `D` oder `Spr`.

## Personal- und Stundenbedarf

Für jeden Einsatzort wird je Wochentag beziehungsweise konkretem Datum festgelegt, wie viele Personen in welchem tatsächlichen Zeitraum benötigt werden. Jeder Bedarf verlangt genau einen Diensttyp.

Beispiel: Werden vier Personen im Frühdienst von 06:30 bis 13:30 Uhr gebraucht, erkennt die App daraus automatisch einen Bedarf von 28 Arbeitsstunden. Wird die tatsächliche Zeit für ein einzelnes Datum auf 06:30 bis 12:30 Uhr verkürzt, berechnet die App für dieses Datum 24 Stunden. Die Service-Leitung muss die Summe nicht zusätzlich eintragen.

Standardbedarfe können dauerhaft für zukünftige Wochen geändert werden. Zusätzlich sind Ausnahmen nur für ein ausgewähltes Datum möglich. Frühere und abgenommene Pläne behalten ihre damaligen Zeiten.

Die automatische Planung darf nicht mehr Personen als benötigt einplanen.

Bestätigte Startwerte sind: Cafeteria Montag bis Freitag eine Person von 13:30 bis 20:30 Uhr, am Wochenende zusätzlich eine zweite Person von 13:30 bis 17:30 Uhr; Restaurant täglich vier Frühdienste von 06:30 bis 13:30 Uhr und vier Spätdienste von 16:30 bis 19:30 Uhr.

## Regeln für den Wochenplan

Es wird zwei Arten von Regeln geben:

1. **Zwingende Regeln:** Diese dürfen nicht verletzt werden.
2. **Wünsche:** Diese sollen möglichst erfüllt werden. Sie erhalten die Priorität hoch, mittel oder niedrig.

Kann ein Bedarf wegen einer zwingenden Regel nicht oder nur teilweise gedeckt werden, erstellt die App trotzdem den übrigen Plan. Der vollständig oder teilweise ungedeckte Zeitraum wird deutlich angezeigt. Die App erklärt außerdem den Grund und nennt mögliche Lösungen.

Wünsche dürfen nur dann unerfüllt bleiben, wenn keine bessere erlaubte Lösung gefunden wird. Die Service-Leitung kann bei einer manuellen Änderung bewusst von einem Wunsch abweichen. Die App zeigt diese Abweichung sichtbar an.

Die konkreten Regeln werden später gemeinsam einzeln aufgeschrieben und geprüft.

## Erstellen und Bearbeiten eines Plans

- Die Service-Leitung wählt aus, welche Mitarbeitenden im Planungszeitraum verfügbar sind.
- Sie bestimmt, für wie viele Wochen der Plan erstellt wird.
- Die App erzeugt einen vollständigen Vorschlag, soweit dies mit den zwingenden Regeln möglich ist.
- Nicht besetzbare Dienste und andere Probleme werden verständlich erklärt.
- Einzelne bereits passende Einteilungen können gesperrt werden.
- Bei einer neuen automatischen Erstellung dürfen alle nicht gesperrten Einteilungen neu verteilt werden.

Für Änderungen gibt es einen eigenen Bearbeitungsmodus. Nach dem Speichern berechnet die App Stunden, Bedarfsdeckung und Meldungen neu. Sie erzeugt dabei nicht ungefragt einen komplett neuen Plan.

Die Service-Leitung darf eine Person manuell auch außerhalb ihrer normalen Freigaben eintragen. Die App zeigt dabei eine klare Warnung.

## Abnahme und frühere Versionen

Ein geprüfter Plan wird bewusst abgenommen. Erst danach kann er als Excel-Datei ausgegeben werden.

Bei jeder Abnahme speichert die App eine unveränderliche Momentaufnahme. Wird ein bereits abgenommener Plan später geändert, bleibt die alte Fassung erhalten. Nach der erneuten Abnahme entsteht eine neue Fassung.

Frühere Pläne werden zunächst archiviert und nicht automatisch gelöscht. Sie können weiterhin angesehen und bei notwendigen Änderungen bearbeitet werden.

## Excel-Ausgabe

Die Service-Leitung stellt später eine Excel-Vorlage bereit. Die App muss deren Aufbau und Aussehen genau einhalten.

In der vorgesehenen Ausgabe werden drei Wochen auf einer Seite dargestellt. Mitarbeitende stehen in den Zeilen und Tage in den Spalten. Zusätzlich soll die Besetzung aus Sicht eines Einsatzortes angezeigt werden können.

Ein PDF-Export und eine zusätzliche Druckansicht sind zunächst nicht geplant.

## Datensicherung

Die App soll alle Daten regelmäßig lokal sichern. Außerdem wird es eine einfache Funktion zum Sichern und Wiederherstellen geben.

Ein eigenes App-Kennwort oder eine zusätzliche Verschlüsselung ist zunächst nicht vorgesehen. Der Schutz erfolgt über das Windows-Benutzerkonto.

## Über- und Minusstunden

Über- und Minusstunden sollen später aus den geplanten Diensten berechnet werden. Manuelle Korrekturen bleiben möglich und können freiwillig mit einem Grund versehen werden.

Dieses Zeitkonto ist vorgesehen, hat für die erste funktionsfähige Version aber keine hohe Priorität.
