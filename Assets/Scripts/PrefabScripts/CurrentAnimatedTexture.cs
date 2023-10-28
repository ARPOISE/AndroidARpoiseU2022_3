using UnityEngine;

public class CurrentAnimatedTexture : MonoBehaviour
{
    public float SpeedX = 0.1f;
    public float SpeedY = 0.1f;

    private float curX;
    private float curY;

    public void SetParameter(bool setValue, string label, string value)
    {
        switch (label)
        {
            case nameof(SpeedX):
                SpeedX = ParameterHelper.SetParameter(setValue, value, SpeedX).Value;
                break;
            case nameof(SpeedY):
                SpeedY = ParameterHelper.SetParameter(setValue, value, SpeedY).Value;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        curX = GetComponent<Renderer>().material.mainTextureOffset.x;
        curY = GetComponent<Renderer>().material.mainTextureOffset.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curX += Time.deltaTime * SpeedX;
        curY += Time.deltaTime * SpeedY;
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(curX, curY));
    }
}
