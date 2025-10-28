using UnityEngine;

// PG: 251027, This mirros the game play, but not the camera background.
// Attach this script to the Main Camera, and assign the AR Camera to the "cameraToMirror"
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
