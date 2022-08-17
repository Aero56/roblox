namespace Roblox.UI
{
	public partial class ChatEntry : Panel
	{
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		protected static int MessageLimit => 100;

		public ChatEntry()
		{
			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public override void Tick() 
		{
			base.Tick();

			if ( Chat.Canvas.ChildrenCount > MessageLimit ) 
			{ 
				Chat.Canvas.Children.First().Delete();
			}
		}
	}
}