using System;
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
            var lineArrays = new List<string[]>();
            foreach (var file in Files)
            {
                using (var reader = File.OpenText(file))
                {
                    var fileData = await reader.ReadToEndAsync();
                    lineArrays.Add(fileData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            int copyOffset = 0;
            var concatenatedArray = new string[lineArrays.Sum(l => l.Length)];
            foreach (var lineArray in lineArrays)
            {
                Array.Copy(lineArray, 0, concatenatedArray, copyOffset, lineArray.Length);
                copyOffset += lineArray.Length;
            }
            return concatenatedArray;
        }

        public override string ToString() => $"Group: {string.Join(", ",Files.Select(Path.GetFileName))}";

        public LogGroup(IEnumerable<string> files) => Files = files.ToList().AsReadOnly();
    }
}
