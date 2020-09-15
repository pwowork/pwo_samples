using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dropbox.testapp
{
    class Program
    {
        private static string url = @"https://www.dropbox.com/sh/4v0cyjzjqe1bee4/AACYw4cry8kGuFqfkAP3TcDca?dl=1";

        static void Main(string[] args)
        {
            Timer timer = new Timer((state) => DownloadFile(url), null, TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(1));
            Console.ReadKey();
//            DownloadFile(url);
        }

        private static void DownloadFile(string url)
        {
            var request = WebRequest.Create(url);
            var stream = new MemoryStream();
            var now = DateTime.Now;
            try
            {
                using (var response = request.GetResponse())
                {
                    using (var webStream = response.GetResponseStream())
                    {
                        webStream.CopyTo(stream);
                    }
                }

                stream.Position = 0;
                if (false)
//                if (stream.Length != 1121)
                {
                    using (var output = File.OpenWrite("errors.bin"))
                    {
                        output.Position = output.Length;
                        var text = Encoding.ASCII.GetBytes($"{now.ToShortDateString()} {now.ToLongTimeString()} {stream.Length} bytes {Environment.NewLine}");
                        output.Write(text, 0, text.Length);
                        stream.CopyTo(output);
                    }
                    throw new ApplicationException("invalid text length");
                }
                else if (stream.Length > 0)
                {
                    using (var archive = new ZipArchive(stream))
                    {
                        foreach (var zipArchiveEntry in archive.Entries)
                        {
                            var extension = Path.GetExtension(zipArchiveEntry.FullName);
                            if (extension == ".key")
                            {
                                var path = Path.Combine(Environment.CurrentDirectory, zipArchiveEntry.FullName);
                                zipArchiveEntry.ExtractToFile(path, true);
                            }
                        }
                    }
                }
                Console.WriteLine($"{now.ToString()}  processed in {DateTime.Now.Subtract(now).TotalSeconds:N} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{now.ToString()}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            finally
            {
                stream.Dispose();
            }
        }

    }
}
