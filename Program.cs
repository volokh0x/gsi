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
                new AddCli(),
                new RmCli(),
                new CommitCli(),
                new BranchCli(),
                new CheckoutCli(),
                new MergeCli(),
                new CatFileCli(),
                new LsFilesCli(),
                new StatusCli()
            };
            return commands.Run(args);
        }
    }
}
