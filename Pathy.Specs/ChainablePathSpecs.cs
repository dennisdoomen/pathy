using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathSpecs
{
    private static readonly char Slash = Path.DirectorySeparatorChar;
    private readonly ChainablePath testFolder;

    public ChainablePathSpecs()
    {
        testFolder = ChainablePath.Temp / nameof(ChainablePathSpecs) / Environment.Version.ToString();
        testFolder.DeleteFileOrDirectory();
        testFolder.CreateDirectoryRecursively();
    }

    [Fact]
    public void Can_build_from_an_absolute_path()
    {
        // Arrange
        string location = Assembly.GetCallingAssembly()!.Location;

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.DirectoryName.Should().Be(Path.GetDirectoryName(location));
        path.IsRooted.Should().BeTrue();
    }

    [Fact]
    public void Can_build_from_a_string()
    {
        // Arrange
        string location = Assembly.GetCallingAssembly()!.Location;

        // Act
        var path = (ChainablePath)location;

        // Assert
        path.DirectoryName.Should().Be(Path.GetDirectoryName(location));
        path.IsRooted.Should().BeTrue();
    }

    [Theory]
    [InlineData("C:")]
    [InlineData("C:/")]
    [InlineData("c:/")]
    public void Can_build_a_path_from_a_drive_letter(string drive)
    {
        // Act
        var path = ChainablePath.From(drive);

        // Assert
        path.ToString().Should().BeEquivalentTo("C:" + Slash);
        path.IsRooted.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Cannot_build_from_an_invalid_path(string path)
    {
        // Act
        var act = () => ChainablePath.From(path);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void A_trailing_slash_is_fine()
    {
        // Arrange
        string directory = Path.GetDirectoryName(Assembly.GetCallingAssembly()!.Location);
        string directoryWithSlash = directory + Path.DirectorySeparatorChar;

        // Act
        var path = ChainablePath.From(directoryWithSlash);

        // Assert
        path.DirectoryName.Should().Be(Path.GetDirectoryName(directory));
        path.IsRooted.Should().BeTrue();
        path.ToString().Should().EndWith($"{Path.DirectorySeparatorChar}");
    }

    [Fact]
    public void Can_build_from_a_path_with_reverse_traversals()
    {
        // Arrange
        var nestedPath = Directory.CreateDirectory(testFolder.ToString() + "/dir1" + Slash + "dir2" + Slash + "dir3/");

        string location = nestedPath.FullName + "/../../..";

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.ToString().Should().Be(testFolder.ToString().Trim(Slash));
        path.IsRooted.Should().BeTrue();
    }

    [Fact]
    public void Can_build_from_a_relative_path()
    {
        // Act
        var path = ChainablePath.From("temp/somefile.txt");

        // Assert
        path.DirectoryName.Should().Be("temp");
        path.IsRooted.Should().BeFalse();
    }

    [Fact]
    public void Can_convert_the_relative_path_to_an_absolute_path_using_the_current_working_directory()
    {
        // Arrange
        var path = ChainablePath.From("temp/somefile.txt");

        // Act
        var absolutePath = path.ToAbsolute();

        // Assert
        absolutePath.ToString().Should().Be(Path.Combine(Environment.CurrentDirectory, "temp", "somefile.txt"));
    }

    [Fact]
    public void Can_combine_a_relative_path_using_a_specific_absolute_path()
    {
        // Arrange
        var path = ChainablePath.From("temp/somefile.txt");

        // Act
        var absolutePath = path.ToAbsolute(ChainablePath.Temp);

        // Assert
        absolutePath.ToString().Should().Be(Path.Combine(Path.GetTempPath(), "temp", "somefile.txt"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void The_absolute_path_must_be_valid(string absolutePath)
    {
        // Arrange
        var path = ChainablePath.From("temp/somefile.txt");

        // Act
        Action act = () => path.ToAbsolute(absolutePath);

        // Assert
        act.Should().Throw<ArgumentException>("*absolutePath*");
    }

    [Fact]
    public void Can_start_with_an_empty_path()
    {
        // Act
        var path = ChainablePath.New / "c:" / "temp" / "somefile.txt";

        // Assert
        path.ToString().Should().Be("c:" + Slash + "temp" + Slash + "somefile.txt");
    }

    [Fact]
    public void Can_chain_a_relative_path_to_an_absolute_path()
    {
        // Arrange
        var absolutePath = ChainablePath.From(testFolder);
        var relativePath = ChainablePath.From("dir1") / "somefile.txt";

        // Act
        var path = absolutePath / relativePath;

        // Assert
        path.ToString().Should().Be(testFolder + Slash + "dir1" + Slash + "somefile.txt");
    }

    [Fact]
    public void Can_be_assigned_to_a_string()
    {
        // Act
        string result = ChainablePath.From("temp/somefile.txt");

        // Assert
        result.Should().Be("temp" + Slash + "somefile.txt");
    }

    [Fact]
    public void Can_chain_multiple_directories()
    {
        // Arrange
        var temp = testFolder;

        // Act
        var result = temp / "dir1" / "dir2" / "dir3";

        // Assert
        result.DirectoryName.Should().Be(temp + Slash + "dir1" + Slash + "dir2");
        result.Name.Should().Be("dir3");
    }

    [Fact]
    public void Chaining_an_empty_string_does_not_do_anything()
    {
        // Arrange
        var temp = testFolder;

        // Act
        var result = temp / "";

        // Assert
        result.Should().Be(temp);
    }

    [Fact]
    public void Chaining_a_null_is_not_allowed()
    {
        // Act
        Action act = () => _ = testFolder / null!;

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Can_chain_directories_and_files()
    {
        // Act
        var result = testFolder / "dir1" / "dir2" / "dir3" / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(testFolder + Slash + "dir1" + Slash + "dir2" + Slash + "dir3");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Ignores_superfluous_slashes()
    {
        // Act
        var result = testFolder / "dir1" / "dir2/" / "dir3/" / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(testFolder + Slash + "dir1" + Slash + "dir2" + Slash + "dir3");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Can_check_that_a_file_exists()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / ".." / "ChainablePathSpecs.cs";

        // Assert
        path.FileExists.Should().BeTrue();
        path.DirectoryName.Should().EndWith("Pathy.Specs");
    }

    [Fact]
    public void Can_check_that_a_file_does_not_exist()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / ".." / "SomeRandomName.cs";

        // Assert
        path.FileExists.Should().BeFalse();
    }

    [Fact]
    public void A_directory_is_not_a_file()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / "..";

        // Assert
        path.FileExists.Should().BeFalse();
    }

    [Fact]
    public void A_file_is_not_a_directory()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / ".." / "ChainablePathSpecs.cs";

        // Assert
        path.DirectoryExists.Should().BeFalse();
    }

    [Fact]
    public void Can_check_that_a_directory_exists()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath().Parent.Parent.Parent;

        // Assert
        path.DirectoryExists.Should().BeTrue();
    }

    [Fact]
    public void Can_check_that_a_directory_does_not_exist()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / ".." / "SomeRandomDirectory";

        // Assert
        path.FileExists.Should().BeFalse();
    }

    [Fact]
    public void Can_get_the_extension_for_a_file()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / ".." / "ChainablePathSpecs.cs";

        // Assert
        path.Extension.Should().Be(".cs");
        path.DirectoryName.Should().EndWith("Pathy.Specs");
    }

    [Fact]
    public void Can_get_the_root()
    {
        // Act
        var path = testFolder;

        // Assert
        path.Root.ToString().Should().Be(Path.GetPathRoot(path.ToString()));
    }

    [Theory]
    [InlineData("C:\\temp\\")]
    [InlineData("C:\\temp")]
    public void A_trailing_slash_does_not_affect_the_directory(string path)
    {
        // Act
        var result = ChainablePath.From(path);

        // Assert
        result.Directory!.ToString().Should().Be("C:" + Slash);
    }

    [Fact]
    public void The_root_does_not_have_a_parent_directory()
    {
        // Act
        var path = ChainablePath.From("C://");

        // Assert
        path.Directory.Should().Be(ChainablePath.Empty);
        path.Directory.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Can_get_the_current_directory()
    {
        // Act
        var path = ChainablePath.Current;

        // Assert
        path.ToString().Should().Be(Environment.CurrentDirectory);
    }

    [Fact]
    public void A_directory_can_have_an_extension_too()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / "..";

        // Assert
        path.Extension.Should().Be(".Specs");
    }

    [Fact]
    public void Can_add_an_extension()
    {
        // Arrange
        var path = ChainablePath.Temp / "SomeFile";

        // Act
        path += ".txt";

        // Assert
        path.Name.Should().Be("SomeFile.txt");
        path.Extension.Should().Be(".txt");
    }

    [Theory]
    [InlineData(".txt", true)]
    [InlineData(".TXT", true)]
    [InlineData("TXT", true)]
    [InlineData("DOC", false)]
    public void Can_check_for_an_extension(string extension, bool shouldMatch)
    {
        // Act
        var path = ChainablePath.Temp / "SomeFile.txt";

        // Assert
        path.HasExtension(extension).Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Checking_for_an_extension_requires_a_valid_extension(string extension)
    {
        // Arrange
        var path = ChainablePath.Temp / "SomeFile.txt";

        // Act
        Action act = () => path.HasExtension(extension);

        // Assert
        act.Should().Throw<ArgumentException>("*null*empty*");
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Can_get_the_difference_as_a_relative_path()
    {
        // Act
        var basePath = Environment.CurrentDirectory.ToPath() / ".." / ".." / "..";
        var path = basePath / "SomeRandomFileOrDirectory";

        // Assert
        var relativePath = path.AsRelativeTo(basePath);
        relativePath.IsRooted.Should().BeFalse();
        relativePath.Name.Should().Be("SomeRandomFileOrDirectory");
    }

    [Fact]
    public void Can_also_determine_the_relative_path_for_reverse_traversals()
    {
        // Act
        var basePath = Environment.CurrentDirectory.ToPath() / ".." / ".." / "..";
        var path = basePath / "SomeRandomFileOrDirectory";

        // Assert
        var relativePath = basePath.AsRelativeTo(path);
        relativePath.Name.Should().Be("..");
    }
#endif

    [Fact]
    public void Can_find_files_using_globbing_patterns()
    {
        // Arrange
        var temp = ChainablePath.Temp / "dir1" / "dir2" / "dir3" / "file.txt";

        temp.CreateDirectoryRecursively();
        File.WriteAllText(temp / "file.txt", "Hello World!");
        File.WriteAllText(temp / "file2.txt", "Hello World!");
        File.WriteAllText(temp / "file3.doc", "Hello World!");

        // Act
        var files = (ChainablePath.Temp / "dir1").GlobFiles("**/*.txt");

        // Assert
        files.Should().BeEquivalentTo([
            temp / "file.txt",
            temp / "file2.txt"
        ], options => options.ComparingRecordsByValue());
    }

    [Fact]
    public void Can_convert_to_directory_info()
    {
        // Act
        DirectoryInfo directory = ChainablePath.Temp.ToDirectoryInfo();

        // Assert
        directory.ToString().Should().Be(new DirectoryInfo(Path.GetTempPath()).ToString());
    }

    [Fact]
    public void Can_convert_to_file_info()
    {
        // Arrange
        var chainablePath = ChainablePath.Temp / "file.txt";
        File.WriteAllText(chainablePath, "Hello World!");

        // Act
        FileInfo file = chainablePath.ToFileInfo();

        // Assert
        file.ToString().Should().Be(new FileInfo(chainablePath.ToString()).ToString());
    }

    [Fact]
    public void Can_determine_if_a_path_refers_to_a_file()
    {
        // Act
        var path = ChainablePath.Temp / "file.txt";
        File.WriteAllText(path, "Hello World!");

        // Assert
        path.IsFile.Should().BeTrue();
        path.IsDirectory.Should().BeFalse();
    }

    [Fact]
    public void Can_determine_if_a_path_refers_to_a_directory()
    {
        // Act
        var path = ChainablePath.Temp;

        // Assert
        path.IsFile.Should().BeFalse();
        path.IsDirectory.Should().BeTrue();
    }

    [Fact]
    public void Can_find_the_first_existing_file_using_a_string_path()
    {
        // Arrange
        var existingFile = testFolder / "existing.txt";
        var nonExistingFile = testFolder / "nonexisting.txt";
        File.WriteAllText(existingFile, "content");

        // Act
        var result = ChainablePath.FindFirst(nonExistingFile.ToString(), existingFile.ToString());

        // Assert
        result.ToString().Should().Be(existingFile.ToString());
        result.FileExists.Should().BeTrue();
    }

    [Fact]
    public void Can_find_the_first_existing_file_using_a_chainable_path()
    {
        // Arrange
        var existingFile = testFolder / "existing.txt";
        var nonExistingFile = testFolder / "nonexisting.txt";
        File.WriteAllText(existingFile, "content");

        // Act
        var result = ChainablePath.FindFirst(nonExistingFile, existingFile);

        // Assert
        result.ToString().Should().Be(existingFile.ToString());
        result.FileExists.Should().BeTrue();
    }

    [Fact]
    public void Can_find_the_first_existing_directory_using_a_string_path()
    {
        // Arrange
        var existingDir = testFolder / "existing-dir";
        var nonExistingDir = testFolder / "nonexisting-dir";
        existingDir.CreateDirectoryRecursively();

        // Act
        var result = ChainablePath.FindFirst(nonExistingDir.ToString(), existingDir.ToString());

        // Assert
        result.ToString().Should().Be(existingDir.ToString());
        result.DirectoryExists.Should().BeTrue();
    }

    [Fact]
    public void Can_find_the_first_existing_directory_using_a_chainable_path()
    {
        // Arrange
        var existingDir = testFolder / "existing-dir";
        var nonExistingDir = testFolder / "nonexisting-dir";
        existingDir.CreateDirectoryRecursively();

        // Act
        var result = ChainablePath.FindFirst(nonExistingDir, existingDir);

        // Assert
        result.ToString().Should().Be(existingDir.ToString());
        result.DirectoryExists.Should().BeTrue();
    }

    [Fact]
    public void Returns_empty_for_non_existing_string_paths()
    {
        // Arrange
        var nonExistingFile1 = testFolder / "nonexisting1.txt";
        var nonExistingFile2 = testFolder / "nonexisting2.txt";

        // Act
        var result = ChainablePath.FindFirst(nonExistingFile1.ToString(), nonExistingFile2.ToString());

        // Assert
        result.Should().Be(ChainablePath.Empty);
    }

    [Fact]
    public void Returns_empty_for_non_existing_paths()
    {
        // Arrange
        var nonExistingFile1 = testFolder / "nonexisting1.txt";
        var nonExistingFile2 = testFolder / "nonexisting2.txt";

        // Act
        var result = ChainablePath.FindFirst(nonExistingFile1, nonExistingFile2);

        // Assert
        result.Should().Be(ChainablePath.Empty);
    }

    [Fact]
    public void Cannot_find_the_first_existing_path_from_a_null_as_string_array()
    {
        // Act
        var act = () => ChainablePath.FindFirst((string[])null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("paths");
    }

    [Fact]
    public void Cannot_find_the_first_existing_path_from_a_null_array()
    {
        // Act & Assert
        var act = () => ChainablePath.FindFirst((ChainablePath[])null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("paths");
    }

    [Fact]
    public void Cannot_find_the_first_existing_path_from_an_empty_string_array()
    {
        // Act & Assert
        var act = () =>
            ChainablePath.FindFirst(new string[0]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one path must be provided*")
            .WithParameterName("paths");
    }

    [Fact]
    public void Cannot_find_the_first_existing_path_from_an_empty_array()
    {
        // Act & Assert
        var act = () =>
            ChainablePath.FindFirst(new ChainablePath[0]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one path must be provided*")
            .WithParameterName("paths");
    }

    [Theory]
    [InlineData("config.json", ".subdirectory/config.json")]
    [InlineData("app.config", "config/app.config", ".config/app.config")]
    public void FromFirstExisting_usage_examples_work_as_expected(params string[] paths)
    {
        // Arrange - create the second path in each case
        var testPath = testFolder / paths[1];
        testPath.Directory.CreateDirectoryRecursively();
        File.WriteAllText(testPath, "configuration content");

        // Act
        var result = ChainablePath.FindFirst(paths.Select(p => testFolder / p).Select(p => p.ToString()).ToArray());

        // Assert
        result.ToString().Should().Be(testPath.ToString());
        result.FileExists.Should().BeTrue();
    }
}
