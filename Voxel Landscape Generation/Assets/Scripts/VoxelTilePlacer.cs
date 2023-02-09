using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class VoxelTilePlacer : MonoBehaviour
{
    public List<VoxelTile> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);
    private VoxelTile[,] spawnedTiles;
    //Start is called before the first frame update
    void Start()
    {
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        foreach(VoxelTile TilePrefab in TilePrefabs)
        {
            TilePrefab.CalculateSideColor();
        }

        int countBeforeTailRotation = TilePrefabs.Count;
        VoxelTile clone;
        for (int i = 0; i < countBeforeTailRotation; i++)
        {
            TilePrefabs[i].weight = TilePrefabs[i].weight / 4;
            if (TilePrefabs[i].weight <= 0) TilePrefabs[i].weight = 1;

            //Clone our Tile to 90
            clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.back - new Vector3(0, 0, 0.2f), Quaternion.identity);
            clone.Rotate90();
            TilePrefabs.Add(clone);

            //Clone our Tile to 180
            clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.back * 2 - new Vector3(0, 0, 0.4f), Quaternion.identity);
            clone.Rotate90();
            clone.Rotate90();
            TilePrefabs.Add(clone);

            //Clone our Tile to 270
            clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.back * 3 - new Vector3(0, 0, 0.6f), Quaternion.identity);
            clone.Rotate90();
            clone.Rotate90();
            clone.Rotate90();
            TilePrefabs.Add(clone);
        }

        StartCoroutine(Generate());
    }


    public IEnumerator Generate()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        {
            for (int y = 1; y < MapSize.y - 1; y++)
            {
                yield return new WaitForSeconds(0.100f);
                PlaceTile(x, y);
            }
        }

    }

    public void PlaceTile(int x, int y)
    {
        List<VoxelTile> avaliableTiles = new List<VoxelTile>();

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            if (CanAppendTile(spawnedTiles[x - 1, y], tilePrefab, Vector3.left) &&
                CanAppendTile(spawnedTiles[x + 1, y], tilePrefab, Vector3.right) &&
                CanAppendTile(spawnedTiles[x, y - 1], tilePrefab, Vector3.back) &&
                CanAppendTile(spawnedTiles[x, y + 1], tilePrefab, Vector3.forward)
                )
            {
                avaliableTiles.Add(tilePrefab);
            }
        }

        if (avaliableTiles.Count == 0) return;


        VoxelTile selected = GetRandomTile(avaliableTiles);
        Vector3 position = selected.VoxelSize * selected.TileSizeVoxels * new Vector3(x, 0, y);
        spawnedTiles[x, y] = Instantiate(selected, position, selected.transform.rotation);
    }

    private VoxelTile GetRandomTile(List<VoxelTile> avaliableTiles)
    {
        List<float> chances = new List<float>();

        for (int i = 0; i < avaliableTiles.Count; i++)
        {
            chances.Add(avaliableTiles[i].weight);
        }

        float value = UnityEngine.Random.Range(0, chances.Sum());
        float sum = 0;
        
        for(int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if(value <= sum)
            {
                return avaliableTiles[i];
            }
        }

        return avaliableTiles[avaliableTiles.Count - 1];
    }

    private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Vector3 direction)
    {
        if (existingTile == null)
        {
            return true;
        }


        if (direction == Vector3.right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorRight, tileToAppend.ColorLeft);
        }
        else if (direction == Vector3.left)
        {
            return Enumerable.SequenceEqual(existingTile.ColorLeft, tileToAppend.ColorRight);
        }
        else if (direction == Vector3.back)
        {
            return Enumerable.SequenceEqual(existingTile.ColorBack, tileToAppend.ColorForward);
        }
        else if (direction == Vector3.forward)
        {
            return Enumerable.SequenceEqual(existingTile.ColorForward, tileToAppend.ColorBack);
        }
        else
        {
            throw new ArgumentException("Something wrong", nameof(direction));
        }
    }
}
