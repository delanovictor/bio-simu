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
    public bool collision;

    public Species targetSpecies;

    public DNA dna;

    public enum Reproduction
    {
        Fertilization,
        Polen
    }

    public Reproduction reproduction;
}

