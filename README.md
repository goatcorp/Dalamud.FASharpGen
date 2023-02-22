# Dalamud.FASharpGen

This app is used to generate the FontAwesomeIcon enum for Dalamud.

## How it works
- Downloads latest font and metadata from FontAwesome Github.
- Loads icons and categories from FontAwesome metadata.
- Loads current dalamud icons from `dalamud_icons.json`.
- Generates FontAwesomeIcon.cs for copying into Dalamud.
  - Adds Obsolete attribute to icons removed from FontAwesome or in excluded sets (e.g. brands).
  - Adds Categories/SearchTerms attributes based on FontAwesome metadata.
  - Excludes icons that have a hidden state per `dalamud_icons.json`.
  - Avoids changing unicode values by referencing `dalamud_icons.json`.
- Updates `dalamud_icons.json` with latest icons.

## How to use
1. Open in Rider or Visual Studio.
2. Run application.
3. Copy `FontAwesomeFreeSolid.otf` to DalamudAssets.
4. Copy `FontAwesomeIcon.cs` to Dalamud.
5. Commit `dalamud_icons.json` changes.

## How to hide icons
1. Update "state" in `dalamud_icons.json` to `2`.
2. Follow usual update steps.

## Sample Icon Enum Member
```csharp
    /// <summary>
    /// The Font Awesome "cow" icon unicode character.
    /// </summary>
    [FontAwesomeSearchTerms(new[] { "livestock", "mammal", "milk", "moo" })]
    [FontAwesomeCategoriesAttribute(new[] { "Animals", "Humanitarian" })]
    Cow = 0xF6C8,
```