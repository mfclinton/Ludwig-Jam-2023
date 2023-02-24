using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

[Serializable]
public class User
{
    [SerializeField] string name;
    [SerializeField] Sprite profilePic;

    public User(string name, Sprite profilePic)
    {
        this.name = name;
        this.profilePic = profilePic;
    }
}

public class Commentors : MonoBehaviour
{
    [SerializeField] TextAsset commentorsFile;
    [Tooltip("Assumes you're in `Assets/Resources` folder")]
    [SerializeField] string commentorPicsDir;
    [SerializeField] User[] users;

    void OnValidate()
    {
        if (commentorsFile == null || commentorPicsDir.Length == 0)
        {
            Debug.Log("Unable to update commentors");
            return;
        }

        string[] lines = commentorsFile.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        users = lines.Select((string line) => {
            string[] lineData = line.Split(',');

            string name = lineData[0];
            string picPath = commentorPicsDir + lineData[1];
            Sprite profilePic = Resources.Load<Sprite>(picPath);

            if(profilePic == null)
                Debug.LogWarning($"Unable to load file {picPath}");
            if (name.Length == 0)
                Debug.LogWarning($"Line has empty name {line}");

            return new User(name, profilePic);
        }).ToArray();
    }
}
