using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Utils.SimpleLocalizedStringDrawer.Editor
{
[CustomPropertyDrawer(typeof(SimpleLocalizedStringAttribute))]
public class SimpleLocalizedStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var isLocaleSet = (LocalizationSettings.SelectedLocale != null);

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

            var attr = attribute as SimpleLocalizedStringAttribute;
            var fieldHeight = attr.Multiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;

            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            var textFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth, fieldHeight);

            var localizedString = (LocalizedString)property.boxedValue;

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
                    table = LocalizationSettings.StringDatabase.GetTable(attr.TableName);
                    var tempKey = $"TEMP_{System.Guid.NewGuid():N}";

                    entry = table.AddEntry(tempKey, newValue);
                    Debug.Log($"Created Entry in Table {table}: <b>{tempKey}</b> (will be renamed on save)");

                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);

                    localizedString.TableReference = table.SharedData.TableCollectionNameGuid;
                    localizedString.TableEntryReference = entry.KeyId;
                    property.boxedValue = localizedString;
                }
            }

            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight;
        }

        var attr = attribute as SimpleLocalizedStringAttribute;
        return attr.Multiline ? EditorGUIUtility.singleLineHeight * 3 : EditorGUIUtility.singleLineHeight;
    }
}
}