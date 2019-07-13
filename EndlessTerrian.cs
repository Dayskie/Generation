using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrian : MonoBehaviour {

    public const float maxViewDistance = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrianChunk> terrianChunkDictionary = new Dictionary<Vector2, TerrianChunk>();
    List<TerrianChunk> terrianChunksVisibleLastUpdate = new List<TerrianChunk>();

	void Start() {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);
	}

	void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
	}

	void UpdateVisibleChunks() {

        for (int i = 0; i < terrianChunksVisibleLastUpdate.Count; i++) {
            terrianChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrianChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrianChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    terrianChunkDictionary[viewedChunkCoord].UpdateTerrianChunk();
                    if (terrianChunkDictionary[viewedChunkCoord].IsVisible()) {
                        terrianChunksVisibleLastUpdate.Add(terrianChunkDictionary[viewedChunkCoord]); 
                    }
                } else {
                    terrianChunkDictionary.Add(viewedChunkCoord, new TerrianChunk(viewedChunkCoord, chunkSize, transform));  
                }
            }
        }
    }

    public class TerrianChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrianChunk(Vector2 coord, int size, Transform parent) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrianChunk() {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}
