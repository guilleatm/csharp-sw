# CSharp Silent Wolf

This is a simple implementation of the Silent Wolf plugin for C#.

## How to use

```c#

const string API_KEY = "YOUR_API_KEY";
const string GAME_ID = "YOUR_GAME_ID";

SW.SWConfig swConfig = new SW.SWConfig(API_KEY, GAME_ID);
SW sw = new SW(swConfig, GetTree().Root );

sw.Register("my_user", "my_password", "my_password", )


void _OnRegisterSuccess

```


## Contributing

Feel free to open issues or pull requests. Any help is welcome.
