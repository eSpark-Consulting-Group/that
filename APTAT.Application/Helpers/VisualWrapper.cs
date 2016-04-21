using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace APTAT.Application.Helpers
{
    [ContentProperty("Child")]
    public class VisualWrapper : FrameworkElement
    {
        private Visual _child;

        public Visual Child
        {
            get { return _child; }
            set
            {
                if (_child != null)
                {
                    RemoveVisualChild(_child);
                }

                _child = value;
                if (_child != null)
                {
                    AddVisualChild(_child);
                }
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (_child != null && index == 0)
            {
                return _child;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        protected override int VisualChildrenCount
        {
            get { return _child != null ? 1 : 0; }
        }
    }

    public class VisualTargetPresentationSource : PresentationSource
    {
        private readonly VisualTarget _visualTarget;

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            _visualTarget = new VisualTarget(hostVisual);
        }

        public override Visual RootVisual
        {
            get
            {
                lock (_visualTarget)
                {
                    return _visualTarget.RootVisual;
                }
            }
            set
            {
                lock (_visualTarget)
                {
                    Visual oldRoot = _visualTarget.RootVisual;

                    _visualTarget.RootVisual = value;
                    RootChanged(oldRoot, value);

                    var rootElement = value as UIElement;

                    if (rootElement != null)
                    {
                        rootElement.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                        rootElement.Arrange(new Rect(rootElement.DesiredSize));
                    }
                }
            }
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return _visualTarget;
        }

        public override bool IsDisposed
        {
            get { return false; }
        }
    }
}
