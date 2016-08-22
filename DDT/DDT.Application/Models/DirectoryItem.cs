using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DDT.Application.Models
{
    public enum DirectoryItemType
    {
        Root,
        OrgUnit,
        Computer
    }

    public class DirectoryItem : DeploymentItem
    {
        public DirectoryItemType Type { get; set; }
        public bool IsWorkgroup { get; set; }
        public DirectoryItem Parent { get; set; }

        private bool _ignoreSelectChilds;
        private bool _ignoreSelectParents;
        private bool _ignoreSelectionChangedAction;

        private void SelectChilds(bool? value)
        {
            if (Items != null)
                Parallel.ForEach(Items, item =>
                {
                    if (item != null)
                    {
                        item._ignoreSelectionChangedAction = true;
                        item._ignoreSelectParents = true;
                        item.IsSelected = value;
                        item._ignoreSelectionChangedAction = false;
                        item._ignoreSelectParents = false;
                    }
                });
        }

        private void SelectParents(bool? value)
        {
            if (Parent != null && Parent.IsSelected != value)
            {
                Parent._ignoreSelectionChangedAction = true;
                Parent._ignoreSelectChilds = true;
                Parent.IsSelected = Parent.Items.Any(_ => _ != null && _.IsSelected != value) ? null : value;
                Parent._ignoreSelectChilds = false;
                Parent._ignoreSelectionChangedAction = false;
            }
        }

        public List<DeploymentItem> GetSelectedItems()
        {
            var items = new List<DeploymentItem>();
            if (IsSelected.GetValueOrDefault(true))
            {
                if (IsSelected.GetValueOrDefault(false))
                {
                    if (Items == null || !Items.Any() || Items[0] == null)
                        items.Add(this);
                }

                if (Items != null)
                    foreach(var item in Items)
                        if (item != null)
                            items.AddRange(item.GetSelectedItems());
            }

            return items;
        }

        private Collection<Action> _mementoActions;

        public void SetMementoActions(Collection<Action> actions)
        {
            _mementoActions = actions;

            if (Items != null)
                Parallel.ForEach(Items, item =>
                {
                    if (item != null)
                        item.SetMementoActions(actions);
                });
        }

        public override double Height
        {
            get
            {
                switch (Type)
                {
                    case DirectoryItemType.Computer:
                        return base.Height;
                    default:
                        return 45;
                }
            }
        }

        #region Memento Properties

        private bool _itemsMementoSaved;
        private DirectoryItem[] _itemsMementoValue;
        private DirectoryItem[] _items;
        public DirectoryItem[] Items
        {
            get { return _items; }
            set
            {
                if (_mementoActions != null && !_itemsMementoSaved)
                {
                    _itemsMementoSaved = true;
                    _itemsMementoValue = _items;
                    _mementoActions.Add(() =>
                    {
                        _items = _itemsMementoValue;
                        _itemsMementoSaved = false;
                        _itemsMementoValue = null;
                    });
                }

                _items = value;
                RaisePropertyChanged(nameof(Items));
            }
        }


        private bool _isSelectedMementoSaved;
        private bool? _isSelectedMementoValue;
        public override bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value && _mementoActions != null && !_isSelectedMementoSaved)
                {
                    _isSelectedMementoSaved = true;
                    _isSelectedMementoValue = _isSelected;
                    _mementoActions.Add(() =>
                    {
                        _isSelected = _isSelectedMementoValue;
                        _isSelectedMementoSaved = false;
                    });
                }

                _isSelected = value;

                if (!_ignoreSelectChilds)
                    SelectChilds(value);
                if (!_ignoreSelectParents)
                    SelectParents(value);

                RaisePropertyChanged(nameof(IsSelected));
                if (!_ignoreSelectionChangedAction)
                    SelectionChangedAction?.Invoke();
            }
        }


        #endregion
    }
}
