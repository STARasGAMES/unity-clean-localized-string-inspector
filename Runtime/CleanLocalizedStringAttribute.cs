using System;
using System.Collections;
using System.Collections.Generic;
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

        public bool Multiline { get; }

        /// <summary>
        /// Marks LocalizedString field to be drawn using CleanLocalizedStringInspector.
        /// </summary>
        /// <param name="tableNameOrGuid">
        /// Indicates in what table put newly created localization keys.
        /// If null then default string table reference is used when creating a new localization key.
        /// If starts with `GUID:` prefix then string is parsed as table Guid reference.
        /// This Guid should be equal to AssetDatabase guid of the SharedData of the required table.
        /// 
        /// Otherwise, string is treated as table name reference.</param>
        /// <param name="multiline"></param>
        public CleanLocalizedStringAttribute(string tableNameOrGuid = null, bool multiline = false)
        {
            TableNameOrGuid = tableNameOrGuid;
            Multiline = multiline;
        }
    }
}