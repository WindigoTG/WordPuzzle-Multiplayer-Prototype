namespace WordPuzzle
{
    public interface ICrosswordService
    {
        int ThemesCount { get; }
        Theme GetThemeByIndex(int index);

        string GetThemeNameByIndex(int index);
    }
}