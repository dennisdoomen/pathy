using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathSpecs
{
    // todo: handles leading or trailing slashes
    // todo: get files or directories
    // todo: glob files or directories
    // todo: cannot chain a file or directory to a file
    // todo: isfile or isdirectory
    // todo: can create a path hierarchy
    // todo: can delete a file or directory recursively
    // todo: get root
    // todo: move
    // todo: current


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
    public void A_trailing_slash_is_fine()
    {
        // Arrange
        string location = Assembly.GetCallingAssembly()!.Location + "/";

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.DirectoryName.Should().Be(Path.GetDirectoryName(location));
        path.IsRooted.Should().BeTrue();
    }

    [Fact]
    public void Can_build_from_a_path_with_reverse_traversals()
    {
        // Arrange
        string location = Assembly.GetCallingAssembly()!.Location + "/../../../..";

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.ToString().Should().EndWith("Pathy.Specs");
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
    public void Can_chain_multiple_directories()
    {
        // Arrange
        var temp = ChainablePath.Temp;

        // Act
        var result = temp / "dir1" / "dir2" / "dir3";

        // Assert
        result.DirectoryName.Should().Be(Path.GetTempPath() + "dir1" + Path.DirectorySeparatorChar + "dir2");
        result.Name.Should().Be("dir3");
    }

    [Fact]
    public void Can_chain_directories_and_files()
    {
        // Arrange
        var temp = ChainablePath.Temp;

        // Act
        var result = temp / "dir1" / "dir2" / "dir3" / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(Path.GetTempPath() + "dir1/dir2/dir3");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Ignores_superfluous_slashes()
    {
        // Arrange
        var temp = ChainablePath.Temp;

        // Act
        var result = temp / "dir1" / "/dir2" / "/dir3" / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(Path.GetTempPath() + "dir1/dir2/dir3");
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
    public void A_directory_can_have_an_extension_too()
    {
        // Act
        var path = Environment.CurrentDirectory.ToPath() / ".." / ".." / "..";

        // Assert
        path.Extension.Should().Be(".Specs");
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
}
