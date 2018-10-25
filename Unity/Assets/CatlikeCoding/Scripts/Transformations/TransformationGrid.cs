using UnityEngine;
using System.Collections.Generic;

public class TransformationGrid : MonoBehaviour
{
    [SerializeField]
    private Transform mPrefab;

    [SerializeField]
    private int mGridResolution;

    private Transform[] mGrid;

    private List<Transformation> mTransformations;

    private Matrix4x4 mTransformation;

    // Use this for initialization
    private void Awake()
    {
        mGrid = new Transform[mGridResolution * mGridResolution * mGridResolution];

        for (int i = 0, z = 0; z < mGridResolution; z++)
            for (int y = 0; y < mGridResolution; y++)
                for (int x = 0; x < mGridResolution; x++, i++)
                    mGrid[i] = CreateGridPoint(x, y, z);

        mTransformations = new List<Transformation>();
    }

    private void Update()
    {
        UpdateTransformations();
        for (int i = 0, z = 0; z < mGridResolution; z++)
            for (int y = 0; y < mGridResolution; y++)
                for (int x = 0; x < mGridResolution; x++, i++)
                    mGrid[i].localPosition = TransformPoint(x, y, z);
    }

    private Transform CreateGridPoint(int x, int y, int z)
    {
        var point = Instantiate(mPrefab);
        point.localPosition = GetCoordinates(x, y, z);
        point.GetComponent<MeshRenderer>().material.color = new Color(
            (float)x / mGridResolution,
            (float)y / mGridResolution,
            (float)z / mGridResolution
        );
        return point;
    }

    private Vector3 GetCoordinates(int x, int y, int z)
    {
        return new Vector3(x - (mGridResolution - 1) * 0.5f,
                           y - (mGridResolution - 1) * 0.5f,
                           z - (mGridResolution - 1) * 0.5f
        );
    }

    private Vector3 TransformPoint(int x, int y, int z)
    {
        Vector3 coordinates = GetCoordinates(x, y, z);
        return mTransformation.MultiplyPoint(coordinates);
    }

    private void UpdateTransformations()
    {
        GetComponents(mTransformations);
        if (mTransformations.Count > 0)
        {
            mTransformation = mTransformations[0].Matrix;
            for (int i = 1; i < mTransformations.Count; i++)
                mTransformation = mTransformations[i].Matrix * mTransformation;
        }
    }
}
