
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
			Canvas.TryScrollToBottom();

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;
			Input.Placeholder = "To chat click here or press \"enter\" key";

			Sandbox.Hooks.Chat.OnOpenChat += Open;
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "draggingcamera", Sandbox.Input.Down( InputButton.SecondaryAttack ) );
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Canvas.TryScrollToBottom();
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


		private void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) ) return;

			SendChat( msg );
		}

		public virtual void AddEntry( string name, string message )
		{
			var e = Canvas.AddChild<ChatEntry>();

			e.Message.Text = message;
			e.NameLabel.Text = $"[{name}]:";

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
		}

		[ConCmd.Server]
		public static void SendChat( string message )
		{
			if ( !ConsoleSystem.Caller.IsValid() ) return;

			AddChat( To.Everyone, ConsoleSystem.Caller.Name, message );
		}

		[ConCmd.Client( "sbox_chat_add", CanBeCalledFromServer = true )]
		public static void AddChat( string name, string message )
		{
			Current?.AddEntry( name, message );
		}

		[ConCmd.Client( "sbox_chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message )
		{
			Current?.AddEntry( null, message );
		}
	}
}
