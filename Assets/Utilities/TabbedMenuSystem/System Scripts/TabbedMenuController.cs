using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TabbedMenuSystem
{
	[RequireComponent(typeof(RectTransform))]
	public class TabbedMenuController : MonoBehaviour
	{
		private RectTransform rt;
		private RectTransform Rt => rt != null ? rt
			: (rt = GetComponent<RectTransform>());
		[SerializeField] private RectTransform tabsHolder;
		[SerializeField] private RectTransform menuContentHolder;
		[SerializeField] private Component tabPrefab;
		[SerializeField] private List<Component> menuContentPrefabs;
		private List<IMenuTab> tabs = new List<IMenuTab>();
		private SortedList<int, IMenuContent> menuContents = new SortedList<int, IMenuContent>();
		[SerializeField] private int tabMinSortingOrder;
		private IMenuContent currentContent;
		[SerializeField] private float tabFadeDuration = 0.5f;
		[SerializeField] private List<GameObject> _objList;

		public virtual void Open()
		{
			ActivateObjects(true);

			ClearTabsHolder();
			AddPreExistingMenuContentsToList();
			InstantiateTabContentPrefabs();
			CreateTabs();
			UpdateTabDrawOrder();
			SubscribeToTabClickEvents();

			KeyValuePair<int, IMenuContent> firstContent = menuContents.FirstOrDefault();
			currentContent = firstContent.Value;
			ContentToShow(currentContent, 0f);

			IsOpen = true;
		}

		public virtual void Close()
		{
			ActivateObjects(false);
			IsOpen = false;
		}

		protected bool IsOpen { get; set; }

		private void ActivateObjects(bool activate)
		{
			foreach (GameObject go in _objList)
			{
				go.SetActive(activate);
			}
		}

		/// <summary>
		/// Removes objects under the tabs holder transform.
		/// </summary>
		private void ClearTabsHolder()
		{
			foreach (Transform child in tabsHolder)
			{
				Destroy(child.gameObject);
			}
		}

		/// <summary>
		/// Add any pre-existing menu contents to menu contents list, ignoring anything with a duplicate tab name.
		/// </summary>
		private void AddPreExistingMenuContentsToList()
		{
			menuContents.Clear();
			currentContent = null;

			foreach (Transform child in menuContentHolder)
			{
				IMenuContent menuContent = child.GetComponent<IMenuContent>();
				if (menuContent == null || ContainsTabWithName(menuContent.TabName))
				{
					Destroy(child.gameObject);
					continue;
				}
				menuContent.CGroup.alpha = 0f;
				menuContent.CGroup.gameObject.SetActive(false);
				menuContents.Add(menuContent.PreferredTabIndex, menuContent);
			}
		}

		/// <summary>
		/// Create all menu contents from prefab list, excluding anything with a duplicate tab name.
		/// </summary>
		private void InstantiateTabContentPrefabs()
		{
			for (int i = 0; i < menuContentPrefabs.Count; i++)
			{
				IMenuContent prefab = menuContentPrefabs[i] as IMenuContent;
				if (prefab == null)
				{
					Debug.Log($"Menu Content prefab is not of type: {typeof(IMenuContent)}", menuContentPrefabs[i]);
					continue;
				}
				if (ContainsTabWithName(prefab.TabName)) continue;
				IMenuContent newContent = prefab.CreateCopy(menuContentHolder);
				newContent.CGroup.alpha = 0f;
				newContent.CGroup.gameObject.SetActive(false);
				menuContents.Add(newContent.PreferredTabIndex, newContent);
			}
		}

		/// <summary>
		/// Create tabs for all menu contents.
		/// </summary>
		private void CreateTabs()
		{
			tabs.Clear();

			IMenuTab prefab = tabPrefab as IMenuTab;
			if (prefab == null)
			{
				Debug.Log($"Tab prefab is not of type: {typeof(IMenuTab)}", tabPrefab);
				return;
			}
			for (int i = 0; i < menuContents.Count; i++)
			{
				IMenuTab newTab = prefab.CreateCopy(tabsHolder);
				newTab.TabText = menuContents[i].TabName;
				newTab.SetIndex(i);
				tabs.Add(newTab);
			}
		}

		/// <summary>
		/// Set the draw order for all the tabs based on the currently selected tab
		/// </summary>
		private void UpdateTabDrawOrder()
		{
			int upperValue = tabMinSortingOrder + tabs.Count - 1;
			int startingIndex = menuContents.IndexOfValue(currentContent);

			for (int i = 0; i < tabs.Count; i++)
			{
				//calculate draw order
				int drawOrder = upperValue - Mathf.Abs(startingIndex - i);
				tabs[i].DrawOrder = drawOrder;
				//notifies the tab of the new main tab
				tabs[i].NotifyOfMainIndex(startingIndex);
			}
		}

		/// <summary>
		/// Subscribe to the relevant events on all tabs
		/// </summary>
		private void SubscribeToTabClickEvents()
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				tabs[i].OnClicked += TabClicked;
			}
		}

		private void TabClicked(IMenuTab tab)
		{
			TabToShow(tab, tabFadeDuration);
		}

		/// <summary>
		/// Updates tab draw order and views the appropriate menu content associated with the given tab.
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="fadeTime"></param>
		private void TabToShow(IMenuTab tab, float fadeTime)
		{
			IMenuContent content = GetContentMatchingTab(tab);
			TabAndContentToShow(tab, content, fadeTime);
		}

		/// <summary>
		/// Updates tab draw order and views the given menu content.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="fadeTime"></param>
		private void ContentToShow(IMenuContent content, float fadeTime)
		{
			IMenuTab tab = GetTabForContent(content);
			TabAndContentToShow(tab, content, fadeTime);
		}

		/// <summary>
		/// Updates tab draw order and views the given menu content.
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="content"></param>
		/// <param name="fadeTime"></param>
		private void TabAndContentToShow(IMenuTab tab, IMenuContent content, float fadeTime)
		{
			if (tab == null || content == null) return;

			CanvasGroup previousCGroup = currentContent?.CGroup;
			IMenuContent previousContent = currentContent;
			currentContent = content;
			UpdateTabDrawOrder();
			if (previousCGroup != null)
			{
				void finishAction()
				{
					previousCGroup.gameObject.SetActive(false);
					previousContent.OnClose();
				}
				StartCoroutine(TimedAction(fadeTime,
					(float delta) => previousCGroup.alpha = 1f - delta,
					finishAction));
			}
			CanvasGroup currentCGroup = currentContent.CGroup;
			currentCGroup.gameObject.SetActive(true);
			currentContent.OnOpen();
			StartCoroutine(TimedAction(fadeTime,
				(float delta) => currentCGroup.alpha = delta,
				null));
		}

		protected IEnumerator TimedAction(float duration, Action<float> action, Action finishAction)
		{
			if (duration <= 0f) action?.Invoke(1f);

			float timer = 0f;
			while (timer < duration)
			{
				timer += Time.unscaledDeltaTime;
				float delta = timer / duration;
				action?.Invoke(delta);
				yield return null;
			}

			action?.Invoke(1f);
			finishAction?.Invoke();
		}

		/// <summary>
		/// Checks list of menu contents for matching tab name.
		/// </summary>
		/// <param name="tabName"></param>
		/// <returns>Returns true if any tab names match the given name.</returns>
		private bool ContainsTabWithName(string tabName)
		{
			for (int i = 0; i < menuContents.Count; i++)
			{
				if (menuContents[i].TabName == tabName) return true;
			}
			return false;
		}

		/// <summary>
		/// Checks list of menu contents for matching tab.
		/// </summary>
		/// <param name="tab"></param>
		/// <returns>Returns matching menu content. Returns null if no match found.</returns>
		private IMenuContent GetContentMatchingTab(IMenuTab tab)
		{
			int index = GetIndexOfContentMatchingTab(tab);
			if (index < 0 || index >= menuContents.Count) return null;
			return menuContents[index];
		}

		/// <summary>
		/// Checks list of menu contents for matching tab.
		/// </summary>
		/// <param name="tab"></param>
		/// <returns>Returns index of matching menu content. Returns -1 if no match found.</returns>
		private int GetIndexOfContentMatchingTab(IMenuTab tab)
		{
			if (tab == null) return -1;

			for (int i = 0; i < menuContents.Count; i++)
			{
				if (menuContents[i].TabName == tab.TabText) return i;
			}
			return -1;
		}

		/// <summary>
		/// Checks list of tabs for index of the tab matching given content.
		/// </summary>
		/// <param name="menuContent"></param>
		/// <returns>Returns index for tab that matches the content. Returns -1 if match not found.</returns>
		private int GetIndexOfTabForContent(IMenuContent menuContent)
		{
			if (menuContent == null) return -1;

			string tabName = menuContent.TabName;
			for (int i = 0; i < tabs.Count; i++)
			{
				if (tabs[i].TabText == tabName) return i;
			}
			return -1;
		}

		/// <summary>
		/// Checks list of tabs for tab matching given content.
		/// </summary>
		/// <param name="menuContent"></param>
		/// <returns>Returns matching tab. Returns null if no tab found.</returns>
		private IMenuTab GetTabForContent(IMenuContent menuContent)
		{
			int index = GetIndexOfTabForContent(menuContent);
			IMenuTab tab = GetTabAtIndex(index);
			return tab;
		}

		/// <summary>
		/// Checks list of tabs looking for a match with given content.
		/// </summary>
		/// <param name="menuContent"></param>
		/// <returns>Returns true if matching tab is found.</returns>
		private bool TabExistsForContent(IMenuContent menuContent)
		{
			IMenuTab tab = GetTabForContent(menuContent);
			return tab != null;
		}

		/// <summary>
		/// Gets the tab at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns>Returns tab at given index. Returns null if index is invalid.</returns>
		private IMenuTab GetTabAtIndex(int index)
		{
			if (index < 0 || index > tabs.Count) return null;
			return tabs[index];
		}
	}
}