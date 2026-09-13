# Technische Paketbasis

Stand: 2026-09-13

Status: Produktionspakete aus TG-06 und Testpaket aus TG-07 abgenommen

## Zweck

Dieses Dokument hält die in TG-06 und TG-07 geprüften stabilen Paketversionen, ihre Zuständigkeit und die noch offenen praktischen Auslieferungsgates fest. Paketversionen werden zentral in `Directory.Packages.props` verwaltet; die einzelnen Projekte referenzieren Pakete ohne eigene Versionsangabe.

## Bestätigte Pakete

| Paket | Version | Zuständiges Projekt | Zweck | Lizenz |
|---|---:|---|---|---|
| `CommunityToolkit.Mvvm` | `8.4.2` | `Salztal.Dienstplanung.Desktop` | MVVM-Bausteine für die spätere WPF-Oberfläche | MIT |
| `Microsoft.EntityFrameworkCore.Sqlite` | `10.0.12` | `Salztal.Dienstplanung.Infrastructure` | Lokale SQLite-Persistenz über Entity Framework Core | MIT |
| `Google.OrTools` | `9.15.6755` | `Salztal.Dienstplanung.Planning` | Spätere mathematische Dienstplanoptimierung | Apache-2.0 |
| `xunit.v3` | `4.0.1` | ausschließlich alle Projekte unter `tests` | Testframework mit integrierter Microsoft Testing Platform | Apache-2.0 |

## Prüfergebnis

- Alle vier direkt referenzierten Versionen sind am jeweiligen Prüftag die aktuellen stabilen Versionen auf NuGet.org; Vorabversionen wurden nicht aufgenommen.
- `CommunityToolkit.Mvvm` stellt ein .NET-8-Ziel bereit und wird von NuGet als kompatibel mit `net10.0-windows` ausgewiesen. Das Projekt wird von Microsoft veröffentlicht und gepflegt und gehört zur .NET Foundation.
- `Microsoft.EntityFrameworkCore.Sqlite` zielt direkt auf .NET 10 und ist damit passend zum festgelegten Ziel-Framework.
- `Google.OrTools` stellt ein .NET-8-Ziel bereit, wird von NuGet als kompatibel mit .NET 10 ausgewiesen und enthält eine Abhängigkeit auf das Laufzeitpaket `Google.OrTools.runtime.win-x64`.
- `xunit.v3` unterstützt .NET 8 und höher, wird von NuGet als kompatibel mit .NET 10 ausgewiesen und gehört zur .NET Foundation.
- Die Wiederherstellung wird durch versionierte `packages.lock.json`-Dateien gesperrt reproduzierbar gemacht.
- Die Pakete werden ausschließlich in ihren architektonisch zuständigen Projekten referenziert. Domain, Application und Excel bleiben frei von technischen Paketen.

## Testläufer

- `global.json` wählt für das festgelegte .NET-10-SDK ausdrücklich `Microsoft.Testing.Platform` aus.
- xUnit v3 bindet die Microsoft Testing Platform bereits über seine transitiven Pakete ein. Zusätzliche direkte Referenzen auf `Microsoft.NET.Test.Sdk` oder `xunit.runner.visualstudio` sind für den festgelegten Kommandozeilen-Testlauf nicht erforderlich.
- Die Testprojekte sind eigenständig ausführbare Testmodule. TG-08 ergänzt zwölf bestandene Architekturtests; Fachtests entstehen erst zusammen mit den späteren Fachsystemen.

## Offene praktische Gates

- OR-Tools verwendet eine native C++-Bibliothek. Google nennt deshalb die Microsoft Visual C++ Redistributable für Visual Studio 2022 in der x64-Variante als Laufzeitvoraussetzung.
- Ob diese Voraussetzung auf einem sauberen Windows-11-System und auf dem späteren Klinikrechner erfüllt oder mit der portablen Ausgabe bereitzustellen ist, wird erst in System 15 praktisch geprüft.
- Die Paketaufnahme und ein erfolgreicher Entwicklungs-Build ersetzen weder einen OR-Tools-Laufzeittest noch den späteren portablen Startnachweis.
- Eine Excel-Bibliothek bleibt bis zum Charakterisierungstest mit der echten Vorlage ausdrücklich offen.

## Offizielle Quellen

- CommunityToolkit.Mvvm: <https://www.nuget.org/packages/CommunityToolkit.Mvvm/>
- .NET Community Toolkit: <https://github.com/CommunityToolkit/dotnet>
- Microsoft.EntityFrameworkCore.Sqlite: <https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/>
- Google.OrTools: <https://www.nuget.org/packages/Google.OrTools/>
- OR-Tools für .NET unter Windows: <https://developers.google.com/optimization/install/dotnet/pkg_windows>
- xUnit.net v3: <https://www.nuget.org/packages/xunit.v3/>
- xUnit.net v3 mit dem .NET SDK: <https://xunit.net/docs/getting-started/v3/getting-started>
- `dotnet test` und Microsoft Testing Platform: <https://learn.microsoft.com/dotnet/core/tools/dotnet-test>
