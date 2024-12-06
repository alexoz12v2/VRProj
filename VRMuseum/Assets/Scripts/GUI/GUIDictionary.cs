using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Tommy;

namespace vrm {
    public class GUIDictionary : MonoBehaviour
    {
        public delegate void GUIShow(); // to update
        public static event GUIShow OnGUIShow;

        private Dictionary<string, CanvasData> _dict = new();

        [SerializeField]
        private string _path = "";
        private static bool _instanced = false;

        [ContextMenu("Select File Path")]
        public void SelectFilePath()
        {
            // Use OpenFilePanel to let the user select a file (for file selection)
            _path = EditorUtility.OpenFolderPanel("Select a folder", "", "");

            // Alternatively, you can use OpenFolderPanel for selecting a folder
            // _path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
            
            // Optionally check if a valid file was selected
            if (string.IsNullOrEmpty(_path))
            {
                Debug.Log("No file was selected.");
            }
            else
            {
                Debug.Log("Selected file: " + _path);
            }
        }

        public void Awake()
        {
            if (_instanced)
            {
                throw new System.InvalidOperationException("[GUIDictionary::Awake]" +
                    " there can only be one");
            }

            if (_path == "*")
            {
                throw new System.InvalidOperationException("[GUIDictionary::Awake]" +
                    " you didn't initialize the path from which opening TOML files");
            }

            Regex tomlRegex = new Regex(@"^(.*)\.toml$");
            var fileIt = Directory.EnumerateFiles(Path.Combine(Application.dataPath, _path), "*.toml", SearchOption.AllDirectories);
            foreach (var file in fileIt)
            {
                // if (m.Groups[0].ToString().Length > 0) <- should always be true
                Match m = tomlRegex.Match(file);
                string name = GUIDictionary.RemoveAccents(m.Groups[1].ToString());
                String.Intern(name);
                using StreamReader reader = File.OpenText(file);
                var table = TOML.Parse(reader);
                _dict.Add(name, extractCanvas(table));
            }

            _instanced = true;
        }

        public void Start() 
        {
            Debug.Log($"{_dict}");
        }

        // OnGUI method for displaying the dictionary in a readable format using IMGUI
        private void OnGUI()
        {
            // Iterate through the dictionary and display each canvas and paragraph
            foreach (var canvasEntry in _dict)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Canvas Title: {canvasEntry.Value.Title}", EditorStyles.boldLabel);
                GUILayout.Label($"Max Width: {canvasEntry.Value.MaxWidth}, Max Height: {canvasEntry.Value.MaxHeight}");

                // Display paragraphs
                foreach (var paragraph in canvasEntry.Value.Paragraphs)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Paragraph:");
                    GUILayout.Label($"Corpus: {paragraph.Corpus}");

                    // Check for nullable fields and display them
                    GUILayout.Label($"Audio: {paragraph.Audio ?? "No Audio"}");
                    GUILayout.Label($"Title: {paragraph.Title ?? "No Title"}");
                }

                GUILayout.Space(10);
            }
        }

        private CanvasData extractCanvas(TomlTable table) 
        {
            table.TryGetNode(TomlKey.Header.Value, out TomlNode headerNode);

            float maxWidth = 0f;
            headerNode.AsTable.TryGetNode(TomlKey.MaxWidth.Value, out TomlNode MaxWidthNode);
            if (MaxWidthNode.IsInteger)
            {
                maxWidth = (float)MaxWidthNode.AsInteger.Value;
            }
            else 
            {
                maxWidth = (float)MaxWidthNode.AsFloat.Value;
            }

            float maxHeight = 0f;
            headerNode.AsTable.TryGetNode(TomlKey.MaxHeight.Value, out TomlNode MaxHeightNode);
            if (MaxHeightNode.IsInteger)
            {
                maxHeight = (float)MaxHeightNode.AsInteger.Value;
            }
            else 
            {
                maxHeight = (float)MaxHeightNode.AsFloat.Value;
            }

            headerNode.AsTable.TryGetNode(TomlKey.Title.Value, out TomlNode titleNode);
            string headerTitle = titleNode.AsString.ToString();

            table.TryGetNode(TomlKey.Body.Value, out TomlNode bodyNode);
            bodyNode.AsTable.TryGetNode(TomlKey.Paragraphs.Value, out TomlNode paragraphsNode);
            List<ParagraphData> paragraphs = new(paragraphsNode.AsArray.ChildrenCount);
            for (int i = 0; i != paragraphsNode.AsArray.ChildrenCount; ++i) 
            { 
                paragraphs.Add(extractParagraph(paragraphsNode.AsArray[i].AsTable));
            }

            return new CanvasData
            {
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                Title = headerTitle,
                Paragraphs = paragraphs,
            };
        }

#nullable enable
        private static ParagraphData extractParagraph(TomlTable table) 
        {
            table.TryGetNode(TomlKey.Corpus.Value, out TomlNode corpusNode);
            string corpus = corpusNode.AsString.ToString();

            table.TryGetNode(TomlKey.Title.Value, out TomlNode titleNode);
            string? title = titleNode != null && titleNode.IsString && !string.IsNullOrEmpty(titleNode.AsString.ToString()) ? titleNode.AsString.ToString() : null;

            // TODO: load audio file
            table.TryGetNode(TomlKey.Audio.Value, out TomlNode audioNode);
            string? audio = audioNode != null && audioNode.IsString && !string.IsNullOrEmpty(audioNode.AsString.ToString()) ? audioNode.AsString.ToString() : null;

            return new ParagraphData(corpus)
            {
                Audio = audio,
                Title = title,
            };
        }
#nullable disable

        private static string RemoveAccents(string input)
        {
            // the normalization to FormD splits accented letters in letters+accents
            // the rest removes those accents (and other non-spacing characters)
            // and creates a new string from the remaining chars
            return new string(input
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        private class TomlKey
        {
            public string Value { get; private set; }

            private TomlKey(string value) { Value = value; }

            public static TomlKey Header { get { return new TomlKey("header"); } }
            public static TomlKey Paragraphs { get { return new TomlKey("paragraphs"); } }
            public static TomlKey Body { get { return new TomlKey("body"); } }
            public static TomlKey MaxWidth { get { return new TomlKey("max_width"); } }
            public static TomlKey MaxHeight { get { return new TomlKey("max_height"); } }
            public static TomlKey Audio { get { return new TomlKey("audio"); } }
            public static TomlKey Corpus { get { return new TomlKey("corpus"); } }
            public static TomlKey Title { get { return new TomlKey("title"); } }
        }
    }
}
