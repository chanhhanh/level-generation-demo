using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Class",menuName ="ScriptableObjects/Class")]
public class ClassData : ScriptableObject
{
    new public string name;
    public int ID;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int constitution;
    public int perception;
    public int speed;
    public float strengthModifier;
    public float dexterityModifier;
    public float intelligenceModifier;
    public float constitutionModifier;
    public float perceptionModifier;
    public float speedModifier;
}
