using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    public float VoxelSize = 0.1f;
    public int TileSizeVoxels = 10;

    [Range(0, 100)]
    public int weight = 50; 

    [HideInInspector] public byte[] ColorRight;
    [HideInInspector] public byte[] ColorForward;
    [HideInInspector] public byte[] ColorLeft;
    [HideInInspector] public byte[] ColorBack;

    // Start is called before the first frame update
    public void CalculateSideColor()
    {
        ColorRight = new byte[TileSizeVoxels * TileSizeVoxels];
        ColorForward = new byte[TileSizeVoxels * TileSizeVoxels];
        ColorLeft = new byte[TileSizeVoxels * TileSizeVoxels];
        ColorBack = new byte[TileSizeVoxels * TileSizeVoxels];


        //цикл по слою 
        for (int y = 0; y < TileSizeVoxels; y++)
        {
            for (int i = 0; i < TileSizeVoxels; i++)
            {
                ColorForward[y * TileSizeVoxels + i] = GetVoxelColor(y, i, Vector3.forward);
                ColorBack[y * TileSizeVoxels + i] = GetVoxelColor(y, i, Vector3.back);
                ColorLeft[y * TileSizeVoxels + i] = GetVoxelColor(y, i, Vector3.left);
                ColorRight[y * TileSizeVoxels + i] = GetVoxelColor(y, i, Vector3.right);
            }
        }
    }

    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);

        byte[] NewColorRight = new byte[TileSizeVoxels * TileSizeVoxels];
        byte[] NewColorLeft = new byte[TileSizeVoxels * TileSizeVoxels];
        byte[] NewColorBack = new byte[TileSizeVoxels * TileSizeVoxels];
        byte[] NewColorForward = new byte[TileSizeVoxels * TileSizeVoxels];

        for(int layer = 0; layer < TileSizeVoxels; layer++)
        {
            for (int offset = 0; offset < TileSizeVoxels; offset++)
            {
                NewColorRight[layer * TileSizeVoxels + offset] = ColorForward[layer * TileSizeVoxels + TileSizeVoxels - offset - 1];
                NewColorForward[layer * TileSizeVoxels + offset] = ColorLeft[layer * TileSizeVoxels + offset];
                NewColorLeft[layer * TileSizeVoxels + offset] = ColorBack[layer * TileSizeVoxels + TileSizeVoxels - offset - 1];
                NewColorBack[layer * TileSizeVoxels + offset] = ColorRight[layer * TileSizeVoxels + offset];
            }
        }

        ColorRight = NewColorRight;
        ColorLeft = NewColorLeft;
        ColorForward = NewColorForward;
        ColorBack = NewColorBack;

    }

    private byte GetVoxelColor(int VerticalLayer, int HorizontalOffset, Vector3 direction)
    {
        var meshCollider = GetComponentInChildren<MeshCollider>();

        float vox = VoxelSize;
        float half = VoxelSize / 2;

        Vector3 rayStart;

        if (direction == Vector3.right)
        {

            rayStart = meshCollider.bounds.min +
                new Vector3(-half, 0, half + HorizontalOffset * vox);
        }
        else if (direction == Vector3.forward)
        {

            rayStart = meshCollider.bounds.min +
                new Vector3(half + HorizontalOffset * vox, 0, -half);
        }
        else if (direction == Vector3.left)
        {

            rayStart = meshCollider.bounds.max +
                new Vector3(half, 0, -half - (TileSizeVoxels - HorizontalOffset - 1) * vox);
        }
        else if (direction == Vector3.back)
        {

            rayStart = meshCollider.bounds.max +
                new Vector3(-half - (TileSizeVoxels - HorizontalOffset - 1) * vox, 0, half);
        }
        else
        {
            throw new ArgumentException("ERROR", nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + half + VerticalLayer * vox;

       
       Debug.DrawRay(rayStart, direction * .05f, Color.blue, 2);


        if (Physics.Raycast(new Ray(rayStart, direction), out RaycastHit hit, vox))
        {
            Mesh mesh = meshCollider.sharedMesh;

            int hitTriangleVertex = mesh.triangles[hit.triangleIndex * 3 + 0];
            byte colorIndex = (byte)(mesh.uv[hitTriangleVertex].x * 256);

            return colorIndex;
        }

        return 0;
    }

}
