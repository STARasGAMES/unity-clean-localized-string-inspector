using UnityEngine;

namespace CleanLocalizedStringInspector
{
    public class CleanLocalizedStringAttribute : PropertyAttribute
    {
        public const string k_GuidTag = "GUID:";

        /// <summary>
        /// Overrides default string table reference.
        /// If null then default string table reference is used when creating a new localization key.
        /// If starts with `GUID:` prefix then string is parsed as table Guid reference.
        /// Otherwise, string is treated as table name reference.
        /// </summary>
        public string TableNameOrGuid { get; }

        public int LineCount { get; }

        public string KeyNameTemplate { get; }

        /// <summary>
        /// Marks LocalizedString field to be drawn using CleanLocalizedStringInspector.
        /// </summary>
        /// <param name="tableNameOrGuid">
        /// Indicates in what table put newly created localization keys.
        /// If null then default string table reference is used when creating a new localization key.
        /// If starts with <b>'GUID:'</b> prefix then string is parsed as table Guid reference.
        /// This Guid should be equal to AssetDatabase guid of the SharedData of the required table. You can grab it from <b>'.meta'</b> file.
        /// Otherwise, string is treated as table name reference.</param>
        /// <param name="lineCount">How many lines this field should use. In range [1..20]</param>
        /// <param name="keyNameTemplate">Provides a template for generator to create unique key name with additional context. See: <see cref="NameTemplateToken"/></param>
        public CleanLocalizedStringAttribute(string tableNameOrGuid = null, int lineCount = 1, string keyNameTemplate = NameTemplateToken.DefaultTemplate)
        {
            KeyNameTemplate = keyNameTemplate;
            TableNameOrGuid = tableNameOrGuid;
            LineCount = Mathf.Clamp(lineCount, 1, 20);
        }
    }
}