using FluentAssertions;
using OtChaim.Domain.Users;

namespace OtChaim.Domain.Tests.Users;

[TestFixture]
public class PersonNameTests
{
    [Test]
    public void Constructor_ShouldTrimAndNormalizeValues()
    {
        PersonName name = new("  John  ", null!);

        name.First.Should().Be("John");
        name.Last.Should().Be(string.Empty);
        name.Full.Should().Be("John");
    }

    [Test]
    public void Empty_ShouldReturnSingletonWithBlankParts()
    {
        PersonName.Empty.First.Should().BeEmpty();
        PersonName.Empty.Last.Should().BeEmpty();
        PersonName.Empty.Full.Should().BeEmpty();
    }

    [Test]
    public void FromFullName_ShouldSplitFirstAndLast()
    {
        PersonName name = PersonName.FromFullName("Ada Lovelace");

        name.First.Should().Be("Ada");
        name.Last.Should().Be("Lovelace");
    }

    [Test]
    public void FromFullName_WithNullOrWhitespace_ShouldReturnEmpty()
    {
        PersonName nullName = PersonName.FromFullName(null);
        PersonName whitespaceName = PersonName.FromFullName("   ");

        nullName.Should().BeSameAs(PersonName.Empty);
        whitespaceName.Should().BeSameAs(PersonName.Empty);
    }

    [Test]
    public void Equality_ShouldBeCaseInsensitive()
    {
        PersonName first = new("joHN", "doE");
        PersonName second = new("John", "Doe");

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Test]
    public void ToString_ShouldReturnFullName()
    {
        PersonName name = new("Grace", "Hopper");

        name.ToString().Should().Be("Grace Hopper");
    }
}
