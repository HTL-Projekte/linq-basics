namespace LinqBasics.Tests;

public sealed class UnitTests
{
    private static decimal TotalOf(int qty, decimal price) => qty * price;

    private static object? GetProp(object obj, string name)
        => obj.GetType().GetProperty(name)?.GetValue(obj);

    [Fact]
    public void Task1_ProductNames_AreOrderedAlphabetically()
    {
        var ex = new LinqExercises();

        var result = ex.GetAllProductNamesOrdered().ToList();

        Assert.Equal(
            new[] { "Dock", "Keyboard", "Laptop", "Laptop", "Monitor", "Monitor", "Mouse", "Mouse" },
            result
        );
    }

    [Fact]
    public void Task2_EuSummaries_AreFilteredAndOrderedByTotalDesc()
    {
        var ex = new LinqExercises();

        var result = ex.GetEuSaleSummariesByTotalDesc().ToList();

        // EU sales are Ids: 1,2,3,5,6,8
        Assert.Equal(new[] { 1, 5, 6, 8, 3, 2 }, result.Select(r => r.SaleId).ToArray());

        // Spot-check totals
        var byId = result.ToDictionary(x => x.SaleId);
        Assert.Equal(1200m, byId[1].Total);
        Assert.Equal(1100m, byId[5].Total);
        Assert.Equal(280m,  byId[6].Total);
        Assert.Equal(160m,  byId[8].Total);
        Assert.Equal(80m,   byId[3].Total);
        Assert.Equal(50m,   byId[2].Total);

        // All are EU
        Assert.All(result, x => Assert.NotNull(x)); // sanity
    }

    [Fact]
    public void Task3_MustBeDeferred_ModificationsAfterCallAreVisibleOnEnumeration()
    {
        var ex = new LinqExercises();

        // Call first (should NOT materialize)
        var query = ex.GetEuProductTotalsDeferred();

        // Modify underlying list AFTER calling the method
        ex.SampleSales.Add(new Sale(
            Id: 999,
            Customer: "Zoe",
            Product: "Headset",
            Quantity: 2,
            UnitPrice: 90m,
            SoldAt: new DateTime(2025, 01, 11),
            Region: "EU"
        ));

        // Now enumerate
        var result = query.ToList();

        // Expect the new item to appear (because deferred execution)
        Assert.Contains(result, o =>
            (string)GetProp(o, "Product")! == "Headset" &&
            (decimal)GetProp(o, "Total")! == 180m
        );

        // Also verify ordering by SoldAt ascending: earliest EU sale is 2025-01-10 (Id 1)
        // and Headset is 2025-01-11, so it should come after the first EU item.
        var orderedDates = result.Select(o => (DateTime)GetProp(o, "SoldAt")! ).ToList();
        Assert.True(orderedDates.SequenceEqual(orderedDates.OrderBy(d => d)));
    }

    [Fact]
    public void Task4_MustBeEager_ModificationsAfterCallDoNotChangeReturnedList()
    {
        var ex = new LinqExercises();

        // Top 2 totals initially: Id 1 (1200), Id 5 (1100)
        var top2 = ex.GetTopSalesEager(2);

        Assert.Equal(2, top2.Count);
        Assert.Equal(new[] { 1, 5 }, top2.Select(s => s.Id).ToArray());

        // Add a new huge sale AFTER the call
        ex.SampleSales.Add(new Sale(
            Id: 1000,
            Customer: "Mallory",
            Product: "Server",
            Quantity: 1,
            UnitPrice: 9999m,
            SoldAt: new DateTime(2025, 05, 01),
            Region: "EU"
        ));

        // If method was eager, the previously returned list is unchanged
        Assert.DoesNotContain(top2, s => s.Id == 1000);
        Assert.Equal(new[] { 1, 5 }, top2.Select(s => s.Id).ToArray());
    }

    [Fact]
    public void Task5_AnonymousProjection_FilterAndMultiSort_WorkCorrectly()
    {
        var ex = new LinqExercises();

        // minTotal = 200 includes:
        // Id 1 (1200) Alice Laptop
        // Id 4 (600)  Chloe Monitor (US)
        // Id 5 (1100) Dylan Laptop
        // Id 6 (280)  Bob Monitor
        var result = ex.GetHighValueSalesAnonymous(200m).ToList();

        // Verify shape + values exist
        Assert.All(result, o =>
        {
            Assert.NotNull(GetProp(o, "Customer"));
            Assert.NotNull(GetProp(o, "Product"));
            Assert.NotNull(GetProp(o, "Total"));
        });

        // Verify ordering: Customer asc, Total desc within same customer
        // Expected customers in order: Alice, Bob, Chloe, Dylan
        var customers = result.Select(o => (string)GetProp(o, "Customer")!).ToList();
        Assert.Equal(new[] { "Alice", "Bob", "Chloe", "Dylan" }, customers);

        // Verify totals match those rows
        var tuples = result.Select(o => (
            Customer: (string)GetProp(o, "Customer")!,
            Product:  (string)GetProp(o, "Product")!,
            Total:    (decimal)GetProp(o, "Total")!
        )).ToList();

        Assert.Contains(("Alice", "Laptop", 1200m), tuples);
        Assert.Contains(("Bob", "Monitor", 280m), tuples);
        Assert.Contains(("Chloe", "Monitor", 600m), tuples);
        Assert.Contains(("Dylan", "Laptop", 1100m), tuples);
    }
}