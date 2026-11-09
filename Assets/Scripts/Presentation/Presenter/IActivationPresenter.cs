using System.Collections.Generic;

public interface IActivationPresenter
{
    float Present(SpecialActivationData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline);
}
