using UnityEngine;
using System.Collections.Generic;
using System;

namespace vrm
{
#nullable enable
    [System.Serializable]
    public class ParagraphData
    {
        public string? Audio;
        public string? Title;
        public string  Corpus;

        public ParagraphData(string corpus)
        {
            Corpus = corpus ?? throw new ArgumentNullException(nameof(corpus)); // Ensure non-null Corpus
        }
    }
#nullable disable 

    [System.Serializable]
    public class CanvasData
    {
        public float MaxWidth;
        public float MaxHeight;
        public string Title;
        public List<ParagraphData> Paragraphs;
    }
}
