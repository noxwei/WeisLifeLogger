// Retro terminal theme configuration for Chart.js
import { ChartOptions } from '../types/models.js';

export const terminalColors = {
    background: '#0a0a0a',
    backgroundCard: '#0d0d0d',
    backgroundLighter: '#111111',
    green: '#00ff41',
    cyan: '#00ffff',
    yellow: '#ffff00',
    red: '#ff0040',
    white: '#f0f0f0',
    gray: '#404040',
    grayDark: '#202020',
    scanLine: 'rgba(0, 255, 65, 0.05)',
    glow: 'rgba(0, 255, 65, 0.8)'
};

export const terminalFonts = {
    primary: "'Fira Code', 'JetBrains Mono', 'Courier New', monospace",
    secondary: "'JetBrains Mono', 'Courier New', monospace"
};

export function createTerminalChartOptions(title?: string): ChartOptions {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                labels: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.primary,
                        size: 12
                    }
                }
            },
            tooltip: {
                backgroundColor: terminalColors.backgroundCard,
                titleColor: terminalColors.cyan,
                bodyColor: terminalColors.white,
                borderColor: terminalColors.green,
                borderWidth: 1
            }
        },
        scales: {
            x: {
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary
                    }
                },
                grid: {
                    color: terminalColors.grayDark
                }
            },
            y: {
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary
                    }
                },
                grid: {
                    color: terminalColors.grayDark
                }
            }
        }
    };
}

export function createTimeSeriesOptions(title: string): any {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: title,
                color: terminalColors.cyan,
                font: {
                    family: terminalFonts.primary,
                    size: 16,
                    weight: 'bold'
                }
            },
            legend: {
                display: true,
                labels: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.primary,
                        size: 12
                    }
                }
            },
            tooltip: {
                backgroundColor: terminalColors.backgroundCard,
                titleColor: terminalColors.cyan,
                bodyColor: terminalColors.white,
                borderColor: terminalColors.green,
                borderWidth: 2,
                cornerRadius: 0,
                titleFont: {
                    family: terminalFonts.primary
                },
                bodyFont: {
                    family: terminalFonts.secondary
                }
            }
        },
        scales: {
            x: {
                type: 'time',
                time: {
                    unit: 'day',
                    displayFormats: {
                        day: 'MMM dd'
                    }
                },
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary,
                        size: 11
                    }
                },
                grid: {
                    color: terminalColors.grayDark,
                    lineWidth: 1
                },
                border: {
                    color: terminalColors.green
                }
            },
            y: {
                beginAtZero: true,
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary,
                        size: 11
                    }
                },
                grid: {
                    color: terminalColors.grayDark,
                    lineWidth: 1
                },
                border: {
                    color: terminalColors.green
                }
            }
        },
        elements: {
            line: {
                borderWidth: 2,
                tension: 0.1
            },
            point: {
                radius: 4,
                hoverRadius: 6,
                borderWidth: 2
            }
        }
    };
}

export function createBarChartOptions(title: string): any {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: title,
                color: terminalColors.cyan,
                font: {
                    family: terminalFonts.primary,
                    size: 16,
                    weight: 'bold'
                }
            },
            legend: {
                display: false
            },
            tooltip: {
                backgroundColor: terminalColors.backgroundCard,
                titleColor: terminalColors.cyan,
                bodyColor: terminalColors.white,
                borderColor: terminalColors.green,
                borderWidth: 2,
                cornerRadius: 0,
                titleFont: {
                    family: terminalFonts.primary
                },
                bodyFont: {
                    family: terminalFonts.secondary
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary,
                        size: 11
                    }
                },
                grid: {
                    display: false
                },
                border: {
                    color: terminalColors.green
                }
            },
            y: {
                beginAtZero: true,
                ticks: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.secondary,
                        size: 11
                    }
                },
                grid: {
                    color: terminalColors.grayDark,
                    lineWidth: 1
                },
                border: {
                    color: terminalColors.green
                }
            }
        },
        elements: {
            bar: {
                borderWidth: 1,
                borderColor: terminalColors.green
            }
        }
    };
}

export function createDoughnutOptions(title: string): any {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: title,
                color: terminalColors.cyan,
                font: {
                    family: terminalFonts.primary,
                    size: 16,
                    weight: 'bold'
                }
            },
            legend: {
                display: true,
                position: 'right',
                labels: {
                    color: terminalColors.green,
                    font: {
                        family: terminalFonts.primary,
                        size: 11
                    },
                    padding: 15
                }
            },
            tooltip: {
                backgroundColor: terminalColors.backgroundCard,
                titleColor: terminalColors.cyan,
                bodyColor: terminalColors.white,
                borderColor: terminalColors.green,
                borderWidth: 2,
                cornerRadius: 0,
                titleFont: {
                    family: terminalFonts.primary
                },
                bodyFont: {
                    family: terminalFonts.secondary
                }
            }
        },
        elements: {
            arc: {
                borderWidth: 2,
                borderColor: terminalColors.background
            }
        }
    };
}

export const terminalColorPalette = [
    terminalColors.green,
    terminalColors.cyan,
    terminalColors.yellow,
    terminalColors.red,
    terminalColors.white,
    'rgba(0, 255, 65, 0.7)',
    'rgba(0, 255, 255, 0.7)',
    'rgba(255, 255, 0, 0.7)',
    'rgba(255, 0, 64, 0.7)',
    'rgba(240, 240, 240, 0.7)'
];

export function getDatasetColors(dataCount: number): { backgroundColor: string[], borderColor: string[] } {
    const backgroundColor: string[] = [];
    const borderColor: string[] = [];
    
    for (let i = 0; i < dataCount; i++) {
        const colorIndex = i % terminalColorPalette.length;
        backgroundColor.push(terminalColorPalette[colorIndex]);
        borderColor.push(terminalColorPalette[colorIndex]);
    }
    
    return { backgroundColor, borderColor };
}