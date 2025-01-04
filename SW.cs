using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace guilleatm.SilentWolf;
public class SW
{
	const string SW_VERSION = "0.9.9";
	SWConfig m_Config;
	Node m_Node;


	readonly string GDVersion = Engine.GetVersionInfo().GetValueOrDefault("string").ToString();
	public SW(SWConfig config, Node node)
	{
		m_Config = config;
		m_Node = node;
	}


	public void Register(string playerName, string password, string? confirmPassword = null, Action<Dictionary>? successListener = null, Action<string>? errorListener = null)
	{
		const string URL = "https://api.silentwolf.com/create_new_player";
		RegisterPayload payload = new RegisterPayload(m_Config.GameID, playerName, password, confirmPassword ?? password);
		Request(URL, HttpClient.Method.Post, payload.Serialize(), successListener, errorListener);
	}


	public void Login(string playerName, string password, Action<Dictionary>? successListener = null, Action<string>? errorListener = null)
	{
		const string URL = "https://api.silentwolf.com/login_player";
		LoginPayload payload = new LoginPayload(m_Config.APIKey, m_Config.GameID, playerName, password);
		Request(URL, HttpClient.Method.Post, payload.Serialize(), _SuccessListener, errorListener);

		void _SuccessListener(Dictionary data)
		{
			m_Config.PlayerName = playerName;
			m_Config.SWToken = data["swtoken"].AsString();
			m_Config.SWIDToken = data["swidtoken"].AsString();
	
			successListener ??= DefaultSuccessListener;
			successListener.Invoke(data);
		}
	}

	public void GetPlayerData(Action<Dictionary>? successListener = null, Action<string>? errorListener = null)
	{
		GetPlayerDataPayload payload = new GetPlayerDataPayload(m_Config.GameID, m_Config.PlayerName);
		string url = $"https://api.silentwolf.com/get_player_data/{payload.GameID}/{payload.PlayerName}";
		Request(url, HttpClient.Method.Get, payload.Serialize(), successListener, errorListener);
	}

	public void SetPlayerData(Dictionary data, Action<Dictionary>? successListener = null, Action<string>? errorListener = null)
	{
		const string URL = "https://api.silentwolf.com/push_player_data";

		SetPlayerDataPayload payload = new SetPlayerDataPayload(m_Config.GameID, m_Config.PlayerName, data, true);
		Request(URL, HttpClient.Method.Post, payload.Serialize(), successListener, errorListener);
	}

	void Request(string url, HttpClient.Method method, string payload, Action<Dictionary>? successListener, Action<string>? errorListener)
	{
		HttpRequest httpRequest = new HttpRequest();
		m_Node.AddChild(httpRequest);

		successListener ??= DefaultSuccessListener;
		errorListener ??= DefaultErrorListener;

		string[] headers = new string[]
		{
			"Content-Type: application/json",
			$"x-api-key: {m_Config.APIKey}",
			$"x-sw-game-id: {m_Config.GameID}",
			$"x-sw-plugin-version: {SW_VERSION}",
			$"x-sw-godot-version: {GDVersion}",
			$"x-sw-id-token: {m_Config.SWIDToken}",
			$"x-sw-access-token: {m_Config.SWToken}"
		};

		httpRequest.RequestCompleted += _OnRequestCompleted;

		httpRequest.Request(url, headers, method, payload);

		void _OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
		{
			httpRequest.RequestCompleted -= _OnRequestCompleted;

			httpRequest.GetParent().RemoveChild(httpRequest);
			httpRequest.QueueFree();

			if (result == (int) HttpRequest.Result.Success)
			{
				string data = body.GetStringFromUtf8();
				Godot.Collections.Dictionary dict = Json.ParseString( data ).AsGodotDictionary();

				if ( dict["success"].AsBool() )
				{
					successListener!.Invoke( dict );
				}
				else
				{
					errorListener!.Invoke( dict["error"].AsString() );
				}
			}
			else
			{
				errorListener!.Invoke( ((HttpRequest.Result) result).ToString() );
			}
		}
	}


	void DefaultErrorListener(string data)
	{
		GD.PrintErr($"ERROR: {data}");
	}

	void DefaultSuccessListener(Dictionary data)
	{
		GD.Print( data.ToString() );
	}


	abstract record Payload()
	{
		static JsonSerializerOptions s_JSONSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			WriteIndented = false,
			Converters = { new VariantConverter() }
		};

		public string Serialize()
		{
			return JsonSerializer.Serialize(this, this.GetType(), s_JSONSerializerOptions);
		}


        class VariantConverter : JsonConverter<Variant>
        {
            public override Variant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string s = reader.GetBytesFromBase64().GetStringFromUtf8();
				return Json.ParseString(s);
            }

            public override void Write(Utf8JsonWriter writer, Variant value, JsonSerializerOptions options)
            {
				string s = Json.Stringify(value);
				writer.WriteStringValue(s);
            }
        }
    }

	record LoginPayload (string APIKey, string GameID, string Username, string Password, string RememberMeExpiresIn = "") : Payload;
	record RegisterPayload (string GameID, string PlayerName, string Password, string ConfirmPassword) : Payload;
	record GetPlayerDataPayload (string GameID, string PlayerName) : Payload;
	record SetPlayerDataPayload (string GameID, string PlayerName, Variant PlayerData, bool @override) : Payload;


	public record SWConfig (string APIKey, string GameID)
	{
		public string SWToken = string.Empty;
		public string SWIDToken = string.Empty;
		public string PlayerName = string.Empty;
	};

}