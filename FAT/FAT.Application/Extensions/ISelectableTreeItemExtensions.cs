using System;
using System.Collections.Generic;
using System.Linq;

using APTAT.Application.Models.ModelBase;

namespace APTAT.Application.Extensions
{
    public static class SelectableTreeItemExtensions
    {


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
