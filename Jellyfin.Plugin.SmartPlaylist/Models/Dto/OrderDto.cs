namespace Jellyfin.Plugin.SmartPlaylist.Models.Dto;

public interface IOrderDetails
{
	public string Name { get; set; }
	public bool Ascending { get; set; }
	public bool IsInValid { get; }
}

public class OrderDto: IOrderDetails
{
	public string Name { get; set; }
	public bool Ascending { get; set; } = true;
	public bool IsInValid { get; } = false;
}

public class OrderByDto: IOrderDetails, IEnumerable<IOrderDetails>
{
	public List<OrderDto> ThenBy { get; set; } = new();

	/// <inheritdoc />
	public IEnumerator<IOrderDetails> GetEnumerator()
	{
		yield return this;

		foreach (var orderDto in ThenBy)
		{
			yield return orderDto;
		}
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public string Name { get; set; }

	public bool Ascending { get; set; } = true;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool IsInValid => !OrderManager.IsValidOrderName(Name);
}
