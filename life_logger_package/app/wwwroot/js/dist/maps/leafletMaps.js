// Leaflet.js map components with retro terminal styling for Ann Arbor locations
import * as L from 'leaflet';
import 'leaflet.markercluster';
import { DataProcessor } from '../utils/api.js';
import { terminalColors } from '../utils/chartTheme.js';
// Fix Leaflet default icon paths
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
    iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
});
export class TerminalMaps {
    static createMap(containerId, options) {
        const container = document.getElementById(containerId);
        if (!container)
            return null;
        // Destroy existing map
        this.destroyMap(containerId);
        const map = L.map(containerId, {
            zoomControl: true,
            attributionControl: true
        }).setView(options?.center || this.ANN_ARBOR_CENTER, options?.zoom || this.DEFAULT_ZOOM);
        // Add retro-styled tile layer
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            className: 'terminal-map-tiles'
        }).addTo(map);
        // Add custom CSS for retro styling
        this.applyTerminalStyling(map);
        this.maps.set(containerId, map);
        return map;
    }
    static applyTerminalStyling(map) {
        const container = map.getContainer();
        // Add terminal styling classes
        container.style.filter = 'contrast(1.2) brightness(0.9) hue-rotate(120deg)';
        container.style.border = `2px solid ${terminalColors.green}`;
        container.style.boxShadow = `0 0 20px ${terminalColors.glow}`;
        // Style attribution and zoom controls
        const attribution = container.querySelector('.leaflet-control-attribution');
        if (attribution) {
            attribution.style.backgroundColor = terminalColors.backgroundCard;
            attribution.style.color = terminalColors.green;
            attribution.style.fontFamily = "'JetBrains Mono', monospace";
            attribution.style.fontSize = '10px';
            attribution.style.border = `1px solid ${terminalColors.green}`;
        }
        const zoomControl = container.querySelector('.leaflet-control-zoom');
        if (zoomControl) {
            zoomControl.style.border = `1px solid ${terminalColors.green}`;
            const buttons = zoomControl.querySelectorAll('a');
            buttons.forEach(button => {
                button.style.backgroundColor = terminalColors.backgroundCard;
                button.style.color = terminalColors.green;
                button.style.borderColor = terminalColors.green;
                button.style.fontFamily = "'Fira Code', monospace";
            });
        }
    }
    static createLocationMarkers(mapId, entries) {
        const map = this.maps.get(mapId);
        if (!map)
            return;
        // Clear existing markers
        this.clearMarkers(mapId);
        // Create marker cluster group
        const markers = L.markerClusterGroup({
            iconCreateFunction: (cluster) => {
                const count = cluster.getChildCount();
                return L.divIcon({
                    html: `<div style="
                        background: ${terminalColors.backgroundCard};
                        color: ${terminalColors.cyan};
                        border: 2px solid ${terminalColors.green};
                        border-radius: 50%;
                        width: 40px;
                        height: 40px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        font-family: 'Fira Code', monospace;
                        font-weight: bold;
                        font-size: 12px;
                        box-shadow: 0 0 10px ${terminalColors.glow};
                    ">[${count}]</div>`,
                    className: 'terminal-cluster-marker',
                    iconSize: [40, 40]
                });
            },
            spiderfyOnMaxZoom: true,
            showCoverageOnHover: false,
            zoomToBoundsOnClick: true,
            maxClusterRadius: 50
        });
        // Process locations and create markers
        const locationGroups = this.groupEntriesByLocation(entries);
        locationGroups.forEach((locationEntries, location) => {
            const coords = DataProcessor.parseCoordinates(location);
            if (!coords)
                return;
            const category = DataProcessor.categorizeLocation(location);
            const icon = this.createTerminalIcon(category, locationEntries.length);
            const marker = L.marker([coords.latitude, coords.longitude], { icon })
                .bindPopup(this.createLocationPopup(location, locationEntries));
            markers.addLayer(marker);
        });
        markers.addTo(map);
        this.markerClusters.set(mapId, markers);
    }
    static groupEntriesByLocation(entries) {
        const locationGroups = new Map();
        entries.forEach(entry => {
            if (entry.location) {
                const existing = locationGroups.get(entry.location) || [];
                existing.push(entry);
                locationGroups.set(entry.location, existing);
            }
        });
        return locationGroups;
    }
    static createTerminalIcon(category, visitCount) {
        let color = terminalColors.green;
        let symbol = '●';
        switch (category) {
            case 'home':
                color = terminalColors.cyan;
                symbol = '⌂';
                break;
            case 'shopping':
                color = terminalColors.yellow;
                symbol = '⚏';
                break;
            case 'dining':
                color = terminalColors.red;
                symbol = '🍽';
                break;
            case 'travel':
                color = terminalColors.white;
                symbol = '⚡';
                break;
            default:
                color = terminalColors.green;
                symbol = '●';
        }
        return L.divIcon({
            html: `<div style="
                background: ${terminalColors.backgroundCard};
                color: ${color};
                border: 2px solid ${color};
                border-radius: 50%;
                width: 30px;
                height: 30px;
                display: flex;
                align-items: center;
                justify-content: center;
                font-family: 'Fira Code', monospace;
                font-weight: bold;
                font-size: 16px;
                box-shadow: 0 0 10px ${color}80;
                position: relative;
            ">${symbol}
            <span style="
                position: absolute;
                top: -8px;
                right: -8px;
                background: ${terminalColors.green};
                color: ${terminalColors.background};
                border-radius: 50%;
                width: 16px;
                height: 16px;
                font-size: 10px;
                display: flex;
                align-items: center;
                justify-content: center;
            ">${visitCount}</span>
            </div>`,
            className: 'terminal-location-marker',
            iconSize: [30, 30],
            iconAnchor: [15, 15]
        });
    }
    static createLocationPopup(location, entries) {
        const sortedEntries = entries
            .sort((a, b) => b.datetime.localeCompare(a.datetime))
            .slice(0, 5); // Show latest 5 entries
        return `
            <div style="
                background: ${terminalColors.background};
                color: ${terminalColors.green};
                font-family: 'JetBrains Mono', monospace;
                padding: 10px;
                border: 1px solid ${terminalColors.green};
                max-width: 300px;
            ">
                <div style="
                    color: ${terminalColors.cyan};
                    font-weight: bold;
                    margin-bottom: 8px;
                    font-size: 14px;
                ">
                    > LOCATION_DATA
                </div>
                <div style="margin-bottom: 8px; font-size: 12px;">
                    ADDRESS: ${location.length > 50 ? location.substring(0, 47) + '...' : location}
                </div>
                <div style="margin-bottom: 8px; font-size: 12px;">
                    VISITS: ${entries.length}
                </div>
                <div style="
                    color: ${terminalColors.cyan};
                    font-weight: bold;
                    margin: 8px 0 4px 0;
                    font-size: 12px;
                ">
                    RECENT_ENTRIES:
                </div>
                ${sortedEntries.map(entry => `
                    <div style="
                        margin: 4px 0;
                        padding: 4px;
                        border-left: 2px solid ${terminalColors.green};
                        padding-left: 8px;
                        font-size: 11px;
                    ">
                        <div style="color: ${terminalColors.yellow};">
                            ${DataProcessor.formatDateTime(entry.datetime)}
                        </div>
                        <div style="margin-top: 2px;">
                            ${entry.entry.length > 80 ? entry.entry.substring(0, 77) + '...' : entry.entry}
                        </div>
                        ${entry.steps ? `<div style="color: ${terminalColors.cyan};">STEPS: ${entry.steps}</div>` : ''}
                    </div>
                `).join('')}
                ${entries.length > 5 ? `
                    <div style="
                        color: ${terminalColors.gray};
                        font-size: 10px;
                        margin-top: 8px;
                    ">
                        ... and ${entries.length - 5} more entries
                    </div>
                ` : ''}
            </div>
        `;
    }
    static createRouteVisualization(mapId, entries) {
        const map = this.maps.get(mapId);
        if (!map)
            return;
        // Get entries with location data, sorted by time
        const locationEntries = entries
            .filter(entry => entry.location)
            .sort((a, b) => a.datetime.localeCompare(b.datetime));
        if (locationEntries.length < 2)
            return;
        const routeCoordinates = [];
        const routeSegments = [];
        // Create route coordinates and segments
        for (let i = 0; i < locationEntries.length - 1; i++) {
            const current = locationEntries[i];
            const next = locationEntries[i + 1];
            const currentCoords = DataProcessor.parseCoordinates(current.location);
            const nextCoords = DataProcessor.parseCoordinates(next.location);
            if (currentCoords && nextCoords) {
                routeCoordinates.push([currentCoords.latitude, currentCoords.longitude]);
                // Only add next coordinate if it's the last entry
                if (i === locationEntries.length - 2) {
                    routeCoordinates.push([nextCoords.latitude, nextCoords.longitude]);
                }
            }
        }
        // Create route polyline with terminal styling
        if (routeCoordinates.length > 1) {
            const routeLine = L.polyline(routeCoordinates, {
                color: terminalColors.cyan,
                weight: 3,
                opacity: 0.8,
                dashArray: '10, 5',
                className: 'terminal-route-line'
            }).addTo(map);
            // Add route animation effect
            let offset = 0;
            const animateRoute = () => {
                offset += 1;
                routeLine.setStyle({ dashOffset: offset });
                if (offset < 100) {
                    requestAnimationFrame(animateRoute);
                }
            };
            animateRoute();
            // Fit map to route bounds
            map.fitBounds(routeLine.getBounds(), { padding: [20, 20] });
        }
    }
    static createHeatmap(mapId, entries) {
        const map = this.maps.get(mapId);
        if (!map)
            return;
        // Simple heatmap using circle markers with varying opacity
        const locationCounts = DataProcessor.aggregateLocationVisits(entries);
        const maxCount = Math.max(...locationCounts.values());
        locationCounts.forEach((count, location) => {
            const coords = DataProcessor.parseCoordinates(location);
            if (!coords)
                return;
            const intensity = count / maxCount;
            const radius = 20 + (intensity * 50); // Scale radius by frequency
            const opacity = 0.3 + (intensity * 0.5); // Scale opacity by frequency
            L.circle([coords.latitude, coords.longitude], {
                radius: radius,
                fillColor: terminalColors.green,
                color: terminalColors.cyan,
                weight: 2,
                opacity: opacity,
                fillOpacity: opacity * 0.5,
                className: 'terminal-heatmap-circle'
            }).addTo(map);
        });
    }
    static clearMarkers(mapId) {
        const cluster = this.markerClusters.get(mapId);
        if (cluster) {
            const map = this.maps.get(mapId);
            if (map) {
                map.removeLayer(cluster);
            }
            this.markerClusters.delete(mapId);
        }
    }
    static destroyMap(mapId) {
        this.clearMarkers(mapId);
        const map = this.maps.get(mapId);
        if (map) {
            map.remove();
            this.maps.delete(mapId);
        }
    }
    static getMap(mapId) {
        return this.maps.get(mapId);
    }
    static centerOnLocation(mapId, location) {
        const map = this.maps.get(mapId);
        if (!map)
            return;
        const coords = DataProcessor.parseCoordinates(location);
        if (coords) {
            map.setView([coords.latitude, coords.longitude], 15);
        }
    }
    static addLocationSearch(mapId, entries) {
        const map = this.maps.get(mapId);
        if (!map)
            return;
        // Add search control for locations
        const searchControl = L.control({ position: 'topright' });
        searchControl.onAdd = () => {
            const div = L.DomUtil.create('div', 'leaflet-control-search');
            div.style.backgroundColor = terminalColors.backgroundCard;
            div.style.border = `1px solid ${terminalColors.green}`;
            div.style.padding = '5px';
            const select = L.DomUtil.create('select', '', div);
            select.style.backgroundColor = terminalColors.backgroundCard;
            select.style.color = terminalColors.green;
            select.style.border = `1px solid ${terminalColors.green}`;
            select.style.fontFamily = "'JetBrains Mono', monospace";
            select.style.fontSize = '12px';
            // Add default option
            const defaultOption = L.DomUtil.create('option', '', select);
            defaultOption.value = '';
            defaultOption.text = 'Select Location...';
            // Add location options
            const uniqueLocations = [...new Set(entries.map(e => e.location).filter(Boolean))];
            uniqueLocations.forEach(location => {
                const option = L.DomUtil.create('option', '', select);
                option.value = location;
                option.text = location.length > 30 ? location.substring(0, 27) + '...' : location;
            });
            select.addEventListener('change', () => {
                if (select.value) {
                    this.centerOnLocation(mapId, select.value);
                }
            });
            L.DomEvent.disableClickPropagation(div);
            return div;
        };
        searchControl.addTo(map);
    }
}
TerminalMaps.maps = new Map();
TerminalMaps.markerClusters = new Map();
// Ann Arbor center coordinates
TerminalMaps.ANN_ARBOR_CENTER = [42.2808, -83.7430];
TerminalMaps.DEFAULT_ZOOM = 13;
//# sourceMappingURL=leafletMaps.js.map