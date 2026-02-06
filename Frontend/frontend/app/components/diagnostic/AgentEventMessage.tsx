import { Button } from "@fluentui/react-components";
import { ChevronDownRegular, ChevronRightRegular } from "@fluentui/react-icons";
import { Markdown } from "@copilotkit/react-ui";

interface AgentEventMessageProps {
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

export function AgentEventMessage({ 
    event, 
    expandedPayloads, 
    expandedResults, 
    togglePayload, 
    toggleResult 
}: AgentEventMessageProps) {
    const request = (event.Payload as any)?.Arguments?.request;

    return (
        <>
            <div style={{ marginBottom: "8px" }}>
                <span style={{ color: "#E8EAED", fontSize: "13px" }}>
                    @<strong>{event.Target}</strong>, j'ai une requete pour toi:
                </span>
                {request && (
                    <div style={{ 
                        marginTop: "4px", 
                        padding: "8px", 
                        background: "#0F1419",
                        borderRadius: "4px",
                        border: "1px solid #2A476C",
                        color: "#E8EAED",
                        lineHeight: "1.2"
                    }}>
                        <Markdown content={request} />
                    </div>
                )}
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
