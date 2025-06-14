// TypeScript interfaces matching C# models for Wei Life Logger

export interface JournalEntry {
    id: number;
    entry: string;
    action?: string;
    datetime: string;
    date: string;
    time: string;
    location?: string;
    weather?: string;
    phone_mode?: string;
    device?: string;
    steps?: number;
    hrv?: number;
    device_battery?: number;
    created_at: string;
    updated_at: string;
}

export interface DailySummary {
    date: string;
    entryCount: number;
    totalSteps?: number;
    averageHrv?: number;
    locations: string[];
    activities: string[];
    mostFrequentLocation?: string;
    weatherSummary?: string;
    deviceBatteryRange?: {
        min: number;
        max: number;
    };
}

export interface LocationFrequency {
    location: string;
    count: number;
    coordinates?: {
        latitude: number;
        longitude: number;
    };
}

export interface TimelineEntry {
    datetime: string;
    entry: string;
    location?: string;
    action?: string;
    steps?: number;
    type: 'entry' | 'location' | 'activity';
}

export interface AnalyticsData {
    totalEntries: number;
    uniqueLocations: number;
    totalSteps: number;
    averageEntriesPerDay: number;
    dateRange: {
        start: string;
        end: string;
    };
    topLocations: LocationFrequency[];
    dailyStats: DailySummary[];
    activityPatterns: {
        hourlyDistribution: { [hour: number]: number };
        weeklyDistribution: { [day: string]: number };
    };
}

export interface MapLocation {
    id: string;
    name: string;
    coordinates: {
        latitude: number;
        longitude: number;
    };
    visits: number;
    entries: JournalEntry[];
    category: 'home' | 'travel' | 'shopping' | 'dining' | 'other';
}

export interface RouteSegment {
    from: MapLocation;
    to: MapLocation;
    frequency: number;
    entries: JournalEntry[];
}

export interface ChartDataPoint {
    x: string | number | Date;
    y: number;
    label?: string;
    metadata?: any;
}

export interface ChartOptions {
    responsive: boolean;
    maintainAspectRatio: boolean;
    plugins: {
        legend: {
            display: boolean;
            labels: {
                color: string;
                font: {
                    family: string;
                    size: number;
                };
            };
        };
        tooltip: {
            backgroundColor: string;
            titleColor: string;
            bodyColor: string;
            borderColor: string;
            borderWidth: number;
        };
    };
    scales: {
        x: {
            ticks: {
                color: string;
                font: {
                    family: string;
                };
            };
            grid: {
                color: string;
            };
        };
        y: {
            ticks: {
                color: string;
                font: {
                    family: string;
                };
            };
            grid: {
                color: string;
            };
        };
    };
}

export interface ApiResponse<T> {
    success: boolean;
    data: T;
    message?: string;
    error?: string;
}

export interface SearchFilters {
    query?: string;
    startDate?: string;
    endDate?: string;
    location?: string;
    action?: string;
    minSteps?: number;
    maxSteps?: number;
}

export interface ExportOptions {
    format: 'json' | 'csv' | 'png' | 'pdf';
    dateRange?: {
        start: string;
        end: string;
    };
    includeCharts?: boolean;
    includeMaps?: boolean;
}