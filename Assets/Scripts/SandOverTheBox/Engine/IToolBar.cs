namespace SandOverTheBox.Engine {
	public interface IToolBar {
		void SetGameController(IGameController gameController);
		
		void SelectBlockType(int key);
	}
}