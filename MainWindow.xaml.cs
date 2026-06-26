using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ErsmAnim.Models;

namespace ErsmAnim
{
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private System.Windows.Shapes.Polyline currentLine;
        private bool isEraser;
        private Color currentColor = Colors.Black;
        private double brushSize = 4;

        private bool cursorDragging;
        private double timelineZoom = 1.0;
        private int activeFrameIndex;
        private DrawingLayer activeLayer;

        private readonly Stack<DrawingAction> undoStack = new Stack<DrawingAction>();
        private readonly Stack<DrawingAction> redoStack = new Stack<DrawingAction>();
        private readonly List<AnimationFrame> frames = new List<AnimationFrame>();

        private const double FrameButtonWidth = 40;
        private const double FrameButtonSpacing = 6;
        private const double TimelineHeight = 90;
        private double FrameSlot => FrameButtonWidth + FrameButtonSpacing;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddFrame();
            DrawTimelineGrid();
            DrawKeyframes();
            MoveCursorToFrame(activeFrameIndex);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTimelineGrid();
            DrawKeyframes();
        }
    }
}
