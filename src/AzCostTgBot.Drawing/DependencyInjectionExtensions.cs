using Microsoft.Extensions.DependencyInjection;

namespace AzCostTgBot.Drawing;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDrawing(this IServiceCollection services)
    {
        return services
            .AddSingleton<IPlotting, SkiaPlotting>();
    }
}
