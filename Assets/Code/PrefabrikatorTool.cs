using UnityEngine;
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

        public const float MaxWidth = 500;
        public const float MaxHeght = 750;

        private static PrefabrikatorTool _window = null;

        private GameObject _selectedObject = null;
        private ArrayContainer _loadedContainer = null;
        private bool IsInEditMode => _loadedContainer != null;

        private Vector2 _scrollPosition = new Vector2();

        private bool _isSaving = false;

        private UndoStack _undoStack = null;

        private bool _keepOriginal = false;

        [MenuItem("Prefabikator/Duplicator &a")]
        static void ArrayToolWindow()
        {
            Open();
        }

        public static void Open(ArrayContainer container = null)
        {
            _window = ScriptableObject.CreateInstance<PrefabrikatorTool>();
            _window.maxSize = new Vector2(MaxWidth, MaxHeght);
            _window.minSize = _window.maxSize;
            _window.titleContent = new GUIContent(WindowName);

            if (Selection.activeObject is GameObject targetObj)
            {
                _window._selectedObject = targetObj;
                _window._creator = _window.GetCreator(_window._shapeType, targetObj);

                if (IsPrefab(targetObj) == false || !_window._keepOriginal)
                {
                    targetObj.SetActive(false);
                    Selection.activeObject = null;
                }
            }

            //_window.ShowUtility();
            _window.Show();

            if (container != null)
            {
                _window.PopulateFromExistingData(container);
            }

            _window._loadedContainer = container;
        }

        private void Awake()
        {
            _undoStack = new UndoStack();
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

            // #DG: ensure this works each close
            if (IsPrefab(_selectedObject) == false)
            {
                if (_isSaving)
                {
                    GameObject.Destroy(_selectedObject);
                    _selectedObject = null;
                }
                else
                {
                    _selectedObject.SetActive(true);
                }
            }
            else
            {
                if (_selectedObject != null)
                {
                    _selectedObject.SetActive(true);
                }
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
                            bool undoStackIsEmpty = (_undoStack.UndoOperationsAvailable == 0) && (_undoStack.RedoOperationsAvailable == 0);
                            if (_creator == null || undoStackIsEmpty)
                            {
                                _shapeType = type;

                                if (_selectedObject != null)
                                {
                                    _creator = GetCreator(_shapeType, _selectedObject);
                                }
                            }
                            else
                            {
                                if (ShowShapeChangeDialog())
                                {
                                    _shapeType = type;

                                    if (_selectedObject != null)
                                    {
                                        _creator.Teardown();
                                        _creator = GetCreator(_shapeType, _selectedObject);
                                        _undoStack.Clear();
                                    }
                                }
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
                EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(100f));
                        GUILayout.FlexibleSpace();
                        GameObject target = (GameObject)EditorGUILayout.ObjectField(_selectedObject, typeof(GameObject), false);
                        if (target != null && target != _selectedObject)
                        {
                            _selectedObject = target;

                            if (_creator == null)
                            {
                                _creator = GetCreator(_shapeType, _selectedObject);
                            }

                            _creator.SetTarget(_selectedObject);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (_selectedObject != null && IsPrefab(_selectedObject))
                    {
                        bool keepOriginal = EditorGUILayout.ToggleLeft("Keep Original", _keepOriginal);
                        if (_keepOriginal != keepOriginal)
                        {
                            _selectedObject.SetActive(keepOriginal);
                            _keepOriginal = keepOriginal;
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                // Shape Options
                GUILayout.Space(Extensions.IndentSize);
                EditorGUILayout.LabelField("Shape Options", EditorStyles.boldLabel);
                if (_creator != null)
                {
                    _creator.DrawEditor();
                    _creator.DrawModifiers();
                }

                GUILayout.FlexibleSpace();

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

        private void ResizeWindow(ArrayCreator creator)
        {
            float maxHeight = Mathf.Max(creator.MaxWindowHeight, _window.maxSize.y);
            _window.maxSize = new Vector2(MaxWidth, maxHeight);
            _window.minSize = _window.maxSize;
        }

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
                case ShapeType.Ellipse:
                    creator = new EllipseArrayCreator(target);
                    break;
                case ShapeType.Grid:
                    creator = new GridArrayCreator(target);
                    break;
#if PATH
                case ShapeType.Path:
                    creator = new BezierArrayCreator(target);
                    break;
#endif // PATH
                case ShapeType.ScatterBox:
                    creator = new ScatterBoxCreator(target);
                    break;
                case ShapeType.ScatterSphere:
                    creator = new ScatterSphereCreator(target);
                    break;
                case ShapeType.ScatterPlane:
                    creator = new ScatterPlaneCreator(target);
                    break;
                case ShapeType.Line:
                default:
                    creator = new LinearArrayCreator(target);
                    break;
            }

            ResizeWindow(creator);
            creator.OnCommandExecuted += OnCommandExecuted;
            return creator;
        }

        private void PopulateFromExistingData(ArrayContainer container)
        {
            if (container.Data != null)
            {
                _shapeType = container.Data.Type;
                //_creator = GetCreator(_shapeType, container.Data.Prefab);
                _creator.PopulateFromExistingContainer(container);
            }
        }

        private void OnCommandExecuted(ICommand command)
        {
            _undoStack.OnCommandExecuted(command);
        }
        
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

        private bool ShowShapeChangeDialog()
        {
            return EditorUtility.DisplayDialog("Change Shape Type?", "Changing shapes will lose current progress. \nDo you want to continue?", "Change", "Cancel");
        }

        private static bool IsPrefab(GameObject obj)
        {
            return PrefabUtility.GetPrefabInstanceHandle(obj) == null;
        }
    }
}
