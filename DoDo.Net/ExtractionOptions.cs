namespace DoDo.Net;

public class ExtractionOptions
{
    public int FallbackPlainTextMaxFileSizeBytes { get; set; } = 1024 * 1024; // 1MB
    public ExtractionErrorHandling ErrorHandling { get; set; } = ExtractionErrorHandling.ContinueOnError;
    public ExtractionConcurrency Concurrency { get; set; } = ExtractionConcurrency.Parallel;
    public OutputFormat PreferredOutputFormat { get; set; } = OutputFormat.Markdown;

    public static ExtractionOptions Default => new();
}

public enum ExtractionErrorHandling
{
    /// <summary>
    ///     Throw an exception on the first error encountered
    /// </summary>
    ThrowOnFirstError,

    /// <summary>
    ///     Continue processing files even if some errors occur
    /// </summary>
    ContinueOnError
}

public enum ExtractionConcurrency
{
    /// <summary>
    ///     Process files sequentially, one at a time
    /// </summary>
    Sequential,

    /// <summary>
    ///     Process files in parallel using multiple threads
    /// </summary>
    Parallel
}

public enum OutputFormat
{
    PlainText,
    Markdown
}