namespace TextCleaner
{
    public class CharacterRule
    {
        public string Find { get; set; } = string.Empty;
        public string ReplaceWith { get; set; } = string.Empty;

        public override string ToString()
        {
            string findDisplay = Find switch
            {
                "\u00A0" => "\\u00A0 (NBSP)",
                _ => Find
            };

            return string.IsNullOrEmpty(ReplaceWith)
                ? $"Remove \"{findDisplay}\""
                : $"Replace \"{findDisplay}\" → \"{ReplaceWith}\"";
        }
    }
}
