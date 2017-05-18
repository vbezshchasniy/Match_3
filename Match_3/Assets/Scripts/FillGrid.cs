using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillGrid : MonoBehaviour
{

    GameObject[] Candies;
    public float CandiesWidth = 0.5f;
    public float CandiesHeight = 0.5f;
    public int Xsize;
    public int Ysize;
    public float MoveDuration = 0.1f;
    public float DestroyDuration = 0.01f;
    public int minItemForMatch = 3;
    public float DelayBetweenMathes = 0.2f;

    private GridItems[,] items;
    private GridItems SelectedItem;

    void Start()
    {
        GetCandies();
        MakeGridFilled();
        GridItems.OnMouseOverItemEventHendler += OnMouseOverItem; // select Candy
    }

    void GetCandies()
    {
        Candies = Resources.LoadAll<GameObject>("Prefabs");
        for (int i = 0; i < Candies.Length; i++)
            Candies[i].GetComponent<GridItems>().id = i;
    }

    void MakeGridFilled()
    {
        items = new GridItems[Xsize, Ysize];
        for (int i = 0; i < Xsize; i++)
        {
            for (int j = 0; j < Ysize; j++)
            {
                items[i, j] = InstantiateCandy(i, j);
            }
        }

    }

    GridItems InstantiateCandy(int x, int y)
    {

        GameObject randomCandy = Candies[Random.Range(0, Candies.Length)];
        GridItems newCandy = ((GameObject)Instantiate(randomCandy, new Vector2(x * CandiesWidth, y), Quaternion.identity)).GetComponent<GridItems>();
        newCandy.OnItemPositionChanged(x, y);
        return newCandy;
    }

    void OnMouseOverItem(GridItems item)
    {
        if (SelectedItem == null)
        {
            Debug.Log("Start Point");
            SelectedItem = item;
        }
        else
        {
            Debug.Log("End Point");
            float xDiff = Mathf.Abs(item.x - SelectedItem.x);
            float yDiff = Mathf.Abs(item.y - SelectedItem.y);
            if (xDiff + yDiff == 1)
            {
                Debug.Log("Try match Function");
                StartCoroutine(TryMatch(SelectedItem, item));
            }
            else
            {
                Debug.Log("Error");
            }
            SelectedItem = null;
        }
    }

    IEnumerator TryMatch(GridItems a, GridItems b)
    {
        yield return StartCoroutine(Swap(a, b));
        Debug.Log("Swap Candies");

        MatchInfo matchA = GetMatchInfo(a);
        MatchInfo matchB = GetMatchInfo(b);

        if (!matchA.validMatch() && !matchB.validMatch())
        {
            yield return StartCoroutine(Swap(a, b));
            yield break;
        }
        if (matchA.validMatch())
        {
            Debug.Log("Matche");
            yield return StartCoroutine(MyDestroy(matchA.match));
            yield return new WaitForSeconds(DelayBetweenMathes);
            yield return StartCoroutine(UpdateGridAfterMAtch(matchA));

        }
        else if (matchB.validMatch())
        {
            Debug.Log("No Matches");
            yield return StartCoroutine(MyDestroy(matchB.match));
            yield return new WaitForSeconds(DelayBetweenMathes);
            yield return StartCoroutine(UpdateGridAfterMAtch(matchB));
        }

    }

    List<GridItems> SearchHorizontally(GridItems item)
    {
        List<GridItems> H_items = new List<GridItems> { item };
        int left = item.x - 1;
        int right = item.x + 1;

        while (left >= 0 && items[left, item.y] != null && items[left, item.y].id == item.id)
        {
            H_items.Add(items[left, item.y]);
            --left;
        }

        while (right < Xsize && items[right, item.y] != null && items[right, item.y].id == item.id)
        {
            H_items.Add(items[right, item.y]);
            ++right;
        }
        return H_items;
    }

    List<GridItems> SearchVertically(GridItems item)
    {
        List<GridItems> V_items = new List<GridItems> { item };
        int up = item.y + 1;
        int down = item.y - 1;

        while (up < Ysize && items[item.x, up] != null && items[item.x, up].id == item.id)
        {
            V_items.Add(items[item.x, up]);
            up++;
        }

        while (down >= 0 && items[item.x, down] != null && items[item.x, down].id == item.id)
        {
            V_items.Add(items[item.x, down]);
            --down;
        }
        return V_items;
    }

    int GetMinimumX(List<GridItems> items)
    {
        float[] indeces = new float[items.Count];
        for (int i = 0; i < indeces.Length; i++)
            indeces[i] = items[i].x;
        return (int)Mathf.Min(indeces);
    }

    int GetMaximumX(List<GridItems> items)
    {
        float[] indeces = new float[items.Count];
        for (int i = 0; i < indeces.Length; i++)
            indeces[i] = items[i].x;
        return (int)Mathf.Max(indeces);
    }

    int GetMinimumY(List<GridItems> items)
    {
        float[] indeces = new float[items.Count];
        for (int i = 0; i < indeces.Length; i++)
            indeces[i] = items[i].y;
        return (int)Mathf.Min(indeces);
    }

    int GetMaximumY(List<GridItems> items)
    {
        float[] indeces = new float[items.Count];
        for (int i = 0; i < indeces.Length; i++)
            indeces[i] = items[i].y;
        return (int)Mathf.Max(indeces);
    }

    MatchInfo GetMatchInfo(GridItems item)
    {
        MatchInfo h = new MatchInfo();
        h.match = null;
        List<GridItems> hmatch = SearchHorizontally(item);
        List<GridItems> vmatch = SearchVertically(item);

        if (hmatch.Count > minItemForMatch && hmatch.Count > vmatch.Count)
        {
            h.matchStartingX = GetMinimumX(hmatch);
            h.matchEndX = GetMaximumX(hmatch);
            h.matchStartingY = h.matchEndY = hmatch[0].y;
            h.match = hmatch;
        }
        else if (vmatch.Count >= minItemForMatch)
        {
            h.matchStartingY = GetMinimumY(vmatch);
            h.matchEndY = GetMaximumY(vmatch);
            h.matchStartingX = h.matchEndX = vmatch[0].x;
            h.match = vmatch;
        }

        return h;
    }

    void ChangeRigidBodyStatus(bool status)
    {
        foreach (GridItems g in items)
            if (g != null)
                g.GetComponent<Rigidbody2D>().isKinematic = !status;
    }

    void SwapIndeces(GridItems a, GridItems b)
    {
        GridItems tempA = items[a.x, a.y];
        items[a.x, a.y] = items[b.x, b.y];
        items[b.x, b.y] = tempA;
        int bOldX = b.x;
        int bOldY = b.y;

        b.OnItemPositionChanged(a.x, a.y);
        a.OnItemPositionChanged(bOldX, bOldY);
    }

    IEnumerator Swap(GridItems a, GridItems b)
    {
        ChangeRigidBodyStatus(false);
        Vector3 apos = a.transform.position;
        Vector3 bpos = b.transform.position;
        StartCoroutine(a.transform.Move(bpos, MoveDuration));
        StartCoroutine(b.transform.Move(apos, MoveDuration));
        yield return new WaitForSeconds(MoveDuration);
        SwapIndeces(a, b);
        ChangeRigidBodyStatus(true);
    }

    IEnumerator MyDestroy(List<GridItems> item)
    {
        foreach (GridItems i in item)
        {
            if (i!=null)
            {
                yield return StartCoroutine(i.transform.Scale(Vector3.zero, DestroyDuration));
                Destroy(i.gameObject);
            }
        }
    }

    //New candies after destrpy old
    IEnumerator UpdateGridAfterMAtch(MatchInfo match)
    {
        //match Horizontally 
        if (match.matchStartingY == match.matchEndX)
        {
            for (int x = match.matchStartingX; x <= match.matchEndX; x++)
            {
                for (int y = match.matchStartingY; y < Ysize-1; y++)
                {
                    GridItems upperIndex = items[x, y + 1];
                    GridItems current = items[x, y];
                    items[x, y] = upperIndex;
                    items[x, y + 1] = current;
                    items[x, y].OnItemPositionChanged(items[x, y].x, items[x, y].y-1);
                }
            }
        }
        //match Vertically
        else if (match.matchEndX == match.matchStartingX)
        {
            int matchHeigh = 1 + (match.matchEndY - match.matchStartingY);
            for (int y = match.matchStartingY + matchHeigh; y < Ysize - 1; y++)
            {
                GridItems lowerIndex = items[match.matchStartingX, y - matchHeigh];
                GridItems current = items[match.matchStartingX, y];
                items[match.matchStartingX, y - matchHeigh] = current;
                items[match.matchStartingX, y + 1] = lowerIndex;
            }
            for (int y = 0; y < Ysize-matchHeigh; y++)
                items[match.matchStartingX, y].OnItemPositionChanged(match.matchStartingX, y);
            for (int i = 0; i < match.match.Count; i++)
                items[match.matchStartingX, (Ysize - 1) - i] = InstantiateCandy(match.matchStartingX, (Ysize - 1) - i);
        }
        for (int x = 0; x < Xsize; x++)
        {
            for (int y = 0; y < Ysize; y++)
            {
                MatchInfo matchinfo = GetMatchInfo(items[x, y]);
                if (matchinfo.validMatch())
                {
                    yield return new WaitForSeconds(DelayBetweenMathes);
                    yield return StartCoroutine(MyDestroy(matchinfo.match));
                    yield return new WaitForSeconds(DelayBetweenMathes);
                    yield return StartCoroutine(UpdateGridAfterMAtch(matchinfo));
                    yield return new WaitForSeconds(DelayBetweenMathes);

                }
            }
        }
    }
}
