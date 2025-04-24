namespace Support
{
    public interface IMainWindow
    {
        void SetText(string text, LoggingLevel level = LoggingLevel.Info);
    }
    public enum LoggingLevel
    {
        Info, Warning, Error, Debug
    }
}