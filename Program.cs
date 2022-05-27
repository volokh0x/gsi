using Mono.Options;

namespace gsi
{
    static class Program
    {
        static int Main(string[] args)
        {
            var commands = new CommandSet("gsi") {
                "use as: gsi [sub-command] ...",
                new InitCli(),
                new TrackCli(),
                new TmpCli(),
                new UnTrackCli(),
                new CommitCli(),
                new RestoreCli(),
                new CleanHistoryCli(),
                new BranchCli(),
                new SwitchCli(),
                new MergeCli(),
                new CatFileCli(),
                new LsFilesCli(),
                new StatusCli()
            };
            return commands.Run(args);
        }
    }
}
