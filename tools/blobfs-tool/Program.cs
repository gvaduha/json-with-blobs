using System;
using System.IO;
using gvaduha.JsonWithBlobs;
using Microsoft.Extensions.CommandLineUtils;

namespace blobfs_tool
{
    class Program
    {
        enum CommandVerb { List, Extract, Write }

        static int Main(string[] args)
        {
            var cla = new CommandLineApplication(throwOnUnexpectedArg: false);
            var cmdfsfile = cla.Option("-b | --bfs", "blob fs file", CommandOptionType.SingleValue);
            var cmdnewfs = cla.Option("-n | --new", "create new blob fs file", CommandOptionType.NoValue);
            var cmdlist = cla.Option("-l | --list", "list files", CommandOptionType.NoValue);
            var cmdextract = cla.Option("-x | --extract", "extract all files to disk", CommandOptionType.NoValue);
            var cmdwrite = cla.Option("-w | --write", "write one or multiple files", CommandOptionType.MultipleValue);

            cla.Execute(args);

            var cmd = CommandVerb.List;
            if (cmdextract.HasValue()) cmd = CommandVerb.Extract;
            if (cmdwrite.HasValue()) cmd = CommandVerb.Write;

            string blobFile = cmdfsfile.Value();

            if (blobFile == null)
            {
                cla.ShowHelp();
                Console.WriteLine($"Example:{Environment.NewLine}\t-b blobfs.blob -x{Environment.NewLine}\t-b newbfs.blob -n -w test1 -w test2");
                return -1;
            }

            var blobfs = cmdnewfs.HasValue()
                ? new AdHocBlobFS()
                : new AdHocBlobFS(File.ReadAllBytes(blobFile));

            switch (cmd)
            {
                case CommandVerb.List:
                    Console.WriteLine($"There are {blobfs.GetFileCount()} files in {blobFile}");
                    break;
                case CommandVerb.Extract:
                    for(var i=0; i<blobfs.GetFileCount(); ++i)
                        File.WriteAllBytes($"{blobFile}-{i}.file", blobfs.ReadFile(i));
                    break;
                case CommandVerb.Write:
                    var files = cmdwrite.Values;
                    foreach (var f in files)
                        blobfs.WriteFile(File.ReadAllBytes(f));

                    File.WriteAllBytes(blobFile, blobfs.GetCurrentBlobFile());
                    break;
                default:
                    Console.WriteLine($"Unknown action {cmd}");
                    break;
            }

            return 0;
        }
    }
}
