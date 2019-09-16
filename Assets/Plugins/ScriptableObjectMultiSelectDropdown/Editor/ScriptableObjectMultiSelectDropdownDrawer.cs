// Copyright (c) ATHellboy (Alireza Tarahomi) Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ScriptableObjectMultiSelectDropdown.Extension;

namespace ScriptableObjectMultiSelectDropdown.Editor
{
    // TODO: Mixed value (-) for selecting multi objects
    // TODO: Don't clear the SCriptableObjects list when it is unselected
    // TODO: Remove null from selected ScriptableObjects list when one or more assets are removed from the project
    [CustomPropertyDrawer(typeof(ScriptableObjectReference))]
    [CustomPropertyDrawer(typeof(ScriptableObjectMultiSelectDropdownAttribute))]
    public class ScriptableObjectMultiSelectionDropdownDrawer : PropertyDrawer
    {
        private readonly GenericMenu.MenuFunction2 _onSelectedScriptableObject;
        private readonly int _controlHint = typeof(ScriptableObjectMultiSelectDropdownAttribute).GetHashCode();

        private List<ScriptableObject> _scriptableObjects = new List<ScriptableObject>();
        private List<ScriptableObject> _selectedScriptableObjects = new List<ScriptableObject>();
        private GUIContent _popupContent = new GUIContent();
        private int _selectionControlID;
        private bool _isChanged;

        public ScriptableObjectMultiSelectionDropdownDrawer()
        {
            _onSelectedScriptableObject = OnSelectedScriptableObject;

            EditorApplication.projectChanged += ClearCache;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ScriptableObjectMultiSelectDropdownAttribute castedAttribute = attribute as
                ScriptableObjectMultiSelectDropdownAttribute;

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

        /// <summary>
        /// How you can get type of field which it uses PropertyAttribute
        /// </summary>
        private Type GetPropertyType(SerializedProperty property)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = parentType.GetFieldViaPath(property.propertyPath);
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }
            return null;
        }

        private bool ValidateAttribute(ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            if (attribute.BaseType.IsInterface)
            {
                return true;
            }

            if (attribute.BaseType.IsSubclassOf(typeof(ScriptableObject)))
            {
                return true;
            }
            return false;
        }

        private bool ValidateProperty(SerializedProperty property)
        {
            Type propertyType = GetPropertyType(property);
            if (propertyType == null || propertyType != typeof(ScriptableObjectReference))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// When new ScriptableObject added to the project
        /// </summary>
        private void ClearCache()
        {
            _scriptableObjects.Clear();
        }

        /// <summary>
        /// Gets ScriptableObjects just when it is selected or new ScriptableObject added to the project
        /// </summary>
        private void GetScriptableObjects(ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            if (attribute.BaseType.IsClass)
            {
                string[] guids = AssetDatabase.FindAssets(String.Format("t:{0}", attribute.BaseType));
                for (int i = 0; i < guids.Length; i++)
                {
                    _scriptableObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), attribute.BaseType) as ScriptableObject);
                }
            }

            if (attribute.BaseType.IsInterface)
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => attribute.BaseType.IsAssignableFrom(p));

                foreach (Type type in types)
                {
                    string[] guids = AssetDatabase.FindAssets(String.Format("t:{0}", type));
                    for (int i = 0; i < guids.Length; i++)
                    {
                        _scriptableObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), attribute.BaseType) as ScriptableObject);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the ScriptableObject is selected or not by checking if the list contains it.
        /// </summary>
        private bool ResolveSelectedScriptableObject(ScriptableObject scriptableObject)
        {
            if (_selectedScriptableObjects == null)
            {
                return false;
            }
            return _selectedScriptableObjects.Contains(scriptableObject);
        }

        private void Draw(Rect position, GUIContent label,
            SerializedProperty property, ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            if (label != null && label != GUIContent.none)
                position = EditorGUI.PrefixLabel(position, label);

            if (!ValidateAttribute(attribute))
            {
                EditorGUI.LabelField(position, "PropertyAttribute baseType does not inherit ScriptableObject");
                return;
            }

            if (!ValidateProperty(property))
            {
                EditorGUI.LabelField(position, "Use it with ScriptableObjectReference");
                return;
            }

            if (_scriptableObjects.Count == 0)
            {
                EditorGUI.LabelField(position, "This type asset does not exist in the project");
                return;
            }

            UpdateScriptableObjectSelectionControl(position, label, property, attribute);
        }

        /// <summary>
        /// Iterats through the property for finding selected ScriptableObjects
        /// </summary>
        private ScriptableObject[] Read(SerializedProperty property)
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

        /// <summary>
        /// Iterats through the property for storing selected ScriptableObjects
        /// </summary>
        private void Write(SerializedProperty property, ScriptableObject[] scriptableObjects)
        {
            // Faster way
            // var w = new System.Diagnostics.Stopwatch();
            // w.Start();
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
            // w.Stop();
            // long milliseconds = w.ElapsedMilliseconds;
            // Debug.Log(w.Elapsed.TotalMilliseconds + " ms");

            // Another way
            // property.arraySize = scriptableObjects.Length;
            // for (int i = 0; i < property.arraySize; i++)
            // {
            //     property.GetArrayElementAtIndex(i).objectReferenceValue = scriptableObjects[i];
            // }
        }

        private void UpdateScriptableObjectSelectionControl(Rect position, GUIContent label,
            SerializedProperty property, ScriptableObjectMultiSelectDropdownAttribute attribute)
        {
            SerializedProperty values = property.FindPropertyRelative("values");
            ScriptableObject[] output = DrawScriptableObjectSelectionControl(position, label, Read(values), attribute);
            if (_isChanged)
            {
                _isChanged = false;
                Write(values, output);
            }
        }

        private ScriptableObject[] DrawScriptableObjectSelectionControl(Rect position, GUIContent label,
            ScriptableObject[] scriptableObjects, ScriptableObjectMultiSelectDropdownAttribute attribute)
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
                                _isChanged = true;
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
                        _popupContent.text = "Nothing";
                    }
                    else if (scriptableObjects.Length == _scriptableObjects.Count)
                    {
                        _popupContent.text = "Everything";
                    }
                    else if (scriptableObjects.Length >= 2)
                    {
                        _popupContent.text = "Mixed ...";
                    }
                    else
                    {
                        _popupContent.text = scriptableObjects[0].name;
                    }

                    EditorStyles.popup.Draw(position, _popupContent, controlID);
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

        private void DisplayDropDown(Rect position, ScriptableObject[] selectedScriptableObject, ScriptableObjectGrouping grouping)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Nothing"), selectedScriptableObject.Length == 0, _onSelectedScriptableObject, null);
            menu.AddItem(new GUIContent("Everything"),
                (_scriptableObjects.Count != 0 && selectedScriptableObject.Length == _scriptableObjects.Count),
                _onSelectedScriptableObject, _scriptableObjects.ToArray());

            for (int i = 0; i < _scriptableObjects.Count; ++i)
            {
                var scriptableObject = _scriptableObjects[i];

                string menuLabel = MakeDropDownGroup(scriptableObject, grouping);
                if (string.IsNullOrEmpty(menuLabel))
                    continue;

                var content = new GUIContent(menuLabel);
                menu.AddItem(content, ResolveSelectedScriptableObject(scriptableObject), _onSelectedScriptableObject, scriptableObject);
            }

            menu.DropDown(position);
        }

        private void OnSelectedScriptableObject(object userData)
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

        private string FindScriptableObjectFolderPath(ScriptableObject scriptableObject)
        {
            string path = AssetDatabase.GetAssetPath(scriptableObject);
            path = path.Replace("Assets/", "");
            path = path.Replace(".asset", "");

            return path;
        }

        private string MakeDropDownGroup(ScriptableObject scriptableObject, ScriptableObjectGrouping grouping)
        {
            string path = FindScriptableObjectFolderPath(scriptableObject);

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
    }
}