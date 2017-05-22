using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortableBytes
{
    public uint position;
    public byte[] bytes;

    public SortableBytes()
    {
        position = 0;
        bytes = new byte[0];
    }

    public SortableBytes(uint position, byte[] bytes)
    {
        this.position = position;
        this.bytes = bytes;
    }

    public static void Sort(SortableBytes[] bytes)
    {
        Sort(bytes, 0, bytes.Length - 1);
    }

    static void Sort(SortableBytes[] bytes, int left, int right)
    {
        int mid;

        if (right > left)
        {
            mid = (right + left) / 2;

            Sort(bytes, left, mid);
            Sort(bytes, mid + 1, right);

            Merge(bytes, left, (mid + 1), right);
        }

    }

    static void Merge(SortableBytes[] bytes, int left, int mid, int right)
    {
        SortableBytes[] temp = new SortableBytes[bytes.Length];
        int i, eol, num, pos;

        eol = (mid - 1);
        pos = left;
        num = (right - left + 1);

        while ((left <= eol) && (mid <= right))
        {
            if (bytes[left].position <= bytes[mid].position)
                temp[pos++] = bytes[left++];
            else
                temp[pos++] = bytes[mid++];
        }

        while (left <= eol)
            temp[pos++] = bytes[left++];

        while (mid <= right)
            temp[pos++] = bytes[mid++];

        for (i = 0; i < num; i++)
        {
            bytes[right] = temp[right];
            right--;
        }
    }
}
