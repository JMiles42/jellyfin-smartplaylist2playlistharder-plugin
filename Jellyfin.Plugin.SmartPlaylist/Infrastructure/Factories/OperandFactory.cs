namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;

public class OperandFactory
{
	private readonly ILibraryManager  _libraryManager;
	private readonly IUserDataManager _userDataManager;

	public OperandFactory(ILibraryManager  libraryManager,
						  IUserDataManager userDataManager)
	{
		_libraryManager  = libraryManager;
		_userDataManager = userDataManager;
	}

	public Operand Create(BaseItem baseItem,
						  User     user) =>
			new(baseItem, user, _libraryManager, _userDataManager);
}
