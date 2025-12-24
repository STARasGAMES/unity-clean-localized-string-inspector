using System;
using CleanLocalizedStringInspector;
using UnityEngine;
using UnityEngine.Localization;

public class TestScript : MonoBehaviour
{
    public LocalizedString defaultDrawer;

    [Tooltip("Test tooltip")]
    [CleanLocalizedString()]
    public LocalizedString cleanDrawerDefaultTableRef;

    [Tooltip("Test tooltip")]
    [CleanLocalizedString(ProjectTableReferences.NameReferenceToTable)]
    public LocalizedString cleanDrawerNameTableRef;

    [Tooltip("Test tooltip")]
    [CleanLocalizedString(ProjectTableReferences.GuidReferenceToTable)]
    public LocalizedString cleanDrawerGuidTableRef;

    [Tooltip("This LocalizedString has an invalid name reference to the table. It should display error when you try to enter text.")]
    [CleanLocalizedString(ProjectTableReferences.InvalidNameReference)]
    public LocalizedString cleanDrawerInvalidNameTableRef;

    [Tooltip("This LocalizedString has an invalid name reference to the table. It should display error when you try to enter text.")]
    [CleanLocalizedString(ProjectTableReferences.InvalidGuidReference)]
    public LocalizedString cleanDrawerInvalidGuidTableRef;

    [Tooltip("Test tooltip")]
    [CleanLocalizedString(lineCount: 5)]
    public LocalizedString cleanDrawerMultiline;

    [CleanLocalizedString()]
    public string invalidFieldType;

    [Range(0f, 1f)]
    public string unityTest;

    public TestInnerMembers testInnerMembers = new TestInnerMembers();

    [Serializable]
    public class TestInnerMembers
    {
        public LocalizedString defaultDrawer;

        [Tooltip("Test tooltip")]
        [CleanLocalizedString()]
        public LocalizedString cleanDrawer;

        [CleanLocalizedString()]
        public string invalidFieldType;
    }
}