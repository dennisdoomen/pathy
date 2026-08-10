using System.ComponentModel;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Pathy.Specs;

public class ChainablePathConverterSpecs
{
    [Fact]
    public void Type_converter_is_registered_on_ChainablePath()
    {
        // Act
        var converter = TypeDescriptor.GetConverter(typeof(ChainablePath));

        // Assert
        converter.Should().BeOfType<ChainablePathTypeConverter>();
    }

    [Fact]
    public void Type_converter_reports_it_can_convert_from_and_to_a_string()
    {
        // Arrange
        var converter = new ChainablePathTypeConverter();

        // Act & Assert
        converter.CanConvertFrom(typeof(string)).Should().BeTrue();
        converter.CanConvertTo(typeof(string)).Should().BeTrue();
    }

    [Fact]
    public void Type_converter_converts_a_string_into_a_ChainablePath()
    {
        // Arrange
        var converter = new ChainablePathTypeConverter();
        string rawPath = ChainablePath.Current.ToString();

        // Act
        object result = converter.ConvertFrom(rawPath);

        // Assert
        result.Should().Be(ChainablePath.From(rawPath));
    }

    [Fact]
    public void Type_converter_converts_a_ChainablePath_into_a_string()
    {
        // Arrange
        var converter = new ChainablePathTypeConverter();
        var path = ChainablePath.Current;

        // Act
        object result = converter.ConvertTo(path, typeof(string));

        // Assert
        result.Should().Be(path.ToString());
    }

    [Fact]
    public void Type_converter_round_trips_a_ChainablePath_through_its_string_representation()
    {
        // Arrange
        var converter = new ChainablePathTypeConverter();
        var original = ChainablePath.Current / "some" / "sub" / "path";

        // Act
        object asString = converter.ConvertTo(original, typeof(string));
        object roundTripped = converter.ConvertFrom(asString!);

        // Assert
        roundTripped.Should().Be(original);
    }

    private class ConfigurationModel
    {
        public ChainablePath WorkingDirectory { get; set; }
    }

    [Fact]
    public void Json_converter_serializes_a_ChainablePath_as_a_plain_string()
    {
        // Arrange
        var model = new ConfigurationModel { WorkingDirectory = ChainablePath.Current };

        // Act
        string json = JsonSerializer.Serialize(model);

        // Assert
        json.Should().Contain(model.WorkingDirectory.ToString().Replace("\\", "\\\\"));
    }

    [Fact]
    public void Json_converter_round_trips_a_ChainablePath_property()
    {
        // Arrange
        var model = new ConfigurationModel { WorkingDirectory = ChainablePath.Current / "some" / "sub" / "path" };

        // Act
        string json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ConfigurationModel>(json);

        // Assert
        deserialized!.WorkingDirectory.Should().Be(model.WorkingDirectory);
    }
}
