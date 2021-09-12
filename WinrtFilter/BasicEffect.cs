using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI;

namespace WinrtFilter
{
    public sealed class BasicEffectProperty
    {
        public bool IsShapeActive { get; set; } = false;
        public double TranslationX { get; set; } = 0;
        public double TranslationY { get; set; } = 0;
        public double Scale { get; set; } = 1;
        public double Rotation { get; set; } = 0;
    }
    public sealed class BasicEffect : IBasicVideoEffect
    {
        public static BasicEffectProperty Default { get; } = new BasicEffectProperty { IsShapeActive = false };

        CanvasDevice m_device;
        CanvasGeometry m_geo;
        Matrix3x2 mat;
        bool m_crntMode;
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            m_device = CanvasDevice.CreateFromDirect3D11Device(device);
            CanvasDevice.DebugLevel = CanvasDebugLevel.Warning;
            mat = Matrix3x2.Identity;
            m_crntMode = false;
        }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (var output = CanvasRenderTarget.CreateFromDirect3D11Surface(m_device, context.OutputFrame.Direct3DSurface))
            using (var input = CanvasBitmap.CreateFromDirect3D11Surface(m_device, context.InputFrame.Direct3DSurface))
            {
                var m_prop = Property;

                if (m_geo == null || m_geo.Device != m_device || m_crntMode != m_prop.IsShapeActive)
                {
                    m_geo?.Dispose();
                    m_geo = GetGeometry(m_device, output.Bounds.Width, output.Bounds.Height, m_prop.IsShapeActive);
                    m_crntMode = m_prop.IsShapeActive;
                }

                Vector2 center = new Vector2((float)(output.Bounds.Width / 2), (float)(output.Bounds.Height / 2));
                mat = Matrix3x2.CreateScale((float)m_prop.Scale, center);

                using (var session = output.CreateDrawingSession())
                {
                    session.Clear(Colors.Transparent);
                    using (var layer = session.CreateLayer(1.0f, m_geo, mat))
                        session.DrawImage(input, output.Bounds, input.Bounds);
                }
            }
        }
        CanvasGeometry GetGeometry(CanvasDevice device, double Width, double Height, bool isShape)
        {
            if (!isShape)
            {
                return CanvasGeometry.CreateRectangle(device, new Windows.Foundation.Rect(0, 0, Width, Height));
            }

            float MaxSize = 500;
            Vector2[] Pts = new Vector2[]
            {
                new Vector2(0,194/MaxSize),
                new Vector2(167/MaxSize,162/MaxSize),
                new Vector2(249/MaxSize,13/MaxSize),
                new Vector2(332/MaxSize,162/MaxSize),
                new Vector2(500/MaxSize,194/MaxSize),
                new Vector2(383/MaxSize,318/MaxSize),
                new Vector2(404/MaxSize,487/MaxSize),
                new Vector2(249/MaxSize,414/MaxSize),
                new Vector2(95/MaxSize,487/MaxSize),
                new Vector2(117/MaxSize,318/MaxSize),
            };

            using (CanvasPathBuilder builder = new CanvasPathBuilder(device))
            {
                float min = (float)Math.Min(Width, Height);
                float mid = min / 2;
                Vector2 translation = new Vector2((float)(Width / 2 - mid), (float)(Height / 2 - mid));

                builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                builder.BeginFigure(TransformVec(Pts[0], translation, min));
                foreach (var pt in Pts)
                {
                    builder.AddLine(TransformVec(pt, translation, min));
                }
                builder.EndFigure(CanvasFigureLoop.Closed);

                var geo = CanvasGeometry.CreatePath(builder);
                return geo;
            }
        }
        public static Vector2 TransformVec(Vector2 input, Vector2 translation, float minranged)
        {
            return new Vector2((input.X * minranged) + translation.X, (input.Y * minranged) + translation.Y);
        }
        public void Close(MediaEffectClosedReason reason)
        {

        }

        public void DiscardQueuedFrames()
        {

        }

        public bool IsReadOnly => true;

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties => new List<VideoEncodingProperties>();

        public MediaMemoryTypes SupportedMemoryTypes => MediaMemoryTypes.Gpu;

        public bool TimeIndependent => false;

        public BasicEffectProperty Property
        {
            get
            {
                if (config == null) return Default;
                if (config.TryGetValue(nameof(Property), out object p)) return (BasicEffectProperty)p;
                return Default;
            }
        }

        IPropertySet config;
        public void SetProperties(IPropertySet configuration)
        {
            config = configuration;
        }
    }
}
