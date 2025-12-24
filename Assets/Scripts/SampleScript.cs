using CleanLocalizedStringInspector;
using UnityEngine;
using UnityEngine.Localization;

public class SampleScript : MonoBehaviour
{
        public LocalizedString defaultDrawer;
        [CleanLocalizedString]
        public LocalizedString cleanDrawer;
        [CleanLocalizedString(isMultiline: true)]
        public LocalizedString cleanDrawerMultiline;
}