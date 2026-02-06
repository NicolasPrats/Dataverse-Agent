"use client";

import { useEffect, useState } from "react";
import {
    Card,
    CardHeader,
    Badge,
    Body1,
    Caption1,
    Text,
    Button,
} from "@fluentui/react-components";
import {
    ChevronDownRegular,
    ChevronRightRegular,
    CircleFilled,
} from "@fluentui/react-icons";

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
    const [expandedResults, setExpandedResults] = useState<Set<number>>(new Set());

    useEffect(() => {
        const eventSource = new EventSource("/api/diagnostics");

        eventSource.onopen = () => {
            setIsConnected(true);
        };

        eventSource.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data) as AgentDiagnosticEvent;
                setEvents((prev) => [...prev, data].slice(-100));
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

    const toggleResult = (index: number) => {
        const newExpanded = new Set(expandedResults);
        if (newExpanded.has(index)) {
            newExpanded.delete(index);
        } else {
            newExpanded.add(index);
        }
        setExpandedResults(newExpanded);
    };

    return (
        <div style={{ display: "flex", flexDirection: "column", flex: 1, minHeight: 0, gap: "16px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexShrink: 0 }}>
                <Text size={500} weight="semibold">
                    Diagnostic Logs
                </Text>
                <Badge
                    appearance="filled"
                    color={isConnected ? "success" : "danger"}
                    icon={<CircleFilled />}
                >
                    {isConnected ? "Connected" : "Disconnected"}
                </Badge>
            </div>

            <div
                style={{
                    flex: 1,
                    overflow: "auto",
                    minHeight: 0,
                }}
            >
                <div
                    style={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "12px",
                    }}
                >
                    {events.length === 0 ? (
                        <div style={{ textAlign: "center", padding: "32px" }}>
                            <Caption1>Waiting for logs...</Caption1>
                        </div>
                    ) : (
                        events.map((event, index) => (
                            <Card key={index} style={{ padding: "12px" }}>
                                <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                                    <div
                                        style={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: "8px",
                                            flexWrap: "wrap",
                                        }}
                                    >
                                        <Caption1 style={{ color: "#9AA0A6" }}>
                                            {formatTimestamp(event.Timestamp)}
                                        </Caption1>
                                        <Badge appearance="filled" color="informative">
                                            {event.SourceAgent}
                                        </Badge>
                                        <Text style={{ color: "#9AA0A6" }}>→</Text>
                                        <Badge appearance="filled" color="brand">
                                            {event.Target}
                                        </Badge>
                                        {event.Duration && (
                                            <Badge appearance="tint" color="success">
                                                {formatDuration(event.Duration)}
                                            </Badge>
                                        )}
                                    </div>

                                    {event.Payload ? (
                                        <div style={{ marginTop: "8px" }}>
                                            <Caption1 style={{ color: "#9AA0A6", fontWeight: 600 }}>
                                                Payload:
                                            </Caption1>
                                            <pre
                                                style={{
                                                    background: "#1B2A3D",
                                                    padding: "8px",
                                                    borderRadius: "4px",
                                                    fontSize: "12px",
                                                    overflow: "auto",
                                                    margin: "4px 0 0 0",
                                                    border: "1px solid #2A476C",
                                                    color: "#E8EAED",
                                                }}
                                            >
                                                {JSON.stringify(event.Payload, null, 2)}
                                            </pre>
                                        </div>
                                    ) : null}

                                    {event.Result ? (
                                        <div style={{ marginTop: "8px" }}>
                                            <Button
                                                appearance="subtle"
                                                size="small"
                                                icon={
                                                    expandedResults.has(index) ? (
                                                        <ChevronDownRegular />
                                                    ) : (
                                                        <ChevronRightRegular />
                                                    )
                                                }
                                                onClick={() => toggleResult(index)}
                                            >
                                                Result
                                            </Button>
                                            {expandedResults.has(index) ? (
                                                <pre
                                                    style={{
                                                        background: "#1B2A3D",
                                                        padding: "8px",
                                                        borderRadius: "4px",
                                                        fontSize: "12px",
                                                        overflow: "auto",
                                                        margin: "4px 0 0 0",
                                                        border: "1px solid #2A476C",
                                                        color: "#E8EAED",
                                                    }}
                                                >
                                                    {JSON.stringify(event.Result, null, 2)}
                                                </pre>
                                            ) : null}
                                        </div>
                                    ) : null}
                                </div>
                            </Card>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}

