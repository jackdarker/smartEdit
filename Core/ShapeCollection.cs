using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartEdit.Core {
    #region Enumerator templates
    public class ElementEnumerator<T> : IEnumerator<T> {
        public ElementEnumerator(List<T> Collection)
            : base() {
            m_InternalEnumerator = Collection.GetEnumerator();
        }
        object System.Collections.IEnumerator.Current {
            get { return Current; }
        }
        public virtual T Current {
            get {
                try {
                    return m_InternalEnumerator.Current;
                } catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Reset() {
            m_InternalEnumerator.Reset();
        }
        public virtual bool MoveNext() {
            return m_InternalEnumerator.MoveNext();
        }
        public virtual void Dispose() {
            m_InternalEnumerator.Dispose();
        }
        protected IEnumerator<T> m_InternalEnumerator;
    }
    /// <summary>
    /// An enumerator that can use FilterSettings
    /// </summary>
    public class ElementEnumeratorWithFilter<T> : ElementEnumerator<T>, IEnumerator<T> {
        public ElementEnumeratorWithFilter(List<T> Collection)
            : base(Collection) {
            m_InternalEnumerator = Collection.GetEnumerator();
            m_FilterStrategy = new ElementEnumeratorFilterStrategyBase<T>();
        }
        public ElementEnumeratorWithFilter(List<T> Shapes, ElementEnumeratorFilterStrategyBase<T> Filter)
            : base(Shapes) {
            m_InternalEnumerator = Shapes.GetEnumerator();
            m_FilterStrategy = Filter;
        }
        public override bool MoveNext() {
            bool _Found = false;
            while (!_Found) {
                if (!m_InternalEnumerator.MoveNext()) break;
                if (m_FilterStrategy.MatchsFilter(m_InternalEnumerator.Current)) _Found = true;
            }
            return _Found;
        }
        public override void Dispose() {
            base.Dispose();
        }
        private ElementEnumeratorFilterStrategyBase<T> m_FilterStrategy;
    }
    public class ElementEnumeratorFilterStrategyBase<T> {
        public ElementEnumeratorFilterStrategyBase() { }
        public virtual bool MatchsFilter(T Element) { return true; }
    }
    public class ElementEnumeratorFilterExclude<T> : ElementEnumeratorFilterStrategyBase<T> {
        public ElementEnumeratorFilterExclude(T Criteria) { m_Criteria = Criteria; }
        public override bool MatchsFilter(T Element) { return !Element.Equals(m_Criteria); }
        T m_Criteria;
    }
    /// <summary>
    /// Filters elements that matches at least one condition
    /// </summary>
    public class ElementEnumeratorFilterMultipleOr<T> : ElementEnumeratorFilterStrategyBase<T> {
        public ElementEnumeratorFilterMultipleOr(List<ElementEnumeratorFilterStrategyBase<T>> Filter)
            : base() {
            m_Filter = new List<ElementEnumeratorFilterStrategyBase<T>>(Filter); //make a copy!
        }
        public override bool MatchsFilter(T Element) {
            bool Match = false;
            IEnumerator<ElementEnumeratorFilterStrategyBase<T>> _Iter = m_Filter.GetEnumerator();
            while (!Match && _Iter.MoveNext()) {
                Match = _Iter.Current.MatchsFilter(Element);
            }
            return Match;
        }
        List<ElementEnumeratorFilterStrategyBase<T>> m_Filter;
    }
    /// <summary>
    /// Filters elements that matches all conditions 
    /// </summary>
    public class ElementEnumeratorFilterMultipleAnd<T> : ElementEnumeratorFilterStrategyBase<T> {
        public ElementEnumeratorFilterMultipleAnd(List<ElementEnumeratorFilterStrategyBase<T>> Filter)
            : base() {
            m_Filter = new List<ElementEnumeratorFilterStrategyBase<T>>(Filter); //make a copy!
        }
        public override bool MatchsFilter(T Element) {
            bool Match = true;
            int i = 0;
            IEnumerator<ElementEnumeratorFilterStrategyBase<T>> _Iter = m_Filter.GetEnumerator();
            while (_Iter.MoveNext()) {
                Match = _Iter.Current.MatchsFilter(Element) && Match;
                i++;
            }
            return (Match && i != 0);
        }
        List<ElementEnumeratorFilterStrategyBase<T>> m_Filter;
    }
    #endregion
    public class ShapeEnumeratorFilterBySelection : ElementEnumeratorFilterStrategyBase<ShapeInterface> {
        public ShapeEnumeratorFilterBySelection() : base() { }
        public override bool MatchsFilter(ShapeInterface Shape) { return Shape.IsSelected(); }
    }
    public class ShapeEnumeratorFilterByLocation : ElementEnumeratorFilterStrategyBase<ShapeInterface> {
        public ShapeEnumeratorFilterByLocation(Point Position)
            : base() {
            m_Position = Position;
            m_FilterByPosition = true;
        }
        public ShapeEnumeratorFilterByLocation(Rectangle Area)
            : base() {
            m_Rectangle = Area;
            m_FilterByRectangle = true;
        }
        public override bool MatchsFilter(ShapeInterface Shape) {
            bool Match = false;
            if (m_FilterByPosition)
                Match = Shape.Intersects(m_Position);
            else if (m_FilterByRectangle)
                Match = m_Rectangle.Contains(Shape.GetBoundingBox());
            return Match;
        }
        private bool m_FilterByRectangle = false;
        private Rectangle m_Rectangle;
        private bool m_FilterByPosition = false;
        private Point m_Position;
    }

    public class HandleEnumeratorFilterBySelection : ElementEnumeratorFilterStrategyBase<ShapeActionHandleBase> {
        public HandleEnumeratorFilterBySelection() : base() { }
        public override bool MatchsFilter(ShapeActionHandleBase Element) { return Element.IsSelected(); }
    }

    public class HandleEnumerator : IEnumerator<ShapeActionHandleBase> {
        public HandleEnumerator(ElementEnumerator<ShapeInterface> Collection)
            : base() {
            m_InternalShapeEnumerator = Collection;
        }
        object System.Collections.IEnumerator.Current {
            get { return Current; }
        }
        public virtual ShapeActionHandleBase Current {
            get {
                try {
                    return m_InternalHandleEnumerator.Current;
                } catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Reset() {
            m_InternalShapeEnumerator.Reset();
            m_InternalHandleEnumerator.Reset();
        }
        public virtual bool MoveNext() {
            if (m_InternalHandleEnumerator != null) {
                if (m_InternalHandleEnumerator.MoveNext()) {
                    return true;
                }
            }
            m_InternalHandleEnumerator = null;
            while (m_InternalShapeEnumerator.MoveNext()) {

                m_InternalHandleEnumerator = m_InternalShapeEnumerator.Current.GetHandles();// m_InternalShapeEnumerator.Current.GetSelectedHandles();
                if (m_InternalHandleEnumerator.MoveNext()) {
                    return true;
                } else {
                    m_InternalHandleEnumerator = null;
                }
            }

            if (m_InternalHandleEnumerator != null) {
                return true;
            } else {
                return false;
            }
        }
        public virtual void Dispose() {
            m_InternalHandleEnumerator.Dispose();
            m_InternalShapeEnumerator.Dispose();
        }
        protected IEnumerator<ShapeInterface> m_InternalShapeEnumerator = null;
        protected IEnumerator<ShapeActionHandleBase> m_InternalHandleEnumerator = null;

    }
}
