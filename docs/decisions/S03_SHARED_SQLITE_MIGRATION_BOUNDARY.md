# Gemeinsame SQLite- und Migrationsgrenze

Status: Mit MA-07 am 2026-09-14 ausdrücklich abgenommen

## Zweck

Dieses Dokument legt fest, wie die bestehende lokale SQLite-Datenbank aus System 04 um die späteren Mitarbeiterdaten erweitert wird. Es verhindert zwei unkoordinierte Migrationsverläufe für dieselbe Datenbankdatei.

## Verbindliche Entscheidung

- `ServiceCatalogDbContext` bleibt der einzige Entity-Framework-Context für die gemeinsame lokale Anwendungsdatenbank.
- Der historische Name bleibt bestehen, weil die veröffentlichte System-04-Migration und ihr Modell-Snapshot bereits an diesen Context gebunden sind. Eine Umbenennung hätte in MA-07 keinen fachlichen Nutzen und würde veröffentlichte Migrationsmetadaten berühren.
- Alle weiteren Schemaänderungen werden als neue Migrationen im bestehenden Ordner `Persistence/ServiceCatalog/Migrations` und in derselben Infrastructure-Assembly fortgeführt.
- Die gemeinsame Historientabelle bleibt `__EFMigrationsHistory`. Die zusätzliche technische Tabelle `__EFMigrationsLock` gehört zur Migrationssperre von EF Core 10 und stellt keinen zweiten Migrationsverlauf dar. Laufzeit- und Designzeit-Erzeugung verwenden dieselbe Context-Factory und damit dieselbe ausdrückliche Migrationskonfiguration.
- Die veröffentlichte Migration `20260913202156_InitialServiceCatalog` sowie ihr Designer werden nicht nachträglich geändert.
- Ein zweiter `DbContext`, ein zweiter Migrationsordner oder eine zweite Historientabelle für dieselbe Datenbankdatei sind nicht zulässig.

## Grenze zu MA-08

MA-07 fügt weder Mitarbeiter-Tabellen noch Mitarbeiter-Startdaten hinzu. MA-08 ergänzt Typkatalog, Einsatzfreigaben und Mitarbeitende mit einer neuen Migration innerhalb der hier festgelegten gemeinsamen Folge. Die vorhandenen System-04-Katalogdaten müssen dabei unverändert lesbar bleiben.

## Technische Absicherung

- Infrastructure-Tests prüfen die exakte veröffentlichte Migration und das daraus erzeugte System-04-Schema auf einer leeren SQLite-Datei.
- Ein Charakterisierungstest öffnet eine bereits initialisierte und synthetisch geänderte System-04-Datenbank erneut und vergleicht Schema sowie Migrationshistorie unverändert; der Katalog bleibt lesbar.
- Architekturtests sichern genau einen `DbContext` und genau einen Migrationsordner im Infrastructure-Projekt.
- Die Context-Factory benennt Migrationsassembly und Historientabelle ausdrücklich über `SharedDatabaseMigrationBoundary`.

## Bewusster Wartungspunkt

Der Name `ServiceCatalogDbContext` ist enger als seine künftige Verantwortung. Er wird bewusst als historischer technischer Name weitergeführt. Sollte später eine Umbenennung nötig werden, benötigt sie einen eigenen abgenommenen Migrationsschritt mit Upgrade-Nachweis für vorhandene Datenbanken; sie darf nicht durch nebenläufige Migrationen umgangen werden.
