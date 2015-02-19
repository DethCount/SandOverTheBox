using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SandOverTheBox.Engine.Terrain {
    public class TerrainGenerator : UnityEngine.Object {
        const int REAL_WORLD_FACTOR = 2;

        private short[,] heightMap;
        private static Dictionary<int, Dictionary<int, int>> generatedTiles = new Dictionary<int, Dictionary<int, int>>();
        private enum TileStatus : int {
            Unknown = 1,
            Generating = 2,
            Active = 3,
            Inactive = 4
        };

        public short[,] GenerateHeightMap(
            int size,
            int maxHeight,
            float randomness
        ) {
            Random.seed = (int) System.DateTime.Now.Ticks;

            heightMap = new short[size + 1, size + 1];
            int i;
            int j;
            for (i = 0; i < size + 1; i++) {
                for (j = 0; j < size + 1; j++) {
                    heightMap[i, j] = 0;
                }
            }

            // DebugHeightMap("init");

            DoDiamonds(0, 0, size, size, maxHeight, randomness);

            return heightMap;
        }

        private void DoDiamonds(int bottomLeftX, int bottomLeftY, int topRightX, int topRightY, int maxHeight, float randomness)
        {
            /*
            DebugHeightMap("DoDiamonds before: "
                + bottomLeftX.ToString() + ", " + bottomLeftY.ToString() + ", " 
                + topRightX.ToString() + ", " + topRightY.ToString() + ", " 
                + maxHeight.ToString() + ", " + randomness.ToString()
            );
            */
            if (heightMap[bottomLeftX, bottomLeftY] == 0) {
                heightMap[bottomLeftX, bottomLeftY] = (short) GetRandomNumber(maxHeight, randomness);
                heightMap[bottomLeftX, topRightY] = (short) GetRandomNumber(maxHeight, randomness);
                heightMap[topRightX, topRightY] = (short) GetRandomNumber(maxHeight, randomness);
                heightMap[topRightX, bottomLeftY] = (short) GetRandomNumber(maxHeight, randomness);
            } else {
                maxHeight = Mathf.Max(--maxHeight, 0);
                // Debug.Log("New max height: " + maxHeight.ToString());
            }

            int midX = (bottomLeftX + topRightX) / 2;
            int midY = (bottomLeftY + topRightY) / 2;
            heightMap[midX, midY] = (short)(
                (
                    heightMap[bottomLeftX, bottomLeftY]
                    + heightMap[bottomLeftX, topRightY]
                    + heightMap[topRightX, topRightY]
                    + heightMap[topRightX, bottomLeftY]
                    + Random.Range(0f, randomness * maxHeight)
                ) / (4 + randomness)
            );
            /*
            DebugHeightMap("DoDiamonds after: "
                + bottomLeftX.ToString() + ", " + bottomLeftY.ToString() + ", " 
                + topRightX.ToString() + ", " + topRightY.ToString() + ", " 
                + maxHeight.ToString() + ", " + randomness.ToString()
            );
            */

            DoSquares(bottomLeftX, bottomLeftY, topRightX, topRightY, maxHeight, randomness);
        }

        private float GetRandomNumber(int maxHeight, float randomness)
        {
            return Random.Range(0f, randomness * maxHeight) / randomness;
        }

        private void DoSquares(int bottomLeftX, int bottomLeftY, int topRightX, int topRightY, int maxHeight, float randomness)
        {
            /*
            DebugHeightMap("DoSquares before: " 
                + bottomLeftX.ToString() + ", " + bottomLeftY.ToString() + ", " 
                + topRightX.ToString() + ", " + topRightY.ToString() + ", " 
                + maxHeight.ToString() + ", " + randomness.ToString()
            );
            */

            int midX = (bottomLeftX + topRightX) / 2;
            int midY = (bottomLeftY + topRightY) / 2;
            heightMap[bottomLeftX, midY] = (short) GetRandomNumber(maxHeight, randomness);
            float total = heightMap[bottomLeftX, (bottomLeftY + topRightY) / 2];
            heightMap[(bottomLeftX + topRightX) / 2, topRightY] = (short) GetRandomNumber(maxHeight, randomness);
            total += heightMap[(bottomLeftX + topRightX) / 2, topRightY];
            heightMap[topRightX, (bottomLeftY + topRightY) / 2] = (short) GetRandomNumber(maxHeight, randomness);
            total += heightMap[topRightX, (bottomLeftY + topRightY) / 2];
            heightMap[(bottomLeftX + topRightX) / 2, bottomLeftY] = (short) GetRandomNumber(maxHeight, randomness);
            total += heightMap[(bottomLeftX + topRightX) / 2, bottomLeftY];

            /*
            DebugHeightMap("DoSquares after: " 
                + bottomLeftX.ToString() + ", " + bottomLeftY.ToString() + ", " 
                + topRightX.ToString() + ", " + topRightY.ToString() + ", " 
                + maxHeight.ToString() + ", " + randomness.ToString()
            );
            */

            // Debug.Log("midX : " + midX.ToString());
            // Debug.Log("Modulo 4 : " + (midX % 4).ToString());
            if (midX % 2 > 0) {
                return;
            }
/*
            heightMap[midX / 2, midY / 2] = (
                total + Random.Range(0f, randomness * maxHeight)
            ) / (4 + randomness);
            heightMap[midX / 2, 3 * midY / 2] = (
                total + Random.Range(0f, randomness * maxHeight)
            ) / (4 + randomness);
            heightMap[3 * midX / 2, midY / 2] = (
                total + Random.Range(0f, randomness * maxHeight)
            ) / (4 + randomness);
            heightMap[3 * midX / 2, 3 * midY / 2] = (
                total + Random.Range(0f, randomness * maxHeight)
            ) / (4 + randomness);
*/

            DoDiamonds(bottomLeftX, bottomLeftY, midX, midY, maxHeight, randomness);
            DoDiamonds(bottomLeftX, midY, midX, topRightY, maxHeight, randomness);
            DoDiamonds(midX, bottomLeftY, topRightX, midY, maxHeight, randomness);
            DoDiamonds(midX, midY, topRightX, topRightY, maxHeight, randomness);
        }

        private void DebugHeightMap(string message)
        {
            /*
            int i;
            int j;
            string str = "";
            
            Debug.Log(message);
            Debug.Log("--- New heightmap :");
            for (i = 0; i < heightMap.GetLength(0); i++) {
                for (j = 0; j < heightMap.GetLength(1); j++) {
                    str += heightMap[i, j].ToString() + " ";
                }
                str += "\n";
            }
            Debug.Log(str);
            */
        }

        public IEnumerator UpdateTerrain(
            GameObject voxelPrefab,
            Material grass,
            Material water,
            int size,
            int maxHeight,
            float randomness,
            Transform player,
            int playerVision
        ) {
            yield return null;
            // Debug.Log("real player: " + player.position.ToString());

            Vector3 playerForwardVector = Vector3.Project(player.position, player.forward);
            Vector3 playerRightVector = Vector3.Project(player.position, player.right);

            float playerForward = playerForwardVector.x + playerForwardVector.y + playerForwardVector.z;
            float playerRight = playerRightVector.x + playerRightVector.y + playerRightVector.z;

            int playerX = Mathf.RoundToInt(playerForward / size);
            int playerY = Mathf.RoundToInt(playerRight / size);

            if (TileMustBeGenerated(playerX, playerY)) {
                GenerateTile(
                    playerX, 
                    playerY, 
                    voxelPrefab,
                    grass,
                    water,
                    size,
                    maxHeight,
                    randomness
                );
            }
            
            int visionX = Mathf.RoundToInt((playerForward + Mathf.Sign(playerForward) * playerVision) / size);
            int visionY = Mathf.RoundToInt((playerRight + Mathf.Sign(playerRight) * playerVision) / size);

            if (TileMustBeGenerated(visionX, playerY)) {
                GenerateTile(
                    visionX,
                    playerY, 
                    voxelPrefab,
                    grass,
                    water,
                    size,
                    maxHeight,
                    randomness
                );
            }

            if (TileMustBeGenerated(playerX, visionY)) {
                GenerateTile(
                    playerX,
                    visionY, 
                    voxelPrefab,
                    grass,
                    water,
                    size,
                    maxHeight,
                    randomness
                );
            }

            if (TileMustBeGenerated(playerX, -1 * visionY)) {
                GenerateTile(
                    playerX,
                    -1 * visionY, 
                    voxelPrefab,
                    grass,
                    water,
                    size,
                    maxHeight,
                    randomness
                );
            }

            yield break;
        }

        public bool TileMustBeGenerated(int tileX, int tileY)
        {
            if (!generatedTiles.ContainsKey(tileX)) {
                generatedTiles.Add(tileX, new Dictionary<int, int>());
            }

            if (!generatedTiles[tileX].ContainsKey(tileY)) {
                generatedTiles[tileX].Add(tileY, (int)TileStatus.Unknown);
            }

            return (generatedTiles[tileX][tileY] == (int)TileStatus.Unknown);
        }

        public void GenerateTile(
            int tileX, 
            int tileY,
            GameObject voxelPrefab,
            Material grass,
            Material water,
            int size,
            int maxHeight,
            float randomness
        ) {
            Debug.Log("Generating tile " + tileX + ", " + tileY);
            generatedTiles[tileX][tileY] = (int)TileStatus.Generating;

            GenerateVoxelTerrain(
                voxelPrefab,
                grass,
                water,
                size,
                maxHeight,
                randomness,
                new Vector3(tileY * size * REAL_WORLD_FACTOR, 0, tileX * size * REAL_WORLD_FACTOR)
            );

            generatedTiles[tileX][tileY] = (int)TileStatus.Active;
        }

        public void GenerateTerrain(
            GameObject blockPrefab, 
            GameObject waterPrefab,
            int size,
            int maxHeight,
            float randomness,
            Vector3 worldPosition
        ) {
            Debug.Log("Generating terrain at " + worldPosition.ToString());
            GenerateHeightMap(size, maxHeight, randomness);

            int i;
            int j;
            int k;
            float x;
            float y;
            float z;
            
            Vector3 position;
            GameObject prefab;
            for (i = 0; i < heightMap.GetLength(0); i++) {
                for (j = 0; j < heightMap.GetLength(1); j++) {
                    for (k = 0; k < 1 + heightMap[i, j]; k++) {
                        // Debug.Log("i: " + i.ToString() + ", j: " + j.ToString() + ", k: " + k.ToString());

                        x = (i - (heightMap.GetLength(0) / 2)) * REAL_WORLD_FACTOR;
                        y = k;
                        z = (j - (heightMap.GetLength(1) / 2)) * REAL_WORLD_FACTOR;

                        prefab = k == 0 ? waterPrefab : blockPrefab;
                        
                        position = new Vector3(x, y, z);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        Instantiate(prefab, worldPosition + position, Quaternion.identity);

                        position = new Vector3(x, y, z + 1);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        Instantiate(prefab, worldPosition + position, Quaternion.identity);
                        
                        position = new Vector3(x + 1, y, z);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        Instantiate(prefab, worldPosition + position, Quaternion.identity);

                        position = new Vector3(x + 1, y, z + 1);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        Instantiate(prefab, worldPosition + position, Quaternion.identity);

                        // return;
                        // mesh.RecalculateNormals();
                        // mesh.RecalculateBounds();
                        // Graphics.DrawMesh(mesh, position, Quaternion.identity, mat, 0);
                    }
                }
            }
        }

        public void GenerateVoxelTerrain(
            GameObject voxelPrefab,
            Material grass,
            Material water,
            int size,
            int maxHeight,
            float randomness,
            Vector3 worldPosition
        ) {
            GenerateHeightMap(size, maxHeight, randomness);

            /*
            MeshFilter filter = voxelPrefab.GetComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            mesh.Clear();

            float length = 1f;
            float width = 1f;
            float height = 1f;
             
            #region Vertices
            Vector3 p0 = new Vector3( -length * .5f,    -width * .5f, height * .5f );
            Vector3 p1 = new Vector3( length * .5f,     -width * .5f, height * .5f );
            Vector3 p2 = new Vector3( length * .5f,     -width * .5f, -height * .5f );
            Vector3 p3 = new Vector3( -length * .5f,    -width * .5f, -height * .5f );  
             
            Vector3 p4 = new Vector3( -length * .5f,    width * .5f,  height * .5f );
            Vector3 p5 = new Vector3( length * .5f,     width * .5f,  height * .5f );
            Vector3 p6 = new Vector3( length * .5f,     width * .5f,  -height * .5f );
            Vector3 p7 = new Vector3( -length * .5f,    width * .5f,  -height * .5f );
             
            Vector3[] vertices = new Vector3[]
            {
                // Bottom
                p0, p1, p2, p3,
             
                // Left
                p7, p4, p0, p3,
             
                // Front
                p4, p5, p1, p0,
             
                // Back
                p6, p7, p3, p2,
             
                // Right
                p5, p6, p2, p1,
             
                // Top
                p7, p6, p5, p4
            };
            #endregion

            #region Normales
            Vector3 up  = Vector3.up;
            Vector3 down    = Vector3.down;
            Vector3 front   = Vector3.forward;
            Vector3 back    = Vector3.back;
            Vector3 left    = Vector3.left;
            Vector3 right   = Vector3.right;
             
            Vector3[] normales = new Vector3[]
            {
                // Bottom
                down, down, down, down,
             
                // Left
                left, left, left, left,
             
                // Front
                front, front, front, front,
             
                // Back
                back, back, back, back,
             
                // Right
                right, right, right, right,
             
                // Top
                up, up, up, up
            };
            #endregion

            #region UVs
            Vector2 _00 = new Vector2( 0f, 0f );
            Vector2 _10 = new Vector2( 1f, 0f );
            Vector2 _01 = new Vector2( 0f, 1f );
            Vector2 _11 = new Vector2( 1f, 1f );
             
            Vector2[] uvs = new Vector2[]
            {
                // Bottom
                _11, _01, _00, _10,
             
                // Left
                _11, _01, _00, _10,
             
                // Front
                _11, _01, _00, _10,
             
                // Back
                _11, _01, _00, _10,
             
                // Right
                _11, _01, _00, _10,
             
                // Top
                _11, _01, _00, _10,
            };
            #endregion

            #region Triangles
            int[] triangles = new int[]
            {
                // Bottom
                3, 1, 0,
                3, 2, 1,            
             
                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
             
                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
             
                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
             
                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
             
                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
             
            };
            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;
             
            mesh.RecalculateBounds();
            mesh.Optimize();
            */
            int i;
            int j;
            int k;
            float x;
            float y;
            float z;
            GameObject voxel;
            Material mat;
            
            Vector3 position;
            for (i = 0; i < heightMap.GetLength(0); i++) {
                for (j = 0; j < heightMap.GetLength(1); j++) {
                    for (k = 0; k < 1 + heightMap[i, j]; k++) {
                        // Debug.Log("i: " + i.ToString() + ", j: " + j.ToString() + ", k: " + k.ToString());

                        x = (i - (heightMap.GetLength(0) / 2)) * REAL_WORLD_FACTOR;
                        y = k;
                        z = (j - (heightMap.GetLength(1) / 2)) * REAL_WORLD_FACTOR;

                        mat = (y == 0) ? water : grass;
                        
                        position = new Vector3(x, y, z);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        voxel = (GameObject) Instantiate(voxelPrefab, worldPosition + position, Quaternion.identity);
                        voxel.GetComponent<MeshRenderer>().material = mat;

                        position = new Vector3(x, y, z + 1);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        voxel = (GameObject) Instantiate(voxelPrefab, worldPosition + position, Quaternion.identity);
                        voxel.GetComponent<MeshRenderer>().material = mat;
                        
                        position = new Vector3(x + 1, y, z);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        voxel = (GameObject) Instantiate(voxelPrefab, worldPosition + position, Quaternion.identity);
                        voxel.GetComponent<MeshRenderer>().material = mat;

                        position = new Vector3(x + 1, y, z + 1);
                        // Debug.Log("x: " + position.x.ToString() + ", y: " + position.y.ToString() + ", z: " + position.z.ToString());
                        voxel = (GameObject) Instantiate(voxelPrefab, worldPosition + position, Quaternion.identity);
                        voxel.GetComponent<MeshRenderer>().material = mat;
                    }
                }
            }
        }
    }
}