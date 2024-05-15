using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace AzCostTgBot.Core.Providers.CostManagement.Models;

[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
public class TableResponse
{
    public required ColumnModel[] Columns { get; init; } = [];

    public required JsonElement[][] Rows { get; init; } = [];
}
