namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class TypeExtensions {
	private static readonly Dictionary<Type, string> primitiveTypes = new ()
	{
		{ typeof(bool), "bool" },
		{ typeof(byte), "byte" },
		{ typeof(char), "char" },
		{ typeof(decimal), "decimal" },
		{ typeof(double), "double" },
		{ typeof(float), "float" },
		{ typeof(int), "int" },
		{ typeof(long), "long" },
		{ typeof(sbyte), "sbyte" },
		{ typeof(short), "short" },
		{ typeof(string), "string" },
		{ typeof(uint), "uint" },
		{ typeof(ulong), "ulong" },
		{ typeof(ushort), "ushort" },
	};

	public static string GetCSharpName(this Type type)
	{

		if (primitiveTypes.TryGetValue(type, out var primitiveResult))
		{
			return primitiveResult;
		}


		var    elementType = type.GetElementType();
		if (type.IsArray && elementType is not null) {
			return GetCSharpName(elementType) + "[]";
		}

		string result      = type.Name.Replace('+', '.');

		if (!type.IsGenericType) {
			return result;
		}

		if (type is { IsNested: true, DeclaringType.IsGenericType: true, })
		{
			throw new NotImplementedException();
		}

		if (type.Name == "Nullable`1")
		{
			return type.GetGenericArguments().First().GetCSharpName() + "?";
		}

		result = result[..result.IndexOf("`", StringComparison.Ordinal)];

		return result + "<" + string.Join(", ", type.GetGenericArguments().Select(GetCSharpName)) + ">";
	}
}
