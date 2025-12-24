using System;
using System.Linq;
using UnityEditor;
using UnityEngine.Localization.Tables;
using static CleanLocalizedStringInspector.NameTemplateToken;

namespace CleanLocalizedStringInspector.Editor
{
    public static class LocalizationKeyNameGenerator
    {
        public static string GetKeyNameFromTemplate(string template, SerializedProperty property, StringTableEntry tableEntry, SharedTableData sharedTableData)
        {
            var result = template
                    .SmartReplace(TkObjectName, () => property.serializedObject.targetObject.name)
                    .SmartReplace(TkPropertyName, () =>
                    {
                        var lastSlash = property.propertyPath.LastIndexOf('/');
                        if (lastSlash == -1) lastSlash = 0;
                        return property.propertyPath[lastSlash..];
                    })
                    .SmartReplace(TkPropertyPath, () => property.propertyPath.Replace(".Array.data[", "[").Replace("/", "."))
                    .SmartReplace(TkNewLongGuid, () => System.Guid.NewGuid().ToString("N"))
                    .SmartReplace(TkNewShortGuid, () => System.Guid.NewGuid().ToString("N")[16..])
                    .SmartReplace(TkEntryId, () => tableEntry.KeyId.ToString())
                    .SmartReplace(TkEntryIdHex, () => tableEntry.KeyId.ToString("X"))
                ;

            if (sharedTableData.Contains(result))
            {
                var baseLength = result.Length;
                var similarEntries = sharedTableData.Entries.Where(x => x.Key.EndsWith(result)).ToArray();
                int maxIndex = 0;
                foreach (var e in similarEntries)
                {
                    var prefix = e.Key.Substring(e.Key.Length - baseLength);
                    if (string.IsNullOrEmpty(prefix) || prefix[^1] != '_')
                        continue;
                    if (int.TryParse(prefix[..^1], out var num) && num > maxIndex)
                        maxIndex = num;
                }

                result = $"{maxIndex + 1}_{result}";
            }

            return result;
        }

        private static string SmartReplace(this string str, string pattern, Func<string> replacement)
        {
            if (str.Contains(pattern, StringComparison.Ordinal))
                return str.Replace(pattern, replacement.Invoke(), StringComparison.Ordinal);
            return str;
        }
    }
}