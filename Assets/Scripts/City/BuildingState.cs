using System;

[Serializable]
public class BuildingState
{
    public BuildingType type;
    public int level;

    public BuildingState(BuildingType type, int level = 0)
    {
        this.type = type;
        this.level = level;
    }
}
