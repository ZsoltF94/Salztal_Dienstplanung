# Grundlagen und Entscheidungen

## Wofür ist die App gedacht?

Die App soll die Service-Leitung beim Erstellen von Arbeitsplänen unterstützen. Sie ist für den gastronomischen Servicebereich einer Rehaklinik vorgesehen.

Die Service-Leitung trägt die Mitarbeitenden und ihre Einsatzmöglichkeiten ein. Danach kann die App für eine gewünschte Anzahl von Wochen einen möglichst guten Dienstplan vorschlagen. Normalerweise werden drei Wochen im Voraus geplant.

## Wo läuft die App?

- Die App soll auf Windows 10 und Windows 11 funktionieren.
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

Wochenstunden und Einsatzmöglichkeiten werden getrennt gespeichert. Eine Person mit 35 Wochenstunden kann dadurch unabhängig davon für die Cafeteria, das Restaurant oder beide Bereiche freigegeben werden.

Die genauen Mitarbeitertypen und Regeln werden erst in einem späteren eigenen Schritt festgelegt.

## Einsatzorte und Dienste

Die zunächst bekannten Einsatzorte sind:

- Cafeteria,
- Restaurant.

Weitere Einsatzorte können später ergänzt werden.

Es wird feste Diensttypen mit festen Anfangs- und Endzeiten geben. Die App darf nur diese vorher festgelegten Dienste verwenden und keine eigenen Arbeitszeiten erfinden.

Ein Doppeldienst besteht aus zwei getrennten Diensten an einem Tag. Die Pause zwischen beiden Teilen ist unbezahlte Freizeit. Beide Teile finden am selben Einsatzort statt. Nach heutigem Stand sind Doppeldienste nur im Restaurant möglich.

## Personal- und Stundenbedarf

Für jeden Einsatzort wird je Wochentag und Schicht festgelegt, wie viele Personen benötigt werden.

Beispiel: Werden vier Personen für eine siebenstündige Frühschicht gebraucht, erkennt die App daraus automatisch einen Bedarf von 28 Arbeitsstunden. Die Service-Leitung muss diese 28 Stunden nicht noch einmal getrennt eintragen.

Die automatische Planung darf nicht mehr Personen als benötigt einplanen.

Die später festzulegenden Zahlen und Dienstzeiten sind in diesem Dokument noch keine echten Vorgaben.

## Regeln für den Wochenplan

Es wird zwei Arten von Regeln geben:

1. **Zwingende Regeln:** Diese dürfen nicht verletzt werden.
2. **Wünsche:** Diese sollen möglichst erfüllt werden. Sie erhalten die Priorität hoch, mittel oder niedrig.

Kann ein Dienst wegen einer zwingenden Regel nicht besetzt werden, erstellt die App trotzdem den übrigen Plan. Der unbesetzte Dienst wird deutlich angezeigt. Die App erklärt außerdem den Grund und nennt mögliche Lösungen.

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
