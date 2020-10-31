using System;
using System.IO;
using gvaduha.JsonWithBlobs;
using Microsoft.Extensions.CommandLineUtils;

namespace blobfs_tool
{
    class Program
    {
        static int Main(string[] args)
        {
            var cla = new CommandLineApplication(throwOnUnexpectedArg: false);
            var cmdfsfile = cla.Option("-b | --bfs", "blob fs file", CommandOptionType.SingleValue);
            var cmdnewfs = cla.Option("-n | --new", "create new blob fs file", CommandOptionType.NoValue);
            var cmdwrite = cla.Option("-w | --write", "write files list", CommandOptionType.MultipleValue);

            cla.Execute(args);

            bool newFile = cmdnewfs.HasValue() ? true : false;

            string blobFile = cmdfsfile.Value();
            var files = cmdwrite.Values;

            if (blobFile == null || files == null)
            {
                cla.ShowHelp();
                Console.WriteLine($"Example:{Environment.NewLine}\t-b newbfs.blob -n -w test1 -w test2");
                return -1;
            }

            var blobfs = newFile
                ? new AdHocBlobFS()
                : new AdHocBlobFS(File.ReadAllBytes(blobFile));

            foreach (var f in files)
                blobfs.WriteFile(File.ReadAllBytes(f));

            File.WriteAllBytes(blobFile, blobfs.GetCurrentBlobFile());

            return 0;
        }
    }
}
