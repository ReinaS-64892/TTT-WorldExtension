using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace net.rs64.TexTransTool.DestructiveTextureUtilities
{
    static class UpdateDependentUtil
    {
        const string TEX_TRANS_TOOL_PACKAGE_DOT_JSON_PATH = "Packages/TexTransTool/package.json";
        const string THIS_PACKAGE_PATH = "Packages/TTT-WorldExtension";
        const string TARGET_PACKAGE_DOT_JSON_PATH = THIS_PACKAGE_PATH + "/package.json";
        const string TARGET_ASMDEF = THIS_PACKAGE_PATH + "/Editor/net.rs64.ttt-world-extension.asmdef";

        [InitializeOnLoadMethod]
        static void UpdateNow()
        {
            string tttVersion = GetTTTVersion();
            writeTTTVersion(tttVersion);
        }

        private static string GetTTTVersion()
        {
            var ttt = File.ReadAllText(TEX_TRANS_TOOL_PACKAGE_DOT_JSON_PATH).Split("\n");
            var vstr = "\"version\":";
            var tttVersionLine = ttt.First(str => str.Contains(vstr));
            var tttVersion = GetString(tttVersionLine);
            return tttVersion;
        }

        private static void writeTTTVersion(string tttVersion)
        {
            var tAsmdef = File.ReadAllText(TARGET_ASMDEF).Split("\n");
            var tpj = File.ReadAllText(TARGET_PACKAGE_DOT_JSON_PATH).Split("\n");
            foreach (var i in FindIndexAll(tpj, str => str.Contains("\"net.rs64.tex-trans-tool\":")))
            {
                tpj[i] = tpj[i].Replace(GetString(tpj[i]), tttVersion);
            }
            {
                var i = Array.FindIndex(tAsmdef, str => str.Contains("\"expression\":"));
                tAsmdef[i] = tAsmdef[i].Replace(GetString(tAsmdef[i]), $"[{tttVersion}]");
            }
            File.WriteAllText(TARGET_PACKAGE_DOT_JSON_PATH, string.Join("\n", tpj));
            File.WriteAllText(TARGET_ASMDEF, string.Join("\n", tAsmdef));
        }

        private static string GetString(string tttVersionLine)
        {
            var spIndex = tttVersionLine.IndexOf(":");
            var stringStart = tttVersionLine.IndexOf("\"", spIndex + 1);
            var stringEnd = tttVersionLine.LastIndexOf("\"");

            var stringIndex = stringStart + 1;
            var stringLength = stringEnd - stringIndex;

            return tttVersionLine.Substring(stringIndex, stringLength);
        }
        private static IEnumerable<int> FindIndexAll<T>(T[] array, Predicate<T> predicate)
        {
            for (var i = 0; i < array.Length; i += 1)
            {
                if (predicate.Invoke(array[i])) yield return i;
            }
        }
    }
}
