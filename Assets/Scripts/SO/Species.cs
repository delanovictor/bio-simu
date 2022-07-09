using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Species", menuName = "bio/Species", order = 0)]
public class Species : ScriptableObject
{
    //Meta
    public string name;
    public string tag;
    public Sprite sprite;
    public Sprite childSprite;
    public bool collision;
    public Species targetSpecies;
    public DNA baseDNA;
    public DNA mutation;
    public enum Reproduction
    {
        Fertilization,
        Polen
    }
    public Reproduction reproduction;
    public float adulthoodTime;
    public float childhoodSize;
}

