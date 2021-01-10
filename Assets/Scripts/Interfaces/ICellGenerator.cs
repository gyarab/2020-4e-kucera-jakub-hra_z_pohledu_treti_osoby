using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICellGenerator
{
    CellData GenerateCells(MazeSettingsSO mazeSettings, Vector3 position);
}
