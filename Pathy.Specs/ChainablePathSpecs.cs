using System;
using System.Globalization;
using System.IO;
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

    [Fact]
    public void Can_use_a_path_inside_an_interpolated_string()
    {
        // Arrange
        var path = ChainablePath.From(@"C:\some\file.txt");

        // Act
        string result = $"The path is {path}";

        // Assert
        result.Should().Be(@"The path is C:\some\file.txt");
    }

    [Fact]
    public void Can_format_a_path_using_ToString_with_format_and_provider()
    {
        // Arrange
        var path = ChainablePath.From(@"C:\some\file.txt");

        // Act
        string result = path.ToString(format: "whatever", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(@"C:\some\file.txt");
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Can_try_format_a_path_into_a_span_that_is_large_enough()
    {
        // Arrange
        var path = ChainablePath.From(@"C:\some\file.txt");
        Span<char> destination = stackalloc char[path.ToString().Length];

        // Act
        bool success = path.TryFormat(destination, out int charsWritten, default, null);

        // Assert
        success.Should().BeTrue();
        charsWritten.Should().Be(path.ToString().Length);
        destination.ToString().Should().Be(path.ToString());
    }

    [Fact]
    public void Cannot_try_format_a_path_into_a_span_that_is_too_small()
    {
        // Arrange
        var path = ChainablePath.From(@"C:\some\file.txt");
        Span<char> destination = stackalloc char[2];

        // Act
        bool success = path.TryFormat(destination, out int charsWritten, default, null);

        // Assert
        success.Should().BeFalse();
        charsWritten.Should().Be(0);
    }
#endif

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

    [Fact]
    public void Cannot_get_the_root_of_a_relative_path()
    {
        // Act
        var path = ChainablePath.From("temp/somefile.txt");

        // Assert
        path.Root.ToString().Should().Be(ChainablePath.Null);
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

    [Theory]
    [InlineData("SomeFile.txt", true)]
    [InlineData("somefile.txt", true)]
    [InlineData("SOMEFILE.TXT", true)]
    [InlineData("SomeFile", false)]
    [InlineData("OtherFile.txt", false)]
    public void Can_check_for_a_name(string name, bool shouldMatch)
    {
        // Act
        var path = ChainablePath.Temp / "SomeFile.txt";

        // Assert
        path.HasName(name).Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Checking_for_a_name_requires_a_valid_name(string name)
    {
        // Arrange
        var path = ChainablePath.Temp / "SomeFile.txt";

        // Act
        Action act = () => path.HasName(name);

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
    public void Can_find_files_using_multiple_globbing_patterns()
    {
        // Arrange
        var temp = ChainablePath.Temp / "dir1" / "dir2" / "dir3" / "file.txt";

        temp.CreateDirectoryRecursively();
        File.WriteAllText(temp / "file.txt", "Hello World!");
        File.WriteAllText(temp / "file2.txt", "Hello World!");
        File.WriteAllText(temp / "file3.doc", "Hello World!");
        File.WriteAllText(temp / "file4.md", "# Markdown");

        // Act
        var files = (ChainablePath.Temp / "dir1").GlobFiles("**/*.txt", "**/*.doc");

        // Assert
        files.Should().BeEquivalentTo([
            temp / "file.txt",
            temp / "file2.txt",
            temp / "file3.doc"
        ], options => options.ComparingRecordsByValue());
    }

    [Fact]
    public void Can_find_files_using_multiple_globbing_patterns_with_different_depths()
    {
        // Arrange
        var baseDir = testFolder / "MultiPatternTest";
        var deepDir = baseDir / "level1" / "level2";
        deepDir.CreateDirectoryRecursively();

        File.WriteAllText(baseDir / "root.txt", "Root");
        File.WriteAllText(baseDir / "root.md", "# Root");
        File.WriteAllText(deepDir / "deep.txt", "Deep");
        File.WriteAllText(deepDir / "deep.json", "{}");

        // Act
        var files = baseDir.GlobFiles("**/*.txt", "**/*.json");

        // Assert
        files.Should().BeEquivalentTo([
            baseDir / "root.txt",
            deepDir / "deep.txt",
            deepDir / "deep.json"
        ], options => options.ComparingRecordsByValue());
    }

    [Fact]
    public void GlobFiles_with_multiple_patterns_throws_when_no_patterns_provided()
    {
        // Arrange
        var temp = ChainablePath.Temp / "dir1";
        temp.CreateDirectoryRecursively();

        // Act & Assert
        var act = () => temp.GlobFiles(Array.Empty<string>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one glob pattern must be provided*")
            .WithParameterName("globPatterns");
    }

    [Fact]
    public void GlobFiles_with_multiple_patterns_throws_when_pattern_is_null()
    {
        // Arrange
        var temp = ChainablePath.Temp / "dir1";
        temp.CreateDirectoryRecursively();

        // Act & Assert
        var act = () => temp.GlobFiles("**/*.txt", null, "**/*.doc");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Glob patterns cannot be null or empty*")
            .WithParameterName("globPatterns");
    }

    [Fact]
    public void GlobFiles_with_multiple_patterns_throws_when_pattern_is_empty()
    {
        // Arrange
        var temp = ChainablePath.Temp / "dir1";
        temp.CreateDirectoryRecursively();

        // Act & Assert
        var act = () => temp.GlobFiles("**/*.txt", "", "**/*.doc");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Glob patterns cannot be null or empty*")
            .WithParameterName("globPatterns");
    }

    // Issue #35 asked for a `Match(wildcard)` method and was closed without shipping a public API. `Matches`
    // (below) is what actually delivers that capability - purely in-memory, without ever touching the file
    // system or requiring the path to exist.
    [Fact]
    public void An_absolute_path_matches_a_pattern_that_corresponds_to_its_suffix()
    {
        // Arrange
        var path = ChainablePath.Temp / "src" / "Pathy" / "ChainablePath.cs";

        // Act & Assert
        path.Matches("**/*.cs").Should().BeTrue();
    }

    [Fact]
    public void An_absolute_path_does_not_match_an_unrelated_pattern()
    {
        // Arrange
        var path = ChainablePath.Temp / "src" / "Pathy" / "ChainablePath.cs";

        // Act & Assert
        path.Matches("**/*.md").Should().BeFalse();
    }

    [Fact]
    public void Matches_does_not_require_the_path_to_exist_on_disk()
    {
        // Arrange
        var path = ChainablePath.Temp / Guid.NewGuid().ToString("N") / "does-not-exist.cs";

        // Act & Assert
        path.Matches("**/*.cs").Should().BeTrue();
    }

    [Fact]
    public void Matches_with_multiple_patterns_returns_true_if_any_pattern_matches()
    {
        // Arrange
        var path = ChainablePath.Temp / "src" / "Pathy" / "ChainablePath.cs";

        // Act & Assert
        path.Matches("**/bin/**", "**/obj/**", "**/*.cs").Should().BeTrue();
    }

    [Fact]
    public void Matches_with_multiple_patterns_returns_false_if_none_match()
    {
        // Arrange
        var path = ChainablePath.Temp / "src" / "Pathy" / "ChainablePath.cs";

        // Act & Assert
        path.Matches("**/bin/**", "**/obj/**").Should().BeFalse();
    }

    [Fact]
    public void Matches_is_case_insensitive()
    {
        // Arrange
        var path = ChainablePath.Temp / "src" / "Pathy" / "ChainablePath.cs";

        // Act & Assert
        path.Matches("**/*.CS").Should().BeTrue();
    }

    [Fact]
    public void Matches_with_single_pattern_throws_when_pattern_is_null()
    {
        // Arrange
        var path = ChainablePath.Temp / "file.cs";

        // Act & Assert
        var act = () => path.Matches((string)null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Glob patterns cannot be null or empty*")
            .WithParameterName("globPatterns");
    }

    [Fact]
    public void Matches_with_multiple_patterns_throws_when_no_patterns_provided()
    {
        // Arrange
        var path = ChainablePath.Temp / "file.cs";

        // Act & Assert
        var act = () => path.Matches(new string[0]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one glob pattern must be provided*")
            .WithParameterName("globPatterns");
    }

    [Fact]
    public void Matches_with_multiple_patterns_throws_when_a_pattern_is_null()
    {
        // Arrange
        var path = ChainablePath.Temp / "file.cs";

        // Act & Assert
        var act = () => path.Matches("**/*.cs", null, "**/*.doc");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Glob patterns cannot be null or empty*")
            .WithParameterName("globPatterns");
    }

    [Fact]
    public void Matches_with_multiple_patterns_throws_when_a_pattern_is_empty()
    {
        // Arrange
        var path = ChainablePath.Temp / "file.cs";

        // Act & Assert
        var act = () => path.Matches("**/*.cs", "", "**/*.doc");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Glob patterns cannot be null or empty*")
            .WithParameterName("globPatterns");
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
    public void Parent_directory_with_matching_file_is_found()
    {
        // Arrange
        var testRoot = testFolder / "FindParentTest";
        var grandparentDir = testRoot / "Grandparent";
        var parentDir = grandparentDir / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(parentDir / "project.sln", "solution file");
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("*.sln");

        // Assert
        result.Should().Be(parentDir);
    }

    [Fact]
    public void Parent_directory_with_multiple_matching_wildcards_is_found()
    {
        // Arrange
        var testRoot = testFolder / "FindParentMultipleTest";
        var grandparentDir = testRoot / "Grandparent";
        var parentDir = grandparentDir / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(parentDir / "project.slnx", "solution file");
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("*.sln", "*.slnx");

        // Assert
        result.Should().Be(parentDir);
    }

    [Fact]
    public void Empty_path_returned_when_no_match_found()
    {
        // Arrange
        var testRoot = testFolder / "FindParentNoMatchTest";
        var parentDir = testRoot / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("*.sln");

        // Assert
        result.Should().Be(ChainablePath.Null);
    }

    [Fact]
    public void Parent_search_works_from_file_path()
    {
        // Arrange
        var testRoot = testFolder / "FindParentFromFileTest";
        var parentDir = testRoot / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(parentDir / "project.sln", "solution file");
        File.WriteAllText(childDir / "program.cs", "some code");

        var filePath = childDir / "program.cs";

        // Act
        var result = filePath.FindParentWithFileMatching("*.sln");

        // Assert
        result.Should().Be(parentDir);
    }

    [Fact]
    public void Closest_parent_directory_is_found()
    {
        // Arrange
        var testRoot = testFolder / "FindParentClosestTest";
        var grandparentDir = testRoot / "Grandparent";
        var parentDir = grandparentDir / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(grandparentDir / "outer.sln", "outer solution file");
        File.WriteAllText(parentDir / "inner.sln", "inner solution file");
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("*.sln");

        // Assert
        result.Should().Be(parentDir); // Should find the closest parent, not the grandparent
    }

    [Fact]
    public void Case_insensitive_matching_works()
    {
        // Arrange
        var testRoot = testFolder / "FindParentCaseTest";
        var parentDir = testRoot / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(parentDir / "PROJECT.SLN", "solution file");
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("*.sln");

        // Assert
        result.Should().Be(parentDir);
    }

    [Fact]
    public void Null_wildcards_throws_exception()
    {
        // Arrange
        var path = testFolder / "SomeDir";

        // Act
        Action act = () => path.FindParentWithFileMatching(null);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*wildcard*provided*");
    }

    [Fact]
    public void Empty_wildcards_throws_exception()
    {
        // Arrange
        var path = testFolder / "SomeDir";

        // Act
        Action act = () => path.FindParentWithFileMatching();

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*wildcard*provided*");
    }

    [Fact]
    public void Null_or_empty_wildcard_throws_exception()
    {
        // Arrange
        var path = testFolder / "SomeDir";

        // Act
        Action act = () => path.FindParentWithFileMatching("*.sln", "", "*.slnx");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Question_mark_wildcard_matching_works()
    {
        // Arrange
        var testRoot = testFolder / "FindParentQuestionTest";
        var parentDir = testRoot / "Parent";
        var childDir = parentDir / "Child";

        childDir.CreateDirectoryRecursively();
        File.WriteAllText(parentDir / "test1.txt", "test file");
        File.WriteAllText(childDir / "program.cs", "some code");

        // Act
        var result = childDir.FindParentWithFileMatching("test?.txt");

        // Assert
        result.Should().Be(parentDir);
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
        result.IsNull.Should().BeTrue();
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
            ChainablePath.FindFirst(Array.Empty<string>());

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
            ChainablePath.FindFirst(Array.Empty<ChainablePath>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one path must be provided*")
            .WithParameterName("paths");
    }

    [Fact]
    public void Can_resolve_a_file_when_path_is_the_file_itself()
    {
        // Arrange
        var file = testFolder / "test.txt";
        File.WriteAllText(file, "content");

        // Act
        var result = file.ResolveFile("test.txt");

        // Assert
        result.ToString().Should().Be(file.ToString());
        result.FileExists.Should().BeTrue();
    }

    [Fact]
    public void Can_resolve_a_file_when_path_is_a_directory_containing_the_file()
    {
        // Arrange
        var file = testFolder / "test.txt";
        File.WriteAllText(file, "content");

        // Act
        var result = testFolder.ResolveFile("test.txt");

        // Assert
        result.ToString().Should().Be(file.ToString());
        result.FileExists.Should().BeTrue();
    }

    [Fact]
    public void ResolveFile_is_case_insensitive_when_path_is_the_file()
    {
        // Arrange
        var file = testFolder / "Test.txt";
        File.WriteAllText(file, "content");

        // Act
        var result = file.ResolveFile("test.TXT");

        // Assert
        result.ToString().Should().Be(file.ToString());
        result.FileExists.Should().BeTrue();
    }

    [Fact]
    public void ResolveFile_returns_empty_when_file_does_not_exist_in_directory()
    {
        // Act
        var result = testFolder.ResolveFile("nonexistent.txt");

        // Assert
        result.Should().Be(ChainablePath.Empty);
    }

    [Fact]
    public void ResolveFile_returns_empty_when_path_is_a_file_with_different_name()
    {
        // Arrange
        var file = testFolder / "actual.txt";
        File.WriteAllText(file, "content");

        // Act
        var result = file.ResolveFile("different.txt");

        // Assert
        result.Should().Be(ChainablePath.Empty);
    }

    [Fact]
    public void ResolveFile_throws_when_fileName_is_null()
    {
        // Act
        var act = () => testFolder.ResolveFile(null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File name cannot be null or empty*")
            .WithParameterName("fileName");
    }

    [Fact]
    public void ResolveFile_throws_when_fileName_is_empty()
    {
        // Act
        var act = () => testFolder.ResolveFile(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File name cannot be null or empty*")
            .WithParameterName("fileName");
    }

    [Fact]
    public void Can_get_last_write_time_utc_for_file()
    {
        // Arrange
        var filePath = testFolder / "test_file.txt";
        File.WriteAllText(filePath, "Hello World!");
        var expectedTime = File.GetLastWriteTimeUtc(filePath.ToString());

        // Act
        var actualTime = filePath.LastWriteTimeUtc;

        // Assert
        actualTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
        actualTime.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Can_get_last_write_time_utc_for_directory()
    {
        // Arrange
        var dirPath = testFolder / "test_dir";
        Directory.CreateDirectory(dirPath);
        var expectedTime = Directory.GetLastWriteTimeUtc(dirPath.ToString());

        // Act
        var actualTime = dirPath.LastWriteTimeUtc;

        // Assert
        actualTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
        actualTime.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Returns_min_value_for_non_existing_path()
    {
        // Arrange
        var nonExistingPath = testFolder / "non_existing_file.txt";

        // Act
        var actualTime = nonExistingPath.LastWriteTimeUtc;

        // Assert
        actualTime.Should().Be(DateTime.MinValue);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Can_navigate_to_parent_using_range_operator()
    {
        // Arrange
        var path = testFolder / "dir1" / "dir2" / "dir3";
        path.CreateDirectoryRecursively();

        // Act
        var result = path / .. / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(testFolder + Slash + "dir1" + Slash + "dir2");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Can_chain_multiple_range_operators_for_parent_navigation()
    {
        // Arrange
        var path = testFolder / "level1" / "level2" / "level3" / "level4";
        path.CreateDirectoryRecursively();

        // Act
        var result = path / .. / .. / .. / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(testFolder + Slash + "level1");
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Range_operator_is_equivalent_to_using_the_parent_property()
    {
        // Arrange
        var path = testFolder / "dir1" / "dir2" / "dir3";
        path.CreateDirectoryRecursively();

        // Act
        var usingRangeOperator = path / .. / .. / "file.txt";
        var usingParentProperty = path.Parent.Parent / "file.txt";

        // Assert
        usingRangeOperator.Should().Be(usingParentProperty);
    }

    [Fact]
    public void Can_mix_range_operator_with_regular_path_operations()
    {
        // Arrange
        var baseDir = testFolder / "project" / "src" / "core";
        baseDir.CreateDirectoryRecursively();

        var testDir = testFolder / "project" / "tests";
        testDir.CreateDirectoryRecursively();

        // Act - Navigate from core to tests using range operator
        var result = baseDir / .. / .. / "tests" / "CoreTests.cs";

        // Assert
        result.ToString().Should().Be(testFolder + Slash + "project" + Slash + "tests" + Slash + "CoreTests.cs");
    }

    [Fact]
    public void Range_operator_works_with_current_path()
    {
        // Act
        var result = ChainablePath.Current / .. / .. / "file.txt";

        // Assert
        result.DirectoryName.Should().Be(Environment.CurrentDirectory.ToPath().Parent.Parent.ToString());
        result.Name.Should().Be("file.txt");
    }

    [Fact]
    public void Only_two_dots_are_allowed()
    {
        // Arrange
        var path = testFolder / "dir1" / "dir2";
        path.CreateDirectoryRecursively();

        // Act
        Action act = () => { _ = path / 1..3; };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Only the '..' range operator is supported*")
            .WithParameterName("range");
    }
#endif

    [Fact]
    public void Can_convert_an_absolute_path_to_a_file_uri()
    {
        // Arrange
        var path = testFolder / "report.txt";

        // Act
        var uri = path.ToUri();

        // Assert
        uri.IsAbsoluteUri.Should().BeTrue();
        uri.Scheme.Should().Be(Uri.UriSchemeFile);
        uri.LocalPath.Should().Be(path.ToString());
    }

    [Fact]
    public void Cannot_convert_a_relative_path_to_a_uri()
    {
        // Arrange
        ChainablePath path = "relative/path.txt";

        // Act
        var act = () => path.ToUri();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be absolute*");
    }

    [Fact]
    public void Can_round_trip_an_absolute_path_through_to_uri_and_from_uri()
    {
        // Arrange
        var path = testFolder / "report.txt";

        // Act
        var result = ChainablePath.FromUri(path.ToUri());

        // Assert
        result.ToString().Should().Be(path.ToString());
    }

    [Fact]
    public void Can_round_trip_a_path_with_spaces_and_special_characters()
    {
        // Arrange
        var path = testFolder / "my report (final) #1.txt";

        // Act
        var uri = path.ToUri();
        var result = ChainablePath.FromUri(uri);

        // Assert
        result.ToString().Should().Be(path.ToString());
    }

    [Fact]
    public void Can_convert_a_unc_path_to_a_uri_and_back()
    {
        // Arrange
        ChainablePath path = @"\\server\share\file.txt";

        // Act
        var uri = path.ToUri();
        var result = ChainablePath.FromUri(uri);

        // Assert
        uri.IsUnc.Should().BeTrue();
        uri.Host.Should().Be("server");
        result.ToString().Should().Be(path.ToString());
    }

    [Fact]
    public void From_uri_rejects_a_null_uri()
    {
        // Act
        var act = () => ChainablePath.FromUri(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("uri");
    }

    [Fact]
    public void From_uri_rejects_a_non_file_scheme()
    {
        // Act
        var act = () => ChainablePath.FromUri(new Uri("https://example.com/some/path"));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*https*")
            .WithParameterName("uri");
    }

    [Fact]
    public void From_uri_rejects_a_relative_uri()
    {
        // Act
        var act = () => ChainablePath.FromUri(new Uri("relative/path.txt", UriKind.Relative));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*absolute*")
            .WithParameterName("uri");
    }
}
