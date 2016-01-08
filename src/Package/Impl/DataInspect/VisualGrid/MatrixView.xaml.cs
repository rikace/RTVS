﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// a fast matrix view usercontrol for two dimensional data
    /// Internally Visual is used directly for performance
    /// </summary>
    public partial class MatrixView : UserControl {
        private VisualGridScroller _scroller;

        public MatrixView() {
            InitializeComponent();

            _scroller = new VisualGridScroller();
            _scroller.ColumnHeader = ColumnHeader;
            _scroller.RowHeader = RowHeader;
            _scroller.DataGrid = Data;
        }

        public void Initialize(IGridProvider<string> dataProvider) {
            Points = new GridPoints(dataProvider.RowCount, dataProvider.ColumnCount);

            DataProvider = dataProvider;
        }

        private GridPoints _gridPoints;
        private GridPoints Points {
            get {
                return _gridPoints;
            }

            set {
                _gridPoints = value;
                RowHeader.Points = _gridPoints;
                ColumnHeader.Points = _gridPoints;
                Data.Points = _gridPoints;
                _scroller.Points = _gridPoints;
            }
        }

        private IGridProvider<string> _dataProvider;
        private IGridProvider<string> DataProvider {
            get {
                return _dataProvider;
            }
            set {
                if (_dataProvider == value) {
                    return;
                }

                _dataProvider = value;

                RowHeader.DataProvider = _dataProvider;
                ColumnHeader.DataProvider = _dataProvider;
                Data.DataProvider = _dataProvider;
                _scroller.DataProvider = DataProvider;

                // queue fake command for visual refresh
                _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, 0);
            }
        }

        public int RowCount {
            get {
                return _dataProvider.RowCount;
            }
        }

        public int ColumnCount {
            get {
                return _dataProvider.ColumnCount;
            }
        }

        #region GridLinesBrush

        public static readonly DependencyProperty GridLinesBrushProperty =
            DependencyProperty.Register(
                "GridLinesBrush",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnGridLinesBrushPropertyChanged)));

        public Brush GridLinesBrush {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        private static void OnGridLinesBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnGridLinesBrushPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridLinesBrushPropertyChanged(Brush gridLineBrush) {
            Data.SetGridLineBrush(gridLineBrush);
        }

        #endregion

        #region GridBackground

        public static readonly DependencyProperty GridBackgroundProperty =
            DependencyProperty.Register(
                "GridBackground",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnGridBackgroundPropertyChanged)));

        public Brush GridBackground {
            get { return (Brush)GetValue(GridBackgroundProperty); }
            set { SetValue(GridBackgroundProperty, value); }
        }

        private static void OnGridBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnGridBackgroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridBackgroundPropertyChanged(Brush gridBackgroundBrush) {
            Data.Background = gridBackgroundBrush;
        }

        #endregion

        #region HeaderLinesBrush

        public static readonly DependencyProperty HeaderLinesBrushProperty =
            DependencyProperty.Register(
                "HeaderLinesBrush",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnHeaderLinesBrushPropertyChanged)));

        public Brush HeaderLinesBrush {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        private static void OnHeaderLinesBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnHeaderLinesBrushPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnHeaderLinesBrushPropertyChanged(Brush gridLineBrush) {
            ColumnHeader.SetGridLineBrush(gridLineBrush);
            RowHeader.SetGridLineBrush(gridLineBrush);
        }

        #endregion

        #region HeaderBackground

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                "HeaderBackground",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnHeaderBackgroundPropertyChanged)));

        public Brush HeaderBackground {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        private static void OnHeaderBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnHeaderBackgroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnHeaderBackgroundPropertyChanged(Brush headerBackground) {
            ColumnHeader.Background = headerBackground;
            RowHeader.Background = headerBackground;
        }

        #endregion

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            // null if not Initialized yet
            if (_scroller != null) {
                _scroller.EnqueueCommand(ScrollType.SizeChange, Data.RenderSize);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (e.Delta > 0 || e.Delta < 0) {
                _scroller.EnqueueCommand(ScrollType.MouseWheel, e.Delta);
                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            Point pt = e.GetPosition(this);

            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result.VisualHit is TextVisual) {
                var textVisual = (TextVisual)result.VisualHit;
                textVisual.ToggleHighlight(Background);

                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseLeftButtonDown(e);
            }
        }

        private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    _scroller.EnqueueCommand(ScrollType.PageUp, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    _scroller.EnqueueCommand(ScrollType.PageDown, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    _scroller.EnqueueCommand(ScrollType.LineUp, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    _scroller.EnqueueCommand(ScrollType.LineDown, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    _scroller.EnqueueCommand(ScrollType.PageLeft, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    _scroller.EnqueueCommand(ScrollType.PageRight, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    _scroller.EnqueueCommand(ScrollType.LineLeft, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    _scroller.EnqueueCommand(ScrollType.LineRight, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }
    }
}