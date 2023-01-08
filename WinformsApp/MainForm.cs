using System.Diagnostics;
using System.Drawing.Imaging;

namespace EulerianFluidSim
{
    public partial class MainForm : Form
    {
        private Bitmap _bitmap;
        private Simulation _simulation;
        private SimulationRenderer _simulationRenderer;

        public void StepSimulation(float elapsed)
        {
            _simulation?.StepSimulation(elapsed);
        }

        private void CreateSim()
        {
            var width = ClientRectangle.Width;
            var height = ClientRectangle.Height;
            width /= 5;
            height /= 5;
            _bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _simulation = new Simulation(width, height, 1.0f, 1.0f, 1.9f);
            _simulationRenderer = new SimulationRenderer(_simulation);

            speedBar.Value = (int)_simulation.FlowRate;
            checkPressure.Checked = _simulationRenderer.ShowPressure;
            checkColorSmoke.Checked = _simulationRenderer.ShowColoredSmoke;
            checkFlowLines.Checked = _simulationRenderer.ShowFlowLines;
        }

        public MainForm()
        {
            InitializeComponent();

            CreateSim();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            _simulationRenderer.Render(_bitmap);
            e.Graphics.DrawImage(_bitmap, ClientRectangle);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //CreateSim();
            //Refresh();
        }

        // This is needed to avoid Windows clearing the background with causing flickering
        protected override void OnPaintBackground(PaintEventArgs e)
        {

        }

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