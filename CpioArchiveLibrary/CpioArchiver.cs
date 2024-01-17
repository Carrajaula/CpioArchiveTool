using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;

namespace CpioArchive
{
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Provides methods for creating, extracting, and verifying CPIO archives.
    /// </summary>
    public class CpioArchiver : ICpioArchiver
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Fields
        private const ushort CpioMagic = 0x71c7; // Magic number for old binary cpio format

        private Dictionary<string, string> extractedFiles = new Dictionary<string, string>();
        private List<string> tempFilePaths = new List<string>();
        private string pathWithoutFiles;

        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from a list of files.
        /// </summary>
        /// <param name="files">The list of file paths to archive.</param>
        /// <param name="archivePath">The path to the output CPIO archive file.</param>
        public void CreateArchive(List<string> files, string archivePath, bool useFileNameAsPath)
        {
            using (var writer = new BinaryWriter(System.IO.File.OpenWrite(archivePath)))
            {
                foreach (var filePath in files)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileMode = GetFileMode(fileInfo);
                    var fileSize = (uint)fileInfo.Length;
                    var mtime = GetUnixTimestamp(fileInfo.LastWriteTimeUtc);

                    string fileName;
                    if (useFileNameAsPath)
                    {
                        fileName = Path.GetFileName(filePath);
                    }
                    else
                    {
                        fileName = filePath;
                    }

                    var pathBytes = Encoding.ASCII.GetBytes(fileName);
                    var namesize = (ushort)(pathBytes.Length + 1); //necessary trailing NUL byte

                    writer.Write((ushort)0x71C7); // c_magic
                    writer.Write((ushort)0); // c_dev
                    writer.Write((ushort)0); // c_ino
                    writer.Write((ushort)0x81ED); // c_mode (regular file with default permissions)
                    writer.Write((ushort)0); // c_uid
                    writer.Write((ushort)0); // c_gid
                    writer.Write((ushort)0); // c_nlink
                    writer.Write((ushort)0); // c_rdev
                    writer.Write((ushort)(mtime >> 16)); // c_mtime[0]
                    writer.Write((ushort)(mtime & 0xFFFF)); // c_mtime[1]
                    writer.Write(namesize); // c_namesize
                    writer.Write((ushort)((fileSize) >> 16)); // c_filesize[0]
                    writer.Write((ushort)((fileSize) & 0xFFFF)); // c_filesize[1]

                    writer.Write(pathBytes);
                    writer.Write((byte)0); // Null-terminate the pathname
                    if (namesize % 2 == 1)
                        writer.Write((byte)0); // Add padding byte if necessary

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(writer.BaseStream);
                    }
                    if (fileSize % 2 == 1)
                        writer.Write((byte)0); // Add padding byte if necessary
                }

                // Write the trailer record
                writer.Write((ushort)(0x71C7)); // c_magic (trailer magic number)
                writer.Write((ushort)0); // c_dev
                writer.Write((ushort)0); // c_ino
                writer.Write((ushort)0); // c_mode
                writer.Write((ushort)0); // c_uid
                writer.Write((ushort)0); // c_gid
                writer.Write((ushort)0); // c_nlink
                writer.Write((ushort)0); // c_rdev
                writer.Write((ushort)0); // c_mtime[0]
                writer.Write((ushort)0); // c_mtime[1]
                writer.Write((ushort)(11)); // c_namesize
                writer.Write((ushort)0); // c_filesize[0]
                writer.Write((ushort)0); // c_filesize[1]
                writer.Write(Encoding.ASCII.GetBytes("TRAILER!!!"));
                writer.Write((byte)0x00); // Null-terminate the pathname
                if (11 % 2 == 1)
                    writer.Write((byte)0); // Add padding byte if necessary
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from files in a source directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing files to archive.</param>
        /// <param name="archivePath">The path to the output CPIO archive file.</param>
        public void CreateArchive(string sourceDirectory, string archivePath, bool useFileNameAsPath)
        {
            // Implementation of CreateArchive method
            var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            using (var writer = new BinaryWriter(System.IO.File.OpenWrite(archivePath)))
            {
                foreach (var filePath in files)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileMode = GetFileMode(fileInfo);
                    var fileSize = (uint)fileInfo.Length;
                    var mtime = GetUnixTimestamp(fileInfo.LastWriteTimeUtc);

                    string fileName;
                    if (useFileNameAsPath)
                    {
                        fileName = Path.GetFileName(filePath);
                    }
                    else
                    {
                        fileName = filePath;
                    }

                    var pathBytes = Encoding.ASCII.GetBytes(fileName);
                    var namesize = (ushort)(pathBytes.Length + 1); //necessary trailing NUL byte

                    writer.Write((ushort)0x71C7); // c_magic
                    writer.Write((ushort)0); // c_dev
                    writer.Write((ushort)0); // c_ino
                    writer.Write((ushort)0x81ED); // c_mode (regular file with default permissions)
                    writer.Write((ushort)0); // c_uid
                    writer.Write((ushort)0); // c_gid
                    writer.Write((ushort)0); // c_nlink
                    writer.Write((ushort)0); // c_rdev
                    writer.Write((ushort)(mtime >> 16)); // c_mtime[0]
                    writer.Write((ushort)(mtime & 0xFFFF)); // c_mtime[1]
                    writer.Write(namesize); // c_namesize
                    writer.Write((ushort)((fileSize) >> 16)); // c_filesize[0]
                    writer.Write((ushort)((fileSize) & 0xFFFF)); // c_filesize[1]

                    writer.Write(pathBytes);
                    writer.Write((byte)0); // Null-terminate the pathname
                    if (namesize % 2 == 1)
                        writer.Write((byte)0); // Add padding byte if necessary

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(writer.BaseStream);
                    }
                    if (fileSize % 2 == 1)
                        writer.Write((byte)0); // Add padding byte if necessary
                }

                // Write the trailer record
                writer.Write((ushort)(0x71C7)); // c_magic (trailer magic number)
                writer.Write((ushort)0); // c_dev
                writer.Write((ushort)0); // c_ino
                writer.Write((ushort)0); // c_mode
                writer.Write((ushort)0); // c_uid
                writer.Write((ushort)0); // c_gid
                writer.Write((ushort)0); // c_nlink
                writer.Write((ushort)0); // c_rdev
                writer.Write((ushort)0); // c_mtime[0]
                writer.Write((ushort)0); // c_mtime[1]
                writer.Write((ushort)(11)); // c_namesize
                writer.Write((ushort)0); // c_filesize[0]
                writer.Write((ushort)0); // c_filesize[1]
                writer.Write(Encoding.ASCII.GetBytes("TRAILER!!!"));
                writer.Write((byte)0x00); // Null-terminate the pathname
                if (11 % 2 == 1)
                    writer.Write((byte)0); // Add padding byte if necessary
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a CPIO archive from a byte array containing data read from a device in 30-byte chunks.
        /// </summary>
        /// <param name="archivePath">The path where the CPIO archive will be created.</param>
        /// <param name="deviceData">The byte array containing data read from a device in 30-byte chunks.</param>
        public void CreateArchiveFromByteArray(string archivePath, byte[] deviceData)
        {
            using (var writer = new BinaryWriter(System.IO.File.OpenWrite(archivePath)))
            {
                int offset = 0;

                // Iterate over the deviceData array
                while (offset < deviceData.Length)
                {
                    // Extract file information
                    var fileSize = BitConverter.ToUInt32(deviceData, offset + 18); // Assuming the offset for file size is correct
                    var mtime = BitConverter.ToUInt32(deviceData, offset + 8); // Assuming the offset for mtime is correct
                    var pathSize = BitConverter.ToUInt16(deviceData, offset + 16); // Assuming the offset for path size is correct

                    // Write file header
                    WriteFileHeader(writer, mtime, fileSize, deviceData, offset + 30, pathSize);

                    // Find the corresponding chunk in the deviceData array
                    int chunkSize = Math.Min(30, deviceData.Length - (offset + 30 + pathSize)); // Adjusting the chunkSize calculation
                    writer.Write(deviceData, offset + 30 + pathSize, chunkSize);
                    offset += 30 + pathSize + chunkSize; // Adjusting the offset calculation

                    // Add padding byte if necessary
                    if (fileSize % 2 == 1)
                        writer.Write((byte)0);
                }

                // Write the trailer record
                WriteTrailerRecord(writer);
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts a CPIO archive to a target directory.
        /// </summary>
        /// <param name="archivePath">The path to the CPIO archive file.</param>
        /// <param name="targetDirectory">The target directory for extracting files.</param>
        public void ExtractArchive(string archivePath, string targetDirectory)
        {
            extractedFiles.Clear();
            tempFilePaths.Clear();

            Directory.CreateDirectory(targetDirectory); // Create the target directory outside the loop

            pathWithoutFiles = targetDirectory;

            using (var reader = new BinaryReader(System.IO.File.OpenRead(archivePath)))
            {
                try
                {
                    while (true)
                    {
                        var headerBuffer = reader.ReadBytes(26); // Read the fixed-size part of the header
                        var header = new HeaderOldCpio
                        {
                            C_magic = BitConverter.ToUInt16(headerBuffer, 0),
                            C_dev = BitConverter.ToUInt16(headerBuffer, 2),
                            C_ino = BitConverter.ToUInt16(headerBuffer, 4),
                            C_mode = BitConverter.ToUInt16(headerBuffer, 6),
                            C_uid = BitConverter.ToUInt16(headerBuffer, 8),
                            C_gid = BitConverter.ToUInt16(headerBuffer, 10),
                            C_nlink = BitConverter.ToUInt16(headerBuffer, 12),
                            C_rdev = BitConverter.ToUInt16(headerBuffer, 14),
                            C_mtime = new ushort[2] { BitConverter.ToUInt16(headerBuffer, 16), BitConverter.ToUInt16(headerBuffer, 18) },
                            C_namesize = BitConverter.ToUInt16(headerBuffer, 20),
                            C_filesize = new ushort[2] { BitConverter.ToUInt16(headerBuffer, 22), BitConverter.ToUInt16(headerBuffer, 24) }
                        };

                        // Check if the current and next headers form the trailer
                        if (header.C_ino == 0 && header.C_namesize == 0)
                        {
                            // Found the trailer
                            Console.WriteLine("Trailer found. End of CPIO archive.");
                            break;
                        }

                        Console.WriteLine("Extracting namesize: {0}...", header.C_namesize.ToString());
                        var fileSize = (int)((header.C_filesize[0] << 16) | header.C_filesize[1]);
                        Console.WriteLine("Extracting filesize: {0}...", fileSize.ToString());
                        var pathBytes = reader.ReadBytes(header.C_namesize); // Read the correct number of bytes for the path
                        Console.WriteLine("pathBytes: " + pathBytes);
                        var filePath = CleanseFilePath(Encoding.ASCII.GetString(pathBytes).TrimEnd('\0'));
                        Console.WriteLine("filePath: " + filePath);

                        // Construct the full path where the file should be extracted within the targetDirectory
                        var extractedFilePath = Path.Combine(targetDirectory, Path.GetFileName(filePath));
                        var fileDirectory = Path.GetDirectoryName(extractedFilePath);

                        Console.WriteLine("extractedFilePath: " + extractedFilePath);

                        if (extractedFilePath.Contains("TRAILER!!!"))
                        {
                            Console.WriteLine("Trailer found. End of CPIO archive.");
                            break;
                        }

                        var namePaddingBytes = (header.C_namesize % 2 == 0) ? 0 : (2 - (header.C_namesize % 2));
                        reader.ReadBytes(namePaddingBytes); // Skip padding bytes for the file name

                        using (var fileStream = new FileStream(extractedFilePath, FileMode.Create, FileAccess.Write))
                        {
                            var remainingBytes = fileSize;
                            var bufferSize = 4096;
                            var buffer = new byte[bufferSize];
                            int bytesRead;

                            while (remainingBytes > 0 && (bytesRead = reader.Read(buffer, 0, Math.Min(bufferSize, remainingBytes))) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                                remainingBytes -= bytesRead;
                            }

                            var contentPaddingBytes = (fileSize % 2 == 0) ? 0 : (2 - (fileSize % 2));
                            reader.ReadBytes(contentPaddingBytes); // Skip padding bytes for the file content

                            Console.WriteLine("Extracted file: " + filePath);
                            Console.WriteLine("Path to: " + filePath + " | Extracted to file path: " + extractedFilePath + " | fileDirectory: " + fileDirectory);
                        }
                        extractedFiles.Add(filePath, extractedFilePath);
                        tempFilePaths.Add(extractedFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting archive: {ex.Message}");
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts files from a CPIO archive and writes them to the specified target directory.
        /// The method reads the CPIO archive data from a byte array containing all the data at once.
        /// </summary>
        /// <param name="archiveData">The byte array containing the CPIO archive data.</param>
        /// <param name="targetDirectory">The directory where the extracted files will be written.</param>
        public void ExtractArchive(byte[] archiveData, string targetDirectory)
        {
            extractedFiles.Clear();
            tempFilePaths.Clear();

            Directory.CreateDirectory(targetDirectory); // Create the target directory outside the loop

            pathWithoutFiles = targetDirectory;

            using (var reader = new BinaryReader(new MemoryStream(archiveData)))
            {
                while (true)
                {
                    var header = new HeaderOldCpio();
                    header.C_magic = reader.ReadUInt16();
                    header.C_dev = reader.ReadUInt16();
                    header.C_ino = reader.ReadUInt16();
                    header.C_mode = reader.ReadUInt16();
                    header.C_uid = reader.ReadUInt16();
                    header.C_gid = reader.ReadUInt16();
                    header.C_nlink = reader.ReadUInt16();
                    header.C_rdev = reader.ReadUInt16();
                    header.C_mtime = new ushort[2] { reader.ReadUInt16(), reader.ReadUInt16() };
                    header.C_namesize = reader.ReadUInt16();
                    header.C_filesize = new ushort[2] { reader.ReadUInt16(), reader.ReadUInt16() };

                    // Check if the current and next headers form the trailer
                    if (header.C_ino == 0 && header.C_namesize == 0)
                    {
                        // Found the trailer
                        Console.WriteLine("Trailer found. End of CPIO archive.");
                        break;
                    }

                    Console.WriteLine("Extracting namesize: {0}...", header.C_namesize.ToString());
                    var fileSize = (int)((header.C_filesize[0] << 16) | header.C_filesize[1]);
                    Console.WriteLine("Extracting filesize: {0}...", fileSize.ToString());
                    var pathBytes = reader.ReadBytes(header.C_namesize - 1); // Read one byte less
                    Console.WriteLine("pathBytes: " + BitConverter.ToString(pathBytes));
                    var filePath = Encoding.ASCII.GetString(pathBytes).TrimEnd('\0');
                    Console.WriteLine("filePath: " + filePath);

                    // Construct the full path where the file should be extracted within the targetDirectory
                    var extractedFilePath = Path.Combine(targetDirectory, Path.GetFileName(filePath));
                    var fileDirectory = Path.GetDirectoryName(extractedFilePath);

                    Console.WriteLine("extractedFilePath: " + extractedFilePath);

                    if (extractedFilePath.Contains("TRAILER!!!"))
                    {
                        Console.WriteLine("Trailer found. End of CPIO archive.");
                        break;
                    }

                    if (extractedFilePath.Contains(".txt"))
                    {
                        var paddingBytesToSkip = (2 - (header.C_namesize + 1) % 2) % 2;

                        reader.ReadBytes(paddingBytesToSkip); // Skip padding bytes

                        using (var fileStream = new FileStream(extractedFilePath, FileMode.Create, FileAccess.Write))
                        {
                            var remainingBytes = fileSize;
                            var bufferSize = 4096;
                            var buffer = new byte[bufferSize];
                            int bytesRead;

                            while (remainingBytes > 0 && (bytesRead = reader.Read(buffer, 0, Math.Min(bufferSize, remainingBytes))) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                                remainingBytes -= bytesRead;
                            }

                            // Skip padding bytes
                            var paddingBytes = (2 - (fileSize - 1 + 1) % 2) % 2; // Subtract 1 from fileSize
                            reader.ReadBytes(paddingBytes);

                            Console.WriteLine("Extracted file: " + filePath);
                            Console.WriteLine("Path to: " + filePath + " | Extracted to file path: " + extractedFilePath + " | fileDirectory: " + fileDirectory);
                        }
                        extractedFiles.Add(filePath, extractedFilePath);
                        tempFilePaths.Add(extractedFilePath);
                    }
                }
            }
        }



        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if a file contains a valid CPIO header.
        /// </summary>
        /// <param name="filePath">The path to the file to check.</param>
        /// <returns>True if the file has a valid CPIO header; otherwise, false.</returns>
        public bool IsCpioHeaderValid(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileStream.Length < Marshal.SizeOf<HeaderOldCpio>())
                        return false;

                    // Read the header data directly from the file
                    var header = new HeaderOldCpio();
                    using (var reader = new BinaryReader(fileStream))
                    {
                        header.C_magic = reader.ReadUInt16();
                        header.C_dev = reader.ReadUInt16();
                        header.C_ino = reader.ReadUInt16();
                        header.C_mode = reader.ReadUInt16();
                        header.C_uid = reader.ReadUInt16();
                        header.C_gid = reader.ReadUInt16();
                        header.C_nlink = reader.ReadUInt16();
                        header.C_rdev = reader.ReadUInt16();

                        // Initialize the C_mtime array before accessing its elements
                        header.C_mtime = new ushort[2];
                        header.C_mtime[0] = reader.ReadUInt16();
                        header.C_mtime[1] = reader.ReadUInt16();

                        header.C_namesize = reader.ReadUInt16();

                        // Initialize the C_filesize array before accessing its elements
                        header.C_filesize = new ushort[2];
                        header.C_filesize[0] = reader.ReadUInt16();
                        header.C_filesize[1] = reader.ReadUInt16();
                    }

                    return header.C_magic == CpioMagic;
                }
            }
            catch (System.AccessViolationException ex)
            {
                // Handle the exception or log it, and return false or take appropriate action.
                Console.WriteLine("AccessViolationException: " + ex.Message);
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Verifies the integrity of extracted files by comparing them to source files.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing original files.</param>
        /// <param name="targetDirectory">The target directory containing extracted files.</param>
        /// <param name="nestedSearch">Might result in wrong verification if nested files are found and counted. Default is false</param>
        public void VerifyFiles(string sourceDirectory, string targetDirectory, bool nestedSearch = false)
        {
            // Implementation of VerifyFiles method
            string[] sourceFiles;
            string[] targetFiles;
            if (nestedSearch)
            {
                sourceFiles = Directory.GetFiles(sourceDirectory, "text*.txt", SearchOption.AllDirectories);
                targetFiles = Directory.GetFiles(targetDirectory, "text*.txt", SearchOption.AllDirectories);
            }
            else
            {
                sourceFiles = Directory.GetFiles(sourceDirectory, "text*.txt", SearchOption.TopDirectoryOnly);
                targetFiles = Directory.GetFiles(targetDirectory, "text*.txt", SearchOption.TopDirectoryOnly);
            }

            var sourceFileNames = sourceFiles.Select(filePath => Path.GetFileName(filePath)).OrderBy(name => name).ToList();
            var targetFileNames = targetFiles.Select(filePath => Path.GetFileName(filePath)).OrderBy(name => name).ToList();

            Console.WriteLine("Verifying extracted files...");

            if (sourceFileNames.Count != targetFileNames.Count)
            {
                Console.WriteLine("Error: Number of files in the source and target directories do not match.");
                return;
            }

            for (int i = 0; i < sourceFileNames.Count; i++)
            {
                if (sourceFileNames[i] != targetFileNames[i])
                {
                    Console.WriteLine($"Error: File name mismatch - {sourceFileNames[i]}");
                    return;
                }

                var sourceFilePath = Path.Combine(sourceDirectory, sourceFileNames[i]);
                var targetFilePath = Path.Combine(targetDirectory, targetFileNames[i]);

                var sourceFileData = System.IO.File.ReadAllBytes(sourceFilePath);
                var targetFileData = System.IO.File.ReadAllBytes(targetFilePath);

                if (!sourceFileData.SequenceEqual(targetFileData))
                {
                    Console.WriteLine($"Error: File content mismatch - {sourceFileNames[i]}");
                    return;
                }
            }

            Console.WriteLine("All files verified successfully.");
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts a CPIO archive from a byte array.
        /// </summary>
        /// <param name="archiveData">Byte array containing the CPIO archive.</param>
        public void ExtractArchive(byte[] archiveData)
        {
            // Create a temporary file to store the archive data
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cpio");

            try
            {
                // Write the archive data to the temporary file
                System.IO.File.WriteAllBytes(tempFilePath, archiveData);

                // Call the existing ExtractArchive method that works with file paths
                ExtractArchive(tempFilePath);
            }
            finally
            {
                // Delete the temporary file after extraction
                System.IO.File.Delete(tempFilePath);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts files from a CPIO archive located at the specified source path.
        /// Parses headers, retrieves file content, and stores it in a dictionary.
        /// Saves the paths of the extracted temporary files for later deletion.
        /// </summary>
        /// <param name="sourcePath">Path to the CPIO archive file.</param>
        public void ExtractArchive(string sourcePath)
        {
            // Clear the dictionaries before extraction
            extractedFiles.Clear();
            tempFilePaths.Clear();

            // Create a temporary folder to store extracted files
            string tempFolder = pathWithoutFiles = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);

            using (var reader = new BinaryReader(System.IO.File.OpenRead(sourcePath)))
            {
                try
                {
                    while (true)
                    {
                        var headerBuffer = reader.ReadBytes(26); // Read the fixed-size part of the header
                        var header = new HeaderOldCpio
                        {
                            C_magic = BitConverter.ToUInt16(headerBuffer, 0),
                            C_dev = BitConverter.ToUInt16(headerBuffer, 2),
                            C_ino = BitConverter.ToUInt16(headerBuffer, 4),
                            C_mode = BitConverter.ToUInt16(headerBuffer, 6),
                            C_uid = BitConverter.ToUInt16(headerBuffer, 8),
                            C_gid = BitConverter.ToUInt16(headerBuffer, 10),
                            C_nlink = BitConverter.ToUInt16(headerBuffer, 12),
                            C_rdev = BitConverter.ToUInt16(headerBuffer, 14),
                            C_mtime = new ushort[2] { BitConverter.ToUInt16(headerBuffer, 16), BitConverter.ToUInt16(headerBuffer, 18) },
                            C_namesize = BitConverter.ToUInt16(headerBuffer, 20),
                            C_filesize = new ushort[2] { BitConverter.ToUInt16(headerBuffer, 22), BitConverter.ToUInt16(headerBuffer, 24) }
                        };

                        // Check if the current and next headers form the trailer
                        if (header.C_ino == 0 && header.C_namesize == 0)
                        {
                            // Found the trailer
                            Console.WriteLine("Trailer found. End of CPIO archive.");
                            break;
                        }

                        Console.WriteLine("Extracting namesize: {0}...", header.C_namesize.ToString());
                        var fileSize = (int)((header.C_filesize[0] << 16) | header.C_filesize[1]);
                        Console.WriteLine("Extracting filesize: {0}...", fileSize.ToString());
                        var pathBytes = reader.ReadBytes(header.C_namesize); // Read the correct number of bytes for the path
                        Console.WriteLine("pathBytes: " + pathBytes);
                        var filePath = CleanseFilePath(Encoding.ASCII.GetString(pathBytes).TrimEnd('\0'));
                        Console.WriteLine("filePath: " + filePath);

                        // Construct the full path where the file should be extracted within the targetDirectory
                        var extractedFilePath = Path.Combine(tempFolder, Path.GetFileName(filePath));
                        var fileDirectory = Path.GetDirectoryName(extractedFilePath);

                        Console.WriteLine("extractedFilePath: " + extractedFilePath);

                        if (extractedFilePath.Contains("TRAILER!!!"))
                        {
                            Console.WriteLine("Trailer found. End of CPIO archive.");
                            break;
                        }

                        var namePaddingBytes = (header.C_namesize % 2 == 0) ? 0 : (2 - (header.C_namesize % 2));
                        reader.ReadBytes(namePaddingBytes); // Skip padding bytes for the file name

                        using (var fileStream = new FileStream(extractedFilePath, FileMode.Create, FileAccess.Write))
                        {
                            var remainingBytes = fileSize;
                            var bufferSize = 4096;
                            var buffer = new byte[bufferSize];
                            int bytesRead;

                            while (remainingBytes > 0 && (bytesRead = reader.Read(buffer, 0, Math.Min(bufferSize, remainingBytes))) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                                remainingBytes -= bytesRead;
                            }

                            var contentPaddingBytes = (fileSize % 2 == 0) ? 0 : (2 - (fileSize % 2));
                            reader.ReadBytes(contentPaddingBytes); // Skip padding bytes for the file content

                            Console.WriteLine("Extracted file: " + filePath);
                            Console.WriteLine("Path to: " + filePath + " | Extracted to file path: " + extractedFilePath + " | fileDirectory: " + fileDirectory);

                            // Store file path in dictionaries
                            extractedFiles.Add(filePath, extractedFilePath);
                            tempFilePaths.Add(extractedFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting archive: {ex.Message}");
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the temporary files and folder created during the extraction process.
        /// </summary>
        ///     private void DeleteTemporaryFolder(string folderPath)
        public void DeleteTemporaryFolder(string folderPath)
        {
            try
            {
                Directory.Delete(folderPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting temporary folder: {ex.Message}");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cleanses the file path by replacing or removing invalid characters.
        /// </summary>
        /// <param name="filePath">The original file path.</param>
        /// <returns>The cleansed file path.</returns>
        private string CleanseFilePath(string filePath)
        {
            // Replace or remove invalid characters from the file path
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidChar in invalidChars)
            {
                filePath = filePath.Replace(invalidChar.ToString(), string.Empty);
            }

            return filePath;
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the dictionary containing original file paths and their corresponding extracted file paths.
        /// </summary>
        private Dictionary<string, string> ExtractedFiles
        {
            get { return extractedFiles; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the list of temporary file paths where the extracted files are stored.
        /// </summary>
        public List<string> TempFilePaths
        {
            get { return tempFilePaths; }
            set { tempFilePaths = value; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the dictionary containing original file paths and their corresponding extracted file paths.
        /// </summary>
        public Dictionary<string, string> GetExtractedFiles()
        {
            return extractedFiles;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the list of temporary file paths where the extracted files are stored.
        /// </summary>
        public List<string> GetTempFilePaths()
        {
            return tempFilePaths;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the temporary folder path where the extracted files are stored.
        /// </summary>
        public string GetTempFolderPath()
        {
            return pathWithoutFiles;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the file mode for a FileInfo object.
        /// </summary>
        /// <param name="fileInfo">The FileInfo object representing the file.</param>
        /// <returns>The file mode as a ushort value.</returns>
        private ushort GetFileMode(FileInfo fileInfo)
        {
            // Implementation of GetFileMode method
            var mode = (ushort)fileInfo.Attributes;

            // Convert file attributes to octal number
            var octalMode = Convert.ToString(mode, 8);
            mode = Convert.ToUInt16(octalMode, 8);

            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                mode |= 0x4000; // Directory bit

            return mode;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a DateTime object to a Unix timestamp.
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>The Unix timestamp as a uint value.</returns>
        private uint GetUnixTimestamp(DateTime dateTime)
        {
            // Implementation of GetUnixTimestamp method
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = dateTime - epoch;
            return (uint)timeSpan.TotalSeconds;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Represents the header of an old CPIO archive.
        /// </summary>
        private struct HeaderOldCpio
        {
            // Definition of HeaderOldCpio structure
            public ushort C_magic;
            public ushort C_dev;
            public ushort C_ino;
            public ushort C_mode;
            public ushort C_uid;
            public ushort C_gid;
            public ushort C_nlink;
            public ushort C_rdev;
            public ushort[] C_mtime; // Changed to ushort array
            public ushort C_namesize;
            public ushort[] C_filesize; // Changed to ushort array
        }

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
        public void WriteFileHeader(BinaryWriter writer, uint mtime, uint fileSize, byte[] deviceData, int pathOffset, ushort pathSize)
        {
            // CPIO file header structure
            writer.Write((ushort)0x71C7);                        // c_magic
            writer.Write((ushort)0);                             // c_dev
            writer.Write((ushort)0);                             // c_ino
            writer.Write((ushort)0x81ED);                        // c_mode (regular file with default permissions)
            writer.Write((ushort)0);                             // c_uid
            writer.Write((ushort)0);                             // c_gid
            writer.Write((ushort)0);                             // c_nlink
            writer.Write((ushort)0);                             // c_rdev
            writer.Write((ushort)(mtime >> 16));                 // c_mtime[0]
            writer.Write((ushort)(mtime & 0xFFFF));              // c_mtime[1]
            writer.Write((ushort)(pathSize + 1));                // c_namesize
            writer.Write((ushort)(fileSize >> 16));              // c_filesize[0]
            writer.Write((ushort)(fileSize & 0xFFFF));           // c_filesize[1]

            // Write the pathBytes
            writer.Write(deviceData, pathOffset, pathSize);

            // Add padding byte if necessary
            if (pathSize % 2 == 1)
                writer.Write((byte)0);
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Writes the trailer record at the end of the CPIO archive, indicating the end of the archive.
        /// </summary>
        /// <param name="writer">The BinaryWriter instance used to write to the CPIO archive.</param>
        public void WriteTrailerRecord(BinaryWriter writer)
        {
            // CPIO trailer record structure
            writer.Write((ushort)0x71C7);                   // c_magic (trailer magic number)
            writer.Write((ushort)0);                        // c_dev
            writer.Write((ushort)0);                        // c_ino
            writer.Write((ushort)0);                        // c_mode
            writer.Write((ushort)0);                        // c_uid
            writer.Write((ushort)0);                        // c_gid
            writer.Write((ushort)0);                        // c_nlink
            writer.Write((ushort)0);                        // c_rdev
            writer.Write((ushort)0);                        // c_mtime[0]
            writer.Write((ushort)0);                        // c_mtime[1]
            writer.Write((ushort)11);                       // c_namesize
            writer.Write((ushort)0);                        // c_filesize[0]
            writer.Write((ushort)0);                        // c_filesize[1]

            // Write the "TRAILER!!!" string
            writer.Write(Encoding.ASCII.GetBytes("TRAILER!!!"));

            // Null-terminate the pathname
            writer.Write((byte)0x00);

            // Add padding byte if necessary
            if (11 % 2 == 1)
                writer.Write((byte)0);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the content of a file from in-memory storage based on the file path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The content of the file, or null if not found.</returns>
        public string GetFileContentFromMemory(string filePath)
        {
            if (extractedFiles.TryGetValue(filePath, out var fileContent))
            {
                return fileContent;
            }

            return null;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the given byte array represents a valid CPIO archive.
        /// </summary>
        /// <param name="archiveData">The byte array representing the CPIO archive.</param>
        /// <returns>True if the data is a valid CPIO archive, otherwise false.</returns>
        public bool IsCpioDataValid(byte[] archiveData)
        {
            // Ensure that the byte array is not null and has a minimum length for CPIO magic number
            if (archiveData == null || archiveData.Length < 6)
            {
                return false;
            }

            // Check for the CPIO magic number (c_magic)
            ushort magicNumber = BitConverter.ToUInt16(archiveData, 0);
            return magicNumber == 0x71C7; // Replace with the actual CPIO magic number
        }

        private bool IsTextFile(byte[] buffer)
        {
            try
            {
                // Check for BOM or recognizable text pattern
                return IsBom(buffer) || ContainsTextPattern(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if file is a text file: {ex.Message}");
                return false;
            }
        }

        private bool IsBom(byte[] buffer)
        {
            // Check for UTF-8, UTF-16, or UTF-32 BOM
            return buffer.Length >= 3 && (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF) ||
                   (buffer.Length >= 2 && (buffer[0] == 0xFE && buffer[1] == 0xFF || buffer[0] == 0xFF && buffer[1] == 0xFE)) ||
                   (buffer.Length >= 4 && buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF);
        }

        private bool ContainsTextPattern(byte[] buffer)
        {
            // Check for printable ASCII characters
            foreach (var b in buffer)
            {
                if (!(b >= 32 && b <= 126))
                {
                    return false; // Non-printable ASCII character found
                }
            }

            return true; // All characters in the buffer are printable ASCII
        }
        #endregion
    }
}
