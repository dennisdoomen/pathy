using System;
using System.IO;
using System.Text;
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
    public void Can_ensure_a_directory_exists()
    {
        // Arrange
        var directory = testFolder / "NewDirectory" / "NestedDirectory";

        // Act
        var result = directory.EnsureDirectoryExists();

        // Assert
        directory.DirectoryExists.Should().BeTrue();
        result.Should().Be(directory);
    }

    [Fact]
    public void Ensuring_an_already_existing_directory_exists_is_a_no_op()
    {
        // Arrange
        var directory = testFolder / "ExistingDirectory";
        directory.CreateDirectoryRecursively();
        File.WriteAllText(directory / "file.txt", "Hello World!");

        // Act
        var result = directory.EnsureDirectoryExists();

        // Assert
        (directory / "file.txt").FileExists.Should().BeTrue();
        result.Should().Be(directory);
    }

    [Fact]
    public void Ensuring_a_directory_exists_where_a_file_already_exists_throws()
    {
        // Arrange
        var file = testFolder / "file.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        var act = () => file.EnsureDirectoryExists();

        // Assert
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void Touching_a_missing_file_creates_it_and_any_missing_parent_directories()
    {
        // Arrange
        var file = testFolder / "sub1" / "sub2" / "stamp.txt";

        // Act
        var result = file.TouchFile();

        // Assert
        file.FileExists.Should().BeTrue();
        result.Should().Be(file);
    }

    [Fact]
    public void Touching_an_existing_file_updates_its_last_write_time_without_changing_its_content()
    {
        // Arrange
        var file = testFolder / "existing.txt";
        File.WriteAllText(file, "Hello World!");
        file.SetLastWriteTimeUtc(DateTime.UtcNow.AddDays(-1));

        // Act
        file.TouchFile();

        // Assert
        File.ReadAllText(file).Should().Be("Hello World!");
        file.LastWriteTimeUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Touching_a_path_that_is_already_a_directory_throws()
    {
        // Arrange
        var directory = testFolder / "already-a-directory";
        directory.CreateDirectoryRecursively();

        // Act
        var act = () => directory.TouchFile();

        // Assert
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void Can_set_the_last_write_time_of_a_file()
    {
        // Arrange
        var file = testFolder / "file.txt";
        File.WriteAllText(file, "Hello World!");
        var expected = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        file.SetLastWriteTimeUtc(expected);

        // Assert
        file.LastWriteTimeUtc.Should().Be(expected);
    }

    [Fact]
    public void Can_set_the_last_write_time_of_a_directory()
    {
        // Arrange
        var directory = testFolder / "SomeDirectory";
        directory.CreateDirectoryRecursively();
        var expected = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        directory.SetLastWriteTimeUtc(expected);

        // Assert
        directory.LastWriteTimeUtc.Should().Be(expected);
    }

    [Fact]
    public void Setting_the_last_write_time_of_a_non_existing_path_throws()
    {
        // Arrange
        var missing = testFolder / "does-not-exist.txt";

        // Act
        var act = () => missing.SetLastWriteTimeUtc(DateTime.UtcNow);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Can_write_and_read_all_text()
    {
        // Arrange
        var file = testFolder / "text.txt";

        // Act
        file.WriteAllText("Hello World!");

        // Assert
        file.ReadAllText().Should().Be("Hello World!");
    }

    [Fact]
    public void Can_write_and_read_all_text_using_a_specific_encoding()
    {
        // Arrange
        var file = testFolder / "encoded.txt";

        // Act
        file.WriteAllText("Hello World!", Encoding.ASCII);

        // Assert
        file.ReadAllText(Encoding.ASCII).Should().Be("Hello World!");
    }

    [Fact]
    public void Can_write_and_read_all_lines()
    {
        // Arrange
        var file = testFolder / "lines.txt";
        var lines = new[] { "line1", "line2", "line3" };

        // Act
        file.WriteAllLines(lines);

        // Assert
        file.ReadAllLines().Should().Equal(lines);
    }

    [Fact]
    public void Can_lazily_read_lines()
    {
        // Arrange
        var file = testFolder / "lazy-lines.txt";
        var lines = new[] { "line1", "line2", "line3" };
        file.WriteAllLines(lines);

        // Act
        var result = file.ReadLines();

        // Assert
        result.Should().Equal(lines);
    }

    [Fact]
    public void Can_write_and_read_all_bytes()
    {
        // Arrange
        var file = testFolder / "data.bin";
        var bytes = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        file.WriteAllBytes(bytes);

        // Assert
        file.ReadAllBytes().Should().Equal(bytes);
    }

    [Fact]
    public void Can_append_text_to_a_new_file()
    {
        // Arrange
        var file = testFolder / "appended.txt";

        // Act
        file.AppendAllText("Hello");
        file.AppendAllText(" World!");

        // Assert
        file.ReadAllText().Should().Be("Hello World!");
    }

    [Fact]
    public void Writing_text_does_not_create_missing_parent_directories()
    {
        // Arrange
        var file = testFolder / "missing-directory" / "file.txt";

        // Act
        var act = () => file.WriteAllText("Hello World!");

        // Assert
        act.Should().Throw<DirectoryNotFoundException>();
    }

    [Fact]
    public void Can_open_a_file_for_reading()
    {
        // Arrange
        var file = testFolder / "open-read.txt";
        File.WriteAllText(file, "Hello World!");

        // Act
        using var stream = file.OpenRead();
        using var reader = new StreamReader(stream);

        // Assert
        reader.ReadToEnd().Should().Be("Hello World!");
    }

    [Fact]
    public void Can_open_a_file_for_writing()
    {
        // Arrange
        var file = testFolder / "open-write.txt";

        // Act
        using (var stream = file.OpenWrite())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("Hello World!");
        }

        // Assert
        File.ReadAllText(file).Should().Be("Hello World!");
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
        var act = () => file.MoveFileOrDirectory(testFolder / "SomeDestination", "");

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
