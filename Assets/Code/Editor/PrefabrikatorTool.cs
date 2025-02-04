using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    using Runtime;
    using Shapes;

    public class PrefabrikatorTool : EditorWindow
    {
        private static readonly string WindowName = "Prefabrikator";

        private ArrayCreator _creator = null;
        private ShapeType _shapeType = ShapeType.Line;
        private CustomShape _customShape;

        private GameObject SelectedObject
        {
            get => _customShape?.Seletion;
            set => _customShape?.SetSelection(value);
        }

        private bool IsInEditMode => _openMode == OpenMode.Edit;
        private OpenMode _openMode;

        private Vector2 _scrollPosition = new Vector2();

        private bool _isSaving = false;

        private UndoStack _undoStack = null;

        [MenuItem("Prefabrikator/Editor Window &a")]
        private static void ArrayToolWindow()
        {
            Open();
        }

        public static void Open(CustomShape shape = null)
        {
            PrefabrikatorTool window = ScriptableObject.CreateInstance<PrefabrikatorTool>();
            window.maxSize = new Vector2(Constants.MaxWidth, Constants.MaxHeght);
            window.minSize = window.maxSize;
            window.titleContent = new GUIContent(WindowName);

            window._customShape = shape;
            window._openMode = shape == null ? OpenMode.Create : OpenMode.Edit;

            if (window._openMode == OpenMode.Edit)
            {
                window._shapeType = shape.BaseShapeType;
                window.SelectedObject = window._customShape.Seletion;
                window._creator = window.GetCreator(window._shapeType, window.SelectedObject, window._customShape, window._openMode);
            }
            else if (Selection.activeObject is GameObject selectedObj)
            {
                GameObject proxy = new GameObject("Custom Shape");
                window._customShape = proxy.AddComponent<CustomShape>();

                window.SelectedObject = selectedObj;
                window._creator = window.GetCreator(window._shapeType, selectedObj, window._customShape, window._openMode);

                if (selectedObj.IsPrefab() == false)
                {
                    Selection.activeObject = null;
                }
            }
            else
            {
                GameObject proxy = new GameObject("Custom Shape");
                window._customShape = proxy.AddComponent<CustomShape>();
            }

            window.Show();
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
            if (SelectedObject != null)
            {
                SelectedObject.SetActive(true);
            }

            _creator = null;
        }

        private void OnGUI()
        {
            ShowToolBar();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
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

                            if (SelectedObject != null)
                            {
                                _creator = GetCreator(_shapeType, SelectedObject, _customShape, _openMode);
                            }
                        }
                        else
                        {
                            if (ShowShapeChangeDialog())
                            {
                                _shapeType = type;

                                if (SelectedObject != null)
                                {
                                    _creator.Teardown();
                                    _creator = GetCreator(_shapeType, SelectedObject, _customShape, _openMode);
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

                // Selection Field
                EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(100f));
                        GUILayout.FlexibleSpace();
                        GameObject target = (GameObject)EditorGUILayout.ObjectField(SelectedObject, typeof(GameObject), true);
                        if (target != null && target != SelectedObject)
                        {
                            SelectedObject = target;

                            if (IsInEditMode)
                            {
                                _customShape.SetSelection(SelectedObject);
                            }

                            if (_creator == null)
                            {
                                _creator = GetCreator(_shapeType, SelectedObject, _customShape, _openMode);
                            }

                            _creator.SetOriginal(SelectedObject);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
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
            float maxHeight = Mathf.Max(creator.MaxWindowHeight, this.maxSize.y);
            this.maxSize = new Vector2(Constants.MaxWidth, maxHeight);
            this.minSize = this.maxSize;
        }

        public ArrayCreator GetCreator(ShapeType type, GameObject target, CustomShape shape, OpenMode openMode)
        {
            if (_creator != null)
            {
                _creator.Teardown();
            }

            ArrayCreator creator = null;

            switch (type)
            {
                case ShapeType.Circle:
                    creator = new CircularArrayCreator(target, shape, openMode);
                    break;
                case ShapeType.Arc:
                    creator = new ArcArrayCreator(target, shape, openMode);
                    break;
                case ShapeType.Sphere:
                    creator = new SphereArrayCreator(target, shape, openMode);
                    break;
                case ShapeType.Ellipse:
                    creator = new EllipseArrayCreator(target, shape, openMode);
                    break;
                case ShapeType.Grid:
                    creator = new GridArrayCreator(target, shape, openMode);
                    break;
#if SPLINE_CREATOR
                case ShapeType.Spline:
                    creator = new BezierArrayCreator(target, shape);
                    break;
#endif
                case ShapeType.ScatterBox:
                    creator = new ScatterBoxCreator(target, shape, openMode);
                    break;
                case ShapeType.ScatterSphere:
                    creator = new ScatterSphereCreator(target, shape, openMode);
                    break;
                case ShapeType.ScatterPlane:
                    creator = new ScatterPlaneCreator(target, shape, openMode);
                    break;
                case ShapeType.Line:
                default:
                    creator = new LinearArrayCreator(target, shape, openMode);
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

        private void Undo(Object obj)
        {
            _undoStack.Undo(obj);
            RefreshArray();
        }

        private void Redo(Object obj)
        {
            _undoStack.Redo(obj);
            RefreshArray();
        }

        private bool ShowShapeChangeDialog()
        {
            return EditorUtility.DisplayDialog("Change Shape Type?", "Changing shapes will lose current progress. \nDo you want to continue?", "Change", "Cancel");
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

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Constants.SaveButton, EditorStyles.toolbarButton, GUILayout.Width(Constants.SmallButtonWidth)))
                {
                    SaveAndClose();
                }

                GUIContent undoButton = (_undoStack.UndoOperationsAvailable > 0) ? Constants.UndoButton : Constants.UndoDisabledButton;

                if (GUILayout.Button(undoButton, EditorStyles.toolbarButton, GUILayout.Width(Constants.SmallButtonWidth)))
                {
                    Undo(_customShape);
                }

                GUIContent redoButton = (_undoStack.RedoOperationsAvailable > 0) ? Constants.RedoButton : Constants.RedoDisabledButton;
                if (GUILayout.Button(redoButton, EditorStyles.toolbarButton, GUILayout.Width(Constants.SmallButtonWidth)))
                {
                    Redo(_customShape);
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
            fileMenu.Add("Save and Close", SaveAndClose);
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

            editMenu.Add("Undo", () => Undo(_customShape), _undoStack.UndoOperationsAvailable == 0);
            editMenu.Add("Redo", () => Redo(_customShape), _undoStack.RedoOperationsAvailable == 0);

            //editMenu.ShowAsContext();
            Rect pos = new Rect();
            pos.x = Constants.ToolbarButtonWidth;
            pos.y = EditorStyles.toolbar.fixedHeight;

            editMenu.DropDown(pos);
        }
    }
}
