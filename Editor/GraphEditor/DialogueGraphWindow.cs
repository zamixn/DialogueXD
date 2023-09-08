using FrameworksXD.DialogueXD.Editor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private const string WindowName = "Dialogue Graph";
        private const string DefaultFileName = "DialogueFileName";

        private Button SaveButton;
        private Button MiniMapButton;
        private static TextField FileNameTextField;

        private DialogueGraphView DialogueGraphView;

        [MenuItem("Tools/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueGraphWindow>(WindowName);
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }


        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueVariables.uss");
        }

        private void AddGraphView()
        {
            DialogueGraphView = new DialogueGraphView(this);

            DialogueGraphView.StretchToParentSize();

            rootVisualElement.Add(DialogueGraphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            Button loadButton = DialogueUIElementUtilities.CreateButton("Load", Load);

            FileNameTextField = DialogueUIElementUtilities.CreateTextField(DefaultFileName, "File Name:", callback =>
            {
                FileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            SaveButton = DialogueUIElementUtilities.CreateButton("Save", TrySave);

            Button clearButton = DialogueUIElementUtilities.CreateButton("Clear", Clear);
            Button resetButton = DialogueUIElementUtilities.CreateButton("Reset", ResetGraph);

            MiniMapButton = DialogueUIElementUtilities.CreateButton("MiniMap", ToggleMiniMap);

            toolbar.Add(FileNameTextField);
            toolbar.Add(SaveButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(loadButton);
            toolbar.Add(MiniMapButton);

            toolbar.AddStyleSheets("DialogueGraphToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        public void SetSavingEnabled(bool enabled)
        {
            SaveButton.SetEnabled(enabled);    
        }

        public static void UpdateFileName(string newFileName)
        {
            FileNameTextField.value = newFileName;
        }

        private void TrySave()
        {
            if (string.IsNullOrEmpty(FileNameTextField.value))
            {
                EditorUtility.DisplayDialog
                (
                    "Invalid file name.",
                    "Please ensure the file name you've typed in is valid.",
                    "Ok"
                );
                return;
            }
            if (DialogueGraphView.EmptyDialogueNames > 0)
            {
                EditorUtility.DisplayDialog
                (
                    "Cannot save",
                    "Some dialogue nodes have empty names. They are marked with colors",
                    "Ok"
                );
                return;
            }
            if (DialogueGraphView.RepeatedNameErrorCount > 0)
            {
                EditorUtility.DisplayDialog
                (
                    "Cannot save",
                    "Some dialogue nodes have duplicate names.\nDuplicates are not allowed and are marked with colors.",
                    "Ok"
                );
                return;
            }
            DialogueIOUtility.Initialize(DialogueGraphView, FileNameTextField.value);
            DialogueIOUtility.Save();
        }

        private void Clear()
        {
            DialogueGraphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(DefaultFileName);
        }

        private void Load()
        {
            string path = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog
                (
                    "Invalid file path.",
                    "Please ensure the file is valid",
                    "Ok"
                );
                return;
            }

            Clear();
            DialogueIOUtility.Initialize(DialogueGraphView, Path.GetFileNameWithoutExtension(path));
            DialogueIOUtility.Load();
        }

        private void ToggleMiniMap()
        {
            DialogueGraphView.ToggleMiniMap();
            MiniMapButton.ToggleInClassList(".d-toolbar__button__selected");
        }
    }
}
