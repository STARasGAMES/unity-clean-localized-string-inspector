## Why
Default Unity's property look:

<img width="635" height="129" alt="image" src="https://github.com/user-attachments/assets/bbe845ac-b6b1-4880-bc28-710a70d54274" />

Improved property look and UX:

<img width="636" height="160" alt="image" src="https://github.com/user-attachments/assets/7511cfcf-9161-42e8-8ab6-a4bb7d378216" />

All you need to do is mark `LocalizedString` field with `[CleanLocalizedString]` attribute:
```c#
using UnityEngine;
using UnityEngine.Localization;
using CleanLocalizedStringInspector;

public class SampleScript : MonoBehaviour
{
    public LocalizedString defaultDrawer;

    [CleanLocalizedString] // Single line
    public LocalizedString cleanDrawer;

    [CleanLocalizedString(lineCount: 3)] // Multiline
    public LocalizedString cleanDrawerMultiline;
}
```

## Warning: Work-In-Progress 
This is not a battle-tested solution, but rather a quick prototype made in two evenings. 

## More features
 - quickly clean table entry reference.
 - quickly create a new entry by simply typing into the text field.
 - set table reference where to put new localization keys per field.
 - context-aware key name generation using templates.

## Installation
Install via git url by adding this entry in your **manifest.json**

`"com.starasgames.unity-clean-localized-string-inspector": "https://github.com/STARasGAMES/com.starasgames.unity-clean-localized-string-inspector.git#upm"`

## Credits

Based on this brilliant code by Thomas "noio" van den Berg:
https://gist.github.com/noio/98a2b480321128ee4926973e33da0381
