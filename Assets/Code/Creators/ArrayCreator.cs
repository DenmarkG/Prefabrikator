using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ArrayCreator
    {
        public event System.Action<ICommand> OnCommandExecuted = null;

        protected Queue<ICommand> CommandQueue => _commandQueue;
        private Queue<ICommand> _commandQueue = new Queue<ICommand>();

        protected GameObject _target = null;

        public GameObject TargetProxy
        {
            get { return _targetProxy; }
            set { _targetProxy = value; }
        }

        protected GameObject _targetProxy = null;
        protected List<GameObject> _createdObjects = null;

        protected int _targetCount = 1;
        protected bool _needsRefresh = false;

        protected GUIStyle _boxedHeaderStyle = null;
        private readonly string _boxStyle = "toolbar";

        public abstract float MaxWindowHeight { get; }
        public abstract string Name { get; }

        bool _showTransformControls = false;
        protected bool _showRotationControls = true;
        protected Quaternion _targetRotation = Quaternion.identity;
        protected Vector3 _targetScale = Vector3.one;

        protected ArrayData _defaultData = null;

        public ArrayCreator(GameObject target)
        {
            _target = target;
            _createdObjects = new List<GameObject>();

            _boxedHeaderStyle = new GUIStyle(_boxStyle);
            _boxedHeaderStyle.fixedHeight = 0;
            _boxedHeaderStyle.fontSize = EditorStyles.label.fontSize;
            int h = Mathf.CeilToInt(EditorGUIUtility.singleLineHeight);
            int v = Mathf.CeilToInt(EditorGUIUtility.singleLineHeight * .3f);
            _boxedHeaderStyle.padding = new RectOffset(h, h, v, v);

            _needsRefresh = true;
        }

        public virtual void Teardown()
        {
            OnCommandExecuted = null;
            DestroyAll();
        }

        public virtual void OnCreateWindow() { }

        public abstract void DrawEditor();
        public abstract void UpdateEditor();
        public abstract void Refresh(bool hardRefresh = false, bool useDefaultData = false);

        protected void DestroyAll()
        {
            if (_targetProxy != null)
            {
                GameObject.DestroyImmediate(_targetProxy);
            }

            if (_createdObjects.Count > 0)
            {
                int numObjectsCreated = _createdObjects.Count;
                if (numObjectsCreated > 0)
                {
                    for (int i = 0; i < numObjectsCreated; ++i)
                    {
                        GameObject.DestroyImmediate(_createdObjects[i]);
                    }
                }
            }

            _createdObjects.Clear();
        }

        public void OnCloseWindow(bool shouldSaveObjects = true)
        {
            if (!shouldSaveObjects)
            {
                Teardown();
            }
            else
            {
                OnSave();
            }
        }

        protected virtual void OnSave() { }

        public void SaveAndContinue()
        {
            OnSave();
            _targetProxy = null;
            _createdObjects.Clear();
            _defaultData = null;
            Refresh(true);
        }

        public void CancelPendingEdits()
        {
            PopulateFromExistingData(_defaultData);
            Refresh();
        }

        public virtual void OnSelectionChange()
        {
            if ((Selection.activeObject is GameObject activeObject) && activeObject != _target && activeObject != _targetProxy)
            {
                if (!string.IsNullOrEmpty(activeObject.scene.name))
                {
                    bool isChildSelection = (_createdObjects.Count > 0 && _createdObjects.Contains(activeObject));
                    if (!isChildSelection)
                    {
                        _target = activeObject;
                        Refresh(hardRefresh: true);
                    }
                    else
                    {
                        Selection.activeGameObject = _target;
                    }
                }
            }
        }

        protected void DestroyClone(GameObject clone)
        {
            _createdObjects.RemoveAt(_createdObjects.IndexOf(clone));
            GameObject.DestroyImmediate(clone);
        }

        public void SetTarget(GameObject target)
        {
            if (target != null)
            {
                _target = target;
                EstablishHelper();

                Refresh(true);
            }
        }

        protected void EstablishHelper(bool useDefaultData = false)
        {
            if (_targetProxy == null)
            {
                _targetProxy = new GameObject($"{_target.name} {Name}");
            }

            ArrayContainer container = GetOrAddContainer(_targetProxy);
            container.SetData((useDefaultData && _defaultData != null) ? _defaultData : GetContainerData());
        }

        protected virtual void UpdateLocalRotations()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.localRotation = _targetRotation;
            }
        }

        protected virtual void UpdateLocalScales()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.localScale = _targetScale;
            }
        }


        public void DrawTransformControls()
        {
            _showTransformControls = EditorGUILayout.Foldout(_showTransformControls, "Original Transform");

            if (_showTransformControls)
            {
                if (_showRotationControls)
                {
                    if (ArrayToolExtensions.DisplayRotationField(ref _targetRotation))
                    {
                        _needsRefresh = true;
                    }
                }

                if (ArrayToolExtensions.DisplayScaleField(ref _targetScale))
                {
                    _needsRefresh = true;
                }
            }
        }

        private ArrayContainer GetOrAddContainer(GameObject target)
        {
            ArrayContainer container = target.GetComponent<ArrayContainer>();
            if (container == null)
            {
                container = target.AddComponent<ArrayContainer>();
            }

            return container;
        }

        protected abstract ArrayData GetContainerData();
        public void PopulateFromExistingContainer(ArrayContainer container)
        {
            _defaultData = container.Data;
            PopulateFromExistingData(container.Data);
            PopulateFromExistingClones(container.gameObject);
            Refresh();
        }

        protected abstract void PopulateFromExistingData(ArrayData data);

        protected void PopulateFromExistingClones(GameObject targetProxy)
        {
            _targetProxy = targetProxy;
            int childCount = _targetProxy.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform child = _targetProxy.transform.GetChild(i);
                if (child != null)
                {
                    _createdObjects.Add(child.gameObject);
                }
            }
        }

        public virtual void SetTargetCount(int targetCount)
        {
            _targetCount = targetCount;
        }

        protected void ExecuteCommand(ICommand command)
        {
            command.Execute();
            OnCommandExecuted(command);
        }
    }
}
