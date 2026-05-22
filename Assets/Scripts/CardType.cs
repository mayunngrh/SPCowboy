public enum CardType
{
    Basic,      // loads a bullet, adds a basic hit zone
    Elemental,  // adds elemental hit zone with bonus damage
    Buff,       // positive effect, no tradeoff
    Utility,    // special effect like extra energy/bullet slots
    Tradeoff,   // powerful but negative side effect
    Support     // healing, once per combat effects
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Wood,
    Metal,
    Earth
}