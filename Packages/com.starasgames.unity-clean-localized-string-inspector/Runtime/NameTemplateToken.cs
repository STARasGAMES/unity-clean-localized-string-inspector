namespace CleanLocalizedStringInspector
{
    public static class NameTemplateToken
    {
        public const string TkPropertyName = "{PropName}";
        public const string TkPropertyPath = "{PropPath}";
        public const string TkObjectName = "{ObjectName}";
        public const string TkNewLongGuid = "{NewLongGuid}";
        public const string TkNewShortGuid = "{NewShortGuid}";
        public const string TkEntryId = "{EntryId}";
        public const string TkEntryIdHex = "{EntryIdHex}";
        
        public const string DefaultTemplate = TkObjectName + "." + TkPropertyPath;
    }
}