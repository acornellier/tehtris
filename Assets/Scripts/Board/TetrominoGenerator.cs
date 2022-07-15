using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class TetrominoGenerator : MonoBehaviour
{
    public List<TetrominoData> datas;
    private Random gen;

    private void Awake()
    {
        foreach (var data in datas)
        {
            data.Initialize();
        }

        gen = new Random();
    }

    public IEnumerable<TetrominoData> Generate()
    {
        return datas.OrderBy(_ => gen.Next());
    }
}
