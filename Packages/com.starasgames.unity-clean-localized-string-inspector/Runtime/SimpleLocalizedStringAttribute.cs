using UnityEngine;

namespace Utils.SimpleLocalizedStringDrawer
{
    public class SimpleLocalizedStringAttribute : PropertyAttribute
    {
        public string TableName { get; }
        public bool Multiline { get; }

        public SimpleLocalizedStringAttribute(string tableName, bool multiline = false)
        {
            TableName = tableName;
            Multiline = multiline;
        }
    }
}