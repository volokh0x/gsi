using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace gsi
{
    static partial class GitCommand
    {
        public static void JsonDiffCmd(string path_or_hash1, string path_or_hash2, List<string> exclude, List<string> include, bool ignore_append, bool html_output)
        {
            // valid non-bare repo
            GitFS gitfs = new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr != null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            string text1 = File.Exists(path_or_hash1) ?
                File.ReadAllText(path_or_hash1) :
                new Blob(gitfs, gitfs.gitp.PathFromHash(path_or_hash1), false).Content;

            string text2 = File.Exists(path_or_hash2) ?
                File.ReadAllText(path_or_hash2) :
                new Blob(gitfs, gitfs.gitp.PathFromHash(path_or_hash2), false).Content;

            var jc = new JsonComparator(text1, text2, exclude, include, ignore_append);
            var diffRes = jc.CompareDicts();

            if (html_output)
            {
                Console.WriteLine(HtmlReportGenerator.GenerateHTML(diffRes, path_or_hash1, path_or_hash2));
            }
            else
            {
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.Indentation = 4;

                    JsonSerializer serializer = JsonSerializer.Create();
                    serializer.Serialize(jsonWriter, diffRes);
                    Console.WriteLine(stringWriter.ToString());
                }
            }
        }
    }
}