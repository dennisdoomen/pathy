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

    [Fact]
    public void Can_delete_multiple_files()
    {
        // Arrange
        var file1 = testFolder / "file1.txt";
        var file2 = testFolder / "file2.txt";
        var file3 = testFolder / "file3.txt";
        File.WriteAllText(file1, "Hello World!");
        File.WriteAllText(file2, "Hello World!");
        File.WriteAllText(file3, "Hello World!");

        var files = new[] { file1, file2, file3 };

        // Act
        files.DeleteFileOrDirectory();

        // Assert
        file1.FileExists.Should().BeFalse();
        file2.FileExists.Should().BeFalse();
        file3.FileExists.Should().BeFalse();
    }

    [Fact]
    public void Can_delete_multiple_directories()
    {
        // Arrange
        var dir1 = testFolder / "dir1";
        var dir2 = testFolder / "dir2";
        var dir3 = testFolder / "dir3";
        dir1.CreateDirectoryRecursively();
        dir2.CreateDirectoryRecursively();
        dir3.CreateDirectoryRecursively();
        File.WriteAllText(dir1 / "file.txt", "Hello World!");
        File.WriteAllText(dir2 / "file.txt", "Hello World!");
        File.WriteAllText(dir3 / "file.txt", "Hello World!");

        var directories = new[] { dir1, dir2, dir3 };

        // Act
        directories.DeleteFileOrDirectory();

        // Assert
        dir1.Exists.Should().BeFalse();
        dir2.Exists.Should().BeFalse();
        dir3.Exists.Should().BeFalse();
    }

    [Fact]
    public void Can_move_multiple_files_to_a_directory()
    {
        // Arrange
        (testFolder / "Source").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        var file1 = testFolder / "Source" / "file1.txt";
        var file2 = testFolder / "Source" / "file2.txt";
        var file3 = testFolder / "Source" / "file3.txt";
        File.WriteAllText(file1, "Hello World!");
        File.WriteAllText(file2, "Hello World!");
        File.WriteAllText(file3, "Hello World!");

        var files = new[] { file1, file2, file3 };

        // Act
        files.MoveFileOrDirectory(testFolder / "Destination");

        // Assert
        file1.Exists.Should().BeFalse();
        file2.Exists.Should().BeFalse();
        file3.Exists.Should().BeFalse();
        (testFolder / "Destination" / "file1.txt").Exists.Should().BeTrue();
        (testFolder / "Destination" / "file2.txt").Exists.Should().BeTrue();
        (testFolder / "Destination" / "file3.txt").Exists.Should().BeTrue();
    }

    [Fact]
    public void Can_move_multiple_directories_under_another_directory()
    {
        // Arrange
        (testFolder / "Source" / "dir1").CreateDirectoryRecursively();
        (testFolder / "Source" / "dir2").CreateDirectoryRecursively();
        (testFolder / "Destination").CreateDirectoryRecursively();
        File.WriteAllText(testFolder / "Source" / "dir1" / "file.txt", "Hello World!");
        File.WriteAllText(testFolder / "Source" / "dir2" / "file.txt", "Hello World!");

        var directories = new[] { testFolder / "Source" / "dir1", testFolder / "Source" / "dir2" };

        // Act
        directories.MoveFileOrDirectory(testFolder / "Destination");

        // Assert
        (testFolder / "Source" / "dir1").Exists.Should().BeFalse();
        (testFolder / "Source" / "dir2").Exists.Should().BeFalse();
        (testFolder / "Destination" / "dir1" / "file.txt").Exists.Should().BeTrue();
        (testFolder / "Destination" / "dir2" / "file.txt").Exists.Should().BeTrue();
    }
}
