using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrian : MonoBehaviour {

    const float viewerMoveThresholdForChunkUpdate = 25f;
    Vector2 viewerPostionOld;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrianChunk> terrianChunkDictionary = new Dictionary<Vector2, TerrianChunk>();
    List<TerrianChunk> terrianChunksVisibleLastUpdate = new List<TerrianChunk>();

	void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
	}

	void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPostionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            viewerPostionOld = viewerPosition;
            UpdateVisibleChunks();
        }
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
                    terrianChunkDictionary.Add(viewedChunkCoord, new TerrianChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));  
                }
            }
        }
    }

    public class TerrianChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;


        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lODMeshes;

        MapData mapData;
        bool mapDataRecieved;
        int previousLODindex = -1;

        public TerrianChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrian Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrianChunk);
            }

            mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataRecieved = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrianChunk();
        }


        public void UpdateTerrianChunk()
        {
            if (mapDataRecieved) { }
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodindex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodindex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodindex != previousLODindex)
                    {
                        LODMesh lodMesh = lODMeshes[lodindex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODindex = lodindex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);

        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }

    class LODMesh {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;
              
        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }

    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDstThreshold;
    }

}
