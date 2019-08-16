//http://answers.unity3d.com/questions/836642/new-ui-46-how-to-show-ellipsis-for-text-overflow.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTruncExt : Text
{
    string updatedText = string.Empty;
    
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(base.text, settings);

        float scale = extents.x / preferredWidth;
        //text is going to be truncated, 
        //cant update the text directly as we are in the graphics update loop
        if (scale < 1)
        {
            updatedText = base.text.Substring(0, cachedTextGenerator.characterCount - 4);
            updatedText += "...";
        }
        base.OnPopulateMesh(toFill);
    }

    void Update()
    {
        if (updatedText != string.Empty && updatedText != base.text)
        {
            base.text = updatedText;
        }
    }
}
