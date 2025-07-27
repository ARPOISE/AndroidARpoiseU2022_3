using UnityEngine;

public class CameraMirror : MonoBehaviour
{
    [SerializeField]
    private Camera cameraToMirror;

    void OnPreCull()
    {
        if (cameraToMirror != null)
        {
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(-1, 1, 1));
            cameraToMirror.ResetWorldToCameraMatrix();
            cameraToMirror.ResetProjectionMatrix();
            cameraToMirror.projectionMatrix = cameraToMirror.projectionMatrix * scale;
        }
    }
    void OnPreRender()
    {
        if (cameraToMirror != null)
        {
            GL.invertCulling = true;
        }
    }
    void OnPostRender()
    {
        if (cameraToMirror != null)
        {
            GL.invertCulling = false;
        }
    }
}
