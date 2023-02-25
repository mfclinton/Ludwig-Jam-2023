using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

[System.Serializable]
public class Topic
{
    public string name;
    public int pops;

    Normal normal_dist = new Normal(2500, 500);

    public Topic(string name, int pops)
    {
        this.name = name;
        this.pops = (int)normal_dist.Sample();
        if (this.pops < 5)
            this.pops = 5;
    }

    public Topic Clone()
    {
        return new Topic(name, pops);
    }

  
}
