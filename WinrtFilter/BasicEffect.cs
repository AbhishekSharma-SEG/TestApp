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
    public sealed class BasicEffect : IBasicVideoEffect
    {
        CanvasDevice m_device;
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            m_device = CanvasDevice.CreateFromDirect3D11Device(device);
        }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (var output = CanvasRenderTarget.CreateFromDirect3D11Surface(m_device, context.OutputFrame.Direct3DSurface))
            using (var input = CanvasBitmap.CreateFromDirect3D11Surface(m_device, context.InputFrame.Direct3DSurface))
            {
                using (var session = output.CreateDrawingSession())
                {
                    session.Clear(Colors.Transparent);
                    session.DrawImage(input, output.Bounds, input.Bounds);
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {

        }

        public void DiscardQueuedFrames()
        {

        }

        public bool IsReadOnly => false;

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties => new List<VideoEncodingProperties>();

        public MediaMemoryTypes SupportedMemoryTypes => MediaMemoryTypes.Gpu;

        public bool TimeIndependent => false;

        IPropertySet config;
        public void SetProperties(IPropertySet configuration)
        {
            config = configuration;
        }
    }
}
