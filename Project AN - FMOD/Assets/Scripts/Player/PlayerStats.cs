using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats stats;

    public int Health;
    public int Stamina;
    [Range(0, 100f)]
    public float Stress;
    [Range(0, 5f)]
    public float StressReductionSpeed;
    [Range(0, 100f)]
    public float StressResistant;

    public int Speed_walk;
    public int Speed_run;
    public int Speed_sneak;

    public float Range_interact;

    private void Awake()
    {
        stats = this;
    }

    private void Update()
    {
        if (Stress > 0)
            Stress -= StressReductionSpeed * Time.deltaTime;
        else
            Stress = 0;

        SoundManager.manager.ambience.SetParameter("Mood", Stress / 100f);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Range_interact);
    }
}
