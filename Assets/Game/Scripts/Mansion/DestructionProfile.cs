using UnityEngine;

public enum DestructionSubstance { Wood, Glass, Ceramic, Metal, Plastic, Fabric }
public enum DestructionFailure { Fracture, DentAndDetach, TearAndSag }

[CreateAssetMenu(menuName = "Major Damage/Destruction profile")]
public sealed class DestructionProfile : ScriptableObject
{
    public DestructionSubstance substance;
    public DestructionFailure failure;
    [Min(.01f)] public float minimumEnergy = 2, fractureEnergy = 100;
    [Min(0)] public float minimumImpactSpeed = 1.2f;
    [Min(.01f)] public float deformationRadius = .3f, maximumDent = .07f;
    public float maximumFragmentSize = .55f;
    public int targetFragments = 14;
    [Min(0)] public float wallImpactMultiplier = 1;
    public AudioClip impactSound, breakSound;
    public float ImpactDamage(float energy, float hp) => Mathf.Max(0, energy - minimumEnergy) * hp / Mathf.Max(.01f, fractureEnergy);
}
