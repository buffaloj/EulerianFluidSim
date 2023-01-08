using EulerianFluidSim;
using System.Drawing.Imaging;

namespace SimRunnerApp.Winforms
{
    public partial class SimulationView : UserControl
    {
        public Bitmap _bitmap;
        public ColorSimRenderer _simulationRenderer;
        private Pen linePen = new(Color.Red, 1);

        public ColorSimRenderer SetSimulation(Simulation sim)
        {
            _bitmap = new Bitmap(sim.NumCellsX, sim.NumCellsY, PixelFormat.Format24bppRgb);
            _simulationRenderer = new ColorSimRenderer(sim, SetPixelColor, DrawLine);

            return _simulationRenderer;
        }

        public SimulationView()
        {
            InitializeComponent();
        }

        // This is needed to avoid Windows clearing the background with causing flickering
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void GraphicsView_Paint(object sender, PaintEventArgs e)
        {
            if (_simulationRenderer != null)
            {
                _simulationRenderer.Render();
                var oldMode = e.Graphics.CompositingMode;
                
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                e.Graphics.DrawImage(_bitmap, ClientRectangle);
                e.Graphics.CompositingMode = oldMode;
            }
        }

        private void SetPixelColor(int x, int y, float red, float green, float blue)
        {
            _bitmap.SetPixel(x, y, Color.FromArgb(255, (byte)(255 * red), (byte)(255 * green), (byte)(255 * blue)));
        }

        private void DrawLine(float x1, float y1, float x2, float y2)
        {
            using (var graphics = Graphics.FromImage(_bitmap))
            {
                graphics.DrawLine(linePen, x1, y1, x2, y2);
            }
        }
    }
}
