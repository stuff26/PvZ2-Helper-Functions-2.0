using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace XflComponents
{
    public sealed class Bitmap(string bitmapDir, string name) : IDisposable
    {
        public Image Image = Image.Load(bitmapDir);
        public string Name = Path.ChangeExtension(name, null); // Should not include .png at the end

        public int Width => Image.Width;
        public int Height => Image.Height;
        public (int width, int height) Size => (Width, Height);

        /// <summary>
        /// Get the end file name of the bitmap
        /// </summary>
        /// <param name="addFileType">If true, `.png` will be added to the end of the file name</param>
        /// <returns>The file name of the bitmap</returns>
        public string GetFileName(bool addFileType = false) => Path.GetFileName(Name) + 
                                                               (addFileType ? ".png" : string.Empty);
        
        /// <summary>
        /// Get the png file size of the bitmap
        /// </summary>
        /// <returns>A long representing the file size of the bitmap</returns>
        public long GetFileSize()
        {
            using MemoryStream ms = new();
            Image.Save(ms, new PngEncoder());
            return ms.Length;
        }

        /// <summary>
        /// Saves the bitmap to an external path
        /// </summary>
        /// <param name="directory">File path to save to, should not contain `.png`</param>
        public void SaveFile(string path)
        {
            path = Path.ChangeExtension(path, "png");
            Image.Save(path);
        }

        /// <summary>
        /// Adds a prefix to the file name of the bitmap and sets `Name` equal to it
        /// </summary>
        /// <param name="prefix">Prefix to add</param>
        /// <returns>A string being the new bitmap name</returns>
        public string AddPrefix(string prefix)
        {
            var parentDir = Path.GetDirectoryName(Name);
            var fileName = Path.GetFileName(Name);
            Name = Path.Join(parentDir, $"{prefix}{fileName}");
            return Name;
        }
        
        /// <summary>
        /// Gets string representation of the bitmap, with its name and size
        /// </summary>
        /// <returns>A string with basic details of bitmap</returns>
        public override string ToString()
        {
            return $"{Width}x{Height} bitmap named {Name}";
        }

        /// <summary>
        /// Removes current image
        /// </summary>
        public void Dispose()
        {
            Image.Dispose();
        }
    }
}