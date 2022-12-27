using System.Diagnostics;
using System.Drawing.Imaging;

namespace EulerianFluidSim
{
    public partial class MainForm : Form
    {
        private Bitmap _bitmap;
        private Simulation _simulation;

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

            speedBar.Value = (int)_simulation.FlowRate;
            checkPressure.Checked = _simulation.ShowPressure;
            checkColorSmoke.Checked = _simulation.ShowColoredSmoke;
            checkFlowLines.Checked = _simulation.ShowFlowLines;
        }

        public MainForm()
        {
            InitializeComponent();

            CreateSim();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            _simulation.Render(_bitmap);
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
            _simulation.ShowPressure = checkPressure.Checked;
        }

        private void checkColorSmoke_CheckedChanged(object sender, EventArgs e)
        {
            _simulation.ShowColoredSmoke = checkColorSmoke.Checked;
        }

        private void checkFlowLines_CheckedChanged(object sender, EventArgs e)
        {
            _simulation.ShowFlowLines = checkFlowLines.Checked;
        }
    }
}