using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileGenerator
{
    void GenerateTiles(SubcellData subcellData, int count);
}
