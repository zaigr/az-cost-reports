using System.Text.Json;

namespace AzCostTgBot.Core.Providers.CostManagement.Models;

public class TableResponse
{
    public required ColumnModel[] Columns { get; init; }

    public required JsonElement[][] Rows { get; init; }
}
