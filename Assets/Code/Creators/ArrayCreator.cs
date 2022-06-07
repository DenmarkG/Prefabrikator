﻿using System.Collections.Generic;
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

        private GameObject _targetProxy = null;

        public List<GameObject> CreatedObjects => _createdObjects;
        protected List<GameObject> _createdObjects = null;

        protected int _targetCount = 1;
        protected bool NeedsRefresh => CommandQueue.Count > 0;

        public abstract float MaxWindowHeight { get; }
        public abstract string Name { get; }

        protected Quaternion _targetRotation = Quaternion.identity;
        protected ArrayData _defaultData = null;


        private List<Modifier> _modifierStack = new List<Modifier>();
        int _selectedModifier = 0;

        public ArrayCreator(GameObject target, int defaultCount)
        {
            _target = target;

            _targetCount = defaultCount;

            _createdObjects = new List<GameObject>(_targetCount);
            OnTargetCountChanged();

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

        protected abstract void CreateClone(int index = 0);

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

        protected GameObject GetProxy()
        {
            if (_targetProxy == null)
            {
                EstablishHelper();
            }

            return _targetProxy;
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
            OnTargetCountChanged();
        }

        protected abstract void OnTargetCountChanged();

        protected void ExecuteCommand(ICommand command)
        {
            command.Execute();
            OnCommandExecuted(command);
        }

        public Vector3 GetDefaultScale()
        {
            if (_target != null)
            {
                return _target.transform.localScale;
            }

            return new Vector3(1f, 1f, 1f);
        }

        public Quaternion GetDefaultRotation()
        {
            if (_target != null)
            {
                return _target.transform.rotation;
            }

            return Quaternion.identity;
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
        protected virtual string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom,
                ModifierType.ScaleRandom,
                ModifierType.ScaleUniform,
                ModifierType.RotationRandom,
                ModifierType.RotationUniform,
            };

            return mods;
        }

        public void DrawModifiers()
        {
            int numMods = _modifierStack.Count;
            for (int i = 0; i < numMods; ++i)
            {
                _modifierStack[i].UpdateInspector();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(PrefabrikatorTool.MaxWidth));
            {
                // #DG: Get the modifier list from the Array itself. 
                string[] options = GetAllowedModifiers();
                _selectedModifier = EditorGUILayout.Popup(_selectedModifier, options);
                if (GUILayout.Button("Add"))
                {
                    AddModifier(options[_selectedModifier]);
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

        private void AddModifier(string modifierName)
        {
            Modifier mod = ModifierFactory.CreateModifier(modifierName, this);

            if (mod != null)
            {
                CommandQueue.Enqueue(new ModifierAddCommand(mod, this));
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
