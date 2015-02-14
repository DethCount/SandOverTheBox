using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace SandOverTheBox.Engine {
    public class GameController : MonoBehaviour, IGameController {
        const int TOOLBAR_MAX_ITEMS = 10;

        public static GameController instance;
        public GameObject[] blocksPrefabs;
        public Sprite[] blocksButtonImages;
        public Sprite defaultBlockButtonImage;
        public GameObject toolBarPanel;
        public Button toolBarButtonPrefab;
        public GameObject firstPersonControllerPrefab;

        private ArrayList blockTypes;
        private ArrayList toolBars;
        private int selectedToolBarIndex;
        private GameObject firstPersonController;

        public int GetSelectedToolBarIndex()
        {
            return selectedToolBarIndex;
        }

        public ToolBar GetSelectedToolBar()
        {
            return (ToolBar) toolBars[selectedToolBarIndex];
        }

        public IBlockType GetSelectedBlockType()
        {
            return (BlockType) GetSelectedToolBar().GetSelectedBlockType();
        }

        public void SelectBlockType(int key)
        {
            GetSelectedToolBar().SelectBlockType(key);
        }

        void Start()
        {
            Screen.showCursor = false;
            if (instance == null) {
                instance = this;
            } else if(instance != this) {
                Destroy(this);
            }

            Init();

            this.Log("Game started!");
        }

        void Init()
        {

            // init block types
            InitBlockTypes();
            InitToolBars();

            GameObject loadingScreen = GameObject.FindWithTag("LoadingScreen");
            if (null != loadingScreen) {
                loadingScreen.SetActive(false);
            }

            SetSelectedToolBar(0);

            InitPlayerController();
        }

        private void InitBlockTypes()
        {
            blockTypes = new ArrayList();

            int key = 0;
            foreach (GameObject blockPrefab in blocksPrefabs) {
                Sprite image = defaultBlockButtonImage;
                if (blocksButtonImages.Length > key) {
                    image = blocksButtonImages[key];
                }

                blockTypes.Add(new BlockType(blockPrefab, image));
                key++;
            }
        }

        private void InitToolBars()
        {
            toolBars = new ArrayList();

            int key = 0;
            int lastToolBarIndex = -1;
            ToolBar toolBar = default(ToolBar);
            int toolBarIndex = 0;
            foreach (BlockType blockType in blockTypes) {
                toolBarIndex = key / TOOLBAR_MAX_ITEMS;
                if (lastToolBarIndex < toolBarIndex) {
                    toolBar = new ToolBar("Toolbar " + toolBarIndex);
                    toolBar.SetGameController(this);
                    toolBars.Add(toolBar);
                    lastToolBarIndex = toolBarIndex;
                }

                toolBar.AddBlockType(blockType);
                key++;
            }
        }

        private void InitPlayerController()
        {
            firstPersonController = (GameObject) Instantiate(
                firstPersonControllerPrefab, 
                transform.position, 
                transform.rotation
            );
        }

        public void SetSelectedToolBar(int key)
        {
            selectedToolBarIndex = key;
            GetSelectedToolBar().show(toolBarPanel, toolBarButtonPrefab);
        }

        void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Screen.showCursor = !Screen.showCursor;
            }
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }
    }
}