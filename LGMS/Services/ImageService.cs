using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;

namespace LGMS.Services
{
    public class ImageService
    {
        private static readonly Color BackgroundColor = Color.White;
        private static readonly Color TextColor = Color.Black;
        private const int Width = 1400;
        private const int EntryHeight = 250;
        private const int VerticalMargin = 60;
        private const int HorizontalMargin = 20;
        private const int LeftSectionWidth = 700;
        private static readonly string FontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Poppins-Bold.ttf");
        private static readonly float FontSize = 80f;

        private Font font;

        public ImageService()
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(FontPath);
            font = new Font(fontFamily, FontSize); 
        }

        public byte[] GenerateImage(List<(string Code, string Description)> data)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));
            }

            int height = data.Count * EntryHeight;
            using var image = new Image<Rgba32>(Width, height);
            image.Mutate(ctx => ctx.Fill(BackgroundColor));

            int yOffset = VerticalMargin;

            foreach (var (code, description) in data)
            {
                DrawEntry(image, font, code, description, yOffset);
                yOffset += EntryHeight;
            }

            image.Mutate(ctx =>
            {
                ctx.DrawLine(TextColor, 4, new PointF(LeftSectionWidth, 0), new PointF(LeftSectionWidth, height));
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        private void DrawEntry(Image<Rgba32> image, Font textFont, string code, string description, int yOffset)
        {
            image.Mutate(ctx =>
            {
                var codeLines = code.Split('\n');
                ctx.DrawText(codeLines[0], textFont, TextColor, new PointF(HorizontalMargin, yOffset));
                ctx.DrawText(codeLines.Length > 1 ? codeLines[1] : "", textFont, TextColor, new PointF(HorizontalMargin, yOffset + 65));

                ctx.DrawLine(TextColor, 4, new PointF(LeftSectionWidth, yOffset - VerticalMargin / 2), new PointF(LeftSectionWidth, yOffset + EntryHeight - VerticalMargin));

                ctx.DrawText(description, textFont, TextColor, new PointF(LeftSectionWidth + HorizontalMargin / 2, yOffset));
            });

            image.Mutate(ctx =>
            {
                ctx.DrawLine(TextColor, 4, new PointF(0, yOffset + EntryHeight - VerticalMargin / 2), new PointF(Width, yOffset + EntryHeight - VerticalMargin / 2));
            });
        }
    }
}
