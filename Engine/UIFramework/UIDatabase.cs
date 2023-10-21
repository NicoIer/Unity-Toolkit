using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public class UIDatabase : ScriptableObject
    {
        private Dictionary<int, GameObject> _panelDict;
        [SerializeField] private List<GameObject> _panels = new List<GameObject>();

        private void InitPanelDict()
        {
            _panelDict = new Dictionary<int, GameObject>();

            foreach (var panel in _panels)
            {
                int id = panel.GetComponent<IUIPanel>().GetType().GetHashCode();
                _panelDict.Add(id, panel);
            }
        }

        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public T CreatePanel<T>() where T : class, IUIPanel
        {
            if (_panelDict == null)
            {
                InitPanelDict();
            }

            int id = typeof(T).GetHashCode();
            if (_panelDict.TryGetValue(id, out GameObject value))
            {
                if (value.GetComponent<T>() == null)
                {
                    throw new ArgumentException(
                        $"UIPanel prefab:{value} doesn't contain UIPanel {typeof(T)} component");
                }

                GameObject panel = Instantiate(value);
                //修改RectTransform为填满的模式
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return panel.GetComponent<T>();
            }

            throw new KeyNotFoundException($"{typeof(T)} hasn't been register in ui database");
        }

        public IUIPanel CreatePanel(Type type)
        {
            if (_panelDict == null) InitPanelDict();
            int id = type.GetHashCode();
            if (_panelDict.TryGetValue(id, out GameObject value))
            {
                if (value.GetComponent(type) == null)
                {
                    throw new ArgumentException(
                        $"UIPanel prefab:{value} doesn't contain UIPanel {type} component");
                }

                GameObject panel = Instantiate(value);
                //修改RectTransform为填满的模式
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return (IUIPanel)panel.GetComponent(type);
            }

            throw new KeyNotFoundException($"{type} hasn't been register in ui database");
        }


        //运行时动态添加
        public void Add(GameObject gameObject)
        {
            if (_panelDict == null) InitPanelDict();
            if (!gameObject.TryGetComponent(out IUIPanel panel)) return;
            int id = panel.GetType().GetHashCode();

            _panelDict[id] = gameObject;
        }

#if UNITY_EDITOR
        // 编辑器下动态添加 校验使用
        private void OnValidate()
        {
            // 移除空的
            HashSet<int> ids = new HashSet<int>();
            for (int i = _panels.Count - 1; i > 0; i--)
            {
                var panelPrefab = _panels[i];
                if (panelPrefab == null)
                {
                    Debug.LogError($"UIDatabase中存在空的panel prefab");
                    _panels.RemoveAt(i);
                    continue;
                }

                if (!panelPrefab.TryGetComponent(out IUIPanel uiPanel))
                {
                    Debug.LogError($"UIDatabase中存在不包含IUIPanel的panel prefab:{panelPrefab}");
                    _panels.RemoveAt(i);
                    continue;
                }

                int id = uiPanel.GetType().GetHashCode();
                if (!ids.Contains(id)) continue;
                Debug.LogError($"UIDatabase中存在重复的panel id:{id}");
                _panels.RemoveAt(i);
                continue;
            }
        }
#endif

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Tooltip("刷新Database所在文件夹下的所有prefab")]
        [Sirenix.OdinInspector.Button("Refresh")]
        public void Refresh()
        {
            //搜索自己所在的文件夹下的所有prefab
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            string dir = System.IO.Path.GetDirectoryName(path);
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { dir });
            HashSet<int> ids = new HashSet<int>();
            foreach (var panel in _panels)
            {
                ids.Add(panel.GetComponent<IUIPanel>().GetType().GetHashCode());
            }

            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue; //不是prefab
                if (!prefab.TryGetComponent(out IUIPanel panel)) continue; //不包含IUIPanel
                if (_panels.Contains(prefab)) continue; //已经存在
                if (ids.Contains(panel.GetType().GetHashCode())) continue; //已经存在
                _panels.Add(prefab);
            }
        }
#endif
    }
}