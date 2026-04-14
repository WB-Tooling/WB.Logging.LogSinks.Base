namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Has a writer of type <typeparamref name="TWriter"/> .
/// </summary>
/// <typeparam name="TWriter">The type of the <typeparamref name="TWriter"/>.</typeparam>
public interface IHasWriter<TWriter>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the <typeparamref name="TWriter"/>.
    /// </summary>
    public TWriter Writer { get; set;}
}