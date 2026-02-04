using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.Lane.Base;
using OngekiFumenEditor.Base.OngekiObjects.Projectiles;

namespace OngekiFumenEditor.UI.ValueConverters
{
    public class SelectionHasMirrorableObjectCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selection = value as IEnumerable<ISelectableObject>;
            return selection?.Any(s
                => (s is LaneStartBase laneStart && laneStart.Children.All(c => c.IsSelected))
                   || (s is Bullet bullet && bullet.ReferenceBulletPallete == BulletPallete.DummyCustomPallete)
                   || s is LaneBlockArea || s is Flick);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}