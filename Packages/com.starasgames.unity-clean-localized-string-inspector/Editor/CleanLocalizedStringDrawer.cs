using System;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace CleanLocalizedStringInspector.Editor
{
    [CustomPropertyDrawer(typeof(CleanLocalizedStringAttribute))]
    public class CleanLocalizedStringDrawer : PropertyDrawer
    {
        private static class Styles
        {
            public static readonly GUIStyle linkButton = (GUIStyle)"FloatFieldLinkButton";

            public static readonly GUIStyle localeLabel = new GUIStyle(EditorStyles.miniTextField)
            {
                fontSize = 10,
                fixedHeight = EditorGUIUtility.singleLineHeight - 6,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private static readonly GUIContent UnlinkTableEntry = new("", "");

        private static readonly GUIContent LinkTableEntry = new("",
            "Table entry reference is not specified.\nType text into the field to create a new localization key.\nOr expand field to select existing one.");

        private static readonly GUIContent NoSelectedLocale = new("no locale", 
            "No active locale selected.\nSet in:\nWindow/Asset Management/Localization Scene Controls");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Do not access any property of LocalizationSettings if HasSettings == false.
            var isLocaleSet = LocalizationSettings.HasSettings && LocalizationSettings.SelectedLocale != null;
            var localizedString = (LocalizedString)property.boxedValue;

            // Draw locale label
            {
                var localeLabelRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                    position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                localeLabelRect.x -= 16 * 4;
                localeLabelRect.width = 16 * 3;
                localeLabelRect.y += 3;
                localeLabelRect.height -= 6;

                EditorGUI.BeginDisabledGroup(true);
                if (isLocaleSet)
                {
                    var selectedLocale = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale();
                    EditorGUI.LabelField(localeLabelRect, new GUIContent(selectedLocale.Identifier.Code, "Currently selected locale in the project."), Styles.localeLabel);
                }
                else
                {
                    EditorGUI.LabelField(localeLabelRect, NoSelectedLocale, Styles.localeLabel);
                }

                EditorGUI.EndDisabledGroup();
            }

            // Draw table entry link/unlink toggle.
            {
                var tableEntryLinkRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                    position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                tableEntryLinkRect.x -= 16;
                tableEntryLinkRect.width = 16;
                EditorGUI.BeginDisabledGroup(localizedString.IsEmpty);
                EditorGUI.BeginChangeCheck();
                var isLinked = localizedString.IsEmpty == false;
                if (isLinked)
                {
                    var entryKey = localizedString.TableEntryReference.Key;
                    if (localizedString.TableEntryReference.ReferenceType == TableEntryReference.Type.Id)
                        entryKey = LocalizationSettings.StringDatabase.GetTable(localizedString.TableReference).SharedData.GetKey(localizedString.TableEntryReference.KeyId);
                    UnlinkTableEntry.tooltip = $"{localizedString.TableReference.TableCollectionName}/{entryKey}\n\nClick to clear reference";
                }

                var newIsLinked = GUI.Toggle(tableEntryLinkRect, isLinked, isLinked ? UnlinkTableEntry : LinkTableEntry, Styles.linkButton);
                if (EditorGUI.EndChangeCheck() && newIsLinked == false)
                {
                    property.boxedValue = new LocalizedString();
                    Event.current.Use();
                }

                EditorGUI.EndDisabledGroup();
            }

            if (property.isExpanded || isLocaleSet == false)
            {
                /*
                 * JUST DRAW THE REGULAR LOCALIZED STRING
                 */
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                /*
                 * IF WE CAN FIND A TRANSLATION IN THE CURRENT LOCALE:
                 * just draw a text field with that value.
                 *
                 */
                EditorGUI.BeginProperty(position, label, property);

                var attr = attribute as CleanLocalizedStringAttribute;
                var fieldHeight = attr.Multiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;

                var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - 32, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

                var textFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                    position.width - EditorGUIUtility.labelWidth, fieldHeight);


                var localeLabelRect = textFieldRect;
                localeLabelRect.x -= 16 * 4;
                localeLabelRect.width = 16 * 3;
                localeLabelRect.y += 3;
                localeLabelRect.height -= 6;


                StringTable table = null;
                StringTableEntry entry = null;
                string value = "";
                if (localizedString.IsEmpty == false)
                {
                    table = LocalizationSettings.StringDatabase.GetTable(localizedString.TableReference);
                    entry = table?.GetEntryFromReference(localizedString.TableEntryReference);
                    value = entry?.Value;
                }

                EditorGUI.BeginChangeCheck();
                string newValue = attr.Multiline
                    ? EditorGUI.TextArea(textFieldRect, value)
                    : EditorGUI.TextField(textFieldRect, value);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newValue))
                {
                    if (entry != null)
                    {
                        entry.Value = newValue;
                        EditorUtility.SetDirty(table);
                        EditorUtility.SetDirty(table.SharedData);
                    }
                    else
                    {
                        var tableRef = GetTableReferenceFromString(attr.TableNameOrGuid);
                        if (IsTableReferenceValid(tableRef) == false)
                        {
                            string message;
                            if (string.IsNullOrEmpty(attr.TableNameOrGuid))
                            {
                                message = $"Can't resolve default string table reference.\n" +
                                          $"You can set it here:\n" +
                                          $"Project Settings/Localization/String Database/Default Table Reference\n\n" +
                                          $"Localization key creation is canceled.";
                            }
                            else
                            {
                                message = $"Can't resolve table reference from '{attr.TableNameOrGuid}'.\n" +
                                          $"Specify valid {nameof(CleanLocalizedStringAttribute.TableNameOrGuid)} in the {nameof(CleanLocalizedStringAttribute)} for the field '{property.propertyPath}'\n\n" +
                                          $"Localization key creation is canceled.";
                            }

                            Debug.LogError(message);
                            EditorUtility.DisplayDialog("Error: Create new localization key", message, "OK");
                            goto EndPropertyLabel;
                        }

                        table = LocalizationSettings.StringDatabase.GetTable(tableRef);
                        var tempKey = $"TEMP_{System.Guid.NewGuid():N}";

                        entry = table.AddEntry(tempKey, newValue);
                        Debug.Log($"Created Entry in Table {table}: <b>{tempKey}</b> (will be renamed on save)");

                        EditorUtility.SetDirty(table);
                        EditorUtility.SetDirty(table.SharedData);

                        localizedString.TableReference = table.SharedData.TableCollectionNameGuid;
                        localizedString.TableEntryReference = entry.KeyId;
                        property.boxedValue = localizedString;

                        AssetDatabase.SaveAssets();
                    }
                }

                EndPropertyLabel:
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight;
            }

            var attr = attribute as CleanLocalizedStringAttribute;
            return attr.Multiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;
        }

        private static bool IsTableReferenceValid(TableReference tableReference)
        {
            if (tableReference.ReferenceType == TableReference.Type.Empty)
                return false;
            if (LocalizationSettings.HasSettings == false)
                return false;
            var table = LocalizationSettings.StringDatabase.GetTable(tableReference);
            return table != null;
        }

        private static TableReference GetTableReferenceFromString(string tableNameOrGuid)
        {
            if (string.IsNullOrEmpty(tableNameOrGuid))
            {
                if (LocalizationProjectSettings.instance != null && LocalizationProjectSettings.NewStringTable.IsEmpty == false)
                {
                    return LocalizationProjectSettings.NewStringTable.TableReference;
                }

                // Checking whether LocalizationSettings are present in the project.
                // We don't use LocalizationSettings.Instance right away, because this call automatically creates localization settings when there is no settings in the project.
                // And we don't want to be responsible for that.   
                if (LocalizationSettings.HasSettings == false)
                {
                    // ideally we catch absence of LocalizationSettings earlier in the call chain. But just in case repeat this here.
                    Debug.LogError($"This project has no LocalizationSettings asset. Head to ProjectSettings/Localization and setup LocalizationSettings.");
                    return (TableReference)null;
                }

                return LocalizationSettings.StringDatabase.DefaultTable;
            }

            // Check whether provided string is guid reference.
            if (IsGuid(tableNameOrGuid))
            {
                return (TableReference)GuidFromString(tableNameOrGuid);
            }

            return (TableReference)tableNameOrGuid;
        }

        /// <summary>
        /// Is the string identified as a <see cref="Guid"/> string.
        /// Strings that start with <see cref="CleanLocalizedStringAttribute.k_GuidTag"/> are considered a Guid.
        /// Note: Copied from TableReference.cs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsGuid(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            return value.StartsWith(CleanLocalizedStringAttribute.k_GuidTag, StringComparison.OrdinalIgnoreCase);
        }

        /// Note: Copied from TableReference.cs.
        internal static Guid GuidFromString(string value)
        {
            const string k_GuidTag = CleanLocalizedStringAttribute.k_GuidTag;
            if (Guid.TryParse(value.Substring(k_GuidTag.Length, value.Length - k_GuidTag.Length), out var guid))
                return guid;

            return Guid.Empty;
        }
    }
}