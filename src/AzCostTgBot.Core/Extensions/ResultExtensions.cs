using FluentResults;

namespace AzCostTgBot.Core.Extensions;

public static class ResultExtensions
{
    public static string GetErrorMessage(this Result result)
        => string.Join(';', result.Errors.Select(e => e.Message));
}
