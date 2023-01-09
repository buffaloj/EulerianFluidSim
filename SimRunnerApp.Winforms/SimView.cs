using EulerianFluidSim;
using System.Drawing.Imaging;

namespace SimRunnerApp.Winforms
{
    public class SimView : Control
    {
        public Bitmap? _bitmap;
        public ColorSimRenderer? _simulationRenderer;
        private Pen linePen = new(Color.Red, 1);
        private BufferedGraphics? _graphicsBuffer;

        public bool ShowFlowLines { get; set; }

        public ColorSimRenderer SetSimulation(Simulation sim)
        {
            using (Graphics graphics = CreateGraphics())
            {
                _graphicsBuffer = BufferedGraphicsManager.Current.Allocate(graphics, ClientRectangle);// new Rectangle(0, 0, sim.NumCellsX, sim.NumCellsY));
            }

            _bitmap = new Bitmap(sim.NumCellsX, sim.NumCellsY, PixelFormat.Format24bppRgb);
            _simulationRenderer = new ColorSimRenderer(sim);

            return _simulationRenderer;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            _graphicsBuffer?.Dispose();
            using (Graphics graphics = CreateGraphics())
            {
                _graphicsBuffer = BufferedGraphicsManager.Current.Allocate(graphics, ClientRectangle);// new Rectangle(0, 0, sim.NumCellsX, sim.NumCellsY));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_simulationRenderer != null)
            {
                _simulationRenderer.Render();

                Rectangle rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
                var bmpData = _bitmap.LockBits(rect, ImageLockMode.WriteOnly, _bitmap.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(_simulationRenderer.bits, 0, ptr, _simulationRenderer.bits.Length);
                _bitmap.UnlockBits(bmpData);

                if (ShowFlowLines)
                    _simulationRenderer.RenderFlowLines(DrawLine);

                Graphics g = _graphicsBuffer.Graphics;
                g.DrawImage(_bitmap, ClientRectangle);
                _graphicsBuffer.Render(e.Graphics);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e){ /* Leave this empty to avoid clearing the background since the bitmap will do this anyway */}

        private void DrawLine(float x1, float y1, float x2, float y2)
        {
            using (var graphics = Graphics.FromImage(_bitmap))
            {
                graphics.DrawLine(linePen, x1, y1, x2, y2);
            }
        }

    }
}
