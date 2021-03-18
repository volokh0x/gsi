namespace gsi
{
    interface IRef
    {
        Commit GetCommit();
        string GetCommitHash();
    }
}