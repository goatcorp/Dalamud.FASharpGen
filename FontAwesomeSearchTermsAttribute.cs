// This is copied from Dalamud just to avoid FontAwesomeIcon.cs throwing errors.

using System;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ArrangeThisQualifier
// ReSharper disable CheckNamespace

namespace Dalamud.Interface;

/// <summary>
/// Set search terms associated with a font awesome icon.
/// </summary>
#pragma warning disable CA1018
public class FontAwesomeSearchTermsAttribute : Attribute
#pragma warning restore CA1018
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesomeSearchTermsAttribute"/> class.
    /// </summary>
    /// <param name="searchTerms">search terms for enum member.</param>
    public FontAwesomeSearchTermsAttribute(string[] searchTerms) => this.SearchTerms = searchTerms;

    /// <summary>
    /// Gets or sets search terms.
    /// </summary>
    public string[] SearchTerms { get; set; }
}
