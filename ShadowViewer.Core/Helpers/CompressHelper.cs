using Aspose.Zip;
using Aspose.Zip.SevenZip;

namespace ShadowViewer.Helpers
{
    public static class CompressHelper
    {
        public static void DeCompress(string zip, string destinationDirectory)
        {
            if (zip.EndsWith(".zip"))
            {
                ZipDeCompress(zip,destinationDirectory);
            }else if (zip.EndsWith(".7z"))
            {
                SevenZipDeCompress(zip,destinationDirectory);
            }

        }
        public static void SevenZipDeCompress(string zip, string destinationDirectory)
        {
            using (SevenZipArchive archive = new SevenZipArchive(zip))
            {
                archive.ExtractToDirectory(destinationDirectory);
            }
        }
        public static void ZipDeCompress(string zip, string destinationDirectory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (FileStream zipFile = File.Open(zip, FileMode.Open))
            {
                using (var archive = new  Archive(zipFile))
                {
                    archive.ExtractToDirectory(destinationDirectory);
                }
            }
        }
    }
}
