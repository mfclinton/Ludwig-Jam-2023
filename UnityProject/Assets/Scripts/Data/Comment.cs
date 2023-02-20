using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comment : MonoBehaviour
{
    public string poster { get; private set; }
    public string text { get; private set; }

    public Comment(string poster, string text)
    {
        this.poster = poster;
        this.text = text;
    }
}
