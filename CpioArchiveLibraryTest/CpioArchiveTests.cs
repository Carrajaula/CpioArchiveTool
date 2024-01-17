using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CpioArchive;

namespace CpioArchiveTests
{
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Contains unit tests for the <see cref="CpioArchiver"/> class.
    /// </summary>
    [TestFixture]
    public class CpioArchiverTests
    {
        private string _testDirectory;
        private string _archivePath;
        private string _extractedDirectory;

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes test directories and creates test files.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "CpioArchiverTest");
            _archivePath = Path.Combine(_testDirectory, "test.cpio");
            _extractedDirectory = Path.Combine(_testDirectory, "extracted");

            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(_extractedDirectory);

            // Create test files
            for (int i = 1; i <= 3; i++)
            {
                File.WriteAllText(Path.Combine(_testDirectory, $"text{i}.txt"), $"Test content {i}");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cleans up test files and directories.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            // Clean up test files and directories
            // Check if directory exists before deleting
            if (Directory.Exists(_extractedDirectory))
            {
                // Clean up test files and directories
                Directory.Delete(_extractedDirectory, true);
            }
            // Check if directory exists before deleting
            if (Directory.Exists(_testDirectory))
            {
                // Clean up test files and directories
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void RealWorldTest()
        {
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
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if a valid CPIO archive can be created.
        /// </summary>
        [Test]
        public void CreateArchive_ValidInput_ArchiveCreated()
        {
            // Arrange
            var archiver = new CpioArchiver();

            // Act
            archiver.CreateArchive(_testDirectory, _archivePath, false);

            // Assert
            Assert.IsTrue(File.Exists(_archivePath));
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if a valid CPIO archive can be extracted.
        /// </summary>
        [Test]
        public void ExtractArchive_ValidArchive_ArchiveExtracted()
        {
            // Arrange
            var archiver = new CpioArchiver();
            archiver.CreateArchive(_testDirectory, _archivePath, false);

            // Act
            archiver.ExtractArchive(_archivePath, _extractedDirectory);

            // Assert
            foreach (var file in Directory.GetFiles(_extractedDirectory))
            {
                var fileName = Path.GetFileName(file);
                var originalFilePath = Path.Combine(_testDirectory, fileName);
                var extractedFilePath = Path.Combine(_extractedDirectory, fileName);

                Assert.IsTrue(File.Exists(originalFilePath));
                Assert.IsTrue(File.Exists(extractedFilePath));

                var originalContent = File.ReadAllText(originalFilePath);
                var extractedContent = File.ReadAllText(extractedFilePath);

                Assert.AreEqual(originalContent, extractedContent);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if a valid CPIO header is correctly identified.
        /// </summary>
        [Test]
        public void IsCpioHeaderValid_ValidHeader_ReturnsTrue()
        {
            // Arrange
            var archiver = new CpioArchiver();
            archiver.CreateArchive(_testDirectory, _archivePath, false);

            // Act
            var isValid = archiver.IsCpioHeaderValid(_archivePath);

            // Assert
            Assert.IsTrue(isValid);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if extracted files are verified successfully.
        /// </summary>
        [Test]
        public void VerifyFiles_ValidFiles_FilesVerifiedSuccessfully()
        {
            // Arrange
            var archiver = new CpioArchiver();
            archiver.CreateArchive(_testDirectory, _archivePath, false);
            archiver.ExtractArchive(_archivePath, _extractedDirectory);

            // Act & Assert
            Assert.DoesNotThrow(() => archiver.VerifyFiles(_testDirectory, _extractedDirectory));
        }

        [Test]
        [Description("Creates and extracts a CPIO archive, then compares the extracted files with the original files.")]
        public void CreateAndExtractArchive_ShouldMatchOriginalFiles()
        {
            // Arrange
            var files = new List<string> { "file1.txt", "file2.txt" };
            var archivePath = "testArchive.cpio";
            var extractedDirectory = "extractedDirectory";

            // Generate deviceData buffer
            var deviceData = GenerateDeviceData(files);

            // Act
            var cpioArchiver = new CpioArchiver();
            cpioArchiver.CreateArchiveFromByteArray(archivePath, deviceData);
            cpioArchiver.ExtractArchive(archivePath, extractedDirectory);

            // Assert
            foreach (var file in files)
            {
                var originalContent = File.ReadAllText(file);
                var extractedFilePath = Path.Combine(extractedDirectory, Path.GetFileName(file));

                // Ensure the extracted file exists
                Assert.IsTrue(File.Exists(extractedFilePath), $"Extracted file not found: {extractedFilePath}");

                // Read the content from the extracted file
                var extractedContent = File.ReadAllText(extractedFilePath);

                // Assert that the content matches the original
                Assert.That(extractedContent, Is.EqualTo(originalContent), $"Content mismatch for file: {file}");
            }
        }

        [Test]
        [Description("Writes the header for a file entry in the CPIO archive and verifies the correctness of the header.")]
        public void WriteFileHeader_ShouldWriteCorrectHeader()
        {
            // Arrange
            using (var memoryStream = new MemoryStream())
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                var archiver = new CpioArchiver();
                var mtime = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var fileSize = 100;
                var path = "testFile.txt";
                var pathBytes = Encoding.ASCII.GetBytes(path);
                var pathSize = (ushort)pathBytes.Length;

                // Create a buffer with enough space for deviceData
                var deviceDataBuffer = new byte[pathSize + 1];  // Adding 1 for potential padding byte

                // Act
                archiver.WriteFileHeader(binaryWriter, mtime, (uint)fileSize, deviceDataBuffer, 0, pathSize);

                // Assert
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // Add your assertions here to verify the correctness of the header
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0x71C7), "c_magic mismatch");
                    // ... (other assertions)

                    // Verify the pathBytes
                    var actualPathBytes = binaryReader.ReadBytes(pathSize);
                    Assert.That(actualPathBytes, Is.EqualTo(pathBytes), "pathBytes mismatch");

                    // Verify padding byte
                    Assert.That(binaryReader.ReadByte(), Is.EqualTo(0), "Padding byte mismatch");
                }
            }
        }

        [Test]
        [Description("Writes the trailer record at the end of the CPIO archive and verifies the correctness of the trailer record.")]
        public void WriteTrailerRecord_ShouldWriteCorrectTrailerRecord()
        {
            // Arrange
            using (var memoryStream = new MemoryStream())
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                var archiver = new CpioArchiver();

                // Act
                archiver.WriteTrailerRecord(binaryWriter);

                // Assert
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0x71C7), "c_magic mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_dev mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_ino mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_mode mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_uid mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_gid mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_nlink mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_rdev mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_mtime[0] mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_mtime[1] mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(11), "c_namesize mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_filesize[0] mismatch");
                    Assert.That(binaryReader.ReadUInt16(), Is.EqualTo(0), "c_filesize[1] mismatch");

                    // Read and verify the "TRAILER!!!" string
                    var trailerBytes = binaryReader.ReadBytes(10);
                    Assert.That(Encoding.ASCII.GetString(trailerBytes), Is.EqualTo("TRAILER!!!"), "Trailer string mismatch");

                    // Verify null-terminated pathname and padding byte
                    Assert.That(binaryReader.ReadByte(), Is.EqualTo(0), "Null-terminate the pathname mismatch");
                    Assert.That(binaryReader.ReadByte(), Is.EqualTo(0), "Padding byte mismatch");
                }
            }
        }


        private byte[] GenerateDeviceData(List<string> files)
        {
            var archiver = new CpioArchiver();
            using (var memoryStream = new MemoryStream())
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                foreach (var file in files)
                {
                    // Add mock file data to the device data
                    var fileSize = (uint)new FileInfo(file).Length;
                    var mtime = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    var pathBytes = Encoding.ASCII.GetBytes(file);
                    var pathSize = (ushort)(pathBytes.Length + 1);

                    binaryWriter.Write((ushort)0x71C7); // c_magic
                    binaryWriter.Write((ushort)0); // c_dev
                    binaryWriter.Write((ushort)0); // c_ino
                    binaryWriter.Write((ushort)0x81ED); // c_mode (regular file with default permissions)
                    binaryWriter.Write((ushort)0); // c_uid
                    binaryWriter.Write((ushort)0); // c_gid
                    binaryWriter.Write((ushort)0); // c_nlink
                    binaryWriter.Write((ushort)0); // c_rdev
                    binaryWriter.Write((ushort)(mtime >> 16)); // c_mtime[0]
                    binaryWriter.Write((ushort)(mtime & 0xFFFF)); // c_mtime[1]
                    binaryWriter.Write(pathSize); // c_namesize
                    binaryWriter.Write((ushort)(fileSize >> 16)); // c_filesize[0]
                    binaryWriter.Write((ushort)(fileSize & 0xFFFF)); // c_filesize[1]

                    binaryWriter.Write(pathBytes);
                    binaryWriter.Write((byte)0); // Null-terminate the pathname

                    // Add padding byte if necessary
                    if (pathSize % 2 == 1)
                        binaryWriter.Write((byte)0);

                    // Add mock file content to the device data
                    var fileContent = Encoding.ASCII.GetBytes($"Content of {file}");
                    binaryWriter.Write(fileContent);

                    // Add padding byte if necessary
                    if (fileSize % 2 == 1)
                        binaryWriter.Write((byte)0);
                }

                // Add trailer record to the device data
                archiver.WriteTrailerRecord(binaryWriter);

                return memoryStream.ToArray();
            }
        }
    }
}
