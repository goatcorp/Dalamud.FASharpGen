// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FASharpGen;

[DataContract]
public class DalamudIcon
{
    [DataMember(Name = "name")] public string Name { get; set; }
    [DataMember(Name = "unicode")] public string Unicode { get; set; }
    [DataMember(Name = "state")] public DalamudIconState State { get; set; }
    public bool UseLegacyUnicode { get; set; }
    public string FAUnicode { get; set; }
    public string FAName { get; set; }
    public string[] FASearchTerms { get; set; }
    public string[] FACategories { get; set; }

    public void MapFAFields(FAIcon faIcon)
    {
        FAUnicode = faIcon.Unicode;
        FAName = faIcon.Name;

        var searchTerms = new List<string>(faIcon.SearchTerms);
        if (!searchTerms.Contains(FAName))
        {
            searchTerms.Insert(0, FAName.Replace("-", " "));
        }

        FASearchTerms = searchTerms.ToArray();
        if (!faIcon.Included && State == DalamudIconState.Active)
        {
            State = DalamudIconState.Deprecated;
        }
    }

}