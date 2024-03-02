using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class Sorter {
	private readonly List<BaseItem>                  Items = new(1000);
	private readonly SmartPlaylist                   _owner;
	private readonly List<List<Func<Operand, bool>>> _rules;

	internal Sorter(SmartPlaylist owner) {
		_owner = owner;
		_rules = _owner.GetCompiledRules();
	}

	public void SortItem(Operand item) {

		if (_rules.Any(set => ProcessRule(set, item))) {
			Items.Add(item.BaseItem);
		}
	}

	public IEnumerable<BaseItem> GetResults() {
		var enumerable = _owner.Order.OrderItems(Items);

		return enumerable;
	}

	private static bool ProcessRule(List<Func<Operand, bool>> set, Operand operand) {
		return set.All(rule => rule(operand));
	}
}
