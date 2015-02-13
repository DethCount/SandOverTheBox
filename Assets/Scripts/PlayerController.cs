using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    const float POINTER_MAX_DISTANCE = 5f;
    const int NO_BLOCK_SELECTED = -1;
    const int DEFAULT_SELECTED_BLOCK_INDEX = 0;
    const float BLOCK_WIDTH = 1f;

    const string TAG_BLOCK = "Block";
    const string TAG_TERRAIN = "Terrain";

    public GameObject pointer;
    public GameObject[] blocksPrefabs;
    public LineRenderer line;

    private bool constructingBlock;
    private ArrayList blocks;
    private GameContoller gameController;
    private float pointerMaxDistance;
    private int selectedBlockIndex;
    private GameObject constructedBlock;

    void Start () {
        Init ();
    }

    void Init () {
        constructingBlock = false;
        selectedBlockIndex = DEFAULT_SELECTED_BLOCK_INDEX;
        blocks = new ArrayList ();
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

        while (Input.GetButton ("Fire1") && selectedBlockIndex != NO_BLOCK_SELECTED) {
            if (Physics.Raycast(origin.position, origin.forward, out hit, POINTER_MAX_DISTANCE)) {
                // line.SetPosition(0, ray.origin);
                // line.SetPosition(1, hit.point);
                ShowBlockConstruct(ray, hit);
            } else {
                // line.SetPosition(0, ray.origin);
                // line.SetPosition(1, ray.GetPoint(POINTER_MAX_DISTANCE));
            }
            // line.enabled = true;

            yield return null;
        }
        
        constructingBlock = false;

        if (constructedBlock != null) {
            constructedBlock.collider.enabled = true;
            Color color = constructedBlock.renderer.material.color;
            color.a = 1f;
            constructedBlock.renderer.material.SetColor("_Color", color);
            // blocks.Add(constructedBlock);
            constructedBlock = null;
        }

        line.enabled = false;

        if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1)) {
            selectedBlockIndex = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2)) {
            selectedBlockIndex = 1;
        }

        if (Input.GetButtonDown ("Fire2")) {
            if (Physics.Raycast(origin.position, origin.forward, out hit, POINTER_MAX_DISTANCE)) {
                // line.SetPosition(0, ray.origin);
                // line.SetPosition(1, hit.point);
                DestroyBlock(ray, hit);
            }
        }
    }

    void ShowBlockConstruct(Ray ray, RaycastHit hit)
    {
        Vector3 blockCenter = default(Vector3);
        Quaternion blockRotation = default(Quaternion);

        GameContoller.instance.log("hit got a: " + hit.collider.tag);
        switch (hit.collider.tag) {
            case TAG_BLOCK:
                // block
                GameContoller.instance.log(Input.GetButtonDown("Fire1") ? "true" : "false");
                if (null == constructedBlock
                    && Input.GetButtonDown("Fire1")
                    && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                ) {
                    GameContoller.instance.log("selected block");
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
                GameContoller.instance.log ("Unhandled tag: " + hit.collider.tag);
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
        if (blocksPrefabs.Length <= selectedBlockIndex) {
            return default(GameObject);
        }

        // check for an existing block at given position
        if (CheckBlockCollision(position, BLOCK_WIDTH)) {
            return default(GameObject);
        }

        return (GameObject) Instantiate(blocksPrefabs[selectedBlockIndex], position, rotation);
    }

    bool CheckBlockCollision(Vector3 position, float blockWidth)
    {
        foreach(Collider obj in Physics.OverlapSphere(position, (blockWidth/2) * 0.99f)) {
            if (obj.tag != TAG_TERRAIN) {
                GameContoller.instance.log("block construction collision with " + obj.tag);
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
        GameContoller.instance.log ("Translating...");
        float timeSpent = 0;
        GameContoller.instance.log (block.transform.position.ToString ());
        GameContoller.instance.log (position.ToString ());
        GameContoller.instance.log ("Distance: " + Vector3.Distance (block.transform.position, position));
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
            GameContoller.instance.log ("Time spent: " + timeSpent + " VS Total time: " + translationTotalTime);
            yield return new WaitForSeconds(refreshTime);
        }

        foreach(Collider obj in Physics.OverlapSphere(block.transform.position, (BLOCK_WIDTH/2) * 0.99f)) {
                GameContoller.instance.log("" + obj.name);
                GameContoller.instance.log("" + block.name);
            if (obj.tag != TAG_TERRAIN && obj.name != block.name) {
                GameContoller.instance.log("block smooth move collision with " + obj.tag);
                Destroy(block);
                break;
            }
        }
        
        GameContoller.instance.log ("translated block smoothly");
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
        GameContoller.instance.log ("rotated block smoothly");
    }

    void DestroyBlock(Ray ray, RaycastHit hit)
    {
        if (hit.collider.tag != TAG_BLOCK) {
            return;
        }

        Destroy (hit.collider.gameObject);
    }
}
