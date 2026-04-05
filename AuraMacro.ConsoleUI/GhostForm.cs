using System;
using System.Drawing;
using System.Windows.Forms;

namespace AuraMacro.ConsoleUI
{
    public class GhostForm : Form
    {
        private Point _startPoint;
        private Point _endPoint;
        private bool _isDrawing;
        public Rectangle SelectedRegion { get; private set; }

        public GhostForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.Opacity = 0.4;
            this.Cursor = Cursors.Cross;
            this.TopMost = true;

            // Make the form double buffered to prevent flicker when drawing
            this.DoubleBuffered = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDrawing = true;
                _startPoint = e.Location;
                _endPoint = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDrawing)
            {
                _endPoint = e.Location;
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isDrawing && e.Button == MouseButtons.Left)
            {
                _isDrawing = false;
                _endPoint = e.Location;

                SelectedRegion = new Rectangle(
                    Math.Min(_startPoint.X, _endPoint.X),
                    Math.Min(_startPoint.Y, _endPoint.Y),
                    Math.Abs(_startPoint.X - _endPoint.X),
                    Math.Abs(_startPoint.Y - _endPoint.Y)
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_isDrawing)
            {
                var rect = new Rectangle(
                    Math.Min(_startPoint.X, _endPoint.X),
                    Math.Min(_startPoint.Y, _endPoint.Y),
                    Math.Abs(_startPoint.X - _endPoint.X),
                    Math.Abs(_startPoint.Y - _endPoint.Y)
                );

                using (var pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }
    }
}