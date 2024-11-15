using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Netko.Download
{
    public class Range
    {
        public long from { get; set; }
        public long to { get; set; }
    }

    public class Downloader
    {
        private readonly object _lock = new object();
        private FileStream FileStream { get; set; }
        private int TotalThread { get; set; }
        private int CurrentThread { get; set; }
        private string Url { get; set; }
        private string UserAgent { get; set; }

        private long downloadSize = 0;
        private float downloadProgress = 0;
        private long downloaded = 0;
        private long totalSize = 0;

        public long downloadBlockSize = 5054287;
        public Downloader(string url, string user_agent, string file_path, long total_size, int thread)
        {
            TotalThread = thread;
            CurrentThread = 0;
            Url = url;
            UserAgent = user_agent;
            totalSize = total_size;

            FileStream = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
        private void CalcProgress()
        {
            if (downloadSize > 0)
            {
                downloadProgress = totalSize / downloadSize;
            }
            Trace.WriteLine(downloadProgress.ToString() + "\r");
        }
        private List<Range> CalcRange()
        {
            List<Range> ranges = new List<Range>();
            long calced_size = 0;
            while (true)
            {
                if (calced_size + downloadBlockSize > totalSize)
                {
                    ranges.Add(new Range
                    {
                        from = calced_size,
                        to = totalSize,
                    });
                    break;

                }
                else
                {
                    ranges.Add(new Range
                    {
                        from = calced_size,
                        to = calced_size + downloadBlockSize,
                    });
                }
                calced_size += downloadBlockSize + 1;

            }
            return ranges;
        }

        public async Task DownloadThread(Range range)
        {

            long pointer = range.from;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Range", $"bytes={range.from}-{range.to}");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
                using (HttpResponseMessage response = await client.GetAsync(Url))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalSize = response.Content.Headers.ContentLength;
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            //await write 
                            downloaded += bytesRead;
                            lock (_lock)
                            {
                                Console.WriteLine("WriteTo: " + pointer.ToString() + " By: " + range.from.ToString());
                                CalcRange();
                                FileStream.Seek(pointer, SeekOrigin.Begin);
                                FileStream.Write(buffer, 0, bytesRead);
                            }
                            pointer += bytesRead;

                        }
                    }
                }
            }
        }
        public void releaseFile()
        {
            Console.WriteLine("File has been relese");
            lock (_lock)
            {
                FileStream.Close();
            }
        }
        public async void Run()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(TotalThread);
            Task[] tasks = new Task[TotalThread];
            int thread_id = 0;
            foreach (Range range in CalcRange())
            {
                await semaphore.WaitAsync();
                tasks[thread_id] = Task.Run(async () =>
                {
                    try
                    {
                        await DownloadThread(range);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
            await Task.WhenAll(tasks);
            releaseFile();
        }

    }

}
