namespace LinqBasics;

public sealed record Sale(
    int Id,
    string Customer,
    string Product,
    int Quantity,
    decimal UnitPrice,
    DateTime SoldAt,
    string Region
);

public sealed record SaleSummary(
    int SaleId,
    string Customer,
    string Product,
    decimal Total,
    DateTime SoldAt
);