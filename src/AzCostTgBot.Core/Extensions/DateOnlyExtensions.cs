namespace AzCostTgBot.Core.Extensions;

public static class DateOnlyExtensions
{
    public static DateTime ToUtcDate(this DateOnly date)
    {
        return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day), DateTimeKind.Utc);
    }
}
