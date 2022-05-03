using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ArrayCreator
    {
        public event System.Action<ICommand> OnCommandExecuted = null;

        public Queue<ICommand> CommandQueue => _commandQueue;
        private Queue<ICommand> _commandQueue = new Queue<ICommand>();

        protected GameObject _target = null;

        public GameObject TargetProxy
        {
            get { return _targetProxy; }
            set { _targetProxy = value; }
        }

        protected GameObject _targetProxy = null;

        public List<GameObject> CreatedObjects => _createdObjects;
        protected List<GameObject> _createdObjects = null;

        protected int _targetCount = 1;
        protected bool NeedsRefresh => CommandQueue.Count > 0;

        public abstract float MaxWindowHeight { get; }
        public abstract string Name { get; }

        bool _showTransformControls = false;
        protected bool _showRotationControls = true;
        protected Quaternion _targetRotation = Quaternion.identity;
        private QuaternionProperty _rotationProperty = null;

        protected ArrayData _defaultData = null;


        private List<Modifier> _modifierStack = new List<Modifier>();
        ModifierType _selectedModifier = ModifierType.ScaleRandom;

        public ArrayCreator(GameObject target)
        {
            _target = target;
            _createdObjects = new List<GameObject>();

            _rotationProperty = new QuaternionProperty("Rotation", _targetRotation, OnRotationChanged);

            Refresh();
        }

        public virtual void Teardown()
        {
            OnCommandExecuted = null;
            DestroyAll();
        }

        public virtual void OnCreateWindow() { }

        public abstract void DrawEditor();
        public abstract void UpdateEditor();
        protected abstract void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false);

        public void Refresh(bool hardRefresh = false, bool useDefaultData = false)
        {
            ExecuteAllCommands();
            OnRefreshStart(hardRefresh, useDefaultData);
            ProcessModifiers();
        }

        private void ExecuteAllCommands()
        {
            ICommand nextCommand = null;
            while (CommandQueue.Count > 0)
            {
                nextCommand = CommandQueue.Dequeue();
                ExecuteCommand(nextCommand);
            }
        }

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

        public void DrawTransformControls()
        {
            _showTransformControls = EditorGUILayout.Foldout(_showTransformControls, "Original Transform");

            if (_showTransformControls)
            {
                if (_showRotationControls)
                {
                    _targetRotation = _rotationProperty.Update();
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

        private void OnRotationChanged(Quaternion current, Quaternion previous)
        {
            void SetRotation(Quaternion rotation)
            {
                _targetRotation = rotation;
            }
            CommandQueue.Enqueue(new ValueChangedCommand<Quaternion>(current, previous, SetRotation));
        }

        public Vector3 GetDefaultScale()
        {
            if (_target != null)
            {
                return _target.transform.localScale;
            }

            return new Vector3(1f, 1f, 1f);
        }

        public delegate void ApplicatorDelegate(GameObject go);

        public void ApplyToAll(ApplicatorDelegate applicator)
        {
            int numObjs = _createdObjects.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(_createdObjects[i]);
            }
        }

        //
        // Modifiers
        public void DrawModifiers()
        {
            int numMods = _modifierStack.Count;
            for (int i = 0; i < numMods; ++i)
            {
                _modifierStack[i].UpdateInspector();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(PrefabrikatorTool.MaxWidth));
            {
                _selectedModifier = (ModifierType)EditorGUILayout.EnumPopup(_selectedModifier);
                if (GUILayout.Button("Add"))
                {
                    AddModifier(_selectedModifier);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public void ProcessModifiers()
        {
            int numMods = _modifierStack.Count;
            for (int i = 0; i < numMods; ++i)
            {
                _modifierStack[i].Process(_createdObjects.ToArray());
            }
        }

        private void AddModifier(ModifierType modifierType)
        {
            Modifier mod = GetModifierFromType(modifierType);

            if (mod != null)
            {
                CommandQueue.Enqueue(new ModifierAddCommand(mod, this));
            }
        }

        private Modifier GetModifierFromType(ModifierType modifierType)
        {
            switch (modifierType)
            {
                case ModifierType.ScaleRandom:
                    return new RandomScaleModifier(this);
                case ModifierType.ScaleUniform:
                    return new UniformScaleModifier(this);
                default:
                    return null;
            }
        }

        public void AddModifier(Modifier modifier)
        {
            _modifierStack.Add(modifier);
        }

        public void RemoveModifier(int index)
        {
            // #DG: TODO: add some bounds checking here
            Modifier mod = _modifierStack[index];
            _modifierStack.RemoveAt(index);

            mod.OnRemoved();
        }

        public void RemoveModifier(Modifier modifier)
        {
            RemoveModifier(_modifierStack.IndexOf(modifier));
        }
    }
}
