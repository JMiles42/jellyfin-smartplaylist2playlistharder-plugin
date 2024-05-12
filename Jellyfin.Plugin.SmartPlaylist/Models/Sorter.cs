namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class Sorter
{
	private readonly ConcurrentBag<Operand>               Items = new();
	private readonly SmartPlaylist                        _owner;
	private readonly List<CompiledExpressionSet<Operand>> _rules;

	internal Sorter(SmartPlaylist owner)
	{
		_owner = owner;
		_rules = _owner.GetCompiledRules();
	}

	public void SortItem(Operand item)
	{
		var matches = _rules.Count(set => ProcessRule(set, item));

		if (_owner.Match.DoesMatch(matches, _rules.Count))
		{
			Items.Add(item);
		}
	}

	public IEnumerable<BaseItem> GetResults() => _owner.Order.OrderItems(Items);

	private static bool ProcessRule(CompiledExpressionSet<Operand> set, Operand operand)
	{
		var setHits = 0;

		foreach (var cmd in set)
		{
			var runHits = cmd.Count(EvaluateExpression);

			if (cmd.Match.DoesMatch(runHits, cmd.Count))
			{
				setHits++;
			}

			continue;

			bool EvaluateExpression(CompiledExpressionResult<Operand> compiledExpression)
			{
				//TODO: Change return type to allow errors to be handled
				try
				{
					var value = compiledExpression.Expression(operand);

					if (compiledExpression
						.ParsedValueExpression
						.SourceExpression
						.InvertResult)
					{
						return !value;
					}

					return value;
				}
				catch (Exception ex)
				{
					throw new CompiledExpressionException(compiledExpression, ex);
				}
			}
		}

		return set.Match.DoesMatch(setHits, set.Count);
	}
}