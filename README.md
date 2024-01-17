# CpioArchiveTool
A .NET library for creating and extracting CPIO archives. It provides an efficient way to handle CPIO file formats, offering methods for archiving multiple files into a single CPIO file and extracting them back. This tool is essential for anyone working with CPIO archives in .NET environments.

## Installation
To use CpioArchiveTool in your project, include the DLL in your project and ensure it is properly referenced in your project file.

## Usage
Here is a basic example of how to use CpioArchiveTool:

```csharp
var archiver = new CpioArchiver();
string archivePath = "path/to/your/archive.cpio";
string targetDirectory = "path/to/target/directory";

// To create an archive
// archiver.CreateArchive(sourceDirectory, archivePath);

// To extract an archive
archiver.ExtractArchive(archivePath, targetDirectory);
```

## Contributing
Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
