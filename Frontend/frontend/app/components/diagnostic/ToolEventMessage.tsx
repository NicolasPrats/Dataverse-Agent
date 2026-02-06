import { Button } from "@fluentui/react-components";
import { ChevronDownRegular, ChevronRightRegular } from "@fluentui/react-icons";

interface ToolEventMessageProps {
    event: {
        EventId: string;
        Target: string;
        Payload?: unknown;
        Result?: unknown;
    };
    expandedPayloads: Set<string>;
    expandedResults: Set<string>;
    togglePayload: (eventId: string) => void;
    toggleResult: (eventId: string) => void;
}

export function ToolEventMessage({ 
    event, 
    expandedPayloads, 
    expandedResults, 
    togglePayload, 
    toggleResult 
}: ToolEventMessageProps) {
    return (
        <>
            <div style={{ marginBottom: "8px" }}>
                <span style={{ color: "#E8EAED", fontSize: "13px" }}>
                    Using tool <strong>{event.Target}</strong>
                </span>
            </div>

            {event.Payload && (
                <div style={{ marginBottom: event.Result ? "1px" : "0" }}>
                    <Button
                        appearance="subtle"
                        size="small"
                        icon={
                            expandedPayloads.has(event.EventId) ? (
                                <ChevronDownRegular />
                            ) : (
                                <ChevronRightRegular />
                            )
                        }
                        onClick={() => togglePayload(event.EventId)}
                        style={{ padding: "4px", minWidth: "auto" }}
                    >
                        Payload
                    </Button>
                    {expandedPayloads.has(event.EventId) && (
                        <pre
                            style={{
                                background: "#0F1419",
                                padding: "6px 8px",
                                borderRadius: "4px",
                                fontSize: "11px",
                                overflow: "auto",
                                margin: "4px 0 0 0",
                                border: "1px solid #2A476C",
                                color: "#E8EAED",
                                maxHeight: "200px",
                            }}
                        >
                            {JSON.stringify(event.Payload, null, 2)}
                        </pre>
                    )}
                </div>
            )}

            {event.Result && (
                <div>
                    <Button
                        appearance="subtle"
                        size="small"
                        icon={
                            expandedResults.has(event.EventId) ? (
                                <ChevronDownRegular />
                            ) : (
                                <ChevronRightRegular />
                            )
                        }
                        onClick={() => toggleResult(event.EventId)}
                        style={{ padding: "4px", minWidth: "auto" }}
                    >
                        Result
                    </Button>
                    {expandedResults.has(event.EventId) && (
                        <pre
                            style={{
                                background: "#0F1419",
                                padding: "6px 8px",
                                borderRadius: "4px",
                                fontSize: "11px",
                                overflow: "auto",
                                margin: "4px 0 0 0",
                                border: "1px solid #2A476C",
                                color: "#E8EAED",
                                maxHeight: "200px",
                            }}
                        >
                            {JSON.stringify(event.Result, null, 2)}
                        </pre>
                    )}
                </div>
            )}

            {!event.Payload && !event.Result && (
                <span style={{ color: "#9AA0A6", fontStyle: "italic", fontSize: "12px" }}>
                    Thinking...
                </span>
            )}
        </>
    );
}
