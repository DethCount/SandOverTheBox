using UnityEngine;

namespace SandOverTheBox.Engine {
    public class BlockType : Object, IBlockType {
        private GameObject model;
        private Sprite buttonImage;

        public GameObject GetModel()
        {
            return model;
        }

        public Sprite GetButtonImage()
        {
            return buttonImage;
        }

        public BlockType (GameObject model, Sprite buttonImage)
        {
            this.model = model;
            this.buttonImage = buttonImage;
        }

        public GameObject CreateNew(Vector3 position, Quaternion rotation)
        {
            return (GameObject) Instantiate(GetModel(), position, rotation);
        }
    }
}

