using SkiaSharp;
using System;
using System.IO;
using System.Xml;

namespace FaviconFetcher
{
    public class SVGIconImage : IconImage
    {
        /// <summary>
        /// The raw SVG data
        /// </summary>
        private Svg.Skia.SKSvg _svg;

        /// <summary>
        /// True if the SVG was successfully parsed and has renderable content.
        /// A parse failure (e.g. an incompatible SkiaSharp/SVG library pairing)
        /// is swallowed internally, so callers should check this rather than
        /// assume a non-null SVGIconImage means usable image data.
        /// </summary>
        public bool IsValid => _svg?.Picture != null;

        /// <summary>
        /// Constructor
        /// </summary>
        public SVGIconImage() : base() { }

        /// <summary>
        /// Creates an IconImage from a bitmap.  As SVGs are not bitmap data, this
        /// method converts the provided bitmap into a PNG, encodes it in base64
        /// and embeds it into the SVG.
        /// </summary>
        /// <param name="bitmap">The bitmap to embed in the SVG</param>
        /// <param name="size">The size of the returned SVG. If not provided, bitmap
        /// dimensions will be used.  Otherwise, bitmap will be scaled to fit.</param>
        /// <returns>SVGIconImage with embedded bitmap.</returns>
        public new static IconImage FromSKBitmap(SKBitmap bitmap, IconSize size = null)
        {
            if (bitmap == null) return new SVGIconImage();

            // Use the bitmap dimensions unless a size requested
            var svgIconImage = new SVGIconImage
            {
                Size = size ?? new IconSize(bitmap.Width, bitmap.Height)
            };

            try
            {
                // Resize if mismatch
                if (svgIconImage.Size.Width != bitmap.Width || svgIconImage.Size.Height != bitmap.Height)
                {
                    bitmap = bitmap.Resize(new SKImageInfo(svgIconImage.Size.Width, svgIconImage.Size.Height), SKFilterQuality.High);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    SKData d = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                    d.SaveTo(ms);

                    var bitmapBase64 = Convert.ToBase64String(ms.GetBuffer());

                    // Construct raw SVG XML with bitmap data embedded as base64 string
                    var rawSVG = string.Format(
                        @"<svg xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""{1}"" height=""{2}""><image width=""{1}"" height=""{2}"" xlink:href=""data:image/png;base64,{0}""/></svg>",
                        bitmapBase64,
                        svgIconImage.Size.Width,
                        svgIconImage.Size.Height);

                    using (XmlReader r = XmlReader.Create(new StringReader(rawSVG)))
                    {
                        svgIconImage._svg = new Svg.Skia.SKSvg();
                        // Attempt to load the svg data
                        svgIconImage._svg.Load(r);
                    }
                }
            }
            catch (Exception)
            {

            }

            return svgIconImage;
        }

        /// <summary>
        /// Creates an IconImage from a stream.
        /// </summary>
        /// <param name="stream">The strem to read the SVG data from.</param>
        /// <param name="size">The size of the returned SVG. If not provided, the
        /// SVG's original dimensions will be used.  Does not change the SVG's
        /// actual dimensions, but will affect the output of the Save() method.</param>
        /// <returns>SVGIconImage</returns>
        public new static IconImage FromStream(Stream stream, IconSize size = null)
        {
            var svgIconImage = new SVGIconImage
            {
                Size = size == null || size == IconSize.Empty
                    ? IconSize.Scaleable
                    : size
            };

            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.Position = 0;
                    stream.CopyTo(memStream);
                    memStream.Position = 0; // Reset to start for decode

                    var svg = new Svg.Skia.SKSvg();
                    svg.Load(memStream);
                    svgIconImage._svg = svg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVGIconImage] Failed to parse SVG: {ex}");
            }

            return svgIconImage;
        }

        /// <summary>
        /// Creates an IconImage from a file.
        /// </summary>
        /// <param name="filename">The full file path and name of the data to be read.</param>
        /// <param name="size">The size of the returned icon. If not provided, the
        /// SVG's original dimensions will be used.  Does not change the SVG's
        /// actual dimensions, but will affect any examination of the IconImage's size
        /// and output of the Save() method.</param>
        /// <returns>SVGIconImage</returns>
        public new static IconImage FromFile(string filename, IconSize size = null)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return FromStream(fs, size);
            }
        }

        /// <summary>
        /// Saves the SVGIconImage to disk in PNG form.
        /// </summary>
        /// <param name="filename">The full path and filename to write the SVGIconImage to.</param>
        /// <param name="outputSize">An IconSize object specifying the size that the scaled icon
        /// should fit within proportionally.  Specifiy null for no scaling, which will write
        /// the IconImage to disk at its original size.</param>
        /// <returns></returns>
        public override bool Save(string filename, IconSize outputSize = null)
        {
            var canvasSize = _GetCanvasSize();

            if (outputSize != null && (canvasSize.Width != outputSize.Width || canvasSize.Height != outputSize.Height))
            {
                var scaleFactor = Math.Min((double)outputSize.Width / canvasSize.Width, (double)outputSize.Height / canvasSize.Height);
                var width = (int)(canvasSize.Width * scaleFactor);
                var height = (int)(canvasSize.Height * scaleFactor);

                outputSize = new IconSize(width, height);
            }

            var _bitmap = ToSKBitmap(outputSize ?? Size);

            try
            {
                using (FileStream fs = File.Create(filename))
                {
                    SKData d = _bitmap.Encode(SKEncodedImageFormat.Png, 100);
                    d.SaveTo(fs);
                }

                return File.Exists(filename);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the SVG data to a bitmap at the specified size.
        /// </summary>
        /// <param name="size">An IconSize object specifying the size that the scaled icon
        /// should fit within proportionally.</param>
        /// <returns>SKBitmap</returns>
        public SKBitmap ToSKBitmap(IconSize size)
        {
            SKBitmap bitmap = new SKBitmap(size.Width, size.Height);

            try
            {
                // We can't use SkBitmap.FromImage(SKImage.FromPicture(_svg.Picture)... here
                // unfortunately, because that doesn't allow us to scale vector to fit size
                //bitmap = SKBitmap.FromImage(SKImage.FromPicture(_svg.Picture, new SKSizeI(size.Width, size.Height)));

                SKCanvas canvas = new SKCanvas(bitmap);

                // ViewBox not always reliable and browsers seem to typically ignore the
                // SVG ViewBox for favicons, so adopting same behavior which resolves
                // some clipping seen in testing with various sites.
                var canvasSize = _GetCanvasSize();
                SKRect bounds = new SKRect(0, 0, canvasSize.Width, canvasSize.Height);

                float xRatio = size.Width / bounds.Width;
                float yRatio = size.Height / bounds.Height;

                float ratio = Math.Min(xRatio, yRatio);

                canvas.Scale(ratio);
                canvas.DrawPicture(_svg.Picture);
                canvas.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVGIconImage] ToSKBitmap threw: {ex}");
            }

            return bitmap;
        }

        /// <summary>
        /// Converts the IconImage to a bitmap at the size specified by the IconImage's Size property.
        /// </summary>
        /// <returns>SKBitmap</returns>
        public override SKBitmap ToSKBitmap()
        {
            return ToSKBitmap(Size);
        }

        // The SVG's natural bounds, taken from the parsed picture's cull rect
        // (Svg.Skia has no direct CanvasSize equivalent to the old SkiaSharp.Svg API).
        private SKSize _GetCanvasSize()
        {
            var cullRect = _svg?.Picture?.CullRect ?? SKRect.Empty;
            return new SKSize(cullRect.Width, cullRect.Height);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            _svg?.Dispose();
            _svg = null;

            base.Dispose();
        }
    }
}
