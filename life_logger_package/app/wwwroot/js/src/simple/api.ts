// Simple API functions for quick demo
export class SimpleAPI {
    static async getEntries() {
        try {
            const response = await fetch('/api/entries');
            const data = await response.json();
            return { success: true, data };
        } catch (error) {
            console.error('API Error:', error);
            return { success: false, data: [] };
        }
    }

    static processEntriesForCharts(entries: any[]) {
        // Group entries by date
        const entriesByDate: { [key: string]: number } = {};
        const stepsByDate: { [key: string]: number } = {};

        entries.forEach(entry => {
            const date = entry.date;
            entriesByDate[date] = (entriesByDate[date] || 0) + 1;
            if (entry.steps) {
                stepsByDate[date] = (stepsByDate[date] || 0) + entry.steps;
            }
        });

        const entryData = Object.entries(entriesByDate)
            .sort(([a], [b]) => a.localeCompare(b))
            .slice(-14) // Last 14 days
            .map(([date, count]) => ({ date, value: count }));

        const stepsData = Object.entries(stepsByDate)
            .sort(([a], [b]) => a.localeCompare(b))
            .slice(-14) // Last 14 days
            .map(([date, steps]) => ({ date, value: steps }));

        return { entryData, stepsData };
    }

    static processLocationsForChart(entries: any[]) {
        const locationCounts: { [key: string]: number } = {};
        
        entries.forEach(entry => {
            if (entry.location) {
                // Simplify location names
                let location = entry.location;
                if (location.includes('Country Village')) location = 'Home';
                else if (location.includes('Waters Place')) location = 'Shopping Center';
                else if (location.includes('Jackson Ave')) location = 'Jackson Ave';
                else location = 'Other';
                
                locationCounts[location] = (locationCounts[location] || 0) + 1;
            }
        });

        return Object.entries(locationCounts)
            .sort(([, a], [, b]) => b - a)
            .slice(0, 5)
            .map(([label, value]) => ({ label, value }));
    }
}