namespace SandOverTheBox.Engine {
	public interface IGameController {
		IBlockType GetSelectedBlockType();

		void Log(string message);

        void SelectBlockType(int key);
	}
}