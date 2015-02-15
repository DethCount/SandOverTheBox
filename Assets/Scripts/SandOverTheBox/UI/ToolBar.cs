using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SandOverTheBox.Engine {
    public class ToolBar : Object, IToolBar {
        private ArrayList blockTypes;
        private ArrayList emptyItems;
        private string localName;
        private int selectedBlockTypeIndex;
        private IGameController controller;

        public void SetGameController(IGameController controller)
        {
            this.controller = controller;
        }

        public string GetName()
        {
            return localName;
        }

        public void AddBlockType(BlockType blockType)
        {
            blockTypes.Add(blockType);
        }

        public void AddEmptyItem(Sprite buttonImage)
        {
            emptyItems.Add(buttonImage);
        }

        public BlockType GetBlockType(int key)
        {
            if (blockTypes.Count <= key) {
                return default(BlockType);
            }

            return (BlockType) blockTypes[key];
        }

        public int Count()
        {
            return blockTypes.Count + emptyItems.Count;
        }

        public BlockType GetSelectedBlockType()
        {
            controller.Log("somebody wanted selected block type, gave: " + selectedBlockTypeIndex);
            return GetBlockType(selectedBlockTypeIndex);
        }

        public void SelectBlockType(int key)
        {
            controller.Log("block type selected: " + key);
            selectedBlockTypeIndex = key;
        }

        public ToolBar (string name)
        {
            this.localName = name;
            this.blockTypes = new ArrayList();
            this.emptyItems = new ArrayList();
            this.selectedBlockTypeIndex = 0;
        }

        public void show(GameObject panel, Button buttonPrefab)
        {
            Button button;
            int key = 0;
            foreach (BlockType blockType in blockTypes) {
                int localKey = key;
                button = (Button) Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                button.GetComponent<Image>().sprite = blockType.GetButtonImage();
                button.GetComponentInChildren<Text>().text = "" + ((localKey + 1) % 10);
                button.transform.SetParent(panel.transform, false);
                LayoutRebuilder.MarkLayoutForRebuild (button.transform as RectTransform);
                button.onClick.AddListener(() => { SelectBlockType(localKey); });
                key++;
            }

            foreach (Sprite buttonImage in emptyItems) {
                int localKey = key;
                button = (Button) Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                button.GetComponent<Image>().sprite = buttonImage;
                button.GetComponentInChildren<Text>().text = "" + ((localKey + 1) % 10);
                button.interactable = false;
                button.transform.SetParent(panel.transform, false);
                LayoutRebuilder.MarkLayoutForRebuild (button.transform as RectTransform);
                key++;
            }

            LayoutRebuilder.MarkLayoutForRebuild (panel.transform as RectTransform);
        }
    }
}

