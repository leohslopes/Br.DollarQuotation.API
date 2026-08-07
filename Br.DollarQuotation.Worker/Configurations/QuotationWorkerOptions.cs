namespace Br.DollarQuotation.Worker.Configurations;

public sealed class QuotationWorkerOptions
{
    public const string SectionName = "QuotationWorker";

    public bool Enabled { get; set; } = true;

    public int IntervalInSeconds { get; set; } = 30;

    public int DelayBetweenRequestsInMilliseconds { get; set; } = 500;

    public IReadOnlyCollection<string> CurrencyPairs { get; set; } = [];
}