using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomNotes.Models;
using CustomNotes.Utilities;
using IPA.Utilities;

namespace CustomNotes.Managers;

internal class NoteFileManager
{
    private readonly DirectoryInfo customNotes;
    
    private NoteFileInfo[] loadedFiles = [];
    
    private static string[] FileTypes { get; } =
    [
        "note2",
        "bloq",
        "note"
    ];

    public NoteFileManager()
    {
        var customNotesPath = Path.Combine(UnityGame.InstallPath, "CustomNotes");
        customNotes = new(customNotesPath);
    }

    public NoteFileInfo[] GetLoadedNoteFiles() =>
        loadedFiles;
    
    public async Task<NoteFileInfo[]> ReloadAllFiles(CancellationToken token, IProgress<int>? progress = null) => 
        await Task.Run(() => GetDistinctFiles(token, progress), token);
    
    private NoteFileInfo[] GetDistinctFiles(CancellationToken token, IProgress<int>? progress)
    {
        progress?.Report(0);
        
        var fileInfos = EnumerateNoteFiles(customNotes, SearchOption.AllDirectories).ToList();
        int i = 0;
        int lastPercent = 0;
        var fileBag = new ConcurrentBag<NoteFileInfo>();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2 - 1,
            CancellationToken = token
        };
        
        Parallel.ForEach(fileInfos, parallelOptions, file =>
        {
            if (TryCreateFile(file, out var noteFileInfo)) fileBag.Add(noteFileInfo);
            
            int newPercent = (i + 1) * 100 / fileInfos.Count;
            if (newPercent != lastPercent)
            {
                progress?.Report(newPercent);
                lastPercent = newPercent;
            }
            i++;
        });

        loadedFiles = fileBag.Distinct(new NoteFileInfoHashComparer()).ToArray();
        return loadedFiles;
    }

    private static bool TryCreateFile(FileInfo file, [NotNullWhen(true)] out NoteFileInfo? saberFileInfo)
    {
        saberFileInfo = null;
        try
        {
            var saberHash = GetFileHash(file);
            if (string.IsNullOrEmpty(saberHash)) return false; 
            saberFileInfo = new(file, saberHash, DateTime.UtcNow);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetFileHash(FileInfo file)
    {
        using var fileStream = file.OpenRead();
        return MD5Checksum(fileStream, "x2");
    }

    private static string MD5Checksum(Stream stream, string format) =>
        stream.Length == 0 || !stream.CanRead || !stream.CanSeek ? string.Empty
            : GetMD5String(stream, format);

    private static string GetMD5String(Stream stream, string format)
    {
        using var md5 = MD5.Create();
        md5.ComputeHash(stream);
        return HashToString(md5.Hash, format) switch
        {
            { Length: > 0 } hashString => hashString,
            _ => string.Empty
        };
    }

    private static string HashToString(byte[] hashBytes, string format)
    {
        var sb = new StringBuilder();
        foreach (var s in hashBytes.Select(b => b.ToString(format)))
        {
            sb.Append(s);
        }
        return sb.ToString();
    }
    
    private static IEnumerable<FileInfo> EnumerateNoteFiles(DirectoryInfo directory, SearchOption searchOption) =>
        FileTypes.SelectMany(type => directory.EnumerateFiles($"*.{type}", searchOption));

    private class NoteFileInfoHashComparer : IEqualityComparer<NoteFileInfo>
    {
        public bool Equals(NoteFileInfo? a, NoteFileInfo? b) => a != null && b != null && a.Hash == b.Hash;
        public int GetHashCode(NoteFileInfo obj) => obj.Hash.GetHashCode();
    }
}