using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Prefabrikator
{
    public abstract class ArrayCreator
    {
        public event System.Action<ICommand> OnCommandExecuted = null;

        public delegate void ApplicatorDelegate(GameObject go);
        public delegate void IndexedApplicatorDelegate(GameObject go, int index);

        public Queue<ICommand> CommandQueue => _commandQueue;
        private Queue<ICommand> _commandQueue = new Queue<ICommand>();

        protected GameObject _target = null;

        private GameObject _targetProxy = null;

        public List<GameObject> CreatedObjects => _createdObjects;
        protected List<GameObject> _createdObjects = null;

        public int TargetCount => _targetCount;
        private Shared<int> _targetCount = new Shared<int>(1);
        private IntProperty _countProperty = null;
        
        protected bool NeedsRefresh => CommandQueue.Count > 0;

        public abstract float MaxWindowHeight { get; }
        public abstract string Name { get; }

        protected Quaternion _targetRotation = Quaternion.identity;

        private List<Modifier> _modifierStack = new List<Modifier>();
        int _indexOfModifierToAdd = 0;
        private Modifier _activeModifierSelection= null;

        public abstract int MinCount { get; }

        private ReorderableList _modifierDisplay = null;

        protected bool IsEditMode => _editMode != EditMode.None;
        protected EditMode _editMode = EditMode.None;
        protected SceneView _sceneView = null;

        protected bool _refreshOnCountChange = false;

        public abstract ShapeType Shape { get; }

        public ArrayState State { get; private set; }

        public ArrayCreator(GameObject target, int defaultCount)
        {
            _target = target;

            _targetCount.Set(defaultCount);
            void OnCountChange(int current, int previous)
            {
                current = EnforceValidCount(current);
                CommandQueue.Enqueue(new CountChangeCommand(this, previous, current));
            }

            _countProperty = new IntProperty("Count", _targetCount, OnCountChange, EnforceValidCount);
            _countProperty.AddCustomButton(Constants.PlusButton, (value) => { _targetCount.Set(++value); });
            _countProperty.AddCustomButton(Constants.MinusButton, (value) => { _targetCount.Set(--value); });
            
            // Modifier Reorderable List Setup
            _modifierDisplay = new ReorderableList(_modifierStack, typeof(Modifier), true, true, false, true);
            _modifierDisplay.drawHeaderCallback = DrawListHeader;
            _modifierDisplay.multiSelect = false;
            _modifierDisplay.drawElementCallback = DrawElement;
            _modifierDisplay.onRemoveCallback = RemoveModifierFromList;

            _modifierDisplay.onSelectCallback = OnModifierSelectionChange;

            _createdObjects = new List<GameObject>(_targetCount);
            OnTargetCountChanged();

            SceneView.duringSceneGui += OnSceneGUI;

            Refresh();
        }

        public void Teardown()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();

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

        public void ClearSceneGUI()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        protected virtual void OnSave() { }

        public void SaveAndContinue()
        {
            OnSave();
            _targetProxy = null;
            _createdObjects.Clear();
            Refresh(true);
        }

        public void CancelPendingEdits()
        {
            PopulateFromExistingData(State);
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

        /// <summary>
        /// Attempts to clone the target object
        /// </summary>
        /// <param name="index">The position in the list of created objects to create the clone</param>
        /// <returns>true if the clone was created successfully</returns>
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
            container.SetData((useDefaultData && State != null) ? State : GetContainerData());
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

        protected abstract ArrayState GetContainerData();
        protected abstract void PopulateFromExistingData(ArrayState data);
        
        public void PopulateFromExistingContainer(ArrayContainer container)
        {
            SetState(container.Data);
            PopulateFromExistingData(container.Data);
            PopulateFromExistingClones(container.gameObject);
            Refresh();
        }

        public abstract void OnStateSet(ArrayState stateData);
        public void SetState(ArrayState stateData)
        {
            State = stateData;
            OnStateSet(stateData);
            Refresh();
        }

        public virtual ArrayState GetState() => null;

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

        protected void ShowCountField()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                int count = EnforceValidCount(_countProperty.Update());
                SetTargetCount(count);
            }
        }

        protected virtual int EnforceValidCount(int count)
        {
            return Mathf.Max(count, MinCount);
        }

        public virtual void SetTargetCount(int targetCount, bool shouldTriggerCallback = true)
        {
            _targetCount.Set(targetCount);

            if (shouldTriggerCallback)
            {
                OnTargetCountChanged();

                if (_refreshOnCountChange)
                {
                    Refresh();
                }
            }
        }

        protected void OnTargetCountChanged()
        {
            if (TargetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > TargetCount)
                {
                    int index = _createdObjects.Count - 1;
                    if (index >= 0)
                    {
                        DestroyClone(_createdObjects[_createdObjects.Count - 1]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (TargetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

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

        public void ApplyToAll(ApplicatorDelegate applicator)
        {
            int numObjs = _createdObjects.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(_createdObjects[i]);
            }
        }

        public void ApplyToAll(IndexedApplicatorDelegate applicator)
        {
            int numObjs = _createdObjects.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(_createdObjects[i], i);
            }
        }

        protected void SetSceneViewDirty()
        {
            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        public abstract Vector3 GetDefaultPositionAtIndex(int index);
        protected virtual void OnSceneGUI(SceneView view) { }
        //protected abstract void OnEditModeEnter(EditMode mode);
        //protected abstract void OnEditModeExit();

        #region Modifiers
        //
        // Modifiers
        // #DG: move this to a unified dictionary, keyed by modifier type
        protected virtual string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom,
                ModifierType.ScaleRandom,
                ModifierType.ScaleUniform,
                ModifierType.RotationRandom,
                ModifierType.RotationUniform,
                ModifierType.PositionNoise,
            };

            return mods;
        }
        
        public void DrawModifiers()
        {
            int indentLevel = 20;
            GUILayout.Space(indentLevel);
            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(PrefabrikatorTool.MaxWidth - indentLevel));
            {
                GUILayout.Space(indentLevel);
                // #DG: Get the modifier list from the Array itself. 
                string[] options = GetAllowedModifiers();
                _indexOfModifierToAdd = EditorGUILayout.Popup(_indexOfModifierToAdd, options);
                if (GUILayout.Button(Constants.PlusButton))
                {
                    AddModifier(options[_indexOfModifierToAdd]);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(PrefabrikatorTool.MaxWidth- indentLevel));
            {
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();
                {
                    _modifierDisplay.DoLayoutList();
                    if (_activeModifierSelection != null)
                    {
                        _activeModifierSelection.UpdateInspector();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isActive)
            {
                _activeModifierSelection = _modifierStack[index];
            }

            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _modifierStack[index].GetType().ToString());
        }

        private void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Added Modifiers");
        }

        void OnModifierSelectionChange(ReorderableList list)
        {
            if (list.selectedIndices != null && list.selectedIndices.Count > 0)
            {
                int index = list.selectedIndices[0];
                if (_modifierStack != null && _modifierStack.Count > 0 && index < _modifierStack.Count)
                {
                    _activeModifierSelection = _modifierStack[index];
                    return;
                }
            }

            _activeModifierSelection = null;
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

        private void RemoveModifierFromList(ReorderableList list)
        {
            if (_activeModifierSelection != null)
            {
                _activeModifierSelection.OnRemoved();
                CommandQueue.Enqueue(new ModifierRemoveCommand(_activeModifierSelection, this));
            }

            if (_modifierStack.Count == 0)
            {
                _activeModifierSelection = null;
            }
        }

        public void RemoveModifier(int index)
        {
            // #DG: TODO: add some bounds checking here
            Modifier mod = _modifierStack[index];
            _modifierStack.RemoveAt(index);

            if (_modifierStack.Count == 0 || _activeModifierSelection == mod)
            {
                _activeModifierSelection = null;
            }

            mod.OnRemoved();
        }

        public void RemoveModifier(Modifier modifier)
        {
            RemoveModifier(_modifierStack.IndexOf(modifier));
        }

        public int? GetIndexOfModifier(Modifier mod)
        {
            if (_modifierStack != null)
            {
                for (int i = 0; i < _modifierStack.Count; ++i)
                {
                    if (mod == _modifierStack[i])
                    {
                        return i;
                    }
                }
            }
            return null;
        }

        public T GetUpstreamModifierOfType<T>(int startIndex)
        {
            if (_modifierStack != null)
            {
                Modifier mod = null;
                for (int i = startIndex; i >= 0; --i)
                {
                    mod = _modifierStack[i];
                    if (mod is T typedMod)
                    {
                        return typedMod;
                    }
                }
            }

            return default(T);
        }

        #endregion // Modifiers
    }
}
