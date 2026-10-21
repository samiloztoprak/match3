
public static class ShapeDefinitions
{
    public static void RegisterAll()
    {
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("T-Shape", priority: 20, 
            matrix: new bool[,]
            {
                { true, true, true},
                { false, true, false},
                { false, true, false},
            }));
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("L1-Shape", priority: 30, 
            matrix: new bool[,]
            {
                { true, true, true},
                { false, false, true},
                { false, false, true},
            }));
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("L2-Shape", priority: 30, 
            matrix: new bool[,]
            {
                { true, true, true},
                { true, false, false},
                { true, false, false},
            }));
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Square-Shape", priority: 15, 
            matrix: new bool[,]
            {
                { true, true},
                { true, true},
            }));
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line3-Shape", priority: 1, 
            matrix: new bool[,]
            {
                { true, true, true }
            }));

        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line4-Shape", priority: 5, 
            matrix: new bool[,]
            {
                { true, true, true, true }
            }));

        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line5-Shape", priority: 10, 
            matrix: new bool[,]
            {
                { true, true, true, true, true }
            }));
    }
}
