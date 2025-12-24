using System;
using System.Collections.Generic;
using CleanLocalizedStringInspector;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu()]
public class Weapon : ScriptableObject
{
    [CleanLocalizedString(tableNameOrGuid: ProjectTableReferences.WeaponTable)]
    public LocalizedString displayName;

    [CleanLocalizedString(tableNameOrGuid: ProjectTableReferences.WeaponTable, lineCount: 3)]
    public LocalizedString description;

    public List<LevelData> levels;

    [Serializable]
    public class LevelData
    {
        public int damageBoost;
        [CleanLocalizedString(tableNameOrGuid: ProjectTableReferences.WeaponTable)]
        public LocalizedString description;
    }
}