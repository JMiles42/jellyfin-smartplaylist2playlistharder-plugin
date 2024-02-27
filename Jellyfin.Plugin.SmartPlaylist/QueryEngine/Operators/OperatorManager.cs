namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class OperatorManager
{
	public IOperator[] EngineOperators { get; } = {
			new TimeSpanOperator(),
			new StringOperator(),
			new IsNullOperator(),
			new RegexOperator(),
			new StringListContainsSubstringOperator(),
			new ExpressionTypeOperator(),
	};
}
