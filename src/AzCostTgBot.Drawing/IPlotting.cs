using System.Numerics;

namespace AzCostTgBot.Drawing;

public interface IPlotting
{
    Stream Pie<T>(T[] values, string[] categories, PlotOptions? options = default)
        where T : INumber<T>;
}
