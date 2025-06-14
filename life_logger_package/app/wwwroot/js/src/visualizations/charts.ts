// Chart.js visualization components with retro terminal styling
import { Chart, registerables } from 'chart.js';
import 'chartjs-adapter-date-fns';
import { JournalEntry, AnalyticsData, ChartDataPoint } from '../types/models.js';
import { DataProcessor } from '../utils/api.js';
import { 
    createTimeSeriesOptions, 
    createBarChartOptions, 
    createDoughnutOptions,
    terminalColors,
    getDatasetColors 
} from '../utils/chartTheme.js';

// Register Chart.js components
Chart.register(...registerables);

export class TerminalCharts {
    private static charts: Map<string, Chart> = new Map();

    static destroyChart(chartId: string): void {
        const existingChart = this.charts.get(chartId);
        if (existingChart) {
            existingChart.destroy();
            this.charts.delete(chartId);
        }
    }

    static createTimeSeriesChart(
        canvasId: string, 
        entries: JournalEntry[], 
        metric: 'entries' | 'steps' | 'hrv'
    ): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        // Aggregate data by date
        const dataMap = new Map<string, number>();
        
        entries.forEach(entry => {
            const date = entry.date;
            let value = 0;
            
            switch (metric) {
                case 'entries':
                    value = 1;
                    break;
                case 'steps':
                    value = entry.steps || 0;
                    break;
                case 'hrv':
                    value = entry.hrv || 0;
                    break;
            }
            
            const existing = dataMap.get(date) || 0;
            dataMap.set(date, existing + value);
        });

        const data = Array.from(dataMap.entries())
            .sort(([a], [b]) => a.localeCompare(b))
            .map(([date, value]) => ({
                x: date,
                y: value
            }));

        const chart = new Chart(canvas, {
            type: 'line',
            data: {
                datasets: [{
                    label: metric.charAt(0).toUpperCase() + metric.slice(1),
                    data: data,
                    borderColor: terminalColors.cyan,
                    backgroundColor: 'rgba(0, 255, 255, 0.1)',
                    pointBackgroundColor: terminalColors.cyan,
                    pointBorderColor: terminalColors.green,
                    pointHoverBackgroundColor: terminalColors.green,
                    pointHoverBorderColor: terminalColors.cyan,
                    fill: true
                }]
            },
            options: createTimeSeriesOptions(`${metric.toUpperCase()} OVER TIME`)
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static createHourlyActivityChart(canvasId: string, entries: JournalEntry[]): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        const hourlyData = DataProcessor.createHourlyDistribution(entries);
        
        const data = Object.entries(hourlyData).map(([hour, count]) => ({
            x: parseInt(hour),
            y: count
        }));

        const { backgroundColor, borderColor } = getDatasetColors(24);

        const chart = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: Array.from({ length: 24 }, (_, i) => i.toString().padStart(2, '0') + ':00'),
                datasets: [{
                    label: 'Activity Count',
                    data: data.map(d => d.y),
                    backgroundColor: backgroundColor.map(color => color + '80'), // Add transparency
                    borderColor: borderColor,
                    borderWidth: 1
                }]
            },
            options: createBarChartOptions('HOURLY ACTIVITY DISTRIBUTION')
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static createLocationFrequencyChart(canvasId: string, entries: JournalEntry[]): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        const locationCounts = DataProcessor.aggregateLocationVisits(entries);
        const sortedLocations = Array.from(locationCounts.entries())
            .sort(([, a], [, b]) => b - a)
            .slice(0, 10); // Top 10 locations

        const labels = sortedLocations.map(([location]) => {
            // Truncate long addresses for display
            if (location.length > 30) {
                return location.substring(0, 27) + '...';
            }
            return location;
        });

        const data = sortedLocations.map(([, count]) => count);
        const { backgroundColor, borderColor } = getDatasetColors(data.length);

        const chart = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColor.map(color => color + 'CC'), // Add transparency
                    borderColor: borderColor,
                    borderWidth: 2
                }]
            },
            options: createDoughnutOptions('TOP LOCATIONS BY FREQUENCY')
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static createWeeklyPatternChart(canvasId: string, entries: JournalEntry[]): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        const weeklyData = DataProcessor.createWeeklyDistribution(entries);
        const daysOrder = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
        
        const data = daysOrder.map(day => weeklyData[day]);
        const { backgroundColor, borderColor } = getDatasetColors(7);

        const chart = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: daysOrder.map(day => day.substring(0, 3).toUpperCase()),
                datasets: [{
                    label: 'Activity Count',
                    data: data,
                    backgroundColor: backgroundColor.map(color => color + '80'),
                    borderColor: borderColor,
                    borderWidth: 1
                }]
            },
            options: createBarChartOptions('WEEKLY ACTIVITY PATTERN')
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static createStepsProgressChart(canvasId: string, entries: JournalEntry[]): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        // Get entries with steps data
        const stepsEntries = entries.filter(entry => entry.steps && entry.steps > 0);
        
        if (stepsEntries.length === 0) {
            // Show placeholder for no data
            const ctx = canvas.getContext('2d');
            if (ctx) {
                ctx.fillStyle = terminalColors.green;
                ctx.font = '16px ' + "'Fira Code', monospace";
                ctx.textAlign = 'center';
                ctx.fillText('NO STEPS DATA AVAILABLE', canvas.width / 2, canvas.height / 2);
            }
            return null;
        }

        // Aggregate steps by date
        const stepsMap = new Map<string, number>();
        stepsEntries.forEach(entry => {
            const date = entry.date;
            const existing = stepsMap.get(date) || 0;
            stepsMap.set(date, existing + (entry.steps || 0));
        });

        const data = Array.from(stepsMap.entries())
            .sort(([a], [b]) => a.localeCompare(b))
            .map(([date, steps]) => ({
                x: date,
                y: steps
            }));

        const chart = new Chart(canvas, {
            type: 'line',
            data: {
                datasets: [{
                    label: 'Daily Steps',
                    data: data,
                    borderColor: terminalColors.yellow,
                    backgroundColor: 'rgba(255, 255, 0, 0.1)',
                    pointBackgroundColor: terminalColors.yellow,
                    pointBorderColor: terminalColors.green,
                    pointHoverBackgroundColor: terminalColors.green,
                    pointHoverBorderColor: terminalColors.yellow,
                    fill: true,
                    tension: 0.2
                }]
            },
            options: createTimeSeriesOptions('DAILY STEPS TRACKING')
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static createDeviceBatteryChart(canvasId: string, entries: JournalEntry[]): Chart | null {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return null;

        this.destroyChart(canvasId);

        // Get entries with battery data
        const batteryEntries = entries.filter(entry => entry.device_battery != null);
        
        if (batteryEntries.length === 0) {
            const ctx = canvas.getContext('2d');
            if (ctx) {
                ctx.fillStyle = terminalColors.green;
                ctx.font = '16px ' + "'Fira Code', monospace";
                ctx.textAlign = 'center';
                ctx.fillText('NO BATTERY DATA AVAILABLE', canvas.width / 2, canvas.height / 2);
            }
            return null;
        }

        const data = batteryEntries
            .sort((a, b) => a.datetime.localeCompare(b.datetime))
            .map(entry => ({
                x: entry.datetime,
                y: entry.device_battery || 0
            }));

        const chart = new Chart(canvas, {
            type: 'line',
            data: {
                datasets: [{
                    label: 'Device Battery %',
                    data: data,
                    borderColor: terminalColors.red,
                    backgroundColor: 'rgba(255, 0, 64, 0.1)',
                    pointBackgroundColor: terminalColors.red,
                    pointBorderColor: terminalColors.green,
                    pointHoverBackgroundColor: terminalColors.green,
                    pointHoverBorderColor: terminalColors.red,
                    fill: true
                }]
            },
            options: {
                ...createTimeSeriesOptions('DEVICE BATTERY LEVEL'),
                scales: {
                    ...createTimeSeriesOptions('DEVICE BATTERY LEVEL').scales,
                    y: {
                        ...createTimeSeriesOptions('DEVICE BATTERY LEVEL').scales.y,
                        min: 0,
                        max: 100,
                        ticks: {
                            ...createTimeSeriesOptions('DEVICE BATTERY LEVEL').scales.y.ticks,
                            callback: function(value) {
                                return value + '%';
                            }
                        }
                    }
                }
            }
        });

        this.charts.set(canvasId, chart);
        return chart;
    }

    static destroyAllCharts(): void {
        this.charts.forEach(chart => chart.destroy());
        this.charts.clear();
    }

    static getChart(chartId: string): Chart | undefined {
        return this.charts.get(chartId);
    }
}