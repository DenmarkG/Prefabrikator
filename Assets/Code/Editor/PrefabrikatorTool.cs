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
        public enum OpenMode
        {
            Create,
            Edit,
        }

        private static readonly string WindowName = "Prefabrikator";

        private ArrayCreator _creator = null;
        private ShapeType _shapeType = ShapeType.Line;

        private static PrefabrikatorTool _window = null;

        private GameObject _selectedObject = null;
        private bool IsInEditMode => _openMode == OpenMode.Edit;
        private OpenMode _openMode;

        private Vector2 _scrollPosition = new Vector2();

        private bool _isSaving = false;

        private UndoStack _undoStack = null;

        private bool _keepOriginal = false;

        [MenuItem("Prefabikator/Duplicator &a")]
        private static void ArrayToolWindow()
        {
            Open();
        }

        public static void Open(OpenMode mode = OpenMode.Create)
        {
            _window = ScriptableObject.CreateInstance<PrefabrikatorTool>();
            _window.maxSize = new Vector2(Constants.MaxWidth, Constants.MaxHeght);
            _window.minSize = _window.maxSize;
            _window.titleContent = new GUIContent(WindowName);
            _window._openMode = mode;

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

            _window.Show();
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
            _creator.OnCloseWindow(ToolCloseMode.SaveAndContinue);
        }

        private void Cancel()
        {
            if (_creator != null)
            {
                if (IsInEditMode)
                {
                    _isSaving = true;
                    _creator.CancelPendingEdits();
                    _isSaving = true;
                }
                else
                {
                    _isSaving = false;
                }
            }

            this.Close();
        }

        private void OnDestroy()
        {
            if (_undoStack.UndoOperationsAvailable > 0 && _isSaving == false)
            {
                if (EditorUtility.DisplayDialog("Save and Close", "Would you like to save changes?", "Save", "Close"))
                {
                    _isSaving = true;
                }
            }

            if (_creator != null)
            {
                _creator.ClearSceneGUI();
                _creator.OnCloseWindow(_isSaving ? ToolCloseMode.SaveAndClose : ToolCloseMode.CancelAndClose);
            }

            // #DG: ensure this works each close
            if (_selectedObject != null)
            {
                if (_isSaving)
                {
                    if (_keepOriginal)
                    {
                        _selectedObject.SetActive(true);
                    }
                    else
                    {
                        if (IsPrefab(_selectedObject) == false)
                        {
                            GameObject.DestroyImmediate(_selectedObject);
                        }
                    }
                    _selectedObject = null;
                }
                else
                {
                    _selectedObject.SetActive(true);
                }
            }

            _creator = null;
            _selectedObject = null;
            _window = null;
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                if (!IsInEditMode)
                {
                    ShowToolBar();

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
                        GameObject target = (GameObject)EditorGUILayout.ObjectField(_selectedObject, typeof(GameObject), true);
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

                    if (_selectedObject != null && !IsPrefab(_selectedObject))
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
                GUILayout.Space(Constants.IndentSize);
                EditorGUILayout.LabelField("Shape Options", EditorStyles.boldLabel);
                if (_creator != null)
                {
                    _creator.DrawEditor();
                    _creator.DrawModifiers();
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate()
        {
            if (_creator != null)
            {
                _creator.UpdateEditor();
                TransformProxy[] proxies = _creator.ProcessModifiers();
                _creator.ApplyTransforms(proxies);
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
            _window.maxSize = new Vector2(Constants.MaxWidth, maxHeight);
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
#if SPLINE_CREATOR
                case ShapeType.Spline:
                    creator = new BezierArrayCreator(target);
                    break;
#endif
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
            return string.IsNullOrEmpty(obj.scene.name);
        }

        private void ShowToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (ToolbarButton("File"))
                {
                    ShowFileMenu();
                }
                if (ToolbarButton("Edit"))
                {
                    ShowEditMenu();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool ToolbarButton(string buttonName)
        {
            return GUILayout.Button(buttonName, EditorStyles.toolbarButton, GUILayout.Width(Constants.ToolbarButtonWidth));
        }

        private void ShowFileMenu()
        {
            GenericMenu fileMenu = new GenericMenu();
            fileMenu.Add("Save", SaveAndContinue);
            fileMenu.Add("Save and Close", SaveAndContinue);
            fileMenu.AddSeparator("");
            fileMenu.Add("Cancel", Cancel);

            Rect pos = new Rect();
            pos.y = EditorStyles.toolbar.fixedHeight;

            fileMenu.DropDown(pos);
        }

        private void ShowEditMenu()
        {
            _undoStack ??= new UndoStack();

            GenericMenu editMenu = new GenericMenu();

            editMenu.Add("Undo", SaveAndContinue, _undoStack.UndoOperationsAvailable == 0);
            editMenu.Add("Redo", SaveAndContinue, _undoStack.RedoOperationsAvailable == 0);

            //editMenu.ShowAsContext();
            Rect pos = new Rect();
            pos.x = Constants.ToolbarButtonWidth;
            pos.y = EditorStyles.toolbar.fixedHeight;

            editMenu.DropDown(pos);
        }
    }
}
