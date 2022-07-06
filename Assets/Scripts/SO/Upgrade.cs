using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Upgrade", menuName = "bio/Upgrade", order = 0)]
public class Upgrade : ScriptableObject
{
    public string text;
    public List<Species> species;
    public string effect;
    public float value;
}