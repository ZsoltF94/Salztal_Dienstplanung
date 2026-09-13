# Vorschlag für die technische Architektur

Status: Am 2026-09-13 mit der Änderung „Windows 10 entfällt“ abgenommen

Stand der Prüfung: 2026-09-13

## Entscheidung in einem Satz

Empfohlen wird eine klassische Windows-Anwendung mit **C#, .NET 10 LTS und WPF**, einer lokalen **SQLite-Datenbank**, einer getrennten Planungsengine auf Basis von **Google OR-Tools CP-SAT** und einem austauschbaren **Excel-Exportmodul**.

Die erste Auslieferung soll als selbstständiger `win-x64`-Ordner erfolgen. Dieser Ordner kann kopiert und entpackt werden; eine zusätzliche .NET-Laufzeit muss nicht installiert werden. Die native Visual-C++-Laufzeit für OR-Tools bleibt bis zu einem Test auf einem sauberen Zielrechner eine gesonderte Voraussetzung.

Dieser Vorschlag wurde bestätigt. Die verbindliche Umsetzung der Entscheidung steht in `ARCHITECTURE.md`.

## Warum diese Lösung empfohlen wird

- Die Zielplattform ist ausschließlich Windows 11. Eine zusätzliche plattformübergreifende Oberflächentechnik bringt daher zunächst keinen Nutzen.
- WPF ist eine ausgereifte Windows-Oberfläche und enthält bereits geeignete Tabellen-, Datenbindungs- und Layoutfunktionen.
- Oberfläche, Datenbank, Excel-Verarbeitung und Planungsalgorithmus können in derselben Sprache C# entwickelt werden.
- .NET kann Anwendungen mitsamt Laufzeit als selbstständigen Ordner veröffentlichen. Auf dem Zielrechner muss dann keine passende .NET-Version installiert sein.
- Google OR-Tools unterstützt .NET und enthält mit CP-SAT einen geeigneten Ansatz für Dienstplanung mit zwingenden Bedingungen und gewichteten Wünschen.
- SQLite benötigt keinen Datenbankserver und passt deshalb zu einer lokalen Einzelbenutzer-App.
- Die Planungslogik kann vollständig ohne Benutzeroberfläche getestet werden.
- Die Anwendung bleibt modular: Ein späterer Wechsel der Excel-Bibliothek oder einzelner Speicherbausteine erfordert keinen Umbau der Planungsregeln.

## Vergleich der Oberflächen-Technologien

| Kriterium | .NET 10 mit WPF | .NET 10 mit WinUI 3 | Tauri 2 mit Web-Oberfläche |
|---|---|---|---|
| Klassische Windows-App | Ja | Ja | Ja, über eingebettete Web-Oberfläche |
| Eignung für große Plantabellen | Sehr gut; WPF enthält DataGrid und Datenbindung | Möglich, aber mehr Zusatzentscheidungen für komplexe Tabellen | Sehr flexibel mit HTML und CSS |
| Portable Ordner-Auslieferung | Direkt als selbstständige .NET-Veröffentlichung | Möglich, benötigt aber zusätzliche Windows-App-SDK-Konfiguration und größere Laufzeitbestandteile | Möglich, setzt WebView2 voraus oder muss dessen Laufzeit mitliefern |
| Anzahl der Technologiewelten | C# und XAML | C# und XAML plus Windows App SDK | TypeScript/HTML/CSS plus Rust und WebView |
| Verbindung zu SQLite, OR-Tools und Excel | Direkt über .NET-Bibliotheken | Direkt über .NET-Bibliotheken | Zusätzliche Brücke oder alternative Bibliotheken erforderlich |
| Lokales Testen im Browser | Nein | Nein | Ja |
| Passung zu diesem Projekt | **Am höchsten** | Gut, aber unnötig aufwendiger | Gut für Web-Oberflächen, hier jedoch mehr Komplexität als Nutzen |

### Warum nicht WinUI 3 als erste Wahl?

Microsoft empfiehlt WinUI 3 allgemein für neue native Windows-Anwendungen. Für eine interne Einzelbenutzer-App ohne Store, automatische Updates, Benachrichtigungen oder andere moderne Windows-Integrationen werden diese Vorteile zunächst nicht benötigt.

Eine portable WinUI-3-Auslieferung ist möglich. Eine vollständig selbstständige Ausgabe muss jedoch die Windows-App-SDK-Laufzeit mitbringen; eine Ein-Datei-Ausgabe entpackt ihre Bestandteile beim ersten Start. WPF besitzt für diesen Anwendungsfall den einfacheren Bereitstellungsweg und eine sehr ausgereifte Tabellenoberfläche.

### Warum nicht Tauri als erste Wahl?

Tauri würde lokale Browsertests der Oberfläche ermöglichen. Dafür kämen jedoch Rust, eine Web-Oberfläche und WebView2 als zusätzliche technische Ebenen hinzu. Für die Planungsengine und den Excel-Export wären weitere Schnittstellen zwischen diesen Ebenen nötig.

Auf Windows 11 ist WebView2 normalerweise vorhanden. Für garantiert offline nutzbare Rechner ohne passende Laufzeit müsste diese jedoch mitgeliefert werden, was die Ausgabe deutlich vergrößert. Da die endgültige App ausdrücklich eine klassische Windows-App sein soll, überwiegen hier die Vorteile einer einheitlichen .NET-Lösung.

## Vorgeschlagener technischer Aufbau

### Laufzeit und Sprache

- .NET 10 LTS
- C#
- Zielarchitektur Windows 11 x64
- keine Vorabversionen von .NET oder anderen Kernbibliotheken
- Paketversionen werden zentral festgelegt und nur kontrolliert aktualisiert

.NET 10 ist eine aktive LTS-Version und wird von Microsoft bis November 2028 unterstützt. Vor dem ersten App-Projekt soll das lokal installierte SDK auf einen aktuellen, unterstützten Patchstand gebracht werden.

### Benutzeroberfläche

- WPF als Windows-Oberfläche
- XAML für Fenster, Ansichten und wiederverwendbare Darstellung
- MVVM zur Trennung von Anzeige und Programmlogik
- `CommunityToolkit.Mvvm` als kleine, von Microsoft gepflegte MVVM-Hilfe
- deutsche sichtbare Texte; Fachlogik und Datenbankzugriffe bleiben außerhalb der Fensterklassen

### Lokale Datenhaltung

- eine lokale SQLite-Datenbank
- Entity Framework Core mit dem offiziellen SQLite-Provider
- Datenbankänderungen ausschließlich über nachvollziehbare Migrationen
- Datenbankdatei im normalen lokalen Windows-Benutzerordner
- Sicherung vor jeder Datenbankmigration
- Schreibvorgänge, die zusammengehören, werden gemeinsam oder gar nicht gespeichert

SQLite passt zur Einzelbenutzer-App, weil kein Dienst installiert und kein Datenbankserver betrieben werden muss. Entity Framework Core erleichtert nachvollziehbare Änderungen des Datenmodells. Bekannte Einschränkungen von SQLite werden bei Datum, Uhrzeit, Dezimalwerten und Nebenläufigkeit ausdrücklich getestet.

### Automatische Dienstplanung

- eine eigene Planungsengine ohne Abhängigkeit von WPF oder SQLite
- Google OR-Tools CP-SAT hinter einer projekteeigenen Schnittstelle
- zwingende Regeln werden als unverletzbare Bedingungen modelliert
- jeder tatsächliche Bedarfszeitraum wird entweder durch eine zulässige Person vollständig oder teilweise gedeckt oder ausdrücklich als ungedeckt ausgewiesen
- Bedarfsdeckung wird so modelliert, dass Überbesetzung nicht zulässig ist; Unterbesetzung bleibt als sichtbarer Konflikt möglich
- zuerst wird der Umfang ungedeckter Bedarfszeiträume minimiert, danach werden weiche Regeln in der Reihenfolge hoch, mittel und niedrig optimiert
- die Prioritätsstufen werden hierarchisch gelöst, damit viele niedrige Wünsche niemals einen hohen Wunsch überstimmen
- nicht oder nur teilweise deckbare Bedarfszeiträume bleiben als konkrete offene Zeiträume im Ergebnis erhalten
- ein fester Startwert und gespeicherte Eingaben machen Testergebnisse wiederholbar
- eine einstellbare Zeitgrenze verhindert endlos lange Berechnungen

Die ausdrückliche Möglichkeit vollständiger oder teilweiser Unterdeckung stellt sicher, dass die Planerzeugung nicht vollständig scheitert, wenn ein Bedarf nicht lückenlos gedeckt werden kann. Zwingende Regeln bleiben trotzdem unverletzt.

OR-Tools allein erklärt noch nicht automatisch verständlich, warum etwas unmöglich ist. Deshalb erhält jede erzeugte Bedingung zusätzlich eine eigene fachliche Kennung mit Regel, betroffener Person, Tag, Dienst und Einsatzort. Eine getrennte Diagnose ermittelt daraus:

1. welcher Bedarfszeitraum vollständig oder teilweise ungedeckt blieb,
2. welche zwingenden Bedingungen mögliche Besetzungen ausgeschlossen haben,
3. welche priorisierten Wünsche betroffen sind,
4. welche konkrete Änderung eine Besetzung ermöglichen könnte.

Die Konflikterklärung wird damit als eigenes System behandelt und nicht nur aus einer technischen Fehlermeldung des Solvers abgeleitet.

### Excel-Ausgabe

- ein eigenes Exportmodul ohne Abhängigkeit von WPF oder der Planungsengine
- die gelieferte Excel-Datei wird als Vorlage geöffnet und unter neuem Namen gespeichert
- ClosedXML ist der bevorzugte erste Kandidat, weil es Excel-Dateien ohne installiertes Microsoft Excel lesen, verändern und speichern kann
- das Exportmodul wird über eine eigene Schnittstelle austauschbar gehalten

Die exakte Excel-Vorlage liegt noch nicht vor. Daher ist ClosedXML noch keine endgültige Festlegung. Sobald die Vorlage verfügbar ist, wird eine unveränderte Kopie geladen, mit Testdaten befüllt und anschließend auf Format, Druckbereich, Seitenaufteilung, verbundene Zellen, Formeln und sonstige Bestandteile geprüft.

Falls ClosedXML Bestandteile der Vorlage nicht zuverlässig erhält, kann hinter derselben Exportschnittstelle auf das niedrigere Open XML SDK gewechselt werden. Dieser Wechsel darf die übrige Anwendung nicht berühren.

### Bereitstellung

- erste Zielausgabe: selbstständiger `win-x64`-Ordner als ZIP-Datei
- keine Installation einer .NET-Laufzeit auf dem Zielrechner erforderlich
- Start über eine klar benannte `.exe`
- Datenbank und Sicherungen liegen außerhalb des Programmordners im Benutzerbereich
- ein späterer Installer bleibt möglich, ist aber nicht Bestandteil der ersten Ausbaustufe
- eine Ein-Datei-Ausgabe wird erst nach Einbindung von SQLite, OR-Tools und Excel praktisch geprüft
- die offizielle Windows-Anleitung von OR-Tools verlangt die x64-Ausgabe der Microsoft Visual C++ Redistributable, weil die .NET-Bibliothek eine native C++-Bibliothek einbindet
- vor dem Versprechen „ohne Installation“ wird auf einem sauberen Zielrechner geprüft, ob diese Laufzeit bereits vorhanden ist oder zuverlässig mitgeliefert werden kann
- falls die Voraussetzung nicht ohne Zusatzinstallation erfüllt werden kann, wird entsprechend der bereits bestätigten Vorgabe ein kleiner Installer angeboten

Ein selbstständiger Ordner ist zuverlässiger als das frühe Versprechen einer einzigen Datei, weil OR-Tools und SQLite auch native Bestandteile mitbringen können. Der Nutzer hat den entpackbaren Ordner ausdrücklich als akzeptable portable Form bestätigt.

## Vorgeschlagene Modulgrenzen

Die endgültigen Projekt- und Ordnernamen werden erst beim späteren App-Grundgerüst festgelegt. Fachlich werden folgende Grenzen vorgeschlagen:

1. **Domain**
   - Mitarbeiter, Arbeitszeitmodelle, Einsatzorte, Dienste, Bedarfe, Regeln, Pläne und Konflikte
   - keine Abhängigkeit von Oberfläche, Datenbank, Excel oder OR-Tools
2. **Application**
   - Anwendungsabläufe wie Plan erzeugen, manuell speichern, abnehmen, archivieren und exportieren
   - koordiniert Fachlogik über Schnittstellen
3. **Planning**
   - übersetzt fachliche Regeln in ein Optimierungsmodell
   - führt OR-Tools aus und erzeugt ein fachliches Planungsergebnis
4. **Conflict Explanation**
   - erklärt vollständig oder teilweise ungedeckte Bedarfszeiträume und Regelkonflikte in deutscher Sprache
   - nennt mögliche Änderungen, ohne selbst Daten zu verändern
5. **Infrastructure**
   - SQLite, Entity Framework Core, Migrationen, lokale Dateien und Sicherungen
6. **Excel Export**
   - liest die Vorlage, trägt einen abgenommenen Plan ein und prüft die Ausgabedatei
7. **Desktop UI**
   - WPF-Fenster, Ansichten und Bedienabläufe
   - greift nur über die Anwendungsschicht auf Funktionen zu
8. **Tests**
   - getrennte Tests für Fachregeln, Planung, Konflikterklärung, Datenbank, Excel und Oberfläche

## Daten- und Sicherheitsgrenzen

- Das GitHub-Repository enthält niemals echte Mitarbeiter-, Krankheits-, Verfügbarkeits- oder Plandaten.
- Tests verwenden ausschließlich erfundene Namen und synthetische Pläne.
- Protokolle dürfen keine vollständigen Mitarbeiter- oder Gesundheitsdaten enthalten.
- Excel-Ausgaben, Datenbanken und Sicherungen bleiben außerhalb des Repositorys.
- Die Anwendung verwendet keine Cloud und sendet keine Daten über das Netzwerk.
- Automatische Sicherungen werden lokal erstellt und können von der Service-Leitung wiederhergestellt werden.

## Test- und Abnahmegrenzen

Spätere Prüfungen werden getrennt ausgewiesen:

1. Kompilierung
2. Fachregeltests
3. Tests der Planungsengine
4. Datenbank- und Migrationstests
5. Excel-Vorlagentest
6. Bedienprüfung der Windows-App
7. portable Veröffentlichung als `win-x64`-Ordner
8. Start- und Bedienprüfung auf dem tatsächlichen Windows-11-Zielrechner

Ein bestandener Test ersetzt keinen anderen. Die Funktionsfähigkeit wird erst nach einem erfolgreichen Test auf dem vorgesehenen Windows-11-Klinikrechner als bestätigt gemeldet.

## Bestätigte Entscheidungen

1. Die Zielplattform ist ausschließlich Windows 11 x64. Windows 10 wird nicht mehr berücksichtigt.
2. Die Architektur wird auf C#, .NET 10 LTS und WPF festgelegt.
3. Die erste portable Ausgabe wird als selbstständiger `win-x64`-Ordner vorbereitet.
4. SQLite, OR-Tools CP-SAT und ein austauschbares Excel-Modul werden als technische Bausteine bestätigt.
5. Die genaue Excel-Bibliothek bleibt bis zum Test mit der echten Vorlage austauschbar.

## Folge der Zustimmung

Die Entscheidung wird in PF-05B verbindlich in `ARCHITECTURE.md` dokumentiert. Dabei werden noch keine App-Funktionen implementiert.

## Geprüfte Primärquellen

- [.NET-Supportrichtlinie](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [Installation und Betriebssystem-Unterstützung von .NET unter Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
- [Microsoft-Dokumentation zu WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Überblick über WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Bereitstellung einer selbstständigen .NET-App als einzelne Datei](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
- [Verteilungswege für Windows-Apps](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/choose-distribution-path)
- [Unverpackte WinUI-3-Apps verteilen](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/unpackage-winui-app)
- [Windows-Verteilung mit Tauri und WebView2](https://v2.tauri.app/distribute/windows-installer/)
- [MVVM Toolkit von Microsoft](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Offizieller SQLite-Provider für Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [OR-Tools für .NET](https://developers.google.com/optimization/install/dotnet/)
- [OR-Tools-NuGet unter Windows und benötigte Visual-C++-Laufzeit](https://developers.google.com/optimization/install/dotnet/pkg_windows)
- [Mitarbeiterplanung mit OR-Tools CP-SAT](https://developers.google.com/optimization/scheduling/employee_scheduling)
- [ClosedXML-Projekt](https://github.com/ClosedXML/ClosedXML)
- [Microsoft Open XML SDK](https://github.com/dotnet/Open-XML-SDK)
