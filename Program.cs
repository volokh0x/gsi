using System;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Options;

namespace gsi
{
    static class Program
    {
        static int Main (string[] args)
        {
            var commands = new CommandSet ("gsi") {
                "use as: gsi [sub-command] ...",
                new InitCli(), 
                new TrackCli(),
                new UnTrackCli(), 
                new CommitCli(),
                new CreateBranchCli(),
                new SwitchCli(),
                new MergeCli(),
                new CatFileCli(),
                new LsFilesCli(),
                new LsBranchesCli(),
                new StatusCli()
            };
            return commands.Run(args);
        }
    }
}
