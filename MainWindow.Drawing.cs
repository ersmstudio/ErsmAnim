using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ErsmAnim.Models;

namespace ErsmAnim
{
    public partial class MainWindow
    {
        private System.Windows.Controls.Canvas GetActiveLayerCanvas()
        {
            if (activeLayer == null)
                return null;

            // Prefer explicit Canvas pointer (kept in sync by MainWindow when switching frames),
            // otherwise try to resolve via per-layer per-frame canvases.
            if (activeLayer.Canvas != null)
                return activeLayer.Canvas;

            return activeLayer.GetCanvasForFrameIndex(activeFrameIndex);
        }

        private void CanvasContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var targetCanvas = GetActiveLayerCanvas();
            if (targetCanvas == null)
            {
                return;
            }

            isDrawing = true;
            currentLine = new Polyline
            {
                StrokeThickness = brushSize,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Stroke = isEraser ? Brushes.White : new SolidColorBrush(currentColor)
            };

            currentLine.Points.Add(e.GetPosition(targetCanvas));
            targetCanvas.Children.Add(currentLine);

            undoStack.Push(new DrawingAction(targetCanvas, currentLine));
            redoStack.Clear();
            Mouse.Capture(CanvasContainer);
        }

        private void CanvasContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentLine != null)
            {
                var targetCanvas = GetActiveLayerCanvas();
                if (targetCanvas != null)
                {
                    currentLine.Points.Add(e.GetPosition(targetCanvas));
                }
            }
        }

        private void CanvasContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            currentLine = null;
            Mouse.Capture(null);
        }

        private void BrushBtn_Click(object sender, RoutedEventArgs e)
        {
            isEraser = false;
        }

        private void EraserBtn_Click(object sender, RoutedEventArgs e)
        {
            isEraser = true;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            var targetCanvas = GetActiveLayerCanvas();
            if (targetCanvas == null)
            {
                return;
            }

            targetCanvas.Children.Clear();
            RemoveUndoItemsForCanvas(targetCanvas);
            redoStack.Clear();
        }

        private void ColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new ColorPickerWindow(currentColor)
            {
                Owner = this
            };

            if (picker.ShowDialog() == true)
            {
                currentColor = picker.SelectedColor;
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            while (undoStack.Count > 0)
            {
                var action = undoStack.Pop();
                if (action.LayerCanvas.Children.Contains(action.Element))
                {
                    action.LayerCanvas.Children.Remove(action.Element);
                    redoStack.Push(action);
                    return;
                }
            }
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            while (redoStack.Count > 0)
            {
                var action = redoStack.Pop();
                if (!action.LayerCanvas.Children.Contains(action.Element))
                {
                    action.LayerCanvas.Children.Add(action.Element);
                    undoStack.Push(action);
                    return;
                }
            }
        }

        private void RemoveUndoItemsForCanvas(System.Windows.Controls.Canvas canvas)
        {
            RebuildStackWithout(undoStack, action => action.LayerCanvas == canvas);
            RebuildStackWithout(redoStack, action => action.LayerCanvas == canvas);
        }

        private void RemoveUndoItemsForMissingCanvases()
        {
            // Collect all live canvases from frames -> layer -> per-frame canvases (or fallback Canvas).
            var canvases = frames
                .SelectMany(frame => frame.Layers)
                .SelectMany(layer =>
                    (layer.Canvases != null && layer.Canvases.Count > 0)
                        ? layer.Canvases
                        : new List<System.Windows.Controls.Canvas> { layer.Canvas })
                .Where(c => c != null)
                .ToList();

            RebuildStackWithout(undoStack, action => !canvases.Contains(action.LayerCanvas));
            RebuildStackWithout(redoStack, action => !canvases.Contains(action.LayerCanvas));
        }

        private static void RebuildStackWithout(Stack<DrawingAction> stack, System.Predicate<DrawingAction> removeWhen)
        {
            var remaining = stack.Reverse().Where(action => !removeWhen(action)).ToList();
            stack.Clear();

            foreach (var action in remaining)
            {
                stack.Push(action);
            }
        }
    }
}
