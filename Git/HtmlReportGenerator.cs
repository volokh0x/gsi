using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static class HtmlReportGenerator
{
    private static string htmlTemplate = @"<!DOCTYPE html>
<html>
<head>
    <title>{0}</title>
    <style>
        table {{
            border-collapse: collapse;
        }}
        td {{
            border: 1px solid black;
            padding: 0px 10px 0px 10px;
            text-align: center;
        }}
        .append {{
            background-color: #060;
            color: #fff;
        }}
        .remove {{
            background-color: #600;
            color: #fff;
        }}
        .update {{
            background-color: #006;
            color: #fff;
        }}
        .other {{
            background-color: #ddd;
            color: #000;
        }}
        .collapsible {{
            padding: 10px;
            font-size: 15px;

            outline-color: #fff;
            outline-style: solid;

            border: none;
            text-align: left;
            width: 100%;
            cursor: pointer;
        }}
        .node {{
            padding: 3px 0px 3px 0px;
        }}
        .leaf {{
            padding: 10px 0px 10px 10px;
            background-color: #ddd;
            color: #000;
        }}
        .content {{
            margin-left: 25px;
            display: none;
        }}
    </style>
</head>
<body>
    <h1>{0}</h1>

{1}

    <script>
    var coll = document.getElementsByClassName('collapsible');
    for (var i = 0; i < coll.length; i++) {{
        coll[i].addEventListener('click', function() {{
            this.classList.toggle('active');
            var content = this.nextElementSibling;
            if (content.style.display === 'block') {{
                content.style.display = 'none';
            }} else {{
                content.style.display = 'block';
            }}
        }});
    }}
    </script>
</body>
</html>";

    private static Dictionary<string, string> KeysToString = new Dictionary<string, string>
    {
        {"_append", "append"},
        {"_remove", "remove"},
        {"_update", "update"},
    };
    private static HashSet<string> ChangeKeys = new HashSet<string>(KeysToString.Keys);
    private static bool IsNode(JObject obj)
    {
        return obj.Properties().Select(p => p.Name).Any(ChangeKeys.Contains) || obj.Properties().Any(p => p.Value.Type == JTokenType.Object);
    }
    private static List<string> FormatObjectLines(JObject obj)
    {
        var lines = new List<string>();

        foreach (var property in obj.Properties())
        {
            string key = property.Name;
            JToken value = property.Value;

            bool special_key = ChangeKeys.Contains(key);
            if (special_key || value is JObject)
            {
                var value_object = value as JObject;
                string class_type = special_key ? KeysToString[key] : "other";
                string change_str = special_key ? KeysToString[key] : key;
                bool is_node = IsNode(value_object);
                lines.Add(string.Format("<button class='collapsible {0}'>{1}</button>", class_type, change_str));
                lines.Add(string.Format("<div class='content {0}'>", is_node ? "node" : "leaf"));
                if (!is_node)
                    lines.Add("<table>");
                lines.AddRange(FormatObjectLines(value_object as JObject).Select(line => "    " + line));
                if (!is_node)
                    lines.Add("</table>");
                lines.Add("</div>");
            }
            else
            {
                lines.Add(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", key, value.ToString()));
            }
        }

        return lines;
    }
    private static string FormatObject(JObject obj)
    {
        return string.Join(Environment.NewLine, FormatObjectLines(obj).Select(line => "    " + line));
    }
    public static string GenerateHTML(JObject diffRes, string file1, string file2)
    {
        string title = $"Diff: {file1} {file2}";
        string formattedHtml = FormatObject(diffRes);
        return string.Format(htmlTemplate, title, formattedHtml);
    }
}
