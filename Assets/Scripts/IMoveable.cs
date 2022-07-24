using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IMoveable
{
    //position/quaternion at start of tick, used for lerping
    Vector3 TickPosition { get; set; }
    Quaternion TickRotation { get; set; }

    Vector3 LerpTargetPosition { get; set; }
    Quaternion LerpTargetRotation { get; set; }

    bool ShouldMove { get; set; }

    void CalculateTarget((int, int) newPos, Direction dir);

    void LerpMove();

}