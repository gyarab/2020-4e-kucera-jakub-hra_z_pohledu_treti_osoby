using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubcellGenerator
{
    SubcellData GenerateSubcells(MazeSettingsSO mazeSettings, CellData cellData, Vector3 position, int additionalArraySize);
}
