using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathSpecs
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
    public void Fact()
    {
        // Arrange


        // Act


        // Assert
    }
}
