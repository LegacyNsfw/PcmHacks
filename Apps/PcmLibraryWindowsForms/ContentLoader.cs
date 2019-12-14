using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Encapsulates code to load content (HTML, text) from network, local cache, or embedded resources.
    /// </summary>
    public class ContentLoader
    {
        private readonly string fileName;
        private readonly string appVersion;
        private readonly Assembly assembly;
        private readonly ILogger logger;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentLoader(string fileName, string appVersion, Assembly assembly, ILogger logger)
        {
            this.fileName = fileName;
            this.appVersion = appVersion;
            this.assembly = assembly;
            this.logger = logger;
        }

        /// <summary>
        /// Get content from (in order of preference) network, local cache, or embedded resource.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetContentStream()
        {
            Stream result = await TryGetContentFromNetwork();
            if (result != null)
            {
                return result;
            }

            result = this.TryGetContentFromCache();
            if (result != null)
            {
                return result;
            }

            this.logger.AddDebugMessage("Loading " + fileName + " from embedded resource.");
            var resourceName = "PcmHacking." + fileName;
            return this.assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Get the URL to a file on github, using the release branch or the develop branch.
        /// </summary>
        private string GetFileUrl(string path)
        {
            string urlBase = "https://raw.githubusercontent.com/LegacyNsfw/PcmHacks/";
            string branch = this.appVersion == null ? "develop" : "Release/" + this.appVersion;
            string result = urlBase + branch + path;
            return result;
        }

        /// <summary>
        /// Gets the path to a file in the local cache.
        /// </summary>
        private string GetCacheFilePath()
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PcmHacking",
                "ContentCache");

            Directory.CreateDirectory(directory);

            return Path.Combine(directory, this.fileName);
        }

        /// <summary>
        /// Try to get content from the network.
        /// </summary>
        private async Task<Stream> TryGetContentFromNetwork()
        {
            Stream stream = null;

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    GetFileUrl("/Apps/PcmHammer/" + fileName));

                request.Headers.Add("Cache-Control", "no-cache");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stream = await response.Content.ReadAsStreamAsync();

                    // Store locally in case the network isn't available next time.
                    try
                    {
                        string path = this.GetCacheFilePath();
                        using (Stream file = File.OpenWrite(path))
                        {
                            await stream.CopyToAsync(file);
                        }
                    }
                    catch (Exception saveException)
                    {
                        this.logger.AddDebugMessage("Unable to cache " + fileName + ": " + saveException.ToString());
                    }
                    finally
                    {
                        // Surprisingly, you actually can rewind a network stream.
                        // Something in .net or the OS must be caching it somewhere.
                        stream.Position = 0;
                    }

                    this.logger.AddDebugMessage("Loaded " + this.fileName + " from network.");
                    return stream;
                }
                else
                {
                    this.logger.AddDebugMessage("Unable to retrieve " + fileName + " from network: HTTP " + response.StatusCode + ".");
                    return null;
                }
            }
            catch (Exception exception)
            {
                this.logger.AddDebugMessage("Unable to retrieve " + fileName + " from network: " + exception.ToString());
                return null;
            }
        }

        /// <summary>
        /// Try to get content from the local cache.
        /// </summary>
        private Stream TryGetContentFromCache()
        {
            string path = GetCacheFilePath();
            try
            {
                Stream result = File.OpenRead(path);
                this.logger.AddDebugMessage("Loaded " + this.fileName + " from cache.");
                return result;
            }
            catch (Exception exception)
            {
                this.logger.AddDebugMessage("Unable to retrieve " + fileName + " from cache: " + exception.ToString());
                return null;
            }
        }
    }
}
