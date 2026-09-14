# Folgefragen zu MA-10A: Reaktivieren und endgültiges Löschen

Stand: 2026-09-14

Status: Beide Empfehlungen am 2026-09-14 bestätigt; MA-10A zur Implementierung freigegeben

## Bereits eindeutig verstanden

- Nur deaktivierte Mitarbeitende können reaktiviert oder endgültig gelöscht werden.
- Reaktivieren behält Kennung, Namen und Mitarbeitertyp unverändert bei.
- Eine deaktivierte Typ1-Person darf nur reaktiviert werden, wenn dadurch nicht zwei aktive Typ1-Personen entstehen. Andernfalls wird die Reaktivierung verständlich abgelehnt; es erfolgt kein automatischer Typwechsel.
- Endgültiges Löschen benötigt eine eigene bewusste Bestätigung und ist kein Nebeneffekt der Deaktivierung.
- Eine gelöschte Mitarbeiterkennung wird nicht erneut vergeben.
- Löschen verändert keine bestehenden oder abgenommenen Pläne und entfernt keine historischen Planmomentaufnahmen.
- Aktive Mitarbeitende können nicht direkt gelöscht werden. Sie müssen zuerst bewusst deaktiviert werden.
- Massenänderungen, Papierkorb, Wiederherstellung gelöschter Personen und automatische Typwechsel gehören nicht zu MA-10A.

## Offene Entscheidungen

### MA10A-01 – Bereits verwendete Mitarbeitende

Soll eine deaktivierte Person endgültig gelöscht werden dürfen, wenn sie bereits in einem Plan, einer Verfügbarkeit, einer Abwesenheit, einem Zeitkonto oder anderen späteren Fachdaten verwendet wird?

Empfehlung: Nein. Endgültiges Löschen ist nur möglich, solange die Person noch nirgendwo fachlich verwendet wurde. Sobald ein Bezug besteht, bleibt sie deaktiviert erhalten und die App erklärt, warum sie nicht gelöscht werden kann. So bleiben Historie und spätere Querverweise zuverlässig nachvollziehbar.

Antwort: Empfehlung bestätigt. Bereits fachlich verwendete Mitarbeitende dürfen nicht endgültig gelöscht werden und bleiben deaktiviert erhalten.

### MA10A-02 – Bestätigung des endgültigen Löschens

Welche Bestätigung soll unmittelbar vor dem endgültigen Löschen verlangt werden?

Empfehlung: Eine klare rote Warnung mit vollständigem Anzeigenamen, dem Hinweis „Diese Aktion kann nicht rückgängig gemacht werden“ und den getrennten Buttons „Abbrechen“ und „Endgültig löschen“. Das erneute Eintippen des Namens wäre für diese lokale Einzelbenutzer-App unnötig umständlich.

Antwort: Empfehlung bestätigt. Es genügt die klare Warnung mit vollständigem Namen sowie „Abbrechen“ und „Endgültig löschen“; der Name muss nicht erneut eingegeben werden.

## Nach den Antworten

Die Entscheidungen sind in den MA-10A-Schritt und die betroffenen Leitdokumente übernommen. MA-10A wurde am 2026-09-14 ausdrücklich zur Implementierung gestartet.
