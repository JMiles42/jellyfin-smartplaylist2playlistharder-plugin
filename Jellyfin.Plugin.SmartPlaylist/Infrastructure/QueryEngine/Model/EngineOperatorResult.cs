namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

public record EngineOperatorResult(string Name, EngineOperatorResultKind Kind)
{

    public static EngineOperatorResult Success() => new(nameof(Success), EngineOperatorResultKind.Success);

    public static EngineOperatorResult Success(string message) => new(message, EngineOperatorResultKind.Success);

    public static EngineOperatorResult NotAValidFor() =>
            new(nameof(NotAValidFor), EngineOperatorResultKind.NotAValidOperatorFor);

    public static EngineOperatorResult NotAValidFor(string message) =>
            new($"{nameof(NotAValidFor)}: {message}", EngineOperatorResultKind.NotAValidOperatorFor);

    public static EngineOperatorResult NotAValidFor(object message) =>
            new($"{nameof(NotAValidFor)}: {message}", EngineOperatorResultKind.NotAValidOperatorFor);

    public static EngineOperatorResult Error(string message) => new(message, EngineOperatorResultKind.Error);

    public static MultipleEngineOperatorResult Error(EngineOperatorResult a, EngineOperatorResult b) => new(a, b);

    public static MultipleEngineOperatorResult Error(params EngineOperatorResult[] errors) => new(errors);
}

public enum EngineOperatorResultKind
{
    NotAValidOperatorFor,
    Success,
    Error,
}

public record MultipleEngineOperatorResult : EngineOperatorResult
{
    public EngineOperatorResult[] OriginalErrors { get; }

    public MultipleEngineOperatorResult(params EngineOperatorResult[] errors) : base(string.Empty,
        EngineOperatorResultKind.Error)
    {
        OriginalErrors = errors;

        StringBuilder builder = new();
        builder.AppendLine("Multiple Errors Occurred.");

        foreach (var engineOperatorResult in errors)
        {
            builder.AppendLine(engineOperatorResult.Name);
        }

        Name = builder.ToString();
    }

    public MultipleEngineOperatorResult Add(EngineOperatorResult a) => new(OriginalErrors.Append(a).ToArray());
}
