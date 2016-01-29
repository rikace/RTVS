﻿using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryPreviousPlotCommand : PlotWindowCommand {
        public HistoryPreviousPlotCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdPrevPlot) {
        }
        internal override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex > 0;
        }
        internal override void Handle() {
            PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.PreviousPlotAsync());
        }
    }
}
