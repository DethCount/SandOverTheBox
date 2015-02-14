using UnityEngine;
using System.Collections;

namespace SandOverTheBox.Engine {
    public class PlayerController : MonoBehaviour {
        const float POINTER_MAX_DISTANCE = 5f;
        const int NO_BLOCK_SELECTED = -1;
        const int DEFAULT_SELECTED_BLOCK_INDEX = 0;
        const float BLOCK_WIDTH = 1f;

        const string TAG_BLOCK = "Block";
        const string TAG_TERRAIN = "Terrain";

        public GameObject pointer;
        public LineRenderer line;

        private bool constructingBlock;
        private float pointerMaxDistance;
        private GameObject constructedBlock;
        private IGameController controller;

        void Start () {
            controller = GameController.instance;
            if (null == controller) {
                Debug.Log("No game controller found.");
                Destroy(this);
            }

            Init ();
        }

        void Init () {
            constructingBlock = false;
            constructedBlock = null;
            line = pointer.GetComponent<LineRenderer>();
        }

        void FixedUpdate () {
            pointer.transform.localPosition.Set (0, 1.6f, 0);
            if (constructingBlock) {
                return;
            }
            StartCoroutine ("ConstructionRoutine");
        }

        IEnumerator ConstructionRoutine ()
        {
            constructingBlock = true;
            Transform origin = pointer.transform.parent;
            Ray ray = new Ray(origin.position, origin.forward);
            RaycastHit hit;

            while (Input.GetButton ("Fire1") && !Screen.showCursor) {
                if (Physics.Raycast(origin.position, origin.forward, out hit, POINTER_MAX_DISTANCE)) {
                    ShowBlockConstruct(ray, hit);
                }

                yield return null;
            }
            
            constructingBlock = false;

            if (constructedBlock != null) {
                constructedBlock.collider.enabled = true;
                Color color = constructedBlock.renderer.material.color;
                color.a = 1f;
                constructedBlock.renderer.material.SetColor("_Color", color);
                constructedBlock = null;
            }

            line.enabled = false;

            if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1)) {
                controller.SelectBlockType(0);
            }

            if (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2)) {
                controller.SelectBlockType(1);
            }

            if (Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Keypad3)) {
                controller.SelectBlockType(2);
            }

            if (Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Keypad4)) {
                controller.SelectBlockType(3);
            }

            if (Input.GetKey(KeyCode.Alpha5) || Input.GetKey(KeyCode.Keypad5)) {
                controller.SelectBlockType(4);
            }

            if (Input.GetKey(KeyCode.Alpha6) || Input.GetKey(KeyCode.Keypad6)) {
                controller.SelectBlockType(5);
            }

            if (Input.GetKey(KeyCode.Alpha7) || Input.GetKey(KeyCode.Keypad7)) {
                controller.SelectBlockType(6);
            }

            if (Input.GetKey(KeyCode.Alpha8) || Input.GetKey(KeyCode.Keypad8)) {
                controller.SelectBlockType(7);
            }

            if (Input.GetKey(KeyCode.Alpha9) || Input.GetKey(KeyCode.Keypad9)) {
                controller.SelectBlockType(8);
            }

            if (Input.GetKey(KeyCode.Alpha0) || Input.GetKey(KeyCode.Keypad9)) {
                controller.SelectBlockType(9);
            }

            if (Input.GetButtonDown ("Fire2")) {
                if (Physics.Raycast(origin.position, origin.forward, out hit, POINTER_MAX_DISTANCE)) {
                    DestroyBlock(ray, hit);
                }
            }
        }

        void ShowBlockConstruct(Ray ray, RaycastHit hit)
        {
            Vector3 blockCenter = default(Vector3);
            Quaternion blockRotation = default(Quaternion);

            controller.Log("hit got a: " + hit.collider.tag);
            switch (hit.collider.tag) {
                case TAG_BLOCK:
                    // block
                    controller.Log(Input.GetButtonDown("Fire1") ? "true" : "false");
                    if (null == constructedBlock
                        && Input.GetButtonDown("Fire1")
                        && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    ) {
                        controller.Log("selected block");
                        constructedBlock = hit.collider.gameObject;
                        blockRotation = hit.collider.transform.rotation;
                    }

                    blockCenter = new Vector3(
                        ((int) (hit.point.x / BLOCK_WIDTH)) * BLOCK_WIDTH, 
                        ((int) (hit.point.y / BLOCK_WIDTH)) * BLOCK_WIDTH + (BLOCK_WIDTH / 2),
                        ((int) (hit.point.z / BLOCK_WIDTH)) * BLOCK_WIDTH
                    );

                    blockRotation = new Quaternion(0, 0, 0, 0);
                    break;
                case TAG_TERRAIN:
                    // terrain
                    blockCenter = new Vector3(
                        ((int) (hit.point.x / BLOCK_WIDTH)) * BLOCK_WIDTH, 
                        ((int) (hit.point.y / BLOCK_WIDTH)) * BLOCK_WIDTH + (BLOCK_WIDTH / 2), 
                        ((int) (hit.point.z / BLOCK_WIDTH)) * BLOCK_WIDTH
                    );
                    blockRotation = new Quaternion(0, 0, 0, 0);
                    break;
                case null:
                    // no tag
                    break;
                
                default:
                    controller.Log ("Unhandled tag: " + hit.collider.tag);
                    break;
            }
            
            if (default(Vector3) == blockCenter 
                || default(Quaternion) == blockRotation 
            ) {
                return;
            }

            ConstructBlock(blockCenter, blockRotation);
        }
        
        void ConstructBlock(Vector3 blockCenter, Quaternion blockRotation)
        {
            if (constructedBlock == null) {
                constructedBlock = CreateNewBlock(blockCenter, blockRotation);
            } else {
                MoveBlock(constructedBlock, blockCenter, blockRotation, 5f);
            }


            if (constructedBlock == null) {
                return;
            }

            constructedBlock.collider.enabled = false;
            Color color = constructedBlock.renderer.material.color;
            color.a = 0.1f;
            constructedBlock.renderer.material.SetColor("_Color", color);
        }

        GameObject CreateNewBlock(Vector3 position, Quaternion rotation)
        {
            IBlockType selectedBlockType = controller.GetSelectedBlockType();
            if (null == selectedBlockType || CheckBlockCollision(position, BLOCK_WIDTH)) {
                return default(GameObject);
            }

            return selectedBlockType.CreateNew(position, rotation);
        }

        bool CheckBlockCollision(Vector3 position, float blockWidth)
        {
            foreach(Collider obj in Physics.OverlapSphere(position, (blockWidth/2) * 0.99f)) {
                if (obj.tag != TAG_TERRAIN) {
                    controller.Log("block construction collision with " + obj.tag);
                    return true;
                }
            }

            return false;
        }

        void MoveBlock(GameObject block, Vector3 position, Quaternion rotation, float positionSmoothSpeed = 0, float rotationSmoothSpeed = 0)
        {
            if (
                (
                    Vector3.Equals (block.transform.position, position) 
                    && Quaternion.Equals (block.transform.rotation, rotation)
                ) || CheckBlockCollision(position, BLOCK_WIDTH)
            ) {
                return;
            }

            if (positionSmoothSpeed > 0) {
                StopCoroutine ("TranslateBlockSmoothly");
                StartCoroutine (TranslateBlockSmoothly(block, position, positionSmoothSpeed));
            } else {
                block.transform.position = position;
            }

            if (rotationSmoothSpeed > 0) {
                StopCoroutine ("RotateBlockSmoothly");
                StartCoroutine (RotateBlockSmoothly(block, rotation, rotationSmoothSpeed));
            } else {
                block.transform.rotation = rotation;
            }
        }

        IEnumerator TranslateBlockSmoothly(GameObject block, Vector3 position, float positionSmoothSpeed = 0, float refreshTime = 0f)
        {
            controller.Log ("Translating...");
            float timeSpent = 0;
            controller.Log (block.transform.position.ToString ());
            controller.Log (position.ToString ());
            controller.Log ("Distance: " + Vector3.Distance (block.transform.position, position));
            float translationTotalTime = Vector3.Distance (block.transform.position, position) / positionSmoothSpeed;
            if (refreshTime == 0) {
                refreshTime = Time.deltaTime;
            }

            while(timeSpent <= translationTotalTime) {
                float startTime = Time.time;
                if (timeSpent <= translationTotalTime) {
                    block.transform.position = Vector3.Lerp(block.transform.position, position, positionSmoothSpeed * refreshTime);
                }
                timeSpent += (Time.time - startTime) + refreshTime;
                controller.Log ("Time spent: " + timeSpent + " VS Total time: " + translationTotalTime);
                yield return new WaitForSeconds(refreshTime);
            }

            foreach(Collider obj in Physics.OverlapSphere(block.transform.position, (BLOCK_WIDTH/2) * 0.99f)) {
                    controller.Log("" + obj.name);
                    controller.Log("" + block.name);
                if (obj.tag != TAG_TERRAIN && obj.name != block.name) {
                    controller.Log("block smooth move collision with " + obj.tag);
                    Destroy(block);
                    break;
                }
            }
            
            controller.Log ("translated block smoothly");
        }
        
        IEnumerator RotateBlockSmoothly(GameObject block, Quaternion rotation, float rotationSmoothSpeed = 0, float refreshTime = 0f)
        {
            float timeSpent = 0;
            float rotationTotalTime = Quaternion.Angle (block.transform.rotation, rotation) / rotationSmoothSpeed;
            if (refreshTime == 0) {
                refreshTime = Time.deltaTime;
            }
            while(timeSpent <= rotationTotalTime) {
                float startTime = Time.time;
                if (timeSpent <= rotationTotalTime) {
                    block.transform.rotation = Quaternion.Slerp(block.transform.rotation, rotation, rotationSmoothSpeed * refreshTime);
                }
                timeSpent += (Time.time - startTime) + refreshTime;
                yield return new WaitForSeconds(refreshTime);
            }
            controller.Log ("rotated block smoothly");
        }

        void DestroyBlock(Ray ray, RaycastHit hit)
        {
            if (hit.collider.tag != TAG_BLOCK) {
                return;
            }

            Destroy (hit.collider.gameObject);
        }
    }
}

