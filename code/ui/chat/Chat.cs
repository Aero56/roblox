namespace Roblox.UI

{
	public partial class Chat : Panel
	{
		static Chat Current;
		public static Panel Canvas { get; protected set; }
		public TextEntry Input { get; protected set; }

		public Chat()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/Chat.scss" );

			Canvas = Add.Panel( "chat_canvas" );
			Canvas.PreferScrollToBottom = true;

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;
			Input.Placeholder = "To chat click here or press \"enter\" key";

			Sandbox.Hooks.ChatHook.OnOpenChat += Open;
		}

		void Open()
		{
			AddClass( "open" );
			Input.Focus();
		}

		void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg );
		}

		public virtual void AddEntry( string name, string message, string lobbyState = null )
		{
			var e = Canvas.AddChild<ChatEntry>();

			e.Message.Text = message;
			e.NameLabel.Text = $"[{name}]:";

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );

			if ( lobbyState == "ready" || lobbyState == "staging" )
			{
				e.SetClass( "is-lobby", true );
			}
		}


		[ConCmd.Client( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string lobbyState = null )
		{
			Current?.AddEntry( name, message, lobbyState );

			// Only log clientside if we're not the listen server host
			if ( !Global.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ConCmd.Client( "chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message )
		{
			Current?.AddEntry( null, message );
		}

		[ConCmd.Server( "say" )]
		public static void Say( string message )
		{
			Assert.NotNull( ConsoleSystem.Caller );

			// todo - reject more stuff
			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{ConsoleSystem.Caller}: {message}" );
			AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message );
		}
	}
}

namespace Sandbox.Hooks
{
	public static partial class ChatHook
	{
		public static event Action OnOpenChat;

		[ConCmd.Client( "openchat" )]
		internal static void MessageMode()
		{
			OnOpenChat?.Invoke();
		}

	}
}
