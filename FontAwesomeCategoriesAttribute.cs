// This is copied from Dalamud just to avoid FontAwesomeIcon.cs throwing errors.

using System;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ArrangeThisQualifier
// ReSharper disable CheckNamespace

namespace Dalamud.Interface;

/// <summary>
/// Set categories associated with a font awesome icon.
/// </summary>
public class FontAwesomeCategoriesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesomeCategoriesAttribute"/> class.
    /// </summary>
    /// <param name="categories">categories for enum member.</param>
    public FontAwesomeCategoriesAttribute(string[] categories) => this.Categories = categories;

    /// <summary>
    /// Gets or sets categories.
    /// </summary>
    public string[] Categories { get; set; }
}
