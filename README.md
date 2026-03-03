# LINQ Basics - Übung

Diese Übung führt in grundlegende LINQ-Konzepte in C# ein, mit praktischen Beispielen zur Datenabfrage und -verarbeitung.

## Projektstruktur

- **LinqBasics/** - Hauptprojekt mit den LINQ-Übungen
  - `Models.cs` - Datenmodelle (`Sale` und `SaleSummary`)
  - `LinqExercises.cs` - Implementierung der fünf Aufgaben
  - `Program.cs` - Einstiegspunkt
- **LinqBasics.Tests/** - Unit Tests (mit xUnit)

## Datenmodelle

### `Sale`
Ein Record, der einen Verkauf darstellt:
- **Id**: Eindeutige Verkaufsnummer
- **Customer**: Kundenname
- **Product**: Produktname
- **Quantity**: Menge
- **UnitPrice**: Einheitspreis
- **SoldAt**: Verkaufsdatum
- **Region**: Region (EU oder US)

### `SaleSummary`
Ein Record für Verkaufszusammenfassungen:
- **SaleId**: Verkaufsnummer
- **Customer**: Kundenname
- **Product**: Produktname
- **Total**: Gesamtsumme (Quantity × UnitPrice)
- **SoldAt**: Verkaufsdatum

## Implementierte Aufgaben

### Task 1: Produktnamen sortiert
`GetAllProductNamesOrdered()` - Gibt alle Produktnamen alphabetisch sortiert aus (mit Wiederholungen).
```csharp
return SampleSales.Select(s => s.Product).OrderBy(p => p);
```

### Task 2: EU-Verkaufszusammenfassungen
`GetEuSaleSummariesByTotalDesc()` - Filtert nur EU-Verkäufe, projiziert sie in `SaleSummary`-Objekte und sortiert nach Gesamtsumme absteigend.
```csharp
return SampleSales
    .Where(s => s.Region == "EU")
    .Select(s => new SaleSummary(...))
    .OrderByDescending(ss => ss.Total);
```

### Task 3: EU-Verkäufe als verzögerte Abfrage
`GetEuProductTotalsDeferred()` - Gibt EU-Verkäufe als anonyme Objekte mit Produkt und Gesamtsumme zurück.
**Wichtig**: Diese Methode gibt eine **verzögerte Abfrage** zurück (keine `ToList()`), sodass Änderungen an der `SampleSales`-Liste reflektiert werden.

### Task 4: Top N Verkäufe (eager)
`GetTopSalesEager(int n)` - Gibt die Top N Verkäufe nach Gesamtsumme zurück.
**Wichtig**: Diese Methode materalisiert das Ergebnis in eine `List<Sale>` mit `.ToList()`, sodass nachträgliche Änderungen nicht reflektiert werden.

### Task 5: Hochwertige Verkäufe
`GetHighValueSalestAnonymous(decimal minTotal)` - Filtert Verkäufe mit Gesamtsumme ≥ minTotal und projiziert sie als anonyme Objekte. Sortiert nach Kunde aufsteigend, dann nach Gesamtsumme absteigend.

## Wichtige LINQ-Konzepte

- **Select()**: Projektion - Transformiert Daten
- **Where()**: Filterung - Selektiert Daten nach Bedingungen
- **OrderBy() / OrderByDescending()**: Sortierung
- **ThenBy() / ThenByDescending()**: Sekundäre Sortierung
- **Take()**: Nimmt die erste N Elemente
- **ToList()**: Eager evaluation - Materialisiert sofort
- **Deferred execution**: LINQ-Abfragen werden erst bei Enumeration ausgeführt
