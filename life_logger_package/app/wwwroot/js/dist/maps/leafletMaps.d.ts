import * as L from 'leaflet';
import 'leaflet.markercluster';
import { JournalEntry } from '../types/models.js';
export declare class TerminalMaps {
    private static maps;
    private static markerClusters;
    private static readonly ANN_ARBOR_CENTER;
    private static readonly DEFAULT_ZOOM;
    static createMap(containerId: string, options?: {
        center?: [number, number];
        zoom?: number;
    }): L.Map | null;
    private static applyTerminalStyling;
    static createLocationMarkers(mapId: string, entries: JournalEntry[]): void;
    private static groupEntriesByLocation;
    private static createTerminalIcon;
    private static createLocationPopup;
    static createRouteVisualization(mapId: string, entries: JournalEntry[]): void;
    static createHeatmap(mapId: string, entries: JournalEntry[]): void;
    static clearMarkers(mapId: string): void;
    static destroyMap(mapId: string): void;
    static getMap(mapId: string): L.Map | undefined;
    static centerOnLocation(mapId: string, location: string): void;
    static addLocationSearch(mapId: string, entries: JournalEntry[]): void;
}
