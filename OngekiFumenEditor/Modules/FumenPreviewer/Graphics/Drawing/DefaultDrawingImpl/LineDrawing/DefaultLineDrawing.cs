﻿using OngekiFumenEditor.Modules.FumenPreviewer.Graphics.Drawing.Base;
using OngekiFumenEditor.Modules.FumenPreviewer.Graphics.Drawing.Shaders;
using OngekiFumenEditor.Utils.ObjectPool;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Documents;
using OngekiFumenEditor.Utils;
using System.ComponentModel.Composition;
using Polyline2DCSharp;

namespace OngekiFumenEditor.Modules.FumenPreviewer.Graphics.Drawing.DefaultDrawingImpl.LineDrawing
{
    [Export(typeof(ILineDrawing))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DefaultLineDrawing : ILineDrawing, IDisposable
    {
        private readonly Shader shader;
        private readonly int vbo;
        private readonly int vao;

        private static StateStack DefaultRenderStateStack { get; } = new StateStack(() =>
        {
            var list = ObjectPool<List<int>>.Get();
            list.Clear();

            list.Add(GL.IsEnabled(EnableCap.PolygonSmooth) ? 1 : 0);
            list.Add(GL.GetInteger(GetPName.PolygonSmoothHint));

            return list;
        }, (l) =>
        {
            var list = l as List<int>;

            if (list[0] is 1)
                GL.Enable(EnableCap.PolygonSmooth);
            else
                GL.Disable(EnableCap.PolygonSmooth);

            GL.Hint(HintTarget.PolygonSmoothHint, (HintMode)list[1]);

            ObjectPool<List<int>>.Return(list);
        });

        public DefaultLineDrawing()
        {
            shader = CommonLineShader.Shared;

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            Init();
        }

        private void Init()
        {
            GL.BindVertexArray(vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                {
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 6, 0);

                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, sizeof(float) * 6, sizeof(float) * 2);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        public void Draw(IFumenPreviewer target, IEnumerable<ILineDrawing.LineVertex> points, float lineWidth)
        {
            shader.Begin();
            {
                shader.PassUniform("Model", Matrix4.CreateTranslation(-target.ViewWidth / 2, -target.ViewHeight / 2, 0));
                shader.PassUniform("ViewProjection", target.ViewProjectionMatrix);
                GL.BindVertexArray(vao);

                using var d = ObjectPool<List<Vec2>>.GetWithUsingDisposable(out var vecList, out _);
                vecList.Clear();

                var color = points.FirstOrDefault().Color;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                {
                    using var d2 = points.Select(x => new Vec2() { x = x.Point.X, y = x.Point.Y }).ToListWithObjectPool(out var inputVecList);

                    var genVertices = Polyline2D.Create(vecList, inputVecList, lineWidth,
                        Polyline2D.JointStyle.ROUND,
                        Polyline2D.EndCapStyle.ROUND
                        );


                    var arrBuffer2 = ArrayPool<float>.Shared.Rent(genVertices.Count * 6);
                    var arrBufferIdx2 = 0;

                    foreach (var p in genVertices)
                    {
                        arrBuffer2[(6 * arrBufferIdx2) + 0] = p.x;
                        arrBuffer2[(6 * arrBufferIdx2) + 1] = p.y;
                        arrBuffer2[(6 * arrBufferIdx2) + 2] = color.X;
                        arrBuffer2[(6 * arrBufferIdx2) + 3] = color.Y;
                        arrBuffer2[(6 * arrBufferIdx2) + 4] = color.Z;
                        arrBuffer2[(6 * arrBufferIdx2) + 5] = color.W;

                        arrBufferIdx2++;
                    }

                    GL.InvalidateBufferData(vbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * arrBufferIdx2 * 6), arrBuffer2, BufferUsageHint.DynamicDraw);

                    DefaultRenderStateStack.PushState();
                    {
                        GL.Enable(EnableCap.PolygonSmooth);
                        GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

                        GL.DrawArrays(PrimitiveType.Triangles, 0, arrBufferIdx2);
                    }
                    DefaultRenderStateStack.PopState();

                    ArrayPool<float>.Shared.Return(arrBuffer2);
                }

                GL.BindVertexArray(0);
            }
            shader.End();
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
        }
    }
}