import { ChartOptions } from '../types/models.js';
export declare const terminalColors: {
    background: string;
    backgroundCard: string;
    backgroundLighter: string;
    green: string;
    cyan: string;
    yellow: string;
    red: string;
    white: string;
    gray: string;
    grayDark: string;
    scanLine: string;
    glow: string;
};
export declare const terminalFonts: {
    primary: string;
    secondary: string;
};
export declare function createTerminalChartOptions(title?: string): ChartOptions;
export declare function createTimeSeriesOptions(title: string): any;
export declare function createBarChartOptions(title: string): any;
export declare function createDoughnutOptions(title: string): any;
export declare const terminalColorPalette: string[];
export declare function getDatasetColors(dataCount: number): {
    backgroundColor: string[];
    borderColor: string[];
};
