namespace SandOverTheBox.Engine {
	public interface IGameController {
		IBlockType GetSelectedBlockType();

		bool ControlsEnabled();

		void Log(string message);

        void SelectBlockType(int key);

        void SelectToolBar(int key);
	}
}