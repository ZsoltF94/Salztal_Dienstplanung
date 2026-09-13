# Arbeitswochenplaner Servicebereich

## Zweck dieses Dokuments

Dieses Dokument hält das gemeinsam bestätigte Grundverständnis, bereits getroffene Entscheidungen und noch offene Grundsatzfragen fest. Es ist noch keine technische Spezifikation und enthält bewusst noch nicht die später zu erfassenden Detailregeln für Mitarbeitertypen, Dienste und Einsatzorte.

Status: Abgenommen am 2026-09-13

## Ziel der App

Entwickelt werden soll eine klassische Windows-Anwendung für die Service-Leitung einer Rehaklinik. Die Anwendung unterstützt die Planung der Mitarbeitenden im gastronomischen Servicebereich und erzeugt für einen wählbaren Zeitraum automatisch einen möglichst guten Wochenplan.

Der erzeugte Plan muss zwingende Regeln einhalten und priorisierte Wünsche möglichst gut erfüllen. Falls keine vollständig passende Besetzung möglich ist, soll die Anwendung einen bestmöglichen Plan erstellen und jeden Konflikt konkret und verständlich erklären.

Nach manueller Prüfung wird ein Plan abgenommen. Erst danach kann er in eine noch bereitzustellende Excel-Vorlage exportiert werden. Darstellung, Aufteilung und Format dieser Vorlage müssen exakt eingehalten werden.

## Bestätigte Grundlagen

### Nutzung und Plattform

- Die App wird zunächst von genau einer Person verwendet: der Service-Leitung.
- Es werden zunächst keine Anmeldung, Benutzerkonten oder Rollen benötigt.
- Die App soll vollständig offline funktionieren.
- Sämtliche Daten werden lokal gespeichert.
- Geplant wird nur für eine Rehaklinik beziehungsweise einen Standort.
- Ziel ist eine klassische Windows-App.
- Wenn technisch sinnvoll, soll sie ohne Installation gestartet werden können. Eine Installation ist akzeptabel, falls eine portable Auslieferung nicht zuverlässig umsetzbar ist.
- Während der Entwicklung darf eine lokale Browserdarstellung zum Testen verwendet werden. Das endgültige Produkt soll jedoch eine Windows-App sein.

### Planungszeitraum

- Eine Planungswoche läuft von Montag bis Sonntag.
- Der übliche Planungsvorlauf beträgt drei Wochen.
- Die Service-Leitung kann einstellen, für wie viele Wochen im Voraus ein Plan erzeugt wird.
- Die Anzahl vollständig gespeicherter zurückliegender Wochen soll konfigurierbar sein.
- Fehlende historische Wochen dürfen die Generierung eines neuen Plans nicht verhindern.

### Mitarbeitende und Verfügbarkeit

- Mitarbeitende werden mit ihrem Namen erfasst.
- Vor einer Generierung kann festgelegt werden, welche Mitarbeitenden im gewählten Zeitraum zur Verfügung stehen.
- Urlaub, Krankheit, Fortbildung, Wunschfrei und eingeschränkte Verfügbarkeiten sollen berücksichtigt werden können.
- Arbeitszeitmodell, Qualifikationen und zulässige Einsatzorte sollen fachlich getrennt modelliert werden. Dadurch ist beispielsweise eine 35-Stunden-Woche nicht untrennbar mit der Einsatzfreigabe für die Cafeteria verbunden.

### Dienste und Einsatzorte

- Es wird verschiedene Diensttypen mit festen Zeiten geben.
- Doppeldienste müssen unterstützt werden.
- Bekannte Einsatzorte sind zunächst Cafeteria und Restaurant.
- Weitere Einsatzorte können später ergänzt werden.
- Die konkreten Dienst-, Einsatzort- und Mitarbeitertypregeln werden in einer späteren eigenen Phase gemeinsam definiert.

### Bedarf

- Die Regeln bestimmen den Personalbedarf je Einsatzort, Tag und Dienst.
- Zusätzlich zum Personalbedarf muss ein Stundenbedarf berücksichtigt werden.
- Personalbedarf und Stundenbedarf werden getrennt betrachtet, damit sowohl die notwendige Anzahl anwesender Personen als auch die erforderliche Arbeitszeit abgedeckt werden kann.

### Regeln und Optimierung

- Es gibt zwingende Regeln, die nicht verletzt werden dürfen.
- Daneben gibt es Wünsche beziehungsweise weiche Regeln, die möglichst erfüllt werden sollen.
- Weiche Regeln sollen priorisiert werden können.
- Beispiele für weiche Ziele sind zwei freie Tage in Folge oder ein freies Wochenende im Monat; ihre endgültige Einordnung wird erst mit dem vollständigen Regelwerk festgelegt.
- Wenn nicht alle Anforderungen erfüllbar sind, soll die App einen bestmöglichen Plan erzeugen.
- Nicht erfüllte Anforderungen müssen in einer präzisen Konfliktliste erklärt werden.
- Die Service-Leitung kann den erzeugten Vorschlag manuell bearbeiten.
- Einzelne Dienste oder Zuweisungen können gesperrt werden, damit eine erneute Generierung sie unverändert lässt.

### Historie, Änderungen und Zeitkonten

- Frühere Pläne werden lokal gespeichert.
- Frühere beziehungsweise bereits laufende Pläne können nachträglich bearbeitet werden, wenn sich während einer Woche etwas ändert.
- Eine interne Datenbank verwaltet Überstunden, Minusstunden und gegebenenfalls weitere Zeitkonten.
- Die Werte dieser Zeitkonten können manuell korrigiert werden.
- Zeitkonten sind vorgesehen, haben für die erste funktionsfähige Version aber keine hohe Priorität.
- Bereits abgenommene Pläne dürfen geändert werden.
- Frühere Versionen sollen nachvollziehbar erhalten bleiben.

### Abnahme und Export

- Ein Plan wird über einen bewussten Abnahmeschritt verbindlich festgeschrieben.
- Der Excel-Export ist erst nach der Abnahme möglich.
- Ein Export umfasst drei Wochen auf einer Seite.
- Die konkrete Excel-Vorlage wird später bereitgestellt.
- Der Export muss die Vorlage in Aufbau und Format exakt einhalten.
- PDF-Export und separate Druckansicht gehören zunächst nicht zum Umfang.

## Vorläufiges fachliches Modell

Die folgenden Bereiche sollen getrennt bleiben, damit spätere Änderungen ohne grundlegenden Umbau möglich sind:

1. Mitarbeiter
2. Arbeitszeitmodell beziehungsweise Vertragsstunden
3. Qualifikationen und zulässige Einsatzorte
4. Verfügbarkeiten und Abwesenheiten
5. Diensttypen und Doppeldienste
6. Einsatzorte
7. Personalbedarf
8. Stundenbedarf
9. Zwingende Regeln
10. Priorisierte weiche Regeln
11. Generierter und manuell bearbeitbarer Wochenplan
12. Konflikte und Begründungen
13. Planversionen und Abnahme
14. Überstunden- und Minusstundenkonto
15. Excel-Export

## Geklärte Grundsatzfragen

### A. Personalbedarf und Stundenbedarf

- Der Bedarf wird je Einsatzort, Wochentag und Schicht festgelegt.
- Ein Bedarf beschreibt sowohl die Zahl benötigter Mitarbeitender als auch die durch den festen Diensttyp vorgegebene Arbeitsdauer.
- Beispiel Cafeteria: Montag bis Freitag wird jeweils eine Person für sieben Stunden benötigt; Samstag und Sonntag jeweils eine Person für vier Stunden.
- Beispiel Restaurant: Montag bis Freitag könnten vier Personen in einer siebenstündigen Frühschicht und vier Personen in einer vierstündigen Spätschicht benötigt werden. Diese Werte dienen nur der Veranschaulichung und sind noch keine echten Regeln.
- Die automatische Planung darf ausschließlich vorher definierte Diensttypen verwenden.
- Sie darf keine Überbesetzung erzeugen und den vorgegebenen Bedarf nicht überschreiten.

### B. Dienste und Doppeldienste

- Ein Doppeldienst besteht aus zwei getrennten, bereits definierten Diensten am selben Tag.
- Die Unterbrechung zwischen den beiden Dienstteilen zählt als unbezahlte Freizeit.
- Beide Teile müssen am selben Einsatzort stattfinden.
- Doppeldienste sind nach aktuellem Stand ausschließlich im Restaurant zulässig.

### C. Zwingende Regeln, Wünsche und Konflikte

- Eine zwingende Regel ist unverletzbar.
- Falls dadurch ein Dienst nicht besetzt werden kann, wird der übrige Plan trotzdem erzeugt.
- Jeder unbesetzte Dienst wird konkret gemeldet.
- Eine Konfliktmeldung beschreibt das Problem, dessen Ursache und hilfreiche Lösungsmöglichkeiten.
- Weiche Regeln werden mit den drei Stufen hoch, mittel und niedrig priorisiert.
- Eine weiche Regel darf bei der automatischen Planung nur verletzt werden, wenn keine bessere zulässige Lösung gefunden wird.
- Die Service-Leitung darf bei einer manuellen Änderung eine weiche Regel bewusst übergehen. Die App kennzeichnet die entstehende Abweichung sichtbar.

### D. Planung und manuelle Bearbeitung

- Der vorgesehene Ablauf lautet: „Manuell bearbeiten“ wählen, Plan bearbeiten und anschließend speichern.
- Nach dem Speichern werden Arbeitsstunden, Bedarfsdeckung, Regeln und Konfliktmeldungen neu berechnet.
- Eine manuelle Speicherung löst keine automatische Neugenerierung des Plans aus.
- Zunächst können einzelne Dienste beziehungsweise Zuweisungen gesperrt werden. Sperren für ganze Tage, Personen oder Wochen gehören vorerst nicht zum Umfang.
- Bei einer ausdrücklich gestarteten Neugenerierung dürfen alle nicht gesperrten Zuweisungen neu verteilt werden.
- Die Service-Leitung darf Mitarbeitende manuell auch außerhalb ihrer normalen Einsatzfreigaben eintragen. Die App soll die Regelabweichung sichtbar kennzeichnen.

### E. Planstunden und Zeitkonten

- Über- und Minusstunden werden zunächst aus den geplanten Diensten berechnet.
- Die berechneten Werte können manuell korrigiert werden.
- Änderungen an einem laufenden Plan aktualisieren das Zeitkonto automatisch.
- Bei einer manuellen Korrektur kann optional ein Grund angegeben werden.
- Zeitkonten werden dauerhaft weitergeführt, auch wenn ältere vollständige Pläne nur noch archiviert vorliegen.
- Zeitkonten sind gegenüber der eigentlichen Dienstplanung nachrangig und werden in einer späteren Ausbaustufe umgesetzt.

### F. Historie und Abnahme

- Ein nachträglich geänderter, zuvor abgenommener Plan muss erneut abgenommen werden, bevor er wieder exportiert werden kann.
- Ältere Pläne werden zunächst archiviert und nicht automatisch gelöscht.

### G. Datensicherung

- Die App erhält eine Funktion zum Sichern und Wiederherstellen aller lokalen Daten.
- Lokale Sicherungen sollen automatisch in regelmäßigen Abständen angelegt werden.
- Die App benötigt zunächst keinen eigenen Kennwort- oder Verschlüsselungsschutz. Der Zugriffsschutz des Windows-Benutzerkontos genügt.

### H. Bedienung und Darstellung

- Die gesamte App wird auf Deutsch angeboten.
- Die wichtigste Planansicht orientiert sich an der späteren Excel-Vorlage.
- Grundsätzlich zeigt diese Tabelle Mitarbeitende als Zeilen und Tage als Spalten.
- Zusätzlich soll es eine Ansicht aus Sicht des Einsatzortes geben, um dessen Besetzung direkt prüfen zu können.
- Konflikte und Warnungen werden im Plan farblich markiert und zusätzlich in einer separaten Liste zusammengefasst.

### I. Technischer Rahmen

- Unterstützt wird ausschließlich Windows 11. Windows 10 gehört nach der Entscheidung vom 2026-09-13 nicht mehr zum Zielumfang.
- Eine portable Anwendung als entpackbarer Ordner mit einer Startdatei ist akzeptabel.
- Die App darf Datenbank und Sicherungen im normalen Windows-Benutzerordner ablegen.

## Abschließend bestätigte Punkte

### Verhältnis von Schichtbedarf und Stundenbedarf

Die Service-Leitung trägt beispielsweise „vier Personen in der Frühschicht“ ein. Weil die Frühschicht fest sieben Stunden dauert, berechnet die App daraus automatisch 28 benötigte Mitarbeiterstunden. Es wird kein zusätzlicher, unabhängiger Wert „28 Stunden“ eingetragen. Die automatische Planung darf auch keinen neuen Dienst mit abweichenden Zeiten erfinden, um eine Stundenlücke zu schließen.

Diese Auslegung ist bestätigt.

### Planversionen

Mit einer Planversion ist eine unveränderliche Momentaufnahme des gesamten Plans zu einem bestimmten Zeitpunkt gemeint. So wäre später nachvollziehbar, wie der ursprünglich abgenommene Plan aussah und was nach einer spontanen Änderung anders ist. Eine ältere Version könnte bei Bedarf angesehen oder wiederhergestellt werden.

Eine Version wird automatisch bei jeder Abnahme gespeichert. Wird ein bereits abgenommener Plan bearbeitet, bleibt die alte Version erhalten; nach der erneuten Abnahme wird eine neue Version gespeichert. Während jedes einzelnen Bearbeitungsschritts entstehen dagegen keine unnötigen Zwischenversionen.

Diese Versionsverwaltung ist bestätigt.

## Vereinbarter Entwicklungsablauf

1. Zuerst werden Grundverständnis und Grundsatzfragen vollständig geklärt.
2. Danach wird der Projektordner mit den gemeinsam abgestimmten Leitdokumenten vorbereitet.
3. Vorgesehene Leitdokumente sind `AGENTS.md`, `CLEANCODE.md`, `ARCHITECTURE.md`, `MASTER_ROADMAP.md` und `STATUS.md`.
4. Die Master-Roadmap dient nur der übergeordneten Orientierung.
5. Jedes konkrete System erhält vor seiner Umsetzung eine eigene kleinschrittige Teil-Roadmap.
6. Es wird immer nur der nächste freigegebene minimale Schritt umgesetzt.
7. Nach jedem Schritt folgt ein kurzer Bericht mit:
   - dem tatsächlich umgesetzten Inhalt,
   - der durchgeführten Prüfung,
   - offenen Punkten oder Risiken,
   - dem nächsten minimalen Schritt.
8. Vor dem nächsten Schritt wird auf die Abnahme der Service-Leitung beziehungsweise des Auftraggebers gewartet.

## Bewusst noch nicht festgelegt

- konkrete Mitarbeitertypen und Vertragsmodelle,
- konkrete Diensttypen und Dienstzeiten,
- genaue Definition der Doppeldienste,
- vollständige Regeln je Einsatzort,
- gesetzliche und betriebliche zwingende Regeln,
- Prioritäten der weichen Regeln,
- genaue Personal- und Stundenbedarfe,
- verwendete Planungs- beziehungsweise Optimierungsmethode,
- endgültige technische Architektur,
- Aufbau der Excel-Vorlage.
