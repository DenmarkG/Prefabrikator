﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// #DG: Convert this to UI Elements
//using UnityEngine.UIElements;
//using UnityEditor.UIElements;

namespace Prefabrikator
{
    public class PrefabrikatorTool : EditorWindow
    {
        private static readonly string WindowName = "Prefabrikator";
        private ArrayCreator _creator = null;
        private ShapeType _shapeType = ShapeType.Line;

        public const float MaxWidth = 350f;
        public const float MaxHeght = 250;

        private static PrefabrikatorTool _window = null;

        private GameObject _selectedPrefab = null;
        private ArrayContainer _loadedContainer = null;
        private bool IsInEditMode => _loadedContainer != null;

        private Vector2 _scrollPosition = new Vector2();

        private bool _isSaving = false;

        private UndoStack _undoStack = null;

        // #DG: make this work as another option
        //private bool _keepOriginal = true;

        [MenuItem("Tools/Prefabikator &a")]
        static void ArrayToolWindow()
        {
            Open();
        }

        public static void Open(ArrayContainer container = null)
        {
            _window = ScriptableObject.CreateInstance<PrefabrikatorTool>();
            //_window.maxSize = new Vector2(MaxWidth, MaxHeght);
            //_window.minSize = _window.maxSize;
            _window.titleContent = new GUIContent(WindowName);

            if (Selection.activeObject is GameObject targetObj)
            {
                _window._selectedPrefab = targetObj;
                _window._creator = _window.GetCreator(_window._shapeType, targetObj);
            }

            //_window.ShowUtility();
            _window.Show();


            if (container != null)
            {
                _window.PopulateFromExistingData(container);
            }

            _window._loadedContainer = container;

            _window._undoStack = new UndoStack();
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
                    EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                    {
                        ShapeType type = (ShapeType)EditorGUILayout.EnumPopup("Shape", _shapeType);
                        if (type != _shapeType)
                        {
                            _shapeType = type;

                            if (_selectedPrefab != null)
                            {
                                _creator = GetCreator(_shapeType, _selectedPrefab);
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
                    EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                    {
                        EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(100f));
                        GUILayout.FlexibleSpace();
                        GameObject target = (GameObject)EditorGUILayout.ObjectField(_selectedPrefab, typeof(GameObject), false);
                        if (target != null && target != _selectedPrefab)
                        {
                            _selectedPrefab = target;

                            if (_creator == null)
                            {
                                _creator = GetCreator(_shapeType, _selectedPrefab);
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
                    _creator.DrawModifiers();
                }

                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                {
                    EditorGUI.BeginDisabledGroup(_undoStack.UndoOperationsAvailable == 0);
                    {
                        if (GUILayout.Button("Undo"))
                        {
                            Undo();
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(_undoStack.RedoOperationsAvailable == 0);
                    {
                        if (GUILayout.Button("Redo"))
                        {
                            Redo();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
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
                _creator.ProcessModifiers();
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

        //private void ResizeWindow(ArrayCreator creator)
        //{
        //    _window.maxSize = new Vector2(MaxWidth, creator.MaxWindowHeight);
        //    _window.minSize = _window.maxSize;
        //}

        // #DG: Make this Generic
        // then it can be used at runtime by passing params
        private ArrayCreator GetCreator(ShapeType type, GameObject target)
        {
            if (_creator != null)
            {
                _creator.Teardown();
            }

            ArrayCreator creator = null;

            switch (type)
            {
                case ShapeType.Circle:
                    creator = new CircularArrayCreator(target);
                    break;
                case ShapeType.Arc:
                    creator = new ArcArrayCreator(target);
                    break;
                case ShapeType.Sphere:
                    creator = new SphereArrayCreator(target);
                    break;
                case ShapeType.Grid:
                    creator = new GridArrayCreator(target);
                    break;
                case ShapeType.Path:
                    creator = new BezierArrayCreator(target);
                    break;
                case ShapeType.Line:
                default:
                    creator = new LinearArrayCreator(target);
                    break;
            }

            //ResizeWindow(creator);
            creator.OnCommandExecuted += OnCommandExecuted;
            return creator;
        }

        private void PopulateFromExistingData(ArrayContainer container)
        {
            if (container.Data != null)
            {
                _shapeType = container.Data.Type;
                _selectedPrefab = container.Data.Prefab;
                _creator = GetCreator(_shapeType, container.Data.Prefab);
                _creator.PopulateFromExistingContainer(container);
            }
        }

        private void OnCommandExecuted(ICommand command)
        {
            _undoStack.OnCommandExecuted(command);
        }
        // #DG: Undo does not currently work for Add/Remove Modifiers. 
        // Need to add proccessor to when undo/redo is performed
        private void Undo()
        {
            _undoStack.Undo();
            RefreshArray();
        }

        private void Redo()
        {
            _undoStack.Redo();
            RefreshArray();
        }

        private class ModidifierOptionsPopup : EditorWindow
        {
            //
        }

        private void ShowModifierOptions(int windowId)
        {
            //
        }
    }
}