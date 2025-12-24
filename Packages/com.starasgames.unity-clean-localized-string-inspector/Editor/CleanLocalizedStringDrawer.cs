using System;
using System.Linq;
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
            if (LocalizationSettings.HasSettings == false)
            {
                EditorGUI.LabelField(position, label.text, $"Localization is not properly setup in the project");
                return;
            }

            if (property.boxedValue is not LocalizedString localizedString)
            {
                EditorGUI.LabelField(position, label.text, $"Use {nameof(CleanLocalizedStringAttribute)} with {nameof(LocalizedString)}");
                return;
            }

            var selectedLocale = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale();
            var isLocaleSet = selectedLocale != null;
            var attr = attribute as CleanLocalizedStringAttribute;

            // This has to be drawn before property to be first to catch click event.
            DrawTableReferenceLinkToggle(position, property, localizedString);

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
                 */
                EditorGUI.BeginProperty(position, label, property);

                var fieldHeight = attr.IsMultiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;

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
                string newValue = attr.IsMultiline
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
                        // Check whether we can create a new localization key.
                        // Sidenote:
                        // Ideally, I would like to disable input field when we can't create new localization key.
                        // But this requires costly validations every editor Repaint, slowing down already slow editor gui.
                        // That's why we stick with last-moment-check. In theory, invalid state should be a rare thing, so shouldn't bother too much.
                        var tableRef = GetTableReferenceFromString(attr.TableNameOrGuid);
                        if (IsTableReferenceValid(tableRef, out var tableCollection) == false)
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

                        var hasTableForLocale = tableCollection.StringTables.Any(x => x.LocaleIdentifier == selectedLocale.Identifier);
                        if (hasTableForLocale == false)
                        {
                            var message = $"Table collection {tableCollection.name} doesn't have currently selected locale '{selectedLocale}'.\n" +
                                          $"Choose another locale or setup locale for this table collection.\n\n" +
                                          $"Localization key creation is canceled.";
                            Debug.LogError(message, tableCollection);
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

                // Yes, using goto is considered code smell, but we must call EditorGUI.EndProperty(), and in this case goto is the simplest solution.  
                EndPropertyLabel:
                EditorGUI.EndProperty();
            }

            DrawLocale(position, isLocaleSet);
            DrawTableReference(position, localizedString, attr.IsMultiline);
        }

        private static void DrawTableReferenceLinkToggle(Rect position, SerializedProperty property, LocalizedString localizedString)
        {
            var tableEntryLinkRect = new Rect(position.x + EditorGUIUtility.labelWidth - 16, position.y, 16, EditorGUIUtility.singleLineHeight);
            var isLinked = localizedString.IsEmpty == false;
            if (isLinked)
            {
                var tableEntryRefStr = GetDisplayableTableEntryReference(localizedString.TableReference, localizedString.TableEntryReference);
                UnlinkTableEntry.tooltip = $"Click to clear reference\n\n{tableEntryRefStr}";
            }

            EditorGUI.BeginDisabledGroup(localizedString.IsEmpty);
            EditorGUI.BeginChangeCheck();

            var newIsLinked = GUI.Toggle(tableEntryLinkRect, isLinked, isLinked ? UnlinkTableEntry : LinkTableEntry, Styles.linkButton);
            if (EditorGUI.EndChangeCheck() && newIsLinked == false)
            {
                property.boxedValue = new LocalizedString();
                Event.current.Use(); // Prevent further event propagation.
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawTableReference(Rect position, LocalizedString localizedString, bool isMultiline)
        {
            bool isHover = position.Contains(Event.current.mousePosition);
            if (isHover == false)
                return;

            var content = new GUIContent(GetDisplayableTableEntryReference(localizedString.TableReference, localizedString.TableEntryReference));
            var size = Styles.localeLabel.CalcSize(content);
            var rect = new Rect(EditorGUIUtility.labelWidth - size.x, position.y, size.x, EditorGUIUtility.singleLineHeight);
            // I didn't like how this offset looks and feels on multiline fields.
            // if (isMultiline) 
            //     rect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginDisabledGroup(false);
            EditorGUI.LabelField(rect, content, Styles.localeLabel);
            EditorGUI.EndDisabledGroup();
        }

        private static string GetDisplayableTableEntryReference(TableReference tableReference, TableEntryReference entryReference)
        {
            if (LocalizationSettings.HasSettings == false)
                return $"LocSettings are not initialized.";
            if (tableReference.ReferenceType == TableReference.Type.Empty)
                return "None";

            var stringTable = LocalizationEditorSettings.GetStringTableCollection(tableReference);
            if (stringTable == null)
                return "None";
            var sharedData = stringTable.SharedData;
            if (sharedData == null)
                return "None";

            var realEntryRef = sharedData.GetEntryFromReference(entryReference);

            var entryKey = realEntryRef != null ? realEntryRef.Key : "???";

            return $"{tableReference.TableCollectionName}/{entryKey}";
        }

        private static void DrawLocale(Rect position, bool isLocaleSet)
        {
            // When any text field is in focus - don't draw locale to prevent obscuring text.
            if (EditorGUIUtility.editingTextField)
                return;

            var selectedLocale = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale();
            var content = isLocaleSet ? new GUIContent(selectedLocale.Identifier.Code, "Currently selected locale in the project.") : NoSelectedLocale;
            var size = Styles.localeLabel.CalcSize(content);
            var localeLabelRect = new Rect(position.xMax - size.x - 2, position.y, size.x, EditorGUIUtility.singleLineHeight);
            localeLabelRect.y += 3;
            localeLabelRect.height -= 6;

            EditorGUI.LabelField(localeLabelRect, content, Styles.localeLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight;
            }

            var attr = attribute as CleanLocalizedStringAttribute;
            return attr.IsMultiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;
        }

        private static bool IsTableReferenceValid(TableReference tableReference, out StringTableCollection tableCollection)
        {
            tableCollection = null;
            if (tableReference.ReferenceType == TableReference.Type.Empty)
                return false;
            if (LocalizationSettings.HasSettings == false)
                return false;
            tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableReference);
            return tableCollection != null;
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