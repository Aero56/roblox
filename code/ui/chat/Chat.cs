
namespace Roblox.UI

{
	public partial class Chat : Panel
	{
		static Chat Current;
		public static Panel Canvas { get; protected set; }
		public TextEntry ChatInput { get; protected set; }
		public bool IsChatOpen { get; protected set; } = false;

		public Chat()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/Chat.scss" );

			Canvas = Add.Panel( "chat_canvas" );
			Canvas.PreferScrollToBottom = true;
			Canvas.TryScrollToBottom();

			ChatInput = Add.TextEntry( "" );
			ChatInput.AddEventListener( "onsubmit", () => Submit() );
			ChatInput.AddEventListener( "onblur", () => Close() );
			ChatInput.AcceptsFocus = true;
			ChatInput.AllowEmojiReplace = true;
			ChatInput.Placeholder = "To chat click here or press \"enter\" key";
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "draggingcamera", Input.Down( InputButton.SecondaryAttack ) );

			if ( Input.Pressed( InputButton.Chat ) )
			{
				Open();
			}
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Canvas.TryScrollToBottom();
		}

		void Open()
		{
			AddClass( "open" );
			ChatInput.Focus();
		}

		void Close()
		{
			RemoveClass( "open" );
			ChatInput.Blur();
		}


		private void Submit()
		{
			Close();

			var msg = ChatInput.Text.Trim();
			ChatInput.Text = "";

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
