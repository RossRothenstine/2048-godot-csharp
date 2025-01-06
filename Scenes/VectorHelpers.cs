using Godot;

public static class VectorHelpers
{
    public static Vector2I Randf(int MaxX, int MaxY) 
    {
        Vector2I Out = new()
        { 
            X = Mathf.FloorToInt(GD.Randf() * MaxX), 
            Y = Mathf.FloorToInt(GD.Randf() * MaxY) 
        };
        return Out;
    }
}