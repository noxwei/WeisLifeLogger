import { Chart } from 'chart.js';
import 'chartjs-adapter-date-fns';
import { JournalEntry } from '../types/models.js';
export declare class TerminalCharts {
    private static charts;
    static destroyChart(chartId: string): void;
    static createTimeSeriesChart(canvasId: string, entries: JournalEntry[], metric: 'entries' | 'steps' | 'hrv'): Chart | null;
    static createHourlyActivityChart(canvasId: string, entries: JournalEntry[]): Chart | null;
    static createLocationFrequencyChart(canvasId: string, entries: JournalEntry[]): Chart | null;
    static createWeeklyPatternChart(canvasId: string, entries: JournalEntry[]): Chart | null;
    static createStepsProgressChart(canvasId: string, entries: JournalEntry[]): Chart | null;
    static createDeviceBatteryChart(canvasId: string, entries: JournalEntry[]): Chart | null;
    static destroyAllCharts(): void;
    static getChart(chartId: string): Chart | undefined;
}
