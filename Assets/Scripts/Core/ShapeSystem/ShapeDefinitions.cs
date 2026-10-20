
public static class ShapeDefinitions
{
    public static void RegisterAll()
    {
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("T-Shape", priority: 10, 
            matrix: new bool[,]
            {
                { true, true, true},
                { false, true, false},
                { false, true, false},
            }));
    }
}
