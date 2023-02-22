// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace FASharpGen;

public class FAIcon
{
    public string Name { get; set; }
    public string FormattedName { get; set; }
    public bool Included { get; set; }
    public string Label { get; set; }
    public string Unicode { get; set; }
    public string[] SearchTerms { get; set; }
    public string[] Styles { get; set; }
    public string[] FreeStyles { get; set; }
    public Aliases Aliases { get; set; }
}

public class Aliases
{
    public string[] Names { get; set; }
    public UnicodeAliases Unicodes { get; set; }
}

public class UnicodeAliases
{
    public string[] Primaries { get; set; }
    public string[] Secondaries { get; set; }
    public string[] Composites { get; set; }
}