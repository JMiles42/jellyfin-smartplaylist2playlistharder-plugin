namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class CompiledExpressionException : Exception
{
	public CompiledExpressionResult<Operand> CompiledExpression { get; }

	public CompiledExpressionException(CompiledExpressionResult<Operand> compiledExpression, Exception innerException)
			:base($"Error processing: {GetText(compiledExpression)}",innerException)
	{
		CompiledExpression = compiledExpression;
	}

	private static string GetText(CompiledExpressionResult<Operand> compiledExpression)
	{
		return JsonSerializer.Serialize(compiledExpression.ParsedValueExpression.SourceExpression);
	}
}
