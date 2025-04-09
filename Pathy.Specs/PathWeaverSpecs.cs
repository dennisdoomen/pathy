using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class PathWeaverSpecs
{
    [Fact]
    public void Can_build_an_absolute_path()
    {
        // Arrange
        string location = Assembly.GetCallingAssembly()!.Location;

        // Act
        var path = ChainablePath.From(location);

        // Assert
        path.DirectoryName.Should().Be(Path.GetDirectoryName(location));
    }

    [Fact]
    public void An_absolute_path_cannot_be_relative()
    {
        // Act
        var act = () => ChainablePath.From("some/relative/path");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Path*some/relative/path*must be absolute. (Parameter 'path')")
            .And.ParamName.Should().Be("path");
    }

    [Fact]
    public void Fact()
    {
        // Arrange


        // Act


        // Assert
    }
}
