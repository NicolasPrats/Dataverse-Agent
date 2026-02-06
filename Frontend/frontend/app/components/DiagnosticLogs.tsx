"use client";

import { useEffect, useState } from "react";
import { Badge, Caption1, Text, ToggleButton } from "@fluentui/react-components";
import { CircleFilled } from "@fluentui/react-icons";
import { getAgentConfig } from "../config/agentConfig";
import { ToolEventMessage } from "./diagnostic/ToolEventMessage";
import { AgentEventMessage } from "./diagnostic/AgentEventMessage";
import { SimulatedResponseEventMessage } from "./diagnostic/SimulatedResponseEventMessage";

enum TargetType {
    Tool = "Tool",
    Agent = "Agent",
    SimulatedResponse = "SimulatedResponse"
}

interface AgentDiagnosticEvent {
    Timestamp: string;
    SourceAgent: string;
    Target: string;
    TargetType: TargetType;
    Payload?: unknown;
    Result?: unknown;
    Duration?: string | number;
    EventId: string;
}

export default function DiagnosticLogs() {
const [events, setEvents] = useState<AgentDiagnosticEvent[]>([]);
const [isConnected, setIsConnected] = useState(false);
const [expandedResults, setExpandedResults] = useState<Set<string>>(new Set());
const [expandedPayloads, setExpandedPayloads] = useState<Set<string>>(new Set());
const [isChatOnlyMode, setIsChatOnlyMode] = useState(false);

    useEffect(() => {
        const eventSource = new EventSource("/api/diagnostics");

        eventSource.onopen = () => {
            setIsConnected(true);
        };

        eventSource.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data) as AgentDiagnosticEvent;
                setEvents((prev) => {
                    const existingIndex = prev.findIndex(e => e.EventId === data.EventId);
                    if (existingIndex !== -1) {
                        const newEvents = [...prev];
                        newEvents[existingIndex] = data;
                        return newEvents.slice(-100);
                    } else {
                        return [...prev, data].slice(-100);
                    }
                });
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

    const toggleResult = (eventId: string) => {
        const newExpanded = new Set(expandedResults);
        if (newExpanded.has(eventId)) {
            newExpanded.delete(eventId);
        } else {
            newExpanded.add(eventId);
        }
        setExpandedResults(newExpanded);
    };

    const togglePayload = (eventId: string) => {
        const newExpanded = new Set(expandedPayloads);
        if (newExpanded.has(eventId)) {
            newExpanded.delete(eventId);
        } else {
            newExpanded.add(eventId);
        }
        setExpandedPayloads(newExpanded);
    };

    return (
        <div style={{ display: "flex", flexDirection: "column", flex: 1, minHeight: 0, gap: "16px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexShrink: 0 }}>
                <Text size={500} weight="semibold">
                    Diagnostic Logs
                </Text>
                <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
                    <ToggleButton
                        checked={isChatOnlyMode}
                        onClick={() => setIsChatOnlyMode(!isChatOnlyMode)}
                        size="small"
                    >
                        {isChatOnlyMode ? "Chat only" : "Details"}
                    </ToggleButton>
                    <Badge
                        appearance="filled"
                        color={isConnected ? "success" : "danger"}
                        icon={<CircleFilled />}
                    >
                        {isConnected ? "Connected" : "Disconnected"}
                    </Badge>
                </div>
            </div>

            <div style={{ flex: 1, overflow: "auto", minHeight: 0 }}>
                <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                    {events.length === 0 ? (
                        <div style={{ textAlign: "center", padding: "32px" }}>
                            <Caption1>Waiting for logs...</Caption1>
                        </div>
                    ) : (
                        events
                            .filter(event => {
                                if (!isChatOnlyMode) return true;
                                const targetType = event.TargetType as any;
                                return targetType !== TargetType.Tool && 
                                       targetType !== "Tool" && 
                                       targetType !== 0;
                            })
                            .map((event) => {
                            const agentConfig = getAgentConfig(event.SourceAgent);
                            const timestamp = new Date(event.Timestamp).toLocaleString("fr-FR", {
                                hour: "2-digit",
                                minute: "2-digit",
                                second: "2-digit",
                            });

                            return (
                                <div
                                    key={event.EventId}
                                    style={{
                                        display: "flex",
                                        gap: "12px",
                                        padding: "12px 0",
                                        alignItems: "flex-start",
                                    }}
                                >
                                    <img
                                        src={agentConfig.avatar}
                                        alt={agentConfig.displayName}
                                        style={{
                                            width: "32px",
                                            height: "32px",
                                            borderRadius: "50%",
                                            flexShrink: 0,
                                            objectFit: "cover",
                                            border: `2px solid ${agentConfig.color}`,
                                        }}
                                        onError={(e) => {
                                            e.currentTarget.style.display = "none";
                                        }}
                                    />
                                    <div style={{ flex: 1, minWidth: 0 }}>
                                        <div
                                            style={{
                                                display: "flex",
                                                alignItems: "center",
                                                gap: "8px",
                                                marginBottom: "6px",
                                                flexWrap: "wrap",
                                            }}
                                        >
                                            <span
                                                style={{
                                                    fontSize: "13px",
                                                    fontWeight: 600,
                                                    color: agentConfig.color,
                                                }}
                                            >
                                                {agentConfig.displayName}
                                            </span>
                                            <span
                                                style={{
                                                    fontSize: "11px",
                                                    color: "#9AA0A6",
                                                }}
                                            >
                                                {timestamp}
                                            </span>
                                            {event.Duration && (
                                                <Badge appearance="tint" size="small" color="warning">
                                                    {formatDuration(event.Duration)}
                                                </Badge>
                                            )}
                                        </div>
                                        <div
                                            style={{
                                                background: "#16212D",
                                                padding: "10px 12px",
                                                borderRadius: "8px",
                                                border: `1px solid ${agentConfig.color}`,
                                                color: "#E8EAED",
                                                fontSize: "13px",
                                            }}
                                        >
                                            {((event.TargetType as any) === TargetType.Tool || (event.TargetType as any) === "Tool" || (event.TargetType as any) === 0) && (
                                                <ToolEventMessage
                                                    event={event}
                                                    expandedPayloads={expandedPayloads}
                                                    expandedResults={expandedResults}
                                                    togglePayload={togglePayload}
                                                    toggleResult={toggleResult}
                                                />
                                            )}
                                            {((event.TargetType as any) === TargetType.Agent || (event.TargetType as any) === "Agent" || (event.TargetType as any) === 1) && (
                                                <AgentEventMessage
                                                    event={event}
                                                    expandedPayloads={expandedPayloads}
                                                    expandedResults={expandedResults}
                                                    togglePayload={togglePayload}
                                                    toggleResult={toggleResult}
                                                    showDetails={!isChatOnlyMode}
                                                />
                                            )}
                                            {((event.TargetType as any) === TargetType.SimulatedResponse || (event.TargetType as any) === "SimulatedResponse" || (event.TargetType as any) === 2) && (
                                                <SimulatedResponseEventMessage event={event} />
                                            )}
                                        </div>
                                    </div>
                                </div>
                            );
                        })
                    )}
                </div>
            </div>
        </div>
    );
}
