using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public interface IAddable<T>
    {
        T Add(T other);
    }

    public interface ISubtractable<T>
    {
        T Sub(T other);
    }

    public interface IScalable<T>
    {
        T Scale(float num);
    }

    public interface ILerpable<T>
    {
        T LerpUnclamped(T other, float lerp);
        T Lerp(T other, float lerp);
        T ConstantLerp(T target, float delta);
        T SmoothLerp(T target, ref T intermediate, float delta);
    }
}
