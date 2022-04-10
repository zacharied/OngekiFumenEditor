﻿using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Modules.EditorSvgObjectControlProvider.ViewModels;
using OngekiFumenEditor.Utils;
using SharpVectors.Renderers.Wpf;
using SvgConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OngekiFumenEditor.Base.EditorObjects.Svg
{
    public class SvgPrefab : OngekiMovableObjectBase
    {
        public override string IDShortName => "SVG";
        public override Type ModelViewType => typeof(SvgPrefabViewModel);

        private RangeValue rotation = RangeValue.Create(0, 360f, 0f);
        public RangeValue Rotation
        {
            get => rotation;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(rotation, value);
                Set(ref rotation, value);
            }
        }

        private RangeValue offsetX = RangeValue.CreateNormalized(0.5f);
        public RangeValue OffsetX
        {
            get => offsetX;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(offsetX, value);
                Set(ref offsetX, value);
            }
        }

        private RangeValue colorSimilar = RangeValue.Create(1, 1000, 600);
        public RangeValue ColorSimilar
        {
            get => colorSimilar;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(colorSimilar, value);
                Set(ref colorSimilar, value);
            }
        }

        private RangeValue offsetY = RangeValue.CreateNormalized(0.5f);
        public RangeValue OffsetY
        {
            get => offsetY;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(offsetY, value);
                Set(ref offsetY, value);
            }
        }

        private bool enableColorfulLaneSimilar = false;
        public bool EnableColorfulLaneSimilar
        {
            get => enableColorfulLaneSimilar;
            set
            {
                Set(ref enableColorfulLaneSimilar, value);
            }
        }

        private bool showOriginColor = true;
        public bool ShowOriginColor
        {
            get => showOriginColor;
            set
            {
                Set(ref showOriginColor, value);
            }
        }

        private float scale = 1;
        public float Scale
        {
            get => scale;
            set => Set(ref scale, value);
        }

        private RangeValue opacity = RangeValue.CreateNormalized(1);
        public RangeValue Opacity
        {
            get => opacity;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(opacity, value);
                Set(ref opacity, value);
            }
        }

        private RangeValue tolerance = RangeValue.Create(0, 20f, 1f);
        public RangeValue Tolerance
        {
            get => tolerance;
            set
            {
                this.RegisterOrUnregisterPropertyChangeEvent(tolerance, value);
                Set(ref tolerance, value);
            }
        }

        private FileInfo svgFile = null;
        private DrawingGroup drawingGroup;

        private DrawingGroup processingDrawingGroup;
        public DrawingGroup ProcessingDrawingGroup
        {
            get => processingDrawingGroup;
            set => Set(ref processingDrawingGroup, value);
        }

        public FileInfo SvgFile
        {
            get => svgFile;
            set => Set(ref svgFile, value);
        }

        public SvgPrefab()
        {
            Tolerance = Tolerance;
            Opacity = Opacity;
            Rotation = Rotation;
            OffsetX = OffsetX;
            OffsetY = OffsetY;
            ColorSimilar = ColorSimilar;
        }

        public override void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            base.NotifyOfPropertyChange(propertyName);

            switch (propertyName)
            {
                case nameof(SvgFile):
                    ReloadSvgFile();
                    break;
                case nameof(EnableColorfulLaneSimilar):
                case nameof(Rotation):
                case nameof(Scale):
                case nameof(ShowOriginColor):
                case nameof(Opacity):
                case nameof(OffsetX):
                case nameof(OffsetY):
                case nameof(ColorSimilar):
                case nameof(RangeValue.CurrentValue):
                case nameof(Tolerance):
                    RebuildGeometry();
                    break;
                default:
                    break;
            }
        }

        private void ReloadSvgFile()
        {
            CleanGeometry();

            if (SvgFile is null)
                return;

            drawingGroup = ConverterLogic.ConvertSvgToObject(SvgFile.FullName, ResultMode.DrawingGroup, new WpfDrawingSettings()
            {
                IncludeRuntime = false,
                TextAsGeometry = true,
                OptimizePath = true,
                EnsureViewboxSize = true
            }, out _, new()) as DrawingGroup;
            drawingGroup.Freeze();

            RebuildGeometry();
        }

        private void CleanGeometry()
        {
            drawingGroup = null;
            ProcessingDrawingGroup = default;
        }

        private void RebuildGeometry()
        {
            ProcessingDrawingGroup = default;
            var inter = drawingGroup.Children.FirstOrDefault();
            if (inter is null)
                return;

            var procDrawingGroup = new DrawingGroup();
            var bound = drawingGroup.Bounds;

            var transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform()
            {
                X = -OffsetX.CurrentValue * bound.Width,
                Y = -OffsetY.CurrentValue * bound.Height
            });
            transform.Children.Add(new ScaleTransform()
            {
                ScaleX = Scale,
                ScaleY = Scale,
            });
            transform.Children.Add(new RotateTransform()
            {
                Angle = Rotation.CurrentValue
            });
            procDrawingGroup.Transform = transform;

            Geometry GenFlattedGeometry(Geometry geometry)
            {
                /*
                if (geometry is RectangleGeometry)
                    return default;
                */
                /*
                var r = geometry.GetFlattenedPathGeometry();
                var flattedGeometry = new PathGeometry();
                var fig = new PathFigure();
                r.GetPointAtFractionLength(0, out var point, out _);
                fig.StartPoint = point;
                for (var i = RefSvgPrefab.Tolerance; i < 1; i += RefSvgPrefab.Tolerance)
                {
                    r.GetPointAtFractionLength(i, out point, out _);
                    fig.Segments.Add(new LineSegment()
                    {
                        IsStroked = true,
                        Point = point,
                    });
                }
                r.GetPointAtFractionLength(1, out point, out _);
                fig.Segments.Add(new LineSegment()
                {
                    IsStroked = true,
                    Point = point,
                });
                flattedGeometry.Figures.Add(fig);
                /**/
                var flattedGeometry = geometry.GetFlattenedPathGeometry(Tolerance.CurrentValue, ToleranceType.Absolute);
                flattedGeometry.Freeze();
                return flattedGeometry;
            }

            void VisitGeometryDrawing(GeometryDrawing geometryDrawing, DrawingGroup parentGroup = default)
            {
                if (GenFlattedGeometry(geometryDrawing.Geometry) is not Geometry geometry)
                    return;

                var newDrawing = new GeometryDrawing();
                newDrawing.Geometry = geometry;
                newDrawing.Brush = geometryDrawing.Brush;
                newDrawing.Pen = CalculateRelativePen(geometryDrawing.Pen);
                newDrawing.Freeze();

                //append to list
                procDrawingGroup.Children.Add(newDrawing);
            }

            void VisitGroup(DrawingGroup group, DrawingGroup parentGroup = default)
            {
                foreach (var child in group.Children.OfType<DrawingGroup>())
                    VisitGroup(child, group);
                foreach (var child in group.Children.OfType<GeometryDrawing>())
                    VisitGeometryDrawing(child, group);
            }

            VisitGroup(drawingGroup);

            procDrawingGroup.Freeze();
            ProcessingDrawingGroup = procDrawingGroup;

            Log.LogDebug($"Generate {ProcessingDrawingGroup.Children.Count} geometries from svg file: {SvgFile}.");
        }

        private Pen CalculateRelativePen(Pen pen)
        {
            float ColorDistance(Color a, Color b)
            {
                byte ra = a.R, rb = b.R, ga = a.G, gb = b.G, ba = a.B, bb = b.B;
                var rm = (ra + rb) / 2.0f;
                var R = (ra - rb);
                var G = (ga - gb);
                var B = (ba - bb);
                return MathF.Sqrt((2 + rm / 256.0f) * MathF.Pow(R, 2) + 4 * MathF.Pow(G, 2) + (2 + (255 - rm) / 256.0f) * MathF.Pow(B, 2));
            }

            Color PickColor(Color color)
            {
                var arr = LaneColor.AllLaneColors;
                if (!EnableColorfulLaneSimilar)
                    arr = arr.Where(x => x.LaneType != LaneType.Colorful);

                var r = arr
                    .Select(x => (x.Color, ColorDistance(x.Color, color)))
                    .OrderByDescending(x => x.Item2)
                    .Where(x => x.Item2 > ColorSimilar.CurrentValue);

                return r.Select(x => x.Color).FirstOrDefault();
            }
            if (ShowOriginColor)
                return pen;
            var color = pen?.Brush is SolidColorBrush b ? PickColor(b.Color) : Colors.Green;
            var brush = new SolidColorBrush(Color.FromArgb((byte)(Opacity.CurrentValue * color.A), color.R, color.G, color.B));
            brush.Freeze();
            var p = new Pen(brush, 2);
            p.Freeze();
            return p;
        }

        public override string ToString() => $"{base.ToString()} R:∠{Rotation}° O:{Opacity.ValuePercent * 100:F2}% S:{Rotation:F2}x File:{Path.GetFileName(SvgFile?.Name)}";
    }
}