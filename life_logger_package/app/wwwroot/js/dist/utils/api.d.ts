import { JournalEntry, DailySummary, AnalyticsData, ApiResponse, SearchFilters } from '../types/models.js';
export declare class LifeLoggerAPI {
    private static readonly BASE_URL;
    private static fetchJSON;
    static getEntries(filters?: SearchFilters): Promise<ApiResponse<JournalEntry[]>>;
    static getAnalytics(startDate?: string, endDate?: string): Promise<ApiResponse<AnalyticsData>>;
    static getLocationStats(): Promise<ApiResponse<any[]>>;
    static getDailySummary(date: string): Promise<ApiResponse<DailySummary>>;
    static getTimelineData(date: string): Promise<ApiResponse<JournalEntry[]>>;
    static searchEntries(query: string): Promise<ApiResponse<JournalEntry[]>>;
    static getHealthStatus(): Promise<ApiResponse<any>>;
}
export declare class DataProcessor {
    static parseCoordinates(location: string): {
        latitude: number;
        longitude: number;
    } | null;
    static categorizeLocation(location: string): 'home' | 'travel' | 'shopping' | 'dining' | 'other';
    static formatDateTime(dateTime: string): string;
    static getHourFromDateTime(dateTime: string): number;
    static getDayOfWeek(dateTime: string): string;
    static aggregateLocationVisits(entries: JournalEntry[]): Map<string, number>;
    static createHourlyDistribution(entries: JournalEntry[]): {
        [hour: number]: number;
    };
    static createWeeklyDistribution(entries: JournalEntry[]): {
        [day: string]: number;
    };
}
