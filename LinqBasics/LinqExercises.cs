namespace LinqBasics;

public class LinqExercises
{
    /// <summary>
    /// The predefined list all queries must target.
    /// Tests will also modify this list to check deferred/eager behavior.
    /// </summary>
    public List<Sale> SampleSales { get; } = new()
    {
        new Sale(1, "Alice", "Laptop",   1, 1200m, new DateTime(2025, 01, 10), "EU"),
        new Sale(2, "Bob",   "Mouse",    2,   25m, new DateTime(2025, 01, 12), "EU"),
        new Sale(3, "Alice", "Keyboard", 1,   80m, new DateTime(2025, 02, 02), "EU"),
        new Sale(4, "Chloe", "Monitor",  2,  300m, new DateTime(2025, 02, 10), "US"),
        new Sale(5, "Dylan", "Laptop",   1, 1100m, new DateTime(2025, 03, 01), "EU"),
        new Sale(6, "Bob",   "Monitor",  1,  280m, new DateTime(2025, 03, 05), "EU"),
        new Sale(7, "Chloe", "Mouse",    3,   22m, new DateTime(2025, 03, 08), "US"),
        new Sale(8, "Eva",   "Dock",     1,  160m, new DateTime(2025, 04, 11), "EU"),
    };

    // ---------------------------
    // Task 1 (basic projection + ordering, primitive types)
    // ---------------------------
    // Return all product names (can repeat) ordered alphabetically A..Z.
    // Must return an IEnumerable<string>.
    public IEnumerable<string> GetAllProductNamesOrdered()
    {
        return SampleSales .Select(s => s.Product) .OrderBy(p => p);
    }

    // ---------------------------
    // Task 2 (filtering + ordering + projection to complex type)
    // ---------------------------
    // Return summaries for EU sales only, ordered by Total descending.
    // Total = Quantity * UnitPrice.
    // Project into SaleSummary objects.
    public IEnumerable<SaleSummary> GetEuSaleSummariesByTotalDesc()
    {
        return SampleSales .Where(s => s.Region == "EU")
            .Select(s => new SaleSummary(
                s.Id,
                s.Customer,
                s.Product,
                s.Quantity * s.UnitPrice,
                s.SoldAt))
            .OrderByDescending(ss => ss.Total);
    }

    // ---------------------------
    // Task 3 (anonymous class projection + MUST be DEFERRED)
    // ---------------------------
    // Return ONLY EU sales as anonymous objects:
    //   new { Product, Total }
    // where Total = Quantity * UnitPrice.
    // Ordered by SoldAt ascending (oldest first).
    //
    // IMPORTANT: This method MUST be deferred (do NOT call ToList/ToArray here).
    // The tests will modify SampleSales after calling this method; enumeration must reflect the change.
    public IEnumerable<object> GetEuProductTotalsDeferred()
    {
        return SampleSales
            .Where(s => s.Region == "EU")
            .OrderBy(s => s.SoldAt)
            .Select(s => (object)new
            {
                s.Product,
                Total = s.Quantity * s.UnitPrice,
                s.SoldAt
            });
    }

    // ---------------------------
    // Task 4 (eager fetching required)
    // ---------------------------
    // Return the top N sales by Total descending (Total = Quantity * UnitPrice).
    // This method MUST eagerly materialize the result into a List<Sale>.
    // The tests will add new sales after calling this method; your returned list must NOT change.
    public List<Sale> GetTopSalesEager(int n)
    {
        return SampleSales
            .OrderByDescending(s => s.Quantity * s.UnitPrice)
            .Take(n)
            .ToList();
    }

    // ---------------------------
    // Task 5 (anonymous class projection + filtering + multi-level ordering)
    // ---------------------------
    // Return sales with Total >= minTotal as anonymous objects:
    //   new { Customer, Product, Total }
    // Ordered by Customer ascending, then by Total descending.
    // Returning as IEnumerable<object> is fine.
    public IEnumerable<object> GetHighValueSalesAnonymous(decimal minTotal)
    {
        return SampleSales
            .Where(s => s.Quantity * s.UnitPrice >= minTotal)
            .Select(s => new
            {
                s.Customer,
                s.Product,
                Total = s.Quantity * s.UnitPrice
            })
            .OrderBy(s => s.Customer)
            .ThenByDescending(s => s.Total);
    }
}