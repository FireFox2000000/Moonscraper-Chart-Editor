using UnityEngine;
using System.Collections;

static class Utility {
    public const int NOTFOUND = -1;
    public static int BinarySearchPos<T> (T searchItem, T[] sortedSearchArea)
    {   
        int lowerBound = 0;
        int upperBound = sortedSearchArea.Length;
        int index = NOTFOUND;

        int midPoint;
        
        while (lowerBound <= upperBound)
        {
            midPoint = lowerBound + (upperBound - lowerBound) / 2;
            
            if (Comparer.Default.Compare(sortedSearchArea[midPoint], searchItem) == 0)
            {
                index = midPoint;
                
                break;
            }
            else
            {
                
                if (Comparer.Default.Compare(sortedSearchArea[midPoint], searchItem) < 0)
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
        
        return index;
    }
}
