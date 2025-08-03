using UnityEngine;

public interface IShearable
{
  bool IsSheared { get; }

  void Shear(bool doubleShear = false);

  bool CanBeSheared();
}