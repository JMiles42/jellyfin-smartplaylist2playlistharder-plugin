using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class ExpressionExtensions
{
	public static string GetMemberName<T>(this Expression<T> expression) =>
			expression.Body switch
			{
				MemberExpression m => m.Member.Name,
				UnaryExpression { Operand: MemberExpression m, } => m.Member.Name,
				_ => throw new NotImplementedException(expression.GetType().ToString()),
			};

	public static bool TryGetMemberName<T>(this Expression<T> expression, out string Name)
	{
		switch (expression.Body)
		{
			case MemberExpression m:
				Name = m.Member.Name;

				return true;
			case UnaryExpression { Operand: MemberExpression m, }:
				Name = m.Member.Name;

				return true;
			default:
				Name = string.Empty;

				return false;
		}
	}
}
