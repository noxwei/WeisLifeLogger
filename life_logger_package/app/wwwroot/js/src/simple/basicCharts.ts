// Simple chart implementations for quick demo
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

export class BasicCharts {
    private static terminalColors = {
        green: '#00ff41',
        cyan: '#00ffff',
        yellow: '#ffff00',
        background: '#0a0a0a'
    };

    static createSimpleLineChart(canvasId: string, data: any[], label: string) {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return;

        const existingChart = Chart.getChart(canvas);
        if (existingChart) existingChart.destroy();

        new Chart(canvas, {
            type: 'line',
            data: {
                labels: data.map(d => d.date),
                datasets: [{
                    label: label,
                    data: data.map(d => d.value),
                    borderColor: this.terminalColors.cyan,
                    backgroundColor: this.terminalColors.cyan + '20',
                    pointBackgroundColor: this.terminalColors.green,
                    borderWidth: 2,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        labels: {
                            color: this.terminalColors.green,
                            font: { family: "'JetBrains Mono', monospace" }
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: this.terminalColors.green },
                        grid: { color: '#202020' }
                    },
                    y: {
                        ticks: { color: this.terminalColors.green },
                        grid: { color: '#202020' }
                    }
                }
            }
        });
    }

    static createSimpleBarChart(canvasId: string, data: any[], label: string) {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return;

        const existingChart = Chart.getChart(canvas);
        if (existingChart) existingChart.destroy();

        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: data.map(d => d.label),
                datasets: [{
                    label: label,
                    data: data.map(d => d.value),
                    backgroundColor: this.terminalColors.green + '80',
                    borderColor: this.terminalColors.green,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        labels: {
                            color: this.terminalColors.green,
                            font: { family: "'JetBrains Mono', monospace" }
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: this.terminalColors.green },
                        grid: { display: false }
                    },
                    y: {
                        ticks: { color: this.terminalColors.green },
                        grid: { color: '#202020' }
                    }
                }
            }
        });
    }
}