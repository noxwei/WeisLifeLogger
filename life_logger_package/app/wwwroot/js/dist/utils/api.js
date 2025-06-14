export class LifeLoggerAPI {
    static async fetchJSON(url, options) {
        try {
            const response = await fetch(url, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options?.headers
                },
                ...options
            });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            const data = await response.json();
            return {
                success: true,
                data
            };
        }
        catch (error) {
            console.error('API Error:', error);
            return {
                success: false,
                data: null,
                error: error instanceof Error ? error.message : 'Unknown error'
            };
        }
    }
    static async getEntries(filters) {
        const params = new URLSearchParams();
        if (filters) {
            if (filters.query)
                params.append('query', filters.query);
            if (filters.startDate)
                params.append('startDate', filters.startDate);
            if (filters.endDate)
                params.append('endDate', filters.endDate);
            if (filters.location)
                params.append('location', filters.location);
            if (filters.action)
                params.append('action', filters.action);
            if (filters.minSteps)
                params.append('minSteps', filters.minSteps.toString());
            if (filters.maxSteps)
                params.append('maxSteps', filters.maxSteps.toString());
        }
        const url = `${this.BASE_URL}/entries${params.toString() ? '?' + params.toString() : ''}`;
        return this.fetchJSON(url);
    }
    static async getAnalytics(startDate, endDate) {
        const params = new URLSearchParams();
        if (startDate)
            params.append('startDate', startDate);
        if (endDate)
            params.append('endDate', endDate);
        const url = `${this.BASE_URL}/analytics${params.toString() ? '?' + params.toString() : ''}`;
        return this.fetchJSON(url);
    }
    static async getLocationStats() {
        return this.fetchJSON(`${this.BASE_URL}/locations`);
    }
    static async getDailySummary(date) {
        return this.fetchJSON(`${this.BASE_URL}/summary/${date}`);
    }
    static async getTimelineData(date) {
        return this.fetchJSON(`${this.BASE_URL}/timeline/${date}`);
    }
    static async searchEntries(query) {
        const params = new URLSearchParams({ query });
        return this.fetchJSON(`${this.BASE_URL}/search?${params.toString()}`);
    }
    static async getHealthStatus() {
        return this.fetchJSON('/health');
    }
}
LifeLoggerAPI.BASE_URL = '/api';
// Utility functions for data processing
export class DataProcessor {
    static parseCoordinates(location) {
        // Ann Arbor location coordinates mapping
        const annArborLocations = {
            '2509 Country Village Ct Ann Arbor MI 48103 United States': { latitude: 42.2808, longitude: -83.7430 },
            '1423 Iroquois Pl Ann Arbor MI 48104 United States': { latitude: 42.2808, longitude: -83.7430 },
            'Waters Place Shopping Center 3160 Lohr Rd Ann Arbor MI 48108 United States': { latitude: 42.2499, longitude: -83.7036 },
            '2507 Adrienne Dr Ann Arbor MI 48103 United States': { latitude: 42.2801, longitude: -83.7425 },
            '2395 Jackson Ave Ann Arbor MI 48103 United States': { latitude: 42.2735, longitude: -83.7307 },
            'Ann Arbor': { latitude: 42.2808, longitude: -83.7430 },
            'University of Michigan': { latitude: 42.2780, longitude: -83.7382 },
            'Downtown Ann Arbor': { latitude: 42.2808, longitude: -83.7430 }
        };
        // Try exact match first
        if (annArborLocations[location]) {
            return annArborLocations[location];
        }
        // Try partial matching for addresses
        for (const [key, coords] of Object.entries(annArborLocations)) {
            if (location.includes(key) || key.includes(location)) {
                return coords;
            }
        }
        // Default to Ann Arbor center if no match
        return { latitude: 42.2808, longitude: -83.7430 };
    }
    static categorizeLocation(location) {
        const lower = location.toLowerCase();
        if (lower.includes('country village') || lower.includes('home')) {
            return 'home';
        }
        else if (lower.includes('shopping') || lower.includes('waters place')) {
            return 'shopping';
        }
        else if (lower.includes('restaurant') || lower.includes('cafe') || lower.includes('tamales')) {
            return 'dining';
        }
        else if (lower.includes('driving') || lower.includes('travel')) {
            return 'travel';
        }
        return 'other';
    }
    static formatDateTime(dateTime) {
        try {
            const date = new Date(dateTime);
            return date.toLocaleString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
        catch {
            return dateTime;
        }
    }
    static getHourFromDateTime(dateTime) {
        try {
            const date = new Date(dateTime);
            return date.getHours();
        }
        catch {
            return 0;
        }
    }
    static getDayOfWeek(dateTime) {
        try {
            const date = new Date(dateTime);
            return date.toLocaleDateString('en-US', { weekday: 'long' });
        }
        catch {
            return 'Unknown';
        }
    }
    static aggregateLocationVisits(entries) {
        const locationCounts = new Map();
        entries.forEach(entry => {
            if (entry.location) {
                const count = locationCounts.get(entry.location) || 0;
                locationCounts.set(entry.location, count + 1);
            }
        });
        return locationCounts;
    }
    static createHourlyDistribution(entries) {
        const hourlyCount = {};
        // Initialize all hours
        for (let i = 0; i < 24; i++) {
            hourlyCount[i] = 0;
        }
        entries.forEach(entry => {
            const hour = this.getHourFromDateTime(entry.datetime);
            hourlyCount[hour]++;
        });
        return hourlyCount;
    }
    static createWeeklyDistribution(entries) {
        const weeklyCount = {
            'Monday': 0,
            'Tuesday': 0,
            'Wednesday': 0,
            'Thursday': 0,
            'Friday': 0,
            'Saturday': 0,
            'Sunday': 0
        };
        entries.forEach(entry => {
            const day = this.getDayOfWeek(entry.datetime);
            if (weeklyCount[day] !== undefined) {
                weeklyCount[day]++;
            }
        });
        return weeklyCount;
    }
}
//# sourceMappingURL=api.js.map