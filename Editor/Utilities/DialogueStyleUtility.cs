using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.Utilities
{
    public static class DialogueStyleUtility
    {

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            for (int i = 0; i < styleSheetNames.Length; i++)
            {
                string name = styleSheetNames[i];

                string path = Path.Combine("Packages\\com.frameworksxd.dialoguesxd\\StyleSheets\\Editor", name);

                StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(path);
                if (styleSheet)
                    element.styleSheets.Add(styleSheet);
                else
                    Debug.LogError($"Could not load style sheet: '{name}' at path: '{path}'");
            }
            return element;
        }


        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            for (int i = 0; i < classNames.Length; i++)
            {
                string name = classNames[i];
                element.AddToClassList(name);
            }
            return element;
        }
    }
}
