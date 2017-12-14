using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace ItsGoStats.Parsing
{
    class LogFileGroup
    {
        public IEnumerable<string> Files { get; }

        public static IObservable<LogFileGroup> FromDirectory(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return Directory.GetFiles(fullPath, "*.log")
                .Select(file => new { File = file, GroupKey = Path.GetFileNameWithoutExtension(file) })
                .GroupBy(s => s.GroupKey.Substring(0, s.GroupKey.Length - 4), s => s.File)
                .Select(grp => new LogFileGroup(grp.AsEnumerable()))
                .ToObservable();
        }

        public IObservable<string> ReadConcatenatedLines() => Observable.For(Files, file => File.ReadAllLines(file).ToObservable());

        public override string ToString() => $"Group: {string.Join(", ",Files.Select(Path.GetFileName))}";

        public LogFileGroup(IEnumerable<string> files) => Files = files ?? throw new ArgumentNullException(nameof(files));
    }
}
