using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomNotes.Utilities;

internal static class DirectoryInfoExtensions
{
    private static string[] FileTypes { get; } =
    [
        "bloq",
        "note"
    ];
    
    public static IEnumerable<FileInfo> EnumerateNoteFiles(this DirectoryInfo directory, SearchOption searchOption) =>
        FileTypes.SelectMany(type => directory.EnumerateFiles($"*.{type}", searchOption));
}