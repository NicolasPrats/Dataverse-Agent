"use client";

import { useEffect, useState } from "react";

interface AgentDiagnosticEvent {
    Timestamp: string;
    SourceAgent: string;
    Target: string;
    Payload?: unknown;
    Result?: unknown;
    Duration?: string | number;
}

export default function DiagnosticLogs() {
    const [events, setEvents] = useState<AgentDiagnosticEvent[]>([]);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        const eventSource = new EventSource("/api/diagnostics");

        eventSource.onopen = () => {
            setIsConnected(true);
        };

        eventSource.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data) as AgentDiagnosticEvent;
                setEvents((prev) => [data, ...prev].slice(0, 100));
            } catch (error) {
                console.error("Failed to parse diagnostic event:", error);
            }
        };

        eventSource.onerror = () => {
            setIsConnected(false);
            eventSource.close();
        };

        return () => {
            eventSource.close();
        };
    }, []);

    const formatDuration = (duration?: string | number) => {
        if (!duration) return "";
        
        if (typeof duration === "number") {
            return `${duration.toFixed(2)}ms`;
        }
        
        // Parse TimeSpan format: "00:00:00.1234567" or "00:00:00"
        const parts = duration.split(":");
        if (parts.length >= 3) {
            const hours = parseInt(parts[0]);
            const minutes = parseInt(parts[1]);
            const seconds = parseFloat(parts[2]);
            
            const totalMs = (hours * 3600 + minutes * 60 + seconds) * 1000;
            
            if (totalMs < 1000) {
                return `${totalMs.toFixed(2)}ms`;
            } else if (totalMs < 60000) {
                return `${(totalMs / 1000).toFixed(2)}s`;
            } else {
                return `${(totalMs / 60000).toFixed(2)}min`;
            }
        }
        
        return duration.toString();
    };

    const formatTimestamp = (timestamp: string) => {
        return new Date(timestamp).toLocaleTimeString();
    };

    return (
        <div className="diagnostic-logs">
            <div className="header">
                <h2>Diagnostic Logs</h2>
                <div className={`status ${isConnected ? "connected" : "disconnected"}`}>
                    {isConnected ? "● Connected" : "○ Disconnected"}
                </div>
            </div>

            <div className="logs-container">
                {events.length === 0 ? (
                    <div className="no-logs">Waiting for logs...</div>
                ) : (
                    events.map((event, index) => (
                        <div key={index} className="log-entry">
                            <div className="log-header">
                                <span className="timestamp">{formatTimestamp(event.Timestamp)}</span>
                                <span className="agent">{event.SourceAgent}</span>
                                <span className="arrow">→</span>
                                <span className="target">{event.Target}</span>
                                {event.Duration && (
                                    <span className="duration">{formatDuration(event.Duration)}</span>
                                )}
                            </div>
                            {event.Payload && (
                                <div className="log-payload">
                                    <strong>Payload:</strong>
                                    <pre>{JSON.stringify(event.Payload, null, 2)}</pre>
                                </div>
                            )}
                            {event.Result && (
                                <div className="log-result">
                                    <strong>Result:</strong>
                                    <pre>{JSON.stringify(event.Result, null, 2)}</pre>
                                </div>
                            )}
                        </div>
                    ))
                )}
            </div>

            <style jsx>{`
                .diagnostic-logs {
                    border: 1px solid #e5e7eb;
                    border-radius: 8px;
                    padding: 16px;
                    background: white;
                    height: 100%;
                    display: flex;
                    flex-direction: column;
                }

                .header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 16px;
                    padding-bottom: 12px;
                    border-bottom: 1px solid #e5e7eb;
                }

                .header h2 {
                    margin: 0;
                    font-size: 18px;
                    font-weight: 600;
                }

                .status {
                    font-size: 14px;
                    font-weight: 500;
                }

                .status.connected {
                    color: #10b981;
                }

                .status.disconnected {
                    color: #ef4444;
                }

                .logs-container {
                    flex: 1;
                    overflow-y: auto;
                    display: flex;
                    flex-direction: column;
                    gap: 12px;
                }

                .no-logs {
                    text-align: center;
                    color: #9ca3af;
                    padding: 32px;
                }

                .log-entry {
                    padding: 12px;
                    background: #f9fafb;
                    border-radius: 6px;
                    border: 1px solid #e5e7eb;
                }

                .log-header {
                    display: flex;
                    gap: 8px;
                    align-items: center;
                    flex-wrap: wrap;
                    margin-bottom: 8px;
                }

                .timestamp {
                    color: #6b7280;
                    font-size: 12px;
                    font-family: monospace;
                }

                .agent {
                    color: #3b82f6;
                    font-weight: 600;
                    font-size: 14px;
                }

                .arrow {
                    color: #9ca3af;
                }

                .target {
                    color: #8b5cf6;
                    font-weight: 600;
                    font-size: 14px;
                }

                .duration {
                    color: #10b981;
                    font-size: 12px;
                    font-family: monospace;
                    margin-left: auto;
                }

                .log-payload,
                .log-result {
                    margin-top: 8px;
                }

                .log-payload strong,
                .log-result strong {
                    display: block;
                    font-size: 12px;
                    color: #6b7280;
                    margin-bottom: 4px;
                }

                .log-payload pre,
                .log-result pre {
                    background: white;
                    padding: 8px;
                    border-radius: 4px;
                    font-size: 12px;
                    overflow-x: auto;
                    margin: 0;
                    border: 1px solid #e5e7eb;
                }
            `}</style>
        </div>
    );
}
