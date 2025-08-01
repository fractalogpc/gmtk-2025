using UnityEngine;

public interface IShearable
{
  bool IsSheared { get; }

  void Shear();
}