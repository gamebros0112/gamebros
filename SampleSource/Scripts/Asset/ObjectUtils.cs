using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class ObjectUtils
{
    /// <summary>
    /// 센터 포지션 구하기 
    /// </summary>
    public static Vector3 GetChildrenAverageWorldPosition(MeshFilter[] allChildren)
    {
        var avgPos = Vector3.zero;
       // int polygonCnt = 0;

        foreach (MeshFilter child in allChildren)
        {
            avgPos += child.transform.position;
            child.gameObject.AddComponent<MeshCollider>();
           // polygonCnt += child.mesh.triangles.Length / 3;
        }
       // Debug.Log("polygonCnt : " + polygonCnt);
        return avgPos / allChildren.Length;
    }
    /// <summary>
    /// 렌더러로 Bound 영역 정보 가져오기 
    /// </summary>
    public static Bounds GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }
    /// <summary>
    /// 콜라이더로 Bound 영역 정보 가져오기 
    /// </summary>
    public static Bounds GetChildColliderBounds(GameObject go)
    {
        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            for (int i = 1, ni = colliders.Length; i < ni; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }
    /// <summary>
    /// 레이어 이름으로 변경 
    /// </summary>
    /// <param name="trans">타겟</param>
    /// <param name="layerName">레이어 이름</param>
    public static void ChangeLayersRecursively(Transform trans, string layerName)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layerName);
        }
    }
    /// <summary>
    /// 레이어 레벨로 변경 
    /// </summary>
    /// <param name="trans">타겟</param>
    /// <param name="layerLevel">레이어 레벨</param>
    public static void ChangeLayersRecursively(Transform trans, int layerLevel)
    {
        trans.gameObject.layer = layerLevel;
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layerLevel);
        }
    }
    /// <summary>
    /// 레이어 마스크 사용해서 레이어 변경 
    /// </summary>
    public static int layermask_to_layer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }
    /// <summary>
    /// 버텍스 컬러 쉐이더 적용
    /// 마테리얼 노말맵, 오클루전맵 제거 
    /// </summary>
    /// <param name="go">타겟 오브젝트</param>
    /// <param name="_shader">버텍스컬러 쉐이더</param>
    public static void CustomMaterialWithMeshCollider(GameObject go, Shader _shader)
    {
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        Material[] mat;
        foreach (MeshFilter mf in meshFilters)
        {
            if (!mf.TryGetComponent(out Collider col))
            {
                mf.gameObject.AddComponent<MeshCollider>();
            }

            var originalMesh = mf.sharedMesh;
            mat = mf.GetComponent<Renderer>().sharedMaterials;
            int len = mat.Length;

            // 에셋 전체 마테리얼 정보에서 _BumpMap,_OcclusionMap 값 제거
거            for (var i = 0; i < len; i++)
            {
                mat[i].SetTexture("_BumpMap", null);
                mat[i].SetTexture("_OcclusionMap", null);

                // 버텍스 컬러 정보가 있으면 버텍스컬러 쉐이더로 교체 
                if (originalMesh.colors.Length > 0)
                    mat[i].shader = _shader;
            }
        }
    }
    /// <summary>
    /// 버텍스 컬러 쉐이더 적용
    /// 마테리얼 노말맵, 오클루전맵 제거 
    /// </summary>
    /// <param name="go">타겟 오브젝트</param>
    /// <param name="_shader">버텍스컬러 쉐이더</param>
    public static void CustomMaterial(GameObject go, Shader _shader)
    {
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        Material[] mat;
        foreach (MeshFilter mf in meshFilters)
        {
            var originalMesh = mf.sharedMesh;
            mat = mf.GetComponent<Renderer>().sharedMaterials;
            int len = mat.Length;

            for (var i = 0; i < len; i++)
            {
                mat[i].SetTexture("_BumpMap", null);
                mat[i].SetTexture("_OcclusionMap", null);

                if (originalMesh.colors.Length > 0)
                    mat[i].shader = _shader;
            }
        }
    }
}
