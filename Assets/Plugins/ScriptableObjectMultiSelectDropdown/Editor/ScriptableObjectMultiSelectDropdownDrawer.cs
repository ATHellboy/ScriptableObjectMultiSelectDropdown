﻿// Copyright (c) ATHellboy (Alireza Tarahomi) Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace ScriptableObjectMultiSelectDropdown.Editor
{
    // TODO: Mixed value (-) for selecting multi objects
    [CustomPropertyDrawer(typeof(ScriptableObjectReference))]
    [CustomPropertyDrawer(typeof(ScriptableObjectMultiSelectDropdownAttribute))]
    public class ScriptableObjectMultiSelectionDropdownDrawer : PropertyDrawer
    {
        private static ScriptableObjectMultiSelectDropdownAttribute _attribute;
        private static List<ScriptableObject> _scriptableObjects = new List<ScriptableObject>();
        private static List<ScriptableObject> _selectedScriptableObjects = new List<ScriptableObject>();
        private static readonly int _controlHint = typeof(ScriptableObjectMultiSelectDropdownAttribute).GetHashCode();
        private static GUIContent _tempContent = new GUIContent();
        private static int _selectionControlID;
        private static readonly GenericMenu.MenuFunction2 _onSelectedScriptableObject = OnSelectedScriptableObject;
        private static bool isChanged;

        static ScriptableObjectMultiSelectionDropdownDrawer()
        {
            EditorApplication.projectChanged += ClearCache;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ScriptableObjectMultiSelectDropdownAttribute castedAttribute = attribute as ScriptableObjectMultiSelectDropdownAttribute;

            if (_scriptableObjects.Count == 0)
            {
                GetScriptableObjects(castedAttribute);
            }

            Draw(position, label, property, castedAttribute);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
        }

        private static Type GetPropertyType(SerializedProperty property)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = parentType.GetField(property.propertyPath);
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }
            return null;
        }

        private static bool ValidateProperty(SerializedProperty property)
        {
            Type propertyType = GetPropertyType(property);
            if (propertyType == null)
            {
                return false;
            }
            if (propertyType != typeof(ScriptableObjectReference))
            {
                return false;
            }
            return true;
        }

        private static void ClearCache()
        {
            _scriptableObjects.Clear();
        }

        private static void GetScriptableObjects(ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            UnityEngine.Object[] loadedObject = Resources.LoadAll("", attribute.BaseType);
            for (int i = 0; i < loadedObject.Length; i++)
            {
                _scriptableObjects.Add(loadedObject[i] as ScriptableObject);
            }
        }

        private static bool ResolveSelectedScriptableObject(ScriptableObject scriptableObject)
        {
            if (_selectedScriptableObjects == null)
            {
                return false;
            }
            return _selectedScriptableObjects.Contains(scriptableObject);
        }

        private static void Draw(Rect position, GUIContent label,
            SerializedProperty property, ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            if (label != null && label != GUIContent.none)
                position = EditorGUI.PrefixLabel(position, label);

            if (ValidateProperty(property))
            {
                if (_scriptableObjects.Count != 0)
                {
                    UpdateScriptableObjectSelectionControl(position, label, property.FindPropertyRelative("values"), attribute);
                }
                else
                {
                    EditorGUI.LabelField(position, "No this type asset in Resources folder");
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Use it with ScriptableObjectReference");
            }
        }

        private static ScriptableObject[] Read(SerializedProperty property)
        {
            List<ScriptableObject> selectedScriptableObjects = new List<ScriptableObject>();
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            while (!SerializedProperty.EqualContents(iterator, end) && iterator.Next(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    selectedScriptableObjects.Add(iterator.objectReferenceValue as ScriptableObject);
                }
            }

            return selectedScriptableObjects.ToArray();
        }

        private static void Write(SerializedProperty property, ScriptableObject[] scriptableObjects)
        {
            //var w = new System.Diagnostics.Stopwatch();
            //w.Start();
            int i = 0;
            SerializedProperty iterator = property.Copy();
            iterator.arraySize = scriptableObjects.Length;
            SerializedProperty end = iterator.GetEndProperty();
            while (!SerializedProperty.EqualContents(iterator, end) && iterator.Next(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    iterator.objectReferenceValue = scriptableObjects[i];
                    i++;
                }
            }
            //w.Stop();
            //long milliseconds = w.ElapsedMilliseconds;
            //Debug.Log(w.Elapsed.TotalMilliseconds + " ms");

            // Another way
            // property.arraySize = scriptableObjects.Length;
            // for (int i = 0; i < property.arraySize; i++)
            // {
            //     property.GetArrayElementAtIndex(i).objectReferenceValue = scriptableObjects[i];
            // }
        }

        private static void UpdateScriptableObjectSelectionControl(Rect position, GUIContent label,
            SerializedProperty property, ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            ScriptableObject[] output = DrawScriptableObjectSelectionControl(position, label, Read(property), property, attribute);
            if (isChanged)
            {
                isChanged = false;
                Write(property, output);
            }
        }

        private static ScriptableObject[] DrawScriptableObjectSelectionControl(Rect position, GUIContent label,
            ScriptableObject[] scriptableObjects, SerializedProperty property, ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            bool triggerDropDown = false;
            int controlID = GUIUtility.GetControlID(_controlHint, FocusType.Keyboard, position);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == "ScriptableObjectReferenceUpdated")
                    {
                        if (_selectionControlID == controlID)
                        {
                            if (scriptableObjects != _selectedScriptableObjects.ToArray())
                            {
                                scriptableObjects = _selectedScriptableObjects.ToArray();
                                isChanged = true;
                            }

                            _selectionControlID = 0;
                            _selectedScriptableObjects = null;
                        }
                    }
                    break;

                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlID;
                        triggerDropDown = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlID)
                    {
                        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space)
                        {
                            triggerDropDown = true;
                            Event.current.Use();
                        }
                    }
                    break;

                case EventType.Repaint:
                    if (scriptableObjects.Length == 0)
                    {
                        _tempContent.text = "Nothing";
                    }
                    else if (scriptableObjects.Length == _scriptableObjects.Count)
                    {
                        _tempContent.text = "Everything";
                    }
                    else if (scriptableObjects.Length >= 2)
                    {
                        _tempContent.text = "Mixed ...";
                    }
                    else
                    {
                        _tempContent.text = scriptableObjects[0].name;
                    }

                    EditorStyles.popup.Draw(position, _tempContent, controlID);
                    break;
            }

            if (triggerDropDown)
            {
                _selectionControlID = controlID;
                _selectedScriptableObjects = scriptableObjects.ToList();

                DisplayDropDown(position, scriptableObjects, attribute.grouping);
            }

            return scriptableObjects;
        }

        private static void DisplayDropDown(Rect position, ScriptableObject[] selectedScriptableObject, ScriptableObjectGrouping grouping)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Nothing"), selectedScriptableObject.Length == 0, _onSelectedScriptableObject, null);
            menu.AddItem(new GUIContent("Everything"),
                (_scriptableObjects.Count != 0 && selectedScriptableObject.Length == _scriptableObjects.Count),
                _onSelectedScriptableObject, _scriptableObjects.ToArray());

            for (int i = 0; i < _scriptableObjects.Count; ++i)
            {
                var scriptableObject = _scriptableObjects[i];

                string menuLabel = FormatGroupedScriptableObject(scriptableObject, grouping);
                if (string.IsNullOrEmpty(menuLabel))
                    continue;

                var content = new GUIContent(menuLabel);
                menu.AddItem(content, ResolveSelectedScriptableObject(scriptableObject), _onSelectedScriptableObject, scriptableObject);
            }

            menu.DropDown(position);
        }

        private static string FindPath(ScriptableObject scriptableObject)
        {
            string path = AssetDatabase.GetAssetPath(scriptableObject);
            path = path.Substring(path.IndexOf("Resources"));
            path = path.Replace("Resources/", "");
            path = path.Replace(".asset", "");

            return path;
        }

        private static string FormatGroupedScriptableObject(ScriptableObject scriptableObject, ScriptableObjectGrouping grouping)
        {
            string path = FindPath(scriptableObject);

            switch (grouping)
            {
                default:
                case ScriptableObjectGrouping.None:
                    path = path.Replace("/", " > ");
                    return path;

                case ScriptableObjectGrouping.ByFolder:
                    return path;

                case ScriptableObjectGrouping.ByFolderFlat:
                    int last = path.LastIndexOf('/');
                    string part1 = path.Substring(0, last);
                    string part2 = path.Substring(last);
                    path = part1.Replace("/", " > ") + part2;
                    return path;
            }
        }

        private static void OnSelectedScriptableObject(object userData)
        {
            if (userData == null)
            {
                _selectedScriptableObjects.Clear();
            }
            else if (userData.GetType().IsArray)
            {
                _selectedScriptableObjects = (userData as ScriptableObject[]).ToList();
            }
            else
            {
                ScriptableObject scriptableObject = userData as ScriptableObject;
                if (!ResolveSelectedScriptableObject(scriptableObject))
                {
                    _selectedScriptableObjects.Add(scriptableObject);
                }
                else
                {
                    _selectedScriptableObjects.Remove(scriptableObject);
                }
            }

            var scriptableObjectReferenceUpdatedEvent = EditorGUIUtility.CommandEvent("ScriptableObjectReferenceUpdated");
            EditorWindow.focusedWindow.SendEvent(scriptableObjectReferenceUpdatedEvent);
        }
    }
}