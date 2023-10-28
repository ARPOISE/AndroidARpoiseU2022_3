using UnityEngine;

public class CurrentSwarm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for( int i = 0; i < transform.childCount; i++)
        {
            var fish = transform.GetChild(i);
            var animator = fish.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("ArmatureAction", 0, Random.Range(0f, 1f));
            }
        }
    }
}
