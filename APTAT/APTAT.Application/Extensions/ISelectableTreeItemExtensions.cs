using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Application.Models.ModelBase;

namespace APTAT.Application.Extensions
{
    public static class SelectableTreeItemExtensions
    {
        public static ISelectableTreeItem<T>[] GetSelectedItemsWithContainerPriority<T>(this ISelectableTreeItem<T>[] inItems) where T:class, ISelectableTreeItem<T>
        {
            var items = new List<ISelectableTreeItem<T>>();
            var stack = new Stack<ISelectableTreeItem<T>>();
            foreach (var item in inItems)
            {
                var node = item;
                while (true)
                {
                    if (node.IsSelected.GetValueOrDefault())
                    {
                        items.Add(node);
                        break;
                    }
                    if (node.Items == null)
                        break;
                    foreach (var nodeChild in node.Items)
                    {
                        if (nodeChild.IsSelected.GetValueOrDefault())
                            items.Add(nodeChild);
                        else
                            stack.Push(nodeChild);
                    }
                    if (stack.Any())
                    {
                        node = stack.Pop();
                    }
                    else
                        break;
                }
            }
            return items.ToArray();
        }

        public static ISelectableTreeItem<T>[] GetSelectedItemsWithItemsPriority<T>(this IEnumerable<ISelectableTreeItem<T>> inItems) where T : class, ISelectableTreeItem<T>
        {
            var items = new List<ISelectableTreeItem<T>>();
            var stack = new Stack<ISelectableTreeItem<T>>();
            foreach (var item in inItems)
            {
                var node = item;
                while (true)
                {
                    if (node.IsSelected.GetValueOrDefault() && !(node.Items??Enumerable.Empty<T>()).Any())
                    {
                        items.Add(node);
                    }
                    foreach (var nodeChild in node.Items??Enumerable.Empty<T>())
                    {
                        stack.Push(nodeChild);
                    }
                    if (stack.Any())
                    {
                        node = stack.Pop();
                    }
                    else
                        break;
                }
            }
            return items.ToArray();
        }
    }
}
