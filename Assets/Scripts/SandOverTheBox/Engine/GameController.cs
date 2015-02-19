using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using SandOverTheBox.Engine.Terrain;

namespace SandOverTheBox.Engine {
    public class GameController : MonoBehaviour, IGameController {
        const int TOOLBAR_MAX_ITEMS = 10;
        const int TILE_WIDTH = 32;
        const int TILE_HEIGHT = 4;
        const int TILE_RANDOMNESS = 4;
        const int PLAYER_VISION = 20;

        public static GameController instance;
        public GameObject[] blocksPrefabs;
        public GameObject voxelPrefab;
        public Material grass;
        public Material water;
        public Sprite[] blocksButtonImages;
        public Sprite defaultBlockButtonImage;
        public GameObject toolBarPanel;
        public Button toolBarButtonPrefab;
        public GameObject firstPersonControllerPrefab;

        private ArrayList blockTypes;
        private ArrayList toolBars;
        private int selectedToolBarIndex;
        private bool controlsEnabled;
        private TerrainGenerator terrainGenerator;
        public GameObject playerController;

        public bool ControlsEnabled()
        {
            return controlsEnabled;
        }

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
            terrainGenerator = new TerrainGenerator();

            Screen.showCursor = false;
            controlsEnabled = true;

            if (instance == null) {
                instance = this;
            } else if(instance != this) {
                Destroy(this);
            }

            Init();

            this.Log("Game started!");
        }

        private void UpdateTerrain()
        {
            StartCoroutine(
                terrainGenerator.UpdateTerrain(
                    voxelPrefab, 
                    grass,
                    water,
                    TILE_WIDTH, 
                    TILE_HEIGHT, 
                    TILE_RANDOMNESS,
                    playerController.transform,
                    PLAYER_VISION
                )
            );
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

            SelectToolBar(0);

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

            while (toolBar.Count() < TOOLBAR_MAX_ITEMS) {
                key = toolBar.Count();
                toolBar.AddEmptyItem(defaultBlockButtonImage);
            }
        }

        private void InitPlayerController()
        {
            playerController = (GameObject) Instantiate(
                firstPersonControllerPrefab, 
                new Vector3(transform.position.x, (TILE_HEIGHT * 2) + 1, transform.position.z), 
                transform.rotation
            );
        }

        public void SelectToolBar(int key)
        {
            selectedToolBarIndex = key;
            GetSelectedToolBar().show(toolBarPanel, toolBarButtonPrefab);
        }

        void FixedUpdate()
        {
            UpdateTerrain();

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Screen.showCursor = !Screen.showCursor;
                controlsEnabled = !controlsEnabled;
            }
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }
    }
}