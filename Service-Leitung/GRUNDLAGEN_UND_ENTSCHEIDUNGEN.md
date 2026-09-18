# Grundlagen und Entscheidungen

## Wofür ist die App gedacht?

Die App soll die Service-Leitung beim Erstellen von Arbeitsplänen unterstützen. Sie ist für den gastronomischen Servicebereich einer Rehaklinik vorgesehen.

Die Service-Leitung trägt die Mitarbeitenden und ihre Einsatzmöglichkeiten ein. Danach kann die App für genau drei vollständige Wochen einen möglichst guten Dienstplan vorschlagen. Ein neuer Planungszeitraum darf sich nicht mit einem bereits gespeicherten Zeitraum überschneiden.

## Wo läuft die App?

- Die App soll auf Windows 11 funktionieren.
- Sie soll vollständig ohne Internet nutzbar sein.
- Alle Arbeitsdaten werden lokal auf dem Windows-PC gespeichert.
- Es wird zunächst nur ein Benutzer benötigt. Eine Anmeldung ist nicht vorgesehen.
- Wenn möglich, soll die App ohne Installation aus einem entpackten Ordner gestartet werden können.
- Die Bedienung der App wird auf Deutsch sein.

## Mitarbeitende und Mitarbeitertypen

Für jeden Mitarbeiter werden Vorname und Nachname getrennt eingetragen. Zusätzlich wird genau einer der gemeinsam festgelegten Mitarbeitertypen ausgewählt. Der Typ bestimmt:

- die vereinbarten Wochenstunden,
- mögliche Einsatzorte,
- regulär erlaubte Dienste,
- besondere Dienste, die nur innerhalb einer Kombination erlaubt sind,
- Dienste, die nur als manuelle Notlösung vorgeschlagen werden dürfen.

Urlaub und andere persönliche Planungsangaben bleiben davon getrennt. Dazu gehören später:

- Urlaub `U`,
- Krankheit `K`,
- ein rotes `X` für einen verbindlich freien Tag.

Fortbildungen, ein eigenes Wunschfrei und stundenweise Einschränkungen werden für die erste Fassung nicht benötigt. Ein gewünschter verbindlicher freier Tag wird als rotes `X` eingetragen.

Für die Abwesenheitserfassung wurde die Liste um `Typ20`, `Typ20a` und `Typ25a` ergänzt. Die App besitzt elf Starttypen. Die Zahl steht grundsätzlich für die Wochenstunden; `Typ1` besitzt 40 Stunden und AH besitzt 10 Stunden. Typen ohne `a` dürfen Frühdienst, Spätdienst und den Restaurant-Doppeldienst übernehmen. Typen mit `a` dürfen zusätzlich in der Cafeteria und als Springer eingesetzt werden.

Ein Mitarbeitertyp wird gemeinsam verwendet. Ändert die Service-Leitung später einen Typ, gilt die Änderung für alle ihm zugeordneten Mitarbeitenden. Für eine einzelne Person wird stattdessen bewusst ein anderer Typ ausgewählt. Bereits abgenommene Pläne bleiben unverändert.

Für die erste Fassung werden keine zusätzlichen Qualifikationen benötigt. Im eigenen Mitarbeitertypen-Tab können neue normale Typen angelegt und vorhandene Typen bearbeitet werden. Dort werden Name, Wochenstunden, der Tageswert für `U` und `K`, normale Dienste sowie die Berechtigungen für Doppeldienst und Springer gepflegt. Der Typcode bleibt nach dem Anlegen unverändert. Die besondere Planungsart von Typ1 und AH bleibt geschützt.

Ein noch nie verwendeter normaler Typ darf nach einer Sicherheitsabfrage gelöscht werden. Sobald eine Person oder eine andere Fachinformation den Typ verwendet, bleibt er erhalten.

Bei `U` und `K` wird das Wochen-Soll je Tag um den am Typ eingestellten Wert reduziert: vier Stunden bei Typ20, fünf bei Typ25, sechs bei Typ30, sieben bei Typ35 und acht bei Typ1. Das gilt auch am Wochenende und an Feiertagen. Für AH werden `U` und `K` zunächst nicht angeboten; eine krankheitsbedingte Sperre trägt die Service-Leitung als rotes `X` ein. Rote `X` reduzieren das Wochen-Soll nicht.

Deaktivierte Mitarbeitende können mit derselben Kennung, demselben Namen und demselben Typ wieder aktiviert werden. Dabei darf weiterhin höchstens eine aktive Person vom Typ1 vorhanden sein.

Endgültig gelöscht werden dürfen nur deaktivierte Mitarbeitende, die noch in keinem Plan und keinen anderen Fachdaten verwendet wurden. Vorher zeigt die App den vollständigen Namen und warnt, dass das Löschen nicht rückgängig gemacht werden kann. Bereits verwendete Personen bleiben zum Schutz der Historie deaktiviert erhalten.

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

Standardbedarfe können ab einer bewusst ausgewählten Planungswoche dauerhaft geändert werden. Die Änderung gilt ab dem Montag dieser Woche; frühere Wochen bleiben unverändert. Ein Standardbedarf darf auch für denselben Wirksamkeitsmontag mehrfach korrigiert werden. Die jeweils letzte Korrektur gilt, während ältere Fassungen intern nachvollziehbar bleiben.

Unter „Einsatzorte und Dienste“ wird der zu bearbeitende Einsatzort für „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ einheitlich links gewählt. Beim regelmäßigen Bedarf zeigt die rechte Seite anschließend die vollständige Woche von Montag bis Sonntag für diesen Einsatzort.

Zusätzlich sind Ausnahmen nur für ein ausgewähltes Datum möglich. Eine solche Ausnahme kann Personenzahl und Zeit ändern, einen Bedarf für diesen Tag aufheben oder einen sonst fehlenden Bedarf ergänzen. Wird die Ausnahme entfernt, gilt wieder der aktuelle Standard.

Feiertage und andere besondere Tage werden in der ersten Fassung bewusst als Datumsausnahme eingetragen. Die App erkennt Feiertage zunächst nicht automatisch.

Für denselben Einsatzort, dasselbe Datum und denselben Diensttyp genügt zunächst ein zusammenhängender Zeitraum mit gleichbleibender Personenzahl. Verschiedene Diensttypen dürfen gleichzeitig gebraucht werden.

Die automatische Planung darf nicht mehr Personen als benötigt einplanen.

Die für die erste Fassung vollständigen Startwerte sind: Cafeteria Montag bis Freitag eine Person von 13:30 bis 20:30 Uhr; Samstag und Sonntag eine Person von 13:30 bis 20:30 Uhr und zusätzlich eine zweite Person von 13:30 bis 17:30 Uhr; Restaurant täglich vier Frühdienste von 06:30 bis 13:30 Uhr und vier Spätdienste von 16:30 bis 19:30 Uhr.

## Regeln für den Wochenplan

Für den automatisch erzeugten Plan und die spätere manuelle Bearbeitung werden drei Wirkungen unterschieden:

1. **Zwingend für die Automatik:** Die automatische Planung darf diese Regeln nicht verletzen.
2. **Manuell übersteuerbare Planungsregeln:** Die Service-Leitung darf bewusst abweichen. Die App zeigt vorher eine Warnung und verlangt eine Bestätigung.
3. **Nicht übersteuerbare Strukturregeln:** Widersprüchliche Pläne bleiben ausgeschlossen, zum Beispiel zeitlich überlappende Dienste oder ein Dienst auf einem Tag mit `U`, `K` oder rotem `X`.

Kann ein Bedarf wegen einer zwingenden automatischen Regel nicht gedeckt werden, erstellt die App trotzdem den übrigen Plan. Ein normaler Dienst wird vollständig besetzt oder als ungedeckt angezeigt. Nur beim bestätigten Springer-Einsatz darf ein Teil des Restaurant-Spätdienstes besetzt sein; der frühere offene Zeitraum bleibt sichtbar. Die App erklärt außerdem den Grund und nennt mögliche Lösungen.

Nach jeder erfolgreichen Plangenerierung soll eine vollständige Übersicht alle Bedarfe der drei Wochen aufführen – nicht nur die offenen. Für jeden Bedarf zeigt sie Datum, Einsatzort, Dienst, tatsächliche Bedarfszeit, benötigte Personen beziehungsweise Minuten und den gedeckten sowie offenen Anteil. Die Zustände „vollständig gedeckt“, „teilweise gedeckt“ und „ungedeckt“ werden klar unterschieden und zusätzlich zusammengezählt. Teildeckung bleibt ausschließlich beim bestätigten Springer-Sonderfall möglich. Für teilweise oder vollständig ungedeckte Bedarfe ergänzt die spätere Konflikterklärung Ursachen und mögliche Lösungen.

Nach dem ersten technisch erfolgreichen Generierungsstand bleibt die Berechnung zunächst unverändert. S09A hat nur die Dienstplanoberfläche übersichtlicher angeordnet und ist abgeschlossen. Der S09B-Fragenkatalog, die Roadmap und QB-01 bis QB-05 sind ausdrücklich abgenommen. QB-06 stellt die Berichte in einem eigenen, weiterhin neben dem Dienstplan bedienbaren Fenster dar. Die sichtbare Prüfung hat gezeigt, dass der gespeicherte Bericht eine tatsächlich durchgeführte Eingangsprüfung verliert und bei einer Zeitgrenze noch nicht verständlich erklärt, warum kein Optimalitätsbeweis vorliegt und spätere Schritte nicht begonnen wurden. QB-06A ist für diese Korrektur in fünf kleinen Teilen freigegeben. QB-06A.1 bis QB-06A.4 sind abgenommen: Die eindeutigen technischen Angaben unterscheiden vollständige und ältere unvollständige Aufzeichnungen sowie Zeitgrenze, Abbruch und technischen Fehler, werden verlustfrei mit dem aktuellen automatischen Lauf gespeichert und als verständliche Aussagen vorbereitet. QB-06A.5 zeigt nun, ob ein Teilziel optimal bewiesen wurde, ob eine Zeitgrenze einen zulässigen Zwischenstand beendet hat und warum Folgephasen nicht begonnen wurden. Vorhandene ältere Laufdetails werden nicht erfunden; fehlende Angaben werden ehrlich als nicht gespeichert gekennzeichnet. Eine zusätzliche Historie oder eine neue Datenbankstruktur war dafür nicht nötig. Die Angaben erklären weiterhin keine einzelne Personenauswahl und verändern weder Berechnung noch Zeitgrenze. Die sichtbare Abnahme steht noch aus. Erst nach dem fertigen Bericht wird ein eigener Optimierungsplan erstellt. Die große Abschlussprüfung des Generators erfolgt erst für die danach endgültig vorgesehene Berechnung.

Der S09B-Qualitätsbericht ist noch nicht die spätere vollständige Konflikterklärung aus System 10. Ursachen je ausgeschlossener Person und konkrete Lösungsvorschläge bleiben weiterhin diesem späteren System vorbehalten.

Wünsche erhalten die Priorität hoch, mittel oder niedrig und dürfen nur dann unerfüllt bleiben, wenn keine bessere erlaubte Lösung gefunden wird. Eine bewusst bestätigte manuelle Abweichung bleibt sichtbar und ändert die Regel für zukünftige Pläne nicht.

Der konkrete Regelkatalog und seine Prioritätsmatrix wurden am 16. September 2026 vollständig bestätigt. Die erste Fassung prüft nur die internen Regeln der Service-Leitung und behauptet keine vollständige gesetzliche oder tarifliche Prüfung.

## Erstellen und Bearbeiten eines Plans

- Alle aktiven Mitarbeitenden gelten an einem leeren Tag grundsätzlich als verfügbar. `U`, `K` und ein rotes `X` tragen die Abweichungen für konkrete Kalendertage ein; eine zusätzliche Personenauswahl vor jedem Lauf gibt es nicht.
- Der Plan wird in der ersten Fassung immer für genau drei vollständige Wochen ab einem ausgewählten Montag erstellt.
- Die App erzeugt einen vollständigen Vorschlag, soweit dies mit den zwingenden Regeln der Automatik möglich ist.
- Nicht besetzbare Dienste und andere Probleme werden verständlich erklärt.
- Einzelne bereits passende Einteilungen können gesperrt werden.
- Bei einer neuen automatischen Erstellung dürfen alle nicht gesperrten Einteilungen neu verteilt werden.
- Für den neuen Zielstand betrachtet die App normale Nicht-AH- und AH-Personen gemeinsam. Bereits vorläufig verteilte normale Dienste dürfen nochmals getauscht werden, damit der Gesamtplan besser wird.
- Zuerst bleiben Bedarf, zwingende Regeln und die hohen Schutzwünsche maßgeblich. Danach werden Springer und Doppeldienste möglichst vermieden. Sie werden nicht allein zum Auffüllen von Stunden erzeugt.
- Jede AH-Person soll bei passenden offenen Diensten möglichst mindestens drei Stunden je Woche erhalten. Dafür darf eine normale Person unter ihrem exakten Wochensoll bleiben, aber nicht unter die bisherige Untergrenze von Wochensoll minus drei Stunden gedrängt werden.
- Anschließend versucht die App, alle Mitarbeitenden relativ fair an ihre eigenen Wochenstunden heranzuführen. Für AH bleiben zehn Stunden das Wochenziel und zwölf Stunden die automatische Obergrenze. Unter sechs und über zehn Stunden werden weiterhin gemeldet.
- Ein manuell eingetragener Typ1-Früh- oder Spätdienst kann während der Bearbeitung mit `B` als Bürozeit markiert werden. Die Stunden zählen für Typ1, der Dienst deckt aber keinen benötigten Mitarbeiterplatz. Nach der Abnahme bleibt der zugrunde liegende Dienst sichtbar und nur das `B` verschwindet aus Plan und späterer Excel-Ausgabe.

Für Änderungen gibt es einen eigenen Bearbeitungsmodus. Nach dem Speichern berechnet die App Stunden, Bedarfsdeckung und Meldungen neu. Sie erzeugt dabei nicht ungefragt einen komplett neuen Plan.

Die Service-Leitung darf eine Person manuell auch außerhalb ihrer normalen Freigaben oder zusätzlich auf einen bereits vollständig besetzten Dienst eintragen. Die App zeigt dabei eine klare Warnung, verlangt eine Bestätigung und verändert den gespeicherten Bedarf nicht.

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
