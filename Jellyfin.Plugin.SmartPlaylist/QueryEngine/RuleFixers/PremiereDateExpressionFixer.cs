namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.RuleFixers;

public class PremiereDateExpressionFixer: IExpressionFixer
{

	/// <inheritdoc />
	public void FixExpression(SmartPlExpression expression)
	{
		if (!expression.TargetValue.IsSingleValue)
		{
			return;
		}

		DateTime rst;

		switch (expression.TargetValue)
		{
			case ExpressionValue<DateTime> ev:
			{
				rst = ev.Value;

				break;
			}

			case ExpressionValueList<DateTime> ev:
			{
				rst = ev.Value.First();

				break;
			}
			case ExpressionValue<DateOnly> ev:
			{
				rst = ev.Value.ToDateTime(TimeOnly.MinValue);

				break;
			}

			case ExpressionValueList<DateOnly> ev:
			{
				rst = ev.Value.First().ToDateTime(TimeOnly.MinValue);

				break;
			}

			case ExpressionValue<string> ev:
			{
				rst = DateTime.Parse(ev.Value);

				break;
			}

			case ExpressionValueList<string> { IsSingleValue: true } ev:
			{
				rst = DateTime.Parse(ev.Value.First());

				break;
			}

			default: return;
		}

		expression.TargetValue = rst.ToString(CultureInfo.InvariantCulture).ToExpressionValue();
	}
}
