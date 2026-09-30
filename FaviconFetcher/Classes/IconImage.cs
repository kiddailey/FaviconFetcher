using FaviconFetcher.Classes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FaviconFetcher
{
    public abstract class IconImage : IIconImage
    {
        public IconSize Size { get; set; }

        public IconImage()
        {
            Size = IconSize.Empty;
        }

        /// <summary>
        /// Creates an IconImage from a bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap data for the IconImage</param>
        /// <param name="size">The size of the returned IconImage. If not provided, the 
        /// images original dimensions will be used.</param>
        /// <returns>IconImage</returns>
        public virtual IconImage FromSKBitmap(SKBitmap bitmap, IconSize size = null)
        {
            // Implemented as static method on inherited class
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an IconImage from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the image data from.</param>
        /// <param name="size">The size of the returned IconImage. If not provided, the 
        /// images original dimensions will be used.</param>
        /// <returns>SVGIconImage</returns>
        public virtual IconImage FromStream(Stream stream, IconSize size = null)
        {
            // Implemented as static method on inherited class
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an IconImage from a file.
        /// </summary>
        /// <param name="filename">The full file path and name of the data to be read.</param>
        /// <param name="size">The size of the returned IconImage. If not provided, the 
        /// images original dimensions will be used.</param>
        /// <returns>IconImage</returns>
        public virtual IconImage FromFile(string filename, IconSize size = null)
        {
            // Implemented as static method on inherited class
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves IconImage to disk in PNG form.
        /// </summary>
        /// <param name="filename">The full path and filename to write the IconImage to.</param>
        /// <param name="size">The size of the output IconImage.  Icon will be scaled proportionally
        /// to fit.  If no size is specified, the icon will be written at its original size.</param>
        /// <returns>True if the save was successfull and the file exists.</returns>
        public abstract bool Save(string filename, IconSize outputSize = null);

        /// <summary>
        /// Converts the IconImage to a bitmap at the size specified by the IconImage's Size property.
        /// </summary>
        /// <returns>SKBitmap</returns>
        public abstract SKBitmap ToSKBitmap();

        /// <summary>
        /// Disposes the IconImage
        /// </summary>
        public virtual void Dispose()
        {
            Size = null;
        }
    }
}
