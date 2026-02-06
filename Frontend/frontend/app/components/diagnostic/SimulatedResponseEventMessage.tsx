import { Markdown } from "@copilotkit/react-ui";

interface SimulatedResponseEventMessageProps {
    event: {
        Result?: unknown;
    };
}

export function SimulatedResponseEventMessage({ event }: SimulatedResponseEventMessageProps) {
    if (!event.Result) {
        return (
            <span style={{ color: "#9AA0A6", fontStyle: "italic", fontSize: "12px" }}>
                Thinking...
            </span>
        );
    }

    let resultContent = event.Result;
    
    if (typeof resultContent === 'string' && resultContent.startsWith('Agent response:')) {
        resultContent = resultContent.substring('Agent response:'.length).trim();
    }

    return (
        <div style={{ marginBottom: "8px" }}>
                {typeof resultContent === 'string' 
                    ? <Markdown content={resultContent} />
                    : <pre style={{ margin: 0, fontSize: "11px", whiteSpace: "pre-wrap" }}>
                        {JSON.stringify(resultContent, null, 2)}
                    </pre>
                }
        </div>
    );
}
