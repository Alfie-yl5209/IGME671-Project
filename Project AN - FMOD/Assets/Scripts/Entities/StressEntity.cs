using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressEntity : MonoBehaviour
{

    [Range(0, 10f)]
    public float distance;
    [Range(0, 10f)]
    public float intensity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //simple distance check instead of vision check
        if (Vector3.Distance(PlayerMovement.instance.transform.position, transform.position) < distance)
            if (PlayerStats.stats.Stress < 100f)
                PlayerStats.stats.Stress += intensity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
