﻿using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class ArrayTool : EditorWindow
    {
        private ArrayCreator _creator = null;
        private ArrayType _arrayType = ArrayType.Line;

        private const float MaxWidth = 350f;
        private const float MaxHeght = 250;

        private static ArrayTool _window = null;

        private GameObject _selectedPrefab = null;
        private ArrayContainer _loadedContainer = null;
        private bool IsInEditMode => _loadedContainer != null;

        private Vector2 _scrollPosition = new Vector2();

        private bool _isSaving = false;

        [MenuItem("Tools/Array Tool &a")]
        static void ArrayToolWindow()
        {
            Open();
        }

        public static void Open(ArrayContainer container = null)
        {
            _window = ScriptableObject.CreateInstance<ArrayTool>();
            _window.maxSize = new Vector2(MaxWidth, MaxHeght);
            _window.minSize = _window.maxSize;

            if (Selection.activeObject is GameObject targetObj)
            {
                _window._selectedPrefab = targetObj;
                _window._creator = _window.GetCreator(_window._arrayType, targetObj);
            }

            _window.ShowUtility();

            if (container != null)
            {
                _window.PopulateFromExistingData(container);
            }

            _window._loadedContainer = container;
        }

        private void SaveAndClose()
        {
            _isSaving = true;
            this.Close();
        }

        private void SaveAndContinue()
        {
            _creator.SaveAndContinue();
        }

        private void Cancel()
        {
            if (_creator != null)
            {
                if (IsInEditMode)
                {
                    // #DG: figure out how to save and close
                    _isSaving = true;
                    _creator.CancelPendingEdits();
                    _creator.OnCloseWindow(true);
                }
                else
                {
                    _creator.OnCloseWindow();
                }
            }

            this.Close();
        }

        private void OnDestroy()
        {
            if (_creator != null)
            {
                _creator.OnCloseWindow(_isSaving);
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                if (!IsInEditMode)
                {
                    EditorGUILayout.BeginHorizontal(ArrayToolExtensions.BoxedHeaderStyle);
                    {
                        ArrayType type = (ArrayType)EditorGUILayout.EnumPopup("Array Type", _arrayType);
                        if (type != _arrayType)
                        {
                            _arrayType = type;

                            if (_selectedPrefab != null)
                            {
                                _creator = GetCreator(_arrayType, _selectedPrefab);
                            }

                            if (_creator != null)
                            {
                                _creator.Refresh(true);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Selection Field
                {
                    EditorGUILayout.BeginHorizontal(ArrayToolExtensions.BoxedHeaderStyle);
                    {
                        EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(100f));
                        GUILayout.FlexibleSpace();
                        GameObject target = (GameObject)EditorGUILayout.ObjectField(_selectedPrefab, typeof(GameObject), false);
                        if (target != null && target != _selectedPrefab)
                        {
                            _selectedPrefab = target;

                            if (_creator == null)
                            {
                                _creator = GetCreator(_arrayType, _selectedPrefab);
                            }

                            _creator.SetTarget(_selectedPrefab);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (_creator != null)
                {
                    _creator.DrawTransformControls();
                    _creator.DrawEditor();
                }

                EditorGUILayout.BeginHorizontal(ArrayToolExtensions.BoxedHeaderStyle);
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        Cancel();
                    }
                    else if (GUILayout.Button("Close"))
                    {
                        SaveAndClose();
                    }
                    else if (GUILayout.Button("Continue"))
                    {
                        SaveAndContinue();
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate()
        {
            if (_creator != null)
            {
                _creator.UpdateEditor();
            }

            Repaint();
        }

        private void RefreshArray()
        {
            if (_creator != null)
            {
                _creator.Refresh();
            }
        }

        private void ResizeWindow(ArrayCreator creator)
        {
            _window.maxSize = new Vector2(MaxWidth, creator.MaxWindowHeight);
            _window.minSize = _window.maxSize;
        }

        // #DG: Make this Generic
        // then it can be used at runtime by passing params
        private ArrayCreator GetCreator(ArrayType type, GameObject target)
        {
            if (_creator != null)
            {
                _creator.Teardown();
            }

            ArrayCreator creator = null;

            switch (type)
            {
                case ArrayType.Circle:
                    creator = new CircularArrayCreator(target);
                    break;
                case ArrayType.Arc:
                    creator = new ArcArrayCreator(target);
                    break;
                case ArrayType.Sphere:
                    creator = new SphereArrayCreator(target);
                    break;
                case ArrayType.Grid:
                    creator = new GridArrayCreator(target);
                    break;
                case ArrayType.Path:
                    creator = new BezierArrayCreator(target);
                    break;
                case ArrayType.Line:
                default:
                    creator = new LinearArrayCreator(target);
                    break;
            }

            ResizeWindow(creator);
            return creator;
        }

        private void PopulateFromExistingData(ArrayContainer container)
        {
            if (container.Data != null)
            {
                _arrayType = container.Data.Type;
                _selectedPrefab = container.Data.Prefab;
                _creator = GetCreator(_arrayType, container.Data.Prefab);
                _creator.PopulateFromExistingContainer(container);
            }
        }
    }
}
