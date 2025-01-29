console.log("timeChart.js loaded!");
window.timeChartFunctions = {
    chartInstance: null,

    updateChart: function (canvasId, labels, data) {
        if (!this.chartInstance) {
            // Create new chart
            const ctx = document.getElementById(canvasId).getContext('2d');
            this.chartInstance = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Algorithm Time (ms)',
                        data: data,
                        backgroundColor: ['rgba(54, 162, 235, 0.6)', 'rgba(255, 99, 132, 0.6)'],
                    }]
                },
                options: {
                    scales: {
                        y: { beginAtZero: true }
                    }
                }
            });
        } else {
            // Update existing chart
            this.chartInstance.data.labels = labels;
            this.chartInstance.data.datasets[0].data = data;
            this.chartInstance.update();
        }
    }
};
