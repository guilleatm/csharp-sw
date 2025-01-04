# CSharp Silent Wolf

This is a simple implementation of the [Silent Wolf](https://silentwolf.com/features) plugin for C#.

## Quickstart

Paste [this `SWExample` class](example/SWExample.cs) into your project. Set your `API_KEY` and your `GAME_ID` variables and don't forget to add the script to a Node.
It should register and login a player.

> Take into account that the second time you run the code, the player should already exist so registration will not complete. You will see something like: `ERROR: This player name already exists. Please select another name.`
> You can run the login instead.

## How to use

```c#

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

```


## Contributing

Feel free to open issues or pull requests. Any help is welcome.
