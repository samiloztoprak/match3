using System.Collections.Generic;

public static class LevelElements
{
    private static readonly List<ILevelElement> _all = new List<ILevelElement>
    {
        new HoleElement(),
        new BoxElement(),
        new ToyElement(),
        new ColorElement(PieceColor.Red, 'r'),
        new ColorElement(PieceColor.Yellow, 'y'),
        new ColorElement(PieceColor.Blue, 'b'),
        new ColorElement(PieceColor.Green, 'g'),
        new SpecialElement('R', "Rocket (H)", id => new RocketPiece(id, RocketDirection.Horizontal)),
        new SpecialElement('V', "Rocket (V)", id => new RocketPiece(id, RocketDirection.Vertical)),
        new SpecialElement('T', "TNT", id => new SpecialPiece(id, PieceKind.Tnt)),
        new SpecialElement('P', "Propeller", id => new SpecialPiece(id, PieceKind.Propeller)),
        new SpecialElement('X', "Light Ball", id => new SpecialPiece(id, PieceKind.LightBall)),

        new LockElement(),
        new CageElement(),

        new GrassElement(),
    };

    private static readonly Dictionary<(LevelLayer, char), ILevelElement> _byCode = BuildIndex();

    public static IReadOnlyList<ILevelElement> All => _all;

    public static ILevelElement Get(LevelLayer layer, char code) =>
        _byCode.TryGetValue((layer, code), out ILevelElement element) ? element : null;

    public static IEnumerable<ILevelElement> ByLayer(LevelLayer layer)
    {
        foreach (ILevelElement element in _all)
        {
            if (element.Layer == layer) yield return element;
        }
    }

    private static Dictionary<(LevelLayer, char), ILevelElement> BuildIndex()
    {
        var index = new Dictionary<(LevelLayer, char), ILevelElement>();
        foreach (ILevelElement element in _all)
        {
            index.Add((element.Layer, element.Code), element);
        }
        return index;
    }
}