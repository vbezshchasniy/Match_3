using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItems : MonoBehaviour {

    [HideInInspector] public int id;

    public int x
    {
        get;
        private set;
    }

    public int y
    {
        get;
        private set;
    }

    public void OnItemPositionChanged(int newX, int newY)
    {
        x = newX;
        y = newY;

        gameObject.name = string.Format("bean_[{0}][{1}]", x, y);
    }

    private void OnMouseDown()
    {
        if (OnMouseOverItemEventHendler != null)
        {
            OnMouseOverItemEventHendler(this);
        }
    }

    public delegate void OnMouseOverItem(GridItems item);
    public static event OnMouseOverItem OnMouseOverItemEventHendler;


}
