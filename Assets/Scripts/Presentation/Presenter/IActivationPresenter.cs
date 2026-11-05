using System.Collections.Generic;

public interface IActivationPresenter
{
    float Present(SpecialActivatedData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline);
}
