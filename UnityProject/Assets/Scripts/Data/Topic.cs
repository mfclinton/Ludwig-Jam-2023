using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Topic
{
    public string name;
    public int[] pops;

    public Topic(string name, int[] pops)
    {
        this.name = name;
        this.pops = pops;
    }

    public Topic Clone()
    {
        return new Topic(name, pops);
    }

    // TODO: put a call here to update topics when this is modified
    public int ModifyPopulation(int index, int num)
    {
        int absPopChange = Mathf.Min(Mathf.Abs(num), Mathf.Abs(pops[index]));

        if (pops[index] < 0)
            pops[index] += absPopChange;
        else if (0 < pops[index])
            pops[index] -= absPopChange;

        return absPopChange;
    }

    public int TotalFollowers()
    {
        return pops.Sum(x => Mathf.Abs(x));
    }

    public bool IsExhausted()
    {
        return pops.Any(x => x != 0);
    }
}
