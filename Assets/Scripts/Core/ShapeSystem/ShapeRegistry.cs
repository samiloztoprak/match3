
using System.Collections.Generic;

public static class ShapeRegistry
{
    private static readonly List<ShapeTemplate> _templates = new List<ShapeTemplate>();

    public static IReadOnlyList<ShapeTemplate> Templates => _templates;
    
    static ShapeRegistry()
    {
        ShapeDefinitions.RegisterAll();
    }

    public static void RegisterTemplate(ShapeTemplate template)
    {
        _templates.Add(template);
    }
}
