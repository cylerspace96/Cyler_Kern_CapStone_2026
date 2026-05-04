using System;
using UnityEngine;

// Class for attack properties, add stuff later if needed
[Serializable]
public class AttackData
{
    public String attackName;
    public int damage;
    public int hitCount = 1;
    public float startUp = 0.1f;
    public float returnDelay = 0.3f;
    public String animationName;
    public AudioClip attackSound;
}
