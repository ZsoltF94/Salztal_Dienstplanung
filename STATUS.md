# Projektstatus

Stand: 2026-09-14

Status dieses Dokuments: Aktuell – System 05 mit abgenommener Teil-Roadmap aktiv

## Aktueller Überblick

| Bereich | Aktueller Stand |
|---|---|
| Projektphase | Beginn der kleinschrittigen System-05-Umsetzung |
| Aktives System | System 05 – Personal-, Schicht- und Stundenbedarf |
| Aktive Teil-Roadmap | `docs/roadmaps/active/STAFFING_DEMAND_ROADMAP.md` |
| Aktueller Stand | BE-09B – wiederholte Standardkorrekturen und konsistente Wochenansicht implementiert und automatisch geprüft; Sichtprüfung offen |
| Zuletzt abgenommener Schritt | BE-08 – sichtbare WPF-Wochenübersicht |
| Funktionsfähige App | Einsatzorte, Diensttyp-Standardzeiten, Mitarbeitende und regelmäßige Wochenbedarfe können im jeweils bestätigten Umfang verwaltet werden; noch keine Dienstplanfunktion |
| Echte Mitarbeiter- oder Plandaten im Repository | Keine festgestellt; das zuvor vorhandene sensible Beispielbild ist nicht mehr im Arbeitsordner |

## Nachweislich fertig und abgenommen

- Das Grundverständnis und die bestätigten Produktentscheidungen sind dokumentiert.
- Das Projekt ist als Git-Repository eingerichtet und mit dem vorgesehenen GitHub-Repository verbunden.
- Der leicht verständliche Lesebereich für die Service-Leitung ist eingerichtet.
- Zielplattform, Technik und modulare Architektur sind verbindlich festgelegt.
- Clean-Code-, Namespace-, Test- und Qualitätsregeln sind festgelegt.
- Der kleinschrittige Arbeits-, Berichts- und Abnahmeprozess ist in `AGENTS.md` verbindlich geregelt.
- Die abgenommene `MASTER_ROADMAP.md` ordnet 16 geplante Systeme und ihre Abhängigkeiten.
- Die zentrale `STATUS.md` führt aktiven Schritt, offene Entscheidungen, Blockaden und ausstehende Gates zusammen.
- System 01 – Projektgrundlage ist vollständig geprüft, abgenommen und archiviert.
- Die Teil-Roadmap für System 02 ist abgenommen.
- System 03 – Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben – ist vollständig geprüft, abgenommen und archiviert.

## Aktuell

- System 05 – Personal-, Schicht- und Stundenbedarf – ist als nächstes Fachsystem ausgewählt.
- Die aktive Teil-Roadmap `docs/roadmaps/active/STAFFING_DEMAND_ROADMAP.md` gliedert System 05 nach den Bedienkorrekturen in BE-01 bis BE-10 einschließlich BE-09A und BE-09B. BE-02 bis BE-08 sind umgesetzt und abgenommen. BE-09 wurde nach der Sichtprüfung nicht abgenommen; BE-09A und der daraus entstandene Korrekturschritt BE-09B sind umgesetzt und automatisch geprüft. Ihre gemeinsame Sichtprüfung ist offen.
- Die vollständigen Startbedarfe, ab Montag wirksame Standardrevisionen, vollständige Datumsausnahmen, manuelle Feiertagsbehandlung und höchstens ein zusammenhängender Bedarfsblock je Einsatzort, Datum und Diensttyp sind fachlich bestätigt.
- Die geplanten Code-Anker halten Bedarfs-Domain, Application-Verträge, `Persistence/StaffingDemands` und `Features/StaffingDemands` getrennt. Die bestehende gemeinsame SQLite-Migrationsfolge bleibt verbindlich.
- BE-02 führt `StaffingDemandId`, `RequiredEmployeeCount`, `StaffingDemandTime` und `StaffingDemand` als unveränderliche Domain-Werte ein.
- Ein einzelner Bedarf besitzt Datum, stabile Einsatzort- und Diensttypkennungen, tatsächliche Anfangs- und Endzeit sowie eine positive ganzzahlige Personenzahl.
- Bedarfsdauer und Mitarbeiterbedarf werden ausschließlich daraus in ganzen Minuten berechnet. Die Gesamtberechnung verwendet `long` und bleibt auch beim technisch größten `int`-Personenwert überlaufsicher; ein unabhängiger Stundenwert kann nicht übergeben werden.
- Leere Kennungen, nicht positive Personenzahlen, Sekunden, nicht halbstündige Eingaben sowie leere oder über Mitternacht reichende Zeiträume liefern strukturierte Domain-Fehler.
- 18 neue BE-02-Tests bestehen. Insgesamt bestehen jetzt 113 Domain-Tests; der vollständige Solution-Build und alle 16 Architekturtests sind ebenfalls grün.
- BE-03 ergänzt einen eindeutigen Standardschlüssel aus Wochentag, Einsatzort und normalem Diensttyp sowie unveränderliche, ab einem Montag wirksame Revisionen.
- Ergänzen, Ersetzen und Aufheben sind getrennte Revisionsarten. BE-03 verhinderte zunächst doppelte Revisionen desselben Schlüssels am selben Wirksamkeitsmontag; BE-09B erweitert dieses Modell um fortlaufende unveränderliche Korrekturfassungen, von denen die höchste Folge gilt.
- Der initiale Bedarfskatalog enthält alle 23 bestätigten Wochentagswerte. Cafeteria-Dienst A und B bleiben am Wochenende getrennt; `D` und `Spr` kommen nicht als einzelne Bedarfsdiensttypen vor.
- Bei der späteren erstmaligen Anlage übernimmt der Katalog die dann aktuellen Standardzeiten der normalen Diensttypen, damit bereits bewusst geänderte Zeiten nicht zurückgesetzt werden.
- 15 neue BE-03-Tests bestehen. Insgesamt bestehen jetzt 128 Domain-Tests; der vollständige Solution-Build, alle 16 Architekturtests und alle 254 Tests sind grün.
- BE-03 mit Wochenvorlage, Wirksamkeitsprinzip und Startkatalog ist ausdrücklich abgenommen.
- BE-04 ergänzt vollständige Datumsausnahmen zum Ergänzen, Ersetzen und Aufheben eines Bedarfs. Pro Datum, Einsatzort und normalem Diensttyp ist höchstens eine Ausnahme zulässig.
- Die Domain löst eine ausgewählte Montag-bis-Sonntag-Woche deterministisch auf. Datumsausnahmen haben Vorrang vor dem wirksamen Standard und bleiben vollständige Momentaufnahmen ihrer tatsächlichen Zeit und Personenzahl.
- Wird eine vorhandene Ausnahme aus der Menge entfernt, gilt wieder die aktuell wirksame Standardrevision. Fehlt die zu entfernende Ausnahme bereits, meldet BE-06 idempotent Erfolg; eine inzwischen andere Ausnahme unter demselben Schlüssel bleibt ein sichtbarer Konflikt.
- Atomare Bedarfe enthalten ihre Quelle und berechneten Minuten. Daraus entstehen Tagessummen je Einsatzort, Wochensummen je Einsatzort und die Gesamtminuten der Woche.
- 16 neue BE-04-Tests bestehen. Insgesamt bestehen jetzt 144 Domain-Tests; der vollständige Solution-Build, alle 16 Architekturtests und alle 270 Tests sind grün.
- BE-04 mit Datumsausnahmen, Ausnahmevorrang, Rückkehr zum Wochenstandard und berechneter Wochenansicht ist ausdrücklich abgenommen.
- BE-05 ergänzt einen schmalen Lesevertrag, der Standardrevisionen, Datumsausnahmen und den Dienstkatalog als einen konsistenten unveränderlichen Lesestand liefert.
- Der Leseanwendungsfall löst eine ausdrücklich übergebene Montag-bis-Sonntag-Woche auf und gibt je Bedarf Quelle, Einsatzort, Diensttyp, tatsächliche Zeit, Personenzahl, Dauer und Mitarbeiterbedarf sowie bestätigte Tages-, Einsatzort- und Gesamtsummen zurück.
- Widersprüchliche Bestandsdaten, unbekannte Katalogkennungen und falsche Diensttyp-Einsatzort-Zuordnungen werden als strukturierte deutsche Fehler sichtbar zurückgegeben. Abbruch ist vor und während des Lesens geprüft.
- 16 neue BE-05-Application-Tests bestehen. Insgesamt bestehen jetzt 71 Application-Tests und alle 286 vorhandenen Tests; der vollständige Solution-Build bleibt bei 0 Warnungen und 0 Fehlern.
- BE-05 mit Lesevertrag, unveränderlicher Wochenmomentaufnahme und sichtbaren Fehlergrenzen ist ausdrücklich abgenommen.
- BE-06 ergänzt drei getrennte Schreibabläufe für Standardrevisionen, das Speichern einer vollständigen Datumsausnahme und das Entfernen einer vorhandenen Datumsausnahme.
- Vor einem tatsächlichen Schreiben werden aktueller Lesestand, Dienstkatalog, Diensttyp-Einsatzort-Zuordnung und erwartete aktuelle Momentaufnahme geprüft. Der atomare Speichervertrag erhält denselben aktuellen Stand und kann eine zwischenzeitliche Änderung als Konflikt ablehnen.
- Standard und Datumsausnahme unterstützen jeweils Ergänzen, Ersetzen und Aufheben. Fachlich ungültige Änderungen, nicht gefundene Referenzen, widersprüchliche Bestandsdaten, Konflikte und Abbruch werden deutsch und strukturiert gemeldet; abgelehnte Vorgänge schreiben nichts.
- 26 neue BE-06-Application-Tests bestehen. Insgesamt bestehen jetzt 97 Application-Tests und alle 312 vorhandenen Tests; der vollständige Solution-Build bleibt bei 0 Warnungen und 0 Fehlern.
- BE-06 mit den drei Schreibabläufen, ihren deutschen Ergebnissen und den atomaren Speichergrenzen ist ausdrücklich abgenommen.
- BE-07 ergänzt zwei Bedarfstabellen und den getrennten `SqliteStaffingDemandStore` in der bestehenden gemeinsamen SQLite-Datenbank. Standardrevisionen werden unveränderlich angehängt; Datumsausnahmen werden atomar gespeichert, ersetzt und entfernt.
- Die 23 Startbedarfe werden genau einmal anhand der bei der ersten Bedarfsinitialisierung aktuell gespeicherten Diensttyp-Standardzeiten angelegt. Bereits bewusst geänderte Zeiten werden nicht zurückgesetzt.
- Datenbankbedingungen schützen eindeutige Fachschlüssel, Katalogbeziehungen, Montag, Änderungsarten, halbstündige Tageszeiten und positive Personenzahlen. Ein erzwungener Schreibfehler bestätigt die vollständige Rücknahme der Transaktion.
- Upgrade-Tests von der ältesten System-04-Migration und vom aktuellen System-03-Stand erhalten synthetisch geänderte Katalogwerte sowie vorhandene synthetische Mitarbeiterdaten.
- 9 neue BE-07-Infrastructure-Tests bestehen. Insgesamt bestehen jetzt 32 Infrastructure-Tests, 16 Architekturtests und alle 321 vorhandenen Tests; der vollständige Solution-Build bleibt bei 0 Warnungen und 0 Fehlern.
- BE-07 mit gemeinsamer Migration, dynamischen Startwerten und atomarem SQLite-Adapter ist ausdrücklich abgenommen.
- BE-08 ergänzt den Reiter „Bedarf“ mit Kalenderauswahl, eindeutiger Montag-bis-Sonntag-Woche und Wechsel zur vorherigen oder nächsten Woche.
- Bedarfe werden nach Einsatzort und Tageszeile gruppiert. Diensttyp, tatsächliche Zeit, Personenzahl, berechnete Stunden und Quelle bleiben je atomarem Bedarf sichtbar; Datumsausnahmen sind textlich und gestalterisch vom Wochenstandard unterschieden.
- Tagessummen je Einsatzort, Wochensummen je Einsatzort und die Gesamtstundenzahl stammen aus derselben Application-Momentaufnahme. Für die Ausgangswoche zeigt die Ansicht 57, 280 und insgesamt 337 Stunden.
- Lade-, Leer-, Fehler-, Wiederholungs- und Abbruchzustände sind umgesetzt. Alle atomaren Werte bleiben im ViewModel für spätere abgestimmte Darstellungen und Bearbeitung erhalten.
- 8 neue BE-08-Desktop-Tests bestehen. Insgesamt bestehen jetzt 40 Desktop-Tests, 16 Architekturtests und alle 329 vorhandenen Tests; der vollständige Solution-Build bleibt bei 0 Warnungen und 0 Fehlern.
- Die sichtbare WPF-Wochenübersicht aus BE-08 ist vom Auftraggeber bestätigt und ausdrücklich abgenommen.
- BE-09 ergänzt die Bearbeitung regelmäßiger Wochenstandards mit sichtbar gewähltem Wirksamkeitsmontag, Einsatzort, Wochentag, normalem Diensttyp, tatsächlicher Zeit und Personenzahl.
- Fehlende Standards können ergänzt, bestehende ersetzt und ab der gewählten Woche aufgehoben werden. Die Oberfläche lädt den historischen Ausgangsstand, erhält abgelehnte Eingaben und lädt nach Erfolg Woche und Summen aus der gespeicherten Quelle neu.
- Die Auswahl ist auf normale Diensttypen des Einsatzortes begrenzt; `D` und `Spr` werden nicht angeboten. Eine historische Aufhebung bleibt als erwarteter aktueller Revisionsstand erhalten, damit auch späteres Ergänzen konfliktgeschützt erfolgt.
- 12 neue BE-09-Desktop-Tests bestehen. Insgesamt bestehen jetzt 52 Desktop-Tests, 16 Architekturtests und alle 341 vorhandenen Tests; der vollständige Solution-Build bleibt bei 0 Warnungen und 0 Fehlern.
- Die Sichtprüfung von BE-09 hat einen Änderungsbedarf ergeben. BE-09A hat den regelmäßigen Standardeditor aus dem Reiter „Bedarf“ entfernt und in die Unterteilung „Einsatzorte“, „Diensttypen“ und „Regelmäßiger Bedarf“ im Reiter „Einsatzorte und Dienste“ verschoben.
- Der Reiter „Bedarf“ bearbeitet nun nur konkrete Kalendertage über „Nur diesen Tag ändern“. Eine solche „Einmalige Änderung“ kann wiederholt bearbeitet, für „kein Bedarf“ verwendet oder auf den dann wirksamen regelmäßigen Standard zurückgesetzt werden.
- Diensttyp-Standardzeit und regelmäßiger Personalbedarf bleiben getrennte Speichervorgänge. Regelmäßige Bedarfsänderungen gelten weiterhin ausschließlich ab einem gewählten Montag und verändern frühere Wochen nicht.
- BE-09A ist umgesetzt und automatisch geprüft. 8 neue Desktop-Tests und 1 zusätzlicher Architekturtest erhöhen den Gesamtstand auf 350 erfolgreiche Tests; das sichtbare Abnahmegate bleibt offen.
- BE-09B erlaubt nun eine zweite und weitere Änderung desselben regelmäßigen Bedarfs am selben Wirksamkeitsmontag. Fortlaufende unveränderliche Korrekturfassungen bleiben erhalten; für den jeweiligen Montag gilt die zuletzt gespeicherte Fassung.
- Der regelmäßige Bedarfsbereich zeigt den links gewählten Einsatzort vollständig von Montag bis Sonntag. Die zusätzliche Einsatzortauswahl im rechten Editor ist entfallen; vorhandene und fehlende Bedarfe können aus der Woche zur Bearbeitung gewählt werden.
- Die neue verlustfreie Migration `20260914185047_AddStandardDemandCorrectionSequence` übernimmt bestehende Revisionen mit Folge `1` und sichert positive, je Bedarfsschlüssel und Montag eindeutige Korrekturfolgen.
- Die Mitarbeiterfragen und Folgefragen sind vollständig beantwortet. Die fachlichen Entscheidungen stehen in `docs/decisions/EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`.
- Jeder Mitarbeiter erhält genau einen gemeinsam referenzierten bindenden Mitarbeitertyp. Wochen-Soll und Einsatzfreigaben gehören zum Typ und werden nicht als unabhängige Mitarbeiterkopien gespeichert.
- Die acht Starttypen, ihre Wochen-Sollwerte, regulären Einsatzmöglichkeiten sowie die Typ1- und AH-Sonderfälle sind festgehalten.
- Für die erste Fassung wird kein zusätzlicher Qualifikationskatalog benötigt. System 03 heißt deshalb künftig „Mitarbeitende, Mitarbeitertypen und Einsatzfreigaben“.
- Die überarbeitete Teil-Roadmap enthält die bestätigten Code-Anker, eine eigene Absicherung der bestehenden SQLite-Migrationsfolge und eindeutige Übergaben an die späteren Planungs- und Konfliktsysteme.
- MA-01 bis MA-10A einschließlich MA-02A sind ausdrücklich abgenommen. MA-02 führt ausschließlich die stark typisierten, unveränderlichen Domain-Grundwerte für Mitarbeitertypkennung, Code, Name und Wochen-Soll ein.
- Ein Wochen-Soll wird minutengenau als ganze Zahl gespeichert, muss positiv sein und kann die vollständige Wochenlänge von 10.080 Minuten nicht überschreiten. Es werden weder `double` noch `float` verwendet.
- MA-02A ergänzt `.gitattributes` und gleicht die Repository-Textdateien einmalig an die bereits bestätigte CRLF-Regel aus `.editorconfig` an. Shell-Skripte bleiben ausdrücklich auf LF.
- Der zeilenendenunabhängige Inhaltsfingerabdruck von 169 geprüften Textdateien war vor und nach der Normalisierung identisch. Die vollständige Formatprüfung, der Build und alle 109 automatischen Tests bestehen.
- MA-03 ergänzt die acht Starttypen mit stabilen Kennungen, verständlichen Namen und strukturierten Freigaben für normale Diensttypen und Einsatzmuster.
- Reguläre Freigaben, manuelle Lösungsvorschläge und eine ausdrücklich vor dem Planungslauf zu aktivierende Musterfreigabe sind getrennte Fachwerte. TypAH2 darf dadurch `D` regulär, einen einzelnen Frühdienst nur als Vorschlag und `Spr` nur nach aktivierter Laufoption übernehmen.
- Typ1 trägt eine strukturierte Richtlinie: keine automatische Einteilung, mindestens eine spätere manuelle Wochenzuweisung, Schutz vorhandener manueller Zuweisungen und letzte Vorschlagspriorität.
- Freigaben speichern ausschließlich stabile System-04-Kennungen und keine kopierten Namen, Farben, Standardzeiten oder Musterbestandteile.
- MA-04 ergänzt ein unveränderliches Mitarbeiterobjekt mit stabiler Kennung, getrenntem Vor- und Nachnamen, verständlichem Anzeigenamen, Aktivstatus und genau einer `EmployeeTypeId`.
- Namensänderung, Typwechsel und Deaktivierung erzeugen neue gültige Fassungen und lassen den vorherigen Stand unverändert. Gleiche menschliche Namen bleiben zulässig, weil die technische Identität über `EmployeeId` gesichert ist.
- Das Mitarbeiterobjekt enthält weder ein kopiertes Wochen-Soll noch eigene Einsatzfreigaben. Die Prüfung, ob eine Typkennung im gemeinsamen Katalog existiert, und die Grenze von höchstens einer aktiven Typ1-Person folgen erst in Application.
- MA-05 stellt getrennte Leseabfragen für Mitarbeiterübersicht, Mitarbeiterdetails und den verständlichen Mitarbeitertypkatalog bereit.
- Die unveränderlichen Ausgaben enthalten getrennte und zusammengesetzte Namen, Aktivstatus, aktuelle Typangaben, Wochenstunden sowie verständliche deutsche Freigabetexte. Gleiche Anzeigenamen bleiben als getrennte Kennungen erhalten.
- Typangaben werden bei jedem Lesen aus der gemeinsam gelieferten aktuellen Typdefinition aufgelöst und nicht aus Mitarbeiterkopien übernommen. Leere Ergebnisse und Abbruch vor dem Lesen sind geprüft.
- MA-06 stellt getrennte Schreibabläufe für Anlegen, Namen bearbeiten, Typ wechseln und Deaktivieren bereit. Ergebnisse besitzen stabile Codes und verständliche deutsche Meldungen für Validierung, nicht gefundene Daten, ungültige Typbezüge, Typ1- und Änderungskonflikte.
- Anlegen und Typwechsel verlangen über eigene Speicherverträge eine atomare Prüfung auf höchstens eine aktive Typ1-Person. Die Deaktivierung der letzten aktiven Typ1-Person bleibt während der Stammdateneinrichtung zulässig; eine spätere Planung wird den fehlenden Zielzustand blockieren.
- MA-07 legt den bestehenden `ServiceCatalogDbContext`, die Infrastructure-Assembly, `__EFMigrationsHistory` und den vorhandenen Migrationsordner als einzige gemeinsame SQLite-Migrationsfolge fest. Der historische Context-Name bleibt bewusst erhalten, damit die veröffentlichte System-04-Migration nicht umgeschrieben wird.
- Charakterisierungstests sichern das exakte leere System-04-Ausgangsschema und das unveränderte erneute Öffnen einer bereits initialisierten, synthetisch geänderten Datenbank. Zwei zusätzliche Architekturtests verhindern einen zweiten `DbContext` oder Migrationsordner.
- MA-08 ergänzt die acht Mitarbeitertypen, ihre 38 Einsatzfreigaben und die Mitarbeitertabelle über eine zweite generierte Migration in derselben gemeinsamen Folge. Es werden keine realen Mitarbeiter vorbefüllt.
- `SqliteEmployeeStore` implementiert die bestätigten Lese- und Schreibverträge. Namen, Typwechsel, Deaktivierung und erneutes Laden bleiben über Store-Neustarts erhalten; Typwerte werden aus dem gemeinsam gespeicherten Katalog aufgelöst.
- Fremdschlüssel, Prüfregeln und ein eindeutiger gefilterter Datenbankindex verhindern unbekannte Referenzen, das Entfernen verwendeter Typen und mehr als eine aktive Typ1-Person. Optimistische Vergleiche schützen vor dem Überschreiben zwischenzeitlicher Änderungen.
- MA-09 ergänzt einen getrennten Reiter „Mitarbeitende“. Die Übersicht zeigt Namen, Aktivstatus, Typ, Wochen-Soll und Einsatzmöglichkeiten.
- Lade-, Leer- und Fehlerzustand, erneutes Laden und die Auswahl einer Person sind automatisch geprüft. Der Auftraggeber hat den Reiter „Mitarbeitende“, den leeren deutschen Zustand und den Button „Aktualisieren“ am 2026-09-14 sichtbar bestätigt.
- MA-10 ergänzt getrennte WPF-Aktionen zum Anlegen, Namen bearbeiten, Typ wechseln und Deaktivieren. Pflichtfeld- und Typ1-Konflikte bleiben verständlich und ohne Verlust der Eingaben sichtbar.
- Die Typauswahl erklärt vor dem Speichern Code, Namen, Wochen-Soll und Einsatzmöglichkeiten. Eine Deaktivierung benötigt eine ausdrückliche Bestätigung und löscht keine Person.
- MA-10A ergänzt Reaktivieren und endgültiges Löschen für deaktivierte Mitarbeitende. Reaktivieren erhält Kennung, Namen und Typ und verwendet dieselbe atomare Typ1-Grenze wie Anlegen und Typwechsel.
- Nur inaktive und noch nie fachlich referenzierte Personen dürfen endgültig gelöscht werden. Ein SQLite-Fremdschlüsseltest weist nach, dass verwendete Personen deaktiviert erhalten bleiben und verständlich als nicht löschbar gemeldet werden.
- Die rote Löschbestätigung zeigt den vollständigen Namen und den Hinweis, dass die Aktion nicht rückgängig gemacht werden kann. Abbruch und Fehler lassen die Person unverändert; erst erfolgreiche Speicherung entfernt sie aus der Übersicht.
- MA-10A benötigt keine Datenbankmigration. Künftige Plan-, Verfügbarkeits-, Abwesenheits- und Zeitkontobezüge müssen das Löschen mit restriktiven Fremdschlüsseln verhindern.
- System 04 – Einsatzorte, Diensttypen und Doppeldienste – ist als erstes Fachsystem ausgewählt, damit System 03 später vorhandene stabile Kennungen zuordnen kann.
- Die beantwortete Fragen-Datei bestätigt Einsatzorte, Diensttypen, Standardzeiten, tatsächliche Bedarfszeiten, Doppeldienst, Springer und den minimalen Bedienumfang.
- Die frühere Annahme einer stets festen Diensttypzeit ist ersetzt: Jeder Bedarf verlangt genau einen Diensttyp, besitzt aber seine tatsächliche, ausdrücklich bearbeitbare Zeit.
- Die Entscheidung und ihre Auswirkungen auf die Systeme 03 bis 12 sind in `docs/decisions/SHIFT_TYPES_AND_STAFFING_DEMAND_MODEL.md` festgehalten.
- ED-01 und damit der Roadmap-Entwurf sind ausdrücklich abgenommen.
- ED-02 trennt normale Diensttypen von den zusammengesetzten Einsatzmustern `D` und `Spr` und ist ausdrücklich abgenommen.
- ED-03 führt `WorkLocation`, die stark typisierte `WorkLocationId`, geprüfte Namen und erweiterbare semantische Farbkennungen ausschließlich in Domain ein.
- Cafeteria und Restaurant besitzen feste technische Kennungen und die bestätigten Farbkennungen `yellow` beziehungsweise `red`. Weitere Orte und Farbkennungen bleiben ohne feste Aufzählung möglich.
- Änderungen an Name oder Farbe erzeugen ein neues gültiges Einsatzortobjekt und bewahren die stabile Kennung; ungültige Eingaben liefern strukturierte Fehlercodes.
- ED-03 und sein Einsatzortmodell sind ausdrücklich abgenommen.
- ED-04 führt `ShiftType`, die stark typisierte `ShiftTypeId`, geprüfte Bezeichnungen, Einsatzortbezüge, Tabellenanzeigen und getrennte `ShiftStandardTime`-Werte ausschließlich in Domain ein.
- Frühdienst `F`, Spätdienst `S` sowie Cafeteria-Dienst A und B besitzen feste technische Kennungen und die bestätigten Standardzeiten. Cafeteria-Dienste verwenden statt eines Kürzels die spätere tatsächliche Zeitanzeige.
- Standardzeiten akzeptieren nur ganze Minuten auf 30-Minuten-Grenzen, müssen am selben Tag enden und liefern ihre Dauer in ganzen Minuten. Eine Änderung erzeugt eine neue Fassung und bewahrt die Diensttyp-Kennung.
- ED-04 und sein Diensttyp- und Standardzeitmodell sind ausdrücklich abgenommen.
- ED-05 führt die stark typisierte `ShiftPatternId` und einen gemeinsamen Einsatzmustervertrag ausschließlich in Domain ein.
- Der feste Doppeldienst `D` referenziert ausschließlich Früh- und Spätdienst im Restaurant. Seine drei Stunden Unterbrechung zählen nicht zu den zehn Stunden Standardarbeitszeit.
- Der samstägliche Springer `Spr` referenziert Cafeteria-Dienst B und anschließend den Spätdienst. Seine Wechselregel folgt dem tatsächlichen Ende des ersten Bedarfs; eine feste Wechselzeit oder konkrete Zuweisungsabschnitte werden noch nicht gespeichert.
- Die zusätzliche semantische Farbkennung `blue` des Springers wird zusammen mit `Spr` aus Domain an Application übergeben; Text bleibt weiterhin der primäre Informationsträger.
- ED-05 und sein Doppeldienst- und Springer-Modell sind ausdrücklich abgenommen.
- ED-06 führt eine konsistente unveränderliche Katalogabfrage für Einsatzorte, normale Diensttypen, `D` und `Spr` in Application ein und ist ausdrücklich abgenommen.
- Zwei getrennte Commands erlauben ausschließlich die bestätigten Änderungen: Name und Farbkennung eines Einsatzortes sowie die Standardzeit eines Diensttyps. Anlegen, Löschen und freie Musterkombinationen besitzen weiterhin keinen Anwendungsfall.
- Drei kleine Speicherverträge decken Kataloglesen, Einsatzortänderung und Standardzeitänderung ab.
- Validierungs-, Nicht-gefunden- und Änderungskonflikte werden mit stabilen Codes und verständlichen deutschen Meldungen zurückgegeben. Abbruchtoken werden vor und während der Speicheraufrufe weitergereicht.
- Eine Standardzeitänderung von Früh- oder Spätdienst wird vor dem Speichern zusätzlich gegen den festen Doppeldienst geprüft und darf dessen Reihenfolge, Überschneidungsfreiheit und Unterbrechung nicht verletzen.
- ED-07 implementiert diese drei Verträge mit einer lokalen SQLite-Datenbank ausschließlich in Infrastructure und ist ausdrücklich abgenommen.
- Die erste veröffentlichbare Migration legt Einsatzorte, Diensttypen und die Beziehungen von `D` und `Spr` an. Sie stellt Cafeteria, Restaurant und die vier bestätigten Diensttypen mit stabilen Kennungen idempotent bereit.
- Standardzeiten werden als ganze Minuten gespeichert. Fremdschlüssel, Zeitprüfungen sowie eindeutige Musterarten und Anzeigecodes werden durch SQLite selbst abgesichert.
- Die konkrete Adaptererzeugung liegt ausschließlich in `Desktop.Composition`; eine WPF-Fachseite oder produktive Datenbankdatei wird in ED-07 noch nicht gestartet beziehungsweise angelegt.
- Sechs Infrastructure-Tests mit echten temporären SQLite-Dateien prüfen leere Migration, wiederholte Initialisierung, Laden, Ändern, Neustart, Beziehungen, Eindeutigkeit, Konflikte und fehlgeschlagene Schreibvorgänge.
- ED-08 ersetzt das leere Hauptfenster durch eine deutsche Einsatzortverwaltung auf der gemeinsamen Seite „Einsatzorte und Diensttypen“.
- Cafeteria und Restaurant sind mit Name sowie zusätzlicher gelber beziehungsweise roter Text- und Farbkennzeichnung auswählbar. Name und Farbe können gespeichert werden; Anlegen, Löschen, Deaktivieren und Reaktivieren bleiben weiterhin ausgeschlossen.
- Die Oberfläche zeigt getrennte Lade-, Leer-, Arbeits-, Erfolgs-, Validierungs- und technische Fehlerzustände. Unerwartete technische Fehler werden ohne Namen, Eingabewerte oder lokale Pfade protokolliert.
- Die App erzeugt und verwendet ihre lokale Katalogdatenbank unter `%LocalAppData%\Salztal Dienstplanung\dienstplanung.db`; im Repository liegt weiterhin keine Datenbankdatei.
- Acht Desktop-Tests prüfen Laden, Auswahl, Leerzustand, laufende Vorgänge, gültiges Speichern, Validierung und technische Fehler.
- Die sichtbare Einsatzortverwaltung aus ED-08 wurde vom Auftraggeber geprüft und am 2026-09-13 ausdrücklich abgenommen.
- ED-09 zeigt unter jedem gewählten Einsatzort ausschließlich seine zugehörigen normalen Diensttypen. Beginn und Ende der Standardzeit lassen sich in 30-Minuten-Schritten auswählen und getrennt je Diensttyp speichern.
- Cafeteria-Dienste zeigen nach dem Speichern ihre Standardzeit als Anzeige, während Früh- und Spätdienst ihre festen Kürzel `F` und `S` behalten. Ungültige Zeitfolgen sowie eine Verletzung des festen Doppeldienstes werden deutsch gemeldet, ohne die gewählte Eingabe zu verlieren.
- `D` wird mit Restaurant, Früh- und Spätdienst, Unterbrechung ohne Arbeitszeit sowie Standardarbeitszeit angezeigt. `Spr` wird zusätzlich blau und als samstägliche Abfolge von Cafeteria-Dienst B zum Spätdienst dargestellt; eine feste Wechselzeit und freie Musterbearbeitung werden nicht angeboten.
- Die sichtbaren Diensttyp-, Doppeldienst- und Springer-Abläufe aus ED-09 wurden vom Auftraggeber geprüft und am 2026-09-13 ausdrücklich abgenommen.
- ED-10 hat Domain, Application, Infrastructure und Desktop gegen den bestätigten Umfang von System 04 abgeglichen. Die stabilen Kennungen und unveränderlichen Katalogmomentaufnahmen bilden eine eindeutige Übergabe an das spätere System 03.
- Der damalige System-03-Entwurf plante nur `WorkLocationId`- und `ShiftTypeId`-Freigaben. Die am 2026-09-14 bestätigte Mitarbeitertyp-Entscheidung ergänzt strukturierte kontextabhängige Musterfreigaben, ohne Katalogdaten oder Musterdefinitionen aus System 04 zu kopieren.
- ED-10 und damit System 04 sind am 2026-09-13 ausdrücklich abgenommen. Die Roadmap und ihre beantwortete Fragen-Datei sind unter `docs/roadmaps/completed` archiviert.
- Der damalige System-03-Entwurf wurde nach dieser Archivierung unter `docs/roadmaps/active` wieder aufgenommen, fachlich überarbeitet und umgesetzt. MA-01 bis MA-11 einschließlich MA-02A sind bestätigt; die Roadmap und ihre beantworteten Fragen sind unter `docs/roadmaps/completed` archiviert.
- Zum Abschluss von System 04 bestanden 14 Application-Tests, 46 Domain-Tests, 6 Infrastructure-Tests, 13 Desktop-Tests und 13 Architekturtests. Die aktuellen Gesamtzahlen stehen unter „Offene Prüf- und Abnahmegates“.
- TG-07 mit den sieben getrennten Testprojekten für die sechs Produktionsmodule und die Architektur ist abgenommen.
- TG-08 mit zwölf Architekturtests und den drei nachgewiesenen Fehlermutationen ist abgenommen.
- TG-09 und damit System 02 sind nach dem manuellen sichtbaren Starttest vollständig abgenommen und archiviert.
- TG-08 hat zwölf Architektur-Testfälle gegen reale Projektdateien, Produktionsassemblies und Desktop-Quellen angelegt.
- Die Tests sichern den erwarteten Projektbestand, erlaubte Projektreferenzen, Zyklusfreiheit, Paketgrenzen, Testprojektreferenzen und die besondere `Desktop.Composition`-Grenze.
- Drei kontrollierte Fehlermutationen für Projektreferenz, Paketposition und Namespace-Nutzung wurden zuverlässig erkannt und danach vollständig entfernt.
- Jedes modulspezifische Testprojekt referenziert nur sein Produktionsprojekt; das Architektur-Testprojekt darf alle Produktionsprojekte untersuchen.
- xUnit v3 `4.0.1` und die Microsoft Testing Platform bilden die zentrale .NET-10-Testinfrastruktur.
- Alle sieben Testprojekte besitzen Paket-Lockdateien und sind in der Solution enthalten.
- CommunityToolkit.Mvvm `8.4.2` wird nur in Desktop, Microsoft.EntityFrameworkCore.Sqlite `10.0.12` sowie das private Designtime-Paket Microsoft.EntityFrameworkCore.Design `10.0.12` nur in Infrastructure und Google.OrTools `9.15.6755` nur in Planning direkt referenziert.
- Domain, Application und Excel besitzen weiterhin keine direkte Paketreferenz.
- Paketversionen sind zentral festgelegt; `packages.lock.json` liegt für alle 13 Projekte vor.
- Desktop besitzt nur die vorgesehenen Projektreferenzen auf Application und die technischen Adaptermodule.
- Das Hauptfenster zeigt die deutsche Einsatzort- und Diensttypverwaltung und weist die weiterhin fehlenden Dienstplanfunktionen klar aus.
- Die vollständige Solution mit 13 Projekten kompiliert mit 0 Warnungen und 0 Fehlern.
- Normale und gesperrte Paketwiederherstellung bestehen; die aktuelle NuGet-Sicherheitsprüfung meldet keine bekannten verwundbaren Pakete.
- Der vollständige Testlauf des damaligen technischen Grundgerüsts bestand mit 12 von 12 Architekturtests; System 04 ergänzt einen weiteren Architekturtest.
- TG-09 hat Solution, Projekt- und Testbestand, zentrale Konfiguration, Paketgrenzen und Architekturtests gemeinsam abgeglichen und ist abgenommen.
- Die gesperrte Wiederherstellung, der vollständige Build mit 0 Warnungen und 0 Fehlern sowie aktuell 13 von 13 Architekturtests bestehen erneut.
- Die Desktop-App startete technisch mit reagierendem Hauptfenster und dem Titel „Salztal Dienstplanung“ und wurde regulär mit Exitcode 0 beendet.
- Der Auftraggeber hat den sichtbaren lokalen Start, den Fenstertitel und die beiden deutschen Hinweistexte bestätigt.
- Es wurden noch keine fachlichen Planungs- oder Excel-Abläufe angelegt. Der vollständige sichtbare Mitarbeiterablauf einschließlich Reaktivieren, Löschbestätigung und erneutem Laden wurde vom Auftraggeber bestätigt.

## Noch nicht begonnen

- Systeme 06 bis 14
- System 15 – Portable Windows-Auslieferung und Endabnahme der Kernversion
- System 16 – Zeitkonten als spätere Ausbaustufe

Die genaue Einordnung steht in der [Master-Roadmap](MASTER_ROADMAP.md). Vor jedem System wird eine eigene Teil-Roadmap erstellt und abgenommen.

## Bestätigte Entscheidungen für System 04

- Cafeteria und Restaurant als bearbeitbare Startwerte; weitere Einträge technisch vorbereitet,
- Cafeteria gelb, Restaurant rot und Springer `Spr` blau, jeweils zusätzlich mit Text,
- normale Diensttypen mit genau einem Einsatzort und bearbeitbarer Standardzeit,
- Doppeldienst `D` und Springer `Spr` als zusammengesetzte Einsatzmuster, die niemals selbst von einem einzelnen Bedarf verlangt werden,
- tatsächlich zu besetzende Zeit im Bedarf mit genau einem verlangten Diensttyp,
- Standardänderungen für zukünftige Vorgaben und getrennte Ausnahmen für ein Datum,
- 30-Minuten-Eingabeschritte, minutengenaue Speicherung und keine Dienste über Mitternacht,
- Doppeldienst `D` ausschließlich als Frühdienst plus Spätdienst im Restaurant,
- samstäglicher Springer `Spr` nur als Notfall mit sichtbar verbleibender Teilunterdeckung vor dem Restaurantwechsel,
- erste Bedienfassung mit Anzeigen und Bearbeiten auf einer gemeinsamen Seite; weitere Stammdatenfunktionen später.

Die Antworten sind in der Fragen-Datei und im Entscheidungsdokument festgehalten. Roadmap, Fachmodell und die Implementierungsschritte ED-01 bis ED-10 sind abgenommen; System 04 ist abgeschlossen und archiviert.

## Offene Detailentscheidungen für spätere Schritte

- Verhalten beim späteren Löschen noch aktiv referenzierter Stammdaten,
- genaue Regelpriorität der Springer-Teilunterdeckung.

## Bestätigte Entscheidungen für System 05

- Die bekannten Startbedarfe für Cafeteria und Restaurant sind für die erste Fassung vollständig.
- Cafeteria benötigt Montag bis Freitag eine Person von 13:30 bis 20:30 Uhr. Samstag und Sonntag werden eine Person von 13:30 bis 20:30 Uhr und zusätzlich eine zweite Person von 13:30 bis 17:30 Uhr benötigt.
- Restaurant benötigt täglich vier Personen im Frühdienst von 06:30 bis 13:30 Uhr und vier Personen im Spätdienst von 16:30 bis 19:30 Uhr.
- Je Einsatzort, Datum und normalem Diensttyp genügt ein zusammenhängender Bedarfsblock mit konstanter Personenzahl. Verschiedene Diensttypen dürfen sich zeitlich überschneiden.
- Eine Standardänderung gilt ab einer bewusst gewählten Planungswoche und damit ab deren Montag. Frühere Wochen und bereits angelegte Datumsausnahmen bleiben unverändert.
- Derselbe regelmäßige Bedarf darf am selben Wirksamkeitsmontag wiederholt korrigiert werden. Ältere Fassungen bleiben nachvollziehbar und die zuletzt gespeicherte Korrektur ist wirksam.
- Die regelmäßige Bedarfsansicht soll den links gewählten Einsatzort vollständig von Montag bis Sonntag zeigen.
- Eine Datumsausnahme kann Personenzahl und tatsächliche Zeit ersetzen, einen Bedarf aufheben oder einen sonst fehlenden Bedarf ergänzen. Entfernen stellt den dann wirksamen Standard wieder her.
- Feiertage werden zunächst manuell als Datumsausnahmen erfasst.
- Stunden werden aus tatsächlicher Dauer mal Personenzahl in ganzen Minuten berechnet und nicht unabhängig eingegeben.
- Die erste Übersicht soll Bedarf, Tagessummen je Einsatzort, Wochensummen je Einsatzort und die Gesamtsumme der Woche darstellen; die atomaren Daten bleiben für spätere abgestimmte Summen verfügbar.

Diese Entscheidungen und die umgesetzten Code-Anker stehen in `docs/roadmaps/active/STAFFING_DEMAND_ROADMAP.md`. BE-01 bis BE-08 sind ausdrücklich abgenommen. BE-09 wurde nach der Sichtprüfung nicht abgenommen; BE-09A und BE-09B sind umgesetzt und automatisch geprüft. Ihre gemeinsame sichtbare Abnahme ist offen.

## Bestätigte Entscheidungen für System 03

- getrennte Vor- und Nachnamen, stabile unsichtbare Mitarbeiterkennung, gleiche Namen erlaubt und keine Personalnummer,
- genau ein gemeinsam referenzierter bindender Mitarbeitertyp je Mitarbeiter,
- acht datengetriebene Starttypen mit 40, 25, 30, 30, 35, 35, 10 beziehungsweise 10 Wochenstunden,
- Änderungen einer Typdefinition wirken auf alle aktuell zugeordneten Mitarbeitenden und nachfolgende Generierungen,
- höchstens eine aktive Typ1-Person; vor einer späteren Planung wird genau eine aktive Person verlangt,
- Typ1 wird manuell vorgetragen, automatisch geschützt und nur als letzte manuelle Lösung vorgeschlagen,
- kontextabhängige Doppeldienstfreigabe für TypAH2 sowie standardmäßig ausgeschaltete Planungslaufoption für dessen Springer-Verwendung,
- zwingender Wochenkorridor von plus/minus drei Stunden für Typ25, Typ30, Typ30a, Typ35 und Typ35a; Typ1 wird nur bewertet und gemeldet; besonderer AH-Korridor von sieben bis zwölf Stunden,
- unveränderlicher Wochenstundenbericht als Bestandteil jedes späteren Planungsergebnisses,
- Mitarbeitende können deaktiviert und reaktiviert werden; eine zweite aktive Typ1-Person bleibt ausgeschlossen,
- nur deaktivierte, noch nie fachlich verwendete Mitarbeitende können nach einer eigenen Unwiderruflichkeitswarnung endgültig gelöscht werden,
- verwendete Typen können erst nach bewusster Neuzuordnung aller betroffenen Personen entfernt werden,
- kein zusätzlicher Qualifikationskatalog in der ersten Fassung,
- erste Mitarbeiteroberfläche mit Übersicht, Anlegen, Bearbeiten, Typwechsel und Deaktivieren; noch ohne Suche, Filter und Typenkatalogpflege.

Die vollständige Fachentscheidung steht in `docs/decisions/EMPLOYEE_TYPES_AND_SHIFT_ELIGIBILITY_MODEL.md`; die gemeinsame technische Migrationsgrenze in `docs/decisions/SHARED_SQLITE_MIGRATION_BOUNDARY.md`. MA-01 bis MA-11 einschließlich MA-02A sind abgenommen; System 03 ist abgeschlossen und archiviert.

## Offene Entscheidungen für spätere Systeme

- vollständige zwingende und priorisierte weiche Regeln,
- genaue Reduktionsformel des Wochen-Solls bei Abwesenheiten,
- Verhalten der Typ1-Generierungsvoraussetzung bei einer vollständig abwesenden Woche,
- Inhalt und Aufbau der noch bereitzustellenden Excel-Vorlage,
- endgültige Bestätigung der Excel-Bibliothek nach dem Vorlagentest,
- praktische Voraussetzungen der portablen Ausgabe auf dem vorgesehenen Windows-11-Rechner.

Diese Entscheidungen sind für den Abschluss der Dokumentationsgrundlage noch nicht erforderlich. Sie werden vor dem jeweils betroffenen System geklärt und nicht vorweggenommen.

## Echte Blockaden

Für das abgeschlossene System 03 und die bisherige Umsetzung von System 05 besteht keine technische Blockade. Für BE-09A und BE-09B steht die gemeinsame sichtbare Bedienprüfung noch aus.

Das zuvor unversionierte Beispielbild mit echten Namen und konkreten Plandaten ist nicht mehr im Arbeitsordner vorhanden. Die daraus benötigten Fachinformationen sind nur abstrahiert und ohne personenbezogene Daten dokumentiert.

Die fehlende Excel-Vorlage blockiert später den Excel-Vorlagentest und System 13, aber nicht die gegenwärtige Projektgrundlage.

## Offene Prüf- und Abnahmegates

- Alle 13 Produktions- und Testprojekte kompilieren erfolgreich; 98 von 98 Application-Tests, 148 von 148 Domain-Tests, 34 von 34 Infrastructure-Tests, 64 von 64 Desktop-Tests und 17 von 17 Architekturtests bestehen. Insgesamt bestehen alle 361 vorhandenen Tests. Planning- und Excel-Testprojekte enthalten im aktuellen Ausbauzustand noch keine Tests; ihr Exitcode 8 wird beim Gesamtlauf ausdrücklich als „keine Tests vorhanden“ behandelt.
- Die korrigierten BE-09A- und BE-09B-Abläufe sind programmiert und automatisch geprüft. Offen ist die gemeinsame Sichtprüfung der wiederholten Bearbeitung desselben Wirksamkeitsmontags sowie der Wochen- und Einsatzortführung.
- Der vollständige sichtbare Einsatzort-, Diensttyp-, Doppeldienst- und Springer-Ablauf wurde schrittweise manuell geprüft und durch den Auftraggeber bestätigt.
- Die Paketwiederherstellung und der Build bestätigen noch keinen OR-Tools-Lauf auf einem sauberen Zielsystem. Die Visual-C++-x64-Laufzeitvoraussetzung wird erst bei der portablen Auslieferung praktisch geprüft.
- Die gemeinsame SQLite-Speicherung für Servicekatalog, Mitarbeitertypen, Einsatzfreigaben und Mitarbeitende ist mit leeren sowie vom System-04-Stand aktualisierten temporären Testdateien geprüft. Übersicht, Bearbeitung, Reaktivierung und referenzgeschütztes Löschen sind implementiert und sichtbar bestätigt; Planungsengine und Excel-Export stehen noch aus.
- Die WPF-Mitarbeiterbedienung wurde vom Auftraggeber einschließlich des vollständigen MA-10-/MA-10A-Ablaufs bestätigt. Dieses System-03-Gate ist bestanden.
- Portable Windows-Ausgabe und Start auf einem geeigneten Windows-11-System stehen noch aus.
- Die fachliche Endabnahme durch die Service-Leitung steht noch aus.

Keines dieser späteren Gates wird vorzeitig als bestanden geführt.

## Nächster minimaler Schritt

BE-09B sichtbar prüfen: Cafeteria und Restaurant über die linke Einsatzortliste wechseln, die vollständige Woche kontrollieren und denselben regelmäßigen Bedarf zweimal für denselben Wirksamkeitsmontag speichern. Danach BE-09A und BE-09B ausdrücklich abnehmen oder Abweichungen melden.
