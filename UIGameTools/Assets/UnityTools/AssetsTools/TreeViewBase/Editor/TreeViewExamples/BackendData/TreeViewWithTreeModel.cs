using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace UnityEditor.TreeViewExamples
{
	/// <summary>
	/// 自定义的TreeView，包装一下保存自己的数据。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TreeViewItem<T> : TreeViewItem where T : TreeElement
	{
		public T data { get; set; }

		public TreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
		{
			this.data = data;
		}
	}

	/// <summary>
	/// 自定义的TreeView的基类。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TreeViewWithTreeModel<T> : TreeView where T : TreeElement
	{
		/// <summary>
		/// 树结构的数据，存放自定义的数据。
		/// </summary>
		TreeModel<T> m_TreeModel;
		public TreeModel<T> treeModel { get { return m_TreeModel; } }
		/// <summary>
		/// 树的数据发生变化传出的事件。
		/// </summary>
		/// 
		public event Action treeChanged;
		public event Action<IList<TreeViewItem>>  beforeDroppingDraggedItems;
		readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);


		public TreeViewWithTreeModel (TreeViewState state, TreeModel<T> model) : base (state)
		{
			k_GenericDragID = "TreeView" + this.GetType().Name;
			Init (model);
		}

		public TreeViewWithTreeModel (TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
			: base(state, multiColumnHeader)
		{
			k_GenericDragID = "TreeView" + this.GetType().Name;
			Init(model);
		}

		void Init (TreeModel<T> model)
		{
			m_TreeModel = model;
			m_TreeModel.modelChanged += ModelChanged;
		}

		void ModelChanged ()
		{
			if (treeChanged != null)
				treeChanged ();
			//Reload，TreeView构建后必须要调用一次，其次数据结构发生变化后，也调用一次。
			Reload ();
		}

		//创建Root节点。
		protected override TreeViewItem BuildRoot()
		{
			int depthForHiddenRoot = -1;
			var t = new TreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
			return t;
		}

		//给Root节点填充数据
		protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
		{
			if (m_TreeModel.root == null)
			{
				Debug.LogError ("tree model root is null. did you call SetData()?");
			}

			m_Rows.Clear ();
			if (!string.IsNullOrEmpty(searchString))
			{
				Search (m_TreeModel.root, searchString, m_Rows);
			}
			else
			{
				if (m_TreeModel.root.hasChildren)
					AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
			}

			// We still need to setup the child parent information for the rows since this 
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths (root, m_Rows);

			return m_Rows;
		}

		void AddChildrenRecursive (T parent, int depth, IList<TreeViewItem> newRows)
		{
			foreach (T child in parent.children)
			{
				var item = new TreeViewItem<T>(child.id, depth, child.name, child);
				newRows.Add(item);

				if (child.hasChildren)
				{
					//判断是否展开子节点。
					//TreeViewState会记录TreeView的状态，包括哪个id展开了。
					//这里child.id和treeviewitem的id一致，所以可以这样判断。
					if (IsExpanded(child.id))
					{
						AddChildrenRecursive (child, depth + 1, newRows);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		void Search(T searchFromThis, string search, List<TreeViewItem> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", "search");

			const int kItemDepth = 0; // tree is flattened when searching

			Stack<T> stack = new Stack<T>();
			foreach (var element in searchFromThis.children)
				stack.Push((T)element);
			while (stack.Count > 0)
			{
				T current = stack.Pop();
				// Matches search?
				if (current.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.Add(new TreeViewItem<T>(current.id, kItemDepth, current.name, current));
				}

				if (current.children != null && current.children.Count > 0)
				{
					foreach (var element in current.children)
					{
						stack.Push((T)element);
					}
				}
			}
			SortSearchResult(result);
		}

		protected virtual void SortSearchResult (List<TreeViewItem> rows)
		{
			rows.Sort ((x,y) => EditorUtility.NaturalCompare (x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}
	
		/// <summary>
		/// 获取某个id的所有祖先。
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected override IList<int> GetAncestors (int id)
		{
			return m_TreeModel.GetAncestors(id);
		}

		/// <summary>
		/// 获取某个id的所有子节点
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected override IList<int> GetDescendantsThatHaveChildren (int id)
		{
			return m_TreeModel.GetDescendantsThatHaveChildren(id);
		}


		// Dragging
		//-----------
		// 应该是设置成唯一的Id
		readonly string k_GenericDragID = "GenericDragColumnDragging";

		/// <summary>
		/// 是否允许拖拽
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		protected override bool CanStartDrag (CanStartDragArgs args)
		{
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
		{
			if (hasSearch)
				return;
			//DragAndDrop的一系列操作类型GUI，设置的参数是内部短暂保存的。
			DragAndDrop.PrepareStartDrag();
			var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
			//往DragAndDrop内部设置数据
			DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
			DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
			string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
			//设置标题。
			DragAndDrop.StartDrag (title);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args)
		{
			// Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
			// 判断这次Hanlde的回调是不是来自于自身。
			var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
			if (draggedRows == null)
				return DragAndDropVisualMode.None;

			// Parent item is null when dragging outside any tree view items.
			switch (args.dragAndDropPosition)
			{
				case DragAndDropPosition.UponItem:
				case DragAndDropPosition.BetweenItems:
					{
						bool validDrag = ValidDrag(args.parentItem, draggedRows);

						if (args.performDrop && validDrag)
						{
							T parentData = ((TreeViewItem<T>)args.parentItem).data;
							OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
						}
						return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
					}

				case DragAndDropPosition.OutsideItems:
					{
						//此时 args.parentItem == null , 所以将所有的item移动到root节点下。
						if (args.performDrop)
							OnDropDraggedElementsAtIndex(draggedRows, m_TreeModel.root, m_TreeModel.root.children.Count);

						return DragAndDropVisualMode.Move;
					}
				default:
					Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
					return DragAndDropVisualMode.None;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="draggedRows"></param>
		/// <param name="parent"></param>
		/// <param name="insertIndex"></param>
		public virtual void OnDropDraggedElementsAtIndex (List<TreeViewItem> draggedRows, T parent, int insertIndex)
		{
			if (beforeDroppingDraggedItems != null)
				beforeDroppingDraggedItems (draggedRows);

			var draggedElements = new List<TreeElement> ();
			foreach (var x in draggedRows)
				draggedElements.Add (((TreeViewItem<T>) x).data);
		
			var selectedIDs = draggedElements.Select (x => x.id).ToArray();
			// MoveElements 内会调用onchange事件，然后回头执行这个类的ModelChanged()刷新数据。
			m_TreeModel.MoveElements (parent, insertIndex, draggedElements);
			// 设置选中状态。
			SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
		}


		bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
		{
			TreeViewItem currentParent = parent;
			while (currentParent != null)
			{
				//不能移动到被移动的物体下。
				if (draggedItems.Contains(currentParent))
					return false;
				currentParent = currentParent.parent;
			}
			return true;
		}
	
	}

}
