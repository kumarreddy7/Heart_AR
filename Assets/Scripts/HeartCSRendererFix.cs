using UnityEngine;
public class HeartCSRendererFix : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;

    void Awake()
    {
        // Store all MeshRenderers at startup
        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
    }

    public void EnableAllMeshRenderers()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
    }
}