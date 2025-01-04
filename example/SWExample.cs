using Godot;
using Godot.Collections;
using guilleatm.SilentWolf;

public partial class SWExample : Node
{
	public override void _Ready()
	{
		// We use call deferred because SW creates some HttpRequest nodes. We can't create nodes in _Ready so we delay the call.
		CallDeferred(nameof(EasySW));
	}

	void EasySW()
	{
		const string API_KEY = "YOUR_API_KEY";
		const string GAME_ID = "YOUR_GAME_ID";

		SW.SWConfig swConfig = new SW.SWConfig(API_KEY, GAME_ID);
		SW sw = new SW(swConfig, GetTree().Root );

		string user = "my_new_user";
		string password = "not3asyPWD_";

		sw.Register(user, password, successListener: _OnRegisterSuccess);


		void _OnRegisterSuccess(Dictionary dict)
		{
			string playerName = dict["player_name"].AsString();
			GD.Print($"Player {playerName} successfully registered!");

			sw.Login(user, password, successListener: _OnLoginSuccess);
		}

		void _OnLoginSuccess(Dictionary dict)
		{
			GD.Print($"Player successfully logged in!");
		}
	}
}