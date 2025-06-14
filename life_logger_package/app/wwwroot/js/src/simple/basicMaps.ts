// Simple map implementation for quick demo
import * as L from 'leaflet';

export class BasicMaps {
    private static maps: Map<string, L.Map> = new Map();

    static createSimpleMap(containerId: string) {
        const container = document.getElementById(containerId);
        if (!container) return null;

        // Destroy existing map
        const existingMap = this.maps.get(containerId);
        if (existingMap) {
            existingMap.remove();
            this.maps.delete(containerId);
        }

        // Ann Arbor center
        const map = L.map(containerId).setView([42.2808, -83.7430], 13);

        // Add tile layer
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);

        // Style the map container
        container.style.filter = 'contrast(1.2) brightness(0.9) hue-rotate(120deg)';
        container.style.border = '2px solid #00ff41';
        container.style.boxShadow = '0 0 20px rgba(0, 255, 65, 0.8)';

        this.maps.set(containerId, map);
        return map;
    }

    static addLocationMarkers(mapId: string, entries: any[]) {
        const map = this.maps.get(mapId);
        if (!map) return;

        // Simple location coordinates for Ann Arbor addresses
        const locationCoords = {
            '2509 Country Village Ct Ann Arbor MI 48103 United States': [42.2808, -83.7430],
            'Waters Place Shopping Center 3160 Lohr Rd Ann Arbor MI 48108 United States': [42.2499, -83.7036],
            '2395 Jackson Ave Ann Arbor MI 48103 United States': [42.2735, -83.7307]
        };

        const locationCounts: { [key: string]: number } = {};
        
        entries.forEach(entry => {
            if (entry.location && locationCoords[entry.location]) {
                locationCounts[entry.location] = (locationCounts[entry.location] || 0) + 1;
            }
        });

        Object.entries(locationCounts).forEach(([location, count]) => {
            const coords = locationCoords[location];
            if (coords) {
                const marker = L.marker([coords[0], coords[1]])
                    .bindPopup(`
                        <div style="background: #0a0a0a; color: #00ff41; font-family: 'JetBrains Mono', monospace; padding: 10px;">
                            <strong style="color: #00ffff;">${location.substring(0, 30)}...</strong><br>
                            Visits: ${count}
                        </div>
                    `)
                    .addTo(map);
            }
        });
    }

    static getMap(mapId: string) {
        return this.maps.get(mapId);
    }
}