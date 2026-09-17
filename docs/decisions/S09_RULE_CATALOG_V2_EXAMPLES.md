# Regelkatalogversion 2 – ergänzende Beispielszenarien

Stand: 2026-09-17

## Zweck und Versionsgrenze

Dieses Dokument ergänzt die unverändert weitergeltenden synthetischen Beispiele aus `S07_RULE_CATALOG_EXAMPLES.md` für Regelkatalogversion 2. Alle in Version 2 unverändert übernommenen Regeln verwenden weiterhin ihre dortigen `S07-*`-Szenarien. Die folgenden `S09-*`-Szenarien decken ausschließlich die drei neuen Regeln ab.

`AH_WEEKLY_TARGET` gehört nur zu Katalogversion 1. Version 2 ersetzt seine frühere Gesamtwirkung durch `AH_WEEKLY_MINIMUM` und `RELATIVE_WEEKLY_TARGET`. `MINIMIZE_SPLIT_SHIFTS` bleibt fachlich unverändert, wird im neuen Zielvektor jedoch nur noch in der eigenen `D`-Stufe ausgewertet und nicht zusätzlich unter den verbleibenden mittleren Regeln gezählt.

## `MINIMIZE_RELIEF_SHIFTS`

- `S09-MINIMIZE_RELIEF_SHIFTS-SATISFIED`: Von zwei hinsichtlich Bedarfsdeckung und früheren Schutzregeln gleichen Plänen wird der Plan mit weniger zulässigen Springer-Einsätzen gewählt.
- `S09-MINIMIZE_RELIEF_SHIFTS-VIOLATED`: Ein Plan enthält einen Springer-Einsatz, obwohl ein hinsichtlich aller früheren Ziele gleicher Plan ohne Springer-Einsatz möglich ist.
- `S09-MINIMIZE_RELIEF_SHIFTS-NOT_APPLICABLE`: Es gibt keinen zulässigen Springer-Kandidaten oder alle hinsichtlich früherer Ziele gleichen Pläne enthalten gleich viele Springer-Einsätze.

## `AH_WEEKLY_MINIMUM`

- `S09-AH_WEEKLY_MINIMUM-SATISFIED`: Eine automatisch planbare AH-Person erreicht bei vorhandenem zulässigem Bedarf mindestens 180 Minuten in der betrachteten Montag-bis-Sonntag-Woche.
- `S09-AH_WEEKLY_MINIMUM-VIOLATED`: Eine automatisch planbare AH-Person erhält bei vorhandenem zulässigem Bedarf nur 179 Minuten; der Regelfall weist eine fehlende Minute aus.
- `S09-AH_WEEKLY_MINIMUM-NOT_APPLICABLE`: Für die AH-Person besteht in der betrachteten Woche kein zulässiger Bedarf oder die Person besitzt nicht die Planungsrolle AH.

## `RELATIVE_WEEKLY_TARGET`

- `S09-RELATIVE_WEEKLY_TARGET-SATISFIED`: Nach allen früheren Zielstufen ist die größte relative Abweichung vom persönlichen Wochenziel minimal; weitere Abweichungen werden anschließend absteigend fair ausgeglichen.
- `S09-RELATIVE_WEEKLY_TARGET-VIOLATED`: Bei identischen früheren Zielwerten wird ein Plan gewählt, dessen größte relative Abweichung höher ist als die einer zulässigen Alternative.
- `S09-RELATIVE_WEEKLY_TARGET-NOT_APPLICABLE`: Das wirksame Wochenziel beträgt null; diese Person-Woche wird ohne künstlichen Quotienten aus dem relativen Vergleich ausgeschlossen.

## Vergleichssemantik

- Relative Abweichungen werden als exakte Brüche `absolute Minutenabweichung / wirksames Wochenziel` verglichen.
- Zuerst entscheidet die größte relative Abweichung, danach die nächstgrößte und so weiter.
- Die Implementierung verwendet Kreuzmultiplikation mit ausreichend breiter Ganzzahlarithmetik und weder Rundung noch frei geschätzte Punkte.
- Für Nicht-AH ist der Nenner das durch `U` und `K` wirksam reduzierte Wochensoll; für AH sind es 600 Minuten.
