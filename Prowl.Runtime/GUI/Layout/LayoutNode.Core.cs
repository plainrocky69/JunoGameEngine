﻿using System;
using System.Collections.Generic;
using System.Linq;
using static BepuPhysics.Collidables.CompoundBuilder;

namespace Prowl.Runtime.GUI.Layout
{
    public partial class LayoutNode
    {
        public struct PostLayoutData(LayoutNode node)
        {
            internal LayoutNode _node = node;

            public readonly Rect InnerRect => new(GlobalContentPosition, new(GlobalContentWidth, GlobalContentHeight));
            public readonly Rect Rect => new(GlobalPosition, new(Scale.x, Scale.y));
            public readonly Rect OuterRect => new(GlobalPosition - Margins.TopLeft, new(Scale.x + Margins.Horizontal, Scale.y + Margins.Vertical));

            public Vector2 GlobalPosition {
                get {
                    if(_node == null)
                        return Vector2.zero;
                    Vector2 globalPosition = _node.Parent != null ? _node.Parent.LayoutData.GlobalContentPosition + Position + Margins.TopLeft : Position;
                    if (_node.Parent != null && !_node._ignore)
                        globalPosition -= new Vector2(_node.Parent.HScroll, _node.Parent.VScroll);
                    return globalPosition;
                }
            }

            public readonly Vector2 GlobalContentPosition => GlobalPosition + Paddings.TopLeft;
            public readonly double GlobalContentWidth => Scale.x - Paddings.Horizontal;
            public readonly double GlobalContentHeight => Scale.y - Paddings.Vertical;

            // Cached
            public Vector2 Scale;
            public Vector2 MaxScale;
            public Spacing Margins;
            public Spacing Paddings;
            public Vector2 Position;
            public Rect ContentRect;
        }

        public bool HasLayoutData => _data._node == this;
        public PostLayoutData LayoutData => _data;

        public Gui Gui { get; private set; }
        public LayoutNode Parent { get; internal set; }
        public double VScroll { get; set; } = 0;
        public double HScroll { get; set; } = 0;
        public ulong ID { get; private set; } = 0;

        private PostLayoutData _data;
        private Offset _positionX = Offset.Default;
        private Offset _positionY = Offset.Default;
        private Size _width = Size.Default;
        private Size _height = Size.Default;
        private Size _maxWidth = Size.Max;
        private Size _maxHeight = Size.Max;
        private Offset _marginLeft = Offset.Default;
        private Offset _marginRight = Offset.Default;
        private Offset _marginTop = Offset.Default;
        private Offset _marginBottom = Offset.Default;
        private Offset _paddingLeft = Offset.Default;
        private Offset _paddingRight = Offset.Default;
        private Offset _paddingTop = Offset.Default;
        private Offset _paddingBottom = Offset.Default;
        private bool _ignore = false;
        private bool _fitContentX = false;
        private bool _fitContentY = false;
        private bool _centerContent = false;
        private bool _canScaleChildren = false;
        private LayoutNode _positionRelativeTo;
        private LayoutNode _sizeRelativeTo;

        private LayoutType _layout = LayoutType.None;
        internal ClipType _clipped = ClipType.None;


        internal int ZIndex = 0;

        internal List<LayoutNode> Children = new List<LayoutNode>();

        public LayoutNode(LayoutNode? parent, Gui gui, ulong storageHash)
        {
            ID = storageHash;
            Gui = gui;
            if (parent != null)
            {
                ZIndex = parent.ZIndex;
            }
        }

        }

        public void UpdateCache()
        {
            _data = new(this);

            // Cache scale first
            UpdateScaleCache();

            // Then Margin/Paddings (They rely on Scale)
            _data.Margins = new(
                    _marginLeft.ToPixels(_positionRelativeTo?._data.Scale.x ?? 0),
                    _marginRight.ToPixels(_positionRelativeTo?._data.Scale.x ?? 0),
                    _marginTop.ToPixels(_positionRelativeTo?._data.Scale.y ?? 0),
                    _marginBottom.ToPixels(_positionRelativeTo?._data.Scale.y ?? 0)
                );
            _data.Paddings = new(
                    _paddingLeft.ToPixels(_positionRelativeTo?._data.Scale.x ?? 0),
                    _paddingRight.ToPixels(_positionRelativeTo?._data.Scale.x ?? 0),
                    _paddingTop.ToPixels(_positionRelativeTo?._data.Scale.y ?? 0),
                    _paddingBottom.ToPixels(_positionRelativeTo?._data.Scale.y ?? 0)
                );

            // Then finally position (Relies on Scale and Padding)
            UpdatePositionCache();

            foreach (var child in Children)
                child.UpdateCache();
        }

        public void UpdateScaleCache()
        {
            _data.Scale = new(
                Math.Min(_width.ToPixels(_sizeRelativeTo?._data.GlobalContentWidth ?? 0),
                         _maxWidth.ToPixels(_sizeRelativeTo?._data.GlobalContentWidth ?? 0)
                ),
                Math.Min(_height.ToPixels(_sizeRelativeTo?._data.GlobalContentHeight ?? 0),
                         _maxHeight.ToPixels(_sizeRelativeTo?._data.GlobalContentHeight ?? 0)
                )
            );

            _data.MaxScale = new(
                _maxWidth.ToPixels(_sizeRelativeTo?._data.GlobalContentWidth ?? 0),
                _maxHeight.ToPixels(_sizeRelativeTo?._data.GlobalContentHeight ?? 0)
            );
        }

        public void UpdatePositionCache()
        {
            _data.Position = new(
                    _positionX.ToPixels(_positionRelativeTo?._data.GlobalContentWidth ?? 0),
                    _positionY.ToPixels(_positionRelativeTo?._data.GlobalContentHeight ?? 0)
                );
        }

        public void ProcessLayout()
        {

            ScaleChildren();
            foreach (var child in Children)
                child.ProcessLayout();
            ScaleChildren();
            UpdatePositionCache();

            ApplyLayout();

            // Make sure we never exceed max size
            // TODO: Is this needed since we calculate the max in Scale
            //_width = Math.Min(Scale.x, _maxWidth.ToPixels(Parent?.GlobalContentWidth ?? 0));
            //_height = Math.Min(Scale.y, _maxHeight.ToPixels(Parent?.GlobalContentWidth ?? 0));
            //UpdateScaleCache();

            if (_centerContent)
            {
                Vector2 childrenCenter = new Vector2();
                if (_layout == LayoutType.Grid)
                {
                    int childCount = 0;
                    foreach (var childB in Children)
                    {
                        if (childB._ignore) continue;
                        childrenCenter += childB._data.Rect.Center;
                        childCount++;
                    }
                    childrenCenter /= new Vector2(childCount, childCount);
                }

                foreach (var child in Children)
                {
                    if (child._ignore) continue;

                    switch (_layout)
                    {
                        case LayoutType.Column:
                            child._positionX = (_data.GlobalContentWidth - child._data.Scale.x - child._data.Margins.Horizontal) / 2;
                            break;
                        case LayoutType.Row:
                            child._positionY = (_data.GlobalContentHeight - child._data.Scale.y - child._data.Margins.Vertical) / 2;
                            break;
                        case LayoutType.Grid:
                            // TODO: This isnt working correctly, All the layout types should use just this, but it needs to work properly first
                            Vector2 ourCenter = new Vector2(_data.GlobalContentWidth / 2, _data.GlobalContentHeight / 2);
                            Vector2 offset = ourCenter - childrenCenter;
                            child._positionX = child._data.Position.x + offset.x;
                            child._positionY = child._data.Position.y + offset.y;
                            break;
                        default:
                            child._positionX = (_data.GlobalContentWidth - child._data.Scale.x - child._data.Margins.Horizontal) / 2;
                            child._positionY = (_data.GlobalContentHeight - child._data.Scale.y - child._data.Margins.Vertical) / 2;
                            break;
                    }
                    child.UpdatePositionCache();
                }
            }

            if (Children.Count > 0)
            {
                _data.ContentRect = Children[0]._data.OuterRect;
                for (int i = 0; i < Children.Count; i++)
                        _data.ContentRect = Rect.CombineRect(_data.ContentRect, Children[i]._data.OuterRect);
            } else _data.ContentRect = new Rect();


            ApplyFitContent();
        }

        internal void ScaleChildren()
        {
            if (!_canScaleChildren) return;

            var scalableChildren = Children.Where(c => !c._ignore).ToList();
            double totalAvailableWidth = _data.GlobalContentWidth;
            double totalAvailableHeight = _data.GlobalContentHeight;

            if (_layout == LayoutType.Row)
            {
                double remainingWidth = totalAvailableWidth;
                double remainingChildren = scalableChildren.Count;

                foreach (var child in scalableChildren)
                {
                    double width = remainingWidth / remainingChildren;
                    width = Math.Min(width, child._data.MaxScale.x);
                    child._width = Math.Max(width - child._data.Margins.Horizontal, 0);
                    remainingWidth -= width;
                    remainingChildren--;
                    child.UpdateScaleCache();
                }
            }

            if (_layout == LayoutType.Column)
            {
                double remainingHeight = totalAvailableHeight;
                double remainingChildren = scalableChildren.Count;

                foreach (var child in scalableChildren)
                {
                    double height = remainingHeight / remainingChildren;
                    height = Math.Min(height, child._data.MaxScale.y);
                    child._height = Math.Max(height - child._data.Margins.Vertical, 0);
                    remainingHeight -= height;
                    remainingChildren--;
                    child.UpdateScaleCache();
                }
            }
        }

        internal void ApplyLayout()
        {
            double x = 0, y = 0;
            switch (_layout)
            {
                case LayoutType.Column:
                    foreach (var child in Children)
                    {
                        if (child._ignore) continue;
                        child._positionX = 0;
                        child._positionY = y;
                        y += child._data.Margins.Vertical + child._data.Scale.y;
                        child.UpdatePositionCache();
                    }
                    break;
                case LayoutType.Row:
                    foreach (var child in Children)
                    {
                        if (child._ignore) continue;
                        child._positionX = x;
                        child._positionY = 0;
                        x += child._data.Margins.Horizontal + child._data.Scale.x;
                        child.UpdatePositionCache();
                    }
                    break;
                case LayoutType.Grid:
                    double maxY = 0;
                    foreach (var child in Children)
                    {
                        if (child._ignore) continue;
                        if (x + child._data.Scale.x + child._data.Margins.Horizontal > _data.GlobalContentWidth)
                        {
                            y += maxY;
                            x = 0;
                            maxY = 0;
                        }

                        child._positionX = x;
                        child._positionY = y;
                        x += child._data.Margins.Horizontal + child._data.Scale.x;

                        if (child._data.Margins.Vertical + child._data.Scale.y > maxY)
                            maxY = child._data.Margins.Vertical + child._data.Scale.y;
                        child.UpdatePositionCache();
                    }
                    break;
                default:
                    break;
            }
        }

        public void ApplyFitContent()
        {
            if (!_fitContentX && !_fitContentY)
                return;

            if (_fitContentX)
                _width = _data.ContentRect.width;
            if (_fitContentY)           
                _height = _data.ContentRect.height;
            UpdateScaleCache();
        }

        public ulong GetHashCode64()
        {
            ulong hash = 17;
            hash = hash * 23 + (ulong)ID.GetHashCode();
            hash = hash * 23 + (ulong)_positionX.GetHashCode64();
            hash = hash * 23 + (ulong)_positionY.GetHashCode64();
            hash = hash * 23 + (ulong)_width.GetHashCode64();
            hash = hash * 23 + (ulong)_height.GetHashCode64();
            hash = hash * 23 + (ulong)_maxWidth.GetHashCode64();
            hash = hash * 23 + (ulong)_maxHeight.GetHashCode64();
            hash = hash * 23 + (ulong)_marginLeft.GetHashCode64();
            hash = hash * 23 + (ulong)_marginRight.GetHashCode64();
            hash = hash * 23 + (ulong)_marginTop.GetHashCode64();
            hash = hash * 23 + (ulong)_marginBottom.GetHashCode64();
            hash = hash * 23 + (ulong)_paddingLeft.GetHashCode64();
            hash = hash * 23 + (ulong)_paddingRight.GetHashCode64();
            hash = hash * 23 + (ulong)_paddingTop.GetHashCode64();
            hash = hash * 23 + (ulong)_paddingBottom.GetHashCode64();
            hash = hash * 23 + (ulong)_ignore.GetHashCode();
            hash = hash * 23 + (ulong)_fitContentX.GetHashCode();
            hash = hash * 23 + (ulong)_fitContentY.GetHashCode();
            hash = hash * 23 + (ulong)_centerContent.GetHashCode();
            hash = hash * 23 + (ulong)_canScaleChildren.GetHashCode();
            hash = hash * 23 + (ulong)_layout.GetHashCode();
            hash = hash * 23 + (ulong)_clipped.GetHashCode();
            hash = hash * 23 + (ulong)VScroll.GetHashCode();
            hash = hash * 23 + (ulong)HScroll.GetHashCode();
            //hash = hash * 23 + _nextNodeIndexA.GetHashCode();
            //hash = hash * 23 + _nextNodeIndexB.GetHashCode();
            hash = hash * 23 + (ulong)Children.Count.GetHashCode();
            return hash;
        }
    }
}
