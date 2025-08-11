using System.Globalization;
using System.Numerics;
using SkiaSharp;

namespace AzCostTgBot.Drawing;

public class SkiaPlotting : IPlotting
{
    public Stream Pie<T>(T[] values, string[] categories, PlotOptions? options = default) where T : INumber<T>
    {
        options ??= new PlotOptions();

        var width = options.Width;
        var height = width;
        var fontSize = options.FontSize;

        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);

            canvas.SetMatrix(SKMatrix.CreateScale(1.0f, 1.0f));
            canvas.SetMatrix(SKMatrix.CreateIdentity());

            // Define pie chart starting rectangle
            var rectPadding = width / 5;
            var rect = new SKRect(rectPadding, rectPadding, width - rectPadding, height - rectPadding);

            var colors = options.Palette.GetColorHexCodes()
                .Select(ToSkColor)
                .ToList();

            var total = values.Aggregate(T.Zero, (acc, value) => acc + value);

            float startAngle = 0;
            for (var i = 0; i < values.Length; i++)
            {
                // Cast generic math type to float to satisfy SkiaSharp API
                var valueShare = Convert.ToSingle(values[i] / total, CultureInfo.InvariantCulture);
                var sweepAngle = valueShare * 360;
                using (var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = colors[i % colors.Count], IsAntialias = true })
                {
                    canvas.DrawArc(rect, startAngle, sweepAngle, useCenter: true, paint);
                }

                // Calculate label position outside the circle
                var segmentMidAngle = startAngle + (sweepAngle / 2);
                var labelOutsideDistance = rectPadding / 4.0f;
                var labelXOutside = rect.MidX + (float)(((rect.Width / 2) + labelOutsideDistance) * Math.Cos(segmentMidAngle * Math.PI / 180));
                var labelYOutside = rect.MidY + (float)(((rect.Height / 2) + labelOutsideDistance) * Math.Sin(segmentMidAngle * Math.PI / 180));

                // Draw line from the segment center to the label
                var segmCenterX = rect.MidX + (float)(rect.Width / 2 * Math.Cos(segmentMidAngle * Math.PI / 180));
                var segmCenterY = rect.MidY + (float)(rect.Height / 2 * Math.Sin(segmentMidAngle * Math.PI / 180));
                float strokeWidth = fontSize > 10 ? fontSize / 10 : 1;
                using (var linePaint = new SKPaint { Color = colors[i % colors.Count], StrokeWidth = strokeWidth, IsAntialias = true })
                {
                    canvas.DrawLine(segmCenterX, segmCenterY, labelXOutside, labelYOutside, linePaint);
                }

                // Draw the label
                using (var textPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true })
                using (var font = new SKFont { Size = fontSize })
                {
                    var percentage = valueShare * 100;
                    var valueLabelText = $"{FormatNumber(values[i])} ({(percentage < 0.1 ? "~" : string.Empty)}{percentage:F1}%)";

                    var textBounds = new SKRect();
                    _ = font.MeasureText(valueLabelText, out textBounds);

                    // On right side left-align the text
                    if (labelXOutside < rect.MidX)
                    {
                        canvas.DrawText(valueLabelText, labelXOutside - textBounds.Width, labelYOutside, SKTextAlign.Left, font, textPaint);
                        canvas.DrawText(categories[i], labelXOutside - textBounds.Width, labelYOutside + font.Size, SKTextAlign.Left, font, textPaint);
                    }
                    else
                    {
                        canvas.DrawText(valueLabelText, labelXOutside, labelYOutside, SKTextAlign.Left, font, textPaint);
                        canvas.DrawText(categories[i], labelXOutside, labelYOutside + font.Size, SKTextAlign.Left, font, textPaint);
                    }
                }

                startAngle += sweepAngle;
            }
        }

        var stream = new MemoryStream();
        var success = bitmap.Encode(stream, ToSkFormat(options.Format), 100);
        if (!success)
        {
            // TODO: use fluent results
            throw new InvalidOperationException("Failed to encode the image");
        }

        stream.Position = 0;
        return stream;
    }

    private static SKColor ToSkColor(string hexCode)
    {
        return SKColor.Parse(hexCode);
    }

    private static SKEncodedImageFormat ToSkFormat(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            ImageFormat.Png => SKEncodedImageFormat.Png,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format")
        };
    }

    private static string FormatNumber<T>(T value) where T : INumber<T>
    {
        if (value is IFormattable formattable)
        {
            return value % T.One == T.Zero
                ? formattable.ToString("0", CultureInfo.InvariantCulture)
                : formattable.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // Suppress nullable warning, as the method is only called for INumber<T> types
        return value.ToString()!;
    }
}
