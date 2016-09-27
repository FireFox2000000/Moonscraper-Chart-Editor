using UnityEngine;
using System.Collections;

static class Utility {
    public const int NOTFOUND = -1;

    public static int BinarySearchChartClosestNote (Note searchItem, Note[] sortedSearchArea)
    {
        int lowerBound = 0;
        int upperBound = sortedSearchArea.Length;
        int index = NOTFOUND;

        int midPoint = NOTFOUND;

        while (lowerBound <= upperBound)
        {
            midPoint = lowerBound + (upperBound - lowerBound) / 2;

            if (sortedSearchArea[midPoint] == searchItem)
            {
                index = midPoint;

                break;
            }
            else
            {
                if (sortedSearchArea[midPoint] < searchItem)
                {
                    // data is in upper half
                    lowerBound = midPoint + 1;
                }
                else
                {
                    // data is in lower half 
                    upperBound = midPoint - 1;
                }
            }
        }

        index = midPoint;

        return index;
    }

    public static int BinarySearchChartExactNote (Note searchItem, Note[] sortedSearchArea) 
    {
        int pos = BinarySearchChartClosestNote(searchItem, sortedSearchArea);

        if (pos != NOTFOUND)
        {
            if (sortedSearchArea[pos] != searchItem)
                pos = NOTFOUND;
        }

        return pos;
    }
}
