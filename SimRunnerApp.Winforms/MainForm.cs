using EulerianFluidSim;

namespace SimRunnerApp.Winforms
{
    public partial class MainForm : Form
    {
        //private Bitmap _bitmap;
        private Simulation _simulation;
        private ColorSimRenderer _simulationRenderer;
        //private Pen linePen = new(Color.Red, 1);

        public void StepSimulation(float elapsed)
        {
            var fps = 1.0f / elapsed;
            fpsLabel.Text = $"{fps}fps";

            _simulation?.StepSimulation(elapsed);
        }

        public void InvalidateSimView()
        {
            simulationView.Invalidate();  
        }

        private void CreateSim()
        {
            var width = ClientRectangle.Width;
            var height = ClientRectangle.Height;
            width /= 5;
            height /= 5;
            //_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _simulation = new Simulation(width, height, 1.0f, 1.0f, 1.9f);
            //_simulationRenderer = new ColorSimRenderer(_simulation, SetPixelColor, DrawLine);
            _simulationRenderer = simulationView.SetSimulation(_simulation);

            speedBar.Value = (int)_simulation.FlowRate;
            checkPressure.Checked = _simulationRenderer.ShowPressure;
            checkColorSmoke.Checked = _simulationRenderer.ShowColoredSmoke;
            checkFlowLines.Checked = _simulationRenderer.ShowFlowLines;
        }

        //private void SetPixelColor(int x, int y, float red, float green, float blue)
        //{
        //    _bitmap.SetPixel(x, y, Color.FromArgb(255, (byte)(255 * red), (byte)(255 * green), (byte)(255 * blue)));
        //}

        //private void DrawLine(float x1, float y1, float x2, float y2)
        //{
        //    using (var graphics = Graphics.FromImage(_bitmap))
        //    {
        //        graphics.DrawLine(linePen, x1, y1, x2, y2);
        //    }
        //}

        public MainForm()
        {
            InitializeComponent();

            CreateSim();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            //_simulationRenderer.Render();
            //e.Graphics.DrawImage(_bitmap, ClientRectangle);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //CreateSim();
            //Refresh();
        }

        // This is needed to avoid Windows clearing the background with causing flickering
        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //}

        private void speedBar_Scroll(object sender, EventArgs e)
        {
            _simulation.FlowRate = speedBar.Value;
        }

        private void checkPressure_CheckedChanged(object sender, EventArgs e)
        {
            _simulationRenderer.ShowPressure = checkPressure.Checked;
        }

        private void checkColorSmoke_CheckedChanged(object sender, EventArgs e)
        {
            _simulationRenderer.ShowColoredSmoke = checkColorSmoke.Checked;
        }

        private void checkFlowLines_CheckedChanged(object sender, EventArgs e)
        {
            _simulationRenderer.ShowFlowLines = checkFlowLines.Checked;
        }
    }
}