using System;
using UnityEngine;

public class CurrentBlendShapeLoop : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedMeshRenderer = null;
    public Mesh SkinnedMesh = null;
    public int FramesPerSecond = 10;
    private int _blendShapeCount;
    private int _playIndex = 0;
    private DateTime _dateAtStart;

    public void SetParameter(bool setValue, string label, string value)
    {
        switch (label)
        {
            case nameof(FramesPerSecond):
                FramesPerSecond = ParameterHelper.SetParameter(setValue, value, FramesPerSecond).Value;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _dateAtStart = DateTime.Now;

        if (SkinnedMeshRenderer == null)
        {
            SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }
        if (SkinnedMesh == null)
        {
            SkinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        var count = SkinnedMesh?.blendShapeCount;
        if (count.HasValue)
        {
            _blendShapeCount = count.Value;
        }
    }

    private DateTime _lastUpdateDate = DateTime.MinValue;
    // Update is called once per frame
    void Update()
    {
        if (FramesPerSecond > 0)
        {
            if ((DateTime.Now - _lastUpdateDate).TotalMilliseconds < 1000f / FramesPerSecond)
            {
                return;
            }
            _lastUpdateDate = DateTime.Now;
        }

        if (_playIndex > 0)
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(_playIndex - 1, 0f);
        }
        if (_playIndex == 0)
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(_blendShapeCount - 1, 0f);
        }
        SkinnedMeshRenderer.SetBlendShapeWeight(_playIndex, 100f);
        _playIndex++;
        if (_playIndex > _blendShapeCount - 1)
        {
            _playIndex = 0;
        }
    }
}
