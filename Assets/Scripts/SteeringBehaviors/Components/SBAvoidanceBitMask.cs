namespace SteeringBehaviors
{
    [System.FlagsAttribute]
    public enum SBAvoidanceBitMask : int
    {
        None = 0,
        EnemyShip = 1,
        PlayerShip = 2,
        RepairShip = 4,
        Asteroid = 8,
        MainStation = 16,
        TransportShip = 32,
        Building = 64,
    }
}
