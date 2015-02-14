using UnityEngine;

namespace SandOverTheBox.Engine {
	public interface IBlockType {
        GameObject GetModel();

        Sprite GetButtonImage();

        GameObject CreateNew(Vector3 position, Quaternion rotation);
	}
}