namespace AzCostTgBot.Drawing;

public sealed class PlotOptions
{
    public int Width { get; set; } = 1000;

    public int FontSize { get; set; } = 16;

    public ImageFormat Format { get; set; } = ImageFormat.Jpeg;

    public IColorPalette Palette { get; set; } = new Tableau10Palette();
}
