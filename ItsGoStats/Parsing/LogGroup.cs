using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ItsGoStats.Parsing
{
    class LogGroup
    {
        public IReadOnlyCollection<string> Files { get; }

        public static IEnumerable<LogGroup> FromDirectory(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return Directory.EnumerateFiles(fullPath, "*.log")
                .Select(file => new { File = file, GroupKey = Path.GetFileNameWithoutExtension(file) })
                .GroupBy(s => s.GroupKey.Substring(0, s.GroupKey.Length - 4), s => s.File)
                .Select(grp => new LogGroup(grp.AsEnumerable()));
        }

        public async Task<string[]> ReadConcatenatedLinesAsync()
        {
            return await Task.Run(() =>
            {
                var fileLines = new List<string>(Files.SelectMany(f => File.ReadLines(f)));
                return fileLines.ToArray();
            });
        }

        public override string ToString() => $"Group: {string.Join(", ", Files.Select(Path.GetFileName))}";

        public LogGroup(IEnumerable<string> files) => Files = files.ToList().AsReadOnly();
    }
}
