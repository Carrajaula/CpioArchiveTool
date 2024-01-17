using System;

namespace CpioArchive
{
    class Program
    {
        static void Main(string[] args)
        {
            //string sourceDirectory = "C:\\Users\\Tadeas\\cpioArchiveTest";
            //string archivePath = "C:\\Users\\Tadeas\\cpioArchiveTestarchive.cpio";
            //string targetDirectory = "C:\\Users\\Tadeas\\cpioArchiveTest\\result"; // Update the target directory path

            string archivePath = "D:\\CpioArchiveLibrary\\archive.cpio";
            //string sourceDirectory = "D:\\CpioArchiveLibrary";
            string targetDirectory = "D:\\CpioArchiveLibrary\\result";

            var archiver = new CpioArchiver();

            // Create the archive
            //archiver.CreateArchive(sourceDirectory, archivePath);
            //Console.WriteLine("Archive created successfully.");

            // Extract the archive
            archiver.ExtractArchive(archivePath, targetDirectory);
            Console.WriteLine("Archive extracted successfully.");

            // Verify the extracted files
            //archiver.VerifyFiles(sourceDirectory, targetDirectory);

            //string filePath = "/Users/tadeastoth/cpioArchive/archive.cpio";

            //if (archiver.IsCpioHeaderValid(filePath))
            //{
            //    Console.WriteLine("The file has a valid .cpio old binary header format.");
            //}
            //else
            //{
            //    Console.WriteLine("The file does not have a valid .cpio old binary header format.");
            //}

            Console.ReadKey();
        }
    }
}
