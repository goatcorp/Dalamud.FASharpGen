using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable RedundantAssignment
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
namespace FASharpGen;

public static class Program
{
    private const string FontAwesomeIconPath = @"..\..\temp\icons.json";
    private const string FontAwesomeCategoryPath = @"..\..\temp\categories.yml";
    private const string DalamudIconPath = @"..\..\dalamud_icons.json";
    private const string IconEnumOutputPath = @"..\..\output\FontAwesomeIcon.cs";
    private const string FontOutputPath = @"..\..\output\FontAwesomeFreeSolid.otf";
    private const string FontAwesomeDirPath = @"..\..\temp\fa";
    private const string FontAwesomeZipPath = @"..\..\temp\fa.zip";
    private const string OutputPath = @"..\..\output";
    private static readonly StringBuilder Output = new();
    private static readonly Regex WsRegex = new(@"\s+");
    private static readonly List<FAIcon> FAIcons = new();
    private static readonly List<FACategory> FACategories = new();
    private static List<DalamudIcon> _dalamudIcons = new();
    private static string _faVersion = "";

    private static void Main()
    {
        DownloadFontAndMetaData();
        LoadFontAwesomeCategories();
        LoadDalamudIcons();
        LoadFontAwesomeIcons();
        UpdateExistingDalamudIcons();
        AddNewFontAwesomeIcons();
        AddFontAwesomeCategories();
        GenerateIconEnum();
        SaveDalamudIcons();
    }

    private static void DownloadFontAndMetaData()
    {
        var task = Task.Run(Download); 
        task.Wait();
    }
    
    private static async Task Download()
    {
        // setup
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("MyApplication", "1"));
        
        // get release
        const string releaseUrl = "https://api.github.com/repos/FortAwesome/Font-Awesome/releases/latest";
        var releaseJson = await httpClient.GetStringAsync(releaseUrl);
        var release = (JObject)JsonConvert.DeserializeObject(releaseJson);
        if (release == null) throw new Exception("Can't find release.");
        _faVersion = release["tag_name"]?.ToString();
        Console.WriteLine($"VERSION: {_faVersion}");

        // get asset
        var assets = release["assets"];
        if (assets == null) throw new Exception("Can't find release assets.");
        var downloadUrl = string.Empty; 
        foreach (var asset in assets)
        {
            if (asset["name"]!.ToString().Contains("desktop"))
            {
                downloadUrl = asset["browser_download_url"]!.ToString();
            }
        }
        if (string.IsNullOrEmpty(downloadUrl)) throw new Exception("Can't find desktop icons release asset.");
        Console.WriteLine($"DOWNLOAD_URL: {downloadUrl}");
        
        // download asset
        Directory.CreateDirectory(FontAwesomeDirPath);
        var response = await httpClient.GetByteArrayAsync(downloadUrl);
        File.WriteAllBytes(FontAwesomeZipPath, response);
        
        // unzip asset
        Directory.Delete(FontAwesomeDirPath, true);
        System.IO.Compression.ZipFile.ExtractToDirectory(FontAwesomeZipPath, FontAwesomeDirPath);
        
        // copy metadata to root
        var metadataPath = Path.Combine(Directory.GetDirectories(FontAwesomeDirPath, "fontawesome-free*")[0], "metadata");
        File.Copy(Path.Combine(metadataPath, "categories.yml"), FontAwesomeCategoryPath, true);
        File.Copy(Path.Combine(metadataPath, "icons.json"), FontAwesomeIconPath, true);

        // copy font to root
        Directory.CreateDirectory(OutputPath);
        var fontDirPath = Path.Combine(Directory.GetDirectories(FontAwesomeDirPath, "fontawesome-free*")[0], "otfs");
        var fontFilePath = Directory.GetFiles(fontDirPath, "Font Awesome*Solid*")[0];
        File.Copy(fontFilePath, FontOutputPath, true);
        
        // clean up
        Directory.Delete(FontAwesomeDirPath, true);
        File.Delete(FontAwesomeZipPath);
        httpClient.Dispose();
    }

    private static void SaveDalamudIcons()
    {
        File.WriteAllText(DalamudIconPath, JsonConvert.SerializeObject(_dalamudIcons, Formatting.Indented));
    }
    
    private static void AddFontAwesomeCategories()
    {
        foreach (var dalamudIcon in _dalamudIcons)
        {
            var categories = new List<string>();
            foreach (var faCategory in FACategories)
            {
                if (faCategory.IconNames.Contains(dalamudIcon.FAName))
                {
                    categories.Add(faCategory.Label);
                }
            }

            dalamudIcon.FACategories = categories.ToArray();
        }
    }
    
    private static void LoadFontAwesomeCategories()
    {
        // load yaml and convert to json
        var yaml = File.ReadAllText(FontAwesomeCategoryPath);
        var reader = new StringReader(yaml);
        var deserializer = new Deserializer();
        var yamlObject = deserializer.Deserialize(reader);
        var serializer = new JsonSerializer();
        var writer = new StringWriter();
        serializer.Serialize(writer, yamlObject);
        var json = writer.ToString();
        
        // parse into FACategory class
        var jsonData = JsonConvert.DeserializeObject<JObject>(json);
        foreach (var data in jsonData!.Properties())
        {
            var category = new FACategory
            {
                Name = data.Name,
                Label = data.Value["label"]!.ToString(),
                IconNames = data.Value!["icons"]!.Values<string>().ToArray()
            };
            FACategories.Add(category);
        }
    }
    
    private static void LoadDalamudIcons()
    {
        var json = File.ReadAllText(DalamudIconPath);
        _dalamudIcons = JsonConvert.DeserializeObject<List<DalamudIcon>>(json);
    }
    
    private static void LoadFontAwesomeIcons()
    {
        // load FAs metadata file
        var json = File.ReadAllText(FontAwesomeIconPath);
        var jsonData = JsonConvert.DeserializeObject<JObject>(json);
        
        // parse into FAIcon class
        foreach (var data in jsonData!.Properties())
        {
            var icon = new FAIcon
            {
                Name = data.Name,
                FormattedName = FormatName(data.Value["label"]!.ToString()),
                Label = data.Value["label"]!.ToString(),
                Unicode = FormatUnicode(data.Value["unicode"]!.ToString()),
                SearchTerms = data.Value!["search"]!["terms"]!.Values<string>().ToArray(),
                Styles = data.Value!["styles"]!.Values<string>().ToArray(),
                FreeStyles = data.Value!["free"]!.Values<string>().ToArray(),
            };
            var aliases = data.Value!["aliases"];
            if (aliases != null)
            {
                icon.Aliases = new Aliases();
                if (aliases["names"] != null)
                {
                    icon.Aliases.Names = aliases["names"].Values<string>().ToArray();
                }

                if (aliases["unicodes"] != null)
                {
                    icon.Aliases.Unicodes = new UnicodeAliases();
                    if (aliases["unicodes"]["primary"] != null)
                    {
                        icon.Aliases.Unicodes.Primaries = FormatUnicodes(aliases["unicodes"]["primary"].Values<string>().ToArray());
                    }
                    if (aliases["unicodes"]["secondary"] != null)
                    {
                        icon.Aliases.Unicodes.Secondaries = FormatUnicodes(aliases["unicodes"]["secondary"].Values<string>().ToArray());
                    }
                    if (aliases["unicodes"]["composite"] != null)
                    {
                        icon.Aliases.Unicodes.Composites = FormatUnicodes(aliases["unicodes"]["composite"].Values<string>().ToArray());
                    }
                }

            }

            // filter by font weight
            icon.Included = icon.FreeStyles.Contains("solid");
            FAIcons.Add(icon);
            
        }
    }

    private static void UpdateExistingDalamudIcons()
    {
        foreach (var dalamudIcon in _dalamudIcons)
        {
            var found = false;
            
            // check for match by current unicode or alias unicodes
            for (var i = 0; i < FAIcons.Count && !found; i++)
            {
                var faIcon = FAIcons[i];
                if (faIcon.Unicode == dalamudIcon.Unicode)
                {
                    found = true;
                    dalamudIcon.MapFAFields(faIcon);
                }
                else if (faIcon.Aliases?.Unicodes?.Primaries != null &&
                         faIcon.Aliases.Unicodes.Primaries.Contains(dalamudIcon.Unicode) || faIcon.Aliases?.Unicodes?.Secondaries != null &&
                         faIcon.Aliases.Unicodes.Secondaries.Contains(dalamudIcon.Unicode) || faIcon.Aliases?.Unicodes?.Composites != null &&
                         faIcon.Aliases.Unicodes.Composites.Contains(dalamudIcon.Unicode))
                {
                    found = true;
                    dalamudIcon.UseLegacyUnicode = true; // using legacy unicode value for bw compatability
                    dalamudIcon.MapFAFields(faIcon);
                }
            }
            
            if (!found)
            {
                // check for name matches
                for (var i = 0; i < FAIcons.Count && !found; i++)
                {
                    var faIcon = FAIcons[i];
                    if (dalamudIcon.Name != faIcon.FormattedName) continue;
                    found = true;
                    dalamudIcon.MapFAFields(faIcon);
                }
            }

            if (!found)
            {
                // mark as deprecated if still can't find
                dalamudIcon.State = DalamudIconState.Deprecated;
                dalamudIcon.FAName = dalamudIcon.Name.ToLower();
            }
        }
    }

    private static void AddNewFontAwesomeIcons()
    {
        var newIconCount = 0;
        foreach (var faIcon in FAIcons)
        {
            if (!faIcon.Included) continue;
            if (_dalamudIcons.Any(dalamudIcon => dalamudIcon.Unicode == faIcon.Unicode)) continue;
            if (_dalamudIcons.Any(dalamudIcon => dalamudIcon.FAUnicode == faIcon.Unicode)) continue;
            newIconCount++;
            var icon = new DalamudIcon
            {
                Name = faIcon.FormattedName,
                Unicode = faIcon.Unicode,
                State = DalamudIconState.Active,
                UseLegacyUnicode = false,
            };
            icon.MapFAFields(faIcon);
            _dalamudIcons.Add(icon);
        }
        
        Console.WriteLine($"NEW ICONS ADDED: {newIconCount}");
    }

    private static void GenerateIconEnum()
    {
        _dalamudIcons = _dalamudIcons.OrderBy(dalamudIcon => dalamudIcon.Name).ToList();

        // Add Header
        Output.AppendLine("//------------------------------------------------------------------------------");
        Output.AppendLine("// <auto-generated>");
        Output.AppendLine("// Generated by Dalamud.FASharpGen - don't modify this file directly.");
        Output.AppendLine($"// Font-Awesome Version: {_faVersion}");
        Output.AppendLine("// </auto-generated>");
        Output.AppendLine("//------------------------------------------------------------------------------");
        Output.Append(Environment.NewLine);
        Output.AppendLine("using System;");
        Output.Append(Environment.NewLine);
        Output.AppendLine("namespace Dalamud.Interface;");
        Output.Append(Environment.NewLine);
        Output.AppendLine("/// <summary>");
        Output.AppendLine("/// Font Awesome unicode characters for use with the <see cref=\"UiBuilder.IconFont\"/> font.");
        Output.AppendLine("/// </summary>");
        Output.AppendLine("public enum FontAwesomeIcon");
        Output.AppendLine("{");
        
        // Add "None" icon for placeholder
        Output.AppendLine("    /// <summary>");
        Output.AppendLine("    /// No icon.");
        Output.AppendLine("    /// </summary>");
        Output.AppendLine("    None = 0,");
        Output.Append(Environment.NewLine);
        
        // Add icons
        foreach (var icon in _dalamudIcons)
        {
            if (icon.State == DalamudIconState.Hidden) continue;

            Output.AppendLine("    /// <summary>");
            if (!string.IsNullOrEmpty(icon.FAName))
            {
                Output.AppendLine($"    /// The Font Awesome \"{icon.FAName}\" icon unicode character.");
            }
            else
            {
                Output.AppendLine($"    /// The Font Awesome \"{icon.Name}\" icon unicode character.");
            }

            if (icon.UseLegacyUnicode)
            {
                Output.AppendLine($"    /// Uses a legacy unicode value for backwards compatability. The current unicode value is {icon.FAUnicode}.");
            }

            Output.AppendLine("    /// </summary>");

            if (icon.State == DalamudIconState.Deprecated)
            {
                Output.AppendLine("    [Obsolete]");
            }

            if (icon.FASearchTerms?.Length > 0)
            {
                var terms = icon.FASearchTerms.Select(term => $"\"{term.ToLower()}\"").ToList();
                Output.AppendLine("    [FontAwesomeSearchTerms(new[] { " + string.Join(", ", terms) +" })]");
            }
            
            if (icon.FACategories?.Length > 0)
            {
                var categories = icon.FACategories.Select(category => $"\"{category}\"").ToList();
                Output.AppendLine("    [FontAwesomeCategoriesAttribute(new[] { " + string.Join(", ", categories) +" })]");
            }

            Output.AppendLine($"    {icon.Name} = {icon.Unicode},");
            Output.Append(Environment.NewLine);
        }
        
        // Close enum
        Output.AppendLine("}");
        
        // Write to file
        File.WriteAllText(IconEnumOutputPath, Output.ToString());
    }

    private static string FormatName(string label) {
        var name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(label);
        name = WsRegex.Replace(name, string.Empty);
        return name.Length == 1 && char.IsDigit(name.First()) ? $"_{name}" : name;
    }

    private static string FormatUnicode(string unicode)
    {
        return $"0x{unicode.ToUpper()}";
    }
    
    private static string[] FormatUnicodes(string[] unicodes)
    {
        for (var i = 0; i < unicodes.Length; i++)
        {
            unicodes[i] = FormatUnicode(unicodes[i]);
        }

        return unicodes;
    }
}
