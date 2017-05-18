using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchInfo : MonoBehaviour
{

    public List<GridItems> match;
    public int matchStartingX;
    public int matchEndX;
    public int matchStartingY;
    public int matchEndY;

    public bool validMatch()
    {
        return match != null;
    }
}
