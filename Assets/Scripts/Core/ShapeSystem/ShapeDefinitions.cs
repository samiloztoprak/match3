
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
            })
        {
            Produces = PieceKind.Tnt 
        });
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("L1-Shape", priority: 30, 
            matrix: new bool[,]
            {
                { true, true, true},
                { false, false, true},
                { false, false, true},
            })
        {
            Produces = PieceKind.Tnt 
        });
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("L2-Shape", priority: 30, 
            matrix: new bool[,]
            {
                { true, true, true},
                { true, false, false},
                { true, false, false},
            })
        {
            Produces = PieceKind.Tnt 
        });
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Square-Shape", priority: 15, 
            matrix: new bool[,]
            {
                { true, true},
                { true, true},
            })
        {
            Produces = PieceKind.Propeller 
        });
        
        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line3-Shape", priority: 1, 
            matrix: new bool[,]
            {
                { true, true, true }
            })
        {
            Produces = PieceKind.Normal 
        });

        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line4-Shape", priority: 5, 
            matrix: new bool[,]
            {
                { true, true, true, true }
            })
        {
            Produces = PieceKind.Rocket 
        });

        ShapeRegistry.RegisterTemplate(new ShapeTemplate("Line5-Shape", priority: 10, 
            matrix: new bool[,]
            {
                { true, true, true, true, true }
            })
        {
            Produces = PieceKind.LightBall 
        });
    }
}
