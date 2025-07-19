using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathSpecs
{
    private readonly char slash = Path.DirectorySeparatorChar;
    private readonly ChainablePath testFolder;

    public ChainablePathSpecs()
    {
        testFolder =  ChainablePath.Temp / nameof(ChainablePathSpecs) / Environment.Version.ToString();
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
        path.ToString().Should().BeEquivalentTo("C:" + slash);
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
        var nestedPath = Directory.CreateDirectory(testFolder.ToString() + "/dir1" + slash + "dir2" + slash + "dir3/");

        string location = nestedPath.FullName + "/../../..";

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.ToString().Should().Be(testFolder.ToString().Trim(slash));
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
    public void Can_start_with_an_empty_path()
    {
        // Act
        var path = ChainablePath.New / "c:" / "temp" / "somefile.txt";

        // Assert
        path.ToString().Should().Be("c:" + slash + "temp" + slash + "somefile.txt");
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
        path.ToString().Should().Be(testFolder + slash + "dir1" + slash + "somefile.txt");
    }

    [Fact]
    public void Can_be_assigned_to_a_string()
    {
        // Act
        string result = ChainablePath.From("temp/somefile.txt");

        // Assert
        result.Should().Be("temp" + slash + "somefile.txt");
    }

    [Fact]
    public void Can_chain_multiple_directories()
    {
        // Arrange
        var temp = testFolder;

        // Act
        var result = temp / "dir1" / "dir2" / "dir3";

        // Assert
        result.DirectoryName.Should().Be(temp + slash + "dir1" + slash + "dir2");
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
        result.DirectoryName.Should().Be(testFolder + slash + "dir1" + slash + "dir2" + slash + "dir3");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Ignores_superfluous_slashes()
    {
        // Act
        var result = testFolder / "dir1" / "dir2/" / "dir3/" / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(testFolder + slash + "dir1" + slash + "dir2" + slash + "dir3");
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
        result.Directory!.ToString().Should().Be("C:" + slash);
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
}
