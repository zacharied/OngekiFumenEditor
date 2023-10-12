﻿using IntervalTree;
using OngekiFumenEditor.Base.Collections.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OngekiFumenEditor.Base.Collections
{
    public partial class SoflanList : IEnumerable<Soflan>
    {
        private IntervalTreeWrapper<TGrid, Soflan> soflans = new(
            x => new() { Min = x.TGrid, Max = x.EndIndicator.TGrid },
            true,
            nameof(Soflan.TGrid),
            nameof(Soflan.EndTGrid)
            );

        public event Action OnChangedEvent;

        public IEnumerator<Soflan> GetEnumerator() => soflans.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<Soflan> GetVisibleStartObjects(TGrid min, TGrid max)
        {
            return soflans.QueryInRange(min, max);
        }

        public SoflanList(IEnumerable<Soflan> initBpmChanges = default)
        {
            OnChangedEvent += OnChilidrenSubPropsChangedEvent;
            foreach (var item in initBpmChanges ?? Enumerable.Empty<Soflan>())
                Add(item);
        }

        private void OnChilidrenSubPropsChangedEvent()
        {
            cachedSoflanListCacheHash = int.MinValue;
        }

        public void Add(Soflan soflan)
        {
            soflans.Add(soflan);
            soflan.PropertyChanged += OnSoflanPropChanged;
            OnChangedEvent?.Invoke();
        }

        private void OnSoflanPropChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Soflan.Speed):
                case nameof(Soflan.TGrid):
                case nameof(TGrid.Grid):
                case nameof(TGrid.Unit):
                case nameof(Soflan.ApplySpeedInDesignMode):
                case nameof(Soflan.EndTGrid):
                case nameof(Soflan.GridLength):
                    OnChangedEvent?.Invoke();
                    break;
                default:
                    break;
            }
        }

        public void Remove(Soflan soflan)
        {
            soflans.Remove(soflan);
            soflan.PropertyChanged -= OnSoflanPropChanged;
            OnChangedEvent?.Invoke();
        }
    }
}
