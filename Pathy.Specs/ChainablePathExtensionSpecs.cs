using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathExtensionSpecs
{
    private readonly ChainablePath testFolder;

    public ChainablePathExtensionSpecs()
    {
        testFolder = ChainablePath.Temp / nameof(ChainablePathExtensionSpecs);
        testFolder.DeleteFileOrDirectory();
        testFolder.CreateDirectoryRecursively();
    }

    [Fact]
    public void Can_delete_a_file()
    {
        // Arrange
        var path = ChainablePath.Temp / "file.txt";
        File.WriteAllText(path, "Hello World!");

        // Act
        path.DeleteFileOrDirectory();

        // Assert
        path.FileExists.Should().BeFalse();
    }

    [Fact]
    public void Can_delete_a_directory_recursively()
    {
        // Arrange
        ChainablePath root = ChainablePath.Temp / "dir1";

        var nestedDirectory = root / "dir2" / "dir3";
        nestedDirectory.CreateDirectoryRecursively();

        File.WriteAllText(nestedDirectory / "filetobedeleted.txt", "Hello World!");

        // Act
        root.DeleteFileOrDirectory();

        // Assert
        root.Exists.Should().BeFalse();
    }

    [Fact]
    public void Can_move_a_file_to_a_directory_without_renaming()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        var file = testFolder / "Source" / "temp.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        file.MoveFileOrDirectory(testFolder / "Destination");

        // Assert
        file.Exists.Should().BeFalse();
        (testFolder / "Destination" / "temp.txt").Exists.Should().BeTrue();
    }

    [Fact]
    public void Can_move_a_file_to_a_directory_under_a_new_name()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        var file = testFolder / "Source" / "oldname.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        file.MoveFileOrDirectory(testFolder / "Destination", "newname.txt");

        // Assert
        file.Exists.Should().BeFalse();
        (testFolder / "Destination" / "newname.txt").Exists.Should().BeTrue();
    }

    [Fact]
    public void Moving_a_file_under_a_new_name_requires_a_non_empty_name()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        var file = testFolder / "Source" / "oldname.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        var act = () => file.MoveFileOrDirectory(testFolder / "SomeDestination", newName: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Renaming requires a valid name*newName*");
    }

    [Fact]
    public void Can_move_a_directory_under_a_directory_without_renaming()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        var file = testFolder / "Source" / "temp.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        (testFolder / "Source").MoveFileOrDirectory(testFolder / "Destination");

        // Assert
        (testFolder / "Destination" / "Source" / "temp.txt").Exists.Should().BeTrue();
    }

    [Fact]
    public void Can_move_a_directory_under_another_using_a_new_name()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        var file = testFolder / "Source" / "temp.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        (testFolder / "Source").MoveFileOrDirectory(testFolder / "Destination", "NewName");

        // Assert
        (testFolder / "Destination" / "NewName" / "temp.txt").Exists.Should().BeTrue();
    }
}
