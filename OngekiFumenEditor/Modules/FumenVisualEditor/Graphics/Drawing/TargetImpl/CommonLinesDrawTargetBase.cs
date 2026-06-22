using Caliburn.Micro;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Kernel.Graphics;
using OngekiFumenEditor.Kernel.Graphics.DrawCommands;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using static OngekiFumenEditor.Kernel.Graphics.ILineDrawing;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Graphics.Drawing.TargetImpl
{
    public abstract class CommonLinesDrawTargetBase<T> : CommonBatchDrawTargetBase<T> where T : ConnectableStartObject
    {
        public virtual int LineWidth { get; } = 2;
        private static VertexDash invailedDash = new VertexDash(6, 3);

        private static readonly Vector4 GlowColor = new(252f / 255f, 1f, 75f / 255f, 0.6f);
        public virtual float GlowLineWidthMultiplier => 3f;

        public override void Initialize(IRenderManagerImpl impl)
        {
        }

        public abstract Vector4 GetLanePointColor(ConnectableObjectBase obj);

        public void FillLine(IFumenEditorDrawingContext target, IDrawCommandListBuilder builder, T start)
        {
            var color = GetLanePointColor(start);
            var soflanList = target.CurrentDrawingTargetContext.CurrentSoflanList;

            if (Properties.EditorGlobalSetting.Default.EnableHighlightSelectedLane)
                VisibleLineVerticesQuery.QueryGlowLineVertices(target, start, soflanList, GlowColor,
                    segment => builder.DrawSimpleLines(segment, LineWidth * GlowLineWidthMultiplier));

            using var list = ObjectPool.GetPooledList<LineVertex>();
            VisibleLineVerticesQuery.QueryVisibleLineVertices(target, start, soflanList, invailedDash, color, list);
            builder.DrawSimpleLines(list, LineWidth);
        }

        public override void DrawBatch(IFumenEditorDrawingContext target, IDrawCommandListBuilder builder, IEnumerable<T> starts)
        {
            foreach (var laneStart in starts)
            {
                FillLine(target, builder, laneStart);
            }
        }
    }
}
