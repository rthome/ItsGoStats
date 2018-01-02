using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ItsGoStats
{
    sealed class ServerConfiguration
    {
        /// <summary>
        /// The path to the log directory
        /// </summary>
        public string LogDirectory { get; set; } = Environment.CurrentDirectory;

        /// <summary>
        /// The interval, in seconds, between scans for new log files
        /// </summary>
        public int LogScanInterval { get; set; } = 30 * 1000;

        /// <summary>
        /// The Uri and port to listen on
        /// </summary>
        public string ListenUri { get; set; } = "http://localhost:5555";

        #region Saving/Loading

        public static async Task<ServerConfiguration> LoadAsync(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                return await LoadAsync(stream);
        }

        public static async Task<ServerConfiguration> LoadAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ServerConfiguration>(content);
            }
        }

        public async Task SaveAsync(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous))
                await SaveAsync(stream);
        }

        public async Task SaveAsync(Stream stream)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var writer = new StreamWriter(stream))
                await writer.WriteLineAsync(json);
        }
        
        #endregion
    }
}
