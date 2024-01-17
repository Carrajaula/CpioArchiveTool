using System;
using System.Collections.Generic;
using System.IO;

namespace CpioArchive
{
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Represents an interface for creating, extracting, and verifying CPIO archives.
    /// </summary>
    public interface ICpioArchiver
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from the specified source directory and saves it to the given archive path.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing files to be archived.</param>
        /// <param name="archivePath">The path where the CPIO archive will be saved.</param>
        /// <param name="useFileNameAsPath">Bool value whether to use the fileName as path (true) or the full path (false)</param>
        void CreateArchive(string sourceDirectory, string archivePath, bool useFileNameAsPath);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts a CPIO archive from the specified archive path and saves its contents to the target directory.
        /// </summary>
        /// <param name="archivePath">The path to the CPIO archive to be extracted.</param>
        /// <param name="targetDirectory">The target directory where the contents will be extracted.</param>
        void ExtractArchive(string archivePath, string targetDirectory);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the CPIO header in the specified file is valid.
        /// </summary>
        /// <param name="filePath">The path to the file containing the CPIO header.</param>
        /// <returns><c>true</c> if the CPIO header is valid; otherwise, <c>false</c>.</returns>
        bool IsCpioHeaderValid(string filePath);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Verifies files between the source directory and the target directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing the original files.</param>
        /// <param name="targetDirectory">The target directory containing the extracted files to be verified.</param>
        /// <param name="nestedSearch">Might result in wrong verification if nested files are found and counted.</param>
        void VerifyFiles(string sourceDirectory, string targetDirectory, bool nestedSearch = false);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from a list of files.
        /// </summary>
        /// <param name="files">The list of file paths to archive.</param>
        /// <param name="archivePath">The path to the output CPIO archive file.</param>
        /// <param name="useFileNameAsPath">Bool value whether to use the fileName as path (true) or the full path (false)</param>
        void CreateArchive(List<string> files, string archivePath, bool useFileNameAsPath);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from a byte array containing data read from a device in 30-byte chunks.
        /// </summary>
        /// <param name="archivePath">The path where the CPIO archive will be created.</param>
        /// <param name="deviceData">The byte array containing data read from a device in 30-byte chunks.</param>
        void CreateArchiveFromByteArray(string archivePath, byte[] deviceData);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts files from a CPIO archive and writes them to the specified target directory.
        /// The method reads the CPIO archive data from a byte array containing all the data at once.
        /// </summary>
        /// <param name="archiveData">The byte array containing the CPIO archive data.</param>
        /// <param name="targetDirectory">The directory where the extracted files will be written.</param>
        void ExtractArchive(byte[] archiveData, string targetDirectory);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts a CPIO archive from a byte array.
        /// </summary>
        /// <param name="archiveData">Byte array containing the CPIO archive.</param>
        void ExtractArchive(byte[] archiveData);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts files from a CPIO archive located at the specified source path.
        /// Parses headers, retrieves file content, and stores it in a dictionary.
        /// Saves the paths of the extracted temporary files for later deletion.
        /// </summary>
        /// <param name="sourcePath">Path to the CPIO archive file.</param>
        void ExtractArchive(string sourcePath);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Writes the header for a file entry in the CPIO archive, including metadata and path information.
        /// </summary>
        /// <param name="writer">The BinaryWriter instance used to write to the CPIO archive.</param>
        /// <param name="mtime">The modified time of the file in Unix timestamp format.</param>
        /// <param name="fileSize">The size of the file in bytes.</param>
        /// <param name="deviceData">The byte array containing data read from a device in 30-byte chunks.</param>
        /// <param name="pathOffset">The offset in the deviceData array where the path starts.</param>
        /// <param name="pathSize">The size of the path in bytes.</param>
        void WriteFileHeader(BinaryWriter writer, uint mtime, uint fileSize, byte[] deviceData, int pathOffset, ushort pathSize);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Writes the trailer record at the end of the CPIO archive, indicating the end of the archive.
        /// </summary>
        /// <param name="writer">The BinaryWriter instance used to write to the CPIO archive.</param>
        void WriteTrailerRecord(BinaryWriter writer);
        /// <summary>
        /// Retrieves the content of a file from in-memory storage based on the file path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The content of the file, or null if not found.</returns>
        string GetFileContentFromMemory(string filePath);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the given byte array represents a valid CPIO archive.
        /// </summary>
        /// <param name="archiveData">The byte array representing the CPIO archive.</param>
        /// <returns>True if the data is a valid CPIO archive, otherwise false.</returns>
        bool IsCpioDataValid(byte[] archiveData);
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the temporary files and folder created during the extraction process.
        /// </summary>
        ///     private void DeleteTemporaryFolder(string folderPath)
        void DeleteTemporaryFolder(string folderPath);
        /// <summary>
        /// Gets the dictionary containing original file paths and their corresponding extracted file paths.
        /// </summary>
        Dictionary<string, string> GetExtractedFiles();

        /// <summary>
        /// Gets the list of temporary file paths where the extracted files are stored.
        /// </summary>
        List<string> GetTempFilePaths();
        /// <summary>
        /// Gets the temporary folder path where the extracted files are stored.
        /// </summary>
        string GetTempFolderPath();
        /// <summary>
        /// Gets or Sets the list of temporary file paths where the extracted files are stored.
        /// </summary>
        List<string> TempFilePaths { get; set; }
    }
}